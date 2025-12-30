<h1 align="center">CefWinWrapper</h1>

## This wrapper give access and create a bridge to your Web application through a CefSharp windows application in C#

### Bridge helpers:

- Send Windows Toast Notifications from your WEB application
- Request Native Location from your WEB application
- Get a persistent Device ID for each Windows device (defined the way to send it to your WEB application)

# Compilation architecture:

### Make sure the project will compile using the x64 architecture, otherwise CefSharp will not work properly.

# Change the Title App Name [MainWindow.xaml]:

```xml
<Window x:Class="cefWinWrapper.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:cef="clr-namespace:CefSharp.Wpf;assembly=CefSharp.Wpf"


        Title="Your App Title" <!-- Change the Title here -->


        Icon="Assets/AppIcon.ico"
        Height="800" Width="1000"
        WindowState="Maximized">
<!-- The rest of your XAML code goes here -->        
</Window>
```

# Change the Icon App

In the Assets directory, replace:
- AppIcon.ico
- AppIcon.png

# Change the browser uri [MainWindow.xaml.cs]:

```csharp
public MainWindow()
{
    InitializeComponent();
    // Change the URL to your desired web application
    browser.Address = "https://your-web-application-url.com";
}
```

# Send notifications to your Windows device using Windows Toast Notifications.

## Call the `sendNotification` function with the desired parameters from your WEB application.

### JavaScript Example:

```javascript	

const isWindowApp = navigator.userAgent.includes("SchedulumWinApp");

async function notifyWindows(title: string, msg: string) {
    if (typeof window !== "undefined" && (window as any).CefSharp && typeof (window as any).CefSharp.BindObjectAsync === "function") {
        await (window as any).CefSharp.BindObjectAsync("bridgeNotificationAsync");
        
        if ((window as any).bridgeNotificationAsync && typeof (window as any).bridgeNotificationAsync.sendNotification === "function") {
            (window as any).bridgeNotificationAsync.sendNotification(title, msg);
        }
    }
}

if (isWindowApp) {
    window.bridgeNotificationAsync.sendNotification(payload.notification.title, payload.notification.body);
}
```

### TypeScript Example:

```javascript	

const isWindowApp = navigator.userAgent.includes("SchedulumWinApp");

async function notifyWindows(title: string, msg: string) {
    if (typeof window !== "undefined" && (window as any).CefSharp && typeof (window as any).CefSharp.BindObjectAsync === "function") {
        await (window as any).CefSharp.BindObjectAsync("bridgeNotificationAsync");
        
        if ((window as any).bridgeNotificationAsync && typeof (window as any).bridgeNotificationAsync.sendNotification === "function") {
            (window as any).bridgeNotificationAsync.sendNotification(title, msg);
        }
    }
}

if (isWindowApp) {
    notifyWindows(payload.notification.title, payload.notification.body);
}
```

# Send notifications to your Windows device using Windows Toast Notifications.

## Your WEB application have to call the `requestNativeLocation` function and receive the location coordinates with this `onNativeLocationResult` function callback.

### JavaScript Example:

```javascript
let myMapInstance = null; 

const isWindowApp = navigator.userAgent.includes("SchedulumWinApp");

if (isWindowApp) {
    window.onNativeLocationResult = function(data) {
        if (data.status === 'granted') {
            const newPos = { lat: data.lat, lng: data.lon };
            
            console.log("Position reçue du C# :", newPos);

            if (myMapInstance) {
                myMapInstance.panTo(newPos);
                myMapInstance.setCenter(newPos);
            }
        } else {
            console.error("Erreur de localisation native :", data.status);
        }
    };

    if (window.bridgeNotificationAsync) {
        window.bridgeNotificationAsync.requestNativeLocation();
    }
}
```

### TypeScript (React) Example:


```javascript
const mapRef = useRef<google.maps.Map | null>(null);
// ... other code lines

declare global {
  interface Window {
    onNativeLocationResult?: (data: any) => void;
    bridgeNotificationAsync?: {
      requestNativeLocation: () => void;
    };
  }
}

const isWindowApp = navigator.userAgent.includes("SchedulumWinApp");

if (isWindowApp){
    window.onNativeLocationResult = (data) => {
        if (data.status === 'granted') {
            const newPos = { lat: data.lat, lng: data.lon };
            setCenter(newPos);
            mapRef.current?.panTo(newPos);
        }
    };
    window.bridgeNotificationAsync.requestNativeLocation();
}
```

# Remove the DeviceID border on the top of the window [MainWindow.xaml]:

```xml
<Window x:Class="cefWinWrapper.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:cef="clr-namespace:CefSharp.Wpf;assembly=CefSharp.Wpf"
        Title="Your App Title"
        Icon="Assets/AppIcon.ico"
        Height="800" Width="1000"
        WindowState="Maximized">

    <Grid Background="Black">

        <!--<Grid.RowDefinitions>
            <RowDefinition Height="40"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>-->

        <!--<Border Grid.Row="0" Background="#222">
            <TextBlock x:Name="DeviceIdTextBlock" 
                       Foreground="White" 
                       HorizontalAlignment="Center" 
                       VerticalAlignment="Center" 
                       FontSize="14" FontWeight="Bold"/>
        </Border>-->
        
        <!--<cef:ChromiumWebBrowser x:Name="browser" Grid.Row="1" />-->
        <cef:ChromiumWebBrowser x:Name="browser" />

        <!--<Grid x:Name="SplashScreenGrid" Grid.Row="1" Background="Black">-->
        <Grid x:Name="SplashScreenGrid" Background="Black">
            <StackPanel VerticalAlignment="Center">
                <Image Source="Assets/AppIcon.png" 
                   Stretch="Uniform" 
                   Width="200" 
                   Height="200"
                   HorizontalAlignment="Center">
                    <Image.Effect>
                        <DropShadowEffect BlurRadius="200" Color="White" Opacity="0.5" ShadowDepth="0"/>
                    </Image.Effect>
                </Image>
            </StackPanel>

            <ProgressBar x:Name="LoadingBar" 
                     IsIndeterminate="True" 
                     VerticalAlignment="Bottom" 
                     Height="10" 
                     Foreground="#FF007ACC"
                     Visibility="Visible"/>
        </Grid>
    </Grid>
</Window>
```

# If you don't wan't to use the device ID, you can remove the following code from [MainWindow.xaml.cs]:

```csharp
public MainWindow()
    {
        SetCurrentProcessExplicitAppUserModelID("ankh.cef_win_wrapper");
        InitializeComponent();

        // replace the browser uri with your web app url
        string origin = "https://schedulum.org";
        string deviceId = MachineIdentity.GetPersistentMachineID();
        // DeviceIdTextBlock.Text = $"DEVICE ID : {deviceId}";

        // Authorize Location & Notification
        browser.PermissionHandler = new CustomPermissionHandler();

        browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
        browser.JavascriptObjectRepository.Register("bridgeNotificationAsync", new CefWinWrapperBridge(browser));

        // You should pass the deviceId using a POST REQUEST or at least with encryption
        // browser.Address = $"{origin}?deviceId={deviceId}";
        browser.Address = origin;


    }
```


