using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UpSparkle;

namespace UpSparkleDemos.WpfDemo;

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
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        updater.CheckUpdateWithUI();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        updater.Dispose();
    }
}