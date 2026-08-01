using System;
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
            /*
            updater.Initialize(
                System.Reflection.Assembly.GetExecutingAssembly(),
                "https://sparkle-project.org/files/sparkletestcast.xml",
                "MCowBQYDK2VwAyEA0+6Z1g5k3l7J8x4F9G");
            */
            updater.Initialize(System.Reflection.Assembly.GetExecutingAssembly());
            updater.Error += (s, args) => lblStatus.Text = "An error occurred while checking for updates.";

            chkAutomaticChecks.Checked = updater.IsAutomaticCheckForUpdates;
            txtInterval.Text = updater.UpdateCheckInterval.ToString();
            lblLastCheckValue.Text = FormatLastCheck(updater.LastCheckTime);
        }

        private void chkAutomaticChecks_CheckedChanged(object sender, System.EventArgs e)
        {
            updater.IsAutomaticCheckForUpdates = chkAutomaticChecks.Checked;
        }

        private void txtInterval_Leave(object sender, System.EventArgs e)
        {
            int seconds;
            if (int.TryParse(txtInterval.Text, out seconds) && seconds > 0)
            {
                updater.UpdateCheckInterval = seconds;
            }
        }

        private void btnCheckUpdate_Click(object sender, System.EventArgs e)
        {
            updater.CheckUpdateWithUI();
            lblLastCheckValue.Text = FormatLastCheck(updater.LastCheckTime);
            lblStatus.Text = "Requested an update check.";
        }

        private void btnCheckUpdateWithoutUI_Click(object sender, System.EventArgs e)
        {
            updater.CheckUpdateWithoutUI();
            lblStatus.Text = "Requested a background update check.";
        }

        private void btnSetHeader_Click(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHeaderName.Text))
            {
                lblStatus.Text = "HTTP header name is required.";
                return;
            }

            updater.SetHttpHeader(txtHeaderName.Text, txtHeaderValue.Text);
            lblStatus.Text = $"HTTP header set: {txtHeaderName.Text}: {txtHeaderValue.Text}";
        }

        private void btnClearHeaders_Click(object sender, System.EventArgs e)
        {
            updater.ClearHttpHeaders();
            lblStatus.Text = "HTTP headers cleared.";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            updater.Dispose();
        }

        private static string FormatLastCheck(DateTime? lastCheckTime)
        {
            return lastCheckTime.HasValue ? lastCheckTime.Value.ToLocalTime().ToString("u") : "-";
        }
    }
}
