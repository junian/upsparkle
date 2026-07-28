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

    public IRelayCommand CheckForUpdatesCommand { get; }

    public MainWindowViewModel()
    {
        CheckForUpdatesCommand = new RelayCommand(CheckForUpdates);
    }

    public void Init()
    {
        sparkle.Init(
            "https://sparkle-project.org/files/sparkletestcast.xml",
            "replace-with-public-key",
            Assembly.GetExecutingAssembly());

        CompanyName = sparkle.CompanyName ?? "-";
        AppName     = sparkle.AppName     ?? "-";
        AppVersion  = sparkle.AppVersion  ?? "-";
    }

    private async void CheckForUpdates()
    {
        try
        {
            sparkle.CheckUpdateWithUI();
            Status = "Requested an update check.";
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }
}
