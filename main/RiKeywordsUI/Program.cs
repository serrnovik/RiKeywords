using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using RIKeywords;

namespace RiKeywords {
    class Program {
        public static void Main (string[] args) => BuildAvaloniaApp ()
            .StartWithClassicDesktopLifetime (args);

        // Avalonia configuration, don't remove;
        public static AppBuilder BuildAvaloniaApp () => AppBuilder.Configure<App> ()
            .UsePlatformDetect ()
            .LogToDebug ();
    }
}