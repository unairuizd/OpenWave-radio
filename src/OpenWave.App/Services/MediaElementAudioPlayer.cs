using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Platform;

namespace OpenWave.App.Services;

/// <summary>
/// <see cref="IAudioPlayer"/> backed by the app's single <see cref="MediaElement"/>. The element belongs to no page
/// and no theme layout: it is created once with the window, so audio keeps playing while the user changes page or
/// theme. A stream requested before the window exists starts when the element is created.
/// </summary>
public sealed class MediaElementAudioPlayer : IAudioPlayer
{
    private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(15);

    private MediaElement? _element;
    private string? _pendingUrl;
    private CancellationTokenSource? _watchdogCts;

    // True from Play until Stop or a failure; element events outside that window are ignored.
    private bool _isActive;
    private bool _hasReachedPlaying;

    public event EventHandler<PlaybackState>? StateChanged;

    public event EventHandler<string>? Failed;

    /// <summary>
    /// Creates the audio element and its platform player and places it in <paramref name="window"/>, outside every page.
    /// Call once, when the app window has been created.
    /// </summary>
    public void CreateElement(Window window)
    {
        if (_element is not null || window.Handler?.MauiContext is not { } context)
        {
            return;
        }

        // Audio only: a 1x1 element that needs no place in any page.
        var element = new MediaElement { ShouldShowPlaybackControls = false, WidthRequest = 1, HeightRequest = 1 };
        _ = element.ToPlatform(context);
        AudioHost.Attach(element, window);

        _element = element;
        _element.ShouldAutoPlay = true;
        _element.StateChanged += OnElementStateChanged;
        _element.MediaFailed += OnElementMediaFailed;

        if (_pendingUrl is not null)
        {
            Start(_pendingUrl);
        }
    }

    public void Play(string streamUrl)
    {
        _isActive = true;
        _hasReachedPlaying = false;

        if (_element is null)
        {
            _pendingUrl = streamUrl;
            return;
        }

        Start(streamUrl);
    }

    public void Stop()
    {
        _pendingUrl = null;
        _isActive = false;
        CancelWatchdog();
        ReleaseStream();
        RaiseStateChanged(PlaybackState.Stopped);
    }

    private void Start(string streamUrl)
    {
        _pendingUrl = null;

        if (!Uri.TryCreate(streamUrl, UriKind.Absolute, out var uri))
        {
            Fail($"Invalid stream URL: {streamUrl}");
            return;
        }

        StartWatchdog();

        // A plain URI: with HTTP headers the Windows player reads the source through HEAD and Range requests,
        // which never start a live stream that has no content length.
        _element!.Source = MediaSource.FromUri(uri);
    }

    private void OnElementStateChanged(object? sender, MediaStateChangedEventArgs e)
    {
        RunOnMainThread(() => HandleElementState(e.NewState));
    }

    private void HandleElementState(MediaElementState newState)
    {
        if (!_isActive)
        {
            // Windows can finish opening a released source later (for example after the watchdog gave up):
            // stop it so no audio plays while the app shows Detenido or Error.
            if (newState is MediaElementState.Opening or MediaElementState.Buffering or MediaElementState.Playing)
            {
                ReleaseStream();
            }

            return;
        }

        switch (newState)
        {
            case MediaElementState.Opening:
            case MediaElementState.Buffering:
                RaiseStateChanged(PlaybackState.Buffering);
                break;

            case MediaElementState.Playing:
                _hasReachedPlaying = true;
                CancelWatchdog();
                RaiseStateChanged(PlaybackState.Playing);
                break;

            case MediaElementState.Failed:
                Fail("The media element reported the Failed state.");
                break;

            case MediaElementState.Paused:
                // Windows reports Paused while a source opens, before auto-play starts it.
                // After playback started it means the platform paused it (for example audio focus loss on Android): treat as a stop.
                if (_hasReachedPlaying)
                {
                    Stop();
                }

                break;

            case MediaElementState.Stopped:
            case MediaElementState.None:
                // Transitional while a new source opens; after playback started it means the stream ended.
                if (_hasReachedPlaying)
                {
                    Fail("The stream stopped unexpectedly.");
                }

                break;
        }
    }

    private void OnElementMediaFailed(object? sender, MediaFailedEventArgs e)
    {
        RunOnMainThread(() =>
        {
            if (_isActive)
            {
                Fail(e.ErrorMessage);
            }
        });
    }

    private void Fail(string reason)
    {
        _pendingUrl = null;
        _isActive = false;
        CancelWatchdog();
        ReleaseStream();
        RunOnMainThread(() => Failed?.Invoke(this, reason));
    }

    private void ReleaseStream()
    {
        if (_element is null)
        {
            return;
        }

        if (_element.CurrentState is MediaElementState.Opening or MediaElementState.Buffering
            or MediaElementState.Playing or MediaElementState.Paused)
        {
            _element.Stop();
        }

        _element.Source = null;
    }

    private void StartWatchdog()
    {
        CancelWatchdog();
        var cts = new CancellationTokenSource();
        _watchdogCts = cts;

        _ = Task.Delay(StartTimeout, cts.Token).ContinueWith(
            _ => RunOnMainThread(() =>
            {
                if (_watchdogCts == cts && _isActive && !_hasReachedPlaying)
                {
                    Fail($"The stream did not start within {StartTimeout.TotalSeconds} seconds.");
                }
            }),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnRanToCompletion,
            TaskScheduler.Default);
    }

    private void CancelWatchdog()
    {
        _watchdogCts?.Cancel();
        _watchdogCts?.Dispose();
        _watchdogCts = null;
    }

    private void RaiseStateChanged(PlaybackState state)
    {
        RunOnMainThread(() => StateChanged?.Invoke(this, state));
    }

    private static void RunOnMainThread(Action action)
    {
        if (MainThread.IsMainThread)
        {
            action();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }
}
