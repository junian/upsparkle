using System.Reflection;
using AppKit;
using Foundation;
using ObjCRuntime;
using UpSparkle;

namespace UpSparkleDemos.MacOSDemo;

public partial class ViewController : NSViewController {
	private UpSparkleUpdater? _updater;

	protected ViewController (NativeHandle handle) : base (handle)
	{
		// This constructor is required if the view controller is loaded from a xib or a storyboard.
		// Do not put any initialization here, use ViewDidLoad instead.
	}

	public override void ViewDidLoad ()
	{
		base.ViewDidLoad ();

		_updater = new UpSparkleUpdater();
		_updater.Initialize(Assembly.GetExecutingAssembly());
		/*
		_updater.Initialize(
			Assembly.GetExecutingAssembly(),
			"https://sparkle-project.org/files/sparkletestcast.xml",
			"MCwCFC9S9Yv8lzxX6BTMvR1/6K6O4sSVAhRNLAnl9jH+P86p5595B0vC+59L");
			*/

		var stackView = new NSStackView
		{
			Orientation = NSUserInterfaceLayoutOrientation.Vertical,
			Alignment = NSLayoutAttribute.CenterX,
			Spacing = 12,
			TranslatesAutoresizingMaskIntoConstraints = false
		};

		View.AddSubview(stackView);

		stackView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor).Active = true;
		stackView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;

		// Update settings (backed by IsAutomaticCheckForUpdates / UpdateCheckInterval / LastCheckTime)
		var automaticCheckbox = new NSButton
		{
			Title = "Automatic checks",
			BezelStyle = NSBezelStyle.Rounded
		};
		automaticCheckbox.SetButtonType(NSButtonType.Switch);
		automaticCheckbox.State = _updater.IsAutomaticCheckForUpdates ? NSCellStateValue.On : NSCellStateValue.Off;
		automaticCheckbox.Activated += (sender, e) =>
		{
			_updater.IsAutomaticCheckForUpdates = automaticCheckbox.State == NSCellStateValue.On;
		};
		stackView.AddArrangedSubview(automaticCheckbox);

		var intervalLabel = new NSTextField
		{
			StringValue = "Check interval (s):",
			Editable = false,
			Bordered = false,
			DrawsBackground = false
		};
		stackView.AddArrangedSubview(intervalLabel);

		var intervalField = new NSTextField
		{
			StringValue = _updater.UpdateCheckInterval.ToString()
		};
		intervalField.Activated += (sender, e) =>
		{
			if (int.TryParse(intervalField.StringValue, out var seconds) && seconds > 0)
			{
				_updater.UpdateCheckInterval = seconds;
			}
		};
		stackView.AddArrangedSubview(intervalField);

		var lastCheckLabel = new NSTextField
		{
			StringValue = $"Last check: {FormatLastCheck(_updater.LastCheckTime)}",
			Editable = false,
			Bordered = false,
			DrawsBackground = false
		};
		stackView.AddArrangedSubview(lastCheckLabel);

		var checkUpdateButton = new NSButton
		{
			Title = "Check for update",
			BezelStyle = NSBezelStyle.Rounded
		};

		checkUpdateButton.Activated += (sender, e) =>
		{
			_updater.CheckUpdateWithUI();
			lastCheckLabel.StringValue = $"Last check: {FormatLastCheck(_updater.LastCheckTime)}";
		};

		stackView.AddArrangedSubview(checkUpdateButton);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_updater?.Dispose();
		}
		base.Dispose(disposing);
	}

	public override NSObject RepresentedObject {
		get => base.RepresentedObject;
		set {
			base.RepresentedObject = value;

			// Update the view, if already loaded.
		}
	}

	private static string FormatLastCheck(DateTime? lastCheckTime)
	{
		return lastCheckTime.HasValue ? lastCheckTime.Value.ToLocalTime().ToString("u") : "-";
	}
}
