using System.Drawing;
using System.Drawing.Drawing2D;

namespace ClaudeLight;

/// <summary>
/// Renders the tray icon: a Claude-style 4-petal sparkle,
/// green for off-peak, red for peak. Matches the macOS NSImage drawing.
/// </summary>
static class TrayIconRenderer
{
    private const int Size = 16; // Windows tray icons are 16×16

    public static Icon Create(bool isPeak)
    {
        using var bmp = new Bitmap(Size, Size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var g   = Graphics.FromImage(bmp);

        g.Clear(Color.Transparent);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var baseColor = isPeak ? Color.FromArgb(220, 38, 38)   // red-600
                                : Color.FromArgb(34,  197, 94); // green-500
        using var brush = new SolidBrush(baseColor);

        float cx = Size / 2f;
        float cy = Size / 2f;
        float petalLength = 5.5f;
        float petalWidth  = 2.0f;

        // 4 petals at 0°, 45°, 90°, 135° — matches macOS sparkle
        for (int i = 0; i < 4; i++)
        {
            float angleDeg = i * 45f;
            var   state    = g.Save();

            g.TranslateTransform(cx, cy);
            g.RotateTransform(angleDeg);

            g.FillEllipse(brush,
                -petalWidth / 2,
                -petalLength,
                petalWidth,
                petalLength * 2);

            g.Restore(state);
        }

        // Center dot
        float cd = 2.5f;
        g.FillEllipse(brush, cx - cd / 2, cy - cd / 2, cd, cd);

        // Convert Bitmap → Icon without an .ico file on disk
        IntPtr hIcon = bmp.GetHicon();
        var icon = Icon.FromHandle(hIcon);

        // Clone so we can safely destroy the HICON
        var result = (Icon)icon.Clone();
        NativeMethods.DestroyIcon(hIcon);
        return result;
    }
}
