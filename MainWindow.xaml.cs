using CefSharp;
using CefSharp.Wpf;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Geolocation;
using System.Runtime.InteropServices;
using System.Windows.Interop;


namespace cefWinWrapper
{
    public partial class MainWindow : Window
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);
        private bool _firstLoadDone = false;
        public MainWindow()
        {
            SetCurrentProcessExplicitAppUserModelID("ankh.cef_win_wrapper");
            InitializeComponent();

            // replace the browser uri with your web app url
            string origin = "https://schedulum.org";
            string deviceId = MachineIdentity.GetPersistentMachineID();
            DeviceIdTextBlock.Text = $"DEVICE ID : {deviceId}";

            // Authorize Location & Notification
            browser.PermissionHandler = new CustomPermissionHandler();

            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            browser.JavascriptObjectRepository.Register("bridgeNotificationAsync", new CefWinWrapperBridge(browser));

            // You should pass the deviceId using a POST REQUEST or at least with encryption
            browser.Address = $"{origin}?deviceId={deviceId}";

            browser.LoadError += async (s, e) =>
            {
                if (e.Frame.IsMain && (e.ErrorCode == CefErrorCode.Aborted) && !_firstLoadDone)
                {
                    //System.Diagnostics.Debug.WriteLine("DEBUG: Loading interrupt by WPF. Connection attempt...");

                    _firstLoadDone = true;

                    Dispatcher.Invoke(() =>
                    {
                        if (browser.IsBrowserInitialized)
                        {
                            browser.Reload(true);
                            // System.Diagnostics.Debug.WriteLine("DEBUG: Send Forced Reload");
                        }
                    });

                    await Task.Delay(6000);

                    Dispatcher.Invoke(() =>
                    {
                        // System.Diagnostics.Debug.WriteLine("DEBUG: Launch Splash Animation");
                        var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
                        {
                            From = 1,
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(2000)
                        };

                        fadeOut.Completed += (s2, e2) => {
                            SplashScreenGrid.Visibility = Visibility.Collapsed;
                        };

                        SplashScreenGrid.BeginAnimation(OpacityProperty, fadeOut);
                    });
                }
            };

            this.KeyDown += (s, e) => {
                // Reload url page with Ctrl+F5
                if (e.Key == System.Windows.Input.Key.F5)
                {
                    browser.Reload(true);
                }
                // Inspect cefSharp browser with Ctrl+F12
                // if (e.Key == System.Windows.Input.Key.F12)
                // {
                //     browser.ShowDevTools();
                // }
            };
        }
    }

    public class CustomPermissionHandler : IPermissionHandler
    {
        // Classic Permissions (Notifications, Gps)
        public bool OnPermissionRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, uint resources, IPermissionPromptCallback callback)
        {
            using (callback)
            {
                callback.Continue(PermissionRequestResult.Accept);
            }
            return true;
        }

        // Media Access Permission (Caméra/Micro)
        public bool OnRequestMediaAccessPermission(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string originUrl, MediaAccessPermissionType resources, IMediaAccessCallback callback)
        {
            using (callback)
            {
                callback.Continue(resources);
            }
            return true;
        }

        // Prompt System Permission (V143+)
        public bool OnShowPermissionPrompt(IWebBrowser chromiumWebBrowser, IBrowser browser, ulong promptId, string requestOriginUrl, PermissionRequestType permissions, IPermissionPromptCallback callback)
        {
            using (callback)
            {
                callback.Continue(PermissionRequestResult.Accept);
            }
            return true;
        }

        // When the prompt is closed by the  system
        public void OnDismissPermissionPrompt(IWebBrowser chromiumWebBrowser, IBrowser browser, ulong promptId, PermissionRequestResult result)
        {
            // Nothing to do here   
        }
    }

    public class CefWinWrapperBridge
    {
        private readonly IWebBrowser _browser;

        public CefWinWrapperBridge(IWebBrowser browser)
        {
            _browser = browser;
        }
        public void sendNotification(string title, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "AppIcon.png");
                    var toast = new ToastContentBuilder()
                            .AddText(title)
                            .AddText(message);

                    if (System.IO.File.Exists(imagePath))
                    {
                        toast.AddAppLogoOverride(new Uri(imagePath));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("DEBUG: Can't find Image : " + imagePath);
                    }

                    toast.Show();
                    System.Diagnostics.Debug.WriteLine("Notification send successfully !");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Notification error: " + ex.Message);
            }
            }); 
        }

        public async void requestNativeLocation()
        {
            // System.Diagnostics.Debug.WriteLine("LOCATION REQUEST");
            try
            {
                var accessStatus = await Geolocator.RequestAccessAsync();

                if (accessStatus == GeolocationAccessStatus.Allowed)
                {
                    // For a better accuracy you should use navigator.geolocation.getCurrentPosition(... handled with JS & the C# permission for chromiumWebBrowser 

                    // Lower accuracy example:
                    // Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 50 };

                    // More accurate could be better if you use navigator.geolocation.getCurrentPosition
                    Geolocator geolocator = new Geolocator
                    {
                        DesiredAccuracy = PositionAccuracy.High,
                        DesiredAccuracyInMeters = 10
                    };
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    double lat = pos.Coordinate.Point.Position.Latitude;
                    double lon = pos.Coordinate.Point.Position.Longitude;

                    string js = $"window.onNativeLocationResult({{status: 'granted', lat: {lat}, lon: {lon}}});";
                    _browser.ExecuteScriptAsync(js);
                }
                else
                {
                    _browser.ExecuteScriptAsync("window.onNativeLocationResult({status: 'denied'});");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GPS error : " + ex.Message);
                _browser.ExecuteScriptAsync("window.onNativeLocationResult({status: 'error'});");
            }
        }
    }
}


