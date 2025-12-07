using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Interop;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfCursors = System.Windows.Input.Cursors;

namespace SnapMate.Views;

public partial class RegionSelectorWindow : Window
{
    private System.Windows.Point _startPoint;
    private Rectangle? _selectionRectangle;
    private bool _isSelecting;
    private readonly int _virtualScreenLeft;
    private readonly int _virtualScreenTop;
    private readonly int _virtualScreenWidth;
    private readonly int _virtualScreenHeight;

    /// <summary>
    /// Gets the selected region in absolute screen coordinates (compatible with GDI+ CopyFromScreen).
    /// </summary>
    public Int32Rect SelectedRegion { get; private set; }

    /// <summary>
    /// Gets whether a valid region was selected.
    /// </summary>
    public bool HasSelection { get; private set; }

    public RegionSelectorWindow()
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            minX = Math.Min(minX, screen.Bounds.Left);
            minY = Math.Min(minY, screen.Bounds.Top);
            maxX = Math.Max(maxX, screen.Bounds.Right);
            maxY = Math.Max(maxY, screen.Bounds.Bottom);
        }

        _virtualScreenLeft = minX;
        _virtualScreenTop = minY;
        _virtualScreenWidth = maxX - minX;
        _virtualScreenHeight = maxY - minY;

        InitializeComponent();
        SetupFullScreenOverlay();

        Focusable = true;
        Loaded += OnWindowLoaded;
        Activated += (_, __) => { Keyboard.Focus(this); };

        if (SelectionCanvas != null)
        {
            SelectionCanvas.Focusable = false;
        }

        ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessage;
        Closed += OnWindowClosed;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Activate();
        Focus();
        Keyboard.Focus(this);
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessage;
        Loaded -= OnWindowLoaded;
        Closed -= OnWindowClosed;
    }

    private void SetupFullScreenOverlay()
    {
        Left = _virtualScreenLeft;
        Top = _virtualScreenTop;
        Width = _virtualScreenWidth;
        Height = _virtualScreenHeight;
        WindowState = WindowState.Normal;
        Cursor = WpfCursors.Cross;
    }

    private void ThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;
        const int VK_ESCAPE = 0x1B;

        if ((msg.message == WM_KEYDOWN || msg.message == WM_SYSKEYDOWN) && (int)msg.wParam == VK_ESCAPE)
        {
            handled = true;
            CancelSelection();
        }
    }

    private void CancelSelection()
    {
        HasSelection = false;
        DialogResult = false;

        if (_isSelecting)
        {
            _isSelecting = false;
            ReleaseMouseCapture();
        }

        Close();
    }

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        _startPoint = e.GetPosition(this);
        _isSelecting = true;

        _selectionRectangle = new Rectangle
        {
            Stroke = Brushes.Cyan,
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(30, 0, 255, 255))
        };

        SelectionCanvas.Children.Add(_selectionRectangle);
        CaptureMouse();
        Keyboard.Focus(this);
    }

    protected override void OnMouseMove(WpfMouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isSelecting && _selectionRectangle != null)
        {
            System.Windows.Point currentPoint = e.GetPosition(this);

            double x = Math.Min(_startPoint.X, currentPoint.X);
            double y = Math.Min(_startPoint.Y, currentPoint.Y);
            double width = Math.Abs(_startPoint.X - currentPoint.X);
            double height = Math.Abs(_startPoint.Y - currentPoint.Y);

            Canvas.SetLeft(_selectionRectangle, x);
            Canvas.SetTop(_selectionRectangle, y);
            _selectionRectangle.Width = width;
            _selectionRectangle.Height = height;
        }
    }

    protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (_isSelecting && _selectionRectangle != null)
        {
            ReleaseMouseCapture();
            _isSelecting = false;

            double rectX = Canvas.GetLeft(_selectionRectangle);
            double rectY = Canvas.GetTop(_selectionRectangle);
            double rectWidth = _selectionRectangle.Width;
            double rectHeight = _selectionRectangle.Height;

            var topLeftInWindow = SelectionCanvas.TranslatePoint(new System.Windows.Point(rectX, rectY), this);
            var bottomRightInWindow = SelectionCanvas.TranslatePoint(new System.Windows.Point(rectX + rectWidth, rectY + rectHeight), this);

            var topLeftScreenDips = PointToScreen(topLeftInWindow);
            var bottomRightScreenDips = PointToScreen(bottomRightInWindow);

            var source = PresentationSource.FromVisual(this);
            var toDevice = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
            var topLeftScreenPx = toDevice.Transform(topLeftScreenDips);
            var bottomRightScreenPx = toDevice.Transform(bottomRightScreenDips);

            int screenX = (int)Math.Round(topLeftScreenPx.X);
            int screenY = (int)Math.Round(topLeftScreenPx.Y);
            int screenWidth = (int)Math.Round(bottomRightScreenPx.X - topLeftScreenPx.X);
            int screenHeight = (int)Math.Round(bottomRightScreenPx.Y - topLeftScreenPx.Y);

            if (screenWidth >= 10 && screenHeight >= 10)
            {
                SelectedRegion = new Int32Rect(screenX, screenY, screenWidth, screenHeight);
                HasSelection = true;
                DialogResult = true;
            }
            else
            {
                HasSelection = false;
                DialogResult = false;
            }

            Close();
        }
    }

    protected override void OnPreviewKeyDown(WpfKeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            CancelSelection();
        }
    }

    protected override void OnKeyDown(WpfKeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == System.Windows.Input.Key.Escape)
        {
            e.Handled = true;
            CancelSelection();
        }
    }
}
