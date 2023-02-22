using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace app_with_login
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Debug.Assert(!IsHandleCreated, "Expecting handle is not yet created.");
            // Ordinarily we don't get the handle until
            // window is shown. But we want it now.
            _ = Handle;
            Debug.Assert(IsHandleCreated, "Expecting handle exists.");

            // Call BeginInvoke on the new handle so as not to block the CTor.
            BeginInvoke(new Action(()=> StartForm()));
        }
        protected override void SetVisibleCore(bool value) =>
            base.SetVisibleCore(value && _initialized);

        bool _initialized = false;

        private void StartForm()
        {
            using (var splash = new SplashForm())
            {
                splash.ShowDialog();
            }
            _initialized= true;
            WindowState = FormWindowState.Maximized;

            Debug.Assert(Application.OpenForms.Count.Equals(0));
            Show();
            Debug.Assert(Application.OpenForms.Count.Equals(1));
            Debug.Assert(
                Application.OpenForms[0] == this, 
                "Expecting this is the application's main form."
            );
        }
    }
}
