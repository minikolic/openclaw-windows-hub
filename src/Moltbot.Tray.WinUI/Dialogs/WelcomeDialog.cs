using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MoltbotTray.Dialogs;

/// <summary>
/// First-run welcome dialog for new users.
/// </summary>
public sealed class WelcomeDialog
{
    private ContentDialog? _dialog;
    private ContentDialogResult _result;

    public async Task<ContentDialogResult> ShowAsync()
    {
        // Create a temporary window to host the dialog
        var window = new Window();
        window.Content = new Grid();
        window.Activate();

        // Build dialog content
        var content = new StackPanel
        {
            Spacing = 16,
            MaxWidth = 400
        };

        // Lobster header
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        headerPanel.Children.Add(new TextBlock
        {
            Text = "🦞",
            FontSize = 48
        });
        headerPanel.Children.Add(new TextBlock
        {
            Text = "Welcome to Moltbot!",
            Style = (Style)Application.Current.Resources["TitleTextBlockStyle"],
            VerticalAlignment = VerticalAlignment.Center
        });
        content.Children.Add(headerPanel);

        // Description
        content.Children.Add(new TextBlock
        {
            Text = "Moltbot Tray is your Windows companion for Moltbot, the AI-powered personal assistant.",
            TextWrapping = TextWrapping.Wrap
        });

        // Getting started
        var gettingStarted = new StackPanel { Spacing = 8 };
        gettingStarted.Children.Add(new TextBlock
        {
            Text = "To get started, you'll need:",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });

        var bulletList = new StackPanel { Spacing = 4, Margin = new Thickness(16, 0, 0, 0) };
        bulletList.Children.Add(new TextBlock { Text = "• A running Moltbot gateway" });
        bulletList.Children.Add(new TextBlock { Text = "• Your API token from the dashboard" });
        gettingStarted.Children.Add(bulletList);
        content.Children.Add(gettingStarted);

        // Documentation link
        var docsButton = new HyperlinkButton
        {
            Content = "📚 View Documentation",
            NavigateUri = new Uri("https://docs.molt.bot/web/dashboard")
        };
        content.Children.Add(docsButton);

        // Create and show dialog
        _dialog = new ContentDialog
        {
            Title = "Welcome",
            Content = content,
            PrimaryButtonText = "Open Settings",
            CloseButtonText = "Later",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = window.Content.XamlRoot
        };

        _result = await _dialog.ShowAsync();
        window.Close();
        
        return _result;
    }
}
