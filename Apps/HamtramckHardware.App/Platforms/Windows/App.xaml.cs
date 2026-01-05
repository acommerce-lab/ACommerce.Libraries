using Microsoft.UI.Xaml;

namespace HamtramckHardware.App.WinUI;

public partial class App : MauiWinUIApplication
{
	public App()
	{
		this.InitializeComponent();
	}

	protected override MauiApp CreateMauiApp() => HamtramckHardware.App.MauiProgram.CreateMauiApp();
}
