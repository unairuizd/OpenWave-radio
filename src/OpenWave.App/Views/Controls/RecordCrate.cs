using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// A wooden record crate seen from the front, split into two compartments of three records: six items per page, with
/// arrows, swipe and a "1/2" indicator when there are more. Pressing a record runs <see cref="SelectCommand"/>.
/// Logged out, the whole crate is one button that runs <see cref="LogInCommand"/>. Original art for OpenWave; every
/// colour comes from the palette.
/// </summary>
public class RecordCrate : PageAwareView
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IList), typeof(RecordCrate), null, propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(RecordCrate), null, propertyChanged: (b, _, _) => ((RecordCrate)b).Refresh());

    public static readonly BindableProperty ItemDisplayPathProperty = BindableProperty.Create(
        nameof(ItemDisplayPath), typeof(string), typeof(RecordCrate));

    public static readonly BindableProperty ItemIsCurrentPathProperty = BindableProperty.Create(
        nameof(ItemIsCurrentPath), typeof(string), typeof(RecordCrate));

    public static readonly BindableProperty SelectCommandProperty = BindableProperty.Create(
        nameof(SelectCommand), typeof(ICommand), typeof(RecordCrate));

    public static readonly BindableProperty LogInCommandProperty = BindableProperty.Create(
        nameof(LogInCommand), typeof(ICommand), typeof(RecordCrate));

    public static readonly BindableProperty IsLoggedInProperty = BindableProperty.Create(
        nameof(IsLoggedIn), typeof(bool), typeof(RecordCrate), false, propertyChanged: (b, _, _) => ((RecordCrate)b).Refresh());

    public static readonly BindableProperty LoggedOutTextProperty = BindableProperty.Create(
        nameof(LoggedOutText), typeof(string), typeof(RecordCrate), null, propertyChanged: (b, _, _) => ((RecordCrate)b).Refresh());

    public static readonly BindableProperty EmptyTextProperty = BindableProperty.Create(
        nameof(EmptyText), typeof(string), typeof(RecordCrate), null, propertyChanged: (b, _, _) => ((RecordCrate)b).Refresh());

    private const int PerPage = 6;
    private static readonly double[] Tilts = [-4, -1, 3, -3, 1, 4];

    private readonly Grid[] _slots = new Grid[PerPage];
    private readonly ContentView[] _records = new ContentView[PerPage];
    private readonly Button[] _buttons = new Button[PerPage];
    private readonly ImageButton _previous;
    private readonly ImageButton _next;
    private readonly Label _indicator = new() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.End };
    // Text inside a drawn piece wraps instead of making the piece wider.
    private readonly Label _message = new() { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(8, 0) };
    private readonly Button _logInCover = new() { BorderWidth = 0, Padding = new Thickness(8, 0), BackgroundColor = Colors.Transparent, LineBreakMode = LineBreakMode.WordWrap };
    private int _page;

    public RecordCrate()
    {
        // The descriptions follow the app language.
        Loaded += (_, _) => Localizer.Instance.LanguageChanged += OnLanguageChanged;
        Unloaded += (_, _) => Localizer.Instance.LanguageChanged -= OnLanguageChanged;

        _previous = CreateArrow("ic_previous.png", -1);
        _next = CreateArrow("ic_next.png", 1);

        // The box: a dark wooden back, two compartments and a partition, with a lighter front lip.
        var box = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(10)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto),
            ],
            RowDefinitions = [new RowDefinition(GridLength.Star), new RowDefinition(new GridLength(14))],
            Padding = new Thickness(4, 8, 4, 0),
        };
        box.SetDynamicResource(BackgroundColorProperty, "TabBarBackgroundColor");

        var lip = new BoxView();
        lip.SetDynamicResource(BoxView.ColorProperty, "DividerColor");
        Grid.SetRow(lip, 1);
        Grid.SetColumnSpan(lip, 5);

        var partition = new BoxView { Margin = new Thickness(0, 6, 0, 0) };
        partition.SetDynamicResource(BoxView.ColorProperty, "DividerColor");
        Grid.SetColumn(partition, 2);

        var left = CreateCompartment(0);
        var right = CreateCompartment(3);
        Grid.SetColumn(left, 1);
        Grid.SetColumn(right, 3);
        Grid.SetColumn(_previous, 0);
        Grid.SetColumn(_next, 4);

        _indicator.SetDynamicResource(StyleProperty, "SecondaryLabel");
        _indicator.Margin = new Thickness(0, 0, 0, -2);
        Grid.SetRow(_indicator, 1);
        Grid.SetColumnSpan(_indicator, 5);

        _message.SetDynamicResource(StyleProperty, "SecondaryLabel");
        Grid.SetColumnSpan(_message, 5);
        _logInCover.SetDynamicResource(Button.TextColorProperty, "PrimaryTextColor");
        Grid.SetColumnSpan(_logInCover, 5);
        Grid.SetRowSpan(_logInCover, 2);
        _logInCover.Clicked += (_, _) => Run(LogInCommand, null);

        box.Children.Add(left);
        box.Children.Add(partition);
        box.Children.Add(right);
        box.Children.Add(lip);
        box.Children.Add(_previous);
        box.Children.Add(_next);
        box.Children.Add(_indicator);
        box.Children.Add(_message);
        box.Children.Add(_logInCover);

        var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeLeft.Swiped += (_, _) => ChangePage(1);
        var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeRight.Swiped += (_, _) => ChangePage(-1);
        box.GestureRecognizers.Add(swipeLeft);
        box.GestureRecognizers.Add(swipeRight);

        Content = box;
        HandlerChanged += (_, _) => AttachPlatformInput();
        Refresh();
    }

    public IList? ItemsSource
    {
        get => (IList?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// The record drawing (sleeve, logo and caption); its binding context is the item.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public string? ItemDisplayPath
    {
        get => (string?)GetValue(ItemDisplayPathProperty);
        set => SetValue(ItemDisplayPathProperty, value);
    }

    /// <summary>
    /// True for the current station's item: the crate opens on its page.
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

    private int Count => ItemsSource?.Count ?? 0;

    private int PageCount => Math.Max(1, (Count + PerPage - 1) / PerPage);

    protected override void OnShownChanged(bool shown)
    {
        if (shown)
        {
            ShowCurrentPage();
        }
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var crate = (RecordCrate)bindable;
        if (oldValue is INotifyCollectionChanged oldList)
        {
            oldList.CollectionChanged -= crate.OnCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newList)
        {
            newList.CollectionChanged += crate.OnCollectionChanged;
        }

        crate.ShowCurrentPage();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private Grid CreateCompartment(int firstSlot)
    {
        var compartment = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star), new(GridLength.Star)], ColumnSpacing = 4 };
        for (var i = 0; i < 3; i++)
        {
            var slot = firstSlot + i;

            // An empty slot is a dark gap in the crate.
            var gap = new BoxView { CornerRadius = 3, Margin = new Thickness(2, 10, 2, 0) };
            gap.SetDynamicResource(BoxView.ColorProperty, "InputBackgroundColor");

            var record = new ContentView { InputTransparent = true, Rotation = Tilts[slot], AnchorY = 1 };
            AutomationProperties.SetIsInAccessibleTree(record, false);

            var button = new Button { BackgroundColor = Colors.Transparent, BorderWidth = 0, Padding = 0 };
            button.Clicked += (_, _) => OnRecordPressed(slot);

            var cell = new Grid();
            cell.Children.Add(gap);
            cell.Children.Add(record);
            cell.Children.Add(button);
            Grid.SetColumn(cell, i);
            compartment.Children.Add(cell);

            _slots[slot] = cell;
            _records[slot] = record;
            _buttons[slot] = button;
        }

        return compartment;
    }

    private ImageButton CreateArrow(string icon, int step)
    {
        var arrow = new ImageButton { Source = icon, VerticalOptions = LayoutOptions.Center };
        arrow.SetDynamicResource(StyleProperty, "IconButton");
        arrow.SetDynamicResource(OpenWave.App.Behaviors.IconTint.ColorProperty, "TabBarSelectedColor");
        arrow.Clicked += (_, _) => ChangePage(step);
        return arrow;
    }

    // Opens on the page holding the current station, or the first page.
    private void ShowCurrentPage()
    {
        _page = 0;
        for (var i = 0; i < Count; i++)
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
        var target = Math.Clamp(_page + step, 0, PageCount - 1);
        if (target != _page)
        {
            _page = target;
            Refresh();
        }
    }

    private void Refresh()
    {
        SemanticProperties.SetDescription(_previous, Localizer.Instance["Favoritos_PreviousPage"]);
        SemanticProperties.SetDescription(_next, Localizer.Instance["Favoritos_NextPage"]);
        if (_records[0] is null)
        {
            return;
        }

        var count = IsLoggedIn ? Count : 0;
        _page = Math.Clamp(_page, 0, PageCount - 1);

        for (var slot = 0; slot < PerPage; slot++)
        {
            var index = _page * PerPage + slot;
            var item = index < count ? ItemsSource![index] : null;
            var record = _records[slot];
            if (record.Content is null && ItemTemplate is not null)
            {
                record.Content = ItemTemplate.CreateContent() as View;
            }

            record.BindingContext = item;
            record.IsVisible = item is not null;
            _buttons[slot].IsVisible = item is not null;
            var name = ItemBinding.Read(item, ItemDisplayPath) as string ?? string.Empty;
            SemanticProperties.SetDescription(_buttons[slot], Localizer.Instance.Format("Fmt_PlayFavorite", name, index + 1, count));
        }

        var paged = count > PerPage;
        _previous.IsVisible = _next.IsVisible = _indicator.IsVisible = paged;
        _indicator.Text = $"{_page + 1}/{PageCount}";

        _logInCover.IsVisible = !IsLoggedIn;
        _logInCover.Text = LoggedOutText;
        SemanticProperties.SetDescription(_logInCover, LoggedOutText);
        _message.IsVisible = IsLoggedIn && count == 0;
        _message.Text = EmptyText;
    }

    private void OnRecordPressed(int slot)
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

    // Windows: Left and Right move between the records, changing page at the ends.
    private void AttachPlatformInput()
    {
#if WINDOWS
        if (Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement view)
        {
            return;
        }

        view.AddHandler(Microsoft.UI.Xaml.UIElement.KeyDownEvent, new Microsoft.UI.Xaml.Input.KeyEventHandler((_, e) =>
        {
            if (e.Key is not (global::Windows.System.VirtualKey.Left or global::Windows.System.VirtualKey.Right))
            {
                return;
            }

            var focused = Array.FindIndex(_buttons, b => b.IsFocused);
            if (focused < 0)
            {
                return;
            }

            var step = e.Key == global::Windows.System.VirtualKey.Right ? 1 : -1;
            var target = focused + step;
            var visible = Math.Min(PerPage, Count - _page * PerPage);
            if (target < 0 && _page > 0)
            {
                ChangePage(-1);
                target = PerPage - 1;
            }
            else if (target >= visible && _page < PageCount - 1)
            {
                ChangePage(1);
                target = 0;
            }

            target = Math.Clamp(target, 0, Math.Max(0, Math.Min(PerPage, Count - _page * PerPage) - 1));
            _buttons[target].Focus();
            e.Handled = true;
        }), true);
#endif
    }

    private void OnLanguageChanged(object? sender, EventArgs e) => Refresh();
}
