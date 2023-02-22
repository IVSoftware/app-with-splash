using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace app_with_login
{
    public partial class SplashForm : Form
    {
        public SplashForm()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
        }
        protected async override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                labelProgress.Text = "Updating installation...";
                progressBar.Value = 5;
                await Task.Delay(1000);
                labelProgress.Text = "Loading avatars...";
                progressBar.Value = 25;
                await Task.Delay(1000);
                labelProgress.Text = "Fetching game history...";
                progressBar.Value = 50;
                await Task.Delay(1000);
                labelProgress.Text = "Initializing scene...";
                progressBar.Value = 75;
                await Task.Delay(1000);
                labelProgress.Text = "Success!";
                progressBar.Value = 100;
                await Task.Delay(1000);
                DialogResult= DialogResult.OK;
            }
        }
    }
}
