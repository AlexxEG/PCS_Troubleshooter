using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PCS_Troubleshooter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.SetupMainMenu();
            this.Text = string.Format("{0} {1}", this.Text, Program.Version);
        }

        private void btnTroubleshoot_Click(object sender, EventArgs e)
        {
            using (var tf = new TroubleshootingForm())
                tf.ShowDialog(this);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region MainMenu

        private void SetupMainMenu()
        {
            var mainMenu = new MainMenu();
            var links = mainMenu.MenuItems.Add("&Links");

            links.MenuItems.Add("&Fallout 3 Nexus Mod Page", Fallout3NexusModPage_Click);
            links.MenuItems.Add("&GitHub Page", GitHubPage_Click);
            links.MenuItems.Add("-");
            links.MenuItems.Add("&Report A Error", ReportAError_Click);

            this.Menu = mainMenu;
        }

        private void Fallout3NexusModPage_Click(object sender, EventArgs e)
        {
            this.OpenLink(@"http://www.nexusmods.com/fallout3/mods/9679");
        }

        private void GitHubPage_Click(object sender, EventArgs e)
        {
            this.OpenLink(@"https://github.com/AlexxEG/PCS_Troubleshooter");
        }

        private void ReportAError_Click(object sender, EventArgs e)
        {
            this.OpenLink(@"https://github.com/AlexxEG/PCS_Troubleshooter/issues");
        }

        #endregion

        private void OpenLink(string link)
        {
            try
            {
                Process.Start(link);
            }
            catch
            {
                MessageBox.Show(this, "Couldn't open link.");
            }
        }
    }
}
