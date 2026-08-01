using System;
using System.Windows;
using UpSparkle;

namespace WpfDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private UpSparkleUpdater updater;

    public MainWindow()
    {
        InitializeComponent();

        updater = new UpSparkleUpdater();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        /*
        updater.Initialize(
            System.Reflection.Assembly.GetExecutingAssembly(),
                "https://sparkle-project.org/files/sparkletestcast.xml",
                "MCowBQYDK2VwAyEA0+6Z1g5k3l7J8x4F9G");
        */

        updater.Initialize(System.Reflection.Assembly.GetExecutingAssembly());
        updater.Error += (s, args) => StatusTextBlock.Text = "An error occurred while checking for updates.";

        AutomaticChecksCheckBox.IsChecked = updater.IsAutomaticCheckForUpdates;
        IntervalTextBox.Text = updater.UpdateCheckInterval.ToString();
        LastCheckTextBlock.Text = FormatLastCheck(updater.LastCheckTime);
    }

    private void OnAutomaticChecksChanged(object sender, RoutedEventArgs e)
    {
        updater.IsAutomaticCheckForUpdates = AutomaticChecksCheckBox.IsChecked == true;
    }

    private void OnIntervalLostFocus(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(IntervalTextBox.Text, out var seconds) && seconds > 0)
        {
            updater.UpdateCheckInterval = seconds;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        updater.CheckUpdateWithUI();
        LastCheckTextBlock.Text = FormatLastCheck(updater.LastCheckTime);
        StatusTextBlock.Text = "Requested an update check.";
    }

    private void OnCheckForUpdatesWithoutUIClicked(object sender, RoutedEventArgs e)
    {
        updater.CheckUpdateWithoutUI();
        StatusTextBlock.Text = "Requested a background update check.";
    }

    private void OnSetHttpHeaderClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(HeaderNameTextBox.Text))
        {
            StatusTextBlock.Text = "HTTP header name is required.";
            return;
        }

        updater.SetHttpHeader(HeaderNameTextBox.Text, HeaderValueTextBox.Text);
        StatusTextBlock.Text = $"HTTP header set: {HeaderNameTextBox.Text}: {HeaderValueTextBox.Text}";
    }

    private void OnClearHttpHeadersClicked(object sender, RoutedEventArgs e)
    {
        updater.ClearHttpHeaders();
        StatusTextBlock.Text = "HTTP headers cleared.";
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        updater.Dispose();
    }

    private static string FormatLastCheck(DateTime? lastCheckTime)
    {
        return lastCheckTime.HasValue ? lastCheckTime.Value.ToLocalTime().ToString("u") : "-";
    }
}
