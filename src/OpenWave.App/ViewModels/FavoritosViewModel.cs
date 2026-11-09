using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.ViewModels;

public partial class FavoritosViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly FavoritesService _favorites;
    private readonly PlaybackService _playback;

    public FavoritosViewModel(SessionService session, FavoritesService favorites, PlaybackService playback)
    {
        _session = session;
        _favorites = favorites;
        _playback = playback;
        _session.PropertyChanged += OnSessionPropertyChanged;
        _favorites.FavoritesChanged += (_, _) => RefreshList();
        _playback.PropertyChanged += OnPlaybackPropertyChanged;
    }

    public ObservableCollection<StationListItem> Stations { get; } = [];

    public bool IsLoggedIn => _session.IsLoggedIn;

    public bool IsLoggedOut => !_session.IsLoggedIn;

    public string? Email => _session.CurrentUser?.Email;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    /// <summary>
    /// Reloads from the API each time the page appears, so it always shows stored data.
    /// </summary>
    public Task OnAppearingAsync()
    {
        return LoadAsync();
    }

    [RelayCommand]
    private Task RetryAsync()
    {
        return LoadAsync();
    }

    [RelayCommand]
    private Task LogInAsync()
    {
        // Opened from here, not from a heart: nothing to add after logging in.
        _favorites.PendingStation = null;
        return Shell.Current.GoToAsync("login");
    }

    [RelayCommand]
    private void LogOut()
    {
        _session.LogOut();
    }

    [RelayCommand]
    private async Task PlayAsync(StationListItem? item)
    {
        if (item is null)
        {
            return;
        }

        await _playback.SelectStationAsync(item.Station);
        await Shell.Current.GoToAsync("//reproductor");
    }

    /// <summary>
    /// The play state, for themes that show it next to the station (for example "Reproduciendo").
    /// </summary>
    public string PlaybackStatusText => _playback.StatusText;

    /// <summary>
    /// Plays <paramref name="item"/> without leaving Favoritos; on the current station it toggles play and stop.
    /// </summary>
    [RelayCommand]
    private Task PlayHereAsync(StationListItem? item)
    {
        if (item is null)
        {
            return Task.CompletedTask;
        }

        return _playback.CurrentStation?.Id == item.Station.Id
            ? _playback.ToggleAsync()
            : _playback.SelectStationAsync(item.Station);
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task RemoveAsync(StationListItem? item)
    {
        if (item is not null && !await _favorites.RemoveAsync(item.Station))
        {
            await Shell.Current.DisplayAlertAsync(Localizer.Instance["Favoritos_AlertTitle"], FavoritesService.SaveFailedMessage, Localizer.Instance["Action_Accept"]);
        }
    }

    private async Task LoadAsync()
    {
        if (!_session.IsLoggedIn || IsLoading)
        {
            return;
        }

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;
        IsEmpty = false;
        Stations.Clear();

        try
        {
            await _favorites.LoadAsync();
        }
        catch (ApiRequestException ex)
        {
            // Unreachable and 503 carry their fixed Spanish messages.
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }

        RefreshList();
    }

    private void RefreshList()
    {
        if (IsLoading)
        {
            return;
        }

        Stations.Clear();
        if (!_session.IsLoggedIn || HasError)
        {
            IsEmpty = false;
            return;
        }

        // Filtered by the ids, so a removal shows at once and a failed one brings the station back.
        var currentId = _playback.CurrentStation?.Id;
        foreach (var station in _favorites.Favorites.Where(f => _favorites.IsFavorite(f.Id)))
        {
            Stations.Add(new StationListItem(station)
            {
                PlayCommand = PlayCommand,
                HeartCommand = RemoveCommand,
                IsFavorite = true,
                IsSelected = station.Id == currentId,
            });
        }

        IsEmpty = Stations.Count == 0;
    }

    private void OnPlaybackPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlaybackService.StatusText))
        {
            OnPropertyChanged(nameof(PlaybackStatusText));
        }

        if (e.PropertyName != nameof(PlaybackService.CurrentStation))
        {
            return;
        }

        var currentId = _playback.CurrentStation?.Id;
        foreach (var item in Stations)
        {
            item.IsSelected = item.Station.Id == currentId;
        }
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SessionService.CurrentUser))
        {
            return;
        }

        if (!_session.IsLoggedIn)
        {
            HasError = false;
            ErrorMessage = null;
        }

        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(IsLoggedOut));
        OnPropertyChanged(nameof(Email));
        RefreshList();
    }
}
