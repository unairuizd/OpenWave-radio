using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// One slot of a <see cref="PagedSlots"/>: its binding context is the item, and it tells the item template the
/// item's number in the whole list and on the page, for templates that show "1" or "M1".
/// </summary>
public class PagedSlot : ContentView
{
    public static readonly BindableProperty NumberProperty = BindableProperty.Create(
        nameof(Number), typeof(int), typeof(PagedSlot), 0);

    public static readonly BindableProperty SlotNumberProperty = BindableProperty.Create(
        nameof(SlotNumber), typeof(int), typeof(PagedSlot), 0);

    /// <summary>
    /// The item's position in the whole list, from 1.
    /// </summary>
    public int Number
    {
        get => (int)GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }

    /// <summary>
    /// The slot's position on the page, from 1.
    /// </summary>
    public int SlotNumber
    {
        get => (int)GetValue(SlotNumberProperty);
        set => SetValue(SlotNumberProperty, value);
    }
}

/// <summary>
/// A grid of <see cref="Columns"/> × <see cref="Rows"/> slots showing a page of the items, for the favourites holders
/// of several themes (preset keys, memories, pads, a J-card). Pressing a filled slot runs <see cref="SelectCommand"/>;
/// a Left/Right swipe changes the page, and the layout draws its own arrows and indicator from
/// <see cref="HasPages"/>, <see cref="PageText"/> and the page commands. It opens on the page holding the current item
/// and stays on a valid page after removals. Logged out, the whole control is one button that runs
/// <see cref="LogInCommand"/>.
/// </summary>
public class PagedSlots : PageAwareView
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IList), typeof(PagedSlots), null, propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(PagedSlots), null, propertyChanged: (b, _, _) => ((PagedSlots)b).CreateSlots());

    public static readonly BindableProperty EmptySlotTemplateProperty = BindableProperty.Create(
        nameof(EmptySlotTemplate), typeof(DataTemplate), typeof(PagedSlots), null, propertyChanged: (b, _, _) => ((PagedSlots)b).CreateSlots());

    public static readonly BindableProperty ColumnsProperty = BindableProperty.Create(
        nameof(Columns), typeof(int), typeof(PagedSlots), 1, propertyChanged: (b, _, _) => ((PagedSlots)b).CreateSlots());

    public static readonly BindableProperty RowsProperty = BindableProperty.Create(
        nameof(Rows), typeof(int), typeof(PagedSlots), 1, propertyChanged: (b, _, _) => ((PagedSlots)b).CreateSlots());

    public static readonly BindableProperty ColumnMajorProperty = BindableProperty.Create(
        nameof(ColumnMajor), typeof(bool), typeof(PagedSlots), false, propertyChanged: (b, _, _) => ((PagedSlots)b).CreateSlots());

    public static readonly BindableProperty SlotSpacingProperty = BindableProperty.Create(
        nameof(SlotSpacing), typeof(double), typeof(PagedSlots), 6.0, propertyChanged: (b, _, _) => ((PagedSlots)b).CreateSlots());

    public static readonly BindableProperty ItemDisplayPathProperty = BindableProperty.Create(
        nameof(ItemDisplayPath), typeof(string), typeof(PagedSlots), null, propertyChanged: (b, _, _) => ((PagedSlots)b).Refresh());

    public static readonly BindableProperty ItemDescriptionFormatProperty = BindableProperty.Create(
        nameof(ItemDescriptionFormat), typeof(string), typeof(PagedSlots), null,
        propertyChanged: (b, _, _) => ((PagedSlots)b).Refresh());

    public static readonly BindableProperty ArrowKeysMoveFocusProperty = BindableProperty.Create(
        nameof(ArrowKeysMoveFocus), typeof(bool), typeof(PagedSlots), false);

    public static readonly BindableProperty MessageTextColorProperty = BindableProperty.Create(
        nameof(MessageTextColor), typeof(Color), typeof(PagedSlots), null, propertyChanged: OnMessageTextColorChanged);

    public static readonly BindableProperty ItemIsCurrentPathProperty = BindableProperty.Create(
        nameof(ItemIsCurrentPath), typeof(string), typeof(PagedSlots));

    public static readonly BindableProperty SelectCommandProperty = BindableProperty.Create(
        nameof(SelectCommand), typeof(ICommand), typeof(PagedSlots));

    public static readonly BindableProperty LogInCommandProperty = BindableProperty.Create(
        nameof(LogInCommand), typeof(ICommand), typeof(PagedSlots));

    public static readonly BindableProperty IsLoggedInProperty = BindableProperty.Create(
        nameof(IsLoggedIn), typeof(bool), typeof(PagedSlots), false, propertyChanged: (b, _, _) => ((PagedSlots)b).Refresh());

    public static readonly BindableProperty LoggedOutTextProperty = BindableProperty.Create(
        nameof(LoggedOutText), typeof(string), typeof(PagedSlots), null, propertyChanged: (b, _, _) => ((PagedSlots)b).Refresh());

    public static readonly BindableProperty EmptyTextProperty = BindableProperty.Create(
        nameof(EmptyText), typeof(string), typeof(PagedSlots), null, propertyChanged: (b, _, _) => ((PagedSlots)b).Refresh());

    private static readonly BindablePropertyKey PageIndexPropertyKey = BindableProperty.CreateReadOnly(
        nameof(PageIndex), typeof(int), typeof(PagedSlots), 0);

    private static readonly BindablePropertyKey PageCountPropertyKey = BindableProperty.CreateReadOnly(
        nameof(PageCount), typeof(int), typeof(PagedSlots), 1);

    private static readonly BindablePropertyKey PageTextPropertyKey = BindableProperty.CreateReadOnly(
        nameof(PageText), typeof(string), typeof(PagedSlots), "1/1");

    private static readonly BindablePropertyKey HasPagesPropertyKey = BindableProperty.CreateReadOnly(
        nameof(HasPages), typeof(bool), typeof(PagedSlots), false);

    public static readonly BindableProperty PageIndexProperty = PageIndexPropertyKey.BindableProperty;
    public static readonly BindableProperty PageCountProperty = PageCountPropertyKey.BindableProperty;
    public static readonly BindableProperty PageTextProperty = PageTextPropertyKey.BindableProperty;
    public static readonly BindableProperty HasPagesProperty = HasPagesPropertyKey.BindableProperty;

    private readonly Grid _root = new();
    private readonly Grid _grid = new();
    // Text inside a drawn piece wraps instead of making the piece wider.
    private readonly Label _message = new() { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(8, 0) };
    private readonly Button _logInCover = new() { BorderWidth = 0, Padding = new Thickness(8, 0), BackgroundColor = Colors.Transparent, LineBreakMode = LineBreakMode.WordWrap };
    private PagedSlot[] _slots = [];
    private View?[] _emptyViews = [];
    private Button[] _buttons = [];
    private int _page;

    public PagedSlots()
    {
        // The descriptions follow the app language.
        Loaded += (_, _) => Localizer.Instance.LanguageChanged += OnLanguageChanged;
        Unloaded += (_, _) => Localizer.Instance.LanguageChanged -= OnLanguageChanged;

        NextPageCommand = new RelayCommand(() => ChangePage(1));
        PreviousPageCommand = new RelayCommand(() => ChangePage(-1));
        TurnPageCommand = new RelayCommand(TurnPage);

        _message.SetDynamicResource(StyleProperty, "SecondaryLabel");
        _logInCover.SetDynamicResource(Button.TextColorProperty, "PrimaryTextColor");
        _logInCover.Clicked += (_, _) => Run(LogInCommand, null);
        HandlerChanged += (_, _) => AttachPlatformInput();

        _root.Children.Add(_grid);
        _root.Children.Add(_message);
        _root.Children.Add(_logInCover);

        var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeLeft.Swiped += (_, _) => ChangePage(1);
        var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeRight.Swiped += (_, _) => ChangePage(-1);
        _root.GestureRecognizers.Add(swipeLeft);
        _root.GestureRecognizers.Add(swipeRight);

        Content = _root;
        CreateSlots();
    }

    public IList? ItemsSource
    {
        get => (IList?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// How a filled slot is drawn; its binding context is the item, and the <see cref="PagedSlot"/> around it gives
    /// the numbers. Buttons inside it (for example a remove button) keep working; the rest of the slot selects.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// How an empty slot is drawn (also while logged out or with no items).
    /// </summary>
    public DataTemplate? EmptySlotTemplate
    {
        get => (DataTemplate?)GetValue(EmptySlotTemplateProperty);
        set => SetValue(EmptySlotTemplateProperty, value);
    }

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    /// <summary>
    /// Fill the slots column by column (the J-card's two sides) instead of row by row.
    /// </summary>
    public bool ColumnMajor
    {
        get => (bool)GetValue(ColumnMajorProperty);
        set => SetValue(ColumnMajorProperty, value);
    }

    public double SlotSpacing
    {
        get => (double)GetValue(SlotSpacingProperty);
        set => SetValue(SlotSpacingProperty, value);
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
    /// Windows: false (the default) makes Left/Right change the page; true makes them move the focus from slot to slot,
    /// changing page at the ends, like a row of buttons.
    /// </summary>
    public bool ArrowKeysMoveFocus
    {
        get => (bool)GetValue(ArrowKeysMoveFocusProperty);
        set => SetValue(ArrowKeysMoveFocusProperty, value);
    }

    /// <summary>
    /// The colour of the logged-out and empty texts, for slots drawn on a light card; by default they use the
    /// secondary and primary text colours.
    /// </summary>
    public Color? MessageTextColor
    {
        get => (Color?)GetValue(MessageTextColorProperty);
        set => SetValue(MessageTextColorProperty, value);
    }

    /// <summary>
    /// Accessible name of a filled slot: {0} is the item's name, {1} its number in the list, {2} the count, {3} its
    /// position in its column, {4} its column's letter (A, B, …) and {5} the slot's number on the page.
    /// </summary>
    public string? ItemDescriptionFormat
    {
        get => (string?)GetValue(ItemDescriptionFormatProperty);
        set => SetValue(ItemDescriptionFormatProperty, value);
    }

    /// <summary>
    /// True for the current station's item: the control opens on its page.
    /// </summary>
    public string? ItemIsCurrentPath
    {
        get => (string?)GetValue(ItemIsCurrentPathProperty);
        set => SetValue(ItemIsCurrentPathProperty, value);
    }

    public ICommand? SelectCommand
    {
        get => (ICommand?)GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
    }

    public ICommand? LogInCommand
    {
        get => (ICommand?)GetValue(LogInCommandProperty);
        set => SetValue(LogInCommandProperty, value);
    }

    public bool IsLoggedIn
    {
        get => (bool)GetValue(IsLoggedInProperty);
        set => SetValue(IsLoggedInProperty, value);
    }

    public string? LoggedOutText
    {
        get => (string?)GetValue(LoggedOutTextProperty);
        set => SetValue(LoggedOutTextProperty, value);
    }

    public string? EmptyText
    {
        get => (string?)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    /// <summary>
    /// The page shown, from 0.
    /// </summary>
    public int PageIndex => (int)GetValue(PageIndexProperty);

    public int PageCount => (int)GetValue(PageCountProperty);

    /// <summary>
    /// For example "1/2".
    /// </summary>
    public string PageText => (string)GetValue(PageTextProperty);

    /// <summary>
    /// True when the items do not fit in one page, so the layout shows its arrows and indicator.
    /// </summary>
    public bool HasPages => (bool)GetValue(HasPagesProperty);

    public ICommand NextPageCommand { get; }

    public ICommand PreviousPageCommand { get; }

    /// <summary>
    /// Turns the card over: the next page, or the first after the last one. The slots flip in about 0.3 s (at once
    /// with reduced motion).
    /// </summary>
    public ICommand TurnPageCommand { get; }

    private int PerPage => Math.Max(1, Columns * Rows);

    private int Count => IsLoggedIn ? ItemsSource?.Count ?? 0 : 0;

    protected override void OnShownChanged(bool shown)
    {
        if (shown)
        {
            ShowCurrentPage();
        }
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var slots = (PagedSlots)bindable;
        if (oldValue is INotifyCollectionChanged oldList)
        {
            oldList.CollectionChanged -= slots.OnCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newList)
        {
            newList.CollectionChanged += slots.OnCollectionChanged;
        }

        slots.ShowCurrentPage();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private static void OnMessageTextColorChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var slots = (PagedSlots)bindable;
        if (newValue is Color color)
        {
            slots._message.TextColor = color;
            slots._logInCover.TextColor = color;
        }
    }

    private void CreateSlots()
    {
        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();
        _grid.ColumnSpacing = _grid.RowSpacing = SlotSpacing;
        for (var c = 0; c < Columns; c++)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        for (var r = 0; r < Rows; r++)
        {
            _grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        }

        _slots = new PagedSlot[PerPage];
        _emptyViews = new View?[PerPage];
        _buttons = new Button[PerPage];
        for (var i = 0; i < PerPage; i++)
        {
            // The button lies under the drawing, which lets touches through except on its own buttons
            // (CascadeInputTransparent only works on a layout, hence the holder grid).
            var index = i;
            var button = new Button { BackgroundColor = Colors.Transparent, BorderWidth = 0, Padding = 0 };
            button.Clicked += (_, _) => OnSlotPressed(index);
            ButtonSwipe.Attach(button, horizontal: true, ChangePage);

            var slot = new PagedSlot
            {
                Content = ItemTemplate?.CreateContent() as View,
                SlotNumber = i + 1,
            };
            var holder = new Grid { InputTransparent = true, CascadeInputTransparent = false, Children = { slot } };
            var empty = EmptySlotTemplate?.CreateContent() as View;
            if (empty is not null)
            {
                empty.InputTransparent = true;
                AutomationProperties.SetIsInAccessibleTree(empty, false);
            }

            var cell = new Grid();
            if (empty is not null)
            {
                cell.Children.Add(empty);
            }

            cell.Children.Add(button);
            cell.Children.Add(holder);
            Grid.SetColumn(cell, ColumnMajor ? i / Rows : i % Columns);
            Grid.SetRow(cell, ColumnMajor ? i % Rows : i / Columns);
            _grid.Children.Add(cell);

            _slots[i] = slot;
            _emptyViews[i] = empty;
            _buttons[i] = button;
        }

        Refresh();
    }

    // Opens on the page holding the current item, or the first page.
    private void ShowCurrentPage()
    {
        _page = 0;
        var count = Count;
        for (var i = 0; i < count; i++)
        {
            if (ItemBinding.Read(ItemsSource![i], ItemIsCurrentPath) is true)
            {
                _page = i / PerPage;
                break;
            }
        }

        Refresh();
    }

    private void ChangePage(int step)
    {
        var target = Math.Clamp(_page + step, 0, PageCountFor(Count) - 1);
        if (target != _page)
        {
            _page = target;
            Refresh();
        }
    }

    private async void TurnPage()
    {
        var pageCount = PageCountFor(Count);
        if (pageCount < 2)
        {
            return;
        }

        var target = (_page + 1) % pageCount;
        if (MotionSettings.IsReducedMotion)
        {
            _page = target;
            Refresh();
            return;
        }

        _grid.CancelAnimations();
        await _grid.RotateYToAsync(90, 150, Easing.CubicIn);
        _page = target;
        Refresh();
        _grid.RotationY = -90;
        await _grid.RotateYToAsync(0, 150, Easing.CubicOut);
    }

    private int PageCountFor(int count) => Math.Max(1, (count + PerPage - 1) / PerPage);

    private void Refresh()
    {
        if (_slots.Length != PerPage)
        {
            return;
        }

        var count = Count;
        var pageCount = PageCountFor(count);
        _page = Math.Clamp(_page, 0, pageCount - 1);

        for (var i = 0; i < PerPage; i++)
        {
            var index = _page * PerPage + i;
            var item = index < count ? ItemsSource![index] : null;
            var slot = _slots[i];
            slot.BindingContext = item;
            slot.Number = index + 1;
            slot.IsVisible = item is not null;
            _buttons[i].IsVisible = item is not null;
            if (_emptyViews[i] is { } empty)
            {
                empty.IsVisible = item is null;
            }

            var name = ItemBinding.Read(item, ItemDisplayPath) as string ?? string.Empty;
            var column = ColumnMajor ? i / Rows : i % Columns;
            var positionInColumn = (ColumnMajor ? i % Rows : i / Columns) + 1;
            SemanticProperties.SetDescription(_buttons[i], string.Format(ItemDescriptionFormat ?? Localizer.Instance["Fmt_PlayFavorite"], name, index + 1, count, positionInColumn, (char)('A' + column), i + 1));
        }

        SetValue(PageIndexPropertyKey, _page);
        SetValue(PageCountPropertyKey, pageCount);
        SetValue(PageTextPropertyKey, $"{_page + 1}/{pageCount}");
        SetValue(HasPagesPropertyKey, count > PerPage);

        _logInCover.IsVisible = !IsLoggedIn;
        _logInCover.Text = LoggedOutText;
        SemanticProperties.SetDescription(_logInCover, LoggedOutText);
        _message.IsVisible = IsLoggedIn && count == 0;
        _message.Text = EmptyText;
    }

    private void OnSlotPressed(int slot)
    {
        var index = _page * PerPage + slot;
        if (index < Count)
        {
            Run(SelectCommand, ItemsSource![index]);
        }
    }

    private static void Run(ICommand? command, object? parameter)
    {
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }

    // Moves the focus to the next or previous filled slot, changing page at the ends.
    private void MoveFocus(int step)
    {
        var focused = Array.FindIndex(_buttons, b => b.IsFocused);
        if (focused < 0)
        {
            return;
        }

        var target = focused + step;
        if (target < 0 && _page > 0)
        {
            ChangePage(-1);
            target = PerPage - 1;
        }
        else if (target >= PerPage && _page < PageCountFor(Count) - 1)
        {
            ChangePage(1);
            target = 0;
        }

        var filled = Math.Min(PerPage, Count - _page * PerPage);
        _buttons[Math.Clamp(target, 0, Math.Max(0, filled - 1))].Focus();
    }

    // Windows: Left and Right change the page, or move the focus, while the focus is inside the slots.
    private void AttachPlatformInput()
    {
#if WINDOWS
        if (Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement view)
        {
            return;
        }

        view.KeyDown += (_, e) =>
        {
            if (e.Key is global::Windows.System.VirtualKey.Left or global::Windows.System.VirtualKey.Right)
            {
                var step = e.Key == global::Windows.System.VirtualKey.Right ? 1 : -1;
                if (ArrowKeysMoveFocus)
                {
                    MoveFocus(step);
                }
                else
                {
                    ChangePage(step);
                }

                e.Handled = true;
            }
        };
#endif
    }

    private void OnLanguageChanged(object? sender, EventArgs e) => Refresh();
}
