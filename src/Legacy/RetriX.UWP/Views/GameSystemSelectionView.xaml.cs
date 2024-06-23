using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Toolkit.Uwp.UI.Animations;
using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Cores;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using WinUniversalTool.Models;
using WinUniversalTool;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Data;
using System.Globalization;
using RetriX.Shared.StreamProviders;
using static System.Net.WebRequestMethods;
using System.Timers;
using System.Drawing;
using Windows.UI.Xaml.Markup;
using static TaskMonitor.NativeMethods;
using Windows.Graphics.Display;
using static System.Net.Mime.MediaTypeNames;
using Windows.Graphics.Imaging;
using Image = Windows.UI.Xaml.Controls.Image;
using TextAlignment = Windows.UI.Xaml.TextAlignment;
using System.ServiceModel.Channels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Playback;
using Windows.Media.Core;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Networking.Vpn;
#if !USING_NATIVEARCHIVE
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Archives.Zip;
#endif

/**
  Copyright (c) RetriX Developer Alberto Fustinoni:
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.UWP.Pages
{
    public sealed partial class GameSystemSelectionView : Page
    {
        bool SideLoad = true;

        MediaElement _player;
        List<CoreIssues> coresIssues = new List<CoreIssues>();

        public GameSystemSelectionViewModel VM
        {
            get
            {
                return PlatformService.gameSystemSelectionView;
            }
        }

        int InitWidthSize { get => PlatformService.InitWidthSize; }
        int InitWidthSizeCustom { get => PlatformService.InitWidthSize - 30; }
        bool ShowErrorsIcon { get { return ShowErrorNotification; } }
        bool ShowErrorsList = false;
        public static EventHandler DirectRestore;
        ObservableCollection<string> SkippedListSource { get { return SkippedList; } }
        HorizontalAlignment horizontalAlignment { get => PlatformService.horizontalAlignment; }

        public ObservableCollection<GroupCoreOptionListItems> coreOptionsGroupped = new ObservableCollection<GroupCoreOptionListItems>();
        public ObservableCollection<GroupBIOSListItems> biosGroupped
        {
            get
            {
                return VM.biosGroupped;
            }
        }

        bool AnyCoreLoaded = false;
        ApplicationView AppView = ApplicationView.GetForCurrentView();
        public static string logfileLocation = "";


        public bool isXBOX
        {
            get
            {
                return PlatformService.isXBOX;
            }
        }

        bool active = false;
        bool activeState
        {
            get
            {
                return active || SideLoad;
            }
            set
            {
                active = value;
            }
        }

        bool thumnailsIconsState
        {
            get
            {
                return PlatformService.ShowThumbNailsIcons;
            }
        }

        public GameSystemSelectionView()
        {
            this.InitializeComponent();
            ShowLoader(true);
            try
            {
                var currentHeight = Window.Current.CoreWindow.Bounds.Height;
                var currentWidth = Window.Current.CoreWindow.Bounds.Width;
                GamePlayerView.LPX = currentWidth / 2;
                GamePlayerView.LPY = currentHeight / 2;
            }
            catch (Exception ex)
            {

            }
            activeState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RetriXActive", false);

            try
            {
                var currentView = SystemNavigationManager.GetForCurrentView();
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            catch (Exception e)
            {
            }
            try
            {
                logfileLocation = $"{ApplicationData.Current.LocalFolder.Path}\\RetriXGoldLog.txt";
                if (PlatformService.JustStarted)
                {
                    System.IO.File.WriteAllText(logfileLocation, "");
                    string DateText = DateTime.Now.ToString();
                    WriteLog($"**************{DateText}***************");
                }
            }
            catch { }

            try
            {
                CoreOptionListItemsGroup.Source = coreOptionsGroupped;
                BIOSListItemsGroup.Source = biosGroupped;
            }
            catch (Exception ex)
            {

            }
            try
            {
                string AppLocation = ApplicationData.Current.LocalFolder.Path;
                string CoreLocation = AppLocation + @"\AnyCore";
                if (Directory.Exists(CoreLocation))
                {
                    AnyCoreLoaded = true;
                }
            }
            catch (Exception ed)
            {

            }

            try
            {
                if (PlatformService.DeviceIsPhone())
                {
                    try
                    {
                        PlatformService.MobileStatusBar = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("MobileStatusBar", false);
                        WriteLog($"Mobile status bar: {(PlatformService.MobileStatusBar ? "ON" : "OFF")}");
                    }
                    catch (Exception e)
                    {

                    }
                    if (!PlatformService.MobileStatusBar)
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        _ = statusBar.HideAsync();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            try
            {
                DPIFixup();
                DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
                displayInformation.DpiChanged += DisplayProperties_DpiChanged;
            }
            catch (Exception ex)
            {

            }
            try
            {
                _player = new MediaElement();
                _player.Visibility = Visibility.Collapsed;
                mainGrid.Children.Add(_player);
            }
            catch (Exception ex)
            {

            }
            try
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                {
                    var uiSettings = new Windows.UI.ViewManagement.UISettings();
                    var color = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
                    Windows.UI.Color themeColor = color;
                    Windows.UI.Xaml.Media.AcrylicBrush myBrush = new Windows.UI.Xaml.Media.AcrylicBrush();
                    myBrush.BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop;
                    myBrush.TintColor = themeColor;
                    myBrush.FallbackColor = themeColor;
                    myBrush.TintOpacity = 0.99;
                    App.Current.Resources["ApplicationPageBackgroundThemeBrush"] = myBrush;
                    App.Current.Resources["SystemControlHighlightAccentBrush"] = myBrush;
                    App.Current.Resources["AccentButtonBackgroundPointerOver"] = myBrush;
                    mainGrid.Background = myBrush;
                }
            }
            catch (Exception ex)
            {

            }
            try
            {
                var build = int.Parse(GetAppVersion().Replace(".", ""));
                PlatformService.RetriXBuildNumber = build;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }

            PlatformService.NotificationHandlerMain = pushLocalNotification;
            PlatformService.RetroPassHandler = RetroPassHandler;
            PlatformService.CoreDownloaderHandler = CoreDownloaderHandler;
            PlatformService.CoresLoaderHandler = CoresLoaderHandler;

            try
            {
                if (!PlatformService.pageReady)
                {
                    CorePagePivot.KeyDown += (s, e) =>
                    {
                        if (!PlatformService.isXBOX)
                        {
                            if (e.Key == Windows.System.VirtualKey.Escape)
                            {
                                e.Handled = true;
                                HideSubPages(null, null);
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {

            }


            if (PlatformService.pageReady)
            {
                //ProgressContainer.Visibility = Visibility.Collapsed;
                //return;
            }
            try
            {
                PlatformService.MuteSFX = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("MuteSFX", false);
                WriteLog($"Sound Effects: {(PlatformService.MuteSFX ? "OFF" : "ON")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.AutoResolveVFSIssues = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AutoResolveVFSIssues", false);
                WriteLog($"Auto Resolve VFS Issues: {(PlatformService.AutoResolveVFSIssues ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.UseWindowsIndexer = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("UseWindowsIndexer", true);
                WriteLog($"Use Windows Indexer: {(PlatformService.UseWindowsIndexer ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.UseWindowsIndexerSubFolders = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("UseWindowsIndexerSubFolders", true);
                WriteLog($"Search In Sub Folders: {(PlatformService.UseWindowsIndexerSubFolders ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.ExtraConfirmation = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ExtraConfirmation", true);
                WriteLog($"Extra Confirmation: {(PlatformService.ExtraConfirmation ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.AdjustInGameLists = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AdjustInGameLists", true);
                WriteLog($"Adjust In Game Lists: {(PlatformService.AdjustInGameLists ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.ShowIndicators = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ShowIndicators", true);
                WriteLog($"Show Indicators: {(PlatformService.ShowIndicators ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.ShowThumbNailsIcons = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ShowThumbNailsIcons", true);
                WriteLog($"Show thumbnailsIcons: {(PlatformService.ShowThumbNailsIcons ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.SafeRender = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("SafeRender", false);
                WriteLog($"Safe render method: {(PlatformService.SafeRender ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.ForceOldBufferMethods = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ForceOldBufferMethods", false);
                WriteLog($"Old render method: {(PlatformService.ForceOldBufferMethods ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.PreventGCAlways = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("PreventGCAlways", false);
                WriteLog($"Prevent GC Always: {(PlatformService.PreventGCAlways ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.ExtraDelay = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ExtraDelay", false);
                WriteLog($"Extra Delay: {(PlatformService.ExtraDelay ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.DetectInputs = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("DetectInputs", false);
                WriteLog($"Detect Inputs: {(PlatformService.DetectInputs ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.useNativeKeyboardByDefault = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("useNativeKeyboardByDefault", true);
                WriteLog($"Native Keyboard (By default): {(PlatformService.useNativeKeyboardByDefault ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.ShowScreeshots = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ShowScreeshots", true);
                WriteLog($"Show Screenshots: {(PlatformService.ShowScreeshots ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.UseColoredDialog = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("UseColoredDialog", true);
                WriteLog($"Use Colored Dialog: {(PlatformService.UseColoredDialog ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.RetroPassRoot = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RetroPassRoot", false);
                WriteLog($"Search In Root For RetroPass: {(PlatformService.RetroPassRoot ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.AutoFitResolver = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("AutoFitResolver", true);
                WriteLog($"Auto fit resolver: {(PlatformService.AutoFitResolver ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                App.HandleBackPress = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("HandleBackPress", !PlatformService.isXBOX);
                WriteLog($"BackPress Handle: {(App.HandleBackPress ? "ON" : (PlatformService.isXBOXPure ? " XBOX ON" : "OFF"))}");
            }
            catch (Exception e)
            {

            }
            try
            {
                App.CleanTempOnStartup = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("CleanTempOnStartup", true);
                WriteLog($"Clean temp on startup: {(App.CleanTempOnStartup ? "ON" : "OFF")}");
            }
            catch (Exception e)
            {

            }

            try
            {
                PlatformService.SupportSections = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("SupportSections", false);
                WriteLog($"Support Section: {(PlatformService.SupportSections ? "VISIBLE" : "HIDDEN")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.SettingsSections = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("SettingsSections", false);
                WriteLog($"Settings Section: {(PlatformService.SettingsSections ? "VISIBLE" : "HIDDEN")}");
            }
            catch (Exception e)
            {

            }
            try
            {
                PlatformService.FullScreen = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("FullScreen", PlatformService.DeviceIsPhone());
                WriteLog($"FullScreen State: {(PlatformService.FullScreen ? "ON" : "OFF")}");
                if (PlatformService.FullScreen)
                {
                    if (!AppView.TryEnterFullScreenMode())
                    {
                        WriteLog($"FullScreen State: Failed to enter fullscreen");
                    }
                }
            }
            catch (Exception e)
            {

            }

            /***
             * Crazy buffer generating area
             * NOTE: this just to generate minified code to be used in CrazyBuffer function and it should never be active in production release
             */
            bool generateCrazyBufferCode = false;
            if (generateCrazyBufferCode)
            {
                var pixelsCount = new int[] { 450, 500, 550, 600, 650, 700, 750, 800, 850, 900, 950, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000 };
                var code = "";
                var code8888 = "";
                foreach (var pItem in pixelsCount)
                {
                    for (int i = 0; i < pItem; i++)
                    {
                        code += $"*((uint*)(destination + (outputOffest * {i}))) = FramebufferConverter.getPattern(*((ushort*)(source + (inputOffset * {i}))));";
                        code8888 += $"*((uint*)(destination + (outputOffest * {i}))) = (*((uint*)(source + (inputOffset * {i}))));";
                    }
                    var location = $"{ApplicationData.Current.LocalFolder.Path}\\SaveActions\\code ({pItem}).txt";
                    var location8888 = $"{ApplicationData.Current.LocalFolder.Path}\\SaveActions\\code8888 ({pItem}).txt";
                    System.IO.File.WriteAllText(location, code);
                    System.IO.File.WriteAllText(location8888, code8888);
                }
            }


            try
            {
                loadShortcuts();
            }
            catch (Exception ex)
            {

            }
            try
            {
                try
                {
                    PlatformService.ReloadSystemsHandler -= ReloadSystemsHandler;
                }
                catch
                {

                }
                try
                {
                    PlatformService.ImportFolderHandler -= ImportFolderHandler;
                }
                catch
                {

                }
                PlatformService.ReloadSystemsHandler += ReloadSystemsHandler;
                PlatformService.ImportFolderHandler += ImportFolderHandler;
                RetriXGoldCoresLoader();
                try
                {
                    PlatformService.SaveListStateGlobal -= GameSystemSelectionView_eventHandler;
                }
                catch
                {

                }
                PlatformService.SaveListStateGlobal += GameSystemSelectionView_eventHandler;

                Window.Current.SizeChanged += (sender, args) =>
                {
                    PlatformService.checkInitWidth(false);
                    ApplicationView currentView = ApplicationView.GetForCurrentView();

                    if (currentView.Orientation == ApplicationViewOrientation.Landscape)
                    {
                    }
                    else if (currentView.Orientation == ApplicationViewOrientation.Portrait)
                    {
                    }
                    BindingsUpdate();
                };
                PlatformService.checkInitWidth();
                UpdateBindingDelay();
            }
            catch (Exception e)
            {
                ProgressContainerMain.Visibility = Visibility.Collapsed;
                PlatformService.ShowErrorMessageDirect(e);
            }
            callMemoryTimerTimer(true);
            PlatformService.pageReady = true;
            checkCoresIssues();
            checkUpscaleSupport();

            //Convert all app PNGs to BMPs, for one time usage not for production
            //_ = TranscodeProjectResources();
        }

        string DPIScaleInfo = "";
        void DisplayProperties_DpiChanged(DisplayInformation sender, object args)
        {
            DPIFixup();
        }
        void DPIFixup()
        {
            try
            {
                DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
                PlatformService.ScreenScale = (displayInformation.RawPixelsPerViewPixel * 100.0);
                PlatformService.DPI = displayInformation.LogicalDpi;
                String scaleValue = PlatformService.ScreenScale.ToString("F0");
                DPIScaleInfo = scaleValue + $"% [{PlatformService.DPI} DPI]";

                /*List<MeterScaleSection> sections = new List<MeterScaleSection>();
                List<MeterScaleLabel> labels = new List<MeterScaleLabel>();
                for(var i=0;i< vuMeter.ScaleSections.Length;i++)
                {
                    var sItem = vuMeter.ScaleSections[i];
                    if (sItem.LineColor == Colors.Black)
                    {
                        sItem.LineColor = Colors.DodgerBlue;
                    }
                    else
                    {
                        sItem.LineColor = Colors.Gold;
                    }
                    sections.Add(sItem);
                }
                for (var i = 0; i < vuMeter.ScaleLabels.Length; i++)
                {
                    var sItem = vuMeter.ScaleLabels[i];
                    if (sItem.TickColor == Colors.Black)
                    {
                        sItem.TickColor = Colors.DodgerBlue;
                    }
                    else
                    {
                        sItem.TickColor = Colors.Gold;
                    }
                    labels.Add(sItem);
                }
                vuMeter.ScaleSections = sections.ToArray();
                vuMeter.ScaleLabels = labels.ToArray();*/
            }
            catch (Exception ex)
            {

            }
        }
        public static bool isUpscaleSupported = false;
        Visibility upscaleOptionState
        {
            get
            {
                if (isUpscaleSupported)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        private void FixUpScale()
        {
            // Need to add a using for System.Reflection;

            var displayInfo = DisplayInformation.GetForCurrentView();
            var realScale = displayInfo.RawPixelsPerViewPixel;
            {
                // To get to actual Windows 10 scale, we need to convert from 
                // the de-scaled Win8.1 (100/125) back to 100, then up again to
                // the desired scale factor (125). So we get the ratio between the
                // Win8.1 pixels and real pixels, and then square it. 
                var fixupFactor = Math.Pow((double)displayInfo.ResolutionScale /
                  realScale / 100, 2);

                Window.Current.Content.RenderTransform = new ScaleTransform
                {
                    ScaleX = fixupFactor,
                    ScaleY = fixupFactor
                };
            }
        }
        private void checkUpscaleSupport()
        {
            try
            {
                isUpscaleSupported = false;
            }
            catch (Exception ex)
            {

            }
        }
        private async void checkCoresIssues()
        {
            try
            {
                coresIssues = new List<CoreIssues>();
                if (Helpers.CheckInternetConnection())
                {
                    var timeStamp = new TimeSpan(DateTime.Now.Ticks);
                    var time = timeStamp.Milliseconds;
                    var mainLink = $"https://github.com/basharast/RetrixGold/raw/main/cores/issues.json";
                    var unCahcedLink = $"{mainLink}?time={time}";
                    var testResponse = await Helpers.GetResponse(unCahcedLink, cancellationTokenSource.Token, null, false);
                    if (testResponse != null)
                    {
                        var baseStreamTemp = await testResponse.Content.ReadAsInputStreamAsync();
                        var tempStream = baseStreamTemp.AsStreamForRead();

                        MemoryStream memoryStreamFile = new MemoryStream();
                        using (tempStream)
                        {
                            using (memoryStreamFile)
                            {
                                await tempStream.CopyToAsync(memoryStreamFile);
                            }
                            tempStream.Dispose();
                        }
                        byte[] resultInBytes;
                        resultInBytes = memoryStreamFile.ToArray();
                        var textRead = Encoding.UTF8.GetString(resultInBytes, 0, resultInBytes.Length);

                        if (textRead != null && textRead.Length > 0)
                        {
                            coresIssues = JsonConvert.DeserializeObject<List<CoreIssues>>(textRead);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        private void ReloadSystemsHandler(object sender, EventArgs e)
        {
            HideSubPages(sender, e);
            RetriXGoldCoresLoader();
        }

        bool RetriXGoldCoresLoaderInProgress = false;
        private async void RetriXGoldCoresLoader(bool forceReload = false, bool deleteCore = false)
        {
            activeState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RetriXActive", false);
            RetriXGoldCoresLoaderInProgress = true;
            WriteLog($"Start loading..");
            if (forceReload)
            {
                WriteLog($"Core loader called with 'force reload' state..");
                currentProgress = 0;
                GameSystemSelectionView_eventHandler(null, null);
                ShowErrorsList = false;
                BindingsUpdate();
            }
            try
            {

                var firstRun = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("FirstRun", true);
                if (firstRun)
                {
                    //Set few cores as pinned by default, it can be changed by user
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"scummvm-ScummVM-Pinned", true);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"FinalBurn Neo-Arcade-Pinned", true);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"Snes9x-Super Nintendo-Pinned", true);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"DOSBox-pure-DOSBox Pure-Pinned", true);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"Genesis Plus GX-Mega Drive-Pinned", true);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"NXEngine-Cave Story-Pinned", true);
#if TARGET_X64
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"Beetle PSX-PlayStation-Pinned", true);
#elif TARGET_ARM
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"Beetle PSX-PlayStation-Pinned", true);
#elif TARGET_X86
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"Beetle PSX-PlayStation-Pinned", true);
#endif
                }

                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    CoresQuickAccessContainer.IsEnabled = false;

                    ShowLoader(true, "Cleaning Temp..");
                    await CleanTempFolder();
                    WriteLog($"Temp files cleaned..");

                    ShowLoader(true, "Retriving Data..");
                    await PlatformService.RetriveRecents();
                    WriteLog($"Recents retrive done..");

                    await PlatformService.GetAnyCores();
                    WriteLog($"AnyCore retrive done..");

                    await GameSystemSelectionViewModel.AnyCoreRetrieveAsync();
                    WriteLog($"AnyCore Infos retrive done..");

                    /*if (await IsCoresDLLFileSyncRequired())
                    {
                        WriteLog($"Cores Contents files need to be synced..");
                        await SyncCoresDLLFiles();
                    }
                    else
                    {
                        WriteLog($"Cores Contents files already synced..");
                    }*/

                    ShowLoader(true, "Loading Cores..");
                    WriteLog($"Loading cores..");
                    try
                    {
                        CoreLoadingProgress.Maximum = PlatformService.MaxProgressValue;
                        CoreLoadingProgress.Visibility = Visibility.Visible;
                        while (VM == null)
                        {
                            await Task.Delay(500);
                        }
                        if (forceReload)
                        {
                            returningFromSubPage = true;
                        }
                        if ((!PlatformService.isCoresLoaded || forceReload) && activeState)
                        {
                            if (!PlatformService.isCoresLoaded)
                            {
                                WriteLog($"Cores not loaded yet, generating cores..");
                            }
                            else if (forceReload)
                            {
                                WriteLog($"Cores already loaded but requesting force reload, generating cores..");
                                PlatformService.Consoles.Clear();
                            }
                            PlatformService.Consoles = await GenerateSystemsList(forceReload);
                            await VM.AsyncLoader();

                            if (activeState)
                            {
                                ShowLoader(true, "Syncing Files..\n");
                                WriteLog($"Syncing cores main files..");
                                if (await IsScummVMFileSyncRequired())
                                {
                                    WriteLog($"ScummVM files need to be synced..");
                                    await SyncScummVMFiles();
                                }
                                else
                                {
                                    WriteLog($"ScummVM files already synced..");
                                }
                                if (await IsCaveStoryFileSyncRequired())
                                {
                                    WriteLog($"Cave Story files need to be synced..");
                                    await SyncCaveStoryFiles();
                                }
                                else
                                {
                                    WriteLog($"Cave Story files already synced..");
                                }
                                if (await IsQuakeFileSyncRequired())
                                {
                                    WriteLog($"Quake files need to be synced..");
                                    await SyncQuakeFiles();
                                }
                                else
                                {
                                    WriteLog($"Quake files already synced..");
                                }
                                if (await IsTestContentFileSyncRequired())
                                {
                                    WriteLog($"Test Contents files need to be synced..");
                                    await SyncTestContentFiles();
                                }
                                else
                                {
                                    WriteLog($"Test Contents already synced..");
                                }
                                await SyncCoresFiles();
                            }

                            ShowLoader(true, "Starting RetriXGold..");
                            if (forceReload)
                            {
                                if (!deleteCore)
                                {
                                    if (VM.SelectedSystem != null)
                                    {
                                        WriteLog($"System already selected..");
                                        foreach (var system in PlatformService.Consoles)
                                        {
                                            if (system.DLLName.Equals(VM.SelectedSystem.DLLName) && system.Name.Equals(VM.SelectedSystem.Name))
                                            {
                                                VM.SelectedSystem = system;
                                                WriteLog($"Update system selection for {system.Name}..");
                                                break;
                                            }
                                        }
                                        if (VM.SelectedSystem.Core.FailedToLoad || VM.SelectedSystem.Core.RestartRequired)
                                        {
                                            WriteLog($"System failed to load or restart required..");
                                            HideSystemGames();
                                        }
                                        else
                                        {
                                            ReloadCorePage_Click(null, null);
                                        }
                                    }
                                }
                                else
                                {
                                    WriteLog($"Core delete requested..");
                                    HideSystemGames();
                                }
                            }
                        }
                        else
                        {
                            WriteLog($"Cores already loaded..");
                            //When system already selected, I should display the core page again
                            if (VM.SelectedSystem != null)
                            {
                                WriteLog($"System already selected..");
                                returningFromSubPage = true;
                                PrepareCoreData();
                                while (CorePageInProgress)
                                {
                                    await Task.Delay(100);
                                }
                            }

                            //If cores already loaded rebuild the list from VM
                            //Generate Cores List
                            WriteLog($"Generating cores from cache..");
                            GenerateCoresList(PlatformService.Consoles);
                        }
                        PlatformService.SetHideSubPageHandler(HideSubPages);

                        while (currentProgress <= PlatformService.MaxProgressValue)
                        {
                            currentProgress++;
                            CoreLoadingProgress.Value = currentProgress;
                            await Task.Delay(1);
                        }
                        CoreLoadingProgress.Visibility = Visibility.Collapsed;
                        ShowLoader(false);
                        VM.SystemCoreIsLoadingState(false);

                        try
                        {
                            if (PlatformService.OpenBackupFile != null)
                            {
                                _ = ImportSettingsSlotsAction(PlatformService.OpenBackupFile);
                                WriteLog($"Backup file called at startup..");
                            }
                        }
                        catch (Exception ee)
                        {
                            PlatformService.ShowErrorMessageDirect(ee);
                        }
                        DirectRestore = (sender, e) =>
                        {
                            if (PlatformService.OpenBackupFile != null)
                            {
                                _ = ImportSettingsSlotsAction(PlatformService.OpenBackupFile);
                                WriteLog($"Backup file called at startup..");
                            }
                        };

                        if (!PlatformService.JustStarted)
                        {
                            restoreListPosition();
                        }
                        PlatformService.ReloadCorePageGlobal = ReloadCorePageHandler;
                        BindingsUpdate();
                    }
                    catch (Exception er)
                    {
                        PlatformService.ShowErrorMessageDirect(er);
                    }
                    RetriXGoldCoresLoaderInProgress = false;

                    //Renable this when core page is active will cause focus issue
                    //It will be renabled when core page closed
                    if (VM.SelectedSystem == null)
                    {
                        VM.SetCoresQuickAccessContainerState(true);
                    }
                    if (!returningFromSubPage)
                    {
                        if (PlatformService.JustStarted)
                        {

                            PlatformService.JustStarted = false;
                            PlatformService.PlayNotificationSoundDirect("launched");
                            try
                            {
                                if (firstRun)
                                {
                                    PlatformService.ShowNotificationMain("Thanks for using RetriXGold", 4);
                                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("FirstRun", false);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }

                    //Build cores html page
                    /*var html = "<div class=\"cores-container\">";
                    foreach(var cItem in PlatformService.Consoles.OrderBy(a=>a.Name))
                    {
                        html += "<div class=\"core-item\">";
                        html += $"<h3>{cItem.Name}</h3>";
                        html += $"<h4>{cItem.Manufacturer}</h4>";
                        html += $"<img src=\"{(cItem.Symbol.Replace("ms-appx:///Assets/Icons", "assets/cicons"))}\" width=\"60\" height=\"60\"/>";
                        html += $"<h5>{cItem.Core.Name}</h4>";
                        //html += $"<a href=\"core::{(cItem.DLLName.Replace(".dll", ""))}\">Install (Click Here)</a>";
                        var coreCleanName = cItem.Core.Name.Replace("-", "_").Replace("&", "").Replace("'", "").Replace("  ", " ").Replace(" ", "_");
                        var resolvedName = ResolveCoreName(coreCleanName);
                        var coreLibretroLink = resolvedName.StartsWith("https://") ? resolvedName : $"https://docs.libretro.com/library/{resolvedName}";
                        html += $"<a href=\"{coreLibretroLink}\">Authors & Docs (Click)</a>";
                        html += "</div>";
                    }

                    html += "</div>";
                    File.WriteAllText(ApplicationData.Current.LocalFolder.Path + "\\test.html", html);*/

                    WriteLog($"System loading done..");
                    CheckPlayTimeTemp();
                    if (PlatformService.AppStartedByRetroPass)
                    {
                        WriteLog($"App started by retropass..");
                        RetroPassHandler(null, null);
                    }
                    if (PlatformService.RequestingCoreDownload.Length > 0)
                    {
                        DownloadNewCore(PlatformService.RequestingCoreDownload);
                        PlatformService.RequestingCoreDownload = "";
                    }
                });
            }
            catch (Exception ex)
            {
                RetriXGoldCoresLoaderInProgress = false;
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    CoresQuickAccessContainer.IsEnabled = true;
                });
                PlatformService.ShowErrorMessageDirect(ex);
                WriteLog($"{ex.Message}");
            }
        }

        private void DownloadNewCore(string coreName)
        {
            try
            {
                PureCoreUpdateOnline_Click(coreName);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }
        public void CoresLoaderHandler(object sender, EventArgs args)
        {
            try
            {
                activeState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RetriXActive", false);
                AddConsoles();
                RetriXGoldCoresLoader(true);
            }
            catch (Exception ex)
            {

            }
        }
        public void CoreDownloaderHandler(object sender, EventArgs args)
        {
            try
            {
                if (PlatformService.RequestingCoreDownload.Length > 0)
                {
                    DownloadNewCore(PlatformService.RequestingCoreDownload);
                    PlatformService.RequestingCoreDownload = "";
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        Visibility ShowGameMenuOptions
        {
            get
            {
                if (VM != null && VM.targetFilesRequested)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }
        Visibility ShowFilesMenu
        {
            get
            {
                if (VM != null && VM.targetFilesRequested)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
        public async void RetroPassHandler(object sender, EventArgs args)
        {
            try
            {
                if (PlatformService.AppStartedByRetroPass)
                {
                    bool gobackToRetroPass = false;

                    var gameName = System.IO.Path.GetFileName(PlatformService.RetroPassLaunchGame);
                    var gameExt = System.IO.Path.GetExtension(PlatformService.RetroPassLaunchGame);
                    var coreName = System.IO.Path.GetFileName(PlatformService.RetroPassLaunchCore);
                    GameSystemViewModel targetSystem = null;

                    foreach (var cItem in PlatformService.Consoles)
                    {
                        try
                        {
                            var cleanDLLName = System.IO.Path.GetFileNameWithoutExtension(cItem.DLLName);
                            var cleanCoreName = System.IO.Path.GetFileNameWithoutExtension(coreName);

                            if (cleanDLLName.ToLower().Equals(cleanCoreName.ToLower()))
                            {
                                if (cItem.SupportedExtensions.Contains(gameExt.ToLower()) || ArchiveStreamProvider.SupportedExtensions.Contains(gameExt.ToLower()))
                                {
                                    targetSystem = cItem;
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    if (targetSystem != null)
                    {
                        VM.SelectedSystem = targetSystem;
                        //if (PlatformService.IsCoreRequiredGamesFolderDirect(VM.SelectedSystem.Core.Name))
                        {
                            /*if (!await VM.IsCoreGamesFolderAlreadySelected(VM.folderPickerTokenName))
                            {
                                PlatformService.PlayNotificationSound("error");
                                PlatformService.ShowNotificationMain("Games folder not found", 3);
                                gobackToRetroPass = true;
                            }
                            else*/
                            {
                                bool globalRootSelected = false;
                                var systemRootFolder = await PlatformService.PickDirectory(VM.folderPickerTokenName, false);
                                if (systemRootFolder == null)
                                {
                                    systemRootFolder = await PlatformService.GetGlobalFolder();
                                    if (systemRootFolder != null)
                                    {
                                        globalRootSelected = true;
                                    }
                                }
                                if (systemRootFolder != null)
                                {
                                    PlatformService.ShowNotificationMain($"Searching for '{gameName}'", 3);
                                    List<string> fileTypeFilter = new List<string>();
                                    fileTypeFilter.Add(gameExt);
                                    QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);
                                    queryOptions.FolderDepth = PlatformService.UseWindowsIndexerSubFolders ? FolderDepth.Deep : FolderDepth.Shallow;
                                    if (PlatformService.UseWindowsIndexer)
                                    {
                                        queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                                    }
                                    queryOptions.ApplicationSearchFilter = $"System.FileName:{gameName}";
                                    StorageFileQueryResult queryResult = systemRootFolder.CreateFileQueryWithOptions(queryOptions);
                                    var files = await queryResult.GetFilesAsync();
                                    if (files != null && files.Count > 0)
                                    {
                                        VM.SystemSelected = true;
                                        VM.SystemSelectDone = true;
                                        PlatformService.OpenGameFile = files.FirstOrDefault();
                                        PrepareCoreData(true);
                                    }
                                    else
                                    {
                                        PlatformService.ShowNotificationMain($"Trying without Windows Indexer, please wait..", 3);
                                        queryOptions.IndexerOption = IndexerOption.DoNotUseIndexer;
                                        queryOptions.ApplicationSearchFilter = $"System.FileName:{gameName}";
                                        queryResult = systemRootFolder.CreateFileQueryWithOptions(queryOptions);
                                        files = await queryResult.GetFilesAsync();

                                        if (files != null && files.Count > 0)
                                        {
                                            VM.SystemSelected = true;
                                            VM.SystemSelectDone = true;
                                            PlatformService.OpenGameFile = files.FirstOrDefault();
                                            PrepareCoreData(true);
                                        }
                                        else
                                        {
                                            //Try to find the game in the global root if is not selected
                                            if (!globalRootSelected && PlatformService.RetroPassRoot)
                                            {
                                                systemRootFolder = await PlatformService.GetGlobalFolder();
                                                if (systemRootFolder != null)
                                                {
                                                    PlatformService.ShowNotificationMain($"Searching in games root, please wait..", 3);
                                                    if (PlatformService.UseWindowsIndexer)
                                                    {
                                                        queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                                                    }
                                                    queryOptions.ApplicationSearchFilter = $"System.FileName:{gameName}";
                                                    queryResult = systemRootFolder.CreateFileQueryWithOptions(queryOptions);
                                                    files = await queryResult.GetFilesAsync();
                                                    if (files != null && files.Count > 0)
                                                    {
                                                        VM.SystemSelected = true;
                                                        VM.SystemSelectDone = true;
                                                        PlatformService.OpenGameFile = files.FirstOrDefault();
                                                        PrepareCoreData(true);
                                                    }
                                                    else
                                                    {
                                                        PlatformService.ShowNotificationMain($"Trying without Windows Indexer, please wait..", 3);
                                                        queryOptions.IndexerOption = IndexerOption.DoNotUseIndexer;
                                                        queryOptions.ApplicationSearchFilter = $"System.FileName:{gameName}";
                                                        queryResult = systemRootFolder.CreateFileQueryWithOptions(queryOptions);
                                                        files = await queryResult.GetFilesAsync();

                                                        if (files != null && files.Count > 0)
                                                        {
                                                            VM.SystemSelected = true;
                                                            VM.SystemSelectDone = true;
                                                            PlatformService.OpenGameFile = files.FirstOrDefault();
                                                            PrepareCoreData(true);
                                                        }
                                                        else
                                                        {
                                                            PlatformService.PlayNotificationSound("error");
                                                            PlatformService.ShowNotificationMain("Unable to find this game", 3);
                                                            gobackToRetroPass = true;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    PlatformService.PlayNotificationSound("error");
                                                    PlatformService.ShowNotificationMain("Unable to find this game", 3);
                                                    gobackToRetroPass = true;
                                                }
                                            }
                                            else
                                            {
                                                PlatformService.PlayNotificationSound("error");
                                                PlatformService.ShowNotificationMain("Unable to find this game", 3);
                                                gobackToRetroPass = true;
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    PlatformService.PlayNotificationSound("error");
                                    PlatformService.ShowNotificationMain("Unable to get games folder", 3);
                                    gobackToRetroPass = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("error");
                        PlatformService.ShowNotificationMain("Cannot find the target system", 3);
                        gobackToRetroPass = true;
                    }
                    if (gobackToRetroPass)
                    {
                        await Task.Delay(2000);
                        PlatformService.PlayNotificationSound("alert");
                        ConfirmConfig confirmReset = new ConfirmConfig();
                        confirmReset.SetTitle("Go Back");
                        confirmReset.SetMessage($"Do you want to go back to Retropass?");
                        confirmReset.UseYesNo();

                        var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                        if (StartReset)
                        {
                            VM.SetCoresQuickAccessContainerState(true);
                            PreparingCoreLoading(false);
                            returningFromSubPage = false;
                            CorePageInProgress = false;
                            if (PlatformService.AppStartedByRetroPass)
                            {
                                try
                                {
                                    Windows.System.Launcher.LaunchUriAsync(new Uri(PlatformService.RetroPassLaunchOnExit));
                                }
                                catch (Exception exb)
                                {

                                }
                                PlatformService.AppStartedByRetroPass = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }
        bool cleanupInProgress = false;
        private async Task CleanTempFolder(bool force = false)
        {
            if ((PlatformService.TempAlreadyCleaned && !force) || cleanupInProgress)
            {
                return;
            }
            cleanupInProgress = true;
            try
            {
                //Clean Temp Folder
                var tempFolder = ApplicationData.Current.TemporaryFolder;
                if (tempFolder != null)
                {
                    var tempFiles = await tempFolder.GetFilesAsync();
                    if (tempFiles != null && tempFiles.Count > 0)
                    {
                        foreach (var file in tempFiles)
                        {
                            try
                            {
                                await file.DeleteAsync();
                            }
                            catch
                            {

                            }
                        }
                    }
                    var tempFolders = await tempFolder.GetFoldersAsync();
                    if (tempFolders != null && tempFolders.Count > 0)
                    {
                        foreach (var folder in tempFolders)
                        {
                            try
                            {
                                await folder.DeleteAsync();
                            }
                            catch
                            {

                            }
                        }
                    }
                }

                //Clean temp in LocalFolder
                if (App.CleanTempOnStartup)
                {
                    var localFolder = ApplicationData.Current.LocalFolder;
                    var tempLocalFolder = await localFolder.CreateFolderAsync("Temporary", CreationCollisionOption.OpenIfExists);
                    if (tempLocalFolder != null)
                    {
                        var tempFiles = await tempLocalFolder.GetFilesAsync();
                        if (tempFiles != null && tempFiles.Count > 0)
                        {
                            foreach (var file in tempFiles)
                            {
                                try
                                {
                                    await file.DeleteAsync();
                                }
                                catch
                                {

                                }
                            }
                        }
                        var tempFolders = await tempLocalFolder.GetFoldersAsync();
                        if (tempFolders != null && tempFolders.Count > 0)
                        {
                            foreach (var folder in tempFolders)
                            {
                                try
                                {
                                    await folder.DeleteAsync();
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
                WriteLog($"{ex.Message}");
            }
            PlatformService.TempAlreadyCleaned = true;
            cleanupInProgress = false;
        }

        #region Update Memory
        SolidColorBrush memoryUsageColor = new SolidColorBrush(Colors.Green);
        SolidColorBrush MemoryUsageColor
        {
            get
            {
                return memoryUsageColor;
            }
            set
            {
                MemoryValue.Foreground = value;
                memoryUsageColor = value;
            }
        }

        string totalMUsage = "...";
        string TotalMUsage
        {
            get
            {
                return totalMUsage;
            }
            set
            {
                MemoryValue.Text = value;
                totalMUsage = value;
            }
        }

        private void callMemoryTimerTimer(bool startState = false)
        {
            try
            {
                PlatformService.MemoryTimer?.Dispose();
                if (startState)
                {
                    PlatformService.MemoryTimer = new System.Threading.Timer(delegate { UpdateTotalMemoryUsage(); }, null, 0, 1000);
                }
            }
            catch (Exception e)
            {

            }
        }

        bool totalMemoryUsageInProgress;
        private async void UpdateTotalMemoryUsage()
        {
            try
            {
                if (totalMemoryUsageInProgress || App.GameStarted)
                {
                    return;
                }
                totalMemoryUsageInProgress = true;
                var TotalMUsageValue = "...";
                var MemoryLoadColor = Colors.Green;
                var MemoryLoadColor2 = Colors.Green;

                uint MEMOVALUE = 0;

                try
                {
                    MEMORYSTATUSEX memoryStatus = new MEMORYSTATUSEX();
                    GlobalMemoryStatusEx(memoryStatus);
                    MEMOVALUE = memoryStatus.dwMemoryLoad;
                }
                catch (Exception ex)
                {

                }
                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                await Task.Run(async () =>
                {
                    try
                    {
                        TotalMUsageValue = PlatformService.GetMemoryUsageDirect();
                        if (MEMOVALUE > 0)
                        {
                            TotalMUsageValue = $"{TotalMUsageValue} ({MEMOVALUE}%)";
                        }
                        taskCompletionSource.SetResult(true);
                    }
                    catch (Exception ex)
                    {

                    }
                });
                await taskCompletionSource.Task;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
                {
                    try
                    {
                        TotalMUsage = TotalMUsageValue;
                        MemoryUsageColor = new SolidColorBrush(MemoryLoadColor);
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
            {

            }
            totalMemoryUsageInProgress = false;
        }
        #endregion


        public async void CheckPlayTimeTemp()
        {
            try
            {
                var temp = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("GamePlayTime", "");
                if (temp != null && temp.Length > 0)
                {
                    var data = temp.Split('|');
                    var CoreNameClean = data[0];
                    var SystemName = data[1];
                    var MainFilePath = data[2];
                    var RootNeeded = bool.Parse(data[3]);
                    var GameID = data[4];
                    var PlayedTime = long.Parse(data[5]);
                    var IsNewCore = bool.Parse(data[6]);
                    var delete = bool.Parse(data[7]);
                    await PlatformService.AddGameToRecents(CoreNameClean, SystemName, MainFilePath, RootNeeded, GameID, PlayedTime, IsNewCore, delete);

                    //This step made to avoid losing statistics if the app crash
                    //values will be updated next startup
                    //when play time saved I should reset it.
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GamePlayTime", "");
                }
            }
            catch (Exception ex)
            {
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GamePlayTime", "");
            }
        }
        private void BindingsUpdate()
        {
            try
            {
                _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
            {
                try
                {
                    Bindings.Update();
                }
                catch (Exception e)
                {

                }
            });
            }
            catch (Exception e)
            {

            }
        }

        public string waitMusic()
        {
            try
            {
                var musics = new string[] { "wait_music 01.mp3", "wait_music 02.mp3", "wait_music 03.mp3", "wait_music 04.mp3" };
                var index = new Random().Next(0, musics.Length - 1);
                return musics[index];
            }
            catch (Exception xe)
            {
                return "wait_music 01.mp3";
            }
        }
        private void ShowLoader(bool state, string message = "Please Wait..")
        {
            if (VM != null && state)
            {
                VM.SystemCoreIsLoadingState(false);
            }
            _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
            {
                if (!PlatformService.MuteSFX)
                {
                    if (state)
                    {
                        try
                        {
                            if (ProgressContainerMain.Visibility == Visibility.Collapsed)
                            {
                                PlayWaitMusic();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        StopWaitMusic();
                    }
                }
                ProgressContainerMain.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
                SystemCoresLoadingProgress.IsActive = state;
                LoadingState.Text = message;
                if (!state)
                {
                    await Task.Delay(10);
                    if (RecentsPanel.Visibility == Visibility.Collapsed)
                    {
                        //In some cases the main container stuck disabled
                        //hope this here will ensure it will be enabled in case cores page hidden
                        CoresQuickAccessContainer.IsEnabled = true;
                    }
                }
            });
        }
        private void PreparingCoreLoading(bool state, string message = "Please Wait..")
        {
            _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
            {
                PreparingCoreProgress.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
                PreparingCoreProgressBar.IsActive = state;
                PreparingCoreProgressState.Text = message;
            });
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
        private async void pushLocalNotification(string text, Windows.UI.Color background, Windows.UI.Color forground, char icon = '\0', int time = 3, Position position = Position.Bottom, EventHandler eventHandler = null)
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
                    pushLocalNotification(NotificationData.message, Windows.UI.Colors.DodgerBlue, Windows.UI.Colors.White, NotificationData.icon, NotificationData.time, DefaultPosition);
                }
            }
            catch (Exception ex)
            {

            }
        }
        bool restoreInProgress = false;
        private async void restoreListPosition(object sender = null, EventArgs args = null)
        {
            if (restoreInProgress)
            {
                return;
            }
            restoreInProgress = true;
            try
            {
                while (ProgressContainerMain.Visibility == Visibility.Visible)
                {
                    await Task.Delay(100);
                }
                try
                {
                    SystemRecentsList.UpdateLayout();
                    CoresQuickAccessContainer.UpdateLayout();
                    CoreQuickAccessContainer.UpdateLayout();
                }
                catch (Exception ex)
                {

                }

                try
                {
                    if (PlatformService.gamesListAlreadyLoaded)
                    {
                        while (!gamesListLoaded)
                        {
                            await Task.Delay(500);
                        }
                    }
                    ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(SystemRecentsList).FirstOrDefault();
                    if (svl != null)
                    {
                        svl.ChangeView(0, PlatformService.vScroll, 1);
                    }

                    CoresQuickAccessContainer.ChangeView(0, PlatformService.vScrollS, 1);
                    CoreQuickAccessContainer.ChangeView(0, PlatformService.pivotMainPosition, 1);
                    if (PlatformService.SubPageActive && CorePagePivot.SelectedItem == PivotCoreMain)
                    {
                        if (item2 != null)
                        {
                            item2.Focus(FocusState.Programmatic);
                        }
                    }
                }
                catch (Exception ex)
                {
                    PlatformService.ShowErrorMessageDirect(ex);
                }
            }
            catch (Exception ex)
            {

            }
            restoreInProgress = false;
        }

        private void GameSystemSelectionView_eventHandler(object sender, EventArgs e)
        {
            try
            {
                SystemRecentsList.UpdateLayout();
                CoresQuickAccessContainer.UpdateLayout();
                CoreQuickAccessContainer.UpdateLayout();
            }
            catch (Exception ex)
            {
            }
            try
            {
                ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(SystemRecentsList).FirstOrDefault();
                if (svl != null)
                {
                    PlatformService.vScroll = svl.VerticalOffset;
                }


                PlatformService.vScrollS = CoresQuickAccessContainer.VerticalOffset;
                PlatformService.pivotPosition = CorePagePivot.SelectedIndex;
                PlatformService.pivotMainPosition = CoreQuickAccessContainer.VerticalOffset;

                PlatformService.gameSystemViewModel = (GameSystemRecentModel)sender;
            }
            catch (Exception ex)
            {
            }
        }

        public async void UpdateBindingDelay(bool delayRequest = true)
        {
            try
            {
                if (delayRequest)
                {
                    await Task.Delay(2000);
                }
                BindingsUpdate();
            }
            catch (Exception e)
            {
            }
        }

        bool optionsGenerated = false;
        private void CoreOptionsPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var targetItem = (CoreOptionListItem)e.ClickedItem;
                targetItem.DroppedDown = !targetItem.DroppedDown;
            }
            catch (Exception ex)
            {

            }
        }
        private async Task ShowSelectedSystemOptions(bool forceReload = false)
        {
            try
            {
                if (optionsGenerated && !forceReload)
                {
                    return;
                }
                isOptionsGenerateInProgress = true;
                coreOptionsGroupped.Clear();
                var TargetSystem = VM.SelectedSystem.TempName;
                var expectedName = $"{VM.SelectedSystem.Core.Name}_{TargetSystem}";
                CoresOptions testOptions;
                await VM.OptionsRetrieveAsync(VM.SelectedSystem);
                if (!VM.CoresOptionsDictionary.TryGetValue(expectedName, out testOptions))
                {
                    if (!VM.SelectedSystem.Core.IsNewCore && VM.CoresOptionsDictionary.TryGetValue(TargetSystem, out testOptions))
                    {
                        expectedName = TargetSystem;
                    }
                    else
                    {
                        return;
                    }
                }
                var CurrentOptions = VM.CoresOptionsDictionary[expectedName].OptionsList.Keys;
                if (CurrentOptions.Count > 0)
                {
                    var optionsCount = 0;
                    NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    OptionsReset.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    OptionsSave.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    Dictionary<string, GroupCoreOptionListItems> groups = new Dictionary<string, GroupCoreOptionListItems>();
                    foreach (var CoreItem in CurrentOptions)
                    {
                        var OptionValues = VM.CoresOptionsDictionary[expectedName].OptionsList[CoreItem];
                        var optionName = OptionValues.OptionsDescription.Replace("(Needs Restart)", "").Replace("(restart)", "").Replace("(Restart)", "").Replace("(Need Restart)", "").Replace("Need", "").Replace("Restart", "");
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
                        if (VM.SelectedSystem == null)
                        {
                            return;
                        }
                        var SystemName = VM.SelectedSystem.TempName;
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
                    PivotCoreOptions.Header = $"Options ({optionsCount})";
                    optionsGenerated = true;
                    isOptionsGenerateInProgress = false;
                    restoreOptionsListPosition();
                }
                else
                {
                    PivotCoreOptions.Header = $"Options";
                    NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    OptionsReset.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    OptionsSave.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                PlatformService.checkInitWidth(false);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        bool isOptionsGenerateInProgress = false;
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

                VM.setCoresOptionsDictionary(VM.SelectedSystem, TargetKey, (uint)TargetIndex);
                PlatformService.PlayNotificationSoundDirect("option-changed");
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private void SelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            //Core here is causing some issue because it's getting triggered while list scrolling
            //for now will be disabled and will be handled in ComboBox_DropDownClosed
            /*try
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
                if (TargetIndexTemp.Equals(TargetIndex))
                {
                    return;
                }
                var TargetKey = TargetData[0];
                ((ComboBox)sender).Tag = $"{TargetKey}|{TargetIndex}";
                VM.setCoresOptionsDictionary(VM.SelectedSystem, TargetKey, (uint)TargetIndex);
                PlatformService.PlayNotificationSoundDirect("option-changed");
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }*/
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

        private void GamesPick_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("select");
            VM.BrowseSingleGame();
        }
        private void GamesCancel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HideSystemGames();
        }
        public async void HideSystemGames()
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        try
                        {
                            screenshotsGetterCancellation.Cancel();
                        }
                        catch (Exception ex)
                        {

                        }
                        CoresQuickAccessContainer.IsEnabled = true;
                        MemoryValueContainer.Visibility = Visibility.Visible;
                        MemoryValueContainer.Margin = new Thickness(0, 14, 17, 5);
                        RecentsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        CorePageInProgress = false;
                        VM.GamesListVisible = false;
                        VM.SelectedSystem = null;
                        PlatformService.gamesListAlreadyLoaded = false;
                        VM.GamesMainList.Clear();
                        VM.GamesMainListTemp.Clear();
                        VM.GamesRecentsList.Clear();
                        VM.ClearSelectedSystem();
                        dataSourceScreenshots.Clear();
                        dataSourceRecents.Clear();

                        PivotCoreGames.Header = "Games";
                        PivotCoreBIOS.Header = "BIOS";
                        PivotCoreOptions.Header = "Options";

                        PlatformService.PlayNotificationSoundDirect("button-01");
                        PlatformService.SubPageActive = false;
                        PlatformService.isGamesList = false;
                        optionsGenerated = false;
                        PlatformService.vScroll = 0;
                        CorePagePivot.SelectedIndex = 0;
                        PlatformService.pivotPosition = 0;
                        daysData.Clear();
                    }
                    catch (Exception ex)
                    {
                        PlatformService.ShowErrorMessage(ex);
                    }
                    try
                    {
                        CoreQuickMenu.Clear();
                        if (PlatformService.PreventGCAlways) GC.SuppressFinalize(CoreQuickMenu);
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        SystemRecentsList.UpdateLayout();
                        ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(SystemRecentsList).FirstOrDefault();
                        if (svl != null)
                        {
                            svl.ChangeView(0, PlatformService.vScroll, 1);
                        }
                    }
                    catch
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    await Task.Delay(50);
                    CoresQuickAccessContainer.ChangeView(0, PlatformService.vScrollS, 1);
                    if (item != null)
                    {
                        item.Focus(FocusState.Programmatic);
                    }
                });
            }
            catch (Exception ex)
            {

            }
        }


        public event EventHandler eventHandler;
        public event EventHandler GamesEventHandler;
        public event EventHandler NoGamesEventHandler;
        private async void AllGetter_Click(bool reloadGamesList = false, bool ignorePicker = false)
        {
            if (PlatformService.gamesListAlreadyLoaded)
            {
                try
                {
                    //await Task.Delay(1000);
                    while (!gamesListLoaded)
                    {
                        await Task.Delay(500);
                    }
                    if (VM.GamesMainList.Count == 0)
                    {
                        AllGetterReload_Click(null, null);
                    }
                    else
                    {
                        SystemRecentsList.UpdateLayout();
                        ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(SystemRecentsList).FirstOrDefault();
                        if (svl != null)
                        {
                            svl.ChangeView(0, PlatformService.vScroll, 1);
                        }
                        PlatformService.checkInitWidth(false);
                        BindingsUpdate();
                        VM.ReloadGamesVisibleState(true);
                        PivotCoreGames.Header = $"Games ({VM.GamesMainList.Count})";
                        VM.ReloadGamesVisibleState(true);
                        MemoryValueContainer.Margin = new Thickness(0, 60, 20, 5);
                        buildParentFilters();
                    }
                    PivotCoreGames.UpdateLayout();
                }
                catch
                {

                }

                return;
            }
            if (!returningFromSubPage)
            {
                PlatformService.vScroll = 0;
            }
            PlatformService.PlayNotificationSoundDirect("button-01");
            GamesEventHandler = async (sender, e) =>
            {
                PlatformService.gamesListAlreadyLoaded = true;
                ReloadCorePage_Click(null, null);
                MemoryValueContainer.Margin = new Thickness(0, 60, 20, 5);
                buildParentFilters();
                VM.searchCleardByGetter = false;
                VM.gamesLoadingInProgress = false;
                try
                {
                    {
                        try
                        {
                            StopWaitMusic();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                catch (Exception ex)
                {

                }
                //PivotCoreGames.Header = $"Games ({sender})";
                //PivotCoreGames.UpdateLayout();
            };
            NoGamesEventHandler = async (sender, e) =>
            {
                try
                {
                    StopWaitMusic();
                }
                catch (Exception ex)
                {

                }
            };
            if (!PlatformService.MuteSFX)
            {
                {
                    try
                    {
                        {
                            PlayWaitMusic();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            VM.GetAllGames(GamesEventHandler, reloadGamesList, ignorePicker, NoGamesEventHandler);
            GamesListFilter.PlaceholderText = "Search in games..";
            PlatformService.checkInitWidth(false);
            PlatformService.isGamesList = true;
        }

        bool VFSSupport
        {
            get
            {
                if (ShowGameMenuOptions != Visibility.Visible)
                {
                    return false;
                }
                var vfsState = false;
                if (VM.SelectedSystem != null)
                {
                    vfsState = VM.SelectedSystem.Core.VFSSupport;
                }
                return vfsState;
            }
            set
            {
                if (VM.SelectedSystem != null)
                {
                    VM.SelectedSystem.Core.VFSSupport = value;
                    BindingsUpdate();
                }
            }
        }

        bool ZipSupport
        {
            get
            {
                var zipState = false;
                if (VM.SelectedSystem != null)
                {
                    zipState = VM.SelectedSystem.Core.NativeArchiveSupport;
                }
                return zipState;
            }
            set
            {
                if (VM.SelectedSystem != null)
                {
                    VM.SelectedSystem.Core.NativeArchiveSupport = value;
                    BindingsUpdate();
                }
            }
        }
        bool filterBuildInProgress = false;
        private void buildParentFilters()
        {
            try
            {
                filterBuildInProgress = true;
                if (VM.GamesMainListTemp != null && VM.GamesMainListTemp.Count > 1 && (VM.GamesSubFolders.Count > 1 || VM.SelectedSystem.SupportedExtensions.Count > 1))
                {
                    SearchExtraFilter.Items.Clear();

                    var AllFiles = new FilterItem("Show All", "", "NONE");
                    var CSystem = new FilterItem("Core System", "core:system", "SYSTEM");
                    var CSave = new FilterItem("Core Saves", "core:saves", "SAVES");

                    SearchExtraFilter.Items.Add(AllFiles);
                    SearchExtraFilter.Items.Add(CSystem);
                    SearchExtraFilter.Items.Add(CSave);
                    if (VM.ParentFilter == "")
                    {
                        SearchExtraFilter.SelectedItem = AllFiles;
                    }

                    bool showFilters = false;
                    if (VM.GamesSubFolders.Count > 1)
                    {
                        foreach (var gItem in VM.GamesSubFolders.OrderBy(a => a))
                        {
                            var filterValue = $"parent:{gItem}";
                            var GameFolder = new FilterItem(gItem, filterValue, "FOLDER");

                            SearchExtraFilter.Items.Add(GameFolder);
                            if (VM.ParentFilter.Equals(filterValue))
                            {
                                SearchExtraFilter.SelectedItem = GameFolder;
                            }
                        }
                        showFilters = true;
                    }

                    if (VM.GamesExtens.Count > 0 && VM.SelectedSystem.Core.SupportedExtensions != null && VM.SelectedSystem.Core.SupportedExtensions.Count > 1 && !VM.SelectedSystem.Core.SupportedExtensions.FirstOrDefault().Equals(".null"))
                    {
                        var indexer = 0;
                        foreach (var eItem in VM.SelectedSystem.SupportedExtensions)
                        {
                            if (VM.GamesExtens.Contains(eItem))
                            {
                                var filterValue = $"ext:{eItem}";
                                var GameExten = new FilterItem(eItem, filterValue, "EXTEN");

                                SearchExtraFilter.Items.Add(GameExten);
                                if (VM.ParentFilter.Equals(filterValue))
                                {
                                    SearchExtraFilter.SelectedItem = GameExten;
                                }
                                indexer++;
                            }
                        }
                        if (indexer >= (PlatformService.isXBOX || PlatformService.DPadActive ? 0 : 2))
                        {
                            showFilters = true;
                        }
                    }
                    if (showFilters)
                    {
                        SearchExtraFilterContainer.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SearchExtraFilterContainer.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    SearchExtraFilterContainer.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            filterBuildInProgress = false;
        }

        private async void SearchExtraFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!filterBuildInProgress)
                {
                    var item = (FilterItem)e.AddedItems?.FirstOrDefault();
                    if (item != null)
                    {
                        VM.ParentFilter = item.FilterValue;
                        filterHandler = (fsender, fe) =>
                        {
                            PivotCoreGames.Header = $"Games ({fsender})";
                        };
                        var CurrentText = VM.SearchText;
                        await VM.FilterCurrentGamesList(CurrentText, filterHandler);
                    }
                    BindingsUpdate();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void AllGetterReload_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlatformService.vScroll = 0;
            PlatformService.PlayNotificationSoundDirect("button-01");
            GamesEventHandler = async (rsender, re) =>
            {
                try
                {
                    try
                    {
                        PlatformService.gamesListAlreadyLoaded = true;

                        if (VM.GamesMainList.Count > 0)
                        {
                            PivotCoreGames.Header = $"Games ({rsender})";
                        }
                        else
                        {
                            PivotCoreGames.Header = $"Games";
                        }
                        VM.ReloadGamesVisibleState(true);
                        if (!gamesFolderReady)
                        {
                            ReloadCorePageHandler(null, null);
                        }
                        VM.searchCleardByGetter = false;
                        buildParentFilters();
                        VM.gamesLoadingInProgress = false;
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }
                try
                {
                    {
                        try
                        {
                            StopWaitMusic();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                catch (Exception ex)
                {

                }
            };
            if (!PlatformService.MuteSFX)
            {
                {
                    try
                    {
                        {
                            PlayWaitMusic();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            NoGamesEventHandler = async (senders, es) =>
            {
                try
                {
                    StopWaitMusic();
                }
                catch (Exception ex)
                {

                }
            };
            VM.GetAllGames(GamesEventHandler, true, false, NoGamesEventHandler);
            GamesListFilter.PlaceholderText = "Search in games..";
            PlatformService.checkInitWidth(false);
            PlatformService.isGamesList = true;
        }

        private async void PlayWaitMusic()
        {
            try
            {
                _player.Stop();
            }
            catch (Exception ex)
            {

            }
            try
            {
                StorageFolder folder = (StorageFolder)await Windows.ApplicationModel.Package.Current.InstalledLocation.TryGetItemAsync(@"Assets\SFX");
                StorageFile file = (StorageFile)await folder.TryGetItemAsync(waitMusic());
                if (file != null)
                {
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    _player.Volume = 0.3;
                    _player.IsLooping = true;
                    _player.SetSource(stream, file.ContentType);
                    _player.Play();
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void StopWaitMusic()
        {
            try
            {
                var vol = 0.3;
                while (vol > 0)
                {
                    vol -= 0.01;
                    _player.Volume = vol;
                    await Task.Delay(10);
                }
                _player.Stop();
                _player.Source = null;
            }
            catch (Exception ex)
            {

            }
        }
        private void RecentsList_DataContextChanged(Windows.UI.Xaml.FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs args)
        {
            try
            {
                if (eventHandler != null)
                {
                    eventHandler.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private bool pushNormalNotification(string message, char icon = SegoeMDL2Assets.ActionCenterNotification, int time = 3, EventHandler eventHandler = null)
        {
            try
            {
                pushLocalNotification(message, Windows.UI.Colors.DodgerBlue, Windows.UI.Colors.White, icon, time, DefaultPosition, eventHandler);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private async void OptionsReset_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (VM.SelectedSystem.SkippedCore || VM.SelectedSystem.FailedToLoad)
                {
                    if (!pushNormalNotification($"This core has loading issue, please fix it first!"))
                    {
                        _ = UserDialogs.Instance.AlertAsync($"This core has loading issue, please fix it first!");
                    }
                    return;
                }
                PlatformService.PlayNotificationSoundDirect("button-01");
                {
                    var TargetSystem = VM.SelectedSystem.TempName;
                    if (TargetSystem != null && TargetSystem?.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmReset = new ConfirmConfig();
                        confirmReset.SetTitle("Reset Options");
                        confirmReset.SetMessage($"Do you want to reset {TargetSystem}'s options to the default values? ");
                        confirmReset.UseYesNo();

                        var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                        if (StartReset)
                        {
                            saveOptionsListPosition();
                            await VM.resetCoresOptionsDictionary(VM.SelectedSystem);
                            PlatformService.PlayNotificationSoundDirect("success");
                            if (!pushNormalNotification($"{TargetSystem}'s options reseted"))
                            {
                                await UserDialogs.Instance.AlertAsync($"{TargetSystem}'s options reseted", "Reset Done");
                            }

                            await ShowSelectedSystemOptions(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void OptionsSave_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (VM.SelectedSystem.SkippedCore || VM.SelectedSystem.FailedToLoad)
                {
                    if (!pushNormalNotification($"This core has loading issue, please fix it first!"))
                    {
                        _ = UserDialogs.Instance.AlertAsync($"This core has loading issue, please fix it first!");
                    }
                    return;
                }
                PlatformService.PlayNotificationSoundDirect("button-01");
                {
                    var TargetSystem = VM.SelectedSystem.TempName;
                    if (TargetSystem != null && TargetSystem?.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmSave = new ConfirmConfig();
                        confirmSave.SetTitle("Save Options");
                        confirmSave.SetMessage($"Do you want to save {TargetSystem}'s options as default values? ");
                        confirmSave.UseYesNo();

                        var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                        if (StartSave)
                        {
                            await VM.CoreOptionsStoreAsync(VM.SelectedSystem);
                            PlatformService.PlayNotificationSoundDirect("success");
                            if (!pushNormalNotification($"{TargetSystem}'s options has been saved"))
                            {
                                await UserDialogs.Instance.AlertAsync($"{TargetSystem}'s options has been saved", "Save Done");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        void HideSubPages(object sender, EventArgs e)
        {
            HideSystemGames();
        }


        EventHandler filterHandler;
        private async void GamesListFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                filterHandler = (fsender, fe) =>
                {
                    PivotCoreGames.Header = $"Games ({fsender})";
                };
                var CurrentText = ((TextBox)sender).Text.Trim();
                await VM.FilterCurrentGamesList(CurrentText, filterHandler);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        StorageFile IconFile = null;
        private async void ConsoleSettings_Click()
        {
            try
            {
                IconFile = null;
                PlatformService.PlayNotificationSoundDirect("button-01");

                int left = 0;
                int top = 3;
                int right = 0;
                int bottom = 5;


                TextBox NameTextBox = new TextBox();
                NameTextBox.AcceptsReturn = false;
                NameTextBox.Height = 32;
                NameTextBox.Text = VM.SelectedSystem.Name;
                NameTextBox.VerticalAlignment = VerticalAlignment.Center;
                NameTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                NameTextBox.PlaceholderText = "Core Name";
                NameTextBox.Margin = new Thickness(left, top, right, bottom);

                TextBox ManufacturerTextBox = new TextBox();
                ManufacturerTextBox.AcceptsReturn = false;
                ManufacturerTextBox.Height = 32;
                ManufacturerTextBox.Text = VM.SelectedSystem.ManufacturerTemp;
                ManufacturerTextBox.VerticalAlignment = VerticalAlignment.Center;
                ManufacturerTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                ManufacturerTextBox.PlaceholderText = "Manufacturer";
                ManufacturerTextBox.Margin = new Thickness(left, top, right, bottom);

                CheckBox checkBoxCD = new CheckBox();
                checkBoxCD.IsChecked = VM.SelectedSystem.CDSupport;
                checkBoxCD.Content = "CD Support";
                checkBoxCD.VerticalAlignment = VerticalAlignment.Center;
                checkBoxCD.HorizontalAlignment = HorizontalAlignment.Left;
                checkBoxCD.Margin = new Thickness(left, top, right, bottom);

                Image SystemImage = new Image();
                SystemImage.Height = 75;
                SystemImage.Width = 75;
                BitmapImage ImageSource = new BitmapImage(new Uri(VM.SelectedSystem.Symbol));
                SystemImage.Source = ImageSource;
                SystemImage.VerticalAlignment = VerticalAlignment.Center;
                SystemImage.Stretch = Windows.UI.Xaml.Media.Stretch.Uniform;
                SystemImage.Margin = new Thickness(left, top, right, bottom);
                Button imageButton = new Button();
                imageButton.VerticalAlignment = VerticalAlignment.Center;
                imageButton.HorizontalAlignment = HorizontalAlignment.Center;
                imageButton.Content = SystemImage;


                try
                {
                    imageButton.Click -= SelectImage;
                }
                catch (Exception x) { }
                imageButton.Click += SelectImage;

                StackPanel dialogContent = new StackPanel();
                dialogContent.Children.Add(NameTextBox);
                dialogContent.Children.Add(ManufacturerTextBox);
                dialogContent.Children.Add(checkBoxCD);
                dialogContent.Children.Add(imageButton);

                ContentDialog dialog = new ContentDialog();
                dialog.Content = dialogContent;
                dialog.Title = $"{VM.SelectedSystem.Name} Settings";
                dialog.IsSecondaryButtonEnabled = true;
                dialog.PrimaryButtonText = "Save";
                dialog.SecondaryButtonText = "Cancel";
                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    updateAnyCore(NameTextBox.Text, ManufacturerTextBox.Text, await SystemIconImport(IconFile));
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }


        }
        private async void SelectImage(object sender, object e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                FileOpenPicker picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".bmp");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".gif");


                IconFile = await picker.PickSingleFileAsync();
                if (IconFile != null)
                {
                    using (IRandomAccessStream fileStream = await IconFile.OpenAsync(FileAccessMode.Read))
                    {
                        // Set the image source to the selected bitmap
                        BitmapImage bitmapImage = new BitmapImage();
                        // Decode pixel sizes are optional
                        // It's generally a good optimisation to decode to match the size you'll display
                        bitmapImage.DecodePixelHeight = 75;
                        bitmapImage.DecodePixelWidth = 75;

                        await bitmapImage.SetSourceAsync(fileStream);
                        ((Image)sender).Source = bitmapImage;
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private async void updateAnyCore(string AnyCoreName, string AnyCoreManufactur, string AnyCoreSaveIconValue)
        {
            try
            {
                var CurrentSystem = VM.SelectedSystem;
                //Issue: If only image changed dialog is not appear?
                if (CurrentSystem != null)
                {
                    PlatformService.PlayNotificationSoundDirect("notice");
                    ConfirmConfig confirCoreSettings = new ConfirmConfig();
                    confirCoreSettings.SetTitle("Save settings?");
                    confirCoreSettings.SetMessage("Do you want to save the current settings?");
                    confirCoreSettings.UseYesNo();
                    bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                    if (confirCoreSettingsState)
                    {
                        await VM.SetAnyCoreInfo(CurrentSystem.TempName, AnyCoreName, AnyCoreManufactur, AnyCoreSaveIconValue);
                        PlatformService.PlayNotificationSoundDirect("success");
                        PlatformService.ShowNotificationMain("Core info updated successfully", 3);
                        RetriXGoldCoresLoader(true);
                    }

                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }
        }

        private async Task<string> SystemIconImport(StorageFile file)
        {
            try
            {
                if (file != null)
                {
                    StorageFolder zipsDirectory = null;
                    var localFolder = ApplicationData.Current.LocalFolder;
                    zipsDirectory = (StorageFolder)await localFolder.TryGetItemAsync("AnyCore");
                    if (zipsDirectory == null)
                    {
                        zipsDirectory = await localFolder.CreateFolderAsync("AnyCore");
                    }
                    string targetFileName = $"{VM.SelectedSystem.Core.CoreFileName}{System.IO.Path.GetExtension(file.Name)}";
                    var targetFile = await file.CopyAsync(zipsDirectory, targetFileName, NameCollisionOption.ReplaceExisting);

                    if (targetFile != null)
                    {
                        return targetFile.Path;
                    }
                }
            }
            catch (Exception ex)
            {

                PlatformService.ShowErrorMessageDirect(ex);

            }
            return "";
        }

        private async void AnyCoreAdvancedImport()
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                PlatformService.PlayNotificationSoundDirect("notice");
                ConfirmConfig confirCoreSettings = new ConfirmConfig();
                confirCoreSettings.SetTitle("Advanced Import?");
                confirCoreSettings.SetMessage("1-You can update the core\n2-You can import BIOS map\n\nDo you want to start?");
                confirCoreSettings.UseYesNo();
                bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                if (!confirCoreSettingsState)
                {
                    return;
                }

                FileOpenPicker picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.List;
                picker.SuggestedStartLocation = PickerLocationId.Downloads;
                picker.FileTypeFilter.Add(".rab");
                picker.FileTypeFilter.Add(".dll");

                StorageFile file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    VM.SystemCoreIsLoadingState(true, "Please wait..");
                    StorageFolder zipsDirectory = null;
                    var localFolder = ApplicationData.Current.LocalFolder;
                    zipsDirectory = (StorageFolder)await localFolder.TryGetItemAsync("AnyCore");
                    if (zipsDirectory == null)
                    {
                        zipsDirectory = await localFolder.CreateFolderAsync("AnyCore");
                    }
                    var isDLL = System.IO.Path.GetExtension(file.Name.ToLower()).Equals(".dll");
                    if (isDLL)
                    {
                        VM.SystemCoreIsLoadingState(true, "Updating Core..");
                    }
                    else
                    {
                        VM.SystemCoreIsLoadingState(true, "Updating BIOS-Map..");
                    }
                    string targetFileName = VM.SelectedSystem.DLLName.Replace(".dll", "") + (isDLL ? ".dll" : ".rab");

                    if (isDLL)
                    {
                        VM.SelectedSystem.Core.FreeLibretroCore();
                    }
                    var targetFile = await file.CopyAsync(zipsDirectory, targetFileName, NameCollisionOption.ReplaceExisting);

                    if (targetFile != null)
                    {
                        //Reset skipped list entry in case the core skipped or failed
                        if (VM.SelectedSystem.DLLName != null)
                        {
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(VM.SelectedSystem.DLLName, false);
                        }
                        if (VM.SelectedSystem.ConsoleName != null && VM.SelectedSystem.ConsoleName.Length > 0)
                        {
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(VM.SelectedSystem.ConsoleName, false);
                        }
                        await Task.Delay(2000);

                        PlatformService.PlayNotificationSoundDirect("success");
                        PlatformService.ShowNotificationMain($"{(isDLL ? "Core" : "BIOS map")} has been updated", 3);
                        RetriXGoldCoresLoader(true);
                    }
                    else
                    {
                        VM.SystemCoreIsLoadingState(false);
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
                VM.SystemCoreIsLoadingState(false);
            }
        }

        private async void AnyCoreDelete_Click()
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmDelete = new ConfirmConfig();
                confirmDelete.SetTitle("Delete Core");
                confirmDelete.SetMessage($"Do you want to delete {VM.SelectedSystem.Name}?");
                confirmDelete.UseYesNo();

                var confirmDeleteState = await UserDialogs.Instance.ConfirmAsync(confirmDelete);
                if (confirmDeleteState)
                {
                    ShowLoader(true, "Please wait..");
                    StorageFolder zipsDirectory = null;
                    zipsDirectory = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("AnyCore");

                    string targetFileName = System.IO.Path.GetFileName(VM.SelectedSystem.Core.DLLName + ((VM.SelectedSystem.Core.DLLName.ToLower().EndsWith(".dll") ? "" : ".dll")));
                    var targetFielTest = (StorageFile)await zipsDirectory.TryGetItemAsync(targetFileName);
                    if (targetFielTest != null)
                    {
                        VM.SelectedSystem.Core.FreeLibretroCore();
                        await targetFielTest.DeleteAsync();
                        await VM.DeleteAnyCore(VM.SelectedSystem.TempName, true);

                        string BIOSFileName = System.IO.Path.GetFileName(((VM.SelectedSystem.Core.DLLName.ToLower().EndsWith(".dll") ? VM.SelectedSystem.Core.DLLName.Replace(".dll", "") : VM.SelectedSystem.DLLName.Replace(".dll", "")) + ".rab"));
                        var targetBIOSTest = (StorageFile)await zipsDirectory.TryGetItemAsync(BIOSFileName);
                        if (targetBIOSTest != null)
                        {
                            await targetBIOSTest.DeleteAsync();
                        }
                        PlatformService.PlayNotificationSoundDirect("success");


                        //Reset skipped list entry in case the core skipped or failed
                        if (VM.SelectedSystem.DLLName != null)
                        {
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(VM.SelectedSystem.DLLName, false);
                        }
                        if (VM.SelectedSystem.ConsoleName != null && VM.SelectedSystem.ConsoleName.Length > 0)
                        {
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(VM.SelectedSystem.ConsoleName, false);
                        }
                        await Task.Delay(2000);

                        VM.setGamesListState(false);
                        //VM.SelectedSystem.Core.RestartRequired = true;
                        PlatformService.ShowNotificationMain($"{VM.SelectedSystem.Name} Deleted", 3);
                        RetriXGoldCoresLoader(true, true);
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("faild");
                        if (!pushNormalNotification($"{VM.SelectedSystem.Name} not found!"))
                        {
                            await UserDialogs.Instance.AlertAsync($"{VM.SelectedSystem.Name} not found!");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
            ShowLoader(false);
        }

        private async void AnyCoreBiosSample_Click()
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmDelete = new ConfirmConfig();
                confirmDelete.SetTitle("Save Sample");
                confirmDelete.SetMessage($"This option will help you to save\nBIOS Map sample file\nAfter saving the file open it using any text editor\n\nDo you want to start?");
                confirmDelete.UseYesNo();

                var confirmDeleteState = await UserDialogs.Instance.ConfirmAsync(confirmDelete);
                if (confirmDeleteState)
                {

                    FolderPicker picker = new FolderPicker();
                    picker.SuggestedStartLocation = PickerLocationId.Downloads;
                    picker.ViewMode = PickerViewMode.List;
                    picker.SuggestedStartLocation = PickerLocationId.Downloads;
                    picker.FileTypeFilter.Add(".rab");

                    var folder = await picker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        VM.SystemCoreIsLoadingState(true, "Saving BIOS Map..");
                        var BIOSMapFile = (StorageFile)await Package.Current.InstalledLocation.TryGetItemAsync("BIOS-Map.rab");
                        if (BIOSMapFile != null)
                        {
                            await BIOSMapFile.CopyAsync(folder, BIOSMapFile.Name, NameCollisionOption.GenerateUniqueName);
                            PlatformService.PlayNotificationSoundDirect("success");
                            PlatformService.ShowNotificationMain($"BIOS Map saved to {folder.Name}", 3);
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("error");
                            PlatformService.ShowNotificationMain($"BIOS Map file not found!", 3);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.PlayNotificationSoundDirect("error");
                PlatformService.ShowErrorMessageDirect(ex);
            }
            VM.SystemCoreIsLoadingState(false);
        }

        private void CloseLogList_Click(object sender, RoutedEventArgs e)
        {
            ShowErrorsList = false;
            BindingsUpdate();
        }


        private async void ResetLogList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmDelete = new ConfirmConfig();
                confirmDelete.SetTitle("Reset Log");
                confirmDelete.SetMessage($"Do you want to reset skipped cores list?");
                confirmDelete.UseYesNo();
                confirmDelete.OkText = "Reset";
                confirmDelete.CancelText = "Cancel";

                var confirmDeleteState = await UserDialogs.Instance.ConfirmAsync(confirmDelete);
                if (confirmDeleteState)
                {
                    VM.SystemCoreIsLoadingState(true, "Reseting list..");
                    foreach (var system in PlatformService.Consoles)
                    {
                        foreach (var SkippedItem in SkippedListNamesOnly)
                        {
                            if (SkippedItem.Equals((system.DLLName.ToLower().EndsWith(".dll") ? system.DLLName : $"{system.DLLName}.dll")))
                            {
                                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue((system.DLLName.ToLower().EndsWith(".dll") ? system.DLLName : $"{system.DLLName}.dll"), false);
                                break;
                            }
                            if (system.ConsoleName?.Length > 0 && SkippedItem.Equals(system.ConsoleName))
                            {
                                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(system.ConsoleName, false);
                                break;
                            }
                        }
                    }
                    foreach (var system in ConsolesSequence)
                    {
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(system.CoreName, false);
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(system.SystemName, false);
                    }
                    await Task.Delay(2000);

                    PlatformService.PlayNotificationSoundDirect("success");
                    PlatformService.ShowNotificationMain($"Reset cores done", 3);
                    VM.SystemCoreIsLoadingState(false);
                    RetriXGoldCoresLoader(true);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }


        StorageFile forceUpdateFile = null;
        private async void PureCoreUpdate_Click(string coreName = "")
        {
            try
            {
                if (forceUpdateFile == null)
                {
                    PlatformService.PlayNotificationSoundDirect("button-01");
                    PlatformService.PlayNotificationSoundDirect("notice");
                    ConfirmConfig confirCoreSettings = new ConfirmConfig();
                    confirCoreSettings.SetTitle("Update Core?");
                    confirCoreSettings.SetMessage("This option used to update the core to newer version\nDo you want to start?");
                    confirCoreSettings.UseYesNo();
                    bool confirCoreSettingsState = PlatformService.ExtraConfirmation ? await UserDialogs.Instance.ConfirmAsync(confirCoreSettings) : true;
                    if (!confirCoreSettingsState)
                    {
                        return;
                    }
                }
                FileOpenPicker picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.List;
                picker.SuggestedStartLocation = PickerLocationId.Downloads;
                //picker.FileTypeFilter.Add(".rxe");
                picker.FileTypeFilter.Add(".dll");

                StorageFile file = forceUpdateFile != null ? forceUpdateFile : await picker.PickSingleFileAsync();

                if (file != null)
                {
                    VM.SystemCoreIsLoadingState(true, "Updating core..");
                    StorageFolder zipsDirectory = null;
                    var localFolder = ApplicationData.Current.LocalFolder;
                    var targetFolderName = "PureCore";
                    if (coreName.Length == 0)
                    {
                        if (VM.SelectedSystem.Core.ImportedCore)
                        {
                            targetFolderName = "AnyCore";
                        }
                    }
                    zipsDirectory = (StorageFolder)await localFolder.TryGetItemAsync(targetFolderName);
                    if (zipsDirectory == null)
                    {
                        zipsDirectory = await localFolder.CreateFolderAsync(targetFolderName);
                    }
                    var isDLL = System.IO.Path.GetExtension(file.Name.ToLower()).Equals(".dll");
                    string targetFileName = coreName.Length > 0 ? coreName : VM.SelectedSystem.DLLName;
                    if (!targetFileName.EndsWith(".dll"))
                    {
                        targetFileName = $"{targetFileName}.dll";
                    }
                    if (isDLL && coreName.Length == 0)
                    {
                        VM.SelectedSystem.Core.FreeLibretroCore();
                    }
                    var targetFile = await file.CopyAsync(zipsDirectory, targetFileName, NameCollisionOption.ReplaceExisting);

                    if (targetFile != null)
                    {
                        //Reset skipped list entry in case the core skipped or failed
                        if (coreName.Length == 0)
                        {
                            if (VM.SelectedSystem.DLLName != null)
                            {
                                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(VM.SelectedSystem.DLLName, false);
                            }
                            if (VM.SelectedSystem.ConsoleName != null && VM.SelectedSystem.ConsoleName.Length > 0)
                            {
                                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(VM.SelectedSystem.ConsoleName, false);
                            }
                            await Task.Delay(2000);
                            await VM.SelectedSystem.XCore.CacheCoreClean();
                        }

                        PlatformService.PlayNotificationSoundDirect("success");
                        PlatformService.ShowNotificationMain($"{(isDLL ? "Core" : "Extras")} has been updated", 3);
                        RetriXGoldCoresLoader(true);
                    }
                    VM.SystemCoreIsLoadingState(false);
                }
            }
            catch (Exception ex)
            {

                PlatformService.ShowErrorMessageDirect(ex);

            }
            forceUpdateFile = null;
        }

        private async Task<bool> CheckUpdateState()
        {
            try
            {
                StorageFolder zipsDirectory = null;
                var localFolder = ApplicationData.Current.LocalFolder;
                var targetFolderName = "PureCore";
                if (VM.SelectedSystem.Core.ImportedCore)
                {
                    targetFolderName = "AnyCore";
                }
                zipsDirectory = (StorageFolder)await localFolder.TryGetItemAsync(targetFolderName);
                if (zipsDirectory != null)
                {
                    string targetFileName = VM.SelectedSystem.DLLName;

                    var targetFielTest = (StorageFile)await zipsDirectory.TryGetItemAsync(targetFileName);
                    if (targetFielTest != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        private async void RemoveCoreUpdate_Click()
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                PlatformService.PlayNotificationSoundDirect("notice");
                ConfirmConfig confirCoreSettings = new ConfirmConfig();
                confirCoreSettings.SetTitle("Remove Updates?");
                confirCoreSettings.SetMessage("This option used to remove all the updates\nDo you want to start?");
                confirCoreSettings.UseYesNo();
                bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                if (!confirCoreSettingsState)
                {
                    return;
                }

                StorageFolder zipsDirectory = null;
                var localFolder = ApplicationData.Current.LocalFolder;
                zipsDirectory = (StorageFolder)await localFolder.TryGetItemAsync("PureCore");
                if (zipsDirectory == null)
                {
                    PlatformService.PlayNotificationSoundDirect("faild");
                    if (!pushNormalNotification($"{VM.SelectedSystem.Name}'s updates not found!"))
                    {
                        await UserDialogs.Instance.AlertAsync($"{VM.SelectedSystem.Name}'s updates not found!");
                    }
                    return;
                }
                var isDLL = true;
                string targetFileName = VM.SelectedSystem.DLLName;
                if (isDLL)
                {
                    VM.SelectedSystem.Core.FreeLibretroCore();
                }
                var targetFielTest = (StorageFile)await zipsDirectory.TryGetItemAsync(targetFileName);
                if (targetFielTest != null)
                {
                    await targetFielTest.DeleteAsync();

                    //Reset skipped list entry in case the core skipped or failed
                    if (VM.SelectedSystem.DLLName != null)
                    {
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(VM.SelectedSystem.DLLName, false);
                    }
                    if (VM.SelectedSystem.ConsoleName != null && VM.SelectedSystem.ConsoleName.Length > 0)
                    {
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(VM.SelectedSystem.ConsoleName, false);
                    }
                    await Task.Delay(2000);
                    await VM.SelectedSystem.XCore.CacheCoreClean();

                    PlatformService.PlayNotificationSoundDirect("success");
                    PlatformService.ShowNotificationMain($"{(isDLL ? "Core" : "Scripts")} updates has been deleted", 3);
                    RetriXGoldCoresLoader(true);
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("faild");
                    if (!pushNormalNotification($"{VM.SelectedSystem.Name}'s updates not found!"))
                    {
                        await UserDialogs.Instance.AlertAsync($"{VM.SelectedSystem.Name}'s updates not found!");
                    }
                }


            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void SetGamesFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VM != null && VM.SelectedSystem != null)
                {
                    var folderState = await VM.SelectedSystem.SetGamesDirectoryAsync(true);
                    if (folderState)
                    {
                        PlatformService.PlayNotificationSoundDirect("success");
                        PlatformService.ShowNotificationMain("Games folder set for " + VM.SelectedSystem.Name, 2);
                        AllGetterReload_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void StartCore_Click()
        {
            try
            {
                if (VM.SelectedSystem.SupportNoGame)
                {
                    var state = await VM.StartCore();
                    if (!state)
                    {
                        VM.SystemCoreIsLoadingState(false);
                    }
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("faild");
                    if (!pushNormalNotification($"This core doesn't support (no content)!"))
                    {
                        await UserDialogs.Instance.AlertAsync($"This core doesn't support (no content)!");
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }


        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private async void PureCoreUpdateOnline_Click(string directName = "")
        {
            await Task.Run(async () =>
            {
                try
                {
                    forceUpdateFile = null;
                    if (VM != null)
                    {
                        ShowLoader(true, directName.Length > 0 ? "Checking Core.." : "Checking updates..");
                    }
                    cancellationTokenSource = new CancellationTokenSource();
                    if (!Helpers.CheckInternetConnection())
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        LocalNotificationData localNotificationData = new LocalNotificationData();
                        localNotificationData.icon = SegoeMDL2Assets.DisconnectDrive;
                        localNotificationData.message = "No internet connection!";
                        localNotificationData.time = 3;
                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                        ShowLoader(false);
                        return;
                    }

                    if (VM != null && (directName.Length > 0 || VM.SelectedSystem != null))
                    {
                        var dllName = (directName.Length > 0 ? directName : VM.SelectedSystem.DLLName).Replace(".dll", "");
                        var version = directName.Length > 0 ? "#####" : VM.SelectedSystem.Version;
                        var timeStamp = new TimeSpan(DateTime.Now.Ticks);
                        var time = timeStamp.Milliseconds;
                        var arch = "ARM";
#if TARGET_ARM
                        arch = "ARM";
#elif TARGET_X64
                        arch = "X64";
#elif TARGET_X86
                        arch = "X86";
#endif

                        var mainLink = $"https://github.com/basharast/RetrixGold/raw/main/cores/{arch}/{dllName}.txt";
                        var mainDLLLink = $"https://github.com/basharast/RetrixGold/raw/main/cores/{arch}/{dllName}.dll";
                        var unCahcedLink = $"{mainLink}?time={time}";
                        var unDLLCahcedLink = $"{mainDLLLink}?time={time}";
                        var testResponse = await Helpers.GetResponse(unCahcedLink, cancellationTokenSource.Token, null, false);
                        if (testResponse == null)
                        {
                            PlatformService.PlayNotificationSoundDirect("error");
                            LocalNotificationData localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.DisconnectDrive;
                            localNotificationData.message = "Failed to check core updates!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        else
                        {
                            var baseStreamTemp = await testResponse.Content.ReadAsInputStreamAsync();
                            var tempStream = baseStreamTemp.AsStreamForRead();

                            MemoryStream memoryStreamFile = new MemoryStream();
                            using (tempStream)
                            {
                                using (memoryStreamFile)
                                {
                                    await tempStream.CopyToAsync(memoryStreamFile);
                                }
                                tempStream.Dispose();
                            }
                            byte[] resultInBytes;
                            resultInBytes = memoryStreamFile.ToArray();
                            var textRead = Encoding.UTF8.GetString(resultInBytes, 0, resultInBytes.Length);

                            if (textRead != null && textRead.Length > 0)
                            {
                                if (textRead.ToLower().Equals(version.ToLower()))
                                {
                                    PlatformService.PlayNotificationSoundDirect("alert");
                                    LocalNotificationData localNotificationData = new LocalNotificationData();
                                    localNotificationData.icon = SegoeMDL2Assets.Globe;
                                    localNotificationData.message = "This core is up-to date!";
                                    localNotificationData.time = 3;
                                    PlatformService.NotificationHandlerMain(null, localNotificationData);
                                }
                                else
                                {
                                    PlatformService.PlayNotificationSoundDirect("success");
                                    LocalNotificationData localNotificationData = new LocalNotificationData();
                                    localNotificationData.icon = SegoeMDL2Assets.Globe;
                                    localNotificationData.message = directName.Length > 0 ? "Core found, please wait.." : "New update found, please wait..!";
                                    localNotificationData.time = 3;
                                    PlatformService.NotificationHandlerMain(null, localNotificationData);

                                    bool updateConfirm = true;
                                    TaskCompletionSource<bool> confirmState = new TaskCompletionSource<bool>();
                                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                    {
                                        try
                                        {
                                            if (!(directName.Length > 0))
                                            {
                                                var confirCoreSettings = new ConfirmConfig();
                                                confirCoreSettings.SetTitle("Update core");
                                                confirCoreSettings.SetMessage($"Online: {textRead}\nCurrent: {version}\n\n{(PlatformService.isXBOXPure ? "Important: XBOX doesn't core update yet!\n" : "")}Do you want to update?");
                                                confirCoreSettings.UseYesNo();
                                                updateConfirm = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                                                confirmState.SetResult(true);
                                            }
                                            else
                                            {
                                                confirmState.SetResult(true);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            confirmState.SetResult(true);
                                        }
                                    });
                                    await confirmState.Task;
                                    if (!updateConfirm)
                                    {
                                        if (VM != null)
                                        {
                                            ShowLoader(false);
                                        }
                                        return;
                                    }
                                    StorageFile updateFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"{dllName}.dll", CreationCollisionOption.ReplaceExisting);
                                    if (updateFile != null)
                                    {
                                        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                                        bool isDownloadFailed = false;
                                        await Task.Run(async () =>
                                        {
                                            try
                                            {
                                                cancellationTokenSource = new CancellationTokenSource();
                                                var testDLLResponse = await Helpers.GetResponse(unDLLCahcedLink, cancellationTokenSource.Token, null, false);
                                                var tempDLLStream = await testDLLResponse.Content.ReadAsInputStreamAsync(); ;
                                                PlatformService.PlayNotificationSoundDirect("success");
                                                localNotificationData = new LocalNotificationData();
                                                localNotificationData.icon = SegoeMDL2Assets.Download;
                                                localNotificationData.message = directName.Length > 0 ? "Core found, please wait.." : "New update found, downloading..!";
                                                localNotificationData.time = 3;
                                                PlatformService.NotificationHandlerMain(null, localNotificationData);
                                                ShowLoader(true, directName.Length > 0 ? "Downloading Core.." : "Downloading update..");
                                                using (tempDLLStream)
                                                {
                                                    using (var targetStream = await updateFile.OpenAsync(FileAccessMode.ReadWrite))
                                                    {
                                                        var output = targetStream.AsStreamForWrite();
                                                        await tempDLLStream.AsStreamForRead().CopyToAsync(output);
                                                        output.Dispose();
                                                    }
                                                    tempStream.Dispose();
                                                }
                                            }
                                            catch (Exception x)
                                            {
                                                PlatformService.ShowErrorMessageDirect(x);
                                                isDownloadFailed = true;
                                            }
                                            try
                                            {
                                                taskCompletionSource.SetResult(true);
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        });
                                        await taskCompletionSource.Task;
                                        if (isDownloadFailed)
                                        {
                                            PlatformService.PlayNotificationSoundDirect("error");
                                            localNotificationData = new LocalNotificationData();
                                            localNotificationData.icon = SegoeMDL2Assets.Globe;
                                            localNotificationData.message = "Failed to download update file!";
                                            localNotificationData.time = 3;
                                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                                        }
                                        else
                                        {
                                            PlatformService.PlayNotificationSoundDirect("success");
                                            localNotificationData = new LocalNotificationData();
                                            localNotificationData.icon = SegoeMDL2Assets.ActionCenterNotification;
                                            localNotificationData.message = "Download done, please wait..!";
                                            localNotificationData.time = 3;
                                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                                            forceUpdateFile = updateFile;
                                            PureCoreUpdate_Click(directName);
                                        }
                                    }
                                    else
                                    {
                                        PlatformService.PlayNotificationSoundDirect("error");
                                        localNotificationData = new LocalNotificationData();
                                        localNotificationData.icon = SegoeMDL2Assets.Globe;
                                        localNotificationData.message = "Failed to create update file!";
                                        localNotificationData.time = 3;
                                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                                    }

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PlatformService.ShowErrorMessageDirect(ex);
                }
                if (VM != null)
                {
                    ShowLoader(false);
                }
            });

        }

        private async void RetrixUpdateOnline_Click()
        {
            await Task.Run(async () =>
            {
                try
                {
                    forceUpdateFile = null;
                    if (VM != null)
                    {
                        ShowLoader(true, "Checking updates..");
                    }
                    cancellationTokenSource = new CancellationTokenSource();
                    if (!Helpers.CheckInternetConnection())
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        LocalNotificationData localNotificationData = new LocalNotificationData();
                        localNotificationData.icon = SegoeMDL2Assets.DisconnectDrive;
                        localNotificationData.message = "No internet connection!";
                        localNotificationData.time = 3;
                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                        ShowLoader(false);
                        return;
                    }

                    {
                        var dllName = "RetriXGold";
                        var version = PlatformService.RetriXBuildNumber;
                        var timeStamp = new TimeSpan(DateTime.Now.Ticks);
                        var time = timeStamp.Milliseconds;
                        var mainLink = $"https://github.com/basharast/RetrixGold/raw/main/update/{dllName}.txt";
                        var arch = "ARM";
#if TARGET_ARM
                        arch = "ARM";
#elif TARGET_X64
                        arch = "X64";
#elif TARGET_X86
                        arch = "X86";
#endif
                        var mainDLLLink = $"https://github.com/basharast/RetrixGold/raw/main/update/{dllName}_{arch}.7z";
                        var unCahcedLink = $"{mainLink}?time={time}";
                        var unDLLCahcedLink = $"{mainDLLLink}?time={time}";
                        var testResponse = await Helpers.GetResponse(unCahcedLink, cancellationTokenSource.Token, null, false);
                        if (testResponse == null)
                        {
                            PlatformService.PlayNotificationSoundDirect("error");
                            LocalNotificationData localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.DisconnectDrive;
                            localNotificationData.message = "Failed to check RetriXGold updates!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        else
                        {
                            var baseStreamTemp = await testResponse.Content.ReadAsInputStreamAsync();
                            var tempStream = baseStreamTemp.AsStreamForRead();

                            MemoryStream memoryStreamFile = new MemoryStream();
                            using (tempStream)
                            {
                                using (memoryStreamFile)
                                {
                                    await tempStream.CopyToAsync(memoryStreamFile);
                                }
                                tempStream.Dispose();
                            }
                            byte[] resultInBytes;
                            resultInBytes = memoryStreamFile.ToArray();
                            var textRead = Encoding.UTF8.GetString(resultInBytes, 0, resultInBytes.Length);

                            if (textRead != null && textRead.Length > 0)
                            {
                                var foundVersion = int.Parse(textRead.Trim());
                                if (foundVersion <= version)
                                {
                                    PlatformService.PlayNotificationSoundDirect("alert");
                                    LocalNotificationData localNotificationData = new LocalNotificationData();
                                    localNotificationData.icon = SegoeMDL2Assets.Globe;
                                    localNotificationData.message = "RetriXGold is up-to date!";
                                    localNotificationData.time = 3;
                                    PlatformService.NotificationHandlerMain(null, localNotificationData);
                                }
                                else if (PlatformService.isXBOXPure)
                                {
                                    PlatformService.PlayNotificationSoundDirect("success");
                                    LocalNotificationData localNotificationData = new LocalNotificationData();
                                    localNotificationData.icon = SegoeMDL2Assets.Globe;
                                    localNotificationData.message = "New update found!";
                                    localNotificationData.time = 3;
                                    PlatformService.NotificationHandlerMain(null, localNotificationData);
                                    await Task.Delay(500);
                                    var storelink = "ms-windows-store://pdp/?productid=9NZTKKSTJQSD";
                                    var success2 = await Windows.System.Launcher.LaunchUriAsync(new Uri(storelink));
                                    if (success2)
                                    {
                                        // URI launched
                                    }
                                }
                                else
                                {
                                    PlatformService.PlayNotificationSoundDirect("success");
                                    LocalNotificationData localNotificationData = new LocalNotificationData();
                                    localNotificationData.icon = SegoeMDL2Assets.Globe;
                                    localNotificationData.message = "New update found, please wait..!";
                                    localNotificationData.time = 3;
                                    PlatformService.NotificationHandlerMain(null, localNotificationData);


                                    PlatformService.PlayNotificationSoundDirect("alert");
                                    ConfirmConfig confirmSelect = new ConfirmConfig();
                                    confirmSelect.SetTitle("Update Download");
                                    confirmSelect.SetMessage($"Please select download folder");
                                    confirmSelect.UseYesNo();
                                    confirmSelect.SetOkText("Choose");
                                    confirmSelect.SetCancelText("Cancel");
                                    var SelectState = await UserDialogs.Instance.ConfirmAsync(confirmSelect);
                                    if (!SelectState)
                                    {
                                        PlatformService.PlayNotificationSoundDirect("error");
                                        localNotificationData = new LocalNotificationData();
                                        localNotificationData.icon = SegoeMDL2Assets.Globe;
                                        localNotificationData.message = "Download canceled!";
                                        localNotificationData.time = 3;
                                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                                    }
                                    else
                                    {
                                        FolderPicker folderPicker = new Windows.Storage.Pickers.FolderPicker();
                                        folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                                        folderPicker.FileTypeFilter.Add(".7z");

                                        StorageFolder downloadFolder = null;
                                        TaskCompletionSource<bool> taskCompletionSourceFolder = new TaskCompletionSource<bool>();
                                        _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                        {
                                            try
                                            {
                                                downloadFolder = await folderPicker.PickSingleFolderAsync();
                                                taskCompletionSourceFolder.SetResult(true);
                                            }
                                            catch (Exception ex)
                                            {
                                                taskCompletionSourceFolder.SetResult(true);
                                            }
                                        });
                                        await taskCompletionSourceFolder.Task;
                                        if (downloadFolder != null)
                                        {
                                            StorageFile updateFile = await downloadFolder.CreateFileAsync($"{dllName}_{arch}.7z", CreationCollisionOption.ReplaceExisting);
                                            if (updateFile != null)
                                            {
                                                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                                                bool isDownloadFailed = false;
                                                await Task.Run(async () =>
                                                {
                                                    try
                                                    {
                                                        cancellationTokenSource = new CancellationTokenSource();
                                                        var testDLLResponse = await Helpers.GetResponse(unDLLCahcedLink, cancellationTokenSource.Token, null, false);
                                                        if (testDLLResponse != null)
                                                        {
                                                            var tempDLLStream = await testDLLResponse.Content.ReadAsInputStreamAsync(); ;
                                                            PlatformService.PlayNotificationSoundDirect("success");
                                                            localNotificationData = new LocalNotificationData();
                                                            localNotificationData.icon = SegoeMDL2Assets.Download;
                                                            localNotificationData.message = "New update found, downloading..!";
                                                            localNotificationData.time = 3;
                                                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                                                            ShowLoader(true, "Downloading update..");
                                                            using (tempDLLStream)
                                                            {
                                                                using (var targetStream = await updateFile.OpenAsync(FileAccessMode.ReadWrite))
                                                                {
                                                                    var output = targetStream.AsStreamForWrite();
                                                                    await tempDLLStream.AsStreamForRead().CopyToAsync(output);
                                                                    output.Dispose();
                                                                }
                                                                tempStream.Dispose();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            PlatformService.PlayNotificationSoundDirect("error");
                                                            localNotificationData = new LocalNotificationData();
                                                            localNotificationData.icon = SegoeMDL2Assets.Globe;
                                                            localNotificationData.message = "Failed to fetch update file!";
                                                            localNotificationData.time = 3;
                                                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                                                            isDownloadFailed = true;
                                                        }
                                                    }
                                                    catch (Exception x)
                                                    {
                                                        PlatformService.ShowErrorMessageDirect(x);
                                                        isDownloadFailed = true;
                                                    }
                                                    try
                                                    {
                                                        taskCompletionSource.SetResult(true);
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                });
                                                await taskCompletionSource.Task;
                                                if (isDownloadFailed)
                                                {
                                                    PlatformService.PlayNotificationSoundDirect("error");
                                                    localNotificationData = new LocalNotificationData();
                                                    localNotificationData.icon = SegoeMDL2Assets.Globe;
                                                    localNotificationData.message = "Failed to download update file!";
                                                    localNotificationData.time = 3;
                                                    PlatformService.NotificationHandlerMain(null, localNotificationData);
                                                }
                                                else
                                                {
                                                    PlatformService.PlayNotificationSoundDirect("success");
                                                    localNotificationData = new LocalNotificationData();
                                                    localNotificationData.icon = SegoeMDL2Assets.ActionCenterNotification;
                                                    localNotificationData.message = "Download done, please wait..!";
                                                    localNotificationData.time = 3;
                                                    PlatformService.NotificationHandlerMain(null, localNotificationData);
                                                    ShowLoader(true, "Extracting files..");
#if USING_NATIVEARCHIVE
                                                    PackageArchiveHelper packageArchiveHelper = new PackageArchiveHelper();
                                                    await packageArchiveHelper.ExtractFiles(downloadFolder, updateFile);
#else
                                                    Stream zipStream = await updateFile.OpenStreamForReadAsync();
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
                                                                await reader.WriteEntryToDirectory(downloadFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                                                //WriteEntryToDirectory can extended to:
                                                                //WriteEntryToDirectory(folder, options, cancellationTokenSource)
                                                                //                                       ^^^^^^^^^^^^^^^^^^^^^^^
                                                                //it will help to terminate the current entry directly if cancellation requested
                                                            }
                                                        }
                                                    }
#endif
                                                    StorageFile InstallationPackage = (StorageFile)await downloadFolder.TryGetItemAsync($"{dllName}_{arch}\\{dllName}_{arch}.appx");
                                                    if (InstallationPackage != null)
                                                    {
                                                        var options = new Windows.System.LauncherOptions();
                                                        options.PreferredApplicationPackageFamilyName = "Microsoft.DesktopAppInstaller_8wekyb3d8bbwe";
                                                        options.PreferredApplicationDisplayName = "App Installer";

                                                        var state = await Windows.System.Launcher.LaunchFileAsync(InstallationPackage, options);
                                                        if (!state)
                                                        {
                                                            PlatformService.PlayNotificationSoundDirect("error");
                                                            localNotificationData = new LocalNotificationData();
                                                            localNotificationData.icon = SegoeMDL2Assets.Globe;
                                                            localNotificationData.message = "Failed to start AppInstaller, please install this update manually!";
                                                            localNotificationData.time = 5;
                                                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        PlatformService.PlayNotificationSoundDirect("error");
                                                        localNotificationData = new LocalNotificationData();
                                                        localNotificationData.icon = SegoeMDL2Assets.Globe;
                                                        localNotificationData.message = "Unable to locate installation file, please install this update manually!";
                                                        localNotificationData.time = 5;
                                                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                PlatformService.PlayNotificationSoundDirect("error");
                                                localNotificationData = new LocalNotificationData();
                                                localNotificationData.icon = SegoeMDL2Assets.Globe;
                                                localNotificationData.message = "Failed to create update file!";
                                                localNotificationData.time = 3;
                                                PlatformService.NotificationHandlerMain(null, localNotificationData);
                                            }
                                        }
                                        else
                                        {
                                            PlatformService.PlayNotificationSoundDirect("error");
                                            localNotificationData = new LocalNotificationData();
                                            localNotificationData.icon = SegoeMDL2Assets.Globe;
                                            localNotificationData.message = "Download canceled, no folder selected!";
                                            localNotificationData.time = 3;
                                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PlatformService.ShowErrorMessageDirect(ex);
                }
                if (VM != null)
                {
                    ShowLoader(false);
                }
            });

        }

        #region Code from GameSystemsProviderService.cs
        List<RequestedCore> ConsolesSequence = new List<RequestedCore>();
        public static bool ShowErrorNotification { get { return SkippedList.Count() > 0; } }
        public static ObservableCollection<string> SkippedList = new ObservableCollection<string>();
        public static ObservableCollection<string> SkippedListNamesOnly = new ObservableCollection<string>();
        public async Task<ObservableCollection<GameSystemViewModel>> GenerateSystemsList(bool forceReload = false)
        {
            try
            {
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            await Task.Run(() =>
            {
                try
                {
                    AddConsoles();
                    WriteLog($"Consoles added and ready to load..");
                }
                catch (Exception ex)
                {

                }
            });

            ObservableCollection<GameSystemViewModel> gameSystemViewModels = new ObservableCollection<GameSystemViewModel>();
            SkippedList.Clear();
            SkippedListNamesOnly.Clear();

            foreach (RequestedCore item in ConsolesSequence)
            {
                try
                {
                    //Note: MonitorLoading cannot use item.CoreName because when core fails the name will not be available and we cannot use it for resetting
                    WriteLog($"\n*********************************");
                    WriteLog($"Checking {item.SystemName} skip state..");
                    var checkCore = MonitorLoadingStart($"{item.SystemName}");
                    if (checkCore)
                    {
                        WriteLog($"{item.SystemName} failed before, will be skipped..");
                        if (item.CoreName.Equals(item.SystemName))
                        {
                            SkippedList.Add($"{item.SystemName} Compatibility issues, or app closed while loading!");
                        }
                        else
                        {
                            SkippedList.Add($"{item.SystemName} ({item.CoreName}) Compatibility issues, or app closed while loading!");
                        }
                        SkippedListNamesOnly.Add(item.SystemName);
                        var consoleTest = await GameConsole(item.CoreName, item.SystemName, true);
                        if (consoleTest != null)
                        {
                            gameSystemViewModels.Add(consoleTest);
                        }
                        continue;
                    }
                    WriteLog($"Loading {item.SystemName}..");
                    MonitorLoadingEnd($"{item.SystemName}", true);
                    var console = await GameConsole(item.CoreName, item.SystemName);
                    if (console != null)
                    {
                        if (!console.XCore.DLLMissing)
                        {
                            if (!console.FailedToLoad)
                            {
                                WriteLog($"Core file {console.Core.DLLName} loaded..");
                                gameSystemViewModels.Add(console);
                                MonitorLoadingEnd($"{item.SystemName}", false);
                            }
                            else
                            {
                                WriteLog($"Core file {console.Core.DLLName} failed to load..");
                                if (item.CoreName.Equals(item.SystemName))
                                {
                                    SkippedList.Add($"{item.SystemName} Compatibility issues, or app closed while loading!");
                                }
                                else
                                {
                                    SkippedList.Add($"{item.SystemName} ({item.CoreName}) Compatibility issues, or app closed while loading!");
                                }
                                SkippedListNamesOnly.Add(item.SystemName);
                                var consoleTest = await GameConsole(item.CoreName, item.SystemName, true);
                                if (consoleTest != null)
                                {
                                    gameSystemViewModels.Add(consoleTest);
                                }
                            }
                        }
                        else
                        {
                            MonitorLoadingEnd($"{item.SystemName}", false);
                        }
                    }
                    else
                    {
                        MonitorLoadingEnd($"{item.SystemName}", false);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }
                _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        currentProgress++;
                        if (currentProgress <= PlatformService.MaxProgressValue)
                        {
                            CoreLoadingProgress.Value = currentProgress;
                        }
                    }
                    catch (Exception er)
                    {
                    }
                });
                //await Task.Delay(1);
            }

            WriteLog($"Loading AnyCore consoles..");
            //Generate AnyCore Consoles
            if (PlatformService.AnyCores != null && PlatformService.AnyCores.Count() > 0)
            {
                foreach (StorageFile currentFile in PlatformService.AnyCores)
                {
                    try
                    {
                        var fileExtension = System.IO.Path.GetExtension(currentFile.Path);
                        if (fileExtension.ToLower().Equals(".dll") || fileExtension.ToLower().Equals(".dat"))
                        {
                            WriteLog($"\n*********************************");
                            WriteLog($"Checking {currentFile.Name} skip state..");
                            var checkCore = MonitorLoadingStart(currentFile.Name);
                            if (checkCore)
                            {
                                WriteLog($"{currentFile.Name} failed before, will be skipped..");
                                SkippedList.Add($"{currentFile.Name} Compatibility issues, or app closed while loading!");
                                SkippedListNamesOnly.Add(currentFile.Name);
                                gameSystemViewModels.Add(await GameConsoleAnyCore(currentFile.Path.Replace(".dll", "").Replace(".dat", ""), true));
                                continue;
                            }

                            WriteLog($"Loading {currentFile.Name}..");
                            MonitorLoadingEnd(currentFile.Name, true);
                            var console = await GameConsoleAnyCore(currentFile.Path.Replace(".dll", "").Replace(".dat", ""));
                            if (!console.FailedToLoad)
                            {
                                WriteLog($"Core file {console.Core.DLLName} loaded..");
                                gameSystemViewModels.Add(console);
                                MonitorLoadingEnd(currentFile.Name, false);
                            }
                            else
                            {
                                WriteLog($"Core file {console.Core.DLLName} failed to load..");
                                SkippedList.Add($"{currentFile.Name} Compatibility issues, or app closed while loading!");
                                SkippedListNamesOnly.Add(currentFile.Name);
                                var consoleToAdd = await GameConsoleAnyCore(currentFile.Path.Replace(".dll", "").Replace(".dat", ""), true);
                                gameSystemViewModels.Add(consoleToAdd);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message);
                    }
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        try
                        {
                            currentProgress++;
                            CoreLoadingProgress.Value = currentProgress;
                        }
                        catch (Exception er)
                        {
                        }
                    });
                    //await Task.Delay(1);
                }
            }

            var sortedConsoles = gameSystemViewModels.OrderByDescending(x => x?.OpenTimes).ToList();
            gameSystemViewModels.Clear();
            foreach (var sItem in sortedConsoles)
            {
                gameSystemViewModels.Add(sItem);
            }
            PlatformService.isCoresLoaded = true;

            foreach (var system in gameSystemViewModels)
            {
                //Set 'StartupLoading' after loading to 'false' (to avoid any extra main log)
                system.Core.StartupLoading = false;
            }
            //Generate Cores List
            GenerateCoresList(gameSystemViewModels, forceReload);
            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }
            return gameSystemViewModels;
        }

        public static string GetAppVersion()
        {
            try
            {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
            catch (Exception ex)
            {

            }
            return "3.0.0.0";
        }


        ObservableCollection<CoresQuickListItem> CoresQuickMenu = new ObservableCollection<CoresQuickListItem>();
        List<GameSystemViewModel> AddedConsoles = new List<GameSystemViewModel>();
        private async void GenerateCoresList(ObservableCollection<GameSystemViewModel> gameSystemViewModel, bool forceReload = false)
        {
            CoresQuickMenu.Clear();
            await Task.Run(() =>
            {
                _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
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

                        //Management
                        /*var ImportBIOSCollectionAction = GenerateMenuItem("Management", "biosci", "Import BIOS", "others");
                        if (PlatformService.DeviceIsPhone())
                        {
                            ImportBIOSCollectionAction.Collapsed = true;
                        }
                        CoresQuickMenu.Add(ImportBIOSCollectionAction);*/


                        var AnyCoreAction = GenerateMenuItem("Management", "import-anycore", "Import".ToUpper(), "web-new", "", "ANYCORE");

                        CoresQuickMenu.Add(AnyCoreAction);

                        var SettingsSectionsState = PlatformService.SettingsSections;
                        var SettingsAction = GenerateMenuItem("Management", "settingssec", "Settings".ToUpper(), "others", "", "");
                        SettingsAction.TagGray = DPIScaleInfo;
                        SettingsAction.TagVisibilityGray = Visibility.Visible;
                        SettingsAction.IsOn = SettingsSectionsState;
                        CoresQuickMenu.Add(SettingsAction);

                        var SupportSectionsState = PlatformService.SupportSections;
                        var SupportSectionAction = GenerateMenuItem("Management", "supportsec", "Support".ToUpper(), "support", "", "");
                        SupportSectionAction.IsOn = SupportSectionsState;
                        SupportSectionAction.TagGray = GetAppVersion();
                        SupportSectionAction.TagVisibilityGray = Visibility.Visible;
                        CoresQuickMenu.Add(SupportSectionAction);

                        if (ShowErrorsIcon)
                        {
                            var SkippedCoreCount = $"{SkippedList.Count}";
                            var CrashsAction = GenerateMenuItem("Management", "crash", "Crashes".ToUpper(), "logs", "", SkippedCoreCount);
                            CoresQuickMenu.Add(CrashsAction);
                        }

                        var ShortcutsAction = GenerateMenuItem("Management", "shortcuts", "Shortcuts".ToUpper(), "xbox");

                        CoresQuickMenu.Add(ShortcutsAction);

                        if (PlatformService.isXBOX || PlatformService.DPadActive)
                        {
                            var FilterAction = GenerateMenuItem("Management", "filter", "Filters".ToUpper(), "search");
                            CoresQuickMenu.Add(FilterAction);
                        }

                        var globalRootState = (await PlatformService.GetGlobalFolder()) != null;
                        var GlobalRootAction = GenerateMenuItem("Management", "change-root", "Root".ToUpper(), "folder-apps");
                        if (!globalRootState)
                        {
                            GlobalRootAction.BorderColor = new SolidColorBrush((Windows.UI.Color)Helpers.HexToColor("#66ffa500"));
                        }
                        else
                        {
                            GlobalRootAction.BorderColor = new SolidColorBrush(Colors.Green);
                        }
                        CoresQuickMenu.Add(GlobalRootAction);

                        var ExitAction = GenerateMenuItem("Management", "exit", "Exit".ToUpper(), "share");

                        CoresQuickMenu.Add(ExitAction);

                        Dictionary<string, CoresQuickListItem> itemSwitchs = new Dictionary<string, CoresQuickListItem>()
                        {
                            { "settings", SettingsAction },
                            { "support", SupportSectionAction }
                        };

                        //Settings
                        /*var SFXState = PlatformService.MuteSFX;
                        var SFXAction = GenerateMenuItem("Settings", "mute", "Sound Effects", "opus", "", !SFXState ? "ON" : "OFF");
                        CoresQuickMenu.Add(SFXAction);

                        var AutoResolveVFSIssuesState = PlatformService.AutoResolveVFSIssues;
                        var AutoResolveVFSIssuesAction = GenerateMenuItem("Settings", "autovfs", "Resolve VFS", "open", "", AutoResolveVFSIssuesState ? "ON" : "OFF");
                        CoresQuickMenu.Add(AutoResolveVFSIssuesAction);

                        var UseWindowsIndexerState = PlatformService.UseWindowsIndexer;
                        var UseWindowsIndexerAction = GenerateMenuItem("Settings", "indexer", "Windows Indexer", "search", "", UseWindowsIndexerState ? "ON" : "OFF");
                        CoresQuickMenu.Add(UseWindowsIndexerAction);

                        var ExtraConfirmationState = PlatformService.ExtraConfirmation;
                        var ExtraConfirmationAction = GenerateMenuItem("Settings", "extraask", "Extra Confirm", "feedback", "", ExtraConfirmationState ? "ON" : "OFF");
                        CoresQuickMenu.Add(ExtraConfirmationAction);

                        var FullScreen = PlatformService.FullScreen;
                        var FullScreenAction = GenerateMenuItem("Settings", "full", "FullScreen", "scale", "", FullScreen ? "ON" : "OFF");
                        CoresQuickMenu.Add(FullScreenAction);

                        var ShowIndicatorsState = PlatformService.ShowIndicators;
                        var ShowIndicatorsAction = GenerateMenuItem("Settings", "indicators", "VFS Indicator", "remote", "", ShowIndicatorsState ? "ON" : "OFF");
                        CoresQuickMenu.Add(ShowIndicatorsAction);

                        var BackPressState = App.HandleBackPress;
                        var BackPressAction = GenerateMenuItem("Settings", "backp", "GoBack Handle", "any", "", BackPressState ? "ON" : "OFF");
                        CoresQuickMenu.Add(BackPressAction);

                        var CleanTempOnStartup = App.CleanTempOnStartup;
                        var CleanTempOnStartupAction = GenerateMenuItem("Settings", "cleantemp", "Clear Temp", "web-cache", "STARTUP", CleanTempOnStartup ? "ON" : "OFF");
                        CoresQuickMenu.Add(CleanTempOnStartupAction);

                        var BackupAction = GenerateMenuItem("Settings", "backup", "Backup", "repos2");
                        CoresQuickMenu.Add(BackupAction);

                        var RestoreAction = GenerateMenuItem("Settings", "restore", "Restore", "uupdate");
                        CoresQuickMenu.Add(RestoreAction);

                        var ResetAction = GenerateMenuItem("Settings", "reset-system", "Reset", "rupdates");
                        CoresQuickMenu.Add(ResetAction);*/

                        //More
                        /*var CheckRetrixUpdateAction = GenerateMenuItem("Support", "checku", "Check Updates", "web-bmode", GetAppVersion(), "GOLD");
                        CoresQuickMenu.Add(CheckRetrixUpdateAction);

                        var HelpAction = GenerateMenuItem("Support", "help", "Help", "info");
                        CoresQuickMenu.Add(HelpAction);

                        var AboutAction = GenerateMenuItem("Support", "about", "About", "about");
                        CoresQuickMenu.Add(AboutAction);

                        var ContactAction = GenerateMenuItem("Support", "contact", "Contact", "support", "TELEGRAM");
                        CoresQuickMenu.Add(ContactAction);

                        var GitHubAction = GenerateMenuItem("Support", "github", "GitHub", "github-link", "SOURCE");
                        CoresQuickMenu.Add(GitHubAction);

                        var SupportAction = GenerateMenuItem("Support", "support", "Patreon", "patreon", "COMMUNITY");
                        CoresQuickMenu.Add(SupportAction);

                        var SupportActionP = GenerateMenuItem("Support", "supportp", "PayPal", "paypal", "COMMUNITY");
                        CoresQuickMenu.Add(SupportActionP);

                        var WUTAction = GenerateMenuItem("Support", "wut", "W.U.T", "wut", "MY PROJECTS");
                        CoresQuickMenu.Add(WUTAction);*/



                        //Check if app started with file
                        if (PlatformService.OpenGameFile != null)
                        {
                            GameFile gameFile = new GameFile(PlatformService.OpenGameFile);
                            var testSystems = await VM.GameSystemsProviderService.GetCompatibleSystems(gameFile);
                            if (testSystems != null && testSystems.Count > 0)
                            {
                                foreach (var system in testSystems)
                                {
                                    string coreTag = (system.AnyCore && !system.ImportedCore) ? "ANYCORE" : (system.OldCore ? "OLD" : "");
                                    string group = "Suggested Cores";
                                    var CoreAction = GenerateMenuItem(group, "core", system.Name, system.Symbol, system.OpenTimes > 0 ? $"{system.OpenTimes}" : "", coreTag, system);
                                    if (coreTag.Length == 0)
                                    {
                                        CoreAction.TagGray = system.Core.Name;
                                        CoreAction.TagVisibilityGray = Visibility.Visible;
                                    }
                                    CoreAction.SuggestedCore = true;
                                    CoresQuickMenu.Add(CoreAction);
                                }
                            }
                        }

                        //Top Cores
                        foreach (var system in gameSystemViewModel)
                        {
                            var pinnedState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{system.Core.Name}-{system.Name}-Pinned", false);
                            if (system.OpenTimes >= 10 || pinnedState)
                            {
                                string coreTag = (system.AnyCore && !system.ImportedCore) ? "ANYCORE" : (system.OldCore ? "OLD" : "");
                                string group = "Top Consoles";
                                var CoreAction = GenerateMenuItem(group, "core", system.Name, system.Symbol, system.OpenTimes > 0 ? $"{system.OpenTimes}" : "", coreTag, system);
                                if (coreTag.Length == 0)
                                {
                                    CoreAction.TagGray = system.Core.Name;
                                    CoreAction.TagVisibilityGray = Visibility.Visible;
                                }
                                CoresQuickMenu.Add(CoreAction);
                            }
                        }


                        //Recent Cores
                        foreach (var system in gameSystemViewModel)
                        {
                            var dllName = system.DLLName;
                            if (lastImportedCores.Contains(dllName))
                            {
                                string coreTag = (system.AnyCore && !system.ImportedCore) ? "ANYCORE" : (system.OldCore ? "OLD" : "");
                                string group = "Recently Added";
                                var CoreAction = GenerateMenuItem(group, "core", system.Name, system.Symbol, system.OpenTimes > 0 ? $"{system.OpenTimes}" : "", coreTag, system);
                                if (coreTag.Length == 0)
                                {
                                    CoreAction.TagGray = system.Core.Name;
                                    CoreAction.TagVisibilityGray = Visibility.Visible;
                                }
                                CoresQuickMenu.Add(CoreAction);
                            }
                        }

                        //Cores
                        foreach (var system in gameSystemViewModel.OrderByDescending(b => b.OpenTimes).ThenBy(a => a.Manufacturer))
                        {
                            if (forceFilter.Length > 0 && !system.ManufacturerTemp.Equals(forceFilter))
                            {
                                if (!system.AnyCore || !forceFilter.ToLower().Equals("anycore"))
                                {
                                    continue;
                                }
                            }
                            string coreTag = system.SkippedCore ? "Issue" : ((system.AnyCore && !system.ImportedCore) ? "ANYCORE" : (system.OldCore ? "OLD" : ""));
                            string group = (system.AnyCore && !system.ImportedCore) ? (system.ManufacturerTemp.Length == 0 ? "ANYCORE" : system.ManufacturerTemp) : system.ManufacturerTemp;

                            //Override group name for some core to avoid many rows
                            switch (system.TempName)
                            {
                                case "MSX Computer":
                                case "Amstrad CPC":
                                case "Sinclair ZX81":
                                case "Mini vMac":
                                case "PC 8000-8800":
                                case "DOSBox Pure":
                                case "vMac":
                                    group = "Computers";
                                    break;

                                case "Intellivision":
                                case "ColecoVision":
                                case "Vectrex":
                                case "Magnavox Odyssey 2":
                                case "Fairchild ChannelF":
                                    group = "Video Game (70~80s)";
                                    break;

                                case "ScummVM":
                                case "Game Music":
                                case "LowresNX":
                                case "PocketCDG":
                                case "TIC-80":
                                case "Oberon":
                                case "Oberon RISC":
                                case "3DO":
                                    group = "Various";
                                    break;

                                case "Watara Supervision":
                                case "WonderSwan":
                                case "WonderSwan Color":
                                    group = "Handheld (Mixed)";
                                    break;

                                case "Doom":
                                case "OutRun":
                                case "REminiscence Flashback":
                                case "Flashback":
                                case "REminiscence":
                                case "2048":
                                case "Jump n' Bump":
                                case "Cave Story":
                                case "Quake":
                                case "Rick Dangerous":
                                    group = "Game Engine";
                                    break;

                                case "Arcade":
                                case "MAME":
                                    group = "Arcade Machine";
                                    break;
                            }

                            string descriptions = system.OpenTimes > 0 ? $"{system.OpenTimes}" : "";
                            var CoreAction = GenerateMenuItem(group, "core", system.Name, system.Symbol, descriptions, coreTag, system);
                            if (coreTag.Length == 0)
                            {
                                CoreAction.TagGray = system.Core.Name;
                                CoreAction.TagVisibilityGray = Visibility.Visible;
                            }
                            CoresQuickMenu.Add(CoreAction);
                        }

                        if (encoderInProgress)
                        {
                            ShowLoader(true, "Transcoding Icons..");
                        }
                        while (encoderInProgress)
                        {
                            await Task.Delay(100);
                        }
                        GenerateMenu(itemSwitchs, "", CoresQuickMenu, CoresQuickAccessContainer);
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
                });
            });
        }

        private CoresQuickListItem GenerateMenuItem(string group, string action, string title, string icon, bool eventHandler, string dataTemplate = "BrowseGridMenuItemTemplate")
        {
            var Icon = $"ms-appx:///Assets/Icons/Menus/{icon}.png";

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
                Icon = $"ms-appx:///Assets/Icons/Menus/{icon}.png";
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
                Icon = $"ms-appx:///Assets/Icons/Menus/{icon}.png";
            }
            CoresQuickListItem coresQuickListItem = new CoresQuickListItem(group, action, title, Icon, descs, tag, recent, dataTemplate);

            return coresQuickListItem;
        }

        ObservableCollection<CoresQuickListItem> dataSourceScreenshots = new ObservableCollection<CoresQuickListItem>();
        ObservableCollection<CoresQuickListItem> dataSourceRecents = new ObservableCollection<CoresQuickListItem>();
        HyperlinkButton textBlockScreenshots = new HyperlinkButton();
        HyperlinkButton textBlockRecents = new HyperlinkButton();

        private void GenerateMenu(Dictionary<string, CoresQuickListItem> itemSwitchs, string prefix, ObservableCollection<CoresQuickListItem> items, ScrollViewer scrollViewer, bool keepFirst = false, bool collapse = false, bool recall = false)
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
                        stackPanel.Children.Add((isRecents ? textBlockRecents : (isScreenshot ? textBlockScreenshots : textBlock)));

                        if (collapse && !recall)
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
                        if (groupCollapseState)
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
                        stackPanel.Children.Add(border);

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
        private async Task ListEventGeneral_Tapped(CoresQuickListItem SelectOption, Windows.Foundation.Point point, UIElement targetElement)
        {
            try
            {
                bool RequiredToRegenerate = false;
                GlobalSelectedItem = null;
                if (SelectOption == null)
                {
                    return;
                }
                Dictionary<string, CoresQuickListItem> itemsSwitchs = new Dictionary<string, CoresQuickListItem>();

                switch (SelectOption.Action)
                {
                    case "import-anycore":
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmBackup = new ConfirmConfig();
                        confirmBackup.SetTitle("Import Core");
                        confirmBackup.SetMessage("Important: Don't import core already added, use update option at core's page instead" + (PlatformService.isXBOXPure ? "\nNote: This is not supported on XBOX yet\n" : "") + "\nDo you want to start?");
                        confirmBackup.UseYesNo();

                        var StartBackup = PlatformService.ExtraConfirmation ? await UserDialogs.Instance.ConfirmAsync(confirmBackup) : true;
                        if (StartBackup)
                        {
                            await ImportAnyCoreNewAction();
                        }
                        break;

                    case "settingssec":
                        RetriXGoldSettings();
                        /*PlatformService.SettingsSections = !PlatformService.SettingsSections;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("SettingsSections", PlatformService.SettingsSections);
                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = PlatformService.SettingsSections ? "VISIBLE" : "HIDDEN";
                                mItem.IsOn = PlatformService.SettingsSections;
                                break;
                            }
                        }
                        RequiredToRegenerate = true;
                        */
                        break;

                    case "ctools":
                        {
                            MenuFlyout coreSettingsMenu = new MenuFlyout();
                            List<MenuFlyoutItem> coresSettingsMenu = new List<MenuFlyoutItem>();
                            if (!VM.SelectedSystem.AnyCore || VM.SelectedSystem.ImportedCore)
                            {
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "Online Update", (s, e) =>
                            {
                                try
                                {
                                    PureCoreUpdateOnline_Click();
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Gold);
                            }
                            if (!VM.SelectedSystem.AnyCore || VM.SelectedSystem.ImportedCore)
                            {
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "Offline Update", (s, e) =>
                            {
                                try
                                {
                                    PureCoreUpdate_Click();
                                }
                                catch (Exception ex)
                                {

                                }
                            });
                            }

                            if (!VM.SelectedSystem.ImportedCore)
                            {
                                if (await CheckUpdateState())
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, "Remove Updates", (s, e) =>
                            {
                                try
                                {
                                    RemoveCoreUpdate_Click();
                                }
                                catch (Exception ex)
                                {

                                }
                            }, Colors.Orange);
                                }
                            }

                            if (VM.SelectedSystem.AnyCore)
                            {
                                if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, "Change Info", (s, e) =>
                                {
                                    try
                                    {
                                        ConsoleSettings_Click();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                                }
                                if (!VM.SelectedSystem.ImportedCore)
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, "Update Core", (s, e) =>
                                {
                                    try
                                    {
                                        AnyCoreAdvancedImport();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                                }
                            }


                            if (VM.SelectedSystem.AnyCore)
                            {
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "spr", (s, e) =>
                            {

                            });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "Delete Core", (s, e) =>
                                {
                                    try
                                    {
                                        AnyCoreDelete_Click();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Red);
                                if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad && !VM.SelectedSystem.ImportedCore)
                                {
                                    GenerateMenuFlyoutItem(ref coresSettingsMenu, "BIOS Template", (s, e) =>
                                {
                                    try
                                    {
                                        AnyCoreBiosSample_Click();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gray);
                                }
                            }


                            if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "spr", (s, e) =>
                            {

                            });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "Backup", (s, e) =>
                                {
                                    try
                                    {
                                        ExportSettings(true);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Green);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "Restore", (s, e) =>
                                {
                                    try
                                    {
                                        ImportSettingsSlotsAction(null, true);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.DodgerBlue);
                            }

                            if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                var testCustomSavesFolder = await PlatformService.GetCustomFolder(VM.SelectedSystem.Core.Name, "saves");
                                var driveSaves = "";
                                var tagSaves = "";
                                if (testCustomSavesFolder != null)
                                {
                                    try
                                    {
                                        driveSaves = System.IO.Path.GetPathRoot(testCustomSavesFolder.Path);
                                        tagSaves = "CUSTOM";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }

                                var testCustomSystemFolder = await PlatformService.GetCustomFolder(VM.SelectedSystem.Core.Name, "system");
                                var driveSystem = "";
                                var tagSystem = "";
                                if (testCustomSystemFolder != null)
                                {
                                    try
                                    {
                                        driveSystem = System.IO.Path.GetPathRoot(testCustomSystemFolder.Path);
                                        tagSystem = "CUSTOM";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }

                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "spr", (s, e) =>
                                {

                                });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "Save Folder" + (driveSaves.Length > 0 ? $" ({driveSaves})" : ""), (s, e) =>
                                {
                                    try
                                    {
                                        ChangeSavesFolder();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "System Folder" + (driveSystem.Length > 0 ? $" ({driveSystem})" : ""), (s, e) =>
                                {
                                    try
                                    {
                                        ChangeSystemFolder();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                            }

                            if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, "spr", (s, e) =>
                            {

                            });
                                var pState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{VM.SelectedSystem.Core.Name}-{VM.SelectedSystem.Name}-Pinned", false);
                                GenerateMenuFlyoutItem(ref coresSettingsMenu, pState ? "Unpin Core" : "Pin Core", (s, e) =>
                                {
                                    try
                                    {
                                        pState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{VM.SelectedSystem.Core.Name}-{VM.SelectedSystem.Name}-Pinned", false);
                                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{VM.SelectedSystem.Core.Name}-{VM.SelectedSystem.Name}-Pinned", !pState);
                                        PlatformService.PlayNotificationSoundDirect("alert");
                                        PlatformService.ShowNotificationMain("Done, Updates will be applied next startup", 2);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, Colors.Gray);
                            }

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

                            coreSettingsMenu.ShowAt(targetElement, point);
                        }
                        break;

                    case "screenshot":
                        {
                            MenuFlyout screenshotMenu = new MenuFlyout();
                            List<MenuFlyoutItem> screenshotFlyoutItems = new List<MenuFlyoutItem>();
                            GenerateMenuFlyoutItem(ref screenshotFlyoutItems, "Open", async (s, e) =>
                            {
                                try
                                {
                                    var state = await Windows.System.Launcher.LaunchFileAsync(SelectOption.attachedFile);
                                }
                                catch (Exception ex)
                                {
                                    if (PlatformService.isXBOXPure)
                                    {
                                        pushNormalNotification("This is not supported in XBOX");
                                        return;
                                    }
                                    else
                                    {
                                        PlatformService.PlayNotificationSoundDirect("error");
                                        PlatformService.ShowErrorMessageDirect(ex);
                                    }
                                }
                            }, Colors.Green);

                            GenerateMenuFlyoutItem(ref screenshotFlyoutItems, "Share", (s, e) =>
                            {
                                try
                                {
                                    DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                                    dataTransferManager.DataRequested += (ss, ee) =>
                                    {
                                        DataRequest request = ee.Request;
                                        request.Data.Properties.Title = $"{VM.SelectedSystem.Name}'s Screenshot";
                                        request.Data.Properties.Description = "Shared by RetriXGold";
                                        var rr = RandomAccessStreamReference.CreateFromFile(SelectOption.attachedFile);
                                        request.Data.SetBitmap(rr);
                                    };
                                    DataTransferManager.ShowShareUI();
                                }
                                catch (Exception ex)
                                {
                                    PlatformService.PlayNotificationSoundDirect("error");
                                    PlatformService.ShowErrorMessageDirect(ex);
                                }
                            }, Colors.DodgerBlue);
                            GenerateMenuFlyoutItem(ref screenshotFlyoutItems, "spr", (s, e) =>
                            {
                                try
                                {

                                }
                                catch (Exception ex)
                                {

                                }
                            });
                            GenerateMenuFlyoutItem(ref screenshotFlyoutItems, "Reveal", async (s, e) =>
                            {
                                try
                                {
                                    var picturesFolder = KnownFolders.PicturesLibrary;
                                    var retrixFolder = (StorageFolder)await picturesFolder.TryGetItemAsync("RetriXGold");
                                    if (retrixFolder != null)
                                    {
                                        var targetFolder = $"{VM.SelectedSystem.Core.OriginalSystemName} ({VM.SelectedSystem.Core.Name})";
                                        var saveFolder = (StorageFolder)await retrixFolder.TryGetItemAsync(targetFolder);
                                        if (saveFolder != null)
                                        {
                                            var successfolder = await Windows.System.Launcher.LaunchFolderAsync(saveFolder);
                                        }
                                        else
                                        {
                                            var successfolder = await Windows.System.Launcher.LaunchFolderAsync(retrixFolder);
                                        }
                                    }
                                    else
                                    {

                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (PlatformService.isXBOXPure)
                                    {
                                        pushNormalNotification("This is not supported in XBOX");
                                        return;
                                    }
                                    else
                                    {
                                        PlatformService.PlayNotificationSoundDirect("error");
                                        PlatformService.ShowErrorMessageDirect(ex);
                                    }
                                }
                            }, Colors.Gray);

                            GenerateMenuFlyoutItem(ref screenshotFlyoutItems, "spr", (s, e) =>
                            {
                                try
                                {

                                }
                                catch (Exception ex)
                                {

                                }
                            });
                            GenerateMenuFlyoutItem(ref screenshotFlyoutItems, "Delete", async (s, e) =>
                            {
                                try
                                {
                                    PlatformService.PlayNotificationSoundDirect("notice");
                                    ConfirmConfig confirCoreSettings = new ConfirmConfig();
                                    confirCoreSettings.SetTitle("Delete Screenshot?");
                                    confirCoreSettings.SetMessage("Do you want to delete this screenshot?");
                                    confirCoreSettings.UseYesNo();
                                    bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                                    if (confirCoreSettingsState)
                                    {
                                        await SelectOption.attachedFile.DeleteAsync();
                                        dataSourceScreenshots.Remove(SelectOption);
                                        if (dataSourceScreenshots.Count == 0)
                                        {
                                            CoreQuickAccessContainer.UpdateLayout();
                                            PlatformService.pivotMainPosition = CoreQuickAccessContainer.VerticalOffset;
                                            returningFromSubPage = true;
                                            PrepareCoreData();
                                        }
                                        else
                                        {
                                            textBlockScreenshots.Content = $"Screenshots ({dataSourceScreenshots.Count})".ToUpper();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    PlatformService.PlayNotificationSoundDirect("error");
                                    PlatformService.ShowErrorMessageDirect(ex);
                                }
                            }, Colors.Red);
                            foreach (var mItem in screenshotFlyoutItems)
                            {
                                if (mItem.Text.Equals("spr"))
                                {
                                    MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                    screenshotMenu.Items.Add(menuFlyoutSeparator);
                                }
                                else
                                {
                                    screenshotMenu.Items.Add(mItem);
                                }
                            }

                            screenshotMenu.ShowAt(targetElement, point);
                        }
                        break;
                    case "supportsec":
                        MenuFlyout supportMenu = new MenuFlyout();
                        List<MenuFlyoutItem> menuFlyoutItems = new List<MenuFlyoutItem>();
                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "About", (s, e) =>
                        {
                            try
                            {
                                VM.ShowAbout.Execute(null);
                            }
                            catch (Exception ex)
                            {

                            }
                        }, Colors.Green);

                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "Help", (s, e) =>
                        {
                            try
                            {
                                VM.ShowHelp.Execute(null);
                            }
                            catch (Exception ex)
                            {

                            }
                        }, Colors.DodgerBlue);
                        /*GenerateMenuFlyoutItem(ref menuFlyoutItems, "Patreon", (s, e) =>
                        {
                            try
                            {
                                SupportLink();
                            }
                            catch (Exception ex)
                            {

                            }
                        });*/

                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "PayPal", (s, e) =>
                        {
                            try
                            {
                                SupportLinkPaypal();
                            }
                            catch (Exception ex)
                            {

                            }
                        }, Colors.Gold);
                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "spr", (s, e) =>
                        {
                        });

                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "Backup", (s, e) =>
                        {
                            try
                            {
                                _ = ExportSettings();
                            }
                            catch (Exception ex)
                            {

                            }
                        }, Colors.Green);
                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "Restore", (s, e) =>
                        {
                            try
                            {
                                _ = ImportSettingsSlotsAction();
                            }
                            catch (Exception ex)
                            {

                            }
                        }, Colors.DodgerBlue);
                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "Reset", (s, e) =>
                        {
                            try
                            {
                                ResetRetrix();
                            }
                            catch (Exception ex)
                            {

                            }
                        }, Colors.Orange);
                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "spr", (s, e) =>
                        {
                        });
                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "Contact", (s, e) =>
                        {
                            try
                            {
                                OpenContact();
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "GitHub", (s, e) =>
                        {
                            try
                            {
                                GitHubLink();
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                        GenerateMenuFlyoutItem(ref menuFlyoutItems, "Get W.U.T", (s, e) =>
                        {
                            try
                            {
                                OpenWUT();
                            }
                            catch (Exception ex)
                            {

                            }
                        });

                        foreach (var mItem in menuFlyoutItems)
                        {
                            if (mItem.Text.Equals("spr"))
                            {
                                MenuFlyoutSeparator menuFlyoutSeparator = new MenuFlyoutSeparator();
                                supportMenu.Items.Add(menuFlyoutSeparator);
                            }
                            else
                            {
                                supportMenu.Items.Add(mItem);
                            }
                        }

                        supportMenu.ShowAt(targetElement, point);

                        /*PlatformService.SupportSections = !PlatformService.SupportSections;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("SupportSections", PlatformService.SupportSections);
                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = PlatformService.SupportSections ? "VISIBLE" : "HIDDEN";
                                mItem.IsOn = PlatformService.SupportSections;
                                break;
                            }
                        }
                        RequiredToRegenerate = true;
                        */
                        break;

                    case "filter":
                        CoresFilter_Click(null, null);
                        break;

                    case "shortcuts":
                        Shortcuts(null);
                        break;

                    case "backup":
                        await ExportSettings();
                        break;

                    case "restore":
                        await ImportSettingsSlotsAction();
                        break;

                    case "exit":
                        ShowExit();
                        break;

                    case "full":
                        PlatformService.FullScreen = !PlatformService.FullScreen;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("FullScreen", PlatformService.FullScreen);
                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = PlatformService.FullScreen ? "ON" : "OFF";
                                break;
                            }
                        }
                        if (PlatformService.FullScreen)
                        {
                            if (!AppView.IsFullScreenMode)
                            {
                                AppView.TryEnterFullScreenMode();
                            }
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.FullScreenLegacy;
                            localNotificationData.message = "FullScreen Enabled";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        else
                        {
                            if (AppView.IsFullScreenMode)
                            {
                                AppView.ExitFullScreenMode();
                            }
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.FullScreenLegacy;
                            localNotificationData.message = "FullScreen Disabled!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        RequiredToRegenerate = true;
                        break;

                    case "crash":
                        ShowErrorsList = !ShowErrorsList;
                        BindingsUpdate();
                        break;

                    case "checku":
                        RetrixUpdateOnline_Click();
                        break;

                    case "reset-system":
                        ResetRetrix();
                        break;

                    case "help":
                        VM.ShowHelp.Execute(null);
                        break;

                    case "about":
                        VM.ShowAbout.Execute(null);
                        break;

                    case "github":
                        GitHubLink();
                        break;

                    case "support":
                        SupportLink();
                        break;

                    case "supportp":
                        SupportLinkPaypal();
                        break;

                    case "autovfs":
                        PlatformService.AutoResolveVFSIssues = !PlatformService.AutoResolveVFSIssues;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("AutoResolveVFSIssues", PlatformService.AutoResolveVFSIssues);
                        var AutoResolveVFSIssuesState = PlatformService.AutoResolveVFSIssues;

                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = AutoResolveVFSIssuesState ? "ON" : "OFF";
                                break;
                            }
                        }
                        if (AutoResolveVFSIssuesState)
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.RefreshLegacy;
                            localNotificationData.message = "VFS Resolver Active";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.RefreshLegacy;
                            localNotificationData.message = "VFS Resolver Disabled!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        RequiredToRegenerate = true;
                        break;

                    case "indexer":
                        PlatformService.UseWindowsIndexer = !PlatformService.UseWindowsIndexer;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("UseWindowsIndexer", PlatformService.UseWindowsIndexer);
                        var UseWindowsIndexerState = PlatformService.UseWindowsIndexer;

                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = UseWindowsIndexerState ? "ON" : "OFF";
                                break;
                            }
                        }
                        if (UseWindowsIndexerState)
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.Search;
                            localNotificationData.message = "Windows Indexer Active";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.Search;
                            localNotificationData.message = "Windows Indexer Disabled!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        RequiredToRegenerate = true;
                        break;

                    case "extraask":
                        PlatformService.ExtraConfirmation = !PlatformService.ExtraConfirmation;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("ExtraConfirmation", PlatformService.ExtraConfirmation);
                        var ExtraConfirmationState = PlatformService.ExtraConfirmation;

                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = ExtraConfirmationState ? "ON" : "OFF";
                                break;
                            }
                        }
                        if (ExtraConfirmationState)
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.RefreshLegacy;
                            localNotificationData.message = "Extra Confirm Active";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.RefreshLegacy;
                            localNotificationData.message = "Extra Confirm Disabled!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        RequiredToRegenerate = true;
                        break;

                    case "mute":
                        PlatformService.MuteSFX = !PlatformService.MuteSFX;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("MuteSFX", PlatformService.MuteSFX);
                        var SFXState = PlatformService.MuteSFX;
                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = !SFXState ? "ON" : "OFF";
                                break;
                            }
                        }
                        if (!SFXState)
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.AudioLegacy;
                            localNotificationData.message = "Sound Effects Enabled";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.AudioLegacy;
                            localNotificationData.message = "Sound Effects Disabled!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        RequiredToRegenerate = true;
                        break;

                    case "backp":
                        App.HandleBackPress = !App.HandleBackPress;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("HandleBackPress", App.HandleBackPress);
                        var HandleBackPressState = App.HandleBackPress;
                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = HandleBackPressState ? "ON" : "OFF";
                                break;
                            }
                        }
                        if (HandleBackPressState)
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.BackLegacy;
                            localNotificationData.message = "GoBack Handle Enabled";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.BackLegacy;
                            localNotificationData.message = "GoBack Handle Disabled!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        RequiredToRegenerate = true;
                        break;


                    case "indicators":
                        PlatformService.ShowIndicators = !PlatformService.ShowIndicators;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("ShowIndicators", PlatformService.ShowIndicators);
                        var ShowIndicatorsState = PlatformService.ShowIndicators;
                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = ShowIndicatorsState ? "ON" : "OFF";
                                break;
                            }
                        }
                        if (ShowIndicatorsState)
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.BackLegacy;
                            localNotificationData.message = "OnScreen Indicators Enabled";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.BackLegacy;
                            localNotificationData.message = "OnScreen Indicators Disabled!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        RequiredToRegenerate = true;
                        break;

                    case "cleantemp":
                        App.CleanTempOnStartup = !App.CleanTempOnStartup;
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("CleanTempOnStartup", App.CleanTempOnStartup);
                        var CleanTempOnStartupState = App.CleanTempOnStartup;
                        foreach (var mItem in CoresQuickMenu)
                        {
                            if (mItem.Action.Equals(SelectOption.Action))
                            {
                                mItem.Tag = CleanTempOnStartupState ? "ON" : "OFF";
                                break;
                            }
                        }
                        if (CleanTempOnStartupState)
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.DeleteLegacy;
                            localNotificationData.message = "Clean Temp Enabled";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                            CleanTempFolder(true);
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            var localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.DeleteLegacy;
                            localNotificationData.message = "Clean Temp Disabled!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                        RequiredToRegenerate = true;
                        break;

                    //Reset selected core data only
                    case "reset":

                        break;

                    case "core":
                        await VM.GameSystemSelectedHandler(SelectOption.Core);
                        PrepareCoreData(SelectOption.SuggestedCore);
                        break;

                    //BIOS Import from main page
                    case "biosci":
                        ImportBIOSDialog();
                        break;

                    //Start Core without content
                    case "start":
                        GamePlayerView.isUpscaleActive = false;
                        StartCore_Click();
                        break;

                    //Start Core without content
                    case "startp":
                        StorageFile entryPoint = null;
                        switch (VM.SelectedSystem.Core.Name)
                        {
                            //Cave story
                            case "NXEngine":
                                var fileName = @"NXEngine - System\Cave Story\Doukutsu.exe";
                                entryPoint = (StorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
                                break;

                            //Doom
                            case "PrBoom":
                                var doomName = @"PrBoom - System\doom1.wad";
                                entryPoint = (StorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(doomName);
                                break;

                            //Doom
                            case "TyrQuake":
                                var quakeName = @"TyrQuake - System\quake\pak0.pak";
                                entryPoint = (StorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(quakeName);
                                break;

                            default:
                                //This case happens when test content available
                                entryPoint = await GetTestContentForCurrentSystem();
                                break;
                        }
                        if (entryPoint == null)
                        {
                            PlatformService.ShowNotificationMain("Unable to start core due missing file!", 3);
                        }
                        else
                        {
                            GamePlayerView.isUpscaleActive = false;
                            await VM.StartSingleGame(entryPoint, true);
                        }
                        break;

                    //BIOS Import from selected core
                    case "cbiosci":
                        PureCoreImportBIOS_Click();
                        break;

                    case "update-online":
                        PureCoreUpdateOnline_Click();
                        break;

                    case "update-offline":
                        PureCoreUpdate_Click();
                        break;

                    case "update-remove":
                        RemoveCoreUpdate_Click();
                        break;

                    //Delete anycore
                    case "delete":
                        AnyCoreDelete_Click();
                        break;

                    //Change saves location
                    case "saves":
                        ChangeSavesFolder();
                        break;

                    //Change system location
                    case "system":
                        ChangeSystemFolder();
                        break;

                    //BIOS sample for anycore (Download)
                    case "bios-sample":
                        AnyCoreBiosSample_Click();
                        break;

                    //Import BIOS map for anycore
                    case "bios-map":
                        AnyCoreAdvancedImport();
                        break;

                    //Settings for anycore
                    case "settings":
                        ConsoleSettings_Click();
                        break;

                    case "import-folder":
                        ImportFolderAction();
                        break;

                    case "pick-folder":
                        {
                            var folderState = await VM.SelectedSystem.SetGamesDirectoryAsync();
                            if (folderState)
                            {
                                PlatformService.PlayNotificationSoundDirect("success");
                                PlatformService.ShowNotificationMain("Games folder set for " + VM.SelectedSystem.Name, 2);
                                ReloadCorePageHandler(null, null);
                                //AllGetterReload_Click(null, null);
                            }
                        }
                        break;

                    case "change-folder":
                        {
                            var folderState = await VM.SelectedSystem.SetGamesDirectoryAsync();
                            if (folderState)
                            {
                                SelectOption.ProgressState = Visibility.Visible;
                                var gamesFolder = await VM.SelectedSystem.GetGamesDirectoryAsync();
                                PlatformService.PlayNotificationSoundDirect("alert");
                                string DialogTitle = "Games Folder";
                                string DialogMessage = $"Current folder for ({VM.SelectedSystem.Name}):\n{gamesFolder.Path}\n\n Do you want to change it.";
                                string[] DialogButtons = new string[] { $"Change", "Reset", "Cancel" };
                                int[] DialogButtonsIds = new int[] { 2, 1, 3 };

                                var ReplacePromptDialog = Helpers.CreateDialog(DialogTitle, DialogMessage, DialogButtons);
                                WinUniversalTool.Views.DialogResultCustom dialogResultCustom = WinUniversalTool.Views.DialogResultCustom.Nothing;
                                SelectOption.ProgressState = Visibility.Collapsed;
                                var ReplaceResult = await ReplacePromptDialog.ShowAsync2(dialogResultCustom);
                                if (Helpers.DialogResultCheck(ReplacePromptDialog, 2))
                                {
                                    folderState = await VM.SelectedSystem.SetGamesDirectoryAsync(true);
                                    if (folderState)
                                    {
                                        PlatformService.PlayNotificationSoundDirect("success");
                                        PlatformService.ShowNotificationMain("Games folder set for " + VM.SelectedSystem.Name, 2);
                                        ReloadCorePageHandler(null, null);
                                    }
                                }
                                else if (Helpers.DialogResultCheck(ReplacePromptDialog, 1))
                                {
                                    PlatformService.RemoveGamesFolderFromAccessList(VM.SelectedSystem.Name);
                                    PlatformService.PlayNotificationSoundDirect("success");
                                    PlatformService.ShowNotificationMain("Games folder reseted for " + VM.SelectedSystem.Name, 2);
                                    ReloadCorePageHandler(null, null);
                                }
                            }
                        }
                        break;

                    case "change-root":
                        {
                            {
                                PlatformService.PlayNotificationSoundDirect("alert");
                                var currentRoot = await PlatformService.GetGlobalFolder();
                                string DialogTitle = "Games Root";
                                string dialogMessage = "";
                                if (currentRoot != null)
                                {
                                    dialogMessage = $"Current root:\n{currentRoot.Path}\n\n";
                                }
                                string DialogMessage = $"{dialogMessage}How this works?\nGames root will be applied to all consoles, RetriXGold will try to match your folders names with the selected console, once the match found it will be selected by default as games folder\n\nNote: you can override this root by choosing another one from the console's page";
                                string[] DialogButtons = new string[] { $"Change", "Reset", "Cancel" };
                                int[] DialogButtonsIds = new int[] { 2, 1, 3 };

                                if (currentRoot == null)
                                {
                                    DialogButtons = new string[] { $"Select", "Cancel" };
                                    DialogButtonsIds = new int[] { 2, 1 };
                                }

                                var ReplacePromptDialog = Helpers.CreateDialog(DialogTitle, DialogMessage, DialogButtons);
                                WinUniversalTool.Views.DialogResultCustom dialogResultCustom = WinUniversalTool.Views.DialogResultCustom.Nothing;
                                var ReplaceResult = await ReplacePromptDialog.ShowAsync2(dialogResultCustom);
                                if (Helpers.DialogResultCheck(ReplacePromptDialog, 2))
                                {
                                    {
                                        var selectedFolder = await PlatformService.SetGlobalFolder(true);
                                        if (selectedFolder != null)
                                        {
                                            PlatformService.PlayNotificationSoundDirect("success");
                                            PlatformService.ShowNotificationMain("Games root successfully selected", 2);
                                            SelectOption.BorderColor = new SolidColorBrush(Colors.Green);

                                            /*
                                            {
                                                ShowLoader(true);
                                                await Task.Delay(500);
                                                RetriXGoldCoresLoader(false);
                                            }
                                            */
                                        }
                                    }
                                }
                                else if (Helpers.DialogResultCheck(ReplacePromptDialog, 1))
                                {
                                    PlatformService.RemoveGlobalFolder();
                                    PlatformService.PlayNotificationSoundDirect("success");
                                    PlatformService.ShowNotificationMain("Games root reseted", 2);
                                    SelectOption.BorderColor = new SolidColorBrush((Windows.UI.Color)Helpers.HexToColor("#66ffa500"));
                                    /*
                                    {
                                        RetriXGoldCoresLoader(false);
                                    }
                                    */
                                }
                            }
                        }
                        break;

                    case "privacy":
                        string message = $"No statistics will be collected at all\nIn general Retrix doesn't allow any kind of data collection as respect of users privacy\nEnjoy :)";
                        _ = UserDialogs.Instance.AlertAsync(message, "Statistics");
                        break;

                    case "custom-backup":
                        ExportSettings(true);
                        break;

                    case "custom-restore":
                        ImportSettingsSlotsAction(null, true);
                        break;

                    case "pincore":
                        var pinnedState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{VM.SelectedSystem.Core.Name}-{VM.SelectedSystem.Name}-Pinned", false);
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{VM.SelectedSystem.Core.Name}-{VM.SelectedSystem.Name}-Pinned", !pinnedState);
                        ReloadCorePageHandler(null, null);
                        PlatformService.PlayNotificationSoundDirect("alert");
                        PlatformService.ShowNotificationMain("Done, Updates will be applied next startup", 2);
                        break;

                    case "recent":
                        GlobalSelectedItem = SelectOption;
                        RecentsContextMenu.ShowAt(targetElement, point);
                        break;

                    case "game":
                        GamePlayerView.isUpscaleActive = false;
                        VM.PlayGameByName(SelectOption.Recent.GameName);
                        break;

                    case "home":
                        HideSubPages(null, null);
                        break;

                    case "resolve":
                        {
                            PlatformService.PlayNotificationSoundDirect("alert");
                            ConfirmConfig confirmResolve = new ConfirmConfig();
                            confirmResolve.SetTitle("Resolve Links");
                            confirmResolve.SetMessage("This action will help you to relink recent games from new folder\nOr when you restore backup on another device.\n\nDo you want to start?");
                            confirmResolve.UseYesNo();
                            var StartResolve = await UserDialogs.Instance.ConfirmAsync(confirmResolve);
                            if (StartResolve)
                            {
                                VM.ResolveGames();
                            }
                        }
                        break;

                    case "wut":
                        OpenWUT();
                        break;

                    case "contact":
                        OpenContact();
                        break;

                    case "retroarch":
                        OpenRetroArch();
                        break;

                    default:
                        break;
                }
                if (RequiredToRegenerate)
                {
                    foreach (var mItem in CoresQuickMenu)
                    {
                        if (mItem.Action.Equals("supportsec"))
                        {
                            itemsSwitchs.Add("support", mItem);
                            break;
                        }
                    }
                    foreach (var mItem in CoresQuickMenu)
                    {
                        if (mItem.Action.Equals("settingssec"))
                        {
                            itemsSwitchs.Add("settings", mItem);
                            break;
                        }
                    }
                    GenerateMenu(itemsSwitchs, "", CoresQuickMenu, CoresQuickAccessContainer, false, false, true);
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

        public static Dictionary<string, string> TestContentBySystem = new Dictionary<string, string>()
        {
            {"atari 2600" , "sheepitup_ntsc.a26" },
            {"wonderswan color" , "swandriving.wsc" },
            {"colecovision" , "Controller Tester.col" },
            {"vectrex" , "3D Mine Storm (USA).vec" },
            {"game & watch" , "Banana.mgw" },
            {"jump n' bump" , "jumpbump.dat" },
            {"lowresnx" , "candy.nx" },
            {"intellivision" , "4-Tris.rom" },
            {"gameboy" , "sheepitup.gb" },
            {"gameboy+" , "sheepitup.gb" },
            {"gameboy advance" , "GBAUtilityTools.gba" },
            {"nes" , "240p Test Suite.nes" },
            {"pokémon mini" , "Touhou - Bad Apple.min" },
            {"snes" , "240pSuite.sfc" },
            {"super nintendo" , "240pSuite.sfc" },
            {"virtual boy" , "Ballface.vb" },
            {"pocketcdg" , "Weatherly - Danny Boy.cdg" },
            {"game gear" , "ButtonTest.gg" },
            {"master system" , "Genesis 6 Button.sms" },
            {"sega wide" , "240pSuite-1.22.bin" },
            {"mega drive" , "240pSuite-1.22.bin" },
            {"neogeo pocket" , "Gears of Fate.ngp" },
            {"playstation" , "240pTestSuitePS1-EMU.cue" },
            {"tic-80" , "Candy Rain.tic" },
        };
        public async Task<StorageFile> GetTestContentForCurrentSystem()
        {
            StorageFile testContent = null;
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var testName = "";
                if (TestContentBySystem.TryGetValue(VM.SelectedSystem.Name.ToLower(), out testName))
                {
                    testContent = (StorageFile)await localFolder.TryGetItemAsync($"TestContent\\{testName}");
                    if (testContent == null)
                    {
                        //Check if user added test content by his own
                        testContent = (StorageFile)await localFolder.TryGetItemAsync($"TestContent\\{VM.SelectedSystem.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            return testContent;
        }
        public async void ChangeSavesFolder()
        {
            try
            {
                var coreName = VM.SelectedSystem.Core.Name;
                var tag = "saves";
                var currentFolder = await VM.SelectedSystem.GetSaveDirectoryAsync();
                var testCustomFolder = await PlatformService.GetCustomFolder(coreName, tag);
                if (testCustomFolder != null)
                {
                    currentFolder = testCustomFolder;
                }
                string message = $"Core saves folder:\n{currentFolder.Path}\n\nImportant: If the core doesn't fully support VFS some functions will fail\nYou can click on 'Reset' to revert this action";
                PlatformService.PlayNotificationSound("alert");
                string DialogTitle = "Saves Folder";
                string DialogMessage = message;
                string[] DialogButtons = new string[] { $"Change", "Reset", "Cancel" };
                int[] DialogButtonsIds = new int[] { 2, 1, 3 };

                var ReplacePromptDialog = Helpers.CreateDialog(DialogTitle, DialogMessage, DialogButtons);
                WinUniversalTool.Views.DialogResultCustom dialogResultCustom = WinUniversalTool.Views.DialogResultCustom.Nothing;
                var ReplaceResult = await ReplacePromptDialog.ShowAsync2(dialogResultCustom);
                if (Helpers.DialogResultCheck(ReplacePromptDialog, 2))
                {
                    var newFolder = await PlatformService.SetCustomSavesFolder(coreName, tag);
                    if (newFolder != null)
                    {
                        PlatformService.ShowNotificationMain($"Save set to:\n{newFolder.Path}", 3);
                        ReloadCorePageHandler(null, null);
                    }
                    else
                    {
                        PlatformService.ShowNotificationMain("Selection cancelled", 3);
                    }
                }
                else if (Helpers.DialogResultCheck(ReplacePromptDialog, 1))
                {
                    await PlatformService.ResetCustomFolder(coreName, tag);
                    PlatformService.ShowNotificationMain("Saves path reset done", 3);
                    ReloadCorePageHandler(null, null);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        public async void ChangeSystemFolder()
        {
            try
            {
                var coreName = VM.SelectedSystem.Core.Name;
                var tag = "system";
                var currentFolder = await VM.SelectedSystem.GetSystemDirectoryAsync();
                var testCustomFolder = await PlatformService.GetCustomFolder(coreName, tag);
                if (testCustomFolder != null)
                {
                    currentFolder = testCustomFolder;
                }
                string message = $"Core system folder:\n{currentFolder.Path}\n\nImportant: If the core doesn't fully support VFS some functions will fail\nYou can click on 'Reset' to revert this action";
                PlatformService.PlayNotificationSound("alert");
                string DialogTitle = "System Folder";
                string DialogMessage = message;
                string[] DialogButtons = new string[] { $"Change", "Reset", "Cancel" };
                int[] DialogButtonsIds = new int[] { 2, 1, 3 };

                var ReplacePromptDialog = Helpers.CreateDialog(DialogTitle, DialogMessage, DialogButtons);
                WinUniversalTool.Views.DialogResultCustom dialogResultCustom = WinUniversalTool.Views.DialogResultCustom.Nothing;
                var ReplaceResult = await ReplacePromptDialog.ShowAsync2(dialogResultCustom);
                if (Helpers.DialogResultCheck(ReplacePromptDialog, 2))
                {
                    var newFolder = await PlatformService.SetCustomSavesFolder(coreName, tag);
                    if (newFolder != null)
                    {
                        PlatformService.ShowNotificationMain($"System set to:\n{newFolder.Path}", 3);
                        ReloadCorePageHandler(null, null);
                    }
                    else
                    {
                        PlatformService.ShowNotificationMain("Selection cancelled", 3);
                    }
                }
                else if (Helpers.DialogResultCheck(ReplacePromptDialog, 1))
                {
                    await PlatformService.ResetCustomFolder(coreName, tag);
                    PlatformService.ShowNotificationMain("Saves path reset done", 3);
                    ReloadCorePageHandler(null, null);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            // Display message showing the label of the command that was invoked
            CoreApplication.Exit();
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
                        VM.GameSystemRecentsHolding.Execute(SelectOption.Recent);
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

        EventHandler PrepareCorePanel;
        EventHandler PrepareCorePanelComplete;
        Dictionary<string, string> coreData;
        bool returningFromSubPage
        {
            get
            {
                return PlatformService.returningFromSubPage;
            }
            set
            {
                PlatformService.returningFromSubPage = value;
            }
        }


        bool CorePageInProgress = false;
        bool gamesFolderReady = false;
        bool globalFolderReady = false;
        bool coreRequiresFolder = false;
        CancellationTokenSource screenshotsGetterCancellation = new CancellationTokenSource();
        ObservableCollection<CoresQuickListItem> CoreQuickMenu = new ObservableCollection<CoresQuickListItem>();

        private async void PrepareCoreData(bool SugggestedCore = false)
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
                CoresQuickAccessContainer.UpdateLayout();
                PlatformService.vScrollS = CoresQuickAccessContainer.VerticalOffset;
            }
            catch (Exception ex)
            {

            }
            if (PlatformService.ExtraDelay)
            {
                await Task.Delay(PlatformService.isMobile ? 500 : 500);
            }
            if (returningFromSubPage && PlatformService.AppStartedByRetroPass)
            {
                HideSystemGames();
                returningFromSubPage = false;
                try
                {
                    Windows.System.Launcher.LaunchUriAsync(new Uri(PlatformService.RetroPassLaunchOnExit));
                }
                catch (Exception ex)
                {

                }
                PlatformService.AppStartedByRetroPass = false;
                return;
            }
            try
            {
                SearchExtraFilterContainer.Visibility = Visibility.Collapsed;
                CorePageInProgress = true;
                bool RequiresOptionsReload = returningFromSubPage;
                MemoryValueContainer.Margin = new Thickness(0, 54, 20, 5);
                PreparingCoreLoading(true, "Preparing core..");
                VM.GamesRecentsList.Clear();
                CoreQuickAccessContainer.Content = null;
                VM.gamesLoadingInProgress = false;
                if (PlatformService.ExtraDelay)
                {
                    await Task.Delay(PlatformService.isMobile ? 500 : 500);
                }
                if (returningFromSubPage)
                {
                    //CorePagePivot.SelectedIndex = PlatformService.pivotPosition;
                }
                else
                {
                    CorePagePivot.SelectedIndex = 0;
                    PivotCoreGames.Header = "Games";
                    VM.GamesMainList?.Clear();
                    PlatformService.pivotMainPosition = 0;
                }
                PrepareCorePanel = (sender, e) =>
                {
                    coreData = null;
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        if (sender != null)
                        {
                            try
                            {
                                coreData = (Dictionary<string, string>)sender;
                            }
                            catch (Exception ex)
                            {
                                PlatformService.ShowErrorMessageDirect(ex);
                            }
                        }
                    });
                    VM.GetRecentGames(PrepareCorePanelComplete);
                };
                PrepareCorePanelComplete = (sender, e) =>
                {
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        if (PlatformService.ExtraDelay)
                        {
                            await Task.Delay(PlatformService.isMobile ? 500 : 500);
                        }
                        var coreCleanName = VM.GetCoreNameCleanForSelectedSystem();
                        var resolvedName = ResolveCoreName(coreCleanName);
                        var coreLibretroLink = resolvedName.StartsWith("https://") ? resolvedName : $"https://docs.libretro.com/library/{resolvedName}";
                        try
                        {
                            if (VM.SelectedSystem.Core.Name == "FinalBurn Neo" || VM.SelectedSystem.Core.Name == "FB Alpha")
                            {
                                ArcadeExtra = Visibility.Visible;
                            }
                            else
                            {
                                ArcadeExtra = Visibility.Collapsed;
                            }
                            BindingsUpdate();
                        }
                        catch
                        {

                        }
                        try
                        {
                            if (VM.SelectedSystem.Name == "PlayStation" || VM.SelectedSystem.Name == "PlayStation")
                            {
                                PSXExtra = Visibility.Visible;
                            }
                            else
                            {
                                PSXExtra = Visibility.Collapsed;
                            }
                            BindingsUpdate();
                        }
                        catch
                        {

                        }
                        try
                        {
                            CoreQuickMenu = new ObservableCollection<CoresQuickListItem>();
                            //Core Infos
                            var CoreInfoAction = GenerateMenuItem("Informations", "information", "Information", "info");

                            StackPanel stackPanel = new StackPanel();

                            ///Build Image & Title
                            Image image = new Image();
                            image.Width = 72;
                            image.Height = 72;
                            image.HorizontalAlignment = HorizontalAlignment.Center;
                            image.VerticalAlignment = VerticalAlignment.Center;
                            image.Margin = new Thickness(10, 15, 10, 10);
                            BitmapImage imageSource = new BitmapImage();
                            var uri = new System.Uri(VM.SelectedSystem.Symbol);
                            try
                            {
                                if (VM.SelectedSystem.Symbol.StartsWith("ms-appx:"))
                                {

                                    var imageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                                    IRandomAccessStream stream = await imageFile.OpenAsync(FileAccessMode.Read);
                                    await imageSource.SetSourceAsync(stream);
                                }
                                else
                                {
                                    imageSource = new BitmapImage(uri);
                                }
                                image.Source = imageSource;
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.Message);
                            }

                            TextBlock textBlock = new TextBlock();
                            textBlock.FontSize = 20;
                            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                            textBlock.VerticalAlignment = VerticalAlignment.Center;
                            textBlock.Margin = new Thickness(0, 5, 0, 5);
                            textBlock.TextAlignment = TextAlignment.Center;
                            textBlock.Text = $"{VM.SelectedSystem.Name}\n{VM.SelectedSystem.ManufacturerTemp}";
                            textBlock.FontSize = 15;
                            textBlock.TextWrapping = TextWrapping.WrapWholeWords;

                            if (coreData != null)
                            {
                                string SystemCompany = coreData["company"];
                                if (SystemCompany.Length > 1)
                                {
                                    textBlock.Text = $"{VM.SelectedSystem.Name} by {SystemCompany}";
                                }
                            }


                            //Generate Supported features section
                            StackPanel features = new StackPanel();
                            features.Orientation = Orientation.Horizontal;
                            features.HorizontalAlignment = HorizontalAlignment.Center;



                            ///Build Info
                            string SystemDescriptions = "";
                            string SystemYear = "";
                            if (coreData != null)
                            {
                                SystemDescriptions = coreData["desc"];
                                SystemYear = coreData["year"];
                            }

                            if (SystemYear.Length > 1)
                            {
                                /*StackPanel CoreYearBlock = new StackPanel();
                                CoreYearBlock.Orientation = Orientation.Horizontal;
                                CoreYearBlock.Margin = new Thickness(0, 0, 0, 5);
                                TextBlock YearTag = new TextBlock();
                                YearTag.FontWeight = FontWeights.Bold;
                                YearTag.Text = "Year:";
                                YearTag.Margin = new Thickness(5, 0, 5, 0);
                                YearTag.FontSize = 13;

                                TextBlock YearText = new TextBlock();
                                YearText.Text = SystemYear;
                                YearText.FontSize = 13;

                                CoreYearBlock.Children.Add(YearTag);
                                CoreYearBlock.Children.Add(YearText);
                                CoreYearBlock.Margin = new Thickness(15, 0, 0, 0);
                                stackPanel.Children.Add(CoreYearBlock);*/
                                Border yearFeature = new Border();
                                yearFeature.CornerRadius = new CornerRadius(5);
                                yearFeature.BorderBrush = new SolidColorBrush(Colors.Gold);
                                yearFeature.Margin = new Thickness(2, 2, 2, 2);
                                yearFeature.BorderThickness = new Thickness(2);
                                TextBlock yearFeatureText = new TextBlock();
                                yearFeatureText.Text = SystemYear;
                                yearFeatureText.Padding = new Thickness(3, 2, 3, 2);
                                yearFeatureText.FontWeight = FontWeights.Bold;
                                yearFeatureText.FontSize = 12;
                                yearFeature.Child = yearFeatureText;
                                features.Children.Add(yearFeature);
                            }

                            if (VM.SelectedSystem.Core.NativeArchiveSupport)
                            {
                                Border archiveFeature = new Border();
                                archiveFeature.CornerRadius = new CornerRadius(5);
                                archiveFeature.BorderBrush = new SolidColorBrush(Colors.DodgerBlue);
                                archiveFeature.Margin = new Thickness(2, 2, 2, 2);
                                archiveFeature.BorderThickness = new Thickness(2);
                                TextBlock archiveFeatureText = new TextBlock();
                                archiveFeatureText.Text = "ZIP";
                                archiveFeatureText.Padding = new Thickness(3, 2, 3, 2);
                                archiveFeatureText.FontWeight = FontWeights.Bold;
                                archiveFeatureText.FontSize = 12;
                                archiveFeature.Child = archiveFeatureText;
                                features.Children.Add(archiveFeature);
                            }
                            if (VM.SelectedSystem.Core.VFSSupport)
                            {
                                Border vfsFeature = new Border();
                                vfsFeature.CornerRadius = new CornerRadius(5);
                                vfsFeature.BorderBrush = new SolidColorBrush(Colors.LightGreen);
                                vfsFeature.Margin = new Thickness(2, 2, 2, 2);
                                vfsFeature.BorderThickness = new Thickness(2);
                                TextBlock vfsFeatureText = new TextBlock();
                                vfsFeatureText.Text = "VFS";
                                vfsFeatureText.Padding = new Thickness(3, 2, 3, 2);
                                vfsFeatureText.FontWeight = FontWeights.Bold;
                                vfsFeatureText.FontSize = 12;
                                vfsFeature.Child = vfsFeatureText;
                                features.Children.Add(vfsFeature);
                            }
                            features.Margin = new Thickness(0, -25, 0, 0);
                            var uiSettings = new Windows.UI.ViewManagement.UISettings();
                            var color = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
                            var sb = new SolidColorBrush(color);
                            sb.Opacity = 0.7;
                            features.Background = sb;


                            stackPanel.Children.Add(image);
                            stackPanel.Children.Add(features);
                            stackPanel.Children.Add(textBlock);

                            if (coreLibretroLink.Length > 1)
                            {
                                StackPanel CoreLinkBlock = new StackPanel();
                                CoreLinkBlock.Orientation = Orientation.Horizontal;
                                CoreLinkBlock.Margin = new Thickness(0, 0, 0, 5);
                                TextBlock LinkTag = new TextBlock();
                                LinkTag.FontWeight = FontWeights.Bold;
                                LinkTag.Text = "";
                                LinkTag.Margin = new Thickness(3, 0, 0, 0);
                                LinkTag.FontSize = 13;

                                HyperlinkButton LinkText = new HyperlinkButton();
                                LinkText.Content = $"Authors & Docs ({VM.SelectedSystem.Core.Name})";
                                LinkText.NavigateUri = new Uri(coreLibretroLink);
                                LinkText.FontSize = 13;

                                CoreLinkBlock.Children.Add(LinkTag);
                                CoreLinkBlock.Children.Add(LinkText);
                                CoreLinkBlock.Margin = new Thickness(15, 0, 0, 0);
                                stackPanel.Children.Add(CoreLinkBlock);
                            }

                            if (SystemDescriptions.Length > 1)
                            {
                                Grid CoreDescriptionsBlock = new Grid();
                                ColumnDefinition columnDefinition = new ColumnDefinition();
                                columnDefinition.Width = new GridLength(1, GridUnitType.Auto);
                                CoreDescriptionsBlock.ColumnDefinitions.Add(columnDefinition);
                                ColumnDefinition columnDefinition2 = new ColumnDefinition();
                                columnDefinition2.Width = new GridLength(1, GridUnitType.Star);
                                CoreDescriptionsBlock.ColumnDefinitions.Add(columnDefinition2);

                                CoreDescriptionsBlock.Margin = new Thickness(0, 0, 0, 5);
                                TextBlock DescriptionsTag = new TextBlock();
                                DescriptionsTag.FontWeight = FontWeights.Bold;
                                DescriptionsTag.Text = "";
                                DescriptionsTag.Margin = new Thickness(0, 0, 5, 0);
                                DescriptionsTag.FontSize = 14;

                                TextBlock DescriptionsText = new TextBlock();
                                DescriptionsText.Text = SystemDescriptions;
                                DescriptionsText.TextWrapping = TextWrapping.WrapWholeWords;
                                DescriptionsText.FontSize = 14;

                                CoreDescriptionsBlock.Children.Add(DescriptionsTag);
                                Grid.SetColumn(DescriptionsTag, 0);
                                CoreDescriptionsBlock.Children.Add(DescriptionsText);
                                Grid.SetColumn(DescriptionsText, 1);

                                CoreDescriptionsBlock.Margin = new Thickness(15, 0, 0, 0);
                                CoreDescriptionsBlock.MaxWidth = 850;
                                CoreDescriptionsBlock.HorizontalAlignment = HorizontalAlignment.Left;
                                stackPanel.Children.Add(CoreDescriptionsBlock);
                            }


                            if (VM.SelectedSystem.Core.SupportedExtensions != null && VM.SelectedSystem.Core.SupportedExtensions.Count > 0 && !VM.SelectedSystem.Core.SupportedExtensions.FirstOrDefault().Equals(".null"))
                            {
                                Border borderSeparator = new Border();
                                borderSeparator.BorderBrush = new SolidColorBrush(Colors.Gray);
                                borderSeparator.BorderThickness = new Thickness(0, 0, 0, 1);
                                borderSeparator.Margin = new Thickness(20, 3, 2, 2);
                                borderSeparator.HorizontalAlignment = HorizontalAlignment.Left;
                                borderSeparator.Width = PlatformService.InitWidthSize;
                                borderSeparator.Opacity = 0.6;
                                stackPanel.Children.Add(borderSeparator);

                                GridView CoreExtsBlock = new GridView();
                                CoreExtsBlock.ItemTemplate = this.Resources["ExtenstionTemplate"] as DataTemplate;
                                CoreExtsBlock.ItemContainerStyle = this.Resources["GridViewItemStyle1"] as Style;
                                CoreExtsBlock.Margin = new Thickness(0, 0, 0, 5);
                                List<CoreExtensionItem> coreExtensions = new List<CoreExtensionItem>();
                                List<string> extens = new List<string>();
                                int indexer = 0;
                                var supportedTypesList = VM.SelectedSystem.SupportedExtensions.ToList();

                                switch (VM.SelectedSystem.Core.Name)
                                {
                                    case "scummvm":
                                        supportedTypesList = new List<string>()
                                    {
                                        ".scummvm", ".scumm"
                                    };
                                        break;
                                }
                                foreach (var eItem in supportedTypesList)
                                {
                                    if (extens.Contains(eItem.ToLower()))
                                    {
                                        continue;
                                    }
                                    switch (eItem)
                                    {
                                        case ".rar":
                                        case ".tar":
                                        case ".gz":
                                            break;

                                        case ".7z":
                                            if (VM.SelectedSystem.Core.NativeArchiveNonZipSupport)
                                            {
                                                coreExtensions.Add(new CoreExtensionItem(eItem));
                                            }
                                            break;

                                        case ".zip":
                                            if (VM.SelectedSystem.Core.NativeArchiveSupport)
                                            {
                                                coreExtensions.Add(new CoreExtensionItem(eItem));
                                            }
                                            break;

                                        default:
                                            coreExtensions.Add(new CoreExtensionItem(eItem));
                                            break;
                                    }
                                    extens.Add(eItem);
                                    if (indexer > 15)
                                    {
                                        coreExtensions.Add(new CoreExtensionItem("More"));
                                        break;
                                    }
                                    indexer++;
                                }
                                CoreExtsBlock.ItemsSource = coreExtensions;
                                CoreExtsBlock.IsItemClickEnabled = true;
                                CoreExtsBlock.ItemClick += (sender2, e2) =>
                                {
                                    try
                                    {
                                        var exItem = (CoreExtensionItem)e2.ClickedItem;

                                        var info = PlatformService.ExtensionInfo(exItem.extTitle);
                                        if (info.Length > 0)
                                        {
                                            var message = $"What is {exItem.extTitle} type?\n{info}";
                                            _ = PlatformService.ShowMessageDirect(message);
                                        }
                                        else if (exItem.extTitle.Equals(".scummvm") || exItem.extTitle.Equals(".scummvm"))
                                        {
                                            var message = $"{VM.SelectedSystem.Name}'s supported types:\n{(String.Join(", ", supportedTypesList))}\n\nNote: ScummVM support wide range of types, games lists limited to 2 types to avoid a lot of items.. you can place .scummvm file inside the game to use direct start or add the game from scummVM UI directly which is better\nRetriXGold able to resolve compressed roms as well";
                                            _ = PlatformService.ShowMessageDirect(message);
                                        }
                                        else
                                        {
                                            supportedTypesList.Remove(".rar");
                                            supportedTypesList.Remove(".gz");
                                            supportedTypesList.Remove(".tar");
                                            if (!VM.SelectedSystem.Core.NativeArchiveNonZipSupport)
                                            {
                                                supportedTypesList.Remove(".7z");
                                            }
                                            if (!VM.SelectedSystem.Core.NativeArchiveSupport)
                                            {
                                                supportedTypesList.Remove(".zip");
                                            }
                                            var message = $"{VM.SelectedSystem.Name}'s supported types:\n{(String.Join(", ", supportedTypesList))}\n\nNote: Some consoles may show less types than the original core because of specific hardware, like SG1000 it will show only the supported types for SG1000 not for the core";
                                            _ = PlatformService.ShowMessageDirect(message);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                };
                                CoreExtsBlock.Margin = new Thickness(15, 0, 0, 0);
                                CoreExtsBlock.IsFocusEngaged = false;
                                CoreExtsBlock.AllowFocusOnInteraction = false;
                                CoreExtsBlock.IsFocusEngagementEnabled = false;
                                CoreExtsBlock.SelectionMode = ListViewSelectionMode.None;

                                stackPanel.Children.Add(CoreExtsBlock);
                            }

                            stackPanel.Margin = new Thickness(0, 0, 0, 5);
                            CoreInfoAction.CustomControl = stackPanel;
                            CoreInfoAction.Collapsed = VM.GamesRecentsList.Count > 0;
                            CoreQuickMenu.Add(CoreInfoAction);

                            gamesFolderReady = false;
                            globalFolderReady = false;
                            coreRequiresFolder = false;
                            if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                coreRequiresFolder = PlatformService.IsCoreRequiredGamesFolderDirect(VM.SelectedSystem.Core.Name);
                                //Check games folder state
                                if (coreRequiresFolder)
                                {
                                    try
                                    {
                                        if (!await VM.IsCoreGamesFolderAlreadySelected(VM.folderPickerTokenName))
                                        {
                                            gamesFolderReady = false;
                                        }
                                        else
                                        {
                                            gamesFolderReady = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                else
                                {
                                    gamesFolderReady = true;
                                }
                            }

                            if (!gamesFolderReady || !coreRequiresFolder)
                            {
                                //Check global root for possible matches
                                try
                                {
                                    var globalTest = await PlatformService.PickDirectory(VM.SelectedSystem.TempName, false, true, true);
                                    if (globalTest != null)
                                    {
                                        globalFolderReady = true;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }

                            if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                //Recent
                                if (VM.GamesRecentsList.Count > 0)
                                {
                                    if (gamesFolderReady || globalFolderReady)
                                    {
                                        foreach (var recent in VM.GamesRecentsList)
                                        {
                                            var GameAction = GenerateCoreMenuItem($"Played", "recent", recent.GameNameWithoutExtension, recent.GameSnapshot, $"{recent.OpenCounts}", recent.GamePlayedTime, recent, "RecentGridMenuItemTemplate");
                                            GameAction.StretchState = (Stretch)recent.StretchState;
                                            CoreQuickMenu.Add(GameAction);
                                        }
                                    }
                                    else
                                    {
                                        TextBlock gamesFolderNotice = new TextBlock();
                                        gamesFolderNotice.Text = "Set games folder to view this section";
                                        gamesFolderNotice.Margin = new Thickness(20, 5, 5, 25);
                                        var CoreRecentsAction = GenerateMenuItem("Played", "recent", "recent", "recent");
                                        CoreRecentsAction.CustomControl = gamesFolderNotice;
                                        CoreQuickMenu.Add(CoreRecentsAction);
                                    }
                                }
                            }

                            if (PlatformService.ExtraDelay)
                            {
                                await Task.Delay(PlatformService.isMobile ? 500 : 500);
                            }
                            //Management
                            if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                if (VM.SelectedSystem.Core.SupportNoGame)
                                {
                                    var totalDummyOpen = VM.totalDummyOpens;

                                    var StartCoreAction = GenerateMenuItem("Management", "start", "Start".ToUpper(), "media-play", totalDummyOpen > 0 ? $"{totalDummyOpen}" : "");
                                    StartCoreAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                    CoreQuickMenu.Add(StartCoreAction);
                                }
                                else
                                {
                                    switch (VM.SelectedSystem.Core.Name)
                                    {
                                        //Cave story
                                        case "NXEngine":
                                            {
                                                var totalDummyOpen = VM.totalDummyOpens;
                                                var StartCoreAction = GenerateMenuItem("Management", "startp", "Start".ToUpper(), "media-play", totalDummyOpen > 0 ? $"{totalDummyOpen}" : "");
                                                StartCoreAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                                CoreQuickMenu.Add(StartCoreAction);
                                            }
                                            break;

                                        //Doom
                                        case "PrBoom":
                                            {
                                                var totalDummyOpen = VM.totalDummyOpens;
                                                var StartCoreAction = GenerateMenuItem("Management", "startp", "Start".ToUpper(), "media-play", totalDummyOpen > 0 ? $"{totalDummyOpen}" : "");
                                                StartCoreAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                                CoreQuickMenu.Add(StartCoreAction);
                                            }
                                            break;

                                        //Quake
                                        case "TyrQuake":
                                            {
                                                var totalDummyOpen = VM.totalDummyOpens;
                                                var StartCoreAction = GenerateMenuItem("Management", "startp", "Start".ToUpper(), "media-play", totalDummyOpen > 0 ? $"{totalDummyOpen}" : "");
                                                StartCoreAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                                CoreQuickMenu.Add(StartCoreAction);
                                            }
                                            break;
                                    }
                                }
                                try
                                {
                                    //Check if core has test content
                                    var testContent = await GetTestContentForCurrentSystem();
                                    if (testContent != null)
                                    {
                                        var totalDummyOpen = VM.totalDummyOpens;
                                        var TestCoreAction = GenerateMenuItem("Management", "startp", "Start".ToUpper(), "media-test", totalDummyOpen > 0 ? $"{totalDummyOpen}" : "", "DEMO");
                                        TestCoreAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                        CoreQuickMenu.Add(TestCoreAction);
                                    }
                                }
                                catch (Exception ex)
                                {

                                }

                            }

                            var CoreSettingsAction = GenerateMenuItem("Management", "ctools", "Tools".ToUpper(), "settings", "", "");
                            CoreSettingsAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                            CoreSettingsAction.TagVisibilityGray = Visibility.Visible;
                            CoreSettingsAction.TagGray = VM.SelectedSystem.Version;
                            CoreQuickMenu.Add(CoreSettingsAction);

                            if (!gamesFolderReady)
                            {
                                if (globalFolderReady)
                                {
                                    var PickFolderAction = GenerateMenuItem("Management", "change-folder", "Games".ToUpper(), "root", "", "ROOT");
                                    PickFolderAction.BorderColor = new SolidColorBrush((Windows.UI.Color)Helpers.HexToColor("#66ffa500"));
                                    PickFolderAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                    CoreQuickMenu.Add(PickFolderAction);
                                }
                                else
                                {
                                    var PickFolderAction = GenerateMenuItem("Management", "pick-folder", "Games".ToUpper(), "root", "", "REQUIRED");
                                    PickFolderAction.BorderColor = new SolidColorBrush((Windows.UI.Color)Helpers.HexToColor("#66ff0000"));
                                    PickFolderAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && gamesFolderReady;
                                    CoreQuickMenu.Add(PickFolderAction);
                                }
                            }
                            else
                            {
                                if (await VM.IsCoreGamesFolderAlreadySelected(VM.folderPickerTokenName))
                                {
                                    var PickFolderAction = GenerateMenuItem("Management", "change-folder", "Games".ToUpper(), "root", "", "SELECTED");
                                    PickFolderAction.BorderColor = new SolidColorBrush(Colors.Green);
                                    PickFolderAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && gamesFolderReady;
                                    CoreQuickMenu.Add(PickFolderAction);
                                }
                                else if (globalFolderReady)
                                {
                                    var PickFolderAction = GenerateMenuItem("Management", "change-folder", "Games".ToUpper(), "root", "", "ROOT");
                                    PickFolderAction.BorderColor = new SolidColorBrush((Windows.UI.Color)Helpers.HexToColor("#66ffa500"));
                                    PickFolderAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                    CoreQuickMenu.Add(PickFolderAction);
                                }
                            }

                            /*if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                var ImportBIOSCollectionAction = GenerateMenuItem("Management", "cbiosci", "Import Files", "xap", "", "CORE SYSTEM");
                                CoreQuickMenu.Add(ImportBIOSCollectionAction);

                                var ImportFolderAction = GenerateMenuItem("Management", "import-folder", "Import Folder", "folder-microsoft", "", "CORE SYSTEM");
                                ImportFolderAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                CoreQuickMenu.Add(ImportFolderAction);
                            }*/


                            /* if (VM.SelectedSystem.AnyCore)
                             {
                                 if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                                 {
                                     var SettingsAction = GenerateMenuItem("Management", "settings", "Settings", "settings", "", "ANYCORE");
                                     SettingsAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                     CoreQuickMenu.Add(SettingsAction);
                                 }
                                 if (!VM.SelectedSystem.ImportedCore)
                                 {
                                     var BIOSMapAction = GenerateMenuItem("Management", "bios-map", "Import & Update", "notepad", "", "");
                                     BIOSMapAction.TagVisibilityGray = Visibility.Visible;
                                     BIOSMapAction.TagGray = VM.SelectedSystem.Version;
                                     BIOSMapAction.Collapsed = VM.GamesRecentsList.Count > 0 && !VM.SelectedSystem.Core.SupportNoGame && (gamesFolderReady || globalFolderReady);
                                     CoreQuickMenu.Add(BIOSMapAction);
                                 }
                             }
                            */

                            //Issues
                            try
                            {
                                if (coresIssues != null && coresIssues.Count > 0)
                                {
                                    StackPanel entryBlock = new StackPanel();
                                    bool itemsAdded = false;
                                    foreach (var cItem in coresIssues)
                                    {
                                        if (cItem.consoleName.ToLower().Equals(VM.SelectedSystem.TempName.ToLower()))
                                        {
                                            foreach (var iItem in cItem.consoleIssues)
                                            {
                                                try
                                                {
                                                    bool ignoreIssue = false;
                                                    switch (iItem.targetArch.ToLower())
                                                    {
                                                        case "arm":
#if TARGET_X64
                                                            ignoreIssue = true;
#elif TARGET_ARM
                   ignoreIssue = false;
#elif TARGET_X86
                                                            ignoreIssue = true;
#endif
                                                            break;

                                                        case "x64":
#if TARGET_X64
                                                            ignoreIssue = false;
#elif TARGET_ARM
                   ignoreIssue = true;
#elif TARGET_X86
                                                            ignoreIssue = true;
#endif
                                                            break;

                                                        case "x86":
#if TARGET_X64
                                                            ignoreIssue = true;
#elif TARGET_ARM
                   ignoreIssue = true;
#elif TARGET_X86
                                                            ignoreIssue = false;
#endif
                                                            break;
                                                    }

                                                    if (iItem.customCore != null && iItem.customCore.Length > 0)
                                                    {
                                                        if (!VM.SelectedSystem.Core.Name.ToLower().Equals(iItem.customCore.ToLower()))
                                                        {
                                                            ignoreIssue = true;
                                                        }
                                                    }
                                                    if (ignoreIssue)
                                                    {
                                                        continue;
                                                    }

                                                    if (iItem.isEnhancement)
                                                    {
                                                        {
                                                            StackPanel eTextBlock = new StackPanel();
                                                            eTextBlock.Orientation = Orientation.Horizontal;

                                                            {
                                                                Border eTextTagBorder = new Border();
                                                                if (iItem.enhancementTagColor != null && iItem.enhancementTagColor.Length > 0)
                                                                {
                                                                    SolidColorBrush TagColor = new SolidColorBrush(Colors.Purple);
                                                                    try
                                                                    {
                                                                        string TColor = iItem.enhancementTagColor;
                                                                        if (TColor.StartsWith("#"))
                                                                        {
                                                                            TagColor = new SolidColorBrush((Windows.UI.Color)Helpers.HexToColor(TColor));
                                                                        }
                                                                        else
                                                                        {
                                                                            TagColor = new SolidColorBrush((Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), TColor.ToUpper()));
                                                                        }
                                                                    }
                                                                    catch (Exception ex)
                                                                    {

                                                                    }
                                                                    eTextTagBorder.BorderBrush = TagColor;
                                                                }
                                                                else
                                                                {
                                                                    eTextTagBorder.BorderBrush = new SolidColorBrush(Colors.Purple);
                                                                }
                                                                eTextTagBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                                                                eTextTagBorder.MinWidth = 50;

                                                                if (iItem.issueLink != null && iItem.issueLink.Length > 0)
                                                                {
                                                                    HyperlinkButton eTextTag = new HyperlinkButton();
                                                                    eTextTag.Content = iItem.enhancementTag.ToUpper();
                                                                    eTextTag.NavigateUri = new Uri(iItem.issueLink);
                                                                    eTextTag.Padding = new Thickness(1);
                                                                    eTextTag.HorizontalAlignment = HorizontalAlignment.Center;
                                                                    eTextTagBorder.Child = eTextTag;
                                                                    eTextBlock.Children.Add(eTextTagBorder);
                                                                }
                                                                else
                                                                {
                                                                    TextBlock eTextTag = new TextBlock();
                                                                    eTextTag.Text = iItem.enhancementTag.ToUpper();
                                                                    eTextTag.Padding = new Thickness(1);
                                                                    eTextTag.HorizontalAlignment = HorizontalAlignment.Center;
                                                                    eTextTagBorder.Child = eTextTag;
                                                                    eTextBlock.Children.Add(eTextTagBorder);
                                                                }
                                                            }
                                                            {
                                                                TextBlock eTextTag = new TextBlock();
                                                                eTextTag.Text = iItem.issueText;
                                                                eTextTag.Padding = new Thickness(1);
                                                                eTextTag.Margin = new Thickness(3, 0, 0, 0);
                                                                eTextBlock.Children.Add(eTextTag);
                                                            }
                                                            if (iItem.reportedBy != null && iItem.reportedBy.Length > 0)
                                                            {
                                                                {
                                                                    TextBlock eTextTag = new TextBlock();
                                                                    eTextTag.Text = $" (by {iItem.reportedBy.ToUpper()})";
                                                                    eTextTag.Foreground = new SolidColorBrush(Colors.DodgerBlue);
                                                                    eTextTag.Margin = new Thickness(0, 1.5, 0, 0);
                                                                    eTextTag.TextTrimming = TextTrimming.CharacterEllipsis;
                                                                    eTextBlock.Children.Add(eTextTag);
                                                                }
                                                            }
                                                            eTextBlock.Margin = new Thickness(19, 0, 0, 5);
                                                            entryBlock.Children.Add(eTextBlock);

                                                            eTextBlock.Margin = new Thickness(19, 0, 0, 5);
                                                            Border spr = new Border();
                                                            spr.BorderBrush = new SolidColorBrush(Colors.Transparent);
                                                            spr.BorderThickness = new Thickness(0, 1, 0, 0);
                                                            spr.Margin = new Thickness(20, 0, 0, 5);
                                                            spr.MaxWidth = 200;
                                                            entryBlock.Children.Add(spr);

                                                            itemsAdded = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        {
                                                            StackPanel eTextBlock = new StackPanel();
                                                            eTextBlock.Orientation = Orientation.Horizontal;
                                                            {
                                                                Border eTextTagBorder = new Border();
                                                                eTextTagBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                                                                eTextTagBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                                                                eTextTagBorder.MinWidth = 50;

                                                                if (iItem.issueLink != null && iItem.issueLink.Length > 0)
                                                                {
                                                                    HyperlinkButton eTextTag = new HyperlinkButton();
                                                                    eTextTag.Content = "ISSUE";
                                                                    eTextTag.NavigateUri = new Uri(iItem.issueLink);
                                                                    eTextTag.Padding = new Thickness(1);
                                                                    eTextTag.HorizontalAlignment = HorizontalAlignment.Center;

                                                                    eTextTagBorder.Child = eTextTag;
                                                                    eTextBlock.Children.Add(eTextTagBorder);
                                                                }
                                                                else
                                                                {
                                                                    TextBlock eTextTag = new TextBlock();
                                                                    eTextTag.Text = "ISSUE";
                                                                    eTextTag.Padding = new Thickness(1);
                                                                    eTextTag.HorizontalAlignment = HorizontalAlignment.Center;

                                                                    eTextTagBorder.Child = eTextTag;
                                                                    eTextBlock.Children.Add(eTextTagBorder);
                                                                }
                                                            }
                                                            {
                                                                TextBlock eTextTag = new TextBlock();
                                                                eTextTag.Text = iItem.issueText;
                                                                eTextTag.Margin = new Thickness(3, 0, 0, 0);
                                                                eTextTag.TextWrapping = TextWrapping.WrapWholeWords;
                                                                eTextBlock.Children.Add(eTextTag);
                                                            }
                                                            if (iItem.reportedBy != null && iItem.reportedBy.Length > 0)
                                                            {
                                                                {
                                                                    TextBlock eTextTag = new TextBlock();
                                                                    eTextTag.Text = $" (by {iItem.reportedBy.ToUpper()})";
                                                                    eTextTag.Foreground = new SolidColorBrush(Colors.DodgerBlue);
                                                                    eTextTag.Margin = new Thickness(1, 0, 0, 0);
                                                                    eTextBlock.Children.Add(eTextTag);
                                                                }
                                                            }
                                                            eTextBlock.Margin = new Thickness(20, 0, 0, 5);
                                                            entryBlock.Children.Add(eTextBlock);

                                                            eTextBlock.Margin = new Thickness(19, 0, 0, 5);

                                                            itemsAdded = true;
                                                        }

                                                        {
                                                            StackPanel eTextBlock = new StackPanel();
                                                            eTextBlock.Orientation = Orientation.Horizontal;

                                                            {
                                                                Border eTextTagBorder = new Border();
                                                                eTextTagBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                                                                eTextTagBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                                                                eTextTagBorder.MinWidth = 50;

                                                                if (iItem.issueLink != null && iItem.issueLink.Length > 0)
                                                                {
                                                                    HyperlinkButton eTextTag = new HyperlinkButton();
                                                                    eTextTag.Content = "FIX";
                                                                    eTextTag.NavigateUri = new Uri(iItem.issueLink);
                                                                    eTextTag.Padding = new Thickness(1);
                                                                    eTextTag.HorizontalAlignment = HorizontalAlignment.Center;
                                                                    eTextTagBorder.Child = eTextTag;
                                                                    eTextBlock.Children.Add(eTextTagBorder);
                                                                }
                                                                else
                                                                {
                                                                    TextBlock eTextTag = new TextBlock();
                                                                    eTextTag.Text = "FIX";
                                                                    eTextTag.Padding = new Thickness(1);
                                                                    eTextTag.HorizontalAlignment = HorizontalAlignment.Center;
                                                                    eTextTagBorder.Child = eTextTag;
                                                                    eTextBlock.Children.Add(eTextTagBorder);
                                                                }
                                                            }
                                                            {
                                                                TextBlock eTextTag = new TextBlock();
                                                                eTextTag.Text = iItem.issueSolution;
                                                                eTextTag.Padding = new Thickness(1);
                                                                eTextTag.Margin = new Thickness(3, 0, 0, 0);
                                                                eTextBlock.Children.Add(eTextTag);
                                                            }


                                                            entryBlock.Children.Add(eTextBlock);

                                                            eTextBlock.Margin = new Thickness(19, 0, 0, 5);
                                                            Border spr = new Border();
                                                            spr.BorderBrush = new SolidColorBrush(Colors.Transparent);
                                                            spr.BorderThickness = new Thickness(0, 1, 0, 0);
                                                            spr.Margin = new Thickness(20, 0, 0, 5);
                                                            spr.MaxWidth = 200;
                                                            entryBlock.Children.Add(spr);

                                                            itemsAdded = true;
                                                        }
                                                    }

                                                }
                                                catch (Exception ex)
                                                {

                                                }
                                            }
                                            if (itemsAdded)
                                            {
                                                entryBlock.Margin = new Thickness(0, 0, 0, 15);

                                                var CoreIssueAction = GenerateMenuItem("Issues & Fixes", "recent", "recent", "recent");
                                                CoreIssueAction.CustomControl = entryBlock;
                                                CoreIssueAction.Collapsed = true;
                                                CoreQuickMenu.Add(CoreIssueAction);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }

                            try
                            {
                                screenshotsGetterCancellation.Cancel();
                            }
                            catch (Exception ex)
                            {

                            }
                            try
                            {
                                screenshotsGetterCancellation = new CancellationTokenSource();
                            }
                            catch (Exception ex)
                            {

                            }
                            if (PlatformService.ExtraDelay)
                            {
                                await Task.Delay(PlatformService.isMobile ? 500 : 500);
                            }
                            if (PlatformService.ShowScreeshots)
                            {
                                TaskCompletionSource<bool> screenshotsTask = new TaskCompletionSource<bool>();
                                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                {
                                    try
                                    {
                                        //Check for screenshots
                                        var picturesFolder = KnownFolders.PicturesLibrary;
                                        var retrixFolder = (StorageFolder)await picturesFolder.TryGetItemAsync("RetriXGold");
                                        if (retrixFolder != null)
                                        {
                                            if (screenshotsGetterCancellation.IsCancellationRequested)
                                            {
                                                screenshotsTask.SetResult(true);
                                                return;
                                            }
                                            var targetFolder = $"{VM.SelectedSystem.Core.OriginalSystemName} ({VM.SelectedSystem.Core.Name})";
                                            var saveFolder = (StorageFolder)await retrixFolder.TryGetItemAsync(targetFolder);
                                            if (saveFolder != null)
                                            {
                                                if (screenshotsGetterCancellation.IsCancellationRequested)
                                                {
                                                    screenshotsTask.SetResult(true);
                                                    return;
                                                }
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
                                                var files = saveFolder.CreateFileQueryWithOptions(queryOptions);
                                                PreparingCoreLoading(true, "Checking Screenshots..");
                                                var FilesList = await files.GetFilesAsync();
                                                if (screenshotsGetterCancellation.IsCancellationRequested)
                                                {
                                                    screenshotsTask.SetResult(true);
                                                    return;
                                                }
                                                var indexer = 1;
                                                if (FilesList != null && FilesList.Count > 0)
                                                {
                                                    List<StorageFile> sortedFiles = null;
                                                    try
                                                    {
                                                        sortedFiles = FilesList.OrderByDescending(a => (a.GetBasicPropertiesAsync().AsTask().Result).DateModified).ToList();
                                                    }
                                                    catch (Exception ee)
                                                    {

                                                    }
                                                    if (screenshotsGetterCancellation.IsCancellationRequested)
                                                    {
                                                        screenshotsTask.SetResult(true);
                                                        return;
                                                    }
                                                    var shotsCount = (sortedFiles != null ? sortedFiles : FilesList).Count;
                                                    foreach (var fItem in (sortedFiles != null ? sortedFiles : FilesList))
                                                    {
                                                        if (screenshotsGetterCancellation.IsCancellationRequested)
                                                        {
                                                            screenshotsTask.SetResult(true);
                                                            return;
                                                        }
                                                        try
                                                        {
                                                            PreparingCoreLoading(true, $"Screenshots ({indexer} of {shotsCount})..");

                                                            var date = (await fItem.GetBasicPropertiesAsync()).DateModified;
                                                            BitmapImage bitmapImage = new BitmapImage();
                                                            IRandomAccessStream stream = await fItem.OpenAsync(FileAccessMode.Read);
                                                            await bitmapImage.SetSourceAsync(stream);
                                                            var ScreenShotAction = GenerateMenuItem($"Screenshots", "screenshot", $"Screenshot {indexer}", "rems");
                                                            ScreenShotAction.IsImage = true;
                                                            try
                                                            {
                                                                ScreenShotAction.Tag = date.ToString().Substring(0, date.ToString().IndexOf("+")).Trim();
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                ScreenShotAction.Tag = date.ToString();
                                                            }
                                                            ScreenShotAction.bitmapImage = bitmapImage;
                                                            ScreenShotAction.attachedFile = fItem;
                                                            ScreenShotAction.Collapsed = true;
                                                            CoreQuickMenu.Add(ScreenShotAction);
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                        indexer++;
                                                    }
                                                    PreparingCoreLoading(true, "Preparing core..");
                                                }
                                            }
                                        }
                                        if (PlatformService.ExtraDelay)
                                        {
                                            await Task.Delay(PlatformService.isMobile ? 350 : 200);
                                        }
                                        screenshotsTask.SetResult(true);
                                    }
                                    catch (Exception ex)
                                    {
                                        screenshotsTask.SetResult(true);
                                    }
                                }).AsTask(screenshotsGetterCancellation.Token);
                                await screenshotsTask.Task;
                            }
                            //More
                            /*
                            {
                                if (!VM.SelectedSystem.AnyCore || VM.SelectedSystem.ImportedCore)
                                {
                                    var OnlineUpdateAction = GenerateMenuItem("More", "update-online", "Online Update", "download");
                                    OnlineUpdateAction.TagVisibilityGray = Visibility.Visible;
                                    OnlineUpdateAction.TagGray = VM.SelectedSystem.Version;
                                    OnlineUpdateAction.Collapsed = true;
                                    CoreQuickMenu.Add(OnlineUpdateAction);
                                }

                                if (!VM.SelectedSystem.AnyCore || VM.SelectedSystem.ImportedCore)
                                {
                                    var OfflineUpdateAction = GenerateMenuItem("More", "update-offline", "Offline Update", "rems");
                                    OfflineUpdateAction.Collapsed = true;
                                    OfflineUpdateAction.TagVisibilityGray = Visibility.Visible;
                                    OfflineUpdateAction.TagGray = VM.SelectedSystem.Version;
                                    CoreQuickMenu.Add(OfflineUpdateAction);
                                }
                            }
                            if (!VM.SelectedSystem.ImportedCore)
                            {
                                if (await CheckUpdateState())
                                {
                                    var RemoveUpdatesAction = GenerateMenuItem("More", "update-remove", "Remove Updates", "rupdates");
                                    RemoveUpdatesAction.Collapsed = true;
                                    CoreQuickMenu.Add(RemoveUpdatesAction);
                                }
                            }

                            if (VM.SelectedSystem.AnyCore)
                            {
                                if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad && !VM.SelectedSystem.ImportedCore)
                                {
                                    var BIOSSamplesAction = GenerateMenuItem("More", "bios-sample", "BIOS Template", "web-console", "", "ANYCORE");
                                    BIOSSamplesAction.Collapsed = true;
                                    CoreQuickMenu.Add(BIOSSamplesAction);
                                }
                                var DeleteCoreAction = GenerateMenuItem("More", "delete", "Delete Core", "tmp", "", "ANYCORE");
                                CoreQuickMenu.Add(DeleteCoreAction);
                            }

                            if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                var testCustomSavesFolder = await PlatformService.GetCustomFolder(VM.SelectedSystem.Core.Name, "saves");
                                var driveSaves = "";
                                var tagSaves = "";
                                if (testCustomSavesFolder != null)
                                {
                                    try
                                    {
                                        driveSaves = System.IO.Path.GetPathRoot(testCustomSavesFolder.Path);
                                        tagSaves = "CUSTOM";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                var SavesAction = GenerateMenuItem("More", "saves", "Saves", "folder-deps", driveSaves, tagSaves);
                                SavesAction.Collapsed = true;
                                CoreQuickMenu.Add(SavesAction);

                                var testCustomSystemFolder = await PlatformService.GetCustomFolder(VM.SelectedSystem.Core.Name, "system");
                                var driveSystem = "";
                                var tagSystem = "";
                                if (testCustomSystemFolder != null)
                                {
                                    try
                                    {
                                        driveSystem = System.IO.Path.GetPathRoot(testCustomSystemFolder.Path);
                                        tagSystem = "CUSTOM";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                var SystemAction = GenerateMenuItem("More", "system", "System", "folder-wtools", driveSystem, tagSystem);
                                SystemAction.Collapsed = true;
                                CoreQuickMenu.Add(SystemAction);
                            }


                            var pinnedState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{VM.SelectedSystem.Core.Name}-{VM.SelectedSystem.Name}-Pinned", false);
                            var PinAction = GenerateMenuItem("More", "pincore", "Pinned", "fav", "", pinnedState ? "ON" : "OFF");
                            if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                CoreQuickMenu.Add(PinAction);

                                var CoreBackupAction = GenerateMenuItem("More", "custom-backup", "Backup", "repos2");
                                CoreQuickMenu.Add(CoreBackupAction);

                                var CoreRestoreAction = GenerateMenuItem("More", "custom-restore", "Restore", "uupdate");
                                CoreQuickMenu.Add(CoreRestoreAction);
                            }
                            var PrivacyAction = GenerateMenuItem("More", "privacy", "Privacy", "web-iads");
                            CoreQuickMenu.Add(PrivacyAction);
                            */

                            if (PlatformService.ExtraDelay)
                            {
                                await Task.Delay(PlatformService.isMobile ? 500 : 500);
                            }
                            if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                            {
                                //Statistics
                                if (VM.totalPlayedTime > 0)
                                {
                                    var playsCount = VM.totalDummyOpens;
                                    foreach (var rItem in VM.GamesRecentsList)
                                    {
                                        playsCount += rItem.OpenCounts;
                                    }

                                    //Update the core item in main page 
                                    if (returningFromSubPage)
                                    {
                                        foreach (var item in PlatformService.Consoles)
                                        {
                                            if (item.Core != null && item.Core.Name.Equals(VM.SelectedSystem.Core.Name))
                                            {
                                                item.OpenTimes = (int)playsCount;
                                                break;
                                            }
                                        }
                                        foreach(var cItem in CoresQuickMenu)
                                        {
                                            if (cItem.Action.Equals("core"))
                                            {
                                                if (cItem.Core.Core.Name.Equals(VM.SelectedSystem.Core.Name))
                                                {
                                                    cItem.Description = playsCount.ToString();
                                                }
                                            }
                                        }
                                    }


                                    var Statistics1 = GenerateMenuItem("Statistics", "statistics", "Play Count".ToUpper(), "xbox", "", $"{playsCount}", null, "StatisticsGridMenuItemTemplate");
                                    var formatedTime = GameSystemRecentModel.FormatTotalPlayedTimePrettyPrint(VM.totalPlayedTime);
                                    var Statistics2 = GenerateMenuItem("Statistics", "statistics", "Total Time".ToUpper(), "history", formatedTime[1], formatedTime[0], null, "StatisticsGridMenuItemTemplate");


                                    CoreQuickMenu.Add(Statistics1);
                                    CoreQuickMenu.Add(Statistics2);

                                    var totalDummTime = VM.totalDummyTime;
                                    if (totalDummTime > 0 && totalDummTime != VM.totalPlayedTime)
                                    {
                                        var formatedDummyTime = GameSystemRecentModel.FormatTotalPlayedTimePrettyPrint(totalDummTime);
                                        var Statistics3 = GenerateMenuItem("Statistics", "statistics", "Start Core".ToUpper(), "media-play", formatedDummyTime[1], formatedDummyTime[0], null, "StatisticsGridMenuItemTemplate");
                                        CoreQuickMenu.Add(Statistics3);
                                    }
                                }
                            }
                            if (encoderInProgress)
                            {
                                PreparingCoreLoading(true, "Transcoding Icons..");
                            }
                            while (encoderInProgress)
                            {
                                await Task.Delay(100);
                            }
                            PreparingCoreLoading(true, "Preparing core..");
                            GenerateMenu(null, VM.SelectedSystem.Core.Name, CoreQuickMenu, CoreQuickAccessContainer);
                            //ShowLoader(false);

                            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                            {
                                if (PlatformService.ExtraDelay)
                                {
                                    await Task.Delay(PlatformService.isMobile ? 500 : 500);
                                }
                                CoreQuickAccessContainer.UpdateLayout();
                                CoreQuickAccessContainer.ChangeView(0, PlatformService.pivotMainPosition, 1);
                                if (CorePagePivot.SelectedItem == PivotCoreMain)
                                {
                                    await Task.Delay(100);
                                    if (item2 != null)
                                    {
                                        item2.Focus(FocusState.Programmatic);
                                    }
                                }
                            });
                        }
                        catch (Exception ex)
                        {

                        }
                        if (returningFromSubPage)
                        {
                            CorePagePivot.SelectedIndex = PlatformService.pivotPosition;
                        }
                        returningFromSubPage = false;
                        await VM.UpdateCoversByFolder();

                        PreparingCoreLoading(false);
                        CorePagePivot.UpdateLayout();
                    });


                    if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                    {
                        VM.GetFileDependencyForSelectedCore();
                        _ = ShowSelectedSystemOptions(RequiresOptionsReload);
                        if (VM.GamesMainList != null && VM.GamesMainList.Count > 0)
                        {
                            PivotCoreGames.Header = $"Games ({VM.GamesMainList.Count})";
                            VM.ReloadGamesVisibleState(true);
                            buildParentFilters();
                        }
                        else
                        {
                            PivotCoreGames.Header = $"Games";
                            VM.ReloadGamesVisibleState(false);
                        }
                    }
                    else
                    {
                        PivotCoreOptions.Header = $"Options";
                        PivotCoreGames.Header = $"Games";
                        NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        OptionsReset.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        OptionsSave.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        coreOptionsGroupped.Clear();
                        VM.GetFileDependencyForSelectedCore(true);
                        VM.ReloadGamesVisibleState(false);
                    }
                    VM.SetCoresQuickAccessContainerState(false);
                    PrepareCoreUsageBySelectedSystem(DateTime.Today);
                    CorePageInProgress = false;
                    checkLogsState();
                    if (CorePagePivot.SelectedItem == PivotCoreGames)
                    {
                        MemoryValueContainer.Visibility = Visibility.Visible;
                        MemoryValueContainer.Margin = new Thickness(0, 60, 20, 5);
                    }

                    try
                    {
                        if (returningFromSubPage)
                        {
                            _ = VM.SelectedSystem.XCore.CacheCoreData();
                            VFSSupport = VM.SelectedSystem.Core.VFSSupport;
                            ZipSupport = VM.SelectedSystem.Core.NativeArchiveSupport;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                };

                if (VM.SelectedSystem != null && VM.SelectedSystem.Core.RestartRequired)
                {
                    VM.SetCoresQuickAccessContainerState(true);
                    PreparingCoreLoading(false);
                    returningFromSubPage = false;
                    CorePageInProgress = false;
                }
                else
                {
                    await Task.Run(async () =>
                    {
                        if (VM.SelectedSystem == null || !returningFromSubPage)
                        {
                            while ((!VM.SystemSelected && !VM.SystemSelectDone))
                            {
                                await Task.Delay(500);
                            }
                        }
                        if (PlatformService.ExtraDelay)
                        {
                            await Task.Delay(PlatformService.isMobile ? 500 : 500);
                        }
                        VM.GetConsoleInfo(PrepareCorePanel, false);
                    });
                    if (PlatformService.OpenGameFile != null)
                    {
                        if (SugggestedCore)
                        {
                            if (PlatformService.ExtraDelay)
                            {
                                await Task.Delay(PlatformService.isMobile ? 500 : 500);
                            }
                            GamePlayerView.isUpscaleActive = false;
                            await VM.StartSingleGame(PlatformService.OpenGameFile);
                            PlatformService.OpenGameFile = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VM.SetCoresQuickAccessContainerState(true);
                PreparingCoreLoading(false);
                returningFromSubPage = false;
                CorePageInProgress = false;
                if (PlatformService.AppStartedByRetroPass)
                {
                    try
                    {
                        Windows.System.Launcher.LaunchUriAsync(new Uri(PlatformService.RetroPassLaunchOnExit));
                    }
                    catch (Exception exb)
                    {

                    }
                    PlatformService.AppStartedByRetroPass = false;
                }
            }
            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }
        }

        private bool MonitorLoadingStart(string systemName)
        {
            return Plugin.Settings.CrossSettings.Current.GetValueOrDefault(systemName, false);
        }
        private void MonitorLoadingEnd(string systemName, bool state)
        {
            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(systemName, state);
        }

        double currentProgress = 0;
        private void AddNewConsole(string CoreName, string ConsoleName)
        {
            ConsolesSequence.Add(new RequestedCore(CoreName, ConsoleName));
        }
        public static string ResolveCoreName(string coreName)
        {
            coreName = coreName.ToLower();
            var resolvedName = coreName;
            try
            {
                Dictionary<string, string> coreData = new Dictionary<string, string>()
                {
                    { "finalburn_neo" , "fbneo" },
                    { "meteor_gba" , "meteor" },
                    { "nestopia" , "nestopia_ue" },
                    { "game_watch" , "gw" },
                    { "beetle_wonderswan" , "beetle_cygne" },
                    { "jump_n_bump" , "jumpnbump" },
                    { "vice_x64", "vice" },
                    { "cap32", "caprice32" },
                    { "game_music_emulator", "game_music_emu" },
                    { "genesis_plus_gx_wide", "https://github.com/libretro/Genesis-Plus-GX-Wide" },
                    { "lowres_nx", "https://lowresnx.inutilis.com/" },
                    { "mame_2000", "https://github.com/libretro/mame2000-libretro" },
                    { "mame_2003_plus", "mame2003_plus" },
                    { "mini_vmac", "minivmac" },
                    { "oberon", "https://github.com/libretro/oberon-risc-emu" },
                    { "potator", "https://github.com/libretro/potator" },
                    { "stella_2014", "stella" },
                    { "tic_80", "tic80" },
                    { "neocd", "https://github.com/libretro/neocd_libretro" },
                };
                if (!coreData.TryGetValue(coreName, out resolvedName))
                {
                    resolvedName = coreName;
                }
            }
            catch (Exception ex)
            {

            }
            return resolvedName;

        }
        public static string ResolveCoreName2(string coreName)
        {
            coreName = coreName.ToLower();
            var resolvedName = coreName;
            try
            {
                Dictionary<string, string> coreData = new Dictionary<string, string>()
                {
                    { "finalburn_neo" , "fbneo" },
                    { "meteor_gba" , "meteor" },
                    { "nestopia" , "nestopia_ue" },
                    { "game_watch" , "gw" },
                    { "beetle_wonderswan" , "beetle_cygne" },
                    { "jump_n_bump" , "jumpnbump" },
                    { "vice_x64", "vice" },
                    { "cap32", "caprice32" },
                    { "game_music_emulator", "game_music_emu" },
                    { "genesis_plus_gx_wide", "genesis_plus_gx" },
                    { "mame_2003_plus", "mame2003_plus" },
                    { "mini_vmac", "minivmac" },
                    { "stella_2014", "stella" },
                    { "tic_80", "tic80" },
                };
                if (!coreData.TryGetValue(coreName, out resolvedName))
                {
                    resolvedName = coreName;
                }
            }
            catch (Exception ex)
            {

            }
            return resolvedName;

        }
        private void AddConsoles()
        {
            ConsolesSequence.Clear();

            if (!activeState)
            {
                return;
            }

            //I added core name as extra id for all systems
            //to avoid any conflicts bewtween multiple core for the same system
            //only for old systems I will use the same name, not important to spend time to look for them
            //but you have to assign the exact core name with any new system you want to add
            //Example: AddNewConsole('core','console');

            //Arcade
            AddNewConsole("FinalBurn Neo", "Arcade");
            AddNewConsole("FBAlpha", "Arcade");
            AddNewConsole("MAME2000", "MAME"); //<-NOT READY (Need VFS implementation)
            AddNewConsole("MAME2003", "MAME"); //<-NOT READY (Need VFS implementation)

            //Atari
            AddNewConsole("Handy", "Lynx");
            AddNewConsole("Lynx", "Lynx");
            AddNewConsole("Stella", "Atari 2600");
            AddNewConsole("Stella 2014", "Atari 2600");
            AddNewConsole("Atari7800", "Atari 7800");
            AddNewConsole("Jaguar", "Jaguar");
            AddNewConsole("Atari800", "Atari 5200");


            //Nintendo
            AddNewConsole("Snes9X", "Super Nintendo");
            AddNewConsole("FCEUMM", "NES");
            AddNewConsole("VBAM", "Game Boy");
            AddNewConsole("VBAMColor", "Game Boy Color");
            AddNewConsole("VBAMAdvance", "Game Boy Advance");
            AddNewConsole("Snes9X 2005", "Super Nintendo");
            AddNewConsole("Nestopia", "NES");
            AddNewConsole("QuickNES", "NES");
            AddNewConsole("Gambatte", "GameBoy+");
            AddNewConsole("Gearboy", "GameBoy+");
            AddNewConsole("TGB Dual", "GameBoy+");
            AddNewConsole("Nintendo DS", "Nintendo DS");
            AddNewConsole("VirtualBoy", "Virtual Boy");
            AddNewConsole("Pokemon Mini", "Pokemon Mini");
            AddNewConsole("GW", "Game & Watch");
            AddNewConsole("ParallelN64", "Nintendo 64");
#if TARGET_X64
            //AddNewConsole("DolphinGC", "GameCube");
            //AddNewConsole("DolphinWii", "Nintendo Wii");
#endif

            //SEGA
            AddNewConsole("SG-1000", "SG-1000");
            AddNewConsole("Master System", "Master System");
            AddNewConsole("Game Gear", "Game Gear");
            AddNewConsole("Mega Drive", "Mega Drive");
            AddNewConsole("Mega CD", "Mega CD");
            AddNewConsole("GenesisPlusGXWide", "SEGA Wide");
            AddNewConsole("BeetleSaturn", "Saturn");
            AddNewConsole("BeetleSaturnYabause", "Saturn");


            //Game Engine
            AddNewConsole("NXEngine", "Cave Story");
            AddNewConsole("Cannonball", "OutRun");
            AddNewConsole("2048", "2048");
            AddNewConsole("PrBoom", "Doom");
            AddNewConsole("XRick", "Rick Dangerous");
            AddNewConsole("TyrQuake", "Quake");
            AddNewConsole("JNB", "Jump n' Bump");
            AddNewConsole("REminiscence", "Flashback");


            //Commodore (Crash issue, will enable them later)
            /*AddNewConsole("vicex64", "C64");
            AddNewConsole("vicex64sc", "C64 SuperCPU");
            AddNewConsole("vicexscpu64", "C64 SuperCPU");
            AddNewConsole("vicex128", "C128");
            AddNewConsole("vicexcbm2", "CBM-II");
            AddNewConsole("vicexcbm5x0", "CBM-II");
            AddNewConsole("vicexpet", "CPET");
            AddNewConsole("vicexplus4", "Plus4");
            AddNewConsole("vicexvic", "VIC-20");*/


            //Computers
            AddNewConsole("Caprice32", "Amstrad CPC");
            AddNewConsole("DOSBox-pure", "DOSBox Pure");
            AddNewConsole("BlueMSX", "MSX Computer");
            AddNewConsole("fMSX", "MSX Computer");
            AddNewConsole("EightyOne", "Sinclair ZX81");
            AddNewConsole("Quasi88", "PC 8000-8800");
            AddNewConsole("MiniVMac", "Mini vMac");

            //Game Video (70~80s)
            AddNewConsole("FreeChaf", "Fairchild ChannelF");
            AddNewConsole("FreeIntv", "Intellivision");
            AddNewConsole("GearColeco", "ColecoVision");
            AddNewConsole("O2EM", "Magnavox Odyssey 2");
            AddNewConsole("Vectrex", "Vectrex");

            //SNK
            AddNewConsole("BeetleNGP", "Neo Geo Pocket");
            AddNewConsole("RACE", "Neo Geo Pocket");
            AddNewConsole("NeoCD", "NeoGeo CD");


            //NEC
            AddNewConsole("PC Engine", "PC Engine");
            AddNewConsole("PC Engine CD", "PC Engine CD");
            AddNewConsole("BeetlePCEFast", "PCEngine Fast");
            AddNewConsole("BeetlePCFX", "PC-FX");

            //Sony
            AddNewConsole("BeetlePSX", "PlayStation");
#if TARGET_X64
            //AddNewConsole("ReARMedPSX", "PlayStation");
            //AddNewConsole("DuckStation", "PlayStation");
            //AddNewConsole("PCSX2", "PlayStation 2");
#endif

            //Handheld Mixed
            AddNewConsole("WonderSwan", "WonderSwan");
            AddNewConsole("WonderSwan Color", "WonderSwan Color");
            AddNewConsole("Watara Supervision", "Watara Supervision");

            //Others
            AddNewConsole("ScummVM", "ScummVM");
            AddNewConsole("TIC-80", "TIC-80");
            AddNewConsole("GME", "Game Music");
            AddNewConsole("LowresNX", "LowresNX");
            AddNewConsole("Oberon", "Oberon RISC");
            AddNewConsole("The3DO", "3DO");
            AddNewConsole("PocketCDG", "PocketCDG");
        }

        Dictionary<string, string> CoresDLL = new Dictionary<string, string>()
        {
            {"GearColeco" , "gearcoleco_libretro" },
            {"vicex64" , "vice_x64_libretro" },
            {"vicex64sc" , "vice_x64sc_libretro" },
            {"vicexscpu64" , "vice_xscpu64_libretro" },
            {"vicex128" , "vice_x128_libretro" },
            {"vicexcbm2" , "vice_xcbm2_libretro" },
            {"vicexcbm5x0" , "vice_xcbm5x0_libretro" },
            {"vicexpet" , "vice_xpet_libretro" },
            {"vicexplus4" , "vice_xplus4_libretro" },
            {"vicexvic" , "vice_xvic_libretro" },
            {"NeoCD" , "neocd_libretro" },
            {"Oberon" , "oberon_libretro" },
            {"NXEngine" , "nxengine_libretro" },
            {"MiniVMac" , "minivmac_libretro" },
            {"MAME2000" , "mame2000_libretro" },
            {"MAME2003" , "mame2003_plus_libretro" },
            {"EightyOne" , "81_libretro" },
            {"FreeIntv" , "freeintv_libretro" },
            {"Gearboy" , "gearboy_libretro" },
            {"2048" , "2048_libretro" },
            {"Caprice32" , "cap32_libretro" },
            {"Atari7800" , "prosystem_libretro" },
            {"Atari800" , "atari800_libretro" },
            {"AtariJaguar" , "virtualjaguar_libretro" },
            {"Lynx" , "mednafen_lynx_libretro" },
            {"BeetleNGP" , "mednafen_ngp_libretro" },
            {"BeetlePCE" , "mednafen_pce_libretro" },
            {"BeetlePCEFast" , "mednafen_pce_fast_libretro" },
            {"BeetlePCFX" , "mednafen_pcfx_libretro" },
            {"BeetlePSX" , "mednafen_psx_libretro" },
            {"ReARMedPSX" , "pcsx_rearmed_libretro" },
            {"DuckStation" , "duckstation_libretro" },
            {"BeetlePSXHW" , "mednafen_psx_hw_libretro" },
            {"PCSX2" , "pcsx2_libretro" },
            {"DolphinGC" , "dolphin_libretro" },
            {"DolphinWii" , "dolphin_libretro" },
            {"BeetleWSwan" , "mednafen_wswan_libretro" },
            {"BlueMSX" , "bluemsx_libretro" },
            {"Cannonball" , "cannonball_libretro" },
            {"Desmumme" , "desmume_libretro" },
            {"DOSBox-pure" , "dosbox_pure_libretro" },
            {"FBAlpha" , "fbalpha_libretro" },
            {"FinalBurn Neo" , "fbneo_libretro" },
            {"FBPoly" , "fbneo_libretro" },
            {"FCEUMM" , "fceumm_libretro" },
            {"FreeChaf" , "freechaf_libretro" },
            {"Gambatte" , "gambatte_libretro" },
            {"GenesisPlusGX" , "genesis_plus_gx_libretro" },
            {"GenesisPlusGXCD" , "genesis_plus_gx_libretro" },
            {"GenesisPlusGXGameGear" , "genesis_plus_gx_libretro" },
            {"GenesisPlusGXMasterSystem" , "genesis_plus_gx_libretro" },
            {"GenesisPlusGXMegaDrive" , "genesis_plus_gx_libretro" },
            {"GenesisPlusGXWide" , "genesis_plus_gx_wide_libretro" },
            {"GME" , "gme_libretro" },
            {"GW" , "gw_libretro" },
            {"Handy" , "handy_libretro" },
            {"JNB" , "jumpnbump_libretro" },
            {"LowResNX" , "lowresnx_libretro" },
            {"MelonDS" , "melonds_libretro" },
            {"Meteor" , "meteor_libretro" },
            {"fMSX" , "fmsx_libretro" },
            {"Nestopia" , "nestopia_libretro" },
            {"O2EM" , "o2em_libretro" },
            {"ParallelN64" , "parallel_n64_libretro" },
            {"PicoDrive" , "picodrive_libretro" },
            {"PocketCDG" , "pocketcdg_libretro" },
            {"PocketMini" , "pokemini_libretro" },
            {"Potator" , "potator_libretro" },
            {"PPSSPP" , "ppsspp_libretro" },
            {"PrBoom" , "prboom_libretro" },
            {"Quasi88" , "quasi88_libretro" },
            {"QuickNES" , "quicknes_libretro" },
            {"RACE" , "race_libretro" },
            {"REminiscence" , "reminiscence_libretro" },
            {"BeetleSaturn" , "mednafen_saturn_libretro" },
            {"BeetleSaturnYabause" , "yabause_libretro" },
            {"ScummVM" , "scummvm_libretro" },
            {"Snes9X" , "snes9x_libretro" },
            {"Snes9X 2005" , "snes9x2005_libretro" },
            {"Stella" , "stella_libretro" },
            {"Stella 2014" , "stella2014_libretro" },
            {"TGB Dual" , "tgbdual_libretro" },
            {"The3DO" , "opera_libretro" },
            {"TIC80" , "tic80_libretro" },
            {"TyrQuake" , "tyrquake_libretro" },
            {"VBAM" , "vbam_libretro" },
            {"VBAMAdvance" , "vbam_libretro" },
            {"VBAMColor" , "vbam_libretro" },
            {"VBAMMicro" , "vbam_libretro" },
            {"Vectrex" , "vecx_libretro" },
            {"VirtualBoy" , "mednafen_vb_libretro" },
            {"XRick" , "xrick_libretro" },
            {"Flycast" , "flycast_libretro" },
        };
        private string FindAnyCoreInOriginalCores(string anycoreLocation)
        {
            var dllName = System.IO.Path.GetFileNameWithoutExtension(anycoreLocation);
            foreach (var dItem in CoresDLL)
            {
                if (dItem.Value.ToLower().Equals(dllName.ToLower()))
                {
                    return dItem.Key;
                }
            }

            return "";
        }

        private async Task<GameSystemViewModel> GameConsole(string coreName, string ConsoleName, bool skippedCore = false, bool anyCore = false, string directDLL = "")
        {
            GameSystemViewModel CreatedConsole = null;

            //Regarding to (..,..,anyCore, anyCore)
            //When this function called from AnyCore function
            //this mean user is importing core that already has configuration
            //so it will be considerd as ImportedCore

            switch (coreName)
            {
                case "ColecoVision":
                case "GearColeco":
                    CreatedConsole = await GameSystemViewModel.MakeGearColeco(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["GearColeco"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "C64":
                case "vicex64":
                    CreatedConsole = await GameSystemViewModel.MakeC64(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["vicex64"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "C64 SuperCPU":
                case "vicex64sc":
                    CreatedConsole = await GameSystemViewModel.MakeC64SuperCPU(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["vicex64sc"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "vicexscpu64":
                    CreatedConsole = await GameSystemViewModel.MakeC64SuperCPU2(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["vicexscpu64"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "C128":
                case "vicex128":
                    CreatedConsole = await GameSystemViewModel.MakeC128(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["vicex128"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "CBM-II":
                case "vicexcbm2":
                    CreatedConsole = await GameSystemViewModel.MakeCBMII(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["vicexcbm2"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "vicexcbm5x0":
                    CreatedConsole = await GameSystemViewModel.MakeCBMII2(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["vicexcbm5x0"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "CPET":
                case "vicexpet":
                    CreatedConsole = await GameSystemViewModel.MakeCPET(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["vicexpet"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Plus4":
                case "vicexplus4":
                    CreatedConsole = await GameSystemViewModel.MakePlus4(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["vicexplus4"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "VIC-20":
                case "vicexvic":
                    CreatedConsole = await GameSystemViewModel.MakeVIC20(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["vicexvic"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Oberon":
                    CreatedConsole = await GameSystemViewModel.MakeOberon(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Oberon"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "2048":
                    CreatedConsole = await GameSystemViewModel.Make2048(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["2048"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "NXEngine":
                case "Cave Story":
                    CreatedConsole = await GameSystemViewModel.MakeNXEngine(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["NXEngine"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Caprice32":
                    CreatedConsole = await GameSystemViewModel.MakeCaprice32(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Caprice32"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "MiniVMac":
                case "Mini vMac":
                case "vMac":
                    CreatedConsole = await GameSystemViewModel.MakeMiniVMac(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["MiniVMac"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "FreeIntv":
                    CreatedConsole = await GameSystemViewModel.MakeFreeIntv(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["FreeIntv"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Flycast":
                    CreatedConsole = await GameSystemViewModel.MakeFlycast(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Flycast"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "EightyOne":
                    CreatedConsole = await GameSystemViewModel.MakeZX81(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["EightyOne"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "NES":
                case "FCEUMM":
                    CreatedConsole = await GameSystemViewModel.MakeNES(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["FCEUMM"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "NES (Nestopia)":
                case "Nestopia":
                    CreatedConsole = await GameSystemViewModel.MakeNESNestopia(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Nestopia"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Super Nintendo":
                case "Snes9X":
                    CreatedConsole = await GameSystemViewModel.MakeSNES(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Snes9X"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Snes9X 2005":
                    CreatedConsole = await GameSystemViewModel.MakeSNES2005(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Snes9X 2005"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Nintendo 64":
                case "ParallelN64":
                    CreatedConsole = await GameSystemViewModel.MakeN64(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["ParallelN64"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Game & Watch":
                case "GW":
                    CreatedConsole = await GameSystemViewModel.MakeGW(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["GW"], skippedCore, true, anyCore, anyCore, coreName));

                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Jump n' Bump":
                case "JNB":
                    CreatedConsole = await GameSystemViewModel.MakeJNB(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["JNB"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "GameBoy":
                case "VBAM":
                    CreatedConsole = await GameSystemViewModel.MakeGB(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["VBAM"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "GameBoy Color":
                case "VBAMColor":
                    CreatedConsole = await GameSystemViewModel.MakeGBC(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["VBAMColor"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Gambatte":
                    CreatedConsole = await GameSystemViewModel.MakeGambatte(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Gambatte"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Gearboy":
                    CreatedConsole = await GameSystemViewModel.MakeGearboy(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Gearboy"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "GameBoy Advance":
                case "VBAMAdvance":
                    CreatedConsole = await GameSystemViewModel.MakeGBA(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["VBAMAdvance"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Game Boy Advance (Meteor)":
                case "Meteor":
                    CreatedConsole = await GameSystemViewModel.MakeGBAMeteor(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Meteor"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Game Boy Micro":
                case "VBAMMicro":
                    CreatedConsole = await GameSystemViewModel.MakeGBM(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["VBAMMicro"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Game Boy Advance SP":
                    CreatedConsole = await GameSystemViewModel.MakeGBASP(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["VBAM"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Nintendo DS":
                case "Desmumme":
                    CreatedConsole = await GameSystemViewModel.MakeDS(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Desmumme"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "MelonDS":
                    CreatedConsole = await GameSystemViewModel.MakeMelonDS(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["MelonDS"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "SG-1000":
                    CreatedConsole = await GameSystemViewModel.MakeSG1000(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["GenesisPlusGX"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Master System":
                case "GenesisPlusGXMasterSystem":
                    CreatedConsole = await GameSystemViewModel.MakeMasterSystem(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["GenesisPlusGXMasterSystem"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Game Gear":
                case "GenesisPlusGXGameGear":
                    CreatedConsole = await GameSystemViewModel.MakeGameGear(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["GenesisPlusGXGameGear"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Mega Drive":
                case "GenesisPlusGXMegaDrive":
                    CreatedConsole = await GameSystemViewModel.MakeMegaDrive(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["GenesisPlusGXMegaDrive"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Mega CD":
                case "GenesisPlusGXCD":
                    CreatedConsole = await GameSystemViewModel.MakeMegaCD(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["GenesisPlusGXCD"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "32X":
                case "PicoDrive":
                    CreatedConsole = await GameSystemViewModel.Make32X(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["PicoDrive"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Saturn":
                case "BeetleSaturn":
                    CreatedConsole = await GameSystemViewModel.MakeSaturn(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetleSaturn"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "ScummVM":
                    CreatedConsole = await GameSystemViewModel.MakeScummVM(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["ScummVM"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Saturn (Yabause)":
                case "BeetleSaturnYabause":
                    CreatedConsole = await GameSystemViewModel.MakeSaturnYabause(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetleSaturnYabause"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "PlayStation":
                case "BeetlePSX":
                    CreatedConsole = await GameSystemViewModel.MakePlayStation(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetlePSX"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "ReARMedPSX":
                    CreatedConsole = await GameSystemViewModel.MakePlayStationReARMed(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["ReARMedPSX"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "DuckStation":
                    CreatedConsole = await GameSystemViewModel.MakePlayStationDuckStation(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["DuckStation"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "BeetlePSXHW":
                    CreatedConsole = await GameSystemViewModel.MakePlayStationHW(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetlePSXHW"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "PCSX2":
                    CreatedConsole = await GameSystemViewModel.MakePlayStation2(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["PCSX2"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "DolphinGC":
                    CreatedConsole = await GameSystemViewModel.MakeGameCube(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["DolphinGC"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "DolphinWii":
                    CreatedConsole = await GameSystemViewModel.MakeWii(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["DolphinWii"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "PlayStation Portable":
                case "PPSSPP":
                    CreatedConsole = await GameSystemViewModel.MakePlayStationPortable(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["PPSSPP"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "BeetlePCEFast":
                    CreatedConsole = await GameSystemViewModel.MakePCEngineFast(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetlePCEFast"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "BeetlePCE":
                case "PC Engine":
                    CreatedConsole = await GameSystemViewModel.MakePCEngine(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetlePCE"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "PC Engine CD":
                    CreatedConsole = await GameSystemViewModel.MakePCEngineCD(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetlePCE"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "PC-FX":
                case "BeetlePCFX":
                    CreatedConsole = await GameSystemViewModel.MakePCFX(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetlePCFX"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "WonderSwan Color":
                case "BeetleWSwan":
                    CreatedConsole = await GameSystemViewModel.MakeWonderSwanColor(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetleWSwan"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "WonderSwan":
                    CreatedConsole = await GameSystemViewModel.MakeWonderSwan(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetleWSwan"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "FBAlpha":
                    CreatedConsole = await GameSystemViewModel.MakeFBAlpha(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["FBAlpha"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Arcade":
                case "FinalBurn Neo":
                    CreatedConsole = await GameSystemViewModel.MakeArcade(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["FinalBurn Neo"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "MAME2000":
                    CreatedConsole = await GameSystemViewModel.MakeMAME2000(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["MAME2000"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "MAME2003":
                    CreatedConsole = await GameSystemViewModel.MakeMAME2003(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["MAME2003"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Neo Geo Pocket Color":
                case "BeetleNGP":
                    CreatedConsole = await GameSystemViewModel.MakeNeoGeoPocket(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BeetleNGP"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Neo Geo":
                    CreatedConsole = await GameSystemViewModel.MakeNeoGeo(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["FBNeo"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "NeoCD":
                case "NeoGeo CD":
                    CreatedConsole = await GameSystemViewModel.MakeNeoGeoCD(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["NeoCD"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "PolyGame Master":
                case "FBPoly":
                    CreatedConsole = await GameSystemViewModel.MakePolyGameMaster(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["FBPoly"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Lynx":
                    CreatedConsole = await GameSystemViewModel.MakeLynx(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Lynx"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "3DO":
                case "The3DO":
                    CreatedConsole = await GameSystemViewModel.Make3DO(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["The3DO"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Vectrex":
                    CreatedConsole = await GameSystemViewModel.MakeVectrex(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Vectrex"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Atari7800":
                    CreatedConsole = await GameSystemViewModel.MakeAtari7800(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Atari7800"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Jaguar":
                case "AtariJaguar":
                    CreatedConsole = await GameSystemViewModel.MakeAtarJaguar(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["AtariJaguar"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "fMSX":
                    CreatedConsole = await GameSystemViewModel.MakeMSX(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["fMSX"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Virtual Boy":
                case "VirtualBoy":
                    CreatedConsole = await GameSystemViewModel.MakeVirtualBoy(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["VirtualBoy"], skippedCore, true, anyCore, anyCore, coreName));
                    break;


                case "MSX Computer":
                case "BlueMSX":
                    CreatedConsole = await GameSystemViewModel.MakeMSXComputer(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["BlueMSX"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Atari 5200":
                case "Atari800":
                    CreatedConsole = await GameSystemViewModel.MakeAtari5200(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Atari800"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Cannonball":
                    CreatedConsole = await GameSystemViewModel.MakeCannonball(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Cannonball"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "DOSBox-pure":
                    CreatedConsole = await GameSystemViewModel.MakeDOSBoxPure(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["DOSBox-pure"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Fairchild ChannelF":
                case "FreeChaf":
                    CreatedConsole = await GameSystemViewModel.MakeFairchildChannelF(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["FreeChaf"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "SEGA Wide":
                case "GenesisPlusGXWide":
                    CreatedConsole = await GameSystemViewModel.MakeSegaWide(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["GenesisPlusGXWide"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Game Music":
                case "GME":
                    CreatedConsole = await GameSystemViewModel.MakeGameMusicEmu(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["GME"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Lynx (Handy)":
                case "Handy":
                    CreatedConsole = await GameSystemViewModel.MakeHandy(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Handy"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "LowresNX":
                case "LowResNX":
                    CreatedConsole = await GameSystemViewModel.MakeLowResNX(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["LowResNX"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Magnavox Odyssey 2":
                case "O2EM":
                    CreatedConsole = await GameSystemViewModel.MakeO2M(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["O2EM"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "PocketCDG":
                    CreatedConsole = await GameSystemViewModel.MakePocketCDG(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["PocketCDG"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Pokemon Mini":
                case "PocketMini":
                    CreatedConsole = await GameSystemViewModel.MakePokeMini(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["PocketMini"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Watara Supervision":
                case "Potator":
                    CreatedConsole = await GameSystemViewModel.MakeWataraSupervision(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Potator"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Doom":
                case "PrBoom":
                    CreatedConsole = await GameSystemViewModel.MakeDoom(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["PrBoom"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "PC 8000-8800":
                case "Quasi88":
                    CreatedConsole = await GameSystemViewModel.MakePC8800(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Quasi88"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "NES (QuickNES)":
                case "QuickNES":
                    CreatedConsole = await GameSystemViewModel.MakeNESQuick(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["QuickNES"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Neo Geo Pocket (RACE)":
                case "RACE":
                    CreatedConsole = await GameSystemViewModel.MakeNEOGEOPockectRace(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["RACE"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Flashback":
                case "REminiscence Flashback":
                case "REminiscence":
                    CreatedConsole = await GameSystemViewModel.MakeReminiscence(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["REminiscence"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Atari 2600":
                case "Stella":
                    CreatedConsole = await GameSystemViewModel.MakeAtari2600Stella(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Stella"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Stella 2014":
                    CreatedConsole = await GameSystemViewModel.MakeAtari2600Stella2014(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["Stella 2014"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "GameBoy (TGB Dual)":
                case "TGB Dual":
                    CreatedConsole = await GameSystemViewModel.MakeGBDual(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["TGB Dual"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "TIC-80":
                case "TIC80":
                    CreatedConsole = await GameSystemViewModel.MakeTIC80(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["TIC80"], skippedCore, true, anyCore, anyCore, coreName));
                    break;

                case "Quake":
                case "TyrQuake":
                    CreatedConsole = await GameSystemViewModel.MakeQuake(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["TyrQuake"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                case "Rick Dangerous":
                case "XRick":
                    CreatedConsole = await GameSystemViewModel.MakeRickDangerous(new RetriXGoldCore(anyCore ? directDLL : CoresDLL["XRick"], skippedCore, true, anyCore, anyCore, coreName));
                    //This added from v3.0
                    //It should be considerd as new core
                    //'IsNewCore' related to recents games records issue
                    CreatedConsole.Core.IsNewCore = true;
                    break;

                default:
                    break;
            }
            if (CreatedConsole != null)
            {
                if (CreatedConsole.XCore.DLLMissing)
                {
                    WriteLog($"Missing file {CreatedConsole.XCore.CoreDLL}..");
                    return null;
                }
                CreatedConsole.ConsoleName = ConsoleName;
                CreatedConsole.SkippedCore = skippedCore;
                CreatedConsole.Core.SkippedCore = skippedCore;

                CreatedConsole.Core.FreeLibretroCore();
            }
            return CreatedConsole;
        }
        private async Task<GameSystemViewModel> GameConsoleAnyCore(string DllFile, bool skippedCore = false)
        {
            GameSystemViewModel CreatedConsole = null;
            //Test If Core Configuration Exists
            var test = FindAnyCoreInOriginalCores(DllFile);
            if (test.Length > 0)
            {
                CreatedConsole = await GameConsole(test, test, skippedCore, true, DllFile);
            }
            else
            {
                var BiosFiles = await GameSystemSelectionViewModel.GetAnyCoreBIOSFiles(DllFile);
                bool CDSupport = GameSystemSelectionViewModel.isSupportCD(DllFile);
                CreatedConsole = await GameSystemViewModel.MakeAnyCore(new RetriXGoldCore(DllFile, skippedCore, true, true, false, "", null, BiosFiles), CDSupport, skippedCore);
                CreatedConsole.SkippedCore = skippedCore;

                CreatedConsole.Core.FreeLibretroCore();
            }
            return CreatedConsole;
        }

        #endregion

        private async void GitHubLink()
        {
            try
            {
                var options = new Windows.System.LauncherOptions();
                options.PreferredApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe";
                options.PreferredApplicationDisplayName = "Microsoft Edge";
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/basharast/RetrixGold"), options);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                    string message = $"For support and source code visit:\ngithub.com/basharast/RetrixGold";
                    await UserDialogs.Instance.AlertAsync(message, "GitHub");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void OpenContact()
        {
            try
            {
                var options = new Windows.System.LauncherOptions();
                options.PreferredApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe";
                options.PreferredApplicationDisplayName = "Microsoft Edge";
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:services@astifan.online"), options);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                    string message = $"Contact me at:\nservices@astifan.online";
                    await UserDialogs.Instance.AlertAsync(message, "Contact");
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void OpenWUT()
        {
            try
            {
                var options = new Windows.System.LauncherOptions();
                options.PreferredApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe";
                options.PreferredApplicationDisplayName = "Microsoft Edge";
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/basharast/wut"), options);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                    string message = $"Checkout W.U.T project:\ngithub.com/basharast/wut";
                    await UserDialogs.Instance.AlertAsync(message, "GitHub");
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void OpenRetroArch()
        {
            try
            {
                var options = new Windows.System.LauncherOptions();
                options.PreferredApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe";
                options.PreferredApplicationDisplayName = "Microsoft Edge";
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/basharast/RetroArch-ARM"), options);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                    string message = $"Checkout RetroArch for ARM:\ngithub.com/basharast/RetroArch-ARM";
                    await UserDialogs.Instance.AlertAsync(message, "GitHub");
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void SupportLink()
        {
            try
            {
                var options = new Windows.System.LauncherOptions();
                options.PreferredApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe";
                options.PreferredApplicationDisplayName = "Microsoft Edge";
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.patreon.com/bastifan"), options);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                    string message = $"If you would like to support me visit:\npatreon.com/bastifan";
                    await UserDialogs.Instance.AlertAsync(message, "Patreon");
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void SupportLinkPaypal()
        {
            try
            {
                var options = new Windows.System.LauncherOptions();
                options.PreferredApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe";
                options.PreferredApplicationDisplayName = "Microsoft Edge";
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(new Uri("http://paypal.me/astifan"), options);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                    string message = $"If you would like to support me visit:\npaypal.me/astifan";
                    await UserDialogs.Instance.AlertAsync(message, "Patreon");
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void ImportFolderHandler(object sender, EventArgs args)
        {
            try
            {
                ImportFolderAction(null, (string)sender);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }
        private async void ImportFolderAction(GameSystemViewModel forceSystem = null, string folderCheck = "")
        {
            try
            {
                GameSystemViewModel targetSystem = forceSystem;
                if (targetSystem == null)
                {
                    targetSystem = VM.SelectedSystem;
                }
                PlatformService.PlayNotificationSoundDirect("button-01");
                PlatformService.PlayNotificationSoundDirect("notice");
                ConfirmConfig confirCoreSettings = new ConfirmConfig();
                confirCoreSettings.SetTitle("Import Folder?");
                if (folderCheck.Length > 0)
                {
                    confirCoreSettings.SetMessage($"Do you want to import '{folderCheck}' folder?");
                }
                else
                {
                    confirCoreSettings.SetMessage($"This option will help you to import full folder to {targetSystem.Name}'s system\nNote: The folder will be imported along with the files not only the files, if you want to import files only use 'Import Files'\nImportant: Don't import games with this option use 'Games' tab to select your games folder\n\nDo you want to start?");
                }
                confirCoreSettings.UseYesNo();
                bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                if (!confirCoreSettingsState)
                {
                    return;
                }
                FolderPicker picker = new FolderPicker();
                picker.ViewMode = PickerViewMode.List;
                picker.SuggestedStartLocation = PickerLocationId.Downloads;
                picker.FileTypeFilter.Add("*");

                var folder = await picker.PickSingleFolderAsync();
                bool FailedToCopy = false;
                if (folder != null)
                {
                    var SystemFolder = (StorageFolder)await VM.SelectedSystem.GetSystemDirectoryAsync();
                    if (SystemFolder != null)
                    {
                        if (!folder.Name.Equals(folderCheck))
                        {
                            PlatformService.PlayNotificationSoundDirect("notice");
                            confirCoreSettings = new ConfirmConfig();
                            confirCoreSettings.SetTitle("Import Folder?");
                            confirCoreSettings.SetMessage($"The selected folder doesn't match with the requested name \nRequired: {folderCheck}\nSelected: {folder.Name}\n\nDo you want to import anyway?");
                            confirCoreSettings.UseYesNo();
                            confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                            if (!confirCoreSettingsState)
                            {
                                return;
                            }
                        }
                        await CopyFileToFolder(folder, SystemFolder);
                    }
                    else
                    {
                        FailedToCopy = true;
                        PlatformService.PlayNotificationSoundDirect("error");
                        LocalNotificationData localNotificationData2 = new LocalNotificationData();
                        localNotificationData2.icon = SegoeMDL2Assets.DisconnectDrive;
                        localNotificationData2.message = "Failed to get system folder!";
                        localNotificationData2.time = 3;
                        PlatformService.NotificationHandlerMain(null, localNotificationData2);
                    }

                    if (!FailedToCopy)
                    {
                        CoreLoadingProgress.Visibility = Visibility.Collapsed;
                        PlatformService.PlayNotificationSoundDirect("success");
                        var localNotificationData = new LocalNotificationData();
                        localNotificationData.icon = SegoeMDL2Assets.ActionCenterNotification;
                        localNotificationData.message = "Folder imported successfully, Enjoy!\nNote: to remove use Reset button in BIOS page";
                        localNotificationData.time = 5;
                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                        VM.GetFileDependencyForSelectedCore();
                    }
                    if (VM != null)
                    {
                        ShowLoader(false);
                        CoreLoadingProgress.Visibility = Visibility.Collapsed;
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
                if (VM != null)
                {
                    ShowLoader(false);
                    CoreLoadingProgress.Visibility = Visibility.Collapsed;
                }
            }
        }
        private async Task CopyFileToFolder(StorageFolder sourceFolder, StorageFolder destinationFolder)
        {
            try
            {
                var targetFolder = await destinationFolder.CreateFolderAsync(sourceFolder.Name, CreationCollisionOption.OpenIfExists);
                var files = await sourceFolder.GetFilesAsync();
                if (files != null && files.Count > 0)
                {
                    if (VM != null)
                    {
                        ShowLoader(true, $"Importing files..\n{sourceFolder.Name}");
                    }
                    var fileProgress = 0;
                    CoreLoadingProgress.Visibility = Visibility.Visible;
                    var copied = 1;
                    foreach (var file in files)
                    {
                        ShowLoader(true, $"Importing content..\n{sourceFolder.Name} ({copied} of {files.Count})\n{file.Name}");
                        await file.CopyAsync(targetFolder, file.Name, NameCollisionOption.ReplaceExisting);
                        copied++;
                        fileProgress++;
                        _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                        {
                            try
                            {
                                fileProgress++;
                                if (fileProgress <= PlatformService.MaxProgressValue)
                                {
                                    CoreLoadingProgress.Value = fileProgress;
                                }
                            }
                            catch (Exception er)
                            {
                            }
                        });
                    }
                    while (currentProgress <= PlatformService.MaxProgressValue)
                    {
                        currentProgress++;
                        CoreLoadingProgress.Value = currentProgress;
                        await Task.Delay(1);
                    }
                }
                var subFolders = await sourceFolder.GetFoldersAsync();
                if (subFolders != null && subFolders.Count > 0)
                {
                    foreach (var subFolder in subFolders)
                    {
                        await CopyFileToFolder(subFolder, targetFolder);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void ImportBIOSDialog()
        {
            try
            {
                ContentDialog contentDialog = new ContentDialog();
                contentDialog.Title = "Import BIOS";
                contentDialog.PrimaryButtonText = "Select";
                contentDialog.SecondaryButtonText = "Close";
                contentDialog.IsPrimaryButtonEnabled = true;
                contentDialog.IsSecondaryButtonEnabled = true;

                ListView listView = new ListView();
                listView.HorizontalAlignment = HorizontalAlignment.Stretch;
                listView.VerticalAlignment = VerticalAlignment.Stretch;
                foreach (var system in PlatformService.Consoles)
                {
                    if (system != null && system.Name != null && system.Name.Length > 0)
                    {
                        ListViewItem listViewItem = new ListViewItem();
                        listViewItem.Content = system.Name;
                        listViewItem.Tag = system;
                        listView.Items.Add(listViewItem);
                    }
                }

                ScrollViewer scrollViewer = new ScrollViewer();
                scrollViewer.Content = listView;
                scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.VerticalAlignment = VerticalAlignment.Stretch;
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                contentDialog.Content = scrollViewer;
                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    if (listView.SelectedItem != null)
                    {
                        var selectedSystem = (GameSystemViewModel)((ListViewItem)listView.SelectedItem).Tag;
                        PureCoreImportBIOS_Click(selectedSystem);
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        LocalNotificationData localNotificationData = new LocalNotificationData();
                        localNotificationData.icon = SegoeMDL2Assets.DisconnectDrive;
                        localNotificationData.message = "Select system first!";
                        localNotificationData.time = 3;
                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private async void PureCoreImportBIOS_Click(GameSystemViewModel forceSystem = null)
        {
            try
            {
                GameSystemViewModel targetSystem = forceSystem;
                if (targetSystem == null)
                {
                    targetSystem = VM.SelectedSystem;
                }
                PlatformService.PlayNotificationSoundDirect("button-01");
                PlatformService.PlayNotificationSoundDirect("notice");
                ConfirmConfig confirCoreSettings = new ConfirmConfig();
                confirCoreSettings.SetTitle("Import Files?");
                confirCoreSettings.SetMessage($"This option will help you to import BIOS, Assets and other files for {targetSystem.Name}\nNote: Multiple selection is possible\nImportant: Don't import games with this option use 'Games' tab to select your games folder\n\nDo you want to start?");
                confirCoreSettings.UseYesNo();
                bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                if (!confirCoreSettingsState)
                {
                    return;
                }
                FileOpenPicker picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.List;
                picker.SuggestedStartLocation = PickerLocationId.Downloads;
                picker.FileTypeFilter.Add("*");

                var files = await picker.PickMultipleFilesAsync();

                if (files != null && files.Count > 0)
                {
                    if (VM != null)
                    {
                        ShowLoader(true, "Importing files..");
                    }
                    var fileProgress = 0;
                    var SystemFolder = (StorageFolder)await VM.SelectedSystem.GetSystemDirectoryAsync();
                    if (SystemFolder != null)
                    {
                        CoreLoadingProgress.Visibility = Visibility.Visible;
                        foreach (var file in files)
                        {
                            await file.CopyAsync(SystemFolder, file.Name, NameCollisionOption.ReplaceExisting);
                            fileProgress++;
                            _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                            {
                                try
                                {
                                    fileProgress++;
                                    if (fileProgress <= PlatformService.MaxProgressValue)
                                    {
                                        CoreLoadingProgress.Value = fileProgress;
                                    }
                                }
                                catch (Exception er)
                                {
                                }
                            });
                        }
                        while (currentProgress <= PlatformService.MaxProgressValue)
                        {
                            currentProgress++;
                            CoreLoadingProgress.Value = currentProgress;
                            await Task.Delay(1);
                        }
                        CoreLoadingProgress.Visibility = Visibility.Collapsed;
                        PlatformService.PlayNotificationSoundDirect("success");
                        var localNotificationData = new LocalNotificationData();
                        localNotificationData.icon = SegoeMDL2Assets.ActionCenterNotification;
                        localNotificationData.message = "Files imported successfully, Enjoy!\nNote: to remove use Reset button in BIOS page";
                        localNotificationData.time = 5;
                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                        VM.GetFileDependencyForSelectedCore();
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        LocalNotificationData localNotificationData = new LocalNotificationData();
                        localNotificationData.icon = SegoeMDL2Assets.DisconnectDrive;
                        localNotificationData.message = "Failed to get system folder!";
                        localNotificationData.time = 3;
                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                    }
                    if (VM != null)
                    {
                        ShowLoader(false);
                        CoreLoadingProgress.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
                if (VM != null)
                {
                    ShowLoader(false);
                    CoreLoadingProgress.Visibility = Visibility.Collapsed;
                }
            }
        }

        string swipeID = "";
        private async void CorePagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            swipeID = System.IO.Path.GetRandomFileName();
            string swipeIDTemp = swipeID + "";
            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
            {
                try
                {
                    try
                    {
                        await PivotCoreMainContent.Fade(0, 0).StartAsync();
                        await PivotCoreGamesContent.Fade(0, 0).StartAsync();
                        await CoreOptionsContainer.Fade(0, 0).StartAsync();
                        await PivotCoreBIOSContent.Fade(0, 0).StartAsync();
                        await PivotCoreInsightsContent.Fade(0, 0).StartAsync();
                    }
                    catch (Exception ex)
                    {

                    }
                    if (PlatformService.DeviceIsPhone())
                    {
                        await Task.Delay(350);
                    }
                    else
                    {
                        await Task.Delay(250);
                    }

                    if (!swipeIDTemp.Equals(swipeID))
                    {
                        return;
                    }
                    try
                    {
                        if (CorePagePivot.SelectedItem == PivotCoreMain)
                        {
                            PivotCoreMainContent.Fade(1, 250).StartAsync();
                        }
                        else if (CorePagePivot.SelectedItem == PivotCoreGames)
                        {
                            PivotCoreGamesContent.Fade(1, 250).StartAsync();
                        }
                        else if (CorePagePivot.SelectedItem == PivotCoreOptions)
                        {
                            CoreOptionsContainer.Fade(1, 250).StartAsync();
                        }
                        else if (CorePagePivot.SelectedItem == PivotCoreBIOS)
                        {
                            PivotCoreBIOSContent.Fade(1, 250).StartAsync();
                        }
                        else if (CorePagePivot.SelectedItem == PivotCoreInsights)
                        {
                            PivotCoreInsightsContent.Fade(1, 250).StartAsync();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    PlatformService.PlayNotificationSoundDirect("select");
                    if (RecentsPanel.Visibility == Visibility.Collapsed)
                    {
                        return;
                    }

                    if (CorePagePivot.SelectedItem == PivotCoreOptions)
                    {
                        if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                        {
                            await ShowSelectedSystemOptions();
                        }
                        MemoryValueContainer.Visibility = Visibility.Collapsed;
                        PivotCoreOptions.UpdateLayout();
                    }
                    else if (CorePagePivot.SelectedItem == PivotCoreBIOS)
                    {
                        MemoryValueContainer.Visibility = Visibility.Collapsed;
                        PivotCoreBIOS.UpdateLayout();
                    }
                    else if (CorePagePivot.SelectedItem == PivotCoreInsights)
                    {
                        MemoryValueContainer.Visibility = Visibility.Collapsed;
                        PivotCoreInsights.UpdateLayout();
                    }
                    else if (CorePagePivot.SelectedItem == PivotCoreGames)
                    {
                        if (!VM.SelectedSystem.SkippedCore && !VM.SelectedSystem.FailedToLoad)
                        {
                            SetGamesFolder.Visibility = Visibility.Visible;
                            SingleGame.Visibility = Visibility.Visible;
                            AllGetter_Click(false, true);
                        }
                        else
                        {
                            SetGamesFolder.Visibility = Visibility.Collapsed;
                            SingleGame.Visibility = Visibility.Collapsed;
                            VM.NoGamesListVisible = true;
                            VM.RaisePropertyChanged("NoGamesListVisible");
                        }
                        MemoryValueContainer.Visibility = Visibility.Visible;
                        MemoryValueContainer.Margin = new Thickness(0, 61.5, 22, 5);
                        PivotCoreGames.UpdateLayout();
                    }
                    else
                    {
                        MemoryValueContainer.Visibility = Visibility.Visible;
                        MemoryValueContainer.Margin = new Thickness(0, 54, 20, 5);
                        PivotCoreMain.UpdateLayout();
                    }
                    try
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                        {
                            try
                            {
                                SystemRecentsList.UpdateLayout();
                                ScrollViewer svl = PlatformService.FindVisualChildren<ScrollViewer>(SystemRecentsList).FirstOrDefault();
                                if (svl != null)
                                {
                                    PlatformService.vScroll = svl.VerticalOffset;
                                }
                                if (CorePagePivot.SelectedItem == PivotCoreGames)
                                {
                                    await Task.Delay(100);
                                    if (item3 != null)
                                    {
                                        item3.Focus(FocusState.Programmatic);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                    }
                    catch
                    {

                    }
                    while (RetriXGoldCoresLoaderInProgress)
                    {
                        //The cores loader could take some time to finish
                        //returningFromSubPage will be always false if I didn't wait until it's finsih
                        await Task.Delay(100);
                    }
                    if (!returningFromSubPage)
                    {
                        PlatformService.pivotPosition = CorePagePivot.SelectedIndex;
                    }
                }
                catch (Exception ex)
                {

                }
            });
        }

        CancellationTokenSource cancellationTokenSourceBackup = new CancellationTokenSource();
        private async Task ExportSettings(bool currentSystem = false)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");

                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmBackup = new ConfirmConfig();
                confirmBackup.SetTitle("Start Backup");
                confirmBackup.SetMessage("Do you want to backup all settings Saves/Actions/BIOS/Recents?");
                if (currentSystem)
                {
                    confirmBackup.SetMessage($"Do you want to backup {VM.SelectedSystem.Name}?\nNote: Play history will not be included, only Saves & System folders");
                }
                confirmBackup.UseYesNo();

                var StartBackup = await UserDialogs.Instance.ConfirmAsync(confirmBackup);
                if (StartBackup)
                {
                    FolderPicker folderPicker = new FolderPicker();
                    folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                    folderPicker.FileTypeFilter.Add(".rbp");
                    var saveFolder = await folderPicker.PickSingleFolderAsync();
                    if (saveFolder != null)
                    {
                        var localFolder = ApplicationData.Current.LocalFolder;

                        if (localFolder != null)
                        {
                            VM.SystemCoreIsLoadingState(true, "Generating backup..");
                            await Task.Delay(400);

                            var fileDate = DateTime.Now.ToString().Replace("/", ".").Replace("\\", "_").Replace(":", ".").Replace(" ", ".");
                            string targetFileName = $"RXG.Backup{fileDate}.rbp";
                            if (currentSystem)
                            {
                                targetFileName = $"RXG.Backup ({VM.SelectedSystem.Name}) {fileDate}.rbp";
                            }
                            var zipFile = await saveFolder.CreateFileAsync(targetFileName, CreationCollisionOption.GenerateUniqueName);
                            string taskFailed = "";

                            if (zipFile != null)
                            {
                                ArchiverPlus archiverPlus = new ArchiverPlus();
                                TaskCompletionSource<bool> backupTask = new TaskCompletionSource<bool>();
#if USING_NATIVEARCHIVE
                                await Task.Run(async () =>
                                {
                                    try
                                    {
                                        archiverPlus.CompressingProgress += async (sender, e) =>
                                        {
                                            try
                                            {
                                                string fileName = e.CurrentFile;
                                                if (fileName.Length > 30)
                                                {
                                                    fileName = $"{(System.IO.Path.GetFileNameWithoutExtension(fileName)).Substring(0, 20)}{System.IO.Path.GetExtension(fileName)}";
                                                }
                                                var attrs = await zipFile.GetBasicPropertiesAsync();
                                                var zipFileSize = (long)attrs.Size;
                                                _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                                {
                                                    VM.SystemCoreIsLoadingState(true, $"Generating backup..\nBackup Size: {zipFileSize.ToFileSize()}\n\nCompressing files..\n{fileName}");
                                                });
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        };
                                        await archiverPlus.Compress(localFolder, zipFile, System.IO.Compression.CompressionLevel.Fastest);
                                        if (archiverPlus.log.Count > 0)
                                        {
                                            taskFailed = String.Join("\n", archiverPlus.log);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        taskFailed = ex.Message;
                                    }
                                });
#else
                                var exportProgressPrepare = new Progress<int>(async value =>
                                {
                                    await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                    {
                                        try
                                        {
                                            VM.SystemCoreIsLoadingState(true, $"Preparing files {value}..");
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    });
                                });
                                var UpdateCurrentFile = new Progress<Dictionary<string, long>>(async value =>
                                {
                                    try
                                    {
                                        string fileName = value.FirstOrDefault().Key;
                                        string size = value.FirstOrDefault().Value.ToFileSize();
                                        try
                                        {
                                            if (fileName.Length > 30)
                                            {
                                                fileName = $"{(System.IO.Path.GetFileNameWithoutExtension(fileName)).Substring(0, 20)}{System.IO.Path.GetExtension(fileName)}";
                                            }
                                            var attrs = await zipFile.GetBasicPropertiesAsync();
                                            var zipFileSize = (long)attrs.Size;
                                            _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                            {
                                                VM.SystemCoreIsLoadingState(true, $"Generating backup..\nBackup Size: {zipFileSize.ToFileSize()}\n\nCompressing files..\n{fileName} ({size})");
                                            });
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                                cancellationTokenSourceBackup = new CancellationTokenSource();
                                CancelButton.Visibility = Visibility.Visible;
                                VM.SetCoresQuickAccessContainerState(false);
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        using (var stream = await zipFile.OpenStreamForWriteAsync())
                                        {
                                            using (var archive = ZipArchive.Create())
                                            {
                                                try
                                                {
                                                    string[] customFolders = null;
                                                    if (currentSystem)
                                                    {
                                                        customFolders = new string[] { $"{VM.SelectedSystem.Core.Name} - Saves", $"{VM.SelectedSystem.Core.Name} - System" };
                                                    }
                                                    //To avoid UI block run this code into Task
                                                    await archive.AddAllFromDirectory(localFolder, PlatformService.UseWindowsIndexer, null, customFolders, exportProgressPrepare, false, cancellationTokenSourceBackup);
                                                    //AddAllFromDirectory can extended to:
                                                    //AddAllFromDirectory(storageFolder, string[] searchPattern, SearchOption.AllDirectories, IProgress<int> progress, bool IncludeRootFolder, CancellationTokenSource cancellationTokenSource)
                                                    //IProgress<int> will report how many file queued

                                                    if (!cancellationTokenSourceBackup.IsCancellationRequested)
                                                    {
                                                        archive.SaveTo(stream, UpdateCurrentFile, cancellationTokenSourceBackup);
                                                    }
                                                    //SaveTo can extended to:
                                                    //SaveTo(Stream stream, IProgress<Dictionary<string, long>> progress, CancellationTokenSource cancellationTokenSource)
                                                    //IProgress<Dictionary<string, long>> will provide file name / size like below:
                                                    //string fileName = value.FirstOrDefault().Key;
                                                    //string size = value.FirstOrDefault().Value.ToFileSize();
                                                }
                                                catch (Exception ex)
                                                {
                                                    taskFailed = ex.Message;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        taskFailed = ex.Message;
                                    }
                                    backupTask.SetResult(true);
                                });

#endif
                                await backupTask.Task;


                                if (taskFailed.Length > 0)
                                {
                                    VM.SystemCoreIsLoadingState(false);
                                    PlatformService.PlayNotificationSoundDirect("faild");
                                    PlatformService.ShowNotificationMain($"Failed: {taskFailed}", 5);
                                }
                                else
                                {
                                    if (cancellationTokenSourceBackup.IsCancellationRequested)
                                    {
                                        VM.SystemCoreIsLoadingState(false);
                                        PlatformService.PlayNotificationSoundDirect("alert");
                                        pushNormalNotification("Backup cancelled");
                                    }
                                    else
                                    {
                                        VM.SystemCoreIsLoadingState(false);
                                        PlatformService.PlayNotificationSoundDirect("success");
                                        pushNormalNotification("Backup successfully created");
                                    }
                                }
                            }
                            else
                            {
                                VM.SystemCoreIsLoadingState(false);
                                PlatformService.PlayNotificationSoundDirect("faild");
                                pushNormalNotification("Failed to backup, cannot create zip file!");
                            }
                        }
                        else
                        {
                            VM.SystemCoreIsLoadingState(false);
                            PlatformService.PlayNotificationSoundDirect("faild");
                            pushNormalNotification("Failed to backup, local folder not found!");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.PlayNotificationSoundDirect("faild");
                PlatformService.ShowErrorMessageDirect(e);
                VM.SystemCoreIsLoadingState(false);
            }
            CancelButton.Visibility = Visibility.Collapsed;
            VM.SetCoresQuickAccessContainerState(true);
        }

        private async Task ImportSettingsSlotsAction(StorageFile PreBackupFile = null, bool currentSystem = false)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");

                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                filePicker.FileTypeFilter.Add(".rbp");

                StorageFile file = null;
                if (PreBackupFile == null)
                {
                    file = await filePicker.PickSingleFileAsync();
                }
                else
                {
                    file = PreBackupFile;
                }
                if (file != null)
                {
                    PlatformService.PlayNotificationSoundDirect("alert");
                    ConfirmConfig confirmImportSaves = new ConfirmConfig();
                    confirmImportSaves.SetTitle("Restore Backup");
                    confirmImportSaves.SetMessage("All previous files will be lost, are you sure?");
                    if (currentSystem)
                    {
                        confirmImportSaves.SetMessage($"All {VM.SelectedSystem.Name}'s files in Saves and System folder will be lost, are you sure?");
                    }
                    confirmImportSaves.UseYesNo();

                    var StartImport = await UserDialogs.Instance.ConfirmAsync(confirmImportSaves);

                    if (StartImport)
                    {
                        VM.SystemCoreIsLoadingState(true, "Restoring backup..");
                        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                        bool state = false;
#if USING_NATIVEARCHIVE
                        PackageArchiveHelper packageArchiveHelper = new PackageArchiveHelper();
                        packageArchiveHelper.ExtractProgress += (sender, e) =>
                        {
                            var entryProgress = e.PercentageReadExact;
                            var sizeProgress = e.BytesTransferred.ToFileSize();
                            _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                            {
                                try
                                {
                                    VM.SystemCoreIsLoadingState(true, $"Restoring backup..\nExtarcting ({System.IO.Path.GetFileName(e.entry.Name)})\n{entryProgress}% / {sizeProgress}");
                                }
                                catch (Exception ex)
                                {

                                }
                            });
                        };
                        state = await packageArchiveHelper.ExtractFiles(localFolder, file);
#else
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
                                _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                {
                                    try
                                    {
                                        VM.SystemCoreIsLoadingState(true, $"Restoring backup..\nExtarcting ({System.IO.Path.GetFileName(e.Item.Key)})\n{entryProgress}% / {sizeProgress}");
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                            };

                            //Extract files
                            while (reader.MoveToNextEntry())
                            {
                                if (!reader.Entry.IsDirectory)
                                {
                                    await reader.WriteEntryToDirectory(localFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                    //WriteEntryToDirectory can extended to:
                                    //WriteEntryToDirectory(folder, options, cancellationTokenSource)
                                    //                                       ^^^^^^^^^^^^^^^^^^^^^^^
                                    //it will help to terminate the current entry directly if cancellation requested
                                }
                            }
                            state = true;
                        }
#endif

                        if (state)
                        {
                            PlatformService.PlayNotificationSoundDirect("success");
                            PlatformService.ShowNotificationMain($"Backup Successfully Restored", 3);
                            RetriXGoldCoresLoader(true);
                        }
                        else
                        {
                            pushNormalNotification("Backup Restore Failed");
                            RetriXGoldCoresLoader(true);
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("faild");
                        if (PreBackupFile == null)
                        {
                            pushNormalNotification("Backup Restore Failed");
                        }
                        else
                        {
                            pushNormalNotification("Backup Restore Canceled");
                        }
                    }
                }
            }
            catch (Exception e)
            {

                PlatformService.ShowErrorMessageDirect(e);

            }
            PlatformService.OpenBackupFile = null;
            VM.SystemCoreIsLoadingState(false);
        }
        private async void ResetRetrix()
        {
            PlatformService.PlayNotificationSoundDirect("button-01");
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmReset = new ConfirmConfig();
                confirmReset.SetTitle("Reset System");
                confirmReset.SetMessage("This action will reset all system's data\nIncluding BIOS, Saves, Actions..\nAre you sure?");
                confirmReset.UseYesNo();

                var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                if (StartReset)
                {
                    VM.SystemCoreIsLoadingState(true, "Resetting..");
                    var localFolders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();
                    if (localFolders != null && localFolders.Count > 0)
                    {
                        foreach (var DirectoryItem in localFolders)
                        {
                            try
                            {
                                await DirectoryItem.DeleteAsync();
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.Message);
                            }
                        }
                    }
                    var localFiles = await ApplicationData.Current.LocalFolder.GetFilesAsync();
                    if (localFiles != null && localFiles.Count > 0)
                    {
                        foreach (var FileItem in localFiles)
                        {
                            try
                            {
                                await FileItem.DeleteAsync();
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.Message);
                            }
                        }
                    }
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("CoresData", false);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"RETRIXGOLDINGAMESCALEX", 1d);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"RETRIXGOLDINGAMESCALEY", 1d);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"RETRIXGOLDINGAMEOFFSETX", 0d);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"RETRIXGOLDINGAMEOFFSETY", 0d);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"RETRIXGOLDSCALEX", 1d);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"RETRIXGOLDSCALEY", 1d);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"RETRIXGOLDOFFSETX", 0d);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"RETRIXGOLDOFFSETY", 0d);

                    foreach (var sItem in PlatformService.Consoles)
                    {
                        try
                        {
                            PlatformService.RemoveGamesFolderFromAccessList(sItem.Name);
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{sItem.Core.Name}SCALEX", 1d);
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{sItem.Core.Name}SCALEY", 1d);
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{sItem.Core.Name}OFFSETX", 0d);
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{sItem.Core.Name}OFFSETY", 0d);
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{sItem.Core.Name}AIDEVICE", 0);
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{sItem.Core.Name}ASPECT", "4:3");
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    try
                    {
                        var bmpFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("TranscodedImages");
                        if (bmpFolder != null)
                        {
                            await bmpFolder.DeleteAsync();
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        Plugin.Settings.CrossSettings.Current.Clear();
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        PlatformService.GamesRecents.Clear();
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        VM.FilesListCache.Clear();
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        GameSystemSelectionViewModel.SystemRoots.Clear();
                    }
                    catch (Exception ex)
                    {

                    }
                    await Task.Delay(2000);
                    PlatformService.PlayNotificationSoundDirect("success");
                    PlatformService.ShowNotificationMain($"Reset RetriXGold done", 3);
                    RetriXGoldCoresLoader(true);
                }
            }
            catch (Exception ex)
            {

                PlatformService.ShowErrorMessageDirect(ex);

            }
            VM.SystemCoreIsLoadingState(false);
        }

        List<string> lastImportedCores = new List<string>();
        private async Task ImportAnyCoreNewAction()
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01");
                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                filePicker.FileTypeFilter.Add(".rar");
                filePicker.FileTypeFilter.Add(".zip");
                filePicker.FileTypeFilter.Add(".7z");
                filePicker.FileTypeFilter.Add(".dat");
                filePicker.FileTypeFilter.Add(".dll");
                filePicker.FileTypeFilter.Add(".txt");
                var files = await filePicker.PickMultipleFilesAsync();
                if (files != null && files.Count > 0)
                {
                    PlatformService.PlayNotificationSoundDirect("alert");
                    ConfirmConfig confirmImportSaves = new ConfirmConfig();
                    confirmImportSaves.SetTitle("Import Core");
                    confirmImportSaves.SetMessage("After import the core, more settings will be available when you select the system.\nIf you're trying to import core already loaded use update option from core's page.\n\nImport new core?");
                    confirmImportSaves.UseYesNo();

                    var StartImport = await UserDialogs.Instance.ConfirmAsync(confirmImportSaves);

                    if (StartImport)
                    {
                        VM.SystemCoreIsLoadingState(true, "Importing cores..");
                        StorageFolder zipsDirectory = null;
                        var localFolder = ApplicationData.Current.LocalFolder;
                        var targetFolder = "AnyCore";

                        zipsDirectory = await localFolder.CreateFolderAsync(targetFolder, CreationCollisionOption.OpenIfExists);
                        if (zipsDirectory != null)
                        {
                            int skippedCores = 0;
                            foreach (var file in files)
                            {
                                var fileExt = System.IO.Path.GetExtension(file.Name);
                                switch (fileExt)
                                {
                                    case ".zip":
                                    case ".7z":
                                        {
                                            //Compressed files should be extracted directly to local folder
                                            VM.SystemCoreIsLoadingState(true, "Importing core..");
                                            StorageFolder localFolderDirect = ApplicationData.Current.LocalFolder;
                                            bool state = false;
#if USING_NATIVEARCHIVE
                        PackageArchiveHelper packageArchiveHelper = new PackageArchiveHelper();
                        packageArchiveHelper.ExtractProgress += (sender, e) =>
                        {
                            var entryProgress = e.PercentageReadExact;
                            var sizeProgress = e.BytesTransferred.ToFileSize();
                            _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                            {
                                try
                                {
                                    VM.SystemCoreIsLoadingState(true, $"Restoring backup..\nExtarcting ({System.IO.Path.GetFileName(e.entry.Name)})\n{entryProgress}% / {sizeProgress}");
                                }
                                catch (Exception ex)
                                {

                                }
                            });
                        };
                        state = await packageArchiveHelper.ExtractFiles(localFolderDirect, file);
#else
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
                                                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                                                    {
                                                        try
                                                        {
                                                            VM.SystemCoreIsLoadingState(true, $"Importing core..\nExtarcting ({System.IO.Path.GetFileName(e.Item.Key)})\n{entryProgress}% / {sizeProgress}");
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                    });
                                                };

                                                //Extract files
                                                while (reader.MoveToNextEntry())
                                                {
                                                    if (!reader.Entry.IsDirectory)
                                                    {
                                                        await reader.WriteEntryToDirectory(localFolderDirect, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                                        //WriteEntryToDirectory can extended to:
                                                        //WriteEntryToDirectory(folder, options, cancellationTokenSource)
                                                        //                                       ^^^^^^^^^^^^^^^^^^^^^^^
                                                        //it will help to terminate the current entry directly if cancellation requested
                                                    }
                                                }
                                                state = true;
                                            }
#endif

                                            if (!state)
                                            {
                                                skippedCores++;
                                            }
                                        }
                                        break;

                                    default:
                                        {
                                            var IsPureCore = false;
                                            targetFolder = "AnyCore";
                                            try
                                            {
                                                var testPureCore = FindAnyCoreInOriginalCores(file.Name);
                                                if (testPureCore.Length > 0)
                                                {
                                                    targetFolder = "PureCore";
                                                    IsPureCore = true;
                                                }
                                                if (!zipsDirectory.Name.Equals(targetFolder))
                                                {
                                                    zipsDirectory = await localFolder.CreateFolderAsync(targetFolder, CreationCollisionOption.OpenIfExists);
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                            try
                                            {
                                                var mainPath = $@"Assets\Libraries\{System.IO.Path.GetFileNameWithoutExtension(file.Name)}";
                                                var testFile = await Package.Current.InstalledLocation.TryGetItemAsync($"{mainPath}.dll");
                                                if (testFile != null)
                                                {
                                                    skippedCores++;
                                                    continue;
                                                }
                                                else
                                                {
                                                    testFile = await Package.Current.InstalledLocation.TryGetItemAsync($"{mainPath}.dat");
                                                    if (testFile != null)
                                                    {
                                                        skippedCores++;
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        //override IsPureCore start, because the file is not included within the package
                                                        IsPureCore = false;
                                                        targetFolder = "AnyCore";
                                                        if (!zipsDirectory.Name.Equals(targetFolder))
                                                        {
                                                            zipsDirectory = await localFolder.CreateFolderAsync(targetFolder, CreationCollisionOption.OpenIfExists);
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                            await file.CopyAsync(zipsDirectory, file.Name, NameCollisionOption.ReplaceExisting);
                                            if (!IsPureCore)
                                            {
                                                if (!lastImportedCores.Contains(file.Name))
                                                {
                                                    lastImportedCores.Add(file.Name);
                                                }
                                            }
                                        }
                                        break;
                                }

                            }
                            if (skippedCores == files.Count)
                            {
                                PlatformService.PlayNotificationSoundDirect("error");

                                PlatformService.ShowNotificationMain("Cannot import, cores already exists", 4);
                            }
                            else
                            {
                                PlatformService.PlayNotificationSoundDirect("success");

                                if (skippedCores > 0)
                                {
                                    PlatformService.ShowNotificationMain($"Core imported successfully\nSkipped ({skippedCores}) already exists", 4);
                                }
                                else
                                {
                                    PlatformService.ShowNotificationMain("Core imported successfully", 4);
                                }
                                RetriXGoldCoresLoader(true);
                            }
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("faild");
                            pushNormalNotification("Unable to open or create 'AnyCore' folder!");
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("faild");
                        pushNormalNotification("Core import canceled!");
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }
            VM.SystemCoreIsLoadingState(false);
        }

        bool gamesListLoaded = false;
        private void SystemRecentsList_Loaded(object sender, RoutedEventArgs e)
        {
            gamesListLoaded = true;
        }

        private async void ResetBIOS_Click(object sender, RoutedEventArgs e)
        {
            if (VM.SelectedSystem.SkippedCore || VM.SelectedSystem.FailedToLoad)
            {
                if (!pushNormalNotification($"This core has loading issue, please fix it first!"))
                {
                    _ = UserDialogs.Instance.AlertAsync($"This core has loading issue, please fix it first!");
                }
                return;
            }
            PlatformService.PlayNotificationSoundDirect("button-01");
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmReset = new ConfirmConfig();
                confirmReset.SetTitle("Reset BIOS");
                confirmReset.SetMessage($"This action will delete all imported BIOS files?\nImportant: This action will remove all the files in {VM.SelectedSystem.Name}'s system folder, use manual delete if you're not sure");
                confirmReset.UseYesNo();

                var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                if (StartReset)
                {
                    VM.SystemCoreIsLoadingState(true, $" {VM.SelectedSystem.Name}\nResetting BIOS..");
                    var systemFolder = (StorageFolder)await VM.SelectedSystem.GetSystemDirectoryAsync();
                    if (systemFolder != null)
                    {
                        var localFolder = await systemFolder.GetFilesAsync();
                        if (localFolder != null && localFolder.Count() > 0)
                        {
                            foreach (var DirectoryItem in localFolder)
                            {
                                await DirectoryItem.DeleteAsync();
                            }

                            var localFolders = await systemFolder.GetFoldersAsync();
                            if (localFolders != null && localFolders.Count() > 0)
                            {
                                foreach (var subItem in localFolders)
                                {
                                    await subItem.DeleteAsync();
                                }
                            }

                            PlatformService.PlayNotificationSoundDirect("success");
                            LocalNotificationData localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.DisconnectDrive;
                            localNotificationData.message = "BIOS reseted successfully!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                            VM.GetFileDependencyForSelectedCore();
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("error");
                            LocalNotificationData localNotificationData = new LocalNotificationData();
                            localNotificationData.icon = SegoeMDL2Assets.DisconnectDrive;
                            localNotificationData.message = "No BIOS files found!";
                            localNotificationData.time = 3;
                            PlatformService.NotificationHandlerMain(null, localNotificationData);
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        LocalNotificationData localNotificationData = new LocalNotificationData();
                        localNotificationData.icon = SegoeMDL2Assets.DisconnectDrive;
                        localNotificationData.message = "No system folder found!";
                        localNotificationData.time = 3;
                        PlatformService.NotificationHandlerMain(null, localNotificationData);
                    }
                }
            }
            catch (Exception ex)
            {

                PlatformService.ShowErrorMessageDirect(ex);

            }
            VM.SystemCoreIsLoadingState(false);
        }

        CoresQuickListItem GlobalSelectedItem = null;
        private void PlayRecentGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GamePlayerView.isUpscaleActive = false;
                if (GlobalSelectedItem != null)
                {
                    VM.PlayGameByName(GlobalSelectedItem.Recent.GameName);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void PlayRecentGameVFSOff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GamePlayerView.isUpscaleActive = false;
                VM.SelectedSystem.Core.VFSSupport = false;
                if (GlobalGameSelect != null)
                {
                    VM.PlayGameByName(GlobalGameSelect.GameName, false, false, true);
                }
                else if (GlobalSelectedItem != null)
                {
                    VM.PlayGameByName(GlobalSelectedItem.Recent.GameName, false, false, true);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void DeleteRecentGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GlobalSelectedItem != null)
                {
                    EventHandler eventHandler = (dsender, de) =>
                    {
                        if (dsender != null && (bool)dsender)
                        {
                            var ItemFound = false;
                            try
                            {
                                dataSourceRecents.Remove(GlobalSelectedItem);
                                ItemFound = true;
                            }
                            catch (Exception ex)
                            {

                            }
                            if (!ItemFound || dataSourceRecents.Count == 0)
                            {
                                CoreQuickAccessContainer.UpdateLayout();
                                PlatformService.pivotMainPosition = CoreQuickAccessContainer.VerticalOffset;
                                returningFromSubPage = true;
                                PrepareCoreData();
                            }
                            else
                            {
                                textBlockRecents.Content = $"Played ({dataSourceRecents.Count})".ToUpper();
                            }
                        }
                    };
                    GlobalSelectedItem.ProgressState = Visibility.Visible;
                    await VM.GameSystemRecentsHoldingHandler(GlobalSelectedItem.Recent, eventHandler);
                    GlobalSelectedItem.ProgressState = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                if (GlobalSelectedItem != null)
                {
                    GlobalSelectedItem.ProgressState = Visibility.Visible;
                }
            }
        }


        GameSystemRecentModel GlobalGameSelect = null;
        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GamePlayerView.isUpscaleActive = false;
                if (GlobalGameSelect != null)
                {
                    VM.PlayGameByName(GlobalGameSelect.GameName);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void GameSize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GlobalGameSelect != null)
                {
                    VM.GameSystemRecentsHoldingHandler(GlobalGameSelect);
                }
            }
            catch (Exception ex)
            {

            }
        }

        bool systemRecentTappedTriggered = false;
        private void SystemRecentsList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                systemRecentTappedTriggered = true;
                GameSystemRecentModel SelectOption = null;
                UIElement targetElement = null;
                string ObjectType = e.OriginalSource.GetType().Name;
                switch (ObjectType)
                {
                    case "Image":
                        SelectOption = (GameSystemRecentModel)((Image)e.OriginalSource).DataContext;
                        targetElement = ((Image)e.OriginalSource);
                        break;

                    case "TextBlock":
                        SelectOption = (GameSystemRecentModel)((TextBlock)e.OriginalSource).DataContext;
                        targetElement = ((TextBlock)e.OriginalSource);
                        break;

                    case "Border":
                        SelectOption = (GameSystemRecentModel)((Border)e.OriginalSource).DataContext;
                        targetElement = ((Border)e.OriginalSource);
                        break;

                    case "Grid":
                        SelectOption = null;
                        break;

                    case "GridView":
                        SelectOption = (GameSystemRecentModel)((GridView)e.OriginalSource).DataContext;
                        targetElement = ((GridView)e.OriginalSource);
                        break;

                    case "GridViewItem":
                        SelectOption = (GameSystemRecentModel)((GridViewItem)e.OriginalSource).DataContext;
                        targetElement = ((GridViewItem)e.OriginalSource);
                        break;

                    case "GridViewItemPresenter":
                        SelectOption = (GameSystemRecentModel)((GridViewItemPresenter)e.OriginalSource).DataContext;
                        targetElement = ((GridViewItemPresenter)e.OriginalSource);
                        break;

                    case "ListViewItemPresenter":
                        SelectOption = (GameSystemRecentModel)((ListViewItemPresenter)e.OriginalSource).DataContext;
                        targetElement = ((ListViewItemPresenter)e.OriginalSource);
                        break;

                    default:
                        SelectOption = (GameSystemRecentModel)((ListViewItem)e.OriginalSource).DataContext;
                        targetElement = ((ListViewItem)e.OriginalSource);
                        break;
                }
                if (SelectOption == null)
                {
                    return;
                }
                var point = e.GetPosition(targetElement);
                GlobalGameSelect = SelectOption;
                GameContextMenu.ShowAt(targetElement, point);
            }
            catch (Exception ex)
            {

            }
        }

        GridViewItem item3;
        private async void SystemRecentsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                await Task.Delay(10);
                if (systemRecentTappedTriggered)
                {
                    systemRecentTappedTriggered = false;
                    return;
                }
                GameSystemRecentModel SelectOption = (GameSystemRecentModel)e.ClickedItem;
                UIElement targetElement = (UIElement)sender;
                var point = new Windows.Foundation.Point(30, 30);
                var dataItems = ((GridView)sender).Items.Where(item => ((GameSystemRecentModel)item).GameLocation == SelectOption.GameLocation);
                if (dataItems != null && dataItems.Count() > 0)
                {
                    item3 = ((GridView)sender).ContainerFromItem(dataItems.FirstOrDefault()) as GridViewItem;
                    targetElement = item;
                    point = new Windows.Foundation.Point(5, 5);
                    item2 = null;
                }

                if (SelectOption == null)
                {
                    return;
                }
                GlobalGameSelect = SelectOption;
                GameContextMenu.ShowAt(targetElement, point);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private void GoHomePage_Click(object sender, RoutedEventArgs e)
        {
            HideSystemGames();
        }

        private void ReloadCorePageHandler(object sender, EventArgs e)
        {
            ReloadCorePage_Click(null, null);
        }
        private void ReloadCorePage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CoreQuickAccessContainer.UpdateLayout();
                PlatformService.pivotMainPosition = CoreQuickAccessContainer.VerticalOffset;
                returningFromSubPage = true;
                PrepareCoreData();
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }


        Visibility arcadeExtra = Visibility.Collapsed;
        Visibility ArcadeExtra
        {
            get
            {
                if (ShowGameMenuOptions != Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return arcadeExtra;
                }
            }
            set
            {
                arcadeExtra = value;
            }
        }
        Visibility SeparatorExtra
        {
            get
            {
                if (ShowGameMenuOptions != Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
                if (VFSSupport)
                {
                    return Visibility.Visible;
                }
                return PSXExtra;
            }
        }
        Visibility pSXExtra = Visibility.Collapsed;
        Visibility PSXExtra
        {
            get
            {
                if (ShowGameMenuOptions != Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return pSXExtra;
                }
            }
            set
            {
                pSXExtra = value;
            }
        }
        private void PlayRecentGameAnalog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GamePlayerView.isUpscaleActive = false;
                if (GlobalGameSelect != null)
                {
                    VM.PlayGameByName(GlobalGameSelect.GameName, false, true);
                }
                else if (GlobalSelectedItem != null)
                {
                    VM.PlayGameByName(GlobalSelectedItem.Recent.GameName, false, true);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private void PlayRecentGameAI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GamePlayerView.isUpscaleActive = true;
                if (GlobalGameSelect != null)
                {
                    VM.PlayGameByName(GlobalGameSelect.GameName);
                }
                else if (GlobalSelectedItem != null)
                {
                    VM.PlayGameByName(GlobalSelectedItem.Recent.GameName);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private void PlayRecentGameSubSystem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GamePlayerView.isUpscaleActive = false;
                if (GlobalGameSelect != null)
                {
                    VM.PlayGameByName(GlobalGameSelect.GameName, true);
                }
                else if (GlobalSelectedItem != null)
                {
                    VM.PlayGameByName(GlobalSelectedItem.Recent.GameName, true);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void PlayRecentGameSmartRename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GamePlayerView.isUpscaleActive = false;
                if (GlobalGameSelect != null)
                {
                    VM.PlayGameByName(GlobalGameSelect.GameName, true);
                }
                else if (GlobalSelectedItem != null)
                {
                    VM.PlayGameByName(GlobalSelectedItem.Recent.GameName, true);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        #region Games
        private async Task GetAllGames()
        {
            try
            {
                bool foundFiles = false;
                //var items = 

                if (!foundFiles)
                {

                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Cores Configs
        public static async Task<long> CopyFolder(StorageFolder targetFolder, StorageFolder destinationFolder, string customExtention = "")
        {
            long folderSize = 0;
            try
            {
                var folders = targetFolder.CreateFileQuery(CommonFileQuery.OrderByName);
                if (customExtention.Length > 0)
                {
                    var fileSizeTasks = (await folders.GetFilesAsync()).Where(item => System.IO.Path.GetExtension(item.Path).ToLower().Equals(customExtention.ToLower())).Select(async file => (await file.CopyAsync(destinationFolder, file.Name, NameCollisionOption.ReplaceExisting)));

                    await Task.WhenAll(fileSizeTasks);
                }
                else
                {
                    var fileSizeTasks = (await folders.GetFilesAsync()).Select(async file => (await file.CopyAsync(destinationFolder, file.Name, NameCollisionOption.ReplaceExisting)));

                    await Task.WhenAll(fileSizeTasks);
                }
            }
            catch (Exception e)
            {

            }
            return folderSize;
        }


        private async Task<bool> IsCoresDLLFileSyncRequired()
        {
            try
            {
                var CoresSystemFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("PureCore");
                if (CoresSystemFolder == null)
                {
                    return true;
                }

                var CoresFilesFolder = (StorageFolder)await CoresSystemFolder.TryGetItemAsync("PureCore");
                if (CoresFilesFolder == null)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }
        private async Task SyncCoresDLLFiles()
        {
            try
            {
                ShowLoader(true, "Syncing Files..\nCores Contents");
                var CoresSystemFolder = ApplicationData.Current.LocalFolder;

                if (CoresSystemFolder != null)
                {
                    var CoresFilesFolder = (StorageFolder)await CoresSystemFolder.TryGetItemAsync("PureCore");
                    if (CoresFilesFolder == null)
                    {
                        var installLocation = Package.Current.InstalledLocation;
                        CoresFilesFolder = await CoresSystemFolder.CreateFolderAsync("PureCore");
                        if (CoresFilesFolder != null)
                        {
                            var CoresMainFiles = (StorageFile)await installLocation.TryGetItemAsync("PureCore.dat");
                            if (CoresMainFiles != null)
                            {
#if USING_NATIVEARCHIVE
                                PackageArchiveHelper packageArchiveHelper = new PackageArchiveHelper();
                                await packageArchiveHelper.ExtractFiles(CoresFilesFolder, CoresMainFiles);
#else
                                Stream zipStream = await CoresMainFiles.OpenStreamForReadAsync();
                                using (var zipArchive = ArchiveFactory.Open(zipStream))
                                {
                                    //It should support 7z, zip, rar, gz, tar
                                    var reader = zipArchive.ExtractAllEntries();

                                    //Bind progress event
                                    reader.EntryExtractionProgress += (sender, e) =>
                                    {
                                        var entryProgress = e.ReaderProgress.PercentageReadExact;
                                        var sizeProgress = e.ReaderProgress.BytesTransferred.ToFileSize();
                                        ShowLoader(true, $"Syncing Files..\nCores Contents ({Math.Round(entryProgress)}%)\n{System.IO.Path.GetFileName(e.Item.Key)}");
                                    };

                                    //Extract files
                                    while (reader.MoveToNextEntry())
                                    {
                                        if (!reader.Entry.IsDirectory)
                                        {
                                            await reader.WriteEntryToDirectory(CoresFilesFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                        }
                                    }
                                }
#endif
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }






        private async Task<bool> IsTestContentFileSyncRequired()
        {
            try
            {
                var TestContentSystemFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("TestContent");
                if (TestContentSystemFolder == null)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }
        private async Task SyncTestContentFiles()
        {
            try
            {
                ShowLoader(true, "Syncing Files..\nTest Contents");
                var TestContentSystemFolder = ApplicationData.Current.LocalFolder;

                if (TestContentSystemFolder != null)
                {
                    var TestContentFilesFolder = (StorageFolder)await TestContentSystemFolder.TryGetItemAsync("TestContent");
                    if (TestContentFilesFolder == null)
                    {
                        var installLocation = Package.Current.InstalledLocation;
                        TestContentFilesFolder = await TestContentSystemFolder.CreateFolderAsync("TestContent");
                        if (TestContentFilesFolder != null)
                        {
                            var TestContentMainFiles = (StorageFile)await installLocation.TryGetItemAsync("lib7.dat");
                            if (TestContentMainFiles != null)
                            {
#if USING_NATIVEARCHIVE
                                PackageArchiveHelper packageArchiveHelper = new PackageArchiveHelper();
                                await packageArchiveHelper.ExtractFiles(TestContentFilesFolder, TestContentMainFiles);
#else
                                Stream zipStream = await TestContentMainFiles.OpenStreamForReadAsync();
                                using (var zipArchive = ArchiveFactory.Open(zipStream))
                                {
                                    //It should support 7z, zip, rar, gz, tar
                                    var reader = zipArchive.ExtractAllEntries();

                                    //Bind progress event
                                    reader.EntryExtractionProgress += (sender, e) =>
                                    {
                                        var entryProgress = e.ReaderProgress.PercentageReadExact;
                                        var sizeProgress = e.ReaderProgress.BytesTransferred.ToFileSize();
                                        ShowLoader(true, $"Syncing Files..\nTest Contents ({Math.Round(entryProgress)}%)\n{System.IO.Path.GetFileName(e.Item.Key)}");
                                    };

                                    //Extract files
                                    while (reader.MoveToNextEntry())
                                    {
                                        if (!reader.Entry.IsDirectory)
                                        {
                                            await reader.WriteEntryToDirectory(TestContentFilesFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                        }
                                    }
                                }
#endif
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private async Task<bool> IsQuakeFileSyncRequired()
        {
            try
            {
                var mainPath = $@"Assets\Libraries\tyrquake_libretro";
                var testFile = await Package.Current.InstalledLocation.TryGetItemAsync($"{mainPath}.dll");
                if (testFile == null)
                {
                    testFile = await Package.Current.InstalledLocation.TryGetItemAsync($"{mainPath}.dat");
                    if (testFile == null)
                    {
                        var pureCoreFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("PureCore");
                        if (pureCoreFolder != null)
                        {
                            testFile = await pureCoreFolder.TryGetItemAsync("tyrquake_libretro.dll");
                            if (testFile == null)
                            {
                                testFile = await pureCoreFolder.TryGetItemAsync("tyrquake_libretro.dat");
                                if (testFile == null)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                var QuakeSystemFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("TyrQuake - System");
                if (QuakeSystemFolder == null)
                {
                    return true;
                }

                var QuakeFilesFolder = (StorageFolder)await QuakeSystemFolder.TryGetItemAsync("quake");
                if (QuakeFilesFolder == null)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }
        private async Task SyncExtraCoresFiles()
        {
            try
            {
                var installLocation = Package.Current.InstalledLocation;
                var QuakeMainFiles = (StorageFile)await installLocation.TryGetItemAsync("lib49.dat");
                if (QuakeMainFiles != null)
                {
#if USING_NATIVEARCHIVE
                                PackageArchiveHelper packageArchiveHelper = new PackageArchiveHelper();
                                await packageArchiveHelper.ExtractFiles(QuakeFilesFolder, QuakeMainFiles);
#else
                    Stream zipStream = await QuakeMainFiles.OpenStreamForReadAsync();
                    using (var zipArchive = ArchiveFactory.Open(zipStream))
                    {
                        //It should support 7z, zip, rar, gz, tar
                        var reader = zipArchive.ExtractAllEntries();

                        //Bind progress event
                        reader.EntryExtractionProgress += (sender, e) =>
                        {
                            var entryProgress = e.ReaderProgress.PercentageReadExact;
                            var sizeProgress = e.ReaderProgress.BytesTransferred.ToFileSize();
                            ShowLoader(true, $"Syncing Files..\nOther Cores ({Math.Round(entryProgress)}%)\n{System.IO.Path.GetFileName(e.Item.Key)}");
                        };

                        //Extract files
                        while (reader.MoveToNextEntry())
                        {
                            if (!reader.Entry.IsDirectory)
                            {
                                await reader.WriteEntryToDirectory(ApplicationData.Current.LocalFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                            }
                        }
                    }
#endif
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task SyncQuakeFiles()
        {
            try
            {
                ShowLoader(true, "Syncing Files..\nQuake Core");
                var QuakeSystemFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("TyrQuake - System");
                if (QuakeSystemFolder == null)
                {
                    QuakeSystemFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("TyrQuake - System");
                }

                if (QuakeSystemFolder != null)
                {
                    var QuakeFilesFolder = (StorageFolder)await QuakeSystemFolder.TryGetItemAsync("quake");
                    if (QuakeFilesFolder == null)
                    {
                        var installLocation = Package.Current.InstalledLocation;
                        QuakeFilesFolder = await QuakeSystemFolder.CreateFolderAsync("quake");
                        if (QuakeFilesFolder != null)
                        {
                            var QuakeMainFiles = (StorageFile)await installLocation.TryGetItemAsync("lib5.dat");
                            if (QuakeMainFiles != null)
                            {
#if USING_NATIVEARCHIVE
                                PackageArchiveHelper packageArchiveHelper = new PackageArchiveHelper();
                                await packageArchiveHelper.ExtractFiles(QuakeFilesFolder, QuakeMainFiles);
#else
                                Stream zipStream = await QuakeMainFiles.OpenStreamForReadAsync();
                                using (var zipArchive = ArchiveFactory.Open(zipStream))
                                {
                                    //It should support 7z, zip, rar, gz, tar
                                    var reader = zipArchive.ExtractAllEntries();

                                    //Bind progress event
                                    reader.EntryExtractionProgress += (sender, e) =>
                                    {
                                        var entryProgress = e.ReaderProgress.PercentageReadExact;
                                        var sizeProgress = e.ReaderProgress.BytesTransferred.ToFileSize();
                                        ShowLoader(true, $"Syncing Files..\nQuake Core ({Math.Round(entryProgress)}%)\n{System.IO.Path.GetFileName(e.Item.Key)}");
                                    };

                                    //Extract files
                                    while (reader.MoveToNextEntry())
                                    {
                                        if (!reader.Entry.IsDirectory)
                                        {
                                            await reader.WriteEntryToDirectory(QuakeFilesFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                        }
                                    }
                                }
#endif
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private async Task<bool> IsCaveStoryFileSyncRequired()
        {
            try
            {
                var mainPath = $@"Assets\Libraries\nxengine_libretro";
                var testFile = await Package.Current.InstalledLocation.TryGetItemAsync($"{mainPath}.dll");
                if (testFile == null)
                {
                    testFile = await Package.Current.InstalledLocation.TryGetItemAsync($"{mainPath}.dat");
                    if (testFile == null)
                    {
                        var pureCoreFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("PureCore");
                        if (pureCoreFolder != null)
                        {
                            testFile = await pureCoreFolder.TryGetItemAsync("nxengine_libretro.dll");
                            if (testFile == null)
                            {
                                testFile = await pureCoreFolder.TryGetItemAsync("nxengine_libretro.dat");
                                if (testFile == null)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                var CaveStorySystemFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("NXEngine - System");
                if (CaveStorySystemFolder == null)
                {
                    return true;
                }

                var CaveStoryFilesFolder = (StorageFolder)await CaveStorySystemFolder.TryGetItemAsync("Cave Story");
                if (CaveStoryFilesFolder == null)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }
        private async Task SyncCaveStoryFiles()
        {
            try
            {
                ShowLoader(true, "Syncing Files..\nCave Story Core");
                var CaveStorySystemFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("NXEngine - System");
                if (CaveStorySystemFolder == null)
                {
                    CaveStorySystemFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("NXEngine - System");
                }

                if (CaveStorySystemFolder != null)
                {
                    var CaveStoryFilesFolder = (StorageFolder)await CaveStorySystemFolder.TryGetItemAsync("Cave Story");
                    if (CaveStoryFilesFolder == null)
                    {
                        var installLocation = Package.Current.InstalledLocation;
                        CaveStoryFilesFolder = await CaveStorySystemFolder.CreateFolderAsync("Cave Story");
                        if (CaveStoryFilesFolder != null)
                        {
                            var CaveStoryMainFiles = (StorageFile)await installLocation.TryGetItemAsync("lib4.dat");
                            if (CaveStoryMainFiles != null)
                            {
#if USING_NATIVEARCHIVE
                                PackageArchiveHelper packageArchiveHelper = new PackageArchiveHelper();
                                await packageArchiveHelper.ExtractFiles(CaveStoryFilesFolder, CaveStoryMainFiles);
#else
                                Stream zipStream = await CaveStoryMainFiles.OpenStreamForReadAsync();
                                using (var zipArchive = ArchiveFactory.Open(zipStream))
                                {
                                    //It should support 7z, zip, rar, gz, tar
                                    var reader = zipArchive.ExtractAllEntries();

                                    //Bind progress event
                                    reader.EntryExtractionProgress += (sender, e) =>
                                    {
                                        var entryProgress = e.ReaderProgress.PercentageReadExact;
                                        var sizeProgress = e.ReaderProgress.BytesTransferred.ToFileSize();
                                        ShowLoader(true, $"Syncing Files..\nCave Story Core ({Math.Round(entryProgress)}%)\n{System.IO.Path.GetFileName(e.Item.Key)}");
                                    };

                                    //Extract files
                                    while (reader.MoveToNextEntry())
                                    {
                                        if (!reader.Entry.IsDirectory)
                                        {
                                            await reader.WriteEntryToDirectory(CaveStoryFilesFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                        }
                                    }
                                }
#endif
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private async Task<bool> IsScummVMFileSyncRequired()
        {
            try
            {
                var mainPath = $@"Assets\Libraries\scummvm_libretro";
                var testFile = await Package.Current.InstalledLocation.TryGetItemAsync($"{mainPath}.dll");
                if (testFile == null)
                {
                    testFile = await Package.Current.InstalledLocation.TryGetItemAsync($"{mainPath}.dat");
                    if (testFile == null)
                    {
                        var pureCoreFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("PureCore");
                        if (pureCoreFolder != null)
                        {
                            testFile = await pureCoreFolder.TryGetItemAsync("scummvm_libretro.dll");
                            if (testFile == null)
                            {
                                testFile = await pureCoreFolder.TryGetItemAsync("scummvm_libretro.dat");
                                if (testFile == null)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                var scummVMSystemFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("scummvm - System");
                if (scummVMSystemFolder == null)
                {
                    return true;
                }

                //Check if user selected any custom location
                var testSystemCustomFolder = await PlatformService.GetCustomFolder("scummvm", "system");
                if (testSystemCustomFolder != null)
                {
                    scummVMSystemFolder = testSystemCustomFolder;
                }

                var scummVMFilesFolder = (StorageFolder)await scummVMSystemFolder.TryGetItemAsync("scummvm");
                if (scummVMFilesFolder == null)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }
        private async Task SyncScummVMFiles()
        {
            try
            {
                ShowLoader(true, "Syncing Files..\nScummVM Core");
                var scummVMSystemFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("scummvm - System");
                if (scummVMSystemFolder == null)
                {
                    scummVMSystemFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("scummvm - System");
                }

                //Check if user selected any custom location
                var testSystemCustomFolder = await PlatformService.GetCustomFolder("scummvm", "system");
                if (testSystemCustomFolder != null)
                {
                    scummVMSystemFolder = testSystemCustomFolder;
                }

                if (scummVMSystemFolder != null)
                {
                    var scummVMFilesFolder = (StorageFolder)await scummVMSystemFolder.TryGetItemAsync("scummvm");
                    if (scummVMFilesFolder == null)
                    {
                        var installLocation = Package.Current.InstalledLocation;
                        scummVMFilesFolder = await scummVMSystemFolder.CreateFolderAsync("scummvm");
                        if (scummVMFilesFolder != null)
                        {
                            var scummVMMainFiles = (StorageFile)await installLocation.TryGetItemAsync("lib3.dat");
                            if (scummVMMainFiles != null)
                            {
#if USING_NATIVEARCHIVE
                                PackageArchiveHelper packageArchiveHelper = new PackageArchiveHelper();
                                await packageArchiveHelper.ExtractFiles(scummVMFilesFolder, scummVMMainFiles);
#else
                                Stream zipStream = await scummVMMainFiles.OpenStreamForReadAsync();
                                using (var zipArchive = ArchiveFactory.Open(zipStream))
                                {
                                    //It should support 7z, zip, rar, gz, tar
                                    var reader = zipArchive.ExtractAllEntries();

                                    //Bind progress event
                                    reader.EntryExtractionProgress += (sender, e) =>
                                    {
                                        var entryProgress = e.ReaderProgress.PercentageReadExact;
                                        var sizeProgress = e.ReaderProgress.BytesTransferred.ToFileSize();
                                        ShowLoader(true, $"Syncing Files..\nScummVM Core ({Math.Round(entryProgress)}%)\n{System.IO.Path.GetFileName(e.Item.Key)}");
                                    };

                                    //Extract files
                                    while (reader.MoveToNextEntry())
                                    {
                                        if (!reader.Entry.IsDirectory)
                                        {
                                            await reader.WriteEntryToDirectory(scummVMFilesFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                            //WriteEntryToDirectory can extended to:
                                            //WriteEntryToDirectory(folder, options, cancellationTokenSource)
                                            //                                       ^^^^^^^^^^^^^^^^^^^^^^^
                                            //it will help to terminate the current entry directly if cancellation requested
                                        }
                                    }
                                }
#endif
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        private async Task SyncCoresFiles()
        {
            try
            {
                var copyState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("CoresData", false);
                if (!copyState)
                {
                    WriteLog($"Extra Contents not synced yet..");
                    await SyncExtraCoresFiles();

                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("CoresData", true);
                }
                else
                {
                    WriteLog($"Extra Contents already synced.");
                }
            }
            catch (Exception ex)
            {

            }
            /*await SyncCoreFiles(coreName: "DeSmuME", rootName: "data1", filesList: new string[] { "desmume.ddb", "desmume.ini" }, "desmume_libretro.dll");
            await SyncCoreFiles(coreName: "xrick", rootName: "data2", filesList: new string[] { "data.zip" }, "xrick_libretro.dll");
            await SyncCoreFiles(coreName: "PrBoom", rootName: "data3", filesList: new string[] { "prboom.wad", "doom1.wad" }, "prboom_libretro.dll");*/
        }
        private async Task SyncCoreFiles(string coreName, string rootName, string[] filesList, string dll)
        {
            try
            {
                var testFile = await Package.Current.InstalledLocation.TryGetItemAsync(dll);
                if (testFile == null)
                {
                    var mainPath = $@"Assets\Libraries\{dll}";
                    testFile = await Package.Current.InstalledLocation.TryGetItemAsync(mainPath);
                    if (testFile == null)
                    {
                        var pureCoreFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("PureCore");
                        if (pureCoreFolder != null)
                        {
                            testFile = await pureCoreFolder.TryGetItemAsync(dll);
                            if (testFile == null)
                            {
                                testFile = await pureCoreFolder.TryGetItemAsync(dll.Replace(".dll", ".dat"));
                                if (testFile == null)
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }



                WriteLog($"Syncing Files for {coreName} Core");
                ShowLoader(true, $"Syncing Files..\n{coreName} Core");
                var SystemFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync($"{coreName} - System");
                if (SystemFolder == null)
                {
                    SystemFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync($"{coreName} - System");
                }
                //Check if user selected any custom location
                var testSystemCustomFolder = await PlatformService.GetCustomFolder(coreName, "system");
                if (testSystemCustomFolder != null)
                {
                    SystemFolder = testSystemCustomFolder;
                }
                if (SystemFolder != null)
                {
                    var installLocation = Package.Current.InstalledLocation;
                    foreach (var file in filesList)
                    {
                        var filesTest = (StorageFile)await SystemFolder.TryGetItemAsync(file);
                        if (filesTest == null)
                        {
                            var sourceFile = (StorageFile)await installLocation.TryGetItemAsync($"{rootName}\\{file}");
                            if (sourceFile != null)
                            {
                                await sourceFile.CopyAsync(SystemFolder);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        #endregion

        #region Logs
        void checkLogsState()
        {
            try
            {
                LogFileState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("LogFileState", false);
                logToFile.IsChecked = LogFileState;
                VFSFileState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("VFSFileState", false);
                VFSLog.IsChecked = VFSFileState;

                EnabledDebugLogsListUpdate = true;
                EnabledDebugLogsList = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("EnabledDebugLogsList", false);
                DebugLogList.IsChecked = EnabledDebugLogsList;
            }
            catch (Exception ex)
            {

            }
        }
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
            }
        }
        private void logToFile_Click(object sender, RoutedEventArgs e)
        {
            LogFileState = logToFile.IsChecked.Value;
        }

        private async void downloadLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var LogLocation = await VM.SelectedSystem.Core.GetLogFile(false, VM.SelectedSystem);
                if (LogLocation != null)
                {
                    FolderPicker folderPicker = new FolderPicker();
                    folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                    folderPicker.FileTypeFilter.Add(".txt");
                    var saveFolder = await folderPicker.PickSingleFolderAsync();
                    if (saveFolder != null)
                    {
                        VM.SystemCoreIsLoadingState(true, "Saving logs files..");
                        await LogLocation.CopyAsync(saveFolder, LogLocation.Name, NameCollisionOption.GenerateUniqueName);

                        //Copy VFS Logs if exists
                        var vfsFile = await VM.SelectedSystem.Core.GetLogFile(true, VM.SelectedSystem);
                        if (vfsFile != null)
                        {
                            await vfsFile.CopyAsync(saveFolder, vfsFile.Name, NameCollisionOption.GenerateUniqueName);
                        }
                        PlatformService.PlayNotificationSoundDirect("success");
                        pushNormalNotification($"Logs file saved sccussfully");
                    }
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("error");
                    pushNormalNotification("Unable to get log location!");
                }
                VM.SystemCoreIsLoadingState(false);
            }
            catch (Exception ex)
            {
                VM?.SystemCoreIsLoadingState(false);
                PlatformService.PlayNotificationSoundDirect("error");
                pushNormalNotification(ex.Message);
            }
        }

        private async void resetLogFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmReset = new ConfirmConfig();
                confirmReset.SetTitle("Reset logs");
                confirmReset.SetMessage($"Do you want to reset logs file for {VM.SelectedSystem.Name}? ");
                confirmReset.UseYesNo();

                var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                if (StartReset)
                {
                    VM?.SystemCoreIsLoadingState(true, "Reseting logs files..");
                    var LogLocation = await VM.SelectedSystem.Core.GetLogFile(false, VM.SelectedSystem);
                    if (LogLocation != null)
                    {
                        await FileIO.WriteTextAsync(LogLocation, "");

                        //Reset VFS if exists
                        var LogVFSLocation = await VM.SelectedSystem.Core.GetLogFile(true, VM.SelectedSystem);
                        if (LogVFSLocation != null)
                        {
                            await FileIO.WriteTextAsync(LogVFSLocation, "");
                        }

                        PlatformService.PlayNotificationSoundDirect("success");
                        pushNormalNotification($"Reset logs file done");
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        pushNormalNotification("Unable to get log location!");
                    }
                }
                VM?.SystemCoreIsLoadingState(false);
            }
            catch (Exception ex)
            {
                VM?.SystemCoreIsLoadingState(false);
                PlatformService.PlayNotificationSoundDirect("error");
                pushNormalNotification(ex.Message);
            }
        }

        private void VFSLog_Click(object sender, RoutedEventArgs e)
        {
            VFSFileState = VFSLog.IsChecked.Value;
        }

        private bool EnabledDebugLogsListUpdate = false;
        private bool enabledDebugLogsList = false;
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
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("EnabledDebugLogsList", EnabledDebugLogsList);
                    if (EnabledDebugLogsList && !EnabledDebugLogsListUpdate)
                    {
                        PlatformService.PlayNotificationSoundDirect("root-needed");
                        UserDialogs.Instance.AlertAsync($"Debug log could cause heavy performance\nTurn it off when it's not important", "Core Debug");
                    }
                    EnabledDebugLogsListUpdate = false;
                }
                catch (Exception ex)
                {
                    PlatformService.ShowErrorMessageDirect(ex);
                }
            }
        }
        private void DebugLogList_Click(object sender, RoutedEventArgs e)
        {
            EnabledDebugLogsList = DebugLogList.IsChecked.Value;
        }
        #endregion

        private async void ShowExit()
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmExit = new ConfirmConfig();
                confirmExit.SetTitle("Exit");
                confirmExit.SetMessage("Do you want to exit?");
                confirmExit.UseYesNo();

                var confirmExitState = await UserDialogs.Instance.ConfirmAsync(confirmExit);
                if (confirmExitState)
                {
                    CoreApplication.Exit();
                }
            }
            catch
            {
                try
                {
                    PlatformService.PlayNotificationSoundDirect("alert");
                    var messageDialog = new MessageDialog("Do you want to exit?");
                    messageDialog.Commands.Add(new UICommand("Exit", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                    messageDialog.Commands.Add(new UICommand("Dismiss"));
                    await messageDialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    CoreApplication.Exit();
                }
            }
        }
        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            ShowExit();
        }

        string forceFilter
        {
            get
            {
                return PlatformService.ForceFilter;
            }
            set
            {
                PlatformService.ForceFilter = value;
            }
        }

        public static async void Shortcuts(GamePlayerViewModel VM, bool inGame = false)
        {
            try
            {
                PlatformService.ShortcutsVisible = true;
                ContentDialog contentDialog = new ContentDialog();
                contentDialog.Title = "RetriXGold Shortcuts";
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
                keysLabel.Text = "Keys:";
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
                ScrollViewer scrollViewer = new ScrollViewer();
                scrollViewer.Content = ConfigBlock;
                scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.MaxHeight = 300;
                scrollViewer.Margin = new Thickness(3, 7, 0, 7);
                scrollViewer.BorderThickness = new Thickness(0, 1, 0, 1);
                scrollViewer.BorderBrush = new SolidColorBrush(Colors.Gray);
                comboBox.SelectionChanged += (sender, e) =>
                {
                    try
                    {
                        var item = (ComboBoxItem)e.AddedItems?.FirstOrDefault();
                        if (item != null)
                        {
                            var targetItem = (ShortcutEntry)item.Tag;
                            KeysInfo.Visibility = Visibility.Visible;
                            ConfigBlock.Children.Clear();
                            ConfigBlock.Visibility = Visibility.Visible;
                            List<CheckBox> keys = new List<CheckBox>();
                            foreach (var gItem in PlatformService.GamePadStringToVirtual)
                            {
                                CheckBox checkBox = new CheckBox();
                                checkBox.Content = gItem.Key;
                                if (targetItem.keys.Contains(gItem.Key))
                                {
                                    checkBox.IsChecked = true;
                                }
                                checkBox.Click += (senderc, ec) =>
                                {
                                    targetItem.keys.Clear();
                                    foreach (var cItem in keys)
                                    {
                                        if (cItem.IsChecked.Value)
                                        {
                                            targetItem.keys.Add(cItem.Content.ToString());
                                        }
                                    }
                                    if (targetItem.keys.Count == 0)
                                    {
                                        textBlock.Text = "None";
                                    }
                                    else
                                    {
                                        textBlock.Text = String.Join(" + ", targetItem.keys);
                                    }
                                };
                                keys.Add(checkBox);
                                ConfigBlock.Children.Add(checkBox);
                            }
                            if (targetItem.keys.Count == 0)
                            {
                                textBlock.Text = "None";
                            }
                            else
                            {
                                textBlock.Text = String.Join(" + ", targetItem.keys);
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
                bool fastForwardFound = false;
                if (PlatformService.shortcutEntries.Count == 12)
                {
                    PlatformService.shortcutEntries.Insert(9, new ShortcutEntry(new List<string>(), "Fast Forward", "Fast Forward", "fforward"));
                }
                foreach (var sItem in PlatformService.shortcutEntries)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = sItem.name;
                    comboBoxItem.Tag = sItem;
                    comboBox.Items.Add(comboBoxItem);

                    if (sItem.key.Equals("xboxmenu"))
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

                Button button = new Button();
                button.HorizontalAlignment = HorizontalAlignment.Stretch;
                button.Content = "Keyboard Map";
                button.Click += (s, e) =>
                {
                    try
                    {
                        contentDialog.Hide();
                        GamePlayerView.KeyboardRules(VM, !inGame);
                    }
                    catch (Exception ex)
                    {

                    }
                };

                stackPanel.Children.Add(comboBox);
                stackPanel.Children.Add(KeysInfo);
                stackPanel.Children.Add(scrollViewer);
                if (InputService.isKeyboardAvailable() && inGame)
                {
                    stackPanel.Children.Add(button);
                }
                stackPanel.Children.Add(notice);
                contentDialog.Content = stackPanel;
                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    saveShortcuts();
                    if (inGame)
                    {
                        PlatformService.ShowNotificationDirect("Shortcuts saved", 2);
                    }
                    else
                    {
                        PlatformService.ShowNotificationMain("Shortcuts saved", 2);
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
            PlatformService.ShortcutsVisible = false;
        }
        public static async void saveShortcuts()
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync("RetriXGold Shortcuts", CreationCollisionOption.OpenIfExists);
                if (localFolder == null)
                {
                    PlatformService.ShowErrorMessage(new Exception("Unable to create shortcuts folder!"));
                    return;
                }
                var targetFile = await localFolder.CreateFileAsync($"goldshortcuts.rxs", CreationCollisionOption.ReplaceExisting);

                Encoding unicode = Encoding.Unicode;

                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(PlatformService.shortcutEntries));
                using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var outStream = stream.AsStream();
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }
        public static async void loadShortcuts()
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync("RetriXGold Shortcuts", CreationCollisionOption.OpenIfExists);
                if (localFolder == null)
                {
                    PlatformService.ShowErrorMessage(new Exception("Unable to create shortcuts folder!"));
                    return;
                }
                var targetFile = (StorageFile)await localFolder.TryGetItemAsync($"goldshortcuts.rxs");
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
                    var dictionaryList = JsonConvert.DeserializeObject<List<ShortcutEntry>>(OptionsFileContent);
                    PlatformService.shortcutEntries = dictionaryList;
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        public static bool encoderInProgress = false;
        public static bool GetTranscodedImage(string path, ref string output)
        {
            output = path;
            bool state = true;
            try
            {
                if (path.Contains(":///") && path.Contains(".png"))
                {
                    var fileName = path.Replace("ms-appx:///", "").Replace("/", "\\").Replace(".png", ".bmp");
                    var targetPath = Package.Current.InstalledLocation.Path;
                    var filePath = $"{targetPath}\\{fileName}";
                    if (System.IO.File.Exists(filePath))
                    {
                        output = filePath;
                        state = true;
                    }
                }
                else
                {
                    var fileName = path.Replace(".png", ".bmp");
                    if (System.IO.File.Exists(fileName))
                    {
                        output = fileName;
                        state = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return state;
        }
        public async Task TranscodeProjectResources()
        {
            try
            {
                QueryOptions queryOptions = new QueryOptions();
                queryOptions.FolderDepth = FolderDepth.Deep;
                var files = Package.Current.InstalledLocation.CreateFileQueryWithOptions(queryOptions);
                var AllFiles = await files.GetFilesAsync();
                string[] forConvert = new string[] { ".png" };

                foreach (var fItem in AllFiles)
                {
                    try
                    {
                        var imgExt = System.IO.Path.GetExtension(fItem.Path).ToLower();
                        //Require MagickImage.NET in Debug mode
                        /*if (forConvert.Contains(imgExt))
                        {
                            var fileName = System.IO.Path.GetFileNameWithoutExtension(fItem.Path);
                            var bmpFile = $"{fileName}.bmp";
                            var parentFolder = await fItem.GetParentAsync();
                            if (parentFolder != null)
                            {
                                StorageFile file = await parentFolder.CreateFileAsync(bmpFile, CreationCollisionOption.ReplaceExisting);
                                using (var image = new ImageMagick.MagickImage(await fItem.OpenStreamForReadAsync(), MagickFormat.Png))
                                {
                                    await image.WriteAsync(await file.OpenStreamForWriteAsync(), MagickFormat.Bmp);
                                }
                            }
                        }*/
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
        public static async Task<string> ConvertPngToBmp(string path)
        {
            string imagePath = path;

            try
            {
                string[] notForConvert = new string[] { ".gif" };
                var imgExt = System.IO.Path.GetExtension(path).ToLower();
                if (notForConvert.Contains(imgExt))
                {
                    return imagePath;
                }

            }
            catch (Exception ex)
            {

            }
            var tempString = "";
            if (GetTranscodedImage(path, ref tempString))
            {
                return imagePath;
            }
            while (encoderInProgress)
            {
                await Task.Delay(50);
            }

            //Check again in case the image transcoded by other process while waiting
            if (GetTranscodedImage(path, ref tempString))
            {
                return imagePath;
            }
            try
            {
                encoderInProgress = true;
                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                var bmpFile = $"{fileName}.bmp";
                var bmpFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("TranscodedImages", CreationCollisionOption.OpenIfExists);
                var testFile = await bmpFolder.TryGetItemAsync(bmpFile);
                if (testFile == null)
                {
                    StorageFile sourceImageFile = null;
                    if (path.Contains(":///"))
                    {
                        sourceImageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
                    }
                    else
                    {
                        sourceImageFile = await StorageFile.GetFileFromPathAsync(path);
                    }
                    if (sourceImageFile != null)
                    {
                        StorageFile file = await bmpFolder.CreateFileAsync(bmpFile, CreationCollisionOption.ReplaceExisting);
                        /*using (var image = new ImageMagick.MagickImage(await sourceImageFile.OpenStreamForReadAsync(), MagickFormat.Png))
                        {
                            await image.WriteAsync(await file.OpenStreamForWriteAsync(), MagickFormat.Bmp);
                        }*/

                        var fileInfo = await file.GetBasicPropertiesAsync();
                        if (fileInfo.Size > 0)
                        {
                            imagePath = file.Path;
                        }
                        else
                        {
                            await file.DeleteAsync();
                        }

                        /*Stream pixelBufferStream = await sourceImageFile.OpenStreamForReadAsync();
                        BitmapDecoder dec = await BitmapDecoder.CreateAsync(pixelBufferStream.AsRandomAccessStream());
                        using (IRandomAccessStream outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            try
                            {
                                var pixels = await dec.GetPixelDataAsync();

                                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, outputStream);
                                encoder.SetPixelData(
                                BitmapPixelFormat.Bgra8,
                                BitmapAlphaMode.Ignore,
                                    (uint)dec.PixelWidth,
                                    (uint)dec.PixelHeight,
                                    dpiX: 96,
                                    dpiY: 96,
                                    pixels: pixels.DetachPixelData());
                                await encoder.FlushAsync();

                                //Verify the file
                                var fileInfo = await file.GetBasicPropertiesAsync();
                                if (fileInfo.Size > 0)
                                {
                                    imagePath = file.Path;
                                }
                                else
                                {
                                    await file.DeleteAsync();
                                }
                                encoder = null;

                            }
                            catch (Exception ex)
                            {
                                await file.DeleteAsync();
                            }
                            try
                            {
                                outputStream.Dispose();
                                GC.SuppressFinalize(outputStream);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        try
                        {
                            pixelBufferStream.Dispose();
                            GC.SuppressFinalize(pixelBufferStream);
                            dec = null;
                        }
                        catch (Exception ex)
                        {

                        }*/
                    }
                }
                else
                {
                    imagePath = testFile.Path;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    System.IO.File.AppendAllText(logfileLocation, $"Transcoder error: {ex.Message}\n");
                }
                catch (Exception e)
                {

                }
            }
            encoderInProgress = false;
            return imagePath;
        }
        private async void RetriXGoldSettings()
        {
            try
            {
                List<RetriXSettingsItem> settingsList = new List<RetriXSettingsItem>();
                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "Sound Effects",
                    desc: "Turn on/off sound effects",
                    icon: "opus",
                    preferencesKey: "MuteSFX",
                    defaultValueKey: "MuteSFX",
                    onReset: false,
                    true, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "Colored Dialog",
                    desc: "Add color to dialog text",
                    icon: "opus",
                    preferencesKey: "UseColoredDialog",
                    defaultValueKey: "UseColoredDialog",
                    onReset: true,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "Extra Confirm",
                    desc: "Show extra confirmation dialogs",
                    icon: "feedback",
                    preferencesKey: "ExtraConfirmation",
                    defaultValueKey: "ExtraConfirmation",
                    onReset: true,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Behavior",
                    name: "Extra Delay",
                    desc: "Less crashs, more load time",
                    icon: "feedback",
                    preferencesKey: "ExtraDelay",
                    defaultValueKey: "ExtraDelay",
                    onReset: false,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Behavior",
                    name: "Detect Inputs",
                    desc: "Disable features based on inputs",
                    icon: "feedback",
                    preferencesKey: "DetectInputs",
                    defaultValueKey: "DetectInputs",
                    onReset: false,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Behavior",
                    name: "Native Keyboard",
                    desc: "When supported, use keyboard keys as is\notherwise send gamepad mapped key",
                    icon: "feedback",
                    preferencesKey: "useNativeKeyboardByDefault",
                    defaultValueKey: "useNativeKeyboardByDefault",
                    onReset: false,
                    false, typeof(PlatformService));
                
                new RetriXSettingsItem(ref settingsList,
                    group: "Behavior",
                    name: "Prevent GC",
                    desc: "Always try to prevent GC\nTurn it off in case of slow UI",
                    icon: "feedback",
                    preferencesKey: "PreventGCAlways",
                    defaultValueKey: "PreventGCAlways",
                    onReset: false,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Emulation",
                    name: "Resolve VFS",
                    desc: "Auto try without VFS when failed",
                    icon: "open",
                    preferencesKey: "AutoResolveVFSIssues",
                    defaultValueKey: "AutoResolveVFSIssues",
                    onReset: false,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Emulation",
                    name: "VFS Indicator",
                    desc: "Show VFS indicator at top-left corner",
                    icon: "remote",
                    preferencesKey: "ShowIndicators",
                    defaultValueKey: "ShowIndicators",
                    onReset: true,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "Thumbnails & Icons",
                    desc: "Show/hide thumbnails and icons",
                    icon: "remote",
                    preferencesKey: "ShowThumbNailsIcons",
                    defaultValueKey: "ShowThumbNailsIcons",
                    onReset: true,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Emulation",
                    name: "Safe Render",
                    desc: "Use safe render method",
                    icon: "remote",
                    preferencesKey: "SafeRender",
                    defaultValueKey: "SafeRender",
                    onReset: false,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Emulation",
                    name: "Old Render",
                    desc: "Force old render method (slow)",
                    icon: "remote",
                    preferencesKey: "ForceOldBufferMethods",
                    defaultValueKey: "ForceOldBufferMethods",
                    onReset: false,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Content",
                    name: "Screenshots",
                    desc: "Show screenshots in core's page",
                    icon: "remote",
                    preferencesKey: "ShowScreeshots",
                    defaultValueKey: "ShowScreeshots",
                    onReset: false,
                    false, typeof(PlatformService));


                new RetriXSettingsItem(ref settingsList,
                    group: "Content",
                    name: "Windows Indexer",
                    desc: "Use windows indexer for fast results",
                    icon: "search",
                    preferencesKey: "UseWindowsIndexer",
                    defaultValueKey: "UseWindowsIndexer",
                    onReset: true,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Content",
                    name: "Sub Folders",
                    desc: "Search in sub folders",
                    icon: "search",
                    preferencesKey: "UseWindowsIndexerSubFolders",
                    defaultValueKey: "UseWindowsIndexerSubFolders",
                    onReset: true,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Content",
                    name: "RetroPass Root",
                    desc: "Search in root if rom not found",
                    icon: "search",
                    preferencesKey: "RetroPassRoot",
                    defaultValueKey: "RetroPassRoot",
                    onReset: false,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "Emulation",
                    name: "AutoFit Resolver",
                    desc: "Fit the output by screen size",
                    icon: "remote",
                    preferencesKey: "AutoFitResolver",
                    defaultValueKey: "AutoFitResolver",
                    onReset: true,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "Lists Height",
                    desc: "Adjust In-Game lists height",
                    icon: "remote",
                    preferencesKey: "AdjustInGameLists",
                    defaultValueKey: "AdjustInGameLists",
                    onReset: true,
                    false, typeof(PlatformService));

                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "FullScreen",
                    desc: "Turn on/off fullscreen",
                    icon: "scale",
                    preferencesKey: "FullScreen",
                    defaultValueKey: "FullScreen",
                    onReset: false,
                    false, typeof(PlatformService),
                    (s, e) =>
                    {
                        try
                        {
                            var state = (bool)s;
                            if (state)
                            {
                                if (!AppView.IsFullScreenMode)
                                {
                                    AppView.TryEnterFullScreenMode();
                                }
                            }
                            else
                            {
                                if (AppView.IsFullScreenMode)
                                {
                                    AppView.ExitFullScreenMode();
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    });

                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "GoBack Handle",
                    desc: "Handle goback request",
                    icon: "any",
                    preferencesKey: "HandleBackPress",
                    defaultValueKey: "HandleBackPress",
                    onReset: true,
                    false, typeof(App));

                new RetriXSettingsItem(ref settingsList,
                    group: "General",
                    name: "Clear Temp",
                    desc: "Clean temp files at startup",
                    icon: "web-cache",
                    preferencesKey: "CleanTempOnStartup",
                    defaultValueKey: "CleanTempOnStartup",
                    onReset: true,
                    false, typeof(App));

                if (PlatformService.DeviceIsPhone())
                {
                    new RetriXSettingsItem(ref settingsList,
                        group: "Mobile",
                        name: "Status Bar",
                        desc: "Show/Hide status bar",
                        icon: "web-cache",
                        preferencesKey: "MobileStatusBar",
                        defaultValueKey: "MobileStatusBar",
                        onReset: false,
                        false, typeof(PlatformService),
                        (s, e) =>
                        {
                            try
                            {
                                var state = (bool)s;
                                StatusBar statusBar = StatusBar.GetForCurrentView();

                                if (state)
                                {
                                    _ = statusBar.ShowAsync();
                                }
                                else
                                {
                                    _ = statusBar.HideAsync();
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                }
                ContentDialog contentDialog = new ContentDialog();
                contentDialog.Title = "RetriXGold Settings";
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
                    settingsContainer.Children.Add(sItem.SettingSwitchItem);
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

                Button ShowUIScaleDialog = new Button();
                ShowUIScaleDialog.Content = "UI Scale (Testing)";
                ShowUIScaleDialog.Margin = new Thickness(0, 0, 0, 5);
                ShowUIScaleDialog.HorizontalAlignment = HorizontalAlignment.Stretch;
                ShowUIScaleDialog.Click += (sender, e) =>
                {
                    try
                    {
                        contentDialog.Hide();
                        GamePlayerView.LayoutScaler();
                    }
                    catch (Exception ex)
                    {

                    }
                };

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
                stackPanel.Children.Add(ShowUIScaleDialog);
                stackPanel.Children.Add(border1);
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
                    RetriXGoldSettings();
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        private async void CoresFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var coresManfs = new List<string>();
                foreach (var system in PlatformService.Consoles)
                {
                    var manf = system.ManufacturerTemp;
                    if (system.ManufacturerTemp.Length == 0)
                    {
                        manf = "AnyCore";
                    }
                    coresManfs.Add(manf);
                }
                var coresManfsCleaned = coresManfs.Distinct().OrderBy(i => i);
                try
                {
                    ContentDialog contentDialog = new ContentDialog();
                    contentDialog.Title = "Cores Filter";
                    contentDialog.PrimaryButtonText = "Filter";
                    contentDialog.SecondaryButtonText = "Close";
                    contentDialog.IsPrimaryButtonEnabled = true;
                    contentDialog.IsSecondaryButtonEnabled = true;
                    ScrollViewer scrollViewer = new ScrollViewer();
                    ListView listView = new ListView();
                    ComboBox comboBox = new ComboBox();
                    bool isXBOX = PlatformService.isXBOX;
                    if (isXBOX)
                    {
                        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                        ComboBoxItem comboViewItemDefault = new ComboBoxItem();
                        comboViewItemDefault.Content = "Display All";
                        comboViewItemDefault.Tag = "";
                        comboBox.Items.Add(comboViewItemDefault);
                        comboBox.SelectedItem = comboViewItemDefault;
                        foreach (var manf in coresManfsCleaned)
                        {
                            ComboBoxItem comboViewItem = new ComboBoxItem();
                            comboViewItem.Content = manf;
                            comboViewItem.Tag = manf;

                            comboBox.Items.Add(comboViewItem);
                            if (forceFilter.Length > 0)
                            {
                                if (forceFilter.Equals(manf))
                                {
                                    comboBox.SelectedItem = comboViewItem;
                                }
                            }
                        }
                        if (forceFilter.Length == 0)
                        {
                            comboBox.SelectedIndex = 0;
                        }
                        scrollViewer.Content = comboBox;
                    }
                    else
                    {
                        listView.HorizontalAlignment = HorizontalAlignment.Stretch;
                        listView.VerticalAlignment = VerticalAlignment.Stretch;

                        ListViewItem listViewItemDefault = new ListViewItem();
                        listViewItemDefault.Content = "Display All";
                        listViewItemDefault.Tag = "";
                        listView.Items.Add(listViewItemDefault);
                        foreach (var manf in coresManfsCleaned)
                        {
                            ListViewItem listViewItem = new ListViewItem();
                            listViewItem.Content = manf;
                            listViewItem.Tag = manf;

                            listView.Items.Add(listViewItem);
                            if (forceFilter.Length > 0)
                            {
                                if (forceFilter.Equals(manf))
                                {
                                    listView.SelectedItem = listViewItem;
                                    listView.ScrollIntoView(listViewItem, ScrollIntoViewAlignment.Leading);
                                }
                            }

                        }
                        if (forceFilter.Length == 0)
                        {
                            listView.SelectedIndex = 0;
                        }

                        scrollViewer.Content = listView;
                    }
                    scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                    scrollViewer.VerticalAlignment = VerticalAlignment.Stretch;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    contentDialog.Content = scrollViewer;
                    var result = await contentDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        if (isXBOX)
                        {
                            if (comboBox.SelectedItem != null)
                            {
                                var selectedSystem = (string)((ComboBoxItem)comboBox.SelectedItem).Tag;
                                forceFilter = selectedSystem;
                            }
                            else
                            {
                                forceFilter = "";
                            }
                        }
                        else
                        {
                            if (listView.SelectedItem != null)
                            {
                                var selectedSystem = (string)((ListViewItem)listView.SelectedItem).Tag;
                                forceFilter = selectedSystem;
                            }
                            else
                            {
                                forceFilter = "";
                            }
                        }
                        RetriXGoldCoresLoader();
                    }
                }
                catch (Exception ex)
                {
                    PlatformService.ShowErrorMessageDirect(ex);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void CoresRefresh_Click(object sender, RoutedEventArgs e)
        {
            GameSystemSelectionView_eventHandler(null, null);
            RetriXGoldCoresLoader();
        }

        private void WriteLog(string message, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                System.IO.File.AppendAllText(logfileLocation, $"{memberName} ({sourceLineNumber}): {message}\n");
            }
            catch (Exception e)
            {

            }
        }

        private async void LoaderLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                folderPicker.FileTypeFilter.Add(".txt");
                var saveFolder = await folderPicker.PickSingleFolderAsync();
                if (saveFolder != null)
                {
                    var LogLocation = logfileLocation;
                    if (LogLocation != null)
                    {
                        if (System.IO.File.Exists(LogLocation))
                        {
                            var LogFileSimplePath = System.IO.Path.GetFileName(logfileLocation);
                            var logFile = (StorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(LogFileSimplePath);
                            if (logFile != null)
                            {
                                VM.SystemCoreIsLoadingState(true, "Saving logs files..");
                                await logFile.CopyAsync(saveFolder, logFile.Name, NameCollisionOption.GenerateUniqueName);

                                PlatformService.PlayNotificationSoundDirect("success");
                                pushNormalNotification($"Logs file saved sccussfully");
                            }
                            else
                            {
                                PlatformService.PlayNotificationSoundDirect("error");
                                pushNormalNotification("Logs file not found!");
                            }
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("error");
                            pushNormalNotification("Unable to get log file!");
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        pushNormalNotification("Unable to get log location!");
                    }
                }
                VM.SystemCoreIsLoadingState(false);
            }
            catch (Exception ex)
            {
                VM?.SystemCoreIsLoadingState(false);
                PlatformService.PlayNotificationSoundDirect("error");
                pushNormalNotification(ex.Message);
            }
        }

        private void RetriXHelp_Click(object sender, RoutedEventArgs e)
        {
            VM.ShowHelp.Execute(null);
        }

        private void RetriXAbout_Click(object sender, RoutedEventArgs e)
        {
            VM.ShowAbout.Execute(null);
        }

        private void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            RetrixUpdateOnline_Click();
        }

        private async void DeleteBIOSMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmDelete = new ConfirmConfig();
                confirmDelete.SetTitle("Delete B.Map");
                confirmDelete.SetMessage($"Do you want to BIOS Map for {VM.SelectedSystem.Name}? ");
                confirmDelete.UseYesNo();

                var StartDelete = await UserDialogs.Instance.ConfirmAsync(confirmDelete);

                if (StartDelete)
                {
                    VM.SystemCoreIsLoadingState(true, "Deleting Map..");
                    var localFolder = ApplicationData.Current.LocalFolder;
                    var expectedFileName = $"{VM.SelectedSystem.DLLName.Replace(".dll", "")}.rab";
                    var AnyCoreFolder = (StorageFolder)await localFolder.TryGetItemAsync("AnyCore");
                    if (AnyCoreFolder != null)
                    {
                        var fileTest = (StorageFile)await AnyCoreFolder.TryGetItemAsync(expectedFileName);
                        if (fileTest != null)
                        {
                            await fileTest.DeleteAsync();
                            PlatformService.PlayNotificationSoundDirect("success");
                            pushNormalNotification("BIOS Map removed");
                            RetriXGoldCoresLoader(true);
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("error");
                            pushNormalNotification("BIOS map not found!");
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        pushNormalNotification("AnyCore folder not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.PlayNotificationSoundDirect("error");
                PlatformService.ShowErrorMessageDirect(ex);
            }
            VM.SystemCoreIsLoadingState(false);
        }

        private async void OpenSystemFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var TargetFolder = (StorageFolder)await VM.SelectedSystem.GetSystemDirectoryAsync();
                if (TargetFolder == null)
                {
                    TargetFolder = localFolder;
                }
                var success = await Windows.System.Launcher.LaunchFolderAsync(TargetFolder);
                if (!success)
                {
                    PlatformService.PlayNotificationSoundDirect("error");
                    PlatformService.SendToClipboard(TargetFolder.Path);
                    pushNormalNotification("Unable to open folder!\nPath sent to clipboard");
                }
            }
            catch (Exception ex)
            {
                if (PlatformService.isXBOXPure)
                {
                    pushNormalNotification("This is not supported in XBOX");
                    return;
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("error");
                    PlatformService.ShowErrorMessageDirect(ex);
                }
            }
        }

        private async void OpenSavesFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var TargetFolder = (StorageFolder)await VM.SelectedSystem.GetSaveDirectoryAsync();
                if (TargetFolder == null)
                {
                    TargetFolder = localFolder;
                }
                var success = await Windows.System.Launcher.LaunchFolderAsync(TargetFolder);
                if (!success)
                {
                    PlatformService.PlayNotificationSoundDirect("error");
                    PlatformService.SendToClipboard(TargetFolder.Path);
                    pushNormalNotification("Unable to open folder!\nPath sent to clipboard");
                }
            }
            catch (Exception ex)
            {
                if (PlatformService.isXBOXPure)
                {
                    pushNormalNotification("This is not supported in XBOX");
                    return;
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("error");
                    PlatformService.ShowErrorMessageDirect(ex);
                }
            }
        }

        private void BIOSFilesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var item = (FileImporterViewModel)e.ClickedItem;
                if (item != null)
                {
                    item.ImportCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (VM != null && VM.SelectedSystem != null)
                {
                    var folderState = await VM.SelectedSystem.SetGamesDirectoryAsync(true);
                    if (folderState)
                    {
                        PlatformService.PlayNotificationSoundDirect("success");
                        PlatformService.ShowNotificationMain("Games folder set for " + VM.SelectedSystem.Name, 2);
                        AllGetterReload_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void locateAllBiosFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmFind = new ConfirmConfig();
                confirmFind.SetTitle("Find Files");
                confirmFind.SetMessage($"Do you want to start search for {VM.SelectedSystem.Name}'s files?\nNote: select the folder that contains BIOS collection, don't select root drive it will take long time to search");
                confirmFind.UseYesNo();

                var StartFind = await UserDialogs.Instance.ConfirmAsync(confirmFind);

                if (StartFind)
                {
                    FolderPicker picker = new FolderPicker();
                    picker.ViewMode = PickerViewMode.List;
                    picker.SuggestedStartLocation = PickerLocationId.Downloads;
                    picker.FileTypeFilter.Add("*");
                    var folder = await picker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        foreach (var gItem in VM.biosGroupped)
                        {
                            foreach (var bItem in gItem)
                            {
                                try
                                {
                                    if (!bItem.FileAvailable)
                                    {
                                        BIOSFilesList.ScrollIntoView(bItem);
                                        await bItem.ImportHandler(folder);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WriteLog(ex.Message);
                                }
                            }
                        }
                        PlatformService.PlayNotificationSoundDirect("success");
                        PlatformService.ShowNotificationMain("Auto find finished", 2);
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                        pushNormalNotification("Auto find cancelled!");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
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
                            GameSystemSelectionView_eventHandler(null, null);
                            ReloadSystemsHandler(null, null);
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
                            GameSystemSelectionView_eventHandler(null, null);
                            ReloadSystemsHandler(null, null);
                            PlatformService.ShowNotificationMain("Keyboard/Touch Mode", 2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void resetCoreCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VM.SelectedSystem != null)
                {
                    await VM.SelectedSystem.XCore.CacheCoreClean();
                    VM.SelectedSystem.Core.RestartRequired = true;
                    HideSubPages(null, null);
                    PlatformService.PlayNotificationSound("alert");
                    PlatformService.ShowNotificationMain("Core cache reseted, please restart RetriXGold", 3);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cancellationTokenSourceBackup.Cancel();
            }
            catch (Exception ex)
            {

            }
        }

        private async void SetCover_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GlobalSelectedItem != null)
                {
                    GlobalSelectedItem.ProgressState = Visibility.Visible;
                    var gameID = GlobalSelectedItem.Recent.GameID;
                    await SetGameCover(gameID);
                }
                GlobalSelectedItem.ProgressState = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
            ShowLoader(false);
        }

        private async void SetCover2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GlobalGameSelect != null)
                {
                    GlobalGameSelect.ProgressState = Visibility.Visible;
                    var gameID = GlobalGameSelect.GameID;
                    await SetGameCover(gameID);
                }
                GlobalGameSelect.ProgressState = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
            ShowLoader(false);
        }
        public async Task SetGameCover(string gameID)
        {
            try
            {
                var testFile = await VM.GetGameCover(gameID);
                var action = "";
                if (testFile != null)
                {
                    string message = $"Cover already set for this game, do you want to change or reset?";
                    PlatformService.PlayNotificationSound("alert");
                    string DialogTitle = "Game Cover";
                    string DialogMessage = message;
                    string[] DialogButtons = new string[] { $"Change", "Reset", "Cancel" };
                    int[] DialogButtonsIds = new int[] { 2, 1, 3 };

                    var ReplacePromptDialog = Helpers.CreateDialog(DialogTitle, DialogMessage, DialogButtons);
                    WinUniversalTool.Views.DialogResultCustom dialogResultCustom = WinUniversalTool.Views.DialogResultCustom.Nothing;
                    var ReplaceResult = await ReplacePromptDialog.ShowAsync2(dialogResultCustom);
                    if (Helpers.DialogResultCheck(ReplacePromptDialog, 1))
                    {
                        action = "delete";
                    }
                    else if (Helpers.DialogResultCheck(ReplacePromptDialog, 2))
                    {
                        action = "select";
                    }
                }
                else
                {
                    action = "select";
                }
                switch (action)
                {
                    case "select":
                        PlatformService.PlayNotificationSoundDirect("button-01");
                        FileOpenPicker picker = new FileOpenPicker();
                        picker.ViewMode = PickerViewMode.Thumbnail;
                        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                        picker.FileTypeFilter.Add(".bmp");
                        picker.FileTypeFilter.Add(".jpg");
                        picker.FileTypeFilter.Add(".jpeg");
                        picker.FileTypeFilter.Add(".png");
                        picker.FileTypeFilter.Add(".gif");

                        IconFile = await picker.PickSingleFileAsync();
                        if (IconFile != null)
                        {
                            var coversFolder = await VM.GetCoversFolder();
                            if (coversFolder != null)
                            {
                                ShowLoader(true);
                                var ext = System.IO.Path.GetExtension(IconFile.Name);
                                var fileName = $"{gameID}{ext}";
                                await IconFile.CopyAsync(coversFolder, fileName, NameCollisionOption.ReplaceExisting);
                                CoreQuickAccessContainer.UpdateLayout();
                                PlatformService.pivotMainPosition = CoreQuickAccessContainer.VerticalOffset;
                                returningFromSubPage = true;
                                PrepareCoreData();
                            }
                        }
                        break;

                    case "delete":
                        if (testFile != null)
                        {
                            ShowLoader(true);
                            await testFile.DeleteAsync();
                            await VM.UpdateCoversByID(gameID);
                            CoreQuickAccessContainer.UpdateLayout();
                            PlatformService.pivotMainPosition = CoreQuickAccessContainer.VerticalOffset;
                            returningFromSubPage = true;
                            PrepareCoreData();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        ObservableCollection<DaysDataItem> daysData = new ObservableCollection<DaysDataItem>();
        DateTime TargetDayTemp;
        public void PrepareCoreUsageBySelectedSystem(DateTime TargetDay)
        {
            bool foundInsights = false;
            TargetDayTemp = TargetDay;
            long firstDayOfWeek = 0;
            long lastDayOfTheWeek = 0;
            try
            {
                daysData.Clear();

                try
                {
                    ChartsViewer.IsEnabled = !PlatformService.isXBOX;
                }
                catch (Exception ex)
                {

                }
                if (PlatformService.insightItems != null && PlatformService.insightItems.Count > 0)
                {
                    var coreName = VM.GetCoreNameCleanForSelectedSystem();
                    var systemName = VM.SelectedSystem.TempName;

                    var selectedData = PlatformService.insightItems.Where(a => a.CoreName.Equals(coreName) && a.SystemName.Equals(systemName));
                    if (selectedData != null && selectedData.Count() > 0)
                    {
                        SetAdjustRangeState(true);
                        foundInsights = true;
                        int thisWeekNumber = GetIso8601WeekOfYear(TargetDay);
                        bool getLast7IEntries = false;

                        long sunDayOfTheWeek = 0;
                        long monDayOfTheWeek = 0;
                        long tueDayOfTheWeek = 0;
                        long wedDayOfTheWeek = 0;
                        long thuDayOfTheWeek = 0;
                        long friDayOfTheWeek = 0;
                        long satDayOfTheWeek = 0;
                        double sun = 0;
                        double mon = 0;
                        double tue = 0;
                        double wed = 0;
                        double thu = 0;
                        double fri = 0;
                        double sat = 0;

                        if (thisWeekNumber > 0)
                        {
                            firstDayOfWeek = FirstDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                            if (firstDayOfWeek == 0)
                            {
                                getLast7IEntries = true;
                            }
                            else
                            {
                                lastDayOfTheWeek = LastDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                                sunDayOfTheWeek = firstDayOfWeek;
                                monDayOfTheWeek = MonDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                                tueDayOfTheWeek = TueDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                                wedDayOfTheWeek = WedDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                                thuDayOfTheWeek = ThuDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                                friDayOfTheWeek = FriDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                                satDayOfTheWeek = SatDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                            }
                        }
                        else
                        {
                            getLast7IEntries = true;
                        }
                        if (getLast7IEntries)
                        {
                            if (selectedData.Count() > 7)
                            {
                                var skippedArray = selectedData.Skip(selectedData.Count() - 8);
                                FillDaysData(skippedArray, TargetDay, ref sun, ref mon, ref tue, ref wed, ref thu, ref fri, ref sat);
                            }
                            else
                            {
                                FillDaysData(selectedData, TargetDay, ref sun, ref mon, ref tue, ref wed, ref thu, ref fri, ref sat);
                            }
                        }
                        else
                        {
                            var filteredArray = selectedData.Where(a => a.Date >= firstDayOfWeek && a.Date <= lastDayOfTheWeek);
                            FillDaysData(filteredArray, TargetDay, ref sun, ref mon, ref tue, ref wed, ref thu, ref fri, ref sat);
                        }

                        try
                        {
                            if (TargetDay.Ticks == DateTime.Today.Ticks)
                            {
                                SelectedDay.Title = "TARGET";
                            }
                            else
                            {
                                SelectedDay.Title = TargetDay.ToShortDateString();
                            }
                        }
                        catch (Exception e)
                        {

                        }

                        if (sunDayOfTheWeek == TargetDay.Ticks)
                        {
                            daysData.Add(new DaysDataItem() { day = "Sun", main = sun, usage = 0, target = sun });
                        }
                        else
                        {
                            daysData.Add(new DaysDataItem() { day = "Sun", main = sun, usage = sun, target = 0 });
                        }
                        if (monDayOfTheWeek == TargetDay.Ticks)
                        {
                            daysData.Add(new DaysDataItem() { day = "Mon", main = mon, usage = 0, target = mon });
                        }
                        else
                        {
                            daysData.Add(new DaysDataItem() { day = "Mon", main = mon, usage = mon, target = 0 });
                        }
                        if (tueDayOfTheWeek == TargetDay.Ticks)
                        {
                            daysData.Add(new DaysDataItem() { day = "Tue", main = tue, usage = 0, target = tue });
                        }
                        else
                        {
                            daysData.Add(new DaysDataItem() { day = "Tue", main = tue, usage = tue, target = 0 });
                        }
                        if (wedDayOfTheWeek == TargetDay.Ticks)
                        {
                            daysData.Add(new DaysDataItem() { day = "Wed", main = wed, usage = 0, target = wed });
                        }
                        else
                        {
                            daysData.Add(new DaysDataItem() { day = "Wed", main = wed, usage = wed, target = 0 });
                        }
                        if (thuDayOfTheWeek == TargetDay.Ticks)
                        {
                            daysData.Add(new DaysDataItem() { day = "Thu", main = thu, usage = 0, target = thu });
                        }
                        else
                        {
                            daysData.Add(new DaysDataItem() { day = "Thu", main = thu, usage = thu, target = 0 });
                        }
                        if (friDayOfTheWeek == TargetDay.Ticks)
                        {
                            daysData.Add(new DaysDataItem() { day = "Fri", main = fri, usage = 0, target = fri });
                        }
                        else
                        {
                            daysData.Add(new DaysDataItem() { day = "Fri", main = fri, usage = fri, target = 0 });
                        }
                        if (satDayOfTheWeek == TargetDay.Ticks)
                        {
                            daysData.Add(new DaysDataItem() { day = "Sat", main = sat, usage = 0, target = sat });
                        }
                        else
                        {
                            daysData.Add(new DaysDataItem() { day = "Sat", main = sat, usage = sat, target = 0 });
                        }
                    }
                    else
                    {
                        SetAdjustRangeState(false);
                    }
                }
                else
                {
                    SetAdjustRangeState(false);
                }
            }
            catch (Exception ex)
            {
                foundInsights = false;
            }
            try
            {
                if (!foundInsights)
                {
                    NoInsightsTextGrid.Visibility = Visibility.Visible;
                    CoreDaysData.Visibility = Visibility.Collapsed;
                    InsightsRange.Visibility = Visibility.Collapsed;
                }
                else
                {
                    try
                    {
                        DateTime fDate = new DateTime(firstDayOfWeek);
                        DateTime lDate = new DateTime(lastDayOfTheWeek);
                        InsightsRange.Text = $"{fDate.Date.ToShortDateString()} - {lDate.Date.ToShortDateString()}";
                    }
                    catch (Exception ex)
                    {

                    }
                    NoInsightsTextGrid.Visibility = Visibility.Collapsed;
                    CoreDaysData.Visibility = Visibility.Visible;
                    InsightsRange.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void FillDaysData(IEnumerable<InsightItem> filteredArray, DateTime TargetDay, ref double sun, ref double mon, ref double tue, ref double wed, ref double thu, ref double fri, ref double sat)
        {
            try
            {
                long sunDayOfTheWeek = 0;
                long monDayOfTheWeek = 0;
                long tueDayOfTheWeek = 0;
                long wedDayOfTheWeek = 0;
                long thuDayOfTheWeek = 0;
                long friDayOfTheWeek = 0;
                long satDayOfTheWeek = 0;

                int thisWeekNumber = GetIso8601WeekOfYear(TargetDay);

                sunDayOfTheWeek = SunDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                monDayOfTheWeek = MonDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                tueDayOfTheWeek = TueDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                wedDayOfTheWeek = WedDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                thuDayOfTheWeek = ThuDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                friDayOfTheWeek = FriDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);
                satDayOfTheWeek = SatDateOfWeek(TargetDay.Year, thisWeekNumber, CultureInfo.CurrentCulture);

                var sunArray = filteredArray.Where(a => a.Date >= sunDayOfTheWeek && a.Date <= sunDayOfTheWeek);
                var monArray = filteredArray.Where(a => a.Date >= monDayOfTheWeek && a.Date <= monDayOfTheWeek);
                var tueArray = filteredArray.Where(a => a.Date >= tueDayOfTheWeek && a.Date <= tueDayOfTheWeek);
                var wedArray = filteredArray.Where(a => a.Date >= wedDayOfTheWeek && a.Date <= wedDayOfTheWeek);
                var thuArray = filteredArray.Where(a => a.Date >= thuDayOfTheWeek && a.Date <= thuDayOfTheWeek);
                var friArray = filteredArray.Where(a => a.Date >= friDayOfTheWeek && a.Date <= friDayOfTheWeek);
                var satArray = filteredArray.Where(a => a.Date >= satDayOfTheWeek && a.Date <= satDayOfTheWeek);

                if (sunArray != null && sunArray.Count() > 0)
                {
                    sun = sunArray.Sum(a => a.Time) / 1000.0d / 60.0d / 60.0d;
                }
                if (monArray != null && monArray.Count() > 0)
                {
                    mon = monArray.Sum(a => a.Time) / 1000.0d / 60.0d / 60.0d;
                }
                if (tueArray != null && tueArray.Count() > 0)
                {
                    tue = tueArray.Sum(a => a.Time) / 1000.0d / 60.0d / 60.0d;
                }
                if (wedArray != null && wedArray.Count() > 0)
                {
                    wed = wedArray.Sum(a => a.Time) / 1000.0d / 60.0d / 60.0d;
                }
                if (thuArray != null && thuArray.Count() > 0)
                {
                    thu = thuArray.Sum(a => a.Time) / 1000.0d / 60.0d / 60.0d;
                }
                if (friArray != null && friArray.Count() > 0)
                {
                    fri = friArray.Sum(a => a.Time) / 1000.0d / 60.0d / 60.0d;
                }
                if (satArray != null && satArray.Count() > 0)
                {
                    sat = satArray.Sum(a => a.Time) / 1000.0d / 60.0d / 60.0d;
                }
            }
            catch (Exception xex)
            {

            }
        }
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            try
            {
                DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
                if (day >= DayOfWeek.Sunday && day <= DayOfWeek.Wednesday)
                {
                    time = time.AddDays(3);
                }

                return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static long FirstDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                DateTime jan1 = new DateTime(year, 1, 1);
                int daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
                DateTime firstWeekDay = jan1.AddDays(daysOffset);
                int firstWeek = ci.Calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
                if ((firstWeek <= 1 || firstWeek >= 52) && daysOffset >= -3)
                {
                    weekOfYear -= 1;
                }
                return firstWeekDay.AddDays(weekOfYear * 7).Ticks;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static DateTime FirstDateOfTheWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                DateTime jan1 = new DateTime(year, 1, 1);
                int daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
                DateTime firstWeekDay = jan1.AddDays(daysOffset);
                int firstWeek = ci.Calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
                if ((firstWeek <= 1 || firstWeek >= 52) && daysOffset >= -3)
                {
                    weekOfYear -= 1;
                }
                return firstWeekDay.AddDays(weekOfYear * 7);
            }
            catch (Exception ex)
            {
                return default;
            }
        }
        public static long LastDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                return FirstDateOfTheWeek(year, weekOfYear, ci).AddDays(6).Ticks;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static long SunDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                return FirstDateOfTheWeek(year, weekOfYear, ci).Ticks;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static long MonDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                return FirstDateOfTheWeek(year, weekOfYear, ci).AddDays(1).Ticks;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static long TueDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                return FirstDateOfTheWeek(year, weekOfYear, ci).AddDays(2).Ticks;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static long WedDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                return FirstDateOfTheWeek(year, weekOfYear, ci).AddDays(3).Ticks;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static long ThuDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                return FirstDateOfTheWeek(year, weekOfYear, ci).AddDays(4).Ticks;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static long FriDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                return FirstDateOfTheWeek(year, weekOfYear, ci).AddDays(5).Ticks;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static long SatDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            try
            {
                return FirstDateOfTheWeek(year, weekOfYear, ci).AddDays(6).Ticks;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private async void AdjustInsightsDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var coreName = VM.GetCoreNameCleanForSelectedSystem();
                var systemName = VM.SelectedSystem.TempName;

                var selectedData = PlatformService.insightItems.Where(a => a.CoreName.Equals(coreName) && a.SystemName.Equals(systemName));
                if (selectedData != null && selectedData.Count() > 0)
                {
                    List<long> days = new List<long>();
                    foreach (var dItem in selectedData)
                    {
                        days.Add(dItem.Date);
                    }
                    var daysUnique = days.Distinct();
                    ContentDialog contentDialog = new ContentDialog();
                    contentDialog.Title = "Insights (Week)";
                    contentDialog.PrimaryButtonText = "Show";
                    contentDialog.SecondaryButtonText = "Cancel";
                    contentDialog.IsPrimaryButtonEnabled = true;
                    contentDialog.IsSecondaryButtonEnabled = true;
                    ComboBox comboBox = new ComboBox();
                    comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    long lastItem = 0;
                    bool itemSelected = false;
                    foreach (var dItem in daysUnique)
                    {
                        ComboBoxItem day = new ComboBoxItem();
                        TimeSpan time = TimeSpan.FromTicks(dItem);
                        DateTime dt = new DateTime() + time;
                        day.Tag = dt;
                        if (dt.Ticks == DateTime.Today.Ticks)
                        {
                            day.Content = dt.ToShortDateString() + " (Today)";
                        }
                        else
                        {
                            day.Content = dt.ToShortDateString();
                        }
                        comboBox.Items.Add(day);
                        lastItem = dt.Ticks;
                        try
                        {
                            if (lastItem == TargetDayTemp.Ticks)
                            {
                                comboBox.SelectedItem = day;
                                itemSelected = true;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    var currentDate = DateTime.Today.Ticks;
                    if (lastItem != currentDate)
                    {
                        ComboBoxItem day = new ComboBoxItem();
                        TimeSpan time = TimeSpan.FromTicks(currentDate);
                        DateTime dt = new DateTime() + time;
                        day.Tag = dt;
                        day.Content = dt.ToShortDateString() + " (Today)";

                        comboBox.Items.Add(day);
                    }
                    if (!itemSelected)
                    {
                        comboBox.SelectedItem = comboBox.Items[comboBox.Items.Count - 1];
                    }
                    contentDialog.Content = comboBox;
                    var result = await contentDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        var selectedDate = (DateTime)((ComboBoxItem)comboBox.SelectedItem).Tag;
                        PrepareCoreUsageBySelectedSystem(selectedDate);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void SetAdjustRangeState(bool state)
        {
            try
            {
                AdjustInsightsDate.IsEnabled = state;
            }
            catch (Exception ex)
            {

            }
        }

        private void ImportCoreFiles_Click(object sender, RoutedEventArgs e)
        {
            PureCoreImportBIOS_Click();
        }

        private void ImportCoreFolder_Click(object sender, RoutedEventArgs e)
        {
            ImportFolderAction();
        }

        private async void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalGameSelect != null)
            {
                GlobalGameSelect.ProgressState = Visibility.Visible;
                await VM.DeleteGameByName(GlobalGameSelect);
                GlobalGameSelect.ProgressState = Visibility.Collapsed;
                GlobalGameSelect = null;
            }
        }

        private async void OpenDirectFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GlobalGameSelect != null)
                {
                    try
                    {
                        var state = await Windows.System.Launcher.LaunchFileAsync(GlobalGameSelect.attachedFile);
                    }
                    catch (Exception ex)
                    {
                        if (PlatformService.isXBOXPure)
                        {
                            pushNormalNotification("This is not supported in XBOX");
                            return;
                        }
                        else
                        {
                            PlatformService.PlayNotificationSoundDirect("error");
                            PlatformService.ShowErrorMessageDirect(ex);
                        }
                    }
                }
                GlobalGameSelect = null;
            }
            catch (Exception ex)
            {

            }
        }

        private async void SaveDirectFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GlobalGameSelect != null)
                {
                    FolderPicker picker = new FolderPicker();
                    picker.ViewMode = PickerViewMode.List;
                    picker.SuggestedStartLocation = PickerLocationId.Downloads;
                    picker.FileTypeFilter.Add(".zip");

                    var folder = await picker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        //ShowLoader(true, "Saving File..");
                        GlobalGameSelect.ProgressState = Visibility.Visible;
                        await GlobalGameSelect.attachedFile.CopyAsync(folder, GlobalGameSelect.attachedFile.Name, NameCollisionOption.GenerateUniqueName);
                        pushNormalNotification("File successfully copied");
                    }
                    GlobalGameSelect.ProgressState = Visibility.Collapsed;
                    GlobalGameSelect = null;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    GlobalGameSelect.ProgressState = Visibility.Collapsed;
                }catch(Exception exx)
                {

                }
                GlobalGameSelect = null;
                pushNormalNotification($"Error: {ex.Message}");
            }
        }
    }


    class DaysDataItem
    {
        public string day { get; set; }
        public double main { get; set; }
        public double usage { get; set; }
        public double target { get; set; }
    }

    class RetriXSettingsItem
    {
        public string Group;
        public string Name;
        public string Desc;
        public string Icon;
        public string PreferencesKey;
        public string DefaultValueKey;
        public EventHandler ExtraAction;
        public ToggleSwitch SettingSwitchItem;
        public ComboBox SettingComboItem;
        public Type ParentClass;
        public bool OnReset = false;
        public bool IsCombo = false;
        public int DefaultInt = 0;
        public RetriXSettingsItem(ref List<RetriXSettingsItem> settingsList, string group, string name, string desc, string icon, string preferencesKey, string defaultValueKey, bool onReset = false, bool reversedValue = false, Type parentClass = null, EventHandler extraAction = null, bool isCombo = false, int min = 0, int max = 0, int step = 1, int defaultInt = 0, string[] cast = null)
        {
            Group = group;
            Name = name;
            Desc = desc;
            Icon = $"ms-appx:///Assets/Icons/Menus/{icon}.png";
            IsCombo = isCombo;
            DefaultInt = defaultInt;
            try
            {
                PreferencesKey = preferencesKey;
                DefaultValueKey = defaultValueKey;
                ExtraAction = extraAction;
                OnReset = onReset;
                ParentClass = parentClass;
                if (!isCombo)
                {
                    SettingSwitchItem = new ToggleSwitch();
                    SettingSwitchItem.OnContent = "ON";
                    SettingSwitchItem.OffContent = "OFF";
                    SettingSwitchItem.Margin = new Thickness(0, 0, 0, 3);
                    Type myType = parentClass != null ? parentClass : typeof(PlatformService);
                    var fields = myType.GetFields();
                    var probs = myType.GetProperties();
                    bool defaultValue = false;
                    try
                    {
                        defaultValue = (bool)myType.GetField(defaultValueKey).GetValue(myType);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            defaultValue = (bool)myType.GetProperty(defaultValueKey).GetValue(myType);
                        }
                        catch (Exception exx)
                        {

                        }
                    }

                    SettingSwitchItem.IsOn = reversedValue ? !defaultValue : defaultValue;
                    SettingSwitchItem.Toggled += (s, e) =>
                    {
                        try
                        {
                            defaultValue = !(bool)myType.GetField(defaultValueKey).GetValue(myType);
                            myType.GetField(defaultValueKey).SetValue(myType, defaultValue);
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(preferencesKey, defaultValue);
                            if (extraAction != null)
                            {
                                extraAction.Invoke(defaultValue, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            //PlatformService.ShowErrorMessage(ex);
                        }
                    };
                }
                else
                {
                    SettingComboItem = new ComboBox();
                    SettingComboItem.Margin = new Thickness(0, 0, 0, 3);
                    SettingComboItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                    for (var i = min; i <= max;)
                    {
                        ComboBoxItem cbItem = new ComboBoxItem();
                        if (cast != null)
                        {
                            try
                            {
                                cbItem.Content = cast[i];
                            }
                            catch (Exception ex)
                            {
                                cbItem.Content = i.ToString();
                            }
                        }
                        else
                        {
                            cbItem.Content = i.ToString();
                        }
                        SettingComboItem.Items.Add(cbItem);
                        i += step;
                    }

                    Type myType = parentClass != null ? parentClass : typeof(PlatformService);
                    var fields = myType.GetFields();
                    var probs = myType.GetProperties();
                    int defaultValue = 0;
                    try
                    {
                        defaultValue = (int)myType.GetField(defaultValueKey).GetValue(myType);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            defaultValue = (int)myType.GetProperty(defaultValueKey).GetValue(myType);
                        }
                        catch (Exception exx)
                        {

                        }
                    }
                    SettingComboItem.SelectedIndex = (defaultValue);

                    SettingComboItem.SelectionChanged += (s, e) =>
                    {

                        try
                        {
                            defaultValue = SettingComboItem.SelectedIndex;
                            myType.GetField(defaultValueKey).SetValue(myType, defaultValue);
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(preferencesKey, defaultValue);
                            if (extraAction != null)
                            {
                                extraAction.Invoke(defaultValue, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            //PlatformService.ShowErrorMessage(ex);
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                //PlatformService.ShowErrorMessage(ex);
            }
            settingsList.Add(this);
        }
        public void ResetSetting()
        {
            try
            {
                if (!IsCombo)
                {
                    Type myType = ParentClass != null ? ParentClass : typeof(PlatformService);
                    var defaultValue = OnReset;
                    myType.GetField(DefaultValueKey).SetValue(myType, defaultValue);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(PreferencesKey, defaultValue);
                    if (ExtraAction != null)
                    {
                        ExtraAction.Invoke(defaultValue, null);
                    }
                }
                else
                {
                    Type myType = ParentClass != null ? ParentClass : typeof(PlatformService);
                    var defaultValue = DefaultInt;
                    myType.GetField(DefaultValueKey).SetValue(myType, defaultValue);
                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(PreferencesKey, defaultValue);
                    if (ExtraAction != null)
                    {
                        ExtraAction.Invoke(defaultValue, null);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
    class RequestedCore
    {
        public string CoreName;
        public string SystemName;
        public RequestedCore(string core, string system)
        {
            CoreName = core;
            SystemName = system;
        }
    }
    class CoresQuickListItem : BindableBase
    {
        public string Action = "core";
        private string menuIcon;
        public string MenuIcon
        {
            get
            {
                return menuIcon;
            }
            set
            {
                var transcodedState = GameSystemSelectionView.GetTranscodedImage(value, ref menuIcon);
                if (!transcodedState)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var transcodedImage = await GameSystemSelectionView.ConvertPngToBmp(value);
                            if (!transcodedImage.Equals(menuIcon))
                            {
                                menuIcon = transcodedImage;
                                RaisePropertyChanged(MenuIcon);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
            }
        }
        public GameSystemViewModel Core;
        public GameSystemRecentModel Recent;
        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                RaisePropertyChanged(nameof(Title));
            }
        }
        public Visibility progressState = Visibility.Collapsed;
        public Visibility ProgressState
        {
            get
            {
                return progressState;
            }
            set
            {
                progressState = value;
                RaisePropertyChanged(nameof(ProgressState));
                RaisePropertyChanged(nameof(ProgressActive));
                if (Recent != null)
                {
                    Recent.ProgressState = value;
                    Recent.RaiseProgressState();
                }
            }
        }
        public bool ProgressActive
        {
            get
            {
                if(progressState == Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                RaisePropertyChanged(nameof(Description));
                if(Description != null && Description.Length > 0)
                {
                    DescriptionVisibility = Visibility.Visible;
                }
                RaisePropertyChanged(nameof(DescriptionVisibility));
            }
        }
        public Visibility DescriptionVisibility = Visibility.Collapsed;
        private string tag;
        public string Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
                if (Tag != null && Tag.Length > 0 && !IsImage)
                {
                    TagVisibility = Visibility.Visible;
                }
                RaisePropertyChanged(nameof(Tag));
                RaisePropertyChanged(nameof(TagVisibility));
            }
        }
        public string TagGray;
        public Visibility TagVisibility = Visibility.Collapsed;
        public Visibility TagVisibilityGray = Visibility.Collapsed;
        public bool Collapsed = false;
        public bool IsImage = false;
        public bool IsIcon
        {
            get
            {
                return !IsImage;
            }
        }
        public BitmapImage bitmapImage = null;
        public StorageFile attachedFile = null;
        public bool OnHoldEvent = false;
        public bool SuggestedCore = false;
        public bool IsOn = false;
        public string Group;
        public string DataTemplate;
        public object CustomControl;
        public EventHandler LiveItemEvent;
        public Stretch StretchState = Stretch.UniformToFill;
        public int MaxSize
        {
            get
            {
                if(StretchState == Stretch.UniformToFill)
                {
                    return 250;
                }
                else
                {
                    return 72;
                }
            }
        }
        private SolidColorBrush borderColor = new SolidColorBrush((Windows.UI.Color)Helpers.HexToColor("#663949ab"));
        public SolidColorBrush BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                borderColor = value;
                RaisePropertyChanged(nameof(BorderColor));
            }
        }

        public int IconSize = 60;
        public CoresQuickListItem(string group, string action, string title, string icon, bool LiveItemEvent, string dataTemplate = "BrowseGridMenuItemTemplate")
        {
            Group = group;
            Action = action;
            Title = title;
            MenuIcon = icon;
            DescriptionVisibility = Description != null && Description.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            TagVisibility = Tag != null && Tag.Length > 0 && !IsImage ? Visibility.Visible : Visibility.Collapsed;

            DataTemplate = dataTemplate;

            if (PlatformService.isXBOX)
            {
                IconSize = 80;
            }
            else
            {
                IconSize = 65;
            }
        }

        public async void LiveItemTask()
        {
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        LiveItemEvent.Invoke(null, null);
                        await Task.Delay(700);
                        LiveItemTask();
                    }
                    catch
                    {

                    }
                });
            }
            catch
            {

            }
        }

        public CoresQuickListItem(string group, string action, string title, string icon, string descs = "", string tag = "", GameSystemViewModel core = null, string dataTemplate = "BrowseGridMenuItemTemplate")
        {
            Group = group;
            Action = action;
            Title = title;
            MenuIcon = icon;
            Description = descs;
            DescriptionVisibility = Description != null && Description.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            Tag = tag;
            TagVisibility = Tag != null && Tag.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            Core = core;
            DataTemplate = dataTemplate;
        }
        public CoresQuickListItem(string group, string action, string title, string icon, string descs = "", string tag = "", GameSystemRecentModel recent = null, string dataTemplate = "BrowseGridMenuItemTemplate")
        {
            Group = group;
            Action = action;
            Title = title;
            MenuIcon = icon;
            Description = descs;
            DescriptionVisibility = Description != null && Description.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            Tag = tag;
            TagVisibility = Tag != null && Tag.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            Recent = recent;
            DataTemplate = dataTemplate;
        }
    }
    class CoreExtensionItem : BindableBase
    {
        public string extTitle = ".null";
        public SolidColorBrush extTitleColor = new SolidColorBrush(Colors.DodgerBlue);
        Windows.UI.Color[] colors = new Windows.UI.Color[] { Windows.UI.Colors.DodgerBlue, Windows.UI.Colors.Orange, Windows.UI.Colors.Gray, Windows.UI.Colors.Tomato, Windows.UI.Colors.Violet, Windows.UI.Colors.DarkGreen, Windows.UI.Colors.CornflowerBlue, Windows.UI.Colors.RoyalBlue, Windows.UI.Colors.DodgerBlue, Windows.UI.Colors.Orange, Windows.UI.Colors.Gray, Windows.UI.Colors.Tomato, Windows.UI.Colors.Violet, Windows.UI.Colors.DarkGreen, Windows.UI.Colors.CornflowerBlue, Windows.UI.Colors.RoyalBlue, Windows.UI.Colors.DodgerBlue, Windows.UI.Colors.Orange, Windows.UI.Colors.Gray, Windows.UI.Colors.Tomato, Windows.UI.Colors.Violet, Windows.UI.Colors.DarkGreen, Windows.UI.Colors.CornflowerBlue, Windows.UI.Colors.RoyalBlue, Windows.UI.Colors.DodgerBlue, Windows.UI.Colors.Orange, Windows.UI.Colors.Gray, Windows.UI.Colors.Tomato, Windows.UI.Colors.Violet, Windows.UI.Colors.DarkGreen, Windows.UI.Colors.CornflowerBlue, Windows.UI.Colors.RoyalBlue };
        Dictionary<string, Windows.UI.Color> specificColors = new Dictionary<string, Windows.UI.Color>()
        {
            {".zip", Windows.UI.Colors.Gold },
            {".7z", Windows.UI.Colors.Brown },
            {".rar", Windows.UI.Colors.Purple },
            {"Exts", Windows.UI.Colors.Purple },
            {"Support:", Windows.UI.Colors.Gray },
            {"More", Windows.UI.Colors.OrangeRed },
        };
        public CoreExtensionItem(string t)
        {
            extTitle = t;
            Windows.UI.Color testColor = Colors.DodgerBlue;
            if (!specificColors.TryGetValue(t, out testColor))
            {
                try
                {
                    var RandomIndex = new Random().Next(0, 30);
                    testColor = colors[RandomIndex];
                }
                catch (Exception ex)
                {

                }
            }
            extTitleColor = new SolidColorBrush(testColor);
        }
    }
    public class CoreOptionListItem : BindableBase
    {
        public string Name { get; set; }
        public List<string> Values = new List<string>();
        public string Group;
        public string SystemName;
        public int selectedIndex = 0;
        public bool droppedDown = false;
        public bool DroppedDown
        {
            get
            {
                return droppedDown;
            }
            set
            {
                droppedDown = value;
                RaisePropertyChanged(nameof(DroppedDown));
                setDropdownOff();
            }
        }
        public async void setDropdownOff()
        {
            await Task.Delay(100);
            PlatformService.CoreOptionsDropdownOpened = droppedDown;
        }
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;
            }
        }
        public string SelectedValue
        {
            get
            {
                return Values[SelectedIndex];
            }
            set
            {
                if (value != null)
                {
                    SelectedIndex = Values.IndexOf(value);
                }
            }
        }
        public string Key;
        public bool isEnabled = true;
        public CoreOptionListItem(string key, string system, string name, List<string> values, int selected, string group = "Core Options")
        {
            Name = name;
            Values = values;
            Group = group.ToUpper();

            if (Name.Contains(">"))
            {
                try
                {
                    var parts = Name.Split('>');
                    Group = parts[0].ToUpper();
                    Name = Name.Replace($"{parts[0]}> ", "");
                }
                catch (Exception ex)
                {

                }
            }

            SystemName = system;
            SelectedIndex = selected;
            Key = key;
            if (key.StartsWith("beetle_psx_internal_resolution") || key.StartsWith("beetle_psx_hw_internal_resolution"))
            {
                isEnabled = false;
                RaisePropertyChanged(nameof(isEnabled));
            }
            RaisePropertyChanged(nameof(SelectedIndex));
            RaisePropertyChanged(nameof(SelectedValue));
        }
    }
    public class GroupCoreOptionListItems : ObservableCollection<CoreOptionListItem>
    {
        public GroupCoreOptionListItems(string key)
        {
            Key = key;
        }
        public GroupCoreOptionListItems(ObservableCollection<CoreOptionListItem> coreOptionListItems, string key)
        {
            foreach (var item in coreOptionListItems)
            {
                base.Add(item);
            }
            Key = key;
        }
        public string Key { get; set; }
    }

    public class GroupBIOSListItems : ObservableCollection<FileImporterViewModel>
    {
        public GroupBIOSListItems(string key)
        {
            Key = key;
        }
        public GroupBIOSListItems(ObservableCollection<FileImporterViewModel> coreOptionListItems, string key)
        {
            foreach (var item in coreOptionListItems)
            {
                base.Add(item);
            }
            Key = key;
        }
        public string Key { get; set; }
    }

    public class GroupButtonListItems : ObservableCollection<ButtonListItem>
    {
        public GroupButtonListItems(string key)
        {
            Key = key;
        }
        public GroupButtonListItems(ObservableCollection<ButtonListItem> buttonsListItems, string key)
        {
            foreach (var item in buttonsListItems)
            {
                base.Add(item);
            }
            Key = key;
        }
        public string Key { get; set; }
    }
    public class ButtonListItem : BindableBase
    {
        public string Name { get; set; }
        public string touchName = "";
        public bool isEnabled = true;

        public string TouchName
        {
            get
            {
                return touchName;
            }
            set
            {
                touchName = value;
                RaisePropertyChanged(nameof(isTouchNameVisible));
            }
        }
        public string keyboardName = "";
        public string KeyboardName
        {
            get
            {
                return keyboardName;
            }
            set
            {
                keyboardName = value;
                RaisePropertyChanged(nameof(isKeyboardNameVisible));
            }
        }
        public bool isKeyboardNameVisible
        {
            get
            {
                return KeyboardName.Length > 0;
            }
        }
        public bool isTouchNameVisible
        {
            get
            {
                return TouchName.Length > 0;
            }
        }

        public List<string> Values = new List<string>();
        public string Group;
        public string SystemName;
        public int selectedIndex = 0;
        public bool droppedDown = false;
        public bool DroppedDown
        {
            get
            {
                return droppedDown;
            }
            set
            {
                droppedDown = value;
                RaisePropertyChanged(nameof(DroppedDown));
                setDropdownOff();
            }
        }
        public async void setDropdownOff()
        {
            await Task.Delay(100);
            PlatformService.CoreOptionsDropdownOpened = droppedDown;
        }
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;
            }
        }
        public string SelectedValue
        {
            get
            {
                return Values[SelectedIndex];
            }
            set
            {
                if (value != null)
                {
                    SelectedIndex = Values.IndexOf(value);
                }
            }
        }
        public object Key;
        InjectedInputTypes[] AdditionalControls = new InjectedInputTypes[] { InjectedInputTypes.DeviceIdJoypadL, InjectedInputTypes.DeviceIdJoypadR, InjectedInputTypes.DeviceIdJoypadL2, InjectedInputTypes.DeviceIdJoypadR2 };
        public ButtonListItem(object key, string system, string name, List<string> values, int selected, string group = "Gamepad Controls")
        {
            Name = name;
            Values = values;
            Group = group.ToUpper();
            SystemName = system;
            SelectedIndex = selected;
            Key = key;
            isEnabled = true;
            RaisePropertyChanged(nameof(SelectedIndex));
            RaisePropertyChanged(nameof(SelectedValue));

            try
            {
                var keyType = (InjectedInputTypes)key;
                if (AdditionalControls.Contains(keyType))
                {
                    Group = "Additional".ToUpper();
                }

            }
            catch (Exception ex)
            {

            }
            switch (key)
            {
                case "specificCaseHere":
                    isEnabled = false;
                    RaisePropertyChanged(nameof(isEnabled));
                    break;
            }
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
        }
    }
    public class FilterItem : BindableBase
    {
        public string FilterText;
        public string FilterValue;
        public string FilterType;
        public SolidColorBrush FilterColor
        {
            get
            {
                if (FilterType.ToLower().Equals("none"))
                {
                    return new SolidColorBrush(Colors.Gray);
                }
                else if (FilterType.ToLower().Equals("folder"))
                {
                    return new SolidColorBrush(Colors.Purple);
                }
                else if (FilterType.ToLower().Equals("exten"))
                {
                    return new SolidColorBrush(Colors.Orange);
                }
                else if (FilterType.ToLower().Equals("system"))
                {
                    return new SolidColorBrush(Colors.Gold);
                }
                else if (FilterType.ToLower().Equals("saves"))
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }
        }
        public FilterItem(string text, string value, string type)
        {
            FilterText = text;
            FilterValue = value;
            FilterType = type;
        }
    }
    class CoreIssues
    {
        public string consoleName;
        public List<IssueItem> consoleIssues;
    }
    class IssueItem
    {
        public string issueText;
        public string reportedBy;
        public string issueSolution;
        public string targetArch;
        public string issueLink;
        public bool isEnhancement;
        public string enhancementTag;
        public string enhancementTagColor;
        public string customCore;
    }
}


