using Avalonia.Controls;
using UpSparkleDemos.AvaloniaDemo.ViewModels;

namespace UpSparkleDemos.AvaloniaDemo.Views;

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