using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace SnapMate.Views;

/// <summary>
/// Window selector dialog for choosing which window to capture.
/// </summary>
public partial class WindowSelectorWindow : Window
{
    /// <summary>
    /// Gets the selected window handle, or IntPtr.Zero if no selection was made.
    /// </summary>
    public IntPtr SelectedWindowHandle { get; private set; }

    /// <summary>
    /// Gets whether a window was selected.
    /// </summary>
    public bool HasSelection { get; private set; }

    public WindowSelectorWindow()
    {
        InitializeComponent();
        LoadWindows();
    }

    /// <summary>
    /// Loads all visible windows into the list.
    /// </summary>
    private void LoadWindows()
    {
        var windows = new List<WindowInfo>();

        // Enumerate all windows
        EnumWindows((hWnd, lParam) =>
        {
            // Only include visible windows with titles
            if (IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) > 0)
            {
                var title = GetWindowTitle(hWnd);

                // Filter out empty titles and the current window
                if (!string.IsNullOrWhiteSpace(title) && hWnd != new System.Windows.Interop.WindowInteropHelper(this).Handle)
                {
                    var processName = GetProcessName(hWnd);
                    windows.Add(new WindowInfo
                    {
                        Handle = hWnd,
                        Title = title,
                        ProcessName = processName,
                        DisplayName = string.IsNullOrEmpty(processName) ? title : $"{processName} - {title}"
                    });
                }
            }
            return true; // Continue enumeration
        }, IntPtr.Zero);

        // Sort by process name, then by title
        windows.Sort((a, b) =>
        {
            var processCompare = string.Compare(a.ProcessName, b.ProcessName, StringComparison.OrdinalIgnoreCase);
            return processCompare != 0 ? processCompare : string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
        });

        WindowListView.ItemsSource = windows;
    }

    /// <summary>
    /// Gets the title of a window.
    /// </summary>
    private static string GetWindowTitle(IntPtr hWnd)
    {
        int length = GetWindowTextLength(hWnd);
        if (length == 0)
            return string.Empty;

        var builder = new StringBuilder(length + 1);
        GetWindowText(hWnd, builder, builder.Capacity);
        return builder.ToString();
    }

    /// <summary>
    /// Gets the process name for a window.
    /// </summary>
    private static string GetProcessName(IntPtr hWnd)
    {
        try
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            if (processId != 0)
            {
                var process = Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
        }
        catch
        {
            // Ignore errors (e.g., access denied)
        }
        return string.Empty;
    }

    private void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowListView.SelectedItem is WindowInfo selectedWindow)
        {
            SelectedWindowHandle = selectedWindow.Handle;
            HasSelection = true;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Please select a window to capture.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        HasSelection = false;
        DialogResult = false;
        Close();
    }

    private void WindowListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (WindowListView.SelectedItem is WindowInfo selectedWindow)
        {
            SelectedWindowHandle = selectedWindow.Handle;
            HasSelection = true;
            DialogResult = true;
            Close();
        }
    }

    #region Win32 API

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    #endregion

    /// <summary>
    /// Represents a window in the system.
    /// </summary>
    private class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
