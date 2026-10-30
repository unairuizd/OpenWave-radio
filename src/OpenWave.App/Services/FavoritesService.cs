using System.ComponentModel;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Single source of truth for the logged-in user's favourites, so every heart and list agrees.
/// </summary>
public sealed class FavoritesService
{
    public const string SaveFailedMessage = "No se pudo actualizar tus favoritas.";

    // U+FE0E asks for the text glyph: without it Android draws ♥ as a red colour emoji,
    // ignoring the accent colour of the heart button.
    public const string FilledHeart = "♥︎";
    public const string EmptyHeart = "♡︎";

    private readonly FavoritesApiClient _api;
    private readonly SessionService _session;
    private readonly HashSet<string> _ids = new(StringComparer.Ordinal);
    private List<StationDto> _favorites = [];

    public FavoritesService(FavoritesApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
        _session.PropertyChanged += OnSessionPropertyChanged;
    }

    /// <summary>
    /// Raised on the main thread whenever the favourite ids or the list change.
    /// </summary>
    public event EventHandler? FavoritesChanged;

    /// <summary>
    /// The station whose heart opened the login flow; added after a successful login. Null when the flow
    /// was opened another way, so a cancelled flow can never add a stale station later.
    /// </summary>
    public StationDto? PendingStation { get; set; }

    /// <summary>
    /// The last loaded list, newest first, including changes made since.
    /// </summary>
    public IReadOnlyList<StationDto> Favorites => _favorites;

    public bool IsFavorite(string stationId) => _ids.Contains(stationId);

    /// <summary>
    /// Reloads the list from the API.
    /// </summary>
    /// <exception cref="ApiRequestException">The list could not be loaded.</exception>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var user = _session.CurrentUser;
        if (user is null)
        {
            return;
        }

        var favorites = await _api.ListAsync(user.Id, cancellationToken);
        if (_session.CurrentUser?.Id != user.Id)
        {
            // The user logged out (or changed) while the request ran.
            return;
        }

        _favorites = favorites.ToList();
        _ids.Clear();
        _ids.UnionWith(_favorites.Select(f => f.Id));
        RaiseChanged();
    }

    /// <summary>
    /// Adds or removes the station. The heart changes at once and reverts if saving fails.
    /// </summary>
    /// <returns><c>false</c> when the change could not be saved (or nobody is logged in).</returns>
    public Task<bool> ToggleAsync(StationDto station)
    {
        return IsFavorite(station.Id) ? RemoveAsync(station) : AddAsync(station);
    }

    /// <summary>
    /// What a heart does: toggle when logged in (with an alert if saving fails); otherwise remember the
    /// station and open the login page, which adds it after a successful login.
    /// </summary>
    public async Task ToggleFromHeartAsync(StationDto station)
    {
        if (!_session.IsLoggedIn)
        {
            PendingStation = station;
            await Shell.Current.GoToAsync("login");
            return;
        }

        if (!await ToggleAsync(station))
        {
            await Shell.Current.DisplayAlertAsync("Favoritas", SaveFailedMessage, "Aceptar");
        }
    }

    public async Task<bool> AddAsync(StationDto station)
    {
        var user = _session.CurrentUser;
        if (user is null || !_ids.Add(station.Id))
        {
            return user is not null;
        }

        RaiseChanged();
        StationDto stored;
        try
        {
            stored = await _api.AddAsync(user.Id, station);
        }
        catch (ApiRequestException ex) when (ex.Kind == ApiErrorKind.Conflict)
        {
            // Already stored: the wanted state.
            stored = station;
        }
        catch (Exception)
        {
            _ids.Remove(station.Id);
            RaiseChanged();
            return false;
        }

        if (_session.CurrentUser?.Id == user.Id)
        {
            _favorites.RemoveAll(f => f.Id == station.Id);
            _favorites.Insert(0, stored);
            RaiseChanged();
        }

        return true;
    }

    public async Task<bool> RemoveAsync(StationDto station)
    {
        var user = _session.CurrentUser;
        if (user is null || !_ids.Remove(station.Id))
        {
            return user is not null;
        }

        RaiseChanged();
        try
        {
            await _api.RemoveAsync(user.Id, station.Id);
        }
        catch (ApiRequestException ex) when (ex.Kind == ApiErrorKind.NotFound)
        {
            // Already gone: the wanted state.
        }
        catch (Exception)
        {
            _ids.Add(station.Id);
            RaiseChanged();
            return false;
        }

        if (_session.CurrentUser?.Id == user.Id)
        {
            _favorites.RemoveAll(f => f.Id == station.Id);
            RaiseChanged();
        }

        return true;
    }

    private async void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SessionService.CurrentUser))
        {
            return;
        }

        if (!_session.IsLoggedIn)
        {
            _ids.Clear();
            _favorites = [];
            PendingStation = null;
            RaiseChanged();
            return;
        }

        var pending = PendingStation;
        PendingStation = null;

        try
        {
            await LoadAsync();
        }
        catch (ApiRequestException)
        {
            // The hearts stay empty; Favoritos shows the error when opened.
        }

        if (pending is not null && !await AddAsync(pending))
        {
            await Shell.Current.DisplayAlertAsync("Favoritas", SaveFailedMessage, "Aceptar");
        }
    }

    private void RaiseChanged()
    {
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }
}
