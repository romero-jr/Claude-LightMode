using System.Windows.Forms;

namespace ClaudeLight;

/// <summary>
/// Root application context. Owns the NotifyIcon (tray), the context menu,
/// and the 30-second update timer. Equivalent to ClaudeLightApp + AppDelegate.
/// </summary>
sealed class TrayApplicationContext : ApplicationContext
{
    private readonly PeakHoursService _service  = new();
    private readonly NotifyIcon       _trayIcon = new();
    private readonly System.Windows.Forms.Timer _timer = new();

    // Menu items we need to update dynamically
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _ptTimeItem;
    private readonly ToolStripMenuItem _scheduleItem;
    private readonly ToolStripMenuItem _launchAtLoginItem;
    private readonly ToolStripMenuItem _notifyItem;

    private bool _notifyOnOffPeak = false;

    public TrayApplicationContext()
    {
        // ---- Build context menu ----
        _statusItem      = new ToolStripMenuItem { Enabled = false, Font = BoldFont() };
        _ptTimeItem      = new ToolStripMenuItem { Enabled = false };
        _scheduleItem    = new ToolStripMenuItem("Mon–Fri  8:00 AM – 2:00 PM ET") { Enabled = false };

        var docsItem     = new ToolStripMenuItem("About Peak Hours…");
        docsItem.Click  += (_, _) =>
            OpenUrl("https://docs.anthropic.com/en/docs/about-claude/models#model-availability");

        _launchAtLoginItem        = new ToolStripMenuItem("Launch at Login");
        _launchAtLoginItem.Click += ToggleLaunchAtLogin;
        _launchAtLoginItem.Checked = LaunchAtLoginService.IsEnabled;

        _notifyItem        = new ToolStripMenuItem("Notify when off-peak starts");
        _notifyItem.Click += ToggleNotify;

        var quitItem     = new ToolStripMenuItem("Quit ClaudeLight");
        quitItem.Click  += (_, _) => ExitThread();

        var menu = new ContextMenuStrip();
        menu.Items.AddRange(new ToolStripItem[]
        {
            _statusItem,
            _ptTimeItem,
            new ToolStripSeparator(),
            _scheduleItem,
            new ToolStripSeparator(),
            docsItem,
            _launchAtLoginItem,
            _notifyItem,
            new ToolStripSeparator(),
            quitItem,
        });

        // ---- Tray icon ----
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.Visible          = true;

        // ---- Wire up service events ----
        _service.StateChanged   += OnStateChanged;
        _service.OffPeakStarted += OnOffPeakStarted;

        // ---- Initial update ----
        _service.Update();

        // ---- 30-second timer (same cadence as macOS version) ----
        _timer.Interval = 30_000;
        _timer.Tick    += (_, _) => _service.Update();
        _timer.Start();
    }

    // -------------------------------------------------------------------------
    // State change handler
    // -------------------------------------------------------------------------

    private void OnStateChanged(object? sender, EventArgs e)
    {
        // NotifyIcon must be updated on the UI thread
        if (_trayIcon.ContextMenuStrip?.InvokeRequired == true)
        {
            _trayIcon.ContextMenuStrip.Invoke(RefreshUI);
            return;
        }
        RefreshUI();
    }

    private void RefreshUI()
    {
        // Dispose previous icon to avoid GDI handle leak
        var oldIcon = _trayIcon.Icon;

        _trayIcon.Icon    = TrayIconRenderer.Create(_service.IsPeak);
        _trayIcon.Text    = _service.StatusText; // tooltip (max 63 chars on Windows)

        _statusItem.Text  = _service.IsPeak ? "🔴  Peak Hours" : "🟢  Off-Peak";
        _ptTimeItem.Text  = _service.PacificTimeText;

        oldIcon?.Dispose();
    }

    // -------------------------------------------------------------------------
    // Notification
    // -------------------------------------------------------------------------

    private void OnOffPeakStarted(object? sender, EventArgs e)
    {
        if (!_notifyOnOffPeak) return;

        // BalloonTip is the simplest cross-version approach.
        // On Windows 10/11 this surfaces as a toast via the action center.
        _trayIcon.BalloonTipTitle = "ClaudeLight";
        _trayIcon.BalloonTipText  = "Off-peak started. Full speed ahead! 🟢";
        _trayIcon.BalloonTipIcon  = ToolTipIcon.Info;
        _trayIcon.ShowBalloonTip(5000);
    }

    // -------------------------------------------------------------------------
    // Menu actions
    // -------------------------------------------------------------------------

    private void ToggleLaunchAtLogin(object? sender, EventArgs e)
    {
        try
        {
            bool newValue = !LaunchAtLoginService.IsEnabled;
            LaunchAtLoginService.SetEnabled(newValue);
            _launchAtLoginItem.Checked = newValue;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not update launch at login:\n{ex.Message}",
                "ClaudeLight", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ToggleNotify(object? sender, EventArgs e)
    {
        _notifyOnOffPeak      = !_notifyOnOffPeak;
        _notifyItem.Checked   = _notifyOnOffPeak;
    }

    // -------------------------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------------------------

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Icon?.Dispose();
            _trayIcon.Dispose();
        }
        base.Dispose(disposing);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static void OpenUrl(string url)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { /* swallow */ }
    }

    private static Font BoldFont() =>
        new(SystemFonts.MenuFont ?? SystemFonts.DefaultFont, FontStyle.Bold);
}
