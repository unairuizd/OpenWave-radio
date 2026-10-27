using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Holds the simulated playback state. No audio is played in this phase.
/// </summary>
public partial class PlaybackService : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    public partial StationDto? CurrentStation { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    public partial bool IsPlaying { get; set; }

    public string StatusText => IsPlaying ? "Reproduciendo" : "Detenido";

    public void SelectStation(StationDto station)
    {
        if (CurrentStation?.Id == station.Id)
        {
            return;
        }

        if (IsPlaying)
        {
            Stop();
        }

        CurrentStation = station;
    }

    public void Play()
    {
        if (CurrentStation is null)
        {
            return;
        }

        IsPlaying = true;
    }

    public void Stop()
    {
        IsPlaying = false;
    }

    public void Toggle()
    {
        if (IsPlaying)
        {
            Stop();
        }
        else
        {
            Play();
        }
    }
}
