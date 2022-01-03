using Acr.UserDialogs;
using LibRetriX;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using Newtonsoft.Json;
using Plugin.FileSystem;
using Plugin.FileSystem.Abstractions;
using Plugin.Settings.Abstractions;
using RetriX.Shared.Components;
using RetriX.Shared.Models;
using RetriX.Shared.Services;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RetriX.Shared.ViewModels
{
    public class GamePlayerViewModel : MvxViewModel<GameLaunchEnvironment>, IDisposable
    {

        public bool compatibiltyTag
        {
            get
            {
               return FramebufferConverter.isRGB888;
            }
        }
        //Here Progress Helper for archived roms
        public string currentFileEntry
        {
            get
            {
                return FramebufferConverter.currentFileEntry;
            }
        }
        public double currentFileProgress
        {
            get
            {
                return FramebufferConverter.currentFileProgress;
            }
        }
        public bool isProgressVisible
        {
            get
            {
                return FramebufferConverter.currentFileProgress > 0 && !isGameStarted && !FailedToLoadGame;
            }
        }


        //All the junk below for dialog just to avoid crash on Windows Phone in case two dialogs appears at once
        #region DIALOG
        bool isDialogInProgressTemp = false;
        bool isDialogInProgress
        {
            get
            {
                if (PlatformService != null)
                {
                    return PlatformService.DialogInProgress;
                }
                else
                {
                    return isDialogInProgressTemp;
                }
            }
            set
            {
                if (PlatformService != null)
                {
                    PlatformService.DialogInProgress = value;
                }
                else
                {
                    isDialogInProgressTemp = value;
                }
            }
        }
        private async Task GeneralDialog(string Message, string title = null, string okButton = null)
        {
            if (isDialogInProgress)
            {
                UpdateInfoState(Message);
                return;
            }
            isDialogInProgress = true;
            try
            {
                await UserDialogs.Instance.AlertAsync(Message, title, okButton);
            }
            catch (Exception ex)
            {

            }
            isDialogInProgress = false;
        }

        #endregion

        //Memory Helpers
        #region MEMORY
        public bool CrazyBufferActive = true;
        public bool isMemoryHelpersVisible
        {
            get
            {
                return true;
            }
        }
        private bool bufferCopyMemory = true;
        public bool BufferCopyMemory
        {
            get
            {
                return bufferCopyMemory;
            }
            set
            {
                bufferCopyMemory = value;
                if (value && FramebufferConverter.MoveMemoryAvailable)
                {
                    memCPYMemory = false;
                    MarshalMemory = false;
                    SpanlMemory = false;
                    FramebufferConverter.MemoryHelper = "Buffer.CopyMemory";
                    if (isGameStarted && !memoryOptionsInitial)
                    {
                        PlatformService.PlayNotificationSound("success.wav");
                        UpdateInfoState("Memory Helper: Buffer.CopyMemory");
                    }
                }
                else
                {
                    if (!memoryOptionsInitial && !memCPYMemory && !MarshalMemory && !SpanlMemory)
                    {
                        if (!FramebufferConverter.MoveMemoryAvailable)
                        {
                            SpanlMemory = true;
                        }
                        else
                        {
                            BufferCopyMemory = true;
                        }
                    }
                    if (value && !FramebufferConverter.MoveMemoryAvailable)
                    {
                        bufferCopyMemory = false;
                        PlatformService.PlayNotificationSound("root-needed.wav");
                        GeneralDialog($"Sorry!, memmov not available due:\n{FramebufferConverter.memmovErrorMessage}");

                    }
                }
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("BufferCopyMemory", bufferCopyMemory);
                RaisePropertyChanged("BufferCopyMemory");
                forceReloadLogsList = true;
            }
        }

        private bool memcpyMemory = false;
        public bool memCPYMemory
        {
            get
            {
                return memcpyMemory;
            }
            set
            {

                memcpyMemory = value;
                if (value && FramebufferConverter.CopyMemoryAvailable)
                {
                    BufferCopyMemory = false;
                    MarshalMemory = false;
                    SpanlMemory = false;
                    FramebufferConverter.MemoryHelper = "memcpy (msvcrt.dll)";
                    if (isGameStarted && !memoryOptionsInitial)
                    {
                        PlatformService.PlayNotificationSound("success.wav");
                        UpdateInfoState("Memory Helper: memcpy (msvcrt.dll)");
                    }
                }
                else
                {
                    if (!memoryOptionsInitial && !BufferCopyMemory && !MarshalMemory && !SpanlMemory)
                    {
                        if (!FramebufferConverter.CopyMemoryAvailable)
                        {
                            BufferCopyMemory = true;
                        }
                        else
                        {
                            memCPYMemory = true;
                        }
                    }
                    if (value && !FramebufferConverter.CopyMemoryAvailable)
                    {
                        memcpyMemory = false;
                        PlatformService.PlayNotificationSound("root-needed.wav");
                        GeneralDialog($"Sorry!, memcpy not available due:\n{FramebufferConverter.memcpyErrorMessage}");
                    }
                }
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("memCPYMemory", memcpyMemory);
                RaisePropertyChanged("memCPYMemory");
                forceReloadLogsList = true;
            }
        }

        private bool marshaMemory = false;
        public bool MarshalMemory
        {
            get
            {
                return marshaMemory;
            }
            set
            {
                marshaMemory = value;
                if (value)
                {
                    BufferCopyMemory = false;
                    memCPYMemory = false;
                    SpanlMemory = false;
                    FramebufferConverter.MemoryHelper = "Marshal.CopyTo";
                    if (isGameStarted && !memoryOptionsInitial)
                    {
                        PlatformService.PlayNotificationSound("success.wav");
                        UpdateInfoState("Memory Helper: Marshal.CopyTo");
                    }
                }
                else
                {
                    if (!memoryOptionsInitial && !BufferCopyMemory && !memCPYMemory && !spanMemory)
                    {
                        MarshalMemory = true;
                    }
                }
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("MarshalMemory", marshaMemory);
                RaisePropertyChanged("MarshalMemory");
                forceReloadLogsList = true;
            }
        }

        private bool spanMemory = false;
        public bool SpanlMemory
        {
            get
            {
                return spanMemory;
            }
            set
            {
                spanMemory = value;
                if (value)
                {
                    MarshalMemory = false;
                    memCPYMemory = false;
                    BufferCopyMemory = false;
                    FramebufferConverter.MemoryHelper = "Span.CopyTo";
                    UpdateInfoState("Memory Helper: Span.CopyTo");
                    if (isGameStarted && !memoryOptionsInitial)
                    {
                        PlatformService.PlayNotificationSound("success.wav");
                        UpdateInfoState("Memory Helper: Span.CopyTo");
                    }
                }
                else
                {
                    if (!memoryOptionsInitial && !BufferCopyMemory && !memCPYMemory && !MarshalMemory)
                    {
                        SpanlMemory = true;
                    }
                }
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("SpanlMemory", spanMemory);
                RaisePropertyChanged("SpanlMemory");
                forceReloadLogsList = true;
            }
        }

        bool memoryOptionsInitial = false;
        public void SyncMemoryOptions()
        {
            memoryOptionsInitial = true;
            BufferCopyMemory = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BufferCopyMemory", true);
            memCPYMemory = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("memCPYMemory", false);
            MarshalMemory = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("MarshalMemory", false);
            SpanlMemory = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("SpanlMemory", false);
        }
        #endregion
        //Memory Helpers

        //Effects System
        public bool EffectsVisible = false;
        public bool addShaders = false;
        public bool addShadersInitial = false;
        public bool AddShaders
        {
            get
            {
                return addShaders;
            }
            set
            {
                addShaders = value;
                if (!addShadersInitial)
                {
                    if (value)
                    {
                        GetShader();
                    }
                    else
                    {
                        UpdateEffect("PixelShaderEffect", false, null);
                        PlatformService.PlayNotificationSound("success.wav");
                        UpdateInfoState($"Disabled: PixelShaderEffect");
                    }
                    RaisePropertyChanged(nameof(AddShaders));
                }
            }
        }
        bool shaderInProgress = false;
        private async void GetShader()
        {
            if (shaderInProgress)
            {
                return;
            }
            shaderInProgress = true;
            try
            {
                PlatformService.PlayNotificationSound("root-needed.wav");
                await GeneralDialog("Note: Shaders still in development and not fully supported\nRequired shaders compiled as .bin");

                GameIsLoadingState(true);
                var shader = await PlatformService.getShader();
                if (shader != null)
                {
                    UpdateEffect("PixelShaderEffect", true, shader);
                    PlatformService.PlayNotificationSound("success.wav");
                    UpdateInfoState($"Activated: PixelShaderEffect");
                }
                else
                {
                    AddShaders = false;
                }
            }
            catch (Exception ex)
            {
                AddShaders = false;
            }
            GameIsLoadingState(false);
            shaderInProgress = false;
        }

        public bool addOverlays = false;
        public bool addOverlaysInitial = false;
        public bool AddOverlays
        {
            get
            {
                return addOverlays;
            }
            set
            {
                addOverlays = value;
                if (!addOverlaysInitial)
                {
                    if (value)
                    {
                        GetOverlay();
                    }
                    else
                    {
                        UpdateEffect("OverlayEffect", false, null);
                        PlatformService.PlayNotificationSound("success.wav");
                        UpdateInfoState($"Disabled: OverlayEffect");
                    }
                    RaisePropertyChanged(nameof(AddOverlays));
                }
            }
        }
        bool overlayInProgress = false;
        private async void GetOverlay()
        {
            if (overlayInProgress)
            {
                return;
            }
            overlayInProgress = true;
            try
            {
                PlatformService.PlayNotificationSound("root-needed.wav");
                await GeneralDialog("Note: You can select multiple overlays\nOverlays will be saved only for this session");

                GameIsLoadingState(true);
                var overlay = await PlatformService.getOverlay();
                if (overlay != null)
                {
                    UpdateEffect("OverlayEffect", true, overlay);
                    PlatformService.PlayNotificationSound("success.wav");
                    UpdateInfoState($"Activated: OverlayEffect");
                }
                else
                {
                    AddOverlays = false;
                }
            }
            catch (Exception ex)
            {
                AddOverlays = false;
            }
            GameIsLoadingState(false);
            overlayInProgress = false;
        }
        //BrightnessEffect
        #region Brightness Effect
        public double brightnessLevel = 0.0;
        public double BrightnessLevel
        {
            get
            {
                return brightnessLevel;
            }
            set
            {
                brightnessLevel = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("BrightnessLevel", brightnessLevel);
                RaisePropertyChanged("BrightnessLevel");
                UpdateEffect("BrightnessEffect", BrightnessEffect, brightnessLevel);
            }
        }
        public bool brightnessEffect = false;
        public bool BrightnessEffect
        {
            set
            {
                brightnessEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("BrightnessEffect", brightnessEffect);
                RaisePropertyChanged("BrightnessEffect");
                UpdateEffect("BrightnessEffect", BrightnessEffect, brightnessLevel);
            }
            get
            {
                return brightnessEffect;
            }
        }
        #endregion

        //Transform3DEffect
        #region Transform 3D Effect
        public double rotate = 0;
        public double Rotate
        {
            get
            {
                return rotate;
            }
            set
            {
                rotate = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Rotate", rotate);
                RaisePropertyChanged("Rotate");
                UpdateEffect("Transform3DEffect", Transform3DEffect, rotate, RotateX, RotateY);
            }
        }
        public double rotateX = 0;
        public double RotateX
        {
            get
            {
                return rotateX;
            }
            set
            {
                rotateX = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("RotateX", rotateX);
                RaisePropertyChanged("RotateX");
                UpdateEffect("Transform3DEffect", Transform3DEffect, Rotate, rotateX, RotateY);
            }
        }
        public double rotateY = 0;
        public double RotateY
        {
            get
            {
                return rotateY;
            }
            set
            {
                rotateY = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("RotateY", rotateY);
                RaisePropertyChanged("RotateY");
                UpdateEffect("Transform3DEffect", Transform3DEffect, Rotate, RotateX, rotateY);
            }
        }
        public bool transform3DEffect = false;
        public bool Transform3DEffect
        {
            set
            {
                transform3DEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Transform3DEffect", transform3DEffect);
                RaisePropertyChanged("Transform3DEffect");
                UpdateEffect("Transform3DEffect", Transform3DEffect, Rotate, RotateX, RotateY);
            }
            get
            {
                return transform3DEffect;
            }
        }
        #endregion

        //ContrastEffect
        #region Contrast Effect
        public double contrastLevel = 0.0;
        public double ContrastLevel
        {
            get
            {
                return contrastLevel;
            }
            set
            {
                contrastLevel = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("ContrastLevel", contrastLevel);
                RaisePropertyChanged("ContrastLevel");
                UpdateEffect("ContrastEffect", ContrastEffect, contrastLevel);
            }
        }
        public bool contrastEffect = false;
        public bool ContrastEffect
        {
            set
            {
                contrastEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("ContrastEffect", contrastEffect);
                RaisePropertyChanged("ContrastEffect");
                UpdateEffect("ContrastEffect", contrastEffect, ContrastLevel);
            }
            get
            {
                return contrastEffect;
            }
        }
        #endregion

        //ExposureEffect
        #region Exposure Effect
        public double exposure = 0.0;
        public double Exposure
        {
            get
            {
                return exposure;
            }
            set
            {
                exposure = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Exposure", exposure);
                RaisePropertyChanged("Exposure");
                UpdateEffect("ExposureEffect", ExposureEffect, exposure);
            }
        }
        public bool exposureEffect = false;
        public bool ExposureEffect
        {
            set
            {
                exposureEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("ExposureEffect", exposureEffect);
                RaisePropertyChanged("ExposureEffect");
                UpdateEffect("ExposureEffect", ExposureEffect, Exposure);
            }
            get
            {
                return exposureEffect;
            }
        }
        #endregion

        //SepiaEffect
        #region SepiaEffect
        public double intensity = 0.5;
        public double Intensity
        {
            get
            {
                return intensity;
            }
            set
            {
                intensity = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Intensity", intensity);
                RaisePropertyChanged("Intensity");
                UpdateEffect("SepiaEffect", SepiaEffect, intensity);
            }
        }
        public bool sepiaEffect = false;
        public bool SepiaEffect
        {
            set
            {
                sepiaEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("SepiaEffect", sepiaEffect);
                RaisePropertyChanged("SepiaEffect");
                UpdateEffect("SepiaEffect", SepiaEffect, Intensity);
            }
            get
            {
                return sepiaEffect;
            }
        }
        #endregion

        //SharpenEffect
        #region SharpenEffect
        public double amountSharpen = 0.0;
        public double AmountSharpen
        {
            get
            {
                return amountSharpen;
            }
            set
            {
                amountSharpen = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("AmountSharpen", amountSharpen);
                RaisePropertyChanged("AmountSharpen");
                UpdateEffect("SharpenEffect", SharpenEffect, amountSharpen);
            }
        }
        public bool sharpenEffect = false;
        public bool SharpenEffect
        {
            set
            {
                sharpenEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("SharpenEffect", sharpenEffect);
                RaisePropertyChanged("SharpenEffect");
                UpdateEffect("SharpenEffect", SharpenEffect, AmountSharpen);
            }
            get
            {
                return sharpenEffect;
            }
        }
        #endregion

        //StraightenEffect
        #region Straighten Effect
        public double angleStraighten = 0.0;
        public double AngleStraighten
        {
            get
            {
                return angleStraighten;
            }
            set
            {
                angleStraighten = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("AngleStraighten", angleStraighten);
                RaisePropertyChanged("AngleStraighten");
                UpdateEffect("StraightenEffect", StraightenEffect, angleStraighten);
            }
        }
        public bool straightenEffect = false;
        public bool StraightenEffect
        {
            set
            {
                straightenEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("StraightenEffect", straightenEffect);
                RaisePropertyChanged("StraightenEffect");
                UpdateEffect("StraightenEffect", StraightenEffect, AngleStraighten);
            }
            get
            {
                return straightenEffect;
            }
        }
        #endregion

        //VignetteEffect
        #region Vignette Effect
        public double amountVignette = 0.50;
        public double AmountVignette
        {
            get
            {
                return amountVignette;
            }
            set
            {
                amountVignette = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("AmountVignette", amountVignette);
                RaisePropertyChanged("AmountVignette");
                UpdateEffect("VignetteEffect", VignetteEffect, amountVignette, Curve);
            }
        }
        public double curve = 0.0;
        public double Curve
        {
            get
            {
                return curve;
            }
            set
            {
                curve = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Curve", curve);
                RaisePropertyChanged("Curve");
                UpdateEffect("VignetteEffect", VignetteEffect, AmountVignette, curve);
            }
        }
        public bool vignetteEffect = false;
        public bool VignetteEffect
        {
            set
            {
                vignetteEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("VignetteEffect", vignetteEffect);
                RaisePropertyChanged("VignetteEffect");
                UpdateEffect("VignetteEffect", VignetteEffect, AmountVignette, Curve);
            }
            get
            {
                return vignetteEffect;
            }
        }
        #endregion

        //TileEffect
        #region Tile Effect
        public double left = 0.0;
        public double Left
        {
            get
            {
                return left;
            }
            set
            {
                left = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Left", left);
                RaisePropertyChanged("Left");
                UpdateEffect("TileEffect", TileEffect, left, Top, Right, Bottom);
            }
        }
        public double top = 0.0;
        public double Top
        {
            get
            {
                return top;
            }
            set
            {
                top = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Top", top);
                RaisePropertyChanged("Top");
                UpdateEffect("TileEffect", TileEffect, Left, top, Right, Bottom);
            }
        }
        public double right = 256;
        public double Right
        {
            get
            {
                return right;
            }
            set
            {
                right = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Right", right);
                RaisePropertyChanged("Right");
                UpdateEffect("TileEffect", TileEffect, Left, Top, right, Bottom);
            }
        }
        public double bottom = 256;
        public double Bottom
        {
            get
            {
                return bottom;
            }
            set
            {
                bottom = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Bottom", bottom);
                RaisePropertyChanged("Bottom");
                UpdateEffect("TileEffect", TileEffect, Left, Top, Right, bottom);
            }
        }
        public bool tileEffect = false;
        public bool TileEffect
        {
            set
            {
                tileEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("TileEffect", tileEffect);
                RaisePropertyChanged("TileEffect");
                UpdateEffect("TileEffect", TileEffect, Left, Top, Right, Bottom);
            }
            get
            {
                return tileEffect;
            }
        }
        #endregion

        //CropEffect
        #region Crop Effect
        public double leftCrop = 0.0;
        public double LeftCrop
        {
            get
            {
                return leftCrop;
            }
            set
            {
                leftCrop = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("LeftCrop", leftCrop);
                RaisePropertyChanged("LeftCrop");
                UpdateEffect("CropEffect", CropEffect, leftCrop, TopCrop, RightCrop, BottomCrop);
            }
        }
        public double topCrop = 0.0;
        public double TopCrop
        {
            get
            {
                return topCrop;
            }
            set
            {
                topCrop = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("TopCrop", topCrop);
                RaisePropertyChanged("TopCrop");
                UpdateEffect("CropEffect", CropEffect, LeftCrop, topCrop, RightCrop, BottomCrop);
            }
        }
        public double rightCrop = 256;
        public double RightCrop
        {
            get
            {
                return rightCrop;
            }
            set
            {
                rightCrop = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("RightCrop", rightCrop);
                RaisePropertyChanged("RightCrop");
                UpdateEffect("CropEffect", CropEffect, LeftCrop, TopCrop, rightCrop, BottomCrop);
            }
        }
        public double bottomCrop = 256;
        public double BottomCrop
        {
            get
            {
                return bottomCrop;
            }
            set
            {
                bottomCrop = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("BottomCrop", bottomCrop);
                RaisePropertyChanged("BottomCrop");
                UpdateEffect("CropEffect", CropEffect, LeftCrop, TopCrop, RightCrop, bottomCrop);
            }
        }
        public bool cropEffect = false;
        public bool CropEffect
        {
            set
            {
                cropEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("CropEffect", cropEffect);
                RaisePropertyChanged("CropEffect");
                UpdateEffect("CropEffect", CropEffect, LeftCrop, TopCrop, RightCrop, BottomCrop);
            }
            get
            {
                return cropEffect;
            }
        }
        #endregion

        //TemperatureAndTintEffect
        #region Temperature And Tint Effect
        public double temperature = 0.0;
        public double Temperature
        {
            get
            {
                return temperature;
            }
            set
            {
                temperature = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Temperature", temperature);
                RaisePropertyChanged("Temperature");
                UpdateEffect("TemperatureAndTintEffect", TemperatureAndTintEffect, temperature, Tint);
            }
        }
        public double tint = 0.0;
        public double Tint
        {
            get
            {
                return tint;
            }
            set
            {
                tint = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Tint", tint);
                RaisePropertyChanged("Tint");
                UpdateEffect("TemperatureAndTintEffect", TemperatureAndTintEffect, Temperature, tint);
            }
        }
        public bool temperatureAndTintEffect = false;
        public bool TemperatureAndTintEffect
        {
            set
            {
                temperatureAndTintEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("TemperatureAndTintEffect", temperatureAndTintEffect);
                RaisePropertyChanged("TemperatureAndTintEffect");
                UpdateEffect("TemperatureAndTintEffect", TemperatureAndTintEffect, Temperature, Tint);
            }
            get
            {
                return temperatureAndTintEffect;
            }
        }
        #endregion

        //MorphologyEffect
        #region Morphology Effect
        public double heightMorphology = 1.0;
        public double HeightMorphology
        {
            get
            {
                return heightMorphology;
            }
            set
            {
                heightMorphology = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("HeightMorphology", heightMorphology);
                RaisePropertyChanged("HeightMorphology");
                UpdateEffect("MorphologyEffect", MorphologyEffect, heightMorphology);
            }
        }
        public bool morphologyEffect = false;
        public bool MorphologyEffect
        {
            set
            {
                morphologyEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("MorphologyEffect", morphologyEffect);
                RaisePropertyChanged("MorphologyEffect");
                UpdateEffect("MorphologyEffect", MorphologyEffect, HeightMorphology);
            }
            get
            {
                return morphologyEffect;
            }
        }
        #endregion

        //SaturationEffect
        #region Saturation Effect
        public double saturation = 1.0;
        public double Saturation
        {
            get
            {
                return saturation;
            }
            set
            {
                saturation = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Saturation", saturation);
                RaisePropertyChanged("Saturation");
                UpdateEffect("SaturationEffect", SaturationEffect, saturation);
            }
        }
        public bool saturationEffect = false;
        public bool SaturationEffect
        {
            set
            {
                saturationEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("SaturationEffect", saturationEffect);
                RaisePropertyChanged("SaturationEffect");
                UpdateEffect("SaturationEffect", SaturationEffect, Saturation);
            }
            get
            {
                return saturationEffect;
            }
        }
        #endregion

        //ScaleEffect
        #region Scale Effect
        public double widthScale = 1.0;
        public double WidthScale
        {
            get
            {
                return widthScale;
            }
            set
            {
                widthScale = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("WidthScale", widthScale);
                RaisePropertyChanged("WidthScale");
                UpdateEffect("ScaleEffect", ScaleEffect, widthScale, HeightScale, SharpnessScale);
            }
        }
        public double heightScale = 1.0;
        public double HeightScale
        {
            get
            {
                return heightScale;
            }
            set
            {
                heightScale = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("HeightScale", heightScale);
                RaisePropertyChanged("HeightScale");
                UpdateEffect("ScaleEffect", ScaleEffect, WidthScale, heightScale, SharpnessScale);
            }
        }
        public double sharpnessScale = 0.0;
        public double SharpnessScale
        {
            get
            {
                return sharpnessScale;
            }
            set
            {
                sharpnessScale = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("SharpnessScale", sharpnessScale);
                RaisePropertyChanged("SharpnessScale");
                UpdateEffect("ScaleEffect", ScaleEffect, WidthScale, HeightScale, sharpnessScale);
            }
        }
        public bool scaleEffect = false;
        public bool ScaleEffect
        {
            set
            {
                scaleEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("ScaleEffect", scaleEffect);
                RaisePropertyChanged("ScaleEffect");
                UpdateEffect("ScaleEffect", ScaleEffect, WidthScale, HeightScale, SharpnessScale);
            }
            get
            {
                return scaleEffect;
            }
        }
        #endregion

        //PosterizeEffect
        #region Posterize Effect
        public double red = 4.0;
        public double Red
        {
            get
            {
                return red;
            }
            set
            {
                red = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Red", red);
                RaisePropertyChanged("Red");
                UpdateEffect("PosterizeEffect", PosterizeEffect, red, Green, Blue);
            }
        }
        public double green = 4.0;
        public double Green
        {
            get
            {
                return green;
            }
            set
            {
                green = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Green", green);
                RaisePropertyChanged("Green");
                UpdateEffect("PosterizeEffect", PosterizeEffect, Red, green, Blue);
            }
        }
        public double blue = 4.0;
        public double Blue
        {
            get
            {
                return blue;
            }
            set
            {
                blue = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Blue", blue);
                RaisePropertyChanged("Blue");
                UpdateEffect("PosterizeEffect", PosterizeEffect, Red, Green, blue);
            }
        }
        public bool posterizeEffect = false;
        public bool PosterizeEffect
        {
            set
            {
                posterizeEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("PosterizeEffect", posterizeEffect);
                RaisePropertyChanged("PosterizeEffect");
                UpdateEffect("PosterizeEffect", PosterizeEffect, Red, Green, Blue);
            }
            get
            {
                return posterizeEffect;
            }
        }
        #endregion

        //HighlightsAndShadowsEffect
        #region Highlights AndShadows Effect
        public double clarity = 0.0;
        public double Clarity
        {
            get
            {
                return clarity;
            }
            set
            {
                clarity = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Clarity", clarity);
                RaisePropertyChanged("Clarity");
                UpdateEffect("HighlightsAndShadowsEffect", HighlightsAndShadowsEffect, clarity, Highlights, Shadows);
            }
        }
        public double highlights = 0.0;
        public double Highlights
        {
            get
            {
                return highlights;
            }
            set
            {
                highlights = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Highlights", highlights);
                RaisePropertyChanged("Highlights");
                UpdateEffect("HighlightsAndShadowsEffect", HighlightsAndShadowsEffect, Clarity, highlights, Shadows);
            }
        }
        public double shadows = 0.0;
        public double Shadows
        {
            get
            {
                return shadows;
            }
            set
            {
                shadows = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Shadows", shadows);
                RaisePropertyChanged("Shadows");
                UpdateEffect("HighlightsAndShadowsEffect", HighlightsAndShadowsEffect, Clarity, Highlights, shadows);
            }
        }
        public bool highlightsAndShadowsEffect = false;
        public bool HighlightsAndShadowsEffect
        {
            set
            {
                highlightsAndShadowsEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("HighlightsAndShadowsEffect", highlightsAndShadowsEffect);
                RaisePropertyChanged("HighlightsAndShadowsEffect");
                UpdateEffect("HighlightsAndShadowsEffect", HighlightsAndShadowsEffect, Clarity, Highlights, Shadows);
            }
            get
            {
                return highlightsAndShadowsEffect;
            }
        }
        #endregion

        //GaussianBlurEffect
        #region Gaussian Blur Effect
        public double blurAmountGaussianBlur = 1.0;
        public double BlurAmountGaussianBlur
        {
            get
            {
                return blurAmountGaussianBlur;
            }
            set
            {
                blurAmountGaussianBlur = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("BlurAmountGaussianBlur", blurAmountGaussianBlur);
                RaisePropertyChanged("BlurAmountGaussianBlur");
                UpdateEffect("GaussianBlurEffect", GaussianBlurEffect, blurAmountGaussianBlur);
            }
        }
        public bool gaussianBlurEffect = false;
        public bool GaussianBlurEffect
        {
            set
            {
                gaussianBlurEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GaussianBlurEffect", gaussianBlurEffect);
                RaisePropertyChanged("GaussianBlurEffect");
                UpdateEffect("GaussianBlurEffect", GaussianBlurEffect, BlurAmountGaussianBlur);
            }
            get
            {
                return gaussianBlurEffect;
            }
        }
        #endregion

        //GrayscaleEffect
        #region Grayscale Effect
        public bool grayscaleEffect = false;
        public bool GrayscaleEffect
        {
            set
            {
                grayscaleEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GrayscaleEffect", grayscaleEffect);
                RaisePropertyChanged("GaussianBlurEffect");
                UpdateEffect("GrayscaleEffect", GrayscaleEffect);
            }
            get
            {
                return grayscaleEffect;
            }
        }
        #endregion

        //RgbToHueEffect
        #region Rgb To Hue Effect
        public bool rgbToHueEffect = false;
        public bool RgbToHueEffect
        {
            set
            {
                rgbToHueEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("RgbToHueEffect", rgbToHueEffect);
                RaisePropertyChanged("RgbToHueEffect");
                UpdateEffect("RgbToHueEffect", RgbToHueEffect);
            }
            get
            {
                return rgbToHueEffect;
            }
        }
        #endregion

        //InvertEffect
        #region Invert Effect
        public bool invertEffect = false;
        public bool InvertEffect
        {
            set
            {
                invertEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("InvertEffect", invertEffect);
                RaisePropertyChanged("InvertEffect");
                UpdateEffect("InvertEffect", InvertEffect);
            }
            get
            {
                return invertEffect;
            }
        }
        #endregion

        //HueToRgbEffect
        #region Hue To Rgb Effect
        public bool hueToRgbEffect = false;
        public bool HueToRgbEffect
        {
            set
            {
                hueToRgbEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("HueToRgbEffect", hueToRgbEffect);
                RaisePropertyChanged("HueToRgbEffect");
                UpdateEffect("HueToRgbEffect", HueToRgbEffect);
            }
            get
            {
                return hueToRgbEffect;
            }
        }
        #endregion

        //DirectionalBlurEffect
        #region Directional Blur Effect
        public double blurAmount = 3;
        public double BlurAmount
        {
            get
            {
                return blurAmount;
            }
            set
            {
                blurAmount = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("BlurAmount", BlurAmount);
                RaisePropertyChanged("BlurAmount");
                UpdateEffect("DirectionalBlurEffect", DirectionalBlurEffect, blurAmount, Angle);
            }
        }
        public double angle = 0;
        public double Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("Angle", angle);
                RaisePropertyChanged("Angle");
                UpdateEffect("DirectionalBlurEffect", DirectionalBlurEffect, BlurAmount, angle);
            }
        }
        public bool directionalBlurEffect = false;
        public bool DirectionalBlurEffect
        {
            set
            {
                directionalBlurEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("DirectionalBlurEffect", directionalBlurEffect);
                RaisePropertyChanged("DirectionalBlurEffect");
                UpdateEffect("DirectionalBlurEffect", DirectionalBlurEffect, BlurAmount, Angle);
            }
            get
            {
                return directionalBlurEffect;
            }
        }
        #endregion

        //EmbossEffect
        #region Emboss Effect
        public double amountEmboss = 1;
        public double AmountEmboss
        {
            get
            {
                return amountEmboss;
            }
            set
            {
                amountEmboss = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("AmountEmboss", AmountEmboss);
                RaisePropertyChanged("EmbossEffect");
                UpdateEffect("EmbossEffect", EmbossEffect, amountEmboss, AngleEmboss);
            }
        }
        public double angleEmboss = 0;
        public double AngleEmboss
        {
            get
            {
                return angleEmboss;
            }
            set
            {
                angleEmboss = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("AngleEmboss", angleEmboss);
                RaisePropertyChanged("AngleEmboss");
                UpdateEffect("EmbossEffect", EmbossEffect, AmountEmboss, angleEmboss);
            }
        }
        public bool embossEffect = false;
        public bool EmbossEffect
        {
            set
            {
                embossEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("EmbossEffect", embossEffect);
                RaisePropertyChanged("EmbossEffect");
                UpdateEffect("EmbossEffect", EmbossEffect, AmountEmboss, AngleEmboss);
            }
            get
            {
                return embossEffect;
            }
        }
        #endregion

        //EdgeDetectionEffect
        #region Edge Detection Effect 
        public double blurAmountEdge = 0;
        public double BlurAmountEdge
        {
            get
            {
                return blurAmountEdge;
            }
            set
            {
                blurAmountEdge = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("BlurAmountEdge", BlurAmountEdge);
                RaisePropertyChanged("BlurAmountEdge");
                UpdateEffect("EdgeDetectionEffect", EdgeDetectionEffect, AmountEdge, blurAmountEdge);
            }
        }
        public double amountEdge = 0.5;
        public double AmountEdge
        {
            get
            {
                return amountEdge;
            }
            set
            {
                amountEdge = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("AmountEdge", amountEdge);
                RaisePropertyChanged("AmountEdge");
                UpdateEffect("EdgeDetectionEffect", EdgeDetectionEffect, AmountEdge, blurAmountEdge);
            }
        }
        public bool edgeDetectionEffect = false;
        public bool EdgeDetectionEffect
        {
            set
            {
                edgeDetectionEffect = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("EdgeDetectionEffect", edgeDetectionEffect);
                RaisePropertyChanged("EdgeDetectionEffect");
                UpdateEffect("EdgeDetectionEffect", edgeDetectionEffect, AmountEdge, blurAmountEdge);
            }
            get
            {
                return edgeDetectionEffect;
            }
        }
        #endregion

        private void SyncEffectsSettings()
        {
            try
            {

                var effectsOrderString = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("effectsOrderHistory", "");
                if (effectsOrderString.Length > 0)
                {
                    effectsOrderHistory = JsonConvert.DeserializeObject<Dictionary<string, int>>(effectsOrderString);
                }
            }
            catch (Exception ex)
            {

            }
            isEffectsInitial = true;
            try
            {
                //BrightnessEffect
                BrightnessEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BrightnessEffect", false);
                BrightnessLevel = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BrightnessLevel", brightnessLevel);

                //ContrastEffect
                ContrastEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ContrastEffect", false);
                ContrastLevel = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ContrastLevel", contrastLevel);

                //DirectionalBlurEffect
                DirectionalBlurEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("DirectionalBlurEffect", false);
                BlurAmount = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BlurAmount", blurAmount);
                Angle = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Angle", angle);

                //EdgeDetectionEffect
                EdgeDetectionEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("EdgeDetectionEffect", false);
                AmountEdge = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AmountEdge", amountEdge);
                EdgeDetectionEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("EdgeDetectionEffect", edgeDetectionEffect);

                //EmbossEffect
                EmbossEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("EmbossEffect", false);
                AmountEmboss = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AmountEmboss", amountEmboss);
                AngleEmboss = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AngleEmboss", angleEmboss);

                //ExposureEffect
                ExposureEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ExposureEffect", false);
                Exposure = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Exposure", exposure);

                //GaussianBlurEffect
                GaussianBlurEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("GaussianBlurEffect", false);
                BlurAmountGaussianBlur = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BlurAmountGaussianBlur", blurAmountGaussianBlur);

                //SaturationEffect
                SaturationEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("SaturationEffect", false);
                Saturation = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Saturation", saturation);

                //SepiaEffect
                SepiaEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("SepiaEffect", false);
                Intensity = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Intensity", intensity);

                //Transform3DEffect
                Transform3DEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Transform3DEffect", false);
                Rotate = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Rotate", rotate);
                RotateX = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RotateX", rotateX);
                RotateY = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RotateY", rotateY);

                //SharpenEffect
                SharpenEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("SharpenEffect", false);
                AmountSharpen = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AmountSharpen", amountSharpen);

                //StraightenEffect
                StraightenEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("StraightenEffect", false);
                AngleStraighten = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AngleStraighten", angleStraighten);

                //VignetteEffect
                VignetteEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("VignetteEffect", false);
                AmountVignette = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AmountVignette", amountVignette);
                Curve = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Curve", curve);

                //GrayscaleEffect
                GrayscaleEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("GrayscaleEffect", false);

                //HueToRgbEffect 
                HueToRgbEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("HueToRgbEffect", false);

                //InvertEffect 
                InvertEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("InvertEffect", false);

                //RgbToHueEffect 
                RgbToHueEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RgbToHueEffect", false);

                //HighlightsAndShadowsEffect 
                HighlightsAndShadowsEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("HighlightsAndShadowsEffect", false);
                Clarity = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Clarity", clarity);
                Highlights = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Highlights", highlights);
                Shadows = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Shadows", shadows);

                //PosterizeEffect 
                PosterizeEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("PosterizeEffect", false);
                Red = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Red", red);
                Green = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Green", green);
                Blue = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Blue", blue);

                //ScaleEffect 
                ScaleEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ScaleEffect", false);
                WidthScale = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("WidthScale", widthScale);
                HeightScale = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("HeightScale", heightScale);
                SharpnessScale = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("SharpnessScale", sharpnessScale);

                //TemperatureAndTintEffect 
                TemperatureAndTintEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("TemperatureAndTintEffect", false);
                Temperature = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Temperature", temperature);
                Tint = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Tint", tint);

                //TileEffect 
                TileEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("TileEffect", false);
                Left = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Left", left);
                Top = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Top", top);
                Right = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Right", right);
                Bottom = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Bottom", bottom);

                //CropEffect 
                CropEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("CropEffect", false);
                LeftCrop = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("LeftCrop", leftCrop);
                TopCrop = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("TopCrop", topCrop);
                RightCrop = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RightCrop", rightCrop);
                BottomCrop = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BottomCrop", bottomCrop);

            }
            catch (Exception ex)
            {

            }
            isEffectsInitial = false;
        }
        private void ClearAllEffectsCall()
        {
            BrightnessEffect = false;
            ContrastEffect = false;
            DirectionalBlurEffect = false;
            AddShaders = false;
            AddOverlays = false;
            EmbossEffect = false;
            ExposureEffect = false;
            GaussianBlurEffect = false;
            GrayscaleEffect = false;
            HueToRgbEffect = false;
            InvertEffect = false;
            HighlightsAndShadowsEffect = false;
            PosterizeEffect = false;
            RgbToHueEffect = false;
            SaturationEffect = false;
            ScaleEffect = false;
            SepiaEffect = false;
            SharpenEffect = false;
            StraightenEffect = false;
            TemperatureAndTintEffect = false;
            TileEffect = false;
            CropEffect = false;
            VignetteEffect = false;
            Transform3DEffect = false;

            PlatformService.PlayNotificationSound("success.wav");
            UpdateInfoState("All effects cleared");
        }

        bool isEffectsInitial = false;
        Dictionary<string, int> effectsOrderHistory = new Dictionary<string, int>();
        private void UpdateEffect(string EffectName, bool EffectState, double EffectValue1 = 0, double EffectValue2 = 0, double EffectValue3 = 0, double EffectValue4 = 0)
        {
            var ForceOrder = -1;
            if (!EffectState)
            {
                var testOrder = -1;
                if (effectsOrderHistory.TryGetValue(EffectName, out testOrder))
                {
                    effectsOrderHistory.Remove(EffectName);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("effectsOrderHistory", JsonConvert.SerializeObject(effectsOrderHistory));
                }
            }
            else if (isEffectsInitial)
            {
                var testOrder = -1;
                if (effectsOrderHistory.TryGetValue(EffectName, out testOrder))
                {
                    ForceOrder = testOrder;
                }
            }
            var effectOrder = VideoService.SetEffect(EffectName, EffectState, EffectValue1, EffectValue2, EffectValue3, EffectValue4, ForceOrder);
            if (effectOrder != -1)
            {
                var testOrder = -1;
                if (!effectsOrderHistory.TryGetValue(EffectName, out testOrder))
                {
                    effectsOrderHistory.Add(EffectName, effectOrder);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("effectsOrderHistory", JsonConvert.SerializeObject(effectsOrderHistory));
                }
            }
            if (EffectState)
            {
                forceReloadLogsList = true;
            }
        }

        private void UpdateEffect(string EffectName, bool EffectState, List<byte[]> EffectValue1)
        {
            VideoService.SetEffect(EffectName, EffectState, EffectValue1);
            if (EffectState)
            {
                forceReloadLogsList = true;
            }
        }

        public IMvxCommand ShowAllEffects { get; }
        public IMvxCommand ClearAllEffects { get; }

        //Effects System



        private const string ForceDisplayTouchGamepadKey = "ForceDisplayTouchGamepad";
        private const string CurrentFilterKey = "CurrentFilter";
        public int FitScreen = 1;
        public int ScreenRow = 1;
        public int ControlsAreaHeight = 285;
        public bool ControlsVisible = false;
        public bool AudioOnly = false;
        public bool VideoOnly = false;
        public bool AudioLowLevel = false;
        public bool AudioMediumLevel = false;
        public bool AudioNormalLevel = true;
        public bool AudioHighLevel = false;
        public bool AudioMuteLevel = false;
        public bool ActionsGridVisiblity = false;
        public bool SystemInfoVisiblity = false;
        public bool TabSoundEffect = false;
        public bool SensorsMovement = false;
        public bool UseAnalogDirections = true;
        public bool SensorsMovementActive = false;
        public bool ShowSensorsInfo = false;

        public bool rCore1;
        public bool RCore1
        {
            get
            {
                return rCore1;
            }
            set
            {
                rCore1 = value;
                if (value)
                {
                    RCore2 = false;
                    RCore4 = false;
                    RCore6 = false;
                    RCore8 = false;
                    RCore12 = false;
                    RCore20 = false;
                    RCore32 = false;
                }
            }
        }
        public bool rCore2;
        public bool RCore2
        {
            get
            {
                return rCore2;
            }
            set
            {
                rCore2 = value;
                if (value)
                {
                    RCore1 = false;
                    RCore4 = false;
                    RCore6 = false;
                    RCore8 = false;
                    RCore12 = false;
                    RCore20 = false;
                    RCore32 = false;
                }
            }
        }

        public bool rCore4;
        public bool RCore4
        {
            get
            {
                return rCore4;
            }
            set
            {
                rCore4 = value;
                if (value)
                {
                    RCore2 = false;
                    RCore1 = false;
                    RCore6 = false;
                    RCore8 = false;
                    RCore12 = false;
                    RCore20 = false;
                    RCore32 = false;
                }
            }
        }

        public bool rCore6;
        public bool RCore6
        {
            get
            {
                return rCore6;
            }
            set
            {
                rCore6 = value;
                if (value)
                {
                    RCore2 = false;
                    RCore4 = false;
                    RCore1 = false;
                    RCore8 = false;
                    RCore12 = false;
                    RCore20 = false;
                    RCore32 = false;
                }
            }
        }

        public bool rCore8;
        public bool RCore8
        {
            get
            {
                return rCore8;
            }
            set
            {
                rCore8 = value;
                if (value)
                {
                    RCore2 = false;
                    RCore4 = false;
                    RCore6 = false;
                    RCore1 = false;
                    RCore12 = false;
                    RCore20 = false;
                    RCore32 = false;
                }
            }
        }

        public bool rCore12;
        public bool RCore12
        {
            get
            {
                return rCore12;
            }
            set
            {
                rCore12 = value;
                if (value)
                {
                    RCore2 = false;
                    RCore4 = false;
                    RCore6 = false;
                    RCore8 = false;
                    RCore1 = false;
                    RCore20 = false;
                    RCore32 = false;
                }
            }
        }
        public bool rCore20;
        public bool RCore20
        {
            get
            {
                return rCore20;
            }
            set
            {
                rCore20 = value;
                if (value)
                {
                    RCore2 = false;
                    RCore4 = false;
                    RCore6 = false;
                    RCore8 = false;
                    RCore1 = false;
                    RCore12 = false;
                    RCore32 = false;
                }
            }
        }
        public bool rCore32;
        public bool RCore32
        {
            get
            {
                return rCore32;
            }
            set
            {
                rCore32 = value;
                if (value)
                {
                    RCore2 = false;
                    RCore4 = false;
                    RCore6 = false;
                    RCore8 = false;
                    RCore1 = false;
                    RCore12 = false;
                    RCore20 = false;
                }
            }
        }
        public void SetSensorMovementActive()
        {
            SensorsMovementActive = true;
            RaisePropertyChanged(nameof(SensorsMovementActive));
        }
        public bool ColorFilterNone = true;
        public bool ColorFilterGrayscale = false;
        public bool ColorFilterRetro = false;
        public string PreviewCurrentInfo = "";
        public bool PreviewCurrentInfoState = false;
        private string CurrentActionsSet = "S01";
        public string PreviewButtonsSet = "Press on the buttons to record actions..";
        public int ActionsDelay = 150;
        public bool ActionsDelay1 = false;
        public bool ActionsDelay2 = true;
        public bool ActionsDelay3 = false;
        public bool ActionsDelay4 = false;
        public bool ActionsDelay5 = false;
        public bool ShowSpecialButtons = true;
        public bool ShowActionsButtons = true;
        public bool ActionsCustomDelay = false;
        public string ActionsSaveLocation = "SaveActions";
        public string SlotsSaveLocation = "SaveStates";
        public string TouchPadSaveLocation = "SaveTouchPad";
        public Dictionary<string, Dictionary<string[], InjectedInputTypes>> ButtonsDictionary = new Dictionary<string, Dictionary<string[], InjectedInputTypes>>();
        public Dictionary<string, Dictionary<string[], InjectedInputTypes>> ButtonsDictionaryTemp = new Dictionary<string, Dictionary<string[], InjectedInputTypes>>();
        public bool ReverseLeftRight = false;
        public bool ActionsToSave = false;
        private bool ActionsCalled = false;
        public bool ShowFPSCounter = false;
        public bool ShowBufferCounter = false;
        public string FPSCounter = "-";
        public bool NearestNeighbor = true;
        public bool Anisotropic = false;
        public bool Cubic = false;
        public bool HighQualityCubic = false;
        public bool Linear = false;
        public bool MultiSampleLinear = false;
        public bool Aliased = false;
        public bool CoreOptionsVisible = false;
        public bool ControlsMapVisible = false;
        public ObservableCollection<string> LogsList = new ObservableCollection<string>();
        public bool ShowLogsList = false;
        public bool GameStopInProgress = false;
        public bool FPSInProgress = false;
        public bool LogInProgress = false;
        public bool AutoSave = true;
        public bool AutoSave15Sec = false;
        public bool AutoSave30Sec = false;
        public bool AutoSave60Sec = false;
        public bool AutoSave90Sec = false;
        public bool AutoSaveNotify = false;
        public bool InGameOptionsActive = true;
        public bool RotateDegreePlusActive = false;
        public bool RotateDegreeMinusActive = false;
        public bool ShowXYZ = true;
        public bool IsSegaSystem = false;
        public bool ShowL2R2Controls = false;
        public bool AudioEcho = false;
        public bool AudioReverb = false;
        public bool ButtonsIsLoading = true;
        public int RotateDegree = 0;
        private Timer FPSTimer, LogTimer, InfoTimer, AutoSaveTimer, BufferTimer, GCTimer, XBoxModeTimer;
        private long StartTimeStamp = 0;
        public bool ScaleFactorVisible = false;
        public bool ButtonsCustomization = false;
        private float leftScaleFactorValueP = 1f;
        private float leftScaleFactorValueW = 1f;
        private float rightScaleFactorValueP = 1f;
        private float rightScaleFactorValueW = 1f;
        public double RightTransformXDefault = 0.0;
        public double RightTransformYDefault = 0.0;
        public double LeftTransformXDefault = 0.0;
        public double LeftTransformYDefault = 0.0;
        public double ActionsTransformXDefault = 0.0;
        public double ActionsTransformYDefault = 0.0;
        public double RightTransformXCurrent { get { return getRightTransformX(); } set { setRightTransformX(value); } }
        public double RightTransformYCurrent { get { return getRightTransformY(); } set { setRightTransformY(value); } }
        public double LeftTransformXCurrent { get { return getLeftTransformX(); } set { setLeftTransformX(value); } }
        public double LeftTransformYCurrent { get { return getLeftTransformY(); } set { setLeftTransformY(value); } }
        public double ActionsTransformXCurrent { get { return getActionsTransformX(); } set { setActionsTransformX(value); } }
        public double ActionsTransformYCurrent { get { return getActionsTransformY(); } set { setActionsTransformY(value); } }
        public double rightTransformXCurrentP = 0;
        public double rightTransformYCurrentP = 0;
        public double leftTransformXCurrentP = 0;
        public double leftTransformYCurrentP = 0;
        public double actionsTransformXCurrentP = 0;
        public double actionsTransformYCurrentP = 0;
        public float LeftScaleFactorValue { get { return getLeftScaleFactor(); } set { saveLeftScaleFactorValue(value); } }
        public float RightScaleFactorValue { get { return getRightScaleFactor(); } set { saveRightScaleFactorValue(value); } }
        public bool CustomConsoleEditMode = false;

        public void setCustomConsoleEditMode(bool state)
        {
            CustomConsoleEditMode = state;
            RaisePropertyChanged(nameof(CustomConsoleEditMode));
        }
        public void saveLeftScaleFactorValue(float value)
        {
            leftScaleFactorValueP = value;
            if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(leftScaleFactorValueP), value);

            RaisePropertyChanged(nameof(LeftScaleFactorValue));
        }
        public void saveRightScaleFactorValue(float value)
        {
            rightScaleFactorValueP = value;
            if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(rightScaleFactorValueP), value);

            RaisePropertyChanged(nameof(RightScaleFactorValue));
        }

        public float getLeftScaleFactor()
        {
            return leftScaleFactorValueP;
        }
        public float getRightScaleFactor()
        {
            return rightScaleFactorValueP;
        }

        /************** X,Y ************/
        public void setRightTransformX(double value)
        {
            rightTransformXCurrentP = value;
            if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(rightTransformXCurrentP), value);

            RaisePropertyChanged(nameof(RightTransformXCurrent));
        }
        public double getRightTransformX()
        {
            return rightTransformXCurrentP;
        }

        public void setRightTransformY(double value)
        {
            rightTransformYCurrentP = value;
            if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(rightTransformYCurrentP), value);

            RaisePropertyChanged(nameof(RightTransformYCurrent));
        }
        public double getRightTransformY()
        {
            return rightTransformYCurrentP;
        }


        public void setLeftTransformX(double value)
        {
            leftTransformXCurrentP = value;
            if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(leftTransformXCurrentP), value);

            RaisePropertyChanged(nameof(LeftTransformXCurrent));
        }
        public double getLeftTransformX()
        {
            return leftTransformXCurrentP;
        }

        public void setLeftTransformY(double value)
        {
            leftTransformYCurrentP = value;
            if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(leftTransformYCurrentP), value);

            RaisePropertyChanged(nameof(LeftTransformYCurrent));
        }
        public double getLeftTransformY()
        {
            return leftTransformYCurrentP;
        }



        public void setActionsTransformX(double value)
        {
            actionsTransformXCurrentP = value;
            if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(actionsTransformXCurrentP), value);

            RaisePropertyChanged(nameof(ActionsTransformXCurrent));
        }
        public double getActionsTransformX()
        {
            return actionsTransformXCurrentP;
        }

        public void setActionsTransformY(double value)
        {
            actionsTransformYCurrentP = value;
            if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(actionsTransformYCurrentP), value);

            RaisePropertyChanged(nameof(ActionsTransformYCurrent));
        }
        public double getActionsTransformY()
        {
            return actionsTransformYCurrentP;
        }
        /************** X,Y ************/



        public async void AutoSaveManager(object sender, EventArgs e)
        {
            if (GameStopInProgress || GameIsPaused || EmulationService.CorePaused || !isGameStarted || FailedToLoadGame || AutoSaveWorkerInProgress)
            {
                return;
            }
            if (AutoSave15Sec || AutoSave30Sec || AutoSave60Sec || AutoSave90Sec)
            {
                Dispatcher.RequestMainThreadAction(() => AutoSaveWorker());
            }
        }

        public async void UpdateFPSCounter(object sender, EventArgs e)
        {
            try
            {
                if (!GameStopInProgress)
                {
                    if (ShowFPSCounter && !FPSInProgress && !FPSErrorCatched)
                    {
                        Dispatcher.RequestMainThreadAction(() => updateFPSCaller());
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService?.ShowErrorMessage(ex);
            }
        }

        public string txtMemory;
        bool ReduceFreezesInProgress = false;
        public async void UpdateGC(object sender, EventArgs e)
        {
            try
            {
                if (!GameStopInProgress)
                {
                    if (ReduceFreezes && !ReduceFreezesInProgress && AudioService != null)
                    {
                        Dispatcher.RequestMainThreadAction(() => updateGCCaller());
                    }
                    if (ShowSensorsInfo)
                    {
                        try
                        {
                            txtMemory = PlatformService.GetMemoryUsage();
                            RaisePropertyChanged(nameof(txtMemory));
                        }
                        catch (Exception ex)
                        {
                            txtMemory = ex.Message;
                            RaisePropertyChanged(nameof(txtMemory));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService?.ShowErrorMessage(ex);
            }
        }
        bool NoGCRegionState = false;
        private async void updateGCCaller()
        {
            ReduceFreezesInProgress = true;

            try
            {
                if (!NoGCRegionState)
                {
                    GC.WaitForPendingFinalizers();
                    AudioService.TryStartNoGCRegionCall();
                    NoGCRegionState = true;
                    await Task.Delay(1000);
                }
                else
                {
                    AudioService.EndNoGCRegionCall();
                    NoGCRegionState = false;
                }
            }
            catch (Exception e)
            {
                NoGCRegionState = false;
            }

            ReduceFreezesInProgress = false;
        }

        public async void UpdateXBoxMode(object sender, EventArgs e)
        {
            try
            {
                if (!GameStopInProgress)
                {
                    CheckXBoxMode();
                }

            }
            catch (Exception ex)
            {
                PlatformService?.ShowErrorMessage(ex);
            }
        }

        bool BufferInProgress = false;
        bool BufferErrorCatched = false;
        public async void UpdateBufferCounter(object sender, EventArgs e)
        {
            try
            {
                if (!GameStopInProgress)
                {
                    if (ShowBufferCounter && !BufferInProgress && !BufferErrorCatched)
                    {
                        Dispatcher.RequestMainThreadAction(() => updateBufferCaller());
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService?.ShowErrorMessage(ex);
            }
        }
        public async void UpdateLogList(object sender, EventArgs e)
        {
            try
            {
                if (!GameStopInProgress)
                {
                    if ((ShowLogsList || forceReloadLogsList) && !LogInProgress && !LogErrorCatched)
                    {
                        Dispatcher.RequestMainThreadAction(() => updateLogListCaller());
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService?.ShowErrorMessage(ex);
            }
        }


        bool AutoSaveWorkerInProgress = false;
        async void AutoSaveWorker()
        {
            try
            {
                AutoSaveWorkerInProgress = true;
                await AutoSaveState(AutoSaveNotify);
            }
            catch (Exception ex)
            {
                PlatformService?.ShowErrorMessage(ex);
            }
            AutoSaveWorkerInProgress = false;
        }

        private int LogListSizeTemp = 0;
        public bool FPSErrorCatched = false;
        public bool LogErrorCatched = false;
        public bool forceReloadLogsList = false;
        void updateLogListCaller()
        {
            LogInProgress = true;
            try
            {
                if (EmulationService != null && LogsList != null && (ShowLogsList || forceReloadLogsList) && !GameStopInProgress && !LogErrorCatched)
                {
                    lock (LogsList)
                    {
                        var LogsListContent = EmulationService.GetCoreLogsList()?.ToList();
                        if ((LogsListContent != null && (LogsListContent.Count > LogListSizeTemp || forceReloadLogsList)))
                        {
                            LogsList.Clear();
                            foreach (var LogsListContentItem in LogsListContent)
                            {
                                LogsList.Add(LogsListContentItem);
                            }
                            try
                            {
                                var PixelsType = "Pixels Type: " + (FramebufferConverter.isRGB888 ? "XRGB8888" : (FramebufferConverter.isRG565 ? "RGB565" : "RGB555"));
                                var UsingMemoryCopy = "Memory Helper: " + (FramebufferConverter.isRGB888 ? (FramebufferConverter.MemoryHelper + (FramebufferConverter.SkipCached ? " (Ignored due Pixels updates feature)" : "")) : "Memory Pointers");
                                var RenderCores = "Render Cores: " + $"{FramebufferConverter.CoresCount}";
                                var CurrentSize = "Resolution: " + $"{FramebufferConverter.currentWidth} x {FramebufferConverter.currentHeight}";
                                var CrazyBufferState = "Crazy Buffer: " + (CrazyBufferActive ? $"{FramebufferConverter.crazyBufferPercentageHandle}% Handled":"OFF") + (CrazyBufferActive && FramebufferConverter.SkipCached ? " (Ignored due Pixels updates feature)" : "");

                                LogsList.Insert(1, PixelsType);
                                LogsList.Insert(2, UsingMemoryCopy);
                                LogsList.Insert(3, RenderCores);
                                LogsList.Insert(4, CurrentSize);
                                LogsList.Insert(5, CrazyBufferState);
                                if (VideoService != null && VideoService.TotalEffects() > 0)
                                {
                                    var EffectsApplied = $"Active Effects: {VideoService.TotalEffects()} Effect{(VideoService.TotalEffects() > 1 ? "s" : "")}";
                                    LogsList.Insert(6, EffectsApplied);
                                    LogsList.Insert(7, "--------------");
                                }
                                else
                                {
                                    LogsList.Insert(6, "--------------");
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                            RaisePropertyChanged(nameof(LogsList));
                            
                            Interlocked.Exchange(ref LogListSizeTemp, LogsList.Count);
                        }
                        forceReloadLogsList = false;
                    }
                }
            }
            catch (Exception e)
            {
                LogErrorCatched = true;
                PlatformService?.ShowErrorMessage(e);
            }
            LogInProgress = false;
        }

        public int FPSCounterValue = 0;
        void updateFPSCaller()
        {
            FPSInProgress = true;
            try
            {
                if (ShowFPSCounter && !GameStopInProgress && !FPSErrorCatched)
                {
                    UpdateFPS();
                    if (FPSCounterValue > 0 && !EmulationService.CorePaused && FPSCounterValue < 100)
                    {
                        FPSCounter = (FPSCounterValue).ToString();
                        if (FramebufferConverter.SkipCached)
                        {
                            FPSCounter += $" [{FramebufferConverter.totalPercentageSkipped}% Cache]";
                        }
                        if (FramebufferConverter.renderFailedMessage.Length > 0)
                        {
                            FPSCounter += $" - {FramebufferConverter.renderFailedMessage}";
                        }
                    }
                    else
                    {
                        FPSCounter = "-";
                    }
                    RaisePropertyChanged(nameof(FPSCounter));
                }
            }
            catch (Exception e)
            {
                FPSErrorCatched = true;
                PlatformService?.ShowErrorMessage(e);
            }
            FPSInProgress = false;
        }

        public string BufferCounter = "-";
        void updateBufferCaller()
        {
            BufferInProgress = true;
            try
            {
                if (ShowBufferCounter && AudioService != null && !GameStopInProgress && !BufferErrorCatched)
                {
                    int BufferCounterValue = AudioService.GetSamplesBufferCount();
                    if (BufferCounterValue > 0)
                    {
                        int MaxSamples = AudioService.GetMaxSamplesBufferCount();
                        decimal BufferCounterValueDivision = Math.Round((BufferCounterValue * 100m) / MaxSamples);
                        BufferCounter = (BufferCounterValueDivision).ToString() + "%";
                    }
                    else
                    {
                        BufferCounter = "-";
                    }

                    if (AudioService.GetFrameFailedMessage().Length > 0)
                    {
                        UpdateInfoState(AudioService.GetFrameFailedMessage(), true);
                    }
                    if (VideoService != null && VideoService.TotalEffects() > 0)
                    {
                        if (BufferCounter.Equals("-"))
                        {
                            BufferCounter = $"[{VideoService.TotalEffects()} Effect{(VideoService.TotalEffects() > 1 ? "s" : "")}]";
                        }
                        else
                        {
                            BufferCounter += $"[{VideoService.TotalEffects()} Effect{(VideoService.TotalEffects() > 1 ? "s" : "")}]";
                        }
                    }
                    RaisePropertyChanged(nameof(BufferCounter));
                }
            }
            catch (Exception e)
            {
                BufferErrorCatched = true;
                PlatformService?.ShowErrorMessage(e);
            }
            BufferInProgress = false;
        }

        public void UpdateFPS()
        {
            try
            {
                if (ShowFPSCounter && !FPSErrorCatched)
                {
                    //Interlocked.Exchange(ref FPSCounterValue, EmulationService.GetFPSCounterValue());
                    Interlocked.Exchange(ref FPSCounterValue, VideoService.GetFrameRate());
                }
            }
            catch (Exception e)
            {
            }
        }
        public async Task ExcuteActionsAsync(int ActionNumber)
        {
            if (ActionsCalled)
            {
                return;
            }
            try
            {
                string ExecutedKeys = "";
                PlatformService.PlayNotificationSound("button-01.mp3");
                Dictionary<string[], InjectedInputTypes> ButtonsList = new Dictionary<string[], InjectedInputTypes>();
                if (ButtonsDictionary.TryGetValue("S0" + ActionNumber, out ButtonsList))
                {
                    int KeyIndexer = 0;
                    ActionsCalled = true;
                    bool FirstCall = true;
                    foreach (string[] InputsKeys in ButtonsList.Keys)
                    {
                        InjectedInputTypes TargetInputType = InjectedInputTypes.DeviceIdJoypadStart;
                        if (ButtonsList.TryGetValue(InputsKeys, out TargetInputType))
                        {
                            KeyIndexer++;
                            string KeyTitle = InputsKeys[1];
                            if (ReverseLeftRight)
                            {
                                switch (TargetInputType)
                                {
                                    case InjectedInputTypes.DeviceIdJoypadLeft:
                                        TargetInputType = InjectedInputTypes.DeviceIdJoypadRight;
                                        KeyTitle = KeyTitle.Replace("Left", "Right");
                                        break;
                                    case InjectedInputTypes.DeviceIdJoypadRight:
                                        TargetInputType = InjectedInputTypes.DeviceIdJoypadLeft;
                                        KeyTitle = KeyTitle.Replace("Right", "Left");
                                        break;
                                }
                            }

                            string CallType = InputsKeys[3];

                            switch (CallType)
                            {
                                case "+":
                                    ExecutedKeys += KeyTitle + " + ";
                                    break;
                                default:
                                    ExecutedKeys += KeyTitle + ", ";
                                    break;
                            }
                            UpdateInfoState(ExecutedKeys.Substring(0, (ExecutedKeys.Length > 0 ? ExecutedKeys.Length - 2 : ExecutedKeys.Length)), true);

                            await Task.Run(() => ExecuteInjectedCommand(TargetInputType, CallType, KeyTitle, KeyIndexer));
                            FirstCall = false;

                        }
                    }
                    callInfoTimer(true);
                    ActionsCalled = false;
                }
                else
                {
                    UpdateInfoState("Action " + ActionNumber + " is empty!");
                    PlatformService.PlayNotificationSound("faild.wav");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                ActionsCalled = false;
            }
        }
        public async Task ExecuteInjectedCommand(InjectedInputTypes TargetInputType, string CallType, string KeyTitle, int KeyIndex)
        {
            try
            {
                switch (CallType)
                {
                    case "+":

                        CallActionPlusHelper(TargetInputType, 20, 5);
                        await Task.Delay(0);
                        break;
                    default:

                        await CallActionPlusHelper(TargetInputType, 3, 5);
                        await Task.Delay(ActionsDelay);
                        break;
                }
                if (TabSoundEffect)
                {
                    PlatformService.PlayNotificationSound("button-04.mp3");
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        public async Task CallActionPlusHelper(InjectedInputTypes TargetInputType, int RepeatCount, int PressDelay)
        {
            try
            {
                for (int i = 0; i < RepeatCount; ++i)
                {
                    await Task.Delay(PressDelay);
                    InjectInputCommand.Execute(TargetInputType);
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public void AddActionButton(string ButtonKey, string ButtonTitle, InjectedInputTypes ButtonType, string delayCount)
        {
            try
            {
                if (ActionsGridVisiblity)
                {
                    Dictionary<string[], InjectedInputTypes> TargetActionsList;
                    string[] ButtonTitleKey = new string[] { ButtonKey, ButtonTitle, GetRandomString(), delayCount };
                    if (ButtonsDictionaryTemp.TryGetValue(CurrentActionsSet, out TargetActionsList))
                    {
                        TargetActionsList = TargetActionsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                        TargetActionsList.Add(ButtonTitleKey, ButtonType);
                        ButtonsDictionaryTemp[CurrentActionsSet] = TargetActionsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                    }
                    else
                    {
                        TargetActionsList = new Dictionary<string[], InjectedInputTypes>();
                        TargetActionsList.Add(ButtonTitleKey, ButtonType);
                        ButtonsDictionaryTemp.Add(CurrentActionsSet, TargetActionsList.ToDictionary(entry => entry.Key, entry => entry.Value));
                    }
                    ActionsCustomDelay = false;
                    RaisePropertyChanged(nameof(ActionsCustomDelay));
                    UpdateActionsPreviewSet();
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public void ResetActionsSet()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                ButtonsDictionaryTemp.Remove(CurrentActionsSet);
                ActionsCustomDelay = false;
                RaisePropertyChanged(nameof(ActionsCustomDelay));
                UpdateActionsPreviewSet();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public async void SaveActionsSet()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                ButtonsDictionary = ButtonsDictionaryTemp.ToDictionary(entry => entry.Key, entry => entry.Value);
                UpdateActionsPreviewSet();
                HideActionsGrid.Execute();
                await ActionsStoreAsync(ButtonsDictionary);
                PlatformService.PlayNotificationSound("save-state.wav");
                UpdateInfoState("Actions Saved");
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public void CancelActionsSet()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                ButtonsDictionaryTemp = ButtonsDictionary.ToDictionary(entry => entry.Key, entry => entry.Value);
                UpdateActionsPreviewSet();
                HideActionsGrid.Execute();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        private void UpdateActionsPreviewSet()
        {
            try
            {
                Dictionary<string[], InjectedInputTypes> TargetActionsList = new Dictionary<string[], InjectedInputTypes>();
                string ButtonsPreview = "";
                if (ButtonsDictionaryTemp.TryGetValue(CurrentActionsSet, out TargetActionsList))
                {
                    foreach (string[] ButtonTitle in TargetActionsList.Keys)
                    {
                        switch (ButtonTitle[3])
                        {
                            case "+":
                                ButtonsPreview += "[" + ButtonTitle[1] + "] + ";
                                break;
                            default:
                                ButtonsPreview += "[" + ButtonTitle[1] + "], ";
                                break;
                        }

                    }
                    PreviewButtonsSet = ButtonsPreview.Substring(0, ButtonsPreview.Length - 2);
                }
                else
                {
                    PreviewButtonsSet = "Press on the buttons to record actions..";
                }
                RaisePropertyChanged(nameof(PreviewButtonsSet));
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public void UpdateInfoState(string InfoMessage, bool KeepOnScreen = false)
        {
            try
            {
                //if (KeepOnScreen || !PlatformService.ShowNotification(InfoMessage, 2))
                {
                    PreviewCurrentInfoState = true;
                    PreviewCurrentInfo = InfoMessage;
                    RaisePropertyChanged(nameof(PreviewCurrentInfoState));
                    RaisePropertyChanged(nameof(PreviewCurrentInfo));
                }
                if (!KeepOnScreen) callInfoTimer(true);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public static string GetRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path;
        }
        public async Task ActionsStoreAsync(IDictionary<string, Dictionary<string[], InjectedInputTypes>> dictionary)
        {
            try
            {
                GameIsLoadingState(true);
                string GameID = EmulationService.GetGameID();
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(ActionsSaveLocation);
                if (localFolder == null)
                {
                    await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(ActionsSaveLocation);
                    localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(ActionsSaveLocation);
                }
                var StatesDirectory = await localFolder.GetDirectoryAsync(GameID);
                if (StatesDirectory == null)
                {
                    StatesDirectory = await localFolder.CreateDirectoryAsync(GameID);
                }
                var targetFileTest = await StatesDirectory.GetFileAsync("actions.xyz");
                if (targetFileTest != null)
                {
                    await targetFileTest.DeleteAsync();
                }
                var targetFile = await StatesDirectory.CreateFileAsync("actions.xyz");
                Dictionary<string, List<string[]>> dictionaryList = new Dictionary<string, List<string[]>>();
                foreach (string dictionaryKey in dictionary.Keys)
                {
                    dictionaryList.Add(dictionaryKey, new List<string[]>());
                    foreach (string[] dictionarySubKey in dictionary[dictionaryKey].Keys)
                    {
                        dictionaryList[dictionaryKey].Add(new string[] { dictionarySubKey[0], dictionarySubKey[1], dictionary[dictionaryKey][dictionarySubKey].ToString(), dictionarySubKey[3] });
                    }
                }
                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(dictionaryList));
                using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                {
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            GameIsLoadingState(false);
        }
        public async Task ActionsRetrieveAsync()
        {
            try
            {
                string GameID = EmulationService.GetGameID();
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(ActionsSaveLocation);
                if (localFolder == null)
                {
                    await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(ActionsSaveLocation);
                    localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(ActionsSaveLocation);
                }
                var StatesDirectory = await localFolder.GetDirectoryAsync(GameID);
                if (StatesDirectory != null)
                {
                    var targetFileTest = await StatesDirectory.GetFileAsync("actions.xyz");
                    if (targetFileTest != null)
                    {
                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var outStream = await targetFileTest.OpenAsync(FileAccess.Read))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string ActionsFileContent = unicode.GetString(result);
                        var dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, List<string[]>>>(ActionsFileContent);
                        ButtonsDictionaryTemp.Clear();
                        ButtonsDictionary.Clear();
                        foreach (string dictionaryKey in dictionaryList.Keys)
                        {
                            foreach (string[] dictionarySubKey in dictionaryList[dictionaryKey])
                            {
                                InjectedInputTypes injectedInputType = GetInputType(dictionarySubKey[2]);
                                Dictionary<string[], InjectedInputTypes> TargetActionsList;
                                string[] ButtonTitleKey = new string[] { dictionarySubKey[0], dictionarySubKey[1], GetRandomString(), dictionarySubKey[3] };
                                if (ButtonsDictionaryTemp.TryGetValue(dictionaryKey, out TargetActionsList))
                                {
                                    TargetActionsList = TargetActionsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                                    TargetActionsList.Add(ButtonTitleKey, injectedInputType);
                                    ButtonsDictionaryTemp[dictionaryKey] = TargetActionsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                                }
                                else
                                {
                                    TargetActionsList = new Dictionary<string[], InjectedInputTypes>();
                                    TargetActionsList.Add(ButtonTitleKey, injectedInputType);
                                    ButtonsDictionaryTemp.Add(dictionaryKey, TargetActionsList.ToDictionary(entry => entry.Key, entry => entry.Value));
                                }
                            }
                        }
                        ButtonsDictionary = ButtonsDictionaryTemp.ToDictionary(entry => entry.Key, entry => entry.Value);
                        UpdateActionsPreviewSet();
                        UpdateInfoState("Actions Restored");
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        InjectedInputTypes GetInputType(string InputTypeKey)
        {
            InjectedInputTypes TargetInputType = InjectedInputTypes.DeviceIdJoypadStart;
            switch (InputTypeKey)
            {
                case "DeviceIdJoypadA":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadA;
                    break;
                case "DeviceIdJoypadB":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadB;
                    break;
                case "DeviceIdJoypadC":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadC;
                    break;
                case "DeviceIdJoypadX":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadX;
                    break;
                case "DeviceIdJoypadY":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadY;
                    break;
                case "DeviceIdJoypadZ":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadZ;
                    break;
                case "DeviceIdJoypadR":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadR;
                    break;
                case "DeviceIdJoypadR2":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadR2;
                    break;
                case "DeviceIdJoypadL":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadL;
                    break;
                case "DeviceIdJoypadL2":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadL2;
                    break;
                case "DeviceIdJoypadUp":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadUp;
                    break;
                case "DeviceIdJoypadDown":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadDown;
                    break;
                case "DeviceIdJoypadLeft":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadLeft;
                    break;
                case "DeviceIdJoypadRight":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadRight;
                    break;
                case "DeviceIdJoypadStart":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadStart;
                    break;
                case "DeviceIdJoypadSelect":
                    TargetInputType = InjectedInputTypes.DeviceIdJoypadSelect;
                    break;
                case "DeviceIdPointerPressed":
                    TargetInputType = InjectedInputTypes.DeviceIdPointerPressed;
                    break;
            }
            return TargetInputType;
        }

        private IUserDialogs DialogsService { get; }
        private IMvxNavigationService NavigationService { get; }
        private IPlatformService PlatformService { get; }
        private IInputService InputService { get; }
        public IEmulationService EmulationService { get; }
        private IVideoService VideoService { get; }
        private IAudioService AudioService { get; }
        private ISettings Settings { get; }

        public IMvxCommand TappedCommand { get; }
        public IMvxCommand TappedCommand2 { get; }
        public IMvxCommand PointerMovedCommand { get; }
        public IMvxCommand PointerTabbedCommand { get; }
        public IMvxCommand ToggleFullScreenCommand { get; }

        public IMvxCommand TogglePauseCommand { get; }
        public IMvxCommand ResetCommand { get; }
        public IMvxCommand StopCommand { get; }

        public IMvxCommand SaveStateSlot1 { get; }
        public IMvxCommand SaveStateSlot2 { get; }
        public IMvxCommand SaveStateSlot3 { get; }
        public IMvxCommand SaveStateSlot4 { get; }
        public IMvxCommand SaveStateSlot5 { get; }
        public IMvxCommand SaveStateSlot6 { get; }
        public IMvxCommand SaveStateSlot7 { get; }
        public IMvxCommand SaveStateSlot8 { get; }
        public IMvxCommand SaveStateSlot9 { get; }
        public IMvxCommand SaveStateSlot10 { get; }

        public IMvxCommand LoadStateSlot1 { get; }
        public IMvxCommand LoadStateSlot2 { get; }
        public IMvxCommand LoadStateSlot3 { get; }
        public IMvxCommand LoadStateSlot4 { get; }
        public IMvxCommand LoadStateSlot5 { get; }
        public IMvxCommand LoadStateSlot6 { get; }
        public IMvxCommand LoadStateSlot7 { get; }
        public IMvxCommand LoadStateSlot8 { get; }
        public IMvxCommand LoadStateSlot9 { get; }
        public IMvxCommand LoadStateSlot10 { get; }
        public IMvxCommand ImportSavedSlots { get; }
        public IMvxCommand ExportSavedSlots { get; }
        public IMvxCommand ImportActionsSlots { get; }
        public IMvxCommand ExportActionsSlots { get; }
        public IMvxCommand HideLoader { get; }
        public IMvxCommand SetScreenFit { get; }
        public IMvxCommand SetScanlines1 { get; }
        public IMvxCommand SetScanlines2 { get; }
        public IMvxCommand SetScanlines3 { get; }
        public IMvxCommand SetDoublePixel { get; }
        public IMvxCommand SetSpeedup { get; }
        public IMvxCommand SetUpdatesOnly { get; }
        public IMvxCommand SetSkipCached { get; }
        public IMvxCommand SetSkipFrames { get; }
        public IMvxCommand SetSkipFramesRandom { get; }
        public IMvxCommand DontWaitThreads { get; }
        public IMvxCommand SetDelayFrames { get; }
        public IMvxCommand SetReduceFreezes { get; }
        public IMvxCommand SetCrazyBufferActive { get; }
        public IMvxCommand SetAudioOnly { get; }
        public IMvxCommand SetVideoOnly { get; }
        public IMvxCommand SetTabSoundEffect { get; }
        public IMvxCommand SetSensorsMovement { get; }
        public IMvxCommand SetUseAnalogDirections { get; }
        public IMvxCommand SetShowSensorsInfo { get; }
        public IMvxCommand SetShowSpecialButtons { get; }
        public IMvxCommand SetShowActionsButtons { get; }
        public IMvxCommand ShowActionsGrid1 { get; }
        public IMvxCommand ShowActionsGrid2 { get; }
        public IMvxCommand ShowActionsGrid3 { get; }
        public IMvxCommand HideActionsGrid { get; }
        public IMvxCommand SetActionsDelay1 { get; }
        public IMvxCommand SetActionsDelay2 { get; }
        public IMvxCommand SetActionsDelay3 { get; }
        public IMvxCommand SetActionsDelay4 { get; }
        public IMvxCommand SetActionsDelay5 { get; }
        public IMvxCommand SetColorFilterNone { get; }
        public IMvxCommand SetColorFilterGrayscale { get; }
        public IMvxCommand SetColorFilterCool { get; }
        public IMvxCommand SetColorFilterWarm { get; }
        public IMvxCommand SetColorFilterBurn { get; }
        public IMvxCommand SetColorFilterRetro { get; }
        public IMvxCommand SetColorFilterBlue { get; }
        public IMvxCommand SetRCore { get; }
        public IMvxCommand SetColorFilterGreen { get; }
        public IMvxCommand SetColorFilterRed { get; }
        public IMvxCommand SetAudioLevelMute { get; }
        public IMvxCommand SetAudioLevelLow { get; }
        public IMvxCommand SetAudioMediumLevel { get; }
        public IMvxCommand SetAudioLevelNormal { get; }
        public IMvxCommand SetAudioLevelHigh { get; }
        public IMvxCommand ShowFPSCounterCommand { get; }
        public IMvxCommand ShowBufferCounterCommand { get; }
        public IMvxCommand SetNearestNeighbor { get; }
        public IMvxCommand SetAnisotropic { get; }
        public IMvxCommand SetCubic { get; }
        public IMvxCommand SetLinear { get; }
        public IMvxCommand SetHighQualityCubic { get; }
        public IMvxCommand SetMultiSampleLinear { get; }
        public IMvxCommand SetAliased { get; }
        public IMvxCommand SetCoreOptionsVisible { get; }
        public IMvxCommand SetControlsMapVisible { get; }
        public IMvxCommand SetShowLogsList { get; }
        public IMvxCommand SetAutoSave { get; }
        public IMvxCommand SetAutoSave15Sec { get; }
        public IMvxCommand SetAutoSave30Sec { get; }
        public IMvxCommand SetAutoSave60Sec { get; }
        public IMvxCommand SetAutoSave90Sec { get; }
        public IMvxCommand SetRotateDegreePlus { get; }
        public IMvxCommand SetRotateDegreeMinus { get; }
        public IMvxCommand ToggleMuteAudio { get; }
        public IMvxCommand ShowSavesList { get; }
        public IMvxCommand ClearAllSaves { get; }
        public IMvxCommand SetShowXYZ { get; }
        public IMvxCommand SetShowL2R2Controls { get; }
        public IMvxCommand SetAutoSaveNotify { get; }
        public IMvxCommand SetAudioEcho { get; }
        public IMvxCommand SetAudioReverb { get; }
        public IMvxCommand SetScaleFactorVisible { get; }
        public IMvxCommand SetButtonsCustomization { get; }
        public IMvxCommand SetSetCustomConsoleEditMode { get; }
        public IMvxCommand ResetAdjustments { get; }
        public IMvxCommand SetToggleMenuGrid { get; }


        public bool FitScreenState = false;
        public bool ScanLines1 = false;
        public bool ScanLines2 = false;
        public bool ScanLines3 = false;
        public bool DoublePixel = false;
        public bool Speedup = false;
        public bool UpdatesOnly = false;
        public bool skipCached = false;
        private EventHandler RaiseSkippedCachedHandler = null;
        public bool SkipCached
        {
            get
            {
                return skipCached && FramebufferConverter.SkipCached;
            }
            set
            {
                if (FramebufferConverter.RaiseSkippedCachedHandler == null)
                {
                    RaiseSkippedCachedHandler = RaiseSkippedCached;
                    FramebufferConverter.RaiseSkippedCachedHandler = RaiseSkippedCachedHandler;
                }
                skipCached = value;
                FramebufferConverter.SkipCached = skipCached;
            }
        }
        private void RaiseSkippedCached(object sender, EventArgs args)
        {
            try
            {
                RaisePropertyChanged(nameof(SkipCached));
            }
            catch (Exception ex)
            {

            }
        }
        public bool SkipFrames = false;
        public bool SkipFramesRandom = false;
        public bool DelayFrames = false;
        public bool ReduceFreezes = false;

        public bool GameIsLoading = false;

        public IMvxCommand<InjectedInputTypes> InjectInputCommand { get; }

        private IMvxCommand[] AllCoreCommands { get; }

        private bool coreOperationsAllowed = false;
        public bool CoreOperationsAllowed
        {
            get => coreOperationsAllowed;
            set
            {
                if (SetProperty(ref coreOperationsAllowed, value))
                {
                    if (AllCoreCommands == null)
                    {
                        NavigationService.Close(this);
                    }
                    else
                    {
                        foreach (var i in AllCoreCommands)
                        {
                            i.RaiseCanExecuteChanged();
                        }
                    }
                }
            }
        }

        public bool FullScreenChangingPossible => PlatformService.FullScreenChangingPossible;
        public bool IsFullScreenMode => PlatformService.IsFullScreenMode;

        public bool TouchScreenAvailable => PlatformService.TouchScreenAvailable;

        public bool DisplayTouchGamepad => ForceDisplayTouchGamepad || ShouldDisplayTouchGamepad;

        private bool forceDisplayTouchGamepad;
        public bool ForceDisplayTouchGamepad
        {
            get => forceDisplayTouchGamepad;
            set
            {
                if (SetProperty(ref forceDisplayTouchGamepad, value))
                {
                    PlatformService.PlayNotificationSound("button-01.mp3");
                    RaisePropertyChanged(nameof(DisplayTouchGamepad));
                    Settings.AddOrUpdateValue(ForceDisplayTouchGamepadKey, ForceDisplayTouchGamepad);
                }
            }
        }

        private bool shouldDisplayTouchGamepad;
        private bool ShouldDisplayTouchGamepad
        {
            get => shouldDisplayTouchGamepad;
            set
            {
                if (SetProperty(ref shouldDisplayTouchGamepad, value))
                {
                    RaisePropertyChanged(nameof(DisplayTouchGamepad));
                }
            }
        }

        private bool gameIsPaused;
        public bool GameIsPaused
        {
            get => gameIsPaused;
            set => SetProperty(ref gameIsPaused, value);
        }

        private bool displayPlayerUI;
        public bool DisplayPlayerUI
        {
            get => displayPlayerUI;
            set
            {
                SetProperty(ref displayPlayerUI, value);
                if (value)
                {

                }
            }
        }

        public GamePlayerViewModel(IMvxNavigationService navigationService, IPlatformService platformService, IVideoService videoService, IAudioService audioService, IEmulationService emulationService, ISettings settings)
        {
            try
            {
                FramebufferConverter.currentFileEntry = "";
                FramebufferConverter.currentFileProgress = 0;
                FramebufferConverter.UpdateProgressState = (sender, args) =>
                  {
                      try
                      {
                          RaisePropertyChanged(nameof(currentFileEntry));
                          RaisePropertyChanged(nameof(currentFileProgress));
                          RaisePropertyChanged(nameof(isProgressVisible));
                      }
                      catch (Exception ex)
                      {

                      }
                  };
                FramebufferConverter.isGameStarted = false;
                NavigationService = navigationService;
                PlatformService = platformService;
                PlatformService?.SaveGamesListState();
                FramebufferConverter.PlatformService = platformService;
                EmulationService = emulationService;
                VideoService = videoService;
                AudioService = audioService;
                Settings = settings;
                ForceDisplayTouchGamepad = Settings.GetValueOrDefault(ForceDisplayTouchGamepadKey, true);
                ShouldDisplayTouchGamepad = shouldDisplayTouchGamepad;
                ActionsDelay = Settings.GetValueOrDefault(nameof(ActionsDelay), 150);
                ActionsDelay1 = Settings.GetValueOrDefault(nameof(ActionsDelay1), false);
                ActionsDelay2 = Settings.GetValueOrDefault(nameof(ActionsDelay2), true);
                ActionsDelay3 = Settings.GetValueOrDefault(nameof(ActionsDelay3), false);
                ActionsDelay4 = Settings.GetValueOrDefault(nameof(ActionsDelay4), false);
                ActionsDelay5 = Settings.GetValueOrDefault(nameof(ActionsDelay5), false);
                FitScreen = Settings.GetValueOrDefault(nameof(FitScreen), 1);
                ScreenRow = Settings.GetValueOrDefault(nameof(ScreenRow), 1);
                FitScreenState = Settings.GetValueOrDefault(nameof(FitScreenState), false);
                ScanLines1 = Settings.GetValueOrDefault(nameof(ScanLines1), false);
                ScanLines2 = Settings.GetValueOrDefault(nameof(ScanLines2), false);
                ScanLines3 = Settings.GetValueOrDefault(nameof(ScanLines3), false);
                DoublePixel = Settings.GetValueOrDefault(nameof(DoublePixel), false);
                AudioOnly = Settings.GetValueOrDefault(nameof(AudioOnly), false);
                VideoOnly = Settings.GetValueOrDefault(nameof(VideoOnly), false);
                Speedup = Settings.GetValueOrDefault(nameof(Speedup), false);
                UpdatesOnly = Settings.GetValueOrDefault(nameof(UpdatesOnly), false);
                SkipCached = Settings.GetValueOrDefault(nameof(SkipCached), false);
                SkipFrames = Settings.GetValueOrDefault(nameof(SkipFrames), false);
                SkipFramesRandom = Settings.GetValueOrDefault(nameof(SkipFramesRandom), false);
                DelayFrames = Settings.GetValueOrDefault(nameof(DelayFrames), false);
                ReduceFreezes = Settings.GetValueOrDefault(nameof(ReduceFreezes), true);
                CrazyBufferActive = Settings.GetValueOrDefault(nameof(CrazyBufferActive), true);
                TabSoundEffect = Settings.GetValueOrDefault(nameof(TabSoundEffect), false);
                ShowSpecialButtons = Settings.GetValueOrDefault(nameof(ShowSpecialButtons), true);
                ShowActionsButtons = Settings.GetValueOrDefault(nameof(ShowActionsButtons), true);
                AudioLowLevel = Settings.GetValueOrDefault(nameof(AudioLowLevel), false);
                AudioMediumLevel = Settings.GetValueOrDefault(nameof(AudioMediumLevel), false);
                AudioNormalLevel = Settings.GetValueOrDefault(nameof(AudioNormalLevel), true);
                AudioHighLevel = Settings.GetValueOrDefault(nameof(AudioHighLevel), false);
                AudioMuteLevel = Settings.GetValueOrDefault(nameof(AudioMuteLevel), false);
                ShowFPSCounter = Settings.GetValueOrDefault(nameof(ShowFPSCounter), false);
                ShowBufferCounter = Settings.GetValueOrDefault(nameof(ShowBufferCounter), false);
                NearestNeighbor = Settings.GetValueOrDefault(nameof(NearestNeighbor), true);
                Anisotropic = Settings.GetValueOrDefault(nameof(Anisotropic), false);
                Cubic = Settings.GetValueOrDefault(nameof(Cubic), false);
                HighQualityCubic = Settings.GetValueOrDefault(nameof(HighQualityCubic), false);
                Linear = Settings.GetValueOrDefault(nameof(Linear), false);
                MultiSampleLinear = Settings.GetValueOrDefault(nameof(MultiSampleLinear), false);
                Aliased = Settings.GetValueOrDefault(nameof(Aliased), false);
                ShowLogsList = Settings.GetValueOrDefault(nameof(ShowLogsList), false);
                AutoSave = Settings.GetValueOrDefault(nameof(AutoSave), true);
                AutoSave15Sec = Settings.GetValueOrDefault(nameof(AutoSave15Sec), false);
                AutoSave30Sec = Settings.GetValueOrDefault(nameof(AutoSave30Sec), false);
                AutoSave60Sec = Settings.GetValueOrDefault(nameof(AutoSave60Sec), false);
                AutoSave90Sec = Settings.GetValueOrDefault(nameof(AutoSave90Sec), false);
                AutoSaveNotify = Settings.GetValueOrDefault(nameof(AutoSaveNotify), true);
                AudioEcho = Settings.GetValueOrDefault(nameof(AudioEcho), false);
                AudioReverb = Settings.GetValueOrDefault(nameof(AudioReverb), false);
                RotateDegree = Settings.GetValueOrDefault(nameof(RotateDegree), 0);
                UseAnalogDirections = Settings.GetValueOrDefault(nameof(UseAnalogDirections), false);
                var RCoreState = Settings.GetValueOrDefault("RCoreState", nameof(RCore1));
                switch (RCoreState)
                {
                    case "RCore1":
                        RCore1 = true;
                        break;
                    case "RCore2":
                        RCore2 = true;
                        break;
                    case "RCore4":
                        RCore4 = true;
                        break;
                    case "RCore6":
                        RCore6 = true;
                        break;
                    case "RCore8":
                        RCore8 = true;
                        break;
                    case "RCore12":
                        RCore12 = true;
                        break;
                    case "RCore20":
                        RCore20 = true;
                        break;
                    case "RCore32":
                        RCore32 = true;
                        break;
                }
                try
                {
                    leftScaleFactorValueP = (float)Settings.GetValueOrDefault(nameof(leftScaleFactorValueP), 1f);
                    leftScaleFactorValueW = (float)Settings.GetValueOrDefault(nameof(leftScaleFactorValueW), 1f);
                    rightScaleFactorValueP = (float)Settings.GetValueOrDefault(nameof(rightScaleFactorValueP), 1f);
                    rightScaleFactorValueW = (float)Settings.GetValueOrDefault(nameof(rightScaleFactorValueW), 1f);
                }
                finally
                {

                }
                try
                {
                    rightTransformXCurrentP = (double)Settings.GetValueOrDefault(nameof(rightTransformXCurrentP), 0.0);
                    rightTransformYCurrentP = (double)Settings.GetValueOrDefault(nameof(rightTransformYCurrentP), 0.0);

                    leftTransformXCurrentP = (double)Settings.GetValueOrDefault(nameof(leftTransformXCurrentP), 0.0);
                    leftTransformYCurrentP = (double)Settings.GetValueOrDefault(nameof(leftTransformYCurrentP), 0.0);

                    actionsTransformXCurrentP = (double)Settings.GetValueOrDefault(nameof(actionsTransformXCurrentP), 0.0);
                    actionsTransformYCurrentP = (double)Settings.GetValueOrDefault(nameof(actionsTransformYCurrentP), 0.0);
                }
                finally
                {

                }

                //ScaleFactorVisible = Settings.GetValueOrDefault(nameof(ScaleFactorVisible), false);
                GetColorFilter();
                updateScanlines();
                ToggleAliased(true);
                UpdateAudioLevel();
                UpdateFilters();
                ToggleShowLogsList(true);
                ToggleAutoSave(true);
                ToggleAutoSaveSeconds(true);
                ToggleRotateDegree(true);
                ToggleAutoSaveNotify(true);
                ToggleAudioEcho(true);
                ToggleAudioReverb(true);
                ToggleUpdatesOnly(true);
                ToggleSkipCached(true);
                //ToggleScaleFactorVisible(true);
                ToggleShowSpecialButtons(true);
                ToggleShowActionsButtons(true);
                ToggleUseAnalogDirections(true);
                SetRCoreCall();

                try
                {
                    CustomTouchPadRetrieveAsync();
                }
                catch (Exception er)
                {

                }

                TappedCommand = new MvxCommand(() =>
                {
                    DisplayPlayerUI = !DisplayPlayerUI;
                    DisplayPlayerUITemp = DisplayPlayerUI;
                });

                TappedCommand2 = new MvxCommand(() =>
                {
                    if (PlatformService.XBoxMode)
                    {
                        PlatformService.XBoxMode = false;
                        CheckXBoxModeMew();
                    }
                });

                PointerMovedCommand = new MvxCommand(() =>
                {
                    PlatformService.ChangeMousePointerVisibility(MousePointerVisibility.Visible);
                });
                PointerTabbedCommand = new MvxCommand(() =>
                {
                    InjectInputCommand.Execute(InjectedInputTypes.DeviceIdPointerPressed);
                    InjectInputCommand.Execute(InjectedInputTypes.DeviceIdMouseLeft);
                });
                ToggleFullScreenCommand = new MvxCommand(() => RequestFullScreenChange(FullScreenChangeType.Toggle));

                TogglePauseCommand = new MvxCommand(() => { var task = TogglePause(false); }, () => CoreOperationsAllowed);
                ResetCommand = new MvxCommand(Reset, () => CoreOperationsAllowed);
                StopCommand = new MvxCommand(Stop, () => CoreOperationsAllowed);

                SaveStateSlot1 = new MvxCommand(async () => await SaveState(1));
                SaveStateSlot2 = new MvxCommand(async () => await SaveState(2));
                SaveStateSlot3 = new MvxCommand(async () => await SaveState(3));
                SaveStateSlot4 = new MvxCommand(async () => await SaveState(4));
                SaveStateSlot5 = new MvxCommand(async () => await SaveState(5));
                SaveStateSlot6 = new MvxCommand(async () => await SaveState(6));
                SaveStateSlot7 = new MvxCommand(async () => await SaveState(7));
                SaveStateSlot8 = new MvxCommand(async () => await SaveState(8));
                SaveStateSlot9 = new MvxCommand(async () => await SaveState(9));
                SaveStateSlot10 = new MvxCommand(async () => await SaveState(10));

                LoadStateSlot1 = new MvxCommand(() => LoadState(1));
                LoadStateSlot2 = new MvxCommand(() => LoadState(2));
                LoadStateSlot3 = new MvxCommand(() => LoadState(3));
                LoadStateSlot4 = new MvxCommand(() => LoadState(4));
                LoadStateSlot5 = new MvxCommand(() => LoadState(5));
                LoadStateSlot6 = new MvxCommand(() => LoadState(6));
                LoadStateSlot7 = new MvxCommand(() => LoadState(7));
                LoadStateSlot8 = new MvxCommand(() => LoadState(8));
                LoadStateSlot9 = new MvxCommand(() => LoadState(9));
                LoadStateSlot10 = new MvxCommand(() => LoadState(10));
                ImportSavedSlots = new MvxCommand(() => ImportSavedSlotsAction());
                ExportSavedSlots = new MvxCommand(() => ExportSavedSlotsAction());
                ImportActionsSlots = new MvxCommand(() => ImportActionsSlotsAction());
                ExportActionsSlots = new MvxCommand(() => ExportActionsSlotsAction());

                SetScreenFit = new MvxCommand(() => ToggleFitScreen());
                SetScanlines1 = new MvxCommand(() => ToggleScanlines1());
                SetScanlines2 = new MvxCommand(() => ToggleScanlines2());
                SetScanlines3 = new MvxCommand(() => ToggleScanlines3());
                SetDoublePixel = new MvxCommand(() => ToggleDoublePixel(false));
                SetAudioOnly = new MvxCommand(() => ToggleAudioOnly(false));
                SetVideoOnly = new MvxCommand(() => ToggleVideoOnly(false));
                SetSpeedup = new MvxCommand(() => ToggleSpeedup(false));
                SetUpdatesOnly = new MvxCommand(() => ToggleUpdatesOnly(false));
                SetSkipCached = new MvxCommand(() => ToggleSkipCached(false));
                SetSkipFrames = new MvxCommand(() => ToggleSkipFrames(false));
                SetSkipFramesRandom = new MvxCommand(() => ToggleSkipFramesRandom(false));
                DontWaitThreads = new MvxCommand(() => DontWaitThreadsCall());
                SetDelayFrames = new MvxCommand(() => ToggleDelayFrames(false));
                SetReduceFreezes = new MvxCommand(() => ToggleReduceFreezes(false));
                SetCrazyBufferActive = new MvxCommand(() => ToggleCrazyBufferActive(false));
                SetTabSoundEffect = new MvxCommand(() => ToggleTabSoundEffect());
                SetSensorsMovement = new MvxCommand(() => ToggleSensorsMovement());
                SetUseAnalogDirections = new MvxCommand(() => ToggleUseAnalogDirections());
                SetShowSensorsInfo = new MvxCommand(() => ToggleShowSensorsInfo());
                SetShowSpecialButtons = new MvxCommand(() => ToggleShowSpecialButtons());
                SetShowActionsButtons = new MvxCommand(() => ToggleShowActionsButtons());
                ShowActionsGrid1 = new MvxCommand(() => ActionsGridVisible(true, 1));
                ShowActionsGrid2 = new MvxCommand(() => ActionsGridVisible(true, 2));
                ShowActionsGrid3 = new MvxCommand(() => ActionsGridVisible(true, 3));
                HideActionsGrid = new MvxCommand(() => ActionsGridVisible(false, 0));
                SetActionsDelay1 = new MvxCommand(() => SetActionsDelay(100));
                SetActionsDelay2 = new MvxCommand(() => SetActionsDelay(150));
                SetActionsDelay3 = new MvxCommand(() => SetActionsDelay(200));
                SetActionsDelay4 = new MvxCommand(() => SetActionsDelay(300));
                SetActionsDelay5 = new MvxCommand(() => SetActionsDelay(500));
                SetColorFilterNone = new MvxCommand(() => SetColorFilter(0));
                SetColorFilterGrayscale = new MvxCommand(() => SetColorFilter(1));
                SetColorFilterCool = new MvxCommand(() => SetColorFilter(2));
                SetColorFilterWarm = new MvxCommand(() => SetColorFilter(3));
                SetColorFilterBurn = new MvxCommand(() => SetColorFilter(4));
                SetColorFilterRetro = new MvxCommand(() => SetColorFilter(5));
                SetColorFilterBlue = new MvxCommand(() => SetColorFilter(6));
                SetRCore = new MvxCommand(() => SetRCoreCall());
                SetColorFilterGreen = new MvxCommand(() => SetColorFilter(7));
                SetColorFilterRed = new MvxCommand(() => SetColorFilter(8));
                SetAudioLevelMute = new MvxCommand(() => SetAudioLevel(0));
                SetAudioLevelLow = new MvxCommand(() => SetAudioLevel(1));
                SetAudioMediumLevel = new MvxCommand(() => SetAudioLevel(2));
                SetAudioLevelNormal = new MvxCommand(() => SetAudioLevel(3));
                SetAudioLevelHigh = new MvxCommand(() => SetAudioLevel(4));
                ShowFPSCounterCommand = new MvxCommand(() => ShowFPSCounterToggle(false));
                ShowBufferCounterCommand = new MvxCommand(() => ShowBufferCounterToggle(false));
                SetNearestNeighbor = new MvxCommand(() => SetFilters(1));
                SetAnisotropic = new MvxCommand(() => SetFilters(2));
                SetCubic = new MvxCommand(() => SetFilters(3));
                SetHighQualityCubic = new MvxCommand(() => SetFilters(4));
                SetLinear = new MvxCommand(() => SetFilters(5));
                SetMultiSampleLinear = new MvxCommand(() => SetFilters(6));
                SetAliased = new MvxCommand(() => ToggleAliased(false));
                SetCoreOptionsVisible = new MvxCommand(() => ToggleCoreOptionsVisible());
                SetControlsMapVisible = new MvxCommand(() => ToggleControlsVisible());
                SetShowLogsList = new MvxCommand(() => ToggleShowLogsList(false));
                SetAutoSave = new MvxCommand(() => ToggleAutoSave(false));
                SetAutoSave15Sec = new MvxCommand(() => ToggleAutoSaveSeconds(false, 15));
                SetAutoSave30Sec = new MvxCommand(() => ToggleAutoSaveSeconds(false, 30));
                SetAutoSave60Sec = new MvxCommand(() => ToggleAutoSaveSeconds(false, 60));
                SetAutoSave90Sec = new MvxCommand(() => ToggleAutoSaveSeconds(false, 90));
                SetRotateDegreePlus = new MvxCommand(() => ToggleRotateDegree(false, 90));
                SetRotateDegreeMinus = new MvxCommand(() => ToggleRotateDegree(false, -90));
                ToggleMuteAudio = new MvxCommand(() => ToggleMuteAudioCall());
                ShowSavesList = new MvxCommand(() => ShowAllSaves());
                ClearAllSaves = new MvxCommand(() => ClearAllSavesCall());
                SetShowXYZ = new MvxCommand(() => SetShowXYZCall());
                SetShowL2R2Controls = new MvxCommand(() => SetShowL2R2ControlsCall());
                SetAutoSaveNotify = new MvxCommand(() => ToggleAutoSaveNotify(false));
                SetAudioEcho = new MvxCommand(() => ToggleAudioEcho(false));
                SetAudioReverb = new MvxCommand(() => ToggleAudioReverb(false));
                SetScaleFactorVisible = new MvxCommand(() => ToggleScaleFactorVisible(false));
                SetButtonsCustomization = new MvxCommand(() => ToggleButtonsCustomization(false));
                SetSetCustomConsoleEditMode = new MvxCommand(() => ToggleSetCustomConsoleEditMode(false));
                ResetAdjustments = new MvxCommand(() => ResetAdjustmentsCall());
                SetToggleMenuGrid = new MvxCommand(() => ToggleMenuGridActive());



                InjectInputCommand = new MvxCommand<InjectedInputTypes>(d => EmulationService.InjectInputPlayer1(d));

                AllCoreCommands = new IMvxCommand[] { TogglePauseCommand, ResetCommand, StopCommand,
                SaveStateSlot1, SaveStateSlot2, SaveStateSlot3, SaveStateSlot4, SaveStateSlot5, SaveStateSlot6, SaveStateSlot7, SaveStateSlot8, SaveStateSlot9, SaveStateSlot10,
                LoadStateSlot1, LoadStateSlot2, LoadStateSlot3, LoadStateSlot4, LoadStateSlot5, LoadStateSlot6, LoadStateSlot7, LoadStateSlot8, LoadStateSlot9, LoadStateSlot10
                };

                PlatformService.FullScreenChangeRequested += (d, e) => RequestFullScreenChange(e.Type);
                PlatformService.PauseToggleRequested += OnPauseToggleKey;
                PlatformService.XBoxMenuRequested += OnXBoxMenuKey;
                PlatformService.QuickSaveRequested += QuickSaveKey;
                PlatformService.SavesListRequested += SavesListKey;
                PlatformService.ChangeToXBoxModeRequested += ChangeToXBoxModeKey;
                PlatformService.GameStateOperationRequested += OnGameStateOperationRequested;

                GameSystemMenuSelected = new MvxCommand<SystemMenuModel>(GameSystemMenuHandler);
                GameSystemSavetSelected = new MvxCommand<SaveSlotsModel>(SaveSelectHandler);
                GameSystemSaveHolding = new MvxCommand<SaveSlotsModel>(SaveHoldHandler);

                ClearAllEffects = new MvxCommand(() =>
                {
                    ClearAllEffectsCall();
                });

                ShowAllEffects = new MvxCommand(() =>
                {
                    EffectsVisible = !EffectsVisible;
                    RaisePropertyChanged(nameof(EffectsVisible));
                    RaisePropertyChanged(nameof(compatibiltyTag));
                });

                PrepareXBoxMenu();
                //callXBoxModeTimer(true);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public void SetRCoreCall()
        {
            try
            {
                if (RCore1)
                {
                    Settings.AddOrUpdateValue("RCoreState", nameof(RCore1));
                    FramebufferConverter.CoresCount = 1;
                }
                else if (RCore2)
                {
                    Settings.AddOrUpdateValue("RCoreState", nameof(RCore2));
                    FramebufferConverter.CoresCount = 2;
                }
                else if (RCore4)
                {
                    Settings.AddOrUpdateValue("RCoreState", nameof(RCore4));
                    FramebufferConverter.CoresCount = 4;
                }
                else if (RCore6)
                {
                    Settings.AddOrUpdateValue("RCoreState", nameof(RCore6));
                    FramebufferConverter.CoresCount = 6;
                }
                else if (RCore8)
                {
                    Settings.AddOrUpdateValue("RCoreState", nameof(RCore8));
                    FramebufferConverter.CoresCount = 8;
                }
                else if (RCore12)
                {
                    Settings.AddOrUpdateValue("RCoreState", nameof(RCore12));
                    FramebufferConverter.CoresCount = 12;
                }
                else if (RCore20)
                {
                    Settings.AddOrUpdateValue("RCoreState", nameof(RCore20));
                    FramebufferConverter.CoresCount = 18;
                }
                else if (RCore32)
                {
                    Settings.AddOrUpdateValue("RCoreState", nameof(RCore32));
                    FramebufferConverter.CoresCount = 32;
                }
                else
                {
                    RCore1 = true;
                    Settings.AddOrUpdateValue("RCoreState", nameof(RCore1));
                    FramebufferConverter.CoresCount = 1;
                }
                RaisePropertyChanged(nameof(RCore1));
                RaisePropertyChanged(nameof(RCore2));
                RaisePropertyChanged(nameof(RCore4));
                RaisePropertyChanged(nameof(RCore6));
                RaisePropertyChanged(nameof(RCore8));
                RaisePropertyChanged(nameof(RCore12));
                RaisePropertyChanged(nameof(RCore20));
                RaisePropertyChanged(nameof(RCore32));
                FramebufferConverter.inputFillWithBlack = true;
                forceReloadLogsList = true;
            }
            catch (Exception ex)
            {

            }
        }
        bool DisplayPlayerUITemp = true;
        bool ForceDisplayTouchGamepadTest = false;
        bool DisplayPlayerUITest = false;
        public void CheckXBoxModeMew()
        {
            try
            {
                //Check if XBox Mode
                if (PlatformService.XBoxMode)
                {
                    ForceDisplayTouchGamepad = false;
                    DisplayPlayerUI = false;
                }
                else
                {
                    ForceDisplayTouchGamepad = true;
                    DisplayPlayerUI = DisplayPlayerUITemp;
                }
                RaisePropertyChanged(nameof(DisplayTouchGamepad));
                RaisePropertyChanged(nameof(DisplayPlayerUI));
            }
            catch (Exception e)
            {

            }
        }
        private async void ChangeToXBoxModeKey(object sender, EventArgs args)
        {
            try
            {
                CheckXBoxModeMew();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public void CheckXBoxMode()
        {
            try
            {
                //Check if XBox Mode
                ForceDisplayTouchGamepadTest = ForceDisplayTouchGamepad;
                DisplayPlayerUITest = DisplayPlayerUI;
                if (PlatformService.XBoxMode)
                {
                    ForceDisplayTouchGamepad = false;
                    DisplayPlayerUI = false;
                }
                else
                {
                    ForceDisplayTouchGamepad = true;
                    DisplayPlayerUI = DisplayPlayerUITemp;
                }
                if (ForceDisplayTouchGamepadTest != ForceDisplayTouchGamepad)
                {
                    RaisePropertyChanged(nameof(DisplayTouchGamepad));
                }
                if (DisplayPlayerUITest != DisplayPlayerUI)
                {
                    RaisePropertyChanged(nameof(DisplayPlayerUI));
                }
            }
            catch (Exception e)
            {

            }
        }
        int tempLevel = 3;
        public bool ToggleMuteAudioCall()
        {
            if (AudioMuteLevel)
            {
                SetAudioLevel(tempLevel);
            }
            else
            {
                SetAudioLevel(0);
            }
            return AudioMuteLevel;
        }

        bool AutoSaveStartup = true;
        bool InfoStartup = true;
        private void callFPSTimer(bool startState = false)
        {
            try
            {
                FPSTimer?.Dispose();
                if (startState)
                {
                    FPSTimer = new Timer(delegate { UpdateFPSCounter(null, EventArgs.Empty); }, null, 0, 1000);
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }
        private void callGCTimer(bool startState = false)
        {
            try
            {
                GCTimer?.Dispose();
                if (startState)
                {
                    GCTimer = new Timer(delegate { UpdateGC(null, EventArgs.Empty); }, null, 0, 100);
                }
                else
                {
                    if (NoGCRegionState)
                    {
                        AudioService.EndNoGCRegionCall();
                        NoGCRegionState = false;
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
                NoGCRegionState = false;
            }
        }
        private void callXBoxModeTimer(bool startState = false)
        {
            try
            {
                XBoxModeTimer?.Dispose();
                if (startState)
                {
                    XBoxModeTimer = new Timer(delegate { UpdateXBoxMode(null, EventArgs.Empty); }, null, 0, 650);
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }
        private void callBufferTimer(bool startState = false)
        {
            try
            {
                BufferTimer?.Dispose();
                if (startState)
                {
                    BufferTimer = new Timer(delegate { UpdateBufferCounter(null, EventArgs.Empty); }, null, 0, 1000);
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }
        private void callAutoSaveTimer(bool startState = false, int seconds = 0)
        {
            try
            {
                AutoSaveTimer?.Dispose();
                if (startState)
                {
                    AutoSaveTimer = new Timer(delegate
                    {
                        if (AutoSaveStartup)
                        {
                            AutoSaveStartup = false;
                        }
                        else
                        {
                            AutoSaveManager(null, EventArgs.Empty);
                        }

                    }, null, 0, seconds * 1000);
                }
                else
                {
                    AutoSaveStartup = true;
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }

        private void callInfoTimer(bool startState = false)
        {
            try
            {
                InfoTimer?.Dispose();
                if (startState)
                {
                    InfoTimer = new Timer(delegate
                    {
                        if (InfoStartup)
                        {
                            InfoStartup = false;
                        }
                        else
                        {
                            PeriodicChecks(null, EventArgs.Empty);
                        }

                    }, null, 0, 3000);
                }
                else
                {
                    InfoStartup = true;
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }

        public void Dispose()
        {
            try
            {
                GC.SuppressFinalize(this);
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }
        private void callLogTimer(bool startState = false)
        {
            try
            {
                LogTimer?.Dispose();
                if (startState)
                {
                    LogTimer = new Timer(delegate { UpdateLogList(null, EventArgs.Empty); }, null, 0, 1500);
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }

        private void SetFilters(int Filter)
        {
            switch (Filter)
            {
                case 1:
                    //NearestNeighbor
                    NearestNeighbor = true;
                    Anisotropic = false;
                    Cubic = false;
                    HighQualityCubic = false;
                    Linear = false;
                    MultiSampleLinear = false;
                    break;
                case 2:
                    //Anisotropic
                    NearestNeighbor = false;
                    Anisotropic = true;
                    Cubic = false;
                    HighQualityCubic = false;
                    Linear = false;
                    MultiSampleLinear = false;
                    break;
                case 3:
                    //Cubic
                    NearestNeighbor = false;
                    Anisotropic = false;
                    Cubic = true;
                    HighQualityCubic = false;
                    Linear = false;
                    MultiSampleLinear = false;
                    break;
                case 4:
                    //HighQualityCubic
                    NearestNeighbor = false;
                    Anisotropic = false;
                    Cubic = false;
                    HighQualityCubic = true;
                    Linear = false;
                    MultiSampleLinear = false;
                    break;
                case 5:
                    //Linear
                    NearestNeighbor = false;
                    Anisotropic = false;
                    Cubic = false;
                    HighQualityCubic = false;
                    Linear = true;
                    MultiSampleLinear = false;
                    break;
                case 6:
                    //MultiSampleLinear
                    NearestNeighbor = false;
                    Anisotropic = false;
                    Cubic = false;
                    HighQualityCubic = false;
                    Linear = false;
                    MultiSampleLinear = true;
                    break;
                default:
                    //NearestNeighbor
                    NearestNeighbor = true;
                    Anisotropic = false;
                    Cubic = false;
                    HighQualityCubic = false;
                    Linear = false;
                    MultiSampleLinear = false;
                    break;
            }
            UpdateFilters();
        }
        private void UpdateFilters()
        {
            Settings.AddOrUpdateValue(nameof(NearestNeighbor), NearestNeighbor);
            Settings.AddOrUpdateValue(nameof(Anisotropic), Anisotropic);
            Settings.AddOrUpdateValue(nameof(Cubic), Cubic);
            Settings.AddOrUpdateValue(nameof(HighQualityCubic), HighQualityCubic);
            Settings.AddOrUpdateValue(nameof(Linear), Linear);
            Settings.AddOrUpdateValue(nameof(MultiSampleLinear), MultiSampleLinear);
            RaisePropertyChanged(nameof(NearestNeighbor));
            RaisePropertyChanged(nameof(Anisotropic));
            RaisePropertyChanged(nameof(Cubic));
            RaisePropertyChanged(nameof(HighQualityCubic));
            RaisePropertyChanged(nameof(Linear));
            RaisePropertyChanged(nameof(MultiSampleLinear));
            if (NearestNeighbor)
            {
                VideoService.SetFilter(1);
            }
            else if (Anisotropic)
            {
                VideoService.SetFilter(2);
            }
            else if (Cubic)
            {
                VideoService.SetFilter(3);
            }
            else if (HighQualityCubic)
            {
                VideoService.SetFilter(4);
            }
            else if (Linear)
            {
                VideoService.SetFilter(5);
            }
            else if (MultiSampleLinear)
            {
                VideoService.SetFilter(6);
            }
        }
        private void SetActionsDelay(int DelayTime)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                ActionsDelay = DelayTime;
                Settings.AddOrUpdateValue(nameof(ActionsDelay), ActionsDelay);
                switch (DelayTime)
                {
                    case 100:
                        ActionsDelay1 = true;
                        ActionsDelay2 = false;
                        ActionsDelay3 = false;
                        ActionsDelay4 = false;
                        ActionsDelay5 = false;
                        UpdateInfoState("Delay set to Fastest");
                        break;
                    case 150:
                        ActionsDelay1 = false;
                        ActionsDelay2 = true;
                        ActionsDelay3 = false;
                        ActionsDelay4 = false;
                        ActionsDelay5 = false;
                        UpdateInfoState("Delay set to Fast");
                        break;
                    case 200:
                        ActionsDelay1 = false;
                        ActionsDelay2 = false;
                        ActionsDelay3 = true;
                        ActionsDelay4 = false;
                        ActionsDelay5 = false;
                        UpdateInfoState("Delay set to Normal");
                        break;
                    case 300:
                        ActionsDelay1 = false;
                        ActionsDelay2 = false;
                        ActionsDelay3 = false;
                        ActionsDelay4 = true;
                        ActionsDelay5 = false;
                        UpdateInfoState("Delay set to Slow");
                        break;
                    case 500:
                        ActionsDelay1 = false;
                        ActionsDelay2 = false;
                        ActionsDelay3 = false;
                        ActionsDelay4 = false;
                        ActionsDelay5 = true;
                        UpdateInfoState("Delay set to Slowest");
                        break;
                }
                RaisePropertyChanged(nameof(ActionsDelay1));
                RaisePropertyChanged(nameof(ActionsDelay2));
                RaisePropertyChanged(nameof(ActionsDelay3));
                RaisePropertyChanged(nameof(ActionsDelay4));
                RaisePropertyChanged(nameof(ActionsDelay5));
                Settings.AddOrUpdateValue(nameof(ActionsDelay1), ActionsDelay1);
                Settings.AddOrUpdateValue(nameof(ActionsDelay2), ActionsDelay2);
                Settings.AddOrUpdateValue(nameof(ActionsDelay3), ActionsDelay3);
                Settings.AddOrUpdateValue(nameof(ActionsDelay4), ActionsDelay4);
                Settings.AddOrUpdateValue(nameof(ActionsDelay5), ActionsDelay5);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public void SetButtonsIsLoadingState(bool ButtonsIsLoadingState)
        {
            ButtonsIsLoading = ButtonsIsLoadingState;
            RaisePropertyChanged(nameof(ButtonsIsLoading));
        }
        private async void ImportSavedSlotsAction()
        {
            try
            {
                await ImportSettingsSlotsAction(SlotsSaveLocation);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            GameIsLoadingState(false);
        }

        private async void ExportSavedSlotsAction()
        {
            try
            {
                await ExportSettingsSlotsAction(SlotsSaveLocation);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                GameIsLoadingState(false);
            }
        }

        private async void ExportActionsSlotsAction()
        {
            try
            {
                await ExportSettingsSlotsAction(ActionsSaveLocation);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                GameIsLoadingState(false);
            }
        }

        private async void ImportActionsSlotsAction()
        {
            try
            {
                await ImportSettingsSlotsAction(ActionsSaveLocation);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            GameIsLoadingState(false);
        }

        private async void ClearAllSavesCall()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                PlatformService.PlayNotificationSound("alert.wav");
                ConfirmConfig confirmCleanSaves = new ConfirmConfig();
                confirmCleanSaves.SetTitle("Clean all saves");
                confirmCleanSaves.SetMessage("This action will remove all your saves, are you sure?");
                confirmCleanSaves.UseYesNo();
                var StartClean = await UserDialogs.Instance.ConfirmAsync(confirmCleanSaves);

                if (StartClean)
                {


                    string GameID = EmulationService.GetGameID();
                    string GameName = EmulationService.GetGameName();
                    var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(SlotsSaveLocation);
                    if (localFolder != null)
                    {
                        string targetFolder = localFolder.FullName + "\\" + GameID;
                        var gameFolderTest = await CrossFileSystem.Current.GetDirectoryFromPathAsync(targetFolder);
                        if (gameFolderTest != null)
                        {
                            GameIsLoadingState(true);
                            await gameFolderTest.DeleteAsync();
                            PlatformService.PlayNotificationSound("success.wav");
                            await GeneralDialog("All Saves cleaned (deleted)", "Clean done");
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild.wav");
                            await GeneralDialog("No saved slots found!", "Clean all saves");
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("faild.wav");
                        await GeneralDialog("No saved slots found!", "Clean all saves");
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }

        public async void ResetAdjustmentsCall()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                PlatformService.PlayNotificationSound("alert.wav");
                ConfirmConfig confirmReset = new ConfirmConfig();
                confirmReset.SetTitle("Reset Customizations");
                confirmReset.SetMessage("This action will reset the (global) touch controls customizations\nAre you sure?");
                confirmReset.UseYesNo();
                var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                if (StartReset)
                {
                    leftScaleFactorValueP = 1;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(leftScaleFactorValueP), 1f);
                    leftScaleFactorValueW = 1;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(leftScaleFactorValueW), 1f);

                    rightScaleFactorValueP = 1;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(rightScaleFactorValueP), 1f);
                    rightScaleFactorValueW = 1;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(rightScaleFactorValueW), 1f);


                    RaisePropertyChanged(nameof(LeftScaleFactorValue));
                    RaisePropertyChanged(nameof(RightScaleFactorValue));

                    rightTransformXCurrentP = RightTransformXDefault;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(rightTransformXCurrentP), RightTransformXDefault);
                    RaisePropertyChanged(nameof(RightTransformXCurrent));

                    rightTransformYCurrentP = RightTransformYDefault;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(rightTransformYCurrentP), RightTransformYDefault);
                    RaisePropertyChanged(nameof(RightTransformYCurrent));

                    leftTransformXCurrentP = LeftTransformXDefault;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(leftTransformXCurrentP), LeftTransformXDefault);
                    RaisePropertyChanged(nameof(LeftTransformXCurrent));

                    leftTransformYCurrentP = LeftTransformYDefault;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(leftTransformYCurrentP), LeftTransformYDefault);
                    RaisePropertyChanged(nameof(LeftTransformYCurrent));

                    actionsTransformXCurrentP = ActionsTransformXDefault;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(actionsTransformXCurrentP), ActionsTransformXDefault);
                    RaisePropertyChanged(nameof(ActionsTransformXCurrent));

                    actionsTransformYCurrentP = ActionsTransformYDefault;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(actionsTransformYCurrentP), ActionsTransformYDefault);
                    RaisePropertyChanged(nameof(ActionsTransformYCurrent));

                    PlatformService.PlayNotificationSound("success.wav");
                    await GeneralDialog("Touch controls reseted to default", "Reset done");
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }
        private async Task ExportSettingsSlotsAction(string TargetLocation)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                string GameID = EmulationService.GetGameID();
                string GameName = EmulationService.GetGameName();
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(TargetLocation);
                if (localFolder == null)
                {
                    await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(TargetLocation);
                    localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(TargetLocation);
                }
                string targetFolder = localFolder.FullName + "\\" + GameID;
                if (await CrossFileSystem.Current.GetDirectoryFromPathAsync(targetFolder) != null)
                {
                    GameIsLoadingState(true);
                    IDirectoryInfo zipsDirectory = null;
                    zipsDirectory = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync("tempExports");
                    if (zipsDirectory == null)
                    {
                        zipsDirectory = await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync("tempExports");
                    }
                    string targetFileName = GameID + ".sip";
                    string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                    var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                    if (targetFielTest != null)
                    {
                        await targetFielTest.DeleteAsync();
                    }
                    ZipFile.CreateFromDirectory(targetFolder, zipFileName);
                    await Task.Delay(1000);
                    do
                    {
                        targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                    } while (targetFielTest == null);
                    await DownloadExportedSlotsAsync(targetFielTest);
                }
                else
                {
                    PlatformService.PlayNotificationSound("faild.wav");
                    await GeneralDialog(Resources.Strings.ExportSlotsMessageError, Resources.Strings.ExportSlotsTitle);
                    GameIsLoadingState(false);
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                GameIsLoadingState(false);
            }
        }
        private async Task DownloadExportedSlotsAsync(IFileInfo file)
        {
            try
            {
                var saveFile = await CrossFileSystem.Current.PickSaveFileAsync(".sip");
                await Task.Delay(700);
                if (saveFile != null)
                {
                    using (var inStream = await file.OpenAsync(FileAccess.Read))
                    {
                        using (var outStream = await saveFile.OpenAsync(FileAccess.ReadWrite))
                        {
                            await inStream.CopyToAsync(outStream);
                            await outStream.FlushAsync();
                        }
                    }
                    PlatformService.PlayNotificationSound("success.wav");
                    await GeneralDialog(Resources.Strings.ExportSlotsMessage, Resources.Strings.ExportSlotsTitle);
                    UpdateInfoState("Export Done");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            GameIsLoadingState(false);
        }


        private async Task ImportSettingsSlotsAction(string ExtractLocation)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                string GameID = EmulationService.GetGameID();
                string GameName = EmulationService.GetGameName();

                var extensions = new string[] { ".sip" };
                var file = await CrossFileSystem.Current.PickFileAsync(extensions);
                if (file != null)
                {
                    PlatformService.PlayNotificationSound("alert.wav");
                    ConfirmConfig confirmImportSaves = new ConfirmConfig();
                    confirmImportSaves.SetTitle(Resources.Strings.ImportStatesTitle);
                    confirmImportSaves.SetMessage(Resources.Strings.ImportStatesMessage);
                    confirmImportSaves.UseYesNo();
                    IDirectoryInfo folder = null;
                    var StartImport = await UserDialogs.Instance.ConfirmAsync(confirmImportSaves);

                    if (StartImport)
                    {
                        GameIsLoadingState(true);
                        IDirectoryInfo zipsDirectory = null;
                        zipsDirectory = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync("tempExports");
                        if (zipsDirectory == null)
                        {
                            zipsDirectory = await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync("tempExports");
                        }
                        string targetFileName = GameID + ".sip";
                        string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                        var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                        if (targetFielTest != null)
                        {
                            await targetFielTest.DeleteAsync();
                        }
                        var targetFile = await zipsDirectory.CreateFileAsync(targetFileName);
                        using (var inStream = await file.OpenAsync(FileAccess.Read))
                        {
                            using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                            {
                                await inStream.CopyToAsync(outStream);
                                await outStream.FlushAsync();
                            }
                        }
                        var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(ExtractLocation);
                        if (localFolder == null)
                        {
                            await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(ExtractLocation);
                            localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(ExtractLocation);
                        }
                        string targetFolder = localFolder.FullName + "\\" + GameID;
                        var targetFolderTest = await localFolder.GetDirectoryAsync(GameID);
                        if (targetFolderTest != null)
                        {
                            await targetFolderTest.DeleteAsync();
                        }
                        await localFolder.CreateDirectoryAsync(GameID);
                        ZipFile.ExtractToDirectory(zipFileName, targetFolder);
                        PlatformService.PlayNotificationSound("success.wav");
                        await GeneralDialog(Resources.Strings.ImportSlotsMessage, Resources.Strings.ImportSlotsTitle);
                        await ActionsRetrieveAsync();
                        UpdateInfoState("Import Done");
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("faild.wav");
                        await GeneralDialog(Resources.Strings.ImportSlotsMessageCancel, Resources.Strings.ImportSlotsTitle);
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            GameIsLoadingState(false);
        }

        public void GameIsLoadingState(bool LoadingState)
        {
            GameIsLoading = LoadingState;
            RaisePropertyChanged(nameof(GameIsLoading));
        }

        private void ToggleFitScreen()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                FitScreen = FitScreen == 1 ? 3 : 1;
                FitScreenState = FitScreen == 3;
                ScreenRow = FitScreen == 3 ? 0 : 1;
                RaisePropertyChanged(nameof(FitScreen));
                RaisePropertyChanged(nameof(ScreenRow));
                RaisePropertyChanged(nameof(FitScreenState));
                Settings.AddOrUpdateValue(nameof(FitScreen), FitScreen);
                Settings.AddOrUpdateValue(nameof(ScreenRow), ScreenRow);
                Settings.AddOrUpdateValue(nameof(FitScreenState), FitScreenState);
                if (FitScreen == 3)
                {
                    UpdateInfoState("Enter Screen Fit Mode");
                }
                else
                {
                    UpdateInfoState("Exit Screen Fit Mode");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        private void updateScanlines()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                RaisePropertyChanged(nameof(ScanLines1));
                RaisePropertyChanged(nameof(ScanLines2));
                RaisePropertyChanged(nameof(ScanLines3));
                Settings.AddOrUpdateValue(nameof(ScanLines1), ScanLines1);
                Settings.AddOrUpdateValue(nameof(ScanLines2), ScanLines2);
                Settings.AddOrUpdateValue(nameof(ScanLines3), ScanLines3);
                //FramebufferConverter.NativeScanlines = ScanLines1 || ScanLines2 || ScanLines3;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        private void ToggleScanlines1()
        {
            ScanLines1 = !ScanLines1;
            ScanLines2 = false;
            ScanLines3 = false;
            updateScanlines();
        }
        private void ToggleScanlines2()
        {
            ScanLines2 = !ScanLines2;
            ScanLines1 = false;
            ScanLines3 = false;
            updateScanlines();
        }
        private void ToggleScanlines3()
        {
            ScanLines3 = !ScanLines3;
            ScanLines2 = false;
            ScanLines1 = false;
            updateScanlines();
        }
        private void ToggleDoublePixel(bool updateValue)
        {
            if (!updateValue) DoublePixel = !DoublePixel;
            RaisePropertyChanged(nameof(DoublePixel));
            Settings.AddOrUpdateValue(nameof(DoublePixel), DoublePixel);
            FramebufferConverter.NativeDoublePixel = DoublePixel;
        }
        private void ToggleAudioOnly(bool updateValue)
        {
            if (!updateValue) AudioOnly = !AudioOnly;
            RaisePropertyChanged(nameof(AudioOnly));
            Settings.AddOrUpdateValue(nameof(AudioOnly), AudioOnly);
            EmulationService.SetAudioOnlyState(AudioOnly);
            FramebufferConverter.AudioOnly = AudioOnly;
        }
        private void ToggleVideoOnly(bool updateValue)
        {
            if (!updateValue) VideoOnly = !VideoOnly;
            RaisePropertyChanged(nameof(VideoOnly));
            Settings.AddOrUpdateValue(nameof(VideoOnly), VideoOnly);
            EmulationService.SetVideoOnlyState(VideoOnly);
            AudioService.VideoOnlyGlobal = VideoOnly;
        }
        private void ToggleSpeedup(bool updateValue)
        {
            if (!updateValue) Speedup = !Speedup;
            RaisePropertyChanged(nameof(Speedup));
            Settings.AddOrUpdateValue(nameof(Speedup), Speedup);
            FramebufferConverter.NativeSpeedup = Speedup;
            FramebufferConverter.NativePixelStep = Speedup ? 2 : 1;
            if (Speedup)
            {
                FramebufferConverter.inputFillWithBlack = true;
                FramebufferConverter.ResetPixelCache();
            }
            else
            {
                FramebufferConverter.inputFillWithBlack = false;
            }
        }
        private void ToggleUpdatesOnly(bool updateValue)
        {
            if (!updateValue) UpdatesOnly = !UpdatesOnly;
            RaisePropertyChanged(nameof(UpdatesOnly));
            Settings.AddOrUpdateValue(nameof(UpdatesOnly), UpdatesOnly);
            FramebufferConverter.showUpdatesOnly = UpdatesOnly;
        }
        private void ToggleSkipCached(bool updateValue)
        {
            if (!updateValue) SkipCached = !SkipCached;
            if (!updateValue)
            {
                if (SkipCached)
                {
                    if (FramebufferConverter.isRGB888)
                    {
                        PlatformService.PlayNotificationSound("root-needed.wav");
                        GeneralDialog($"This system might not compatible with 'Pixel Updates'\nPerformance will back to normal when you.. turn it off\n\nTo get better results try to enable skip frames or increase render threads", "Pixel Updates");
                    }
                }
            }
            RaisePropertyChanged(nameof(SkipCached));
            if (!FramebufferConverter.isRGB888)
            {
                Settings.AddOrUpdateValue(nameof(SkipCached), SkipCached);
            }
            FramebufferConverter.SkipCached = SkipCached;
            forceReloadLogsList = true;
        }
        private void ToggleAudioEcho(bool updateValue)
        {
            if (!updateValue) AudioEcho = !AudioEcho;
            RaisePropertyChanged(nameof(AudioEcho));
            Settings.AddOrUpdateValue(nameof(AudioEcho), AudioEcho);
            AudioService.AddAudioEcho(AudioEcho);
            if (!updateValue)
            {
                if (AudioEcho)
                {
                    UpdateInfoState("Sound Echo on");
                }
                else
                {
                    UpdateInfoState("Sound Echo off");
                }
            }
        }
        private void ToggleAudioReverb(bool updateValue)
        {
            if (!updateValue) AudioReverb = !AudioReverb;
            RaisePropertyChanged(nameof(AudioReverb));
            Settings.AddOrUpdateValue(nameof(AudioReverb), AudioReverb);
            AudioService.AddAudioReverb(AudioReverb);
            if (!updateValue)
            {
                if (AudioReverb)
                {
                    UpdateInfoState("Sound Reverb on");
                }
                else
                {
                    UpdateInfoState("Sound Reverb off");
                }
            }
        }
        private void ToggleScaleFactorVisible(bool updateValue)
        {
            if (!updateValue) ScaleFactorVisible = !ScaleFactorVisible;
            CustomConsoleEditMode = false;
            ButtonsCustomization = false;
            RaisePropertyChanged(nameof(ScaleFactorVisible));
            RaisePropertyChanged(nameof(CustomConsoleEditMode));
            RaisePropertyChanged(nameof(ButtonsCustomization));
            Settings.AddOrUpdateValue(nameof(ScaleFactorVisible), ScaleFactorVisible);
            if (ScaleFactorVisible)
            {
                //UpdateInfoState("Customization Enabled, controls inactive now");
                //ControlsAreaHeight = 330; //this disabled since the sliders nmoved to top
            }
            else
            {
                if (!updateValue)
                {
                    UpdateInfoState("Customization Disabled, controls active now");
                }
                //ControlsAreaHeight = 285; //this disabled since the sliders nmoved to top
            }
            //RaisePropertyChanged(nameof(ControlsAreaHeight)); //this disabled since the sliders nmoved to top
        }
        private void ToggleButtonsCustomization(bool updateValue)
        {
            if (!updateValue) ButtonsCustomization = !ButtonsCustomization;
            CustomConsoleEditMode = false;
            ScaleFactorVisible = false;
            RaisePropertyChanged(nameof(ButtonsCustomization));
            RaisePropertyChanged(nameof(ScaleFactorVisible));
            RaisePropertyChanged(nameof(CustomConsoleEditMode));
            Settings.AddOrUpdateValue(nameof(ButtonsCustomization), ButtonsCustomization);

            if (!ButtonsCustomization && !updateValue)
            {
                UpdateInfoState("Customization Disabled, controls active now");
            }
            //ControlsAreaHeight = 285; //this disabled since the sliders nmoved to top
            //RaisePropertyChanged(nameof(ControlsAreaHeight)); //this disabled since the sliders nmoved to top
        }
        private void ToggleSetCustomConsoleEditMode(bool updateValue)
        {
            if (!updateValue) CustomConsoleEditMode = !CustomConsoleEditMode;
            ScaleFactorVisible = CustomConsoleEditMode;
            ButtonsCustomization = false;
            RaisePropertyChanged(nameof(CustomConsoleEditMode));
            RaisePropertyChanged(nameof(ScaleFactorVisible));
            RaisePropertyChanged(nameof(ButtonsCustomization));
            Settings.AddOrUpdateValue(nameof(CustomConsoleEditMode), CustomConsoleEditMode);
            if (CustomConsoleEditMode)
            {
                //UpdateInfoState("Customization Enabled, controls inactive now");
                //ControlsAreaHeight = 330; //this disabled since the sliders nmoved to top
            }
            else
            {
                if (!updateValue)
                {
                    UpdateInfoState("Customization Disabled, controls active now");
                }
                //ControlsAreaHeight = 285; //this disabled since the sliders nmoved to top
            }
            //RaisePropertyChanged(nameof(ControlsAreaHeight)); //this disabled since the sliders nmoved to top
        }
        private void ToggleSkipFrames(bool updateValue)
        {
            if (!updateValue) SkipFrames = !SkipFrames;
            if (!updateValue && SkipFrames)
            {
                PlatformService.PlayNotificationSound("notice.mp3");
                GeneralDialog($"This option has small effect on the performance\nyou can check {"Core Options"} for native frame skipping", "Skip Frames (Frontend)");
            }
            RaisePropertyChanged(nameof(SkipFrames));
            Settings.AddOrUpdateValue(nameof(SkipFrames), SkipFrames);
            EmulationService.SetSkipFramesState(SkipFrames);
        }
        public bool DontWaitThreadsState
        {
            get
            {
                return !FramebufferConverter.DontWaitThreads;
            }
        }
        private void DontWaitThreadsCall()
        {
            try
            {
                FramebufferConverter.DontWaitThreads = !FramebufferConverter.DontWaitThreads;
                RaisePropertyChanged(nameof(DontWaitThreadsState));
            }
            catch (Exception ex)
            {

            }
        }
        private void ToggleSkipFramesRandom(bool updateValue)
        {
            if (!updateValue) SkipFramesRandom = !SkipFramesRandom;
            if (!updateValue && SkipFramesRandom)
            {
                PlatformService.PlayNotificationSound("notice.mp3");
                GeneralDialog($"This option has small effect on the performance\nyou can check {"Core Options"} for native frame skipping", "Skip Frames (Frontend)");
            }
            RaisePropertyChanged(nameof(SkipFramesRandom));
            Settings.AddOrUpdateValue(nameof(SkipFramesRandom), SkipFramesRandom);
            EmulationService.SetSkipFramesRandomState(SkipFramesRandom);
        }
        private async void ToggleDelayFrames(bool updateValue)
        {
            if (!updateValue) DelayFrames = !DelayFrames;
            RaisePropertyChanged(nameof(DelayFrames));
            Settings.AddOrUpdateValue(nameof(DelayFrames), DelayFrames);
            AudioService.SmartFrameDelay = DelayFrames;
        }
        private async void ToggleReduceFreezes(bool updateValue)
        {
            if (!updateValue) ReduceFreezes = !ReduceFreezes;
            RaisePropertyChanged(nameof(ReduceFreezes));
            Settings.AddOrUpdateValue(nameof(ReduceFreezes), ReduceFreezes);
            if (!updateValue && !ReduceFreezes)
            {
                PlatformService.PlayNotificationSound("notice.mp3");
                await GeneralDialog($"This option is very important for the performance\nWe prefere to keep it on", "Reduce Freezes");
            }
            AudioService.SetGCPrevent(ReduceFreezes);
            callGCTimer(ReduceFreezes);
        }
        private void ToggleCrazyBufferActive(bool updateValue)
        {
            if (!updateValue) CrazyBufferActive = !CrazyBufferActive;
            RaisePropertyChanged(nameof(CrazyBufferActive));
            Settings.AddOrUpdateValue(nameof(CrazyBufferActive), CrazyBufferActive);
            FramebufferConverter.CrazyBufferActive = CrazyBufferActive;
            forceReloadLogsList = true;
        }
        private void UpdateAudioLevel()
        {
            Settings.AddOrUpdateValue(nameof(AudioLowLevel), AudioLowLevel);
            Settings.AddOrUpdateValue(nameof(AudioMediumLevel), AudioMediumLevel);
            Settings.AddOrUpdateValue(nameof(AudioNormalLevel), AudioNormalLevel);
            Settings.AddOrUpdateValue(nameof(AudioHighLevel), AudioHighLevel);
            Settings.AddOrUpdateValue(nameof(AudioMuteLevel), AudioMuteLevel);
            RaisePropertyChanged(nameof(AudioLowLevel));
            RaisePropertyChanged(nameof(AudioMediumLevel));
            RaisePropertyChanged(nameof(AudioNormalLevel));
            RaisePropertyChanged(nameof(AudioHighLevel));
            RaisePropertyChanged(nameof(AudioMuteLevel));
            if (AudioNormalLevel)
            {
                AudioService.AudioMuteGlobal = false;
                AudioService.ChangeAudioGain(1.0);
                tempLevel = 3;
            }
            else if (AudioLowLevel)
            {
                AudioService.AudioMuteGlobal = false;
                AudioService.ChangeAudioGain(0.25);
                tempLevel = 1;
            }
            else if (AudioMediumLevel)
            {
                AudioService.AudioMuteGlobal = false;
                AudioService.ChangeAudioGain(0.5);
                tempLevel = 2;
            }
            else if (AudioMuteLevel)
            {
                AudioService.AudioMuteGlobal = true;
                AudioService.ChangeAudioGain(0.0);
            }
            else if (AudioHighLevel)
            {
                AudioService.AudioMuteGlobal = false;
                AudioService.ChangeAudioGain(1.5);
                tempLevel = 4;
            }
        }
        private void SetAudioLevel(int AudioLevel)
        {
            switch (AudioLevel)
            {
                case 0:
                    AudioMuteLevel = true;
                    AudioLowLevel = false;
                    AudioMediumLevel = false;
                    AudioNormalLevel = false;
                    AudioHighLevel = false;
                    break;
                case 1:
                    AudioMuteLevel = false;
                    AudioLowLevel = true;
                    AudioMediumLevel = false;
                    AudioNormalLevel = false;
                    AudioHighLevel = false;
                    tempLevel = 1;
                    break;
                case 2:
                    AudioMuteLevel = false;
                    AudioLowLevel = false;
                    AudioMediumLevel = true;
                    AudioNormalLevel = false;
                    AudioHighLevel = false;
                    tempLevel = 2;
                    break;
                case 3:
                    AudioMuteLevel = false;
                    AudioLowLevel = false;
                    AudioMediumLevel = false;
                    AudioNormalLevel = true;
                    AudioHighLevel = false;
                    tempLevel = 3;
                    break;
                case 4:
                    AudioMuteLevel = false;
                    AudioLowLevel = false;
                    AudioMediumLevel = false;
                    AudioNormalLevel = false;
                    AudioHighLevel = true;
                    tempLevel = 4;
                    break;
                default:
                    AudioMuteLevel = false;
                    AudioLowLevel = false;
                    AudioMediumLevel = false;
                    AudioNormalLevel = true;
                    AudioHighLevel = false;
                    tempLevel = 3;
                    break;
            }
            UpdateAudioLevel();
        }

        private void ToggleTabSoundEffect()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                TabSoundEffect = !TabSoundEffect;
                RaisePropertyChanged(nameof(TabSoundEffect));
                Settings.AddOrUpdateValue(nameof(TabSoundEffect), TabSoundEffect);
                if (TabSoundEffect)
                {
                    UpdateInfoState("Keys Sound Effects On");
                }
                else
                {
                    UpdateInfoState("Keys Sound Effects Off");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        private void ToggleSensorsMovement()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                SensorsMovement = !SensorsMovement;
                RaisePropertyChanged(nameof(SensorsMovement));
                Settings.AddOrUpdateValue(nameof(SensorsMovement), SensorsMovement);
                if (SensorsMovement)
                {
                    UpdateInfoState("Sensors Movement On");
                }
                else
                {
                    UpdateInfoState("Sensors Movement Off");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        private void ToggleUseAnalogDirections(bool UpdateState = false)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                if (!UpdateState) UseAnalogDirections = !UseAnalogDirections;
                RaisePropertyChanged(nameof(UseAnalogDirections));
                Settings.AddOrUpdateValue(nameof(UseAnalogDirections), UseAnalogDirections);
                if (!UpdateState)
                {
                    if (UseAnalogDirections)
                    {
                        UpdateInfoState("Analog Movement On");
                    }
                    else
                    {
                        UpdateInfoState("Analog Movement Off");
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        private void ToggleShowSensorsInfo()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                ShowSensorsInfo = !ShowSensorsInfo;
                RaisePropertyChanged(nameof(ShowSensorsInfo));
                Settings.AddOrUpdateValue(nameof(ShowSensorsInfo), ShowSensorsInfo);
                if (ShowSensorsInfo)
                {
                    UpdateInfoState("Sensors Info On");
                }
                else
                {
                    UpdateInfoState("Sensors Info Off");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        private void ToggleShowSpecialButtons(bool updateState = false)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                if (!updateState) ShowSpecialButtons = !ShowSpecialButtons;
                RaisePropertyChanged(nameof(ShowSpecialButtons));
                Settings.AddOrUpdateValue(nameof(ShowSpecialButtons), ShowSpecialButtons);
                if (ShowSpecialButtons)
                {
                    UpdateInfoState("Show Special Keys");
                }
                else
                {
                    UpdateInfoState("Hide Special Keys");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        private void ToggleShowActionsButtons(bool updateState = false)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                if (!updateState) ShowActionsButtons = !ShowActionsButtons;
                RaisePropertyChanged(nameof(ShowActionsButtons));
                Settings.AddOrUpdateValue(nameof(ShowActionsButtons), ShowActionsButtons);
                if (ShowActionsButtons)
                {
                    UpdateInfoState("Show Actions Keys");
                }
                else
                {
                    UpdateInfoState("Hide Actions Keys");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public bool[] ColorFilters = new bool[] { true, false, false, false, false, false, false, false, false };
        private void SetColorFilter(int ColorFilter)
        {
            try
            {
                switch (ColorFilter)
                {
                    case 0:
                        UpdateInfoState("Color Mode set to None");
                        break;
                    case 1:
                        UpdateInfoState("Color Mode set to Garyscale");
                        break;
                    case 2:
                        UpdateInfoState("Color Mode set to Cool");
                        break;
                    case 3:
                        UpdateInfoState("Color Mode set to Warm");
                        break;
                    case 4:
                        UpdateInfoState("Color Mode set to Sepia");
                        break;
                    case 5:
                        UpdateInfoState("Color Mode set to Retro");
                        break;
                    case 6:
                        UpdateInfoState("Color Mode set to Blue");
                        break;
                    case 7:
                        UpdateInfoState("Color Mode set to Green");
                        break;
                    case 8:
                        UpdateInfoState("Color Mode set to Red");
                        break;
                }
                PlatformService.PlayNotificationSound("button-01.mp3");
                SetActiveColorFilter(ColorFilter);
                FramebufferConverter.CurrentColorFilter = ColorFilter;
                FramebufferConverter.SetRGBLookupTable(true);
                FramebufferConverter.ResetPixelCache();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            forceReloadLogsList = true;
        }
        private void SetActiveColorFilter(int ColorIndex)
        {

            for (int i = 0; i < ColorFilters.Length; i++)
            {
                if (i == ColorIndex)
                {
                    ColorFilters[i] = true;
                }
                else
                {
                    ColorFilters[i] = false;
                }
                Settings.AddOrUpdateValue(nameof(ColorFilters) + i, ColorFilters[i]);
            }
            RaisePropertyChanged(nameof(ColorFilters));

        }

        private void ShowFPSCounterToggle(bool UpdateState)
        {
            if (!UpdateState) ShowFPSCounter = !ShowFPSCounter;
            Settings.AddOrUpdateValue(nameof(ShowFPSCounter), ShowFPSCounter);
            RaisePropertyChanged(nameof(ShowFPSCounter));
            callFPSTimer(ShowFPSCounter);
            //EmulationService.SetFPSCounterState(ShowFPSCounter);
            VideoService.SetShowFPS(ShowFPSCounter);
        }
        private void ShowBufferCounterToggle(bool UpdateState)
        {
            if (!UpdateState) ShowBufferCounter = !ShowBufferCounter;
            Settings.AddOrUpdateValue(nameof(ShowBufferCounter), ShowBufferCounter);
            RaisePropertyChanged(nameof(ShowBufferCounter));
            callBufferTimer(ShowBufferCounter);
        }

        private void ToggleAliased(bool UpdateState)
        {
            if (!UpdateState) Aliased = !Aliased;
            Settings.AddOrUpdateValue(nameof(Aliased), Aliased);
            VideoService.SetAliased(Aliased);
            RaisePropertyChanged(nameof(Aliased));
        }




        public EventHandler CoreOptionsHandler;
        private void ToggleCoreOptionsVisible()
        {
            if (CoreOptionsHandler != null)
            {

                CoreOptionsVisible = !CoreOptionsVisible;
                RaisePropertyChanged(nameof(CoreOptionsVisible));
                PlatformService.SetCoreOptionsState(CoreOptionsVisible);
                if (CoreOptionsVisible)
                {
                    CoreOptionsHandler.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public EventHandler ControlsHandler;
        private async void ToggleControlsVisible()
        {
            if (ControlsHandler != null)
            {

                ControlsMapVisible = !ControlsMapVisible;
                RaisePropertyChanged(nameof(ControlsMapVisible));
                PlatformService.SetCoreOptionsState(ControlsMapVisible);
                if (ControlsMapVisible)
                {
                    ControlsHandler.Invoke(null, EventArgs.Empty);
                    if (!EmulationService.CorePaused)
                    {
                        await EmulationService.PauseGameAsync();
                    }
                }
                else
                {
                    if (EmulationService.CorePaused)
                    {
                        await EmulationService.ResumeGameAsync();
                    }
                }
            }
        }

        private void SetShowXYZCall()
        {
            ShowXYZ = !ShowXYZ;
            Settings.AddOrUpdateValue(nameof(ShowXYZ), ShowXYZ);
            RaisePropertyChanged(nameof(ShowXYZ));
        }
        private void SetShowL2R2ControlsCall()
        {
            ShowL2R2Controls = !ShowL2R2Controls;
            Settings.AddOrUpdateValue(nameof(ShowL2R2Controls), ShowL2R2Controls);
            RaisePropertyChanged(nameof(ShowL2R2Controls));
        }
        private void ToggleShowLogsList(bool UpdateState)
        {
            if (!UpdateState) ShowLogsList = !ShowLogsList;
            Settings.AddOrUpdateValue(nameof(ShowLogsList), ShowLogsList);
            RaisePropertyChanged(nameof(ShowLogsList));
            callLogTimer(ShowLogsList);
        }

        private void ToggleAutoSave(bool UpdateState)
        {
            if (!UpdateState) AutoSave = !AutoSave;
            Settings.AddOrUpdateValue(nameof(AutoSave), AutoSave);
            RaisePropertyChanged(nameof(AutoSave));
        }
        private void ToggleAutoSaveNotify(bool UpdateState)
        {
            if (!UpdateState) AutoSaveNotify = !AutoSaveNotify;
            Settings.AddOrUpdateValue(nameof(AutoSaveNotify), AutoSaveNotify);
            RaisePropertyChanged(nameof(AutoSaveNotify));
        }
        private void ToggleAutoSaveSeconds(bool UpdateState, int seconds = 0)
        {
            if (!UpdateState)
            {
                switch (seconds)
                {
                    case 15:
                        AutoSave15Sec = !AutoSave15Sec;
                        AutoSave30Sec = false;
                        AutoSave60Sec = false;
                        AutoSave90Sec = false;
                        break;

                    case 30:
                        AutoSave30Sec = !AutoSave30Sec;
                        AutoSave15Sec = false;
                        AutoSave60Sec = false;
                        AutoSave90Sec = false;
                        break;

                    case 60:
                        AutoSave60Sec = !AutoSave60Sec;
                        AutoSave15Sec = false;
                        AutoSave30Sec = false;
                        AutoSave90Sec = false;
                        break;

                    case 90:
                        AutoSave90Sec = !AutoSave90Sec;
                        AutoSave15Sec = false;
                        AutoSave30Sec = false;
                        AutoSave60Sec = false;
                        break;
                }
                if (AutoSave15Sec || AutoSave30Sec || AutoSave60Sec || AutoSave90Sec)
                {
                    UpdateInfoState($"Auto Save set to {seconds} second");
                }
            }
            if (seconds == 0)
            {
                if (AutoSave15Sec)
                {
                    seconds = 15;
                }
                else if (AutoSave30Sec)
                {
                    seconds = 30;
                }
                else if (AutoSave60Sec)
                {
                    seconds = 60;
                }
                else if (AutoSave90Sec)
                {
                    seconds = 90;
                }
            }
            if ((AutoSave15Sec || AutoSave30Sec || AutoSave60Sec || AutoSave90Sec) && seconds > 0)
            {
                callAutoSaveTimer(true, seconds);
            }
            else
            {
                callAutoSaveTimer();
                UpdateInfoState($"Auto Save disabled");
            }
            Settings.AddOrUpdateValue(nameof(AutoSave15Sec), AutoSave15Sec);
            RaisePropertyChanged(nameof(AutoSave15Sec));
            Settings.AddOrUpdateValue(nameof(AutoSave30Sec), AutoSave30Sec);
            RaisePropertyChanged(nameof(AutoSave30Sec));
            Settings.AddOrUpdateValue(nameof(AutoSave60Sec), AutoSave60Sec);
            RaisePropertyChanged(nameof(AutoSave60Sec));
            Settings.AddOrUpdateValue(nameof(AutoSave90Sec), AutoSave90Sec);
            RaisePropertyChanged(nameof(AutoSave90Sec));
        }

        private void ToggleRotateDegree(bool UpdateState, int degree = 0)
        {
            if (!UpdateState)
            {
                switch (degree)
                {
                    case 90:
                        RotateDegree = RotateDegree == degree ? 0 : degree;
                        break;

                    case -90:
                        RotateDegree = RotateDegree == degree ? 0 : degree;
                        break;
                }
            }

            PlatformService.SetRotateDegree(RotateDegree);
            Settings.AddOrUpdateValue(nameof(RotateDegree), RotateDegree);
            RaisePropertyChanged(nameof(RotateDegree));
            if (RotateDegree > 0)
            {
                RotateDegreeMinusActive = false;
                RotateDegreePlusActive = true;
            }
            else if (RotateDegree < 0)
            {
                RotateDegreeMinusActive = true;
                RotateDegreePlusActive = false;
            }
            else
            {
                RotateDegreeMinusActive = false;
                RotateDegreePlusActive = false;
            }
            RaisePropertyChanged(nameof(RotateDegreePlusActive));
            RaisePropertyChanged(nameof(RotateDegreeMinusActive));
        }

        private void GetColorFilter()
        {
            try
            {
                for (int i = 0; i < ColorFilters.Length; i++)
                {
                    ColorFilters[i] = Settings.GetValueOrDefault(nameof(ColorFilters) + i, (i == 0));
                    if (ColorFilters[i])
                    {
                        FramebufferConverter.CurrentColorFilter = i;
                        FramebufferConverter.SetRGBLookupTable();
                    }
                }
                RaisePropertyChanged(nameof(ColorFilters));
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }

        }

        private void ActionsGridVisible(bool ActionGridState, int ActionsSetNumber)
        {
            try
            {
                ActionsGridVisiblity = ActionGridState;
                switch (ActionsSetNumber)
                {
                    case 1:
                        CurrentActionsSet = "S0" + ActionsSetNumber;
                        break;
                    case 2:
                        CurrentActionsSet = "S0" + ActionsSetNumber;
                        break;
                    case 3:
                        CurrentActionsSet = "S0" + ActionsSetNumber;
                        break;
                }
                if (ActionGridState)
                {
                    UpdateActionsPreviewSet();
                }
                ActionsCustomDelay = false;
                RaisePropertyChanged(nameof(ActionsCustomDelay));
                RaisePropertyChanged(nameof(ActionsGridVisiblity));
                PlatformService.PlayNotificationSound("button-01.mp3");
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public string CoreName = "";
        public string SystemName = "";
        public string SystemNamePreview = "";
        public string SystemIcon = "";
        public bool RootNeeded = false;
        public string MainFilePath = "";
        public EventHandler FreeCore;
        public override async void Prepare(GameLaunchEnvironment parameter)
        {
            try
            {
                CoreName = parameter.Core.Name;
                SystemName = parameter.SystemName;
                SystemNamePreview = parameter.Core.OriginalSystemName;
                SystemIcon = GameSystemViewModel.GetSystemIconByName(SystemName);
                RootNeeded = parameter.RootNeeded;
                MainFilePath = parameter.MainFileRealPath;
                InGameOptionsActive = parameter.Core.IsInGameOptionsActive;
                GameIsLoadingState(true);
                UpdateInfoState("Preparing...");
                PlatformService.SetStopHandler(StopHandler);
                //FPSMonitor.Start();
                await EmulationService.StartGameAsync(parameter.Core, parameter.StreamProvider, parameter.MainFilePath);
                await ActionsRetrieveAsync();
                FreeCore = (sender, args) =>
                {
                    try
                    {
                        parameter.Core.FreeLibretroCore();
                    }
                    catch (Exception ex)
                    {

                    }
                };
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }

        }

        public void updateCoreOptions(string KeyName = "")
        {
            var TargetSystem = GameSystemSelectionViewModel.SystemsOptions[SystemName];
            if (KeyName.Length > 0)
            {
                var optionObject = TargetSystem.OptionsList[KeyName];
                EmulationService.UpdateCoreOption(optionObject.OptionsKey, optionObject.SelectedIndex);
            }
            else
            {
                foreach (var optionItem in TargetSystem.OptionsList.Keys)
                {
                    var optionObject = TargetSystem.OptionsList[optionItem];
                    EmulationService.UpdateCoreOption(optionObject.OptionsKey, optionObject.SelectedIndex);
                }
            }
        }

        public CoresOptions getSystemOptions(string SystemName)
        {
            return GameSystemSelectionViewModel.SystemsOptions[SystemName];
        }

        public async Task CoreOptionsStoreAsync(string SystemName)
        {
            GameIsLoadingState(true);
            await GameSystemSelectionViewModel.CoreOptionsStoreAsyncDirect(SystemName);
            GameIsLoadingState(false);
        }

        private async void RequestFullScreenChange(FullScreenChangeType fullScreenChangeType)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                await PlatformService.ChangeFullScreenStateAsync(fullScreenChangeType);
                RaisePropertyChanged(nameof(IsFullScreenMode));
                if (IsFullScreenMode)
                {
                    UpdateInfoState("Enter Fullscreen Mode");
                }
                else
                {
                    UpdateInfoState("Exit Fullscreen Mode");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public override void ViewAppeared()
        {
            try
            {
                CoreOperationsAllowed = true;
                PlatformService.HandleGameplayKeyShortcuts = true;
                DisplayPlayerUI = true;

                if (EmulationService != null)
                {
                    EmulationService.GameLoaded += EmulationService_GameStarted;
                }

                PlatformService.SetHideCoreOptionsHandler(HideCoreOptions);
                PlatformService.SetHideSavesListHandler(HideSavesList);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        bool addOpenCountInProgress = false;
        bool isGameStarted = false;
        bool errorDialogAppeard = false;
        bool FailedToLoadGame = false;

        private async void EmulationService_GameStarted(object sender, EventArgs e)
        {
            try
            {
                if (isGameStarted)
                {
                    return;
                }
                GameIsLoadingState(false);
                PlatformService.PlayNotificationSound("gamestarted.mp3");
                await EmulationService.ResumeGameAsync();

                isGameStarted = true;
                RaisePropertyChanged(nameof(isProgressVisible));
                if (!PlatformService.GameNoticeShowed)
                {
                    Dispatcher.RequestMainThreadAction(() => ShowGameTipInfo());
                }
                if (VideoService != null)
                {
                    if (VideoService.isShaderActive())
                    {
                        addShadersInitial = true;
                        addShaders = true;
                        RaisePropertyChanged(nameof(AddShaders));
                        addShadersInitial = false;
                    }
                    if (VideoService.isOverlayActive())
                    {
                        addOverlaysInitial = true;
                        addOverlays = true;
                        RaisePropertyChanged(nameof(AddOverlays));
                        addOverlaysInitial = false;
                    }
                }
                if (EmulationService.isGameLoaded())
                {
                    if (!addOpenCountInProgress)
                    {
                        addOpenCountInProgress = true;
                        await PlatformService.AddGameToRecents(SystemName, MainFilePath, RootNeeded, EmulationService.GetGameID(), 0, false);
                        UpdateInfoState("Game Started");

                        if (!FramebufferConverter.CopyMemoryAvailable)
                        {
                            FramebufferConverter.LoadMemcpyFunction();
                        }

                        SyncEffectsSettings();
                        SyncMemoryOptions();

                        FramebufferConverter.isGameStarted = true;
                        FramebufferConverter.fillSpanRequired = true;
                        if (AudioService != null)
                        {
                            AudioService.isGameStarted = true;
                        }
                        ShowSystemInfo();
                        RaisePropertyChanged("Manufacturer");
                        if (StartTimeStamp == 0)
                        {
                            StartTimeStamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        }
                        RaisePropertyChanged(nameof(IsSegaSystem));
                    }
                }
                else if (!errorDialogAppeard)
                {
                    errorDialogAppeard = true;
                    FailedToLoadGame = true;
                    RaisePropertyChanged(nameof(isProgressVisible));
                    UpdateInfoState("Game Failed");
                    PlatformService?.PlayNotificationSound("faild.wav");
                    GeneralDialog("Failed to load the game, for more details check\n\u26EF -> Debug -> Log List", "Load Failed");
                }
                SetExtrasOptions();
            }
            catch (Exception ex)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(ex);
                }
            }
            try
            {
                CheckXBoxModeMew();
            }
            catch (Exception ee)
            {

            }
        }
        void SetExtrasOptions()
        {
            ToggleDoublePixel(true);
            ToggleAudioOnly(true);
            ToggleVideoOnly(true);
            ToggleSpeedup(true);
            ToggleSkipFrames(true);
            ToggleDelayFrames(true);
            ToggleReduceFreezes(true);
            ToggleCrazyBufferActive(true);
            ShowFPSCounterToggle(true);
            ShowBufferCounterToggle(true);
        }
        void ShowSystemInfo()
        {
            SystemInfoVisiblity = true;
            RaisePropertyChanged(nameof(SystemInfoVisiblity));
        }
        public async void ShowGameTipInfo()
        {
            try
            {
                bool ShowNoticeState = Settings.GetValueOrDefault("NeverShowSlow2", true);
                if (ShowNoticeState)
                {
                    await Task.Delay(2200);
                    PlatformService.PlayNotificationSound("notice.mp3");
                    ConfirmConfig confirmLoadNotice = new ConfirmConfig();
                    confirmLoadNotice.SetTitle("Game Tips");
                    confirmLoadNotice.SetMessage("If the game went slow try:\n1- Pause \u25EB then Resume \u25B7.\n2- Enable \u26EF -> Performance -> Skip Frames\n\nXBOX Shortcuts:\nShow Menu-> Down + Select/View\nSave State-> Left + Select/View\nLoad State-> Right + Select/View\n\nEnjoy " + char.ConvertFromUtf32(0x1F609));
                    confirmLoadNotice.UseYesNo();
                    confirmLoadNotice.SetOkText("Never Show");
                    confirmLoadNotice.SetCancelText("Dismiss");
                    PlatformService.GameNoticeShowed = true;
                    var NeverShow = await UserDialogs.Instance.ConfirmAsync(confirmLoadNotice);
                    if (NeverShow)
                    {
                        PlatformService.PlayNotificationSound("button-01.mp3");
                        Settings.AddOrUpdateValue("NeverShowSlow2", false);
                        confirmLoadNotice.DisposeIfDisposable();
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("button-01.mp3");
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public override void ViewDisappearing()
        {
            try
            {
                if (!GameStopped)
                {
                    StopPlaying(true);
                }
                PlatformService.PlayNotificationSound("stop.wav");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                PlatformService.RestoreGamesListState(PlatformService.veScroll);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }


        private async Task TogglePause(bool dismissOverlayImmediately)
        {
            try
            {
                if (!CoreOperationsAllowed)
                {
                    return;
                }
                PlatformService.PlayNotificationSound("button-01.mp3");
                CoreOperationsAllowed = false;

                if (GameIsPaused)
                {
                    await EmulationService.ResumeGameAsync();
                    if (dismissOverlayImmediately)
                    {
                        //DisplayPlayerUI = false;
                    }
                    //UpdateInfoState("Game Resume");
                }
                else
                {
                    await EmulationService.PauseGameAsync();
                    //DisplayPlayerUI = true;
                    //UpdateInfoState("Game Paused");
                }

                GameIsPaused = !GameIsPaused;
                if (AudioService != null)
                {
                    AudioService.gameIsPaused = GameIsPaused;
                }

            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            CoreOperationsAllowed = true;
        }

        private async void OnPauseToggleKey(object sender, EventArgs args)
        {
            try
            {
                await TogglePause(true);
                if (GameIsPaused)
                {
                    //PlatformService.ForceUIElementFocus();
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        private async void Reset()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                PlatformService.PlayNotificationSound("alert.wav");
                ConfirmConfig confirmConfig = new ConfirmConfig();
                confirmConfig.SetTitle(Resources.Strings.GamePlayResetTitle);
                confirmConfig.SetMessage(Resources.Strings.GamePlayResetMessage);
                confirmConfig.UseYesNo();
                var result = await UserDialogs.Instance.ConfirmAsync(confirmConfig);

                if (result)
                {
                    CoreOperationsAllowed = false;
                    await EmulationService.ResetGameAsync();

                    UpdateInfoState("Game Reset");
                    if (GameIsPaused)
                    {
                        await TogglePause(true);
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            CoreOperationsAllowed = true;
        }

        bool stopDialogInProgress = false;
        private async void Stop()
        {
            if (GameStopInProgress || stopDialogInProgress)
            {
                return;
            }
            try
            {
                PlatformService.PlayNotificationSound("button-01.mp3");
                PlatformService.PlayNotificationSound("alert.wav");
                ConfirmConfig confirmConfig = new ConfirmConfig();
                confirmConfig.SetTitle(Resources.Strings.GamePlayStopTitle);
                confirmConfig.SetMessage(Resources.Strings.GamePlayStopMessage);
                confirmConfig.UseYesNo();
                stopDialogInProgress = true;
                var result = await UserDialogs.Instance.ConfirmAsync(confirmConfig);

                if (result)
                {
                    //FPSMonitor.Stop();
                    //FPSMonitor.DisposeIfDisposable();
                    StopPlaying();
                }

            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            stopDialogInProgress = false;
        }


        public void StopHandler(object sender, object o)
        {
            StopPlaying();
        }

        void HideCoreOptions(object sender, EventArgs e)
        {
            if (CoreOptionsVisible)
            {
                ToggleCoreOptionsVisible();
            }
        }
        void HideControls(object sender, EventArgs e)
        {
            if (ControlsMapVisible)
            {
                ToggleControlsVisible();
            }
        }

        public bool GameStopStarted = false;
        public bool GameStopped = false;
        public bool ShowMainActions = true;
        public EventHandler SnapshotHandler;
        public bool SnapshotInProgress = false;
        public EventHandler UnlinkSensorsHandler;
        private async void StopPlaying(bool backPressed = false)
        {
            if (GameStopStarted) return;
            try
            {
                Random rnd = new Random();
                int currentPorgress = 0;
                UpdateInfoState("Please wait...", true);
                GameStopStarted = true;
                ShowMainActions = false;
                ScaleFactorVisible = false;
                ButtonsCustomization = false;
                GameIsLoadingState(true);
                HideSavesList();
                HideMenuGrid();
                HideCoreOptions(null, EventArgs.Empty);
                HideControls(null, EventArgs.Empty);
                ToggleScaleFactorVisible(true);
                ToggleButtonsCustomization(true);
                try
                {
                    await EmulationService.PauseGameAsync();
                }
                catch (Exception ep)
                {

                }
                RaisePropertyChanged(nameof(ShowMainActions));
                bool SnapshotFailed = false;
                GameStopInProgress = true;
                PlatformService.SetGameStopInProgress(GameStopInProgress);
                try
                {
                    await SaveTotalPlayedTime(true);
                }
                catch (Exception et)
                {

                }
                await Task.Delay(300);
                //Take Snapshot
                if (EmulationService.isGameLoaded() && SnapshotHandler != null)
                {
                    try
                    {
                        SnapshotHandler.Invoke(null, new GameIDArgs(EmulationService.GetGameID(), await PlatformService.GetRecentsLocationAsync()));
                    }
                    catch (Exception es)
                    {

                    }
                }
                currentPorgress = rnd.Next(currentPorgress, 10);
                UpdateInfoState($"Stopping the game {currentPorgress}%...", true);

                while (SnapshotInProgress && !SnapshotFailed)
                {
                    await Task.Delay(700);
                    currentPorgress++;
                    UpdateInfoState($"Stopping the game {currentPorgress}%...", true);
                }

                if (AutoSave && isGameStarted && !FailedToLoadGame)
                {
                    UpdateInfoState("Auto saving..", true);
                    try
                    {
                        await AutoSaveState(false);
                    }
                    catch (Exception eas)
                    {

                    }
                    await Task.Delay(700);
                }

                currentPorgress = rnd.Next(currentPorgress, 50);
                UpdateInfoState($"Stopping the game {currentPorgress}%...", true);

                await Task.Delay(700);

                CoreOperationsAllowed = false;
                PlatformService.HandleGameplayKeyShortcuts = false;

                if (EmulationService != null)
                {
                    try
                    {
                        await EmulationService.StopGameAsync();
                    }
                    catch (Exception est)
                    {

                    }
                    try
                    {
                        EmulationService.DisposeIfDisposable();
                    }
                    catch (Exception ed)
                    {

                    }
                    currentPorgress = rnd.Next(currentPorgress, 70);
                    UpdateInfoState($"Stopping the game {currentPorgress}%...", true);
                }

                try
                {
                    callFPSTimer();

                    callGCTimer();

                    //callXBoxModeTimer();

                    callBufferTimer();

                    callLogTimer();

                    callAutoSaveTimer();
                }
                catch (Exception etm)
                {

                }
                try
                {
                    PlatformService.DeSetStopHandler(StopHandler);

                    PlatformService.DeSetHideCoreOptionsHandler(HideCoreOptions);
                    PlatformService.DeSetHideSavesListHandler(HideSavesList);

                }
                catch (Exception ede)
                {

                }

                try
                {
                    if (UnlinkSensorsHandler != null)
                    {
                        UnlinkSensorsHandler.Invoke(null, EventArgs.Empty);
                        UnlinkSensorsHandler = null;
                    }
                }
                catch (Exception eh)
                {

                }
                try
                {
                    SnapshotHandler.DisposeIfDisposable();
                    SnapshotHandler = null;
                }
                catch (Exception esde)
                {

                }
                await Task.Delay(800);
                try
                {
                    PlatformService.ChangeMousePointerVisibility(MousePointerVisibility.Visible);
                    await PlatformService.ChangeFullScreenStateAsync(FullScreenChangeType.Exit);
                }
                catch (Exception ecm)
                {

                }
                try
                {
                    PlatformService.PauseToggleRequested -= OnPauseToggleKey;
                    PlatformService.XBoxMenuRequested -= OnXBoxMenuKey;
                    PlatformService.QuickSaveRequested -= QuickSaveKey;
                    PlatformService.SavesListRequested -= SavesListKey;
                    PlatformService.ChangeToXBoxModeRequested -= ChangeToXBoxModeKey;
                    PlatformService.GameStateOperationRequested -= OnGameStateOperationRequested;
                }
                catch (Exception eh)
                {

                }
                FramebufferConverter.ClearBuffer();
                FramebufferConverter.ResetPixelCache();
                FramebufferConverter.RaiseSkippedCachedHandler = null;
                FramebufferConverter.requestToStopSkipCached = false;
                FramebufferConverter.UpdateProgressState = null;
                FramebufferConverter.currentFileProgress = 0;
                FramebufferConverter.currentFileEntry = "";
                currentPorgress = rnd.Next(currentPorgress, 99);
                UpdateInfoState($"Stopping the game {currentPorgress}%...", true);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            GameStopped = true;

            await Task.Delay(700);
            try
            {
                if (InputService != null)
                {
                    await InputService.DeinitAsync();
                }
                if (AudioService != null)
                {
                    await AudioService.DeinitAsync();
                }
                if (VideoService != null)
                {
                    await VideoService.DeinitAsync();
                }
            }
            catch (Exception ex)
            {

            }
            if (FreeCore != null)
            {
                try
                {
                    FreeCore.Invoke(null, EventArgs.Empty);
                }
                catch (Exception ex)
                {

                }
            }
            UpdateInfoState($"Stopping the game 100%...", true);
            await Task.Delay(300);
            PlatformService.SetGameStopInProgress(false);

            try
            {
                Dispose();
            }
            catch (Exception edis)
            {

            }
            try
            {
                NavigationService.Close(this);
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
                GameStopInProgress = false;
            }
        }

        private async Task SaveTotalPlayedTime(bool StopRequest = false)
        {
            try
            {
                if (StartTimeStamp > 0)
                {
                    var CallTimeStamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    var PlayedTime = CallTimeStamp - StartTimeStamp;
                    if (StartTimeStamp == CallTimeStamp)
                    {
                        return;
                    }
                    if (StopRequest)
                    {
                        StartTimeStamp = 0;
                    }
                    else
                    {
                        StartTimeStamp = CallTimeStamp;
                    }
                    await PlatformService.AddGameToRecents(SystemName, MainFilePath, RootNeeded, EmulationService.GetGameID(), PlayedTime, false);
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }

        private async Task SaveState(uint slotID, bool showMessage = true)
        {
            try
            {
                GameIsLoadingState(true);
                if (showMessage)
                {
                    PlatformService.PlayNotificationSound("button-01.mp3");
                }
                CoreOperationsAllowed = false;
                bool saveState = await EmulationService.SaveGameStateAsync(slotID, showMessage);

                if (saveState)
                {
                    if (showMessage)
                    {
                        if (slotID < 11)
                        {
                            UpdateInfoState("Game Saved to Slot " + slotID);
                        }
                        else if (slotID < 21)
                        {
                            UpdateInfoState("Quick save done");
                        }
                        else if (slotID < 36)
                        {
                            UpdateInfoState("Game Auto Saved");
                        }
                    }
                    IDirectoryInfo SnapshotLocation = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync($@"{SlotsSaveLocation}\{EmulationService.GetGameID().ToLower()}");
                    string SnapshotName = $"{EmulationService.GetGameID().ToLower()}_S{slotID}";
                    SnapshotHandler.Invoke(null, new GameIDArgs(SnapshotName, SnapshotLocation));
                }
                else
                {
                    PlatformService.PlayNotificationSound("faild.wav");
                    if (slotID < 11)
                    {
                        UpdateInfoState("Failed to save on Slot " + slotID);
                    }
                    else if (slotID < 21)
                    {
                        UpdateInfoState("Failed to quick save");
                    }
                    else if (slotID < 36)
                    {
                        UpdateInfoState("Failed to auto save");
                    }
                }
                if (GameIsPaused)
                {
                    //await TogglePause(true);
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            if (!GameStopInProgress)
            {
                try
                {
                    await SaveTotalPlayedTime();
                }
                catch (Exception e)
                {

                }
            }
            CoreOperationsAllowed = true;
            GameIsLoadingState(false);
        }

        public bool SavesListActive = false;
        public bool NoSavesActive = false;
        const int SLOTS_GEN = 1;
        const int SLOTS_QUICK = 2;
        const int SLOTS_AUTO = 3;
        const int SLOTS_ALL = 4;
        public bool LoadSaveListInProgress = false;
        public ObservableCollection<SaveSlotsModel> GameSavesList = new ObservableCollection<SaveSlotsModel>();

        public IMvxCommand<SaveSlotsModel> GameSystemSavetSelected { get; }
        public IMvxCommand<SaveSlotsModel> GameSystemSaveHolding { get; }
        int currentSlotsType = 1;
        private async Task GetSaveSlotsList(int SlotsType)
        {
            try
            {
                currentSlotsType = SlotsType;
                SavesListActive = true;
                RaisePropertyChanged(nameof(SavesListActive));
                LoadSaveListInProgress = true;
                RaisePropertyChanged(nameof(LoadSaveListInProgress));
                NoSavesActive = false;
                RaisePropertyChanged(nameof(NoSavesActive));
                string GameID = EmulationService.GetGameID().ToLower();
                PlatformService.SetSavesListActive(true);
                IDirectoryInfo SavesLocation = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync($@"{SlotsSaveLocation}\{GameID}");
                GameSavesList.Clear();
                Dictionary<SaveSlotsModel, long> GameSavesListTemp = new Dictionary<SaveSlotsModel, long>();
                if (SavesLocation != null)
                {
                    var FilesList = await SavesLocation.EnumerateFilesAsync();

                    foreach (var FileItem in FilesList)
                    {
                        var FileExtension = Path.GetExtension(FileItem.Name).ToLower();
                        if (FileExtension.Contains("png"))
                        {
                            continue;
                        }
                        var FileDate = await FileItem.GetLastModifiedAsync();
                        long FileDateSort = FileDate.UtcTicks;
                        switch (SlotsType)
                        {
                            case SLOTS_GEN:

                                for (int i = 1; i <= 10; i++)
                                {
                                    var testName = $@"{GameID}_S{i}.";
                                    if (FileItem.Name.Contains(testName))
                                    {
                                        SaveSlotsModel saveSlotsModel = new SaveSlotsModel(GameID, i, FileItem.Name, FileDate.DateTime.ToString(), SavesLocation.FullName);
                                        GameSavesListTemp.Add(saveSlotsModel, FileDateSort);
                                        break;
                                    }
                                }


                                break;

                            case SLOTS_QUICK:
                                for (int i = 11; i <= 20; i++)
                                {
                                    var testName = $@"{GameID}_S{i}.";
                                    if (FileItem.Name.Contains(testName))
                                    {
                                        SaveSlotsModel saveSlotsModel = new SaveSlotsModel(GameID, i, FileItem.Name, FileDate.DateTime.ToString(), SavesLocation.FullName);
                                        GameSavesListTemp.Add(saveSlotsModel, FileDateSort);
                                        break;
                                    }
                                }
                                break;

                            case SLOTS_AUTO:
                                for (int i = 21; i <= 35; i++)
                                {
                                    var testName = $@"{GameID}_S{i}.";
                                    if (FileItem.Name.Contains(testName))
                                    {
                                        SaveSlotsModel saveSlotsModel = new SaveSlotsModel(GameID, i, FileItem.Name, FileDate.DateTime.ToString(), SavesLocation.FullName);
                                        GameSavesListTemp.Add(saveSlotsModel, FileDateSort);
                                        break;
                                    }
                                }
                                break;

                            case SLOTS_ALL:
                                for (int i = 1; i <= 35; i++)
                                {
                                    var testName = $@"{GameID}_S{i}.";
                                    if (FileItem.Name.Contains(testName))
                                    {
                                        SaveSlotsModel saveSlotsModel = new SaveSlotsModel(GameID, i, FileItem.Name, FileDate.DateTime.ToString(), SavesLocation.FullName);
                                        GameSavesListTemp.Add(saveSlotsModel, FileDateSort);
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                }
                if (GameSavesListTemp.Count > 0)
                {
                    foreach (KeyValuePair<SaveSlotsModel, long> item in GameSavesListTemp.OrderByDescending(key => key.Value))
                    {
                        GameSavesList.Add(item.Key);
                    }
                    NoSavesActive = false;
                    RaisePropertyChanged(nameof(NoSavesActive));
                }
                else
                {
                    NoSavesActive = true;
                    RaisePropertyChanged(nameof(NoSavesActive));
                    PlatformService.PlayNotificationSound("notice.mp3");
                }
                try
                {
                    GameSavesListTemp.Clear();
                    int identificador = GC.GetGeneration(GameSavesListTemp);
                    GC.Collect(identificador, GCCollectionMode.Forced);
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            LoadSaveListInProgress = false;
            RaisePropertyChanged(nameof(LoadSaveListInProgress));
            try
            {
                try
                {
                    int identificador = GC.GetGeneration(GameSavesList);
                    GC.Collect(identificador, GCCollectionMode.Forced);
                }
                catch (Exception ex)
                {

                }
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }


        private async void SaveSelectHandler(SaveSlotsModel saveSlotsModel)
        {
            try
            {
                if (!DeleteSaveInProgress)
                {
                    LoadState((uint)saveSlotsModel.SlotID);
                    NoSavesActive = false;
                    RaisePropertyChanged(nameof(NoSavesActive));
                    SavesListActive = false;
                    RaisePropertyChanged(nameof(SavesListActive));
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }

        public bool DeleteSaveInProgress = false;
        private async void SaveHoldHandler(SaveSlotsModel saveSlotsModel)
        {
            try
            {
                if (saveSlotsModel == null)
                {
                    return;
                }
                string SlotFileName = saveSlotsModel.SlotFileName;
                string SnapshotFileName = saveSlotsModel.SnapshotFileName;
                string GameID = saveSlotsModel.GameID;
                int SlotID = saveSlotsModel.SlotID;
                IDirectoryInfo SavesLocation = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync($@"{SlotsSaveLocation}\{GameID}");
                if (!DeleteSaveInProgress)
                {
                    DeleteSaveInProgress = true;
                    PlatformService.PlayNotificationSound("alert.wav");
                    ConfirmConfig confirmSaveDelete = new ConfirmConfig();
                    confirmSaveDelete.SetTitle("Save Action");
                    confirmSaveDelete.SetMessage($"Do you want to delete the select slot?");
                    confirmSaveDelete.UseYesNo();
                    confirmSaveDelete.OkText = "Delete";
                    confirmSaveDelete.CancelText = "Cancel";
                    bool SaveDeleteState = await UserDialogs.Instance.ConfirmAsync(confirmSaveDelete);
                    if (SaveDeleteState)
                    {
                        var testSaveFile = await SavesLocation.GetFileAsync(SlotFileName);
                        var testSnapFile = await SavesLocation.GetFileAsync(SnapshotFileName);
                        if (testSaveFile != null)
                        {
                            await testSaveFile.DeleteAsync();
                        }
                        if (testSnapFile != null)
                        {
                            await testSnapFile.DeleteAsync();
                        }
                        GetSaveSlotsList(currentSlotsType);
                        DeleteSaveInProgress = false;
                    }
                    else
                    {
                        DeleteSaveInProgress = false;
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
                DeleteSaveInProgress = false;
            }
        }
        public void HideSavesList(object sender = null, EventArgs e = null)
        {
            SavesListActive = false;
            RaisePropertyChanged(nameof(SavesListActive));
            PlatformService.SetSavesListActive(false);
        }

        public async void ShowQuickSaves()
        {
            await GetSaveSlotsList(SLOTS_QUICK);
        }
        public async void ShowAutoSaves()
        {
            await GetSaveSlotsList(SLOTS_AUTO);
        }
        public async void ShowSlotsSaves()
        {
            await GetSaveSlotsList(SLOTS_GEN);
        }
        public async void ShowAllSaves()
        {
            await GetSaveSlotsList(SLOTS_ALL);
        }

        private async void LoadState(uint slotID)
        {
            try
            {
                GameIsLoadingState(true);
                PlatformService.PlayNotificationSound("button-01.mp3");
                CoreOperationsAllowed = false;
                bool loadState = await EmulationService.LoadGameStateAsync(slotID);
                if (loadState)
                {
                    if (slotID < 11)
                    {
                        UpdateInfoState("Game Loaded from Slot " + slotID);
                    }
                    else if (slotID < 21)
                    {
                        UpdateInfoState("Game Loaded from Quick Save ");
                    }
                    else if (slotID < 36)
                    {
                        UpdateInfoState("Game Loaded from Auto Save");
                    }
                }
                else
                {
                    if (slotID < 11)
                    {
                        UpdateInfoState("Slot " + slotID + " is empty!");
                    }
                    else if (slotID < 21)
                    {
                        UpdateInfoState("Load Quick save -> empty!");
                    }
                    else if (slotID < 36)
                    {
                        UpdateInfoState("Load Auto save -> empty!");
                    }
                    PlatformService.PlayNotificationSound("faild.wav");
                }


                if (GameIsPaused)
                {
                    //await TogglePause(true);
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            CoreOperationsAllowed = true;
            GameIsLoadingState(false);
        }

        public bool QuickSaveInProgress = false;
        public async Task QuickSaveState()
        {
            try
            {
                if (QuickSaveInProgress)
                {
                    return;
                }
                QuickSaveInProgress = true;
                RaisePropertyChanged(nameof(QuickSaveInProgress));
                string GameID = EmulationService.GetGameID().ToLower();
                IDirectoryInfo SavesLocation = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync($@"{SlotsSaveLocation}\{GameID}");
                bool foundEmptySlot = false;
                if (SavesLocation == null)
                {
                    await SaveState(20);
                    foundEmptySlot = true;
                }
                if (!foundEmptySlot)
                {
                    for (var i = 20; i >= 11; i--)
                    {
                        var testFileName = $"{GameID}_S{i}.png";
                        var testFile = await SavesLocation.GetFileAsync(testFileName);
                        if (testFile == null)
                        {
                            await SaveState((uint)i);
                            foundEmptySlot = true;
                            break;
                        }
                    }
                }

                if (!foundEmptySlot)
                {
                    var FilesList = await SavesLocation.EnumerateFilesAsync();
                    Dictionary<IFileInfo, long> GameSavesListTemp = new Dictionary<IFileInfo, long>();
                    foreach (var FileItem in FilesList)
                    {
                        var FileExtension = Path.GetExtension(FileItem.Name).ToLower();
                        if (FileExtension.Contains("png"))
                        {
                            continue;
                        }
                        var FileDate = await FileItem.GetLastModifiedAsync();
                        long FileDateSort = FileDate.UtcTicks;
                        for (int i = 11; i <= 20; i++)
                        {
                            var testName = $@"{GameID}_S{i}.";
                            if (FileItem.Name.Contains(testName))
                            {
                                GameSavesListTemp.Add(FileItem, FileDateSort);
                                break;
                            }
                        }
                    }
                    var sortedFilesList = GameSavesListTemp.OrderByDescending(key => key.Value).LastOrDefault();
                    await sortedFilesList.Key.DeleteAsync();
                    var SnapshotFileName = sortedFilesList.Key.Name.Replace(Path.GetExtension(sortedFilesList.Key.Name), ".png");
                    var SnapshotFile = await SavesLocation.GetFileAsync(SnapshotFileName);
                    if (SnapshotFile != null)
                    {
                        await SnapshotFile.DeleteAsync();
                    }
                    QuickSaveInProgress = false;
                    await QuickSaveState();
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            QuickSaveInProgress = false;
            RaisePropertyChanged(nameof(QuickSaveInProgress));
        }

        public void QuickLoadState()
        {
            GetSaveSlotsList(SLOTS_QUICK);
        }

        bool AutoSaveInProgress = false;
        public async Task AutoSaveState(bool showMessage = true)
        {
            try
            {
                if (AutoSaveInProgress)
                {
                    return;
                }
                bool foundEmptySlot = false;
                AutoSaveInProgress = true;
                string GameID = EmulationService.GetGameID().ToLower();
                IDirectoryInfo SavesLocation = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync($@"{SlotsSaveLocation}\{GameID}");
                if (SavesLocation == null)
                {
                    await SaveState(35, showMessage);
                    foundEmptySlot = true;
                }

                if (!foundEmptySlot)
                {
                    for (var i = 35; i >= 21; i--)
                    {
                        var testFileName = $"{GameID}_S{i}.png";
                        var testFile = await SavesLocation.GetFileAsync(testFileName);
                        if (testFile == null)
                        {
                            await SaveState((uint)i, showMessage);
                            foundEmptySlot = true;
                            break;
                        }
                    }
                }
                if (!foundEmptySlot)
                {
                    var FilesList = await SavesLocation.EnumerateFilesAsync();
                    Dictionary<IFileInfo, long> GameSavesListTemp = new Dictionary<IFileInfo, long>();
                    foreach (var FileItem in FilesList)
                    {
                        var FileExtension = Path.GetExtension(FileItem.Name).ToLower();
                        if (FileExtension.Contains("png"))
                        {
                            continue;
                        }
                        var FileDate = await FileItem.GetLastModifiedAsync();
                        long FileDateSort = FileDate.UtcTicks;
                        for (int i = 21; i <= 35; i++)
                        {
                            var testName = $@"{GameID}_S{i}.";
                            if (FileItem.Name.Contains(testName))
                            {
                                GameSavesListTemp.Add(FileItem, FileDateSort);
                                break;
                            }
                        }
                    }
                    var sortedFilesList = GameSavesListTemp.OrderByDescending(key => key.Value).LastOrDefault();
                    await sortedFilesList.Key.DeleteAsync();
                    var SnapshotFileName = sortedFilesList.Key.Name.Replace(Path.GetExtension(sortedFilesList.Key.Name), ".png");
                    var SnapshotFile = await SavesLocation.GetFileAsync(SnapshotFileName);
                    if (SnapshotFile != null)
                    {
                        await SnapshotFile.DeleteAsync();
                    }
                    await AutoSaveState();
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            AutoSaveInProgress = false;
        }

        private async void OnGameStateOperationRequested(object sender, GameStateOperationEventArgs args)
        {
            try
            {
                if (!CoreOperationsAllowed)
                {
                    return;
                }

                if (args.Type == GameStateOperationEventArgs.GameStateOperationType.Load)
                {
                    LoadState(args.SlotID);
                }
                else if (args.Type == GameStateOperationEventArgs.GameStateOperationType.Save)
                {
                    await SaveState(args.SlotID);
                }
                else
                {
                    if ((int)args.SlotID == 4)
                    {
                        ReverseLeftRight = !ReverseLeftRight;
                        RaisePropertyChanged(nameof(ReverseLeftRight));
                        if (ReverseLeftRight)
                        {
                            UpdateInfoState("Swap Left / Right On");
                        }
                        else
                        {
                            UpdateInfoState("Swap Left / Right Off");
                        }
                    }
                    else
                    {
                        await ExcuteActionsAsync((int)args.SlotID);
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        private async void PeriodicChecks(object sender, EventArgs e)
        {
            try
            {
                if (SystemInfoVisiblity)
                {
                    await Task.Delay(1500);
                }
                PreviewCurrentInfoState = false;
                SystemInfoVisiblity = false;
                PreviewCurrentInfo = "";
                RaisePropertyChanged(nameof(PreviewCurrentInfoState));
                RaisePropertyChanged(nameof(PreviewCurrentInfo));
                RaisePropertyChanged(nameof(SystemInfoVisiblity));
                callInfoTimer();
            }
            catch (Exception ex)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(ex);
                }
            }

        }


        public async Task CustomTouchPadStoreAsync()
        {
            try
            {
                GameIsLoadingState(true);
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(TouchPadSaveLocation);
                if (localFolder == null)
                {
                    await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(TouchPadSaveLocation);
                    localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(TouchPadSaveLocation);
                }

                var targetFileTest = await localFolder.GetFileAsync($"{SystemName}.rct");
                if (targetFileTest != null)
                {
                    await targetFileTest.DeleteAsync();
                }
                var targetFile = await localFolder.CreateFileAsync($"{SystemName}.rct");

                SystemCustomTouchPad customTouchPad = new SystemCustomTouchPad();
                customTouchPad.leftScaleFactorValueP = leftScaleFactorValueP;
                customTouchPad.leftScaleFactorValueW = leftScaleFactorValueW;
                customTouchPad.rightScaleFactorValueP = rightScaleFactorValueP;
                customTouchPad.rightScaleFactorValueW = rightScaleFactorValueW;
                customTouchPad.rightTransformXCurrentP = rightTransformXCurrentP;
                customTouchPad.rightTransformYCurrentP = rightTransformYCurrentP;
                customTouchPad.leftTransformXCurrentP = leftTransformXCurrentP;
                customTouchPad.leftTransformYCurrentP = leftTransformYCurrentP;
                customTouchPad.actionsTransformXCurrentP = actionsTransformXCurrentP;
                customTouchPad.actionsTransformYCurrentP = actionsTransformYCurrentP;

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(customTouchPad));
                using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                {
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }
                PlatformService.PlayNotificationSound("success.wav");
                await GeneralDialog($"Touch pad settings saved for {SystemNamePreview}");
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }
        public async void CustomTouchPadRetrieveAsync()
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(TouchPadSaveLocation);
                if (localFolder == null)
                {
                    return;
                }

                var targetFileTest = await localFolder.GetFileAsync($"{SystemName}.rct");
                if (targetFileTest != null)
                {
                    Encoding unicode = Encoding.Unicode;
                    byte[] result;
                    using (var outStream = await targetFileTest.OpenAsync(FileAccess.Read))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            outStream.CopyTo(memoryStream);
                            result = memoryStream.ToArray();
                        }
                        await outStream.FlushAsync();
                    }
                    string CoreFileContent = unicode.GetString(result);
                    var dictionaryList = JsonConvert.DeserializeObject<SystemCustomTouchPad>(CoreFileContent);

                    if (dictionaryList != null)
                    {
                        leftScaleFactorValueP = dictionaryList.leftScaleFactorValueP;
                        leftScaleFactorValueW = dictionaryList.leftScaleFactorValueW;
                        rightScaleFactorValueP = dictionaryList.rightScaleFactorValueP;
                        rightScaleFactorValueW = dictionaryList.rightScaleFactorValueW;
                        rightTransformXCurrentP = dictionaryList.rightTransformXCurrentP;
                        rightTransformYCurrentP = dictionaryList.rightTransformYCurrentP;
                        leftTransformXCurrentP = dictionaryList.leftTransformXCurrentP;
                        leftTransformYCurrentP = dictionaryList.leftTransformYCurrentP;
                        actionsTransformXCurrentP = dictionaryList.actionsTransformXCurrentP;
                        actionsTransformYCurrentP = dictionaryList.actionsTransformYCurrentP;
                        RefereshCustomizationValues();
                    }

                }

            }
            catch (Exception e)
            {

            }
        }

        public async Task CustomTouchPadDeleteAsync()
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(TouchPadSaveLocation);
                if (localFolder == null)
                {
                    PlatformService.PlayNotificationSound("faild.wav");
                    await GeneralDialog($"Customization for {SystemNamePreview} not found!");
                    return;
                }

                var targetFileTest = await localFolder.GetFileAsync($"{SystemName}.rct");
                if (targetFileTest != null)
                {
                    GameIsLoadingState(true);
                    await targetFileTest.DeleteAsync();
                    PlatformService.PlayNotificationSound("success.wav");
                    await GeneralDialog($"Customization for {SystemNamePreview} deleted\nGlobal customization will be used");
                    try
                    {
                        leftScaleFactorValueP = (float)Settings.GetValueOrDefault(nameof(leftScaleFactorValueP), 1f);
                        leftScaleFactorValueW = (float)Settings.GetValueOrDefault(nameof(leftScaleFactorValueW), 1f);
                        rightScaleFactorValueP = (float)Settings.GetValueOrDefault(nameof(rightScaleFactorValueP), 1f);
                        rightScaleFactorValueW = (float)Settings.GetValueOrDefault(nameof(rightScaleFactorValueW), 1f);
                    }
                    finally
                    {

                    }
                    try
                    {
                        rightTransformXCurrentP = (double)Settings.GetValueOrDefault(nameof(rightTransformXCurrentP), 0.0);
                        rightTransformYCurrentP = (double)Settings.GetValueOrDefault(nameof(rightTransformYCurrentP), 0.0);

                        leftTransformXCurrentP = (double)Settings.GetValueOrDefault(nameof(leftTransformXCurrentP), 0.0);
                        leftTransformYCurrentP = (double)Settings.GetValueOrDefault(nameof(leftTransformYCurrentP), 0.0);

                        actionsTransformXCurrentP = (double)Settings.GetValueOrDefault(nameof(actionsTransformXCurrentP), 0.0);
                        actionsTransformYCurrentP = (double)Settings.GetValueOrDefault(nameof(actionsTransformYCurrentP), 0.0);
                    }
                    finally
                    {

                    }
                    RefereshCustomizationValues();
                }
                else
                {
                    PlatformService.PlayNotificationSound("faild.wav");
                    await GeneralDialog($"Customization for {SystemNamePreview} not found!");
                }

            }
            catch (Exception e)
            {

            }
            GameIsLoadingState(false);
        }

        public void RefereshCustomizationValues()
        {
            RaisePropertyChanged(nameof(LeftScaleFactorValue));
            RaisePropertyChanged(nameof(RightScaleFactorValue));
            RaisePropertyChanged(nameof(RightTransformXCurrent));
            RaisePropertyChanged(nameof(RightTransformYCurrent));
            RaisePropertyChanged(nameof(LeftTransformXCurrent));
            RaisePropertyChanged(nameof(LeftTransformYCurrent));
            RaisePropertyChanged(nameof(ActionsTransformXCurrent));
            RaisePropertyChanged(nameof(ActionsTransformYCurrent));
        }

        private async void QuickSaveKey(object sender, EventArgs args)
        {
            try
            {
                await QuickSaveState();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        private async void SavesListKey(object sender, EventArgs args)
        {
            try
            {
                ShowSavesList.Execute();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public bool MenuGridActive = false;
        private async void OnXBoxMenuKey(object sender, EventArgs args)
        {
            try
            {
                ToggleMenuGridActive();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }

        }
        public async void ToggleMenuGridActive()
        {
            try
            {

                PlatformService.PlayNotificationSound("select.mp3");
                MenuGridActive = !MenuGridActive;
                if (MenuGridActive)
                {
                    PrepareXBoxMenu();
                    if (!EmulationService.CorePaused)
                    {
                        await EmulationService.PauseGameAsync();
                    }
                }
                else
                {
                    if (EmulationService.CorePaused)
                    {
                        await EmulationService.ResumeGameAsync();
                    }
                }
                RaisePropertyChanged(nameof(MenuGridActive));

            }
            catch (Exception ex)
            {

            }
        }
        public async void HideMenuGrid(object sender = null, EventArgs e = null)
        {
            try
            {
                MenuGridActive = false;
                RaisePropertyChanged(nameof(MenuGridActive));
                PlatformService.PlayNotificationSound("option-changed.wav");
                if (EmulationService.CorePaused && !RequestKeepPaused)
                {
                    await EmulationService.ResumeGameAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public string getAssetsIcon(string IconPath, string FolderName = "Menus")
        {
            return $"ms-appx:///Assets/{FolderName}/{IconPath}";
        }

        GroupMenuGrid ControlsMenu, SavesMenu, RenderThreads, AdvancedMenu, MemoryMenu, ScreenMenu, AudioMenu, OverlaysMenu, RenderMenu, ColorModeMenu, DebugMenu;
        public ObservableCollection<GroupMenuGrid> MenusGrid = new ObservableCollection<GroupMenuGrid>();
        public void PrepareXBoxMenu()
        {
            MenusGrid.Clear();
            //Control
            ControlsMenu = new GroupMenuGrid();
            ControlsMenu.Key = "Controls";
            ControlsMenu.Add(AddNewMenu(EmulationService.CorePaused ? "Resume" : "Pause", EmulationService.CorePaused ? "play.png" : "pause.png", "pause"));
            ControlsMenu.Add(AddNewMenu("Stop", "stop.png", "stop"));
            ControlsMenu.Add(AddNewMenu("Gamepad", "controls.png", "controls"));
            ControlsMenu.Add(AddNewMenu("Quick Save", "quicksave.png", "quicksave"));
            if (SensorsMovementActive)
            {
                ControlsMenu.Add(AddNewMenu("Sensors", "sensors.png", "sensors", true, SensorsMovement));
            }
            ControlsMenu.Add(AddNewMenu("Close Menu", "close.png", "close"));
            MenusGrid.Add(ControlsMenu);

            //Saves
            SavesMenu = new GroupMenuGrid();
            SavesMenu.Key = "Save";
            SavesMenu.Add(AddNewMenu("Saves List", "saves.png", "saves"));
            SavesMenu.Add(AddNewMenu("Auto (30 Sec)", "timer30.png", "asave30", true, AutoSave30Sec));
            SavesMenu.Add(AddNewMenu("Auto (1 Min)", "timer60.png", "asave1", true, AutoSave60Sec));
            SavesMenu.Add(AddNewMenu("Auto(1.5 Min)", "timer95.png", "asave15", true, AutoSave90Sec));
            SavesMenu.Add(AddNewMenu("Auto Notify", "timernotify.png", "asaven", true, AutoSaveNotify));
            MenusGrid.Add(SavesMenu);

            //Render
            RenderThreads = new GroupMenuGrid();
            RenderThreads.Key = "Performance";
            RenderThreads.Add(AddNewMenu("Skip Frames", "skipframes.png", "skipframes", true, SkipFrames));
            RenderThreads.Add(AddNewMenu("Wait Threads", "threadswait.png", "threadswait", true, !FramebufferConverter.DontWaitThreads));
            RenderThreads.Add(AddNewMenu("1 Thread", "threadsnone.png", "threadsnone", true, RCore1));
            RenderThreads.Add(AddNewMenu("2 Threads", "threads2.png", "threads2", true, RCore2));
            RenderThreads.Add(AddNewMenu("4 Threads", "threads4.png", "threads4", true, RCore4));
            RenderThreads.Add(AddNewMenu("8 Threads", "threads8.png", "threads8", true, RCore8));
            MenusGrid.Add(RenderThreads);

            //On Screen
            AdvancedMenu = new GroupMenuGrid();
            AdvancedMenu.Key = "Advanced";
            AdvancedMenu.Add(AddNewMenu("FPS Counter", "fps.png", "fps", true, ShowFPSCounter));
            AdvancedMenu.Add(AddNewMenu("Crazy Buffer", "cbuf.png", "cbuf", true, CrazyBufferActive));
            if (InGameOptionsActive)
            {
                AdvancedMenu.Add(AddNewMenu("Core Options", "core.png", "coreoptions"));
            }
            MenusGrid.Add(AdvancedMenu);


            //Effects
            ColorModeMenu = new GroupMenuGrid();
            ColorModeMenu.Key = "Video Effects";
            ColorModeMenu.Add(AddNewMenu("None", "bw.png", "creset", true, VideoService.TotalEffects() == 0));
            ColorModeMenu.Add(AddNewMenu("Show Effects", "sepia.png", "csepia", true, VideoService.TotalEffects() > 0));
            ColorModeMenu.Add(AddNewMenu("Set Overlays", "none.png", "overlays", true, AddOverlays));
            ColorModeMenu.Add(AddNewMenu("Set Shaders", "retro.png", "shaders", true, AddShaders));
            MenusGrid.Add(ColorModeMenu);


            //Memory Helpers
            if (isMemoryHelpersVisible)
            {
                MemoryMenu = new GroupMenuGrid();
                MemoryMenu.Key = "Memory Helpers";
                MemoryMenu.Add(AddNewMenu("Buffer.CopyMemory", "memory.png", "membcpy", true, BufferCopyMemory));
                MemoryMenu.Add(AddNewMenu("memcpy (msvcrt)", "memory.png", "memcpy", true, memCPYMemory));
                MemoryMenu.Add(AddNewMenu("Marshal.CopyTo", "memory.png", "memmarsh", true, MarshalMemory));
                MemoryMenu.Add(AddNewMenu("Span.CopyTo", "memory.png", "memspan", true, SpanlMemory));
                MemoryMenu.Add(AddNewMenu("Help", "help.png", "memhelp"));
                MenusGrid.Add(MemoryMenu);
            }
            //Rotate
            ScreenMenu = new GroupMenuGrid();
            ScreenMenu.Key = "Screen";
            ScreenMenu.Add(AddNewMenu("Rotate Right", "right.png", "rright", true, RotateDegreePlusActive));
            ScreenMenu.Add(AddNewMenu("Rotate Left", "left.png", "rleft", true, RotateDegreeMinusActive));
            MenusGrid.Add(ScreenMenu);

            //Audio
            AudioMenu = new GroupMenuGrid();
            AudioMenu.Key = "Audio";
            AudioMenu.Add(AddNewMenu("Volume Mute", "mute.png", "vmute", true, AudioMuteLevel));
            AudioMenu.Add(AddNewMenu("Volume High", "high.png", "vhigh", true, AudioHighLevel));
            AudioMenu.Add(AddNewMenu("Volume Default", "default.png", "vdefault", true, AudioNormalLevel));
            AudioMenu.Add(AddNewMenu("Volume Low", "low.png", "vlow", true, AudioLowLevel));
            AudioMenu.Add(AddNewMenu("Echo Effect", "echo.png", "aecho", true, AudioEcho));
            MenusGrid.Add(AudioMenu);

            //Overlays
            OverlaysMenu = new GroupMenuGrid();
            OverlaysMenu.Key = "Overlays";
            OverlaysMenu.Add(AddNewMenu("Overlay Lines", "lines.png", "ovlines", true, ScanLines3));
            OverlaysMenu.Add(AddNewMenu("Overlay Grid", "grid.png", "ovgrid", true, ScanLines2));
            MenusGrid.Add(OverlaysMenu);

            //Render
            RenderMenu = new GroupMenuGrid();
            RenderMenu.Key = "Render";
            RenderMenu.Add(AddNewMenu("Nearest", "near.png", "rnearest", true, NearestNeighbor));
            RenderMenu.Add(AddNewMenu("Linear", "line.png", "rlinear", true, Linear));
            RenderMenu.Add(AddNewMenu("MultiSample", "multi.png", "rmultisample", true, MultiSampleLinear));
            MenusGrid.Add(RenderMenu);

            //Debug
            DebugMenu = new GroupMenuGrid();
            DebugMenu.Key = "Debug";
            DebugMenu.Add(AddNewMenu("Log List", "logs.png", "loglist"));
            RenderThreads.Add(AddNewMenu("Pixels Update", "dim.png", "skipcache", true, SkipCached));
            DebugMenu.Add(AddNewMenu("Close Menu", "close.png", "close"));
            MenusGrid.Add(DebugMenu);
        }
        public SystemMenuModel AddNewMenu(string Name, string Icon, string Command, bool SwitchState = false, bool SwitchValue = false)
        {
            SystemMenuModel MenuCommand = new SystemMenuModel(Name, getAssetsIcon(Icon), Command, SwitchState, SwitchValue);
            return MenuCommand;
        }
        public IMvxCommand<SystemMenuModel> GameSystemMenuSelected { get; }
        bool RequestKeepPaused = false;
        private async void GameSystemMenuHandler(SystemMenuModel systemMenuModel)
        {
            try
            {
                RequestKeepPaused = false;
                PlatformService.PlayNotificationSound("button-01.mp3");
                switch (systemMenuModel.MenuCommand)
                {
                    case "controls":
                        SetControlsMapVisible.Execute();
                        RequestKeepPaused = true;
                        break;

                    case "sensors":
                        SetSensorsMovement.Execute();
                        break;

                    case "coreoptions":
                        SetCoreOptionsVisible.Execute();
                        break;

                    case "pause":
                        TogglePauseCommand.Execute();
                        RequestKeepPaused = true;
                        break;

                    case "stop":
                        StopCommand.Execute();
                        break;

                    case "quicksave":
                        await QuickSaveState();
                        break;

                    case "dimcache":
                        SetUpdatesOnly.Execute();
                        break;

                    case "skipcache":
                        SetSkipCached.Execute();
                        break;

                    case "skipframes":
                        SetSkipFrames.Execute();
                        break;

                    case "threadswait":
                        DontWaitThreads.Execute();
                        break;
                    case "threadsnone":
                        RCore1 = true;
                        SetRCore.Execute();
                        break;

                    case "threads2":
                        RCore2 = true;
                        SetRCore.Execute();
                        break;
                    case "threads4":
                        RCore4 = true;
                        SetRCore.Execute();
                        break;
                    case "threads8":
                        RCore8 = true;
                        SetRCore.Execute();
                        break;

                    case "saves":
                        ShowSavesList.Execute();
                        break;
                    case "asave30":
                        SetAutoSave30Sec.Execute();
                        break;
                    case "asave1":
                        SetAutoSave60Sec.Execute();
                        break;
                    case "asave15":
                        SetAutoSave90Sec.Execute();
                        break;
                    case "asaven":
                        SetAutoSaveNotify.Execute();
                        break;

                    case "rright":
                        SetRotateDegreePlus.Execute();
                        break;
                    case "rleft":
                        SetRotateDegreeMinus.Execute();
                        break;

                    case "fps":
                        ShowFPSCounterCommand.Execute();
                        break;

                    case "cbuf":
                        SetCrazyBufferActive.Execute();
                        break;

                    case "vmute":
                        SetAudioLevelMute.Execute();
                        break;
                    case "vhigh":
                        SetAudioLevelHigh.Execute();
                        break;
                    case "vdefault":
                        SetAudioLevelNormal.Execute();
                        break;
                    case "vlow":
                        SetAudioLevelLow.Execute();
                        break;

                    case "aecho":
                        SetAudioEcho.Execute();
                        break;

                    case "ovlines":
                        SetScanlines3.Execute();
                        break;
                    case "ovgrid":
                        SetScanlines2.Execute();
                        break;

                    case "rnearest":
                        SetNearestNeighbor.Execute();
                        break;
                    case "rlinear":
                        SetLinear.Execute();
                        break;
                    case "rmultisample":
                        SetMultiSampleLinear.Execute();
                        break;

                    case "creset":
                        ClearAllEffects.Execute();
                        break;
                    case "cbw":
                        SetColorFilterGrayscale.Execute();
                        break;
                    case "csepia":
                        ShowAllEffects.Execute();
                        break;
                    case "shaders":
                        AddShaders = !AddShaders;
                        break;
                    case "overlays":
                        AddOverlays = !AddOverlays;
                        break;

                    case "loglist":
                        SetShowLogsList.Execute();
                        break;

                    case "membcpy":
                        BufferCopyMemory = true;
                        break;
                    case "memcpy":
                        memCPYMemory = true;
                        break;
                    case "memmarsh":
                        MarshalMemory = true;
                        break;
                    case "memspan":
                        SpanlMemory = true;
                        break;

                    case "memhelp":
                        PlatformService.PlayNotificationSound("notice.mp3");
                        GeneralDialog("Huh, You never heard about these stuff?\nWell, these are methods used to deal with the memory.\nI added multiple options so you can try whatever you want and choose what is the best for your device.");
                        break;

                    case "close":
                        HideMenuGrid();
                        break;
                    default:
                        break;
                }
                if (!systemMenuModel.MenuSwitch)
                {
                    HideMenuGrid();
                }
                else
                {
                    HideMenuGrid();
                    //PrepareXBoxMenu();
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }
    }

    public class GameIDArgs : EventArgs
    {
        public string GameID { get; set; }
        public IDirectoryInfo SaveLocation;
        public GameIDArgs(string gameID, IDirectoryInfo saveLocation)
        {
            this.GameID = gameID;
            this.SaveLocation = saveLocation;
        }
    }
    public class GroupMenuGrid : List<SystemMenuModel>
    {
        public string Key { get; set; }
    }

}
