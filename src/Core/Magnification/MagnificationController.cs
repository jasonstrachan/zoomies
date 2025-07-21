using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Zoomies.Core.ScreenCapture;

namespace Zoomies.Core.Magnification
{
    public class MagnificationController
    {
        private readonly IScreenCapture _screenCapture;

        private double _zoomLevel = 2.0;
        private readonly double _minZoom = 1.5;
        private readonly double _maxZoom = 10.0;
        private readonly double _zoomStep = 1.5;

        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                _zoomLevel = Math.Max(_minZoom, Math.Min(_maxZoom, value));
                ZoomLevelChanged?.Invoke(this, _zoomLevel);
            }
        }

        public event EventHandler<double>? ZoomLevelChanged;
        public event EventHandler<WriteableBitmap?>? FrameReady;

        public MagnificationController(IScreenCapture screenCapture)
        {
            _screenCapture = screenCapture;
        }

        public void Initialize()
        {
            _screenCapture.Initialize();
            // Magnification controller initialized
        }

        public void AdjustZoom(int wheelDelta)
        {
            if (wheelDelta > 0)
            {
                ZoomLevel += _zoomStep;
            }
            else if (wheelDelta < 0)
            {
                ZoomLevel -= _zoomStep;
            }

            // Zoom level adjusted
        }

        public void CaptureAndMagnify()
        {
            try
            {
                // Calculate capture size based on zoom level
                int captureSize = (int)(200 / _zoomLevel);
                var frame = _screenCapture.CaptureAroundCursor(new System.Drawing.Size(captureSize, captureSize));

                if (frame == null)
                {
                    FrameReady?.Invoke(this, null);
                    return;
                }

                // Convert to WPF bitmap
                var bitmap = ConvertToWriteableBitmap(frame);
                FrameReady?.Invoke(this, bitmap);
            }
            catch (Exception ex)
            {
                // Error capturing and magnifying
                FrameReady?.Invoke(this, null);
            }
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
    }
}