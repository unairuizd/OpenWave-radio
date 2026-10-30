using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;

namespace OpenWave.App.ViewModels;

public partial class InicioViewModel : ObservableObject
{
    private const string UnreachableMessage = "No hay conexión con el servidor.";
    private const string CatalogueUnavailableMessage = "No se pudo obtener el catálogo de emisoras.";

    private readonly IStationService _stationService;
    private readonly PlaybackService _playback;
    private readonly FavoritesService _favorites;
    private readonly PreferencesService _preferences;
    private CancellationTokenSource? _loadCts;
    private string? _lastQuery;
    private bool _hasLoaded;
    private bool _preferencesChanged;

    public InicioViewModel(
        IStationService stationService,
        PlaybackService playback,
        FavoritesService favorites,
        PreferencesService preferences)
    {
        _stationService = stationService;
        _playback = playback;
        _favorites = favorites;
        _preferences = preferences;
        _playback.PropertyChanged += OnPlaybackPropertyChanged;
        _favorites.FavoritesChanged += (_, _) => RefreshFavorites();
        _preferences.Changed += OnPreferencesChanged;
    }

    /// <summary>
    /// The bar is shown whenever at least one preference is saved, applied or not.
    /// </summary>
    public bool ShowFilterBar => _preferences.Current.HasAny;

    public bool FiltersApplied => _preferences.ApplyInInicio;

    public bool FiltersNotApplied => !_preferences.ApplyInInicio;

    /// <summary>
    /// For example "Spain · Spanish · Jazz · Música".
    /// </summary>
    public string FilterSummary => PreferenceLabels.Summary(_preferences.Current);

    /// <summary>
    /// No stations, and the request was not filtered by preferences.
    /// </summary>
    [ObservableProperty]
    public partial bool IsEmptyPlain { get; set; }

    /// <summary>
    /// No stations, and the request was filtered by the saved preferences.
    /// </summary>
    [ObservableProperty]
    public partial bool IsEmptyWithPreferences { get; set; }

    public ObservableCollection<StationListItem> Stations { get; } = [];

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task SearchAsync()
    {
        return LoadAsync(SearchText);
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        return LoadAsync(null);
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task RetryAsync()
    {
        return LoadAsync(_lastQuery);
    }

    /// <summary>
    /// Stops applying the saved preferences until the app is closed; the saved values are kept.
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task DeactivateFiltersAsync()
    {
        _preferences.ApplyInInicio = false;
        return LoadAsync(SearchText);
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task ActivateFiltersAsync()
    {
        _preferences.ApplyInInicio = true;
        return LoadAsync(SearchText);
    }

    [RelayCommand]
    private async Task SelectStationAsync(StationListItem? item)
    {
        if (item is null)
        {
            return;
        }

        await _playback.SelectStationAsync(item.Station);
        await Shell.Current.GoToAsync("//reproductor");
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task ToggleFavoriteAsync(StationListItem? item)
    {
        return item is null ? Task.CompletedTask : _favorites.ToggleFromHeartAsync(item.Station);
    }

    /// <summary>
    /// Loads the first time Inicio appears, and again after the saved preferences changed.
    /// </summary>
    public void LoadIfNeeded()
    {
        if ((_hasLoaded && !_preferencesChanged) || IsLoading)
        {
            return;
        }

        _ = LoadAsync(_lastQuery);
    }

    /// <summary>
    /// Loads stations for <paramref name="name"/>, cancelling any older request so only the latest updates the page.
    /// </summary>
    private async Task LoadAsync(string? name)
    {
        _loadCts?.Cancel();
        var cts = new CancellationTokenSource();
        _loadCts = cts;

        _lastQuery = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        _preferencesChanged = false;
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;
        IsEmptyPlain = false;
        IsEmptyWithPreferences = false;

        var filters = _preferences.ApplyInInicio && _preferences.Current.HasAny ? _preferences.Current : null;

        try
        {
            var stations = await _stationService.SearchAsync(_lastQuery, filters, cts.Token);
            if (cts != _loadCts)
            {
                return;
            }

            var currentId = _playback.CurrentStation?.Id;
            Stations.Clear();
            foreach (var station in stations)
            {
                Stations.Add(new StationListItem(station)
                {
                    IsSelected = station.Id == currentId,
                    IsFavorite = _favorites.IsFavorite(station.Id),
                });
            }

            IsEmptyPlain = Stations.Count == 0 && filters is null;
            IsEmptyWithPreferences = Stations.Count == 0 && filters is not null;
            _hasLoaded = true;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // A newer request replaced this one.
        }
        catch (Exception ex)
        {
            if (cts != _loadCts)
            {
                return;
            }

            Stations.Clear();
            HasError = true;
            ErrorMessage = ex is StationRequestException { Error: StationRequestError.Unreachable }
                ? UnreachableMessage
                : CatalogueUnavailableMessage;
        }
        finally
        {
            if (cts == _loadCts)
            {
                IsLoading = false;
                _loadCts = null;
            }

            cts.Dispose();
        }
    }

    private void OnPlaybackPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
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

    private void OnPreferencesChanged(object? sender, EventArgs e)
    {
        // Saved values changed in Preferencias (or the bar toggled): reload the next time Inicio is shown.
        _preferencesChanged = true;
        OnPropertyChanged(nameof(ShowFilterBar));
        OnPropertyChanged(nameof(FiltersApplied));
        OnPropertyChanged(nameof(FiltersNotApplied));
        OnPropertyChanged(nameof(FilterSummary));
    }

    private void RefreshFavorites()
    {
        foreach (var item in Stations)
        {
            item.IsFavorite = _favorites.IsFavorite(item.Station.Id);
        }
    }
}
