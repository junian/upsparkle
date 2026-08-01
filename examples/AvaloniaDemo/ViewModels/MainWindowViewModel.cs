using System;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpSparkle;

namespace AvaloniaDemo.ViewModels;

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

    [ObservableProperty]
    private string httpHeaderName = "";

    [ObservableProperty]
    private string httpHeaderValue = "";

    public IRelayCommand CheckForUpdatesCommand { get; }

    public IRelayCommand CheckForUpdatesWithoutUICommand { get; }

    public IRelayCommand SetHttpHeaderCommand { get; }

    public IRelayCommand ClearHttpHeadersCommand { get; }

    public MainWindowViewModel()
    {
        CheckForUpdatesCommand = new RelayCommand(CheckForUpdates);
        CheckForUpdatesWithoutUICommand = new RelayCommand(CheckForUpdatesWithoutUI);
        SetHttpHeaderCommand = new RelayCommand(SetHttpHeader);
        ClearHttpHeadersCommand = new RelayCommand(ClearHttpHeaders);

        sparkle.Error += (sender, args) => Status = "An error occurred while checking for updates.";
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

    private async void CheckForUpdatesWithoutUI()
    {
        try
        {
            sparkle.CheckUpdateWithoutUI();
            Status = "Requested a background update check.";
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }

    private void SetHttpHeader()
    {
        if (string.IsNullOrWhiteSpace(HttpHeaderName))
        {
            Status = "HTTP header name is required.";
            return;
        }

        sparkle.SetHttpHeader(HttpHeaderName, HttpHeaderValue);
        Status = $"HTTP header set: {HttpHeaderName}: {HttpHeaderValue}";
    }

    private void ClearHttpHeaders()
    {
        sparkle.ClearHttpHeaders();
        Status = "HTTP headers cleared.";
    }
}
