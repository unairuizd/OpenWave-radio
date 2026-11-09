using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.ViewModels;

public partial class LoginViewModel(IAuthService authService, SessionService session) : ObservableObject
{
    private static string InvalidEmailMessage => Localizer.Instance["Account_InvalidEmail"];
    private static string EmptyPasswordMessage => Localizer.Instance["Account_EmptyPassword"];

    [ObservableProperty]
    public partial string? Email { get; set; }

    [ObservableProperty]
    public partial string? Password { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;

        var email = AccountRules.NormalizeEmail(Email);
        if (!AccountRules.IsValidEmail(email))
        {
            ErrorMessage = InvalidEmailMessage;
            return;
        }

        if (string.IsNullOrEmpty(Password))
        {
            ErrorMessage = EmptyPasswordMessage;
            return;
        }

        IsBusy = true;
        try
        {
            var user = await authService.LoginAsync(email, Password);
            session.LogIn(user);
            // Back to the screen that opened the login page.
            await Shell.Current.GoToAsync("..");
        }
        catch (ApiRequestException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = ApiRequestException.UnexpectedMessage;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task GoToRegisterAsync()
    {
        return Shell.Current.GoToAsync("register");
    }
}
