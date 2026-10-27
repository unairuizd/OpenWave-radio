using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.Contracts;

namespace OpenWave.App.ViewModels;

public partial class InicioViewModel : ObservableObject
{
    private const string LoadErrorMessage =
        "No se pudo conectar con el servidor. Comprueba que la API está en marcha.";

    private readonly IStationService _stationService;
    private readonly PlaybackService _playback;
    private bool _hasLoaded;

    public InicioViewModel(IStationService stationService, PlaybackService playback)
    {
        _stationService = stationService;
        _playback = playback;
    }

    public ObservableCollection<StationDto> Stations { get; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand]
    private async Task LoadStationsAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var stations = await _stationService.GetStationsAsync();

            Stations.Clear();
            foreach (var station in stations)
            {
                Stations.Add(station);
            }

            _hasLoaded = true;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = LoadErrorMessage;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SelectStationAsync(StationDto? station)
    {
        if (station is null)
        {
            return;
        }

        _playback.SelectStation(station);
        await Shell.Current.GoToAsync("//reproductor");
    }

    public void LoadIfNeeded()
    {
        if (_hasLoaded)
        {
            return;
        }

        LoadStationsCommand.Execute(null);
    }
}
