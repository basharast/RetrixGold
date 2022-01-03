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
using Windows.Storage;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Effects;
using System.Collections.Generic;
using Windows.Graphics.Effects;
using System.Linq;
using Windows.UI;
using Windows.Graphics.DirectX;
using System.Threading.Tasks;

namespace RetriX.UWP.Components
{

    internal class RenderTargetManager : IDisposable
    {
        private const uint RenderTargetMinSize = 1024;
        private object RenderTargetLock { get; } = new object();
        private CanvasBitmap RenderTarget { get; set; } = null;
        public int RenderTargetFilterType { get; set; } = 1;
        public List<RenderEffect> RenderEffectsList { get; set; } = new List<RenderEffect>();

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
                    
                    if (RenderEffectsList != null && RenderEffectsList.Count > 0)
                    {
                       UpdateEffects(drawingSession, RenderTarget, viewportWidth, viewportHeight, interpolation);
                    }
                    else
                    {
                        drawingSession.DrawImage(RenderTarget, new Rect(-0.5, -0.5, 1, 1), RenderTargetViewport, 1.0f, interpolation);
                    }

                    drawingSession.Transform = Matrix3x2.Identity;
                }
            }
            catch (Exception e)
            {

            }
        }

        private void UpdateEffects(CanvasDrawingSession drawingSession, CanvasBitmap RenderTarget, double viewportWidth, double viewportHeight, CanvasImageInterpolation interpolation)
        {
            ICanvasImage outputResult = RenderTarget;

            try
            {
                List<PixelShaderEffect> outputShaders = new List<PixelShaderEffect>();
                List<CanvasBitmap> outputOverlays = new List<CanvasBitmap>();
                foreach (var effect in RenderEffectsList.OrderBy(item => item.Order))
                {
                    try
                    {
                        switch (effect.Name)
                        {
                            case "PixelShaderEffect":
                                foreach (var eValue in effect.Values1)
                                {
                                    PixelShaderEffect pixelShaderEffect = new PixelShaderEffect(eValue);
                                    pixelShaderEffect.CacheOutput = true;
                                    pixelShaderEffect.Source1Interpolation = interpolation;
                                    pixelShaderEffect.Source1Mapping = SamplerCoordinateMapping.OneToOne;
                                    outputShaders.Add(pixelShaderEffect);
                                }
                                break;

                            case "OverlayEffect":
                                foreach (var eValue in effect.Values1)
                                {
                                    if (effect.tempResult == null)
                                    {
                                        using (var ms = new System.IO.MemoryStream(eValue))
                                        {
                                            var bitmap = CanvasBitmap.LoadAsync(drawingSession.Device, ms.AsRandomAccessStream()).AsTask().Result;
                                            effect.tempResult = bitmap;
                                            outputOverlays.Add(bitmap);
                                        }
                                    }
                                    else
                                    {
                                        outputOverlays.Add(effect.tempResult);
                                    }
                                }
                                break;

                            case "BrightnessEffect":
                                BrightnessEffect brightnessEffect = new BrightnessEffect();
                                brightnessEffect.Source = outputResult;
                                brightnessEffect.CacheOutput = true;
                                brightnessEffect.BlackPoint = new Vector2(0f, (float)effect.Value1);
                                outputResult = brightnessEffect;
                                break;

                            case "ContrastEffect":
                                ContrastEffect contrastEffect = new ContrastEffect();
                                contrastEffect.Source = outputResult;
                                contrastEffect.CacheOutput = true;
                                contrastEffect.Contrast = (float)effect.Value1;
                                outputResult = contrastEffect;
                                break;

                            case "DirectionalBlurEffect":
                                DirectionalBlurEffect directionalBlurEffect = new DirectionalBlurEffect();
                                directionalBlurEffect.Source = outputResult;
                                directionalBlurEffect.CacheOutput = true;
                                directionalBlurEffect.BlurAmount = (float)effect.Value1;
                                directionalBlurEffect.Angle = (float)effect.Value2;
                                outputResult = directionalBlurEffect;
                                break;

                            case "EdgeDetectionEffect":
                                EdgeDetectionEffect edgeDetectionEffect = new EdgeDetectionEffect();
                                edgeDetectionEffect.Source = outputResult;
                                edgeDetectionEffect.CacheOutput = true;
                                edgeDetectionEffect.Amount = (float)effect.Value1;
                                edgeDetectionEffect.BlurAmount = (float)effect.Value2;
                                outputResult = edgeDetectionEffect;
                                break;

                            case "EmbossEffect":
                                EmbossEffect embossEffect = new EmbossEffect();
                                embossEffect.Source = outputResult;
                                embossEffect.CacheOutput = true;
                                embossEffect.Amount = (float)effect.Value1;
                                embossEffect.Angle = (float)effect.Value2;
                                outputResult = embossEffect;
                                break;

                            case "ExposureEffect":
                                ExposureEffect exposureEffect = new ExposureEffect();
                                exposureEffect.Source = outputResult;
                                exposureEffect.CacheOutput = true;
                                exposureEffect.Exposure = (float)effect.Value1;
                                outputResult = exposureEffect;
                                break;

                            case "GaussianBlurEffect":
                                GaussianBlurEffect gaussianBlurEffect = new GaussianBlurEffect();
                                gaussianBlurEffect.Source = outputResult;
                                gaussianBlurEffect.CacheOutput = true;
                                gaussianBlurEffect.BlurAmount = (float)effect.Value1;
                                outputResult = gaussianBlurEffect;
                                break;

                            case "GrayscaleEffect":
                                GrayscaleEffect grayscaleEffect = new GrayscaleEffect();
                                grayscaleEffect.Source = outputResult;
                                grayscaleEffect.CacheOutput = true;
                                outputResult = grayscaleEffect;
                                break;

                            case "InvertEffect":
                                InvertEffect invertEffect = new InvertEffect();
                                invertEffect.Source = outputResult;
                                invertEffect.CacheOutput = true;
                                outputResult = invertEffect;
                                break;

                            case "HueToRgbEffect":
                                HueToRgbEffect hueToRgbEffect = new HueToRgbEffect();
                                hueToRgbEffect.Source = outputResult;
                                hueToRgbEffect.CacheOutput = true;
                                outputResult = hueToRgbEffect;
                                break;

                            case "RgbToHueEffect":
                                RgbToHueEffect rgbToHueEffect = new RgbToHueEffect();
                                rgbToHueEffect.Source = outputResult;
                                rgbToHueEffect.CacheOutput = true;
                                outputResult = rgbToHueEffect;
                                break;

                            case "HighlightsAndShadowsEffect":
                                HighlightsAndShadowsEffect highlightsAndShadowsEffect = new HighlightsAndShadowsEffect();
                                highlightsAndShadowsEffect.Source = outputResult;
                                highlightsAndShadowsEffect.CacheOutput = true;
                                highlightsAndShadowsEffect.Clarity = (float)effect.Value1;
                                highlightsAndShadowsEffect.Highlights = (float)effect.Value2;
                                highlightsAndShadowsEffect.Shadows = (float)effect.Value3;
                                outputResult = highlightsAndShadowsEffect;
                                break;

                            case "PosterizeEffect":
                                PosterizeEffect posterizeEffect = new PosterizeEffect();
                                posterizeEffect.Source = outputResult;
                                posterizeEffect.CacheOutput = true;
                                posterizeEffect.RedValueCount = (int)effect.Value1;
                                posterizeEffect.GreenValueCount = (int)effect.Value2;
                                posterizeEffect.BlueValueCount = (int)effect.Value3;
                                outputResult = posterizeEffect;
                                break;

                            case "MorphologyEffect":
                                MorphologyEffect morphologyEffect = new MorphologyEffect();
                                morphologyEffect.Source = outputResult;
                                morphologyEffect.CacheOutput = true;
                                morphologyEffect.Height = (int)effect.Value1;
                                outputResult = morphologyEffect;
                                break;

                            case "SaturationEffect":
                                SaturationEffect saturationEffect = new SaturationEffect();
                                saturationEffect.Source = outputResult;
                                saturationEffect.CacheOutput = true;
                                saturationEffect.Saturation = (float)effect.Value1;
                                outputResult = saturationEffect;
                                break;

                            case "ScaleEffect":
                                ScaleEffect scaleEffect = new ScaleEffect();
                                scaleEffect.Source = outputResult;
                                scaleEffect.CacheOutput = true;
                                scaleEffect.Scale = new Vector2((float)effect.Value1, (float)effect.Value2);
                                scaleEffect.CenterPoint = new Vector2((float)viewportWidth / 2, (float)viewportHeight / 2);
                                scaleEffect.Sharpness = (float)effect.Value3;
                                outputResult = scaleEffect;
                                break;

                            case "SepiaEffect":
                                SepiaEffect sepiaEffect = new SepiaEffect();
                                sepiaEffect.Source = outputResult;
                                sepiaEffect.CacheOutput = true;
                                sepiaEffect.Intensity = (float)effect.Value1;
                                outputResult = sepiaEffect;
                                break;

                            case "SharpenEffect":
                                SharpenEffect sharpenEffect = new SharpenEffect();
                                sharpenEffect.Source = outputResult;
                                sharpenEffect.CacheOutput = true;
                                sharpenEffect.Amount = (float)effect.Value1;
                                outputResult = sharpenEffect;
                                break;

                            case "StraightenEffect":
                                StraightenEffect straightenEffect = new StraightenEffect();
                                straightenEffect.Source = outputResult;
                                straightenEffect.CacheOutput = true;
                                straightenEffect.Angle = (float)effect.Value1;
                                straightenEffect.MaintainSize = true;
                                straightenEffect.InterpolationMode = interpolation;
                                outputResult = straightenEffect;
                                break;

                            case "TemperatureAndTintEffect":
                                TemperatureAndTintEffect temperatureAndTintEffect = new TemperatureAndTintEffect();
                                temperatureAndTintEffect.Source = outputResult;
                                temperatureAndTintEffect.CacheOutput = true;
                                temperatureAndTintEffect.Temperature = (float)effect.Value1;
                                temperatureAndTintEffect.Tint = (float)effect.Value2;
                                outputResult = temperatureAndTintEffect;
                                break;

                            case "TileEffect":
                                TileEffect tileEffect = new TileEffect();
                                tileEffect.Source = outputResult;
                                tileEffect.CacheOutput = true;
                                tileEffect.SourceRectangle = new Rect(effect.Value1, effect.Value2, effect.Value3, effect.Value4);
                                outputResult = tileEffect;
                                break;

                            case "CropEffect":
                                CropEffect cropEffect = new CropEffect();
                                cropEffect.Source = outputResult;
                                cropEffect.CacheOutput = true;
                                cropEffect.SourceRectangle = new Rect(effect.Value1, effect.Value2, effect.Value3, effect.Value4);
                                outputResult = cropEffect;
                                break;

                            case "VignetteEffect":
                                VignetteEffect vignetteEffect = new VignetteEffect();
                                vignetteEffect.Source = outputResult;
                                vignetteEffect.CacheOutput = true;
                                vignetteEffect.Amount = (float)effect.Value1;
                                vignetteEffect.Curve = (float)effect.Value2;
                                outputResult = vignetteEffect;
                                break;

                            case "Transform3DEffect":
                                Transform3DEffect transform3DEffect = new Transform3DEffect();
                                transform3DEffect.Source = outputResult;
                                transform3DEffect.CacheOutput = true;
                                transform3DEffect.TransformMatrix = Matrix4x4.CreateRotationZ((float)effect.Value1, new Vector3((float)viewportWidth / 2, (float)viewportHeight / 2, 0)) * Matrix4x4.CreateRotationX((float)effect.Value2, new Vector3((float)viewportWidth / 2, (float)viewportHeight / 2, 0)) * Matrix4x4.CreateRotationY((float)effect.Value3, new Vector3((float)viewportWidth / 2, (float)viewportHeight / 2, 0));
                                outputResult = transform3DEffect;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                drawingSession.DrawImage(outputResult, new Rect(-0.5, -0.5, 1, 1), RenderTargetViewport, 1.0f, interpolation);

                if (outputOverlays.Count > 0)
                {
                    foreach (var overlayItem in outputOverlays)
                    {
                        ScaleEffect scaleEffect = new ScaleEffect();
                        scaleEffect.Source = overlayItem;
                        var scaleFactorWidth = viewportWidth / overlayItem.SizeInPixels.Width;
                        var scaleFactorHeight = viewportWidth / overlayItem.SizeInPixels.Height;
                        scaleEffect.Scale = new Vector2((float)scaleFactorWidth, (float)scaleFactorHeight);
                        //scaleEffect.CenterPoint =new Vector2((float)viewportWidth / 2, (float)viewportHeight / 2);
                        drawingSession.DrawImage(scaleEffect, new Rect(-0.5, -0.5, 1, 1), RenderTargetViewport, 0.9f, interpolation);
                    }
                }
                if (outputShaders.Count > 0)
                {
                    foreach (var shaderItem in outputShaders)
                    {
                        drawingSession.DrawImage(shaderItem, new Rect(-0.5, -0.5, 1, 1), RenderTargetViewport, 1.0f, interpolation);
                    }
                }
            }
            catch (Exception e) { 
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

        public unsafe void UpdateFromCoreOutput(CanvasAnimatedControl renderPanel, ReadOnlySpan<byte> data, uint width, uint height, uint pitch)
        {
            var device = renderPanel.Device;
            try
            {
                if (data == null || RenderTarget == null || PlayAudioOnly)
                    return;

                lock (RenderTargetLock)
                {
                    RenderTargetViewport.Width = width;
                    RenderTargetViewport.Height = height;

                    /**
                    * Below my attemp to convert the pixels directly with Win2D
                    * AS PER PIXLES TESTS
                    * 
                    * FORMAT 565 matched:
                    * -R8G8UIntNormalized (Crash)
                    * -R8UIntNormalized (Crash)
                    * -BC1UIntNormalized (Pixels issue)
                    * -BC2UIntNormalized (Pixels issue)
                    * -BC3UIntNormalized (Pixels issue)
                    * 
                    * 
                    * FORMAT X8888 matched:
                    * -R16G16B16A16Float (Black)
                    * -R16G16B16A16UIntNormalized (Black)
                    * -R10G10B10A2UIntNormalized (Black)
                    * -R8G8B8A8UIntNormalized (Black)
                    * -R8G8B8A8UIntNormalizedSrgb (Black)
                    * -B8G8R8A8UIntNormalized (Black)
                    * -B8G8R8X8UIntNormalized (Black)
                    * -B8G8R8A8UIntNormalizedSrgb (Black)
                    * -R8G8UIntNormalized (Black)
                    * -R8UIntNormalized (Black)
                    * -BC1UIntNormalized (Black)
                    * -BC2UIntNormalized (Black)
                    * -BC3UIntNormalized (Black)
                    * 
                    * 
                    * 
                    * FORMAT 555 matched:
                    * -R8G8UIntNormalized (Black)
                    * -R8UIntNormalized (Black)
                    *  
                    */
                    /*
                    byte[] dataBytes = new byte[data.Length];
                    fixed (byte* inputPointer = &data[0])
                        Marshal.Copy((IntPtr)inputPointer, dataBytes, 0, data.Length);

                   switch (CurrentPixelFormat)
                    {
                        case PixelFormats.RGB0555:
                            RenderTarget = CanvasBitmap.CreateFromBytes(renderPanel, dataBytes, (int)width, (int)height, DirectXPixelFormat.R8G8UIntNormalized, 92, CanvasAlphaMode.Ignore);
                            break;
                        case PixelFormats.RGB565:
                            RenderTarget = CanvasBitmap.CreateFromBytes(renderPanel, dataBytes, (int)width, (int)height, DirectXPixelFormat.BC1UIntNormalized, 92, CanvasAlphaMode.Ignore);
                            break;
                        case PixelFormats.XRGB8888:
                            RenderTarget = CanvasBitmap.CreateFromBytes(renderPanel, dataBytes, (int)width, (int)height, DirectXPixelFormat.B8G8R8A8UIntNormalizedSrgb, 92, CanvasAlphaMode.Ignore);
                            break;
                    }
                    */

                    using (var renderTargetMap = new BitmapMap(device, RenderTarget))
                    {
                        var inputPitch = (int)pitch;
                        var mapPitch = (int)renderTargetMap.PitchBytes;

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
                //var LogFile = ApplicationData.Current.TemporaryFolder.CreateFileAsync("Temp.txt", CreationCollisionOption.OpenIfExists).AsTask().Result;
                //FileIO.AppendTextAsync(LogFile, $"\n\r----------------------\n\r{e.Message}\n\r------------------------------\n\r").AsTask().Wait();
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
