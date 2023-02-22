using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace app_with_login
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            // Ordinarily we don't get the handle until
            // window is shown. But we want it now.
            _ = Handle;
            // Call BeginInvoke on the new handle so as not to block the CTor.
            BeginInvoke(new Action(()=> execSplashFlow()));
        }
        protected override void SetVisibleCore(bool value) =>
            base.SetVisibleCore(value && _initialized);

        bool _initialized = false;

        private void execSplashFlow()
        {
            using (var splash = new SplashForm())
            {
                splash.ShowDialog();
            }
            _initialized= true;
            WindowState = FormWindowState.Maximized;
            Show();
        }
    }
}
