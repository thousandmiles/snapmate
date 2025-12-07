using System.Runtime.InteropServices;
using System.Windows.Input;

namespace SnapMate.Services;

/// <summary>
/// Provides global hotkey registration and management for system-wide keyboard shortcuts.
/// </summary>
public interface IHotkeyService
{
    /// <summary>
    /// Registers a global hotkey with the system.
    /// </summary>
    /// <param name="id">Unique identifier for the hotkey.</param>
    /// <param name="modifiers">Modifier keys (Ctrl, Alt, Shift).</param>
    /// <param name="key">The main key.</param>
    /// <param name="callback">Action to execute when hotkey is pressed.</param>
    /// <returns>True if registration succeeded, false otherwise.</returns>
    bool RegisterHotkey(int id, ModifierKeys modifiers, Key key, Action callback);

    /// <summary>
    /// Unregisters a previously registered hotkey.
    /// </summary>
    /// <param name="id">The hotkey ID to unregister.</param>
    /// <returns>True if unregistration succeeded, false otherwise.</returns>
    bool UnregisterHotkey(int id);

    /// <summary>
    /// Unregisters all hotkeys registered by this service.
    /// </summary>
    void UnregisterAll();
}

/// <summary>
/// Implementation of hotkey service using Win32 RegisterHotKey API.
/// </summary>
public class HotkeyService : IHotkeyService
{
    private readonly Dictionary<int, Action> _callbacks = new();
    private IntPtr _windowHandle;

    /// <summary>
    /// Initializes the hotkey service with a window handle for message processing.
    /// </summary>
    /// <param name="windowHandle">Handle of the window that will receive hotkey messages.</param>
    public void Initialize(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }

    /// <inheritdoc />
    public bool RegisterHotkey(int id, ModifierKeys modifiers, Key key, Action callback)
    {
        if (_windowHandle == IntPtr.Zero)
            return false;

        uint modFlag = 0;
        if (modifiers.HasFlag(ModifierKeys.Control))
            modFlag |= 0x0002; // MOD_CONTROL
        if (modifiers.HasFlag(ModifierKeys.Shift))
            modFlag |= 0x0004; // MOD_SHIFT
        if (modifiers.HasFlag(ModifierKeys.Alt))
            modFlag |= 0x0001; // MOD_ALT

        var vk = KeyInterop.VirtualKeyFromKey(key);

        if (NativeMethods.RegisterHotKey(_windowHandle, id, modFlag, (uint)vk))
        {
            _callbacks[id] = callback;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool UnregisterHotkey(int id)
    {
        if (_windowHandle == IntPtr.Zero)
            return false;

        _callbacks.Remove(id);
        return NativeMethods.UnregisterHotKey(_windowHandle, id);
    }

    /// <inheritdoc />
    public void UnregisterAll()
    {
        foreach (var id in _callbacks.Keys.ToList())
        {
            UnregisterHotkey(id);
        }
    }

    /// <summary>
    /// Processes a hotkey message from the window procedure.
    /// </summary>
    /// <param name="id">The hotkey ID that was triggered.</param>
    public void ProcessHotkey(int id)
    {
        if (_callbacks.TryGetValue(id, out var callback))
        {
            callback?.Invoke();
        }
    }

    /// <summary>
    /// P/Invoke declarations for hotkey Win32 API.
    /// </summary>
    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
