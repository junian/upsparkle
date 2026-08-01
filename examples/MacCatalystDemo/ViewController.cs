using System.Reflection;
using UIKit;
using UpSparkle;

namespace MacCatalystDemo;

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

        var backgroundButton = UIButton.FromType(UIButtonType.System);
        backgroundButton.SetTitle("Check for Updates (no UI)", UIControlState.Normal);
        backgroundButton.TouchUpInside += (sender, e) =>
        {
            _updater?.CheckUpdateWithoutUI();
            ShowStatus("Requested a background update check.");
        };
        stackView.AddArrangedSubview(backgroundButton);

        var headerNameLabel = new UILabel
        {
            Text = "HTTP header name:",
            TextColor = UIColor.SecondaryLabel
        };
        stackView.AddArrangedSubview(headerNameLabel);

        var headerNameField = new UITextField
        {
            Placeholder = "X-Custom-Header",
            TextAlignment = UITextAlignment.Center
        };
        stackView.AddArrangedSubview(headerNameField);

        var headerValueLabel = new UILabel
        {
            Text = "HTTP header value:",
            TextColor = UIColor.SecondaryLabel
        };
        stackView.AddArrangedSubview(headerValueLabel);

        var headerValueField = new UITextField
        {
            Placeholder = "header-value",
            TextAlignment = UITextAlignment.Center
        };
        stackView.AddArrangedSubview(headerValueField);

        var setHeaderButton = UIButton.FromType(UIButtonType.System);
        setHeaderButton.SetTitle("Set header", UIControlState.Normal);
        setHeaderButton.TouchUpInside += (sender, e) =>
        {
            if (string.IsNullOrWhiteSpace(headerNameField.Text))
            {
                ShowStatus("HTTP header name is required.");
                return;
            }

            _updater?.SetHttpHeader(headerNameField.Text, headerValueField.Text);
            ShowStatus($"HTTP header set: {headerNameField.Text}: {headerValueField.Text}");
        };
        stackView.AddArrangedSubview(setHeaderButton);

        var clearHeadersButton = UIButton.FromType(UIButtonType.System);
        clearHeadersButton.SetTitle("Clear headers", UIControlState.Normal);
        clearHeadersButton.TouchUpInside += (sender, e) =>
        {
            _updater?.ClearHttpHeaders();
            ShowStatus("HTTP headers cleared.");
        };
        stackView.AddArrangedSubview(clearHeadersButton);

        InitUpdater();

        automaticSwitch.On = _updater!.IsAutomaticCheckForUpdates;
        intervalField.Text = _updater!.UpdateCheckInterval.ToString();
        lastCheckLabel.Text = $"Last check: {FormatLastCheck(_updater.LastCheckTime)}";
    }

    private void InitUpdater()
    {
        _updater = new UpSparkleUpdater();
        _updater.Error += (sender, args) => ShowStatus("An error occurred while checking for updates.");
        // Using dummy values for demo purposes
        /*
        _updater.Initialize(
            Assembly.GetExecutingAssembly(),
            "https://sparkle-project.org/files/sparkletestcast.xml",
            "dummy_public_key");
            */
        _updater.Initialize(Assembly.GetExecutingAssembly());
    }

    private void ShowStatus(string message)
    {
        var alert = new UIAlertController
        {
            Title = message
        };
        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
        PresentViewController(alert, true, null);
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
