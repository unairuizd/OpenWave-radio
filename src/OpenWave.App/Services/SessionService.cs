using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// The logged-in user, kept only in memory: every app start begins logged out.
/// </summary>
public partial class SessionService : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
    public partial UserDto? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser is not null;

    public void LogIn(UserDto user)
    {
        CurrentUser = user;
    }

    public void LogOut()
    {
        CurrentUser = null;
    }
}
