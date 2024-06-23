using LibRetriX;
using LibRetriX.RetroBindings;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Newtonsoft.Json;
using RetriX.Shared.Components;
using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Controls;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Visualizer.UI.Spectrum;
using Visualizer.UI;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WinUniversalTool;
using WinUniversalTool.Models;
using static System.Net.WebRequestMethods;
using Binding = Windows.UI.Xaml.Data.Binding;
using System.Security.Cryptography;
using Visualizer.UI.DSP;
using Microsoft.Toolkit.Uwp.UI.Animations.Behaviors;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.UWP.Pages
{
    /// <summary>
    /// Parent skill class which stores instances of descriptor, skill and binding objects along with list of available execution devices.
    /// Used across child pages (SkillInfoPage and SkillAppPage) to access the same skill instance.
    /// </summary>
    /*public class SuperResolutionSkillObj
    {
        public SuperResolutionDescriptor m_skillDescriptor = null;
        public IReadOnlyList<ISkillExecutionDevice> m_availableExecutionDevices = null;
        public SuperResolutionSkill m_skill = null;
        public SuperResolutionBinding m_binding = null;
        public int m_selectedDeviceId = 0;

        public bool m_osVersionError = false;
        private readonly int m_osVersionMinimumBuild = 17763;

        public SuperResolutionSkillObj()
        {
            // Check for Windows OS version before instantiating skill descriptor. Minimum supported version is 1809 build 17763 (RS5)
            if ((System.Environment.OSVersion.Version.Major == 10) && (System.Environment.OSVersion.Version.Build >= m_osVersionMinimumBuild))
            {
                // Instantiate skill descriptor. This is the gateway to skill, providing details on input/output features, available execution devices.
                m_skillDescriptor = new SuperResolutionDescriptor();
                GetDeviceList();
            }
            else
            {
                // Flag OS version incompatibility error
                m_osVersionError = true;
            }
        }

        public async void GetDeviceList()
        {
            // Get list of available execution devices from skill descriptor
            m_availableExecutionDevices = await m_skillDescriptor.GetSupportedExecutionDevicesAsync();
        }

    }*/
    public sealed partial class GamePlayerView : Page
    {
        public GamePlayerViewModel VM = new GamePlayerViewModel();

        int InitWidthSize { get => PlatformService.InitWidthSize; }
        int InitWidthSizeCustom { get => PlatformService.InitWidthSize - 20; }
        int InitWidthSizeCustom2 { get => PlatformService.InitWidthSize - 40; }
        public ObservableCollection<GroupCoreOptionListItems> coreOptionsGroupped = new ObservableCollection<GroupCoreOptionListItems>();
        public ObservableCollection<GroupButtonListItems> buttonsListItemsGroup = new ObservableCollection<GroupButtonListItems>();
        public ObservableCollection<EffectsListItem> EffectsGroupList = new ObservableCollection<EffectsListItem>();
        public ObservableCollection<EffectsListItem> EffectsGroupListTemp = new ObservableCollection<EffectsListItem>();
        HorizontalAlignment horizontalAlignment { get => PlatformService.horizontalAlignment; }
        public bool isXBOX
        {
            get
            {
                return PlatformService.isXBOX;
            }
        }
        public Thickness boxMargin = new Thickness(0, 500, 0, 0);
        public Thickness boxMargin2 = new Thickness(0, 500, 0, 0);
        public Thickness boxMargin3 = new Thickness(18, 2, 18, 2);
        bool logFileState = false;
        bool LogFileState
        {
            get
            {
                return logFileState;
            }
            set
            {
                logFileState = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("LogFileState", value);
                if (VM != null && VM.EmulationService != null)
                {
                    VM.EmulationService.UpdateCoreDebugFile(value);
                }
            }
        }

        bool vfsFileState = false;
        bool VFSFileState
        {
            get
            {
                return vfsFileState;
            }
            set
            {
                vfsFileState = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("VFSFileState", value);
                if (VM != null && VM.EmulationService != null)
                {
                    VM.EmulationService.VFSLogFile = VFSFileState;
                }
            }
        }
        public GamePlayerView()
        {
            RestoreVisualizerSettings();
            CompleteLoading();
        }


        async void CompleteLoading()
        {
            //To avoid XAML exceptions, I shouldn't load any element until VM Ready
            while (VM == null || !VM.VMREADY)
            {
                await Task.Delay(1000);
            }
            var launchEnviroment = PlatformService.gameLaunchEnvironment;
            var coreName = launchEnviroment.Core.Name;
            try
            {
                var IsNewCore = launchEnviroment.Core.IsNewCore;
                var systemName = launchEnviroment.SystemName;
                var gamePath = launchEnviroment.MainFilePath;
                var gameSPath = launchEnviroment.MainFileStoredPath;
                string gameID = "";
                if (gameSPath != null)
                {
                    gameID = PlatformService.GetGameIDByLocation(coreName, systemName, gameSPath, IsNewCore);
                }
                await loadPorts(gameID, coreName);
            }
            catch (Exception ex)
            {
                await loadPorts("", coreName);
            }
            await VM.Prepare(launchEnviroment);

            //I can now safely load the elements;
            await Task.Delay(PlatformService.isMobile ? 1500 : 500);
            if (PlatformService.ExtraDelay)
            {
                await Task.Delay(PlatformService.isMobile ? 500 : 500);
            }
            InitializeComponent();
            if (PlatformService.ExtraDelay)
            {
                await Task.Delay(PlatformService.isMobile ? 500 : 500);
            }
            PlatformService.GamePlayPageUpdateBindings = GamePlayPageUpdateBindings;

            VM.ViewAppeared();

            GamePadPorts.SelectedItem = Port1;

            this.Tapped += GamePlayerView_Tapped;
            this.PointerPressed += GamePlayerView_PointerPressed;

            PlatformService.VFSIndicatorHandler = VFSIndicatorHandler;
            PlatformService.LogIndicatorHandler = LogIndicatorHandler;
            PlatformService.LEDIndicatorHandler = LEDIndicatorHandler;

            LogFileState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("LogFileState", false);
            logToFile.IsChecked = LogFileState;
            VFSFileState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("VFSFileState", false);
            VFSLog.IsChecked = VFSFileState;

            PlatformService.NotificationHandler = pushLocalNotification;

            Unloaded += OnUnloading;

            PlatformService.videoService.RenderPanel = PlayerPanel;
            try
            {
                CoreOptionListItemsGroup.Source = coreOptionsGroupped;
                ButtonsListItemsGroup.Source = buttonsListItemsGroup;
            }
            catch (Exception ex)
            {

            }
            try
            {
                if (PlatformService.isXBOX)
                {
                    VM.HideAllUI();
                }

                VM.UpdateItemState = UpdateItemState;
                VM.UpdateXBOXListPosition = UpdateXBOXListPosition;
                VM.RestoreXBOXListPosition = RestoreXBOXListPosition;

                PlatformService.SaveButtonHandler = SaveButtonHandler;
                PlatformService.ResetButtonHandler = ResetButtonHandler;
                PlatformService.CancelButtonHandler = CancelButtonHandler;
                PlatformService.HideLogsHandler = CloseLogListHandler;
                PlatformService.HideKeyboardHandler = HideKeyboardHandler;
                PlatformService.ShowKeyboardHandler = ShowKeyboardHandler;
                PlatformService.HideEffects = HideEffects;
                PlatformService.ScreenshotHandler = ScreenshotHandler;
                PlatformService.ResolveCanvasSizeHandler = ResolveCanvasSizeHandler;
                PlatformService.SlidersDialogHandler = SlidersDialogHandler;

                PlatformService.KeyboardRequestedHandler = (sender, e) =>
                {
                    KeyboardToggle.IsChecked = !KeyboardToggle.IsChecked.Value;
                };
                PlatformService.SnapshotRequestedHandler = (sender, e) =>
                {
                    TakeScreenShot();
                };

                await VM.StartGame(launchEnviroment);

                UpdateBindingsWithDelay();

                if (PlatformService.ExtraDelay)
                {
                    await Task.Delay(PlatformService.isMobile ? 350 : 100);
                }
                BindingsUpdate();
                try
                {
                    SetCoreOptionsHandler();
                    RightVirtualPad.GetButtonMap();
                    PreviousPoint.X = 0;
                    PreviousPoint.Y = 0;

                    Window.Current.SizeChanged += windowsSizeChanged;
                    PlatformService.checkInitWidth(false);
                    var customGameControls = await CustomGamePadRetrieveGameAsync();
                    if (!customGameControls)
                    {
                        await CustomGamePadRetrieveAsync();
                    }
                    CheckEffectsBoxMargin();
                }
                catch (Exception e)
                {

                }

                if (PlatformService.ExtraDelay)
                {
                    await Task.Delay(PlatformService.isMobile ? 350 : 200);
                }
                buildLayouts();
                ShowStartScreen();
                try
                {
                    var customGameScale = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{VM.EmulationService.GetGameID()}Custom", false);
                    string PKey = $"{VM.CoreName}";
                    if (customGameScale)
                    {
                        try
                        {
                            PKey = $"{VM.EmulationService.GetGameID()}";
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    SX = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{PKey}SCALEX", 1d);
                    SY = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{PKey}SCALEY", 1d);
                    PX = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{PKey}OFFSETX", 0d);
                    PY = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{PKey}OFFSETY", 0d);
                    try
                    {
                        var ASRKey = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{PKey}ASPECT", "Auto");
                        ASR = aspects[ASRKey];
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        selectedDevice = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{PKey}AIDEVICE", 0);
                    }
                    catch (Exception ex)
                    {

                    }
                    UpdatePanelSize();
                }
                catch (Exception ex)
                {

                }
                if (PlatformService.UpdateMouseButtonsState != null)
                {
                    PlatformService.UpdateMouseButtonsState.Invoke(null, null);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
            if (PlatformService.ExtraDelay)
            {
                await Task.Delay(PlatformService.isMobile ? 350 : 200);
            }
            try
            {
                PrepareUpscaleSupport();
                upscaleHandler = (s, e) =>
                {
                    PrepareUpscaleSupport();
                };
            }
            catch (Exception ex)
            {

            }
            try
            {
                if (VM.AVDebug)
                {
                    PrepareAudioVisualizer();
                }
            }
            catch (Exception ex)
            {

            }
            if (PlatformService.ExtraDelay)
            {
                await Task.Delay(PlatformService.isMobile ? 500 : 500);
            }
            try
            {
                buildInGameMenu();
                VM.MenuGridUpdate = UpdateInGameMenuItems;
            }
            catch (Exception ex)
            {

            }
        }


        // Instance of parent skill class
        //SuperResolutionSkillObj m_psSkill;
        // State variables used while switching from skill output to regular upscale
        private enum ScaledOutputType { SuperResolution, LinearUpsample };
        private ScaledOutputType m_currentOutputType = ScaledOutputType.SuperResolution;
        List<string> UISkillExecutionDevices = new List<string>();
        int selectedDevice = 0;
        // Local copy of parent skill object

        public static EventHandler upscaleHandler;
        public static bool upscaleActive = false;
        public static bool isUpscaleActive
        {
            get
            {
                return upscaleActive;
            }
            set
            {
                upscaleActive = value;
                if (upscaleActive && !isUpscaleRunning && App.GameStarted && upscaleHandler != null)
                {
                    upscaleHandler.Invoke(null, null);
                }
            }
        }
        bool isUpscaleReady = false;
        static bool isUpscaleRunning = false;
        CancellationTokenSource upscaleTasksCancel = new CancellationTokenSource();
        public static TaskCompletionSource<bool> upscaleDisposed = new TaskCompletionSource<bool>();
        public async void PrepareUpscaleSupport()
        {
            try
            {
                upscaleTasksCancel.Cancel();
            }
            catch (Exception ex)
            {

            }
            try
            {
                upscaleDisposed.SetResult(true);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task UpscaleSupport(VideoFrame frame, CancellationTokenSource cancellationTokenSource, StorageFile output = null)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }
        private async Task<byte[]> EncodedBytes(SoftwareBitmap soft, Guid encoderId)
        {
            byte[] array = null;

            // First: Use an encoder to copy from SoftwareBitmap to an in-mem stream (FlushAsync)
            // Next:  Use ReadAsync on the in-mem stream to get byte[] array

            using (var ms = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, ms);
                encoder.SetSoftwareBitmap(soft);

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception ex) { return new byte[0]; }

                array = new byte[ms.Size];
                await ms.ReadAsync(array.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
            }
            return array;
        }

        // Used as a container for output image frame
        private SoftwareBitmapSource m_bitmapSource = new SoftwareBitmapSource();

        private async Task UpscaleCurrentView(CancellationTokenSource cancellationTokenSource)
        {
            if (isUpscaleActive && isUpscaleReady)
            {
                try
                {
                    {
                        RenderTargetBitmap _bitmap = new RenderTargetBitmap();
                        await _bitmap.RenderAsync(PlayerPanel);
                        bool bufferOK = true;
                        byte[] pixels = null;
                        var width = _bitmap.PixelWidth;
                        var height = _bitmap.PixelHeight;
                        var pixelBuffer = await _bitmap.GetPixelsAsync();
                        _bitmap = null;

                        try
                        {
                            pixels = pixelBuffer.ToArray();
                            if (pixelBuffer.Length == 0)
                            {
                                return;
                            }
                            //Blank images will be ignored
                            int pixelsSum = BitConverter.ToInt32(pixels, 0);
                            if (pixelsSum != 0)
                            {
                                bufferOK = true;
                            }
                            pixels = null;
                        }
                        catch (Exception es)
                        {

                        }

                        if (bufferOK)
                        {
                            TaskCompletionSource<bool> frameTask = new TaskCompletionSource<bool>();
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                try
                                {
                                    {
                                        VideoFrame resultFrame = null;
                                        SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height);
                                        softwareBitmap.CopyFromBuffer(pixelBuffer);
                                        pixelBuffer = null;

                                        // Convert to friendly format for UI display purpose
                                        softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                                        // Encapsulate the image in a VideoFrame instance
                                        resultFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
                                        try
                                        {
                                            //softwareBitmap.Dispose();
                                            //softwareBitmap = null;
                                        }
                                        catch (Exception e)
                                        {

                                        }
                                        await UpscaleSupport(resultFrame, cancellationTokenSource).AsAsyncAction().AsTask(cancellationTokenSource.Token);
                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                                frameTask.SetResult(true);
                            }).AsTask(cancellationTokenSource.Token);
                            await frameTask.Task;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        private async Task UpscaleScreenshot(StorageFile file, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                /*VideoFrame resultFrame = null;
                SoftwareBitmap softwareBitmap = null;

                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    // Create the decoder from the stream 
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                    // Get the SoftwareBitmap representation of the file in BGRA8 format
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    // Convert to friendly format for UI display purpose
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                // Encapsulate the image in a VideoFrame instance
                resultFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);*/
                await UpscaleSupport(null, cancellationTokenSource, file);
            }
            catch (Exception ex)
            {

            }
        }
        private void windowsSizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            try
            {
                CheckEffectsBoxMargin();
                BindingsUpdate();
                ApplicationView currentView = ApplicationView.GetForCurrentView();
                if (currentView.Orientation == ApplicationViewOrientation.Landscape)
                {
                    //When the app in landscap mode (if mobile or tablet), screen fit is prefered
                    if (VM != null)
                    {
                        if (PlatformService.DeviceIsPhone() || PlatformService.isTablet)
                        {
                            if (VM.FitScreen == 2 && !VM.FitScreenChangedByUser)
                            {
                                VM.ToggleFitScreen();
                            }
                        }
                    }
                }
                else if (currentView.Orientation == ApplicationViewOrientation.Portrait)
                {
                    //When the app in portrait mode (if mobile or tablet), screen fit is not prefered
                    if (VM != null)
                    {
                        if (PlatformService.DeviceIsPhone() || PlatformService.isTablet)
                        {
                            if (VM.FitScreen == 4 && !VM.FitScreenChangedByUser)
                            {
                                VM.ToggleFitScreen();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        bool ShowExtraShortCuts = true;
        bool ShowExtraShortCuts2 = true;

        private async void buildLayouts()
        {
            try
            {
                //Delay build for few seconds
                await Task.Delay(2500);
                BuildKeyboardLayout();
                buildEffects();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindingsUpdate()
        {
            try
            {
                Bindings.Update();
                if (PlatformService.GameOverlaysUpdateBindings != null)
                {
                    PlatformService.GameOverlaysUpdateBindings.Invoke(null, null);
                }
            }
            catch (Exception e)
            {

            }
        }

        bool UseL3R3InsteadOfX1X2
        {
            get
            {
                return PlatformService.UseL3R3InsteadOfX1X2;
            }
            set
            {
                PlatformService.UseL3R3InsteadOfX1X2 = value;
                if (PlatformService.UseL3R3InsteadOfX1X2Updater != null)
                {
                    PlatformService.UseL3R3InsteadOfX1X2Updater.Invoke(null, null);
                }
            }
        }
        bool ThumbstickSate
        {
            get
            {
                return PlatformService.ThumbstickSate;
            }
            set
            {
                PlatformService.ThumbstickSate = value;
            }
        }
        bool TapEvent
        {
            get
            {
                return PlatformService.TapEvent;
            }
            set
            {
                PlatformService.TapEvent = value;
            }
        }
        bool MouseSate
        {
            get
            {
                return PlatformService.MouseSate;
            }
            set
            {
                PlatformService.MouseSate = value;
                if (PlatformService.UpdateMouseButtonsState != null)
                {
                    PlatformService.UpdateMouseButtonsState.Invoke(null, null);
                }
            }
        }
        bool DynamicPosition
        {
            get
            {
                return PlatformService.DynamicPosition;
            }
            set
            {
                PlatformService.DynamicPosition = value;
            }
        }
        private async void OptionsInfo2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("notice");
                await UserDialogs.Instance.AlertAsync("These options only for actions\n\nSwap\nSwap between directions (Left / Right)\n\nLeft -> Right\nRight -> Left\n\nSlots\nUse action buttons as save state instead of actions");

            }
            catch (Exception ex)
            {

            }
        }

        private Timer LogTimer;
        bool timerState = false;
        private void callResizeTimer(bool startState = false)
        {
            try
            {
                LogTimer?.Dispose();
                timerState = false;
                if (startState)
                {
                    LogTimer = new Timer(async delegate
                    {
                        if (!timerState)
                        {
                            await ResolveCanvasSize();
                        }
                    }, null, 0, 1100);
                }
            }
            catch (Exception e)
            {

            }
        }

        private async void ShowStartScreen()
        {
            await Task.Run(async () =>
            {
                try
                {
                    while (VM == null || (!VM.FailedToLoadGame && !VM.SystemInfoVisiblity))
                    {
                        await Task.Delay(500);
                    }
                    if (VM.SystemInfoVisiblity)
                    {
                        _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                        {
                            try
                            {
                                ResolveCanvasSize();
                                callResizeTimer(true);
                                SystemInfoGrid.Opacity = 0;
                                SystemInfoGrid.Visibility = Visibility.Visible;
                                await SystemInfoGrid.Fade(value: 1.0f, duration: 700, delay: 100).StartAsync();
                                await SystemInfoGrid.Offset(offsetX: -300, offsetY: 0, duration: 800, delay: 2000).StartAsync();
                                await Task.Delay(3000);
                                SystemInfoGrid.Visibility = Visibility.Collapsed;
                                await Task.Delay(2000);
                                BindingsUpdate();
                                UpdateKeyboardKeysByRequested();
                            }
                            catch
                            {

                            }
                        });
                    }
                }
                catch (Exception ex)
                {

                }
            });
        }


        public void LogIndicatorHandler(object sender, EventArgs args)
        {
            LogIndicator(sender);
        }
        public void LEDIndicatorHandler(object sender, EventArgs args)
        {
            LEDIndicator(sender);
        }
        public void VFSIndicatorHandler(object sender, EventArgs args)
        {
            VFSIndicator(sender);
        }
        private async void VFSIndicator(object sender)
        {
            if (!PlatformService.ShowIndicators)
            {
                return;
            }
            await Task.Run(async () =>
            {
                try
                {
                    var senderMessage = "";
                    try
                    {
                        senderMessage = (string)sender;
                    }
                    catch (Exception ex)
                    {

                    }
                    bool isVFSError = false;
                    if (senderMessage.ToLower().Contains("error") || senderMessage.ToLower().Contains("exception") || senderMessage.ToLower().Contains("unabled") || senderMessage.ToLower().Contains("failed") || senderMessage.ToLower().Contains("not found") || senderMessage.ToLower().Contains("missing") || senderMessage.ToLower().Contains("cannot"))
                    {
                        isVFSError = true;
                    }
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        try
                        {
                            (isVFSError ? VFSErrorCounterGrid : VFSCounterGrid).Opacity = 0;
                            (isVFSError ? VFSErrorCounterGrid : VFSCounterGrid).Visibility = Visibility.Visible;
                            //await (isVFSError ? VFSErrorCounterGrid : VFSCounterGrid).Fade(value: 1.0f, duration: 100, delay: 50).StartAsync();
                            //await (isVFSError ? VFSErrorCounterGrid : VFSCounterGrid).Fade(value: 0.0f, duration: 100, delay: 50).StartAsync();
                            (isVFSError ? VFSErrorCounterGrid : VFSCounterGrid).Opacity = 1;
                            await Task.Delay(50);
                            (isVFSError ? VFSErrorCounterGrid : VFSCounterGrid).Visibility = Visibility.Collapsed;
                        }
                        catch
                        {

                        }
                    });
                }
                catch (Exception ex)
                {

                }
            });
        }

        private async void LogIndicator(object sender)
        {
            if (!PlatformService.ShowIndicators)
            {
                return;
            }
            await Task.Run(async () =>
            {
                try
                {
                    var senderMessage = "";
                    try
                    {
                        senderMessage = (string)sender;
                    }
                    catch (Exception ex)
                    {

                    }
                    bool isLogError = false;
                    if (senderMessage.ToLower().Contains("error") || senderMessage.ToLower().Contains("exception") || senderMessage.ToLower().Contains("unabled") || senderMessage.ToLower().Contains("failed") || senderMessage.ToLower().Contains("not found") || senderMessage.ToLower().Contains("missing") || senderMessage.ToLower().Contains("cannot"))
                    {
                        isLogError = true;
                    }
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        try
                        {
                            (isLogError ? LogErrorCounterGrid : LogCounterGrid).Opacity = 0;
                            (isLogError ? LogErrorCounterGrid : LogCounterGrid).Visibility = Visibility.Visible;
                            //await (isLogError ? LogErrorCounterGrid : LogCounterGrid).Fade(value: 0.8f, duration: 100, delay: 50).StartAsync();
                            //await (isLogError ? LogErrorCounterGrid : LogCounterGrid).Fade(value: 0.0f, duration: 100, delay: 50).StartAsync();
                            (isLogError ? LogErrorCounterGrid : LogCounterGrid).Opacity = 1;
                            await Task.Delay(50);
                            (isLogError ? LogErrorCounterGrid : LogCounterGrid).Visibility = Visibility.Collapsed;
                        }
                        catch
                        {

                        }
                    });
                }
                catch (Exception ex)
                {

                }
            });
        }

        private async void LEDIndicator(object sender)
        {
            if (!PlatformService.ShowIndicators)
            {
                return;
            }
            await Task.Run(async () =>
            {
                try
                {
                    int type = 0;
                    try
                    {
                        type = (int)sender;
                    }
                    catch (Exception ex)
                    {

                    }
                    var LEDState = false;
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        try
                        {
                            (LEDState ? LEDOffCounterGrid : LEDCounterGrid).Opacity = 0;
                            (LEDState ? LEDOffCounterGrid : LEDCounterGrid).Visibility = Visibility.Visible;
                            //await (LEDState ? LEDOffCounterGrid : LEDCounterGrid).Fade(value: 0.8f, duration: 100, delay: 50).StartAsync();
                            //await (LEDState ? LEDOffCounterGrid : LEDCounterGrid).Fade(value: 0.0f, duration: 100, delay: 50).StartAsync();
                            (LEDState ? LEDOffCounterGrid : LEDCounterGrid).Opacity = 1;
                            await Task.Delay(50);
                            (LEDState ? LEDOffCounterGrid : LEDCounterGrid).Visibility = Visibility.Collapsed;
                        }
                        catch
                        {

                        }
                    });
                }
                catch (Exception ex)
                {

                }
            });
        }

        private void EffectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (EffectBuildInProgress)
                {
                    return;
                }
                var selectItem = (ComboBoxItem)e.AddedItems[0];
                if (selectItem != null)
                {
                    EffectsGroupList.Clear();

                    var SelectedValue = selectItem.Tag.ToString();
                    if (SelectedValue.Equals("All"))
                    {
                        foreach (var tItem in EffectsGroupListTemp)
                        {
                            EffectsGroupList.Add(tItem);
                        }
                    }
                    else
                    {
                        foreach (var eItem in EffectsGroupListTemp)
                        {
                            if (eItem.Tag.Equals(SelectedValue))
                            {
                                EffectsGroupList.Add(eItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        bool EffectBuildInProgress = true;
        public void buildEffects()
        {
            try
            {
                EffectBuildInProgress = true;
                EffectsLoadingProgress.Visibility = Visibility.Visible;
                EffectsLoadingProgress.IsActive = true;
                EffectsList.IsEnabled = false;
                EffectsList.Items.Clear();
                EffectsGroupList.Clear();
                EffectsGroupListTemp.Clear();
                ComboBoxItem allEffectsItem = new ComboBoxItem();
                allEffectsItem.Tag = "All";
                allEffectsItem.Content = "All Effects";
                EffectsList.Items.Add(allEffectsItem);

                Dictionary<string, List<EffectValue>> EffectsMap = new Dictionary<string, List<EffectValue>>()
                {
                    {
                    "Brightness|BrightnessEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Brightness Level","BrightnessLevel", 1, 0, 0.01)
                        /*               ^^^^^^^^^^^        ^^^^^^^^^^^^^     ^  ^  ^       */
                        /*               Slider Title       Slider Variable  Max Min Step   */
                    }
                    },

                    {
                    "Contrast|ContrastEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Contrast Level","ContrastLevel", 1, -1, 0.01)
                    }
                    },

                    {
                    "Directional Blur|DirectionalBlurEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Amount","BlurAmount", 50, 0, 0.01),
                        new EffectValue("Angle","Angle", 3.6, 0, 0.1)
                    }
                    },

                    {
                    "Edge Detection|EdgeDetectionEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Amount","AmountEdge", 1, 0, 0.01),
                        new EffectValue("Blur","BlurAmountEdge", 10, 0, 0.1)
                    }
                    },

                    {
                    "Emboss|EmbossEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Amount","AmountEmboss", 10, 0, 0.01),
                        new EffectValue("Angle","AngleEmboss", 3.6, 0, 0.1)
                    }
                    },

                    {
                    "Exposure|ExposureEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Amount","Exposure", 2, -2, 0.01),
                    }
                    },

                    {
                    "Gaussian Blur|GaussianBlurEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Amount","BlurAmountGaussianBlur", 50, 0, 0.1),
                    }
                    },

                    {
                    "Morphology|MorphologyEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Height","HeightMorphology", 50, 1, 0.1),
                    }
                    },

                    {
                    "Saturation|SaturationEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Level","Saturation", 1, 0, 0.01),
                    }
                    },

                    {
                    "Sepia|SepiaEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Intensity","Intensity", 1, 0, 0.01),
                    }
                    },

                    {
                    "Sharpen|SharpenEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Amount","AmountSharpen", 10, 0, 0.01),
                    }
                    },

                    {
                    "Straighten|StraightenEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Angle","AngleStraighten", 3.6, -3.6, 0.01),
                    }
                    },

                    {
                    "Vignette|VignetteEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Amount","AmountVignette", 1, 0, 0.01),
                        new EffectValue("Curve","Curve", 1, 0, 0.01),
                    }
                    },

                    {
                    "Transform 3D|Transform3DEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Rotate","Rotate", 3.6, 0, 0.01),
                        new EffectValue("RotateX","RotateX", 3.6, 0, 0.01),
                        new EffectValue("RotateY","RotateY", 3.6, 0, 0.01),
                    }
                    },

                    {
                    "Temperature And Tint|TemperatureAndTintEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Temperature","Temperature", 1, -1, 0.01),
                        new EffectValue("Tint","Tint", 1, -1, 0.01),
                    }
                    },

                    {
                    "Tile|TileEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Left","Left", FramebufferConverter.currentWidth, 0, 0.01),
                        new EffectValue("Top","Top", FramebufferConverter.currentHeight, 0, 0.01),
                        new EffectValue("Right","Right", FramebufferConverter.currentWidth, 0, 0.01),
                        new EffectValue("Bottom","Bottom", FramebufferConverter.currentHeight, 0, 0.01),
                    }
                    },

                    {
                    "Crop|CropEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Left","LeftCrop", FramebufferConverter.currentWidth, 0, 0.01),
                        new EffectValue("Top","TopCrop", FramebufferConverter.currentHeight, 0, 0.01),
                        new EffectValue("Right","RightCrop", FramebufferConverter.currentWidth, 0, 0.01),
                        new EffectValue("Bottom","BottomCrop", FramebufferConverter.currentHeight, 0, 0.01),
                    }
                    },

                    {
                    "Highlights And Shadows|HighlightsAndShadowsEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Clarity","Clarity", 1, -1, 0.01),
                        new EffectValue("Highlights","Highlights", 1, -1, 0.01),
                        new EffectValue("Shadows","Shadows", 1, -1, 0.01),
                    }
                    },

                    {
                    "Scale|ScaleEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Width","WidthScale", 2, 0, 0.01),
                        new EffectValue("Height","HeightScale", 2, 0, 0.01),
                        new EffectValue("Sharpness","SharpnessScale", 1, 0, 0.01),
                    }
                    },

                    {
                    "Posterize|PosterizeEffect",
                    new List<EffectValue>()
                    {
                        new EffectValue("Red","Red", 16, 2, 0.01),
                        new EffectValue("Green","Green", 16, 2, 0.01),
                        new EffectValue("Blue","Blue", 16, 2, 0.01),
                    }
                    },

                    {
                    "Grayscale|GrayscaleEffect",
                    new List<EffectValue>()
                    {

                    }
                    },

                    {
                    "Rgb To Hue|RgbToHueEffect",
                    new List<EffectValue>()
                    {

                    }
                    },

                    {
                    "Invert|InvertEffect",
                    new List<EffectValue>()
                    {

                    }
                    },

                    {
                    "Hue To Rgb|HueToRgbEffect",
                    new List<EffectValue>()
                    {

                    }
                    },
                };

                foreach (var eItem in EffectsMap)
                {
                    var data = eItem.Key.Split('|');
                    EffectsListItem effectsListItem = new EffectsListItem(VM, data[1], data[0], eItem.Value);
                    EffectsGroupList.Add(effectsListItem);
                    EffectsGroupListTemp.Add(effectsListItem);
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = data[0];
                    comboBoxItem.Tag = data[1];
                    EffectsList.Items.Add(comboBoxItem);
                }
                EffectsList.SelectedItem = allEffectsItem;
            }
            catch (Exception ex)
            {

            }
            EffectsLoadingProgress.Visibility = Visibility.Collapsed;
            EffectsLoadingProgress.IsActive = false;
            EffectsList.IsEnabled = true;
            EffectBuildInProgress = false;
        }

        int columnSpan = 3;
        int currentRow = 1;
        private void resolveOptionsAndEffectsListsOnWideScreen()
        {
            try
            {
                var currentHeight = Window.Current.CoreWindow.Bounds.Height;
                var currentWidth = Window.Current.CoreWindow.Bounds.Width;
                if (currentWidth > 1000 || !PlatformService.AdjustInGameLists)
                {
                    //columnSpan = 1;
                    currentRow = 0;
                    if (PlatformService.AdjustInGameLists)
                    {
                        SavesPanel.MaxWidth = 350;
                        SavesList.MaxWidth = 350;
                        CoresOptionsPanel.MaxWidth = 350;
                        CoreOptionsPage.MaxWidth = 350;
                        EffectsPanel.MaxWidth = 400;
                        EffectsPageList.MaxWidth = 400;
                        ControlsPanel.MaxWidth = 400;
                        ControlsPage.MaxWidth = 400;
                    }
                    boxMargin = new Thickness(0, 0, 0, 0);
                    CoresOptionsPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    if (PlatformService.AdjustInGameLists)
                    {
                        CoresOptionsPanel.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                    EffectsPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    if (PlatformService.AdjustInGameLists)
                    {
                        EffectsPanel.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                    SavesPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    if (PlatformService.AdjustInGameLists)
                    {
                        SavesPanel.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                    ControlsPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    if (PlatformService.AdjustInGameLists)
                    {
                        ControlsPanel.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void CheckEffectsBoxMargin()
        {
            try
            {
                columnSpan = 3;
                currentRow = 1;
                var currentHeight = Window.Current.CoreWindow.Bounds.Height;
                var currentWidth = Window.Current.CoreWindow.Bounds.Width;

                try
                {
                    //var displayInfo = DisplayInformation.GetForCurrentView();
                    //var displayScale = (double)displayInfo.ResolutionScale;
                    //currentHeight = currentHeight * (100d / displayScale);
                }
                catch (Exception x)
                {

                }

                CoresOptionsPanel.MaxWidth = currentWidth;
                CoreOptionsPage.MaxWidth = 600;
                EffectsPanel.MaxWidth = currentWidth;
                EffectsPageList.MaxWidth = 600;
                SavesPanel.MaxWidth = currentWidth;
                SavesList.MaxWidth = 600;
                ControlsPanel.MaxWidth = currentWidth;
                ControlsPage.MaxWidth = 600;

                CoresOptionsPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                EffectsPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                SavesPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                ControlsPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

                boxMargin3 = new Thickness(18, 2, 18, 2);

                if (currentHeight > 1000)
                {
                    boxMargin = new Thickness(0, 500, 0, 0);
                    boxMargin2 = new Thickness(0, 350, 0, 0);
                    if (currentWidth > 1200)
                    {
                        boxMargin3 = new Thickness(100, 2, 100, 2);
                    }
                }
                else if (currentHeight > 800)
                {
                    boxMargin = new Thickness(0, 400, 0, 0);
                    boxMargin2 = new Thickness(0, 300, 0, 0);
                }
                else if (currentHeight > 650)
                {
                    boxMargin = new Thickness(0, 350, 0, 0);
                    boxMargin2 = new Thickness(0, 250, 0, 0);
                }
                else if (currentHeight > 550)
                {
                    boxMargin = new Thickness(0, 250, 0, 0);
                    boxMargin2 = new Thickness(0, 150, 0, 0);
                }
                else if (currentHeight > 450)
                {
                    boxMargin = new Thickness(0, 150, 0, 0);
                    boxMargin2 = new Thickness(0, 100, 0, 0);
                }
                else
                {
                    boxMargin = new Thickness(0, 80, 0, 0);
                    boxMargin2 = new Thickness(0, 80, 0, 0);
                }
                if (currentWidth > 600)
                {
                    ShowExtraShortCuts = true;
                }
                else
                {
                    ShowExtraShortCuts = false;
                }
                if (currentWidth > 780)
                {
                    ShowExtraShortCuts2 = true;
                }
                else
                {
                    ShowExtraShortCuts2 = false;
                }
                UpdateKeyboardLayout();
                resolveOptionsAndEffectsListsOnWideScreen();
                ResolveCanvasSize();
            }
            catch (Exception e)
            {

            }
        }

        public void ResolveCanvasSizeHandler(object sender, EventArgs args)
        {
            ResolveCanvasSize();
        }

        public static int[] ASR = new int[] { 0, 0 };
        public double currentKeyboardScale = 1.0;
        private async Task ResolveCanvasSize(bool force = false)
        {
            try
            {
                if (!PlatformService.AutoFitResolver && !force)
                {
                    return;
                }
                timerState = true;

                await Task.Delay(100);
                if (PlatformService.ExtraDelay)
                {
                    await Task.Delay(PlatformService.isMobile ? 1000 : 500);
                }
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    try
                    {
                        var width = 0d;
                        var height = 0d;
                        var currentHeight = 0d;
                        var currentWidth = 0d;

                        if (PlatformService.isXBOX || PlatformService.XBoxMode || VM == null)
                        {
                            if (PlayerPanel.VerticalAlignment != VerticalAlignment.Center)
                            {
                                PlayerPanel.VerticalAlignment = VerticalAlignment.Center;
                            }
                            width = (double)FramebufferConverter.currentWidth;
                            height = (double)FramebufferConverter.currentHeight;
                            currentHeight = Window.Current.CoreWindow.Bounds.Height;
                            currentWidth = Window.Current.CoreWindow.Bounds.Width;
                        }
                        else if (VM.FitScreenState || !VM.DisplayTouchGamepad)
                        {
                            if (PlayerPanel.VerticalAlignment != VerticalAlignment.Center)
                            {
                                PlayerPanel.VerticalAlignment = VerticalAlignment.Center;
                            }
                            width = (double)FramebufferConverter.currentWidth;
                            height = (double)FramebufferConverter.currentHeight;
                            currentHeight = Window.Current.CoreWindow.Bounds.Height;
                            currentWidth = Window.Current.CoreWindow.Bounds.Width;
                        }
                        else
                        {
                            if (PlayerPanel.VerticalAlignment != VerticalAlignment.Top)
                            {
                                PlayerPanel.VerticalAlignment = VerticalAlignment.Top;
                            }
                            width = (double)FramebufferConverter.currentWidth;
                            height = (double)FramebufferConverter.currentHeight;

                            currentHeight = PanelRow.ActualHeight;
                            currentWidth = Window.Current.CoreWindow.Bounds.Width;
                        }
                        if (VM != null && VM.EmulationService != null && VM.EmulationService.currentCore != null)
                        {
                            //It's not important step anymore after the aspect correction
                            /*switch (VM.EmulationService.currentCore.Name)
                            {
                                case "Caprice32":
                                case "cap32":
                                    width = width / 2;
                                    break;

                                case "xrick":
                                    height = height + 40;
                                    break;

                                case "FCEUmm":
                                case "Nestopia":
                                case "QuickNES":
                                    width = width + 35;
                                    break;

                                case "Snes9x":
                                case "Snes9x 2005":
                                    width = width + 43;
                                    break;

                                case "Genesis Plus GX":
                                    height = height + 23;
                                    break;

                                case "Genesis Plus GX Wide":
                                    height = height + 23;
                                    break;

                                case "Beetle PC-FX":
                                    width = width + 50;
                                    break;

                                case "DuckStation":
                                    height = height + 25;
                                    break;

                                case "Beetle PSX":
                                    height = height + 20;
                                    break;

                                case "Beetle Saturn":
                                    width = width + 33;
                                    break;

                                case "NeoCD":
                                    height = height + 15;
                                    break;

                                case "Beetle PCE Fast":
                                    width = width + 35;
                                    break;

                                case "Beetle PCE":
                                    width = width + 35;
                                    break;

                                case "PrBoom":
                                    height = height + 40;
                                    break;

                                case "TyrQuake":
                                    height = height + 40;
                                    break;

                                case "Stella":
                                case "Stella 2014":
                                    height = height / 2;
                                    break;
                            }*/
                        }
                        if (width > 0 && height > 0)
                        {
                            try
                            {
                                double aspectRatio_X = ASR[0];
                                double aspectRatio_Y = ASR[1];

                                double targetHeight = height;
                                if (aspectRatio_X == 0 && aspectRatio_Y == 0)
                                {
                                    //get core aspect
                                    targetHeight = Convert.ToDouble(width) / VM.VideoService.GetAspectRatio();
                                }
                                else
                                {
                                    targetHeight = Convert.ToDouble(width) / (aspectRatio_X / aspectRatio_Y);
                                }
                                height = targetHeight;
                            }
                            catch (Exception ex)
                            {

                            }
                            float ratioX = (float)currentWidth / (float)width;
                            float ratioY = (float)currentHeight / (float)height;
                            float ratio = Math.Min(ratioX, ratioY);

                            float sourceRatio = (float)width / (float)height;

                            // New width and height based on aspect ratio
                            int newWidth = (int)(width * ratio);
                            int newHeight = (int)(height * ratio);

                            if (PlayerPanel.Height != newHeight)
                            {
                                PlayerPanel.Height = newHeight;
                            }
                            if (PlayerPanel.Width != newWidth)
                            {
                                PlayerPanel.Width = newWidth;
                            }
                        }

                        try
                        {
                            //ViewBox now will solve the scale issue
                            /*if (KeyboardVisible)
                            {
                                var MainKeysWidth = TheKeyboard.ActualWidth;
                                var diffs = 1.0 - (MainKeysWidth / currentWidth);
                                var scale = (1.0 + diffs);
                                if (scale != currentKeyboardScale)
                                {
                                    currentKeyboardScale = scale;
                                    TheKeyboard.RenderTransform = new ScaleTransform
                                    {
                                        ScaleX = 1,
                                        ScaleY = 1,
                                        CenterX = 0,
                                        CenterY = 0
                                    };
                                    KeyboardPanel.UpdateLayout();
                                }
                            }*/
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            PlayerPanel.VerticalAlignment = VerticalAlignment.Stretch;
                            PlayerPanel.Width = Double.NaN;
                            PlayerPanel.Height = Double.NaN;
                        }
                        catch (Exception ecx)
                        {

                        }
                    }
                    try
                    {
                        if (VM.AVDebug)
                        {
                            if (_lineSpectrum == null)
                            {
                                PrepareAudioVisualizer();
                            }
                        }
                        else
                        {
                            if (_lineSpectrum != null)
                            {
                                _audioProvider = null;
                                _lineSpectrum = null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    timerState = false;
                });
            }
            catch (Exception ex)
            {

            }
        }


        public static int[] FFTSizeArray = new int[] { 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };
        public static LineSpectrum _lineSpectrum;
        public static IAudioProvider _audioProvider;
        private const FftSize FftSize = Visualizer.UI.DSP.FftSize.Fft4096;
        public static int BarOpacity = 80;
        public static int BarCount = 5;
        public static int BarSpacing = 2;
        public static int BarScalingStrategy = 2;
        public static int MinimumFrequency = 20;
        public static int MaximumFrequency = 2;
        public static int FftSizeValue = 6;
        public static bool UseAverage = false;
        public static bool IsXLogScale = true;
        public static bool LeftChannelState = true;
        public static bool RightChannelState = true;
        public static bool AutoScaleDown = true;

        public void RestoreVisualizerSettings()
        {
            try
            {
                BarOpacity = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BarOpacity", 80);
            }
            catch (Exception e)
            {

            }
            try
            {
                BarCount = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BarCount", 5);
            }
            catch (Exception e)
            {

            }
            try
            {
                BarSpacing = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BarSpacing", 2);
            }
            catch (Exception e)
            {

            }
            try
            {
                FftSizeValue = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("FftSizeValue", 6);
            }
            catch (Exception e)
            {

            }
            try
            {
                BarScalingStrategy = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BarScalingStrategy", 2);
            }
            catch (Exception e)
            {

            }
            try
            {
                MinimumFrequency = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("MinimumFrequency", 20);
            }
            catch (Exception e)
            {

            }
            try
            {
                MaximumFrequency = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("MaximumFrequency", 2);
            }
            catch (Exception e)
            {

            }
            try
            {
                UseAverage = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("UseAverage", false);
            }
            catch (Exception e)
            {

            }
            try
            {
                IsXLogScale = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("IsXLogScale", true);
            }
            catch (Exception e)
            {

            }
            try
            {
                LeftChannelState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("LeftChannelState", true);
            }
            catch (Exception e)
            {

            }
            try
            {
                RightChannelState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RightChannelState", true);
            }
            catch (Exception e)
            {

            }
            try
            {
                AutoScaleDown = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AutoScaleDown", true);
            }
            catch (Exception e)
            {

            }
            PlatformService.VisualizerSettings = VisualizerSettings;
        }
        private void PrepareAudioVisualizer(bool updateSettings = false)
        {
            try
            {
                if (!updateSettings)
                {
                    try
                    {
                        _audioProvider = null;
                        _lineSpectrum = null;
                    }
                    catch (Exception ex)
                    {

                    }
                    _audioProvider = new AudioGraphProvider(VM);
                }
                //linespectrum and voiceprint3dspectrum used for rendering some fft data
                //in oder to get some fft data, set the previously created spectrumprovider 
                _lineSpectrum = new LineSpectrum(FftSize, VM)
                {
                    SpectrumProvider = _audioProvider,
                    UseAverage = UseAverage,
                    BarCount = (BarCount + 1),
                    BarSpacing = (double)BarSpacing,
                    IsXLogScale = IsXLogScale,
                    ScalingStrategy = (ScalingStrategy)BarScalingStrategy,
                    MinimumFrequency = MinimumFrequency,
                    MaximumFrequency = MaximumFrequencyArray[MaximumFrequency]
                };
            }
            catch (Exception ex)
            {

            }
        }

        int[] MaximumFrequencyArray = new int[] { 5000, 10000, 20000, 30000, 40000, 50000 };

        public async void VisualizerSettings(object sender, EventArgs args)
        {
            try
            {
                List<RetriXSettingsItem> settingsList = new List<RetriXSettingsItem>();
                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "Use Average",
                    desc: "Average buffer calculation",
                    icon: "opus",
                    preferencesKey: "UseAverage",
                    defaultValueKey: "UseAverage",
                    onReset: false,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    });

                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "XLog Scale",
                    desc: "Enable XLog scale",
                    icon: "opus",
                    preferencesKey: "IsXLogScale",
                    defaultValueKey: "IsXLogScale",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    });

                new RetriXSettingsItem(ref settingsList,
                    group: "Channels",
                    name: "Left Channel",
                    desc: "Enable left channel",
                    icon: "feedback",
                    preferencesKey: "LeftChannelState",
                    defaultValueKey: "LeftChannelState",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    });

                new RetriXSettingsItem(ref settingsList,
                    group: "Channels",
                    name: "Right Channel",
                    desc: "Enable right channel",
                    icon: "open",
                    preferencesKey: "RightChannelState",
                    defaultValueKey: "RightChannelState",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    });


                new RetriXSettingsItem(ref settingsList,
                    group: "UI Style",
                    name: "Scale Down",
                    desc: "Auto resize bars to corner",
                    icon: "open",
                    preferencesKey: "AutoScaleDown",
                    defaultValueKey: "AutoScaleDown",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    });

                new RetriXSettingsItem(ref settingsList,
                    group: "UI Style",
                    name: "Bars Count",
                    desc: "Amount of visualization bars",
                    icon: "remote",
                    preferencesKey: "BarCount",
                    defaultValueKey: "BarCount",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    }, true, 1, 50, 1, 5);


                new RetriXSettingsItem(ref settingsList,
                    group: "UI Style",
                    name: "Bars Spacing",
                    desc: "Distance between bars",
                    icon: "remote",
                    preferencesKey: "BarSpacing",
                    defaultValueKey: "BarSpacing",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    }, true, 0, 50, 1, 2);


                new RetriXSettingsItem(ref settingsList,
                    group: "UI Style",
                    name: "Opacity",
                    desc: "Bars opacity",
                    icon: "remote",
                    preferencesKey: "BarOpacity",
                    defaultValueKey: "BarOpacity",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    }, true, 0, 100, 1, 80);

                new RetriXSettingsItem(ref settingsList,
                    group: "Behavior",
                    name: "Scale Strategy",
                    desc: "Height scale calculation",
                    icon: "remote",
                    preferencesKey: "BarScalingStrategy",
                    defaultValueKey: "BarScalingStrategy",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    }, true, 0, 2, 1, 2, new string[] { "Decibel", "Linear", "Sqrt" });


                new RetriXSettingsItem(ref settingsList,
                    group: "Behavior",
                    name: "Minimum Frequency",
                    desc: "Minimum detection frequency",
                    icon: "remote",
                    preferencesKey: "MinimumFrequency",
                    defaultValueKey: "MinimumFrequency",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    }, true, 0, 100, 1, 20);


                new RetriXSettingsItem(ref settingsList,
                    group: "Behavior",
                    name: "Maximum Frequency",
                    desc: "Maximum detection frequency",
                    icon: "remote",
                    preferencesKey: "MaximumFrequency",
                    defaultValueKey: "MaximumFrequency",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    }, true, 0, 5, 1, 2, new string[] { "5000", "10000", "20000", "30000", "40000", "50000" });


                List<string> fftSizes = new List<string>();
                foreach (var fItem in FFTSizeArray)
                {
                    fftSizes.Add($"{fItem} bands");
                }
                new RetriXSettingsItem(ref settingsList,
                    group: "Behavior",
                    name: "Fft Size",
                    desc: "FFT data size",
                    icon: "remote",
                    preferencesKey: "FftSizeValue",
                    defaultValueKey: "FftSizeValue",
                    onReset: true,
                    false, typeof(GamePlayerView), (s, e) =>
                    {
                        PrepareAudioVisualizer(true);
                    }, true, 0, 8, 1, 6, fftSizes.ToArray());



                ContentDialog contentDialog = new ContentDialog();
                contentDialog.Title = "Visualizer Settings";
                contentDialog.PrimaryButtonText = "Reset";
                contentDialog.SecondaryButtonText = "Close";
                contentDialog.IsPrimaryButtonEnabled = true;
                contentDialog.IsSecondaryButtonEnabled = true;
                StackPanel ConfigBlock = new StackPanel();
                ConfigBlock.Orientation = Orientation.Vertical;

                List<string> settingsGroups = new List<string>();
                foreach (var sItem in settingsList)
                {
                    TextBlock header = new TextBlock();
                    header.FontWeight = FontWeights.Bold;
                    header.Text = sItem.Name.ToUpper();
                    header.Foreground = new SolidColorBrush(Colors.DodgerBlue);

                    TextBlock info = new TextBlock();
                    info.Text = sItem.Desc;

                    settingsGroups.Add(sItem.Group);

                    StackPanel settingsContainer = new StackPanel();
                    settingsContainer.Children.Add(header);
                    settingsContainer.Children.Add(info);
                    if (sItem.IsCombo)
                    {
                        sItem.SettingComboItem.Margin = new Thickness(0, 0, 0, 5);
                        settingsContainer.Children.Add(sItem.SettingComboItem);
                    }
                    else
                    {
                        settingsContainer.Children.Add(sItem.SettingSwitchItem);
                    }
                    settingsContainer.Tag = sItem.Group;

                    ConfigBlock.Children.Add(settingsContainer);
                }

                ScrollViewer scrollViewer = new ScrollViewer();
                scrollViewer.Content = ConfigBlock;
                scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.MaxHeight = 300;
                scrollViewer.Margin = new Thickness(3, 7, 0, 7);
                scrollViewer.Padding = new Thickness(0, 5, 0, 5);
                scrollViewer.BorderThickness = new Thickness(0, 1, 0, 1);
                scrollViewer.BorderBrush = new SolidColorBrush(Colors.Gray);

                TextBlock notice = new TextBlock();
                notice.HorizontalAlignment = HorizontalAlignment.Stretch;
                notice.Foreground = new SolidColorBrush(Colors.Green);
                notice.Text = "Settings will be saved automatically";
                notice.FontSize = 12;
                notice.Margin = new Thickness(3, 5, 0, 0);


                Border border1 = new Border();
                border1.BorderThickness = new Thickness(0, 1, 0, 0);
                border1.BorderBrush = new SolidColorBrush(Colors.Gray);

                settingsGroups = settingsGroups.Distinct().ToList();
                settingsGroups = settingsGroups.Distinct().ToList();
                settingsGroups.Insert(0, "All");
                ComboBox groupsFilter = new ComboBox();
                groupsFilter.HorizontalAlignment = HorizontalAlignment.Stretch;
                groupsFilter.ItemsSource = settingsGroups;
                bool onBuild = true;
                groupsFilter.SelectionChanged += (s, e) =>
                {
                    if (!onBuild)
                    {
                        try
                        {
                            var targetGroup = settingsGroups[groupsFilter.SelectedIndex].ToLower();
                            foreach (var cItem in ConfigBlock.Children)
                            {
                                var targetElement = (StackPanel)cItem;
                                if (targetGroup.Equals("all") || ((string)targetElement.Tag).ToLower().Equals(targetGroup))
                                {
                                    targetElement.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    targetElement.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                };
                groupsFilter.SelectedIndex = 0;
                onBuild = false;

                StackPanel stackPanel = new StackPanel();
                stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                stackPanel.Orientation = Orientation.Vertical;
                stackPanel.Children.Add(groupsFilter);
                stackPanel.Children.Add(scrollViewer);
                stackPanel.Children.Add(notice);

                contentDialog.Content = stackPanel;
                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    //Reset settings
                    foreach (var sItem in settingsList)
                    {
                        sItem.ResetSetting();
                    }
                    VisualizerSettings(null, null);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        Style KeyboardStyle
        {
            get
            {
                if (PlatformService.isMobile)
                {
                    return this.Resources["GridViewItemStyle2"] as Style;
                }
                else
                {
                    return this.Resources["GridViewItemStyle1"] as Style;
                }
            }
        }
        private void UpdateKeyboardLayout()
        {
            try
            {
                if (keyboard != null)
                {
                }
            }
            catch (Exception ex)
            {

            }
        }
        EventHandler eventHandlerGlobal = null;
        bool gridEventReady = false;
        CancellationTokenSource noticationCancellatin = new CancellationTokenSource();
        string notificationID = "#";
        public async Task LocalNotificationManagerHideAll()
        {
            try
            {
                {
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        try
                        {
                            //await localNotificationGrid.Fade(value: 0.0f, duration: 0, delay: 0).StartAsync();
                            localNotificationGrid.Visibility = Visibility.Collapsed;
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
                noticationCancellatin.Cancel();
                await Task.Delay(5);
            }
            catch (Exception ex)
            {

            }
        }
        private async void pushLocalNotification(string text, Color background, Color forground, char icon = '\0', int time = 3, Position position = Position.Bottom, EventHandler eventHandler = null)
        {
            var tText = text;

            try
            {
                try
                {
                    noticationCancellatin.Cancel();
                }
                catch (Exception ex)
                {

                }
                notificationID = System.IO.Path.GetRandomFileName();
                if (background == Colors.Orange)
                {
                    background = Colors.DarkOrange;
                }
                else if (background == Colors.DodgerBlue)
                {
                    background = PlatformService.GetNotificationColorByMessage(tText);
                }
                noticationCancellatin = new CancellationTokenSource();
                text = text.Replace("NoTelnet", "").Replace("notelnet", "").Replace("noTelnet", "").Replace("Notelnet", "");
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    try
                    {
                        Brush backGroundColor = new SolidColorBrush(background == null ? Colors.DodgerBlue : background);
                        var foreGroundColor = new SolidColorBrush(forground == null ? Colors.White : forground);
                        if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                        {
                            try
                            {
                                Windows.UI.Xaml.Media.AcrylicBrush myBrushm = new Windows.UI.Xaml.Media.AcrylicBrush();
                                myBrushm.BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop;
                                myBrushm.TintColor = background == null ? Colors.DodgerBlue : background;
                                myBrushm.FallbackColor = background == null ? Colors.DodgerBlue : background;
                                myBrushm.TintOpacity = 0.8;
                                backGroundColor = myBrushm;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        notifcationMessageText.Text = text;
                        notifcationMessageText.Foreground = foreGroundColor;
                        localNotificationBorder.Background = backGroundColor;
                        if (icon != '\0' && icon != SegoeMDL2Assets.ActionCenterNotification)
                        {
                            NotificationIcon.Glyph = $"{icon.ToString()}";
                        }
                        else
                        {
                            icon = PlatformService.GetIconByMessage(text);
                            NotificationIcon.Glyph = $"{icon.ToString()}";
                        }
                        localNotificationGrid.VerticalAlignment = position == Position.Bottom ? VerticalAlignment.Bottom : VerticalAlignment.Top;
                        eventHandlerGlobal = eventHandler;
                        if (!gridEventReady)
                        {
                            localNotificationGrid.Tapped += (sender, e) =>
                            {
                                try
                                {
                                    if (eventHandlerGlobal != null)
                                    {
                                        eventHandlerGlobal.Invoke(null, EventArgs.Empty);
                                    }
                                    else
                                    {
                                        {
                                            _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                            {
                                                try
                                                {
                                                    await localNotificationGrid.Fade(value: 0.0f, duration: 700, delay: 0).StartAsync();
                                                    localNotificationGrid.Visibility = Visibility.Collapsed;
                                                }
                                                catch (Exception ex)
                                                {

                                                }
                                            });
                                        }
                                        noticationCancellatin.Cancel();
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            };
                            gridEventReady = true;
                        }
                        _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                        {
                            try
                            {
                                localNotificationGrid.Opacity = 0;
                                localNotificationGrid.Visibility = Visibility.Visible;
                                await localNotificationGrid.Fade(value: 1.0f, duration: 700, delay: 0).StartAsync();
                                var tempID = notificationID;
                                _ = Task.Run(async () =>
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(time)).AsAsyncAction().AsTask(noticationCancellatin.Token);
                                    if (!noticationCancellatin.IsCancellationRequested && tempID == notificationID)
                                    {
                                        _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                        {
                                            try
                                            {
                                                await localNotificationGrid.Fade(value: 0.0f, duration: 700, delay: 0).StartAsync();
                                                localNotificationGrid.Visibility = Visibility.Collapsed;
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        });
                                    }
                                }).AsAsyncAction().AsTask(noticationCancellatin.Token);

                            }
                            catch
                            {

                            }
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
            {

            }
        }

        Position DefaultPosition = Position.Top;
        public void pushLocalNotification(object sender, EventArgs args)
        {
            try
            {

                var NotificationData = (LocalNotificationData)args;
                if (NotificationData != null)
                {
                    pushLocalNotification(NotificationData.message, Colors.DodgerBlue, Colors.White, NotificationData.icon, NotificationData.time, DefaultPosition);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SetCoreOptionsHandler()
        {
            try
            {
                _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        await Task.Delay(1500);
                        VM.CoreOptionsHandler = InitialCoresOptionsMenu;
                        VM.ControlsHandler = InitialControlsMenu;
                        VM.SnapshotHandler = SaveSnapshot;

                    }
                    catch (Exception ec)
                    {
                        PlatformService.ShowErrorMessageDirect(ec);
                        OptionsLoadingProgress.Visibility = Visibility.Collapsed;
                        OptionsLoadingProgress.IsActive = false;
                    }
                });

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }


        public static Point PreviousPoint;
        bool PointerPressedState = false;
        double PointerCurrentX = 0;
        double PointerCurrentY = 0;

        private void PlayerPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void PlayerPanel_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var CurrentPosition = e.GetCurrentPoint(sender as CanvasAnimatedControl).Position;

                var x = CurrentPosition.X;
                var y = CurrentPosition.Y;

                string debugMessage = $"X: {Math.Round(x)},Y: {Math.Round(y)}";
                try
                {
                    if (VM.ShowSensorsInfo)
                    {

                        try
                        {
                            DebugText.Text = debugMessage;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }
        }
        private void Moved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                /*if(InputService.isMouseAvailable() && InputService.CoreRequestingMousePosition)
                {
                    //This code should move the cursor in realtime,I will configure this later
                    {
                        var CurrentPosition = e.GetCurrentPoint(sender as CanvasAnimatedControl).Position;

                        var x = CurrentPosition.X;
                        var y = CurrentPosition.Y;

                       
                    }
                 }*/
                if (PointerPressedState)
                {
                    var CurrentPosition = e.GetCurrentPoint(sender as CanvasAnimatedControl).Position;

                    var x = CurrentPosition.X;
                    var y = CurrentPosition.Y;

                    //Touch case
                    if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                    {
                        var TouchPosition = PointerPoint.GetCurrentPoint(e.Pointer.PointerId).Position;
                        x = TouchPosition.X;
                        y = TouchPosition.Y;
                    }
                    //var CanvasWidth = ((CanvasAnimatedControl)sender).RenderSize.Width;
                    //var CanvasHeight = ((CanvasAnimatedControl)sender).RenderSize.Height;

                    //Resolve position in case of black area
                    /*var dsize = (Windows.Foundation.Size)VM.VideoService.GetDestinationSize();
                    var scaledAreaWidth = dsize.Width;
                    var scaledAreaHeight = dsize.Height;

                    var blackAreaWidth = CanvasWidth - (scaledAreaWidth);
                    var blackAreaHeight = CanvasHeight - scaledAreaHeight;
                    x = (x - (blackAreaWidth / 2));
                    y = (y - (blackAreaHeight / 2));*/

                    if (PreviousPoint != null)
                    {
                        double xDistance = x - PreviousPoint.X;
                        double yDistance = y - PreviousPoint.Y;
                        if (xDistance > 5)
                        {
                            xDistance = 5;
                        }
                        else if (xDistance < -5)
                        {
                            xDistance = -5;
                        }
                        if (yDistance > 5)
                        {
                            yDistance = 5;
                        }
                        else if (yDistance < -5)
                        {
                            yDistance = -5;
                        }

                        InputService.MousePointerPosition[0] = xDistance * 2;
                        InputService.MousePointerPosition[1] = yDistance * 2;
                    }
                    PreviousPoint = new Point(x, y);
                }
            }
            catch (Exception ex)
            {
                //PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void OnUnloading(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                PlatformService.videoService.RenderPanel = null;
                PlayerPanel.RemoveFromVisualTree();
                PlayerPanel.ResetElapsedTime();
                PlayerPanel.UpdateLayout();
                _audioProvider = null;
                _lineSpectrum = null;
            }
            catch (Exception ex)
            {
                //PlatformService.ShowErrorMessageDirect(ex);
            }
        }


        private void GamePlayerView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }
        private void GamePlayerView_Tapped(object sender, TappedRoutedEventArgs e)
        {
        }


        private void PanelPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            string debugMessage = "";

            PointerPressedState = true;
            try
            {
                InputService.PointerPressed = true;

                var CurrentPosition = e.GetCurrentPoint(sender as CanvasAnimatedControl).Position;
                Point TouchPosition;
                //var CanvasWidth = ((CanvasAnimatedControl)sender).RenderSize.Width;
                //var CanvasHeight = ((CanvasAnimatedControl)sender).RenderSize.Height;

                var x = CurrentPosition.X;
                var y = CurrentPosition.Y;

                var renderWidth = FramebufferConverter.currentWidth;
                var renderHeight = FramebufferConverter.currentHeight;


                //Touch case
                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                {
                    TouchPosition = PointerPoint.GetCurrentPoint(e.Pointer.PointerId).Position;
                    x = TouchPosition.X;
                    y = TouchPosition.Y;
                }

                //Resolve position in case of black area
                var dsize = (Windows.Foundation.Size)VM.VideoService.GetDestinationSize();
                var scaledAreaWidth = dsize.Width;
                var scaledAreaHeight = dsize.Height;

                /*var blackAreaWidth = CanvasWidth - (scaledAreaWidth);
                var blackAreaHeight = CanvasHeight - scaledAreaHeight;
                x = (x - (blackAreaWidth / 2));
                y = (y - (blackAreaHeight / 2));*/

                InputService.MousePointerViewportPosition[0] = x;
                InputService.MousePointerViewportPosition[1] = y;
                InputService.CanvasSize[0] = scaledAreaWidth;
                InputService.CanvasSize[1] = scaledAreaHeight;

                try
                {
                    var WindowPosition = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition;
                    if (TouchPosition != null && (TouchPosition.X > 0 || TouchPosition.Y > 0))
                    {
                        debugMessage = $"CX: {Math.Round(CurrentPosition.X)}, CY:  {Math.Round(CurrentPosition.Y)}\nWX: {Math.Round(WindowPosition.X)}, WY: {Math.Round(WindowPosition.Y)}\nTX:  {Math.Round(TouchPosition.X)}, TY:  {Math.Round(TouchPosition.Y)}\nX:  {Math.Round(x)}, Y:  {Math.Round(y)}";
                    }
                    else
                    {
                        debugMessage = $"CX: {Math.Round(CurrentPosition.X)}, CY:  {Math.Round(CurrentPosition.Y)}\nWX: {Math.Round(WindowPosition.X)}, WY: {Math.Round(WindowPosition.Y)}\nX:  {Math.Round(x)}, Y:  {Math.Round(y)}, W:  {scaledAreaWidth}, H:  {scaledAreaHeight}";
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {
                debugMessage = ex.Message;
            }
            try
            {
                if (VM.ShowSensorsInfo)
                {

                    try
                    {
                        DebugText.Text = debugMessage;
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private new void PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                PointerPressedState = false;
                RightTapped = false;
                InputService.MousePointerPosition[0] = 0;
                InputService.MousePointerPosition[1] = 0;
            }
            catch (Exception ex)
            {

            }
        }


        private void ResetButtonHandler(object sender, EventArgs e)
        {
            try
            {
                VM.ResetActionsSet();
            }
            catch (Exception ex)
            {

            }
        }
        private void CancelButtonHandler(object sender, EventArgs e)
        {
            try
            {
                VM.CancelActionsSet();
            }
            catch (Exception ex)
            {

            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.CancelActionsSet();
            }
            catch (Exception ex)
            {

            }
        }

        private void SaveButtonHandler(object sender, EventArgs e)
        {
            try
            {
                VM.SaveActionsSet();
            }
            catch (Exception ex)
            {

            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SaveActionsSet();
            }
            catch (Exception ex)
            {

            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ResetActionsSet();
            }
            catch (Exception ex)
            {

            }
        }

        private void PlayerPanel_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {

        }

        private void InitialCoresOptionsMenu(object sender, EventArgs e)
        {
            if (systemCoreReady)
            {
                return;
            }
            try
            {
                _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                  {
                      try
                      {
                          OptionsLoadingProgress.Visibility = Visibility.Visible;
                          OptionsLoadingProgress.IsActive = true;
                          await Task.Delay(500);
                          await SystemCoresOptions();
                          OptionsLoadingProgress.Visibility = Visibility.Collapsed;
                          OptionsLoadingProgress.IsActive = false;
                      }
                      catch (Exception ec)
                      {
                          PlatformService.ShowErrorMessageDirect(ec);
                          OptionsLoadingProgress.Visibility = Visibility.Collapsed;
                          OptionsLoadingProgress.IsActive = false;
                      }
                  });

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }


        private async void OptionsSave_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.SystemName != null)
                {
                    var TargetSystem = VM.SystemName;
                    var TargetCore = VM.CoreName;
                    var IsNewCore = VM.EmulationService.IsNewCore();
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmSave = new ConfirmConfig();
                        confirmSave.SetTitle("Save Options");
                        confirmSave.SetMessage($"Do you want to save {VM.SystemNamePreview}'s options as default values?\n\nNote: You can save these changes for this game only from sub menu 'Game Only'");
                        confirmSave.UseYesNo();

                        var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                        if (StartSave)
                        {
                            await VM.CoreOptionsStoreAsync(TargetCore, TargetSystem, IsNewCore);
                            PlatformService.PlayNotificationSoundDirect("success");
                            //await UserDialogs.Instance.AlertAsync($"{VM.SystemNamePreview}'s options has been saved", "Save Done");
                            PlatformService.ShowNotificationDirect($"{VM.SystemNamePreview}'s options has been saved");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void OptionsCancel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                VM.SetCoreOptionsVisible.Execute(null);
            }
            catch (Exception ex)
            {

            }
        }

        bool systemCoreReady = false;
        bool optionsGenerated = false;
        private void CoreOptionsPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var targetItem = (CoreOptionListItem)e.ClickedItem;
                if (targetItem.isEnabled)
                {
                    targetItem.DroppedDown = !targetItem.DroppedDown;
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void ControlsPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var targetItem = (ButtonListItem)e.ClickedItem;
                targetItem.DroppedDown = !targetItem.DroppedDown;
            }
            catch (Exception ex)
            {

            }
        }
        bool isOptionsGenerateInProgress = false;
        private async Task SystemCoresOptions(bool forceReload = false)
        {
            try
            {
                if (optionsGenerated && !forceReload)
                {
                    return;
                }
                isOptionsGenerateInProgress = true;
                coreOptionsGroupped.Clear();
                var TargetSystem = VM.SystemName;
                var expectedName = $"{VM.CoreName}_{TargetSystem}";

                CoresOptions testOptions;
                if (!GameSystemSelectionViewModel.SystemsOptions.TryGetValue(expectedName, out testOptions))
                {
                    if (!VM.EmulationService.IsNewCore() && GameSystemSelectionViewModel.SystemsOptions.TryGetValue(TargetSystem, out testOptions))
                    {
                        expectedName = TargetSystem;
                    }
                    else
                    {
                        return;
                    }
                }

                var CurrentOptions = GameSystemSelectionViewModel.SystemsOptions[expectedName].OptionsList.Keys;
                PlatformService.PlayNotificationSoundDirect("select");
                if (CurrentOptions.Count > 0)
                {
                    NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    var optionsCount = 0;
                    Dictionary<string, GroupCoreOptionListItems> groups = new Dictionary<string, GroupCoreOptionListItems>();
                    foreach (var CoreItem in CurrentOptions)
                    {
                        TextBlock SystemOptionText = new TextBlock();
                        ComboBox comboBox = new ComboBox();
                        var OptionValues = GameSystemSelectionViewModel.SystemsOptions[expectedName].OptionsList[CoreItem];
                        var optionName = OptionValues.OptionsDescription;
                        List<string> optionValues = new List<string>();
                        if (OptionValues != null && OptionValues.OptionsValues.Count > 0)
                        {
                            foreach (var OptionValue in OptionValues.OptionsValues)
                            {
                                optionValues.Add(OptionValue);
                            }
                            optionsCount++;
                        }
                        else
                        {
                            continue;
                        }

                        var selectedIndex = (int)OptionValues.SelectedIndex;

                        var SystemName = TargetSystem;
                        var optionKey = $"{OptionValues.OptionsKey}|{selectedIndex}";

                        CoreOptionListItem coreOptionListItem = new CoreOptionListItem(optionKey, SystemName, optionName, optionValues, selectedIndex);
                        GroupCoreOptionListItems testList;
                        if (!groups.TryGetValue(coreOptionListItem.Group, out testList))
                        {
                            testList = new GroupCoreOptionListItems(coreOptionListItem.Group);
                            groups.Add(coreOptionListItem.Group, testList);
                        }
                        testList.Add(coreOptionListItem);
                    }
                    foreach (var gItem in groups)
                    {
                        coreOptionsGroupped.Add(gItem.Value);
                    }
                    optionsGenerated = true;
                    isOptionsGenerateInProgress = false;
                    systemCoreReady = true;
                    restoreOptionsListPosition();
                }
                else
                {
                    NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    PlatformService.PlayNotificationSoundDirect("notice");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private void ComboBox_DropDownClosed(object sender, object e)
        {
            try
            {
                if (isOptionsGenerateInProgress || ((ComboBox)sender).Tag == null)
                {
                    return;
                }

                var TargetIndex = ((ComboBox)sender).SelectedIndex;
                if (TargetIndex == -1)
                {
                    return;
                }
                var TargetData = ((ComboBox)sender).Tag.ToString().Split('|');
                var TargetIndexTemp = TargetData[1];
                if (TargetIndexTemp.Equals(TargetIndex.ToString()))
                {
                    return;
                }
                var TargetKey = TargetData[0];
                ((ComboBox)sender).Tag = $"{TargetKey}|{TargetIndex}";
                var TargetSystem = VM.SystemName;
                var TargetCore = VM.CoreName;
                var IsNewCore = VM.EmulationService.IsNewCore();

                GameSystemSelectionViewModel.setCoresOptionsDictionaryDirect(TargetCore, TargetSystem, IsNewCore, TargetKey, (uint)TargetIndex);
                PlatformService.PlayNotificationSoundDirect("option-changed");
                VM.updateCoreOptions(TargetKey);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private void SelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            //Code here is causing some issues because it's getting triggered while list scrolling
            //for now will be disabled and will be handled in ComboBox_DropDownClosed
            /*try
            {
                if (isOptionsGenerateInProgress || ((ComboBox)sender).Tag == null)
                {
                    return;
                }
                
                var TargetIndex = ((ComboBox)sender).SelectedIndex;
                if(TargetIndex == -1)
                {
                    return;
                }
                var TargetData = ((ComboBox)sender).Tag.ToString().Split('|');
                var TargetIndexTemp = TargetData[1];
                if (TargetIndexTemp.Equals(TargetIndex.ToString()))
                {
                    return;
                }
                var TargetKey = TargetData[0];
                ((ComboBox)sender).Tag = $"{TargetKey}|{TargetIndex}";
                var TargetSystem = VM.SystemName;
                var TargetCore = VM.CoreName;
                var IsNewCore = VM.EmulationService.IsNewCore();

                GameSystemSelectionViewModel.setCoresOptionsDictionaryDirect(TargetCore, TargetSystem, IsNewCore, TargetKey, (uint)TargetIndex);
                PlatformService.PlayNotificationSoundDirect("option-changed");
                VM.updateCoreOptions(TargetKey);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }*/
        }

        private void PlayerPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                VM.PointerTabbedCommand.Execute(null);
                VM.TappedCommand2.Execute(null);
            }
            catch (Exception ex)
            {

            }
        }

        private void PlayerPanel_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {

        }

        private async void OptionsInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("notice");
                await UserDialogs.Instance.AlertAsync("If you found 'Restart' word behind the option, that's mean you have to close the game (not the app) and reopen it to apply the changes");
            }
            catch (Exception ex)
            {

            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ActionsHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("notice");
                await UserDialogs.Instance.AlertAsync($"Press on the buttons to record actions.\n\nActions speed can be set from\nAdvanced -> Actions -> Speed\n\n(Action & Action) \u271A:\nThis options used when the movements required to press two buttons at once\nYou have to activate the 'checkbox' for each button\n\n Visit \u2754 help page for more");
            }
            catch (Exception ex)
            {

            }
        }


        public void ScreenshotHandler(object sender, EventArgs args)
        {
            TakeScreenShot();
        }
        private async void TakeScreenShot()
        {
            try
            {
                if (VM != null && VM.GameStopStarted)
                {
                    PlatformService.PlayNotificationSoundDirect("error");
                    PlatformService.ShowNotificationDirect($"Game failed, you cannot task screenshot!");
                    return;
                }
                var picturesFolder = KnownFolders.PicturesLibrary;
                var retrixFolder = await picturesFolder.CreateFolderAsync("RetriXGold", CreationCollisionOption.OpenIfExists);
                if (retrixFolder != null)
                {
                    var targetFolder = $"{VM.SystemNamePreview} ({VM.CoreName})";
                    var saveFolder = await retrixFolder.CreateFolderAsync(targetFolder, CreationCollisionOption.OpenIfExists);
                    if (saveFolder != null)
                    {
                        var interpolation = CanvasImageInterpolation.NearestNeighbor;
                        VM.TogglePauseCommand.Execute(null);
                        VM.GameIsLoadingState(true);
                        bool newMethodState = false;
                        bool screenshotSaved = false;
                        var device = new CanvasDevice();
                        var RenderResolution = VM.RenderResolution();
                        var width = RenderResolution[0];
                        var height = RenderResolution[1];
                        var RenderTarget = (CanvasBitmap)VM.VideoService.GetRenderTarget();

                        try
                        {
                            ICanvasImage outputResult;
                            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                            {
                                await RenderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Bmp);
                                outputResult = await CanvasBitmap.LoadAsync(device, stream);
                            }
                            var renderer = new CanvasRenderTarget(device,
                                                                              width,
                                                                              height, RenderTarget.Dpi);
                            using (var drawingSession = renderer.CreateDrawingSession())
                            {
                                outputResult = (ICanvasImage)VM.VideoService.UpdateEffects(drawingSession, outputResult, width, height, interpolation);
                                drawingSession.DrawImage(outputResult);
                            }
                            var TargetFile = $"{DateTime.Now.ToString()}.bmp";
                            var fileTemp = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(TargetFile, CreationCollisionOption.GenerateUniqueName);
                            await renderer.SaveAsync(fileTemp.Path);

                            try
                            {
                                await Task.Delay(500);
                                var testFile = (StorageFile)await ApplicationData.Current.TemporaryFolder.TryGetItemAsync(fileTemp.Name);
                                if (testFile != null)
                                {
                                    Stream imagestream = await testFile.OpenStreamForReadAsync();
                                    BitmapDecoder dec = await BitmapDecoder.CreateAsync(imagestream.AsRandomAccessStream());
                                    var pixels = await dec.GetPixelDataAsync();
                                    var pixelsBytes = pixels.DetachPixelData();

                                    //Blank images will be ignored
                                    int pixelsSum = BitConverter.ToInt32(pixelsBytes, 0);
                                    if (pixelsSum == 0)
                                    {
                                        await testFile.DeleteAsync();
                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (isUpscaleActive)
                                            {
                                                await UpscaleScreenshot(fileTemp, upscaleTasksCancel);
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        await fileTemp.MoveAsync(saveFolder);
                                        newMethodState = true;
                                        screenshotSaved = true;
                                    }
                                }

                            }
                            catch (Exception eb)
                            {

                            }
                        }
                        catch (Exception e)
                        {

                        }
                        if (!newMethodState)
                        {
                            RenderTargetBitmap _bitmap = new RenderTargetBitmap();
                            await _bitmap.RenderAsync(PlayerPanel);
                            bool bufferOK = false;
                            byte[] pixels = null;
                            try
                            {
                                var pixelBuffer = await _bitmap.GetPixelsAsync();
                                pixels = pixelBuffer.ToArray();
                                if (pixelBuffer.Length == 0)
                                {
                                    return;
                                }
                                //Blank images will be ignored
                                int pixelsSum = BitConverter.ToInt32(pixels, 0);
                                if (pixelsSum != 0)
                                {
                                    bufferOK = true;
                                }
                            }
                            catch (Exception es)
                            {

                            }
                            if (bufferOK)
                            {
                                var displayInformation = DisplayInformation.GetForCurrentView();

                                var TargetFile = $"{VM.SystemNamePreview}_{DateTime.Now.Ticks}.bmp";
                                var fileSave = await saveFolder.CreateFileAsync(TargetFile, CreationCollisionOption.GenerateUniqueName);
                                using (var stream = await fileSave.OpenAsync(FileAccessMode.ReadWrite))
                                {
                                    var encoder = await
                                 BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
                                    byte[] bytes = pixels.ToArray();
                                    BitmapAlphaMode mode = BitmapAlphaMode.Straight;
                                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                                            mode,
                                                            (uint)_bitmap.PixelWidth,
                                                            (uint)_bitmap.PixelHeight,
                                                            96,
                                                            96,
                                                            bytes);

                                    await encoder.FlushAsync();
                                    //Check if blank image
                                    try
                                    {
                                        await Task.Delay(500);
                                        var testFile = (StorageFile)await saveFolder.TryGetItemAsync(fileSave.Name);
                                        if (testFile != null)
                                        {
                                            var mds = await testFile.GetBasicPropertiesAsync();
                                            ulong testFileSize = mds.Size;
                                            if (testFileSize < 100)
                                            {
                                                await testFile.DeleteAsync();
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    if (isUpscaleActive)
                                                    {
                                                        await UpscaleScreenshot(testFile, upscaleTasksCancel);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {

                                                }
                                                screenshotSaved = true;
                                            }
                                        }
                                    }
                                    catch (Exception eb)
                                    {

                                    }
                                }
                            }
                            #region Test
                            /*
                            var renderer = new CanvasRenderTarget(device,
                                                                              width,
                                                                              height, RenderTarget.Dpi);
                            ICanvasImage outputResult;
                            var tempFile = $"temp_{DateTime.Now.Ticks}.png";
                            var savefile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(tempFile, CreationCollisionOption.ReplaceExisting);
                            var pixels = await _bitmap.GetPixelsAsync();
                            using (IRandomAccessStream stream = await savefile.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                var encoder = await
                                BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                                byte[] bytes = pixels.ToArray();
                                BitmapAlphaMode mode = BitmapAlphaMode.Straight;
                                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                                        mode,
                                                        (uint)_bitmap.PixelWidth,
                                                        (uint)_bitmap.PixelHeight,
                                                        96,
                                                        96,
                                                        bytes);

                                await encoder.FlushAsync();
                            }

                            outputResult = await CanvasBitmap.LoadAsync(device, savefile.Path);
                            using (var drawingSession = renderer.CreateDrawingSession())
                            {
                                var transform = (Matrix3x2)VM.VideoService.GetTransformMattrix();
                                var aspect = VM.VideoService.RenderTargetAspectRatio();
                                var dsize = (Windows.Foundation.Size)VM.VideoService.GetDestinationSize();

                                outputResult = (ICanvasImage)VM.VideoService.UpdateEffects(drawingSession, outputResult, width, height, interpolation);

                                ScaleEffect scaleEffect = new ScaleEffect();
                                scaleEffect.Source = outputResult;
                                float scaleFactorWidth = (width * 1.0f) / ((float)_bitmap.PixelWidth * 1.0f);
                                float scaleFactorHeight = (height * 1.0f) / ((float)_bitmap.PixelHeight * 1.0f);
                                scaleEffect.Scale = new Vector2((float)scaleFactorWidth, (float)scaleFactorHeight);
                                outputResult = scaleEffect;

                                drawingSession.DrawImage(outputResult);
                            }
                            var TargetFile = $"{VM.SystemNamePreview}_{DateTime.Now.Ticks}.png";
                            var fileTemp = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(TargetFile, CreationCollisionOption.GenerateUniqueName);
                            await renderer.SaveAsync(fileTemp.Path);
                            await fileTemp.CopyAsync(saveFolder);*/
                            #endregion
                        }
                        if (screenshotSaved)
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            PlatformService.ShowNotificationDirect($"Screenshot saved to Pictures");
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("error");
                            PlatformService.ShowNotificationDirect($"Failed to create screenshot!");
                        }
                        VM.TogglePauseCommand.Execute(null);
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        PlatformService.ShowNotificationDirect($"Unable to create folder ({VM.SystemNamePreview}) at Pictures");
                    }
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("error");
                    PlatformService.ShowNotificationDirect($"Unable to create folder at Pictures");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
            VM.GameIsLoadingState(false);
        }
        private async void SaveSnapshot(object sender, EventArgs eventArgs)
        {
            try
            {
                try
                {
                    if(PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
                }
                catch (Exception ea)
                {
                }
                string GameID = ((GameIDArgs)eventArgs).GameID;
                if (GameID == null)
                {
                    VM.SnapshotInProgress = false;
                    return;
                }
                var SaveLocation = ((GameIDArgs)eventArgs).SaveLocation;
                if (SaveLocation == null)
                {
                    return;
                }
                if (VM != null && GameID.Length > 0)
                {
                    VM.SnapshotInProgress = true;
                    var newSaveMethod = false;
                    try
                    {
                        var device = new CanvasDevice();
                        var RenderResolution = VM.RenderResolution();
                        var width = RenderResolution[0];
                        var height = RenderResolution[1];
                        var RenderTarget = (CanvasBitmap)VM.VideoService.GetRenderTarget();
                        ICanvasImage outputResult;
                        using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                        {
                            await RenderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Bmp);
                            outputResult = await CanvasBitmap.LoadAsync(device, stream);
                        }
                        var interpolation = CanvasImageInterpolation.NearestNeighbor;
                        var renderer = new CanvasRenderTarget(device,
                                                                          width,
                                                                          height, RenderTarget.Dpi);
                        using (var drawingSession = renderer.CreateDrawingSession())
                        {
                            outputResult = (ICanvasImage)VM.VideoService.UpdateEffects(drawingSession, outputResult, width, height, interpolation);
                            drawingSession.DrawImage(outputResult);
                        }
                        var TargetFile = $"{GameID}.bmp";
                        var saveFolder = await PlatformService.saveStateService.SaveStatesFolder.CreateFolderAsync(VM.EmulationService.GetGameID().ToLower(), CreationCollisionOption.OpenIfExists);
                        var file = await saveFolder.CreateFileAsync(TargetFile, CreationCollisionOption.ReplaceExisting);
                        await renderer.SaveAsync(file.Path);
                        //Check if blank image
                        try
                        {
                            await Task.Delay(500);
                            var testFile = (StorageFile)await SaveLocation.TryGetItemAsync(TargetFile);
                            if (testFile != null)
                            {
                                Stream imagestream = await testFile.OpenStreamForReadAsync();
                                BitmapDecoder dec = await BitmapDecoder.CreateAsync(imagestream.AsRandomAccessStream());
                                var pixels = await dec.GetPixelDataAsync();
                                var pixelsBytes = pixels.DetachPixelData();

                                //Blank images will be ignored
                                int pixelsSum = BitConverter.ToInt32(pixelsBytes, 0);
                                if (pixelsSum != 0)
                                {
                                    newSaveMethod = true;
                                    try
                                    {
                                        if (PlatformService.PreventGCAlways)
                                        {
                                            GC.SuppressFinalize(pixelsBytes);
                                            pixelsBytes = null;
                                            GC.SuppressFinalize(testFile);
                                            testFile = null;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                else
                                {
                                    await testFile.DeleteAsync();
                                }
                            }

                        }
                        catch (Exception eb)
                        {

                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    //Below will be used only if the new method failed
                    #region OLD Saver
                    if (!newSaveMethod)
                    {
                        try
                        {
                            RenderTargetBitmap rtb = new RenderTargetBitmap();
                            await rtb.RenderAsync(PlayerPanel);
                            bool bufferOK = false;
                            byte[] pixels = null;
                            uint bufferSize;
                            try
                            {
                                var pixelBuffer = await rtb.GetPixelsAsync();
                                pixels = pixelBuffer.ToArray();
                                if (pixelBuffer.Length == 0)
                                {
                                    return;
                                }
                                //Blank images will be ignored
                                int pixelsSum = BitConverter.ToInt32(pixels, 0);
                                if (pixelsSum != 0)
                                {
                                    bufferOK = true;
                                }
                            }
                            catch (Exception es)
                            {

                            }
                            if (bufferOK)
                            {
                                var displayInformation = DisplayInformation.GetForCurrentView();

                                var TargetFile = $"{GameID}.bmp";
                                var TestFile = (StorageFile)await SaveLocation.TryGetItemAsync(TargetFile);
                                if (TestFile != null)
                                {
                                    await TestFile.DeleteAsync();
                                }
                                var saveFolder = await PlatformService.saveStateService.SaveStatesFolder.CreateFolderAsync(VM.EmulationService.GetGameID().ToLower(), CreationCollisionOption.OpenIfExists);
                                var file = await saveFolder.CreateFileAsync(TargetFile, CreationCollisionOption.ReplaceExisting);
                                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                                {
                                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
                                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                                         BitmapAlphaMode.Premultiplied,
                                                         (uint)rtb.PixelWidth,
                                                         (uint)rtb.PixelHeight,
                                                         displayInformation.RawDpiX,
                                                         displayInformation.RawDpiY,
                                                         pixels);
                                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;

                                    // Get the image's original width and height
                                    int originalWidth = rtb.PixelWidth;
                                    int originalHeight = rtb.PixelHeight;

                                    // To preserve the aspect ratio
                                    int maxSize = 400;
                                    try
                                    {
                                        maxSize = new Random().Next(390, 510);
                                    }
                                    catch (Exception e)
                                    {

                                    }
                                    float ratioX = (float)maxSize / (float)originalWidth;
                                    float ratioY = (float)maxSize / (float)originalHeight;
                                    float ratio = Math.Min(ratioX, ratioY);

                                    float sourceRatio = (float)originalWidth / originalHeight;

                                    // New width and height based on aspect ratio
                                    int newWidth = (int)(originalWidth * ratio);
                                    int newHeight = (int)(originalHeight * ratio);

                                    encoder.BitmapTransform.ScaledWidth = (uint)newWidth;
                                    encoder.BitmapTransform.ScaledHeight = (uint)newHeight;
                                    await encoder.FlushAsync();

                                    try
                                    {
                                        if (PlatformService.PreventGCAlways)
                                        {
                                            GC.SuppressFinalize(pixels);
                                            pixels = null;
                                            GC.SuppressFinalize(file);
                                            file = null;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }

                                //Check if blank image
                                try
                                {
                                    await Task.Delay(500);
                                    var testFile = (StorageFile)await SaveLocation.TryGetItemAsync(TargetFile);
                                    if (testFile != null)
                                    {
                                        ulong testFileSize = (await testFile.GetBasicPropertiesAsync()).Size;
                                        if (testFileSize < 100)
                                        {
                                            await testFile.DeleteAsync();
                                        }
                                    }
                                }
                                catch (Exception eb)
                                {

                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    #endregion
                }
                VM.SnapshotInProgress = false;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
                VM.SnapshotInProgress = false;
            }
            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }

        }

        private void SavesInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SavesCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.HideSavesList();
            }
            catch (Exception ex)
            {

            }
        }

        private void SavesQuick_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ShowQuickSaves();
            }
            catch (Exception ex)
            {

            }
        }

        private void SavesAuto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ShowAutoSaves();
            }
            catch (Exception ex)
            {

            }
        }

        private void SavesSlots_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ShowSlotsSaves();
            }
            catch (Exception ex)
            {

            }
        }

        private void SavesAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ShowAllSaves();
            }
            catch (Exception ex)
            {

            }
        }

        private void RightControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                if (VM.GameStopStarted || !VM.ScaleFactorVisible) return;
                var PositionX = e.Delta.Translation.X / VM.RightScaleFactorValue;
                var PositionY = e.Delta.Translation.Y / VM.RightScaleFactorValue;
                VM.RightTransformXCurrent += PositionX;
                VM.RightTransformYCurrent += PositionY;
            }
            catch (Exception ex)
            {

            }
        }

        private void LeftControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                if (VM.GameStopStarted || !VM.ScaleFactorVisible) return;
                var PositionX = e.Delta.Translation.X;
                var PositionY = e.Delta.Translation.Y;
                VM.LeftTransformXCurrent += PositionX / VM.LeftScaleFactorValue;
                VM.LeftTransformYCurrent += PositionY / VM.LeftScaleFactorValue;
            }
            catch (Exception ex)
            {

            }
        }
        private void ActionsControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                if (VM.GameStopStarted || !VM.ScaleFactorVisible) return;
                var PositionX = e.Delta.Translation.X;
                var PositionY = e.Delta.Translation.Y;
                VM.ActionsTransformXCurrent += PositionX / VM.LeftScaleFactorValue;
                VM.ActionsTransformYCurrent += PositionY / VM.LeftScaleFactorValue;
            }
            catch (Exception ex)
            {

            }
        }

        private void DisableEditMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SetScaleFactorVisible.Execute(null);
            }
            catch (Exception ex)
            {

            }
        }

        private void CloseLogListHandler(object sender, EventArgs e)
        {
            try
            {
                VM.SetShowLogsList.Execute(null);
            }
            catch (Exception ex)
            {

            }
        }

        private void CloseLogList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SetShowLogsList.Execute(null);
            }
            catch (Exception ex)
            {

            }
        }

        private async void SaveCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.SystemNamePreview != null)
                {
                    var TargetSystem = VM.SystemNamePreview;
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmSave = new ConfirmConfig();
                        confirmSave.SetTitle("Save Customizations");
                        confirmSave.SetMessage($"Do you want to save these customizations for {TargetSystem}? ");
                        confirmSave.UseYesNo();

                        var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                        if (StartSave)
                        {
                            await VM.CustomTouchPadStoreAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void DeleteCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.SystemNamePreview != null)
                {
                    var TargetSystem = VM.SystemNamePreview;
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmDelete = new ConfirmConfig();
                        confirmDelete.SetTitle("Delete Customizations");
                        confirmDelete.SetMessage($"Do you want to delete current customizations for {TargetSystem}? ");
                        confirmDelete.UseYesNo();

                        var StartDelete = await UserDialogs.Instance.ConfirmAsync(confirmDelete);

                        if (StartDelete)
                        {
                            await VM.CustomTouchPadDeleteAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void ResetCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ResetAdjustmentsCall();
            }
            catch (Exception ex)
            {

            }
        }

        bool systemControlsReady = false;
        public static bool systemControlsChanged = false;
        private void InitialControlsMenu(object sender, EventArgs e)
        {
            if (systemControlsReady && !systemControlsChanged)
            {
                return;
            }
            try
            {
                _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {

                        ControlsLoadingProgress.Visibility = Visibility.Visible;
                        ControlsLoadingProgressBar.IsActive = true;
                        await Task.Delay(500);
                        await SystemControls();
                        ControlsLoadingProgress.Visibility = Visibility.Collapsed;
                        ControlsLoadingProgressBar.IsActive = false;
                    }
                    catch (Exception ec)
                    {
                        PlatformService.ShowErrorMessageDirect(ec);
                        ControlsLoadingProgress.Visibility = Visibility.Collapsed;
                        ControlsLoadingProgressBar.IsActive = false;
                    }
                });

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        public void RestoreControlsListPosition(object sender, EventArgs args)
        {
            try
            {
                ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(ControlsPage).FirstOrDefault();
                if (svl != null)
                {
                    svl.ChangeView(0, PlatformService.vXboxScrollPosition, 1);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateControlsListPosition(object sender, EventArgs args)
        {
            try
            {
                ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(ControlsPage).FirstOrDefault();
                if (svl != null)
                {
                    PlatformService.vXboxScrollPosition = svl.VerticalOffset;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public int GetSelectedPort()
        {
            var port = 0;

            try
            {
                var targetPort = (string)((ComboBoxItem)GamePadPorts.SelectedItem).Tag;
                port = int.Parse(targetPort);
            }
            catch (Exception ex)
            {

            }
            return port;
        }

        List<ComboBoxItem> PortsRefs;
        private void updatePortsColorState()
        {
            if (PortsRefs == null)
            {
                PortsRefs = new List<ComboBoxItem>()
        {
            Port1,
            Port2,
            Port3,
            Port4,
            Port5,
            Port6,
            Port7,
            Port8,
            Port9,
            Port10,
            Port11,
            Port12,
        };
            }
            try
            {
                for (int i = 0; i < 12; i++)
                {
                    if (InputService.Descriptors[i].Count == 0)
                    {
                        PortsRefs[i].Foreground = new SolidColorBrush(Colors.Gray);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task SystemControls()
        {
            try
            {
                updatePortsColorState();
                buttonsListItemsGroup.Clear();
                var TargetSystem = VM.SystemName;

                PlatformService.PlayNotificationSoundDirect("select");
                var targetPort = GetSelectedPort();
                if (InputService.CurrentButtonsMaps[targetPort].Count > 0)
                {
                    Dictionary<string, GroupButtonListItems> groups = new Dictionary<string, GroupButtonListItems>();
                    foreach (var ButtonItem in InputService.CurrentButtonsMaps[targetPort])
                    {
                        var Title = ButtonItem.Value;
                        List<string> optionValues = new List<string>();
                        foreach (var OptionValue in InputService.GamepadMap.Keys)
                        {
                            optionValues.Add(OptionValue);
                        }
                        var SelectedIndex = InputService.GetGamePadSelectedIndex(targetPort, ButtonItem.Key);
                        var Tag = ButtonItem.Key;
                        var Name = ButtonItem.Value;

                        ButtonListItem buttonListItem = new ButtonListItem(Tag, Name, Name, optionValues, SelectedIndex);
                        GroupButtonListItems testList;
                        if (!groups.TryGetValue(buttonListItem.Group, out testList))
                        {
                            testList = new GroupButtonListItems(buttonListItem.Group);
                            groups.Add(buttonListItem.Group, testList);
                        }
                        testList.Add(buttonListItem);
                    }

                    foreach (var gItem in groups)
                    {
                        buttonsListItemsGroup.Add(gItem.Value);
                    }
                    systemControlsReady = true;
                    systemControlsChanged = false;
                    RestoreControlsListPosition(null, null);
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("notice");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void Controls_DropDownClosed(object sender, object e)
        {
            try
            {
                var targetPort = GetSelectedPort();
                var TargetSystem = ((ComboBox)sender).Tag;
                var TargetIndex = ((ComboBox)sender).SelectedIndex;
                var TargetKey = ((ComboBox)sender).Name;
                PlatformService.PlayNotificationSoundDirect("option-changed");
                InputService.ChangeGamepadButton(targetPort, (InjectedInputTypes)TargetSystem, TargetIndex);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private void ControlsSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            //Core here is causing some issue because it's getting triggered while list scrolling
            //for now will be disabled and will be handled in Controls_DropDownClosed
            /*try
            {
                var TargetSystem = ((ComboBox)sender).Tag;
                var TargetIndex = ((ComboBox)sender).SelectedIndex;
                var TargetKey = ((ComboBox)sender).Name;
                PlatformService.PlayNotificationSoundDirect("option-changed");
                InputService.ChangeGamepadButton((InjectedInputTypes)TargetSystem, TargetIndex);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }*/
        }

        private async void ControlsCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SetControlsMapVisible.Execute(null);
                if (VM.EmulationService.CorePaused)
                {
                    await VM.EmulationService.ResumeGameAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void ControlsReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.SystemNamePreview != null)
                {
                    var TargetSystem = VM.SystemNamePreview;
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmDelete = new ConfirmConfig();
                        confirmDelete.SetTitle("Reset Controls");
                        confirmDelete.SetMessage($"Do you want to reset controls for {TargetSystem}? ");
                        confirmDelete.UseYesNo();

                        var StartDelete = await UserDialogs.Instance.ConfirmAsync(confirmDelete);

                        if (StartDelete)
                        {
                            await CustomGamePadDeleteAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void ControlsSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.SystemNamePreview != null)
                {
                    var TargetSystem = VM.SystemNamePreview;
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmSave = new ConfirmConfig();
                        confirmSave.SetTitle("Save Gamepad");
                        confirmSave.SetMessage($"Do you want to save controls for {TargetSystem}? ");
                        confirmSave.UseYesNo();

                        var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                        if (StartSave)
                        {
                            await CustomGamePadStoreAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        string GamepadSaveLocation = "GamepadMap";
        public async Task CustomGamePadStoreAsync(string GameName = "")
        {
            try
            {
                ControlsLoadingProgress.Visibility = Visibility.Visible;
                ControlsLoadingProgressBar.IsActive = true;
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(GamepadSaveLocation);
                if (localFolder == null)
                {
                    localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(GamepadSaveLocation);
                }
                var expectedName = $"{VM.EmulationService.currentCore.Name}_{VM.SystemName}.rgm";
                if (GameName.Length > 0)
                {
                    expectedName = $"{VM.EmulationService.currentCore.Name}_{VM.SystemName}_{GameName}.rgm";
                }
                var targetFile = (StorageFile)await localFolder.CreateFileAsync(expectedName, CreationCollisionOption.ReplaceExisting);

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(InputService.GamepadMapWithInputs));
                using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var outStream = stream.AsStream();
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }
                PlatformService.PlayNotificationSoundDirect("success");
                if (GameName.Length > 0)
                {
                    PlatformService.ShowNotificationDirect($"Controls saved for {GameName}");
                }
                else
                {
                    PlatformService.ShowNotificationDirect($"Controls saved for {VM.SystemNamePreview}");
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }
            ControlsLoadingProgress.Visibility = Visibility.Collapsed;
            ControlsLoadingProgressBar.IsActive = false;
        }

        public async Task<bool> CustomGamePadRetrieveGameAsync()
        {
            try
            {
                while (VM == null || VM.SystemName == null)
                {
                    await Task.Delay(650);
                }
                if (VM.MainFilePath != null && VM.MainFilePath.Length > 0)
                {
                    var GameName = Path.GetFileNameWithoutExtension(VM.MainFilePath);
                    return await CustomGamePadRetrieveAsync(GameName);
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        public async Task<bool> CustomGamePadRetrieveAsync(string GameName = "")
        {
            try
            {
                while (VM == null || VM.SystemName == null)
                {
                    await Task.Delay(650);
                }
                for (int i = 0; i < 12; i++)
                {
                    InputService.ResetGamepadButtons(i);
                }
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(GamepadSaveLocation);
                if (localFolder == null)
                {
                    return false;
                }

                var expectedName = $"{VM.EmulationService.currentCore.Name}_{VM.SystemName}.rgm";
                if (GameName.Length > 0)
                {
                    expectedName = $"{VM.EmulationService.currentCore.Name}_{VM.SystemName}_{GameName}.rgm";
                }
                var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync(expectedName);
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
                    var dictionaryList = JsonConvert.DeserializeObject<List<Dictionary<int, int>>>(CoreFileContent);

                    if (dictionaryList != null)
                    {
                        InputService.GamepadMapWithInputs = dictionaryList;
                        InputService.ReSyncGamepadButtons();
                    }
                    return true;
                }
            }
            catch (Exception e)
            {

            }
            return false;
        }

        public async Task CustomGamePadDeleteAsync(string GameName = "")
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(GamepadSaveLocation);
                if (localFolder != null)
                {
                    var expectedName = $"{VM.EmulationService.currentCore.Name}_{VM.SystemName}.rgm";
                    if (GameName.Length > 0)
                    {
                        expectedName = $"{VM.EmulationService.currentCore.Name}_{VM.SystemName}_{GameName}.rgm";
                    }
                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync(expectedName);
                    if (targetFileTest != null)
                    {
                        ControlsLoadingProgress.Visibility = Visibility.Visible;
                        ControlsLoadingProgressBar.IsActive = true;
                        await targetFileTest.DeleteAsync();
                    }
                }

                UpdateControlsListPosition(null, null);
                var extraNotice = "";
                if (GameName.Length > 0)
                {
                    var test = await CustomGamePadRetrieveAsync();
                    if (!test)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            InputService.ResetGamepadButtons(i);
                        }
                    }
                    else
                    {
                        extraNotice = $"\nSystem preset will be used";
                    }
                }
                else
                {
                    var test = await CustomGamePadRetrieveGameAsync();
                    if (!test)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            InputService.ResetGamepadButtons(i);
                        }
                    }
                    else
                    {
                        extraNotice = $"\nGame custom preset will be used";
                    }
                }
                systemControlsReady = false;
                InitialControlsMenu(null, EventArgs.Empty);
                PlatformService.PlayNotificationSoundDirect("success");
                if (GameName.Length > 0)
                {
                    PlatformService.ShowNotificationDirect($"Controls for {GameName} reseted{extraNotice}");
                }
                else
                {
                    PlatformService.ShowNotificationDirect($"Controls for {VM.SystemNamePreview} reseted{extraNotice}");
                }
            }
            catch (Exception e)
            {

            }
            ControlsLoadingProgress.Visibility = Visibility.Collapsed;
            ControlsLoadingProgressBar.IsActive = false;
        }

        private async void SaveControlsGamesOnly_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.MainFilePath != null && VM.MainFilePath.Length > 0)
                {
                    if (VM.SystemNamePreview != null)
                    {
                        var TargetSystem = VM.SystemNamePreview;
                        if (TargetSystem != null && TargetSystem.Length > 0)
                        {
                            var GameName = Path.GetFileNameWithoutExtension(VM.MainFilePath);

                            PlatformService.PlayNotificationSoundDirect("alert");
                            ConfirmConfig confirmSave = new ConfirmConfig();
                            confirmSave.SetTitle("Save Gamepad");
                            confirmSave.SetMessage($"Do you want to save controls for {GameName}? ");
                            confirmSave.UseYesNo();

                            var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                            if (StartSave)
                            {
                                await CustomGamePadStoreAsync(GameName);
                            }
                        }
                    }
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("alert");
                    PlatformService.ShowNotificationDirect($"Core started without content");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void DeleteControlsGamesOnly_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.MainFilePath != null && VM.MainFilePath.Length > 0)
                {
                    if (VM.SystemNamePreview != null)
                    {
                        var TargetSystem = VM.SystemNamePreview;
                        if (TargetSystem != null && TargetSystem.Length > 0)
                        {
                            var GameName = Path.GetFileNameWithoutExtension(VM.MainFilePath);

                            PlatformService.PlayNotificationSoundDirect("alert");
                            ConfirmConfig confirmDelete = new ConfirmConfig();
                            confirmDelete.SetTitle("Reset Controls");
                            confirmDelete.SetMessage($"Do you want to reset controls for {GameName}? ");
                            confirmDelete.UseYesNo();

                            var StartDelete = await UserDialogs.Instance.ConfirmAsync(confirmDelete);

                            if (StartDelete)
                            {
                                await CustomGamePadDeleteAsync(GameName);
                            }
                        }

                    }
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("alert");
                    PlatformService.ShowNotificationDirect($"Core started without content");
                }
            }
            catch (Exception ex)
            {

            }
            ControlsLoadingProgress.Visibility = Visibility.Collapsed;
            ControlsLoadingProgressBar.IsActive = false;
        }

        private void MenusCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.HideMenuGrid();
            }
            catch (Exception ex)
            {

            }
        }



        private void RestoreButtonsPositions()
        {
            try
            {
                InputService.ResetButtonsPositions();
            }
            catch (Exception ex)
            {

            }
        }
        private async void ResetButtonsCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmSave = new ConfirmConfig();
                confirmSave.SetTitle("Reset Buttons");
                confirmSave.SetMessage($"Do you want to reset buttons customizations for {VM.SystemNamePreview}? ");
                confirmSave.UseYesNo();

                var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                if (StartSave)
                {
                    var targetPort = GetSelectedPort();
                    InputService.ResetGamepadButtons(targetPort);
                    RestoreButtonsPositions();
                    PlatformService.PlayNotificationSoundDirect("success");
                    //await UserDialogs.Instance.AlertAsync($"Buttons for {VM.SystemNamePreview} reseted");
                    PlatformService.ShowNotificationDirect($"Buttons for {VM.SystemNamePreview} reseted");

                    var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(GamepadSaveLocation);
                    if (localFolder == null)
                    {
                        return;
                    }

                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{VM.SystemName}.rbm");
                    if (targetFileTest != null)
                    {
                        ControlsLoadingProgress.Visibility = Visibility.Visible;
                        ControlsLoadingProgressBar.IsActive = true;
                        await targetFileTest.DeleteAsync();
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
            ControlsLoadingProgress.Visibility = Visibility.Collapsed;
            ControlsLoadingProgressBar.IsActive = false;
        }

        private void DisableButtonsEditMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SetButtonsCustomization.Execute(null);
            }
            catch (Exception ex)
            {

            }
        }

        private async void SaveButtonsCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmSave = new ConfirmConfig();
                confirmSave.SetTitle("Save Buttons");
                confirmSave.SetMessage($"Do you want to save buttons customizations for {VM.SystemNamePreview}? ");
                confirmSave.UseYesNo();

                var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                if (StartSave)
                {
                    VM.SetButtonsIsLoadingState(true);
                    var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(GamepadSaveLocation);
                    if (localFolder == null)
                    {
                        localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(GamepadSaveLocation);
                    }

                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{VM.SystemName}.rbm");
                    if (targetFileTest != null)
                    {
                        await targetFileTest.DeleteAsync();
                    }
                    var targetFile = await localFolder.CreateFileAsync($"{VM.SystemName}.rbm");

                    Encoding unicode = Encoding.Unicode;
                    byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(InputService.ButtonsPositions));
                    using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var outStream = stream.AsStream();
                        await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                        await outStream.FlushAsync();
                    }
                    PlatformService.PlayNotificationSoundDirect("success");
                    //await UserDialogs.Instance.AlertAsync($"Buttons saved for {VM.SystemNamePreview}");
                    PlatformService.ShowNotificationDirect($"Buttons saved for {VM.SystemNamePreview}");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
            try
            {
                VM.SetButtonsIsLoadingState(false);
            }
            catch (Exception ex)
            {

            }

        }

        private async void EffectsReset_Click(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-01");
            if (VM != null)
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmResetAll = new ConfirmConfig();
                confirmResetAll.SetTitle("Reset Effects");
                confirmResetAll.SetMessage($"Do you want to reset all effects?");
                confirmResetAll.UseYesNo();

                var ResetAll = await UserDialogs.Instance.ConfirmAsync(confirmResetAll);

                if (ResetAll)
                {
                    VM.ClearAllEffects.Execute(null);
                }
            }
        }

        private void HideEffects(object sender, EventArgs e)
        {
            if (VM != null)
            {
                VM.ShowAllEffects.Execute(null);
            }
        }
        private void EffectsCancel_Click(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-01");
            if (VM != null)
            {
                VM.ShowAllEffects.Execute(null);
            }
        }

        private void RefreshLogList_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null)
            {
                VM.forceReloadLogsList = true;
            }
        }



        private void FindChildren<T>(List<T> results, DependencyObject startNode) where T : DependencyObject
        {
            try
            {
                int count = VisualTreeHelper.GetChildrenCount(startNode);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                    if ((current.GetType()).Equals(typeof(T)) || (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T))))
                    {
                        T asType = (T)current;
                        results.Add(asType);
                    }
                    FindChildren<T>(results, current);
                }
            }
            catch (Exception e)
            {

            }
        }

        private void DebugLogList_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null)
            {
                VM.EnabledDebugLogsList = !VM.EnabledDebugLogsList;
            }
        }

        double XCurrent;
        double YCurrent;
        private void FPSCounterGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                XCurrent += e.Delta.Translation.X;
                YCurrent += e.Delta.Translation.Y;
                BindingsUpdate();
            }
            catch (Exception ex)
            {

            }
        }

        double XSCurrent;
        double YSCurrent;
        private void SensorsGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                XSCurrent += e.Delta.Translation.X;
                YSCurrent += e.Delta.Translation.Y;
                BindingsUpdate();
            }
            catch (Exception ex)
            {

            }
        }
        public async void UpdateBindingsWithDelay()
        {
            try
            {
                while (!VM.isGameStarted && !VM.FailedToLoadGame)
                {
                    await Task.Delay(1000);
                }
                await Task.Delay(1500);
                BindingsUpdate();
            }
            catch (Exception ex)
            {

            }
        }
        public void GamePlayPageUpdateBindings(object sender, EventArgs args)
        {
            BindingsUpdate();
        }
        private void ScreenShot_Click(object sender, RoutedEventArgs e)
        {
            TakeScreenShot();
        }

        private void logToFile_Click(object sender, RoutedEventArgs e)
        {
            LogFileState = logToFile.IsChecked.Value;
        }

        private async void downloadLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                folderPicker.FileTypeFilter.Add(".txt");
                var saveFolder = await folderPicker.PickSingleFolderAsync();
                if (saveFolder != null)
                {
                    var LogLocation = await VM.EmulationService.GetLogFileLocation();
                    if (LogLocation != null)
                    {
                        VM.GameIsLoadingState(true);
                        await LogLocation.CopyAsync(saveFolder, LogLocation.Name, NameCollisionOption.GenerateUniqueName);

                        //Copy VFS Logs if exists
                        var vfsFile = await VM.EmulationService.GetLogVFSFileLocation();
                        if (vfsFile != null)
                        {
                            await vfsFile.CopyAsync(saveFolder, vfsFile.Name, NameCollisionOption.GenerateUniqueName);
                        }
                        PlatformService.PlayNotificationSoundDirect("success");
                        PlatformService.ShowNotificationDirect($"Logs file saved sccussfully");
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        PlatformService.ShowNotificationDirect("Unable to get log location!");
                    }
                }
                VM.GameIsLoadingState(false);
            }
            catch (Exception ex)
            {
                VM?.GameIsLoadingState(false);
                PlatformService.PlayNotificationSoundDirect("error");
                PlatformService.ShowNotificationDirect(ex.Message);
            }
        }

        private async void resetLogFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmReset = new ConfirmConfig();
                confirmReset.SetTitle("Reset logs");
                confirmReset.SetMessage($"Do you want to reset logs file for {VM.SystemNamePreview}? ");
                confirmReset.UseYesNo();

                var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                if (StartReset)
                {
                    VM?.GameIsLoadingState(true);
                    var LogLocation = await VM.EmulationService.GetLogFileLocation();
                    if (LogLocation != null)
                    {
                        await FileIO.WriteTextAsync(LogLocation, "");

                        //Reset VFS if exists
                        var LogVFSLocation = await VM.EmulationService.GetLogVFSFileLocation();
                        if (LogVFSLocation != null)
                        {
                            await FileIO.WriteTextAsync(LogVFSLocation, "");
                        }

                        PlatformService.PlayNotificationSoundDirect("success");
                        PlatformService.ShowNotificationDirect($"Reset logs file done");
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        PlatformService.ShowNotificationDirect("Unable to get log location!");
                    }
                }
                VM?.GameIsLoadingState(false);
            }
            catch (Exception ex)
            {
                VM?.GameIsLoadingState(false);
                PlatformService.PlayNotificationSoundDirect("error");
                PlatformService.ShowNotificationDirect(ex.Message);
            }
        }

        private void VFSLog_Click(object sender, RoutedEventArgs e)
        {
            VFSFileState = VFSLog.IsChecked.Value;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Window.Current.SizeChanged -= windowsSizeChanged;
            }
            catch (Exception ex)
            {

            }
            callResizeTimer();
            VM.ViewDisappearing();
        }

        SystemMenuModel SelectedOption = null;
        private void MenusGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SelectedOption = (SystemMenuModel)e.ClickedItem;
                if (SelectedOption != null)
                {
                    VM.GameSystemMenuHandler(SelectedOption);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        private void SavesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SaveSlotsModel SelectedState = (SaveSlotsModel)e.ClickedItem;
                if (SelectedState != null)
                {
                    VM.SaveSelectHandler(SelectedState);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        private void SavesList_Holding(object sender, HoldingRoutedEventArgs e)
        {
            //Handled at RightTapped
            /*try
            {
                var source = (FrameworkElement)e.OriginalSource;
                var data = (SaveSlotsModel)source.DataContext;
                if (data != null)
                {
                    VM.SaveHoldHandler(data);
                }
            }catch(Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }*/
        }

        private void SavesList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                var source = (FrameworkElement)e.OriginalSource;
                var data = (SaveSlotsModel)source.DataContext;
                if (data != null)
                {
                    VM.SaveHoldHandler(data);
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        private void SavesList_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.GamepadX || e.Key == Windows.System.VirtualKey.Delete)
            {
                try
                {
                    var source = (FrameworkElement)e.OriginalSource;
                    var data = (SaveSlotsModel)source.DataContext;
                    if (data != null)
                    {
                        VM.SaveHoldHandler(data);
                    }
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    //PlatformService.ShowErrorMessage(ex);
                }
            }
        }

        private void clearListLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.clearListLogs();
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }
        public async void RestoreXBOXListPosition(object sender, EventArgs args)
        {
            return;
            try
            {
                ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(MenusGrid).FirstOrDefault();
                if (svl != null)
                {
                    svl.ChangeView(0, PlatformService.vXboxScrollPosition, 1);

                    if (SelectedOption != null)
                    {
                        try
                        {
                            await Task.Delay(500);
                            var dataItems = MenusGrid.Items.Where(item => ((SystemMenuModel)item).MenuCommand == SelectedOption.MenuCommand);
                            if (dataItems != null && dataItems.Count() > 0)
                            {
                                var SelectedOptionTemp = MenusGrid.ContainerFromItem(dataItems.FirstOrDefault()) as GridViewItem;
                                if (SelectedOptionTemp != null)
                                {
                                    SelectedOptionTemp.Focus(FocusState.Programmatic);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateItemState(object sender, EventArgs args)
        {
            try
            {
                var senderObject = (SystemMenuModel)sender;
                try
                {
                    var dataItems = MenusGrid.Items.Where(item => ((SystemMenuModel)item).MenuCommand == senderObject.MenuCommand);
                    if (dataItems != null && dataItems.Count() > 0)
                    {
                        ((SystemMenuModel)dataItems.FirstOrDefault()).isEnabled = senderObject.isEnabled;
                        ((SystemMenuModel)dataItems.FirstOrDefault()).SetSwitchState(senderObject.MenuSwitchState);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateXBOXListPosition(object sender, EventArgs args)
        {
            return;
            try
            {
                ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(MenusGrid).FirstOrDefault();
                if (svl != null)
                {
                    PlatformService.vXboxScrollPosition = svl.VerticalOffset;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void OptionsSaveGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.MainFilePath != null && VM.MainFilePath.Length > 0)
                {
                    if (VM.SystemName != null)
                    {
                        var TargetSystem = VM.SystemName;
                        var TargetCore = VM.CoreName;
                        var IsNewCore = VM.EmulationService.IsNewCore();
                        if (TargetSystem != null && TargetSystem.Length > 0)
                        {
                            var GameName = Path.GetFileNameWithoutExtension(VM.MainFilePath);
                            PlatformService.PlayNotificationSoundDirect("alert");
                            ConfirmConfig confirmSave = new ConfirmConfig();
                            confirmSave.SetTitle("Save Options");
                            confirmSave.SetMessage($"Do you want to save {GameName}'s options as default values? ");
                            confirmSave.UseYesNo();

                            var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                            if (StartSave)
                            {
                                await VM.CoreOptionsStoreAsync(TargetCore, TargetSystem, IsNewCore, GameName);
                                PlatformService.PlayNotificationSoundDirect("success");
                                PlatformService.ShowNotificationDirect($"{GameName}'s options has been saved");
                            }
                        }
                    }
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("alert");
                    PlatformService.ShowNotificationDirect($"Core started without content");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void OptionsRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.MainFilePath != null && VM.MainFilePath.Length > 0)
                {
                    if (VM.SystemName != null)
                    {
                        var TargetSystem = VM.SystemName;
                        var TargetCore = VM.CoreName;
                        var IsNewCore = VM.EmulationService.IsNewCore();
                        if (TargetSystem != null && TargetSystem.Length > 0)
                        {
                            var GameName = Path.GetFileNameWithoutExtension(VM.MainFilePath);
                            PlatformService.PlayNotificationSoundDirect("alert");
                            ConfirmConfig confirmSave = new ConfirmConfig();
                            confirmSave.SetTitle("Remove Options");
                            confirmSave.SetMessage($"Do you want to remove {GameName}'s options as default values? ");
                            confirmSave.UseYesNo();

                            var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                            if (StartSave)
                            {
                                saveOptionsListPosition();
                                await VM.CoreOptionsRemoveAsync(TargetCore, TargetSystem, IsNewCore, GameName);
                                PlatformService.PlayNotificationSoundDirect("success");
                                //await UserDialogs.Instance.AlertAsync($"{VM.SystemNamePreview}'s options has been saved", "Save Done");
                                PlatformService.ShowNotificationDirect($"{GameName}'s options has been remove");
                                var test = await GameSystemSelectionViewModel.OptionsRetrieveAsync(VM.CoreName, VM.SystemName, VM.EmulationService.currentCore.IsNewCore);
                                await SystemCoresOptions(true);
                                VM.EmulationService.currentCore.UpdateCoreOptionsValues(false);
                                VM.EmulationService.currentCore.UpdateCoreOptions();
                            }
                        }
                    }
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("alert");
                    PlatformService.ShowNotificationDirect($"Core started without content");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        double vOptionsScrollPosition = 0;
        public void saveOptionsListPosition()
        {
            try
            {
                ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(CoreOptionsPage).FirstOrDefault();
                if (svl != null)
                {
                    vOptionsScrollPosition = svl.VerticalOffset;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void restoreOptionsListPosition()
        {
            try
            {
                ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(CoreOptionsPage).FirstOrDefault();
                if (svl != null)
                {
                    svl.ChangeView(0, vOptionsScrollPosition, 1);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void OptionsReset_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                if (VM.SystemName != null)
                {
                    var TargetSystem = VM.SystemName;
                    var TargetCore = VM.CoreName;
                    var IsNewCore = VM.EmulationService.IsNewCore();
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        var GameName = Path.GetFileNameWithoutExtension(VM.MainFilePath);
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmSave = new ConfirmConfig();
                        confirmSave.SetTitle("Reset Options");
                        confirmSave.SetMessage($"Do you want to reset options to default values? ");
                        confirmSave.UseYesNo();

                        var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                        if (StartSave)
                        {
                            saveOptionsListPosition();
                            var test = await GameSystemSelectionViewModel.OptionsRetrieveAsync(VM.CoreName, VM.SystemName, VM.EmulationService.currentCore.IsNewCore);
                            PlatformService.PlayNotificationSoundDirect("success");
                            PlatformService.ShowNotificationDirect($"Options reset done");
                            await SystemCoresOptions(true);
                            VM.EmulationService.currentCore.UpdateCoreOptionsValues(false);
                            VM.EmulationService.currentCore.UpdateCoreOptions();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        bool RightTapped = false;
        private void PlayerPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                if (VM != null)
                {
                    RightTapped = true;
                    VM.PointerRightTabbedCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {

            }
        }


        public bool KeyboardVisible
        {
            get
            {
                return PlatformService.KeyboardVisibleState;
            }
            set
            {
                PlatformService.KeyboardVisibleState = value;
                if (VM != null)
                {
                    VM.TouchPadOpacity = value ? 0.0f : 1.0f;
                }
                BindingsUpdate();
            }
        }
        public bool isKeyboardActive
        {
            get
            {
                return PlatformService.KeyboardEvent != null || PlatformService.CoreReadingRetroKeyboard;
            }
        }
        public void ShowKeyboardHandler(object sender, EventArgs args)
        {
            PlatformService.KeyboardVisibleState = true;
            BindingsUpdate();
        }
        public void HideKeyboardHandler(object sender, EventArgs args)
        {
            PlatformService.KeyboardVisibleState = false;
            BindingsUpdate();
        }

        static KeyboardLayout Keyboard;
        KeyboardLayout keyboard
        {
            get
            {
                return Keyboard;
            }
            set
            {
                Keyboard = value;
            }
        }

        public void setBinding(UIElement uIElement, DependencyProperty dependencyProperty, object source, string path, bool boolConverter = false)
        {
            try
            {
                Binding binding = new Binding();
                binding.Source = source;
                binding.Path = new PropertyPath(path);
                binding.Mode = BindingMode.OneWay;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                if (boolConverter)
                {
                    binding.Converter = this.Resources["BoolToVisibilityConverter"] as Converters.BoolToVisibilityConverter;
                }
                BindingOperations.SetBinding(uIElement, dependencyProperty, binding);
            }
            catch (Exception ex)
            {

            }
        }
        public void BuildKeyboardLayout()
        {
            try
            {
                keyboard = new KeyboardLayout(VM.EmulationService.SystemName);

                var defaultWidth = 65;
                var defaultHeight = 55;
                var defaultFontSize = 16;
                var defaultSubFontSize = 11;

                keyboard.UpdateKeyboardKeySize(defaultWidth, defaultHeight, defaultFontSize, defaultSubFontSize);

                /*TheKeyboard.Children.Clear();
                int crow = 0;
                int max = 13;
                foreach (var keyboardKeys in keyboard.Main)
                {
                    Grid grid = new Grid();
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Height = GridLength.Auto;
                    TheKeyboard.RowDefinitions.Add(rowDefinition);
                    TheKeyboard.Children.Add(grid);
                    Grid.SetRow(grid, crow);

                    int row = 0;
                    int col = 0;
                    RowDefinition subRowDefinition = new RowDefinition();
                    subRowDefinition.Height = GridLength.Auto;
                    grid.RowDefinitions.Add(subRowDefinition);
                    foreach (var sKey in keyboardKeys)
                    {
                        if (row == 0)
                        {
                            ColumnDefinition columnDefinition = new ColumnDefinition();
                            columnDefinition.Width = GridLength.Auto;
                            grid.ColumnDefinitions.Add(columnDefinition);
                        }

                        Grid keyContainer = new Grid();
                        setBinding(keyContainer, Grid.BorderBrushProperty, sKey, "BorderColor");
                        setBinding(keyContainer, Grid.BackgroundProperty, sKey, "BackgroundColor");
                        setBinding(keyContainer, Grid.WidthProperty, sKey, "MinWidth");
                        setBinding(keyContainer, Grid.HeightProperty, sKey, "MinHeight");

                        keyContainer.CornerRadius = new CornerRadius(5);
                        keyContainer.BorderThickness = new Thickness(1);
                        RowDefinition r1 = new RowDefinition();
                        RowDefinition r2 = new RowDefinition();
                        keyContainer.RowDefinitions.Add(r1);
                        keyContainer.RowDefinitions.Add(r2);
                        TextBlock keyName = new TextBlock();
                        TextBlock shiftName = new TextBlock();

                        setBinding(keyName, TextBlock.TextProperty, sKey, "KeyName");
                        setBinding(keyName, TextBlock.PaddingProperty, sKey, "Padding");
                        setBinding(keyName, TextBlock.FontSizeProperty, sKey, "MainFontSize");
                        setBinding(keyName, TextBlock.MarginProperty, sKey, "Margin");

                        setBinding(shiftName, TextBlock.TextProperty, sKey, "ShiftName");
                        setBinding(shiftName, TextBlock.FontSizeProperty, sKey, "SubFontSize");
                        setBinding(shiftName, TextBlock.VisibilityProperty, sKey, "ShiftTextVisibleState");

                        keyName.VerticalAlignment = VerticalAlignment.Center;
                        keyName.HorizontalAlignment = HorizontalAlignment.Center;

                        shiftName.Padding = new Thickness(5, 0, 0, 1);
                        shiftName.Foreground = new SolidColorBrush(Colors.Gray);
                        shiftName.Opacity = 0.6;

                        keyContainer.Children.Add(keyName);
                        keyContainer.Children.Add(shiftName);
                        Grid.SetRow(keyName, 1);
                        Grid.SetRow(shiftName, 0);

                        Button key = new Button();
                        setBinding(key, Button.OpacityProperty, sKey, "KeyOpacity");
                        setBinding(key, Button.TagProperty, sKey, "KeyRef");

                        key.Background = new SolidColorBrush(Colors.Transparent);
                        key.Content = keyContainer;

                        grid.Children.Add(key);
                        Grid.SetColumn(key, col);
                        Grid.SetRow(key, row);
                        col++;
                        if (col == max)
                        {
                            col = 0;
                            row++;
                            subRowDefinition = new RowDefinition();
                            subRowDefinition.Height = GridLength.Auto;
                            grid.RowDefinitions.Add(subRowDefinition);
                        }
                    }
                    crow++;
                }*/
                KeyboardListItemsGroup.Source = keyboard.Main;
                UpdateKeyboardLayout();
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        public async void UpdateKeyboardKeysByRequested()
        {
            try
            {
                while (keyboard == null)
                {
                    await Task.Delay(1500);
                }
                if (PlatformService.CoreReadingRetroKeyboard)
                {
                    foreach (var kItem in keyboard.Main)
                    {
                        foreach (var sItem in kItem)
                        {
                            if (InputService.RequestedKeys.Contains((uint)sItem.KeyCode))
                            {
                                sItem.IsEnabled = true;
                            }
                            else
                            {
                                sItem.IsEnabled = false;
                            }
                        }
                        BindingsUpdate();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Ctrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PlatformService.KeyboardEvent == null && !PlatformService.CoreReadingRetroKeyboard)
                {
                    PlatformService.ShowNotificationDirect("Core doesn't support keyboard!", 2);
                    return;
                }
                if (PlatformService.KeyboardEvent != null)
                {
                    PlatformService.KeyboardEvent.Invoke(Ctrl.IsChecked.Value, (uint)Keys.RETROK_LCTRL, (uint)Keys.RETROK_LCTRL, 0);
                }
                else if (PlatformService.CoreReadingRetroKeyboard)
                {
                    var keyCode = Keys.RETROK_LCTRL;

                    foreach (var kItem in PlatformService.RetroKeysMap)
                    {
                        if (((uint)kItem.Value).Equals((uint)keyCode))
                        {
                            if (!Ctrl.IsChecked.Value)
                            {
                                InputService.vKeyboardKeys.Remove((uint)kItem.Key);
                                tempKeys.Remove((uint)kItem.Key);
                            }
                            else
                            if (!InputService.vKeyboardKeys.Contains((uint)kItem.Key))
                            {
                                InputService.vKeyboardKeys.Add((uint)kItem.Key);
                                tempKeys.Add((uint)kItem.Key);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Shift_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PlatformService.KeyboardEvent == null && !PlatformService.CoreReadingRetroKeyboard)
                {
                    PlatformService.ShowNotificationDirect("Core doesn't support keyboard!", 2);
                    return;
                }
                keyboard.SetShiftKeyState(!keyboard.ShiftKeyState);
                if (PlatformService.KeyboardEvent != null)
                {
                    PlatformService.KeyboardEvent.Invoke(Shift.IsChecked.Value, (uint)Keys.RETROK_LSHIFT, (uint)Keys.RETROK_LSHIFT, 0);
                }
                else if (PlatformService.CoreReadingRetroKeyboard)
                {
                    var keyCode = Keys.RETROK_LSHIFT;

                    foreach (var kItem in PlatformService.RetroKeysMap)
                    {
                        if (((uint)kItem.Value).Equals((uint)keyCode))
                        {
                            if (!Shift.IsChecked.Value)
                            {
                                InputService.vKeyboardKeys.Remove((uint)kItem.Key);
                                tempKeys.Remove((uint)kItem.Key);
                            }
                            else
                            if (!InputService.vKeyboardKeys.Contains((uint)kItem.Key))
                            {
                                InputService.vKeyboardKeys.Add((uint)kItem.Key);
                                tempKeys.Add((uint)kItem.Key);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Alt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PlatformService.KeyboardEvent == null && !PlatformService.CoreReadingRetroKeyboard)
                {
                    PlatformService.ShowNotificationDirect("Core doesn't support keyboard!", 2);
                    return;
                }
                if (PlatformService.KeyboardEvent != null)
                {
                    PlatformService.KeyboardEvent.Invoke(Alt.IsChecked.Value, (uint)Keys.RETROK_LALT, (uint)Keys.RETROK_LALT, 0);
                }
                else if (PlatformService.CoreReadingRetroKeyboard)
                {
                    var keyCode = Keys.RETROK_LALT;

                    foreach (var kItem in PlatformService.RetroKeysMap)
                    {
                        if (((uint)kItem.Value).Equals((uint)keyCode))
                        {
                            if (!Alt.IsChecked.Value)
                            {
                                InputService.vKeyboardKeys.Remove((uint)kItem.Key);
                                tempKeys.Remove((uint)kItem.Key);
                            }
                            else
                            if (!InputService.vKeyboardKeys.Contains((uint)kItem.Key))
                            {
                                InputService.vKeyboardKeys.Add((uint)kItem.Key);
                                tempKeys.Add((uint)kItem.Key);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void Esc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PlatformService.KeyboardEvent == null && !PlatformService.CoreReadingRetroKeyboard)
                {
                    PlatformService.ShowNotificationDirect("Core doesn't support keyboard!", 2);
                    return;
                }
                if (PlatformService.KeyboardEvent != null)
                {
                    PlatformService.KeyboardEvent.Invoke(true, (uint)Keys.RETROK_ESCAPE, (uint)Keys.RETROK_ESCAPE, 0);
                    await Task.Delay(50);
                    PlatformService.KeyboardEvent.Invoke(false, (uint)Keys.RETROK_ESCAPE, (uint)Keys.RETROK_ESCAPE, 0);
                }
                else if (PlatformService.CoreReadingRetroKeyboard)
                {
                    var keyCode = Keys.RETROK_ESCAPE;

                    foreach (var kItem in PlatformService.RetroKeysMap)
                    {
                        if (((uint)kItem.Value).Equals((uint)keyCode))
                        {
                            if (!InputService.vKeyboardKeys.Contains((uint)kItem.Key))
                            {
                                InputService.vKeyboardKeys.Add((uint)kItem.Key);
                                tempKeys.Add((uint)kItem.Key);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static List<uint> tempKeys = new List<uint>();

        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PlatformService.KeyboardEvent == null && !PlatformService.CoreReadingRetroKeyboard)
                {
                    PlatformService.ShowNotificationDirect("Core doesn't support keyboard!", 2);
                    return;
                }
                KeyboardKey tempKey;

                var clickedItem = (RepeatButton)e.OriginalSource;
                tempKey = (KeyboardKey)clickedItem.DataContext;
                if (!tempKey.IsCaps)
                {
                    foreach (var kItem in PlatformService.RetroKeysMap)
                    {
                        if (((uint)kItem.Value).Equals((uint)tempKey.KeyCode))
                        {
                            var targetKey = (uint)kItem.Key;

                            if (!InputService.vKeyboardKeys.Contains(targetKey))
                            {

                                lock (InputService.vKeyboardKeys)
                                {
                                    InputService.vKeyboardKeys.Add(targetKey);
                                }
                                if (!tempKeys.Contains(targetKey))
                                {
                                    lock (tempKeys)
                                    {
                                        tempKeys.Add(targetKey);
                                    }
                                }
                            }
                            VM.InputService.InjectInputKey(targetKey);

                            break;
                        }
                    }
                    if (!tempKey.IsOn)
                    {
                        tempKey.SwitchKey(true);
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void CloseKeyboard_Click(object sender, RoutedEventArgs e)
        {
            HideKeyboardHandler(null, null);
        }

        private void EffectsPageList_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var effectItem = (EffectsListItem)e.ClickedItem;
                effectItem.IsOn = !effectItem.IsOn;
            }
            catch (Exception ex)
            {

            }
        }

        public float KeyboardOpacity
        {
            get
            {
                if (PlatformService.DPadActive)
                {
                    return 0.85f;
                }
                else
                {
                    return 0.9f;
                }
            }
        }

        private async void MainKeys_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (PlatformService.KeyboardEvent == null && !PlatformService.CoreReadingRetroKeyboard)
                {
                    PlatformService.ShowNotificationDirect("Core doesn't support keyboard!", 2);
                    return;
                }
                if (PlatformService.DPadActive)
                {
                    if (e.Key == VirtualKey.GamepadA || e.Key == VirtualKey.GamepadX || e.Key == VirtualKey.GamepadY)
                    {
                        var tempKey = (KeyboardKey)((GridViewItem)e.OriginalSource).Content;
                        if (!tempKey.IsCaps)
                        {
                            if (!tempKey.IsOn)
                            {
                                foreach (var kItem in PlatformService.RetroKeysMap)
                                {
                                    if (((uint)kItem.Value).Equals((uint)tempKey.KeyCode))
                                    {
                                        if (!InputService.vKeyboardKeys.Contains((uint)kItem.Key))
                                        {
                                            InputService.vKeyboardKeys.Add((uint)kItem.Key);
                                            tempKeys.Add((uint)kItem.Key);
                                        }
                                        break;
                                    }
                                }
                                tempKey.SwitchKey(true);
                            }
                        }
                        else
                        {
                            //For capslock input state will be ignored
                            tempKey.SwitchKey(false);
                            keyboard.SetCapsKeyState(!keyboard.CapsKeyState);
                            PlatformService.KeyboardEvent.Invoke(true, (uint)tempKey.KeyCode, (uint)tempKey.KeyCode, 0);
                            await Task.Delay(50);
                            PlatformService.KeyboardEvent.Invoke(false, (uint)tempKey.KeyCode, (uint)tempKey.KeyCode, 0);
                        }
                    }
                    else
                    {
                        switch (e.Key)
                        {
                            case VirtualKey.GamepadRightShoulder:
                                PlatformService.KeyboardEvent.Invoke(true, (uint)Keys.RETROK_BACKSPACE, (uint)Keys.RETROK_BACKSPACE, 0);
                                await Task.Delay(50);
                                PlatformService.KeyboardEvent.Invoke(false, (uint)Keys.RETROK_BACKSPACE, (uint)Keys.RETROK_BACKSPACE, 0);
                                break;

                            case VirtualKey.GamepadLeftShoulder:
                                PlatformService.KeyboardEvent.Invoke(true, (uint)Keys.RETROK_SPACE, (uint)Keys.RETROK_SPACE, 0);
                                await Task.Delay(50);
                                PlatformService.KeyboardEvent.Invoke(false, (uint)Keys.RETROK_SPACE, (uint)Keys.RETROK_SPACE, 0);
                                break;

                            case VirtualKey.GamepadRightThumbstickUp:
                                PlatformService.KeyboardEvent.Invoke(true, (uint)Keys.RETROK_UP, (uint)Keys.RETROK_UP, 0);
                                await Task.Delay(50);
                                PlatformService.KeyboardEvent.Invoke(false, (uint)Keys.RETROK_UP, (uint)Keys.RETROK_UP, 0);
                                break;

                            case VirtualKey.GamepadRightThumbstickDown:
                                PlatformService.KeyboardEvent.Invoke(true, (uint)Keys.RETROK_DOWN, (uint)Keys.RETROK_DOWN, 0);
                                await Task.Delay(50);
                                PlatformService.KeyboardEvent.Invoke(false, (uint)Keys.RETROK_DOWN, (uint)Keys.RETROK_DOWN, 0);
                                break;

                            case VirtualKey.GamepadRightThumbstickLeft:
                                PlatformService.KeyboardEvent.Invoke(true, (uint)Keys.RETROK_LEFT, (uint)Keys.RETROK_LEFT, 0);
                                await Task.Delay(50);
                                PlatformService.KeyboardEvent.Invoke(false, (uint)Keys.RETROK_LEFT, (uint)Keys.RETROK_LEFT, 0);
                                break;

                            case VirtualKey.GamepadRightThumbstickRight:
                                PlatformService.KeyboardEvent.Invoke(true, (uint)Keys.RETROK_RIGHT, (uint)Keys.RETROK_RIGHT, 0);
                                await Task.Delay(50);
                                PlatformService.KeyboardEvent.Invoke(false, (uint)Keys.RETROK_RIGHT, (uint)Keys.RETROK_RIGHT, 0);
                                break;

                            case VirtualKey.GamepadMenu:
                                PlatformService.KeyboardEvent.Invoke(true, (uint)Keys.RETROK_RETURN, (uint)Keys.RETROK_RETURN, 0);
                                await Task.Delay(50);
                                PlatformService.KeyboardEvent.Invoke(false, (uint)Keys.RETROK_RETURN, (uint)Keys.RETROK_RETURN, 0);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static void SwitchKeyOff(uint key)
        {
            try
            {
                bool buttonFound = false;
                foreach (var kItem in Keyboard.Main)
                {
                    foreach (var sItem in kItem)
                    {
                        foreach (var rItem in PlatformService.RetroKeysMap)
                        {
                            if (((uint)rItem.Value).Equals((uint)sItem.KeyCode))
                            {
                                if (((uint)rItem.Key).Equals(key))
                                {
                                    if (!sItem.IsCaps)
                                    {
                                        //sItem.SwitchKey(false);
                                        lock (tempKeys)
                                        {
                                            tempKeys.Remove(key);
                                        }
                                    }
                                    buttonFound = true;
                                    break;
                                }
                            }
                        }
                        if (buttonFound)
                        {
                            break;
                        }
                    }
                    if (buttonFound)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        public void ReleaseAllKeys()
        {
            try
            {
                foreach (var kItem in MainKeys.Items)
                {
                    releaseKeyDirect(kItem);
                }
                CleanUpVKeys();
            }
            catch (Exception ex)
            {

            }
        }

        public async static void CleanUpVKeys()
        {
            try
            {
                await Task.Delay(50);
                foreach (var tItem in tempKeys)
                {
                    InputService.vKeyboardKeys.Remove(tItem);
                }
                if (Keyboard != null && Keyboard.Main != null)
                {
                    foreach (var kItem in Keyboard.Main)
                    {
                        foreach (var sItem in kItem)
                        {
                            if (!sItem.IsCaps)
                            {
                                sItem.SwitchKey(false);
                            }
                        }
                    }
                }
                tempKeys.Clear();
            }
            catch (Exception ex)
            {

            }
        }

        //Below will release the buttons in many cases Touch, Key, Gamepad, Mouse
        private void Esc_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CleanUpVKeys();
        }

        private async void releaseKeyDirect(object sender, bool forceRelease = false)
        {
            try
            {
                var targetKey = (KeyboardKey)sender;
                if (targetKey.IsCaps)
                {
                    if (!forceRelease)
                    {
                        //For capslock input state will be ignored
                        targetKey.SwitchKey(false);
                        keyboard.SetCapsKeyState(!keyboard.CapsKeyState);
                        PlatformService.KeyboardEvent.Invoke(true, (uint)targetKey.KeyCode, (uint)targetKey.KeyCode, 0);
                        await Task.Delay(50);
                        PlatformService.KeyboardEvent.Invoke(false, (uint)targetKey.KeyCode, (uint)targetKey.KeyCode, 0);
                    }
                    return;
                }

                targetKey.SwitchKey(false);
                foreach (var kItem in PlatformService.RetroKeysMap)
                {
                    if (((uint)kItem.Value).Equals((uint)targetKey.KeyCode))
                    {
                        if (InputService.vKeyboardKeys.Contains((uint)kItem.Key))
                        {
                            PlatformService.InvokeEkeyboardEvent(false, kItem.Key);
                            lock (InputService.vKeyboardKeys)
                            {
                                InputService.vKeyboardKeys.Remove((uint)kItem.Key);
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async void releaseKey(object sender, bool forceRelease = false)
        {
            try
            {
                var targetKey = (KeyboardKey)((RepeatButton)sender).Tag;
                if (targetKey.IsCaps)
                {
                    if (!forceRelease)
                    {
                        //For capslock input state will be ignored
                        targetKey.SwitchKey(false);
                        keyboard.SetCapsKeyState(!keyboard.CapsKeyState);
                        PlatformService.KeyboardEvent.Invoke(true, (uint)targetKey.KeyCode, (uint)targetKey.KeyCode, 0);
                        await Task.Delay(50);
                        PlatformService.KeyboardEvent.Invoke(false, (uint)targetKey.KeyCode, (uint)targetKey.KeyCode, 0);
                    }
                    return;
                }

                targetKey.SwitchKey(false);
                foreach (var kItem in PlatformService.RetroKeysMap)
                {
                    if (((uint)kItem.Value).Equals((uint)targetKey.KeyCode))
                    {
                        if (InputService.vKeyboardKeys.Contains((uint)kItem.Key))
                        {
                            PlatformService.InvokeEkeyboardEvent(false, kItem.Key);
                            lock (InputService.vKeyboardKeys)
                            {
                                InputService.vKeyboardKeys.Remove((uint)kItem.Key);
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                CleanUpVKeys();
            }
        }

        private void KeyboardButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            releaseKey(sender);
        }
        private void KeyboardButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            releaseKey(sender);
        }
        private void KeyboardButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            releaseKey(sender, true);
        }

        private void Esc_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            CleanUpVKeys();
        }

        private void Esc_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            CleanUpVKeys();
        }


        private void SlidersDialogHandler(object sender, EventArgs args)
        {
            SlidersDialog();
        }
        double SX = 1;
        double SY = 1;
        double PX = 0;
        double PY = 0;
        private async void UpdatePanelSize()
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        VM.SX = SX;
                        VM.SY = SY;
                        VM.PX = PX;
                        VM.PY = PY;
                        VM.updatePanelSize();
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
            {

            }
        }

        public static Dictionary<string, int[]> aspects = new Dictionary<string, int[]>()
                {
                    {"Auto", new int[]{0,0}},
                    {"4:3", new int[]{4,3}},
                    {"16:9", new int[]{16,9}},
                    {"16:10", new int[]{16,10}},
                    {"21:9", new int[]{21,9}},
                    {"31:9", new int[]{31,10}},
                };
        public async void SlidersDialog()
        {
            try
            {
                var diaog = new ContentDialog() { Title = "Screen Scale", SecondaryButtonText = "Close" };
                ScrollViewer scrollViewer = new ScrollViewer();
                StackPanel SlidersContainer = new StackPanel();
                var currentHeight = Window.Current.CoreWindow.Bounds.Height;
                var currentWidth = Window.Current.CoreWindow.Bounds.Width;
                TextBlock ScaleXText = new TextBlock();
                Slider ScaleX = new Slider();
                ScaleX.Minimum = -100;
                ScaleX.Maximum = 300;
                ScaleX.StepFrequency = 0.01;
                ScaleX.HorizontalAlignment = HorizontalAlignment.Stretch;
                ScaleX.Margin = new Thickness(5);
                ScaleX.Value = SX * 100;
                ScaleXText.Text = $"Scale (X): {ScaleX.Value}%";
                ScaleX.ValueChanged += (sender, e) =>
                {
                    try
                    {
                        SX = e.NewValue / 100;
                        ScaleXText.Text = $"Scale (X): {e.NewValue}%";
                        UpdatePanelSize();
                    }
                    catch (Exception ex)
                    {

                    }
                };

                TextBlock ScaleYText = new TextBlock();
                Slider ScaleY = new Slider();
                ScaleY.Minimum = -100;
                ScaleY.Maximum = 300;
                ScaleY.StepFrequency = 0.01;
                ScaleY.HorizontalAlignment = HorizontalAlignment.Stretch;
                ScaleY.Margin = new Thickness(5);
                ScaleY.Value = SY * 100;
                ScaleYText.Text = $"Scale (Y): {ScaleY.Value}%";
                ScaleY.ValueChanged += (sender, e) =>
                {
                    try
                    {
                        SY = e.NewValue / 100;
                        ScaleYText.Text = $"Scale (Y): {e.NewValue}%";
                        UpdatePanelSize();
                    }
                    catch (Exception ex)
                    {

                    }
                };

                TextBlock OffsetXText = new TextBlock();
                Slider PositionX = new Slider();
                PositionX.Minimum = -currentWidth;
                PositionX.Maximum = currentWidth;
                PositionX.StepFrequency = 0.01;
                PositionX.HorizontalAlignment = HorizontalAlignment.Stretch;
                PositionX.Margin = new Thickness(5);
                PositionX.Value = PX;
                OffsetXText.Text = $"Offset (X): {PositionX.Value}";
                PositionX.ValueChanged += (sender, e) =>
                {
                    try
                    {
                        PX = e.NewValue;
                        OffsetXText.Text = $"Offset (X): {e.NewValue}";
                        UpdatePanelSize();
                    }
                    catch (Exception ex)
                    {
                    }
                };

                TextBlock OffsetYText = new TextBlock();
                Slider PositionY = new Slider();
                PositionY.Minimum = -currentHeight;
                PositionY.Maximum = currentHeight;
                PositionY.StepFrequency = 0.01;
                PositionY.HorizontalAlignment = HorizontalAlignment.Stretch;
                PositionY.Margin = new Thickness(5);
                PositionY.Value = PY;
                OffsetYText.Text = $"Offset (Y): {PositionY.Value}";
                PositionY.ValueChanged += (sender, e) =>
                {
                    try
                    {
                        PY = e.NewValue;
                        OffsetYText.Text = $"Offset (Y): {e.NewValue}";
                        UpdatePanelSize();
                    }
                    catch (Exception ex)
                    {

                    }
                };

                bool saveForThisGameOnly = false;

                ComboBox aspectCombo = new ComboBox();
                ComboBox AIDevices = new ComboBox();
                bool AIDevicesList = false;
                AIDevices.Header = "AI Device";
                AIDevices.HorizontalAlignment = HorizontalAlignment.Stretch;
                AIDevices.Margin = new Thickness(0, 0, 0, 10);
                if (UISkillExecutionDevices != null && UISkillExecutionDevices.Count > 0)
                {
                    AIDevicesList = true;
                }

                bool changedByReset = false;
                Button ShowUIScaleDialog = new Button();
                ShowUIScaleDialog.Content = "UI Scale (Testing)";
                ShowUIScaleDialog.HorizontalAlignment = HorizontalAlignment.Stretch;
                ShowUIScaleDialog.Click += (sender, e) =>
                {
                    try
                    {
                        diaog.Hide();
                        LayoutScaler(false, VM);
                    }
                    catch (Exception ex)
                    {

                    }
                };
                Button ResetAll = new Button();
                ResetAll.Content = "Reset All";
                ResetAll.Click += (sender, e) =>
                {
                    try
                    {
                        ScaleX.Value = 100;
                        ScaleY.Value = 100;
                        PositionX.Value = 0;
                        PositionY.Value = 0;
                        selectedDevice = 0;
                        changedByReset = true;

                        ASR = new int[] { 0, 0 };
                        aspectCombo.SelectedIndex = 0;
                        if (AIDevicesList)
                        {
                            AIDevices.SelectedIndex = selectedDevice;
                        }
                        ResolveCanvasSize(true);
                        //PlatformService.PlayNotificationSound("success");
                        //PlatformService.ShowNotificationDirect($"{VM.CoreName}'s Customizations cleared");
                    }
                    catch (Exception ex)
                    {

                    }
                };
                ResetAll.HorizontalAlignment = HorizontalAlignment.Stretch;

                TextBlock notice = new TextBlock();
                notice.HorizontalAlignment = HorizontalAlignment.Stretch;
                notice.Foreground = new SolidColorBrush(Colors.Orange);
                notice.Text = "If you didn't save, changes will be lost";
                notice.FontSize = 12;
                notice.Margin = new Thickness(3, 5, 0, 0);

                Border border1 = new Border();
                border1.BorderThickness = new Thickness(0, 1, 0, 0);
                border1.BorderBrush = new SolidColorBrush(Colors.Gray);

                Border border2 = new Border();
                border2.BorderThickness = new Thickness(0, 1, 0, 0);
                border2.BorderBrush = new SolidColorBrush(Colors.Gray);
                border2.Margin = new Thickness(0, 0, 0, 10);

                Border border3 = new Border();
                border3.BorderThickness = new Thickness(0, 1, 0, 0);
                border3.BorderBrush = new SolidColorBrush(Colors.Gray);
                border3.Margin = new Thickness(0, 0, 0, 10);

                var gameOnlyValue = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{VM.EmulationService.GetGameID()}Custom", false); ;

                //Aspect ratio options

                var PKey = $"{VM.CoreName}";
                if (saveForThisGameOnly)
                {
                    try
                    {
                        PKey = $"{VM.EmulationService.GetGameID()}";
                    }
                    catch (Exception ex)
                    {

                    }
                }

                var tempAspect = ASR;
                foreach (var aItem in aspects)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = aItem.Key;
                    comboBoxItem.Tag = aItem.Key;
                    aspectCombo.Items.Add(comboBoxItem);
                    if (ASR[0] == aItem.Value[0] && ASR[1] == aItem.Value[1])
                    {
                        aspectCombo.SelectedItem = comboBoxItem;
                    }
                }
                aspectCombo.Header = "Aspect Ratio";
                if (!PlatformService.AutoFitResolver)
                {
                    aspectCombo.Header = "Aspect Ratio (AutoFit Disabled!)";
                }
                aspectCombo.HorizontalAlignment = HorizontalAlignment.Stretch;
                aspectCombo.Margin = new Thickness(0, 0, 0, 3);
                aspectCombo.SelectionChanged += (s, e) =>
                {
                    try
                    {
                        try
                        {
                            if (!changedByReset)
                            {
                                ASR = aspects[((ComboBoxItem)aspectCombo.SelectedItem).Content.ToString()];
                                ResolveCanvasSize(true);
                            }
                            changedByReset = false;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    catch (Exception ex)
                    {

                    }
                };


                if (AIDevicesList)
                {
                    int index = 0;
                    foreach (var aItem in UISkillExecutionDevices)
                    {
                        ComboBoxItem comboBoxItem = new ComboBoxItem();
                        comboBoxItem.Content = aItem;
                        AIDevices.Items.Add(comboBoxItem);
                        if (index == selectedDevice)
                        {
                            AIDevices.SelectedIndex = index;
                            AIGridText.Text = UISkillExecutionDevices[selectedDevice].ToUpper();
                        }
                        index++;
                    }
                    AIDevices.SelectionChanged += async (s, e) =>
                    {
                        try
                        {
                            selectedDevice = AIDevices.SelectedIndex;
                            AIGridText.Text = UISkillExecutionDevices[selectedDevice].ToUpper();
                            if (isUpscaleActive)
                            {
                                isUpscaleActive = false;
                                upscaleTasksCancel.Cancel();
                                await Task.Delay(50);
                                isUpscaleActive = true;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    };
                }
                else
                {
                    aspectCombo.Margin = new Thickness(0, 0, 0, 10);
                }


                CheckBox customGameScale = new CheckBox();
                customGameScale.Content = "This game only";
                customGameScale.IsChecked = gameOnlyValue;
                customGameScale.Click += (s, e) =>
                {
                    saveForThisGameOnly = customGameScale.IsChecked.Value;
                };

                SlidersContainer.Children.Add(ScaleXText);
                SlidersContainer.Children.Add(ScaleX);
                SlidersContainer.Children.Add(ScaleYText);
                SlidersContainer.Children.Add(ScaleY);
                SlidersContainer.Children.Add(OffsetXText);
                SlidersContainer.Children.Add(PositionX);
                SlidersContainer.Children.Add(OffsetYText);
                SlidersContainer.Children.Add(PositionY);
                SlidersContainer.Children.Add(aspectCombo);
                if (AIDevicesList)
                {
                    SlidersContainer.Children.Add(AIDevices);
                }
                SlidersContainer.Children.Add(border1);
                SlidersContainer.Children.Add(customGameScale);
                SlidersContainer.Children.Add(border2);
                SlidersContainer.Children.Add(ResetAll);
                SlidersContainer.Children.Add(border3);
                SlidersContainer.Children.Add(ShowUIScaleDialog);

                SlidersContainer.Children.Add(notice);

                scrollViewer.Content = SlidersContainer;
                diaog.Content = scrollViewer;

                diaog.PrimaryButtonText = "Save";
                diaog.IsPrimaryButtonEnabled = true;
                PlatformService.ScaleMenuActive = true;

                var result = await diaog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    PKey = $"{VM.CoreName}";
                    if (saveForThisGameOnly)
                    {
                        try
                        {
                            PKey = $"{VM.EmulationService.GetGameID()}";
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{VM.EmulationService.GetGameID()}Custom", true);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{VM.EmulationService.GetGameID()}Custom", false);
                    }
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}SCALEX", SX);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}SCALEY", SY);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}OFFSETX", PX);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}OFFSETY", PY);
                    if (AIDevicesList)
                    {
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}AIDEVICE", AIDevices.SelectedIndex);
                    }
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}ASPECT", ((ComboBoxItem)aspectCombo.SelectedItem).Content.ToString());

                    PlatformService.PlayNotificationSound("success");
                    if (saveForThisGameOnly)
                    {
                        PlatformService.ShowNotificationDirect($"Customizations saved for this game");
                    }
                    else
                    {
                        PlatformService.ShowNotificationDirect($"Customizations saved for ({VM.CoreName})");
                    }
                }
                else
                {
                    //ASR = tempAspect;
                }
            }
            catch (Exception ex)
            {

            }
            PlatformService.ScaleMenuActive = false;
        }




        public static double LSX = 1;
        public static double LSY = 1;
        public static double LPX = 0;
        public static double LPY = 0;

        public static async void UpdateUIScale()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
            {
                try
                {
                    Window.Current.Content.RenderTransform = new ScaleTransform
                    {
                        ScaleX = LSX,
                        ScaleY = LSY,
                        CenterX = LPX,
                        CenterY = LPY
                    };

                }
                catch (Exception ex)
                {

                }
            });
        }
        public static async void LayoutScaler(bool calledFromMainPage = false, GamePlayerViewModel VM = null)
        {
            try
            {
                var diaog = new ContentDialog() { Title = "RetriXGold Scale", SecondaryButtonText = "Close" };
                ScrollViewer scrollViewer = new ScrollViewer();
                StackPanel SlidersContainer = new StackPanel();
                var currentHeight = Window.Current.CoreWindow.Bounds.Height;
                var currentWidth = Window.Current.CoreWindow.Bounds.Width;

                TextBlock ScaleXText = new TextBlock();
                Slider ScaleX = new Slider();
                ScaleX.Minimum = -100;
                ScaleX.Maximum = 300;
                ScaleX.StepFrequency = 0.01;
                ScaleX.HorizontalAlignment = HorizontalAlignment.Stretch;
                ScaleX.Margin = new Thickness(5);
                ScaleX.Value = LSX * 100;
                ScaleXText.Text = $"Scale (X): {ScaleX.Value}%";
                ScaleX.ValueChanged += (sender, e) =>
                {
                    try
                    {
                        LSX = e.NewValue / 100;
                        ScaleXText.Text = $"Scale (X): {e.NewValue}%";
                        UpdateUIScale();
                    }
                    catch (Exception ex)
                    {

                    }
                };

                TextBlock ScaleYText = new TextBlock();
                Slider ScaleY = new Slider();
                ScaleY.Minimum = -100;
                ScaleY.Maximum = 300;
                ScaleY.StepFrequency = 0.01;
                ScaleY.HorizontalAlignment = HorizontalAlignment.Stretch;
                ScaleY.Margin = new Thickness(5);
                ScaleY.Value = LSY * 100;
                ScaleYText.Text = $"Scale (Y): {ScaleY.Value}%";
                ScaleY.ValueChanged += (sender, e) =>
                {
                    try
                    {
                        LSY = e.NewValue / 100;
                        ScaleYText.Text = $"Scale (Y): {e.NewValue}%";
                        UpdateUIScale();
                    }
                    catch (Exception ex)
                    {

                    }
                };

                TextBlock OffsetXText = new TextBlock();
                Slider PositionX = new Slider();

                PositionX.Minimum = 0;
                PositionX.Maximum = currentWidth;
                PositionX.StepFrequency = 0.01;
                PositionX.HorizontalAlignment = HorizontalAlignment.Stretch;
                PositionX.Margin = new Thickness(5);
                PositionX.Value = LPX;
                OffsetXText.Text = $"Center (X): {Math.Round(PositionX.Value)}";
                PositionX.ValueChanged += (sender, e) =>
                {
                    try
                    {
                        LPX = e.NewValue;
                        OffsetXText.Text = $"Center (X): {Math.Round(e.NewValue)}";
                        UpdateUIScale();
                    }
                    catch (Exception ex)
                    {
                    }
                };

                TextBlock OffsetYText = new TextBlock();
                Slider PositionY = new Slider();
                PositionY.Minimum = 0;
                PositionY.Maximum = currentHeight;
                PositionY.StepFrequency = 0.01;
                PositionY.HorizontalAlignment = HorizontalAlignment.Stretch;
                PositionY.Margin = new Thickness(5);
                PositionY.Value = LPY;
                OffsetYText.Text = $"Center (Y): {Math.Round(PositionY.Value)}";
                PositionY.ValueChanged += (sender, e) =>
                {
                    try
                    {
                        LPY = e.NewValue;
                        OffsetYText.Text = $"Center (Y): {Math.Round(e.NewValue)}";
                        UpdateUIScale();
                    }
                    catch (Exception ex)
                    {

                    }
                };


                Button ResetAll = new Button();
                ResetAll.Content = "Reset All";
                ResetAll.Click += (sender, e) =>
                {
                    try
                    {
                        ScaleX.Value = 100;
                        ScaleY.Value = 100;
                        PositionX.Value = currentWidth / 2;
                        PositionY.Value = currentHeight / 2;

                        UpdateUIScale();
                    }
                    catch (Exception ex)
                    {

                    }
                };
                ResetAll.HorizontalAlignment = HorizontalAlignment.Stretch;

                TextBlock notice = new TextBlock();
                notice.HorizontalAlignment = HorizontalAlignment.Stretch;
                notice.Foreground = new SolidColorBrush(Colors.Orange);
                notice.Text = "Changes cannot be saved for this feature";
                notice.FontSize = 12;
                notice.Margin = new Thickness(3, 5, 0, 0);

                Border border1 = new Border();
                border1.BorderThickness = new Thickness(0, 1, 0, 0);
                border1.BorderBrush = new SolidColorBrush(Colors.Gray);

                Border border2 = new Border();
                border2.BorderThickness = new Thickness(0, 1, 0, 0);
                border2.BorderBrush = new SolidColorBrush(Colors.Gray);
                border2.Margin = new Thickness(0, 0, 0, 10);


                SlidersContainer.Children.Add(ScaleXText);
                SlidersContainer.Children.Add(ScaleX);
                SlidersContainer.Children.Add(ScaleYText);
                SlidersContainer.Children.Add(ScaleY);
                SlidersContainer.Children.Add(OffsetXText);
                SlidersContainer.Children.Add(PositionX);
                SlidersContainer.Children.Add(OffsetYText);
                SlidersContainer.Children.Add(PositionY);
                SlidersContainer.Children.Add(border2);
                SlidersContainer.Children.Add(ResetAll);
                SlidersContainer.Children.Add(notice);

                scrollViewer.Content = SlidersContainer;
                diaog.Content = scrollViewer;

                diaog.PrimaryButtonText = calledFromMainPage? "Done" : "Menu";
                diaog.IsPrimaryButtonEnabled = true;
                PlatformService.ScaleMenuActive = true;

                var result = await diaog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    if(!calledFromMainPage && VM!=null)
                    {
                        VM.ToggleMenuGridActive();
                    }
                    /*var PKey = $"RETRIXGOLD";
                    if (!calledFromMainPage)
                    {
                        PKey = $"RETRIXGOLDINGAME";
                    }

                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}SCALEX", LSX);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}SCALEY", LSY);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}OFFSETX", LPX);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{PKey}OFFSETY", LPY);

                    PlatformService.PlayNotificationSound("success");
                    PlatformService.ShowNotificationDirect($"RetriXGold Scale saved");*/
                }
            }
            catch (Exception ex)
            {

            }
            PlatformService.ScaleMenuActive = false;
        }

        public static async void KeyboardRules(GamePlayerViewModel VM, bool fromMainPage = false)
        {
            PlatformService.KeyboardMapVisible = true;

            try
            {
                if (VM != null)
                {
                    VM.GameIsLoadingState(true);
                }
            }
            catch (Exception ex)
            {

            }
            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
            {
                try
                {
                    string CoreName = fromMainPage ? "all" : VM.CoreName;
                    string SystemName = fromMainPage ? "systems" : VM.SystemName;

                    ContentDialog contentDialog = new ContentDialog();
                    contentDialog.Title = "Keyboard Map";
                    contentDialog.PrimaryButtonText = "Save";
                    contentDialog.SecondaryButtonText = "Close";
                    contentDialog.IsPrimaryButtonEnabled = true;
                    contentDialog.IsSecondaryButtonEnabled = true;
                    var globalSave = fromMainPage;
                    Button ResetCurrent = new Button();
                    ResetCurrent.Content = "Reset Customizations";
                    ResetCurrent.HorizontalAlignment = HorizontalAlignment.Stretch;
                    StackPanel confirmReset = new StackPanel();

                    ResetCurrent.Click += async (s, e) =>
                    {

                        try
                        {
                            confirmReset.Visibility = Visibility.Visible;
                            ResetCurrent.Visibility = Visibility.Collapsed;
                        }
                        catch (Exception ex)
                        {

                        }

                    };
                    CheckBox globalSaveCheck = new CheckBox();
                    globalSaveCheck.Visibility = fromMainPage ? Visibility.Collapsed : Visibility.Visible;
                    globalSaveCheck.Content = "Global Customizations";

                    globalSaveCheck.Click += (s, e) =>
                    {
                        try
                        {
                            globalSave = globalSaveCheck.IsChecked.Value;
                        }
                        catch (Exception ex)
                        {

                        }
                    };


                    StackPanel stackPanel = new StackPanel();
                    stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

                    StackPanel ConfigBlock = new StackPanel();

                    //Build combos
                    bool buildiInProgress = true;
                    foreach (var kItem in InputService.KeyboardMapRulesList)
                    {
                        TextBlock textBlock = new TextBlock();

                        var buttontext = GetButtonsNameByKey(kItem.OriginalKey);
                        var Name = buttontext;
                        var TouchName = "";
                        var KeyboardName = "";
                        try
                        {
                            //Fetch touch info
                            var touchPattern = @"\(Touch:.*\)";
                            var tm = Regex.Match(Name, touchPattern);
                            if (tm.Success)
                            {
                                Name = Regex.Replace(Name, touchPattern, "");
                                TouchName = tm.Value;
                            }

                            //Fetch keyboard info
                            var keyboardPattern = @"\[Keyboard:.*\]";
                            var km = Regex.Match(Name, keyboardPattern);
                            if (km.Success)
                            {
                                Name = Regex.Replace(Name, keyboardPattern, "");
                                KeyboardName = km.Value;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        textBlock.Text = buttontext.Length > 0 ? $"{Name}" : "-";
                        textBlock.Margin = new Thickness(0, 3, 0, 3);
                        textBlock.FontWeight = FontWeights.Bold;
                        textBlock.Foreground = new SolidColorBrush(Colors.DodgerBlue);

                        ComboBox comboBox = new ComboBox();
                        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                        comboBox.Margin = new Thickness(0, 0, 0, 5);
                        comboBox.SelectionChanged += (sender, e) =>
                        {
                            if (buildiInProgress)
                            {
                                return;
                            }
                            try
                            {
                                var item = (string)e.AddedItems?.FirstOrDefault();
                                if (item != null)
                                {
                                    var OriginalKey = kItem.OriginalKey;
                                    var newKey = GetKeyByName(item);
                                    foreach (var sItem in InputService.KeyboardMapRulesList)
                                    {
                                        if (sItem.OriginalKey.Equals(OriginalKey))
                                        {
                                            sItem.NewKey = newKey;
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                PlatformService.ShowNotificationMain($"Error: {ex.Message}", 5);
                            }
                        };

                        //Fill combo with keys
                        comboBox.ItemsSource = PlatformService.KeyboardKeysStringMap.Values;
                        foreach (var cItem in comboBox.Items)
                        {
                            var testKey = GetKeyByName((string)cItem);
                            if (testKey.Equals(kItem.NewKey))
                            {
                                comboBox.SelectedItem = cItem;
                                break;
                            }
                        }
                        if (buttontext.Length == 0)
                        {
                            textBlock.Opacity = 0.65;
                            comboBox.Opacity = 0.65;
                        }
                        ConfigBlock.Children.Add(textBlock);
                        ConfigBlock.Children.Add(comboBox);
                    }
                    buildiInProgress = false;

                    ScrollViewer scrollViewer = new ScrollViewer();
                    scrollViewer.Content = ConfigBlock;
                    scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                    scrollViewer.MaxHeight = 300;
                    scrollViewer.Margin = new Thickness(3, 7, 0, 7);
                    scrollViewer.BorderThickness = new Thickness(0, 1, 0, 1);
                    scrollViewer.BorderBrush = new SolidColorBrush(Colors.Gray);

                    TextBlock notice = new TextBlock();
                    notice.HorizontalAlignment = HorizontalAlignment.Stretch;
                    notice.Foreground = new SolidColorBrush(Colors.Orange);
                    notice.Text = "If you didn't save, changes will be lost next startup";
                    notice.FontSize = 12;
                    notice.Margin = new Thickness(3, 5, 0, 0);


                    Border border = new Border();
                    border.BorderThickness = new Thickness(0, 0, 0, 1);
                    border.BorderBrush = new SolidColorBrush(Colors.Gray);
                    border.Margin = new Thickness(0, 3, 0, 5);

                    confirmReset.Orientation = Orientation.Horizontal;
                    confirmReset.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Button confirm = new Button();
                    confirm.Content = "Confirm";
                    confirm.Margin = new Thickness(0, 0, 3, 0);
                    confirm.HorizontalAlignment = HorizontalAlignment.Stretch;
                    confirm.Click += async (s, e) =>
                    {
                        try
                        {
                            if (globalSave)
                            {
                                await InputService.ResetKeyboardRules("all", "systems");
                            }
                            else
                            {
                                await InputService.ResetKeyboardRules(CoreName, SystemName);
                            }
                            PlatformService.PlayNotificationSound("success");
                            if (globalSave)
                            {
                                PlatformService.ShowNotificationDirect($"Global customizations reseted");
                            }
                            else
                            {
                                PlatformService.ShowNotificationDirect($"Customizations reseted for ({VM.CoreName})");
                            }
                            contentDialog.Hide();
                        }
                        catch (Exception ex)
                        {
                            PlatformService.ShowErrorMessage(ex);
                        }
                    };

                    Button cancel = new Button();
                    cancel.Content = "Cancel";
                    cancel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    cancel.Click += (s, e) =>
                    {
                        try
                        {
                            confirmReset.Visibility = Visibility.Collapsed;
                            ResetCurrent.Visibility = Visibility.Visible;
                        }
                        catch (Exception ex)
                        {

                        }
                    };

                    confirmReset.Children.Add(confirm);
                    confirmReset.Children.Add(cancel);
                    confirmReset.Visibility = Visibility.Collapsed;

                    stackPanel.Children.Add(scrollViewer);
                    stackPanel.Children.Add(globalSaveCheck);
                    stackPanel.Children.Add(border);
                    stackPanel.Children.Add(ResetCurrent);
                    stackPanel.Children.Add(confirmReset);
                    stackPanel.Children.Add(notice);


                    contentDialog.Content = stackPanel;

                    var result = await contentDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        PlatformService.PlayNotificationSound("success");
                        if (!globalSave)
                        {
                            await InputService.SaveKeyboardRules(CoreName, SystemName);
                            PlatformService.ShowNotificationDirect($"Customizations saved for ({VM.CoreName})");
                        }
                        else
                        {
                            await InputService.SaveKeyboardRules("all", "systems");
                            PlatformService.ShowNotificationDirect($"Global customizations saved");
                        }
                    }
                }
                catch (Exception ex)
                {
                    PlatformService.ShowErrorMessage(ex);
                }
                try
                {
                    if (VM != null)
                    {
                        VM.GameIsLoadingState(false);
                    }
                }
                catch (Exception ex)
                {

                }
                PlatformService.KeyboardMapVisible = false;
            });
        }
        public static string GetButtonsNameByKey(uint key)
        {
            try
            {
                foreach (var iItem in InputService.LibretroGamepadToKeyboardKeyMapping)
                {
                    if (key.Equals((uint)iItem.Value))
                    {
                        foreach (var ButtonItem in InputService.CurrentButtonsMaps[0])
                        {
                            if (iItem.Key.Equals((InputTypes)ButtonItem.Key))
                            {
                                return ButtonItem.Value;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return "";
        }
        public static uint GetKeyByName(string key)
        {
            try
            {
                foreach (var kmItem in PlatformService.KeyboardKeysStringMap)
                {
                    if (kmItem.Value.Equals(key))
                    {
                        return (uint)kmItem.Key;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        //Ports Map
        public static async void PortsMap(bool inGame = false)
        {
            try
            {
                PlatformService.ShortcutsVisible = true;
                ContentDialog contentDialog = new ContentDialog();
                contentDialog.Title = "Controllers Ports";
                contentDialog.PrimaryButtonText = "Save";
                contentDialog.SecondaryButtonText = "Close";
                contentDialog.IsPrimaryButtonEnabled = true;
                contentDialog.IsSecondaryButtonEnabled = true;
                StackPanel stackPanel = new StackPanel();
                ComboBox comboBox = new ComboBox();
                StackPanel KeysInfo = new StackPanel();
                KeysInfo.Visibility = Visibility.Collapsed;
                KeysInfo.Orientation = Orientation.Horizontal;
                KeysInfo.Margin = new Thickness(0, 5, 0, 0);
                TextBlock keysLabel = new TextBlock();
                keysLabel.Text = "Controllers:";
                keysLabel.Margin = new Thickness(0, 0, 3, 0);
                keysLabel.FontWeight = FontWeights.Bold;
                keysLabel.Foreground = new SolidColorBrush(Colors.DodgerBlue);
                TextBlock textBlock = new TextBlock();
                textBlock.FontSize = 12;
                textBlock.Margin = new Thickness(0, 2, 0, 0);
                textBlock.Text = "...";
                textBlock.TextWrapping = TextWrapping.WrapWholeWords;
                textBlock.HorizontalAlignment = HorizontalAlignment.Stretch;

                KeysInfo.Children.Add(keysLabel);
                KeysInfo.Children.Add(textBlock);

                StackPanel ConfigBlock = new StackPanel();
                ConfigBlock.Visibility = Visibility.Collapsed;
                ConfigBlock.Orientation = Orientation.Vertical;
                ConfigBlock.Padding = new Thickness(0, 5, 0, 5);
                ScrollViewer scrollViewer = new ScrollViewer();
                scrollViewer.Content = ConfigBlock;
                scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.MaxHeight = 300;
                scrollViewer.Margin = new Thickness(3, 7, 0, 7);
                comboBox.SelectionChanged += (sender, e) =>
                {
                    try
                    {
                        var item = (ComboBoxItem)e.AddedItems?.FirstOrDefault();
                        if (item != null)
                        {
                            var targetItem = (PortEntry)item.Tag;
                            KeysInfo.Visibility = Visibility.Visible;
                            ConfigBlock.Children.Clear();
                            ConfigBlock.Visibility = Visibility.Visible;
                            List<CheckBox> keys = new List<CheckBox>();
                            foreach (var gItem in InputService.ControllersList)
                            {
                                CheckBox checkBox = new CheckBox();
                                checkBox.Content = gItem.Key;
                                if (targetItem.controllers.Contains(gItem.Key))
                                {
                                    checkBox.IsChecked = true;
                                }
                                checkBox.Click += (senderc, ec) =>
                                {
                                    targetItem.controllers.Clear();
                                    foreach (var cItem in keys)
                                    {
                                        if (cItem.IsChecked.Value)
                                        {
                                            targetItem.controllers.Add(cItem.Content.ToString());
                                        }
                                    }
                                    if (targetItem.controllers.Count == 0)
                                    {
                                        textBlock.Text = "None";
                                    }
                                    else
                                    {
                                        textBlock.Text = String.Join(" + ", targetItem.controllers);
                                    }
                                };
                                keys.Add(checkBox);
                                ConfigBlock.Children.Add(checkBox);
                            }
                            if (targetItem.controllers.Count == 0)
                            {
                                textBlock.Text = "None";
                            }
                            else
                            {
                                textBlock.Text = String.Join(" + ", targetItem.controllers);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        PlatformService.ShowNotificationMain($"Error: {ex.Message}", 5);
                    }
                };
                stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                for (int i = 0; i < 12; i++)
                {
                    var pItem = InputService.ControllersPortsMap[i];
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = pItem.name;
                    comboBoxItem.Tag = pItem;
                    if (InputService.Descriptors[i].Count == 0)
                    {
                        comboBoxItem.Foreground = new SolidColorBrush(Colors.Gray);
                    }
                    comboBox.Items.Add(comboBoxItem);

                    if (pItem.port == 0)
                    {
                        comboBox.SelectedItem = comboBoxItem;
                    }
                }

                TextBlock notice = new TextBlock();
                notice.HorizontalAlignment = HorizontalAlignment.Stretch;
                notice.Foreground = new SolidColorBrush(Colors.Orange);
                notice.Text = "If you didn't save, changes will be lost next startup";
                notice.FontSize = 12;
                notice.Margin = new Thickness(3, 5, 0, 0);

                CheckBox OnlyForThisGame = new CheckBox();
                OnlyForThisGame.Content = "For this game (ONLY)";
                OnlyForThisGame.IsChecked = PortsLoadedForThisGame;
                OnlyForThisGame.Margin = new Thickness(0, 5, 0, 0);

                CheckBox OnlyForThisCore = new CheckBox();
                OnlyForThisCore.Content = "For this core (ONLY)";
                OnlyForThisCore.IsChecked = PortsLoadedForThisCore;
                OnlyForThisCore.Margin = new Thickness(0, 0, 0, 5);

                scrollViewer.BorderThickness = new Thickness(0, 1, 0, 1);
                scrollViewer.BorderBrush = new SolidColorBrush(Colors.Gray);

                stackPanel.Children.Add(comboBox);
                stackPanel.Children.Add(KeysInfo);
                stackPanel.Children.Add(scrollViewer);
                stackPanel.Children.Add(OnlyForThisGame);
                stackPanel.Children.Add(OnlyForThisCore);
                stackPanel.Children.Add(notice);
                contentDialog.Content = stackPanel;
                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    savePorts(OnlyForThisGame.IsChecked.Value, OnlyForThisCore.IsChecked.Value);
                    if (inGame)
                    {
                        PlatformService.ShowNotificationDirect("Ports map saved", 2);
                    }
                    else
                    {
                        PlatformService.ShowNotificationMain("Ports map saved", 2);
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
            PlatformService.ShortcutsVisible = false;
        }
        public static async void savePorts(bool customGame = false, bool customCore = false)
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync("RetriXGold Shortcuts", CreationCollisionOption.OpenIfExists);
                if (localFolder == null)
                {
                    PlatformService.ShowErrorMessage(new Exception("Unable to create shortcuts folder!"));
                    return;
                }
                if (!customGame)
                {
                    try
                    {
                        //To avoid too many buttons, when user select not to save for this game then I will delete the old custom ports (if exists)
                        var testFile = await localFolder.TryGetItemAsync($"goldports_{GameID}.rxp");
                        if (testFile != null)
                        {
                            await testFile.DeleteAsync();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    PortsLoadedForThisGame = false;
                }

                if (!customCore)
                {
                    try
                    {
                        //To avoid too many buttons, when user select not to save for this core then I will delete the old custom ports (if exists)
                        var testFile = await localFolder.TryGetItemAsync($"goldports_{CoreName}.rxp");
                        if (testFile != null)
                        {
                            await testFile.DeleteAsync();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    PortsLoadedForThisCore = false;
                }

                StorageFile targetFile = null;
                StorageFile targetCoreFile = null;
                if (customGame && GameID.Length > 0)
                {
                    targetFile = await localFolder.CreateFileAsync($"goldports_{GameID}.rxp", CreationCollisionOption.ReplaceExisting);
                    PortsLoadedForThisGame = true;
                }
                if (customCore && CoreName.Length > 0)
                {
                    targetCoreFile = await localFolder.CreateFileAsync($"goldports_{CoreName}.rxp", CreationCollisionOption.ReplaceExisting);
                    PortsLoadedForThisCore = true;
                }
                if (targetFile == null)
                {
                    targetFile = await localFolder.CreateFileAsync($"goldports.rxp", CreationCollisionOption.ReplaceExisting);
                }
                Encoding unicode = Encoding.Unicode;

                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(InputService.ControllersPortsMap));
                using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var outStream = stream.AsStream();
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }
                if (targetCoreFile != null)
                {
                    using (var stream = await targetCoreFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var outStream = stream.AsStream();
                        await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                        await outStream.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        static string GameID = "";
        static string CoreName = "";
        static bool PortsLoadedForThisGame = false;
        static bool PortsLoadedForThisCore = false;
        public static async Task loadPorts(string gameID = "", string coreName = "")
        {
            try
            {
                GameID = gameID == null ? "" : gameID;
                CoreName = coreName;

                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync("RetriXGold Shortcuts", CreationCollisionOption.OpenIfExists);
                if (localFolder == null)
                {
                    PlatformService.ShowErrorMessage(new Exception("Unable to create shortcuts folder!"));
                    return;
                }
                StorageFile targetFile = null;
                if (GameID.Length > 0)
                {
                    targetFile = (StorageFile)await localFolder.TryGetItemAsync($"goldports_{GameID}.rxp");
                    if (targetFile != null)
                    {
                        PortsLoadedForThisGame = true;
                    }
                }
                if (CoreName.Length > 0)
                {
                    var testFile = (StorageFile)await localFolder.TryGetItemAsync($"goldports_{CoreName}.rxp");
                    if (testFile != null)
                    {
                        PortsLoadedForThisCore = true;
                        if (targetFile == null)
                        {
                            targetFile = testFile;
                        }
                    }
                }
                if (targetFile == null)
                {
                    targetFile = (StorageFile)await localFolder.TryGetItemAsync($"goldports.rxp");
                }
                if (targetFile != null)
                {
                    Encoding unicode = Encoding.Unicode;
                    byte[] result;
                    using (var stream = await targetFile.OpenAsync(FileAccessMode.Read))
                    {
                        var outStream = stream.AsStream();
                        using (var memoryStream = new MemoryStream())
                        {
                            outStream.CopyTo(memoryStream);
                            result = memoryStream.ToArray();
                        }
                        await outStream.FlushAsync();
                    }
                    string OptionsFileContent = unicode.GetString(result);
                    var dictionaryList = JsonConvert.DeserializeObject<List<PortEntry>>(OptionsFileContent);
                    lock (InputService.ControllersPortsMap)
                    {
                        InputService.ControllersPortsMap = dictionaryList;
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        private void KeyboardPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        private void MainKeys_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
        }

        private void GamePadPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VM != null && (VM.isGameStarted || VM.FailedToLoadGame))
            {
                systemControlsChanged = true;
                InitialControlsMenu(null, null);
            }
        }

        private void ShowSavesList_Click(object sender, RoutedEventArgs e)
        {
            VM.ShowAllSaves();
        }

        private void mainGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (!PlatformService.isXBOXPure)
                {
                    if (PlatformService.isDPadKey(e.OriginalKey))
                    {
                        var tempState = PlatformService.DPadActive;
                        PlatformService.DPadActive = true;
                        if (tempState != PlatformService.DPadActive)
                        {
                            BindingsUpdate();
                            PlatformService.ShowNotificationMain("XBOX/DPAD Mode", 2);
                        }
                    }
                    else
                    {
                        var tempState = PlatformService.DPadActive;
                        PlatformService.DPadActive = false;
                        if (tempState != PlatformService.DPadActive)
                        {
                            BindingsUpdate();
                            PlatformService.ShowNotificationMain("Keyboard/Touch Mode", 2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        Visibility isPauseVisible
        {
            get
            {
                if (VM != null)
                {
                    if (VM.isGameStarted)
                    {
                        if (VM.DisplayTouchGamepad && (PlatformService.isMobile || PlatformService.isTablet))
                        {
                            return Visibility.Collapsed;
                        }
                        else
                        {
                            return Visibility.Visible;
                        }
                    }
                }
                return Visibility.Collapsed;
            }
        }

        ObservableCollection<CoresQuickListItem> dataSourceScreenshots = new ObservableCollection<CoresQuickListItem>();
        ObservableCollection<CoresQuickListItem> dataSourceRecents = new ObservableCollection<CoresQuickListItem>();
        HyperlinkButton textBlockScreenshots = new HyperlinkButton();
        HyperlinkButton textBlockRecents = new HyperlinkButton();
        private void SetActiveState(CoresQuickListItem coresQuickListItem, bool state, string active = "ENABLED", string disabled = "DISABLED")
        {
            try
            {
                if (!state)
                {
                    coresQuickListItem.BorderColor = new SolidColorBrush((Windows.UI.Color)Helpers.HexToColor("#66ffa500"));
                    coresQuickListItem.Tag = disabled;
                }
                else
                {
                    coresQuickListItem.BorderColor = new SolidColorBrush(Colors.Green);
                    coresQuickListItem.Tag = active;
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void GenerateMenu(Dictionary<string, CoresQuickListItem> itemSwitchs, string prefix, ObservableCollection<CoresQuickListItem> items, ScrollViewer scrollViewer, bool keepFirst = false, bool collapse = false, bool recall = false, bool inDialog =false)
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
                var rowsNumber = 1;

                //Get Rows Number
                List<string> tempList = new List<string>();
                foreach (var bItem in items)
                {
                    tempList.Add(bItem.Group.ToLower());
                }
                var tempListU = tempList.Distinct().ToList();
                rowsNumber = tempListU.Count();

                //Create Rows
                Grid grid = new Grid();
                for (int i = 0; i < rowsNumber; i++)
                {
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Height = new GridLength(1, GridUnitType.Auto);
                    grid.RowDefinitions.Add(rowDefinition);
                }

                bool updatedInProgress = false;
                if (scrollViewer.Tag == null)
                {
                    EventHandler updater = (sender, e) =>
                    {
                        updatedInProgress = true;
                        GenerateMenu(itemSwitchs, prefix, items, scrollViewer, keepFirst, collapse, true);
                        updatedInProgress = false;
                    };
                    items.CollectionChanged += (sender, e) =>
                    {
                        if (!updatedInProgress)
                        {
                            updater.Invoke(null, null);
                        }
                    };
                    scrollViewer.Tag = updater;
                }

                //Prepare Grids
                var indexer = 0;
                foreach (var group in tempListU)
                {
                    if (itemSwitchs != null && itemSwitchs.Keys.Contains(group))
                    {
                        if (!itemSwitchs[group].IsOn)
                        {
                            continue;
                        }
                    }
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    if (inDialog)
                    {
                        stackPanel.VerticalAlignment = VerticalAlignment.Center;
                        stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    }
                    var isRecents = group.ToLower().StartsWith("played");
                    var isScreenshot = group.ToLower().StartsWith("screenshot");
                    if (isRecents)
                    {
                        textBlockRecents = new HyperlinkButton();
                    }
                    else if (isScreenshot)
                    {
                        textBlockScreenshots = new HyperlinkButton();
                    }
                    HyperlinkButton textBlock = new HyperlinkButton();
                    (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Margin = new Thickness(20, 5, 15, 5);
                    (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).FontSize = 16;
                    (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).FontWeight = FontWeights.Bold;
                    if (PlatformService.isXBOX || PlatformService.DPadActive)
                    {
                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).HorizontalAlignment = HorizontalAlignment.Left;
                    }
                    else
                    {
                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).HorizontalAlignment = HorizontalAlignment.Stretch;
                    }
                    if (inDialog)
                    {
                        textBlockRecents.Visibility = Visibility.Collapsed;
                    }
                    Border border = new Border();
                    border.MaxWidth = PlatformService.InitWidthSize;
                    border.Width = InitWidthSizeCustom;
                    border.HorizontalAlignment = HorizontalAlignment.Left;
                    border.Margin = new Thickness(15, 5, 15, 5);
                    border.BorderThickness = new Thickness(1);
                    border.BorderBrush = new SolidColorBrush((Windows.UI.Color)Helpers.HexToColor("#AA304ffe"));
                    border.Opacity = 0.7;

                    GridView gridView = new GridView();

                    var dataSource = new ObservableCollection<CoresQuickListItem>();
                    string dataTemplate = "BrowseGridMenuItemTemplate";
                    if (isRecents)
                    {
                        dataSourceRecents = new ObservableCollection<CoresQuickListItem>();
                    }
                    else if (isScreenshot)
                    {
                        dataSourceScreenshots = new ObservableCollection<CoresQuickListItem>();
                    }
                    foreach (var aItem in items)
                    {
                        if (aItem.Group.ToLower().Equals(group.ToLower()))
                        {
                            {
                                (isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)).Add(aItem);
                            }
                            dataTemplate = aItem.DataTemplate;
                        }
                    }

                    UIElement customElement = null;

                    if ((isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)).FirstOrDefault().CustomControl != null)
                    {
                        customElement = (UIElement)(isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)).FirstOrDefault().CustomControl;
                    }
                    {
                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Tag = group;
                        var titleText = $"{group}";
                        if (isScreenshot || isRecents)
                        {
                            titleText = $"{titleText} ({(isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)).Count})";
                        }
                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Content = titleText.ToUpper();

                        var keyName = $"{scrollViewer.Name}_{group}_{prefix}";
                        if (customElement == null)
                        {
                            if ((isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)).FirstOrDefault().OnHoldEvent)
                            {
                                gridView.Holding += CoresQuickAccessList_ItemHold;
                                gridView.IsHoldingEnabled = true;
                            }
                            gridView.IsItemClickEnabled = true;
                            gridView.ItemClick += CoresQuickAccessList_ItemClick;
                            gridView.Tapped += CoresQuickAccessList_Tapped;
                            gridView.ItemTemplate = this.Resources[dataTemplate] as DataTemplate;
                            gridView.Style = this.Resources["RootListStyle"] as Style;
                            gridView.Margin = new Thickness(15, 5, 0, 0);
                            gridView.VerticalAlignment = VerticalAlignment.Stretch;
                            gridView.HorizontalAlignment = HorizontalAlignment.Stretch;
                            if (inDialog)
                            {
                                gridView.VerticalAlignment = VerticalAlignment.Center;
                                gridView.HorizontalAlignment = HorizontalAlignment.Center;
                            }
                            {
                                gridView.ItemsSource = (isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource));
                            }
                        }
                        EventHandler eventHandler = (sender, e) =>
                        {
                            try
                            {
                                if (customElement == null)
                                {
                                    if (gridView.Visibility == Visibility.Collapsed)
                                    {
                                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Opacity = 1;
                                        border.Opacity = 1;
                                        gridView.Visibility = Visibility.Visible;

                                        foreach (var iItem in (isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)))
                                        {
                                            ((dynamic)iItem).Collapsed = false;
                                        }
                                    }
                                    else
                                    {
                                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Opacity = 0.7;
                                        border.Opacity = 0.5;
                                        gridView.Visibility = Visibility.Collapsed;

                                        foreach (var iItem in (isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)))
                                        {
                                            ((dynamic)iItem).Collapsed = true;
                                        }

                                    }
                                    var stateValue = (gridView.Visibility == Visibility.Collapsed);
                                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(keyName, stateValue);
                                }
                                else
                                {
                                    if (customElement.Visibility == Visibility.Collapsed)
                                    {
                                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Opacity = 1;
                                        border.Opacity = 1;
                                        customElement.Visibility = Visibility.Visible;

                                        foreach (var iItem in (isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)))
                                        {
                                            ((dynamic)iItem).Collapsed = false;
                                        }
                                    }
                                    else
                                    {
                                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Opacity = 0.7;
                                        border.Opacity = 0.5;
                                        customElement.Visibility = Visibility.Collapsed;

                                        foreach (var iItem in (isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)))
                                        {
                                            ((dynamic)iItem).Collapsed = true;
                                        }

                                    }
                                    var stateValue = (customElement.Visibility == Visibility.Collapsed);
                                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(keyName, stateValue);
                                }
                            }
                            catch (Exception ex)
                            {
                                PlatformService.ShowErrorMessageDirect(ex);
                            }
                        };
                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Click += (sender, e) =>
                        {
                            if (!PlatformService.DeviceIsPhone())
                            {
                                eventHandler.Invoke(null, null);
                            }
                        };

                        (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Tapped += (sender, e) =>
                        {
                            if (PlatformService.DeviceIsPhone())
                            {
                                eventHandler.Invoke(null, null);
                            }
                        };

                        bool defaultCollapseState = false;
                        if (!inDialog)
                        {
                            if ((isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)).FirstOrDefault().Collapsed)
                            {
                                (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Opacity = 0.7;
                                border.Opacity = 0.5;
                                if (customElement == null)
                                {
                                    gridView.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    customElement.Visibility = Visibility.Collapsed;
                                }
                                defaultCollapseState = true;
                            }
                        }
                        if (!inDialog)
                        {
                            stackPanel.Children.Add((isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)));
                        }
                        if (collapse && !recall && !inDialog)
                        {
                            if (!keepFirst || indexer > 0)
                            {
                                if (customElement == null)
                                {
                                    gridView.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    customElement.Visibility = Visibility.Collapsed;
                                }
                                (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Opacity = 0.7;
                                border.Opacity = 0.5;
                                foreach (var iItem in (isRecents ? dataSourceRecents : (isScreenshot ? dataSourceScreenshots : dataSource)))
                                {
                                    iItem.Collapsed = true;
                                }
                                defaultCollapseState = true;
                            }
                        }
                        var groupCollapseState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault(keyName, defaultCollapseState);
                        if (groupCollapseState && !inDialog)
                        {
                            (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Opacity = 0.7;
                            border.Opacity = 0.5;
                            if (customElement == null)
                            {
                                gridView.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                customElement.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            (isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)).Opacity = 1;
                            border.Opacity = 1;
                            if (customElement == null)
                            {
                                gridView.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                customElement.Visibility = Visibility.Visible;
                            }
                        }
                        if (!inDialog)
                        {
                            stackPanel.Children.Add(border);
                        }
                        if (customElement == null)
                        {
                            stackPanel.Children.Add(gridView);
                        }
                        else
                        {
                            stackPanel.Children.Add(customElement);
                        }
                    }
                    grid.Children.Add(stackPanel);
                    Grid.SetRow(stackPanel, indexer);
                    indexer++;
                }
                scrollViewer.Content = grid;
            }
            catch (Exception ex)
            {

            }
            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }
        }

        private void CoresQuickAccessList_ItemHold(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                CoresQuickListItem SelectOption = null;
                string ObjectType = e.OriginalSource.GetType().Name;
                switch (ObjectType)
                {
                    case "Image":
                        SelectOption = (CoresQuickListItem)((Image)e.OriginalSource).DataContext;
                        break;

                    case "TextBlock":
                        SelectOption = (CoresQuickListItem)((TextBlock)e.OriginalSource).DataContext;
                        break;

                    case "Border":
                        SelectOption = (CoresQuickListItem)((Border)e.OriginalSource).DataContext;
                        break;

                    case "StackPanel":
                        SelectOption = (CoresQuickListItem)((StackPanel)e.OriginalSource).DataContext;
                        break;

                    case "Grid":
                        SelectOption = null;
                        break;

                    case "GridViewItemPresenter":
                        SelectOption = (CoresQuickListItem)((GridViewItemPresenter)e.OriginalSource).DataContext;
                        break;

                    case "ListViewItemPresenter":
                        SelectOption = (CoresQuickListItem)((ListViewItemPresenter)e.OriginalSource).DataContext;
                        break;

                    default:
                        SelectOption = (CoresQuickListItem)((GridViewItem)e.OriginalSource).DataContext;
                        break;
                }
                if (SelectOption == null)
                {
                    return;
                }
                switch (SelectOption?.Action)
                {
                    case "recent":
                        break;
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        bool isTappedTriggered = false;
        GridViewItem item;
        GridViewItem item2;
        GridViewItem item3;
        public string getAssetsIcon(string IconPath, string FolderName = "Menus")
        {
            return $"ms-appx:///Assets/{FolderName}/{IconPath}";
        }
        private CoresQuickListItem GenerateMenuItem(string group, string action, string title, string icon, bool eventHandler, string dataTemplate = "BrowseGridMenuItemTemplate")
        {
            var Icon = $"ms-appx:///Assets/Menus/{icon}.png";

            CoresQuickListItem coresQuickListItem = new CoresQuickListItem(group, action, title, Icon, eventHandler, dataTemplate);

            return coresQuickListItem;
        }
        private CoresQuickListItem GenerateMenuItem(string group, string action, string title, string icon, string descs = "", string tag = "", GameSystemViewModel core = null, string dataTemplate = "BrowseGridMenuItemTemplate")
        {
            var Icon = "";
            if (core != null)
            {
                Icon = core.Symbol;
            }
            else
            {
                Icon = $"ms-appx:///Assets/Menus/{icon}.png";
            }
            CoresQuickListItem coresQuickListItem = new CoresQuickListItem(group, action, title, Icon, descs, tag, core, dataTemplate);

            return coresQuickListItem;
        }
        private CoresQuickListItem GenerateCoreMenuItem(string group, string action, string title, string icon, string descs = "", string tag = "", GameSystemRecentModel recent = null, string dataTemplate = "BrowseGridMenuItemTemplate")
        {
            var Icon = "";
            if (recent != null)
            {
                Icon = recent.GameSnapshot;
            }
            else
            {
                Icon = $"ms-appx:///Assets/Menus/{icon}.png";
            }
            CoresQuickListItem coresQuickListItem = new CoresQuickListItem(group, action, title, Icon, descs, tag, recent, dataTemplate);

            return coresQuickListItem;
        }

        CancellationTokenSource screenshotsGetterCancellation = new CancellationTokenSource();
        ObservableCollection<CoresQuickListItem> InGameQuickMenu = new ObservableCollection<CoresQuickListItem>();
        private async void UpdateInGameMenuItems(object sender, EventArgs args)
        {
            try
            {
                foreach(var iItem in InGameQuickMenu)
                {
                    switch (iItem.Action)
                    {
                        case "pause":
                            SetActiveState(iItem, !VM.EmulationService.CorePaused, "RUNNING", "PAUSED");
                            break;
                        case "nativek":
                            SetActiveState(iItem, PlatformService.useNativeKeyboard);
                            break;
                        case "arcadec":
                            SetActiveState(iItem, VM.IsArcaeConfirm);
                            break;
                        case "audio":
                            if (VM.AudioNormalLevel)
                            {
                                iItem.Tag = "DEFAULT";
                            }
                            else if (VM.AudioHighLevel)
                            {
                                iItem.Tag = "HIGH";
                            }
                            else if (VM.AudioMediumLevel)
                            {
                                iItem.Tag = "MEDIUM";
                            }
                            else if (VM.AudioLowLevel)
                            {
                                iItem.Tag = "LOW";
                            }
                            else if (VM.AudioMuteLevel)
                            {
                                iItem.Tag = "MUTE";
                            }
                            break;
                        case "mousem":
                            SetActiveState(iItem, PlatformService.MouseSate);
                            break;
                        case "sensors":
                            SetActiveState(iItem, VM.SensorsMovement);
                            break;
                        case "touchpad":
                            SetActiveState(iItem, VM.ForceDisplayTouchGamepad || VM.TouchPadForce);
                            break;
                        case "fps":
                            SetActiveState(iItem, VM.ShowFPSCounter);
                            break;
                        case "fst":
                            SetActiveState(iItem, VM.FastForward);
                            break;
                        case "autosave":
                            SetActiveState(iItem, VM.AutoSave15Sec || VM.AutoSave30Sec || VM.AutoSave60Sec || VM.AutoSave90Sec || VM.AutoSave);
                            break;
                        case "cbuf":
                            SetActiveState(iItem, VM.CrazyBufferActive);
                            break;
                        case "skipframes":
                            SetActiveState(iItem, VM.SkipFrames || VM.SkipFramesRandom || VM.DelayFrames);
                            break;
                        case "threadswait":
                            if (VM.RCore1)
                            {
                                iItem.Tag = "1 THREAD";
                            }
                            else if (VM.RCore2)
                            {
                                iItem.Tag = "2 THREADS";
                            }
                            else if (VM.RCore4)
                            {
                                iItem.Tag = "4 THREADS";
                            }
                            else if (VM.RCore6)
                            {
                                iItem.Tag = "6 THREADS";
                            }
                            else if (VM.RCore8)
                            {
                                iItem.Tag = "8 THREADS";
                            }
                            else if (VM.RCore12)
                            {
                                iItem.Tag = "12 THREAD";
                            }
                            else if (VM.RCore20)
                            {
                                iItem.Tag = "20 THREAD";
                            }
                            else if (VM.RCore32)
                            {
                                iItem.Tag = "32 THREAD";
                            }
                            break;
                        case "render":
                            if (VM.NearestNeighbor)
                            {
                                iItem.Tag = "Nearest Neighbor".ToUpper();
                            }
                            else if (VM.Anisotropic)
                            {
                                iItem.Tag = "Anisotropic".ToUpper();
                            }
                            else if (VM.Cubic)
                            {
                                iItem.Tag = "Cubic".ToUpper();
                            }
                            else if (VM.HighQualityCubic)
                            {
                                iItem.Tag = "HQ Cubic".ToUpper();
                            }
                            else if (VM.Linear)
                            {
                                iItem.Tag = "Linear".ToUpper();
                            }
                            else if (VM.MultiSampleLinear)
                            {
                                iItem.Tag = "MultiSample Linear".ToUpper();
                            }
                            break;
                        case "antigc":
                            SetActiveState(iItem, VM.ReduceFreezes);
                            break;
                        case "aspeed":
                            if (VM.ActionsDelay1)
                            {
                                iItem.Tag = "FASTEST";
                            }
                            else if (VM.ActionsDelay2)
                            {
                                iItem.Tag = "FAST";
                            }
                            else if (VM.ActionsDelay3)
                            {
                                iItem.Tag = "NORMAL";
                            }
                            else if (VM.ActionsDelay4)
                            {
                                iItem.Tag = "SLOW";
                            }
                            else if (VM.ActionsDelay5)
                            {
                                iItem.Tag = "SLOWEST";
                            }
                            break;
                        case "effects":
                            await Task.Delay(500);
                            SetActiveState(iItem, VM.VideoService.TotalEffects() > 0);
                            break;
                        case "overlays":
                            _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, async () =>
                            {
                                try
                                {
                                    bool AddOverlays = false;
                                    var overLaysFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Overlays");
                                    if (overLaysFolder != null)
                                    {
                                        var files = await overLaysFolder.GetFilesAsync();
                                        if (files != null && files.Count > 0)
                                        {
                                            AddOverlays = true;
                                        }
                                    }
                                    SetActiveState(iItem, AddOverlays);
                                }
                                catch (Exception ex)
                                {

                                }
                            });
                            break;
                        case "shaders":
                            SetActiveState(iItem, VM.AddShaders);
                            break;
                        case "rotate":
                            SetActiveState(iItem, VM.RotateDegreePlusActive || VM.RotateDegreeMinusActive, VM.RotateDegreeMinusActive ? "LEFT" : "RIGHT");
                            break;
                        case "loglist":
                            SetActiveState(iItem, VM.ShowLogsList, "OPENED", "CLOSED");
                            break;
                        case "vssetings":
                            SetActiveState(iItem, VM.AVDebug);
                            break;
                        case "skipcache":
                            SetActiveState(iItem, VM.SkipCached);
                            break;
                        case "vfsinfo":
                            SetActiveState(iItem, VM.DisplayVFSDebug);
                            break;
                        case "memory":
                            if (VM.isMemoryHelpersVisible)
                            {
                                if (VM.BufferCopyMemory)
                                {
                                    iItem.Tag = "BUFFER";
                                }
                                else if (VM.memCPYMemory)
                                {
                                    iItem.Tag = "MEMCPY";
                                }
                                else if (VM.MarshalMemory)
                                {
                                    iItem.Tag = "MARSHAL";
                                }
                                else if (VM.SpanlMemory)
                                {
                                    iItem.Tag = "SPAN";
                                }
                            }
                            else
                            {
                                iItem.Tag = "POINTERS";
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void buildInGameMenu()
        {
            try
            {

                //Game
                var PauseCoreAction = GenerateMenuItem("Game", "pause", "State", "play");
                SetActiveState(PauseCoreAction, !VM.EmulationService.CorePaused, "RUNNING", "PAUSED");
                InGameQuickMenu.Add(PauseCoreAction);

                {
                    if (PlatformService.KeyboardEvent != null || PlatformService.CoreReadingRetroKeyboard)
                    {
                        var CoreAction = GenerateMenuItem("Game", "nativek", "Native Keybaord", "keyboard", "", PlatformService.useNativeKeyboard ? "ENABLED" : "DISABLED");
                        SetActiveState(CoreAction, PlatformService.useNativeKeyboard);
                        InGameQuickMenu.Add(CoreAction);
                    }
                }

                {
                    if (VM.ShowArcaeConfirm)
                    {
                        var CoreAction = GenerateMenuItem("Game", "arcadec", "Arcade Name", "iso", "", VM.IsArcaeConfirm ? "CONFIRMED" : "NOT CONFIRMED");
                        SetActiveState(CoreAction, VM.IsArcaeConfirm);
                        InGameQuickMenu.Add(CoreAction);
                    }
                }

                var QSaveCoreAction = GenerateMenuItem("Game", "quicksave", "Save", "quicksave", "", "QUICK");
                InGameQuickMenu.Add(QSaveCoreAction);

                var QLoadCoreAction = GenerateMenuItem("Game", "quickload", "Load", "lnk", "", "QUICK");
                InGameQuickMenu.Add(QLoadCoreAction);

                var ScreenShotCoreAction = GenerateMenuItem("Game", "snapshot", "Screenshot", "camera");
                InGameQuickMenu.Add(ScreenShotCoreAction);

                var AudioCoreAction = GenerateMenuItem("Game", "audio", "Audio", "high");
                if (VM.AudioNormalLevel)
                {
                    AudioCoreAction.Tag = "DEFAULT";
                }
                else if (VM.AudioHighLevel)
                {
                    AudioCoreAction.Tag = "HIGH";
                }
                else if (VM.AudioMediumLevel)
                {
                    AudioCoreAction.Tag = "MEDIUM";
                }
                else if (VM.AudioLowLevel)
                {
                    AudioCoreAction.Tag = "LOW";
                }
                else if (VM.AudioMuteLevel)
                {
                    AudioCoreAction.Tag = "MUTE";
                }
                InGameQuickMenu.Add(AudioCoreAction);


                //Controls
                {
                    var CoreAction = GenerateMenuItem("Controls", "controls", "Gamepad", "xbox", "", "CUSTOMIZATION");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    if (InputService.isKeyboardAvailable())
                    {
                        var CoreAction = GenerateMenuItem("Controls", "keybad", "Keyboard", "xbox", "", "CUSTOMIZATION");
                        CoreAction.Collapsed = true;
                        InGameQuickMenu.Add(CoreAction);
                    }
                }

                {
                    var CoreAction = GenerateMenuItem("Controls", "portsmap", "Ports", "xbox-map", "", "MANAGEMENT");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Controls", "shortcuts", "Shortcuts", "tools", "", "GAMEPAD");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Controls", "mousem", "Mouse", "mouse", "", PlatformService.MouseSate ? "ENABLED" : "DISABLED");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, PlatformService.MouseSate);
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    if (PlatformService.KeyboardEvent != null || PlatformService.CoreReadingRetroKeyboard)
                    {
                        var CoreAction = GenerateMenuItem("Controls", "kyp", "Keyboard", "keyboard", "","ON-SCREEN");
                        CoreAction.Collapsed = true;
                        InGameQuickMenu.Add(CoreAction);
                    }
                }

                {
                    if (VM.SensorsMovementActive)
                    {
                        var CoreAction = GenerateMenuItem("Controls", "sensors", "Sensors", "sensors", "", VM.SensorsMovement ? "ENABLED" : "DISABLED");
                        CoreAction.Collapsed = true;
                        SetActiveState(CoreAction, VM.SensorsMovement);
                        InGameQuickMenu.Add(CoreAction);
                    }
                }


                //Touch
                if (InputService.isTouchAvailable() && !PlatformService.isXBOXPure)
                {
                    {
                        var CoreAction = GenerateMenuItem("Touch", "touchpad", "Touchpad", "savenstop");
                        CoreAction.Collapsed = true;
                        SetActiveState(CoreAction, VM.ForceDisplayTouchGamepad || VM.TouchPadForce);
                        InGameQuickMenu.Add(CoreAction);
                    }
                    {
                        var CoreAction = GenerateMenuItem("Touch", "profiletouch", "Profile", "fav","","SUGGESTED");
                        CoreAction.Collapsed = true;
                        InGameQuickMenu.Add(CoreAction);
                    }
                    {
                        var CoreAction = GenerateMenuItem("Touch", "customtouch", "Customization", "paint");
                        CoreAction.Collapsed = true;
                        InGameQuickMenu.Add(CoreAction);
                    }
                    {
                        var CoreAction = GenerateMenuItem("Touch", "swapc", "Swap Controls", "remote");
                        CoreAction.Collapsed = true;
                        InGameQuickMenu.Add(CoreAction);
                    }
                }


                //Core
                {
                    if (VM.InGameOptionsActive)
                    {
                        var CoreAction = GenerateMenuItem("Core", "coreoptions", "Options", "core");
                        CoreAction.Collapsed = true;
                        InGameQuickMenu.Add(CoreAction);
                    }
                }

                {
                    var CoreAction = GenerateMenuItem("Core", "fps", "FPS", "fps", "", VM.ShowFPSCounter ? "ENABLED" : "DISABLED");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.ShowFPSCounter);
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Core", "fst", "FastForward", "fst", "", VM.FastForward ? "ENABLED" : "DISABLED");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.FastForward);
                    InGameQuickMenu.Add(CoreAction);
                }
                
                {
                    var CoreAction = GenerateMenuItem("Core", "chelp", "Help", "help","","LIBRETRO");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }


                //Performance

                {
                    await Task.Delay(500);
                    if (VM.isCrazyBufferVisible)
                    {
                        var CoreAction = GenerateMenuItem("Performance", "cbuf", "CBuffer", "cbuf", "", VM.CrazyBufferActive ? "ENABLED" : "DISABLED");
                        CoreAction.Collapsed = true;
                        SetActiveState(CoreAction, VM.CrazyBufferActive);
                        InGameQuickMenu.Add(CoreAction);
                    }
                }

                {
                    var CoreAction = GenerateMenuItem("Performance", "skipframes", "Skip", "skipframes");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.SkipFrames || VM.SkipFramesRandom || VM.DelayFrames);
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Performance", "threadswait", "Threads", "threadswait");
                    CoreAction.Collapsed = true;
                    if (VM.RCore1)
                    {
                        CoreAction.Tag = "1 THREAD";
                    }
                    else if (VM.RCore2)
                    {
                        CoreAction.Tag = "2 THREADS";
                    }
                    else if (VM.RCore4)
                    {
                        CoreAction.Tag = "4 THREADS";
                    }
                    else if (VM.RCore6)
                    {
                        CoreAction.Tag = "6 THREADS";
                    }
                    else if (VM.RCore8)
                    {
                        CoreAction.Tag = "8 THREADS";
                    }
                    else if (VM.RCore12)
                    {
                        CoreAction.Tag = "12 THREAD";
                    }
                    else if (VM.RCore20)
                    {
                        CoreAction.Tag = "20 THREAD";
                    }
                    else if (VM.RCore32)
                    {
                        CoreAction.Tag = "32 THREAD";
                    }
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Performance", "render", "Render", "uni-theme");
                    CoreAction.Collapsed = true;
                    if (VM.NearestNeighbor)
                    {
                        CoreAction.Tag = "Nearest Neighbor".ToUpper();
                    }
                    else if (VM.Anisotropic)
                    {
                        CoreAction.Tag = "Anisotropic".ToUpper();
                    }
                    else if (VM.Cubic)
                    {
                        CoreAction.Tag = "Cubic".ToUpper();
                    }
                    else if (VM.HighQualityCubic)
                    {
                        CoreAction.Tag = "HQ Cubic".ToUpper();
                    }
                    else if (VM.Linear)
                    {
                        CoreAction.Tag = "Linear".ToUpper();
                    }
                    else if (VM.MultiSampleLinear)
                    {
                        CoreAction.Tag = "MultiSample Linear".ToUpper();
                    }
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Performance", "antigc", "Anti GC", "threadsnone", "", VM.ReduceFreezes ? "ENABLED" : "DISABLED");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.ReduceFreezes);
                    InGameQuickMenu.Add(CoreAction);
                }

                //Effects

                {
                    var CoreAction = GenerateMenuItem("Effects", "effects", "Settings", "sepia");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.VideoService.TotalEffects() > 0);
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Effects", "overlays", "Overlay", "none");
                    CoreAction.Collapsed = true;
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            bool AddOverlays = false;
                            var overLaysFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Overlays");
                            if (overLaysFolder != null)
                            {
                                var files = await overLaysFolder.GetFilesAsync();
                                if (files != null && files.Count > 0)
                                {
                                    AddOverlays = true;
                                }
                            }
                            SetActiveState(CoreAction, AddOverlays);
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Effects", "shaders", "Shader", "retro", "", "TESTING ONLY");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.AddShaders);
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Effects", "rotate", "Rotate", "right");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.RotateDegreePlusActive || VM.RotateDegreeMinusActive, VM.RotateDegreeMinusActive ? "LEFT" : "RIGHT");
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Effects", "scale", "Scale", "app-record");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }


                //Saves
                {
                    var CoreAction = GenerateMenuItem("Saves", "saves", "States", "saves");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Saves", "autosave", "Auto Save", "timer30");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.AutoSave15Sec || VM.AutoSave30Sec || VM.AutoSave60Sec || VM.AutoSave90Sec || VM.AutoSave);
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Saves", "savemanage", "Manage", "share");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }



                //Actions

                {
                    var CoreAction = GenerateMenuItem("Actions", "actions1", "Action 1", "actions1");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Actions", "actions2", "Action 2", "actions2");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Actions", "actions3", "Action 3", "actions3");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Actions", "aspeed", "Settings", "wutc");
                    if (VM.ActionsDelay1)
                    {
                        CoreAction.Tag = "FASTEST";
                    }
                    else if (VM.ActionsDelay2)
                    {
                        CoreAction.Tag = "FAST";
                    }
                    else if (VM.ActionsDelay3)
                    {
                        CoreAction.Tag = "NORMAL";
                    }
                    else if (VM.ActionsDelay4)
                    {
                        CoreAction.Tag = "SLOW";
                    }
                    else if (VM.ActionsDelay5)
                    {
                        CoreAction.Tag = "SLOWEST";
                    }
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Actions", "achelp", "Help", "help");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }

                
                //Debug

                {
                    var CoreAction = GenerateMenuItem("Debug", "loglist", "Logs", "logs");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.ShowLogsList, "OPENED", "CLOSED");
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Debug", "vssetings", "Visualizer", "performance", "", "AUDIO");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.AVDebug);
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Debug", "skipcache", "Pixels Updates", "dim", "", "PIXELS");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.SkipCached);
                    InGameQuickMenu.Add(CoreAction);
                }

                {
                    var CoreAction = GenerateMenuItem("Debug", "vfsinfo", "VFS OSD", "notepad", "", "");
                    CoreAction.Collapsed = true;
                    SetActiveState(CoreAction, VM.DisplayVFSDebug);
                    InGameQuickMenu.Add(CoreAction);
                }

                {

                    {
                        var CoreAction = GenerateMenuItem("Debug", "memory", "Memory", "memory", "", "HELPERS");
                        CoreAction.Collapsed = true;
                        if (VM.isMemoryHelpersVisible)
                        {
                            if (VM.BufferCopyMemory)
                            {
                                CoreAction.Tag = "BUFFER";
                            }
                            else if (VM.memCPYMemory)
                            {
                                CoreAction.Tag = "MEMCPY";
                            }
                            else if (VM.MarshalMemory)
                            {
                                CoreAction.Tag = "MARSHAL";
                            }
                            else if (VM.SpanlMemory)
                            {
                                CoreAction.Tag = "SPAN";
                            }
                        }
                        else
                        {
                            CoreAction.Tag = "POINTERS";
                        }
                        InGameQuickMenu.Add(CoreAction);
                    }
                }

                {
                    var CoreAction = GenerateMenuItem("Debug", "mored", "More", "tools");
                    CoreAction.Collapsed = true;
                    InGameQuickMenu.Add(CoreAction);
                }


                GenerateMenu(null, "", InGameQuickMenu, CoresQuickAccessContainer);
            }
            catch (Exception ex)
            {

            }
        }

        bool isInGameDialogVisible = false;
        private async void ShowInGameDialog(string action)
        {
            try
            {
                VM.HideMenuGrid();
                var targetElements = InGameQuickMenu.Where(a => a.Action.Equals(action)).ToList();
                var targetElement = targetElements.FirstOrDefault();

                ObservableCollection<CoresQuickListItem> customItems = new ObservableCollection<CoresQuickListItem>();
                foreach (var item in targetElements)
                {
                    item.Collapsed = false;
                    customItems.Add(item);
                }
                ScrollViewer scrollViewer = new ScrollViewer();
                GenerateMenu(null, "", customItems, scrollViewer,false,false,false,true);

                ContentDialog contentDialog = new ContentDialog();
                contentDialog.Title = targetElement.Title;
                contentDialog.PrimaryButtonText = "Menu";
                contentDialog.SecondaryButtonText = "Close";
                contentDialog.IsPrimaryButtonEnabled = true;
                contentDialog.IsSecondaryButtonEnabled = true;
                contentDialog.Content = scrollViewer;
                isInGameDialogVisible = true;
                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    VM.ToggleMenuGridActive();
                }
            }
            catch(Exception ex)
            {

            }
            isInGameDialogVisible = false;
        }
        private async void OverlayModes()
        {
            try
            {
                var array = Enum.GetValues(typeof(BlendEffectMode));
                ComboBox blendModes = new ComboBox();
                blendModes.HorizontalAlignment = HorizontalAlignment.Stretch;

                ComboBoxItem noBlend = new ComboBoxItem();
                noBlend.Content = "None";
                noBlend.Name = "ToggleNone";
                blendModes.Items.Add(noBlend);
                blendModes.SelectedItem = noBlend;
                foreach (var bItem in array)
                {
                    ComboBoxItem blendMode = new ComboBoxItem();
                    blendMode.Content = bItem.ToString();
                    blendMode.Name = "Toggle" + bItem.ToString();
                    blendMode.Tag = bItem;
                    blendModes.Items.Add(blendMode);
                    if (PlatformService.BlendEffectModeGlobal != -1)
                    {
                        if ((int)bItem == PlatformService.BlendEffectModeGlobal)
                        {
                            blendModes.SelectedItem = blendMode;
                        }
                    }
                }
                blendModes.SelectionChanged += (s, e) =>
                {
                    try
                    {
                        var selectedItem = (ComboBoxItem)blendModes.SelectedItem;
                        switch (selectedItem.Name)
                        {
                            case "ToggleNone":
                                PlatformService.BlendEffectModeGlobal = -1;
                                PlatformService.isBlendModeSet = false;
                                break;

                            default:
                                PlatformService.BlendEffectModeGlobal = (int)(BlendEffectMode)selectedItem.Tag;
                                PlatformService.isBlendModeSet = true;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                };

                ToggleSwitch FitOverlay = new ToggleSwitch();
                FitOverlay.Header = "Fit overlay";
                FitOverlay.OnContent = "Active";
                FitOverlay.OffContent = "Disabled";
                FitOverlay.IsOn = PlatformService.FitOverlay;
                FitOverlay.HorizontalAlignment = HorizontalAlignment.Stretch;
                FitOverlay.Margin = new Thickness(0, 5, 0, 3);
                FitOverlay.Toggled += (s, e) =>
                {
                    try
                    {
                        PlatformService.FitOverlay = FitOverlay.IsOn;
                    }
                    catch (Exception ex)
                    {

                    }
                };

                StackPanel overlaySettings = new StackPanel();
                overlaySettings.Children.Add(blendModes);
                overlaySettings.Children.Add(FitOverlay);

                ContentDialog contentDialog = new ContentDialog();
                contentDialog.Title = "Blend Modes";
                contentDialog.PrimaryButtonText = "Menu";
                contentDialog.SecondaryButtonText = "Close";
                contentDialog.IsPrimaryButtonEnabled = true;
                contentDialog.IsSecondaryButtonEnabled = true;
                contentDialog.Content = overlaySettings;
                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    VM.ToggleMenuGridActive();
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            GamePlayerView.PortsMap(true);
        }

        private void MenuFlyoutItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (PlatformService.UpdateButtonsMap != null)
            {
                PlatformService.UpdateButtonsMap.Invoke("", null);
            }
        }

        private void MenuFlyoutItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (PlatformService.UpdateButtonsMap != null)
            {
                PlatformService.UpdateButtonsMap.Invoke("#######", null);
            }
        }

        private void KeyboardRules_Click(object sender, RoutedEventArgs e)
        {
            GamePlayerView.KeyboardRules(VM);
        }
        private async void CoresQuickAccessList_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                await Task.Delay(10);
                if (isTappedTriggered)
                {
                    isTappedTriggered = false;
                    return;
                }
                CoresQuickListItem SelectOption = (CoresQuickListItem)e.ClickedItem;
                UIElement targetElement = (UIElement)sender;
                var dataItems = SelectOption.Recent != null ? ((GridView)sender).Items.Where(item => ((CoresQuickListItem)item).Recent.GameLocation == SelectOption.Recent.GameLocation) : ((GridView)sender).Items.Where(item => ((CoresQuickListItem)item).Title == SelectOption.Title);
                var point = new Windows.Foundation.Point(0, 0);

                if (dataItems != null && dataItems.Count() > 0)
                {
                    if (PlatformService.SubPageActive)
                    {
                        item2 = ((GridView)sender).ContainerFromItem(dataItems.FirstOrDefault()) as GridViewItem;
                        targetElement = item2;
                        item3 = null;
                    }
                    else
                    {
                        item = ((GridView)sender).ContainerFromItem(dataItems.FirstOrDefault()) as GridViewItem;
                        targetElement = item;
                        item2 = null;
                        item3 = null;
                    }
                    point = new Windows.Foundation.Point(5, 5);
                }
                if (SelectOption == null)
                {
                    return;
                }
                await ListEventGeneral_Tapped(SelectOption, point, targetElement);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        CoresQuickListItem GlobalSelectedItem = null;
        
        private async Task ListEventGeneral_Tapped(CoresQuickListItem SelectOption, Windows.Foundation.Point point, UIElement targetElement)
        {
            try
            {
                GlobalSelectedItem = null;
                bool HideMenu = false;
                if (SelectOption == null)
                {
                    return;
                }
                Dictionary<string, CoresQuickListItem> itemsSwitchs = new Dictionary<string, CoresQuickListItem>();

                switch (SelectOption.Action)
                {
                    case "controls":
                        VM.SetControlsMapVisible.Execute(null);
                        VM.HideMenuGrid();
                        break;

                    case "portsmap":
                        GamePlayerView.PortsMap(true);
                        break;

                    case "keybad":
                        GamePlayerView.KeyboardRules(VM);
                        break;

                    case "sensors":
                        VM.SetSensorsMovement.Execute(null);
                        SetActiveState(SelectOption, VM.SensorsMovement);
                        break;

                    case "mousem":
                        PlatformService.MouseSate = !PlatformService.MouseSate;
                        if (PlatformService.DPadActive && PlatformService.MouseSate)
                        {
                            PlatformService.ShowNotificationDirect("A: Right click, B: Left click, Analog: Move", 4);
                        }
                        else if (PlatformService.MouseSate)
                        {
                            PlatformService.ShowNotificationDirect("Gamepad (ONLY)\nA: Right click, B: Left click, Analog: Move", 4);
                        }
                        SetActiveState(SelectOption, PlatformService.MouseSate);
                        break;

                    case "coreoptions":
                        VM.SetCoreOptionsVisible.Execute(null);
                        VM.HideMenuGrid();
                        break;

                    case "scale":
                        if (PlatformService.SlidersDialogHandler != null)
                        {
                            PlatformService.SlidersDialogHandler.Invoke(null, null);
                            VM.HideMenuGrid();
                        }
                        break;

                    case "pause":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, VM.EmulationService.CorePaused ? "Resume" : "Pause", async (s, e) =>
                            {
                                try
                                {
                                    SelectOption.ProgressState = Visibility.Visible;
                                    VM.TogglePauseCommand.Execute(null);
                                    await Task.Delay(1000);
                                    SetActiveState(SelectOption, !VM.EmulationService.CorePaused, "RUNNING", "PAUSED");
                                }
                                catch (Exception ex)
                                {

                                }
                                SelectOption.ProgressState = Visibility.Collapsed;
                            }, VM.EmulationService.CorePaused ? Colors.Green : Colors.Gold);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Reset", async (s, e) =>
                            {
                                try
                                {
                                    SelectOption.ProgressState = Visibility.Visible;
                                    await VM.Reset();
                                    await Task.Delay(1000);
                                    SetActiveState(SelectOption, !VM.EmulationService.CorePaused, "RUNNING", "PAUSED");
                                }
                                catch (Exception ex)
                                {

                                }
                                SelectOption.ProgressState = Visibility.Collapsed;
                            }, Colors.Gray);
                           
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {

                            });
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Stop", (s, e) =>
                            {
                                try
                                {
                                    VM.StopCommand.Execute(null);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Tomato);

                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;

                    case "stop":
                        VM.StopCommand.Execute(null);
                        break;

                    case "quicksave":
                        await VM.QuickSaveState();
                        VM.HideMenuGrid();
                        break;

                    case "quickload":
                        VM.QuickLoadState();
                        VM.HideMenuGrid();
                        break;

                    case "snapshot":
                        VM.SaveSnapshot();
                        VM.HideMenuGrid();
                        break;

                    case "dimcache":
                        VM.SetUpdatesOnly.Execute(null);
                        VM.HideMenuGrid();
                        //SetActiveState(SelectOption, VM.SkipCached);
                        break;

                    case "shortcuts":
                        GameSystemSelectionView.Shortcuts(VM, true);
                        break;
                    case "skipcache":
                        VM.SetSkipCached.Execute(null);
                        SetActiveState(SelectOption, VM.SkipCached);
                        break;

                    case "skipframes":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.SkipFrames, "50% Skip", (s, e) =>
                            {
                                try
                                {
                                    VM.SetSkipFrames.Execute(null);
                                    SetActiveState(SelectOption, VM.SkipFrames || VM.SkipFramesRandom || VM.DelayFrames);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.SkipFramesRandom, "Random Skip", (s, e) =>
                            {
                                try
                                {
                                    VM.SetSkipFramesRandom.Execute(null);
                                    SetActiveState(SelectOption, VM.SkipFrames || VM.SkipFramesRandom || VM.DelayFrames);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {

                            });
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.DelayFrames, "Audio Skip", (s, e) =>
                            {
                                try
                                {
                                    VM.SetDelayFrames.Execute(null);
                                    SetActiveState(SelectOption, VM.SkipFrames || VM.SkipFramesRandom || VM.DelayFrames);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.DodgerBlue);

                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;


                    case "threadswait":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                            if (!isInGameDialogVisible)
                            {
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Edit + Preview", (s, e) =>
                                {
                                    try
                                    {
                                        ShowInGameDialog(SelectOption.Action);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gray);

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                {

                                });
                            }
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RCore1, "1 Thread", (s, e) =>
                            {
                                try
                                {
                                    VM.RCore1 = true;
                                    VM.SetRCore.Execute(null);
                                    SelectOption.Tag = "1 THREAD";
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Green);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RCore2, "2 Threads", (s, e) =>
                            {
                                try
                                {
                                    VM.RCore2 = true;
                                    VM.SetRCore.Execute(null);
                                    SelectOption.Tag = "2 THREADS";
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RCore4, "4 Threads", (s, e) =>
                            {
                                try
                                {
                                    VM.RCore4 = true;
                                    VM.SetRCore.Execute(null);
                                    SelectOption.Tag = "4 THREADS";
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RCore6, "6 Threads", (s, e) =>
                            {
                                try
                                {
                                    VM.RCore6 = true;
                                    VM.SetRCore.Execute(null);
                                    SelectOption.Tag = "6 THREADS";
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RCore8, "8 Threads", (s, e) =>
                            {
                                try
                                {
                                    VM.RCore8 = true;
                                    VM.SetRCore.Execute(null);
                                    SelectOption.Tag = "8 THREADS";
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {

                            });
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RCore12, "12 Thread", (s, e) =>
                            {
                                try
                                {
                                    VM.RCore12 = true;
                                    VM.SetRCore.Execute(null);
                                    SelectOption.Tag = "12 THREAD";
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RCore20, "20 Thread", (s, e) =>
                            {
                                try
                                {
                                    VM.RCore20 = true;
                                    VM.SetRCore.Execute(null);
                                    SelectOption.Tag = "20 THREAD";
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RCore32, "32 Thread", (s, e) =>
                            {
                                try
                                {
                                    VM.RCore32 = true;
                                    VM.SetRCore.Execute(null);
                                    SelectOption.Tag = "32 THREAD";
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {

                            });
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, !FramebufferConverter.DontWaitThreads, "Wait Threads", (s, e) =>
                            {
                                try
                                {
                                    VM.DontWaitThreads.Execute(null);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.DodgerBlue);

                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;
                    case "threadsnone":
                        VM.RCore1 = true;
                        VM.SetRCore.Execute(null);
                        break;

                    case "nativek":
                        PlatformService.useNativeKeyboard = !PlatformService.useNativeKeyboard;
                        SetActiveState(SelectOption, PlatformService.useNativeKeyboard);
                        break;

                    case "arcadec":
                        VM.ConfimeArcadeGame.Execute(null);
                        SetActiveState(SelectOption, VM.IsArcaeConfirm);
                        break;

                    case "threads2":
                        VM.RCore2 = true;
                        VM.SetRCore.Execute(null);
                        break;
                    case "threads4":
                        VM.RCore4 = true;
                        VM.SetRCore.Execute(null);
                        break;
                    case "threads8":
                        VM.RCore8 = true;
                        VM.SetRCore.Execute(null);
                        break;

                    case "actions1":
                        VM.ActionsGridVisible(true, 1);
                        VM.HideMenuGrid();
                        break;
                    case "actions2":
                        VM.ActionsGridVisible(true, 2);
                        VM.HideMenuGrid();
                        break;
                    case "actions3":
                        VM.ActionsGridVisible(true, 3);
                        VM.HideMenuGrid();
                        break;

                    case "aspeed":
                        {
                            {
                                MenuFlyout coreSettingsMenu = new MenuFlyout();
                                List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                                List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ActionsDelay1, "Fastest", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetActionsDelay1.Execute(null);
                                        SelectOption.Tag = "FASTEST";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Tomato);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ActionsDelay2, "Fast", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetActionsDelay2.Execute(null);
                                        SelectOption.Tag = "FAST";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Orange);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ActionsDelay3, "Normal", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetActionsDelay3.Execute(null);
                                        SelectOption.Tag = "NORMAL";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.DodgerBlue);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ActionsDelay4, "Slow", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetActionsDelay4.Execute(null);
                                        SelectOption.Tag = "SLOW";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gold);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ActionsDelay5, "Slowest", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetActionsDelay5.Execute(null);
                                        SelectOption.Tag = "SLOWEST";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gray);

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                {

                                }, Colors.Gray);

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Export", (s, e) =>
                                {
                                    try
                                    {
                                        VM.ExportActionsSlots.Execute(null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Import", (s, e) =>
                                {
                                    try
                                    {
                                        VM.ImportActionsSlots.Execute(null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                foreach (var mItem in coresSettingsMenu)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                foreach (var mItem in coresSettingsMenuNormal)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                coreSettingsMenu.ShowAt(targetElement, point);
                            }
                        }
                        break;

                    case "savemanage":
                        {
                            {
                                MenuFlyout coreSettingsMenu = new MenuFlyout();
                                List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                                List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Export", (s, e) =>
                                {
                                    try
                                    {
                                        VM.ExportSavedSlots.Execute(null);
                                        VM.HideMenuGrid();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Import", (s, e) =>
                                {
                                    try
                                    {
                                        VM.ImportSavedSlots.Execute(null);
                                        VM.HideMenuGrid();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                foreach (var mItem in coresSettingsMenu)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                foreach (var mItem in coresSettingsMenuNormal)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                coreSettingsMenu.ShowAt(targetElement, point);
                            }
                        }
                        break;

                    case "touchpad":
                        {
                            {
                                MenuFlyout coreSettingsMenu = new MenuFlyout();
                                List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                                List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                                if (!isInGameDialogVisible)
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Edit + Preview", (s, e) =>
                                    {
                                        try
                                        {
                                            ShowInGameDialog(SelectOption.Action);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, Colors.Gray);

                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                    {

                                    });
                                }
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ForceDisplayTouchGamepad, "Touchpad", (s, e) =>
                                {
                                    try
                                    {
                                        VM.ForceDisplayTouchGamepad = !VM.ForceDisplayTouchGamepad;
                                        SetActiveState(SelectOption, VM.ForceDisplayTouchGamepad || VM.TouchPadForce);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.TouchPadForce, "Touchpad (Force)", (s, e) =>
                                {
                                    try
                                    {
                                        VM.TouchPadForce = !VM.TouchPadForce;
                                        SetActiveState(SelectOption, VM.ForceDisplayTouchGamepad || VM.TouchPadForce);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                {
                                });

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.UseAnalogDirections, "Analog as (+)", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetUseAnalogDirections.Execute(null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.FitScreenState, "Fit Screen", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetScreenFit.Execute(null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                foreach (var mItem in coresSettingsMenu)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                foreach (var mItem in coresSettingsMenuNormal)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                coreSettingsMenu.ShowAt(targetElement, point);
                            }
                        }
                        break;

                    case "profiletouch":
                        {
                            {
                                MenuFlyout coreSettingsMenu = new MenuFlyout();
                                List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                                List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                                if (!isInGameDialogVisible)
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Edit + Preview", (s, e) =>
                                    {
                                        try
                                        {
                                            ShowInGameDialog(SelectOption.Action);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, Colors.Gray);

                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                    {

                                    });
                                }
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Suggested", (s, e) =>
                                {
                                    try
                                    {
                                        if (PlatformService.UpdateButtonsMap != null)
                                        {
                                            PlatformService.UpdateButtonsMap.Invoke("", null);
                                            SelectOption.Tag = "SUGGESTED";
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Default", (s, e) =>
                                {
                                    try
                                    {
                                        if (PlatformService.UpdateButtonsMap != null)
                                        {
                                            PlatformService.UpdateButtonsMap.Invoke("#######", null);
                                            SelectOption.Tag = "DEFAULT";
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                foreach (var mItem in coresSettingsMenu)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                foreach (var mItem in coresSettingsMenuNormal)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                coreSettingsMenu.ShowAt(targetElement, point);
                            }
                        }
                        break;

                    case "swapc":
                        VM.CallSwapControls.Execute(null);
                        PlatformService.ShowNotificationDirect("Controls swapped successfully", 3);
                        break;
                    case "customtouch":
                        {
                            {
                                MenuFlyout coreSettingsMenu = new MenuFlyout();
                                List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                                List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ScaleFactorVisible, "Scale", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetScaleFactorVisible.Execute(null);
                                        VM.HideMenuGrid();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.CustomConsoleEditMode, "Scale (Current)", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetSetCustomConsoleEditMode.Execute(null);
                                        VM.HideMenuGrid();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ButtonsCustomization, "Position (R.Buttons)", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetButtonsCustomization.Execute(null);
                                        VM.HideMenuGrid();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                {
                                });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Reset", (s, e) =>
                                {
                                    try
                                    {
                                        VM.ResetAdjustments.Execute(null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Orange);

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                {
                                });

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.TabSoundEffect, "Tab SFX", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetTabSoundEffect.Execute(null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                                if (VM.IsSegaSystem)
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ShowXYZ, "Show XYZ", (s, e) =>
                                    {
                                        try
                                        {
                                            VM.SetShowXYZ.Execute(null);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    });
                                }
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ShowSpecialButtons, "Special Buttons", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetShowSpecialButtons.Execute(null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ShowActionsButtons, "Actions Buttons", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetShowActionsButtons.Execute(null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });

                                foreach (var mItem in coresSettingsMenu)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                foreach (var mItem in coresSettingsMenuNormal)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                coreSettingsMenu.ShowAt(targetElement, point);
                            }
                        }
                        break;

                    case "speed1":
                        VM.SetActionsDelay2.Execute(null);
                        break;
                    case "speed2":
                        VM.SetActionsDelay3.Execute(null);
                        break;
                    case "speed3":
                        VM.SetActionsDelay4.Execute(null);
                        break;

                    case "saves":
                        VM.ShowSavesList.Execute(null);
                        VM.HideMenuGrid();
                        break;
                    case "kyp":
                        PlatformService.ShowKeyboardHandler?.Invoke(null, null);
                        VM.HideMenuGrid();
                        break;

                    case "autosave":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AutoSave15Sec, "Every 15 Sec", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAutoSave15Sec.Execute(null);
                                    SetActiveState(SelectOption, VM.AutoSave15Sec || VM.AutoSave30Sec || VM.AutoSave60Sec || VM.AutoSave90Sec || VM.AutoSave);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AutoSave30Sec, "Every 30 Sec", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAutoSave30Sec.Execute(null);
                                    SetActiveState(SelectOption, VM.AutoSave15Sec || VM.AutoSave30Sec || VM.AutoSave60Sec || VM.AutoSave90Sec || VM.AutoSave);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AutoSave60Sec, "Every 60 Sec", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAutoSave60Sec.Execute(null);
                                    SetActiveState(SelectOption, VM.AutoSave15Sec || VM.AutoSave30Sec || VM.AutoSave60Sec || VM.AutoSave90Sec || VM.AutoSave);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AutoSave90Sec, "Every 90 Sec", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAutoSave90Sec.Execute(null);
                                    SetActiveState(SelectOption, VM.AutoSave15Sec || VM.AutoSave30Sec || VM.AutoSave60Sec || VM.AutoSave90Sec || VM.AutoSave);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {

                            });
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AutoSave, "On Stop", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAutoSave.Execute(null);
                                    SetActiveState(SelectOption, VM.AutoSave15Sec || VM.AutoSave30Sec || VM.AutoSave60Sec || VM.AutoSave90Sec || VM.AutoSave);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.DodgerBlue);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {

                            });
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AutoSaveNotify, "Notification", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAutoSaveNotify.Execute(null);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);

                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;

                    case "asave30":
                        VM.SetAutoSave30Sec.Execute(null);
                        break;
                    case "asave1":
                        VM.SetAutoSave60Sec.Execute(null);
                        break;
                    case "asave15":
                        VM.SetAutoSave90Sec.Execute(null);
                        break;
                    case "savestop":
                        VM.SetAutoSave.Execute(null);
                        break;
                    case "asaven":
                        VM.SetAutoSaveNotify.Execute(null);
                        break;

                    case "rotate":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                            if (!isInGameDialogVisible)
                            {
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Edit + Preview", (s, e) =>
                                {
                                    try
                                    {
                                        ShowInGameDialog(SelectOption.Action);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gray);

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                {

                                });
                            }
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RotateDegreePlusActive, "Right (90deg)", (s, e) =>
                            {
                                try
                                {
                                    VM.SetRotateDegreePlus.Execute(null);
                                    SetActiveState(SelectOption, VM.RotateDegreePlusActive || VM.RotateDegreeMinusActive, VM.RotateDegreeMinusActive ? "LEFT" : "RIGHT");
                                }
                                catch (Exception ex)
                                {

                                }
                            });

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.RotateDegreeMinusActive, "Left (-90deg)", (s, e) =>
                            {
                                try
                                {
                                    VM.SetRotateDegreeMinus.Execute(null);
                                    SetActiveState(SelectOption, VM.RotateDegreePlusActive || VM.RotateDegreeMinusActive, VM.RotateDegreeMinusActive ? "LEFT" : "RIGHT");
                                }
                                catch (Exception ex)
                                {

                                }
                            });


                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;

                    case "rright":
                        VM.SetRotateDegreePlus.Execute(null);
                        break;
                    case "rleft":
                        VM.SetRotateDegreeMinus.Execute(null);
                        break;

                    case "fps":
                        VM.ShowFPSCounterCommand.Execute(null);
                        SetActiveState(SelectOption, VM.ShowFPSCounter);
                        break;

                    case "upscale":
                        GamePlayerView.isUpscaleActive = !GamePlayerView.isUpscaleActive;
                        break;

                    case "chelp":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();

                            GenerateMenuFlyoutItem(ref coresSettingsMenuNormal, "Online", async(s, e) =>
                            {
                                try
                                {
                                    try
                                    {
                                        var coreName = GameSystemSelectionView.ResolveCoreName2(VM.CoreNameClean);
                                        if (Helpers.CheckInternetConnection())
                                        {
                                            SelectOption.ProgressState = Visibility.Visible;
                                            var resolvedName = GameSystemSelectionView.ResolveCoreName(coreName);
                                            var coreLibretroLink = resolvedName.StartsWith("https://") ? resolvedName : $"https://docs.libretro.com/library/{resolvedName}";
                                            webViewGuides.Navigate(new Uri(coreLibretroLink));
                                            await Task.Delay(1500);
                                            VM.WebViewGuidesVisible = Visibility.Visible;
                                        }
                                        else
                                        {
                                            {
                                                PlatformService.ShowNotificationDirect("No Internet connection", 3);
                                            }
                                        }
                                    }
                                    catch (Exception dx)
                                    {

                                    }
                                    SelectOption.ProgressState = Visibility.Collapsed;
                                }
                                catch (Exception ex)
                                {
                                    SelectOption.ProgressState = Visibility.Collapsed;
                                }
                            }, Colors.Green);


                            GenerateMenuFlyoutItem(ref coresSettingsMenuNormal, "Offline", async (s, e) =>
                            {
                                try
                                {
                                    try
                                    {
                                        var coreName = GameSystemSelectionView.ResolveCoreName(VM.CoreNameClean);
                                        {

                                            if (!coreName.Contains("github"))
                                            {
                                                SelectOption.ProgressState = Visibility.Visible;
                                                webViewGuides.Navigate(new Uri($"ms-appx-web:///Assets/Guides/{coreName}..html"));
                                                await Task.Delay(1500);
                                                VM.WebViewGuidesVisible = Visibility.Visible;
                                            }
                                            else
                                            {
                                                PlatformService.ShowNotificationDirect("No Guide attached for this core", 3);
                                            }
                                        }
                                    }
                                    catch (Exception dx)
                                    {

                                    }
                                    SelectOption.ProgressState = Visibility.Collapsed;
                                }
                                catch (Exception ex)
                                {
                                    SelectOption.ProgressState = Visibility.Collapsed;
                                }
                            }, Colors.Gold);


                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;

                    case "fst":
                        VM.FastForward = !VM.FastForward;
                        SetActiveState(SelectOption, VM.FastForward);
                        break;

                    case "cbuf":
                        VM.SetCrazyBufferActive.Execute(null);
                        SetActiveState(SelectOption, VM.CrazyBufferActive);
                        break;

                    case "audio":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();

                            

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AudioEcho, "Echo", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAudioEcho.Execute(null);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.ForestGreen);


                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AudioReverb, "Reverb", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAudioReverb.Execute(null);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.ForestGreen);


                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {
                            }, Colors.Green);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AudioNormalLevel, "Default", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAudioLevelNormal.Execute(null);
                                    if (VM.AudioNormalLevel)
                                    {
                                        SelectOption.Tag = "DEFAULT";
                                    }
                                    else if (VM.AudioHighLevel)
                                    {
                                        SelectOption.Tag = "HIGH";
                                    }
                                    else if (VM.AudioMediumLevel)
                                    {
                                        SelectOption.Tag = "MEDIUM";
                                    }
                                    else if (VM.AudioLowLevel)
                                    {
                                        SelectOption.Tag = "LOW";
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Green);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AudioHighLevel, "High", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAudioLevelHigh.Execute(null);
                                    if (VM.AudioNormalLevel)
                                    {
                                        SelectOption.Tag = "DEFAULT";
                                    }
                                    else if (VM.AudioHighLevel)
                                    {
                                        SelectOption.Tag = "HIGH";
                                    }
                                    else if (VM.AudioMediumLevel)
                                    {
                                        SelectOption.Tag = "MEDIUM";
                                    }
                                    else if (VM.AudioLowLevel)
                                    {
                                        SelectOption.Tag = "LOW";
                                    }
                                    else if (VM.AudioMuteLevel)
                                    {
                                        SelectOption.Tag = "MUTE";
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Orange);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AudioMediumLevel, "Medium", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAudioMediumLevel.Execute(null);
                                    if (VM.AudioNormalLevel)
                                    {
                                        SelectOption.Tag = "DEFAULT";
                                    }
                                    else if (VM.AudioHighLevel)
                                    {
                                        SelectOption.Tag = "HIGH";
                                    }
                                    else if (VM.AudioMediumLevel)
                                    {
                                        SelectOption.Tag = "MEDIUM";
                                    }
                                    else if (VM.AudioLowLevel)
                                    {
                                        SelectOption.Tag = "LOW";
                                    }
                                    else if (VM.AudioMuteLevel)
                                    {
                                        SelectOption.Tag = "MUTE";
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AudioLowLevel, "Low", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAudioLevelLow.Execute(null);
                                    if (VM.AudioNormalLevel)
                                    {
                                        SelectOption.Tag = "DEFAULT";
                                    }
                                    else if (VM.AudioHighLevel)
                                    {
                                        SelectOption.Tag = "HIGH";
                                    }
                                    else if (VM.AudioMediumLevel)
                                    {
                                        SelectOption.Tag = "MEDIUM";
                                    }
                                    else if (VM.AudioLowLevel)
                                    {
                                        SelectOption.Tag = "LOW";
                                    }
                                    else if (VM.AudioMuteLevel)
                                    {
                                        SelectOption.Tag = "MUTE";
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.DodgerBlue);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AudioMuteLevel, "Mute", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAudioLevelMute.Execute(null);
                                    if (VM.AudioNormalLevel)
                                    {
                                        SelectOption.Tag = "DEFAULT";
                                    }
                                    else if (VM.AudioHighLevel)
                                    {
                                        SelectOption.Tag = "HIGH";
                                    }
                                    else if (VM.AudioMediumLevel)
                                    {
                                        SelectOption.Tag = "MEDIUM";
                                    }
                                    else if (VM.AudioLowLevel)
                                    {
                                        SelectOption.Tag = "LOW";
                                    }
                                    else if (VM.AudioMuteLevel)
                                    {
                                        SelectOption.Tag = "MUTE";
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);


                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;

                    case "vmute":
                        VM.SetAudioLevelMute.Execute(null);
                        break;
                    case "vhigh":
                        VM.SetAudioLevelHigh.Execute(null);
                        break;
                    case "vdefault":
                        VM.SetAudioLevelNormal.Execute(null);
                        break;
                    case "vmeduim":
                        VM.SetAudioMediumLevel.Execute(null);
                        break;
                    case "vlow":
                        VM.SetAudioLevelLow.Execute(null);
                        break;

                    case "aecho":
                        VM.SetAudioEcho.Execute(null);
                        break;

                    case "ovlines":
                        VM.SetScanlines3.Execute(null);

                        break;
                    case "ovgrid":
                        VM.SetScanlines2.Execute(null);
                        break;

                    case "antigc":
                        VM.SetReduceFreezes.Execute(null);
                        SetActiveState(SelectOption, VM.ReduceFreezes);
                        break;

                    case "render":
                        {
                            {
                                MenuFlyout coreSettingsMenu = new MenuFlyout();
                                List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                                List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                                if (!isInGameDialogVisible)
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Edit + Preview", (s, e) =>
                                    {
                                        try
                                        {
                                            ShowInGameDialog(SelectOption.Action);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, Colors.Gray);

                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                  {

                                  });
                                }
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.NearestNeighbor, "Nearest Neighbor", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetNearestNeighbor.Execute(null);
                                        if (VM.NearestNeighbor)
                                        {
                                            SelectOption.Tag = "Nearest Neighbor".ToUpper();
                                        }else if (VM.Anisotropic)
                                        {
                                            SelectOption.Tag = "Anisotropic".ToUpper();
                                        }else if (VM.Cubic)
                                        {
                                            SelectOption.Tag = "Cubic".ToUpper();
                                        }else if (VM.HighQualityCubic)
                                        {
                                            SelectOption.Tag = "HQ Cubic".ToUpper();
                                        }else if (VM.Linear)
                                        {
                                            SelectOption.Tag = "Linear".ToUpper();
                                        }else if (VM.MultiSampleLinear)
                                        {
                                            SelectOption.Tag = "MultiSample Linear".ToUpper();
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Tomato);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.Anisotropic, "Anisotropic", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetAnisotropic.Execute(null);
                                        if (VM.NearestNeighbor)
                                        {
                                            SelectOption.Tag = "Nearest Neighbor".ToUpper();
                                        }
                                        else if (VM.Anisotropic)
                                        {
                                            SelectOption.Tag = "Anisotropic".ToUpper();
                                        }
                                        else if (VM.Cubic)
                                        {
                                            SelectOption.Tag = "Cubic".ToUpper();
                                        }
                                        else if (VM.HighQualityCubic)
                                        {
                                            SelectOption.Tag = "HQ Cubic".ToUpper();
                                        }
                                        else if (VM.Linear)
                                        {
                                            SelectOption.Tag = "Linear".ToUpper();
                                        }
                                        else if (VM.MultiSampleLinear)
                                        {
                                            SelectOption.Tag = "MultiSample Linear".ToUpper();
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Orange);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.Cubic, "Cubic", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetCubic.Execute(null);
                                        if (VM.NearestNeighbor)
                                        {
                                            SelectOption.Tag = "Nearest Neighbor".ToUpper();
                                        }
                                        else if (VM.Anisotropic)
                                        {
                                            SelectOption.Tag = "Anisotropic".ToUpper();
                                        }
                                        else if (VM.Cubic)
                                        {
                                            SelectOption.Tag = "Cubic".ToUpper();
                                        }
                                        else if (VM.HighQualityCubic)
                                        {
                                            SelectOption.Tag = "HQ Cubic".ToUpper();
                                        }
                                        else if (VM.Linear)
                                        {
                                            SelectOption.Tag = "Linear".ToUpper();
                                        }
                                        else if (VM.MultiSampleLinear)
                                        {
                                            SelectOption.Tag = "MultiSample Linear".ToUpper();
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.DodgerBlue);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.HighQualityCubic, "HQ Cubic", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetHighQualityCubic.Execute(null);
                                        if (VM.NearestNeighbor)
                                        {
                                            SelectOption.Tag = "Nearest Neighbor".ToUpper();
                                        }
                                        else if (VM.Anisotropic)
                                        {
                                            SelectOption.Tag = "Anisotropic".ToUpper();
                                        }
                                        else if (VM.Cubic)
                                        {
                                            SelectOption.Tag = "Cubic".ToUpper();
                                        }
                                        else if (VM.HighQualityCubic)
                                        {
                                            SelectOption.Tag = "HQ Cubic".ToUpper();
                                        }
                                        else if (VM.Linear)
                                        {
                                            SelectOption.Tag = "Linear".ToUpper();
                                        }
                                        else if (VM.MultiSampleLinear)
                                        {
                                            SelectOption.Tag = "MultiSample Linear".ToUpper();
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Green);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.Linear, "Linear", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetLinear.Execute(null);
                                        if (VM.NearestNeighbor)
                                        {
                                            SelectOption.Tag = "Nearest Neighbor".ToUpper();
                                        }
                                        else if (VM.Anisotropic)
                                        {
                                            SelectOption.Tag = "Anisotropic".ToUpper();
                                        }
                                        else if (VM.Cubic)
                                        {
                                            SelectOption.Tag = "Cubic".ToUpper();
                                        }
                                        else if (VM.HighQualityCubic)
                                        {
                                            SelectOption.Tag = "HQ Cubic".ToUpper();
                                        }
                                        else if (VM.Linear)
                                        {
                                            SelectOption.Tag = "Linear".ToUpper();
                                        }
                                        else if (VM.MultiSampleLinear)
                                        {
                                            SelectOption.Tag = "MultiSample Linear".ToUpper();
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gold);


                                GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.MultiSampleLinear, "MultiSample Linear", (s, e) =>
                                {
                                    try
                                    {
                                        VM.SetMultiSampleLinear.Execute(null);
                                        if (VM.NearestNeighbor)
                                        {
                                            SelectOption.Tag = "Nearest Neighbor".ToUpper();
                                        }
                                        else if (VM.Anisotropic)
                                        {
                                            SelectOption.Tag = "Anisotropic".ToUpper();
                                        }
                                        else if (VM.Cubic)
                                        {
                                            SelectOption.Tag = "Cubic".ToUpper();
                                        }
                                        else if (VM.HighQualityCubic)
                                        {
                                            SelectOption.Tag = "HQ Cubic".ToUpper();
                                        }
                                        else if (VM.Linear)
                                        {
                                            SelectOption.Tag = "Linear".ToUpper();
                                        }
                                        else if (VM.MultiSampleLinear)
                                        {
                                            SelectOption.Tag = "MultiSample Linear".ToUpper();
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gray);


                                foreach (var mItem in coresSettingsMenu)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                foreach (var mItem in coresSettingsMenuNormal)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                coreSettingsMenu.ShowAt(targetElement, point);
                            }
                        }
                        break;

                    case "rnearest":
                        VM.SetNearestNeighbor.Execute(null);
                        break;
                    case "rlinear":
                        VM.SetLinear.Execute(null);
                        break;
                    case "rmultisample":
                        VM.SetMultiSampleLinear.Execute(null);
                        break;

                    case "creset":
                        VM.ClearAllEffects.Execute(null);
                        break;


                    case "effects":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.VideoService.TotalEffects() == 0, "None", (s, e) =>
                            {
                                try
                                {
                                    VM.ClearAllEffects.Execute(null);
                                    SetActiveState(SelectOption, VM.VideoService.TotalEffects() > 0);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.VideoService.TotalEffects() > 0, "Active", (s, e) =>
                            {
                                try
                                {
                                    VM.ShowAllEffects.Execute(null);
                                    SetActiveState(SelectOption, VM.VideoService.TotalEffects() > 0);
                                    if (VM.EffectsVisible) VM.HideMenuGrid();
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);


                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {

                            });

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.EffectsVisible, "Show", (s, e) =>
                            {
                                try
                                {
                                    VM.ShowAllEffects.Execute(null);
                                    SetActiveState(SelectOption, VM.VideoService.TotalEffects() > 0);
                                    if(VM.EffectsVisible)VM.HideMenuGrid();
                                }
                                catch (Exception ex)
                                {

                                }
                            });


                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;

                    case "csepia":
                        VM.ShowAllEffects.Execute(null);
                        break;
                    case "shaders":
                        VM.AddShaders = !VM.AddShaders;
                        if (VM.AddShaders)
                        {
                            SelectOption.ProgressState = Visibility.Visible;
                            await VM.GetShader();
                        }
                        SetActiveState(SelectOption, VM.AddShaders);
                        UpdateInGameMenuItems(null, null);
                        SelectOption.ProgressState = Visibility.Collapsed;
                        break;
                    case "overlays":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AddOverlays, "Set Overlay", async (s, e) =>
                            {
                                try
                                {
                                    VM.AddOverlays = !VM.AddOverlays;
                                    if (VM.AddOverlays)
                                    {
                                        SelectOption.ProgressState = Visibility.Visible;
                                        await VM.GetOverlay();
                                    }
                                    else
                                    {
                                        VM.ClearOverLaysFolder();
                                        PlatformService.PlayNotificationSound("success");
                                        VM.UpdateInfoState($"Disabled: OverlayEffect");
                                    }
                                    UpdateInGameMenuItems(null, null);
                                }
                                catch (Exception ex)
                                {

                                }
                                SelectOption.ProgressState = Visibility.Collapsed;
                                if (VM.AddOverlays)
                                {
                                    VM.HideMenuGrid();
                                    OverlayModes();
                                }
                            }, Colors.Gold);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Settings", (s, e) =>
                            {
                                try
                                {
                                    OverlayModes();
                                    HideMenu = true;
                                    VM.HideMenuGrid();
                                    SetActiveState(SelectOption, VM.AddOverlays);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.DodgerBlue);

                            if (VM.AddOverlays)
                            {
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                {

                                });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Re-Select", async (s, e) =>
                                {
                                    try
                                    {
                                        await VM.GetOverlay(null, true);
                                        SetActiveState(SelectOption, VM.AddOverlays);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                            }
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {

                            });

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ScanLines3, "Scanlines", (s, e) =>
                            {
                                try
                                {
                                    VM.SetScanlines3.Execute(null);
                                    SetActiveState(SelectOption, VM.AddOverlays);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ScanLines1, "Scanlines (Double)", (s, e) =>
                            {
                                try
                                {
                                    VM.SetScanlines1.Execute(null);
                                    SetActiveState(SelectOption, VM.AddOverlays);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ScanLines2, "Grid Overlay", (s, e) =>
                            {
                                try
                                {
                                    VM.SetScanlines2.Execute(null);
                                    SetActiveState(SelectOption, VM.AddOverlays);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);

                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;


                    case "vssetings":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AVDebug, "Activate", (s, e) =>
                            {
                                try
                                {
                                    VM.AVDebug = !VM.AVDebug;
                                    SetActiveState(SelectOption, VM.AVDebug);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Settings", (s, e) =>
                            {
                                try
                                {
                                    VisualizerSettings(null, null);
                                    SetActiveState(SelectOption, VM.AVDebug);
                                    VM.HideMenuGrid();
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.DodgerBlue);

                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;

                    case "mored":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                            List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                            if (!isInGameDialogVisible)
                            {
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Edit + Preview", (s, e) =>
                                {
                                    try
                                    {
                                        ShowInGameDialog(SelectOption.Action);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gray);

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                {

                                });
                            }
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ShowSensorsInfo, "Sensors Info", (s, e) =>
                            {
                                try
                                {
                                    VM.SetShowSensorsInfo.Execute(null);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.ShowBufferCounter, "Audio Buffer", (s, e) =>
                            {
                                try
                                {
                                    VM.ShowBufferCounterCommand.Execute(null);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.DodgerBlue);

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                            {

                            });

                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.VideoOnly, "Video Only", (s, e) =>
                            {
                                try
                                {
                                    VM.SetVideoOnly.Execute(null);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);
                            GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.AudioOnly, "Audio Only", (s, e) =>
                            {
                                try
                                {
                                    VM.SetAudioOnly.Execute(null);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gray);


                            foreach (var mItem in coresSettingsMenu)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            foreach (var mItem in coresSettingsMenuNormal)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    coreSettingsMenu.Items.Add(mItem);
                                }
                            }

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;

                    case "loglist":
                        VM.SetShowLogsList.Execute(null);
                        SetActiveState(SelectOption, VM.ShowLogsList, "OPENED", "CLOSED");
                        if(VM.ShowLogsList) VM.HideMenuGrid();
                        break;

                    case "vfsinfo":
                        VM.DisplayVFSDebug = !VM.DisplayVFSDebug;
                        SetActiveState(SelectOption, VM.DisplayVFSDebug);
                        break;

                    case "avdebug":
                        VM.AVDebug = !VM.AVDebug;
                        SetActiveState(SelectOption, VM.AVDebug);
                        break;


                    case "memory":
                        {
                            {
                                MenuFlyout coreSettingsMenu = new MenuFlyout();
                                List<MenuFlyoutItem> coresSettingsMenuNormal = new List<MenuFlyoutItem>();
                                List<ToggleMenuFlyoutItem> coresSettingsMenu = new List<ToggleMenuFlyoutItem>();
                                if (VM.isMemoryHelpersVisible)
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.BufferCopyMemory, "Buffer.CopyMemory", (s, e) =>
                                    {
                                        try
                                        {
                                            VM.BufferCopyMemory = true;
                                            if (VM.BufferCopyMemory)
                                            {
                                                SelectOption.Tag = "BUFFER";
                                            }
                                            else if (VM.memCPYMemory)
                                            {
                                                SelectOption.Tag = "MEMCPY";
                                            }
                                            else if (VM.MarshalMemory)
                                            {
                                                SelectOption.Tag = "MARSHAL";
                                            }
                                            else if (VM.SpanlMemory)
                                            {
                                                SelectOption.Tag = "SPAN";
                                            }
                                            else
                                            {
                                                SelectOption.Tag = "POINTERS";
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, Colors.Tomato);
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.memCPYMemory, "memcpy (msvcrt)", (s, e) =>
                                    {
                                        try
                                        {
                                            VM.memCPYMemory = true;
                                            if (VM.BufferCopyMemory)
                                            {
                                                SelectOption.Tag = "BUFFER";
                                            }
                                            else if (VM.memCPYMemory)
                                            {
                                                SelectOption.Tag = "MEMCPY";
                                            }
                                            else if (VM.MarshalMemory)
                                            {
                                                SelectOption.Tag = "MARSHAL";
                                            }
                                            else if (VM.SpanlMemory)
                                            {
                                                SelectOption.Tag = "SPAN";
                                            }
                                            else
                                            {
                                                SelectOption.Tag = "POINTERS";
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, Colors.Orange);
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.MarshalMemory, "Marshal.CopyTo", (s, e) =>
                                    {
                                        try
                                        {
                                            VM.MarshalMemory = true;
                                            if (VM.BufferCopyMemory)
                                            {
                                                SelectOption.Tag = "BUFFER";
                                            }
                                            else if (VM.memCPYMemory)
                                            {
                                                SelectOption.Tag = "MEMCPY";
                                            }
                                            else if (VM.MarshalMemory)
                                            {
                                                SelectOption.Tag = "MARSHAL";
                                            }
                                            else if (VM.SpanlMemory)
                                            {
                                                SelectOption.Tag = "SPAN";
                                            }
                                            else
                                            {
                                                SelectOption.Tag = "POINTERS";
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, Colors.DodgerBlue);
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, VM.SpanlMemory, "Span.CopyTo", (s, e) =>
                                    {
                                        try
                                        {
                                            VM.SpanlMemory = true;
                                            if (VM.BufferCopyMemory)
                                            {
                                                SelectOption.Tag = "BUFFER";
                                            }
                                            else if (VM.memCPYMemory)
                                            {
                                                SelectOption.Tag = "MEMCPY";
                                            }
                                            else if (VM.MarshalMemory)
                                            {
                                                SelectOption.Tag = "MARSHAL";
                                            }
                                            else if (VM.SpanlMemory)
                                            {
                                                SelectOption.Tag = "SPAN";
                                            }
                                            else
                                            {
                                                SelectOption.Tag = "POINTERS";
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, Colors.Green);
                                }
                                else
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, true, "Pointers", (s, e) =>
                                    {
                                        try
                                        {

                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, Colors.Tomato);
                                }
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "spr", (s, e) =>
                                {

                                }, Colors.Gold);


                                GenerateMenuFlyoutItem(ref coresSettingsMenu, false, "Help", (s, e) =>
                                {
                                    try
                                    {
                                        if (VM.isMemoryHelpersVisible)
                                        {
                                            VM.GeneralDialog("These are methods used to deal with the memory.\nI added multiple options so you can try whatever you want and choose what is the best for your device.\n\nNote: Span and Marshal are safe options");
                                        }
                                        else
                                        {
                                            VM.GeneralDialog("This core use pointers as memory helper and it performs better in this way");
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gray);


                                foreach (var mItem in coresSettingsMenu)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                foreach (var mItem in coresSettingsMenuNormal)
                                {
                                    if (mItem.Text.Equals("spr"))
                                    {
                                        MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                        coreSettingsMenu.Items.Add(menuFlyoutSeparator);
                                    }
                                    else
                                    {
                                        coreSettingsMenu.Items.Add(mItem);
                                    }
                                }

                                coreSettingsMenu.ShowAt(targetElement, point);
                            }
                        }
                        break;

                    case "membcpy":
                        VM.BufferCopyMemory = true;
                        break;
                    case "memcpy":
                        VM.memCPYMemory = true;
                        break;
                    case "memmarsh":
                        VM.MarshalMemory = true;
                        break;
                    case "memspan":
                        VM.SpanlMemory = true;
                        break;

                    case "achelp":
                        //PlatformService.PlayNotificationSound("notice");
                        VM.GeneralDialog("Actions will help you to store multiple keys in one button, when you trigger the action all stored keys will be pressed in order, this feature helpful for fighting games.");
                        break;

                    case "memhelp":
                        //PlatformService.PlayNotificationSound("notice");
                        VM.GeneralDialog("These are methods used to deal with the memory.\nI added multiple options so you can try whatever you want and choose what is the best for your device.\n\nNote: Span and Marshal are safe options");

                        break;


                    default:
                        break;
                }
                if (HideMenu)
                {
                    VM.HideMenuGrid();
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void GenerateMenuFlyoutItem(ref List<MenuFlyoutItem> menuFlyoutItems, string v, Action<object, object> value, Windows.UI.Color color = default(Windows.UI.Color))
        {
            try
            {
                MenuFlyoutItem menuFlyoutItem = new MenuFlyoutItem();
                if (color != default(Windows.UI.Color))
                {
                    menuFlyoutItem.Foreground = new SolidColorBrush(color);
                }
                menuFlyoutItem.Text = v;
                menuFlyoutItem.Click += (s, e) =>
                {
                    value.Invoke(null, null);
                };

                menuFlyoutItems.Add(menuFlyoutItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void GenerateMenuFlyoutItem(ref List<ToggleMenuFlyoutItem> menuFlyoutItems, bool state, string v, Action<object, object> value, Windows.UI.Color color = default(Windows.UI.Color))
        {
            try
            {
                ToggleMenuFlyoutItem menuFlyoutItem = new ToggleMenuFlyoutItem();
                menuFlyoutItem.IsChecked = state;
                if (color != default(Windows.UI.Color))
                {
                    menuFlyoutItem.Foreground = new SolidColorBrush(color);
                }
                menuFlyoutItem.Text = v;
                menuFlyoutItem.Click += (s, e) =>
                {
                    value.Invoke(null, null);
                };

                menuFlyoutItems.Add(menuFlyoutItem);
            }
            catch (Exception ex)
            {

            }
        }
        private async void CoresQuickAccessList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            isTappedTriggered = true;
            CoresQuickListItem SelectOption = null;

            UIElement targetElement = null;
            try
            {
                string ObjectType = e.OriginalSource.GetType().Name;
                switch (ObjectType)
                {
                    case "Image":
                        SelectOption = (CoresQuickListItem)((Image)e.OriginalSource).DataContext;
                        targetElement = ((Image)e.OriginalSource);
                        break;

                    case "TextBlock":
                        SelectOption = (CoresQuickListItem)((TextBlock)e.OriginalSource).DataContext;
                        targetElement = ((TextBlock)e.OriginalSource);
                        break;

                    case "Border":
                        SelectOption = (CoresQuickListItem)((Border)e.OriginalSource).DataContext;
                        targetElement = ((Border)e.OriginalSource);
                        break;

                    case "StackPanel":
                        SelectOption = (CoresQuickListItem)((StackPanel)e.OriginalSource).DataContext;
                        targetElement = ((StackPanel)e.OriginalSource);
                        break;

                    case "Grid":
                        SelectOption = null;
                        break;

                    case "GridViewItemPresenter":
                        SelectOption = (CoresQuickListItem)((GridViewItemPresenter)e.OriginalSource).DataContext;
                        targetElement = ((GridViewItemPresenter)e.OriginalSource);
                        break;

                    case "ListViewItemPresenter":
                        SelectOption = (CoresQuickListItem)((ListViewItemPresenter)e.OriginalSource).DataContext;
                        targetElement = ((ListViewItemPresenter)e.OriginalSource);
                        break;

                    case "GridView":
                        SelectOption = (CoresQuickListItem)((GridView)e.OriginalSource).DataContext;
                        targetElement = ((GridView)e.OriginalSource);
                        break;

                    default:
                        SelectOption = (CoresQuickListItem)((GridViewItem)e.OriginalSource).DataContext;
                        targetElement = ((GridViewItem)e.OriginalSource);
                        break;
                }
            }
            catch (Exception ex)
            {

            }

            var point = e.GetPosition(targetElement);
            await ListEventGeneral_Tapped(SelectOption, point, targetElement);
        }

        private void InGameMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ToggleMenuGridActive();
            }
            catch (Exception ex)
            {

            }
        }
    }
    public class EffectsListItem : BindableBase
    {
        public string Tag;
        public int Sliders = 0;
        public string EffectName;
        bool effectVisible = true;
        public bool EffectVisible
        {
            get
            {
                return effectVisible;
            }
            set
            {
                effectVisible = value;
                RaisePropertyChanged(nameof(EffectVisible));
            }
        }
        public bool IsOn
        {
            get
            {
                try
                {
                    bool temp = (bool)VM.GetType().GetProperty(Tag).GetValue(VM);
                    return temp;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    VM.GetType().GetProperty(Tag).SetValue(VM, value);
                    RaisePropertyChanged(nameof(IsOn));
                }
                catch (Exception ex)
                {

                }
            }
        }
        public bool compatibiltyTag
        {
            get
            {
                return VM.compatibiltyTag;
            }
        }
        public GamePlayerViewModel VM;
        string Value1Name;
        string Value2Name;
        string Value3Name;
        string Value4Name;

        public EffectsListItem(GamePlayerViewModel vm, string tag, string name, List<EffectValue> values)
        {
            VM = vm;
            Tag = tag;
            EffectName = name;
            if (values != null)
            {
                if (values.Count > 0)
                {
                    SetSlider1Settings(values[0].Title, values[0].Name, values[0].Max, values[0].Min, values[0].Step);
                }
                if (values.Count > 1)
                {
                    SetSlider2Settings(values[1].Title, values[1].Name, values[1].Max, values[1].Min, values[1].Step);
                }
                if (values.Count > 2)
                {
                    SetSlider3Settings(values[2].Title, values[2].Name, values[2].Max, values[2].Min, values[2].Step);
                }
                if (values.Count > 3)
                {
                    SetSlider4Settings(values[3].Title, values[3].Name, values[3].Max, values[3].Min, values[3].Step);
                }
            }
        }
        public void SetSlider1Settings(string title, string name, double max, double min, double step)
        {
            Value1Title = title;
            Value1Max = max;
            Value1Min = min;
            Value1Step = step;
            Sliders++;
            Value1Name = name;
        }
        public void SetSlider2Settings(string title, string name, double max, double min, double step)
        {
            Value2Title = title;
            Value2Max = max;
            Value2Min = min;
            Value2Step = step;
            Sliders++;
            Value2Name = name;
        }
        public void SetSlider3Settings(string title, string name, double max, double min, double step)
        {
            Value3Title = title;
            Value3Max = max;
            Value3Min = min;
            Value3Step = step;
            Sliders++;
            Value3Name = name;
        }
        public void SetSlider4Settings(string title, string name, double max, double min, double step)
        {
            Value4Title = title;
            Value4Max = max;
            Value4Min = min;
            Value4Step = step;
            Sliders++;
            Value4Name = name;
        }
        //If sliders visible or not
        public bool SlidersState
        {
            get
            {
                return Sliders != 0;
            }
        }

        public string Value1Title = "";
        public double Value1
        {
            get
            {
                if (Value1Name == null)
                {
                    return 0;
                }
                try
                {
                    double temp = (double)VM.GetType().GetProperty(Value1Name).GetValue(VM);
                    return temp;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    VM.GetType().GetProperty(Value1Name).SetValue(VM, value);
                }
                catch (Exception ex)
                {

                }
            }
        }
        public double Value1Max = 0;
        public double Value1Min = 0;
        public double Value1Step = 0;
        public bool Value1State
        {
            get
            {
                return Sliders > 0;
            }
        }

        public string Value2Title = "";
        public double Value2
        {
            get
            {
                if (Value2Name == null)
                {
                    return 0;
                }
                try
                {
                    double temp = (double)VM.GetType().GetProperty(Value2Name).GetValue(VM);
                    return temp;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    VM.GetType().GetProperty(Value2Name).SetValue(VM, value);
                }
                catch (Exception ex)
                {

                }
            }
        }
        public double Value2Max = 0;
        public double Value2Min = 0;
        public double Value2Step = 0;
        public bool Value2State
        {
            get
            {
                return Sliders > 1;
            }
        }

        public string Value3Title = "";
        public double Value3
        {
            get
            {
                if (Value3Name == null)
                {
                    return 0;
                }
                try
                {
                    double temp = (double)VM.GetType().GetProperty(Value3Name).GetValue(VM);
                    return temp;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    VM.GetType().GetProperty(Value3Name).SetValue(VM, value);
                }
                catch (Exception ex)
                {

                }
            }
        }
        public double Value3Max = 0;
        public double Value3Min = 0;
        public double Value3Step = 0;
        public bool Value3State
        {
            get
            {
                return Sliders > 2;
            }
        }

        public string Value4Title = "";
        public double Value4
        {
            get
            {
                if (Value4Name == null)
                {
                    return 0;
                }
                try
                {
                    double temp = (double)VM.GetType().GetProperty(Value4Name).GetValue(VM);
                    return temp;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    VM.GetType().GetProperty(Value4Name).SetValue(VM, value);
                }
                catch (Exception ex)
                {

                }
            }
        }
        public double Value4Max = 0;
        public double Value4Min = 0;
        public double Value4Step = 0;
        public bool Value4State
        {
            get
            {
                return Sliders > 3;
            }
        }
    }

    public class EffectValue
    {
        public string Name;
        public string Title;
        public double Max;
        public double Min;
        public double Step;
        public EffectValue(string title, string name, double max, double min, double step)
        {
            Title = title;
            Name = name;
            Max = max;
            Min = min;
            Step = step;
        }
    }
}
