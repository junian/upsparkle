using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpSparkle;

namespace UpSparkleDemos.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly UpSparkleUpdater sparkle = new();

    [ObservableProperty]
    private string greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private string status = "Library is loaded.";

    public IRelayCommand CheckForUpdatesCommand { get; }

    public MainWindowViewModel()
    {
        CheckForUpdatesCommand = new RelayCommand(CheckForUpdates);
    }

    private void CheckForUpdates()
    {
        try
        {
            if (!sparkle.IsInitialized)
            {
                sparkle.Init(
                    "https://sparkle-project.org/files/sparkletestcast.xml",
                    "replace-with-public-key",
                    "UpSparkleDemos",
                    "UpSparkleDemos.AvaloniaApp",
                    "0.0.1");
            }

            sparkle.CheckUpdateWithUI();
            Status = "Requested an update check.";
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }
}
