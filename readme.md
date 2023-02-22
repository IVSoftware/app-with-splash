The `Application.Run` call should only be used on `Program.cs` to invoke the main form. Since, we don't want the main form to be the first form shown, so I have found two things to be important:

- Force the main form Handle creation so that it's the first window _created_. Ordinarily the Handle doesn't come into existence until it's shown. Therefore, it needs to be coerced.

- Override the `SetVisibleCore()` preventing the main window from becoming visible until the Splash has finished processing.

[![screenshot][1]][1]

***
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

***
**Splash Example**

The async initialization can be performed in the Splash class itself _or_ it can fire events causeing the main app to do things. Either way, when it closes itself the main for will set the `_initialized` bool to `true` and it is now capable of being visible.

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
                progressBar.Value = 75;
                await Task.Delay(1000);
                DialogResult= DialogResult.OK;
            }
        }
    }


  [1]: https://i.stack.imgur.com/kUINY.png