using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using Zoomies.Core.Input;
using Zoomies.Core.ScreenCapture;
using Zoomies.Native;

namespace Zoomies.UI
{
    public partial class ScreenshotOverlay : Window
    {
        private readonly ILogger<ScreenshotOverlay> _logger;
        private readonly GlobalHookManager _hookManager;
        private readonly IScreenCapture _screenCapture;

        private double _currentZoom = 1.0;
        private const double MinZoom = 1.0;
        private const double MaxZoom = 10.0;
        private const double ZoomStep = 0.75; // Faster zoom increments

        private System.Windows.Point _imageOffset = new System.Windows.Point(0, 0);
        private bool _isActive = false;
        private bool _hooksInitialized = false;
        private bool _windowMeasured = false;
        private bool _skipNextZoom = false;
        private readonly Queue<int> _queuedZoomEvents = new Queue<int>();

        public ScreenshotOverlay(
            ILogger<ScreenshotOverlay> logger,
            GlobalHookManager hookManager,
            IScreenCapture screenCapture)
        {
            _logger = logger;
            _hookManager = hookManager;
            _screenCapture = screenCapture;

            InitializeComponent();
            SetupEventHandlers();
            InitializeServices();
        }

        private void SetupEventHandlers()
        {
            _hookManager.ZoomChanged += OnZoomChanged;
            _hookManager.HotkeysReleased += OnHotkeysReleased;
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private void InitializeServices()
        {
            try
            {
                // Initialize screen capture
                _screenCapture.Initialize();

                // Initialize hooks immediately
                _hookManager.Initialize();
                _hooksInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize services");
                ErrorDialog.Show("Initialization Error",
                    "Failed to initialize services.\n\nPlease run as administrator or check that another magnifier app isn't running.",
                    ex);
                Application.Current.Shutdown();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isActive && ActualWidth > 0 && ActualHeight > 0 && !_windowMeasured)
            {
                _windowMeasured = true;
                _logger.LogInformation($"Window measurement complete: {ActualWidth}x{ActualHeight}");
                
                // Process any queued zoom events
                ProcessQueuedZoomEvents();
            }
        }

        private void ProcessQueuedZoomEvents()
        {
            while (_queuedZoomEvents.Count > 0)
            {
                var wheelDelta = _queuedZoomEvents.Dequeue();
                _logger.LogInformation($"Processing queued zoom event: {wheelDelta}");
                ZoomAtCursor(wheelDelta);
            }
        }

        private Rectangle GetOptimizedCaptureRegion()
        {
            // Get current screen dimensions
            var screenBounds = _screenCapture.ScreenBounds;
            
            // Use smaller region for faster initial capture (25% of screen area)
            int width = Math.Min(screenBounds.Width, screenBounds.Width / 2);
            int height = Math.Min(screenBounds.Height, screenBounds.Height / 2);
            
            // Center the region or around cursor if available
            int x = (screenBounds.Width - width) / 2;
            int y = (screenBounds.Height - height) / 2;
            
            if (NativeMethods.GetCursorPos(out var cursorPos))
            {
                x = Math.Max(0, Math.Min(cursorPos.X - width / 2, screenBounds.Width - width));
                y = Math.Max(0, Math.Min(cursorPos.Y - height / 2, screenBounds.Height - height));
            }
            
            var region = new Rectangle(x, y, width, height);
            _logger.LogInformation($"Using optimized capture region: {region} (instead of full screen {screenBounds})");
            return region;
        }

        private async void OnZoomChanged(object? sender, int wheelDelta)
        {
            _logger.LogInformation($"OnZoomChanged: wheelDelta={wheelDelta}, _isActive={_isActive}, _currentZoom={_currentZoom:F2}");

            if (!_isActive)
            {
                _logger.LogInformation("First scroll - capturing screen");
                await CaptureAndShowAsync();
                return;
            }
            
            // Check if window is properly measured before zooming
            if (ActualWidth <= 0 || ActualHeight <= 0 || !_windowMeasured)
            {
                _logger.LogInformation($"Window not measured yet: {ActualWidth}x{ActualHeight}, measured: {_windowMeasured} - queueing zoom");
                _queuedZoomEvents.Enqueue(wheelDelta);
                return;
            }
            
            _logger.LogInformation("Processing zoom");
            ZoomAtCursor(wheelDelta);
        }

        private void OnHotkeysReleased(object? sender, EventArgs e)
        {
            if (_isActive)
            {
                _logger.LogInformation("OnHotkeysReleased: hiding screenshot window");
                Hide();
                _isActive = false;
                _windowMeasured = false;
                _skipNextZoom = false;
            }
        }

        private async Task CaptureAndShowAsync()
        {
            try
            {
                // Start with optimized capture region for faster initial load
                var captureRegion = GetOptimizedCaptureRegion();
                
                // Capture optimized region asynchronously (off UI thread)
                var frame = await _screenCapture.CaptureFrameAsync(captureRegion);
                if (frame == null)
                {
                    _logger.LogWarning("Failed to capture screen");
                    return;
                }

                // Convert to WPF bitmap (back on UI thread)
                var bitmap = ConvertToWriteableBitmap(frame);
                
                // Set transforms to 1.0 - defer any zoom until window is stable
                _currentZoom = 1.0;
                ZoomTransform.ScaleX = ZoomTransform.ScaleY = _currentZoom;
                PanTransform.X = PanTransform.Y = 0;
                UpdateZoomText();
                
                // Set image source after transforms are configured
                ScreenshotImage.Source = bitmap;
                
                // Show window (let WPF handle layout naturally)
                _isActive = true;
                _skipNextZoom = false;
                Visibility = Visibility.Visible;
                
                // Mark window as ready for measurement after showing
                if (ActualWidth > 0 && ActualHeight > 0)
                {
                    _windowMeasured = true;
                }
                
                var dpiScale = VisualTreeHelper.GetDpi(this);
                _logger.LogInformation($"Window shown - Image: {bitmap?.PixelWidth}x{bitmap?.PixelHeight}, Window: {ActualWidth}x{ActualHeight}, DPI: {dpiScale.DpiScaleX}x{dpiScale.DpiScaleY}");
                
                _logger.LogInformation($"CaptureAndShow: Starting at {_currentZoom:F2}, async approach (v2.13-Async)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing screenshot");
            }
        }

        private void ZoomAtCursor(int wheelDelta)
        {
            // Early exit for zoom-out when already at EXACTLY minimum zoom  
            if (wheelDelta < 0 && _currentZoom == MinZoom)
            {
                _logger.LogInformation("Early exit: already at minimum zoom 1.0");
                return;
            }

            _logger.LogInformation($"ZoomAtCursor START: wheelDelta={wheelDelta}, _currentZoom={_currentZoom:F2}");

            // Get cursor position
            if (!NativeMethods.GetCursorPos(out var cursorPos))
            {
                _logger.LogWarning("Failed to get cursor position");
                return;
            }

            // Convert to window coordinates
            var cursorPoint = PointFromScreen(new System.Windows.Point(cursorPos.X, cursorPos.Y));

            // Calculate zoom change with consistent additive steps
            double newZoom;

            if (wheelDelta > 0)
            {
                // Zooming in
                newZoom = _currentZoom + ZoomStep;
                newZoom = Math.Min(MaxZoom, newZoom);
                if (newZoom != _currentZoom)
                {
                    _logger.LogInformation($"Zooming IN: {_currentZoom:F2} -> {newZoom:F2}");
                }
            }
            else
            {
                // Zooming out - always snap to 1.0 when close
                if (_currentZoom <= 1.0 + ZoomStep)
                {
                    newZoom = 1.0; // Force exact 1.0 when zooming out from close
                    _logger.LogInformation($"Zooming OUT (snap to 1.0): {_currentZoom:F2} -> {newZoom:F2}");
                }
                else
                {
                    newZoom = _currentZoom - ZoomStep;
                    newZoom = Math.Max(MinZoom, newZoom);
                    _logger.LogInformation($"Zooming OUT: {_currentZoom:F2} -> {newZoom:F2}");
                }
            }

            // Skip if no meaningful change
            if (Math.Abs(newZoom - _currentZoom) < 0.001)
            {
                return;
            }

            // Get the image element and its current transform
            var image = ScreenshotImage;
            if (image?.Source == null)
            {
                _logger.LogWarning("No image source available");
                return;
            }

            // Since image fills the window, work with window coordinates
            var windowWidth = ActualWidth;
            var windowHeight = ActualHeight;
            
            // Calculate cursor position within the image (accounting for pan and zoom)
            var imageCursorX = (cursorPoint.X - PanTransform.X) / _currentZoom;
            var imageCursorY = (cursorPoint.Y - PanTransform.Y) / _currentZoom;
            
            // Clamp cursor to window bounds
            imageCursorX = Math.Max(0, Math.Min(windowWidth, imageCursorX));
            imageCursorY = Math.Max(0, Math.Min(windowHeight, imageCursorY));

            // The image point in actual pixel coordinates
            var imagePointX = imageCursorX;
            var imagePointY = imageCursorY;

            // Update zoom
            _currentZoom = newZoom;
            ZoomTransform.ScaleX = ZoomTransform.ScaleY = _currentZoom;

            // Calculate new pan to keep the same image point under cursor
            var newPanX = cursorPoint.X - (imagePointX * _currentZoom);
            var newPanY = cursorPoint.Y - (imagePointY * _currentZoom);

            // Apply bounds checking for panning
            if (_currentZoom > 1.0)
            {
                // For zoom > 1.0, allow panning but keep some image visible
                var maxPanX = windowWidth * (_currentZoom - 1.0) * 0.9; // Allow 90% pan
                var minPanX = -maxPanX;
                var maxPanY = windowHeight * (_currentZoom - 1.0) * 0.9; // Allow 90% pan  
                var minPanY = -maxPanY;

                newPanX = Math.Max(minPanX, Math.Min(maxPanX, newPanX));
                newPanY = Math.Max(minPanY, Math.Min(maxPanY, newPanY));
            }
            else
            {
                // For zoom = 1.0, no panning
                newPanX = 0;
                newPanY = 0;
            }

            // Apply the new pan transform
            PanTransform.X = newPanX;
            PanTransform.Y = newPanY;
            _logger.LogInformation($"Applied zoom {_currentZoom:F2}, pan X={newPanX:F2}, Y={newPanY:F2}");

            UpdateZoomText();
        }

        private void UpdateZoomText()
        {
            ZoomText.Text = $"{_currentZoom * 100:F1}% (x{_currentZoom:F2})";
        }

        private WriteableBitmap ConvertToWriteableBitmap(Frame frame)
        {
            var bitmap = new WriteableBitmap(
                frame.Width,
                frame.Height,
                96, 96,
                PixelFormats.Bgra32,
                null);

            bitmap.Lock();
            try
            {
                unsafe
                {
                    fixed (byte* sourcePtr = frame.Data)
                    {
                        Buffer.MemoryCopy(
                            sourcePtr,
                            bitmap.BackBuffer.ToPointer(),
                            frame.Data.Length,
                            frame.Data.Length);
                    }
                }

                bitmap.AddDirtyRect(new Int32Rect(0, 0, frame.Width, frame.Height));
            }
            finally
            {
                bitmap.Unlock();
            }

            return bitmap;
        }

        protected override void OnClosed(EventArgs e)
        {
            _hookManager.Dispose();
            base.OnClosed(e);
        }
    }
}