using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.ViewModels;

public partial class RegisterViewModel(IAuthService authService, SessionService session) : ObservableObject
{
    private static string InvalidEmailMessage => Localizer.Instance["Account_InvalidEmail"];
    private static string ShortPasswordMessage => Localizer.Instance["Account_ShortPassword"];
    private static string PasswordMismatchMessage => Localizer.Instance["Account_PasswordMismatch"];

    [ObservableProperty]
    public partial string? Email { get; set; }

    [ObservableProperty]
    public partial string? Password { get; set; }

    [ObservableProperty]
    public partial string? ConfirmPassword { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;

        var email = AccountRules.NormalizeEmail(Email);
        if (!AccountRules.IsValidEmail(email))
        {
            ErrorMessage = InvalidEmailMessage;
            return;
        }

        var password = Password ?? string.Empty;
        if (password.Length < AccountRules.MinPasswordLength)
        {
            ErrorMessage = ShortPasswordMessage;
            return;
        }

        if (password != ConfirmPassword)
        {
            ErrorMessage = PasswordMismatchMessage;
            return;
        }

        IsBusy = true;
        try
        {
            var user = await authService.RegisterAsync(email, password);
            session.LogIn(user);
            // Pop this page and the login page: back to the screen that opened the login flow.
            await Shell.Current.GoToAsync("../..");
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
}
