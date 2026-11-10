using CommunityToolkit.Maui.Views;
using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class WelcomePopup : Popup<WelcomeChoice>
{
    // What the pop-up adds around its scroll: its margin, the card's padding and some scrim left visible.
    private const double ContentMargin = 72;

    private Page? _page;

    public WelcomePopup(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.Chosen += OnChosen;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnChosen(object? sender, WelcomeChoice choice)
    {
        await CloseAsync(choice);
    }

    // The toolkit sizes a pop-up to its content, so the scroll inside needs a cap: the height of the page behind it
    // (the window's content, without the title bar), kept up to date while the window is resized.
    private void OnLoaded(object? sender, EventArgs e)
    {
        _page = Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;
        if (_page is not null)
        {
            _page.SizeChanged += OnPageSizeChanged;
            CapHeight();
        }
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_page is not null)
        {
            _page.SizeChanged -= OnPageSizeChanged;
            _page = null;
        }
    }

    private void OnPageSizeChanged(object? sender, EventArgs e) => CapHeight();

    private void CapHeight()
    {
        if (_page is { Height: > ContentMargin } page)
        {
            Scroll.MaximumHeightRequest = page.Height - ContentMargin;
        }
    }
}
