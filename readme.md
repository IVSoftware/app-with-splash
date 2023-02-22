Here's my **2023** take on a **2011** question.
***
Over time, I've done this many times in many ways. The approach that currently use:

- Force the main form `Handle` creation so that the message that creates the splash can be posted into the main form's message queue using `BeginInvoke`. This allows the main form ctor to return. Ordinarily the handle (the native `hWnd`) doesn't come into existence until it's shown. Therefore, it needs to be coerced while it's still hidden.

- Override the `SetVisibleCore()` preventing the main window from becoming visible until the Splash has finished processing.

[![startup flow][1]][1]

***
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

The async initialization can be performed in the Splash class itself _or_ it can fire events causing the main app to do things. Either way, when it closes itself the main form will set the `_initialized` bool to `true` and it is now capable of becoming visible.

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
                labelProgress.Text = "Initializing scenario...";
                progressBar.Value = 75;
                await Task.Delay(1000);
                labelProgress.Text = "Success!";
                progressBar.Value = 75;
                await Task.Delay(1000);
                DialogResult= DialogResult.OK;
            }
        }
    }


  [1]: https://i.stack.imgur.com/zDXTa.png