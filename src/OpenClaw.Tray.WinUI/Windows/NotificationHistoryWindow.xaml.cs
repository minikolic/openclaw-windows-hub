using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OpenClaw.Shared;
using OpenClawTray.Helpers;
using OpenClawTray.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WinUIEx;

namespace OpenClawTray.Windows;

public sealed partial class NotificationHistoryWindow : WindowEx
{
    public bool IsClosed { get; private set; }

    public NotificationHistoryWindow()
    {
        InitializeComponent();
        
        // Window configuration
        this.SetWindowSize(450, 600);
        this.CenterOnScreen();
        this.SetIcon(IconHelper.GetStatusIconPath(ConnectionStatus.Connected));
        
        Closed += (s, e) => IsClosed = true;
        
        LoadNotifications();
    }

    private void LoadNotifications()
    {
        var history = NotificationHistoryService.GetHistory();
        
        if (history.Count == 0)
        {
            NotificationList.Visibility = Visibility.Collapsed;
            EmptyState.Visibility = Visibility.Visible;
            CountText.Text = "(0)";
            return;
        }

        NotificationList.Visibility = Visibility.Visible;
        EmptyState.Visibility = Visibility.Collapsed;
        CountText.Text = $"({history.Count})";

        NotificationList.ItemsSource = history.Select(n => new NotificationViewModel
        {
            Title = n.Title,
            Message = n.Message,
            Category = n.Category ?? "",
            TimeAgo = GetTimeAgo(n.Timestamp),
            ActionUrl = n.ActionUrl,
            CategoryVisibility = string.IsNullOrEmpty(n.Category) ? Visibility.Collapsed : Visibility.Visible,
            LinkVisibility = string.IsNullOrEmpty(n.ActionUrl) ? Visibility.Collapsed : Visibility.Visible
        }).ToList();
    }

    private static string GetTimeAgo(DateTime timestamp)
    {
        var diff = DateTime.Now - timestamp;
        
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        return timestamp.ToString("MMM d, HH:mm");
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is NotificationViewModel vm && !string.IsNullOrEmpty(vm.ActionUrl))
        {
            try
            {
                Process.Start(new ProcessStartInfo(vm.ActionUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to open URL: {ex.Message}");
            }
        }
    }

    private void OnClearAll(object sender, RoutedEventArgs e)
    {
        NotificationHistoryService.Clear();
        LoadNotifications();
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private class NotificationViewModel
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Category { get; set; } = "";
        public string TimeAgo { get; set; } = "";
        public string? ActionUrl { get; set; }
        public Visibility CategoryVisibility { get; set; }
        public Visibility LinkVisibility { get; set; }
    }
}
