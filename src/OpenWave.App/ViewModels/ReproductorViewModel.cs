using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.Contracts;

namespace OpenWave.App.ViewModels;

public partial class ReproductorViewModel : ObservableObject
{
    private const int MaxTags = 3;
    private const string NoFavoritesMessage = "Aún no tienes emisoras favoritas.";

    private readonly PlaybackService _playback;
    private readonly FavoritesService _favorites;
    private readonly SessionService _session;

    public ReproductorViewModel(PlaybackService playback, FavoritesService favorites, SessionService session)
    {
        _playback = playback;
        _favorites = favorites;
        _session = session;
        _playback.PropertyChanged += OnPlaybackPropertyChanged;
        _favorites.FavoritesChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsFavorite));
            RefreshFavorites();
        };
        _session.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SessionService.CurrentUser))
            {
                OnPropertyChanged(nameof(IsLoggedIn));
                RefreshFavorites();
            }
        };
        RefreshFavorites();
    }

    /// <summary>
    /// The user's favourites in the Favoritos order (newest first), for themes that let the user change favourite
    /// from Reproductor (for example the record crate). Empty when logged out.
    /// </summary>
    public ObservableCollection<StationListItem> Favorites { get; } = [];

    public bool IsLoggedIn => _session.IsLoggedIn;

    public bool HasNoFavorites => _session.IsLoggedIn && Favorites.Count == 0;

    public StationDto? CurrentStation => _playback.CurrentStation;

    public string? StationName => _playback.CurrentStation?.Name;

    /// <summary>
    /// For example "Spain · Spanish · pop · rock · news": country, language and up to three tags; missing values are skipped.
    /// </summary>
    public string? Details => _playback.CurrentStation is { } station
        ? string.Join(" · ",
            new[] { station.Country, station.Language }
                .Concat((station.Tags ?? []).Take(MaxTags))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.Trim()))
        : null;

    public bool HasDetails => !string.IsNullOrEmpty(Details);

    public string? LogoUrl => _playback.CurrentStation?.LogoUrl;

    public bool IsFavorite => _playback.CurrentStation is { } station && _favorites.IsFavorite(station.Id);

    public string StatusText => _playback.StatusText;

    public string? ErrorMessage => _playback.ErrorMessage;

    public bool HasStation => _playback.CurrentStation is not null;

    public bool HasNoStation => _playback.CurrentStation is null;

    public bool HasError => HasStation && _playback.State == PlaybackState.Error;

    /// <summary>
    /// Buffering or playing: the centre control shows the stop icon.
    /// </summary>
    public bool IsActive => _playback.IsActive;

    /// <summary>
    /// A short message for the status line, such as "no favourites". Cleared when the station or the state changes.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNotice))]
    public partial string? Notice { get; private set; }

    public bool HasNotice => !string.IsNullOrEmpty(Notice);

    [RelayCommand]
    private Task ToggleAsync()
    {
        return _playback.ToggleAsync();
    }

    [RelayCommand]
    private Task RetryAsync()
    {
        return _playback.RetryAsync();
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task ToggleFavoriteAsync()
    {
        return _playback.CurrentStation is { } station
            ? _favorites.ToggleFromHeartAsync(station)
            : Task.CompletedTask;
    }

    /// <summary>
    /// Selects and plays a favourite, stopping the previous station.
    /// </summary>
    [RelayCommand]
    private Task SelectFavoriteAsync(StationListItem? item)
    {
        return item is null ? Task.CompletedTask : _playback.SelectStationAsync(item.Station);
    }

    [RelayCommand]
    private Task LogInAsync()
    {
        // Opened from here, not from a heart: nothing to add after logging in.
        _favorites.PendingStation = null;
        return Shell.Current.GoToAsync("login");
    }

    [RelayCommand]
    private Task PreviousFavoriteAsync() => MoveToFavoriteAsync(-1);

    [RelayCommand]
    private Task NextFavoriteAsync() => MoveToFavoriteAsync(1);

    /// <summary>
    /// Plays the favourite before (-1) or after (+1) the current station, in the Favoritos order (newest first),
    /// wrapping at both ends. From a station that is not a favourite, next is the first and previous the last.
    /// </summary>
    private async Task MoveToFavoriteAsync(int step)
    {
        if (!_session.IsLoggedIn)
        {
            // Opened from here, not from a heart: nothing to add after logging in.
            _favorites.PendingStation = null;
            await Shell.Current.GoToAsync("login");
            return;
        }

        var list = CurrentFavorites();
        if (list.Count == 0)
        {
            // It can be empty because loading failed at login: try once more.
            try
            {
                await _favorites.LoadAsync();
            }
            catch (ApiRequestException ex)
            {
                Notice = ex.Message;
                return;
            }

            list = CurrentFavorites();
            if (list.Count == 0)
            {
                Notice = NoFavoritesMessage;
                return;
            }
        }

        var index = list.FindIndex(s => s.Id == _playback.CurrentStation?.Id);
        var target = index < 0
            ? (step > 0 ? list[0] : list[^1])
            : list[(index + step + list.Count) % list.Count];

        if (target.Id == _playback.CurrentStation?.Id)
        {
            return;
        }

        await _playback.SelectStationAsync(target);
    }

    // The same filter and order as the Favoritos page.
    private List<StationDto> CurrentFavorites() =>
        _favorites.Favorites.Where(f => _favorites.IsFavorite(f.Id)).ToList();

    private void RefreshFavorites()
    {
        Favorites.Clear();
        if (_session.IsLoggedIn)
        {
            var currentId = _playback.CurrentStation?.Id;
            foreach (var station in CurrentFavorites())
            {
                Favorites.Add(new StationListItem(station)
                {
                    PlayCommand = SelectFavoriteCommand,
                    IsFavorite = true,
                    IsSelected = station.Id == currentId,
                });
            }
        }

        OnPropertyChanged(nameof(HasNoFavorites));
    }

    private void OnPlaybackPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PlaybackService.CurrentStation) or nameof(PlaybackService.State))
        {
            Notice = null;
        }

        if (e.PropertyName == nameof(PlaybackService.CurrentStation))
        {
            var currentId = _playback.CurrentStation?.Id;
            foreach (var item in Favorites)
            {
                item.IsSelected = item.Station.Id == currentId;
            }
        }

        OnPropertyChanged(nameof(CurrentStation));
        OnPropertyChanged(nameof(StationName));
        OnPropertyChanged(nameof(Details));
        OnPropertyChanged(nameof(HasDetails));
        OnPropertyChanged(nameof(LogoUrl));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(ErrorMessage));
        OnPropertyChanged(nameof(HasStation));
        OnPropertyChanged(nameof(HasNoStation));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(IsActive));
        OnPropertyChanged(nameof(IsFavorite));
    }
}
