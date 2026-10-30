using OpenWave.App.Services;

namespace OpenWave.App;

public partial class App : Application
{
	private readonly PlaybackService _playback;
	private readonly AppearanceService _appearance;

	public App(PlaybackService playback, AppearanceService appearance)
	{
		InitializeComponent();
		_playback = playback;
		_appearance = appearance;

		// Before CreateWindow builds the first page, so the stored theme, font and size are used from the start.
		appearance.Initialize();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell(_appearance));
	}

	// No background playback: stop the audio when the app goes to the background.
	protected override void OnSleep()
	{
		_ = _playback.StopAsync();
		base.OnSleep();
	}
}
