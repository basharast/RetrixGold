using Acr.UserDialogs;
using LibRetriX;
using MvvmCross.Platform.Core;
using Plugin.FileSystem.Abstractions;
//using Plugin.LocalNotifications.Abstractions;
using RetriX.Shared.StreamProviders;
using RetriX.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{

    public class EmulationService : IEmulationService
    {
        private static readonly IReadOnlyDictionary<InjectedInputTypes, InputTypes> InjectedInputMapping = new Dictionary<InjectedInputTypes, InputTypes>
        {
            { InjectedInputTypes.DeviceIdJoypadA, InputTypes.DeviceIdJoypadA },
            { InjectedInputTypes.DeviceIdJoypadB, InputTypes.DeviceIdJoypadB },
            { InjectedInputTypes.DeviceIdJoypadDown, InputTypes.DeviceIdJoypadDown },
            { InjectedInputTypes.DeviceIdJoypadLeft, InputTypes.DeviceIdJoypadLeft },
            { InjectedInputTypes.DeviceIdJoypadRight, InputTypes.DeviceIdJoypadRight },
            { InjectedInputTypes.DeviceIdJoypadSelect, InputTypes.DeviceIdJoypadSelect },
            { InjectedInputTypes.DeviceIdJoypadStart, InputTypes.DeviceIdJoypadStart },
            { InjectedInputTypes.DeviceIdJoypadUp, InputTypes.DeviceIdJoypadUp },
            { InjectedInputTypes.DeviceIdJoypadX, InputTypes.DeviceIdJoypadX },
            { InjectedInputTypes.DeviceIdJoypadY, InputTypes.DeviceIdJoypadY },
            { InjectedInputTypes.DeviceIdJoypadC, InputTypes.DeviceIdJoypadL },
            { InjectedInputTypes.DeviceIdJoypadZ, InputTypes.DeviceIdJoypadR },
            { InjectedInputTypes.DeviceIdJoypadL, InputTypes.DeviceIdJoypadL2 },
            { InjectedInputTypes.DeviceIdJoypadR, InputTypes.DeviceIdJoypadR2 },
            { InjectedInputTypes.DeviceIdJoypadL2, InputTypes.DeviceIdJoypadL2 },
            { InjectedInputTypes.DeviceIdJoypadR2, InputTypes.DeviceIdJoypadR2 },
            { InjectedInputTypes.DeviceIdPointerPressed, InputTypes.DeviceIdPointerPressed },
            { InjectedInputTypes.DeviceIdPointerX, InputTypes.DeviceIdPointerX },
            { InjectedInputTypes.DeviceIdPointerY, InputTypes.DeviceIdPointerY },
            { InjectedInputTypes.DeviceIdMouseRight, InputTypes.DeviceIdMouseRight },
            { InjectedInputTypes.DeviceIdMouseLeft, InputTypes.DeviceIdMouseLeft },
            { InjectedInputTypes.DeviceIdMouseX, InputTypes.DeviceIdMouseX },
            { InjectedInputTypes.DeviceIdMouseY, InputTypes.DeviceIdMouseY },
        };

        private IPlatformService PlatformService { get; }
        private ISaveStateService SaveStateService { get; }
        //private ILocalNotifications NotificationService { get; }
        private IMvxMainThreadDispatcher Dispatcher { get; }

        private IVideoService VideoService { get; }
        private IAudioService AudioService { get; }
        private IInputService InputService { get; }

        public bool CorePaused { get; set; } = false;
        private bool StartStopOperationInProgress { get; set; } = false;

        private Func<bool> RequestedFrameAction { get; set; }
        private TaskCompletionSource<bool> RequestedRunFrameThreadActionTCS { get; set; }

        public ICore currentCore;

        private ICore CurrentCore
        {
            get => currentCore;
            set
            {
                if (currentCore == value)
                {
                    return;
                }

                if (currentCore != null)
                {
                    currentCore.GeometryChanged -= VideoService.GeometryChanged;
                    currentCore.PixelFormatChanged -= VideoService.PixelFormatChanged;
                    currentCore.RenderVideoFrame -= VideoService.RenderVideoFrame;
                    currentCore.TimingsChanged -= VideoService.TimingsChanged;
                    currentCore.RotationChanged -= VideoService.RotationChanged;
                    currentCore.TimingsChanged -= AudioService.TimingChanged;
                    currentCore.RenderAudioFrames -= AudioService.RenderAudioFrames;
                    currentCore.PollInput = null;
                    currentCore.GetInputState = null;
                    currentCore.OpenFileStream = null;
                    currentCore.CloseFileStream = null;
                }

                currentCore = value;

                if (currentCore != null)
                {
                    currentCore.GeometryChanged += VideoService.GeometryChanged;
                    currentCore.PixelFormatChanged += VideoService.PixelFormatChanged;
                    currentCore.RenderVideoFrame += VideoService.RenderVideoFrame;
                    currentCore.TimingsChanged += VideoService.TimingsChanged;
                    currentCore.RotationChanged += VideoService.RotationChanged;
                    currentCore.TimingsChanged += AudioService.TimingChanged;
                    currentCore.RenderAudioFrames += AudioService.RenderAudioFrames;
                    currentCore.PollInput = InputService.PollInput;
                    currentCore.GetInputState = InputService.GetInputState;
                    currentCore.OpenFileStream = OnCoreOpenFileStream;
                    currentCore.CloseFileStream = OnCoreCloseFileStream;
                }
            }
        }

        private IStreamProvider streamProvider;
        private IStreamProvider StreamProvider
        {
            get => streamProvider;
            set { if (streamProvider != value) streamProvider?.Dispose(); streamProvider = value; }
        }

        public event EventHandler GameStarted;
        public event EventHandler GameStopped;
        public event EventHandler GameLoaded;
        public event EventHandler<Exception> GameRuntimeExceptionOccurred;
        public string MainFilePath = "";
       

        public EmulationService(IFileSystem fileSystem, IUserDialogs dialogsService,
            IPlatformService platformService, ISaveStateService saveStateService,
            //ILocalNotifications notificationService, ICryptographyService cryptographyService,
            IVideoService videoService, IAudioService audioService,
            IInputService inputService, IMvxMainThreadDispatcher dispatcher)
        {
            PlatformService = platformService;
            SaveStateService = saveStateService;
            //NotificationService = notificationService;
            Dispatcher = dispatcher;

            VideoService = videoService;
            AudioService = audioService;
            InputService = inputService;

            VideoService.RequestRunCoreFrame += OnRunFrameRequested;
        }

        public bool isGameLoaded()
        {
            if (CurrentCore != null) { 
            return !CurrentCore.FailedToLoadGame;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> StartGameAsync(ICore core, IStreamProvider streamProvider, string mainFilePath)
        {
            try
            {
                if (StartStopOperationInProgress)
                {
                    return false;
                }

                StartStopOperationInProgress = true;

                var initTasks = new Task[] { InputService.InitAsync(), AudioService.InitAsync(), VideoService.InitAsync() };
                await Task.WhenAll(initTasks);

                await RequestFrameActionAsync(() =>
                {
                    CurrentCore?.UnloadGame();
                });

                MainFilePath = mainFilePath;
                StreamProvider = streamProvider;
                SaveStateService.SetGameId(mainFilePath);
                CorePaused = false;
                await Task.Delay(1000);
                var TargetSystem = GameSystemSelectionViewModel.SystemsOptions[core.SystemName];
                foreach (var optionItem in TargetSystem.OptionsList.Keys)
                {
                    var optionObject = TargetSystem.OptionsList[optionItem];
                    CoreOption coreOption;
                    if(core.Options.TryGetValue(optionObject.OptionsKey, out coreOption)) {
                    core.Options[optionObject.OptionsKey].SelectedValueIx = optionObject.SelectedIndex;
                    }
                }
                var loadSuccessful = await RequestFrameActionAsync(() =>
                {
                    try
                    {
                        CurrentCore = core;
                        return CurrentCore.LoadGame(mainFilePath);
                    }
                    catch
                    {
                        return false;
                    }
                });
                await Task.Delay(1000);
                await PauseGameAsync();
                await Task.Delay(100);
                await ResumeGameAsync();
                if (!loadSuccessful)
                {
                    await StopGameAsyncInternal();
                    StartStopOperationInProgress = false;
                    return loadSuccessful;
                }

                GameStarted?.Invoke(this, EventArgs.Empty);
                GameLoaded.Invoke(this, EventArgs.Empty);
                StartStopOperationInProgress = false;
                return loadSuccessful;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return false;
            }
        }

        public void UpdateCoreOption(string optionName, uint optionValue)
        {
            if (CurrentCore != null) { 
            CurrentCore.Options[optionName].SelectedValueIx = optionValue;
            //CurrentCore.UpdateCoreOptions(optionName);
            CurrentCore.UpdateOptionsInGame();
            }
        }
        public uint GetCoreOptionValue(string optionName)
        {
            if (CurrentCore != null)
            {
                return CurrentCore.Options[optionName].SelectedValueIx;
            }
            else
            {
                return 0;
            }
        }

        public ObservableCollection<string> GetCoreLogsList()
        {
            if (CurrentCore != null) { 
            return CurrentCore.GetLogsList();
            }
            else
            {
            return null;
            }
        }

        public IDictionary<string, CoreOption> GetCoreOptions()
        {
            if (CurrentCore != null)
            {
                return CurrentCore.Options;
            }
            else
            {
                return null;
            }
        }
        public Task ResetGameAsync()
        {
            try
            {
                return RequestFrameActionAsync(() =>
                {
                    CurrentCore?.Reset();
                });
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }

        public async Task StopGameAsync()
        {
            try
            {
                if (StartStopOperationInProgress)
                {
                    return;
                }

                StartStopOperationInProgress = true;
                await StopGameAsyncInternal();
                GameStopped?.Invoke(this, EventArgs.Empty);
                StartStopOperationInProgress = false;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return;
            }
        }

        private async Task StopGameAsyncInternal()
        {
            try
            {
                await RequestFrameActionAsync(() =>
                {
                    CurrentCore?.UnloadGame();
                    CurrentCore.FreeLibretroCore();
                    CurrentCore = null;
                });

                SaveStateService.SetGameId(null);
                StreamProvider = null;

                var initTasks = new Task[] { InputService.DeinitAsync(), AudioService.DeinitAsync(), VideoService.DeinitAsync() };
                await Task.WhenAll(initTasks);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public Task PauseGameAsync()
        {
            try
            {
                return SetCorePaused(true);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }

        public Task ResumeGameAsync()
        {
            try
            {
                return SetCorePaused(false);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }

        private async Task SetCorePaused(bool value)
        {
            try
            {
                if (value)
                {
                    await Task.Run(() => AudioService.Stop());
                }

                CorePaused = value;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public string GetGameID()
        {
            try
            {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(MainFilePath);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }
        public string GetGameName()
        {
            return MainFilePath;
        }
            public async Task<bool> SaveGameStateAsync(uint slotID, bool showMessage=true)
        {
            try
            {
                var success = false;
                using (var stream = await SaveStateService.GetStreamForSlotAsync(slotID, FileAccess.ReadWrite))
                {
                    if (stream == null)
                    {
                        return success;
                    }

                    success = await RequestFrameActionAsync(() =>
                    {
                        if (CurrentCore == null)
                        {
                            return false;
                        }

                        return CurrentCore.SaveState(stream);
                    });

                    await stream.FlushAsync();
                }

                if (success)
                {
                    var notificationBody = string.Format(Resources.Strings.StateSavedToSlotMessageBody, slotID);
                    if (slotID == 10)
                    {
                        notificationBody = Resources.Strings.StateSavedToAutoMessageBody;
                    }
                    if (showMessage) { 
                    PlatformService.PlayNotificationSound("save-state.wav");
                    }
                }

                return success;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return false;
            }
        }

        
        public async Task<bool> LoadGameStateAsync(uint slotID)
        {
            try
            {
                var success = false;
                using (var stream = await SaveStateService.GetStreamForSlotAsync(slotID, FileAccess.Read))
                {
                    if (stream == null)
                    {
                        return false;
                    }

                    success = await RequestFrameActionAsync(() =>
                    {
                        if (CurrentCore == null)
                        {
                            return false;
                        }

                        return CurrentCore.LoadState(stream);
                    });
                }

                return success;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return false;
            }
        }

        public void SetFPSCounterState(bool FPSCounterState)
        {
            try { 
            if (CurrentCore != null) { 
            CurrentCore.ShowFPSCounter = FPSCounterState;
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

        public int GetFPSCounterValue()
        {
            try
            {
                if (CurrentCore != null)
                {
                    return CurrentCore.FrameRate;
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            return 0;
        }
        public void SetVideoOnlyState(bool VideoOnlyState)
        {
            try
            {
                if (CurrentCore != null)
                {
                    CurrentCore.VideoOnly = VideoOnlyState;
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

        public void SetAudioOnlyState(bool AudioOnlyState)
        {
            try
            {
                if (CurrentCore != null)
                {
                    CurrentCore.AudioOnly = AudioOnlyState;
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

        public void SetSkipFramesState(bool SkipFramesState)
        {
            try
            {
                if (CurrentCore != null)
                {
                    CurrentCore.NativeSkipFrames = SkipFramesState;
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
        public void SetSkipFramesRandomState(bool SkipFramesState)
        {
            try
            {
                if (CurrentCore != null)
                {
                    CurrentCore.NativeSkipFramesRandom = SkipFramesState;
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
        public void InjectInputPlayer1(InjectedInputTypes inputType)
        {
            try
            {
                InputService.InjectInputPlayer1(InjectedInputMapping[inputType]);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        private Task<bool> RequestFrameActionAsync(Action action)
        {
            try
            {
                return RequestFrameActionAsync(() =>
                {
                    action.Invoke();
                    return true;
                });
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }

        private Task<bool> RequestFrameActionAsync(Func<bool> action)
        {
            try
            {
                if (RequestedFrameAction != null)
                {
                    return Task.FromResult(false);
                }

                RequestedFrameAction = action;
                RequestedRunFrameThreadActionTCS = new TaskCompletionSource<bool>();
                return RequestedRunFrameThreadActionTCS.Task;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }

        //Synhronous since it's going to be called by a non UI thread
        private void OnRunFrameRequested(object sender, EventArgs args)
        {
            try
            {
                if (RequestedFrameAction != null)
                {
                    RequestedRunFrameThreadActionTCS.SetResult(RequestedFrameAction.Invoke());
                    RequestedFrameAction = null;
                    RequestedRunFrameThreadActionTCS = null;
                    return;
                }

                if (CorePaused || AudioService.ShouldDelayNextFrame)
                {
                    return;
                }

                try
                {
                    Task.Delay(1000);
                    CurrentCore?.RunFrame();
                }
                catch (Exception e)
                {
                    if (!StartStopOperationInProgress)
                    {
                        StartStopOperationInProgress = true;
                        StopGameAsyncInternal().Wait();
                        StartStopOperationInProgress = false;
                    }

                    var task = Dispatcher.RequestMainThreadAction(() => GameRuntimeExceptionOccurred?.Invoke(this, e));
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

        private Stream OnCoreOpenFileStream(string path, FileAccess fileAccess)
        {
            try
            {
                var stream = Task.Run(() => StreamProvider.OpenFileStreamAsync(path, fileAccess)).Result;
                return stream;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }

        }

        private void OnCoreCloseFileStream(Stream stream)
        {
            try
            {
                StreamProvider.CloseStream(stream);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public int GetSamplesBufferCount()
        {
            return AudioService.GetSamplesBufferCount();
        }
    }
}
