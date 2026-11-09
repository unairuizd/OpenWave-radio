using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using OpenWave.Contracts;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Services;

/// <summary>
/// Single source of truth for the selected station and its playback state.
/// </summary>
public partial class PlaybackService : ObservableObject
{
    private static string PlaybackErrorMessage => Localizer.Instance["Playback_Error"];

    private readonly IAudioPlayer _player;
    private readonly ILogger<PlaybackService> _logger;

    public PlaybackService(IAudioPlayer player, ILogger<PlaybackService> logger)
    {
        _player = player;
        _logger = logger;
        _player.StateChanged += OnPlayerStateChanged;
        _player.Failed += OnPlayerFailed;

        // The status text is in the app language: a language change rewrites it.
        Localizer.Instance.LanguageChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(StatusText));
            PlaybackIndicator.Current.StatusText = StatusText;
        };
    }

    [ObservableProperty]
    public partial StationDto? CurrentStation { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(IsActive))]
    public partial PlaybackState State { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>
    /// True while a stream is buffering or playing.
    /// </summary>
    public bool IsActive => State is PlaybackState.Buffering or PlaybackState.Playing;

    // Every state change also reaches the page chrome that has no view model with the state.
    partial void OnStateChanged(PlaybackState value)
    {
        PlaybackIndicator.Current.IsActive = IsActive;
        PlaybackIndicator.Current.StatusText = StatusText;
    }

    public string StatusText => State switch
    {
        PlaybackState.Buffering => Localizer.Instance["Playback_Buffering"],
        PlaybackState.Playing => Localizer.Instance["Playback_Playing"],
        PlaybackState.Error => Localizer.Instance["Playback_Failed"],
        _ => Localizer.Instance["Playback_Stopped"]
    };

    /// <summary>
    /// Selects <paramref name="station"/> and starts it, stopping any other station first.
    /// </summary>
    public Task SelectStationAsync(StationDto station)
    {
        if (CurrentStation?.Id == station.Id)
        {
            return IsActive ? Task.CompletedTask : PlayAsync();
        }

        _player.Stop();
        CurrentStation = station;
        return PlayAsync();
    }

    public Task PlayAsync()
    {
        if (CurrentStation is null || IsActive)
        {
            return Task.CompletedTask;
        }

        ErrorMessage = null;
        State = PlaybackState.Buffering;
        _player.Play(CurrentStation.StreamUrl);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _player.Stop();
        ErrorMessage = null;
        State = PlaybackState.Stopped;
        return Task.CompletedTask;
    }

    public Task RetryAsync()
    {
        return PlayAsync();
    }

    public Task ToggleAsync()
    {
        return IsActive ? StopAsync() : PlayAsync();
    }

    private void OnPlayerStateChanged(object? sender, PlaybackState state)
    {
        State = state;
        if (state != PlaybackState.Stopped)
        {
            ErrorMessage = null;
        }
    }

    private void OnPlayerFailed(object? sender, string reason)
    {
        _logger.LogWarning("Playback of {Station} failed: {Reason}", CurrentStation?.StreamUrl, reason);
        State = PlaybackState.Error;
        ErrorMessage = PlaybackErrorMessage;
    }
}
