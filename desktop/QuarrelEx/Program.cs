namespace QuarrelEx;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Explicit Per-Monitor V2 support. This must happen before any HWND is
        // created so moving the editor between monitors with different scaling
        // (100% / 125% / 150% / 200%) is handled correctly.
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetDefaultFont(SystemFonts.MessageBoxFont);
        Application.Run(new MainForm());
    }
}
