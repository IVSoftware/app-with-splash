Looking at your code, the fundamental issue is that the `Application.Run(Form)` method is a blocking call intended to run the message loop of the main form specifically. The Microsoft documentation for [Application.Run(Form)](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.application.run) explains:

>Begins running a standard application message loop on the current thread, and makes the specified form visible. [...] Every running Windows-based application requires an active message loop, called the main message loop. When the main message loop is closed, the application exits.

So, it should only be used in `Program.cs` to invoke the main form. 

***
Reading your code, the intent seems to be to spawn a separate thread to display a splash screen. But the splash screen has a message loop also, which needs to run on the UI thread (even as it spawns background tasks to make API calls and read databases and the like).

Since we don't want the main form to be the first form shown, I have found two things to be important:

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
            Show();
        }
    }

***
**Splash Example**

The async initialization can be performed in the Splash class itself _or_ it can fire events causing the main app to do things. Either way, when it closes itself the main form will set the `_initialized` bool to `true` and it is now capable of being visible.

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


  [1]: https://i.stack.imgur.com/zDXTa.png