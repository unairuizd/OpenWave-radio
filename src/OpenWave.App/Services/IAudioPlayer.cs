namespace OpenWave.App.Services;

/// <summary>
/// Plays a single audio stream. Events are raised on the main thread.
/// </summary>
public interface IAudioPlayer
{
    /// <summary>
    /// Raised when the player moves to <see cref="PlaybackState.Stopped"/>, <see cref="PlaybackState.Buffering"/>
    /// or <see cref="PlaybackState.Playing"/>.
    /// </summary>
    event EventHandler<PlaybackState>? StateChanged;

    /// <summary>
    /// Raised when the stream cannot be opened or stops playing; the argument is the technical cause.
    /// </summary>
    event EventHandler<string>? Failed;

    /// <summary>
    /// Starts <paramref name="streamUrl"/>, replacing anything that is playing.
    /// </summary>
    void Play(string streamUrl);

    /// <summary>
    /// Stops playback and forgets any pending stream.
    /// </summary>
    void Stop();
}
