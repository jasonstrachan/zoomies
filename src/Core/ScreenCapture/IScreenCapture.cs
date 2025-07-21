using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Zoomies.Core.ScreenCapture
{
    public interface IScreenCapture : IDisposable
    {
        /// <summary>
        /// Captures a frame from the specified region of the screen
        /// </summary>
        /// <param name="region">The region to capture</param>
        /// <returns>Captured frame data or null if capture failed</returns>
        Frame? CaptureFrame(Rectangle region);

        /// <summary>
        /// Captures a frame around the cursor position
        /// </summary>
        /// <param name="size">Size of the capture area</param>
        /// <returns>Captured frame data or null if capture failed</returns>
        Frame? CaptureAroundCursor(Size size);

        /// <summary>
        /// Captures a frame from the specified region of the screen asynchronously
        /// </summary>
        /// <param name="region">The region to capture</param>
        /// <returns>Captured frame data or null if capture failed</returns>
        Task<Frame?> CaptureFrameAsync(Rectangle region);

        /// <summary>
        /// Captures a frame around the cursor position asynchronously
        /// </summary>
        /// <param name="size">Size of the capture area</param>
        /// <returns>Captured frame data or null if capture failed</returns>
        Task<Frame?> CaptureAroundCursorAsync(Size size);

        /// <summary>
        /// Gets the primary screen bounds
        /// </summary>
        Rectangle ScreenBounds { get; }

        /// <summary>
        /// Gets whether the capture system is initialized and ready
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Initializes the screen capture system
        /// </summary>
        void Initialize();
    }

    public class Frame
    {
        public byte[] Data { get; }
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public Rectangle CaptureRegion { get; }
        public DateTime Timestamp { get; }

        public Frame(byte[] data, int width, int height, int stride, Rectangle region)
        {
            Data = data;
            Width = width;
            Height = height;
            Stride = stride;
            CaptureRegion = region;
            Timestamp = DateTime.UtcNow;
        }
    }
}