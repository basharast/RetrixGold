using LibRetriX;
using LibRetriX.RetroBindings;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Newtonsoft.Json;
using RetriX.Shared.Models;
using RetriX.Shared.Services;
using RetriX.Shared.StreamProviders;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Controls;
using RetriX.UWP.Pages;
using RetriX.UWP.RetroBindings.Structs;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WinUniversalTool;
using WinUniversalTool.Models;


namespace RetriX.UWP.Services
{
    public enum FullScreenChangeType { Enter, Exit, Toggle };

    public enum MousePointerVisibility { Visible, Hidden };

    //All vars/funcs in PlatformService.cs changed to static
    //no more creating instances to deal with it
    public class PlatformService
    {
        public static int RetriXBuildNumber = 3100;
        public static bool JustStarted = true;
        public static bool FullScreen = true;
        public static bool SettingsSections = false;
        public static bool SupportSections = false;
        public static bool TempAlreadyCleaned = false;
        public static bool returningFromSubPage = false;
        public static string ForceFilter = "";


        #region Main Services
        public static GameLaunchEnvironment gameLaunchEnvironment;
        public static ObservableCollection<GameSystemViewModel> Consoles = new ObservableCollection<GameSystemViewModel>();
        public static VideoService videoService = new VideoService();
        public static AudioService audioService = new AudioService();
        public static SaveStateService saveStateService = new SaveStateService();
        public static InputService inputService = new InputService();
        public static GameSystemsProviderServiceBase GameSystemsProviderService = new GameSystemsProviderServiceBase();
        public static EmulationService emulationService = new EmulationService();
        public static GameSystemSelectionViewModel gameSystemSelectionView = new GameSystemSelectionViewModel();
        public static retro_keyboard_event_t KeyboardEvent;
        public static bool CoreReadingRetroKeyboard = false;
        public static bool CacheGamesListResults = true;
        public static bool AppStartedByRetroPass = false;
        public static bool ForceOldBufferMethods = false;
        public static bool ShowScreeshots = false;
        public static bool UseColoredDialog = false;
        public static bool RetroPassRoot = false;
        public static bool SafeRender = false;
        public static bool AdjustInGameLists = true;
        public static bool EnableHDPI = false;
        public static string RequestingCoreDownload = "";
        public static string RetroPassLaunchCMD = "cmd";
        public static string RetroPassLaunchGame = "none";
        public static string RetroPassLaunchCore = "auto";
        public static string RetroPassLaunchOnExit = "retropass:";
        public static EventHandler RetroPassHandler;
        public static EventHandler CoreDownloaderHandler;
        public static EventHandler CoresLoaderHandler;
        public static EventHandler VisualizerSettings;
        public static double ScreenScale = 100;
        public static float DPI = 96;
        #endregion


        #region Handlers
        public static EventHandler SaveButtonHandler;
        public static EventHandler ResetButtonHandler;
        public static EventHandler CancelButtonHandler;
        public static EventHandler HideLogsHandler;

        public static bool ActionVisibile = false;
        public static bool LogsVisibile = false;

        #endregion


        public static bool GameStarted
        {
            get
            {
                return App.GameStarted;
            }
            set
            {
                App.GameStarted = value;
            }
        }

        #region Effects System
        public static EventHandler ReloadOverlayEffectsStaticHandler;

        public static bool BlendModeSet = false;
        public static bool isBlendModeSet
        {
            get
            {
                if (!BlendModeSet)
                {
                    BlendModeSet = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BlendModeSetState", false);
                }
                return BlendModeSet;
            }
            set
            {
                BlendModeSet = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("BlendModeSetState", BlendModeSet);
                if (ReloadOverlayEffectsStaticHandler != null)
                {
                    ReloadOverlayEffectsStaticHandler.Invoke(value, null);
                }
            }
        }


        public static bool fitOverlay = false;
        public static bool FitOverlay
        {
            get
            {
                if (!fitOverlay)
                {
                    fitOverlay = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("FitOverlayState", false);
                }
                return fitOverlay;
            }
            set
            {
                fitOverlay = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("FitOverlayState", fitOverlay);
            }
        }

        public static int BlendModeGlobal
        {
            get
            {
                return BlendEffectModeGlobal;
            }
            set
            {
                BlendEffectModeGlobal = value;
            }
        }
        private static int blendEffectModeGlobal = -1;
        public static int BlendEffectModeGlobal
        {
            get
            {
                if (blendEffectModeGlobal == -1)
                {
                    blendEffectModeGlobal = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("BlendModeState", -1);
                }
                return blendEffectModeGlobal;
            }
            set
            {
                blendEffectModeGlobal = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("BlendModeState", value);
            }
        }
        #endregion

        public static StorageFile OpenBackupFile = null;
        public static StorageFile OpenGameFile = null;
        public static EventHandler ReloadSystemsHandler = null;
        public static EventHandler ImportFolderHandler = null;
        public static EventHandler raiseInGameOptionsActiveState = null;

        public static int InitWidthSize = 600;
        public static HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
        private static readonly ISet<string> DeviceFamiliesAllowingFullScreenChange = new HashSet<string>
        {
            "Windows.Desktop", "Windows.Team", "Windows.Mobile"
        };

        public static double MaxProgressValue = 100;
        public static bool isCoresLoaded = false;
        public static bool MuteSFX = false;
        public static bool ShowIndicators = true;
        public static bool ShowThumbNailsIcons = true;
        public static bool AutoFitResolver = true;
        public static bool MobileStatusBar = true;

        public static bool IsCoresLoaded
        {
            get
            {
                return isCoresLoaded;
            }

            set
            {
                isCoresLoaded = value;
            }
        }

        public static bool AppStartedByFile = false;
        public static bool IsAppStartedByFile
        {
            get
            {
                return AppStartedByFile;
            }
            set
            {
                AppStartedByFile = value;
            }
        }

        private static bool RealXBoxMode = false;
        public static bool XBoxModeState
        {
            get
            {
                return RealXBoxMode;
            }
            set
            {
                RealXBoxMode = value;
                if (RealXBoxMode)
                {
                    try
                    {
                        ChangeToXBoxModeRequested(null, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        public static bool XBoxMode
        {
            get
            {
                return XBoxModeState;
            }
            set
            {
                XBoxModeState = value;
            }
        }
        private static ApplicationView AppView => ApplicationView.GetForCurrentView();
        private static CoreWindow CoreWindow => CoreWindow.GetForCurrentThread();
        public static ISet<VirtualKey> PressedKeys { get; } = new HashSet<VirtualKey>();

        public static bool FullScreenChangingPossible
        {
            get
            {
                var output = DeviceFamiliesAllowingFullScreenChange.Contains(AnalyticsInfo.VersionInfo.DeviceFamily);
                return output;
            }
        }

        public static bool IsFullScreenMode => AppView.IsFullScreenMode;

        public static bool TouchScreenAvailable => new TouchCapabilities().TouchPresent > 0;

        public static bool ShouldDisplayTouchGamepad
        {
            get
            {
                return false;
            }
        }

        private static bool handleGameplayKeyShortcuts = false;
        public static bool HandleGameplayKeyShortcuts
        {
            get => handleGameplayKeyShortcuts;
            set
            {
                PressedKeys.Clear();
                handleGameplayKeyShortcuts = value;
                var window = CoreWindow.GetForCurrentThread();
                window.KeyDown -= OnKeyDown;
                window.KeyUp -= OnKeyUp;
                if (handleGameplayKeyShortcuts)
                {
                    window.KeyDown += OnKeyDown;
                    window.KeyUp += OnKeyUp;
                }
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public static EventHandler<FullScreenChangeEventArgs> FullScreenChangeRequested;
        public static bool pageReady = false;
        public static bool isGamesList = false;
        public static bool gamesListAlreadyLoaded = false;
        public static bool ExtraDelay = false;
        public static bool DetectInputs = false;
        public static bool PreventGCAlways = false;
        public static EventHandler ReloadCorePageGlobal;
        public static EventHandler VFSIndicatorHandler;
        public static EventHandler LogIndicatorHandler;
        public static EventHandler LEDIndicatorHandler;
        public static EventHandler AddNewActionButton;
        public static EventHandler UpdateButtonsMap;
        public static event EventHandler RestoreGamesListStateGlobal;
        public static GameSystemRecentModel gameSystemViewModel = null;
        public static int pivotPosition = 0;
        public static double pivotMainPosition = 0;
        static double vscrollS = 0;
        public static double vXboxScrollPosition = 0;
        public static double vScrollS
        {
            get
            {
                return vscrollS;
            }
            set
            {
                vscrollS = value;
            }
        }
        static double vscroll = 0;
        public static double vScroll
        {
            get
            {
                return vscroll;
            }
            set
            {
                vscroll = value;
            }
        }
        public static double veScroll
        {
            get
            {
                return vScroll;
            }
        }
        public static void RestoreGamesListState(double currentIndex)
        {
            if (RestoreGamesListStateGlobal != null)
            {
                RestoreGamesListStateGlobal.Invoke(currentIndex, EventArgs.Empty);
            }
        }
        public static EventHandler SaveListStateGlobal;
        public static EventHandler NotificationHandler;
        public static EventHandler NotificationHandlerMain;
        public static bool ShowNotification(string text, int time)
        {
            if (NotificationHandler == null)
            {
                return false;
            }
            try
            {
                LocalNotificationData localNotificationData = new LocalNotificationData();
                localNotificationData.icon = SegoeMDL2Assets.ActionCenterNotification;
                localNotificationData.message = text;
                localNotificationData.time = time;
                NotificationHandler(null, localNotificationData);
            }
            catch (Exception x)
            {
                return false;
            }
            return true;
        }
        public static bool ShowNotificationDirect(string text, int time = 3)
        {
            if (NotificationHandler == null)
            {
                return false;
            }
            try
            {
                LocalNotificationData localNotificationData = new LocalNotificationData();
                localNotificationData.icon = SegoeMDL2Assets.ActionCenterNotification;
                localNotificationData.message = text;
                localNotificationData.time = time;
                NotificationHandler(null, localNotificationData);
            }
            catch (Exception x)
            {
                return false;
            }
            return true;
        }
        public static bool ShowNotificationMain(string text, int time)
        {
            if (NotificationHandlerMain == null)
            {
                return false;
            }
            try
            {
                LocalNotificationData localNotificationData = new LocalNotificationData();
                localNotificationData.icon = SegoeMDL2Assets.ActionCenterNotification;
                localNotificationData.message = text;
                localNotificationData.time = time;
                NotificationHandlerMain(null, localNotificationData);
            }
            catch (Exception x)
            {
                return false;
            }
            return true;
        }

        public static void SaveGamesListState()
        {
            if (SaveListStateGlobal != null)
            {
                SaveListStateGlobal.Invoke(null, EventArgs.Empty);
            }
        }
        public static void SaveGamesListStateDirect()
        {
            if (SaveListStateGlobal != null)
            {
                SaveListStateGlobal.Invoke(null, EventArgs.Empty);
            }
        }
        public static bool XBoxMenuActive = false;
        public static bool ScaleMenuActive = false;
        public static EventHandler PauseToggleRequested;
        public static EventHandler XBoxMenuRequested;
        public static EventHandler QuickSaveRequested;
        public static EventHandler SavesListRequested;
        public static EventHandler ChangeToXBoxModeRequested;


        public static EventHandler<GameStateOperationEventArgs> GameStateOperationRequested;

        public static async Task<bool> ChangeFullScreenStateAsync(FullScreenChangeType changeType)
        {
            if ((changeType == FullScreenChangeType.Enter && IsFullScreenMode) || (changeType == FullScreenChangeType.Exit && !IsFullScreenMode))
            {
                return true;
            }

            if (changeType == FullScreenChangeType.Toggle)
            {
                changeType = IsFullScreenMode ? FullScreenChangeType.Exit : FullScreenChangeType.Enter;
            }

            var result = false;
            switch (changeType)
            {
                case FullScreenChangeType.Enter:
                    result = AppView.TryEnterFullScreenMode();
                    break;
                case FullScreenChangeType.Exit:
                    AppView.ExitFullScreenMode();
                    result = true;
                    break;
                default:
                    throw new Exception("null should never happen");
            }

            await Task.Delay(100);
            return result;
        }


        public static Timer MemoryTimer;

        public static string GetMemoryUsageDirect()
        {
            try
            {
                var appMemory = (long)MemoryManager.AppMemoryUsage;
                return appMemory.ToFileSize();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static string GetMemoryUsage()
        {
            try
            {
                var appMemory = (long)MemoryManager.AppMemoryUsage;
                return appMemory.ToFileSize();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static long GetMemoryUsageLong()
        {
            try
            {
                var appMemory = (long)MemoryManager.AppMemoryUsage;
                return appMemory;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static void ChangeMousePointerVisibility(MousePointerVisibility visibility)
        {
            var pointer = visibility == MousePointerVisibility.Hidden ? null : new CoreCursor(CoreCursorType.Arrow, 0);
            Window.Current.CoreWindow.PointerCursor = pointer;
        }

        public static void ForceUIElementFocus()
        {
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
        }

        public static void CopyToClipboard(string content)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(content);
            Clipboard.SetContent(dataPackage);
        }

        public static bool isTablet
        {
            get
            {
                return FormFactorString.Equals("Tablet");
            }
        }

        public static bool DPadActive = false;
        public static bool isXBOX
        {
            get
            {
                return FormFactorString.Equals("Xbox") || DPadActive;
            }
        }
        public static bool isXBOXPure
        {
            get
            {
                return FormFactorString.Equals("Xbox");
            }
        }
        public static bool isMobile
        {
            get
            {
                return FormFactorString.Equals("Mobile");
            }
        }
        public static bool isDesktop
        {
            get
            {
                return FormFactorString.Equals("Desktop");
            }
        }
        public static string FormFactorString
        {
            get
            {
                string formFactorS = "Unknown";
                try
                {
                    {
                        switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                        {
                            case "Windows.Mobile":
                                formFactorS = "Mobile";
                                break;
                            case "Windows.Desktop":
                                var mouseState = false;
                                try
                                {
                                    mouseState = InputService.isMouseAvailable();
                                }
                                catch (Exception ex)
                                {

                                }
                                formFactorS =
                                    mouseState
                                    ? "Desktop"
                                    : "Tablet";
                                break;
                            case "Windows.Universal":
                                formFactorS = "IoT";
                                break;
                            case "Windows.Team":
                                formFactorS = "SurfaceHub";
                                break;
                            case "Windows.Xbox":
                                formFactorS = "Xbox";
                                break;
                            default:
                                formFactorS = "Unknown";
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    return "Mobile";
                }
                return formFactorS;
            }
        }

        public static bool useNativeKeyboardByDefault = true;
        public static bool useNativeKeyboard = false;
        public static bool KeyboardVisibleState = false;
        public static bool KeyboardMapVisible = false;
        public static EventHandler HideKeyboardHandler;
        public static EventHandler ShowKeyboardHandler;
        public static List<KeyboardKey> keyboardKeys = new List<KeyboardKey>()
        {
            { new KeyboardKey("1", "Main", Keys.RETROK_1, "!") },
            { new KeyboardKey("2", "Main", Keys.RETROK_2, "@") },
            { new KeyboardKey("3", "Main", Keys.RETROK_3, "#") },
            { new KeyboardKey("4", "Main", Keys.RETROK_4, "$") },
            { new KeyboardKey("5", "Main", Keys.RETROK_5, "%") },
            { new KeyboardKey("6", "Main", Keys.RETROK_6, "^") },
            { new KeyboardKey("7", "Main", Keys.RETROK_7, "&") },
            { new KeyboardKey("8", "Main", Keys.RETROK_8, "*") },
            { new KeyboardKey("9", "Main", Keys.RETROK_9, "(") },
            { new KeyboardKey("0", "Main", Keys.RETROK_0, ")") },
            { new KeyboardKey("-", "Main", Keys.RETROK_MINUS, "_") },
            { new KeyboardKey("=", "Main", Keys.RETROK_EQUALS, "+") },
            { new KeyboardKey("`", "Main", (Keys)192, "~") },

            { new KeyboardKey("Q", "Main", Keys.RETROK_q) },
            { new KeyboardKey("W", "Main", Keys.RETROK_w) },
            { new KeyboardKey("E", "Main", Keys.RETROK_e) },
            { new KeyboardKey("R", "Main", Keys.RETROK_r) },
            { new KeyboardKey("T", "Main", Keys.RETROK_t) },
            { new KeyboardKey("Y", "Main", Keys.RETROK_y) },
            { new KeyboardKey("U", "Main", Keys.RETROK_u) },
            { new KeyboardKey("I", "Main", Keys.RETROK_i) },
            { new KeyboardKey("O", "Main", Keys.RETROK_o) },
            { new KeyboardKey("P", "Main", Keys.RETROK_p) },
            { new KeyboardKey("[", "Main", Keys.RETROK_LEFTBRACKET, "{") },
            { new KeyboardKey("]", "Main", Keys.RETROK_RIGHTBRACKET, "}") },
            { new KeyboardKey("Back", "Main", Keys.RETROK_BACKSPACE) },


            { new KeyboardKey("A", "Main", Keys.RETROK_a) },
            { new KeyboardKey("S", "Main", Keys.RETROK_s) },
            { new KeyboardKey("D", "Main", Keys.RETROK_d) },
            { new KeyboardKey("F", "Main", Keys.RETROK_f) },
            { new KeyboardKey("G", "Main", Keys.RETROK_g) },
            { new KeyboardKey("H", "Main", Keys.RETROK_h) },
            { new KeyboardKey("J", "Main", Keys.RETROK_j) },
            { new KeyboardKey("K", "Main", Keys.RETROK_k) },
            { new KeyboardKey("L", "Main", Keys.RETROK_l) },
            { new KeyboardKey(";", "Main", Keys.RETROK_SEMICOLON, ":") },
            { new KeyboardKey("'", "Main", Keys.RETROK_QUOTE, "\"") },
            { new KeyboardKey("\\", "Main", Keys.RETROK_BACKSLASH, "|") },
            { new KeyboardKey("Enter", "Main", Keys.RETROK_RETURN) },


            { new KeyboardKey("Z", "Main", Keys.RETROK_z) },
            { new KeyboardKey("X", "Main", Keys.RETROK_x) },
            { new KeyboardKey("C", "Main", Keys.RETROK_c) },
            { new KeyboardKey("V", "Main", Keys.RETROK_v) },
            { new KeyboardKey("B", "Main", Keys.RETROK_b) },
            { new KeyboardKey("N", "Main", Keys.RETROK_n) },
            { new KeyboardKey("M", "Main", Keys.RETROK_m) },
            { new KeyboardKey(",", "Main", Keys.RETROK_COMMA, "<") },
            { new KeyboardKey(".", "Main", Keys.RETROK_PERIOD, ">") },
            { new KeyboardKey("/", "Main", Keys.RETROK_SLASH, "?") },
            { new KeyboardKey("Space", "Main", Keys.RETROK_SPACE) },
            { new KeyboardKey("Del", "Main", Keys.RETROK_DELETE) },
            { new KeyboardKey("Caps", "Main", Keys.RETROK_CAPSLOCK) },

            /*
            { new KeyboardKey("A", "Chars", Keys.RETROK_a) },
            { new KeyboardKey("B", "Chars", Keys.RETROK_b) },
            { new KeyboardKey("C", "Chars", Keys.RETROK_c) },
            { new KeyboardKey("D", "Chars", Keys.RETROK_d) },
            { new KeyboardKey("E", "Chars", Keys.RETROK_e) },
            { new KeyboardKey("F", "Chars", Keys.RETROK_f) },
            { new KeyboardKey("G", "Chars", Keys.RETROK_g) },
            { new KeyboardKey("H", "Chars", Keys.RETROK_h) },
            { new KeyboardKey("I", "Chars", Keys.RETROK_i) },
            { new KeyboardKey("J", "Chars", Keys.RETROK_j) },
            { new KeyboardKey("K", "Chars", Keys.RETROK_k) },
            { new KeyboardKey("L", "Chars", Keys.RETROK_l) },
            { new KeyboardKey("M", "Chars", Keys.RETROK_m) },
            { new KeyboardKey("N", "Chars", Keys.RETROK_n) },
            { new KeyboardKey("O", "Chars", Keys.RETROK_o) },
            { new KeyboardKey("P", "Chars", Keys.RETROK_p) },
            { new KeyboardKey("Q", "Chars", Keys.RETROK_q) },
            { new KeyboardKey("R", "Chars", Keys.RETROK_r) },
            { new KeyboardKey("S", "Chars", Keys.RETROK_s) },
            { new KeyboardKey("T", "Chars", Keys.RETROK_t) },
            { new KeyboardKey("U", "Chars", Keys.RETROK_u) },
            { new KeyboardKey("V", "Chars", Keys.RETROK_v) },
            { new KeyboardKey("W", "Chars", Keys.RETROK_w) },
            { new KeyboardKey("X", "Chars", Keys.RETROK_x) },
            { new KeyboardKey("Y", "Chars", Keys.RETROK_y) },
            { new KeyboardKey("Z", "Chars", Keys.RETROK_z) },
            */

            /*
            { new KeyboardKey("[", "Symbols", Keys.RETROK_LEFTBRACKET, "{") },
            { new KeyboardKey("]", "Symbols", Keys.RETROK_RIGHTBRACKET, "}") },
            { new KeyboardKey(";", "Symbols", Keys.RETROK_SEMICOLON, ":") },
            { new KeyboardKey("'", "Symbols", Keys.RETROK_QUOTE, "\"") },
            { new KeyboardKey("\\", "Symbols", Keys.RETROK_BACKSLASH, "|") },
            { new KeyboardKey(",", "Symbols", Keys.RETROK_COMMA, "<") },
            { new KeyboardKey(".", "Symbols", Keys.RETROK_PERIOD, ">") },
            { new KeyboardKey("/", "Symbols", Keys.RETROK_SLASH, "?") },
            */

            { new KeyboardKey("Up", "Main", Keys.RETROK_UP) },
            { new KeyboardKey("Down", "Main", Keys.RETROK_DOWN) },
            { new KeyboardKey("Left", "Main", Keys.RETROK_LEFT) },
            { new KeyboardKey("Right", "Main", Keys.RETROK_RIGHT) },
            { new KeyboardKey("Win", "Main", Keys.RETROK_MENU) },
            { new KeyboardKey("Tab", "Main", Keys.RETROK_TAB) },
            { new KeyboardKey("Home", "Main", Keys.RETROK_HOME) },
            { new KeyboardKey("End", "Main", Keys.RETROK_END) },
            { new KeyboardKey("Insert", "Main", Keys.RETROK_INSERT) },
            { new KeyboardKey("P.Up", "Main", Keys.RETROK_PAGEUP) },
            { new KeyboardKey("P.Down", "Main", Keys.RETROK_PAGEDOWN) },



            { new KeyboardKey("F1", "Functions", Keys.RETROK_F1) },
            { new KeyboardKey("F2", "Functions", Keys.RETROK_F2) },
            { new KeyboardKey("F3", "Functions", Keys.RETROK_F3) },
            { new KeyboardKey("F4", "Functions", Keys.RETROK_F4) },
            { new KeyboardKey("F5", "Functions", Keys.RETROK_F5) },
            { new KeyboardKey("F6", "Functions", Keys.RETROK_F6) },
            { new KeyboardKey("F7", "Functions", Keys.RETROK_F7) },
            { new KeyboardKey("F8", "Functions", Keys.RETROK_F8) },
            { new KeyboardKey("F9", "Functions", Keys.RETROK_F9) },
            { new KeyboardKey("F10", "Functions", Keys.RETROK_F10) },
            { new KeyboardKey("F11", "Functions", Keys.RETROK_F11) },
            { new KeyboardKey("F12", "Functions", Keys.RETROK_F12) },

            { new KeyboardKey("1", "Numbers Pad", Keys.RETROK_KP1) },
            { new KeyboardKey("2", "Numbers Pad", Keys.RETROK_KP2) },
            { new KeyboardKey("3", "Numbers Pad", Keys.RETROK_KP3) },
            { new KeyboardKey("4", "Numbers Pad", Keys.RETROK_KP4) },
            { new KeyboardKey("5", "Numbers Pad", Keys.RETROK_KP5) },
            { new KeyboardKey("6", "Numbers Pad", Keys.RETROK_KP6) },
            { new KeyboardKey("7", "Numbers Pad", Keys.RETROK_KP7) },
            { new KeyboardKey("8", "Numbers Pad", Keys.RETROK_KP8) },
            { new KeyboardKey("9", "Numbers Pad", Keys.RETROK_KP9) },
            { new KeyboardKey("0", "Numbers Pad", Keys.RETROK_KP0) },
            { new KeyboardKey("+", "Numbers Pad", Keys.RETROK_KP_PLUS) },
            { new KeyboardKey("-", "Numbers Pad", Keys.RETROK_KP_MINUS) },
            { new KeyboardKey("*", "Numbers Pad", Keys.RETROK_KP_MULTIPLY) },
            { new KeyboardKey("/", "Numbers Pad", Keys.RETROK_KP_DIVIDE) },
            { new KeyboardKey("=", "Numbers Pad", Keys.RETROK_KP_EQUALS) },
            { new KeyboardKey(",", "Numbers Pad", Keys.RETROK_KP_PERIOD, ".") },
            { new KeyboardKey("Enter", "Numbers Pad", Keys.RETROK_KP_ENTER) },
        };

        public static Dictionary<VirtualKey, string> KeyboardKeysStringMap = new Dictionary<VirtualKey, string>()
        {
            { VirtualKey.Up, "Up" },
            { VirtualKey.Down, "Down" },
            { VirtualKey.Left, "Left" },
            { VirtualKey.Right, "Right" },

            { VirtualKey.Space, "Space" },

            { VirtualKey.A, "A" },
            { VirtualKey.B, "B" },
            { VirtualKey.C, "C" },
            { VirtualKey.D, "D" },
            { VirtualKey.E, "E" },
            { VirtualKey.F, "F" },
            { VirtualKey.G, "G" },
            { VirtualKey.H, "H" },
            { VirtualKey.I, "I" },
            { VirtualKey.J, "J" },
            { VirtualKey.K, "K" },
            { VirtualKey.L, "L" },
            { VirtualKey.M, "M" },
            { VirtualKey.N, "N" },
            { VirtualKey.O, "O" },
            { VirtualKey.P, "P" },
            { VirtualKey.Q, "Q" },
            { VirtualKey.R, "R" },
            { VirtualKey.S, "S" },
            { VirtualKey.T, "T" },
            { VirtualKey.U, "U" },
            { VirtualKey.V, "V" },
            { VirtualKey.W, "W" },
            { VirtualKey.X, "X" },
            { VirtualKey.Y, "Y" },
            { VirtualKey.Z, "Z" },

            { VirtualKey.Number0, "0" },
            { VirtualKey.Number1, "1" },
            { VirtualKey.Number2, "2" },
            { VirtualKey.Number3, "3" },
            { VirtualKey.Number4, "4" },
            { VirtualKey.Number5, "5" },
            { VirtualKey.Number6, "6" },
            { VirtualKey.Number7, "7" },
            { VirtualKey.Number8, "8" },
            { VirtualKey.Number9, "9" },

            { VirtualKey.NumberPad0, "K.Pad 0" },
            { VirtualKey.NumberPad1, "K.Pad 1" },
            { VirtualKey.NumberPad2, "K.Pad 2" },
            { VirtualKey.NumberPad3, "K.Pad 3" },
            { VirtualKey.NumberPad4, "K.Pad 4" },
            { VirtualKey.NumberPad5, "K.Pad 5" },
            { VirtualKey.NumberPad6, "K.Pad 6" },
            { VirtualKey.NumberPad7, "K.Pad 7" },
            { VirtualKey.NumberPad8, "K.Pad 8" },
            { VirtualKey.NumberPad9, "K.Pad 9" },

            { VirtualKey.LeftWindows, "L.Windows" },
            { VirtualKey.RightWindows, "R.Windows" },

            { VirtualKey.Shift, "Shift" },
            { VirtualKey.RightShift, "R.Shift" },
            { VirtualKey.LeftShift, "L.Shift" },

            { VirtualKey.Control, "Ctrl" },
            { VirtualKey.LeftControl, "L.Ctrl" },
            { VirtualKey.RightControl, "R.Ctrl" },

            { VirtualKey.CapitalLock, "CapsLock" },
            { VirtualKey.Delete, "Delete" },
            { VirtualKey.Tab, "Tab" },
            { VirtualKey.LeftMenu, "L.Alt" },
            { VirtualKey.RightMenu, "R.Alt" },
            { VirtualKey.Menu, "Alt" },

            { VirtualKey.Enter, "Enter" },
            { VirtualKey.Print, "Print" },
            { VirtualKey.Help, "Help" },
            { VirtualKey.Back, "Back" },

            { VirtualKey.Home, "Home" },
            { VirtualKey.Insert, "Inset" },
            { VirtualKey.Escape, "Esc" },
            { VirtualKey.End, "End" },
            { VirtualKey.PageDown, "Page Down" },
            { VirtualKey.PageUp, "Page Up" },

            { VirtualKey.Divide, "K.Pad /" },
            { VirtualKey.Multiply, "K.Pad *" },
            { VirtualKey.Separator, "K.Pad ," },
            { VirtualKey.Add, "K.Pad +" },
            { VirtualKey.Subtract, "K.Pad -" },
            { VirtualKey.Decimal, "." },

            { VirtualKey.F1, "F1" },
            { VirtualKey.F2, "F2" },
            { VirtualKey.F3, "F3" },
            { VirtualKey.F4, "F4" },
            { VirtualKey.F5, "F5" },
            { VirtualKey.F6, "F6" },
            { VirtualKey.F7, "F7" },
            { VirtualKey.F8, "F8" },
            { VirtualKey.F9, "F9" },
            { VirtualKey.F10, "F10" },
            { VirtualKey.F11, "F11" },
            { VirtualKey.F12, "F12" },

            { (VirtualKey)188, ";" },
            { (VirtualKey)190, "," },
            { (VirtualKey)191, "/" },
            { (VirtualKey)219, "[" },
            { (VirtualKey)221, "]" },
            { (VirtualKey)220, "\\" },
            { (VirtualKey)186, "'" },
            { (VirtualKey)222, "\"" },
            { (VirtualKey)189, "-" },
            { (VirtualKey)187, "+" },
            { (VirtualKey)192, "=" },
        };
        public static Dictionary<VirtualKey, Keys> RetroKeysMap = new Dictionary<VirtualKey, Keys>()
        {
            { VirtualKey.A, Keys.RETROK_a },
            { VirtualKey.B, Keys.RETROK_b },
            { VirtualKey.C, Keys.RETROK_c },
            { VirtualKey.D, Keys.RETROK_d },
            { VirtualKey.E, Keys.RETROK_e },
            { VirtualKey.F, Keys.RETROK_f },
            { VirtualKey.G, Keys.RETROK_g },
            { VirtualKey.H, Keys.RETROK_h },
            { VirtualKey.I, Keys.RETROK_i },
            { VirtualKey.J, Keys.RETROK_j },
            { VirtualKey.K, Keys.RETROK_k },
            { VirtualKey.L, Keys.RETROK_l },
            { VirtualKey.M, Keys.RETROK_m },
            { VirtualKey.N, Keys.RETROK_n },
            { VirtualKey.O, Keys.RETROK_o },
            { VirtualKey.P, Keys.RETROK_p },
            { VirtualKey.Q, Keys.RETROK_q },
            { VirtualKey.R, Keys.RETROK_r },
            { VirtualKey.S, Keys.RETROK_s },
            { VirtualKey.T, Keys.RETROK_t },
            { VirtualKey.U, Keys.RETROK_u },
            { VirtualKey.V, Keys.RETROK_v },
            { VirtualKey.W, Keys.RETROK_w },
            { VirtualKey.X, Keys.RETROK_x },
            { VirtualKey.Y, Keys.RETROK_y },
            { VirtualKey.Z, Keys.RETROK_z },

            { VirtualKey.Number0, Keys.RETROK_0 },
            { VirtualKey.Number1, Keys.RETROK_1 },
            { VirtualKey.Number2, Keys.RETROK_2 },
            { VirtualKey.Number3, Keys.RETROK_3 },
            { VirtualKey.Number4, Keys.RETROK_4 },
            { VirtualKey.Number5, Keys.RETROK_5 },
            { VirtualKey.Number6, Keys.RETROK_6 },
            { VirtualKey.Number7, Keys.RETROK_7 },
            { VirtualKey.Number8, Keys.RETROK_8 },
            { VirtualKey.Number9, Keys.RETROK_9 },

            { VirtualKey.NumberPad0, Keys.RETROK_KP0 },
            { VirtualKey.NumberPad1, Keys.RETROK_KP1 },
            { VirtualKey.NumberPad2, Keys.RETROK_KP2 },
            { VirtualKey.NumberPad3, Keys.RETROK_KP3 },
            { VirtualKey.NumberPad4, Keys.RETROK_KP4 },
            { VirtualKey.NumberPad5, Keys.RETROK_KP5 },
            { VirtualKey.NumberPad6, Keys.RETROK_KP6 },
            { VirtualKey.NumberPad7, Keys.RETROK_KP7 },
            { VirtualKey.NumberPad8, Keys.RETROK_KP8 },
            { VirtualKey.NumberPad9, Keys.RETROK_KP9 },

            { VirtualKey.Up, Keys.RETROK_UP },
            { VirtualKey.Down, Keys.RETROK_DOWN },
            { VirtualKey.Left, Keys.RETROK_LEFT },
            { VirtualKey.Right, Keys.RETROK_RIGHT },
            { VirtualKey.LeftWindows, Keys.RETROK_MENU },
            { VirtualKey.RightWindows, Keys.RETROK_MENU },

            { VirtualKey.RightShift, Keys.RETROK_RSHIFT },
            { VirtualKey.LeftShift, Keys.RETROK_LSHIFT },
            { VirtualKey.Shift, Keys.RETROK_RSHIFT },
            { VirtualKey.LeftControl, Keys.RETROK_LCTRL },
            { VirtualKey.RightControl, Keys.RETROK_RCTRL },
            { VirtualKey.Control, Keys.RETROK_RCTRL },
            { VirtualKey.CapitalLock, Keys.RETROK_CAPSLOCK },
            { VirtualKey.Delete, Keys.RETROK_DELETE },
            { VirtualKey.Tab, Keys.RETROK_TAB },
            { VirtualKey.LeftMenu, Keys.RETROK_LALT },
            { VirtualKey.RightMenu, Keys.RETROK_RALT },
            { VirtualKey.Menu, Keys.RETROK_LALT },

            { VirtualKey.Enter, Keys.RETROK_RETURN },
            { VirtualKey.Print, Keys.RETROK_PRINT },
            { VirtualKey.Help, Keys.RETROK_HELP },
            { VirtualKey.Back, Keys.RETROK_BACKSPACE },

            { VirtualKey.Space, Keys.RETROK_SPACE },
            { VirtualKey.Home, Keys.RETROK_HOME },
            { VirtualKey.Insert, Keys.RETROK_INSERT },
            { VirtualKey.Escape, Keys.RETROK_ESCAPE },
            { VirtualKey.End, Keys.RETROK_END },
            { VirtualKey.PageDown, Keys.RETROK_PAGEDOWN },
            { VirtualKey.PageUp, Keys.RETROK_PAGEUP },

            { VirtualKey.Divide, Keys.RETROK_KP_DIVIDE },
            { VirtualKey.Multiply, Keys.RETROK_KP_MULTIPLY },
            { VirtualKey.Separator, Keys.RETROK_KP_PERIOD },
            { VirtualKey.Add, Keys.RETROK_KP_PLUS },
            { VirtualKey.Subtract, Keys.RETROK_KP_MINUS },
            { VirtualKey.Decimal, Keys.RETROK_KP_PERIOD },

            { VirtualKey.F1, Keys.RETROK_F1 },
            { VirtualKey.F2, Keys.RETROK_F2 },
            { VirtualKey.F3, Keys.RETROK_F3 },
            { VirtualKey.F4, Keys.RETROK_F4 },
            { VirtualKey.F5, Keys.RETROK_F5 },
            { VirtualKey.F6, Keys.RETROK_F6 },
            { VirtualKey.F7, Keys.RETROK_F7 },
            { VirtualKey.F8, Keys.RETROK_F8 },
            { VirtualKey.F9, Keys.RETROK_F9 },
            { VirtualKey.F10, Keys.RETROK_F10 },
            { VirtualKey.F11, Keys.RETROK_F11 },
            { VirtualKey.F12, Keys.RETROK_F12 },

            { (VirtualKey)188, Keys.RETROK_COMMA },
            { (VirtualKey)190, Keys.RETROK_PERIOD },
            { (VirtualKey)191, Keys.RETROK_SLASH },
            { (VirtualKey)219, Keys.RETROK_LEFTBRACKET },
            { (VirtualKey)221, Keys.RETROK_RIGHTBRACKET },
            { (VirtualKey)220, Keys.RETROK_BACKSLASH },
            { (VirtualKey)186, Keys.RETROK_SEMICOLON },
            { (VirtualKey)222, Keys.RETROK_QUOTE },
            { (VirtualKey)189, Keys.RETROK_MINUS },
            { (VirtualKey)187, Keys.RETROK_PLUS },
            { (VirtualKey)192, Keys.RETROK_EQUALS },
        };
        public static void InvokeEkeyboardEvent(bool state, VirtualKey virtualKey)
        {
            if (PlatformService.KeyboardEvent != null)
            {
                var Key = Keys.RETROK_UNKNOWN;
                if (RetroKeysMap.TryGetValue(virtualKey, out Key))
                {
                    PlatformService.KeyboardEvent.Invoke(state, (uint)Key, (uint)Key, 0);
                }
                else
                {
                    PlatformService.KeyboardEvent.Invoke(state, (uint)virtualKey, (uint)virtualKey, 0);
                }
            }
        }

        public static bool CoreOptionsDropdownOpened = false;
        public static bool backRequested = false;
        private static void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {

            if (SavesListActive && args.VirtualKey == VirtualKey.GamepadX)
            {

            }
            else
            {
                args.Handled = true;
            }

            var key = args.VirtualKey;

            if (KeyboardVisibleState && DPadActive)
            {

            }
            else
            if (!XBoxMenuActive && !KeyboardMapVisible && !ScaleMenuActive && !SavesListActive && !CoreOptionsActive && !ControlsActive && !EffectsActive && !ShortcutsVisible)
            {
                if (!PressedKeys.Contains(key))
                {
                    PressedKeys.Add(key);
                }
                InvokeEkeyboardEvent(true, key);
            }
            bool BackCalled = false;
            bool isKeyBoundForGame = false;

            if (InputService.CustomRulesLoaded)
            {
                foreach (var kItem in InputService.KeyboardMapRulesList)
                {
                    if (kItem.NewKey.Equals((uint)key))
                    {
                        isKeyBoundForGame = true;
                        break;
                    }
                }
            }

            if (!isKeyBoundForGame)
            {
                switch (key)
                {
                    //Shift+Enter: enter fullscreen
                    case VirtualKey.Enter:
                        if (PressedKeys.Contains(VirtualKey.Shift))
                        {
                            FullScreenChangeRequested(null, new FullScreenChangeEventArgs(FullScreenChangeType.Toggle));
                        }
                        break;

                    case VirtualKey.Escape:
                        if (IsFullScreenMode)
                        {
                            FullScreenChangeRequested(null, new FullScreenChangeEventArgs(FullScreenChangeType.Exit));
                        }
                        else
                        {
                            BackCalled = true;
                        }
                        break;
                    case VirtualKey.P:
                        if (PressedKeys.Contains(VirtualKey.Shift))
                        {
                            PauseToggleRequested(null, EventArgs.Empty);
                        }
                        break;
                    case VirtualKey.GamepadView:
                        if (PressedKeys.Contains(VirtualKey.GamepadMenu))
                        {
                            PauseToggleRequested(null, EventArgs.Empty);
                        }
                        if (ActionVisibile)
                        {
                            if (ResetButtonHandler != null)
                            {
                                ResetButtonHandler.Invoke(null, null);
                            }
                        }
                        break;

                    case VirtualKey.F1:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(1);
                        }
                        break;

                    case VirtualKey.F2:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(2);
                        }
                        break;

                    case VirtualKey.F3:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(3);
                        }
                        break;

                    case VirtualKey.F4:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(4);
                        }
                        break;

                    case VirtualKey.F5:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(5);
                        }
                        break;

                    case VirtualKey.F6:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(6);
                        }
                        break;
                    case VirtualKey.F7:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(7);
                        }
                        break;
                    case VirtualKey.F8:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(8);
                        }
                        break;
                    case VirtualKey.F9:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(9);
                        }
                        break;
                    case VirtualKey.F10:
                        if (!useNativeKeyboard)
                        {
                            HandleSaveSlotKeyPress(10);
                        }
                        break;
                    case VirtualKey.Number7:
                        if (!useNativeKeyboard)
                        {
                            HandleLoadSlotKeyPress(7);
                        }
                        break;
                    case VirtualKey.Number8:
                        if (!useNativeKeyboard)
                        {
                            HandleLoadSlotKeyPress(8);
                        }
                        break;
                    case VirtualKey.Number9:
                        if (!useNativeKeyboard)
                        {
                            HandleLoadSlotKeyPress(9);
                        }
                        break;
                    case VirtualKey.Number1:
                        if (!useNativeKeyboard)
                        {
                            HandleActionSlotKeyPress(1);
                        }
                        break;
                    case VirtualKey.Number2:
                        if (!useNativeKeyboard)
                        {
                            HandleActionSlotKeyPress(2);
                        }
                        break;
                    case VirtualKey.Number3:
                        if (!useNativeKeyboard)
                        {
                            HandleActionSlotKeyPress(3);
                        }
                        break;
                    case VirtualKey.Number4:
                        if (!useNativeKeyboard)
                        {
                            HandleActionSlotKeyPress(4);
                        }
                        break;
                    case VirtualKey.Number6:
                        if (!useNativeKeyboard)
                        {
                            XBoxMenuRequested(null, EventArgs.Empty);
                        }
                        break;
                    case VirtualKey.GamepadB:
                        {
                            BackCalled = true;
                        }
                        break;
                    case VirtualKey.GamepadMenu:
                        if (ActionVisibile)
                        {
                            if (SaveButtonHandler != null)
                            {
                                SaveButtonHandler.Invoke(null, null);
                            }
                        }
                        break;
                    case VirtualKey.GamepadLeftThumbstickButton:
                        if (ActionVisibile)
                        {
                            if (ActionVisibile)
                            {
                                if (CancelButtonHandler != null)
                                {
                                    CancelButtonHandler.Invoke(null, null);
                                }
                            }
                        }
                        break;
                }
                if (BackCalled)
                {
                    if (CoreOptionsDropdownOpened)
                    {
                        CoreOptionsDropdownOpened = false;
                        return;
                    }
                    else if (ShortcutsVisible)
                    {
                        return;
                    }
                    else if ((XBoxMenuActive || KeyboardMapVisible || LogsVisibile || CoreOptionsActive || SavesListActive || EffectsActive || ControlsActive || KeyboardVisibleState) && !backRequested && PlatformService.ShowNotificationDirect("Press again to go back", 2))
                    {
                        backRequested = true;
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(3000);
                            backRequested = false;
                        });
                    }
                    else
                    {
                        if (XBoxMenuActive)
                        {
                            XBoxMenuRequested(null, EventArgs.Empty);
                            PlayNotificationSoundDirect("option-changed");
                        }
                        else if (LogsVisibile)
                        {
                            if (HideLogsHandler != null)
                            {
                                HideLogsHandler.Invoke(null, null);
                            }
                            PlayNotificationSoundDirect("option-changed");
                        }
                        else
                        if (CoreOptionsActive)
                        {
                            InvokeHideCoreOptionsHandler();
                            PlayNotificationSoundDirect("option-changed");
                        }
                        else if (SavesListActive)
                        {
                            InvokeHideSavesListHandler();
                            PlayNotificationSoundDirect("option-changed");
                        }
                        else if (EffectsActive)
                        {
                            if (HideEffects != null)
                            {
                                HideEffects.Invoke(null, null);
                            }
                            PlayNotificationSoundDirect("option-changed");
                        }
                        else if (ControlsActive)
                        {
                            InvokeHideControlsListHandler();
                            PlayNotificationSoundDirect("option-changed");
                        }
                        else if (KeyboardVisibleState)
                        {
                            if (HideKeyboardHandler != null)
                            {
                                HideKeyboardHandler.Invoke(null, null);
                                PlayNotificationSoundDirect("option-changed");
                            }
                        }
                    }
                }
            }
            if (PlatformService.MouseSate && RightClick != null && LeftClick != null)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.GamepadA:
                        RightClick.Invoke(null, null);
                        break;

                    case VirtualKey.GamepadB:
                        LeftClick.Invoke(null, null);
                        break;
                }
            }

            //Below just to show xbox buttons hints
            //And for actions usage
            if (isDPadKey(args.VirtualKey))
            {
                var tempState = PlatformService.DPadActive;
                PlatformService.DPadActive = true;
                if (!tempState && GamePlayPageUpdateBindings != null)
                {
                    GamePlayPageUpdateBindings.Invoke(null, null);
                }
                try
                {
                    var buttonKey = VirtualPadActions.VirtualButtonsMap[args.VirtualKey];
                    if (buttonKey.Length > 0)
                    {
                        if (AddNewActionButton != null)
                        {
                            AddNewActionButton.Invoke(buttonKey, null);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                var tempState2 = PlatformService.DPadActive;
                PlatformService.DPadActive = false;
                if (tempState2 && GamePlayPageUpdateBindings != null)
                {
                    GamePlayPageUpdateBindings.Invoke(null, null);
                }
            }

            bool ignoreShortcuts = false;
            if ((XBoxMenuActive || KeyboardMapVisible || ScaleMenuActive || LogsVisibile || CoreOptionsActive || SavesListActive || EffectsActive || ControlsActive || KeyboardVisibleState))
            {
                ignoreShortcuts = true;
            }

            if (!ignoreShortcuts)
            {
                foreach (var sItem in shortcutEntries)
                {
                    var ShortcutTriggered = true;
                    if (sItem.keys.Count > 0)
                    {
                        foreach (var kItem in sItem.keys)
                        {
                            var vk = GamePadStringToVirtual[kItem];
                            if (key != vk && !PressedKeys.Contains(vk))
                            {
                                ShortcutTriggered = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        ShortcutTriggered = false;
                    }
                    if (ShortcutTriggered)
                    {
                        switch (sItem.action)
                        {
                            case "Fast Forward":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    FastForwardRequested(null, EventArgs.Empty);
                                }
                                break;

                            case "In-Game Menu":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    XBoxMenuRequested(null, EventArgs.Empty);
                                }
                                break;

                            case "Quick Save":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    QuickSaveRequested(null, EventArgs.Empty);
                                }
                                break;

                            case "Quick Load":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    QuickLoadRequested(null, EventArgs.Empty);
                                }
                                break;

                            case "Snapshot":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    SnapshotRequested(null, EventArgs.Empty);
                                }
                                break;

                            case "Saves List":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    SavesListRequested(null, EventArgs.Empty);
                                }
                                break;

                            case "Pause":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    PauseRequested(null, EventArgs.Empty);
                                }
                                break;

                            case "Resume":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    ResumeRequested(null, EventArgs.Empty);
                                }
                                break;

                            case "Stop":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    StopRequested(null, EventArgs.Empty);
                                }
                                break;

                            case "Actions 1":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    A1Requested(null, EventArgs.Empty);
                                }
                                break;

                            case "Actions 2":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    A2Requested(null, EventArgs.Empty);
                                }
                                break;

                            case "Actions 3":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    A3Requested(null, EventArgs.Empty);
                                }
                                break;

                            case "Keyboard":
                                if (!CommandInProgress)
                                {
                                    CommandInProgress = true;
                                    KeyboardRequested(null, EventArgs.Empty);
                                }
                                break;
                        }
                    }
                }
            }

        }
        public static bool isDPadKey(VirtualKey key)
        {
            switch (key)
            {
                case VirtualKey.GamepadDPadDown:
                case VirtualKey.GamepadDPadUp:
                case VirtualKey.GamepadDPadLeft:
                case VirtualKey.GamepadDPadRight:
                case VirtualKey.GamepadA:
                case VirtualKey.GamepadB:
                case VirtualKey.GamepadX:
                case VirtualKey.GamepadY:
                case VirtualKey.GamepadLeftShoulder:
                case VirtualKey.GamepadLeftThumbstickButton:
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.GamepadRightThumbstickButton:
                case VirtualKey.GamepadRightThumbstickDown:
                case VirtualKey.GamepadRightThumbstickLeft:
                case VirtualKey.GamepadRightThumbstickUp:
                case VirtualKey.GamepadRightThumbstickRight:
                case VirtualKey.GamepadLeftTrigger:
                case VirtualKey.GamepadRightTrigger:
                case VirtualKey.GamepadRightShoulder:
                case VirtualKey.GamepadView:
                case VirtualKey.GamepadMenu:
                    return true;

                default:
                    return false;
            }
        }
        public static EventHandler KeyboardRequestedHandler;
        private static void KeyboardRequested(object value, EventArgs empty)
        {
            if (KeyboardRequestedHandler != null)
            {
                KeyboardRequestedHandler.Invoke(null, null);
            }
        }

        public static EventHandler A3RequestededHandler;

        private static void A3Requested(object value, EventArgs empty)
        {
            if (A3RequestededHandler != null)
            {
                A3RequestededHandler.Invoke(null, null);
            }
        }

        public static EventHandler A2RequestedHandler;

        private static void A2Requested(object value, EventArgs empty)
        {
            if (A2RequestedHandler != null)
            {
                A2RequestedHandler.Invoke(null, null);
            }
        }

        public static EventHandler A1RequesteddHandler;

        private static void A1Requested(object value, EventArgs empty)
        {
            if (A1RequesteddHandler != null)
            {
                A1RequesteddHandler.Invoke(null, null);
            }
        }

        public static EventHandler StopRequestedHandler;

        private static void StopRequested(object value, EventArgs empty)
        {
            if (StopRequestedHandler != null)
            {
                StopRequestedHandler.Invoke(null, null);
            }
        }

        public static EventHandler ResumeRequestedHandler;

        private static void ResumeRequested(object value, EventArgs empty)
        {
            if (ResumeRequestedHandler != null)
            {
                ResumeRequestedHandler.Invoke(null, null);
            }
        }

        public static EventHandler PauseRequestedHandler;

        private static void PauseRequested(object value, EventArgs empty)
        {
            if (PauseRequestedHandler != null)
            {
                PauseRequestedHandler.Invoke(null, null);
            }
        }

        public static EventHandler SnapshotRequestedHandler;

        private static void SnapshotRequested(object value, EventArgs empty)
        {
            if (SnapshotRequestedHandler != null)
            {
                SnapshotRequestedHandler.Invoke(null, null);
            }
        }

        public static EventHandler QuickLoadRequestedHandler;

        private static void QuickLoadRequested(object value, EventArgs empty)
        {
            if (QuickLoadRequestedHandler != null)
            {
                QuickLoadRequestedHandler.Invoke(null, null);
            }
        }

        public static bool UseL3R3InsteadOfX1X2 = true;
        public static EventHandler UseL3R3InsteadOfX1X2Updater;

        public static Dictionary<string, VirtualKey> GamePadStringToVirtual = new Dictionary<string, VirtualKey>()
        {
            {"View",VirtualKey.GamepadView },
            {"Menu",VirtualKey.GamepadMenu },
            {"Left Thumbstick",VirtualKey.GamepadLeftThumbstickButton },
            {"Right Thumbstick",VirtualKey.GamepadRightThumbstickButton },
            {"A",VirtualKey.GamepadA },
            {"B",VirtualKey.GamepadB },
            {"X",VirtualKey.GamepadX },
            {"Y",VirtualKey.GamepadY },

            {"DPad Down",VirtualKey.GamepadDPadDown },
            {"DPad Up",VirtualKey.GamepadDPadUp },
            {"DPad Left",VirtualKey.GamepadDPadLeft },
            {"DPad Right",VirtualKey.GamepadDPadRight },

            {"Left Trigger",VirtualKey.GamepadLeftTrigger },
            {"Right Trigger",VirtualKey.GamepadRightTrigger },
            {"LeftShoulder",VirtualKey.GamepadLeftShoulder },
            {"RightShoulder",VirtualKey.GamepadRightShoulder },

            {"Left Thumbstick (Down)",VirtualKey.GamepadLeftThumbstickDown },
            {"Left Thumbstick (Left)",VirtualKey.GamepadLeftThumbstickLeft },
            {"Left Thumbstick (Up)",VirtualKey.GamepadLeftThumbstickUp },
            {"Left Thumbstick (Right)",VirtualKey.GamepadLeftThumbstickRight },
            {"Right Thumbstick (Down)",VirtualKey.GamepadRightThumbstickDown },
            {"Right Thumbstick (Left)",VirtualKey.GamepadRightThumbstickLeft },
            {"Right Thumbstick (Up)",VirtualKey.GamepadRightThumbstickUp },
            {"Right Thumbstick (Right)",VirtualKey.GamepadRightThumbstickRight },
        };
        public static EventHandler GamePlayPageUpdateBindings;
        public static EventHandler GameOverlaysUpdateBindings;
        public static EventHandler FastForwardRequested;
        static bool CommandInProgress = false;
        public static List<ShortcutEntry> shortcutEntries = new List<ShortcutEntry>()
        {
            new ShortcutEntry(new List<string>(){"Right Thumbstick"}, "In-Game Menu", "In-Game Menu", "xboxmenu"),
            new ShortcutEntry(new List<string>(), "Quick Save", "Quick Save", "quicksave"),
            new ShortcutEntry(new List<string>(), "Quick Load", "Quick Load", "quickload"),
            new ShortcutEntry(new List<string>(), "Saves List", "Saves List", "saveslist"),
            new ShortcutEntry(new List<string>(), "Keyboard", "Keyboard", "keyboard"),
            new ShortcutEntry(new List<string>(), "Snapshot", "Snapshot", "snapshot"),
            new ShortcutEntry(new List<string>(), "Stop", "Stop Game", "stopg"),
            new ShortcutEntry(new List<string>(), "Pause", "Pause Game", "pauseg"),
            new ShortcutEntry(new List<string>(), "Resume", "Resume Game", "resumeg"),
            new ShortcutEntry(new List<string>(), "Fast Forward", "Fast Forward", "fforward"),
            new ShortcutEntry(new List<string>(), "Actions 1", "Actions 1", "actions1"),
            new ShortcutEntry(new List<string>(), "Actions 2", "Actions 2", "actions2"),
            new ShortcutEntry(new List<string>(), "Actions 3", "Actions 3", "actions3"),
        };
        public static bool ShortcutsVisible = false;
        private static string KeyNameResolve(InjectedInputTypes TargetInput)
        {
            string TargetInputName = TargetInput.ToString();
            TargetInputName = TargetInputName.Replace(InjectedInputTypes.DeviceIdJoypadL.ToString(), InputTypes.DeviceIdJoypadL2.ToString());
            TargetInputName = TargetInputName.Replace(InjectedInputTypes.DeviceIdJoypadR.ToString(), InputTypes.DeviceIdJoypadR2.ToString());
            TargetInputName = TargetInputName.Replace(InjectedInputTypes.DeviceIdJoypadC.ToString(), InputTypes.DeviceIdJoypadL.ToString());
            TargetInputName = TargetInputName.Replace(InjectedInputTypes.DeviceIdJoypadZ.ToString(), InputTypes.DeviceIdJoypadR.ToString());
            return TargetInputName;
        }
        private static void OnKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if (PressedKeys.Contains(key))
            {
                PressedKeys.Remove(key);
            }
            GamePlayerView.CleanUpVKeys();
            if (KeyboardVisibleState && DPadActive)
            {

            }
            else
            if (!XBoxMenuActive && !KeyboardMapVisible && !ScaleMenuActive && !SavesListActive && !ShortcutsVisible && !CoreOptionsActive && !ControlsActive && !EffectsActive)
            {
                InvokeEkeyboardEvent(false, key);
            }

            CommandInProgress = false;
            if (SavesListActive && (args.VirtualKey == VirtualKey.GamepadX || args.VirtualKey == VirtualKey.Delete))
            {

            }
            else
            {
                args.Handled = true;
            }
        }

        private static void HandleSaveSlotKeyPress(uint slotID)
        {
            var modifierKeyPressed = PressedKeys.Contains(VirtualKey.Shift);
            var eventArgs = new GameStateOperationEventArgs(modifierKeyPressed ? GameStateOperationEventArgs.GameStateOperationType.Load : GameStateOperationEventArgs.GameStateOperationType.Save, slotID);
            GameStateOperationRequested(null, eventArgs);
        }

        private static void HandleLoadSlotKeyPress(uint slotID)
        {
            var modifierKeyPressed = PressedKeys.Contains(VirtualKey.Shift);
            var eventArgs = new GameStateOperationEventArgs(GameStateOperationEventArgs.GameStateOperationType.Load, slotID);
            GameStateOperationRequested(null, eventArgs);
        }
        private static void HandleActionSlotKeyPress(uint slotID)
        {
            var modifierKeyPressed = PressedKeys.Contains(VirtualKey.Shift);
            var eventArgs = new GameStateOperationEventArgs(GameStateOperationEventArgs.GameStateOperationType.Action, slotID);
            GameStateOperationRequested(null, eventArgs);
        }

        static Dictionary<string, MediaElement> EffectsTemp = new Dictionary<string, MediaElement>();
        public static void StopNotificationSound(string TargetSound)
        {
            try
            {
                MediaElement TempEffect;
                if (EffectsTemp.TryGetValue(TargetSound, out TempEffect))
                {
                    TempEffect.Stop();
                }
            }
            catch (Exception e)
            {

            }
        }
        public static void StopNotificationSoundDirect(string TargetSound)
        {
            try
            {
                MediaElement TempEffect;
                if (EffectsTemp.TryGetValue(TargetSound, out TempEffect))
                {
                    TempEffect.Stop();
                }
            }
            catch (Exception e)
            {

            }

        }
        public static async void PlayNotificationSound(string TargetSound)
        {
            if (MuteSFX || (SFXInProgress && tempCurrentEffect.Equals(TargetSound)))
            {
                return;
            }
            SFXInProgress = true;
            tempCurrentEffect = TargetSound;
            double MediaVolume = GetVolumeValue(TargetSound);
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        await PlayMediaDirect(TargetSound, MediaVolume);
                    }
                    catch (Exception e)
                    {
                        //ShowErrorMessage(e);
                    }
                });
            }
            catch (Exception e)
            {
                //ShowErrorMessage(e);
            }
            SFXInProgress = false;
            tempCurrentEffect = "";
        }

        static string tempCurrentEffect = "";
        static bool SFXInProgress = false;
        public static async void PlayNotificationSoundDirect(string TargetSound)
        {
            if (MuteSFX || (SFXInProgress && tempCurrentEffect.Equals(TargetSound)))
            {
                return;
            }
            SFXInProgress = true;
            tempCurrentEffect = TargetSound;
            double MediaVolume = GetVolumeValue(TargetSound);
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        await PlayMediaDirect(TargetSound, MediaVolume);
                    }
                    catch (Exception e)
                    {
                        ShowErrorMessageDirect(e);
                    }
                });
            }
            catch (Exception e)
            {
                ShowErrorMessageDirect(e);
            }
            SFXInProgress = false;
            tempCurrentEffect = "";
        }
        public static async Task PlayMediaDirect(string TargetSound, double MediaVolume)
        {
            try
            {
                MediaElement TempEffect;
                if (EffectsTemp.TryGetValue(TargetSound, out TempEffect))
                {
                    TempEffect.Volume = MediaVolume;
                    TempEffect.Play();
                }
                else
                {
                    if (!TargetSound.EndsWith(".mp3") && !TargetSound.EndsWith(".wav"))
                    {
                        TargetSound = $"{TargetSound}.mp3";
                    }
                    MediaElement NotificationSound = new MediaElement();
                    StorageFolder folder = (StorageFolder)await Windows.ApplicationModel.Package.Current.InstalledLocation.TryGetItemAsync(@"Assets\SFX");
                    StorageFile file = (StorageFile)await folder.TryGetItemAsync(TargetSound);
                    if (file != null)
                    {
                        var stream = await file.OpenAsync(FileAccessMode.Read);
                        NotificationSound.Volume = MediaVolume;
                        if (stream != null)
                        {
                            NotificationSound.SetSource(stream, file.ContentType);
                            if (!EffectsTemp.TryGetValue(TargetSound, out TempEffect))
                            {
                                EffectsTemp.Add(TargetSound, NotificationSound);
                            }
                            NotificationSound.Play();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void PlayWaitMusic()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        public static bool MouseSate = false;
        public static bool ThumbstickSate = false;
        public static bool TapEvent = false;
        public static bool DynamicPosition = false;
        public static EventHandler UpdateMouseButtonsState;
        public static EventHandler RightClick;
        public static EventHandler LeftClick;
        public static EventHandler PrepareLeftControls;
        public static double GetVolumeValue(string TargetSound)
        {
            double MediaVolume = 0.5;
            switch (TargetSound.Replace(".wav", "").Replace(".mp3", ""))
            {
                case "button-01":
                case "donate":
                case "sucess":
                    MediaVolume = 0.4;
                    break;
                case "button-04":
                case "analog":
                    MediaVolume = 0.3;
                    break;
                case "alert":
                    MediaVolume = 0.3;
                    break;
            }
            return MediaVolume;
        }


        //dialogInProgress made only Windows Phone, as the app will crash if two dialogs appears at once
        public static bool DialogInProgress
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
        public static async void ShowErrorMessage(Exception e,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (DialogInProgress)
            {
                return;
            }
            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    if (e != null)
                    {
                        string ExtraData = "";
                        if (memberName.Length > 0 && sourceLineNumber > 0)
                        {
                            sourceFilePath = Path.GetFileName(sourceFilePath);
                            ExtraData = $"\nName: {memberName}\nLine: {sourceLineNumber}";
                        }
                        try
                        {
                            string DialogTitle = "Error Catched!";
                            string extraMessage = "";

                            try
                            {
                                PlayNotificationSoundDirect("error");
                            }
                            catch (Exception ee)
                            {

                            }

                            var errorMessage = e.Message.ToString();
                            try
                            {
                                errorMessage = String.Join("\n", errorMessage.Split('\n').Distinct(StringComparer.CurrentCultureIgnoreCase));
                                errorMessage = errorMessage.Replace("\n\r\n\r", "\n\r");
                            }
                            catch (Exception ex)
                            {

                            }
                            string DialogMessage = errorMessage + extraMessage + ExtraData;
                            string[] DialogButtons = new string[] { "Close" };
                            var QuestionDialog = Helpers.CreateDialog(DialogTitle, DialogMessage, DialogButtons);
                            DialogInProgress = true;
                            var QuestionResult = await QuestionDialog.ShowAsync2();
                            DialogInProgress = false;
                        }
                        catch (Exception ex)
                        {
                            DialogInProgress = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DialogInProgress = false;
                }
            });
        }
        public static async void ShowErrorMessageDirect(Exception e,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            ShowErrorMessage(e, memberName, sourceFilePath, sourceLineNumber);
        }

        public static async Task ShowMessageDirect(string message)
        {
            if (DialogInProgress)
            {
                return;
            }
            try
            {
                PlayNotificationSoundDirect("alert");
                await Helpers.ShowMessage(message);
            }
            catch (Exception ex)
            {

            }
        }
        public static async Task ShowMessageWithTitleDirect(string message, string title)
        {
            if (DialogInProgress)
            {
                return;
            }
            if (title == null)
            {
                title = "RetriXGold";
            }
            try
            {
                PlayNotificationSoundDirect("alert");
                await Helpers.ShowMessage(title, message);
            }
            catch (Exception ex)
            {

            }
        }
        public static async Task ShowMessageWithTitleDirect(string message, string title, string okButton)
        {
            if (DialogInProgress)
            {
                return;
            }
            if (okButton == null)
            {
                okButton = "Close";
            }
            if (title == null)
            {
                title = "RetriXGold";
            }
            try
            {
                PlayNotificationSoundDirect("alert");
                await Helpers.ShowMessage(title, message, okButton);
            }
            catch (Exception ex)
            {

            }
        }


        public static bool gameNoticeShowed = false;
        public static bool GameNoticeShowed
        {
            get => gameNoticeShowed;
            set
            {
                gameNoticeShowed = value;
            }
        }

        public static async Task<string> GetFileToken(string FileLocation)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(FileLocation);
            string FileToken = StorageApplicationPermissions.FutureAccessList.Add(file);
            return FileToken;
        }

        public static async Task<StorageFolder> GetRecentsLocationAsync()
        {
            var localFolder = await GetRecentsLocationAsyncDirect();
            return localFolder;
        }
        public static async Task<StorageFolder> GetRecentsLocationAsyncDirect()
        {
            var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(RecentsLocation);
            if (localFolder == null)
            {
                localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(RecentsLocation);
            }
            return localFolder;
        }
        public static string RecentsLocation = "SaveRecents";
        //Game Recent Data, Key: System Name - [Game Name, Game Location, Token, Root Needed, Play Count, GameID, Total Time, Core Name]
        public static Dictionary<string, List<string[]>> GamesRecents = new Dictionary<string, List<string[]>>();
        public static Dictionary<string, List<string[]>> GetGamesRecents()
        {
            return GamesRecents;
        }

        //This will return new generated ID if there is no old entry
        //'core name' added in v3.0 to avoid conflicts between multiple cores for the same system
        //(for old entries) it will be ignored as second chance
        //Note: if allowFallbackByName expected rare issues when there is other game with the same name but in sub folder..
        //but allowFallbackByName is important for games list I cannot detect the game in other way, location is not prefered when the backup can be restored in other devices
        public static string GetGameIDByLocation(string core, string system, string GameLocation, bool IsNewCore, bool allowFallbackByName = false)
        {
            core = GetCleanCoreName(core);
            //Get real MD5 before search in old entries (in case failed or not found)
            string gameID = GameLocation.MD5();
            try
            {
                //I have to look in the old entries first to get the first generated ID
                //for some reason I had wrong md5 results before, and this caused an issue after restoring backup on another device
                //I should always use the first generated ID to avoid any wrong/duplicated results
                //In general as game location is matched there is no risk
                if (GamesRecents != null && GameLocation != null)
                {
                    string SystemName = system;
                    List<string[]> TempContent = new List<string[]>();
                    string[] tempGameRecord = new string[] { "" };
                    if (GamesRecents.TryGetValue(SystemName, out TempContent))
                    {
                        bool GameFound = false;

                        //Check first with core name
                        foreach (string[] GameRow in TempContent)
                        {
                            //Row with 8 values is new entry
                            if (GameRow.Length > 7 && (GameRow[1].Contains(GameLocation) || (allowFallbackByName && GameRow[0].Equals(Path.GetFileName(GameLocation)))) && GameRow[7].ToLower().Equals(core.ToLower()))
                            {
                                tempGameRecord = GameRow;
                                GameFound = true;
                                break;
                            }
                        }

                        if (!GameFound && !IsNewCore)
                        {
                            //Regarding to old systems, I wasn't having multiple cores for the same system
                            //If game not found, I have to check if the request coming from old recent entry
                            //there could be a match
                            //After the requested entry will be updated by AddGameToRecents it will have core name
                            //So this step can be dropped in the future?
                            foreach (string[] GameRow in TempContent)
                            {
                                //Row with 7 values is old entry
                                if ((GameRow[1].Contains(GameLocation) || (allowFallbackByName && GameRow[0].Equals(Path.GetFileName(GameLocation)))) && GameRow.Length == 7)
                                {
                                    tempGameRecord = GameRow;
                                    GameFound = true;
                                    break;
                                }
                            }
                        }
                        if (GameFound)
                        {
                            gameID = tempGameRecord[5];
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return gameID.ToLower();
        }

        public static List<InsightItem> insightItems = new List<InsightItem>();
        public static async Task UpdatePlayInsights(string core, string system, string name, long total)
        {
            try
            {
                if (total <= 0)
                {
                    return;
                }
                var currentDate = DateTime.Today.Ticks;
                var currentTime = DateTime.Now.Ticks;
                insightItems.Add(new InsightItem() { CoreName = core, SystemName = system, GameName = name, Time = total, Date = currentDate, CTime = currentTime });
                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(insightItems));
                if (dictionaryListBytes != null)
                {
                    var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(RecentsLocation, CreationCollisionOption.OpenIfExists);
                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync("insights.rtx");
                    if (targetFileTest != null)
                    {
                        //keep backup (temp file) in-case the app terminated or crashed
                        await targetFileTest.CopyAsync(localFolder, "insights_temp.rtx", NameCollisionOption.ReplaceExisting);
                    }

                    var targetFile = await localFolder.CreateFileAsync("insights.rtx", CreationCollisionOption.ReplaceExisting);
                    using (var outStream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var stream = outStream.AsStreamForWrite();
                        await stream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                        await stream.FlushAsync();
                    }
                    //when everything is fine delete the temp file
                    var targetFileTest2 = (StorageFile)await localFolder.TryGetItemAsync("insights_temp.rtx");
                    if (targetFileTest2 != null)
                    {
                        //before delete check again if main file exists
                        var targetTest = (StorageFile)await localFolder.TryGetItemAsync("insights.rtx");
                        if (targetTest != null)
                        {
                            await targetFileTest2.DeleteAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static async Task AddGameToRecents(string core, string system, string FilePath, bool RootNeeded, string gameID, long totalTime, bool IsNewCore, bool delete = false)
        {
            try
            {
                core = GetCleanCoreName(core);
                //Important: (core name added in 3.0 to avoid conflict between multiple cores for the same system)

                //when FilePath/GameLocation is null, then the core started without content
                //If I ignored null entry each time, the statisitics will not be accurate
                //I have to add dummy content for null case

                string SystemName = system;
                string GameLocation = FilePath != null && FilePath.Length > 0 ? FilePath : "dummy";
                string GameName = FilePath != null ? Path.GetFileName(FilePath) : "dummy";
                if (GameSystemSelectionView.TestContentBySystem.Values.Contains(GameName))
                {
                    GameLocation = "dummy";
                    GameName = "dummy";
                    gameID = "dummy";
                }
                if (FilePath == null || FilePath.Length == 0)
                {
                    gameID = "dummy";
                }
                //string GameToken = await PlatformService.GetFileToken(GameLocation);
                string GameToken = "";
                string RootState = RootNeeded ? "1" : "0";
                string OpenCount = (GetGameOpenCount(core, SystemName, GameLocation, IsNewCore) + (totalTime > 0 ? 0 : 1)).ToString();
                string TotalTimePlayed = (GetGamePlayedTime(core, SystemName, GameLocation, IsNewCore) + totalTime).ToString();

                await UpdatePlayInsights(core, SystemName, GameName, totalTime);

                //I started these without good plan for future usage
                //they should be with keys and values instead of indexes
                //but anyway cannot be changed before find solution for old backups
                //will keep using the indexes
                string[] NewGame = new string[] { GameName, GameLocation, GameToken, RootState, OpenCount, gameID, TotalTimePlayed, core };
                //                               >>>>0<<<<   >>>>1<<<<    >>>>2<<<<  >>>>3<<<<  >>>>4<<<<  >>5<<      >>>>6<<<<     >>7<<
                var SnapshotNameDelete = "";
                List<string[]> TempContent = new List<string[]>();

                if (GamesRecents != null && GameLocation != null)
                {
                    if (GamesRecents.TryGetValue(SystemName, out TempContent))
                    {
                        int GameRowIndex = 0;
                        bool GameFound = false;

                        //Check first with core name
                        foreach (string[] GameRow in TempContent)
                        {
                            //Row with 8 values is new entry
                            if (GameRow.Length > 7 && GameRow[1].Contains(GameLocation) && GameRow[7].ToLower().Equals(core.ToLower()))
                            {
                                TempContent[GameRowIndex] = NewGame;
                                GameFound = true;
                                break;
                            }
                            GameRowIndex++;
                        }

                        if (!GameFound && !IsNewCore)
                        {
                            GameRowIndex = 0;
                            //Read about this extra check in GetGameIDByLocation(..)
                            foreach (string[] GameRow in TempContent)
                            {
                                //Row with 7 values is old entry
                                if (GameRow[1].Contains(GameLocation) && GameRow.Length == 7)
                                {
                                    TempContent[GameRowIndex] = NewGame;
                                    GameFound = true;
                                    break;
                                }
                                GameRowIndex++;
                            }
                        }

                        if (!GameFound && !delete)
                        {
                            TempContent.Add(NewGame);
                        }
                        else if (delete)
                        {
                            TempContent.RemoveAt(GameRowIndex);
                            if (gameID.Length > 0)
                            {
                                SnapshotNameDelete = $"{core}_{gameID}.png";
                            }
                        }
                        GamesRecents[SystemName] = TempContent;

                    }
                    else
                    {
                        TempContent = new List<string[]>();
                        TempContent.Add(NewGame);
                        GamesRecents.Add(SystemName, TempContent);
                    }
                }

                var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(RecentsLocation, CreationCollisionOption.OpenIfExists);

                if (SnapshotNameDelete.Length > 0)
                {
                    var SnapshotFile = (StorageFile)await localFolder.TryGetItemAsync(SnapshotNameDelete);
                    if (SnapshotFile == null)
                    {
                        //Fallback to the old way
                        SnapshotNameDelete = $"{gameID}.png";
                        SnapshotFile = (StorageFile)await localFolder.TryGetItemAsync(SnapshotNameDelete);
                    }
                    if (SnapshotFile != null)
                    {
                        await SnapshotFile.DeleteAsync();
                    }
                }

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(GamesRecents));
                if (dictionaryListBytes != null)
                {
                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync("recents.rtx");
                    if (targetFileTest != null)
                    {
                        //keep backup (temp file) in-case the app terminated or crashed
                        await targetFileTest.CopyAsync(localFolder, "recents_temp.rtx", NameCollisionOption.ReplaceExisting);
                    }

                    var targetFile = await localFolder.CreateFileAsync("recents.rtx", CreationCollisionOption.ReplaceExisting);
                    using (var outStream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var stream = outStream.AsStreamForWrite();
                        await stream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                        await stream.FlushAsync();
                    }
                    //when everything is fine delete the temp file
                    var targetFileTest2 = (StorageFile)await localFolder.TryGetItemAsync("recents_temp.rtx");
                    if (targetFileTest2 != null)
                    {
                        //before delete check again if main file exists
                        var targetTest = (StorageFile)await localFolder.TryGetItemAsync("recents.rtx");
                        if (targetTest != null)
                        {
                            await targetFileTest2.DeleteAsync();
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
        }
        public static int GetGameOpenCount(string core, string SystemName, string GameLocation, bool IsNewCore)
        {
            try
            {
                core = GetCleanCoreName(core);
                if (GamesRecents != null && GameLocation != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        //Check first with core name
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            //Row with 8 values is new entry
                            if (GamesList.Length > 7 && GamesList[1].Contains(GameLocation) && GamesList[7].ToLower().Equals(core.ToLower()))
                            {
                                return Int32.Parse(GamesList[4]);
                            }
                        }
                        if (!IsNewCore)
                        {
                            //Read about null extra check in GetGameIDByLocation(..)
                            foreach (string[] GamesList in GamesListTemp)
                            {
                                //Row with 7 values is old entry
                                if (GamesList[1].Contains(GameLocation) && GamesList.Length == 7)
                                {
                                    return Int32.Parse(GamesList[4]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
            return 0;
        }
        public static long GetGamePlayedTime(string core, string SystemName, string GameLocation, bool IsNewCore, bool allowFallbackByName = false)
        {
            try
            {
                core = GetCleanCoreName(core);
                if (GamesRecents != null && GameLocation != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        //Check first with core name
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            //Row with 8 values is new entry
                            if (GamesList.Length > 7 && (GamesList[1].Contains(GameLocation) || (allowFallbackByName && GamesList[0].Contains(Path.GetFileName(GameLocation)))) && GamesList[7].ToLower().Equals(core.ToLower()))
                            {
                                return Int32.Parse(GamesList[6]);
                            }
                        }

                        if (!IsNewCore)
                        {
                            //Read about null extra check in GetGameIDByLocation(..)
                            foreach (string[] GamesList in GamesListTemp)
                            {
                                //Row with 7 values is old entry
                                if (GamesList[1] != null && (GamesList[1].Contains(GameLocation) || (allowFallbackByName && GamesList[0].Contains(Path.GetFileName(GameLocation)))) && GamesList.Length == 7)
                                {
                                    return Int32.Parse(GamesList[6]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //ShowErrorMessage(e);
            }
            return 0;
        }

        public static int GetPlaysCount(string core, string SystemName, bool IsNewCore)
        {
            int totalCount = 0;
            try
            {
                core = GetCleanCoreName(core);
                if (GamesRecents != null && SystemName != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        //Check first with core name
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            //Row with 8 values is new entry
                            if (GamesList.Length > 7 && GamesList[7].ToLower().Equals(core.ToLower()))
                            {
                                totalCount += Int32.Parse(GamesList[4]);
                            }
                        }
                        if (!IsNewCore)
                        {
                            //Read about null extra check in GetGameIDByLocation(..)
                            foreach (string[] GamesList in GamesListTemp)
                            {
                                //Row with 7 values is old entry
                                if (GamesList.Length == 7)
                                {
                                    totalCount += Int32.Parse(GamesList[4]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
            return totalCount;
        }
        public static int GetGamePlaysCount(string core, string SystemName, string GameLocation, bool IsNewCore, bool allowFallbackByName = false)
        {
            int totalCount = 0;
            try
            {
                core = GetCleanCoreName(core);
                if (GamesRecents != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        //Check first with core name
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            //Row with 8 values is new entry
                            if (GamesList.Length > 7 && (GamesList[1].Contains(GameLocation) || (allowFallbackByName && GamesList[0].Contains(Path.GetFileName(GameLocation)))) && GamesList[7].ToLower().Equals(core.ToLower()))
                            {
                                return Int32.Parse(GamesList[4]);
                            }
                        }

                        if (!IsNewCore)
                        {
                            //Read about null extra check in GetGameIDByLocation(..)
                            foreach (string[] GamesList in GamesListTemp)
                            {
                                if (GamesList.Length == 7 && GamesList[1] != null && (GamesList[1].Equals(GameLocation) || (allowFallbackByName && GamesList[0].Contains(Path.GetFileName(GameLocation)))))
                                {
                                    return Int32.Parse(GamesList[4]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
            return totalCount;
        }
        public static string GetGameLocation(string core, string SystemName, string GameName, bool IsNewCore)
        {
            try
            {
                core = GetCleanCoreName(core);
                //This function could return wrong results in case of two games with the same name from multiple locations
                if (GamesRecents != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        //Check first with core name
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            //Row with 8 values is new entry
                            if (GamesList.Length > 7 && GamesList[0].Contains(GameName) && GamesList[7].ToLower().Equals(core.ToLower()))
                            {
                                return GamesList[1];
                            }
                        }

                        if (!IsNewCore)
                        {
                            //Read about null extra check in GetGameIDByLocation(..)
                            foreach (string[] GamesList in GamesListTemp)
                            {
                                if (GamesList.Length == 7 && GamesList[0].Contains(GameName))
                                {
                                    return GamesList[1];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
            return null;
        }
        public static bool GetGameRootNeeded(string core, string SystemName, string GameLocation, bool IsNewCore)
        {
            try
            {
                core = GetCleanCoreName(core);
                List<string[]> GamesListTemp = new List<string[]>();
                if (GamesRecents != null)
                {
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        //Check first with core name
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            //Row with 8 values is new entry
                            if (GamesList.Length > 7 && GamesList[7].ToLower().Equals(core.ToLower()))
                            {
                                return GamesList[3] == "1";
                            }
                        }

                        if (!IsNewCore)
                        {
                            //Read about null extra check in GetGameIDByLocation(..)
                            foreach (string[] GamesList in GamesListTemp)
                            {
                                //Row with 7 values is old entry
                                if (GamesList.Length == 7 && GamesList[1].Contains(GameLocation))
                                {
                                    return GamesList[3] == "1";
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
                return false;
            }
        }
        public static string GetCleanCoreName(string core)
        {
            if (core != null)
            {
                return core.Replace("-", "_").Replace("&", "").Replace("'", "").Replace("  ", " ").Replace(" ", "_");
            }
            else
            {
                return "none";
            }
        }
        public static async Task RetriveRecents()
        {
            await RetriveRecentsDirect();
        }
        public static async Task RetriveRecentsDirect()
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(RecentsLocation);

                if (localFolder != null)
                {
                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync("recents.rtx");
                    if (targetFileTest == null)
                    {
                        //Check if there is backup file
                        //if 'true' then the app terminated or crashed while saving the file
                        //so I will get the backup file
                        //then it will be saved again by default
                        targetFileTest = (StorageFile)await localFolder.TryGetItemAsync("recents_temp.rtx");
                    }
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
                        var dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, List<string[]>>>(ActionsFileContent);
                        if (dictionaryList != null)
                        {
                            GamesRecents = dictionaryList;
                        }
                        else
                        {
                            await targetFileTest.DeleteAsync();
                        }
                    }
                }

                await RetriveInsightsDirect();
            }
            catch (Exception e)
            {
                ShowErrorMessageDirect(e);
            }
        }
        public static async Task RetriveInsightsDirect()
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(RecentsLocation);

                if (localFolder != null)
                {
                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync("insights.rtx");
                    if (targetFileTest == null)
                    {
                        //Check if there is backup file
                        //if 'true' then the app terminated or crashed while saving the file
                        //so I will get the backup file
                        //then it will be saved again by default
                        targetFileTest = (StorageFile)await localFolder.TryGetItemAsync("insights_temp.rtx");
                    }
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
                        var dictionaryList = JsonConvert.DeserializeObject<List<InsightItem>>(ActionsFileContent);
                        if (dictionaryList != null)
                        {
                            insightItems = dictionaryList;
                        }
                        else
                        {
                            await targetFileTest.DeleteAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErrorMessageDirect(e);
            }
        }

        public static unsafe byte[] ConvertBytesToBitmap(int height, int width, byte[] bytes)
        {
            unsafe
            {
                var canvasBitmap = CanvasBitmap.CreateFromBytes(null, bytes, width, height, Windows.Graphics.DirectX.DirectXPixelFormat.Unknown);
                return canvasBitmap.GetPixelBytes();
            }
        }

        private static event EventHandler StopHandler;
        private static event EventHandler HideSubHandler;
        private static event EventHandler HideCoreOptionsHandler;
        public static bool SubPageActive = false;
        public static bool CoreOptionsActive = false;
        public static bool ControlsActive = false;
        public static bool EffectsActive = false;
        public static bool StopGameInProgress = false;
        public static void SetGameStopInProgress(bool StopState)
        {
            StopGameInProgress = StopState;
        }
        public static void SetStopHandler(EventHandler eventHandler)
        {
            StopHandler += eventHandler;
        }

        public static void DeSetStopHandler(EventHandler eventHandler)
        {
            StopHandler -= eventHandler;
        }

        public static void InvokeStopHandler(Object rootFrame)
        {
            try
            {
                if (StopHandler != null)
                {
                    StopHandler.Invoke(null, EventArgs.Empty);
                }
                else
                {
                    ((Frame)rootFrame).GoBack();
                }
            }
            catch (Exception ee)
            {

            }
        }
        public static EventHandler ScreenshotHandler;
        public static EventHandler ResolveCanvasSizeHandler;
        public static EventHandler SlidersDialogHandler;
        public static EventHandler HideEffects;
        public static void SetHideSubPageHandler(EventHandler eventHandler)
        {
            HideSubHandler += eventHandler;
        }
        public static void InvokeSubPageHandler(Object rootFrame)
        {
            try
            {
                if (HideSubHandler != null)
                {

                    HideSubHandler.Invoke(null, EventArgs.Empty);
                }
                else
                {
                    SubPageActive = false;
                    ((Frame)rootFrame).GoBack();
                }
            }
            catch (Exception ee)
            {

            }
        }
        public static void SetSubPageState(bool SubPageState)
        {
            SubPageActive = SubPageState;
        }

        public static void SetHideCoreOptionsHandler(EventHandler eventHandler)
        {
            HideCoreOptionsHandler += eventHandler;
        }

        public static void DeSetHideCoreOptionsHandler(EventHandler eventHandler)
        {
            HideCoreOptionsHandler -= eventHandler;
        }

        public static void InvokeHideCoreOptionsHandler(Object rootFrame = null)
        {
            try
            {
                if (HideCoreOptionsHandler != null)
                {
                    HideCoreOptionsHandler.Invoke(null, EventArgs.Empty);
                }
                else if (rootFrame != null)
                {
                    CoreOptionsActive = false;
                    ((Frame)rootFrame).GoBack();
                }
            }
            catch (Exception ee)
            {

            }
        }
        public static void SetCoreOptionsState(bool SubPageState)
        {
            CoreOptionsActive = SubPageState;
        }

        public static bool SavesListActive = false;
        public static event EventHandler HideSavesListHandler;
        public static EventHandler HideControlsListHandler;
        public static void SetHideSavesListHandler(EventHandler eventHandler)
        {
            HideSavesListHandler += eventHandler;
        }
        public static void DeSetHideSavesListHandler(EventHandler eventHandler)
        {
            HideSavesListHandler -= eventHandler;
        }
        public static void InvokeHideSavesListHandler(Object rootFrame = null)
        {
            try
            {
                if (HideSavesListHandler != null)
                {
                    HideSavesListHandler.Invoke(null, EventArgs.Empty);
                }
                else if (rootFrame != null)
                {
                    SavesListActive = false;
                    ((Frame)rootFrame).GoBack();
                }
            }
            catch (Exception ee)
            {

            }
        }
        public static void InvokeHideControlsListHandler(Object rootFrame = null)
        {
            try
            {
                if (HideControlsListHandler != null)
                {
                    HideControlsListHandler.Invoke(null, EventArgs.Empty);
                }
                else if (rootFrame != null)
                {
                    SavesListActive = false;
                    ((Frame)rootFrame).GoBack();
                }
            }
            catch (Exception ee)
            {

            }
        }
        public static void SetSavesListActive(bool SavesListActiveState)
        {
            SavesListActive = SavesListActiveState;
        }

        public static bool FilterModeState = false;
        public static void SetFilterModeState(bool filterModeState)
        {
            FilterModeState = filterModeState;
        }

        private static event EventHandler ResetSelectionHandler;
        public static void SetResetSelectionHandler(EventHandler eventHandler)
        {
            ResetSelectionHandler += eventHandler;
        }
        public static void InvokeResetSelectionHandler(Object rootFrame)
        {
            try
            {
                if (ResetSelectionHandler != null)
                {
                    ResetSelectionHandler.Invoke(null, EventArgs.Empty);
                }
                else
                {
                    FilterModeState = false;
                    ((Frame)rootFrame).GoBack();
                }
            }
            catch (Exception ee)
            {

            }
        }

        public static bool RotatedRight = false;
        public static bool RotatedLeft = false;
        public static void SetRotateDegree(int degree)
        {
            switch (degree)
            {
                case 90:
                    RotatedRight = true;
                    RotatedLeft = false;
                    break;

                case -90:
                    RotatedRight = false;
                    RotatedLeft = true;
                    break;

                default:
                    RotatedRight = false;
                    RotatedLeft = false;
                    break;
            }

        }

        public static async void checkInitWidth(bool delayRequest = true)
        {
            try
            {
                if (delayRequest)
                {
                    await Task.Delay(1500);
                }
                var currentWidth = Window.Current.CoreWindow.Bounds.Width;
                PlatformService.InitWidthSize = (int)currentWidth;
                if (currentWidth > 600)
                {
                    PlatformService.horizontalAlignment = HorizontalAlignment.Center;
                }
                else
                {
                    PlatformService.horizontalAlignment = HorizontalAlignment.Stretch;
                }

            }
            catch (Exception e)
            {

            }

        }

        public static List<StorageFile> AnyCores = null;
        public static async Task GetAnyCores()
        {
            try
            {
                if (AnyCores != null)
                {
                    AnyCores.Clear();
                }
                var AnyCoreFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("AnyCore");
                if (AnyCoreFolder != null)
                {
                    var files = await AnyCoreFolder.GetFilesAsync();
                    if (files != null && files.Count > 0)
                    {
                        AnyCores = files.ToList();
                    }
                }
            }
            catch (Exception ee)
            {

            }
        }

        internal static void FindChildren<T>(List<T> results, DependencyObject startNode) where T : DependencyObject
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

        public static string ExtensionInfo(string ext)
        {
            string info = "";
            try
            {
                Dictionary<string, string> exts = new Dictionary<string, string>()
                {
                  { ".chd", "CHD files are arcade game disk images used by MAME. This explains why they are so big in file size. Nowadays, they have become quite popular among many emulators that use relatively large ROMs. This includes emulators like some of the PlayStation Libretro cores in Retroarch and, by extension, all the popular emulation-oriented distributions for the Raspberry Pi series of microcomputers."},
                };
                var infoTest = "";
                if (exts.TryGetValue(ext, out infoTest))
                {
                    info = infoTest;
                }
            }
            catch (Exception e)
            {

            }
            return info;
        }
        public static bool IsCoreRequiredGamesFolderDirect(string coreName)
        {
            string[] contentLessCores = new string[]
            {
                "2048",
                "DOSBox-pure",
                "cap32",
                "fMSX",
                "Mini vMac",
                "NXEngine",
                "PrBoom",
                "QUASI88",
                "TyrQuake",
                "xrick",
            };
            try
            {
                if (contentLessCores.Contains(coreName))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

            }
            return true;
        }
        public static bool IsCoreRequiredGamesFolder(string coreName)
        {
            return IsCoreRequiredGamesFolderDirect(coreName);
        }

        public static async Task<bool> IsCoreGamesFolderAlreadySelected(string systemName)
        {
            bool folderState = false;
            TaskCompletionSource<bool> pickerTaskWaiter = new TaskCompletionSource<bool>();
            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    var tryGetDirectory = await GetFileForToken(systemName);
                    if (tryGetDirectory != null)
                    {
                        folderState = true;
                    }
                }
                catch (Exception e)
                {
                    ShowErrorMessage(e);

                }
                try
                {
                    pickerTaskWaiter.SetResult(true);
                }
                catch (Exception e)
                {

                }
            });
            await pickerTaskWaiter.Task;
            return folderState;
        }
        public static async Task<StorageFolder> GetCustomFolder(string coreName, string tag)
        {
            StorageFolder folder = null;
            try
            {
                var tryGetDirectory = await GetCustomFolderForToken(coreName, tag);
                if (tryGetDirectory != null)
                {
                    return tryGetDirectory;
                }
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);

            }

            return folder;
        }
        public static async Task ResetCustomFolder(string coreName, string tag)
        {
            try
            {
                await GetCustomFolderForToken(coreName, tag, true);
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
        }
        public static async Task<StorageFolder> SetCustomSavesFolder(string coreName, string tag)
        {
            StorageFolder folder = null;
            try
            {
                try
                {
                    var folderPicker = new FolderPicker();
                    folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                    folderPicker.FileTypeFilter.Add("*");
                    StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
                    if (DownloadsFolderTest != null)
                    {
                        RememberCustomFolder(DownloadsFolderTest, coreName, tag);
                        return DownloadsFolderTest;
                    }
                }
                catch (Exception ex)
                {
                    var folderPicker = new FolderPicker();
                    folderPicker.FileTypeFilter.Add("*");
                    StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
                    if (DownloadsFolderTest != null)
                    {
                        RememberCustomFolder(DownloadsFolderTest, coreName, tag);
                        return DownloadsFolderTest;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return folder;
        }
        public static void RemoveGlobalFolder()
        {
            try
            {
                RemoveGamesFolderFromAccessList("GamesGlobalFolder");
            }
            catch (Exception ex)
            {

            }
        }
        public static async Task<StorageFolder> GetGlobalFolder()
        {
            StorageFolder globalFolder = null;
            try
            {
                globalFolder = await GetFileForToken("GamesGlobalFolder");
            }
            catch (Exception ex)
            {

            }
            return globalFolder;
        }
        public static async Task<StorageFolder> SetGlobalFolder(bool reSelect = false, bool ignorePicker = false)
        {
            StorageFolder globalFolder = null;
            try
            {
                globalFolder = await PickDirectory("GamesGlobalFolder", reSelect, ignorePicker);
            }
            catch (Exception ex)
            {

            }
            return globalFolder;
        }
        public static async Task<StorageFolder> PickDirectory(string systemName, bool reSelect = false, bool ignorePicker = false, bool globalRootCheck = false)
        {
            StorageFolder returnFolder = null;
            TaskCompletionSource<bool> pickerTaskWaiter = new TaskCompletionSource<bool>();
            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    if (reSelect && !globalRootCheck)
                    {
                        try
                        {
                            var folderPicker = new FolderPicker();
                            folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                            folderPicker.FileTypeFilter.Add("*");

                            StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
                            if (DownloadsFolderTest != null)
                            {
                                RememberFile(DownloadsFolderTest, systemName);
                                returnFolder = DownloadsFolderTest;
                            }
                        }
                        catch (Exception ex)
                        {
                            var folderPicker = new FolderPicker();
                            folderPicker.FileTypeFilter.Add("*");

                            StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
                            if (DownloadsFolderTest != null)
                            {
                                RememberFile(DownloadsFolderTest, systemName);
                                returnFolder = DownloadsFolderTest;
                            }
                        }
                    }
                    else
                    {
                        StorageFolder tryGetDirectory = null;
                        if (!globalRootCheck)
                        {
                            try
                            {
                                tryGetDirectory = await GetFileForToken(systemName);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        try
                        {
                            if (tryGetDirectory == null)
                            {
                                //Check if global root is set
                                var globalRoot = await GetGlobalFolder();
                                if (globalRoot != null)
                                {
                                    var folders = await globalRoot.GetFoldersAsync();
                                    if (folders != null && folders.Count > 0)
                                    {
                                        foreach (var fItem in folders)
                                        {
                                            bool folderFound = false;
                                            var folderName = fItem.Name.Replace(" ", "").Replace("-", "").Replace("_", "").Replace("&", "").Replace(".", "").Replace("+", "").Replace("'", "").Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").ToLower();
                                            var checkName = systemName.Replace(" ", "").Replace("-", "").Replace("_", "").Replace("&", "").Replace(".", "").Replace("+", "").Replace("'", "").Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").ToLower();
                                            if (folderName.Equals(checkName))
                                            {
                                                tryGetDirectory = fItem;
                                                break;
                                            }
                                            else
                                            {
                                                //Do some resolves for specific systems
                                                switch (systemName)
                                                {
                                                    case "Super Nintendo":
                                                        if (folderName.EndsWith("snes") || folderName.StartsWith("snes"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.StartsWith("super") && folderName.Contains("nes"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "Jump n' Bump":
                                                        if (folderName.Equals("jumpandbump"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Equals("jumpbump"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("jnb"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "PC Engine CD":
                                                        if (folderName.StartsWith("pcengine") && folderName.Contains("cd"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "NES":
                                                        if (folderName.Contains("nintendo") && folderName.Contains("entertainment"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Contains("nintendo") && folderName.Contains("nes"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Equals("nintendoes") || folderName.StartsWith("nintendoes") || folderName.EndsWith("nintendoes"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Contains("system") && folderName.Contains("entertainment"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "GameBoy":
                                                        if (folderName.Contains("nintendo") && folderName.EndsWith("gb"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("gb"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "GameBoy Color":
                                                        if (folderName.Contains("nintendo") && folderName.EndsWith("gbc"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("gbc"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "GameBoy+":
                                                        if (folderName.Contains("nintendo") && folderName.EndsWith("gbc"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("gameboyplus"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("gbplus"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("gameboycolor"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "GameBoy Advance":
                                                        if (folderName.Contains("nintendo") && folderName.EndsWith("advance"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Contains("nintendo") && folderName.EndsWith("adv"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Contains("nintendo") && folderName.Contains("gba"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Contains("gameboy") && folderName.EndsWith("adv"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Contains("gameboy") && folderName.EndsWith("advance"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("advance"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("adv"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("gba"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "Nintendo DS":
                                                        if (folderName.Equals("nds"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "Virtual Boy":
                                                        if (folderName.Equals("vb"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Contains("nintendo") && folderName.EndsWith("vb"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "MSX Computer":
                                                        if (folderName.Equals("msx"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("msx1"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("msx2"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("msx12"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Contains("microsoft") && folderName.Contains("msx"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "Pokémon Mini":
                                                        if (folderName.Equals("pkm"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Contains("pokemon") && folderName.EndsWith("mini"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Contains("nintendo") && folderName.EndsWith("mini"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "Game & Watch":
                                                        if (folderName.Equals("gw"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("gameandwatch"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Contains("game") && folderName.Contains("watch"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Contains("nintendo") && folderName.EndsWith("watch"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "Nintendo 64":
                                                        if (folderName.Equals("n64"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("64"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.Contains("nintendo") && folderName.Contains("64"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "Mega CD":
                                                        if (folderName.Equals("segacd"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Contains("sega") && folderName.Contains("cd"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "SEGA Wide":
                                                        if (folderName.Equals("segadrive"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("md"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Contains("mega") && folderName.Contains("drive"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "NeoGeo Pocket":
                                                        if (folderName.Equals("ngp"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.StartsWith("ng") && folderName.Contains("pocket"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Contains("ng") && folderName.StartsWith("pocket"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "NeoGeo CD":
                                                        if (folderName.Equals("ngcd"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "DOSBox Pure":
                                                        if (folderName.Equals("dosbox"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    case "PlayStation":
                                                        if (folderName.Equals("sonypsx"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else if (folderName.Equals("psx"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        else
                                                        if (folderName.StartsWith("sony") && folderName.Contains("psx"))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;

                                                    default:
                                                        if (folderName.EndsWith(checkName) || checkName.EndsWith(folderName) || checkName.StartsWith(folderName))
                                                        {
                                                            tryGetDirectory = fItem;
                                                            folderFound = true;
                                                        }
                                                        break;
                                                }
                                            }
                                            if (folderFound)
                                            {
                                                break;
                                            }
                                        }


                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        if (tryGetDirectory == null && !ignorePicker && !globalRootCheck)
                        {
                            try
                            {
                                var folderPicker = new FolderPicker();
                                folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                                folderPicker.FileTypeFilter.Add("*");

                                StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
                                if (DownloadsFolderTest != null)
                                {
                                    RememberFile(DownloadsFolderTest, systemName);
                                    returnFolder = DownloadsFolderTest;
                                }
                            }
                            catch (Exception ex)
                            {
                                var folderPicker = new FolderPicker();
                                folderPicker.FileTypeFilter.Add("*");

                                StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
                                if (DownloadsFolderTest != null)
                                {
                                    RememberFile(DownloadsFolderTest, systemName);
                                    returnFolder = DownloadsFolderTest;
                                }
                            }
                        }
                        else
                        {
                            returnFolder = tryGetDirectory;
                        }
                    }
                }
                catch (Exception e)
                {
                    ShowErrorMessage(e);
                }
                try
                {
                    pickerTaskWaiter.SetResult(true);
                }
                catch (Exception e)
                {

                }
            });
            await pickerTaskWaiter.Task;
            return returnFolder;
        }

        public static string RememberFile(StorageFolder file, string SystemName)
        {
            var fileToken = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{SystemName}_GamesFolder", "");
            if (fileToken.Length > 0)
            {
                StorageApplicationPermissions.FutureAccessList.Remove(fileToken);
            }
            string token = Guid.NewGuid().ToString();
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, file);
            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{SystemName}_GamesFolder", token);
            return token;
        }
        public static string RememberCustomFolder(StorageFolder folder, string coreName, string tag)
        {
            string token = Guid.NewGuid().ToString();
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, folder);
            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{tag}#{coreName}", token);
            return token;
        }
        public static async Task<StorageFolder> GetFileForToken(string systemName)
        {
            try
            {
                var fileToken = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{systemName}_GamesFolder", "");
                if (fileToken.Length > 0)
                {
                    if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(fileToken))
                    {
                        return null;
                    }
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(fileToken);
                }
            }
            catch (Exception e)
            {

            }
            return null;
        }
        public static async Task<StorageFolder> GetCustomFolderForToken(string coreName, string tag, bool reset = false)
        {
            try
            {
                var folderToken = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{tag}#{coreName}", "");
                if (folderToken.Length > 0)
                {
                    if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(folderToken))
                    {
                        return null;
                    }
                    if (reset)
                    {
                        StorageApplicationPermissions.FutureAccessList.Remove(folderToken);
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{tag}#{coreName}", "");
                        return null;
                    }
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                }
            }
            catch (Exception e)
            {

            }
            return null;
        }
        public static void RemoveGamesFolderFromAccessList(string systemName)
        {
            try
            {
                var fileToken = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{systemName}_GamesFolder", "");
                if (fileToken.Length > 0)
                {
                    if (StorageApplicationPermissions.FutureAccessList.ContainsItem(fileToken))
                    {
                        StorageApplicationPermissions.FutureAccessList.Remove(fileToken);
                    }
                    Plugin.Settings.CrossSettings.Current.Remove($"{systemName}_GamesFolder");
                }
            }
            catch (Exception e)
            {

            }
        }
        public static bool CheckDirectoryToken(string systemName)
        {
            try
            {
                var fileToken = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{systemName}_GamesFolder", "");
                if (fileToken.Length > 0)
                {
                    if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(fileToken))
                    {
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception e)
            {

            }
            return false;
        }

        public static async Task<List<byte[]>> getShader()
        {
            try
            {
                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                filePicker.FileTypeFilter.Add(".bin");
                var TargetFiles = await filePicker.PickMultipleFilesAsync();
                if (TargetFiles != null && TargetFiles.Count > 0)
                {
                    List<byte[]> byteArrays = new List<byte[]>();
                    foreach (var fItem in TargetFiles)
                    {
                        byte[] resultInBytes = (await FileIO.ReadBufferAsync(fItem)).ToArray();
                        byteArrays.Add(resultInBytes);
                    }
                    if (byteArrays.Count > 0)
                    {
                        return byteArrays;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
            return null;
        }
        public static async Task<List<StorageFile>> getOverlay()
        {
            try
            {
                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                filePicker.FileTypeFilter.Add(".bmp");
                filePicker.FileTypeFilter.Add(".png");
                filePicker.FileTypeFilter.Add(".jpg");
                filePicker.FileTypeFilter.Add(".jpeg");
                filePicker.FileTypeFilter.Add(".gif");
                var TargetFiles = await filePicker.PickMultipleFilesAsync();
                if (TargetFiles != null && TargetFiles.Count > 0)
                {
                    return TargetFiles.ToList();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
            return null;
        }

        public static async Task<bool> ExtractTemporaryFiles(StorageFile targetFile, string destination, IProgress<Dictionary<string, long[]>> progress)
        {
            bool extractState = false;
            try
            {
                var targetFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(destination);

                if (targetFolder != null)
                {
                    var zipStream = await targetFile.OpenAsync(FileAccessMode.Read);
                    using (var zipArchive = ArchiveFactory.Open(zipStream.AsStream()))
                    {
                        //It should support 7z, zip, rar, gz, tar
                        var reader = zipArchive.ExtractAllEntries();

                        //Bind progress event
                        reader.EntryExtractionProgress += (sender, e) =>
                        {
                            var entryProgress = e.ReaderProgress.BytesTransferred;
                            var sizeProgress = e.Item.Size;
                            var entryKey = e.Item.Key.Replace('/', Path.DirectorySeparatorChar);
                            if (progress != null)
                            {
                                Dictionary<string, long[]> result = new Dictionary<string, long[]>()
                                {
                                {entryKey, new long[]{ entryProgress, sizeProgress } }
                                };
                                progress.Report(result);
                            }
                        };

                        //Extract files
                        TaskCompletionSource<bool> extractTask = new TaskCompletionSource<bool>();
                        await Task.Run(async () =>
                        {
                            try
                            {
                                while (reader.MoveToNextEntry())
                                {
                                    var testFile = await targetFolder.TryGetItemAsync(reader.Entry.Key.Replace('/', Path.DirectorySeparatorChar));
                                    if (testFile == null)
                                    {
                                        if (!reader.Entry.IsDirectory)
                                        {
                                            await reader.WriteEntryToDirectory(targetFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                        }
                                    }
                                    else
                                    {
                                        //File already exists
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                            extractTask.SetResult(true);
                        });

                        await extractTask.Task;
                        extractState = true;
                    }
                    zipStream.Dispose();
                }
                targetFolder = null;
            }
            catch (Exception xe)
            {

            }
            return extractState;
        }

        //This was before getting entries with streams
        //for now I'm using it only to get list of entries nothing more
        public static List<string> GetFilesEntries(Stream stream)
        {
            try
            {
                List<string> entriesList = new List<string>();
                var zipArchive = ArchiveFactory.Open(stream);
                //It should support 7z, zip, rar, gz, tar
                var reader = zipArchive.Entries;
                foreach (var entry in reader)
                {
                    if (!entry.IsDirectory)
                    {
                        var key = entry.Key.Replace('/', Path.DirectorySeparatorChar);
                        entriesList.Add(key);
                    }
                }
                return entriesList;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
            return null;
        }
        public static bool isARM()
        {
            try
            {
                Package package = Package.Current;
                string systemArchitecture = package.Id.Architecture.ToString();
                return systemArchitecture.ToLower().Contains("arm") || systemArchitecture.ToLower().Contains("ARM");
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool isX64()
        {
            try
            {
                Package package = Package.Current;
                string systemArchitecture = package.Id.Architecture.ToString();
                return systemArchitecture.ToLower().Contains("x64") || systemArchitecture.ToLower().Contains("amd64");
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool isX86()
        {
            try
            {
                Package package = Package.Current;
                string systemArchitecture = package.Id.Architecture.ToString();
                return systemArchitecture.ToLower().Contains("x86") || systemArchitecture.ToLower().Contains("win32");
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool IsPhone()
        {
            return DeviceIsPhone();
        }
        public static bool DeviceIsPhone()
        {
            try
            {
                return isMobile;
            }
            catch (Exception e)
            {

            }
            return false;
        }

        public static async Task<StorageFolder> CreateLocalFolderDirectoryAsync(string name, bool openIfExists)
        {
            StorageFolder output = null;
            try
            {
                var LocalState = ApplicationData.Current.LocalFolder;
                if (openIfExists)
                {
                    var folder = await LocalState.CreateFolderAsync(name, CreationCollisionOption.OpenIfExists);
                    output = folder;
                }
                else
                {
                    var folder = await LocalState.CreateFolderAsync(name, CreationCollisionOption.ReplaceExisting);
                    output = folder;
                }
            }
            catch (Exception e)
            {
                ShowMessageDirect(e.Message);
            }

            return output;
        }
        public static void SendToClipboard(string text)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(text);
                Clipboard.SetContent(dataPackage);
            }
            catch (Exception e)
            {

            }
        }
        public static string GetLocalString(string name)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            return resourceLoader.GetString(name);
        }

        public static async Task<StorageFile> PickSingleFile(string[] extensions)
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.List;
                picker.SuggestedStartLocation = PickerLocationId.Downloads;
                foreach (string extension in extensions)
                {
                    if (extension.StartsWith("."))
                    {
                        picker.FileTypeFilter.Add(extension);
                    }
                    else
                    {
                        picker.FileTypeFilter.Add($".{extension}");
                    }
                }
                return await picker.PickSingleFileAsync();
            }
            catch (Exception e)
            {
                ShowErrorMessageDirect(e);
            }
            return null;
        }
        public static async Task<StorageFolder> PickSingleFolder()
        {
            try
            {
                try
                {
                    var folderPicker = new FolderPicker();
                    folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                    folderPicker.FileTypeFilter.Add("*");

                    return await folderPicker.PickSingleFolderAsync();
                }
                catch (Exception ex)
                {
                    var folderPicker = new FolderPicker();
                    folderPicker.FileTypeFilter.Add("*");

                    return await folderPicker.PickSingleFolderAsync();
                }
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
            return null;
        }

        public static bool ExtraConfirmation = false;
        public static bool AutoResolveVFSIssues = false;
        public static bool UseWindowsIndexer = false;
        public static bool UseWindowsIndexerSubFolders = false;
        public static async Task<bool> AskForSecondChance()
        {
            try
            {
                var StartSecondChange = false;
                TaskCompletionSource<bool> secondChangeComplete = new TaskCompletionSource<bool>();
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        PlatformService.PlayNotificationSoundDirect("alert");
                        ConfirmConfig confirmSecondChance = new ConfirmConfig();
                        confirmSecondChance.SetTitle("Game Loading");
                        confirmSecondChance.SetMessage("Game failed to load!\nImportant: This could happen due issue with VFS layer or missing BIOS\nDo you want to try without VFS support?\n\nNote: You can set this as auto action from 'Settings' in the main page");
                        confirmSecondChance.UseYesNo();

                        StartSecondChange = await UserDialogs.Instance.ConfirmAsync(confirmSecondChance);
                    }
                    catch (Exception ex)
                    {

                    }
                    secondChangeComplete.SetResult(true);
                });
                await secondChangeComplete.Task;
                return StartSecondChange;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public static Color GetNotificationColorByMessage(string text)
        {
            var background = Colors.DodgerBlue;
            try
            {
                if (text.ToLower().Contains("failed"))
                {
                    background = Colors.Tomato;
                }
                else if (text.ToLower().Contains("error"))
                {
                    background = Colors.Tomato;
                }
                else if (text.ToLower().Contains("without content"))
                {
                    background = Colors.DarkOrange;
                }
                else if (text.ToLower().Contains("not found"))
                {
                    background = Colors.DarkOrange;
                }
                else if (text.ToLower().Contains("unable"))
                {
                    background = Colors.DarkOrange;
                }
                else if (text.ToLower().Contains("cannot"))
                {
                    background = Colors.DarkOrange;
                }
                else if (text.ToLower().Contains("cancelled"))
                {
                    background = Colors.DarkOrange;
                }
                else if (text.ToLower().Contains("empty"))
                {
                    background = Colors.DarkOrange;
                }
                else if (text.ToLower().Contains("success"))
                {
                    background = Colors.Green;
                }
                else if (text.ToLower().Contains("saved"))
                {
                    background = Colors.Green;
                }
                else if (text.ToLower().Contains("done"))
                {
                    background = Colors.Green;
                }
                else if (text.ToLower().Contains("reseted"))
                {
                    background = Colors.Green;
                }
                else if (text.ToLower().Contains("updated"))
                {
                    background = Colors.Green;
                }
                else if (text.ToLower().Contains("finished"))
                {
                    background = Colors.Green;
                }
                else if (text.ToLower().Contains("removed"))
                {
                    background = Colors.Green;
                }
                else if (text.ToLower().Contains("up-to"))
                {
                    background = Colors.Green;
                }
                else if (text.ToLower().Contains("thanks"))
                {
                    background = Colors.Purple;
                }
                else if (text.ToLower().Contains("fast forward on"))
                {
                    background = Colors.Green;
                }
                else if (text.ToLower().Contains("fast forward off"))
                {
                    background = Colors.DarkOrange;
                }
                else if (text.ToLower().Contains("in-game"))
                {
                    background = Colors.Purple;
                }
            }
            catch (Exception ex)
            {

            }
            return background;
        }
        public static char GetIconByMessage(string text)
        {
            var icon = SegoeMDL2Assets.ActionCenterNotification;
            try
            {
                if (text.ToLower().Contains("failed"))
                {
                    icon = SegoeMDL2Assets.Warning;
                }
                else if (text.ToLower().Contains("error"))
                {
                    icon = SegoeMDL2Assets.Error;
                }
                else if (text.ToLower().Contains("without content"))
                {
                    icon = SegoeMDL2Assets.HardDrive;
                }
                else if (text.ToLower().Contains("not found"))
                {
                    icon = SegoeMDL2Assets.FindLegacy;
                }
                else if (text.ToLower().Contains("unable"))
                {
                    icon = SegoeMDL2Assets.Warning;
                }
                else if (text.ToLower().Contains("cannot"))
                {
                    icon = SegoeMDL2Assets.Warning;
                }
                else if (text.ToLower().Contains("empty"))
                {
                    icon = SegoeMDL2Assets.ClearSelection;
                }
                else if (text.ToLower().Contains("backup"))
                {
                    icon = SegoeMDL2Assets.Cloud;
                }
                else if (text.ToLower().Contains("success"))
                {
                    icon = SegoeMDL2Assets.Completed;
                }
                else if (text.ToLower().Contains("done"))
                {
                    icon = SegoeMDL2Assets.Completed;
                }
                else if (text.ToLower().Contains("reseted"))
                {
                    icon = SegoeMDL2Assets.Refresh;
                }
                else if (text.ToLower().Contains("updated"))
                {
                    icon = SegoeMDL2Assets.GlobeLegacy;
                }
                else if (text.ToLower().Contains("finished"))
                {
                    icon = SegoeMDL2Assets.Completed;
                }
                else if (text.ToLower().Contains("removed"))
                {
                    icon = SegoeMDL2Assets.DeleteLegacy;
                }
                else if (text.ToLower().Contains("delete"))
                {
                    icon = SegoeMDL2Assets.DeleteLegacy;
                }
                else if (text.ToLower().Contains("go back"))
                {
                    icon = SegoeMDL2Assets.BackLegacy;
                }
                else if (text.ToLower().Contains("reset"))
                {
                    icon = SegoeMDL2Assets.RefreshLegacy;
                }
                else if (text.ToLower().Contains("save"))
                {
                    icon = SegoeMDL2Assets.Save;
                }
                else if (text.ToLower().Contains("xbox/dpad"))
                {
                    icon = SegoeMDL2Assets.Game;
                }
                else if (text.ToLower().Contains("keyboard/touch"))
                {
                    icon = SegoeMDL2Assets.TouchPointer;
                }
                else if (text.ToLower().Contains("shortcuts"))
                {
                    icon = SegoeMDL2Assets.Game;
                }
                else if (text.ToLower().Contains("thanks"))
                {
                    icon = SegoeMDL2Assets.FavoriteLegacy;
                }
                else if (text.ToLower().Contains("folder"))
                {
                    icon = SegoeMDL2Assets.Folder;
                }
                else if (text.ToLower().Contains("right click"))
                {
                    icon = SegoeMDL2Assets.Mouse;
                }
                else if (text.ToLower().Contains("onscreen"))
                {
                    icon = SegoeMDL2Assets.Process;
                }
                else if (text.ToLower().Contains("fast forward"))
                {
                    icon = SegoeMDL2Assets.Forward;
                }
                else if (text.ToLower().Contains("in-game"))
                {
                    icon = SegoeMDL2Assets.Game;
                }
                else if (text.ToLower().Contains("search"))
                {
                    icon = SegoeMDL2Assets.FindLegacy;
                }
            }
            catch (Exception ex)
            {

            }
            return icon;
        }
    }
    public class LocalNotificationData : EventArgs
    {
        public string message;
        public int time;
        public char icon;
    }
    public static class ExtensionMethods
    {
        public static string ToFileSize(this long l)
        {
            try
            {
                return String.Format(new FileSizeFormatProvider(), "{0:fs}", l);
            }
            catch (Exception e)
            {
                return "0 KB";
            }
        }
    }
    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter)) return this;
            return null;
        }

        private const string fileSizeFormat = "fs";
        private const Decimal OneKiloByte = 1024M;
        private const Decimal OneMegaByte = OneKiloByte * 1024M;
        private const Decimal OneGigaByte = OneMegaByte * 1024M;

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            try
            {
                if (format == null || !format.StartsWith(fileSizeFormat))
                {
                    return defaultFormat(format, arg, formatProvider);
                }
            }
            catch (Exception e)
            {

            }
            if (arg is string)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            Decimal size;

            try
            {
                size = Convert.ToDecimal(arg);
            }
            catch (Exception e)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            string suffix;
            if (size > OneGigaByte)
            {
                size /= OneGigaByte;
                suffix = " GB";
            }
            else if (size > OneMegaByte)
            {
                size /= OneMegaByte;
                suffix = " MB";
            }
            else if (size > OneKiloByte)
            {
                size /= OneKiloByte;
                suffix = " KB";
            }
            else
            {
                suffix = " B";
            }

            string precision = format.Substring(2);
            if (String.IsNullOrEmpty(precision)) precision = "2";
            return String.Format("{0:N" + precision + "}{1}", size, suffix);

        }

        private static string defaultFormat(string format, object arg, IFormatProvider formatProvider)
        {
            IFormattable formattableArg = arg as IFormattable;
            if (formattableArg != null)
            {
                return formattableArg.ToString(format, formatProvider);
            }
            return arg.ToString();
        }



    }
    public static class StringHasher
    {
        private static readonly UTF8Encoding Encoder = new UTF8Encoding();

        public static string MD5(this string input)
        {
            return HashString(input, HashAlgorithmName.MD5);
        }

        public static string SHA1(this string input)
        {
            return HashString(input, HashAlgorithmName.SHA1);
        }

        private static string HashString(string input, HashAlgorithmName algorithmName)
        {
            using (var hasher = IncrementalHash.CreateHash(algorithmName))
            {
                var bytes = Encoder.GetBytes(input);
                hasher.AppendData(bytes);
                bytes = hasher.GetHashAndReset();
                var hash = BitConverter.ToString(bytes);
                return hash.Replace("-", string.Empty).ToLower();
            }
        }
    }
    public class ShortcutEntry
    {
        public List<string> keys = new List<string>();
        public string action = "";
        public string name = "";
        public string key = "";
        public ShortcutEntry(List<string> ks, string a, string n, string k)
        {
            keys = ks;
            action = a;
            name = n;
            key = k;
        }
    }
    public class CRC32Tool
    {
        private readonly uint[] ChecksumTable;
        private readonly uint Polynomial = 0xEDB88320;

        public CRC32Tool()
        {
            ChecksumTable = new uint[0x100];

            for (uint index = 0; index < 0x100; ++index)
            {
                uint item = index;
                for (int bit = 0; bit < 8; ++bit)
                    item = ((item & 1) != 0) ? (Polynomial ^ (item >> 1)) : (item >> 1);
                ChecksumTable[index] = item;
            }
        }

        public async Task<string> ComputeHash(Stream stream)
        {
            uint result = 0xFFFFFFFF;

            int current;
            while ((current = stream.ReadByte()) != -1)
                result = ChecksumTable[(result & 0xFF) ^ (byte)current] ^ (result >> 8);

            byte[] hash = BitConverter.GetBytes(~result);
            Array.Reverse(hash);

            String hashString = String.Empty;

            foreach (byte b in hash) hashString += b.ToString("x2").ToLower();

            return hashString;
        }
    }
    public class KeyboardLayout : BindableBase
    {
        public ObservableCollection<GroupKeyboardKeyListItems> Main = new ObservableCollection<GroupKeyboardKeyListItems>();

        public KeyboardLayout(string systemName)
        {
            Dictionary<string, GroupKeyboardKeyListItems> groups = new Dictionary<string, GroupKeyboardKeyListItems>();

            var tempList = PlatformService.keyboardKeys.Select(el => new KeyboardKey(el.getKeyName(), el.getKeyGroup(), el.getKeyCode(), el.getShiftName())).ToList();
            foreach (var kItem in tempList)
            {
                GroupKeyboardKeyListItems testList;
                if (!groups.TryGetValue(kItem.KeyGroup, out testList))
                {
                    testList = new GroupKeyboardKeyListItems(kItem.KeyGroup);
                    groups.Add(kItem.KeyGroup, testList);
                }
                if (!kItem.IsControl)
                {
                    //I should resolve some changes here
                    //Not all systems use the same keyboard layout
                    switch (systemName)
                    {
                        case "Amstrad CPC":
                        case "Caprice32":
                            switch (kItem.KeyName)
                            {
                                case "2":
                                    kItem.KeyName = "2";
                                    kItem.ShiftName = "\"";
                                    break;
                                case "6":
                                    kItem.KeyName = "6";
                                    kItem.ShiftName = "&";
                                    break;
                                case "7":
                                    kItem.KeyName = "7";
                                    kItem.ShiftName = "'";
                                    break;
                                case "8":
                                    kItem.KeyName = "8";
                                    kItem.ShiftName = "(";
                                    break;
                                case "9":
                                    kItem.KeyName = "9";
                                    kItem.ShiftName = ")";
                                    break;
                                case "0":
                                    kItem.KeyName = "0";
                                    kItem.ShiftName = "_";
                                    break;
                                case "-":
                                    kItem.KeyName = "-";
                                    kItem.ShiftName = "=";
                                    break;
                                case "=":
                                    kItem.KeyName = "^";
                                    kItem.ShiftName = "€";
                                    break;
                                case "[":
                                    kItem.KeyName = "@";
                                    kItem.ShiftName = "|";
                                    break;
                                case "]":
                                    kItem.KeyName = "[";
                                    kItem.ShiftName = "{";
                                    break;
                                case ";":
                                    kItem.KeyName = ":";
                                    kItem.ShiftName = "*";
                                    break;
                                case "'":
                                    kItem.KeyName = ";";
                                    kItem.ShiftName = "+";
                                    break;
                                case "\\":
                                    kItem.KeyName = "]";
                                    kItem.ShiftName = "}";
                                    break;
                                case "Home":
                                    kItem.ShiftName = "Play";
                                    kItem.ShiftTextVisible = true;
                                    break;
                                case "End":
                                    kItem.ShiftName = "Stop";
                                    kItem.ShiftTextVisible = true;
                                    break;
                                case "Insert":
                                    kItem.ShiftName = "Switch";
                                    kItem.ShiftTextVisible = true;
                                    break;
                                case "P.Up":
                                    kItem.ShiftName = "Rewind";
                                    kItem.ShiftTextVisible = true;
                                    break;
                                case "Del":
                                    kItem.ShiftName = "Clear";
                                    kItem.ShiftTextVisible = true;
                                    break;
                                case "`":
                                    kItem.IsEnabled = false;
                                    break;
                            }
                            break;

                    }
                    testList.Add(kItem);
                }
            }
            foreach (var gItem in groups)
            {
                Main.Add(gItem.Value);
            }
        }
        public bool ShiftKeyState = false;
        public bool CapsKeyState = false;
        public void SetShiftKeyState(bool state)
        {
            ShiftKeyState = state;
            foreach (var kItem in Main)
            {
                foreach (var iItem in kItem)
                {
                    iItem.ShiftKeyState = state;
                }
            }
        }
        public void SetCapsKeyState(bool state)
        {
            CapsKeyState = state;
            foreach (var kItem in Main)
            {
                foreach (var iItem in kItem)
                {
                    iItem.CapsKeyState = state;
                }
            }
        }
        public void UpdateKeyboardKeySize(int width, int height, int fontSize, int subFontSize)
        {
            foreach (var kItem in Main)
            {
                foreach (var iItem in kItem)
                {
                    iItem.UpdateKeyboardKeySize(width, height, fontSize, subFontSize);
                }
            }
        }
    }
    public class KeyboardKey : BindableBase
    {
        string keyName;
        string[] chars = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        string[] special = new string[] { "Back", "Caps", "Up", "Left", "Right", "Down", "Del", "Space", "Enter", "Win", "Tab", "Home", "End", "Insert", "P.Up", "P.Down" };
        string[] controls = new string[] { "Esc", "Ctrl", "Shift", "Alt" };
        string[] switchs = new string[] { "Caps", "Ctrl", "Shift", "Alt" };
        private bool isEnabled = true;
        public KeyboardKey KeyRef;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                RaisePropertyChanged(nameof(IsEnabled));
            }
        }
        public bool IsSwitch = false;
        public bool IsOn = false;
        Dictionary<string, Color> specificColors = new Dictionary<string, Color>()
        {
            {"Back", Colors.OrangeRed },
            {"Del", Colors.OrangeRed },
            {"Caps", Colors.Gold },
            {"Up", Colors.Purple },
            {"Left", Colors.Purple },
            {"Down", Colors.Purple },
            {"Right", Colors.Purple },
            {"Space", Colors.Green },
            {"Enter", Colors.Orange },
            {"Win", Colors.Orange },
            {"Tab", Colors.Gray },
            {"Home", Colors.Gray },
            {"End", Colors.Gray },
            {"Insert", Colors.Gray },
            {"P.Up", Colors.Gray },
            {"P.Down", Colors.Gray },
        };
        public SolidColorBrush BorderColor
        {
            get
            {
                if (IsEnabled && !IsSpecial)
                {
                    return new SolidColorBrush((Color)Helpers.HexToColor("#FA304ffe"));
                }
                else
                {
                    if (IsSpecial)
                    {
                        try
                        {
                            return new SolidColorBrush(specificColors[keyName]);
                        }
                        catch (Exception ex)
                        {
                            return new SolidColorBrush((Color)Helpers.HexToColor("#AA304ffe"));
                        }
                    }
                    else
                    {
                        return new SolidColorBrush((Color)Helpers.HexToColor("#AA304ffe"));
                    }
                }
            }
        }
        public double KeyOpacity
        {
            get
            {
                if (IsEnabled)
                {
                    return 1;
                }
                else
                {
                    return 0.8;
                }
            }
        }
        public async void SwitchKey(bool state)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
            {
                try
                {
                    if (IsSwitch)
                    {
                        IsOn = !IsOn;
                        if (IsOn)
                        {
                            BackgroundColor = new SolidColorBrush(Colors.DarkOrange);
                        }
                        else
                        {
                            BackgroundColor = new SolidColorBrush((Color)Helpers.HexToColor("#AA000000"));
                        }
                        RaisePropertyChanged(nameof(BackgroundColor));
                    }
                    else
                    {
                        IsOn = state;
                        if (IsOn)
                        {
                            BackgroundColor = new SolidColorBrush(Colors.DodgerBlue);
                        }
                        else
                        {
                            BackgroundColor = new SolidColorBrush((Color)Helpers.HexToColor("#AA000000"));
                        }
                        RaisePropertyChanged(nameof(BackgroundColor));
                    }
                }
                catch (Exception ex)
                {

                }
            });
        }
        SolidColorBrush backgroundColor = new SolidColorBrush((Color)Helpers.HexToColor("#AA000000"));
        public SolidColorBrush BackgroundColor
        {
            get
            {
                return backgroundColor;
            }
            set
            {
                backgroundColor = value;
            }
        }
        public string KeyName
        {
            get
            {
                if (CapsKeyState)
                {
                    if (chars.Contains(keyName.ToUpper()))
                    {
                        return shiftName;
                    }
                }
                return ShiftKeyState && ShiftTextVisible ? shiftName : keyName;
            }
            set
            {
                keyName = value;
            }
        }
        public string ButtonName;
        public string KeyGroup;
        string shiftName;
        public string ShiftName
        {
            get
            {
                if (CapsKeyState)
                {
                    if (chars.Contains(keyName.ToUpper()))
                    {
                        return keyName;
                    }
                }
                return !ShiftKeyState ? shiftName : keyName;
            }
            set
            {
                shiftName = value;
            }
        }
        public bool capsKeyState = false;
        public bool CapsKeyState
        {
            get
            {
                return capsKeyState;
            }
            set
            {
                capsKeyState = value;
                RaisePropertyChanged(nameof(KeyName));
                RaisePropertyChanged(nameof(ShiftName));
            }
        }

        public bool shiftKeyState = false;
        public bool ShiftTextVisible = false;
        public Visibility ShiftTextVisibleState
        {
            get
            {
                return ShiftTextVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool ShiftKeyState
        {
            get
            {
                return shiftKeyState;
            }
            set
            {
                shiftKeyState = value;
                RaisePropertyChanged(nameof(KeyName));
                RaisePropertyChanged(nameof(ShiftName));
            }
        }
        public Keys KeyCode;
        public Thickness Margin
        {
            get
            {
                if (ShiftTextVisible)
                {
                    return new Thickness(0, -10, 0, 0);
                }
                else
                {
                    return new Thickness(0, 0, 0, 0);
                }
            }
        }
        public Thickness Padding
        {
            get
            {
                if (ShiftTextVisible)
                {
                    return new Thickness(15, 4, 15, 4);
                }
                else
                {
                    return new Thickness(25, 8, 25, 8);
                }
            }
        }
        public Thickness ContainerPadding
        {
            get
            {
                if (ShiftTextVisible)
                {
                    return new Thickness(0, 0, 0, 0);
                }
                else
                {
                    return new Thickness(5, 0, 5, 0);
                }
            }
        }

        public int MinWidth = 35;
        public int MinHeight = 25;
        public int MainFontSize = 12;
        public int SubFontSize = 10;
        public bool IsControl = false;
        public bool IsSpecial = false;
        public bool IsCaps = false;

        private string name;
        private string group;
        private Keys code;
        private string shift;
        public string getKeyName()
        {
            return name;
        }
        public Keys getKeyCode()
        {
            return code;
        }

        public string getKeyGroup()
        {
            return group;
        }
        public string getShiftName()
        {
            return shift;
        }

        public KeyboardKey(string name, string group, Keys code, string shift = "")
        {
            this.name = name;
            this.group = group.ToUpper();
            this.code = code;
            this.shift = shift;

            KeyName = (chars.Contains(name) ? name.ToLower() : name);
            KeyGroup = group;
            KeyCode = code;
            ButtonName = $"Button_{code}";
            ShiftName = shift.Length > 0 ? shift : (chars.Contains(name) ? name.ToUpper() : name);
            if (shift.Length > 0 || chars.Contains(name))
            {
                ShiftTextVisible = true;
            }
            IsSpecial = special.Contains(name);
            IsControl = controls.Contains(name);
            IsSwitch = switchs.Contains(name);
            IsCaps = name.Equals("Caps");
            KeyRef = this;
        }
        public void UpdateKeyboardKeySize(int width, int height, int mainFontSize, int subFontSize)
        {
            try
            {
                MinHeight = height;
                MinWidth = width;
                MainFontSize = mainFontSize;
                SubFontSize = subFontSize;
                RaisePropertyChanged(nameof(MinHeight));
                RaisePropertyChanged(nameof(MinWidth));
                RaisePropertyChanged(nameof(MainFontSize));
                RaisePropertyChanged(nameof(SubFontSize));
            }
            catch (Exception ex)
            {

            }
        }
    }

    public class GroupKeyboardKeyListItems : ObservableCollection<KeyboardKey>
    {
        public GroupKeyboardKeyListItems(string key)
        {
            Key = key;
        }
        public GroupKeyboardKeyListItems(ObservableCollection<KeyboardKey> keyboardKeyListItems, string key)
        {
            foreach (var item in keyboardKeyListItems)
            {
                base.Add(item);
            }
            Key = key;
        }
        public string Key { get; set; }
    }

    public class InsightItem
    {
        public string CoreName;
        public string SystemName;
        public string GameName;
        public long Date;
        public long CTime = 0;
        public long Time;
    }

}
