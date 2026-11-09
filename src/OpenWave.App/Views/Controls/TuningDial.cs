using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// One label position on a <see cref="TuningDial"/>: its binding context is the item, and <see cref="IsTuned"/> is
/// true for the label under the needle, so the template can draw it larger and in the lit window.
/// </summary>
public class DialSlot : ContentView
{
    public static readonly BindableProperty IsTunedProperty = BindableProperty.Create(
        nameof(IsTuned), typeof(bool), typeof(DialSlot), false);

    public bool IsTuned
    {
        get => (bool)GetValue(IsTunedProperty);
        set => SetValue(IsTunedProperty, value);
    }
}

/// <summary>
/// A horizontal tuning dial: the focused station sits under a fixed needle in the middle, with two neighbours on each
/// side. Only five label views exist, whatever the number of stations. The focus moves one station at a time with a
/// swipe on the dial, the <see cref="TuningKnob"/> (press, swipe, or Left/Right on Windows) and the mouse wheel on
/// Windows; the labels slide to the new focus in about 250 ms (at once with reduced motion). Pressing the focused label,
/// or Enter/Space on the knob, runs <see cref="ActivateCommand"/>; pressing a neighbour tunes to it. The layout draws
/// the glass, the scale and the needle around it.
/// </summary>
public class TuningDial : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IList), typeof(TuningDial), null, propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(TuningDial), null, propertyChanged: (b, _, _) => ((TuningDial)b).CreateSlots());

    public static readonly BindableProperty ItemDisplayPathProperty = BindableProperty.Create(
        nameof(ItemDisplayPath), typeof(string), typeof(TuningDial), null, propertyChanged: (b, _, _) => ((TuningDial)b).Refresh());

    public static readonly BindableProperty ActivateCommandProperty = BindableProperty.Create(
        nameof(ActivateCommand), typeof(ICommand), typeof(TuningDial));

    public static readonly BindableProperty FocusOnResetProperty = BindableProperty.Create(
        nameof(FocusOnReset), typeof(StackFocusOnReset), typeof(TuningDial), StackFocusOnReset.First);

    public static readonly BindableProperty UnderlayProperty = BindableProperty.Create(
        nameof(Underlay), typeof(View), typeof(TuningDial), null, propertyChanged: (b, _, _) => ((TuningDial)b).CreateSlots());

    public static readonly BindableProperty OverlayProperty = BindableProperty.Create(
        nameof(Overlay), typeof(View), typeof(TuningDial), null, propertyChanged: (b, _, _) => ((TuningDial)b).CreateSlots());

    public static readonly BindableProperty KnobSizeProperty = BindableProperty.Create(
        nameof(KnobSize), typeof(double), typeof(TuningDial), 64.0, propertyChanged: (b, _, n) => ((TuningDial)b)._knob.WidthRequest = ((TuningDial)b)._knob.HeightRequest = (double)n);

    private static readonly BindablePropertyKey FocusedItemPropertyKey = BindableProperty.CreateReadOnly(
        nameof(FocusedItem), typeof(object), typeof(TuningDial), null);

    private static readonly BindablePropertyKey FocusedIndexPropertyKey = BindableProperty.CreateReadOnly(
        nameof(FocusedIndex), typeof(int), typeof(TuningDial), -1);

    private static readonly BindablePropertyKey PositionTextPropertyKey = BindableProperty.CreateReadOnly(
        nameof(PositionText), typeof(string), typeof(TuningDial), string.Empty);

    private static readonly BindablePropertyKey HasFocusedItemPropertyKey = BindableProperty.CreateReadOnly(
        nameof(HasFocusedItem), typeof(bool), typeof(TuningDial), false);

    public static readonly BindableProperty FocusedItemProperty = FocusedItemPropertyKey.BindableProperty;
    public static readonly BindableProperty FocusedIndexProperty = FocusedIndexPropertyKey.BindableProperty;
    public static readonly BindableProperty PositionTextProperty = PositionTextPropertyKey.BindableProperty;
    public static readonly BindableProperty HasFocusedItemProperty = HasFocusedItemPropertyKey.BindableProperty;

    private const int SlotCount = 5;
    private const int Centre = SlotCount / 2;
    private const uint SlideMilliseconds = 250;

    private readonly Grid _strip = new();
    private readonly TuningKnob _knob = new() { VerticalOptions = LayoutOptions.Center };
    private readonly DialSlot[] _slots = new DialSlot[SlotCount];
    private readonly Button[] _buttons = new Button[SlotCount];
    private int _focusedIndex = -1;
    private int _wantedIndex;

    public TuningDial()
    {
        // The descriptions follow the app language.
        Loaded += (_, _) => Localizer.Instance.LanguageChanged += OnLanguageChanged;
        Unloaded += (_, _) => Localizer.Instance.LanguageChanged -= OnLanguageChanged;

        _knob.WidthRequest = _knob.HeightRequest = KnobSize;
        _knob.Stepped += (_, step) => Move(step);
        _knob.Activated += (_, _) => Activate();

        var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeLeft.Swiped += (_, _) => Move(1);
        var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeRight.Swiped += (_, _) => Move(-1);
        _strip.GestureRecognizers.Add(swipeLeft);
        _strip.GestureRecognizers.Add(swipeRight);
        _strip.SizeChanged += (_, _) => Arrange(0);
        _strip.HandlerChanged += (_, _) => AttachPlatformInput();

        var root = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)], ColumnSpacing = 12 };
        root.Children.Add(_strip);
        Grid.SetColumn(_knob, 1);
        root.Children.Add(_knob);
        Content = root;
        CreateSlots();
    }

    public IList? ItemsSource
    {
        get => (IList?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// The label drawing; its binding context is the item and the <see cref="DialSlot"/> around it says whether it
    /// is under the needle.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// The item's name, used in the knob's accessible name.
    /// </summary>
    public string? ItemDisplayPath
    {
        get => (string?)GetValue(ItemDisplayPathProperty);
        set => SetValue(ItemDisplayPathProperty, value);
    }

    /// <summary>
    /// Run with the focused item when its label is pressed, or with Enter or Space on the knob.
    /// </summary>
    public ICommand? ActivateCommand
    {
        get => (ICommand?)GetValue(ActivateCommandProperty);
        set => SetValue(ActivateCommandProperty, value);
    }

    public StackFocusOnReset FocusOnReset
    {
        get => (StackFocusOnReset)GetValue(FocusOnResetProperty);
        set => SetValue(FocusOnResetProperty, value);
    }

    /// <summary>
    /// Drawn behind the labels, over the strip only (not the knob), for example the lit window in the middle fifth.
    /// </summary>
    public View? Underlay
    {
        get => (View?)GetValue(UnderlayProperty);
        set => SetValue(UnderlayProperty, value);
    }

    /// <summary>
    /// Drawn in front of the labels, over the strip only, for example the needle. It never takes touches.
    /// </summary>
    public View? Overlay
    {
        get => (View?)GetValue(OverlayProperty);
        set => SetValue(OverlayProperty, value);
    }

    public double KnobSize
    {
        get => (double)GetValue(KnobSizeProperty);
        set => SetValue(KnobSizeProperty, value);
    }

    /// <summary>
    /// The station under the needle; null when the list is empty.
    /// </summary>
    public object? FocusedItem => GetValue(FocusedItemProperty);

    public int FocusedIndex => (int)GetValue(FocusedIndexProperty);

    /// <summary>
    /// For example "emisora 3 de 48"; empty when the list is empty.
    /// </summary>
    public string PositionText => (string)GetValue(PositionTextProperty);

    public bool HasFocusedItem => (bool)GetValue(HasFocusedItemProperty);

    private int Count => ItemsSource?.Count ?? 0;

    private static void OnItemsSourceChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var dial = (TuningDial)bindable;
        if (oldValue is INotifyCollectionChanged oldList)
        {
            oldList.CollectionChanged -= dial.OnCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newList)
        {
            newList.CollectionChanged += dial.OnCollectionChanged;
        }

        dial._wantedIndex = 0;
        dial.Refresh();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // The pages reload their lists with a Reset followed by one Add per item.
        if (e.Action == NotifyCollectionChangedAction.Reset && FocusOnReset == StackFocusOnReset.First)
        {
            _wantedIndex = 0;
        }

        Refresh();
    }

    private void CreateSlots()
    {
        _strip.Children.Clear();
        if (Underlay is { } underlay)
        {
            underlay.InputTransparent = true;
            _strip.Children.Add(underlay);
        }

        for (var i = 0; i < SlotCount; i++)
        {
            var slot = new DialSlot
            {
                Content = ItemTemplate?.CreateContent() as View,
                IsTuned = i == Centre,
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.Center,
            };
            AutomationProperties.SetIsInAccessibleTree(slot, false);

            // The labels are reached by touch; screen readers and the keyboard use the knob instead.
            var offset = i - Centre;
            var button = new Button { BackgroundColor = Colors.Transparent, BorderWidth = 0, Padding = 0, HorizontalOptions = LayoutOptions.Center };
            AutomationProperties.SetIsInAccessibleTree(button, false);
#if WINDOWS
            button.HandlerChanged += (_, _) =>
            {
                if (button.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Control control)
                {
                    control.IsTabStop = false;
                }
            };
#endif
            button.Clicked += (_, _) => OnLabelPressed(offset);
            ButtonSwipe.Attach(button, horizontal: true, Move);

            _slots[i] = slot;
            _buttons[i] = button;
            _strip.Children.Add(slot);
            _strip.Children.Add(button);
        }

        if (Overlay is { } overlay)
        {
            overlay.InputTransparent = true;
            _strip.Children.Add(overlay);
        }

        Arrange(0);
        Refresh();
    }

    // Each label takes a fifth of the strip; slideFrom (in label widths) starts the labels that far off and slides
    // them into place.
    private void Arrange(int slideFrom)
    {
        if (_slots[0] is null || _strip.Width <= 0)
        {
            return;
        }

        var width = _strip.Width / SlotCount;
        var animate = slideFrom != 0 && !MotionSettings.IsReducedMotion;
        for (var i = 0; i < SlotCount; i++)
        {
            var x = (i - Centre) * width;
            foreach (View view in new View[] { _slots[i], _buttons[i] })
            {
                view.WidthRequest = width;
                view.CancelAnimations();
                if (animate)
                {
                    view.TranslationX = x + slideFrom * width;
                    _ = view.TranslateToAsync(x, 0, SlideMilliseconds, Easing.CubicOut);
                }
                else
                {
                    view.TranslationX = x;
                }
            }
        }
    }

    private void Refresh()
    {
        if (_slots[0] is null)
        {
            return;
        }

        var count = Count;
        _focusedIndex = count == 0 ? -1 : Math.Clamp(_wantedIndex, 0, count - 1);
        var focused = _focusedIndex < 0 ? null : ItemsSource![_focusedIndex];
        SetValue(FocusedItemPropertyKey, focused);
        SetValue(FocusedIndexPropertyKey, _focusedIndex);
        SetValue(HasFocusedItemPropertyKey, focused is not null);
        SetValue(PositionTextPropertyKey, focused is null ? string.Empty : Localizer.Instance.Format("Fmt_StationPosition", _focusedIndex + 1, count));

        for (var i = 0; i < SlotCount; i++)
        {
            var index = _focusedIndex + i - Centre;
            var item = _focusedIndex >= 0 && index >= 0 && index < count ? ItemsSource![index] : null;
            _slots[i].BindingContext = item;
            _slots[i].IsVisible = item is not null;
            _buttons[i].IsVisible = item is not null;
        }

        var name = ItemBinding.Read(focused, ItemDisplayPath) as string ?? string.Empty;
        SemanticProperties.SetDescription(_knob.Button, focused is null
            ? Localizer.Instance["Dial_Tune"]
            : Localizer.Instance.Format("Fmt_Tune", _focusedIndex + 1, count, name));
        _knob.IsEnabled = count > 0;
    }

    private void Move(int step)
    {
        var target = Math.Clamp(_focusedIndex + step, 0, Math.Max(0, Count - 1));
        if (Count == 0 || target == _focusedIndex)
        {
            return;
        }

        var moved = target - _focusedIndex;
        _wantedIndex = target;
        Refresh();
        _knob.Turn(moved);
        Arrange(Math.Clamp(moved, -1, 1));
    }

    private void OnLabelPressed(int offset)
    {
        if (offset == 0)
        {
            Activate();
        }
        else
        {
            Move(offset);
        }
    }

    private void Activate()
    {
        if (FocusedItem is { } item && ActivateCommand?.CanExecute(item) == true)
        {
            ActivateCommand.Execute(item);
        }
    }

    private void AttachPlatformInput()
    {
#if WINDOWS
        if (_strip.Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement view)
        {
            return;
        }

        view.PointerWheelChanged += (_, e) =>
        {
            var delta = e.GetCurrentPoint(view).Properties.MouseWheelDelta;
            Move(delta < 0 ? 1 : -1);
            e.Handled = true;
        };
#endif
    }

    private void OnLanguageChanged(object? sender, EventArgs e) => Refresh();
}
