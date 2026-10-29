using OpenWave.App.Services;

namespace OpenWave.App;

public partial class App : Application
{
	private readonly PlaybackService _playback;

	public App(PlaybackService playback)
	{
		InitializeComponent();
		_playback = playback;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}

	// No background playback: stop the audio when the app goes to the background.
	protected override void OnSleep()
	{
		_ = _playback.StopAsync();
		base.OnSleep();
	}
}
