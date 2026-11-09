using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Services;

/// <summary>
/// The play state, for theme parts that every page or both lists share (for example the `EN DIRECTO` tag of the Mesa
/// de DJ header, or the play state on an MP3 line) where the view model does not know it. Only
/// <see cref="PlaybackService"/> writes it; layouts bind to <see cref="Current"/> with <c>x:Static</c>.
/// </summary>
public partial class PlaybackIndicator : ObservableObject
{
    public static PlaybackIndicator Current { get; } = new();

    private PlaybackIndicator()
    {
    }

    /// <summary>
    /// True while a station is buffering or playing.
    /// </summary>
    [ObservableProperty]
    public partial bool IsActive { get; internal set; }

    /// <summary>
    /// The same text as <see cref="PlaybackService.StatusText"/> ("Reproduciendo", "Detenido"… in the app language).
    /// </summary>
    [ObservableProperty]
    public partial string StatusText { get; internal set; } = Localizer.Instance["Playback_Stopped"];
}
