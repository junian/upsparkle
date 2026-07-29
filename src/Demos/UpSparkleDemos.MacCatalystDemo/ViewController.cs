using System.Reflection;
using UpSparkle;

namespace UpSparkleDemos.MacCatalystDemo;

public class ViewController : UIViewController
{
    private UpSparkleUpdater? _updater;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.BackgroundColor = UIColor.SystemBackground;

        var stackView = new UIStackView
        {
            Axis = UILayoutConstraintAxis.Vertical,
            Distribution = UIStackViewDistribution.Fill,
            Alignment = UIStackViewAlignment.Center,
            Spacing = 20,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        View.AddSubview(stackView);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            stackView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
            stackView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor)
        });

        var label = new UILabel
        {
            Text = "Hello, Mac Catalyst!",
            TextAlignment = UITextAlignment.Center
        };
        stackView.AddArrangedSubview(label);

        var button = UIButton.FromType(UIButtonType.System);
        button.SetTitle("Check for Updates", UIControlState.Normal);
        button.TouchUpInside += (sender, e) =>
        {
            _updater?.CheckUpdateWithUI();
        };
        stackView.AddArrangedSubview(button);

        InitUpdater();
    }

    private void InitUpdater()
    {
        _updater = new UpSparkleUpdater();
        // Using dummy values for demo purposes
        /*
        _updater.Initialize(
            Assembly.GetExecutingAssembly(),
            "https://sparkle-project.org/files/sparkletestcast.xml",
            "dummy_public_key");
            */
        _updater.Initialize(Assembly.GetExecutingAssembly());
    }

    public void DisposeUpdater()
    {
        _updater?.Dispose();
        _updater = null;
    }
}
