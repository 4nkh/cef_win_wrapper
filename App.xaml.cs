using CefSharp;
using CefSharp.Wpf;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System;

namespace cefWinWrapper
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Clean processes
            try
            {
                foreach (var p in Process.GetProcessesByName("CefSharp.BrowserSubprocess"))
                { try { p.Kill(); p.WaitForExit(500); } catch { } }
            }
            catch { }

            var settings = new CefSettings();
            settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) CefWinWrapperApp/1.0";

            // Caching
            string cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefWinWrapperApp", "Cache");

            if (Directory.Exists(cachePath))
            {
                try
                {
                    var lockFiles = Directory.GetFiles(cachePath, "LOCK*", SearchOption.AllDirectories);
                    foreach (var file in lockFiles) { File.Delete(file); }
                }
                catch { }
            }

            settings.CachePath = cachePath;
            settings.PersistSessionCookies = true;

            // .NET 8
            string runtimePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", "win-x64", "native");
            if (Directory.Exists(runtimePath))
            {
                settings.BrowserSubprocessPath = Path.Combine(runtimePath, "CefSharp.BrowserSubprocess.exe");
                settings.LocalesDirPath = Path.Combine(runtimePath, "locales");
                settings.ResourcesDirPath = runtimePath;
            }

            settings.BrowserSubprocessPath = Path.Combine(runtimePath, "CefSharp.BrowserSubprocess.exe");

            // CEF Initialization
            Cef.Initialize(settings);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Cef.Shutdown();
            try
            {
                var subordinates = Process.GetProcessesByName("CefSharp.BrowserSubprocess");
                foreach (var p in subordinates) { p.Kill(); }
            }
            catch { }
            base.OnExit(e);
        }
    }
}