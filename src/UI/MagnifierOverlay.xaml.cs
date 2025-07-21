using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Zoomies.Core.Input;
using Zoomies.Core.Magnification;
using Zoomies.Native;

namespace Zoomies.UI
{
    public partial class MagnifierOverlay : Window
    {
        private readonly GlobalHookManager _hookManager;
        private readonly MagnificationController _magnificationController;
        private readonly DispatcherTimer _captureTimer;

        private bool _isActive;

        public MagnifierOverlay(
            GlobalHookManager hookManager,
            MagnificationController magnificationController)
        {
            _hookManager = hookManager;
            _magnificationController = magnificationController;

            InitializeComponent();

            // Remove timer - we'll capture on demand instead
            _captureTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Only update position
            };
            _captureTimer.Tick += OnUpdatePosition;

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _hookManager.ZoomChanged += OnZoomChanged;
            _hookManager.HotkeysReleased += OnHotkeysReleased;
            _magnificationController.FrameReady += OnFrameReady;
            _magnificationController.ZoomLevelChanged += OnZoomLevelChanged;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _hookManager.Initialize();
                _magnificationController.Initialize();
                // Magnifier overlay initialized
            }
            catch (Exception ex)
            {
                // Failed to initialize magnifier
                MessageBox.Show(
                    "Failed to initialize magnifier. Please run as administrator.",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void OnZoomChanged(object? sender, int wheelDelta)
        {
            if (!_isActive && wheelDelta != 0)
            {
                // First scroll activates magnifier
                ShowMagnifier();
            }
            else if (_isActive)
            {
                _magnificationController.AdjustZoom(wheelDelta);
            }
        }

        private void OnHotkeysReleased(object? sender, EventArgs e)
        {
            if (_isActive)
            {
                HideMagnifier();
            }
        }

        private void OnZoomLevelChanged(object? sender, double zoomLevel)
        {
            // Update zoom text
            ZoomText.Text = $"{zoomLevel:F1}x";

            // Optionally recapture with new zoom
            if (_isActive)
            {
                _magnificationController.CaptureAndMagnify();
            }

            // Zoom level changed
        }

        private void OnFrameReady(object? sender, WriteableBitmap? bitmap)
        {
            if (bitmap != null)
            {
                MagnifiedImage.Source = bitmap;
            }
        }

        private void OnUpdatePosition(object? sender, EventArgs e)
        {
            try
            {
                UpdatePosition();
            }
            catch (Exception ex)
            {
                // Error updating position
            }
        }

        private void UpdatePosition()
        {
            if (!NativeMethods.GetCursorPos(out var cursorPos))
                return;

            // Position window next to cursor
            Left = cursorPos.X + 20;
            Top = cursorPos.Y - (Height / 2);

            // Keep window on screen
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            if (Left + Width > screenWidth)
                Left = cursorPos.X - Width - 20;

            if (Top < 0)
                Top = 0;
            else if (Top + Height > screenHeight)
                Top = screenHeight - Height;
        }

        private void ShowMagnifier()
        {
            _isActive = true;

            // Capture once when showing
            _magnificationController.CaptureAndMagnify();

            Visibility = Visibility.Visible;
            _captureTimer.Start();
            // Magnifier activated
        }

        public void HideMagnifier()
        {
            _isActive = false;
            Visibility = Visibility.Hidden;
            _captureTimer.Stop();
            // Magnifier deactivated
        }

        protected override void OnClosed(EventArgs e)
        {
            _captureTimer.Stop();
            _hookManager.Dispose();
            base.OnClosed(e);
        }
    }
}