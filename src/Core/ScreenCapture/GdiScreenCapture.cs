using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Zoomies.Native;

namespace Zoomies.Core.ScreenCapture
{
    public class GdiScreenCapture : IScreenCapture
    {
        private bool _isInitialized;
        private Rectangle _screenBounds;

        public Rectangle ScreenBounds => _screenBounds;
        public bool IsReady => _isInitialized;

        public GdiScreenCapture()
        {
        }

        public void Initialize()
        {
            try
            {
                int screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
                int screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
                _screenBounds = new Rectangle(0, 0, screenWidth, screenHeight);

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Frame? CaptureFrame(Rectangle region)
        {
            if (!_isInitialized)
            {
                return null;
            }

            IntPtr desktopDc = IntPtr.Zero;
            IntPtr memoryDc = IntPtr.Zero;
            IntPtr bitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                // Get desktop device context
                desktopDc = NativeMethods.GetWindowDC(NativeMethods.GetDesktopWindow());
                memoryDc = NativeMethods.CreateCompatibleDC(desktopDc);

                // Create bitmap
                bitmap = NativeMethods.CreateCompatibleBitmap(desktopDc, region.Width, region.Height);
                oldBitmap = NativeMethods.SelectObject(memoryDc, bitmap);

                // Copy screen region to bitmap
                bool success = NativeMethods.BitBlt(memoryDc, 0, 0, region.Width, region.Height,
                    desktopDc, region.X, region.Y, Native.CopyPixelOperation.SourceCopy);

                if (!success)
                {
                    return null;
                }

                // Get bitmap data
                var bitmapInfo = new NativeMethods.BITMAPINFO();
                bitmapInfo.bmiHeader = new NativeMethods.BITMAPINFOHEADER
                {
                    biSize = (uint)Marshal.SizeOf(typeof(NativeMethods.BITMAPINFOHEADER)),
                    biWidth = region.Width,
                    biHeight = -region.Height, // Negative for top-down bitmap
                    biPlanes = 1,
                    biBitCount = 32,
                    biCompression = NativeMethods.BI_RGB
                };

                int stride = ((region.Width * 32 + 31) / 32) * 4;
                byte[] pixels = new byte[stride * region.Height];

                int result = NativeMethods.GetDIBits(memoryDc, bitmap, 0, (uint)region.Height,
                    pixels, ref bitmapInfo, NativeMethods.DIB_RGB_COLORS);

                if (result == 0)
                {
                    return null;
                }

                return new Frame(pixels, region.Width, region.Height, stride, region);
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                // Cleanup
                if (oldBitmap != IntPtr.Zero && memoryDc != IntPtr.Zero)
                    NativeMethods.SelectObject(memoryDc, oldBitmap);
                if (bitmap != IntPtr.Zero)
                    NativeMethods.DeleteObject(bitmap);
                if (memoryDc != IntPtr.Zero)
                    NativeMethods.DeleteDC(memoryDc);
                if (desktopDc != IntPtr.Zero)
                    NativeMethods.ReleaseDC(NativeMethods.GetDesktopWindow(), desktopDc);
            }
        }

        public Frame? CaptureAroundCursor(Size size)
        {
            if (!NativeMethods.GetCursorPos(out var cursorPos))
            {
                return null;
            }

            // Calculate region centered on cursor
            int halfWidth = size.Width / 2;
            int halfHeight = size.Height / 2;

            var region = new Rectangle(
                cursorPos.X - halfWidth,
                cursorPos.Y - halfHeight,
                size.Width,
                size.Height
            );

            // Clamp to screen bounds
            if (region.X < 0) region.X = 0;
            if (region.Y < 0) region.Y = 0;
            if (region.Right > _screenBounds.Width)
                region.X = _screenBounds.Width - region.Width;
            if (region.Bottom > _screenBounds.Height)
                region.Y = _screenBounds.Height - region.Height;

            return CaptureFrame(region);
        }

        public async Task<Frame?> CaptureFrameAsync(Rectangle region)
        {
            return await Task.Run(() => CaptureFrame(region));
        }

        public async Task<Frame?> CaptureAroundCursorAsync(Size size)
        {
            return await Task.Run(() => CaptureAroundCursor(size));
        }

        public void Dispose()
        {
            _isInitialized = false;
        }
    }
}