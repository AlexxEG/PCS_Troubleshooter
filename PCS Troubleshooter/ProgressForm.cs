using System.Windows.Forms;

namespace PCS_Troubleshooter
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public delegate void CloseDelegate();
        public delegate void SetMaximumDelegate(int max);
        public delegate void SetProgressIntDelegate(int progress);
        public delegate void SetProgressStringDelegate(string progress);

        public new void Close()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new CloseDelegate(Close));
            }
            else
            {
                base.Close();
            }
        }

        public void SetMaximum(int max)
        {
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar1.Invoke(new SetMaximumDelegate(SetMaximum), new object[] { max });
            }
            else
            {
                this.progressBar1.Maximum = max;
            }
        }

        public void SetProgress(int progress)
        {
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar1.Invoke(new SetProgressIntDelegate(SetProgress), new object[] { progress });
            }
            else
            {
                this.progressBar1.Value = progress;
            }
        }

        public void SetProgressText(string progress)
        {
            if (this.lProgress.InvokeRequired)
            {
                this.lProgress.Invoke(new SetProgressStringDelegate(SetProgressText), new object[] { progress });
            }
            else
            {
                this.lProgress.Text = progress;
            }
        }
    }
}
