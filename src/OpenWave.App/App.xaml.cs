using OpenWave.App.Services;

namespace OpenWave.App;

public partial class App : Application
{
	private readonly PlaybackService _playback;
	private readonly AppearanceService _appearance;
	private readonly MediaElementAudioPlayer _audioPlayer;

	public App(PlaybackService playback, AppearanceService appearance, MediaElementAudioPlayer audioPlayer)
	{
		InitializeComponent();
		_playback = playback;
		_appearance = appearance;
		_audioPlayer = audioPlayer;

		// Before CreateWindow builds the first page, so the stored theme, font and size are used from the start.
		appearance.Initialize();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var shell = new AppShell(_appearance);
		var window = new Window(shell)
		{
			// Windows only (Android keeps its screen): the window is never smaller than a small phone.
			MinimumWidth = 360,
			MinimumHeight = 600,
		};
		window.SizeChanged += (_, _) => _appearance.SetWindowWidth(window.Width);
		// The single audio element lives with the window, not with a page, so audio survives page and theme changes.
		// It is created once the window shows its content.
		shell.Loaded += (_, _) => _audioPlayer.CreateElement(window);
#if WINDOWS
		shell.Loaded += (_, _) => WinUI.TitleBarText.Attach(window);
#endif
		return window;
	}

	// No background playback: stop the audio when the app goes to the background.
	protected override void OnSleep()
	{
		_ = _playback.StopAsync();
		base.OnSleep();
	}
}
