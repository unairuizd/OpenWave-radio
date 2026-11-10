using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using OpenWave.App.Services;
using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class InicioPage : ContentPage
{
    private readonly InicioViewModel _viewModel;
    private readonly WelcomeService _welcome;
    private readonly IServiceProvider _services;
    private bool _welcomeOpen;

    public InicioPage(InicioViewModel viewModel, WelcomeService welcome, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _welcome = welcome;
        _services = services;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadIfNeeded();
        // Once the page is on screen: the list keeps loading behind the pop-up. OnAppearing runs again when the
        // user comes back from the login pages, but by then the flag is set.
        if (!_welcome.IsSeen && !_welcomeOpen)
        {
            Dispatcher.Dispatch(() => _ = ShowWelcomeAsync());
        }
    }

    private async Task ShowWelcomeAsync()
    {
        _welcomeOpen = true;
        WelcomeChoice? choice;
        try
        {
            var popup = _services.GetRequiredService<WelcomePopup>();
            var scrim = Application.Current?.Resources.TryGetValue("ScrimColor", out var value) == true ? value as Color : null;
            var options = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = true,
                PageOverlayColor = scrim ?? Colors.Transparent,
                // The themed Border inside draws the card: no extra shape or shadow around it.
                Shape = null,
                Shadow = null,
            };
            var result = await this.ShowPopupAsync<WelcomeChoice>(popup, options);
            choice = result.WasDismissedByTappingOutsideOfPopup ? null : result.Result;
        }
        finally
        {
            // Seen as soon as it closes, whatever closed it, and before any navigation starts.
            _welcome.MarkSeen();
            _welcomeOpen = false;
        }

        switch (choice)
        {
            case WelcomeChoice.Login:
                await Shell.Current.GoToAsync("login");
                break;
            case WelcomeChoice.Register:
                // Both pages, so the register page's "../.." on success returns here, as it does from Favoritos.
                await Shell.Current.GoToAsync("login/register");
                break;
        }
    }
}
