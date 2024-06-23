using LibRetriX;
using LibRetriX.RetroBindings;
using Newtonsoft.Json;
using Plugin.Settings.Abstractions;
using RetriX.Shared.Components;
using RetriX.Shared.Models;
using RetriX.Shared.Services;
using RetriX.UWP;
using RetriX.UWP.Pages;
using RetriX.UWP.Services;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Foundation;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinUniversalTool;
using static RetriX.UWP.Services.PlatformService;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Graphics.Canvas.Effects;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.Shared.ViewModels
{
    public class GamePlayerViewModel : BindableBase, IDisposable
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


        //Just to avoid crash on Windows Phone in case two dialogs appears at once
        #region DIALOG
        bool isDialogInProgress
        {
            get
            {
                return Helpers.DialogInProgress;
            }
            set
            {
                Helpers.DialogInProgress = value;
            }
        }
        public async Task GeneralDialog(string Message, string title = null, string okButton = null)
        {
            if (isDialogInProgress)
            {
                UpdateInfoState(Message);
                return;
            }
            try
            {
                await PlatformService.ShowMessageWithTitleDirect(Message, title, okButton);
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        //Memory Helpers
        #region MEMORY
        public bool CrazyBufferActive = true;
        public bool isMemoryHelpersVisible
        {
            get
            {
                return FramebufferConverter.isRGB888;
            }
        }
        public bool isCrazyBufferVisible
        {
            get
            {
                return !FramebufferConverter.isRGB888;
            }
        }
        private bool bufferCopyMemory = false;
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
                        PlatformService.PlayNotificationSound("success");
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
                        PlatformService.PlayNotificationSound("root-needed");
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
                        PlatformService.PlayNotificationSound("success");
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
                        PlatformService.PlayNotificationSound("root-needed");
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
                        PlatformService.PlayNotificationSound("success");
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

        private bool spanMemory = true;
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
                    if (isGameStarted && !memoryOptionsInitial)
                    {
                        PlatformService.PlayNotificationSound("success");
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
            BufferCopyMemory = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BufferCopyMemory", false);
            memCPYMemory = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("memCPYMemory", false);
            MarshalMemory = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("MarshalMemory", false);
            SpanlMemory = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("SpanlMemory", true);
        }
        #endregion
        //Memory Helpers

        //Effects System
        public bool EffectsVisible = false;
        public bool addShaders = false;
        public bool addShadersInitial = false;
        bool fastForward = false;
        public bool FastForward
        {
            get
            {
                return fastForward;
            }
            set
            {
                fastForward = value;
                EmulationService.currentCore.fastForwardState = fastForward;
                RaisePropertyChanged(nameof(FastForward));
            }
        }
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
                    {
                        UpdateEffect("PixelShaderEffect", false, null);
                        PlatformService.PlayNotificationSound("success");
                        UpdateInfoState($"Disabled: PixelShaderEffect");
                    }
                    RaisePropertyChanged(nameof(AddShaders));
                }
            }
        }
        bool shaderInProgress = false;
        public async Task GetShader()
        {
            if (shaderInProgress)
            {
                return;
            }
            shaderInProgress = true;
            try
            {
                PlatformService.PlayNotificationSound("root-needed");
                await GeneralDialog("Important: This is Pixel shader and not Vertex shader, it's in very early development..\nI added this option for testing only\n\nNote: Required shaders compiled as .bin");

                GameIsLoadingState(true);
                var shader = await PlatformService.getShader();
                if (shader != null)
                {
                    UpdateEffect("PixelShaderEffect", true, shader);
                    PlatformService.PlayNotificationSound("success");
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
                    /*if (value)
                    {
                        GetOverlay();
                    }
                    else
                    {
                        ClearOverLaysFolder();
                        UpdateEffect("OverlayEffect", false, null);
                        PlatformService.PlayNotificationSound("success");
                        UpdateInfoState($"Disabled: OverlayEffect");
                    }*/
                    RaisePropertyChanged(nameof(AddOverlays));
                }
            }
        }
        public async void ClearOverLaysFolder()
        {
            try
            {
                var overLaysFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Overlays", CreationCollisionOption.ReplaceExisting);
                UpdateEffect("OverlayEffect", false, null);
                AddOverlays = false;
            }
            catch (Exception ex)
            {

            }
        }
        public bool overlayInProgress = false;
        List<byte[]> overlay;
        public async Task GetOverlay(List<StorageFile> storageFiles = null, bool reselect = false)
        {
            if (overlayInProgress && storageFiles == null)
            {
                return;
            }
            overlayInProgress = true;
            try
            {
                /*if (storageFiles == null && !reselect)
                {
                    PlatformService.PlayNotificationSound("root-needed");
                    await GeneralDialog("Note: You can select multiple overlays");
                }*/
                GameIsLoadingState(true);
                var files = storageFiles != null ? storageFiles : await PlatformService.getOverlay();

                //Save overlays first
                if (files != null)
                {
                    List<byte[]> byteArrays = new List<byte[]>();
                    if (storageFiles == null)
                    {
                        var overLaysFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Overlays", CreationCollisionOption.ReplaceExisting);
                        overlay = new List<byte[]>();
                        foreach (var fItem in files)
                        {
                            try
                            {
                                byte[] resultInBytes = (await FileIO.ReadBufferAsync(fItem)).ToArray();
                                overlay.Add(resultInBytes);
                                await fItem.CopyAsync(overLaysFolder, fItem.Name, NameCollisionOption.ReplaceExisting);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    else
                    {
                        overlay = new List<byte[]>();
                        foreach (var fItem in files)
                        {
                            try
                            {
                                byte[] resultInBytes = (await FileIO.ReadBufferAsync(fItem)).ToArray();
                                overlay.Add(resultInBytes);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    if (overlay != null && overlay.Count > 0)
                    {
                        {
                            if (PlatformService.BlendModeGlobal != -1)
                            {
                                int currentBlendMode = PlatformService.BlendModeGlobal;
                                UpdateEffect("OverlayEffect", true, overlay, currentBlendMode);
                                AddOverlays = true;

                                if (storageFiles == null)
                                {
                                    PlatformService.PlayNotificationSound("success");
                                    UpdateInfoState($"OverlayEffect + BlendMode: {(BlendEffectMode)currentBlendMode}");
                                }
                            }
                            else
                            {
                                UpdateEffect("OverlayEffect", true, overlay);
                                AddOverlays = true;

                                if (storageFiles == null)
                                {
                                    PlatformService.PlayNotificationSound("success");
                                    UpdateInfoState($"Activated: OverlayEffect");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!reselect)
                        {
                            AddOverlays = false;
                        }
                    }
                }
                else
                {
                    if (!reselect)
                    {
                        AddOverlays = false;
                    }
                }
            }
            catch (Exception ex)
            {
                AddOverlays = false;
            }
            GameIsLoadingState(false);
            overlayInProgress = false;
        }
        private void ReloadOverlayEffect(object sender, EventArgs e)
        {
            try
            {
                if (overlayInProgress)
                {
                    return;
                }
                overlayInProgress = true;
                GameIsLoadingState(true);
                if (overlay != null)
                {
                    int currentBlendMode = PlatformService.BlendModeGlobal;
                    bool currentBlendModeState = (bool)sender;
                    if (currentBlendModeState)
                    {
                        UpdateEffect("OverlayEffect", true, overlay, currentBlendMode);
                        PlatformService.PlayNotificationSound("success");
                        UpdateInfoState($"BlendMode: {(BlendEffectMode)currentBlendMode}");
                    }
                    else
                    {
                        UpdateEffect("OverlayEffect", true, overlay);
                        PlatformService.PlayNotificationSound("success");
                        UpdateInfoState($"Disabled: BlendMode");
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
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

        private async Task SyncEffectsSettings()
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
                if (Left > FramebufferConverter.currentWidth)
                {
                    Left = 0;
                }
                Top = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Top", top);
                if (Top > FramebufferConverter.currentHeight)
                {
                    Top = 0;
                }
                Right = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Right", right);
                if (Right > FramebufferConverter.currentWidth || Right == 0)
                {
                    Right = FramebufferConverter.currentWidth;
                }
                Bottom = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("Bottom", bottom);
                if (Bottom > FramebufferConverter.currentHeight || Bottom == 0)
                {
                    Bottom = FramebufferConverter.currentHeight;
                }

                //CropEffect 
                CropEffect = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("CropEffect", false);
                LeftCrop = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("LeftCrop", leftCrop);
                if (LeftCrop > FramebufferConverter.currentWidth)
                {
                    LeftCrop = 0;
                }
                TopCrop = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("TopCrop", topCrop);
                if (TopCrop > FramebufferConverter.currentHeight)
                {
                    TopCrop = 0;
                }
                RightCrop = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RightCrop", rightCrop);
                if (RightCrop > FramebufferConverter.currentWidth || RightCrop == 0)
                {
                    RightCrop = FramebufferConverter.currentWidth;
                }
                BottomCrop = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BottomCrop", bottomCrop);
                if (BottomCrop > FramebufferConverter.currentHeight || BottomCrop == 0)
                {
                    BottomCrop = FramebufferConverter.currentHeight;
                }

            }
            catch (Exception ex)
            {

            }
            try
            {
                var overLaysFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Overlays");
                if (overLaysFolder != null)
                {
                    var files = await overLaysFolder.GetFilesAsync();
                    if (files != null && files.Count > 0)
                    {
                        GetOverlay(files.ToList());
                    }
                }
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
            //AddShaders = false;
            //AddOverlays = false;
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

            PlatformService.PlayNotificationSound("success");
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

        private void UpdateEffect(string EffectName, bool EffectState, List<byte[]> EffectValue1, int currentBlendMode = -1)
        {
            VideoService.SetEffect(EffectName, EffectState, EffectValue1, -1, currentBlendMode);
            if (EffectState)
            {
                forceReloadLogsList = true;
            }
        }

        public ICommand ConfimeArcadeGame { get; }
        public ICommand ShowAllEffects { get; }
        public ICommand ClearAllEffects { get; }

        //Effects System

        public int TopBarZIndex = 0;
        private float topBarOpacity = 1.0f;
        public float TopBarOpacity
        {
            get
            {
                return topBarOpacity;
            }
            set
            {
                topBarOpacity = value;
                RaisePropertyChanged(nameof(TopBarOpacity));
                if (value > 0)
                {
                    TopBarZIndex = 0;
                }
                else
                {
                    TopBarZIndex = -100;
                }
                RaisePropertyChanged(nameof(TopBarZIndex));
            }
        }

        private float touchPadOpacity = 1.0f;
        public float TouchPadOpacity
        {
            get
            {
                return touchPadOpacity;
            }
            set
            {
                touchPadOpacity = value;
                RaisePropertyChanged(nameof(TouchPadOpacity));
            }
        }
        public string LeftControlsScaleText = "Left Scale:";
        public string RightControlsScaleText = "Right Scale:";
        public int LeftMaxWidth = 1000;
        public int LeftMinWidth = 155;
        public int RightMaxWidth = 1000;
        public int RightMinWidth = 230;

        public HorizontalAlignment LeftControlsAlignment = HorizontalAlignment.Left;
        public HorizontalAlignment RightControlsAlignment = HorizontalAlignment.Right;
        public int LeftControlsColumn = 0;
        public int RightControlsColumn = 1;
        public Point LeftControlsTransformOrigin = new Point(0, 1);
        public Point RightControlsTransformOrigin = new Point(1, 1);

        public void SwapControls()
        {
            try
            {
                if (LeftControlsColumn == 0)
                {
                    LeftControlsAlignment = HorizontalAlignment.Right;
                    RightControlsAlignment = HorizontalAlignment.Left;
                    LeftControlsColumn = 1;
                    RightControlsColumn = 0;
                    LeftControlsTransformOrigin = new Point(1, 1);
                    RightControlsTransformOrigin = new Point(0, 1);
                    LeftControlsScaleText = "Right Scale:";
                    RightControlsScaleText = "Left Scale:";
                    LeftMaxWidth = 170;
                    LeftMinWidth = 160;
                    RightMaxWidth = 180;
                    RightMinWidth = 180;
                }
                else
                {
                    LeftControlsAlignment = HorizontalAlignment.Left;
                    RightControlsAlignment = HorizontalAlignment.Right;
                    LeftControlsColumn = 0;
                    RightControlsColumn = 1;
                    LeftControlsTransformOrigin = new Point(0, 1);
                    RightControlsTransformOrigin = new Point(1, 1);
                    LeftControlsScaleText = "Left Scale:";
                    RightControlsScaleText = "Right Scale:";
                    LeftMaxWidth = 1000;
                    LeftMinWidth = 180;
                    RightMaxWidth = 1000;
                    RightMinWidth = 230;
                }
                RaisePropertyChanged(nameof(LeftControlsAlignment));
                RaisePropertyChanged(nameof(RightControlsAlignment));
                RaisePropertyChanged(nameof(LeftControlsColumn));
                RaisePropertyChanged(nameof(RightControlsColumn));
                RaisePropertyChanged(nameof(LeftControlsTransformOrigin));
                RaisePropertyChanged(nameof(RightControlsTransformOrigin));
                RaisePropertyChanged(nameof(LeftControlsScaleText));
                RaisePropertyChanged(nameof(RightControlsScaleText));
                RaisePropertyChanged(nameof(LeftMaxWidth));
                RaisePropertyChanged(nameof(LeftMinWidth));
                RaisePropertyChanged(nameof(RightMaxWidth));
                RaisePropertyChanged(nameof(RightMinWidth));
            }
            catch (Exception ex)
            {

            }
        }

        public double SX = 1;
        public double SY = 1;
        public double PX = 0;
        public double PY = 0;

        public void updatePanelSize()
        {
            try
            {
                RaisePropertyChanged(nameof(SX));
                RaisePropertyChanged(nameof(SY));
                RaisePropertyChanged(nameof(PX));
                RaisePropertyChanged(nameof(PY));
            }
            catch (Exception ex)
            {

            }
        }

        private const string ForceDisplayTouchGamepadKey = "ForceDisplayTouchGamepad";
        private const string CurrentFilterKey = "CurrentFilter";
        public int FitScreen = 2;
        public int ScreenRow = 0;
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
        public bool ShowArcaeConfirm = false;
        public bool IsArcaeConfirm = false;
        public bool WaitThreadsEnable = false;
        public bool ScreenState = false;

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
                    WaitThreadsEnable = false;
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
                    WaitThreadsEnable = true;
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
                    WaitThreadsEnable = true;
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
                    WaitThreadsEnable = true;
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
                    WaitThreadsEnable = true;
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
                    WaitThreadsEnable = true;
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
                    WaitThreadsEnable = true;
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
                    WaitThreadsEnable = true;
                }
            }
        }


        public float buttonsOpacity = 0.50f;
        public float ButtonsSubOpacity
        {
            get
            {
                var subValue = buttonsOpacity - 0.15f;
                if (subValue < 0.0f)
                {
                    return buttonsOpacity;
                }
                return subValue;
            }
        }
        public float ButtonsOpacity
        {
            get
            {
                return buttonsOpacity;
            }
            set
            {
                buttonsOpacity = value;
                RaisePropertyChanged(nameof(ButtonsOpacity));
                RaisePropertyChanged(nameof(ButtonsSubOpacity));
                if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(ButtonsOpacity), value);
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
        //public string SlotsSaveLocation = "SaveStates";
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
        public ObservableCollection<CoreLogItem> LogsList = new ObservableCollection<CoreLogItem>();
        public bool ShowLogsList = false;
        public bool GameStopInProgress = false;
        public bool FPSInProgress = false;
        public bool LogInProgress = false;
        public bool AutoSave = false;
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
        public bool SettingsBar = false;
        public bool ButtonsIsLoading = true;
        public int RotateDegree = 0;
        private Timer FPSTimer, LogTimer, InfoTimer, AutoSaveTimer, PlayTimeTimer, BufferTimer, GCTimer, XBoxModeTimer;
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
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () => AutoSaveWorker());
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
                        await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () => updateFPSCaller());
                    }
                }
            }
            catch (Exception ex)
            {
                //PlatformService.ShowErrorMessage(ex);
            }
        }


        public string txtXAxis = "0,0";
        public string txtYAxis = "0,0";
        public string txtZAxis = "0,0";

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
                        await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () => updateGCCaller());
                    }
                    if (ShowSensorsInfo)
                    {
                        try
                        {
                            txtMemory = PlatformService.GetMemoryUsage();
                            RaisePropertyChanged(nameof(txtMemory));
                            RaisePropertyChanged(nameof(txtXAxis));
                            RaisePropertyChanged(nameof(txtYAxis));
                            RaisePropertyChanged(nameof(txtZAxis));
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
                PlatformService.ShowErrorMessage(ex);
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

        public void UpdateXBoxMode(object sender, EventArgs e)
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
                PlatformService.ShowErrorMessage(ex);
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
                        await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () => updateBufferCaller());
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
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
                        await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () => updateLogListCaller());
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
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
                PlatformService.ShowErrorMessage(ex);
            }
            AutoSaveWorkerInProgress = false;
        }

        private int LogListSizeTemp = 0;
        public bool FPSErrorCatched = false;
        public bool LogErrorCatched = false;
        public bool forceReloadLogsList = false;
        private bool EnabledDebugLogsListUpdate = false;
        private bool enabledDebugLogsList = false;
        public void clearListLogs()
        {
            EmulationService.currentCore.ClearLogs();
            forceReloadLogsList = true;
        }
        public bool EnabledDebugLogsList
        {
            get
            {
                return enabledDebugLogsList;
            }
            set
            {
                try
                {
                    enabledDebugLogsList = value;
                    EmulationService.UpdateCoreDebugState(enabledDebugLogsList);
                    RaisePropertyChanged(nameof(EnabledDebugLogsList));
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("EnabledDebugLogsList", EnabledDebugLogsList);
                    if (EnabledDebugLogsList && !EnabledDebugLogsListUpdate)
                    {
                        PlatformService.PlayNotificationSound("root-needed");
                        GeneralDialog($"Debug log could cause heavy performance\nTurn it off when it's not important", "Core Debug");
                    }
                    EnabledDebugLogsListUpdate = false;
                }
                catch (Exception ex)
                {
                    PlatformService.ShowErrorMessage(ex);
                }
            }
        }
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
                                var PixelsType = (FramebufferConverter.isRGB888 ? "XRGB8888" : (FramebufferConverter.isRG565 ? "RGB565" : "RGB555"));
                                var UsingMemoryCopy = (FramebufferConverter.isRGB888 ? (FramebufferConverter.MemoryHelper + (FramebufferConverter.SkipCached ? " (Ignored due Pixels updates feature)" : "")) : "Memory Pointers");
                                var RenderCores = $"{FramebufferConverter.CoresCount} Thread{(FramebufferConverter.CoresCount > 1 ? "s" : "")}";
                                var CurrentSize = $"{FramebufferConverter.currentWidth} x {FramebufferConverter.currentHeight}";
                                var CrazyBufferState = (CrazyBufferActive ? $"{FramebufferConverter.crazyBufferPercentageHandle}% Handled" : "OFF") + (CrazyBufferActive && FramebufferConverter.SkipCached ? " (Ignored due Pixels updates feature)" : "");
                                if (FramebufferConverter.FallbackToOldWay)
                                {
                                    CrazyBufferState = $"OFF (Safe Render)";
                                }
                                else if (!isCrazyBufferVisible)
                                {
                                    CrazyBufferState = $"OFF (XRGB8888)";
                                }

                                var aspect = $"{GamePlayerView.ASR[0]}:{GamePlayerView.ASR[1]}";

                                if (GamePlayerView.ASR[0] == 0 && GamePlayerView.ASR[1] == 0)
                                {
                                    var floatAspect = VideoService.GetAspectRatio();
                                    try
                                    {
                                        for (int n = 1; n < 20; ++n)
                                        {
                                            int m = (int)(floatAspect * n + 0.5); // Mathematical rounding
                                            if (Math.Abs(floatAspect - (double)m / n) < 0.01)
                                            {

                                                aspect = $"{m}:{n} ({floatAspect})";
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        aspect = $"{floatAspect}";
                                    }
                                }
                                LogsList.Insert(1, new CoreLogItem("Pixels", PixelsType));
                                LogsList.Insert(2, new CoreLogItem("Aspect", aspect));
                                LogsList.Insert(3, new CoreLogItem("Memory", UsingMemoryCopy));
                                LogsList.Insert(4, new CoreLogItem("Render", $"{RenderCores}"));
                                LogsList.Insert(5, new CoreLogItem("Resolution", CurrentSize));
                                LogsList.Insert(6, new CoreLogItem("CBuffer", CrazyBufferState));
                                if (VideoService != null && VideoService.TotalEffects() > 0)
                                {
                                    var EffectsApplied = $"{VideoService.TotalEffects()} Effect{(VideoService.TotalEffects() > 1 ? "s" : "")}";
                                    LogsList.Insert(7, new CoreLogItem("Effects", EffectsApplied));
                                    LogsList.Insert(8, new CoreLogItem("none", "--------------"));
                                }
                                else
                                {
                                    LogsList.Insert(7, new CoreLogItem("none", "--------------"));
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
                PlatformService.ShowErrorMessage(e);
            }
            LogInProgress = false;
        }

        public ulong[] RenderResolution()
        {
            return new ulong[] { FramebufferConverter.currentWidth, FramebufferConverter.currentHeight };
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
                    if (FPSCounterValue > 0 && !EmulationService.CorePaused && FPSCounterValue < 245)
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
                PlatformService.ShowErrorMessage(e);
            }
            FPSInProgress = false;
        }

        public string BufferCounter = "-";
        public void RaiseBufferCounterState()
        {
            try
            {
                RaisePropertyChanged(nameof(BufferCounter));
                RaisePropertyChanged(nameof(ShowBufferCounter));
            }
            catch (Exception ex)
            {

            }
        }
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
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.PlayNotificationSound("button-01");
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
                    PlatformService.PlayNotificationSound("faild");
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
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
                    PlatformService.PlayNotificationSound("button-04");
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
                    InjectInputCommand(TargetInputType, true);
                }
                InjectInputCommand(TargetInputType, false);
            }
            catch (Exception e)
            {
                InjectInputCommand(TargetInputType, false);
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
            }
        }
        public void ResetActionsSet()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                ButtonsDictionaryTemp.Remove(CurrentActionsSet);
                ActionsCustomDelay = false;
                RaisePropertyChanged(nameof(ActionsCustomDelay));
                UpdateActionsPreviewSet();
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        public async void SaveActionsSet()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                ButtonsDictionary = ButtonsDictionaryTemp.ToDictionary(entry => entry.Key, entry => entry.Value);
                UpdateActionsPreviewSet();
                HideActionsGrid.Execute(null);
                await ActionsStoreAsync(ButtonsDictionary);
                PlatformService.PlayNotificationSound("save-state");
                UpdateInfoState("Actions Saved");
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        public void CancelActionsSet()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                ButtonsDictionaryTemp = ButtonsDictionary.ToDictionary(entry => entry.Key, entry => entry.Value);
                UpdateActionsPreviewSet();
                HideActionsGrid.Execute(null);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
            }
        }

        private void ShowNotify(string InfoMessage)
        {
            if (!PlatformService.ShowNotification(InfoMessage, 3))
            {
                UpdateInfoState(InfoMessage);
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
                PlatformService.ShowErrorMessage(e);
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
                if (GameID == null)
                {
                    //GameID can be null when core started without content
                    GameID = $"contentless ({SystemName})";
                }
                var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(ActionsSaveLocation, CreationCollisionOption.OpenIfExists);

                var targetFile = await localFolder.CreateFileAsync($"{GameID}.xyz", CreationCollisionOption.ReplaceExisting);

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
                using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var outStream = stream.AsStream();
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                    try
                    {
                        if (PlatformService.PreventGCAlways)
                        {
                            stream.Dispose();
                            GC.SuppressFinalize(stream);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                try
                {
                    if (PlatformService.PreventGCAlways)
                    {
                        GC.SuppressFinalize(dictionaryListBytes);
                        dictionaryListBytes = null;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }
        public async Task ActionsRetrieveAsync(bool startup = false)
        {
            try
            {
                string GameID = EmulationService.GetGameID();
                if (GameID == null)
                {
                    //GameID can be null when core started without content
                    return;
                }
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync(ActionsSaveLocation, CreationCollisionOption.OpenIfExists);

                if (localFolder != null)
                {
                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{GameID}.xyz");
                    if (targetFileTest != null)
                    {
                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var stream = await targetFileTest.OpenAsync(FileAccessMode.Read))
                        {
                            var outStream = stream.AsStream();
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string ActionsFileContent = unicode.GetString(result);
                        try
                        {
                            if (PlatformService.PreventGCAlways)
                            {
                                GC.SuppressFinalize(result);
                                result = null;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
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
                        //if(!startup)UpdateInfoState("Actions Restored");
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
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

        public InputService InputService
        {
            get
            {
                return PlatformService.inputService;
            }
        }
        public EmulationService EmulationService
        {
            get
            {
                return PlatformService.emulationService;
            }
        }
        public VideoService VideoService
        {
            get
            {
                return PlatformService.videoService;
            }
        }
        private AudioService AudioService
        {
            get
            {
                return PlatformService.audioService;
            }
        }
        private ISettings Settings
        {
            get
            {
                return Plugin.Settings.CrossSettings.Current;
            }
        }
        public void raiseInGameOptionsActiveState(object state, EventArgs args)
        {
            InGameOptionsActive = (bool)state;
            RaisePropertyChanged(nameof(InGameOptionsActive));
        }
        public ICommand TappedCommand { get; }
        public ICommand TappedCommand2 { get; }
        public ICommand PointerMovedCommand { get; }
        public ICommand PointerTabbedCommand { get; }
        public ICommand PointerRightTabbedCommand { get; }
        public ICommand ToggleFullScreenCommand { get; }

        public ICommand TogglePauseCommand { get; }
        public ICommand ScreenScale { get; }
        public ICommand ResetCommand { get; }
        public ICommand StopCommand { get; }

        public ICommand SaveStateSlot1 { get; }
        public ICommand SaveStateSlot2 { get; }
        public ICommand SaveStateSlot3 { get; }
        public ICommand SaveStateSlot4 { get; }
        public ICommand SaveStateSlot5 { get; }
        public ICommand SaveStateSlot6 { get; }
        public ICommand SaveStateSlot7 { get; }
        public ICommand SaveStateSlot8 { get; }
        public ICommand SaveStateSlot9 { get; }
        public ICommand SaveStateSlot10 { get; }

        public ICommand LoadStateSlot1 { get; }
        public ICommand LoadStateSlot2 { get; }
        public ICommand LoadStateSlot3 { get; }
        public ICommand LoadStateSlot4 { get; }
        public ICommand LoadStateSlot5 { get; }
        public ICommand LoadStateSlot6 { get; }
        public ICommand LoadStateSlot7 { get; }
        public ICommand LoadStateSlot8 { get; }
        public ICommand LoadStateSlot9 { get; }
        public ICommand LoadStateSlot10 { get; }
        public ICommand ImportSavedSlots { get; }
        public ICommand ExportSavedSlots { get; }
        public ICommand ImportActionsSlots { get; }
        public ICommand ExportActionsSlots { get; }
        public ICommand HideLoader { get; }
        public ICommand SetScreenFit { get; }
        public ICommand SetScanlines1 { get; }
        public ICommand SetScanlines2 { get; }
        public ICommand SetScanlines3 { get; }
        public ICommand SetDoublePixel { get; }
        public ICommand SetSpeedup { get; }
        public ICommand SetUpdatesOnly { get; }
        public ICommand SetSkipCached { get; }
        public ICommand SetSkipFrames { get; }
        public ICommand SetSkipFramesRandom { get; }
        public ICommand DontWaitThreads { get; }
        public ICommand SetDelayFrames { get; }
        public ICommand SetReduceFreezes { get; }
        public ICommand SetCrazyBufferActive { get; }
        public ICommand SetAudioOnly { get; }
        public ICommand SetVideoOnly { get; }
        public ICommand SetTabSoundEffect { get; }
        public ICommand SetSensorsMovement { get; }
        public ICommand SetUseAnalogDirections { get; }
        public ICommand CallSwapControls { get; }
        public ICommand SetShowSensorsInfo { get; }
        public ICommand SetShowSpecialButtons { get; }
        public ICommand SetShowActionsButtons { get; }
        public ICommand ShowActionsGrid1 { get; }
        public ICommand ShowActionsGrid2 { get; }
        public ICommand ShowActionsGrid3 { get; }
        public ICommand HideActionsGrid { get; }
        public ICommand SetActionsDelay1 { get; }
        public ICommand SetActionsDelay2 { get; }
        public ICommand SetActionsDelay3 { get; }
        public ICommand SetActionsDelay4 { get; }
        public ICommand SetActionsDelay5 { get; }
        public ICommand SetRCore { get; }
        public ICommand SetAudioLevelMute { get; }
        public ICommand SetAudioLevelLow { get; }
        public ICommand SetAudioMediumLevel { get; }
        public ICommand SetAudioLevelNormal { get; }
        public ICommand SetAudioLevelHigh { get; }
        public ICommand ShowFPSCounterCommand { get; }
        public ICommand ShowBufferCounterCommand { get; }
        public ICommand SetNearestNeighbor { get; }
        public ICommand SetAnisotropic { get; }
        public ICommand SetCubic { get; }
        public ICommand SetLinear { get; }
        public ICommand SetHighQualityCubic { get; }
        public ICommand SetMultiSampleLinear { get; }
        public ICommand SetAliased { get; }
        public ICommand SetCoreOptionsVisible { get; }
        public ICommand SetControlsMapVisible { get; }
        public ICommand SetShowLogsList { get; }
        public ICommand SetAutoSave { get; }
        public ICommand SetAutoSave15Sec { get; }
        public ICommand SetAutoSave30Sec { get; }
        public ICommand SetAutoSave60Sec { get; }
        public ICommand SetAutoSave90Sec { get; }
        public ICommand SetRotateDegreePlus { get; }
        public ICommand SetRotateDegreeMinus { get; }
        public ICommand ToggleMuteAudio { get; }
        public ICommand ShowSavesList { get; }
        public ICommand ClearAllSaves { get; }
        public ICommand SetShowXYZ { get; }
        public ICommand SetShowL2R2Controls { get; }
        public ICommand SetAutoSaveNotify { get; }
        public ICommand SetAudioEcho { get; }
        public ICommand SetSettingsBar { get; }
        public ICommand SetAudioReverb { get; }
        public ICommand SetScaleFactorVisible { get; }
        public ICommand SetButtonsCustomization { get; }
        public ICommand SetSetCustomConsoleEditMode { get; }
        public ICommand ResetAdjustments { get; }
        public ICommand SetToggleMenuGrid { get; }


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

        public void InjectInputCommand(InjectedInputTypes d, bool forceState)
        {
            EmulationService.InjectInputPlayer1(d, forceState);
        }

        private ICommand[] AllCoreCommands { get; }

        private bool coreOperationsAllowed = false;
        public bool CoreOperationsAllowed
        {
            get => coreOperationsAllowed;
            set
            {
                coreOperationsAllowed = value;
            }
        }

        public bool FullScreenChangingPossible => PlatformService.FullScreenChangingPossible;
        public bool IsFullScreenMode => PlatformService.IsFullScreenMode;

        public bool TouchScreenAvailable => PlatformService.TouchScreenAvailable;

        public bool DisplayTouchGamepad => forceDisplayTouchGamepad || ShouldDisplayTouchGamepad || TouchPadForce;

        private bool forceDisplayTouchGamepad;
        public bool ForceDisplayTouchGamepad
        {
            get => forceDisplayTouchGamepad;
            set
            {
                forceDisplayTouchGamepad = value;
                PlatformService.PlayNotificationSound("button-01");
                RaisePropertyChanged(nameof(DisplayTouchGamepad));
                Settings.AddOrUpdateValue(ForceDisplayTouchGamepadKey, value);
                if (ResolveCanvasSizeHandler != null)
                {
                    ResolveCanvasSizeHandler.Invoke(null, null);
                }
                if (PlatformService.UpdateButtonsMap != null)
                {
                    PlatformService.UpdateButtonsMap.Invoke(null, null);
                }
            }
        }
        private bool touchPadForce;
        public bool TouchPadForce
        {
            get => touchPadForce;
            set
            {
                touchPadForce = value;
                PlatformService.PlayNotificationSound("button-01");
                RaisePropertyChanged(nameof(DisplayTouchGamepad));
                Settings.AddOrUpdateValue("TouchPadForce", TouchPadForce);
                if (ResolveCanvasSizeHandler != null)
                {
                    ResolveCanvasSizeHandler.Invoke(null, null);
                }
            }
        }

        private bool shouldDisplayTouchGamepad;
        public bool ShouldDisplayTouchGamepad
        {
            get => shouldDisplayTouchGamepad;
            set
            {
                shouldDisplayTouchGamepad = value;
                RaisePropertyChanged(nameof(DisplayTouchGamepad));
                if (ResolveCanvasSizeHandler != null)
                {
                    ResolveCanvasSizeHandler.Invoke(null, null);
                }
            }
        }

        private bool aVDebug;
        public bool AVDebug
        {
            get
            {
                return aVDebug;
            }
            set
            {
                aVDebug = value;
                RaisePropertyChanged(nameof(AVDebug));
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(nameof(AVDebug), value);
            }
        }
        private bool gameIsPaused;
        public bool GameIsPaused
        {
            get => gameIsPaused;
            set
            {
                gameIsPaused = value;
            }
        }

        public Thickness fpsMargin = new Thickness(7, -33, 0, 0);
        public Thickness bufferMargin = new Thickness(5, -33, 0, 0);
        public Thickness vfsMargin = new Thickness(7, 7, 0, 0);
        public Thickness logMargin = new Thickness(47, 7, 0, 0);
        public Thickness infoMargin = new Thickness(7, 40, 0, 0);

        private bool displayPlayerUI;
        public bool DisplayPlayerUI
        {
            get => displayPlayerUI;
            set
            {
                displayPlayerUI = value;
                RaisePropertyChanged(nameof(SettingsBarVisibility));
                if (ResolveCanvasSizeHandler != null)
                {
                    ResolveCanvasSizeHandler.Invoke(null, null);
                }
                updateMargins();
            }
        }
        private void updateMargins()
        {
            try
            {
                if (displayPlayerUI)
                {
                    fpsMargin = new Thickness(7, -33, 0, 0);
                    bufferMargin = new Thickness(5, -33, 0, 0);
                    vfsMargin = new Thickness(7, 7, 0, 0);
                    logMargin = new Thickness(47, 7, 0, 0);
                    infoMargin = new Thickness(7, 37, 0, 0);
                }
                else
                {
                    fpsMargin = new Thickness(7, 7, 0, 0);
                    bufferMargin = new Thickness(5, 7, 0, 0);
                    if (ShowFPSCounter || ShowBufferCounter)
                    {
                        vfsMargin = new Thickness(7, 43, 0, 0);
                        logMargin = new Thickness(47, 43, 0, 0);
                        infoMargin = new Thickness(7, 73, 0, 0);
                    }
                    else
                    {
                        vfsMargin = new Thickness(7, 7, 0, 0);
                        logMargin = new Thickness(47, 7, 0, 0);
                        infoMargin = new Thickness(7, 37, 0, 0);
                    }
                }
                RaisePropertyChanged(nameof(fpsMargin));
                RaisePropertyChanged(nameof(bufferMargin));
                RaisePropertyChanged(nameof(vfsMargin));
                RaisePropertyChanged(nameof(logMargin));
                RaisePropertyChanged(nameof(infoMargin));
            }
            catch (Exception ex)
            {

            }
        }
        public bool SettingsBarVisibility
        {
            get
            {
                return !XBoxMode || TouchPadForce || DisplayTouchGamepad;
            }
        }

        public bool VMREADY = false;
        public void GetDefaultScale(ref float defaultScale, ref float defaultRScale)
        {
            if (InputService.isTouchAvailable())
            {
                switch (PlatformService.ScreenScale)
                {
                    case 100:
                    case 125:
                        defaultScale = 2.35f;
                        defaultRScale = 2.3f;
                        break;

                    case 150:
                        defaultScale = 2.25f;
                        defaultRScale = 2.2f;
                        break;
                    case 175:
                        defaultScale = 2.1f;
                        defaultRScale = 2.05f;
                        break;

                    case 200:
                        defaultScale = 1.85f;
                        defaultRScale = 1.8f;
                        break;
                    case 225:
                        defaultScale = 1.65f;
                        defaultRScale = 1.6f;
                        break;

                    case 250:
                        defaultScale = 1.45f;
                        defaultRScale = 1.4f;
                        break;

                    case 300:
                    case 325:
                        defaultScale = 1.25f;
                        defaultRScale = 1.2f;
                        break;

                    case 350:
                    case 375:
                        defaultScale = 1f;
                        defaultRScale = 1f;
                        break;

                    case 400:
                    case 425:
                        defaultScale = 0.9f;
                        defaultRScale = 0.9f;
                        break;

                    case 450:
                    case 475:
                        defaultScale = 0.8f;
                        defaultRScale = 0.8f;
                        break;

                    case 500:
                        defaultScale = 0.7f;
                        defaultRScale = 0.7f;
                        break;
                }
            }
        }
        public GamePlayerViewModel()
        {
            try
            {
                ScreenState = true;
                RaisePropertyChanged(nameof(ScreenState));

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
                PlatformService.GameStarted = true;
                PlatformService.SaveGamesListState();
                PlatformService.ReloadOverlayEffectsStaticHandler = ReloadOverlayEffect;
                EmulationService.StopGameHandler = StopHandler;
                EmulationService.GameInfoUpdater = GameInfoUpdater;

                PlatformService.A3RequestededHandler = (sender, e) =>
                {
                    ExcuteActionsAsync(3);
                };
                PlatformService.A2RequestedHandler = (sender, e) =>
                {
                    ExcuteActionsAsync(2);
                };
                PlatformService.A1RequesteddHandler = (sender, e) =>
                {
                    ExcuteActionsAsync(1);
                };
                PlatformService.StopRequestedHandler = (sender, e) =>
                {
                    Stop();
                };
                PlatformService.ResumeRequestedHandler = (sender, e) =>
                {
                    TogglePause(true);
                };
                PlatformService.PauseRequestedHandler = (sender, e) =>
                {
                    TogglePause(true);
                };
                PlatformService.QuickLoadRequestedHandler = (sender, e) =>
                {
                    QuickLoadState();
                };
                PlatformService.raiseInGameOptionsActiveState = raiseInGameOptionsActiveState;
                try
                {
                    //Core debug options will be updated before game star, but here just confirmation nothing more
                    EnabledDebugLogsListUpdate = true;
                    displayVFSDebugUpdate = true;
                    DisplayVFSDebug = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("DisplayVFSDebug", false);
                    EnabledDebugLogsList = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("EnabledDebugLogsList", false);
                    var debugFileState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("LogFileState", false);
                    EmulationService.UpdateCoreDebugFile(debugFileState);
                    var debugVFSState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("VFSFileState", false);
                    EmulationService.VFSLogFile = debugVFSState;
                }
                catch
                {

                }

                ForceDisplayTouchGamepad = Settings.GetValueOrDefault(ForceDisplayTouchGamepadKey, (!PlatformService.isXBOX && !PlatformService.isDesktop));
                TouchPadForce = Settings.GetValueOrDefault("TouchPadForce", false);
                ShouldDisplayTouchGamepad = shouldDisplayTouchGamepad;
                ActionsDelay = Settings.GetValueOrDefault(nameof(ActionsDelay), 150);
                ActionsDelay1 = Settings.GetValueOrDefault(nameof(ActionsDelay1), false);
                ActionsDelay2 = Settings.GetValueOrDefault(nameof(ActionsDelay2), true);
                ActionsDelay3 = Settings.GetValueOrDefault(nameof(ActionsDelay3), false);
                ActionsDelay4 = Settings.GetValueOrDefault(nameof(ActionsDelay4), false);
                ActionsDelay5 = Settings.GetValueOrDefault(nameof(ActionsDelay5), false);
                FitScreen = Settings.GetValueOrDefault(nameof(FitScreen), 2);
                ScreenRow = Settings.GetValueOrDefault(nameof(ScreenRow), 0);
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
                AutoSave = Settings.GetValueOrDefault(nameof(AutoSave), false);
                AutoSave15Sec = Settings.GetValueOrDefault(nameof(AutoSave15Sec), false);
                AutoSave30Sec = Settings.GetValueOrDefault(nameof(AutoSave30Sec), false);
                AutoSave60Sec = Settings.GetValueOrDefault(nameof(AutoSave60Sec), false);
                AutoSave90Sec = Settings.GetValueOrDefault(nameof(AutoSave90Sec), false);
                AutoSaveNotify = Settings.GetValueOrDefault(nameof(AutoSaveNotify), true);
                AudioEcho = Settings.GetValueOrDefault(nameof(AudioEcho), false);
                SettingsBar = Settings.GetValueOrDefault(nameof(SettingsBar), true);
                AudioReverb = Settings.GetValueOrDefault(nameof(AudioReverb), false);
                AVDebug = Settings.GetValueOrDefault(nameof(AVDebug), false);
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
                    var defaultScale = 1f;
                    var defaultRScale = 1f;
                    GetDefaultScale(ref defaultScale, ref defaultRScale);

                    leftScaleFactorValueP = (float)Settings.GetValueOrDefault(nameof(leftScaleFactorValueP), defaultScale);
                    leftScaleFactorValueW = (float)Settings.GetValueOrDefault(nameof(leftScaleFactorValueW), defaultScale);
                    rightScaleFactorValueP = (float)Settings.GetValueOrDefault(nameof(rightScaleFactorValueP), defaultRScale);
                    rightScaleFactorValueW = (float)Settings.GetValueOrDefault(nameof(rightScaleFactorValueW), defaultRScale);
                    buttonsOpacity = (float)Settings.GetValueOrDefault(nameof(ButtonsOpacity), 0.50f);
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
                ToggleSettingsBar(true);
                ToggleAudioReverb(true);
                ToggleUpdatesOnly(true);
                ToggleSkipCached(true);
                ToggleShowSpecialButtons(true);
                ToggleShowActionsButtons(true);
                ToggleUseAnalogDirections(true);
                SetRCoreCall();

                SetScreenFit = new Command(() =>
                 {
                     //when fit screen changed by user, auto fit mode will be ignored during the game
                     FitScreenChangedByUser = true;
                     ToggleFitScreen();
                     if (ResolveCanvasSizeHandler != null)
                     {
                         ResolveCanvasSizeHandler.Invoke(null, null);
                     }
                 });


                TappedCommand = new Command(() =>
                {
                    DisplayPlayerUI = !DisplayPlayerUI;
                    DisplayPlayerUITemp = DisplayPlayerUI;
                    RaisePropertyChanged(nameof(DisplayPlayerUI));
                });

                TappedCommand2 = new Command(() =>
                {
                    if (PlatformService.XBoxMode)
                    {
                        PlatformService.XBoxMode = false;
                        CheckXBoxModeNew();
                    }
                    TopBarOpacity = 1.0f;
                });

                PointerMovedCommand = new Command(() =>
                {
                    PlatformService.ChangeMousePointerVisibility(MousePointerVisibility.Visible);
                });
                PointerTabbedCommand = new Command(async () =>
                {
                    InjectInputCommand(InjectedInputTypes.DeviceIdPointerPressed, true);
                    InjectInputCommand(InjectedInputTypes.DeviceIdMouseLeft, true);
                    await Task.Delay(50);
                    InjectInputCommand(InjectedInputTypes.DeviceIdPointerPressed, false);
                    InjectInputCommand(InjectedInputTypes.DeviceIdMouseLeft, false);
                });
                PointerRightTabbedCommand = new Command(async () =>
                {
                    InjectInputCommand(InjectedInputTypes.DeviceIdMouseRight, true);
                    await Task.Delay(50);
                    InjectInputCommand(InjectedInputTypes.DeviceIdMouseRight, false);
                });
                ToggleFullScreenCommand = new Command(() => RequestFullScreenChange(FullScreenChangeType.Toggle));

                TogglePauseCommand = new Command(() => { var task = TogglePause(false); });
                ScreenScale = new Command(() => { if (SlidersDialogHandler != null) SlidersDialogHandler.Invoke(null, null); });
                ResetCommand = new Command(async () => { await Reset(); });
                StopCommand = new Command(Stop);

                SaveStateSlot1 = new Command(async () => await SaveState(1));
                SaveStateSlot2 = new Command(async () => await SaveState(2));
                SaveStateSlot3 = new Command(async () => await SaveState(3));
                SaveStateSlot4 = new Command(async () => await SaveState(4));
                SaveStateSlot5 = new Command(async () => await SaveState(5));
                SaveStateSlot6 = new Command(async () => await SaveState(6));
                SaveStateSlot7 = new Command(async () => await SaveState(7));
                SaveStateSlot8 = new Command(async () => await SaveState(8));
                SaveStateSlot9 = new Command(async () => await SaveState(9));
                SaveStateSlot10 = new Command(async () => await SaveState(10));

                LoadStateSlot1 = new Command(() => LoadState(1));
                LoadStateSlot2 = new Command(() => LoadState(2));
                LoadStateSlot3 = new Command(() => LoadState(3));
                LoadStateSlot4 = new Command(() => LoadState(4));
                LoadStateSlot5 = new Command(() => LoadState(5));
                LoadStateSlot6 = new Command(() => LoadState(6));
                LoadStateSlot7 = new Command(() => LoadState(7));
                LoadStateSlot8 = new Command(() => LoadState(8));
                LoadStateSlot9 = new Command(() => LoadState(9));
                LoadStateSlot10 = new Command(() => LoadState(10));
                ImportSavedSlots = new Command(() => ImportSavedSlotsAction());
                ExportSavedSlots = new Command(() => ExportSavedSlotsAction());
                ImportActionsSlots = new Command(() => ImportActionsSlotsAction());
                ExportActionsSlots = new Command(() => ExportActionsSlotsAction());

                SetScanlines1 = new Command(() => ToggleScanlines1());
                SetScanlines2 = new Command(() => ToggleScanlines2());
                SetScanlines3 = new Command(() => ToggleScanlines3());
                SetDoublePixel = new Command(() => ToggleDoublePixel(false));
                SetAudioOnly = new Command(() => ToggleAudioOnly(false));
                SetVideoOnly = new Command(() => ToggleVideoOnly(false));
                SetSpeedup = new Command(() => ToggleSpeedup(false));
                SetUpdatesOnly = new Command(() => ToggleUpdatesOnly(false));
                SetSkipCached = new Command(() => ToggleSkipCached(false));
                SetSkipFrames = new Command(() => ToggleSkipFrames(false));
                SetSkipFramesRandom = new Command(() => ToggleSkipFramesRandom(false));
                DontWaitThreads = new Command(() => DontWaitThreadsCall());
                SetDelayFrames = new Command(() => ToggleDelayFrames(false));
                SetReduceFreezes = new Command(() => ToggleReduceFreezes(false));
                SetCrazyBufferActive = new Command(() => ToggleCrazyBufferActive(false));
                SetTabSoundEffect = new Command(() => ToggleTabSoundEffect());
                SetSensorsMovement = new Command(() => ToggleSensorsMovement());
                SetUseAnalogDirections = new Command(() => ToggleUseAnalogDirections());
                CallSwapControls = new Command(() => SwapControls());
                SetShowSensorsInfo = new Command(() => ToggleShowSensorsInfo());
                SetShowSpecialButtons = new Command(() => ToggleShowSpecialButtons());
                SetShowActionsButtons = new Command(() => ToggleShowActionsButtons());
                ShowActionsGrid1 = new Command(() => ActionsGridVisible(true, 1));
                ShowActionsGrid2 = new Command(() => ActionsGridVisible(true, 2));
                ShowActionsGrid3 = new Command(() => ActionsGridVisible(true, 3));
                HideActionsGrid = new Command(() => ActionsGridVisible(false, 0));
                SetActionsDelay1 = new Command(() => SetActionsDelay(100));
                SetActionsDelay2 = new Command(() => SetActionsDelay(150));
                SetActionsDelay3 = new Command(() => SetActionsDelay(200));
                SetActionsDelay4 = new Command(() => SetActionsDelay(300));
                SetActionsDelay5 = new Command(() => SetActionsDelay(500));
                SetRCore = new Command(() => SetRCoreCall());
                SetAudioLevelMute = new Command(() => SetAudioLevel(0));
                SetAudioLevelLow = new Command(() => SetAudioLevel(1));
                SetAudioMediumLevel = new Command(() => SetAudioLevel(2));
                SetAudioLevelNormal = new Command(() => SetAudioLevel(3));
                SetAudioLevelHigh = new Command(() => SetAudioLevel(4));
                ShowFPSCounterCommand = new Command(() => ShowFPSCounterToggle(false));
                ShowBufferCounterCommand = new Command(() => ShowBufferCounterToggle(false));
                SetNearestNeighbor = new Command(() => SetFilters(1));
                SetAnisotropic = new Command(() => SetFilters(2));
                SetCubic = new Command(() => SetFilters(3));
                SetHighQualityCubic = new Command(() => SetFilters(4));
                SetLinear = new Command(() => SetFilters(5));
                SetMultiSampleLinear = new Command(() => SetFilters(6));
                SetAliased = new Command(() => ToggleAliased(false));
                SetCoreOptionsVisible = new Command(() => ToggleCoreOptionsVisible());
                SetControlsMapVisible = new Command(() => ToggleControlsVisible());
                SetShowLogsList = new Command(() => ToggleShowLogsList(false));
                SetAutoSave = new Command(() => ToggleAutoSave(false));
                SetAutoSave15Sec = new Command(() => ToggleAutoSaveSeconds(false, 15));
                SetAutoSave30Sec = new Command(() => ToggleAutoSaveSeconds(false, 30));
                SetAutoSave60Sec = new Command(() => ToggleAutoSaveSeconds(false, 60));
                SetAutoSave90Sec = new Command(() => ToggleAutoSaveSeconds(false, 90));
                SetRotateDegreePlus = new Command(() => ToggleRotateDegree(false, 90));
                SetRotateDegreeMinus = new Command(() => ToggleRotateDegree(false, -90));
                ToggleMuteAudio = new Command(() => ToggleMuteAudioCall());
                ShowSavesList = new Command(() => ShowAllSaves());
                ClearAllSaves = new Command(() => ClearAllSavesCall());
                SetShowXYZ = new Command(() => SetShowXYZCall());
                SetShowL2R2Controls = new Command(() => SetShowL2R2ControlsCall());
                SetAutoSaveNotify = new Command(() => ToggleAutoSaveNotify(false));
                SetAudioEcho = new Command(() => ToggleAudioEcho(false));
                SetSettingsBar = new Command(() => ToggleSettingsBar(false));
                SetAudioReverb = new Command(() => ToggleAudioReverb(false));
                SetScaleFactorVisible = new Command(() => ToggleScaleFactorVisible(false));
                SetButtonsCustomization = new Command(() => ToggleButtonsCustomization(false));
                SetSetCustomConsoleEditMode = new Command(() => ToggleSetCustomConsoleEditMode(false));
                ResetAdjustments = new Command(() => ResetAdjustmentsCall());
                SetToggleMenuGrid = new Command(() => ToggleMenuGridActive());


                AllCoreCommands = new ICommand[] { TogglePauseCommand, ResetCommand, StopCommand,
                SaveStateSlot1, SaveStateSlot2, SaveStateSlot3, SaveStateSlot4, SaveStateSlot5, SaveStateSlot6, SaveStateSlot7, SaveStateSlot8, SaveStateSlot9, SaveStateSlot10,
                LoadStateSlot1, LoadStateSlot2, LoadStateSlot3, LoadStateSlot4, LoadStateSlot5, LoadStateSlot6, LoadStateSlot7, LoadStateSlot8, LoadStateSlot9, LoadStateSlot10
                };

                PlatformService.FullScreenChangeRequested = (d, e) => RequestFullScreenChange(e.Type);
                PlatformService.PauseToggleRequested = OnPauseToggleKey;
                PlatformService.XBoxMenuRequested = OnXBoxMenuKey;
                PlatformService.FastForwardRequested = OnFastForwardKey;
                PlatformService.QuickSaveRequested = QuickSaveKey;
                PlatformService.SavesListRequested = SavesListKey;
                PlatformService.ChangeToXBoxModeRequested = ChangeToXBoxModeKey;
                PlatformService.GameStateOperationRequested = OnGameStateOperationRequested;


                ClearAllEffects = new Command(() =>
                {
                    ClearAllEffectsCall();
                });

                ShowAllEffects = new Command(() =>
                {
                    EffectsVisible = !EffectsVisible;
                    PlatformService.EffectsActive = EffectsVisible;
                    RaisePropertyChanged(nameof(EffectsVisible));
                    RaisePropertyChanged(nameof(compatibiltyTag));
                });

                ConfimeArcadeGame = new Command(() =>
                {
                    updateArcadeConfrimState();
                });

                PrepareXBoxMenu();
                callPlayTimeTimer(true);
                VMREADY = true;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }


        //autoSwitch will prevent the value from save if it was changed from startup function not by user
        public void SetRCoreCall(bool autoSwitch = false)
        {
            try
            {
                if (RCore1)
                {
                    if (!autoSwitch)
                    {
                        Settings.AddOrUpdateValue("RCoreState", nameof(RCore1));
                    }
                    FramebufferConverter.CoresCount = 1;
                }
                else if (RCore2)
                {
                    if (!autoSwitch)
                    {
                        Settings.AddOrUpdateValue("RCoreState", nameof(RCore2));
                    }
                    FramebufferConverter.CoresCount = 2;
                }
                else if (RCore4)
                {
                    if (!autoSwitch)
                    {
                        Settings.AddOrUpdateValue("RCoreState", nameof(RCore4));
                    }
                    FramebufferConverter.CoresCount = 4;
                }
                else if (RCore6)
                {
                    if (!autoSwitch)
                    {
                        Settings.AddOrUpdateValue("RCoreState", nameof(RCore6));
                    }
                    FramebufferConverter.CoresCount = 6;
                }
                else if (RCore8)
                {
                    if (!autoSwitch)
                    {
                        Settings.AddOrUpdateValue("RCoreState", nameof(RCore8));
                    }
                    FramebufferConverter.CoresCount = 8;
                }
                else if (RCore12)
                {
                    if (!autoSwitch)
                    {
                        Settings.AddOrUpdateValue("RCoreState", nameof(RCore12));
                    }
                    FramebufferConverter.CoresCount = 12;
                }
                else if (RCore20)
                {
                    if (!autoSwitch)
                    {
                        Settings.AddOrUpdateValue("RCoreState", nameof(RCore20));
                    }
                    FramebufferConverter.CoresCount = 18;
                }
                else if (RCore32)
                {
                    if (!autoSwitch)
                    {
                        Settings.AddOrUpdateValue("RCoreState", nameof(RCore32));
                    }
                    FramebufferConverter.CoresCount = 32;
                }
                else
                {
                    RCore1 = true;
                    if (!autoSwitch)
                    {
                        Settings.AddOrUpdateValue("RCoreState", nameof(RCore1));
                    }
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
                RaisePropertyChanged(nameof(WaitThreadsEnable));
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
        int FitScreenTemp = 2;
        public void HideAllUI()
        {
            PlatformService.XBoxMode = true;
            CheckXBoxModeNew();
        }
        public void CheckXBoxModeNew()
        {
            try
            {
                //Check if XBox Mode
                if (PlatformService.XBoxMode || PlatformService.isDesktop)
                {
                    forceDisplayTouchGamepad = false;
                    if (PlatformService.XBoxMode)
                    {
                        DisplayPlayerUI = false;
                    }
                    else
                    {
                        DisplayPlayerUI = DisplayPlayerUITemp;
                        forceDisplayTouchGamepad = Settings.GetValueOrDefault(ForceDisplayTouchGamepadKey, (!PlatformService.isXBOX && !PlatformService.isDesktop));
                    }
                    FitScreenTemp = FitScreen;
                    FitScreen = 4;
                }
                else
                {
                    forceDisplayTouchGamepad = true;
                    DisplayPlayerUI = DisplayPlayerUITemp;
                    FitScreen = FitScreenTemp;
                }
                RaisePropertyChanged(nameof(DisplayTouchGamepad));
                RaisePropertyChanged(nameof(DisplayPlayerUI));
                RaisePropertyChanged(nameof(SettingsBarVisibility));
                updateFitScreen();
                if (ResolveCanvasSizeHandler != null)
                {
                    ResolveCanvasSizeHandler.Invoke(null, null);
                }
            }
            catch (Exception e)
            {

            }
        }
        private void ChangeToXBoxModeKey(object sender, EventArgs args)
        {
            try
            {
                CheckXBoxModeNew();
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        public void CheckXBoxMode()
        {
            try
            {
                //Check if XBox Mode
                ForceDisplayTouchGamepadTest = ForceDisplayTouchGamepad;
                DisplayPlayerUITest = DisplayPlayerUI;
                if (PlatformService.XBoxMode || PlatformService.isDesktop)
                {
                    forceDisplayTouchGamepad = false;
                    if (PlatformService.XBoxMode)
                    {
                        DisplayPlayerUI = false;
                    }
                    else
                    {
                        DisplayPlayerUI = DisplayPlayerUITemp;
                        forceDisplayTouchGamepad = Settings.GetValueOrDefault(ForceDisplayTouchGamepadKey, (!PlatformService.isXBOX && !PlatformService.isDesktop));
                    }
                }
                else
                {
                    forceDisplayTouchGamepad = true;
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
                if (ResolveCanvasSizeHandler != null)
                {
                    ResolveCanvasSizeHandler.Invoke(null, null);
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
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
            }
        }

        private void callPlayTimeTimer(bool startState = false)
        {
            try
            {
                PlayTimeTimer?.Dispose();
                if (startState)
                {
                    PlayTimeTimer = new Timer(delegate
                    {
                        UpdatePlayTimeTemp();
                    }, null, 0, 60 * 1000);
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        string messageID = "#";
        CancellationTokenSource infoStop = new CancellationTokenSource();
        private void callInfoTimer(bool startState = false)
        {
            try
            {
                if (startState)
                {
                    infoStop = new CancellationTokenSource();
                    messageID = Path.GetRandomFileName();
                    var tempID = messageID;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(3000).AsAsyncAction().AsTask(infoStop.Token);
                            if (!infoStop.IsCancellationRequested && tempID.Equals(messageID))
                            {
                                UpdateInfo();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }).AsAsyncAction().AsTask(infoStop.Token);
                }
                else
                {
                    try
                    {
                        infoStop.Cancel();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        public void Dispose()
        {
            try
            {
                if (PlatformService.PreventGCAlways) GC.SuppressFinalize(this);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.PlayNotificationSound("button-01");
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
                PlatformService.ShowErrorMessage(e);
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
                await ImportSettingsSlotsAction("", true);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }

        private async void ExportSavedSlotsAction()
        {
            try
            {
                await ExportSettingsSlotsAction("", true);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }

        private async void ClearAllSavesCall()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                PlatformService.PlayNotificationSound("alert");
                ConfirmConfig confirmCleanSaves = new ConfirmConfig();
                confirmCleanSaves.SetTitle("Clean all saves");
                confirmCleanSaves.SetMessage("This action will remove all your saves, are you sure?");
                confirmCleanSaves.UseYesNo();
                var StartClean = await UserDialogs.Instance.ConfirmAsync(confirmCleanSaves);

                if (StartClean)
                {


                    string GameID = EmulationService.GetGameID();
                    if (GameID == null)
                    {
                        //GameID can be null when core started without content
                        GameID = $"contentless ({SystemName})";
                    }
                    string GameName = EmulationService.GetGameName();
                    var localFolder = PlatformService.saveStateService.SaveStatesFolder;
                    if (localFolder != null)
                    {
                        var gameFolderTest = (StorageFolder)await localFolder.TryGetItemAsync(GameID);
                        if (gameFolderTest != null)
                        {
                            GameIsLoadingState(true);
                            await gameFolderTest.DeleteAsync();
                            PlatformService.PlayNotificationSound("success");
                            ShowNotify("All Saves cleaned (deleted)");
                            HideSavesList();
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild");
                            ShowNotify("No saved slots found!");
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("faild");
                        ShowNotify("No saved slots found");
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }

        public async void ResetAdjustmentsCall()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                PlatformService.PlayNotificationSound("alert");
                ConfirmConfig confirmReset = new ConfirmConfig();
                confirmReset.SetTitle("Reset Customizations");
                confirmReset.SetMessage("This action will reset the (global) touch controls customizations\nAre you sure?");
                confirmReset.UseYesNo();
                var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                if (StartReset)
                {
                    var defaultScale = 1f;
                    var defaultRScale = 1f;
                    GetDefaultScale(ref defaultScale, ref defaultRScale);

                    leftScaleFactorValueP = defaultScale;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(leftScaleFactorValueP), defaultScale);
                    leftScaleFactorValueW = defaultScale;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(leftScaleFactorValueW), defaultScale);

                    rightScaleFactorValueP = defaultRScale;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(rightScaleFactorValueP), defaultRScale);
                    rightScaleFactorValueW = defaultRScale;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(rightScaleFactorValueW), defaultRScale);

                    buttonsOpacity = 0.50f;
                    if (!CustomConsoleEditMode) Settings.AddOrUpdateValue(nameof(ButtonsOpacity), 0.50f);


                    RaisePropertyChanged(nameof(LeftScaleFactorValue));
                    RaisePropertyChanged(nameof(RightScaleFactorValue));
                    RaisePropertyChanged(nameof(ButtonsOpacity));
                    RaisePropertyChanged(nameof(ButtonsSubOpacity));

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

                    PlatformService.PlayNotificationSound("success");
                    //await GeneralDialog("Touch controls reseted to default", "Reset done");
                    ShowNotify("Touch controls reseted to default");
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }
        private async Task ExportSettingsSlotsAction(string TargetLocation, bool saves = false)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                string GameID = EmulationService.GetGameID();
                if (GameID == null)
                {
                    //GameID can be null when core started without content
                    GameID = $"contentless ({SystemName})";
                }
                string GameName = EmulationService.GetGameName();
                StorageFolder localFolder = null;
                if (saves)
                {
                    localFolder = PlatformService.saveStateService.SaveStatesFolder;
                }
                else
                {
                    localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(TargetLocation);
                    if (localFolder == null)
                    {
                        localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(TargetLocation);
                    }
                }
                var targetFolder = (StorageFolder)(await localFolder.TryGetItemAsync(GameID));
                if (targetFolder != null)
                {
                    GameIsLoadingState(true);
                    StorageFolder zipsDirectory = await PickSingleFolder();
                    if (zipsDirectory != null)
                    {
                        var fileDate = DateTime.Now.ToString().Replace("/", ".").Replace("\\", "_").Replace(":", ".").Replace(" ", ".");
                        string targetFileName = $"{GameID}_{fileDate}.sip";
                        var zipFile = await zipsDirectory.CreateFileAsync(targetFileName, CreationCollisionOption.ReplaceExisting);
                        using (var stream = await zipFile.OpenStreamForWriteAsync())
                        using (var archive = ZipArchive.Create())
                        {
                            //To avoid UI block run this code into Task

                            await archive.AddAllFromDirectory(targetFolder, PlatformService.UseWindowsIndexer, null, null, null, false);
                            //AddAllFromDirectory can extended to:
                            //AddAllFromDirectory(storageFolder, string[] searchPattern, SearchOption.AllDirectories, IProgress<int> progress, bool IncludeRootFolder, CancellationTokenSource cancellationTokenSource)
                            //IProgress<int> will report how many file queued

                            archive.SaveTo(stream);
                            //SaveTo can extended to:
                            //SaveTo(Stream stream, IProgress<Dictionary<string, long>> progress, CancellationTokenSource cancellationTokenSource)
                            //IProgress<Dictionary<string, long>> will provide file name / size like below:
                            //string fileName = value.FirstOrDefault().Key;
                            //string size = value.FirstOrDefault().Value.ToFileSize();
                        }

                        PlatformService.PlayNotificationSound("success");
                        await GeneralDialog(GetLocalString("ExportSlotsMessage"), GetLocalString("ExportSlotsTitle"));
                        UpdateInfoState("Export Done");
                        GameIsLoadingState(false);
                    }
                }
                else
                {
                    PlatformService.PlayNotificationSound("faild");
                    await GeneralDialog(GetLocalString("ExportSlotsMessageError"), GetLocalString("ExportSlotsTitle"));
                    GameIsLoadingState(false);
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                GameIsLoadingState(false);
            }
            GameIsLoadingState(false);
        }


        private async Task ImportSettingsSlotsAction(string ExtractLocation, bool saves = false)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                string GameID = EmulationService.GetGameID();
                string GameName = EmulationService.GetGameName();
                if (GameID == null)
                {
                    //GameID can be null when core started without content
                    GameID = $"contentless ({SystemName})";
                    GameName = "none";
                }
                var extensions = new string[] { ".sip" };
                var file = await PickSingleFile(extensions);
                if (file != null)
                {
                    PlatformService.PlayNotificationSound("alert");
                    ConfirmConfig confirmImportSaves = new ConfirmConfig();
                    confirmImportSaves.SetTitle(GetLocalString("ImportStatesTitle"));
                    confirmImportSaves.SetMessage(GetLocalString("ImportStatesMessage"));
                    confirmImportSaves.UseYesNo();
                    var StartImport = await UserDialogs.Instance.ConfirmAsync(confirmImportSaves);

                    if (StartImport)
                    {
                        GameIsLoadingState(true);
                        StorageFolder localFolder = null;
                        if (saves)
                        {
                            localFolder = PlatformService.saveStateService.SaveStatesFolder;
                        }
                        else
                        {
                            localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(ExtractLocation);
                            if (localFolder == null)
                            {
                                localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(ExtractLocation);
                            }
                        }
                        var targetFolder = await localFolder.CreateFolderAsync(GameID, CreationCollisionOption.OpenIfExists);

                        Stream zipStream = await file.OpenStreamForReadAsync();
                        using (var zipArchive = ArchiveFactory.Open(zipStream))
                        {
                            //It should support 7z, zip, rar, gz, tar
                            var reader = zipArchive.ExtractAllEntries();

                            //Bind progress event
                            reader.EntryExtractionProgress += (sender, e) =>
                            {
                                var entryProgress = e.ReaderProgress.PercentageReadExact;
                                var sizeProgress = e.ReaderProgress.BytesTransferred.ToFileSize();
                            };

                            //Extract files
                            while (reader.MoveToNextEntry())
                            {
                                if (!reader.Entry.IsDirectory)
                                {
                                    await reader.WriteEntryToDirectory(targetFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                }
                            }
                        }

                        PlatformService.PlayNotificationSound("success");
                        await GeneralDialog(GetLocalString("ImportSlotsMessage"), GetLocalString("ImportSlotsTitle"));
                        await ActionsRetrieveAsync();
                        UpdateInfoState("Import Done");
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("faild");
                        await GeneralDialog(GetLocalString("ImportSlotsMessageCancel"), GetLocalString("ImportSlotsTitle"));
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }

        public void GameIsLoadingState(bool LoadingState)
        {
            GameIsLoading = LoadingState;
            RaisePropertyChanged(nameof(GameIsLoading));
        }

        private void updateFitScreen()
        {
            FitScreenState = FitScreen == 4;
            ScreenRow = FitScreen == 4 ? 0 : 0;
            RaisePropertyChanged(nameof(FitScreen));
            RaisePropertyChanged(nameof(ScreenRow));
            RaisePropertyChanged(nameof(FitScreenState));
        }

        public bool FitScreenChangedByUser = false;

        public void ToggleFitScreen()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                FitScreen = FitScreen == 2 ? 4 : 2;
                FitScreenState = FitScreen == 4;
                ScreenRow = FitScreen == 4 ? 0 : 0;
                RaisePropertyChanged(nameof(FitScreen));
                RaisePropertyChanged(nameof(ScreenRow));
                RaisePropertyChanged(nameof(FitScreenState));
                Settings.AddOrUpdateValue(nameof(FitScreen), FitScreen);
                Settings.AddOrUpdateValue(nameof(ScreenRow), ScreenRow);
                Settings.AddOrUpdateValue(nameof(FitScreenState), FitScreenState);
                if (FitScreen == 4)
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
                PlatformService.ShowErrorMessage(e);
            }
        }
        private void updateScanlines()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                RaisePropertyChanged(nameof(ScanLines1));
                RaisePropertyChanged(nameof(ScanLines2));
                RaisePropertyChanged(nameof(ScanLines3));
                Settings.AddOrUpdateValue(nameof(ScanLines1), ScanLines1);
                Settings.AddOrUpdateValue(nameof(ScanLines2), ScanLines2);
                Settings.AddOrUpdateValue(nameof(ScanLines3), ScanLines3);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
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
                        PlatformService.PlayNotificationSound("root-needed");
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
        private void ToggleSettingsBar(bool updateValue)
        {
            if (!updateValue) SettingsBar = !SettingsBar;
            RaisePropertyChanged(nameof(SettingsBar));
            RaisePropertyChanged(nameof(SettingsBarVisibility));
            Settings.AddOrUpdateValue(nameof(SettingsBar), SettingsBar);
            if (!updateValue)
            {
                if (SettingsBar)
                {
                    UpdateInfoState("Settings bar visible");
                }
                else
                {
                    UpdateInfoState("Settings bar hidden");
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
                //PlatformService.PlayNotificationSound("notice");
                //GeneralDialog($"This option has small effect on the performance\nyou can check {"Core Options"} for native frame skipping", "Skip Frames (Frontend)");
                if (!PlatformService.ShowNotificationDirect("This option has small effect on the performance", 3))
                {
                    PlatformService.PlayNotificationSound("notice");
                    GeneralDialog($"This option has small effect on the performance\nyou can check {"Core Options"} for native frame skipping", "Skip Frames (Frontend)");
                }
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
                PlatformService.PlayNotificationSound("notice");
                if (!PlatformService.ShowNotificationDirect("This option has small effect on the performance", 3))
                {
                    PlatformService.PlayNotificationSound("notice");
                    GeneralDialog($"This option has small effect on the performance\nyou can check {"Core Options"} for native frame skipping", "Skip Frames (Frontend)");
                }
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
                PlatformService.PlayNotificationSound("notice");
                await GeneralDialog($"This option is very important for the performance\nWe prefere to keep it on", "Reduce Freezes");
            }
            AudioService.SetGCPrevent(ReduceFreezes);
            callGCTimer(ReduceFreezes);
        }
        private void ToggleCrazyBufferActive(bool updateValue)
        {
            if (!updateValue) CrazyBufferActive = !CrazyBufferActive;
            if (!updateValue)
            {
                if (CrazyBufferActive)
                {
                    UpdateInfoState("Crazy buffer enabled");
                }
                else
                {
                    UpdateInfoState("Crazy buffer disabled");
                }
            }
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
                PlatformService.PlayNotificationSound("button-01");
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
                PlatformService.ShowErrorMessage(e);
            }
        }
        private void ToggleSensorsMovement()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
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
                PlatformService.ShowErrorMessage(e);
            }
        }
        private void ToggleUseAnalogDirections(bool UpdateState = false)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
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
                PlatformService.ShowErrorMessage(e);
            }
        }
        private void ToggleShowSensorsInfo()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
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
                PlatformService.ShowErrorMessage(e);
            }
        }
        private void ToggleShowSpecialButtons(bool updateState = false)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
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
                PlatformService.ShowErrorMessage(e);
            }
        }

        private void ToggleShowActionsButtons(bool updateState = false)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
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
                PlatformService.ShowErrorMessage(e);
            }
        }


        private void ShowFPSCounterToggle(bool UpdateState)
        {
            if (!UpdateState) ShowFPSCounter = !ShowFPSCounter;
            Settings.AddOrUpdateValue(nameof(ShowFPSCounter), ShowFPSCounter);
            RaisePropertyChanged(nameof(ShowFPSCounter));
            callFPSTimer(ShowFPSCounter);
            VideoService.SetShowFPS(ShowFPSCounter);
            updateMargins();
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
            if (CoreOptionsHandler != null && !PlatformService.CoreOptionsDropdownOpened)
            {
                CoreOptionsVisible = !CoreOptionsVisible;
                PlatformService.CoreOptionsActive = CoreOptionsVisible;
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
                PlatformService.ControlsActive = ControlsMapVisible;
                RaisePropertyChanged(nameof(ControlsMapVisible));
                if (ControlsMapVisible)
                {
                    ControlsHandler.Invoke(null, EventArgs.Empty);
                    //if (!EmulationService.CorePaused)
                    {
                        //await EmulationService.PauseGameAsync();
                    }
                }
                else
                {
                    //if (EmulationService.CorePaused)
                    {
                        //await EmulationService.ResumeGameAsync();
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
            PlatformService.LogsVisibile = ShowLogsList;
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

                    case 180:
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

        public void ActionsGridVisible(bool ActionGridState, int ActionsSetNumber)
        {
            try
            {
                ActionsGridVisiblity = ActionGridState;
                PlatformService.ActionVisibile = ActionsGridVisiblity;
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
                PlatformService.PlayNotificationSound("button-01");
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public string CoreName = "";
        public string CoreNameClean
        {
            get
            {
                return CoreName.Replace("-", "_").Replace("&", "").Replace("'", "").Replace("  ", " ").Replace(" ", "_");
            }
        }
        public string SystemName = "";
        public string SystemNamePreview = "";
        public string SystemIcon = "";
        public bool RootNeeded = false;
        public string MainFilePath = "";
        public string MainFilePathReal = "";
        public EventHandler FreeCore;

        //Arcade Data
        public string arcadeData = "";
        string md5 = "";
        string name = "";
        string type = "";
        bool updateArcadeDialogInProgress = false;
        public async void updateArcadeConfrimState()
        {
            try
            {
                if (updateArcadeDialogInProgress)
                {
                    return;
                }
                var testValue = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"k{md5}", "");
                if (testValue != null && testValue.Length > 0)
                {
                    IsArcaeConfirm = true;
                }
                else
                {
                    IsArcaeConfirm = false;
                }

                updateArcadeDialogInProgress = true;
                PlatformService.PlayNotificationSound("alert");
                ConfirmConfig confirmSaveDelete = new ConfirmConfig();
                confirmSaveDelete.SetTitle("Smart rename");
                confirmSaveDelete.SetMessage(IsArcaeConfirm ? $"Do you want to unconfirm the name for this game?" : $"Do you want to confirm the name for this game?");
                confirmSaveDelete.UseYesNo();
                confirmSaveDelete.OkText = IsArcaeConfirm ? "UnConfirm" : "Confirm";
                confirmSaveDelete.CancelText = "Cancel";
                bool ConfirmGameState = await UserDialogs.Instance.ConfirmAsync(confirmSaveDelete);
                if (ConfirmGameState)
                {
                    if (IsArcaeConfirm)
                    {
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"k{md5}", "");
                        IsArcaeConfirm = false;
                        UpdateInfoState("Game confirmation removed");

                    }
                    else
                    {
                        var aData = $"{name}|{type}";
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"k{md5}", aData);
                        IsArcaeConfirm = true;
                        UpdateInfoState("Game name confirmed");
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            RaisePropertyChanged(nameof(IsArcaeConfirm));
            updateArcadeDialogInProgress = false;
        }

        bool customStart = false;
        public async Task Prepare(GameLaunchEnvironment parameter)
        {
            try
            {
                CoreName = parameter.Core.Name;
                customStart = parameter.customStart;

                SystemName = parameter.SystemName;
                if (EmulationService != null)
                {
                    EmulationService.SystemName = SystemName;
                }

                arcadeData = parameter.ArcadeData;
                if (arcadeData != null && arcadeData.Length > 0)
                {
                    ShowArcaeConfirm = true;
                    try
                    {
                        var arcadeDataSplit = arcadeData.Split('|');
                        md5 = arcadeDataSplit[0];
                        name = arcadeDataSplit[1];
                        type = arcadeDataSplit[2];
                        var testValue = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"k{md5}", "");
                        if (testValue != null && testValue.Length > 0)
                        {
                            IsArcaeConfirm = true;
                        }
                    }
                    catch (Exception e)
                    {
                        PlatformService.ShowErrorMessage(e);
                    }
                }
                RaisePropertyChanged(nameof(ShowArcaeConfirm));
                RaisePropertyChanged(nameof(IsArcaeConfirm));
                SystemNamePreview = parameter.Core.OriginalSystemName;
                SystemIcon = GameSystemViewModel.GetSystemIconByName(SystemName, parameter.Core.Name);
                RootNeeded = parameter.RootNeeded;
                MainFilePath = parameter.MainFileStoredPath;
                InGameOptionsActive = parameter.Core.IsInGameOptionsActive;
                GameIsLoadingState(true);
                UpdateInfoState("Preparing...");
                PlatformService.SetStopHandler(StopHandler);

                try
                {
                    CustomTouchPadRetrieveAsync();
                }
                catch (Exception er)
                {

                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }

        }

        public async Task StartGame(GameLaunchEnvironment parameter)
        {
            try
            {
                InputService.useTouchPadReleaseResolver = true;
                try
                {
                    switch (CoreName)
                    {
                        case "ScummVM":
                        case "scummvm":
                        case "Mini vMac":
                            //Some cores requires more render power, I should auto switch 2 threads render for them
                            if (RCore1)
                            {
                                RCore2 = true;
                                SetRCoreCall(true);
                            }
                            break;
                        case "TGB Dual":
                            InputService.useTouchPadReleaseResolver = false;
                            break;
                    }
                }
                catch (Exception ex)
                {

                }


                var StartState = await EmulationService.StartGameAsync(parameter.Core, parameter.MainFileStoredPath, parameter.MainFilePath, parameter.EntryPoint, parameter.isSingleFile, parameter.singleFile);
                if (StartState)
                {
                    await ActionsRetrieveAsync();
                }
                else
                {
                    EmulationService_GameStarted(null, null);
                    updateLogListCaller();
                }
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
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        public void updateCoreOptions(string KeyName = "")
        {
            try
            {
                var expectedName = $"{CoreName}_{SystemName}";
                CoresOptions TargetSystem;
                if (!GameSystemSelectionViewModel.SystemsOptions.TryGetValue(expectedName, out TargetSystem))
                {
                    if (!EmulationService.IsNewCore())
                    {
                        GameSystemSelectionViewModel.SystemsOptions.TryGetValue(SystemName, out TargetSystem);
                    }
                }

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
            catch (Exception ex)
            {

                PlatformService.ShowErrorMessage(ex);
            }
        }

        public CoresOptions getSystemOptions(string core, string SystemName, bool IsNewCore)
        {
            var expectedName = $"{core}_{SystemName}";
            CoresOptions coreOption;
            try
            {
                if (!GameSystemSelectionViewModel.SystemsOptions.TryGetValue(expectedName, out coreOption))
                {
                    if (!IsNewCore)
                    {
                        GameSystemSelectionViewModel.SystemsOptions.TryGetValue(SystemName, out coreOption);
                    }
                }
            }
            catch (Exception ex)
            {


            }
            return null;
        }

        public async Task CoreOptionsStoreAsync(string core, string SystemName, bool IsNewCore, string GameName = "")
        {
            GameIsLoadingState(true);
            await GameSystemSelectionViewModel.CoreOptionsStoreAsyncDirect(CoreName, SystemName, EmulationService.IsNewCore(), GameName);
            GameIsLoadingState(false);
        }
        public async Task CoreOptionsRemoveAsync(string core, string SystemName, bool IsNewCore, string GameName = "")
        {
            GameIsLoadingState(true);
            await GameSystemSelectionViewModel.DeleteSavedOptions(CoreName, SystemName, EmulationService.IsNewCore(), GameName);
            GameIsLoadingState(false);
        }

        private async void RequestFullScreenChange(FullScreenChangeType fullScreenChangeType)
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
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
                PlatformService.ShowErrorMessage(e);
            }
        }

        public void ViewAppeared()
        {
            try
            {
                CoreOperationsAllowed = true;
                PlatformService.HandleGameplayKeyShortcuts = true;
                DisplayPlayerUI = true;

                if (EmulationService != null)
                {
                    EmulationService.GameLoaded = EmulationService_GameStarted;
                }

                PlatformService.SetHideCoreOptionsHandler(HideCoreOptions);
                PlatformService.SetHideSavesListHandler(HideSavesList);
                PlatformService.HideControlsListHandler = HideControlsList;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        bool addOpenCountInProgress = false;
        public bool isGameStarted = false;
        bool errorDialogAppeard = false;
        public bool FailedToLoadGame = false;

        private async void EmulationService_GameStarted(object sender, EventArgs e)
        {
            try
            {
                if (isGameStarted)
                {
                    return;
                }
                GameIsLoadingState(false);
                PlatformService.PlayNotificationSound("gamestarted");
                await EmulationService.ResumeGameAsync();

                isGameStarted = true;
                RaisePropertyChanged(nameof(isGameStarted));
                RaisePropertyChanged(nameof(isProgressVisible));

                /*if (!PlatformService.GameNoticeShowed)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () => ShowGameTipInfo());
                }*/
                try
                {
                    foreach (var kItem in PlatformService.shortcutEntries)
                    {
                        if (kItem.name.Equals("In-Game Menu"))
                        {
                            var menuShortcut = String.Join("+", kItem.keys);
                            var oldKeys = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("GameMenuKeys", "");
                            if (oldKeys != menuShortcut)
                            {
                                if (PlatformService.isXBOX)
                                {
                                    PlatformService.ShowNotificationDirect($"In-Game Menu: {menuShortcut}", 4);
                                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GameMenuKeys", menuShortcut);
                                }
                                else
                                {
                                    PlatformService.ShowNotificationDirect($"In-Game Menu: Keyboard 6", 4);
                                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GameMenuKeys", menuShortcut);
                                }
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {

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
                        if (customStart)
                        {
                            await PlatformService.AddGameToRecents(CoreNameClean, SystemName, null, RootNeeded, EmulationService.GetGameID(), 0, EmulationService.IsNewCore(), false);
                        }
                        else
                        {
                            await PlatformService.AddGameToRecents(CoreNameClean, SystemName, MainFilePath, RootNeeded, EmulationService.GetGameID(), 0, EmulationService.IsNewCore(), false);
                        }
                        if (!FramebufferConverter.CopyMemoryAvailable)
                        {
                            FramebufferConverter.LoadMemcpyFunction();
                        }

                        await SyncEffectsSettings();
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
                    PlatformService.PlayNotificationSound("faild");
                    await GeneralDialog("Failed to load the game, for more details check\n\u26EF -> Debug -> Log List", "Load Failed");
                }
                SetExtrasOptions();
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
            try
            {
                CheckXBoxModeNew();
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
        }
        public async void ShowGameTipInfo()
        {
            try
            {
                bool ShowNoticeState = Settings.GetValueOrDefault("NeverShowSlow2", true);
                if (ShowNoticeState)
                {
                    await Task.Delay(2200);
                    PlatformService.PlayNotificationSound("notice");
                    ConfirmConfig confirmLoadNotice = new ConfirmConfig();
                    confirmLoadNotice.SetTitle("Game Tips");
                    confirmLoadNotice.SetMessage("If the game went slow try:\n1- Pause \u25EB then Resume \u25B7.\n2- Enable \u26EF -> Performance -> Skip Frames\n\nXBOX Shortcuts:\nShow Menu-> Right Thumbstick\n\nEnjoy " + char.ConvertFromUtf32(0x1F609));
                    confirmLoadNotice.UseYesNo();
                    confirmLoadNotice.SetOkText("Never Show");
                    confirmLoadNotice.SetCancelText("Dismiss");
                    PlatformService.GameNoticeShowed = true;
                    var NeverShow = await UserDialogs.Instance.ConfirmAsync(confirmLoadNotice);
                    if (NeverShow)
                    {
                        PlatformService.PlayNotificationSound("button-01");
                        Settings.AddOrUpdateValue("NeverShowSlow2", false);
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("button-01");
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        public void ViewDisappearing()
        {
            try
            {
                if (!GameStopped)
                {
                    StopPlaying(true);
                }
                PlatformService.PlayNotificationSound("stop");

                if (PlatformService.ReloadCorePageGlobal != null)
                {
                    PlatformService.ReloadCorePageGlobal.Invoke(null, null);
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }


        private async Task TogglePause(bool dismissOverlayImmediately)
        {
            try
            {
                TopBarOpacity = 1.0f;
                if (!CoreOperationsAllowed)
                {
                    return;
                }
                PlatformService.PlayNotificationSound("button-01");
                CoreOperationsAllowed = false;

                if (GameIsPaused)
                {
                    await EmulationService.ResumeGameAsync();
                }
                else
                {
                    await EmulationService.PauseGameAsync();
                }

                GameIsPaused = !GameIsPaused;
                if (AudioService != null)
                {
                    AudioService.gameIsPaused = GameIsPaused;
                }
                RaisePropertyChanged(nameof(GameIsPaused));
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            CoreOperationsAllowed = true;
        }

        private async void OnPauseToggleKey(object sender, EventArgs args)
        {
            try
            {
                await TogglePause(true);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public async Task Reset()
        {
            try
            {
                PlatformService.PlayNotificationSound("button-01");
                PlatformService.PlayNotificationSound("alert");
                ConfirmConfig confirmConfig = new ConfirmConfig();
                confirmConfig.SetTitle(GetLocalString("GamePlayResetTitle"));
                confirmConfig.SetMessage(GetLocalString("GamePlayResetMessage"));
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
                PlatformService.ShowErrorMessage(e);
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
                //PlatformService.PlayNotificationSound("button-01");
                PlatformService.PlayNotificationSound("root-needed");
                ConfirmConfig confirmConfig = new ConfirmConfig();
                confirmConfig.SetTitle(GetLocalString("GamePlayStopTitle"));
                confirmConfig.SetMessage(GetLocalString("GamePlayStopMessage"));
                confirmConfig.UseYesNo();
                stopDialogInProgress = true;
                var result = await UserDialogs.Instance.ConfirmAsync(confirmConfig);

                if (result)
                {
                    HideMenuGrid();
                    StopPlaying();
                }

            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            stopDialogInProgress = false;
        }


        public void StopHandler(object sender, EventArgs args)
        {
            StopPlaying();
        }

        public bool displayVFSDebug = false;
        public bool displayVFSDebugUpdate = false;
        public bool DisplayVFSDebug
        {
            get
            {
                return displayVFSDebug;
            }
            set
            {
                displayVFSDebug = value;
                if (!displayVFSDebugUpdate)
                {
                    if (!displayVFSDebug)
                    {
                        UpdateInfoState("VFS Info off");
                    }
                    else
                    {
                        UpdateInfoState("VFS Info on");
                    }
                }
                displayVFSDebugUpdate = false;
                RaisePropertyChanged(nameof(DisplayVFSDebug));
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(nameof(DisplayVFSDebug), value);
            }
        }
        public void GameInfoUpdater(object sender, EventArgs args)
        {
            try
            {
                if (DisplayVFSDebug)
                {
                    var message = (string)sender;
                    UpdateInfoState(message, true);
                }
            }
            catch (Exception e)
            {

            }
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
            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
             {
                 if (GameStopStarted) return;

                 var fakeCounter = 0;

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

                     if (EmulationService.GetGameID() != null)
                     {
                         await Task.Delay(PlatformService.isMobile ? 500 : 50);
                         //Take Snapshot
                         if (EmulationService.isGameLoaded() && SnapshotHandler != null)
                         {
                             try
                             {
                                 //(core name added in 3.0 to avoid conflict between multiple cores for the same system)
                                 SnapshotHandler.Invoke(null, new GameIDArgs($"{CoreNameClean}_{EmulationService.GetGameID()}", await PlatformService.GetRecentsLocationAsync()));
                             }
                             catch (Exception es)
                             {

                             }
                         }
                         currentPorgress = rnd.Next(currentPorgress, 10);
                         fakeCounter = currentPorgress;
                         UpdateInfoState($"Stopping the game {currentPorgress}%...", true);
                         while (fakeCounter <= currentPorgress)
                         {
                             if (PlatformService.isMobile)
                             {
                                 await Task.Delay(1);
                             }
                             fakeCounter++;
                             UpdateInfoState($"Stopping the game {fakeCounter}%...", true);
                         }

                         while (SnapshotInProgress && !SnapshotFailed)
                         {
                             await Task.Delay(PlatformService.isMobile ? 500 : 50);
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
                             await Task.Delay(PlatformService.isMobile ? 500 : 50);
                         }
                     }
                     else
                     {
                         fakeCounter = currentPorgress;
                         currentPorgress = rnd.Next(currentPorgress, 10);
                         while (fakeCounter <= currentPorgress)
                         {
                             if (PlatformService.isMobile)
                             {
                                 await Task.Delay(1);
                             }
                             fakeCounter++;
                             UpdateInfoState($"Stopping the game {fakeCounter}%...", true);
                         }
                     }

                     fakeCounter = currentPorgress;
                     currentPorgress = rnd.Next(currentPorgress, 50);
                     UpdateInfoState($"Stopping the game {currentPorgress}%...", true);
                     while (fakeCounter <= currentPorgress)
                     {
                         if (PlatformService.isMobile)
                         {
                             await Task.Delay(1);
                         }
                         fakeCounter++;
                         UpdateInfoState($"Stopping the game {fakeCounter}%...", true);
                     }

                     await Task.Delay(PlatformService.isMobile ? 500 : 50);

                     CoreOperationsAllowed = false;
                     PlatformService.HandleGameplayKeyShortcuts = false;

                     if (EmulationService != null)
                     {
                         try
                         {
                             ScreenState = false;
                             RaisePropertyChanged(nameof(ScreenState));
                             await EmulationService.StopGameAsync();
                         }
                         catch (Exception est)
                         {

                         }
                         fakeCounter = currentPorgress;
                         currentPorgress = rnd.Next(currentPorgress, 70);
                         UpdateInfoState($"Stopping the game {currentPorgress}%...", true);
                         while (fakeCounter <= currentPorgress)
                         {
                             if (PlatformService.isMobile)
                             {
                                 await Task.Delay(1);
                             }
                             fakeCounter++;
                             UpdateInfoState($"Stopping the game {fakeCounter}%...", true);
                         }
                     }

                     try
                     {
                         callFPSTimer();

                         callGCTimer();

                         //callXBoxModeTimer();

                         callBufferTimer();

                         callLogTimer();

                         callAutoSaveTimer();

                         callPlayTimeTimer();
                     }
                     catch (Exception etm)
                     {

                     }
                     try
                     {
                         PlatformService.DeSetStopHandler(StopHandler);

                         PlatformService.DeSetHideCoreOptionsHandler(HideCoreOptions);
                         PlatformService.DeSetHideSavesListHandler(HideSavesList);
                         PlatformService.HideControlsListHandler = null;
                         PlatformService.AddNewActionButton = null;
                         PlatformService.UpdateButtonsMap = null;
                         PlatformService.SaveButtonHandler = null;
                         PlatformService.ResetButtonHandler = null;
                         PlatformService.CancelButtonHandler = null;
                         PlatformService.HideLogsHandler = null;
                         PlatformService.ShowKeyboardHandler = null;
                         PlatformService.HideKeyboardHandler = null;
                         PlatformService.UseL3R3InsteadOfX1X2Updater = null;
                         PlatformService.KeyboardRequestedHandler = null;
                         PlatformService.A3RequestededHandler = null;
                         PlatformService.A2RequestedHandler = null;
                         PlatformService.A1RequesteddHandler = null;
                         PlatformService.StopRequestedHandler = null;
                         PlatformService.ResumeRequestedHandler = null;
                         PlatformService.PauseRequestedHandler = null;
                         PlatformService.SnapshotRequestedHandler = null;
                         PlatformService.QuickLoadRequestedHandler = null;
                         PlatformService.LogIndicatorHandler = null;
                         PlatformService.VFSIndicatorHandler = null;
                         PlatformService.LEDIndicatorHandler = null;
                         PlatformService.UpdateMouseButtonsState = null;
                         PlatformService.RightClick = null;
                         PlatformService.LeftClick = null;
                         PlatformService.PrepareLeftControls = null;
                         PlatformService.CoreReadingRetroKeyboard = false;
                         InputService.useTouchPadReleaseResolver = true;
                         InputService.vKeyboardKeys.Clear();
                         InputService.RequestedKeys.Clear();
                         InputService.RightControls.Clear();
                         InputService.LeftControls.Clear();
                     }
                     catch (Exception ede)
                     {

                     }

                     try
                     {
                         InputService.RightControls.Clear();
                         InputService.LeftControls.Clear();
                     }
                     catch (Exception ex)
                     {

                     }

                     try
                     {
                         saveStateService.SetSaveStatesFolder(null);
                     }
                     catch (Exception ex)
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
                         SnapshotHandler = null;
                     }
                     catch (Exception esde)
                     {

                     }
                     await Task.Delay(PlatformService.isMobile ? 500 : 50);
                     try
                     {
                         PlatformService.ChangeMousePointerVisibility(MousePointerVisibility.Visible);
                         await PlatformService.ChangeFullScreenStateAsync(FullScreenChangeType.Exit);
                         PlatformService.KeyboardEvent = null;
                         PlatformService.useNativeKeyboard = false;
                     }
                     catch (Exception ecm)
                     {

                     }
                     try
                     {
                         PlatformService.PauseToggleRequested = null;
                         PlatformService.XBoxMenuRequested = null;
                         PlatformService.FastForwardRequested = null;
                         PlatformService.QuickSaveRequested = null;
                         PlatformService.SavesListRequested = null;
                         PlatformService.ChangeToXBoxModeRequested = null;
                         PlatformService.GameStateOperationRequested = null;
                         PlatformService.ReloadOverlayEffectsStaticHandler = null;
                         EmulationService.StopGameHandler = StopHandler;
                         PlatformService.raiseInGameOptionsActiveState = null;
                         PlatformService.XBoxMenuActive = false;
                         PlatformService.ScaleMenuActive = false;
                         PlatformService.SavesListActive = false;
                         PlatformService.CoreOptionsActive = false;
                         PlatformService.ControlsActive = false;
                         PlatformService.MouseSate = false;
                         PlatformService.GamePlayPageUpdateBindings = null;
                         PlatformService.GameOverlaysUpdateBindings = null;
                         PlatformService.KeyboardVisibleState = false;
                         InputService.CoreRequestingMousePosition = false;
                         PlatformService.HideEffects = null;
                         PlatformService.ScreenshotHandler = null;
                         PlatformService.ResolveCanvasSizeHandler = null;
                         PlatformService.SlidersDialogHandler = null;
                         GamePlayerView.upscaleHandler = null;
                     }
                     catch (Exception eh)
                     {

                     }

                     try
                     {
                         try
                         {
                             foreach (var dItem in InputService.Descriptors)
                             {
                                 dItem.Clear();
                             }
                         }
                         catch (Exception ex)
                         {

                         }
                         InputService.InjectedInputFramePermamence = 4;
                         GamePlayerView.tempKeys.Clear();
                     }
                     catch (Exception ex)
                     {

                     }
                     FramebufferConverter.ClearBuffer();
                     FramebufferConverter.ResetPixelCache();
                     FramebufferConverter.RaiseSkippedCachedHandler = null;
                     FramebufferConverter.requestToStopSkipCached = false;
                     FramebufferConverter.UpdateProgressState = null;
                     FramebufferConverter.currentFileProgress = 0;
                     FramebufferConverter.currentFileEntry = "";
                     fakeCounter = currentPorgress;
                     currentPorgress = rnd.Next(currentPorgress, 99);
                     UpdateInfoState($"Stopping the game {currentPorgress}%...", true);
                     while (fakeCounter <= currentPorgress)
                     {
                         if (PlatformService.isMobile)
                         {
                             await Task.Delay(1);
                         }
                         fakeCounter++;
                         UpdateInfoState($"Stopping the game {fakeCounter}%...", true);
                     }
                     UpdateInfoState($"Stopping the game {currentPorgress}%...", true);
                 }
                 catch (Exception e)
                 {
                     //PlatformService.ShowErrorMessage(e);
                 }
                 GameStopped = true;

                 await Task.Delay(PlatformService.isMobile ? 500 : 50);

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
                 await Task.Delay(PlatformService.isMobile ? 500 : 50);
                 PlatformService.SetGameStopInProgress(false);
                 PlatformService.GameStarted = false;
                 try
                 {
                     Dispose();
                 }
                 catch (Exception edis)
                 {

                 }
                 try
                 {
                     if (GameSystemSelectionView.isUpscaleSupported && GamePlayerView.upscaleActive)
                     {
                         await GamePlayerView.upscaleDisposed.Task;
                     }
                     GamePlayerView.upscaleActive = false;
                 }
                 catch (Exception ex)
                 {
                 }
                 try
                 {
                     if (PlatformService.isMobile)
                     {
                         await Task.Delay(500);
                     }
                     if (App.rootFrame.CanGoBack)
                     {
                         App.rootFrame.GoBack();
                     }
                 }
                 catch (Exception e)
                 {
                     //PlatformService.ShowErrorMessage(e);
                     GameStopInProgress = false;
                 }

             });
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
                    if (customStart)
                    {
                        await PlatformService.AddGameToRecents(CoreNameClean, SystemName, null, RootNeeded, EmulationService.GetGameID(), PlayedTime, EmulationService.IsNewCore(), false);
                    }
                    else
                    {
                        await PlatformService.AddGameToRecents(CoreNameClean, SystemName, MainFilePath, RootNeeded, EmulationService.GetGameID(), PlayedTime, EmulationService.IsNewCore(), false);
                    }

                    //This step made to avoid losing statistics if the app crash
                    //values will be updated next startup
                    //when play time saved I should reset it.
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GamePlayTime", "");
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        private void UpdatePlayTimeTemp()
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
                    //This step made to avoid losing statistics if the app crash
                    //values will be updated next startup
                    if (customStart)
                    {
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GamePlayTime", $"{CoreNameClean}|{SystemName}||{RootNeeded}|{EmulationService.GetGameID()}|{PlayedTime}|{EmulationService.IsNewCore()}|false");
                    }
                    else
                    {
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GamePlayTime", $"{CoreNameClean}|{SystemName}|{MainFilePath}|{RootNeeded}|{EmulationService.GetGameID()}|{PlayedTime}|{EmulationService.IsNewCore()}|false");
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task SaveState(uint slotID, bool showMessage = true)
        {
            try
            {
                GameIsLoadingState(true);
                if (showMessage)
                {
                    PlatformService.PlayNotificationSound("button-01");
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
                    StorageFolder SnapshotLocation = await PlatformService.saveStateService.SaveStatesFolder.CreateFolderAsync($@"{EmulationService.GetGameID().ToLower()}", CreationCollisionOption.OpenIfExists);
                    string SnapshotName = $"{EmulationService.GetGameID().ToLower()}_S{slotID}";
                    SnapshotHandler.Invoke(null, new GameIDArgs(SnapshotName, SnapshotLocation));
                }
                else
                {
                    PlatformService.PlayNotificationSound("faild");
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
                PlatformService.ShowErrorMessage(e);
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

        int currentSlotsType = 1;
        private async Task GetSaveSlotsList(int SlotsType, bool calledbyshortcut = false)
        {
            try
            {
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            try
            {
                currentSlotsType = SlotsType;
                if (!calledbyshortcut)
                {
                    SavesListActive = true;
                    RaisePropertyChanged(nameof(SavesListActive));
                    LoadSaveListInProgress = true;
                    RaisePropertyChanged(nameof(LoadSaveListInProgress));
                    NoSavesActive = false;
                    RaisePropertyChanged(nameof(NoSavesActive));
                }
                string GameID = EmulationService.GetGameID()?.ToLower();
                if (GameID == null)
                {
                    //GameID can be null when core started without content
                    GameID = $"contentless ({SystemName})";
                }
                PlatformService.SetSavesListActive(true);
                StorageFolder SavesLocation = await PlatformService.saveStateService.SaveStatesFolder.CreateFolderAsync($@"{GameID}", CreationCollisionOption.OpenIfExists);
                GameSavesList.Clear();
                if (SavesLocation != null)
                {
                    QueryOptions queryOptions = new QueryOptions();
                    queryOptions.FolderDepth = FolderDepth.Deep;
                    if (PlatformService.UseWindowsIndexer)
                    {
                        queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                    }
                    var sortEntry = new SortEntry();
                    sortEntry.PropertyName = "System.DateModified";
                    sortEntry.AscendingOrder = false;
                    queryOptions.SortOrder.Add(sortEntry);
                    var files = SavesLocation.CreateFileQueryWithOptions(queryOptions);
                    var FilesList = await files.GetFilesAsync();
                    if (FilesList == null || FilesList.Count == 0)
                    {
                        try
                        {
                            queryOptions.IndexerOption = IndexerOption.DoNotUseIndexer;
                            files = SavesLocation.CreateFileQueryWithOptions(queryOptions);
                            FilesList = await files.GetFilesAsync();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    List<StorageFile> sortedFiles = null;

                    if (calledbyshortcut && FilesList != null && FilesList.Count > 0)
                    {
                        try
                        {
                            bool slotFound = false;
                            try
                            {
                                sortedFiles = FilesList.OrderByDescending(a => (a.GetBasicPropertiesAsync().AsTask().Result).DateModified).ToList();
                            }
                            catch (Exception ee)
                            {

                            }
                            foreach (var FileItem in (sortedFiles != null ? sortedFiles : FilesList))
                            {
                                for (int i = 11; i <= 20; i++)
                                {
                                    var testName = $@"_S{i}.";
                                    if (FileItem.Name.Contains(testName))
                                    {
                                        LoadState((uint)i);
                                        slotFound = true;
                                        break;
                                    }
                                }
                                if (slotFound)
                                {
                                    SavesListActive = false;
                                    RaisePropertyChanged(nameof(SavesListActive));
                                    LoadSaveListInProgress = false;
                                    RaisePropertyChanged(nameof(LoadSaveListInProgress));
                                    NoSavesActive = false;
                                    RaisePropertyChanged(nameof(NoSavesActive));
                                    PlatformService.SetSavesListActive(false);
                                    return;
                                }
                            }
                            if (!slotFound)
                            {
                                SavesListActive = true;
                                RaisePropertyChanged(nameof(SavesListActive));
                                LoadSaveListInProgress = true;
                                RaisePropertyChanged(nameof(LoadSaveListInProgress));
                                NoSavesActive = false;
                                RaisePropertyChanged(nameof(NoSavesActive));
                            }
                        }
                        catch (Exception ex)
                        {
                            SavesListActive = true;
                            RaisePropertyChanged(nameof(SavesListActive));
                            LoadSaveListInProgress = true;
                            RaisePropertyChanged(nameof(LoadSaveListInProgress));
                            NoSavesActive = false;
                            RaisePropertyChanged(nameof(NoSavesActive));
                        }
                    }
                    else if (calledbyshortcut)
                    {
                        SavesListActive = true;
                        RaisePropertyChanged(nameof(SavesListActive));
                        LoadSaveListInProgress = true;
                        RaisePropertyChanged(nameof(LoadSaveListInProgress));
                        NoSavesActive = false;
                        RaisePropertyChanged(nameof(NoSavesActive));
                    }

                    try
                    {
                        sortedFiles = FilesList.OrderByDescending(a => (a.GetBasicPropertiesAsync().AsTask().Result).DateModified).ToList();
                    }
                    catch (Exception ee)
                    {

                    }
                    foreach (var FileItem in (sortedFiles != null ? sortedFiles : FilesList))
                    {
                        var FileExtension = Path.GetExtension(FileItem.Name).ToLower();
                        if (FileExtension.Contains("png") || FileExtension.Contains("bmp"))
                        {
                            continue;
                        }
                        var FileDate = (await FileItem.GetBasicPropertiesAsync()).DateModified;
                        long FileDateSort = FileDate.UtcTicks;
                        switch (SlotsType)
                        {
                            case SLOTS_GEN:

                                for (int i = 1; i <= 10; i++)
                                {
                                    var testName = $@"_S{i}.";
                                    if (FileItem.Name.Contains(testName))
                                    {
                                        SaveSlotsModel saveSlotsModel = new SaveSlotsModel(GameID, i, FileItem.Name, FileDate, SavesLocation.Path, CoreNameClean, FileDateSort);
                                        GameSavesList.Add(saveSlotsModel);
                                        break;
                                    }
                                }


                                break;

                            case SLOTS_QUICK:
                                for (int i = 11; i <= 20; i++)
                                {
                                    var testName = $@"_S{i}.";
                                    if (FileItem.Name.Contains(testName))
                                    {
                                        SaveSlotsModel saveSlotsModel = new SaveSlotsModel(GameID, i, FileItem.Name, FileDate, SavesLocation.Path, CoreNameClean, FileDateSort);
                                        GameSavesList.Add(saveSlotsModel);
                                        break;
                                    }
                                }
                                break;

                            case SLOTS_AUTO:
                                for (int i = 21; i <= 35; i++)
                                {
                                    var testName = $@"_S{i}.";
                                    if (FileItem.Name.Contains(testName))
                                    {
                                        SaveSlotsModel saveSlotsModel = new SaveSlotsModel(GameID, i, FileItem.Name, FileDate, SavesLocation.Path, CoreNameClean, FileDateSort);
                                        GameSavesList.Add(saveSlotsModel);
                                        break;
                                    }
                                }
                                break;

                            case SLOTS_ALL:
                                for (int i = 1; i <= 35; i++)
                                {
                                    var testName = $@"_S{i}.";
                                    if (FileItem.Name.Contains(testName))
                                    {
                                        SaveSlotsModel saveSlotsModel = new SaveSlotsModel(GameID, i, FileItem.Name, FileDate, SavesLocation.Path, CoreNameClean, FileDateSort);
                                        GameSavesList.Add(saveSlotsModel);
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    try
                    {
                        if (PlatformService.PreventGCAlways)
                        {
                            GC.SuppressFinalize(FilesList);
                            FilesList = null;
                            GC.SuppressFinalize(sortedFiles);
                            sortedFiles = null;
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
                if (GameSavesList.Count > 0)
                {
                    NoSavesActive = false;
                    RaisePropertyChanged(nameof(NoSavesActive));
                }
                else
                {
                    NoSavesActive = true;
                    RaisePropertyChanged(nameof(NoSavesActive));
                    PlatformService.PlayNotificationSound("notice");
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            LoadSaveListInProgress = false;
            RaisePropertyChanged(nameof(LoadSaveListInProgress));

            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }
            
        }


        public void SaveSelectHandler(SaveSlotsModel saveSlotsModel)
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
                    PlatformService.SavesListActive = false;
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public bool DeleteSaveInProgress = false;
        public async void SaveHoldHandler(SaveSlotsModel saveSlotsModel)
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
                if (GameID == null)
                {
                    //GameID can be null when core started without content
                    GameID = $"contentless ({SystemName})";
                }
                int SlotID = saveSlotsModel.SlotID;
                StorageFolder SavesLocation = (StorageFolder)await PlatformService.saveStateService.SaveStatesFolder.TryGetItemAsync($@"{GameID}");
                if (!DeleteSaveInProgress)
                {
                    DeleteSaveInProgress = true;
                    PlatformService.PlayNotificationSound("alert");
                    ConfirmConfig confirmSaveDelete = new ConfirmConfig();
                    confirmSaveDelete.SetTitle("Save Action");
                    confirmSaveDelete.SetMessage($"Do you want to delete the select slot?");
                    confirmSaveDelete.UseYesNo();
                    confirmSaveDelete.OkText = "Delete";
                    confirmSaveDelete.CancelText = "Cancel";
                    bool SaveDeleteState = await UserDialogs.Instance.ConfirmAsync(confirmSaveDelete);
                    if (SaveDeleteState)
                    {
                        var testSaveFile = (StorageFile)await SavesLocation.TryGetItemAsync(SlotFileName);
                        var testSnapFile = (StorageFile)await SavesLocation.TryGetItemAsync(SnapshotFileName);
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
                PlatformService.ShowErrorMessage(e);
                DeleteSaveInProgress = false;
            }
        }
        public void HideSavesList(object sender = null, EventArgs e = null)
        {
            SavesListActive = false;
            RaisePropertyChanged(nameof(SavesListActive));
            PlatformService.SetSavesListActive(false);
        }

        public void HideControlsList(object sender = null, EventArgs e = null)
        {
            if (ControlsMapVisible)
            {
                ToggleControlsVisible();
            }
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
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            try
            {
                GameIsLoadingState(true);
                PlatformService.PlayNotificationSound("button-01");
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
                    PlatformService.PlayNotificationSound("faild");
                }


                if (GameIsPaused)
                {
                    //await TogglePause(true);
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            CoreOperationsAllowed = true;
            GameIsLoadingState(false);
            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }
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
                string GameID = EmulationService.GetGameID()?.ToLower();
                if (GameID == null)
                {
                    //GameID can be null when core started without content
                    GameID = $"contentless ({SystemName})";
                }
                StorageFolder SavesLocation = (StorageFolder)await PlatformService.saveStateService.SaveStatesFolder.TryGetItemAsync($@"{GameID}");
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
                        var testFile = (StorageFile)await SavesLocation.TryGetItemAsync(testFileName);
                        if (testFile == null)
                        {
                            testFileName = $"{GameID}_S{i}.bmp";
                            testFile = (StorageFile)await SavesLocation.TryGetItemAsync(testFileName);
                            if (testFile == null)
                            {
                                await SaveState((uint)i);
                                foundEmptySlot = true;
                                break;
                            }
                        }
                    }
                }

                if (!foundEmptySlot)
                {
                    var FilesList = await SavesLocation.GetFilesAsync();
                    Dictionary<StorageFile, long> GameSavesListTemp = new Dictionary<StorageFile, long>();
                    foreach (var FileItem in FilesList)
                    {
                        var FileExtension = Path.GetExtension(FileItem.Name).ToLower();
                        if (FileExtension.Contains("png") || FileExtension.Contains("bmp"))
                        {
                            continue;
                        }
                        var FileDate = (await FileItem.GetBasicPropertiesAsync()).DateModified;
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
                    var SnapshotFile = (StorageFile)await SavesLocation.TryGetItemAsync(SnapshotFileName);
                    if (SnapshotFile != null)
                    {
                        await SnapshotFile.DeleteAsync();
                    }
                    SnapshotFileName = sortedFilesList.Key.Name.Replace(Path.GetExtension(sortedFilesList.Key.Name), ".bmp");
                    SnapshotFile = (StorageFile)await SavesLocation.TryGetItemAsync(SnapshotFileName);
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
                PlatformService.ShowErrorMessage(e);
            }
            QuickSaveInProgress = false;
            RaisePropertyChanged(nameof(QuickSaveInProgress));
        }

        public void QuickLoadState()
        {
            GetSaveSlotsList(SLOTS_QUICK, true);
        }

        bool AutoSaveInProgress = false;
        public async Task AutoSaveState(bool showMessage = true)
        {
            try
            {
                string GameID = EmulationService.GetGameID()?.ToLower();
                if (GameID == null)
                {
                    //GameID can be null when core started without content
                    GameID = $"contentless ({SystemName})";
                }
                if (AutoSaveInProgress)
                {
                    return;
                }
                bool foundEmptySlot = false;
                AutoSaveInProgress = true;
                StorageFolder SavesLocation = (StorageFolder)await PlatformService.saveStateService.SaveStatesFolder.TryGetItemAsync($@"{GameID}");
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
                        var testFile = (StorageFile)await SavesLocation.TryGetItemAsync(testFileName);
                        if (testFile == null)
                        {
                            testFileName = $"{GameID}_S{i}.bmp";
                            testFile = (StorageFile)await SavesLocation.TryGetItemAsync(testFileName);
                            if (testFile == null)
                            {
                                await SaveState((uint)i, showMessage);
                                foundEmptySlot = true;
                                break;
                            }
                        }
                    }
                }
                if (!foundEmptySlot)
                {
                    var FilesList = await SavesLocation.GetFilesAsync();
                    Dictionary<StorageFile, long> GameSavesListTemp = new Dictionary<StorageFile, long>();
                    foreach (var FileItem in FilesList)
                    {
                        var FileExtension = Path.GetExtension(FileItem.Name).ToLower();
                        if (FileExtension.Contains("png") || FileExtension.Contains("bmp"))
                        {
                            continue;
                        }
                        var FileDate = (await FileItem.GetBasicPropertiesAsync()).DateModified;
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
                    var SnapshotFile = (StorageFile)await SavesLocation.TryGetItemAsync(SnapshotFileName);
                    if (SnapshotFile != null)
                    {
                        await SnapshotFile.DeleteAsync();
                    }
                    SnapshotFileName = sortedFilesList.Key.Name.Replace(Path.GetExtension(sortedFilesList.Key.Name), ".bmp");
                    SnapshotFile = (StorageFile)await SavesLocation.TryGetItemAsync(SnapshotFileName);
                    if (SnapshotFile != null)
                    {
                        await SnapshotFile.DeleteAsync();
                    }
                    await AutoSaveState();
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
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
                PlatformService.ShowErrorMessage(e);
            }
        }

        private async void UpdateInfo()
        {
            try
            {
                if (SystemInfoVisiblity)
                {
                    await Task.Delay(1500);
                }
                PreviewCurrentInfoState = false;
                PreviewCurrentInfo = "";
                RaisePropertyChanged(nameof(PreviewCurrentInfoState));
                RaisePropertyChanged(nameof(PreviewCurrentInfo));
                callInfoTimer();
            }
            catch (Exception ex)
            {
                //PlatformService.ShowErrorMessage(ex);
            }
        }


        public async Task CustomTouchPadStoreAsync()
        {
            try
            {
                GameIsLoadingState(true);
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(TouchPadSaveLocation);
                if (localFolder == null)
                {
                    localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(TouchPadSaveLocation);
                }

                var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{SystemName}.rct");
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
                customTouchPad.buttonsOpacity = ButtonsOpacity;

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(customTouchPad));
                using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var outStream = stream.AsStream();
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }
                try
                {
                    if (PlatformService.PreventGCAlways)
                    {
                        GC.SuppressFinalize(dictionaryListBytes);
                        dictionaryListBytes = null;
                    }
                }
                catch (Exception ex)
                {

                }
                PlatformService.PlayNotificationSound("success");
                //await GeneralDialog($"Touch pad settings saved for {SystemNamePreview}");
                ShowNotify($"Touch pad settings saved for {SystemNamePreview}");
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            GameIsLoadingState(false);
        }
        public async void CustomTouchPadRetrieveAsync()
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(TouchPadSaveLocation);
                if (localFolder == null)
                {
                    return;
                }

                var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{SystemName}.rct");
                if (targetFileTest != null)
                {
                    Encoding unicode = Encoding.Unicode;
                    byte[] result;
                    using (var stream = await targetFileTest.OpenAsync(FileAccessMode.Read))
                    {
                        var outStream = stream.AsStream();
                        using (var memoryStream = new MemoryStream())
                        {
                            outStream.CopyTo(memoryStream);
                            result = memoryStream.ToArray();
                        }
                        await outStream.FlushAsync();
                    }
                    string CoreFileContent = unicode.GetString(result);
                    try
                    {
                        if (PlatformService.PreventGCAlways)
                        {
                            GC.SuppressFinalize(result);
                            result = null;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
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
                        buttonsOpacity = dictionaryList.buttonsOpacity;
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
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(TouchPadSaveLocation);
                if (localFolder == null)
                {
                    PlatformService.PlayNotificationSound("faild");
                    ShowNotify($"Customization for {SystemNamePreview} not found!");
                    return;
                }

                var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{SystemName}.rct");
                if (targetFileTest != null)
                {
                    GameIsLoadingState(true);
                    await targetFileTest.DeleteAsync();
                    PlatformService.PlayNotificationSound("success");
                    ShowNotify($"Customization for {SystemNamePreview} deleted\nGlobal customization will be used");
                    try
                    {
                        leftScaleFactorValueP = (float)Settings.GetValueOrDefault(nameof(leftScaleFactorValueP), 1f);
                        leftScaleFactorValueW = (float)Settings.GetValueOrDefault(nameof(leftScaleFactorValueW), 1f);
                        rightScaleFactorValueP = (float)Settings.GetValueOrDefault(nameof(rightScaleFactorValueP), 1f);
                        rightScaleFactorValueW = (float)Settings.GetValueOrDefault(nameof(rightScaleFactorValueW), 1f);
                        buttonsOpacity = (float)Settings.GetValueOrDefault(nameof(ButtonsOpacity), 0.50f);
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
                    PlatformService.PlayNotificationSound("faild");
                    ShowNotify($"Customization for {SystemNamePreview} not found!");
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
            RaisePropertyChanged(nameof(ButtonsOpacity));
            RaisePropertyChanged(nameof(ButtonsSubOpacity));
        }

        private async void QuickSaveKey(object sender, EventArgs args)
        {
            try
            {
                await QuickSaveState();
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        private void SavesListKey(object sender, EventArgs args)
        {
            try
            {
                ShowSavesList.Execute(null);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public bool MenuGridActive = false;
        private void OnXBoxMenuKey(object sender, EventArgs args)
        {
            try
            {
                if (WebViewGuidesVisible == Visibility.Visible)
                {
                    WebViewGuidesVisible = Visibility.Collapsed;
                }
                else
                {
                    ToggleMenuGridActive();
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }

        }
        private void OnFastForwardKey(object sender, EventArgs args)
        {
            try
            {
                FastForward = !FastForward;
                if (FastForward)
                {
                    PlatformService.ShowNotificationDirect("Fast Forward ON");
                }
                else
                {
                    PlatformService.ShowNotificationDirect("Fast Forward OFF");
                }
                RaisePropertyChanged(nameof(FastForward));
                ReloadXBOXMenu();
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }

        }
        public async void ToggleMenuGridActive()
        {
            try
            {

                PlatformService.PlayNotificationSound("select");
                MenuGridActive = !MenuGridActive;
                PlatformService.XBoxMenuActive = MenuGridActive;
                if (!MenuGridActive)
                {
                    if (UpdateXBOXListPosition != null)
                    {
                        UpdateXBOXListPosition.Invoke(null, null);
                    }
                    if (PlatformService.UpdateMouseButtonsState != null)
                    {
                        PlatformService.UpdateMouseButtonsState.Invoke(null, null);
                    }
                }
                if (MenuGridActive)
                {
                    if (MenuGridUpdate != null)
                    {
                        MenuGridUpdate.Invoke(null, null);
                    }
                    PrepareXBoxMenu();
                    //if (!EmulationService.CorePaused)
                    {
                        //await EmulationService.PauseGameAsync();
                    }
                }
                else
                {
                    //if (EmulationService.CorePaused)
                    {
                        //await EmulationService.ResumeGameAsync();
                    }
                }
                RaisePropertyChanged(nameof(MenuGridActive));

            }
            catch (Exception ex)
            {

            }
        }
        public EventHandler UpdateItemState;
        public EventHandler UpdateXBOXListPosition;
        public EventHandler RestoreXBOXListPosition;
        public EventHandler MenuGridUpdate;
        private Visibility webviewGuidesVisible = Visibility.Collapsed;
        public Visibility WebViewGuidesVisible
        {
            get
            {
                return webviewGuidesVisible;
            }
            set
            {
                webviewGuidesVisible = value;
                RaisePropertyChanged(nameof(WebViewGuidesVisible));
            }
        }
        public async void HideMenuGrid(object sender = null, EventArgs e = null)
        {
            try
            {
                if(WebViewGuidesVisible == Visibility.Visible)
                {
                    WebViewGuidesVisible = Visibility.Collapsed;
                    return;
                }
                if (UpdateXBOXListPosition != null)
                {
                    UpdateXBOXListPosition.Invoke(null, null);
                }
                MenuGridActive = false;
                PlatformService.XBoxMenuActive = MenuGridActive;
                RaisePropertyChanged(nameof(MenuGridActive));
                PlatformService.PlayNotificationSound("option-changed");
                if (EmulationService.CorePaused && !RequestKeepPaused)
                {
                    //await EmulationService.ResumeGameAsync();
                }
                if (PlatformService.UpdateMouseButtonsState != null)
                {
                    PlatformService.UpdateMouseButtonsState.Invoke(null, null);
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

        GroupMenuGrid TempMenu, ActionsMenu, QuickMenu, ControlsMenu, SavesMenu, RenderThreads, AdvancedMenu, MemoryMenu, ScreenMenu, AudioMenu, OverlaysMenu, RenderMenu, ColorModeMenu, DebugMenu;
        public List<GroupMenuGrid> MenusGrid = new List<GroupMenuGrid>();
        List<SystemMenuModel> tempItems = new List<SystemMenuModel>();
        bool isMenuReady = false;
        public async void PrepareXBoxMenu()
        {
            //This function is totally disabled with all it's components
            //Now the new menu will be generated from GamePlayerView.xaml.cs
            return;
            try
            {
                if (!isMenuReady)
                {
                    MenusGrid.Clear();
                }
                tempItems.Clear();
                //Quick
                if (isMenuReady)
                {
                    TempMenu = new GroupMenuGrid();
                }
                else
                {
                    QuickMenu = new GroupMenuGrid();
                    QuickMenu.Key = "Quick".ToUpper();
                }
            (isMenuReady ? TempMenu : QuickMenu).Add(AddNewMenu(EmulationService.CorePaused ? "Resume" : "Pause", EmulationService.CorePaused ? "play.png" : "pause.png", "pause"));
                (isMenuReady ? TempMenu : QuickMenu).Add(AddNewMenu("Stop", "stop.png", "stop", false, false, false));
                (isMenuReady ? TempMenu : QuickMenu).Add(AddNewMenu("Quick Save", "quicksave.png", "quicksave"));
                (isMenuReady ? TempMenu : QuickMenu).Add(AddNewMenu("Quick Load", "lnk.png", "quickload"));
                (isMenuReady ? TempMenu : QuickMenu).Add(AddNewMenu("Snapshot", "camera.png", "snapshot"));
                (isMenuReady ? TempMenu : QuickMenu).Add(AddNewMenu("Screen Scale", "app-record.png", "scale"));
                (isMenuReady ? TempMenu : QuickMenu).Add(AddNewMenu("Close Menu", "close.png", "close"));
                if (!isMenuReady)
                {
                    MenusGrid.Add(QuickMenu);
                }
                //Control
                if (!isMenuReady)
                {
                    ControlsMenu = new GroupMenuGrid();
                    ControlsMenu.Key = "General".ToUpper();
                }
            (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("Gamepad", "xbox.png", "controls"));
                (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("Ports Map", "xbox-map.png", "portsmap", false, false, false));
                (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("Mouse Mode", "mouse.png", "mousem", true, PlatformService.MouseSate, false));
                if (InGameOptionsActive)
                {
                    (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("Core Options", "core.png", "coreoptions"));
                }
            (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("Saves List", "saves.png", "saves"));
                if (PlatformService.KeyboardEvent != null || PlatformService.CoreReadingRetroKeyboard)
                {
                    (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("Keyboard", "keyboard.png", "kyp"));
                }
            (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("Fast Forward", "fst.png", "fst", true, FastForward, false));
                (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("FPS Counter", "fps.png", "fps", true, ShowFPSCounter, false));

                if (GameSystemSelectionView.isUpscaleSupported)
                {
                    (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("AI Upscale", "wutz.png", "upscale", true, GamePlayerView.isUpscaleActive, false));
                }

                if (SensorsMovementActive)
                {
                    (isMenuReady ? TempMenu : ControlsMenu).Add(AddNewMenu("Sensors", "sensors.png", "sensors", true, SensorsMovement, false));
                }
                if (!isMenuReady)
                {
                    MenusGrid.Add(ControlsMenu);
                }


                //Saves
                if (!isMenuReady)
                {
                    SavesMenu = new GroupMenuGrid();
                    SavesMenu.Key = "Save".ToUpper();
                }
            (isMenuReady ? TempMenu : SavesMenu).Add(AddNewMenu("Auto (30 Sec)", "timer30.png", "asave30", true, AutoSave30Sec, false, true, true));
                (isMenuReady ? TempMenu : SavesMenu).Add(AddNewMenu("Auto (1 Min)", "timer60.png", "asave1", true, AutoSave60Sec, false, true, true));
                (isMenuReady ? TempMenu : SavesMenu).Add(AddNewMenu("Auto(1.5 Min)", "timer95.png", "asave15", true, AutoSave90Sec, false, true, true));
                (isMenuReady ? TempMenu : SavesMenu).Add(AddNewMenu("SaveOn Stop", "savenstop.png", "savestop", true, AutoSave, false));
                (isMenuReady ? TempMenu : SavesMenu).Add(AddNewMenu("Auto Notify", "timernotify.png", "asaven", true, AutoSaveNotify, false));
                if (!isMenuReady)
                {
                    MenusGrid.Add(SavesMenu);
                }
                //Render
                if (!isMenuReady)
                {
                    RenderThreads = new GroupMenuGrid();
                    RenderThreads.Key = "Performance".ToUpper();
                }
                if (isCrazyBufferVisible)
                {
                    (isMenuReady ? TempMenu : RenderThreads).Add(AddNewMenu("Crazy Buffer", "cbuf.png", "cbuf", true, CrazyBufferActive));
                }
            (isMenuReady ? TempMenu : RenderThreads).Add(AddNewMenu("Skip Frames", "skipframes.png", "skipframes", true, SkipFrames, false));
                (isMenuReady ? TempMenu : RenderThreads).Add(AddNewMenu("Wait Threads", "threadswait.png", "threadswait", true, !FramebufferConverter.DontWaitThreads, false, !RCore1));
                (isMenuReady ? TempMenu : RenderThreads).Add(AddNewMenu("1 Thread", "threadsnone.png", "threadsnone", true, RCore1, false, true, true));
                (isMenuReady ? TempMenu : RenderThreads).Add(AddNewMenu("2 Threads", "threads2.png", "threads2", true, RCore2, false, true, true));
                (isMenuReady ? TempMenu : RenderThreads).Add(AddNewMenu("4 Threads", "threads4.png", "threads4", true, RCore4, false, true, true));
                (isMenuReady ? TempMenu : RenderThreads).Add(AddNewMenu("8 Threads", "threads8.png", "threads8", true, RCore8, false, true, true));
                if (!isMenuReady)
                {
                    MenusGrid.Add(RenderThreads);
                }
                //Actions
                if (!isMenuReady)
                {
                    ActionsMenu = new GroupMenuGrid();
                    ActionsMenu.Key = "Actions".ToUpper();
                }
            (isMenuReady ? TempMenu : ActionsMenu).Add(AddNewMenu("Actions 1", "actions1.png", "actions1"));
                (isMenuReady ? TempMenu : ActionsMenu).Add(AddNewMenu("Actions 2", "actions2.png", "actions2"));
                (isMenuReady ? TempMenu : ActionsMenu).Add(AddNewMenu("Actions 3", "actions2.png", "actions3"));
                (isMenuReady ? TempMenu : ActionsMenu).Add(AddNewMenu("Fast Speed", "speed1.png", "speed1", true, ActionsDelay2, false, true, true));
                (isMenuReady ? TempMenu : ActionsMenu).Add(AddNewMenu("Normal Speed", "speed2.png", "speed2", true, ActionsDelay3, false, true, true));
                (isMenuReady ? TempMenu : ActionsMenu).Add(AddNewMenu("Slow Speed", "speed3.png", "speed3", true, ActionsDelay4, false, true, true));
                (isMenuReady ? TempMenu : ActionsMenu).Add(AddNewMenu("Help", "help.png", "achelp", false, false, false));
                if (!isMenuReady)
                {
                    MenusGrid.Add(ActionsMenu);
                }

                //Effects
                if (!isMenuReady)
                {
                    ColorModeMenu = new GroupMenuGrid();
                    ColorModeMenu.Key = "Video Effects".ToUpper();
                }
            (isMenuReady ? TempMenu : ColorModeMenu).Add(AddNewMenu("None", "bw.png", "creset", true, VideoService.TotalEffects() == 0, false, true, true));
                (isMenuReady ? TempMenu : ColorModeMenu).Add(AddNewMenu("Show Effects", "sepia.png", "csepia", true, VideoService.TotalEffects() > 0 || EffectsVisible, !EffectsVisible));
                (isMenuReady ? TempMenu : ColorModeMenu).Add(AddNewMenu("Set Overlays", "none.png", "overlays", true, AddOverlays));
                (isMenuReady ? TempMenu : ColorModeMenu).Add(AddNewMenu("Set Shaders", "retro.png", "shaders", true, AddShaders));
                if (!isMenuReady)
                {
                    MenusGrid.Add(ColorModeMenu);
                }
                //Memory Helpers
                if (isMemoryHelpersVisible)
                {
                    if (!isMenuReady)
                    {
                        MemoryMenu = new GroupMenuGrid();
                        MemoryMenu.Key = "Memory Helpers".ToUpper();
                    }
                    (isMenuReady ? TempMenu : MemoryMenu).Add(AddNewMenu("Buffer.CopyMemory", "memory.png", "membcpy", true, BufferCopyMemory, false, true, true));
                    (isMenuReady ? TempMenu : MemoryMenu).Add(AddNewMenu("memcpy (msvcrt)", "memory2.png", "memcpy", true, memCPYMemory, false, true, true));
                    (isMenuReady ? TempMenu : MemoryMenu).Add(AddNewMenu("Marshal.CopyTo", "memory3.png", "memmarsh", true, MarshalMemory, false, true, true));
                    (isMenuReady ? TempMenu : MemoryMenu).Add(AddNewMenu("Span.CopyTo", "memory4.png", "memspan", true, SpanlMemory, false, true, true));
                    (isMenuReady ? TempMenu : MemoryMenu).Add(AddNewMenu("Help", "help.png", "memhelp", false, false, false));
                    if (!isMenuReady)
                    {
                        MenusGrid.Add(MemoryMenu);
                    }
                }
                //Rotate
                if (!isMenuReady)
                {
                    ScreenMenu = new GroupMenuGrid();
                    ScreenMenu.Key = "Screen".ToUpper();
                }
            (isMenuReady ? TempMenu : ScreenMenu).Add(AddNewMenu("Rotate Right", "right.png", "rright", true, RotateDegreePlusActive));
                (isMenuReady ? TempMenu : ScreenMenu).Add(AddNewMenu("Rotate Left", "left.png", "rleft", true, RotateDegreeMinusActive));
                if (!isMenuReady)
                {
                    MenusGrid.Add(ScreenMenu);
                }
                //Audio
                if (!isMenuReady)
                {
                    AudioMenu = new GroupMenuGrid();
                    AudioMenu.Key = "Audio".ToUpper();
                }
            (isMenuReady ? TempMenu : AudioMenu).Add(AddNewMenu("Volume Mute", "mute.png", "vmute", true, AudioMuteLevel, false, true, true));
                (isMenuReady ? TempMenu : AudioMenu).Add(AddNewMenu("Volume High", "high.png", "vhigh", true, AudioHighLevel, false, true, true));
                (isMenuReady ? TempMenu : AudioMenu).Add(AddNewMenu("Volume Default", "default.png", "vdefault", true, AudioNormalLevel, false, true, true));
                (isMenuReady ? TempMenu : AudioMenu).Add(AddNewMenu("Volume Medium", "default.png", "vmeduim", true, AudioMediumLevel, false, true, true));
                (isMenuReady ? TempMenu : AudioMenu).Add(AddNewMenu("Volume Low", "low.png", "vlow", true, AudioLowLevel, false, true, true));
                (isMenuReady ? TempMenu : AudioMenu).Add(AddNewMenu("Echo Effect", "echo.png", "aecho", true, AudioEcho, false, true, true));
                if (!isMenuReady)
                {
                    MenusGrid.Add(AudioMenu);
                }

                //Overlays
                if (!isMenuReady)
                {
                    OverlaysMenu = new GroupMenuGrid();
                    OverlaysMenu.Key = "Overlays".ToUpper();
                }
            (isMenuReady ? TempMenu : OverlaysMenu).Add(AddNewMenu("Overlay Lines", "lines.png", "ovlines", true, ScanLines3));
                (isMenuReady ? TempMenu : OverlaysMenu).Add(AddNewMenu("Overlay Grid", "grid.png", "ovgrid", true, ScanLines2));
                if (!isMenuReady)
                {
                    MenusGrid.Add(OverlaysMenu);
                }
                //Render
                if (!isMenuReady)
                {
                    RenderMenu = new GroupMenuGrid();
                    RenderMenu.Key = "Render".ToUpper();
                }
            (isMenuReady ? TempMenu : RenderMenu).Add(AddNewMenu("Nearest", "near.png", "rnearest", true, NearestNeighbor));
                (isMenuReady ? TempMenu : RenderMenu).Add(AddNewMenu("Linear", "line.png", "rlinear", true, Linear));
                (isMenuReady ? TempMenu : RenderMenu).Add(AddNewMenu("MultiSample", "multi.png", "rmultisample", true, MultiSampleLinear));
                if (!isMenuReady)
                {
                    MenusGrid.Add(RenderMenu);
                }
                //Debug
                if (!isMenuReady)
                {
                    DebugMenu = new GroupMenuGrid();
                    DebugMenu.Key = "Debug".ToUpper();
                }
                (isMenuReady ? TempMenu : DebugMenu).Add(AddNewMenu("Log List", "logs.png", "loglist"));
                (isMenuReady ? TempMenu : DebugMenu).Add(AddNewMenu("Audio Visualizer", "aac.png", "avdebug", true, AVDebug, false));
                (isMenuReady ? TempMenu : DebugMenu).Add(AddNewMenu("VFS Info", "notepad.png", "vfsinfo", true, DisplayVFSDebug, false));
                (isMenuReady ? TempMenu : DebugMenu).Add(AddNewMenu("Pixels Update", "dim.png", "skipcache", true, SkipCached));
                (isMenuReady ? TempMenu : DebugMenu).Add(AddNewMenu("Shortcuts", "xbox.png", "shortcuts", false, false, false));
                (isMenuReady ? TempMenu : DebugMenu).Add(AddNewMenu("Close Menu", "close.png", "close"));
                if (!isMenuReady)
                {
                    MenusGrid.Add(DebugMenu);
                }
                if (isMenuReady)
                {
                    //if ready update all items for any possible updates
                    foreach (var xItem in tempItems)
                    {
                        UpdateXBOXItemStateByCommand(xItem.MenuCommand, xItem.MenuSwitchState, xItem.isEnabled);
                    }
                }
                isMenuReady = true;

                if (RestoreXBOXListPosition != null)
                {
                    await Task.Delay(100);
                    RestoreXBOXListPosition.Invoke(null, null);
                }

            }
            catch (Exception ex)
            {

            }
        }
        public SystemMenuModel AddNewMenu(string Name, string Icon, string Command, bool SwitchState = false, bool SwitchValue = false, bool closeAfterClick = true, bool isEnabled = true, bool reloadMenu = false)
        {
            SystemMenuModel MenuCommand = new SystemMenuModel(Name, getAssetsIcon(Icon), Command, SwitchState, SwitchValue);
            MenuCommand.HideMenuAfterClick = closeAfterClick;
            MenuCommand.isEnabled = isEnabled;
            MenuCommand.reloadRequired = reloadMenu;
            tempItems.Add(MenuCommand);
            return MenuCommand;
        }
        bool RequestKeepPaused = false;
        public async void GameSystemMenuHandler(SystemMenuModel systemMenuModel)
        {
            try
            {
                if (!systemMenuModel.isEnabled)
                {
                    return;
                }
                RequestKeepPaused = false;
                PlatformService.PlayNotificationSound("button-01");
                switch (systemMenuModel.MenuCommand)
                {
                    case "controls":
                        SetControlsMapVisible.Execute(null);
                        RequestKeepPaused = true;
                        break;

                    case "portsmap":
                        GamePlayerView.PortsMap(true);
                        break;

                    case "sensors":
                        SetSensorsMovement.Execute(null);
                        systemMenuModel.MenuSwitchState = SensorsMovement;
                        break;

                    case "mousem":
                        PlatformService.MouseSate = !PlatformService.MouseSate;
                        systemMenuModel.MenuSwitchState = PlatformService.MouseSate;
                        if (PlatformService.DPadActive && PlatformService.MouseSate)
                        {
                            PlatformService.ShowNotificationDirect("A: Right click, B: Left click, Analog: Move");
                        }
                        break;

                    case "coreoptions":
                        SetCoreOptionsVisible.Execute(null);
                        break;

                    case "scale":
                        if (SlidersDialogHandler != null)
                        {
                            SlidersDialogHandler.Invoke(null, null);
                        }
                        break;

                    case "pause":
                        TogglePauseCommand.Execute(null);
                        RequestKeepPaused = true;
                        break;

                    case "stop":
                        StopCommand.Execute(null);
                        break;

                    case "quicksave":
                        await QuickSaveState();
                        break;

                    case "quickload":
                        QuickLoadState();
                        break;

                    case "snapshot":
                        SaveSnapshot();
                        break;

                    case "dimcache":
                        SetUpdatesOnly.Execute(null);
                        break;

                    case "shortcuts":
                        GameSystemSelectionView.Shortcuts(this, true);
                        break;
                    case "skipcache":
                        SetSkipCached.Execute(null);
                        systemMenuModel.MenuSwitchState = SkipCached;
                        break;

                    case "skipframes":
                        SetSkipFrames.Execute(null);
                        systemMenuModel.MenuSwitchState = SkipFrames;
                        break;

                    case "threadswait":
                        DontWaitThreads.Execute(null);
                        systemMenuModel.MenuSwitchState = !FramebufferConverter.DontWaitThreads;
                        UpdateItemState?.Invoke(systemMenuModel, null);
                        break;
                    case "threadsnone":
                        RCore1 = true;
                        SetRCore.Execute(null);
                        systemMenuModel.MenuSwitchState = RCore1;
                        UpdateXBOXItemStateByCommand("threadswait", !FramebufferConverter.DontWaitThreads, !RCore1);
                        UpdateXBOXItemStateByCommand("threadsnone", RCore1);
                        UpdateXBOXItemStateByCommand("threads2", RCore2);
                        UpdateXBOXItemStateByCommand("threads4", RCore4);
                        UpdateXBOXItemStateByCommand("threads8", RCore8);
                        break;


                    case "threads2":
                        RCore2 = true;
                        SetRCore.Execute(null);
                        systemMenuModel.MenuSwitchState = RCore2;
                        UpdateXBOXItemStateByCommand("threadswait", !FramebufferConverter.DontWaitThreads, !RCore1);
                        UpdateXBOXItemStateByCommand("threadsnone", RCore1);
                        UpdateXBOXItemStateByCommand("threads2", RCore2);
                        UpdateXBOXItemStateByCommand("threads4", RCore4);
                        UpdateXBOXItemStateByCommand("threads8", RCore8);
                        break;
                    case "threads4":
                        RCore4 = true;
                        SetRCore.Execute(null);
                        systemMenuModel.MenuSwitchState = RCore4;
                        UpdateXBOXItemStateByCommand("threadswait", !FramebufferConverter.DontWaitThreads, !RCore1);
                        UpdateXBOXItemStateByCommand("threadsnone", RCore1);
                        UpdateXBOXItemStateByCommand("threads2", RCore2);
                        UpdateXBOXItemStateByCommand("threads4", RCore4);
                        UpdateXBOXItemStateByCommand("threads8", RCore8);
                        break;
                    case "threads8":
                        RCore8 = true;
                        SetRCore.Execute(null);
                        systemMenuModel.MenuSwitchState = RCore8;
                        UpdateXBOXItemStateByCommand("threadswait", !FramebufferConverter.DontWaitThreads, !RCore1);
                        UpdateXBOXItemStateByCommand("threadsnone", RCore1);
                        UpdateXBOXItemStateByCommand("threads2", RCore2);
                        UpdateXBOXItemStateByCommand("threads4", RCore4);
                        UpdateXBOXItemStateByCommand("threads8", RCore8);
                        break;

                    case "actions1":
                        ActionsGridVisible(true, 1);
                        break;
                    case "actions2":
                        ActionsGridVisible(true, 2);
                        break;
                    case "actions3":
                        ActionsGridVisible(true, 3);
                        break;

                    case "speed1":
                        SetActionsDelay2.Execute(null);
                        systemMenuModel.MenuSwitchState = ActionsDelay2;
                        UpdateXBOXItemStateByCommand("speed1", ActionsDelay2);
                        UpdateXBOXItemStateByCommand("speed2", ActionsDelay3);
                        UpdateXBOXItemStateByCommand("speed3", ActionsDelay4);
                        break;
                    case "speed2":
                        SetActionsDelay3.Execute(null);
                        systemMenuModel.MenuSwitchState = ActionsDelay3;
                        UpdateXBOXItemStateByCommand("speed1", ActionsDelay2);
                        UpdateXBOXItemStateByCommand("speed2", ActionsDelay3);
                        UpdateXBOXItemStateByCommand("speed3", ActionsDelay4);
                        break;
                    case "speed3":
                        SetActionsDelay4.Execute(null);
                        systemMenuModel.MenuSwitchState = ActionsDelay4;
                        UpdateXBOXItemStateByCommand("speed1", ActionsDelay2);
                        UpdateXBOXItemStateByCommand("speed2", ActionsDelay3);
                        UpdateXBOXItemStateByCommand("speed3", ActionsDelay4);
                        break;

                    case "saves":
                        ShowSavesList.Execute(null);
                        break;
                    case "kyp":
                        PlatformService.ShowKeyboardHandler?.Invoke(null, null);
                        break;

                    case "asave30":
                        SetAutoSave30Sec.Execute(null);
                        systemMenuModel.MenuSwitchState = AutoSave30Sec;
                        UpdateXBOXItemStateByCommand("asave30", AutoSave30Sec);
                        UpdateXBOXItemStateByCommand("asave1", AutoSave60Sec);
                        UpdateXBOXItemStateByCommand("asave15", AutoSave90Sec);
                        UpdateXBOXItemStateByCommand("savestop", AutoSave);
                        break;
                    case "asave1":
                        SetAutoSave60Sec.Execute(null);
                        systemMenuModel.MenuSwitchState = AutoSave60Sec;
                        UpdateXBOXItemStateByCommand("asave30", AutoSave30Sec);
                        UpdateXBOXItemStateByCommand("asave1", AutoSave60Sec);
                        UpdateXBOXItemStateByCommand("asave15", AutoSave90Sec);
                        UpdateXBOXItemStateByCommand("savestop", AutoSave);
                        break;
                    case "asave15":
                        SetAutoSave90Sec.Execute(null);
                        systemMenuModel.MenuSwitchState = AutoSave90Sec;
                        UpdateXBOXItemStateByCommand("asave30", AutoSave30Sec);
                        UpdateXBOXItemStateByCommand("asave1", AutoSave60Sec);
                        UpdateXBOXItemStateByCommand("asave15", AutoSave90Sec);
                        UpdateXBOXItemStateByCommand("savestop", AutoSave);
                        break;
                    case "savestop":
                        SetAutoSave.Execute(null);
                        systemMenuModel.MenuSwitchState = AutoSave;
                        UpdateXBOXItemStateByCommand("asave30", AutoSave30Sec);
                        UpdateXBOXItemStateByCommand("asave1", AutoSave60Sec);
                        UpdateXBOXItemStateByCommand("asave15", AutoSave90Sec);
                        UpdateXBOXItemStateByCommand("savestop", AutoSave);
                        break;
                    case "asaven":
                        SetAutoSaveNotify.Execute(null);
                        systemMenuModel.MenuSwitchState = AutoSaveNotify;
                        break;

                    case "rright":
                        SetRotateDegreePlus.Execute(null);
                        systemMenuModel.MenuSwitchState = RotateDegreePlusActive;
                        break;
                    case "rleft":
                        SetRotateDegreeMinus.Execute(null);
                        systemMenuModel.MenuSwitchState = RotateDegreeMinusActive;
                        break;

                    case "fps":
                        ShowFPSCounterCommand.Execute(null);
                        systemMenuModel.MenuSwitchState = ShowFPSCounter;
                        break;

                    case "upscale":
                        GamePlayerView.isUpscaleActive = !GamePlayerView.isUpscaleActive;
                        systemMenuModel.MenuSwitchState = GamePlayerView.isUpscaleActive;
                        break;

                    case "fst":
                        FastForward = !FastForward;
                        systemMenuModel.MenuSwitchState = FastForward;
                        break;

                    case "cbuf":
                        SetCrazyBufferActive.Execute(null);
                        systemMenuModel.MenuSwitchState = CrazyBufferActive;
                        break;

                    case "vmute":
                        SetAudioLevelMute.Execute(null);
                        systemMenuModel.MenuSwitchState = AudioMuteLevel;
                        UpdateXBOXItemStateByCommand("vmute", AudioMuteLevel);
                        UpdateXBOXItemStateByCommand("vhigh", AudioHighLevel);
                        UpdateXBOXItemStateByCommand("vdefault", AudioNormalLevel);
                        UpdateXBOXItemStateByCommand("vmeduim", AudioMediumLevel);
                        UpdateXBOXItemStateByCommand("vlow", AudioLowLevel);
                        break;
                    case "vhigh":
                        SetAudioLevelHigh.Execute(null);
                        systemMenuModel.MenuSwitchState = AudioHighLevel;
                        UpdateXBOXItemStateByCommand("vmute", AudioMuteLevel);
                        UpdateXBOXItemStateByCommand("vhigh", AudioHighLevel);
                        UpdateXBOXItemStateByCommand("vdefault", AudioNormalLevel);
                        UpdateXBOXItemStateByCommand("vmeduim", AudioMediumLevel);
                        UpdateXBOXItemStateByCommand("vlow", AudioLowLevel);
                        break;
                    case "vdefault":
                        SetAudioLevelNormal.Execute(null);
                        systemMenuModel.MenuSwitchState = AudioNormalLevel;
                        UpdateXBOXItemStateByCommand("vmute", AudioMuteLevel);
                        UpdateXBOXItemStateByCommand("vhigh", AudioHighLevel);
                        UpdateXBOXItemStateByCommand("vdefault", AudioNormalLevel);
                        UpdateXBOXItemStateByCommand("vmeduim", AudioMediumLevel);
                        UpdateXBOXItemStateByCommand("vlow", AudioLowLevel);
                        break;
                    case "vmeduim":
                        SetAudioMediumLevel.Execute(null);
                        systemMenuModel.MenuSwitchState = AudioMediumLevel;
                        UpdateXBOXItemStateByCommand("vmute", AudioMuteLevel);
                        UpdateXBOXItemStateByCommand("vhigh", AudioHighLevel);
                        UpdateXBOXItemStateByCommand("vdefault", AudioNormalLevel);
                        UpdateXBOXItemStateByCommand("vmeduim", AudioMediumLevel);
                        UpdateXBOXItemStateByCommand("vlow", AudioLowLevel);
                        break;
                    case "vlow":
                        SetAudioLevelLow.Execute(null);
                        systemMenuModel.MenuSwitchState = AudioLowLevel;
                        UpdateXBOXItemStateByCommand("vmute", AudioMuteLevel);
                        UpdateXBOXItemStateByCommand("vhigh", AudioHighLevel);
                        UpdateXBOXItemStateByCommand("vdefault", AudioNormalLevel);
                        UpdateXBOXItemStateByCommand("vmeduim", AudioMediumLevel);
                        UpdateXBOXItemStateByCommand("vlow", AudioLowLevel);
                        break;

                    case "aecho":
                        SetAudioEcho.Execute(null);
                        systemMenuModel.MenuSwitchState = AudioEcho;
                        break;

                    case "ovlines":
                        SetScanlines3.Execute(null);
                        systemMenuModel.MenuSwitchState = ScanLines3;

                        break;
                    case "ovgrid":
                        SetScanlines2.Execute(null);
                        systemMenuModel.MenuSwitchState = ScanLines2;
                        break;

                    case "rnearest":
                        SetNearestNeighbor.Execute(null);
                        systemMenuModel.MenuSwitchState = NearestNeighbor;
                        break;
                    case "rlinear":
                        SetLinear.Execute(null);
                        systemMenuModel.MenuSwitchState = Linear;
                        break;
                    case "rmultisample":
                        SetMultiSampleLinear.Execute(null);
                        systemMenuModel.MenuSwitchState = MultiSampleLinear;
                        break;

                    case "creset":
                        ClearAllEffects.Execute(null);
                        UpdateXBOXItemStateByCommand("creset", VideoService.TotalEffects() == 0);
                        UpdateXBOXItemStateByCommand("csepia", VideoService.TotalEffects() > 0 || EffectsVisible);
                        UpdateXBOXItemStateByCommand("shaders", AddShaders);
                        UpdateXBOXItemStateByCommand("overlays", AddOverlays);
                        break;

                    case "csepia":
                        ShowAllEffects.Execute(null);
                        systemMenuModel.MenuSwitchState = EffectsVisible;
                        /*if (!EffectsVisible)
                        {
                            ClearAllEffectsCall();
                        }*/
                        UpdateXBOXItemStateByCommand("creset", VideoService.TotalEffects() == 0);
                        UpdateXBOXItemStateByCommand("csepia", VideoService.TotalEffects() > 0 || EffectsVisible);
                        UpdateXBOXItemStateByCommand("shaders", AddShaders);
                        UpdateXBOXItemStateByCommand("overlays", AddOverlays);
                        break;
                    case "shaders":
                        AddShaders = !AddShaders;
                        systemMenuModel.MenuSwitchState = AddShaders;
                        UpdateXBOXItemStateByCommand("creset", VideoService.TotalEffects() == 0);
                        UpdateXBOXItemStateByCommand("csepia", VideoService.TotalEffects() > 0 || EffectsVisible);
                        UpdateXBOXItemStateByCommand("shaders", AddShaders);
                        UpdateXBOXItemStateByCommand("overlays", AddOverlays);
                        break;
                    case "overlays":
                        AddOverlays = !AddOverlays;
                        systemMenuModel.MenuSwitchState = AddOverlays;
                        UpdateXBOXItemStateByCommand("creset", VideoService.TotalEffects() == 0);
                        UpdateXBOXItemStateByCommand("csepia", VideoService.TotalEffects() > 0 || EffectsVisible);
                        UpdateXBOXItemStateByCommand("shaders", AddShaders);
                        UpdateXBOXItemStateByCommand("overlays", AddOverlays);
                        break;

                    case "loglist":
                        SetShowLogsList.Execute(null);
                        break;

                    case "vfsinfo":
                        DisplayVFSDebug = !DisplayVFSDebug;
                        systemMenuModel.MenuSwitchState = DisplayVFSDebug;
                        break;

                    case "avdebug":
                        AVDebug = !AVDebug;
                        systemMenuModel.MenuSwitchState = AVDebug;
                        break;

                    case "membcpy":
                        BufferCopyMemory = true;
                        systemMenuModel.MenuSwitchState = BufferCopyMemory;
                        break;
                    case "memcpy":
                        memCPYMemory = true;
                        systemMenuModel.MenuSwitchState = memCPYMemory;
                        break;
                    case "memmarsh":
                        MarshalMemory = true;
                        systemMenuModel.MenuSwitchState = MarshalMemory;
                        break;
                    case "memspan":
                        SpanlMemory = true;
                        systemMenuModel.MenuSwitchState = SpanlMemory;
                        break;

                    case "achelp":
                        //PlatformService.PlayNotificationSound("notice");
                        GeneralDialog("Actions will help you to store multiple keys in one button, when you trigger the action all stored keys will be pressed in order, this feature helpful for fighting games.");
                        break;

                    case "memhelp":
                        //PlatformService.PlayNotificationSound("notice");
                        GeneralDialog("These are methods used to deal with the memory.\nI added multiple options so you can try whatever you want and choose what is the best for your device.\n\nNote: Span and Marshal are safe options");
                        break;

                    case "close":
                        HideMenuGrid();
                        break;
                    default:
                        break;
                }
                if (systemMenuModel.HideMenuAfterClick)
                {
                    if (!systemMenuModel.MenuSwitch)
                    {
                        HideMenuGrid();
                    }
                    else
                    {
                        if (systemMenuModel.MenuSwitchState)
                        {
                            HideMenuGrid();
                        }
                    }
                }
                else
                {
                    if (systemMenuModel.reloadRequired)
                    {
                        if (UpdateXBOXListPosition != null)
                        {
                            //UpdateXBOXListPosition.Invoke(systemMenuModel.isEnabled, null);
                        }
                        //PrepareXBoxMenu();
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        public void UpdateXBOXItemStateByCommand(string command, bool state, bool enabled = true)
        {
            try
            {
                if (UpdateItemState != null)
                {
                    SystemMenuModel systemMenuModel = new SystemMenuModel("any", "any", command, true, state);
                    systemMenuModel.isEnabled = enabled;
                    UpdateItemState.Invoke(systemMenuModel, null);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void ReloadXBOXMenu()
        {
            try
            {
                if (XBoxMenuActive)
                {
                    if (UpdateXBOXListPosition != null)
                    {
                        UpdateXBOXListPosition.Invoke(null, null);
                    }
                    PrepareXBoxMenu();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void SaveSnapshot()
        {
            try
            {
                if (PlatformService.ScreenshotHandler != null)
                {
                    ScreenshotHandler.Invoke(null, null);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    public class GameIDArgs : EventArgs
    {
        public string GameID { get; set; }
        public StorageFolder SaveLocation;
        public GameIDArgs(string gameID, StorageFolder saveLocation)
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
