using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

using com.lightstreamer.client;

#pragma warning disable CA1416 // Validate platform compatibility (only works on windows - ok!)

class Program : SubscriptionListener
{
    [STAThread]
    static void Main() => new Program().Start();

    NotifyIcon pissIcon;

    public Program()
    {
        var pissSubscription = new Subscription("MERGE", ["NODE3000005"], ["Value"]);
        pissSubscription.addListener(this);

        var pissClient = new LightstreamerClient("https://push.lightstreamer.com", "ISSLIVE");
        pissClient.subscribe(pissSubscription);
        pissClient.connect();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        pissIcon = new NotifyIcon
        {
            Icon = GeneratePercentageIcon("??"),
            Text = "Loading...",
            Visible = true
        };

        ContextMenuStrip contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("About", null, (_, _) => MessageBox.Show($"This tray icon shows the live urine tank percentage in the International Space Station.\nCurrently:\n\n{pissIcon.Text}", "About"));
        contextMenu.Items.Add("Donate", null, (_, _) => OpenUrl("https://buymeacoffee.com/granthandy"));
        contextMenu.Items.Add("Original", null, (_, _) => OpenUrl("https://github.com/Jaennaet/pISSStream"));
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, (_, _) => Application.Exit());

        pissIcon.ContextMenuStrip = contextMenu;
    }

    void Start()
        => Application.Run();

    static void OpenUrl(string url)
        => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

    void SubscriptionListener.onItemUpdate(ItemUpdate itemUpdate)
    {
        string pissPercentage = itemUpdate.getValue("Value") ?? "??";

        pissIcon.Text = $"🛰️🚽: {pissPercentage}%";
        pissIcon.Icon = GeneratePercentageIcon(pissPercentage);
    }

    public void onSubscriptionError(int code, string message)
    {
        MessageBox.Show($"Code: {code}, Message: {message}", "Failed to Subscribe:");
    }

    static Icon GeneratePercentageIcon(string text)
    {
        const int size = 64;

        using (Bitmap bitmap = new Bitmap(size, size))
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.Clear(Color.Yellow);

            using (Font font = new Font("Arial", 45, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Brush textBrush = Brushes.Black)
            {
                SizeF textSize = graphics.MeasureString(text, font);

                graphics.DrawString(
                    text,
                    font,
                    textBrush,
                    (size - textSize.Width) / 2,
                    (size - textSize.Height) / 2
                );
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }
    }

    public void onClearSnapshot(string itemName, int itemPos) { }
    public void onCommandSecondLevelItemLostUpdates(int lostUpdates, string key) { }
    public void onCommandSecondLevelSubscriptionError(int code, string message, string key) { }
    public void onEndOfSnapshot(string itemName, int itemPos) { }
    public void onItemLostUpdates(string itemName, int itemPos, int lostUpdates) { }
    public void onListenEnd() { }
    public void onListenStart() { }
    public void onRealMaxFrequency(string frequency) { }
    public void onSubscription() { }
    public void onUnsubscription() { }
}

#pragma warning restore CA1416 // Validate platform compatibility
