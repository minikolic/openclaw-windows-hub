using System;
using System.Runtime.InteropServices;

namespace MoltbotTray.Services;

/// <summary>
/// Registers and handles global hotkeys using P/Invoke.
/// Default: Ctrl+Alt+Shift+C for Quick Send.
/// </summary>
public class GlobalHotkeyService : IDisposable
{
    private const int HOTKEY_ID = 1;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_C = 0x43;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private IntPtr _hwnd;
    private bool _registered;

    public event EventHandler? HotkeyPressed;

    public GlobalHotkeyService()
    {
        // Create a message-only window for hotkey messages
        _hwnd = CreateMessageWindow();
    }

    public bool Register()
    {
        if (_registered) return true;

        try
        {
            _registered = RegisterHotKey(_hwnd, HOTKEY_ID, MOD_CONTROL | MOD_ALT | MOD_SHIFT, VK_C);
            if (_registered)
            {
                Logger.Info("Global hotkey registered: Ctrl+Alt+Shift+C");
            }
            else
            {
                Logger.Warn("Failed to register global hotkey");
            }
            return _registered;
        }
        catch (Exception ex)
        {
            Logger.Error($"Hotkey registration error: {ex.Message}");
            return false;
        }
    }

    public void Unregister()
    {
        if (!_registered) return;

        try
        {
            UnregisterHotKey(_hwnd, HOTKEY_ID);
            _registered = false;
            Logger.Info("Global hotkey unregistered");
        }
        catch (Exception ex)
        {
            Logger.Warn($"Hotkey unregistration error: {ex.Message}");
        }
    }

    internal void OnHotkeyPressed()
    {
        HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    private IntPtr CreateMessageWindow()
    {
        // Use a simple approach - we'll hook into the message loop differently in WinUI
        // For now, return IntPtr.Zero and use a different mechanism
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
    }
}
