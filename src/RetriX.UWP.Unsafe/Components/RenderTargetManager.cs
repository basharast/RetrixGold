using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using LibRetriX;
using Retrix.UWP.Native;
using RetriX.Shared.Components;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Foundation;
using System.IO;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace RetriX.UWP.Components
{

    internal class RenderTargetManager : IDisposable
    {
        private const uint RenderTargetMinSize = 1024;
        private object RenderTargetLock { get; } = new object();
        private CanvasBitmap RenderTarget { get; set; } = null;
        public int RenderTargetFilterType { get; set; } = 1;
        public int frameRate = 0;
        public int FrameRate { get { int tempValue = frameRate; frameRate = 0; return tempValue; } set { frameRate = value; } }
        public bool PlayAudioOnly { get; set; } = false;
        public bool Aliased { get; set; } = false;

        private Rect RenderTargetViewport = new Rect();
        //This may be different from viewport's width/haight.
        private float RenderTargetAspectRatio { get; set; } = 1.0f;

        private GameGeometry currentGeometry;
        public GameGeometry CurrentGeometry
        {
            get => currentGeometry;
            set
            {
                currentGeometry = value;
                RenderTargetAspectRatio = currentGeometry.AspectRatio;
                if (RenderTargetAspectRatio < 0.1f)
                {
                    RenderTargetAspectRatio = (float)(currentGeometry.BaseWidth) / currentGeometry.BaseHeight;
                }
            }
        }

        public PixelFormats CurrentPixelFormat { get; set; } = PixelFormats.Unknown;
        public Rotations CurrentRotation { get; set; }

        public void Dispose()
        {
            try
            {
                RenderTarget?.Dispose();
                RenderTarget = null;
            }
            catch (Exception e)
            {

            }
        }

        public void CreateResources(CanvasDrawingSession drawingSession)
        {
            try
            {
                Dispose();
                UpdateRenderTargetSize(drawingSession);
            }
            catch (Exception e)
            {

            }
        }

        public void Render(CanvasDrawingSession drawingSession, Size canvasSize)
        {
            try
            {
                if (PlayAudioOnly)
                {
                    return;
                }

                UpdateRenderTargetSize(drawingSession);

                drawingSession.Antialiasing = Aliased ? CanvasAntialiasing.Aliased : CanvasAntialiasing.Antialiased;
                drawingSession.TextAntialiasing = Aliased ? CanvasTextAntialiasing.Aliased : CanvasTextAntialiasing.Auto;

                var viewportWidth = RenderTargetViewport.Width;
                var viewportHeight = RenderTargetViewport.Height;
                var aspectRatio = RenderTargetAspectRatio;
                if (RenderTarget == null || viewportWidth <= 0 || viewportHeight <= 0)
                    return;

                var rotAngle = 0.0;
                switch (CurrentRotation)
                {
                    case Rotations.CCW90:
                        rotAngle = -0.5 * Math.PI;
                        aspectRatio = 1.0f / aspectRatio;
                        break;
                    case Rotations.CCW180:
                        rotAngle = -Math.PI;
                        break;
                    case Rotations.CCW270:
                        rotAngle = -1.5 * Math.PI;
                        aspectRatio = 1.0f / aspectRatio;
                        break;
                }

                var destinationSize = ComputeBestFittingSize(canvasSize, aspectRatio);
                var scaleMatrix = Matrix3x2.CreateScale((float)(destinationSize.Width), (float)(destinationSize.Height));
                var rotMatrix = Matrix3x2.CreateRotation((float)rotAngle);
                var transMatrix = Matrix3x2.CreateTranslation((float)(0.5 * canvasSize.Width), (float)(0.5f * canvasSize.Height));
                var transformMatrix = rotMatrix * scaleMatrix * transMatrix;

                lock (RenderTargetLock)
                {
                    drawingSession.Transform = transformMatrix;
                    var interpolation = CanvasImageInterpolation.NearestNeighbor;
                    switch (RenderTargetFilterType)
                    {
                        case 1:
                            //NearestNeighbor
                            interpolation = CanvasImageInterpolation.NearestNeighbor;
                            break;
                        case 2:
                            //Anisotropic
                            interpolation = CanvasImageInterpolation.Anisotropic;
                            break;
                        case 3:
                            //Cubic
                            interpolation = CanvasImageInterpolation.Cubic;
                            break;
                        case 4:
                            //HighQualityCubic
                            interpolation = CanvasImageInterpolation.HighQualityCubic;
                            break;
                        case 5:
                            //Linear
                            interpolation = CanvasImageInterpolation.Linear;
                            break;
                        case 6:
                            //MultiSampleLinear
                            interpolation = CanvasImageInterpolation.MultiSampleLinear;
                            break;
                        default:
                            //NearestNeighbor
                            interpolation = CanvasImageInterpolation.NearestNeighbor;
                            break;
                    }
                    //drawingSession.DrawCachedGeometry(currentGeometry, null);
                    drawingSession.DrawImage(RenderTarget, new Rect(-0.5, -0.5, 1, 1), RenderTargetViewport, 1.0f, interpolation);
                    drawingSession.Transform = Matrix3x2.Identity;
                }
            }
            catch (Exception e)
            {

            }
        }

        public bool ShowFPSCounter = false;
        private void UpdateFrameRate()
        {
            if (ShowFPSCounter)
            {
                Interlocked.Increment(ref frameRate);
            }
            else
            {
                Interlocked.Exchange(ref frameRate, 0);
            }
        }
        public unsafe void UpdateFromCoreOutput(CanvasDevice device, ReadOnlySpan<byte> data, uint width, uint height, uint pitch)
        {
            try
            {
                if (data == null || RenderTarget == null || CurrentPixelFormat == PixelFormats.Unknown || PlayAudioOnly)
                    return;


                lock (RenderTargetLock)
                {
                    RenderTargetViewport.Width = width;
                    RenderTargetViewport.Height = height;

                    using (var renderTargetMap = new BitmapMap(device, RenderTarget))
                    {
                        var inputPitch = (int)pitch;
                        var mapPitch = (int)renderTargetMap.PitchBytes;
                        //FramebufferConverter.DataToBitmap(data, new IntPtr(renderTargetMap.Data), (int)width, (int)height);
                        //FramebufferConverter.DataToBitmap(data, renderTargetMap.Data, mapPitch, (int)RenderTarget.Size.Height * mapPitch, (int)width, (int)height);
                        /*var dataArray = data.ToArray();
                        var dataLength = data.Length;
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)width, (int)height);
                            softwareBitmap.CopyFromBuffer(dataArray.AsBuffer());
                            
                            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Rgba16, BitmapAlphaMode.Ignore);
                            
                            byte[] buffer = new byte[] { };
                            softwareBitmap.CopyToBuffer(buffer.AsBuffer());
                            var mapData = new Span<byte>(new IntPtr(renderTargetMap.Data).ToPointer(), (int)RenderTarget.Size.Height * mapPitch);
                            buffer.AsSpan().CopyTo(mapData);
                        }).AsTask().Wait();
                        */
                            var mapData = new Span<byte>(new IntPtr(renderTargetMap.Data).ToPointer(), (int)RenderTarget.Size.Height * mapPitch);
                            switch (CurrentPixelFormat)
                            {
                                case PixelFormats.RGB0555:
                                    FramebufferConverter.ConvertFrameBufferRGB0555ToXRGB8888(width, height, data, inputPitch, mapData, mapPitch);
                                    break;
                                case PixelFormats.RGB565:
                                    FramebufferConverter.ConvertFrameBufferRGB565ToXRGB8888(width, height, data, inputPitch, mapData, mapPitch);
                                    break;
                                case PixelFormats.XRGB8888:
                                    FramebufferConverter.ConvertFrameBufferToXRGB8888(width, height, data, inputPitch, mapData, mapPitch);
                                    break;
                            }
                        
                        

                    }
                }
                UpdateFrameRate();
            }
            catch (Exception e)
            {

            }
        }

        private void UpdateRenderTargetSize(CanvasDrawingSession drawingSession)
        {
            if (RenderTarget != null)
            {
                try
                {
                    var currentSize = RenderTarget.Size;
                    if (currentSize.Width >= CurrentGeometry.MaxWidth && currentSize.Height >= CurrentGeometry.MaxHeight)
                    {
                        return;
                    }
                }
                catch
                {
                    return;
                }
            }
            try
            {
                lock (RenderTargetLock)
                {
                    var size = Math.Max(Math.Max(CurrentGeometry.MaxWidth, CurrentGeometry.MaxHeight), RenderTargetMinSize);
                    size = ClosestGreaterPowerTwo(size);

                    RenderTarget?.Dispose();
                    RenderTarget = BitmapMap.CreateMappableBitmap(drawingSession, size, size);
                    //RenderTarget = CanvasBitmap.CreateFromBytes(drawingSession, new byte[] { }, (int)size, (int)size, Windows.Graphics.DirectX.DirectXPixelFormat.R8G8B8A8UInt);
                }
            }
            catch (Exception e)
            {

            }
        }

        private static Size ComputeBestFittingSize(Size viewportSize, float aspectRatio)
        {
            try
            {
                var candidateWidth = Math.Floor(viewportSize.Height * aspectRatio);
                var size = new Size(candidateWidth, viewportSize.Height);
                if (viewportSize.Width < candidateWidth)
                {
                    var height = viewportSize.Width / aspectRatio;
                    size = new Size(viewportSize.Width, height);
                }

                return size;
            }
            catch (Exception e)
            {
                return viewportSize;
            }
        }

        private static uint ClosestGreaterPowerTwo(uint value)
        {
            uint output = 1;
            while (output < value)
            {
                output *= 2;
            }

            return output;
        }
    }
}
