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
    private CancellationTokenSource? _loadCts;
    private string? _lastQuery;
    private bool _hasLoaded;

    public InicioViewModel(IStationService stationService, PlaybackService playback)
    {
        _stationService = stationService;
        _playback = playback;
        _playback.PropertyChanged += OnPlaybackPropertyChanged;
    }

    public ObservableCollection<StationListItem> Stations { get; } = [];

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

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

    public void LoadIfNeeded()
    {
        if (_hasLoaded || IsLoading)
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
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;
        IsEmpty = false;

        try
        {
            var stations = await _stationService.SearchAsync(_lastQuery, cts.Token);
            if (cts != _loadCts)
            {
                return;
            }

            var currentId = _playback.CurrentStation?.Id;
            Stations.Clear();
            foreach (var station in stations)
            {
                Stations.Add(new StationListItem(station) { IsSelected = station.Id == currentId });
            }

            IsEmpty = Stations.Count == 0;
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
}
