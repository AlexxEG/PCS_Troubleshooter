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
            {
                tf.ShowDialog(this);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region MainMenu

        private void SetupMainMenu()
        {
            MainMenu mainmenu = new MainMenu();

            MenuItem links = mainmenu.MenuItems.Add("&Links");

            links.MenuItems.Add("&Fallout 3 Nexus Mod Page", Fallout3NexusModPage_Click);
            links.MenuItems.Add("&GitHub Page", GitHubPage_Click);
            links.MenuItems.Add("-");
            links.MenuItems.Add("&Report A Error", ReportAError_Click);

            this.Menu = mainmenu;
        }

        private void Fallout3NexusModPage_Click(object sender, EventArgs e)
        {
            try
            {
                string link = @"http://www.nexusmods.com/fallout3/mods/9679";

                Process.Start(link);
            }
            catch
            {
                MessageBox.Show(this, "Couldn't open link.");
            }
        }

        private void GitHubPage_Click(object sender, EventArgs e)
        {
            try
            {
                string link = @"https://github.com/AlexxEG/PCS_Troubleshooter";

                Process.Start(link);
            }
            catch
            {
                MessageBox.Show(this, "Couldn't open link.");
            }
        }

        private void ReportAError_Click(object sender, EventArgs e)
        {
            try
            {
                string link = @"https://github.com/AlexxEG/PCS_Troubleshooter/issues";

                Process.Start(link);
            }
            catch
            {
                MessageBox.Show(this, "Couldn't open link.");
            }
        }

        #endregion
    }
}
