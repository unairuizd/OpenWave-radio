using OpenWave.App.Views;

namespace OpenWave.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Pushed onto the current tab, so going back returns to the screen that opened them.
		Routing.RegisterRoute("login", typeof(LoginPage));
		Routing.RegisterRoute("register", typeof(RegisterPage));
	}
}
