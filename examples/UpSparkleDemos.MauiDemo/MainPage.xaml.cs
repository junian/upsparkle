using System.Reflection;
using UpSparkle;

namespace UpSparkleDemos.MauiDemo;

public partial class MainPage : ContentPage
{
	private UpSparkleUpdater _updater;

	public MainPage()
	{
		InitializeComponent();

		_updater = new UpSparkleUpdater();
		this.Loaded += (sender, args) =>
		{
			/*
			_updater.Initialize(
                Assembly.GetExecutingAssembly(),
                "https://sparkle-project.org/files/sparkletestcast.xml",
				"MCwCFC9S9Yv8lzxX6BTMvR1/6K6O4sSVAhRNLAnl9jH+P86p5595B0vC+59L");
			*/

			_updater.Initialize(Assembly.GetExecutingAssembly());

			AutomaticChecksSwitch.IsToggled = _updater.IsAutomaticCheckForUpdates;
			IntervalEntry.Text = _updater.UpdateCheckInterval.ToString();
			LastCheckLabel.Text = _updater.LastCheckTime?.ToString("u") ?? "-";
		};

		this.Disappearing += (sender, args) =>
		{
			_updater.Dispose();
		};
	}

	private void OnAutomaticChecksToggled(object? sender, ToggledEventArgs e)
	{
		_updater.IsAutomaticCheckForUpdates = e.Value;
	}

	private void OnIntervalChanged(object? sender, EventArgs e)
	{
		if (int.TryParse(IntervalEntry.Text, out var seconds) && seconds > 0)
		{
			_updater.UpdateCheckInterval = seconds;
		}
	}

	private void OnCheckForUpdatesClicked(object? sender, EventArgs e)
	{
		_updater.CheckUpdateWithUI();
		LastCheckLabel.Text = _updater.LastCheckTime?.ToString("u") ?? "-";

		StatusLabel.Text = "Requested an update check.";
	}
}
