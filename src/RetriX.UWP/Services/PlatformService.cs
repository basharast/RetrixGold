using LibRetriX;
using Microsoft.Graphics.Canvas;
using MvvmCross.Platform;
using Newtonsoft.Json;
using Plugin.FileSystem;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
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
using WinUniversalTool.Models;


namespace RetriX.UWP.Services
{
    public class PlatformService : IPlatformService
    {
        public static IFileInfo OpenBackupFile = null;
        public static int InitWidthSize = 600;
        public static HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
        private static readonly ISet<string> DeviceFamiliesAllowingFullScreenChange = new HashSet<string>
        {
            "Windows.Desktop", "Windows.Team", "Windows.Mobile"
        };

        public static bool isCoresLoaded = false;
        public static bool MuteSFX = false;

        public bool IsCoresLoaded
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
        public bool IsAppStartedByFile
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
                        ChangeToXBoxModeRequestedDirect(null, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        public bool XBoxMode
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
        private ApplicationView AppView => ApplicationView.GetForCurrentView();
        private CoreWindow CoreWindow => CoreWindow.GetForCurrentThread();
        private ISet<VirtualKey> PressedKeys { get; } = new HashSet<VirtualKey>();

        public bool FullScreenChangingPossible
        {
            get
            {
                var output = DeviceFamiliesAllowingFullScreenChange.Contains(AnalyticsInfo.VersionInfo.DeviceFamily);
                return output;
            }
        }

        public bool IsFullScreenMode => AppView.IsFullScreenMode;

        public bool TouchScreenAvailable => new TouchCapabilities().TouchPresent > 0;

        public bool ShouldDisplayTouchGamepad
        {
            get
            {
                return false;
            }
        }

        private bool handleGameplayKeyShortcuts = false;
        public bool HandleGameplayKeyShortcuts
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

        public event EventHandler<FullScreenChangeEventArgs> FullScreenChangeRequested;
        public static bool pageReady = false;
        public static bool isGamesList = false;
        public static event EventHandler RestoreGamesListStateGlobal;
        public static GameSystemRecentModel gameSystemViewModel = null;
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
        public double veScroll
        {
            get
            {
                return vScroll;
            }
        }
        public void RestoreGamesListState(double currentIndex)
        {
            if (RestoreGamesListStateGlobal != null)
            {
                RestoreGamesListStateGlobal.Invoke(currentIndex, EventArgs.Empty);
            }
        }
        public static EventHandler SaveListStateGlobal;
        public static EventHandler NotificationHandler;
        public static EventHandler NotificationHandlerMain;
        public bool ShowNotification(string text, int time)
        {
            if (NotificationHandler == null)
            {
                return false;
            }
            try
            {
                LocalNotificationData localNotificationData = new LocalNotificationData();
                localNotificationData.icon = SegoeMDL2Assets.GameConsole;
                localNotificationData.message = text;
                localNotificationData.time = time;
                NotificationHandler(null, localNotificationData);
            }
            catch(Exception x)
            {
                return false;
            }
            return true;
        } 
        public bool ShowNotificationMain(string text, int time)
        {
            if (NotificationHandlerMain == null)
            {
                return false;
            }
            try
            {
                LocalNotificationData localNotificationData = new LocalNotificationData();
                localNotificationData.icon = SegoeMDL2Assets.GameConsole;
                localNotificationData.message = text;
                localNotificationData.time = time;
                NotificationHandlerMain(null, localNotificationData);
            }
            catch(Exception x)
            {
                return false;
            }
            return true;
        }

        public void SaveGamesListState()
        {
            if (SaveListStateGlobal != null)
            {
                SaveListStateGlobal.Invoke(null, EventArgs.Empty);
            }
        }
        public event EventHandler PauseToggleRequested;
        public event EventHandler XBoxMenuRequested;
        public event EventHandler QuickSaveRequested;
        public event EventHandler SavesListRequested;
        public static event EventHandler ChangeToXBoxModeRequestedDirect;
        public event EventHandler ChangeToXBoxModeRequested
        {
            remove
            {
                ChangeToXBoxModeRequestedDirect -= value;
            }
            add
            {
                ChangeToXBoxModeRequestedDirect += value;
            }
        }

        public event EventHandler<GameStateOperationEventArgs> GameStateOperationRequested;

        public async Task<bool> ChangeFullScreenStateAsync(FullScreenChangeType changeType)
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
                    throw new Exception("this should never happen");
            }

            await Task.Delay(100);
            return result;
        }

        public string GetMemoryUsage()
        {
            try
            {
                var appMemory = (long)MemoryManager.AppMemoryUsage;
                return appMemory.ToFileSize();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        public long GetMemoryUsageLong()
        {
            try
            {
                var appMemory = (long)MemoryManager.AppMemoryUsage;
                return appMemory;
            }
            catch(Exception ex)
            {
                return 0;
            }
        }
        public void ChangeMousePointerVisibility(MousePointerVisibility visibility)
        {
            var pointer = visibility == MousePointerVisibility.Hidden ? null : new CoreCursor(CoreCursorType.Arrow, 0);
            Window.Current.CoreWindow.PointerCursor = pointer;
        }

        public void ForceUIElementFocus()
        {
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
        }

        public void CopyToClipboard(string content)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(content);
            Clipboard.SetContent(dataPackage);
        }

        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if (!PressedKeys.Contains(key))
            {
                PressedKeys.Add(key);
            }

            switch (key)
            {
                //Shift+Enter: enter fullscreen
                case VirtualKey.Enter:
                    if (PressedKeys.Contains(VirtualKey.Shift))
                    {
                        FullScreenChangeRequested(this, new FullScreenChangeEventArgs(FullScreenChangeType.Toggle));
                    }
                    break;

                case VirtualKey.Escape:
                    FullScreenChangeRequested(this, new FullScreenChangeEventArgs(FullScreenChangeType.Exit));
                    break;
                case VirtualKey.P:
                    if (PressedKeys.Contains(VirtualKey.Shift))
                    {
                        PauseToggleRequested(this, EventArgs.Empty);
                    }
                    break;
                case VirtualKey.GamepadView:
                    if (PressedKeys.Contains(VirtualKey.GamepadMenu))
                    {
                        PauseToggleRequested(this, EventArgs.Empty);
                    }
                    break;

                case VirtualKey.F1:
                    HandleSaveSlotKeyPress(1);
                    break;

                case VirtualKey.F2:
                    HandleSaveSlotKeyPress(2);
                    break;

                case VirtualKey.F3:
                    HandleSaveSlotKeyPress(3);
                    break;

                case VirtualKey.F4:
                    HandleSaveSlotKeyPress(4);
                    break;

                case VirtualKey.F5:
                    HandleSaveSlotKeyPress(5);
                    break;

                case VirtualKey.F6:
                    HandleSaveSlotKeyPress(6);
                    break;
                case VirtualKey.F7:
                    HandleSaveSlotKeyPress(7);
                    break;
                case VirtualKey.F8:
                    HandleSaveSlotKeyPress(8);
                    break;
                case VirtualKey.F9:
                    HandleSaveSlotKeyPress(9);
                    break;
                case VirtualKey.F10:
                    HandleSaveSlotKeyPress(10);
                    break;
                case VirtualKey.Number7:
                    HandleLoadSlotKeyPress(7);
                    break;
                case VirtualKey.Number8:
                    HandleLoadSlotKeyPress(8);
                    break;
                case VirtualKey.Number9:
                    HandleLoadSlotKeyPress(9);
                    break;
                case VirtualKey.Number1:
                    HandleActionSlotKeyPress(1);
                    break;
                case VirtualKey.Number2:
                    HandleActionSlotKeyPress(2);
                    break;
                case VirtualKey.Number3:
                    HandleActionSlotKeyPress(3);
                    break;
                case VirtualKey.Number4:
                    HandleActionSlotKeyPress(4);
                    break;
                case VirtualKey.Number6:
                    XBoxMenuRequested(this, EventArgs.Empty);
                    break;
                case VirtualKey.GamepadDPadDown:
                    if (PressedKeys.Contains(VirtualKey.GamepadView) && !CommandInProgress)
                    {
                        CommandInProgress = true;
                        XBoxMenuRequested(this, EventArgs.Empty);
                    }
                    break;
                case VirtualKey.GamepadDPadLeft:
                    if (PressedKeys.Contains(VirtualKey.GamepadView) && !CommandInProgress)
                    {
                        CommandInProgress = true;
                        QuickSaveRequested(this, EventArgs.Empty);
                    }
                    break;
                case VirtualKey.GamepadDPadRight:
                    if (PressedKeys.Contains(VirtualKey.GamepadView) && !CommandInProgress)
                    {
                        CommandInProgress = true;
                        SavesListRequested(this, EventArgs.Empty);
                    }
                    break;
            }

            args.Handled = true;
        }
        bool CommandInProgress = false;
        private string KeyNameResolve(InjectedInputTypes TargetInput)
        {
            string TargetInputName = TargetInput.ToString();
            TargetInputName = TargetInputName.Replace(InjectedInputTypes.DeviceIdJoypadL.ToString(), InputTypes.DeviceIdJoypadL2.ToString());
            TargetInputName = TargetInputName.Replace(InjectedInputTypes.DeviceIdJoypadR.ToString(), InputTypes.DeviceIdJoypadR2.ToString());
            TargetInputName = TargetInputName.Replace(InjectedInputTypes.DeviceIdJoypadC.ToString(), InputTypes.DeviceIdJoypadL.ToString());
            TargetInputName = TargetInputName.Replace(InjectedInputTypes.DeviceIdJoypadZ.ToString(), InputTypes.DeviceIdJoypadR.ToString());
            return TargetInputName;
        }
        private void OnKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if (PressedKeys.Contains(key))
            {
                PressedKeys.Remove(key);
            }
            CommandInProgress = false;
            args.Handled = true;
        }

        private void HandleSaveSlotKeyPress(uint slotID)
        {
            var modifierKeyPressed = PressedKeys.Contains(VirtualKey.Shift);
            var eventArgs = new GameStateOperationEventArgs(modifierKeyPressed ? GameStateOperationEventArgs.GameStateOperationType.Load : GameStateOperationEventArgs.GameStateOperationType.Save, slotID);
            GameStateOperationRequested(this, eventArgs);
        }

        private void HandleLoadSlotKeyPress(uint slotID)
        {
            var modifierKeyPressed = PressedKeys.Contains(VirtualKey.Shift);
            var eventArgs = new GameStateOperationEventArgs(GameStateOperationEventArgs.GameStateOperationType.Load, slotID);
            GameStateOperationRequested(this, eventArgs);
        }
        private void HandleActionSlotKeyPress(uint slotID)
        {
            var modifierKeyPressed = PressedKeys.Contains(VirtualKey.Shift);
            var eventArgs = new GameStateOperationEventArgs(GameStateOperationEventArgs.GameStateOperationType.Action, slotID);
            GameStateOperationRequested(this, eventArgs);
        }

        static Dictionary<string, MediaElement> EffectsTemp = new Dictionary<string, MediaElement>();
        public void StopNotificationSound(string TargetSound)
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
        public async void PlayNotificationSound(string TargetSound)
        {
            if (MuteSFX)
            {
                return;
            }
            double MediaVolume = GetVolumeValue(TargetSound);
            try
            {
                if (Window.Current != null)
                {
                    await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
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
                else
                {
                    //await Task.Run(() => PlayMediaDirect(TargetSound, MediaVolume));
                }
            }
            catch (Exception e)
            {
                //ShowErrorMessage(e);
            }

        }

        public static async void PlayNotificationSoundDirect(string TargetSound)
        {
            if (MuteSFX)
            {
                return;
            }
            double MediaVolume = GetVolumeValue(TargetSound);
            try
            {
                if (Window.Current != null)
                {
                    await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
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
                else
                {
                    //await Task.Run(() => PlayMediaDirect(TargetSound, MediaVolume));
                }
            }
            catch (Exception e)
            {
                ShowErrorMessageDirect(e);
            }

        }
        public static async Task PlayMediaDirect(string TargetSound, double MediaVolume)
        {
            MediaElement TempEffect;
            if (EffectsTemp.TryGetValue(TargetSound, out TempEffect))
            {
                TempEffect.Volume = MediaVolume;
                TempEffect.Play();
            }
            else
            {
                MediaElement NotificationSound = new MediaElement();
                StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets\SFX");
                StorageFile file = await folder.GetFileAsync(TargetSound);
                var stream = await file.OpenAsync(FileAccessMode.Read);
                NotificationSound.Volume = MediaVolume;
                NotificationSound.SetSource(stream, file.ContentType);
                if (!EffectsTemp.TryGetValue(TargetSound, out TempEffect))
                {
                    EffectsTemp.Add(TargetSound, NotificationSound);
                }
                NotificationSound.Play();
            }
        }
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


        static string TempError = "";
        public async void ShowErrorMessage(Exception e,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (TempError == e.Message)
            {

            }
            string ExtraData = "";
            if (memberName.Length > 0 && sourceLineNumber > 0)
            {
                sourceFilePath = Path.GetFileName(sourceFilePath);
                ExtraData = $"\n\nName: {memberName}\nLine: {sourceLineNumber}\nFile: {sourceFilePath}\n\n Please send these data to services@astifan.online";
            }
            try
            {
                PlayNotificationSound("error.mp3");
                var messageDialog = new MessageDialog(e.Message.ToString() + " " + ExtraData);
                await messageDialog.ShowAsync();
            }
            catch (Exception ex)
            {

            }
        }
        public static async void ShowErrorMessageDirect(Exception e,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            string ExtraData = "";
            if (memberName.Length > 0 && sourceLineNumber > 0)
            {
                sourceFilePath = Path.GetFileName(sourceFilePath);
                ExtraData = $"\nName: {memberName}\nLine: {sourceLineNumber}\nFile: {sourceFilePath}\n\n Please send these data to services@astifan.online";
            }
            try
            {
                PlayNotificationSoundDirect("error.mp3");
                var messageDialog = new MessageDialog(e.Message.ToString() + " " + ExtraData);
                await messageDialog.ShowAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public static async void ShowMessageDirect(String message)
        {
            try
            {
                PlayNotificationSoundDirect("alert.mp3");
                var messageDialog = new MessageDialog(message);
                await messageDialog.ShowAsync();
            }
            catch (Exception ex)
            {

            }
        }
        public static async void ShowMessageDirect(String message, string sound)
        {
            try
            {
                PlayNotificationSoundDirect(sound);
                var messageDialog = new MessageDialog(message);
                await messageDialog.ShowAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public bool gameNoticeShowed = false;
        public bool GameNoticeShowed
        {
            get => gameNoticeShowed;
            set
            {
                gameNoticeShowed = value;
            }
        }



        public async Task<string> GetFileToken(string FileLocation)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(FileLocation);
            string FileToken = StorageApplicationPermissions.FutureAccessList.Add(file);
            return FileToken;
        }

        public async Task<IDirectoryInfo> GetRecentsLocationAsync()
        {
            var localFolder = await GetRecentsLocationAsyncDirect();
            return localFolder;
        }
        public static async Task<IDirectoryInfo> GetRecentsLocationAsyncDirect()
        {
            var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(RecentsLocation);
            if (localFolder == null)
            {
                await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(RecentsLocation);
                localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(RecentsLocation);
            }
            return localFolder;
        }
        public static string RecentsLocation = "SaveRecents";
        //Game Recent Data, Key: System Name - [Game Name, Game Location, Token, Root Needed, Play Count, GameID, Total Time]
        private static Dictionary<string, List<string[]>> GamesRecents = new Dictionary<string, List<string[]>>();
        public Dictionary<string, List<string[]>> GetGamesRecents()
        {
            return GamesRecents;
        }
        public async Task AddGameToRecents(string system, string FilePath, bool RootNeeded, string gameID, long totalTime, bool delete = false)
        {
            try
            {

                string SystemName = system;
                string GameLocation = FilePath;
                string GameName = Path.GetFileName(FilePath);
                //string GameToken = await PlatformService.GetFileToken(GameLocation);
                string GameToken = "";
                string RootState = RootNeeded ? "1" : "0";
                string OpenCount = (GetGameOpenCount(SystemName, GameLocation) + (totalTime > 0 ? 0 : 1)).ToString();
                string TotalTimePlayed = (GetGamePlayedTime(SystemName, GameLocation) + totalTime).ToString();
                string[] NewGame = new string[] { GameName, GameLocation, GameToken, RootState, OpenCount, gameID, TotalTimePlayed };
                var SnapshotNameDelete = "";
                List<string[]> TempContent = new List<string[]>();
                if (GamesRecents != null)
                {
                    if (GamesRecents.TryGetValue(SystemName, out TempContent))
                    {

                        int GameRowIndex = 0;
                        bool GameFound = false;
                        foreach (string[] GameRow in TempContent)
                        {
                            if (GameRow[1].Contains(GameLocation))
                            {
                                TempContent[GameRowIndex] = NewGame;
                                GameFound = true;
                                break;
                            }
                            GameRowIndex++;
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
                                SnapshotNameDelete = $"{gameID}.png";
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

                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(RecentsLocation);
                if (localFolder == null)
                {
                    await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(RecentsLocation);
                    localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(RecentsLocation);
                }
                else
                {
                    if (SnapshotNameDelete.Length > 0)
                    {
                        var SnapshotFile = await localFolder.GetFileAsync(SnapshotNameDelete);
                        if (SnapshotFile != null)
                        {
                            await SnapshotFile.DeleteAsync();
                        }
                    }
                }

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(GamesRecents));
                if (dictionaryListBytes != null)
                {
                    var targetFileTest = await localFolder.GetFileAsync("recents.rtx");
                    if (targetFileTest != null)
                    {
                        await targetFileTest.DeleteAsync();
                    }

                    var targetFile = await localFolder.CreateFileAsync("recents.rtx");
                    using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                    {
                        await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                        await outStream.FlushAsync();
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
        public int GetGameOpenCount(string SystemName, string GameLocation)
        {
            try
            {
                if (GamesRecents != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            if (GamesList[1].Contains(GameLocation))
                            {
                                return Int32.Parse(GamesList[4]);
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
        public long GetGamePlayedTime(string SystemName, string GameLocation)
        {
            try
            {
                if (GamesRecents != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            if (GamesList[1].Contains(GameLocation))
                            {
                                return Int32.Parse(GamesList[6]);
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
        public string GetGameID(string SystemName, string GameLocation)
        {
            try
            {
                if (GamesRecents != null && SystemName != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            if (GamesList[1].Contains(GameLocation))
                            {
                                try
                                {
                                    return GamesList[5];
                                }
                                catch (Exception e)
                                {
                                    return "";
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
            return "";
        }
        public int GetPlaysCount(string SystemName)
        {
            int totalCount = 0;
            try
            {
                if (GamesRecents != null && SystemName != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            totalCount += Int32.Parse(GamesList[4]);
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
        public int GetGamePlaysCount(string SystemName, string GameName)
        {
            int totalCount = 0;
            try
            {
                if (GamesRecents != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            if (GamesList[1].Equals(GameName))
                            {
                                return Int32.Parse(GamesList[4]);
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
        public string GetGameLocation(string SystemName, string GameName)
        {
            try
            {
                if (GamesRecents != null)
                {
                    List<string[]> GamesListTemp = new List<string[]>();
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            if (GamesList[0].Contains(GameName))
                            {
                                return GamesList[1];
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
        public bool GetGameRootNeeded(string SystemName, string GameLocation)
        {
            try
            {
                List<string[]> GamesListTemp = new List<string[]>();
                if (GamesRecents != null)
                {
                    if (GamesRecents.TryGetValue(SystemName, out GamesListTemp))
                    {
                        foreach (string[] GamesList in GamesListTemp)
                        {
                            if (GamesList[1].Contains(GameLocation))
                            {
                                return GamesList[3] == "1";
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
        public async Task RetriveRecents()
        {
            await RetriveRecentsDirect();
        }
        public static async Task RetriveRecentsDirect()
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(RecentsLocation);

                if (localFolder != null)
                {
                    var targetFileTest = await localFolder.GetFileAsync("recents.rtx");
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
            }
            catch (Exception e)
            {
                ShowErrorMessageDirect(e);
            }
        }
        public void ClearRecentsBySystem(string SystemName)
        {
            GamesRecents.Remove(SystemName);
        }
        public void AddNewSystemOpenCount(string SystemName)
        {
            try
            {
                int OpenCounts = Plugin.Settings.CrossSettings.Current.GetValueOrDefault(SystemName, 0) + 1;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(SystemName, OpenCounts);
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
        }

        public unsafe byte[] ConvertBytesToBitmap(int height, int width, byte[] bytes)
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
        public static bool StopGameInProgress = false;
        public void SetGameStopInProgress(bool StopState)
        {
            StopGameInProgress = StopState;
        }
        public void SetStopHandler(EventHandler eventHandler)
        {
            StopHandler += eventHandler;
        }

        public void DeSetStopHandler(EventHandler eventHandler)
        {
            StopHandler -= eventHandler;
        }

        public void InvokeStopHandler(Object rootFrame)
        {
            try
            {
                if (StopHandler != null)
                {
                    StopHandler.Invoke(this, EventArgs.Empty);
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

        public static void SetHideSubPageHandler(EventHandler eventHandler)
        {
            HideSubHandler += eventHandler;
        }
        public void InvokeSubPageHandler(Object rootFrame)
        {
            try
            {
                if (HideSubHandler != null)
                {

                    HideSubHandler.Invoke(this, EventArgs.Empty);
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
        public void SetSubPageState(bool SubPageState)
        {
            SubPageActive = SubPageState;
        }

        public void SetHideCoreOptionsHandler(EventHandler eventHandler)
        {
            HideCoreOptionsHandler += eventHandler;
        }

        public void DeSetHideCoreOptionsHandler(EventHandler eventHandler)
        {
            HideCoreOptionsHandler -= eventHandler;
        }

        public void InvokeHideCoreOptionsHandler(Object rootFrame)
        {
            try
            {
                if (HideCoreOptionsHandler != null)
                {
                    HideCoreOptionsHandler.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    CoreOptionsActive = false;
                    ((Frame)rootFrame).GoBack();
                }
            }
            catch (Exception ee)
            {

            }
        }
        public void SetCoreOptionsState(bool SubPageState)
        {
            CoreOptionsActive = SubPageState;
        }

        public static bool SavesListActive = false;
        public static event EventHandler HideSavesListHandler;
        public void SetHideSavesListHandler(EventHandler eventHandler)
        {
            HideSavesListHandler += eventHandler;
        }
        public void DeSetHideSavesListHandler(EventHandler eventHandler)
        {
            HideSavesListHandler -= eventHandler;
        }
        public void InvokeHideSavesListHandler(Object rootFrame)
        {
            try
            {
                if (HideSavesListHandler != null)
                {
                    HideSavesListHandler.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    SavesListActive = false;
                    ((Frame)rootFrame).GoBack();
                }
            }
            catch (Exception ee)
            {

            }
        }
        public void SetSavesListActive(bool SavesListActiveState)
        {
            SavesListActive = SavesListActiveState;
        }

        public static bool FilterModeState = false;
        public void SetFilterModeState(bool filterModeState)
        {
            FilterModeState = filterModeState;
        }

        private static event EventHandler ResetSelectionHandler;
        public void SetResetSelectionHandler(EventHandler eventHandler)
        {
            ResetSelectionHandler += eventHandler;
        }
        public void InvokeResetSelectionHandler(Object rootFrame)
        {
            try
            {
                if (ResetSelectionHandler != null)
                {
                    ResetSelectionHandler.Invoke(this, EventArgs.Empty);
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
        public void SetRotateDegree(int degree)
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

        public void InitialServices()
        {
            //Mvx.ConstructAndRegisterSingleton<IPlatformService, PlatformService>();
            Mvx.LazyConstructAndRegisterSingleton<IInputService, InputService>();
            Mvx.LazyConstructAndRegisterSingleton<IAudioService, AudioService>();
            Mvx.LazyConstructAndRegisterSingleton<IVideoService, VideoService>();

            //Mvx.ConstructAndRegisterSingleton<IGameSystemsProviderService, GameSystemsProviderService>();
        }

        public static IEnumerable<IFileInfo> AnyCores = null;
        public static async void GetAnyCores()
        {
            try
            {
                var AnyCoreFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync("AnyCore");
                if (AnyCoreFolder != null)
                {
                    AnyCores = await AnyCoreFolder.EnumerateFilesAsync();
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

        public async Task<IDirectoryInfo> PickDirectory(string systemName, bool reSelect = false)
        {
            try
            {
                if (reSelect)
                {
                    FolderPicker selectWpFolder = new FolderPicker();
                    var folderPicker = new FolderPicker();
                    folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                    folderPicker.FileTypeFilter.Add("*");

                    StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
                    if (DownloadsFolderTest != null)
                    {
                        RememberFile(DownloadsFolderTest, systemName);
                        return new Plugin.FileSystem.DirectoryInfo(DownloadsFolderTest);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var tryGetDirectory = await GetFileForToken(systemName);
                    if (tryGetDirectory == null)
                    {
                        FolderPicker selectWpFolder = new FolderPicker();
                        var folderPicker = new FolderPicker();
                        folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                        folderPicker.FileTypeFilter.Add("*");

                        StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
                        if (DownloadsFolderTest != null)
                        {
                            RememberFile(DownloadsFolderTest, systemName);
                            return new Plugin.FileSystem.DirectoryInfo(DownloadsFolderTest);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return new Plugin.FileSystem.DirectoryInfo(tryGetDirectory);
                    }
                }
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
                return null;
            }
        }

        public string RememberFile(StorageFolder file, string SystemName)
        {
            string token = Guid.NewGuid().ToString();
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, file);
            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue($"{SystemName}_GamesFolder", token);
            return token;
        }
        public async Task<StorageFolder> GetFileForToken(string systemName)
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
            }catch(Exception e)
            {

            }
            return null;
        }
        public bool CheckDirectoryToken(string systemName)
        {
            try { 
            var fileToken = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"{systemName}_GamesFolder", "");
            if (fileToken.Length > 0)
            {
                if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(fileToken))
                {
                    return false;
                }
                return true;
            }
            }catch(Exception e)
            {

            }
            return false;
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


}
