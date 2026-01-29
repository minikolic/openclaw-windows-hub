using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using Moltbot.Shared;
using MoltbotTray.Helpers;
using MoltbotTray.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WinUIEx;

namespace MoltbotTray.Windows;

public sealed partial class WebChatWindow : WindowEx
{
    private readonly string _gatewayUrl;
    private readonly string _token;
    private bool _initialized;
    
    public bool IsClosed { get; private set; }

    public WebChatWindow(string gatewayUrl, string token)
    {
        _gatewayUrl = gatewayUrl;
        _token = token;
        
        InitializeComponent();
        
        // Window configuration
        this.SetWindowSize(520, 750);
        this.MinWidth = 380;
        this.MinHeight = 450;
        this.CenterOnScreen();
        this.SetIcon(IconHelper.GetStatusIconPath(ConnectionStatus.Connected));
        
        Closed += (s, e) => IsClosed = true;
        
        _ = InitializeWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            // Set up user data folder via environment variable (WinUI 3 workaround)
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MoltbotTray", "WebView2");
            
            Directory.CreateDirectory(userDataFolder);
            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", userDataFolder);

            await WebView.EnsureCoreWebView2Async();
            
            // Configure WebView2
            WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            WebView.CoreWebView2.Settings.IsZoomControlEnabled = true;

            // Handle navigation events
            WebView.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
            };

            WebView.CoreWebView2.NavigationStarting += (s, e) =>
            {
                LoadingRing.IsActive = true;
                LoadingRing.Visibility = Visibility.Visible;
            };

            // Navigate to chat
            NavigateToChat();
            _initialized = true;
        }
        catch (Exception ex)
        {
            Logger.Error($"WebView2 initialization failed: {ex.Message}");
            LoadingRing.IsActive = false;
        }
    }

    private void NavigateToChat()
    {
        if (WebView.CoreWebView2 == null) return;

        var baseUrl = _gatewayUrl
            .Replace("ws://", "http://")
            .Replace("wss://", "https://");
        
        var url = $"{baseUrl}?token={Uri.EscapeDataString(_token)}";
        WebView.CoreWebView2.Navigate(url);
    }

    private void OnHome(object sender, RoutedEventArgs e)
    {
        NavigateToChat();
    }

    private void OnRefresh(object sender, RoutedEventArgs e)
    {
        WebView.CoreWebView2?.Reload();
    }

    private void OnPopout(object sender, RoutedEventArgs e)
    {
        var baseUrl = _gatewayUrl
            .Replace("ws://", "http://")
            .Replace("wss://", "https://");
        var url = $"{baseUrl}?token={Uri.EscapeDataString(_token)}";
        
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to open in browser: {ex.Message}");
        }
    }

    private void OnDevTools(object sender, RoutedEventArgs e)
    {
        WebView.CoreWebView2?.OpenDevToolsWindow();
    }
}
