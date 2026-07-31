using System;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpSparkle;

namespace UpSparkleDemos.AvaloniaDemo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly UpSparkleUpdater sparkle = new();

    [ObservableProperty]
    private string greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private string status = "Library is loaded.";

    [ObservableProperty]
    private string companyName = "-";

    [ObservableProperty]
    private string appName = "-";

    [ObservableProperty]
    private string appVersion = "-";

    [ObservableProperty]
    private bool isAutomaticCheckForUpdates;

    [ObservableProperty]
    private string updateCheckIntervalText = "86400";

    [ObservableProperty]
    private string lastCheckTime = "-";

    public IRelayCommand CheckForUpdatesCommand { get; }

    public MainWindowViewModel()
    {
        CheckForUpdatesCommand = new RelayCommand(CheckForUpdates);
    }

    public void Init()
    {
        /*
        sparkle.Initialize(
            Assembly.GetExecutingAssembly(),
            "https://sparkle-project.org/files/sparkletestcast.xml",
            "replace-with-public-key");
        */
        sparkle.Initialize(Assembly.GetExecutingAssembly());

        CompanyName = sparkle.CompanyName ?? "-";
        AppName     = sparkle.AppName     ?? "-";
        AppVersion  = sparkle.AppVersion  ?? "-";

        IsAutomaticCheckForUpdates = sparkle.IsAutomaticCheckForUpdates;
        UpdateCheckIntervalText    = sparkle.UpdateCheckInterval.ToString();
        LastCheckTime              = sparkle.LastCheckTime?.ToString("u") ?? "-";
    }

    partial void OnIsAutomaticCheckForUpdatesChanged(bool value)
    {
        sparkle.IsAutomaticCheckForUpdates = value;
        Status = $"Automatic check for updates: {(value ? "enabled" : "disabled")}.";
    }

    partial void OnUpdateCheckIntervalTextChanged(string value)
    {
        if (int.TryParse(value, out var seconds) && seconds > 0)
        {
            sparkle.UpdateCheckInterval = seconds;
            Status = $"Update check interval set to {seconds} seconds.";
        }
    }

    private async void CheckForUpdates()
    {
        try
        {
            sparkle.CheckUpdateWithUI();
            Status = "Requested an update check.";
            LastCheckTime = sparkle.LastCheckTime?.ToString("u") ?? "-";
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }
}
