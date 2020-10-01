using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using System.Runtime.InteropServices;

namespace UGCS.TelemetryViewer
{
    public static class Program
    {

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return AppBuilder.Configure<TelemetryViewerApp>()
                    .UsePlatformDetect()
                    .UseReactiveUI()
                    .UseDirect2D1()
                    .LogToDebug();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return AppBuilder.Configure<TelemetryViewerApp>()
                    .UsePlatformDetect()
                    .UseReactiveUI()
                    .UseAvaloniaNative()
                    .LogToDebug();
            }
            else
            {
                return AppBuilder.Configure<TelemetryViewerApp>()
                    .UsePlatformDetect()
                    .UseReactiveUI()
                    .LogToDebug();
            }
        }
    }
}
