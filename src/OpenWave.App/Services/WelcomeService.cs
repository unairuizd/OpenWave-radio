namespace OpenWave.App.Services;

/// <summary>
/// Whether the welcome pop-up has been shown and closed on this device. Kept apart from the appearance and the
/// station preferences on purpose: resetting either of them never brings the pop-up back.
/// </summary>
public sealed class WelcomeService
{
    private const string SeenKey = "welcome_seen";

    public bool IsSeen => Preferences.Default.Get(SeenKey, false);

    /// <summary>Called when the pop-up closes, whatever closed it; from then on it is never shown again.</summary>
    public void MarkSeen()
    {
        Preferences.Default.Set(SeenKey, true);
    }
}
