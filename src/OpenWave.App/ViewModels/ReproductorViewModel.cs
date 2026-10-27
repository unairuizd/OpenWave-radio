using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.Contracts;

namespace OpenWave.App.ViewModels;

public partial class ReproductorViewModel : ObservableObject
{
    private readonly PlaybackService _playback;

    public ReproductorViewModel(PlaybackService playback)
    {
        _playback = playback;
        _playback.PropertyChanged += OnPlaybackPropertyChanged;
    }

    public StationDto? CurrentStation => _playback.CurrentStation;

    public bool IsPlaying => _playback.IsPlaying;

    public string StatusText => _playback.StatusText;

    public bool HasStation => _playback.CurrentStation is not null;

    public bool HasNoStation => _playback.CurrentStation is null;

    public string ToggleButtonText => _playback.IsPlaying ? "Detener" : "Reproducir";

    [RelayCommand]
    private void Toggle()
    {
        _playback.Toggle();
    }

    private void OnPlaybackPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(PlaybackService.CurrentStation)
            or nameof(PlaybackService.IsPlaying)
            or nameof(PlaybackService.StatusText)))
        {
            return;
        }

        OnPropertyChanged(nameof(CurrentStation));
        OnPropertyChanged(nameof(IsPlaying));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(HasStation));
        OnPropertyChanged(nameof(HasNoStation));
        OnPropertyChanged(nameof(ToggleButtonText));
    }
}
