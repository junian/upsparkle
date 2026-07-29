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

		var checkUpdateButton = new NSButton
		{
			Title = "Check for update",
			BezelStyle = NSBezelStyle.Rounded,
			Frame = new CoreGraphics.CGRect(20, 20, 150, 40)
		};

		checkUpdateButton.Activated += (sender, e) =>
		{
			_updater.CheckUpdateWithUI();
		};

		View.AddSubview(checkUpdateButton);
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
}
