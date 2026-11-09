using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// What the focus does when the list is reloaded.
/// </summary>
public enum StackFocusOnReset
{
    /// <summary>The first item is focused (a new search).</summary>
    First,

    /// <summary>The same position stays focused, so a removed item is followed by the next one.</summary>
    KeepPosition,
}

/// <summary>
/// A vertical stack of record sleeves: the focused item is shown big at the top and the next two behind it, smaller
/// and dimmed. Only three item views ever exist, whatever the number of items. Pressing the big sleeve runs
/// <see cref="ActivateCommand"/>; pressing a smaller one, swiping, the mouse wheel or the Up and Down keys (Windows)
/// move the focus one item at a time.
/// </summary>
public class SleeveStack : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IList), typeof(SleeveStack), null, propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(SleeveStack), null, propertyChanged: (b, _, _) => ((SleeveStack)b).CreateSlots());

    public static readonly BindableProperty ItemDisplayPathProperty = BindableProperty.Create(
        nameof(ItemDisplayPath), typeof(string), typeof(SleeveStack));

    public static readonly BindableProperty ActivateCommandProperty = BindableProperty.Create(
        nameof(ActivateCommand), typeof(ICommand), typeof(SleeveStack));

    public static readonly BindableProperty FocusOnResetProperty = BindableProperty.Create(
        nameof(FocusOnReset), typeof(StackFocusOnReset), typeof(SleeveStack), StackFocusOnReset.First);

    public static readonly BindableProperty FocusedItemProperty = BindableProperty.Create(
        nameof(FocusedItem), typeof(object), typeof(SleeveStack), null,
        propertyChanged: (b, _, n) => ((SleeveStack)b).HasFocusedItem = n is not null);

    public static readonly BindableProperty HasFocusedItemProperty = BindableProperty.Create(
        nameof(HasFocusedItem), typeof(bool), typeof(SleeveStack), false);

    // Size and dimming of the focused sleeve and the two behind it.
    private static readonly double[] SlotScales = [1, 0.8, 0.65];
    private static readonly double[] SlotOpacities = [1, 0.75, 0.55];
    private static readonly double[] SlotBottoms = [1, 1.25, 1.45];

    private readonly Grid _root = new();
    private readonly ContentView[] _sleeves = new ContentView[3];
    private readonly Button[] _buttons = new Button[3];
    private int _focusedIndex;

    // The position the user chose; the focused index is this, limited to the items present, so a reload that clears
    // the list and adds the items back one by one ends on the same position.
    private int _wantedIndex;

    public SleeveStack()
    {
        // The descriptions follow the app language.
        Loaded += (_, _) => Localizer.Instance.LanguageChanged += OnLanguageChanged;
        Unloaded += (_, _) => Localizer.Instance.LanguageChanged -= OnLanguageChanged;

        Content = _root;
        _root.SizeChanged += (_, _) => Arrange();

        var swipeUp = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
        swipeUp.Swiped += (_, _) => MoveFocus(1);
        var swipeDown = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
        swipeDown.Swiped += (_, _) => MoveFocus(-1);
        _root.GestureRecognizers.Add(swipeUp);
        _root.GestureRecognizers.Add(swipeDown);

        HandlerChanged += (_, _) => AttachPlatformInput();
    }

    public IList? ItemsSource
    {
        get => (IList?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// The sleeve drawing; its binding context is the item.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// The item's name, used in the accessible descriptions.
    /// </summary>
    public string? ItemDisplayPath
    {
        get => (string?)GetValue(ItemDisplayPathProperty);
        set => SetValue(ItemDisplayPathProperty, value);
    }

    /// <summary>
    /// Run with the focused item when the big sleeve is pressed.
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
    /// The item shown big; null when the list is empty.
    /// </summary>
    public object? FocusedItem
    {
        get => GetValue(FocusedItemProperty);
        private set => SetValue(FocusedItemProperty, value);
    }

    /// <summary>
    /// False while the list is empty, so a panel bound to <see cref="FocusedItem"/> can hide.
    /// </summary>
    public bool HasFocusedItem
    {
        get => (bool)GetValue(HasFocusedItemProperty);
        private set => SetValue(HasFocusedItemProperty, value);
    }

    private int Count => ItemsSource?.Count ?? 0;

    private static void OnItemsSourceChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var stack = (SleeveStack)bindable;
        if (oldValue is INotifyCollectionChanged oldList)
        {
            oldList.CollectionChanged -= stack.OnCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newList)
        {
            newList.CollectionChanged += stack.OnCollectionChanged;
        }

        stack._wantedIndex = 0;
        stack.Refresh();
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
        _root.Children.Clear();

        // Back to front, so the focused sleeve is drawn on top.
        for (var slot = 2; slot >= 0; slot--)
        {
            var sleeve = new ContentView
            {
                Content = ItemTemplate?.CreateContent() as View,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Opacity = SlotOpacities[slot],
                InputTransparent = true,
            };
            AutomationProperties.SetIsInAccessibleTree(sleeve, false);

            var index = slot;
            var button = new Button
            {
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                Padding = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
            };
            button.Clicked += (_, _) => OnSleevePressed(index);
            ButtonSwipe.Attach(button, horizontal: false, MoveFocus);

            _sleeves[slot] = sleeve;
            _buttons[slot] = button;
            _root.Children.Add(sleeve);
            _root.Children.Add(button);
        }

        Arrange();
        Refresh();
    }

    // The focused sleeve is as big as fits, and the two behind it peek out below it.
    private void Arrange()
    {
        if (_sleeves[0] is null || _root.Width <= 0)
        {
            return;
        }

        var size = Math.Min(_root.Width, _root.Height / SlotBottoms[2]);
        for (var slot = 0; slot < 3; slot++)
        {
            var side = size * SlotScales[slot];
            var top = size * SlotBottoms[slot] - side;
            foreach (View view in new View[] { _sleeves[slot], _buttons[slot] })
            {
                view.WidthRequest = view.HeightRequest = side;
                view.TranslationY = top;
            }
        }
    }

    private void Refresh()
    {
        if (_sleeves[0] is null)
        {
            return;
        }

        var count = Count;
        _focusedIndex = count == 0 ? 0 : Math.Clamp(_wantedIndex, 0, count - 1);
        FocusedItem = count == 0 ? null : ItemsSource![_focusedIndex];
        UpdateSlots(count);
    }

    private void UpdateSlots(int count)
    {
        for (var slot = 0; slot < 3; slot++)
        {
            var index = _focusedIndex + slot;
            var item = index < count ? ItemsSource![index] : null;
            _sleeves[slot].BindingContext = item;
            _sleeves[slot].IsVisible = item is not null;
            _buttons[slot].IsVisible = item is not null;

            var name = ItemBinding.Read(item, ItemDisplayPath) as string ?? string.Empty;
            SemanticProperties.SetDescription(_buttons[slot], slot == 0
                ? Localizer.Instance.Format("Fmt_SleeveFront", name, index + 1, count)
                : Localizer.Instance.Format("Fmt_Show", name));
        }
    }

    private void OnSleevePressed(int slot)
    {
        if (slot == 0)
        {
            if (FocusedItem is { } item && ActivateCommand?.CanExecute(item) == true)
            {
                ActivateCommand.Execute(item);
            }

            return;
        }

        MoveFocus(slot);
        _buttons[0].Focus();
    }

    private void MoveFocus(int step)
    {
        var target = Math.Clamp(_focusedIndex + step, 0, Math.Max(0, Count - 1));
        if (target == _focusedIndex)
        {
            return;
        }

        _wantedIndex = target;
        Refresh();

        // A quick fade, well under the 150 ms the theme allows.
        _sleeves[0].CancelAnimations();
        _sleeves[0].Opacity = 0.6;
        _ = _sleeves[0].FadeToAsync(1, 120);
    }

    private void AttachPlatformInput()
    {
#if WINDOWS
        if (Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement view)
        {
            return;
        }

        view.PointerWheelChanged += (_, e) =>
        {
            var delta = e.GetCurrentPoint(view).Properties.MouseWheelDelta;
            MoveFocus(delta < 0 ? 1 : -1);
            e.Handled = true;
        };
        view.AddHandler(Microsoft.UI.Xaml.UIElement.KeyDownEvent, new Microsoft.UI.Xaml.Input.KeyEventHandler((_, e) =>
        {
            if (e.Key is global::Windows.System.VirtualKey.Down or global::Windows.System.VirtualKey.Up)
            {
                MoveFocus(e.Key == global::Windows.System.VirtualKey.Down ? 1 : -1);
                _buttons[0].Focus();
                e.Handled = true;
            }
        }), true);
#endif
    }

    private void OnLanguageChanged(object? sender, EventArgs e) => Refresh();
}
