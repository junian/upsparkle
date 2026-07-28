using System.Reflection;
using UpSparkle;

namespace UpSparkleDemos.MauiDemo;

public partial class MainPage : ContentPage
{
	private UpSparkleUpdater _updater; 
		
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
		
		_updater = new UpSparkleUpdater();
		this.Loaded += (sender, args) =>
		{
			_updater.Init(
				"https://sparkle-project.org/files/sparkletestcast.xml",
				"MCwCFC9S9Yv8lzxX6BTMvR1/6K6O4sSVAhRNLAnl9jH+P86p5595B0vC+59L",
				Assembly.GetExecutingAssembly());
		};

		this.Disappearing += (sender, args) =>
		{
			_updater.Dispose();
		};
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		_updater.CheckUpdateWithUI();
		
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}
