using System.ComponentModel;
using OpenWave.App.Services;
using OpenWave.App.Views;

namespace OpenWave.App;

public partial class AppShell : Shell
{
	public AppShell(AppearanceService appearance)
	{
		InitializeComponent();

		// Pushed onto the current tab, so going back returns to the screen that opened them.
		Routing.RegisterRoute("login", typeof(LoginPage));
		Routing.RegisterRoute("register", typeof(RegisterPage));
		Routing.RegisterRoute("personalizacion", typeof(PersonalizacionPage));
		Routing.RegisterRoute("preferencias", typeof(PreferenciasPage));

		// The header and tab labels are drawn by the platform: re-apply the font and size when they change.
		appearance.PropertyChanged += OnAppearanceChanged;
		Loaded += (_, _) => RefreshChrome();
	}

	protected override void OnNavigated(ShellNavigatedEventArgs args)
	{
		base.OnNavigated(args);
		RefreshChrome();
	}

	private void OnAppearanceChanged(object? sender, PropertyChangedEventArgs e)
	{
		RefreshChrome();
	}

	private void RefreshChrome()
	{
		if (CurrentPage is { } page)
		{
			ShellChrome.EnsureTitleView(page);
		}

		// Let the platform finish (re)building the tab views first.
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
		{
			ShellChrome.ApplyTabFonts(this);
			ShellChrome.ApplyRadioColors();
		});
	}
}
