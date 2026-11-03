using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// A vertical list with a cursor on one item, for the themes browsed with buttons or a wheel. The cursor is the list's
/// selection, so the item template draws it with the "Selected" visual state; only this control moves it.
/// <see cref="MoveCommand"/> and <see cref="PageMoveCommand"/> move it, a touch on another item moves it there, and a
/// touch on the cursor item runs <see cref="ActivateCommand"/> (the template calls <see cref="TapCommand"/>). On
/// Windows, Up and Down move it, Enter activates and the mouse wheel moves it one item per notch.
/// </summary>
public class CursorList : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IList), typeof(CursorList), null, propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(CursorList), null,
        propertyChanged: (b, _, n) => ((CursorList)b)._list.ItemTemplate = (DataTemplate?)n);

    public static readonly BindableProperty ActivateCommandProperty = BindableProperty.Create(
        nameof(ActivateCommand), typeof(ICommand), typeof(CursorList));

    public static readonly BindableProperty FocusOnResetProperty = BindableProperty.Create(
        nameof(FocusOnReset), typeof(StackFocusOnReset), typeof(CursorList), StackFocusOnReset.First);

    public static readonly BindableProperty TapActivatesProperty = BindableProperty.Create(
        nameof(TapActivates), typeof(bool), typeof(CursorList), false);

    public static readonly BindableProperty ItemIsCurrentPathProperty = BindableProperty.Create(
        nameof(ItemIsCurrentPath), typeof(string), typeof(CursorList));

    private static readonly BindablePropertyKey CursorItemPropertyKey = BindableProperty.CreateReadOnly(
        nameof(CursorItem), typeof(object), typeof(CursorList), null);

    public static readonly BindableProperty CursorItemProperty = CursorItemPropertyKey.BindableProperty;

    private static readonly BindablePropertyKey PositionTextPropertyKey = BindableProperty.CreateReadOnly(
        nameof(PositionText), typeof(string), typeof(CursorList), string.Empty);

    public static readonly BindableProperty PositionTextProperty = PositionTextPropertyKey.BindableProperty;

    // A touch the list itself turned into a selection is followed by the template's tap; that tap only moved the
    // cursor, so it must not activate.
    private static readonly TimeSpan TouchWindow = TimeSpan.FromMilliseconds(500);

    private readonly CollectionView _list = new() { SelectionMode = SelectionMode.Single };
    private int _cursorIndex = -1;
    private int _wantedIndex;
    private int _rowsPerScreen = 5;
    private bool _selecting;
    private object? _touchedItem;
    private DateTime _touchedAt;

    public CursorList()
    {
        MoveCommand = new Command<object>(step => Move(ToInt(step)));
        PageMoveCommand = new Command<object>(step => Move(ToInt(step) * RowsPerScreen()));
        TapCommand = new Command<object>(OnTapped);
        ActivateCursorCommand = new Command(Activate);
        ActivateCurrentCommand = new Command(ActivateCurrent);

        _list.SelectionChanged += OnSelectionChanged;
        _list.Scrolled += (_, e) => _rowsPerScreen = Math.Max(1, e.LastVisibleItemIndex - e.FirstVisibleItemIndex);
        _list.HandlerChanged += (_, _) => AttachPlatformInput();
        Content = _list;
    }

    public IList? ItemsSource
    {
        get => (IList?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// The row drawing; its binding context is the item. Its tap gesture calls <see cref="TapCommand"/> with the item.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Run with the cursor item by the theme's OK or centre button, Enter, or a touch on the cursor item.
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
    /// True for lists where a touch plays at once (it moves the cursor there and activates); the keys still move the
    /// cursor first. False (the default): the first touch moves the cursor and a touch on the cursor item activates.
    /// </summary>
    public bool TapActivates
    {
        get => (bool)GetValue(TapActivatesProperty);
        set => SetValue(TapActivatesProperty, value);
    }

    /// <summary>
    /// The item under the cursor; null when the list is empty.
    /// </summary>
    public object? CursorItem => GetValue(CursorItemProperty);

    /// <summary>
    /// Moves the cursor by the parameter's number of items (for example -1 or 1).
    /// </summary>
    public ICommand MoveCommand { get; }

    /// <summary>
    /// Moves the cursor by the parameter's number of screens of items.
    /// </summary>
    public ICommand PageMoveCommand { get; }

    /// <summary>
    /// For the item template: a touch on an item.
    /// </summary>
    public ICommand TapCommand { get; }

    /// <summary>
    /// Runs <see cref="ActivateCommand"/> with the cursor item (a theme's OK or centre button).
    /// </summary>
    public ICommand ActivateCursorCommand { get; }

    /// <summary>
    /// True for the current station's item, for <see cref="ActivateCurrentCommand"/>.
    /// </summary>
    public string? ItemIsCurrentPath
    {
        get => (string?)GetValue(ItemIsCurrentPathProperty);
        set => SetValue(ItemIsCurrentPathProperty, value);
    }

    /// <summary>
    /// Runs <see cref="ActivateCommand"/> with the current station's item (a theme's play/pause button); does nothing
    /// when the current station is not in the list.
    /// </summary>
    public ICommand ActivateCurrentCommand { get; }

    /// <summary>
    /// The cursor's position, for example "3/50"; empty when the list is empty.
    /// </summary>
    public string PositionText => (string)GetValue(PositionTextProperty);

    /// <summary>
    /// Runs <see cref="ActivateCommand"/> with the cursor item.
    /// </summary>
    public void Activate()
    {
        if (CursorItem is { } item && ActivateCommand?.CanExecute(item) == true)
        {
            ActivateCommand.Execute(item);
        }
    }

    private int Count => ItemsSource?.Count ?? 0;

    private void ActivateCurrent()
    {
        for (var i = 0; i < Count; i++)
        {
            var item = ItemsSource![i];
            if (ItemBinding.Read(item, ItemIsCurrentPath) is true)
            {
                if (ActivateCommand?.CanExecute(item) == true)
                {
                    ActivateCommand.Execute(item);
                }

                return;
            }
        }
    }

    private static int ToInt(object? value) => value switch
    {
        int number => number,
        string text when int.TryParse(text, out var number) => number,
        _ => 0,
    };

    private static void OnItemsSourceChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var list = (CursorList)bindable;
        if (oldValue is INotifyCollectionChanged oldItems)
        {
            oldItems.CollectionChanged -= list.OnCollectionChanged;
        }

        // The inner list first, so it already has the items when the cursor selects one.
        list._list.ItemsSource = (IList?)newValue;
        if (newValue is INotifyCollectionChanged newItems)
        {
            newItems.CollectionChanged += list.OnCollectionChanged;
        }

        list._wantedIndex = 0;
        list.Refresh();
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

    private void Move(int step)
    {
        if (Count == 0 || step == 0)
        {
            return;
        }

        _wantedIndex = Math.Clamp(_cursorIndex + step, 0, Count - 1);
        Refresh();
    }

    private void OnTapped(object? item)
    {
        var index = item is null ? -1 : ItemsSource?.IndexOf(item) ?? -1;
        if (index < 0)
        {
            return;
        }

        var justTouched = ReferenceEquals(item, _touchedItem) && DateTime.UtcNow - _touchedAt < TouchWindow;
        _touchedItem = null;
        if (index == _cursorIndex && (!justTouched || TapActivates))
        {
            Activate();
            return;
        }

        _wantedIndex = index;
        Refresh();
        if (TapActivates)
        {
            Activate();
        }
    }

    // The list selected an item by itself (a touch, or the arrow keys on Windows): the cursor follows it.
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_selecting)
        {
            return;
        }

        var item = e.CurrentSelection.FirstOrDefault();
        if (item is null)
        {
            // Selection is never cleared by the user: put the cursor back.
            Refresh(force: true);
            return;
        }

        _touchedItem = item;
        _touchedAt = DateTime.UtcNow;
        _wantedIndex = ItemsSource?.IndexOf(item) ?? 0;
        Refresh();
    }

    private void Refresh(bool force = false)
    {
        var count = Count;
        var index = count == 0 ? -1 : Math.Clamp(_wantedIndex, 0, count - 1);
        var item = index < 0 ? null : ItemsSource![index];
        var moved = index != _cursorIndex || !ReferenceEquals(item, CursorItem);
        _cursorIndex = index;
        SetValue(PositionTextPropertyKey, index < 0 ? string.Empty : $"{index + 1}/{count}");

        if (!moved && !force && ReferenceEquals(_list.SelectedItem, item))
        {
            return;
        }

        SetValue(CursorItemPropertyKey, item);
        _selecting = true;
        try
        {
            _list.SelectedItem = item;
        }
        finally
        {
            _selecting = false;
        }

        if (moved && index >= 0)
        {
            // The cursor changes at once; the list follows it, animated unless the system asks for reduced motion.
            _list.ScrollTo(index, position: ScrollToPosition.MakeVisible, animate: !MotionSettings.IsReducedMotion);
        }
    }

    // The rows on screen, from the last Scrolled event. Windows raises none before the first scroll, so its panel is
    // asked directly.
    private int RowsPerScreen()
    {
#if WINDOWS
        if (_list.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.ListViewBase { ItemsPanelRoot: Microsoft.UI.Xaml.Controls.ItemsStackPanel panel }
            && panel.LastVisibleIndex > panel.FirstVisibleIndex)
        {
            return panel.LastVisibleIndex - panel.FirstVisibleIndex;
        }
#endif
        return _rowsPerScreen;
    }

    private void AttachPlatformInput()
    {
#if WINDOWS
        if (_list.Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement view)
        {
            return;
        }

        // The list scrolls by itself with the wheel; the cursor also moves one item per notch and stays visible.
        view.AddHandler(Microsoft.UI.Xaml.UIElement.PointerWheelChangedEvent, new Microsoft.UI.Xaml.Input.PointerEventHandler((_, e) =>
        {
            var delta = e.GetCurrentPoint(view).Properties.MouseWheelDelta;
            Move(delta < 0 ? 1 : -1);
        }), true);

        // Up and Down usually move the list's selection by themselves (followed above); these cover the case where
        // the list did not handle them.
        view.KeyDown += (_, e) =>
        {
            switch (e.Key)
            {
                case global::Windows.System.VirtualKey.Down:
                    Move(1);
                    e.Handled = true;
                    break;
                case global::Windows.System.VirtualKey.Up:
                    Move(-1);
                    e.Handled = true;
                    break;
                case global::Windows.System.VirtualKey.Enter:
                    Activate();
                    e.Handled = true;
                    break;
            }
        };
#endif
    }
}
