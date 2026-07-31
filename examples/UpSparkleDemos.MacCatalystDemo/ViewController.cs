using System.Reflection;
using UIKit;
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

        // Update settings (backed by IsAutomaticCheckForUpdates / UpdateCheckInterval / LastCheckTime)
        var automaticLabel = new UILabel
        {
            Text = "Automatic checks:",
            TextColor = UIColor.SecondaryLabel
        };
        stackView.AddArrangedSubview(automaticLabel);

        var automaticSwitch = new UISwitch();
        automaticSwitch.ValueChanged += (sender, e) =>
        {
            _updater!.IsAutomaticCheckForUpdates = automaticSwitch.On;
        };
        stackView.AddArrangedSubview(automaticSwitch);

        var intervalLabel = new UILabel
        {
            Text = "Check interval (s):",
            TextColor = UIColor.SecondaryLabel
        };
        stackView.AddArrangedSubview(intervalLabel);

        var intervalField = new UITextField
        {
            Placeholder = "86400",
            KeyboardType = UIKeyboardType.NumberPad,
            TextAlignment = UITextAlignment.Center
        };
        intervalField.EditingDidEnd += (sender, e) =>
        {
            if (int.TryParse(intervalField.Text, out var seconds) && seconds > 0)
            {
                _updater!.UpdateCheckInterval = seconds;
            }
        };
        stackView.AddArrangedSubview(intervalField);

        var lastCheckLabel = new UILabel
        {
            Text = $"Last check: {FormatLastCheck(_updater?.LastCheckTime)}",
            TextColor = UIColor.SecondaryLabel,
            TextAlignment = UITextAlignment.Center
        };
        stackView.AddArrangedSubview(lastCheckLabel);

        var button = UIButton.FromType(UIButtonType.System);
        button.SetTitle("Check for Updates", UIControlState.Normal);
        button.TouchUpInside += (sender, e) =>
        {
            _updater?.CheckUpdateWithUI();
            lastCheckLabel.Text = $"Last check: {FormatLastCheck(_updater?.LastCheckTime)}";
        };
        stackView.AddArrangedSubview(button);

        InitUpdater();

        automaticSwitch.On = _updater!.IsAutomaticCheckForUpdates;
        intervalField.Text = _updater!.UpdateCheckInterval.ToString();
        lastCheckLabel.Text = $"Last check: {FormatLastCheck(_updater.LastCheckTime)}";
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

    private static string FormatLastCheck(DateTime? lastCheckTime)
    {
        return lastCheckTime.HasValue ? lastCheckTime.Value.ToLocalTime().ToString("u") : "-";
    }
}
