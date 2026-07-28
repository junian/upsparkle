using System.Windows.Forms;
using UpSparkle;

namespace UpSparkleDemos.WinFormsDemo
{
    public partial class Form1 : Form
    {
        private UpSparkleUpdater updater;
        public Form1()
        {
            InitializeComponent();
            updater = new UpSparkleUpdater();
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            updater.Init(
                "https://sparkle-project.org/files/sparkletestcast.xml",
                "MCowBQYDK2VwAyEA0+6Z1g5k3l7J8x4F9G",
                System.Reflection.Assembly.GetExecutingAssembly());
        }

        private void btnCheckUpdate_Click(object sender, System.EventArgs e)
        {
            updater.CheckUpdateWithUI();
        }

        
    }
}