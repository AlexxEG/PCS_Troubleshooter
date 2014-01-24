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

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(linkLabel1.Text);
            }
            catch
            {
                MessageBox.Show(this, "Couldn't open link.");
            }
        }
    }
}
