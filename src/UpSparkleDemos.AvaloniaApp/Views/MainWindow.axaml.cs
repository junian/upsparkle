using Avalonia.Controls;
using UpSparkleDemos.AvaloniaApp.ViewModels;

namespace UpSparkleDemos.AvaloniaApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContextChanged += (sender, args) =>
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                vm.Init();
            }
        };
    }
}