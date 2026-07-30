using Microsoft.Extensions.DependencyInjection;

namespace UpSparkleDemos.MauiDemo;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}