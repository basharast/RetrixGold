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
    public class GamePlayerViewModel : MvxViewModel<GameLaunchEnvironment>
    {
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
                    RCore32= false;
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
        private void updateGCCaller()
        {
            ReduceFreezesInProgress = true;

            try
            {
                if (!NoGCRegionState)
                {
                    GC.WaitForPendingFinalizers();
                    AudioService.TryStartNoGCRegionCall();
                    NoGCRegionState = true;
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
                    if (ShowLogsList && !LogInProgress && !LogErrorCatched)
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
        void updateLogListCaller()
        {
            LogInProgress = true;
            try
            {
                if (ShowLogsList && !GameStopInProgress && !LogErrorCatched)
                {
                    lock (LogsList)
                    {
                        var LogsListContent = EmulationService.GetCoreLogsList()?.ToList();
                        if (LogsListContent != null && LogsListContent.Count > LogListSizeTemp)
                        {
                            LogsList.Clear();
                            foreach (var LogsListContentItem in LogsListContent)
                            {
                                LogsList.Add(LogsListContentItem);
                            }
                            RaisePropertyChanged(nameof(LogsList));
                            Interlocked.Exchange(ref LogListSizeTemp, LogsList.Count);
                        }
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
        public IMvxCommand SetSkipFrames { get; }
        public IMvxCommand SetSkipFramesRandom { get; }
        public IMvxCommand DontWaitThreads { get; }
        public IMvxCommand SetDelayFrames { get; }
        public IMvxCommand SetReduceFreezes { get; }
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
                SkipFrames = Settings.GetValueOrDefault(nameof(SkipFrames), false);
                SkipFramesRandom = Settings.GetValueOrDefault(nameof(SkipFramesRandom), false);
                DelayFrames = Settings.GetValueOrDefault(nameof(DelayFrames), false);
                ReduceFreezes = Settings.GetValueOrDefault(nameof(ReduceFreezes), true);
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
                UseAnalogDirections = Settings.GetValueOrDefault(nameof(UseAnalogDirections), true);
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
                SetSkipFrames = new MvxCommand(() => ToggleSkipFrames(false));
                SetSkipFramesRandom = new MvxCommand(() => ToggleSkipFramesRandom(false));
                DontWaitThreads = new MvxCommand(() => DontWaitThreadsCall());
                SetDelayFrames = new MvxCommand(() => ToggleDelayFrames(false));
                SetReduceFreezes = new MvxCommand(() => ToggleReduceFreezes(false));
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
            }catch(Exception ex)
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
                    GCTimer = new Timer(delegate { UpdateGC(null, EventArgs.Empty); }, null, 0, 500);
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
                            await UserDialogs.Instance.AlertAsync("All Saves cleaned (deleted)", "Clean done");
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild.wav");
                            await UserDialogs.Instance.AlertAsync("No saved slots found!", "Clean all saves");
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("faild.wav");
                        await UserDialogs.Instance.AlertAsync("No saved slots found!", "Clean all saves");
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
                    await UserDialogs.Instance.AlertAsync("Touch controls reseted to default", "Reset done");
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
                    await UserDialogs.Instance.AlertAsync(Resources.Strings.ExportSlotsMessageError, Resources.Strings.ExportSlotsTitle);
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
                    await UserDialogs.Instance.AlertAsync(Resources.Strings.ExportSlotsMessage, Resources.Strings.ExportSlotsTitle);
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
                        await UserDialogs.Instance.AlertAsync(Resources.Strings.ImportSlotsMessage, Resources.Strings.ImportSlotsTitle);
                        await ActionsRetrieveAsync();
                        UpdateInfoState("Import Done");
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("faild.wav");
                        await UserDialogs.Instance.AlertAsync(Resources.Strings.ImportSlotsMessageCancel, Resources.Strings.ImportSlotsTitle);
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
                UserDialogs.Instance.AlertAsync($"This option has small effect on the performance\nyou can check {"Core Options"} for native frame skipping", "Skip Frames (Frontend)");
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
                UserDialogs.Instance.AlertAsync($"This option has small effect on the performance\nyou can check {"Core Options"} for native frame skipping", "Skip Frames (Frontend)");
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
                await UserDialogs.Instance.AlertAsync($"This option is very important for the performance\nWe prefere to keep it on", "Reduce Freezes");
            }
            AudioService.SetGCPrevent(ReduceFreezes);
            callGCTimer(ReduceFreezes);
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
                FramebufferConverter.SetRGB0555LookupTable();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
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
                        FramebufferConverter.SetRGB0555LookupTable();
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
                if (!PlatformService.GameNoticeShowed)
                {
                    Dispatcher.RequestMainThreadAction(() => ShowGameTipInfo());
                }
                if (EmulationService.isGameLoaded())
                {
                    if (!addOpenCountInProgress)
                    {
                        addOpenCountInProgress = true;
                        await PlatformService.AddGameToRecents(SystemName, MainFilePath, RootNeeded, EmulationService.GetGameID(), 0, false);
                        UpdateInfoState("Game Started");
                        FramebufferConverter.isGameStarted = true;
                        ShowSystemInfo();
                        RaisePropertyChanged("Manufacturer");
                        if (StartTimeStamp == 0)
                        {
                            StartTimeStamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; ;
                        }
                        RaisePropertyChanged(nameof(IsSegaSystem));
                    }
                }
                else if (!errorDialogAppeard)
                {
                    errorDialogAppeard = true;
                    FailedToLoadGame = true;
                    UpdateInfoState("Game Failed");
                    PlatformService?.PlayNotificationSound("faild.wav");
                    UserDialogs.Instance.AlertAsync("Failed to load the game, for more details check\n\u26EF -> Debug -> Log List", "Load Failed");
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
                bool ShowNoticeState = Settings.GetValueOrDefault("NeverShowSlow", true);
                if (ShowNoticeState)
                {
                    await Task.Delay(2200);
                    PlatformService.PlayNotificationSound("notice.mp3");
                    ConfirmConfig confirmLoadNotice = new ConfirmConfig();
                    confirmLoadNotice.SetTitle("Slow Game?");
                    confirmLoadNotice.SetMessage("If the game went slow try:\n\n1- Pause \u25EB then Resume \u25B7.\n2- Enable \u26EF -> Extras -> Delay Frames\n\nEnjoy " + char.ConvertFromUtf32(0x1F609));
                    confirmLoadNotice.UseYesNo();
                    confirmLoadNotice.SetOkText("Never Show");
                    confirmLoadNotice.SetCancelText("Dismiss");
                    PlatformService.GameNoticeShowed = true;
                    var NeverShow = await UserDialogs.Instance.ConfirmAsync(confirmLoadNotice);
                    if (NeverShow)
                    {
                        PlatformService.PlayNotificationSound("button-01.mp3");
                        Settings.AddOrUpdateValue("NeverShowSlow", false);
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
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            LoadSaveListInProgress = false;
            RaisePropertyChanged(nameof(LoadSaveListInProgress));
            try
            {
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
                await UserDialogs.Instance.AlertAsync($"Touch pad settings saved for {SystemNamePreview}");
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
                    await UserDialogs.Instance.AlertAsync($"Customization for {SystemNamePreview} not found!");
                    return;
                }

                var targetFileTest = await localFolder.GetFileAsync($"{SystemName}.rct");
                if (targetFileTest != null)
                {
                    GameIsLoadingState(true);
                    await targetFileTest.DeleteAsync();
                    PlatformService.PlayNotificationSound("success.wav");
                    await UserDialogs.Instance.AlertAsync($"Customization for {SystemNamePreview} deleted\nGlobal customization will be used");
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
                    await UserDialogs.Instance.AlertAsync($"Customization for {SystemNamePreview} not found!");
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

        GroupMenuGrid ControlsMenu, SavesMenu, AdvancedMenu, ScreenMenu, AudioMenu, OverlaysMenu, RenderMenu, ColorModeMenu, DebugMenu;
        public ObservableCollection<GroupMenuGrid> MenusGrid = new ObservableCollection<GroupMenuGrid>();
        public void PrepareXBoxMenu()
        {
            MenusGrid.Clear();
            //Control
            ControlsMenu = new GroupMenuGrid();
            ControlsMenu.Key = "Controls";
            ControlsMenu.Add(AddNewMenu(EmulationService.CorePaused ? "Resume" : "Pause", EmulationService.CorePaused ? "1414 - Play.png" : "1420 - Pause.png", "pause"));
            ControlsMenu.Add(AddNewMenu("Stop", "1422 - Stop.png", "stop"));
            ControlsMenu.Add(AddNewMenu("Gamepad", "3642 - Direction Keys.png", "controls"));
            ControlsMenu.Add(AddNewMenu("Quick Save", "3656 - Target.png", "quicksave"));
            if (SensorsMovementActive)
            {
                ControlsMenu.Add(AddNewMenu("Sensors", "6594 - Smartphone Shake.png", "sensors", true, SensorsMovement));
            }
            ControlsMenu.Add(AddNewMenu("Close Menu", "3672 - Diamond.png", "close"));
            MenusGrid.Add(ControlsMenu);

            //Saves
            SavesMenu = new GroupMenuGrid();
            SavesMenu.Key = "Save";
            SavesMenu.Add(AddNewMenu("Saves List", "7437 - Mobile Applications.png", "saves"));
            SavesMenu.Add(AddNewMenu("Auto (30 Sec)", "3671 - Deck of Cards.png", "asave30", true, AutoSave30Sec));
            SavesMenu.Add(AddNewMenu("Auto (1 Min)", "3668 - Bowling Pin.png", "asave1", true, AutoSave60Sec));
            SavesMenu.Add(AddNewMenu("Auto(1.5 Min)", "3667 - Bowling.png", "asave15", true, AutoSave90Sec));
            SavesMenu.Add(AddNewMenu("Auto Notify", "6585 - Notification.png", "asaven", true, AutoSaveNotify));
            MenusGrid.Add(SavesMenu);

            //On Screen
            AdvancedMenu = new GroupMenuGrid();
            AdvancedMenu.Key = "Advanced";
            AdvancedMenu.Add(AddNewMenu("FPS Counter", "6599 - Stopwatch.png", "fps", true, ShowFPSCounter));
            if (InGameOptionsActive)
            {
                AdvancedMenu.Add(AddNewMenu("Core Options", "6591 - Settings I.png", "coreoptions"));
            }
            MenusGrid.Add(AdvancedMenu);

            //Rotate
            ScreenMenu = new GroupMenuGrid();
            ScreenMenu.Key = "Screen";
            ScreenMenu.Add(AddNewMenu("Rotate Right", "6600 - Switch On.png", "rright", true, RotateDegreePlusActive));
            ScreenMenu.Add(AddNewMenu("Rotate Left", "6601 - Switch Off.png", "rleft", true, RotateDegreeMinusActive));
            MenusGrid.Add(ScreenMenu);

            //Audio
            AudioMenu = new GroupMenuGrid();
            AudioMenu.Key = "Audio";
            AudioMenu.Add(AddNewMenu("Volume Mute", "1424 - Mute.png", "vmute", true, AudioMuteLevel));
            AudioMenu.Add(AddNewMenu("Volume High", "1403 - Speaker.png", "vhigh", true, AudioHighLevel));
            AudioMenu.Add(AddNewMenu("Volume Default", "1425 - Volume.png", "vdefault", true, AudioNormalLevel));
            AudioMenu.Add(AddNewMenu("Volume Low", "6611 - Volume Control.png", "vlow", true, AudioLowLevel));
            AudioMenu.Add(AddNewMenu("Echo Effect", "1405 - Music.png", "aecho", true, AudioEcho));
            MenusGrid.Add(AudioMenu);

            //Overlays
            OverlaysMenu = new GroupMenuGrid();
            OverlaysMenu.Key = "Overlays";
            OverlaysMenu.Add(AddNewMenu("Overlay Lines", "3654 - Bricks.png", "ovlines", true, ScanLines3));
            OverlaysMenu.Add(AddNewMenu("Overlay Grid", "3663 - Dice II.png", "ovgrid", true, ScanLines2));
            MenusGrid.Add(OverlaysMenu);

            //Render
            RenderMenu = new GroupMenuGrid();
            RenderMenu.Key = "Render";
            RenderMenu.Add(AddNewMenu("Nearest", "6567 - Low Battery.png", "rnearest", true, NearestNeighbor));
            RenderMenu.Add(AddNewMenu("Linear", "6566 - Half Battery.png", "rlinear", true, Linear));
            RenderMenu.Add(AddNewMenu("MultiSample", "6565 - Full Battery.png", "rmultisample", true, MultiSampleLinear));
            MenusGrid.Add(RenderMenu);

            //Color
            ColorModeMenu = new GroupMenuGrid();
            ColorModeMenu.Key = "Color Mode";
            ColorModeMenu.Add(AddNewMenu("None", "6570 - Controls.png", "creset", true, ColorFilters[0]));
            ColorModeMenu.Add(AddNewMenu("B/W", "3640 - Brick Game.png", "cbw", true, ColorFilters[1]));
            ColorModeMenu.Add(AddNewMenu("Sepia", "3646 - Game Character II.png", "csepia", true, ColorFilters[4]));
            ColorModeMenu.Add(AddNewMenu("Retro", "3638 - Gaming Control I.png", "cretro", true, ColorFilters[5]));
            MenusGrid.Add(ColorModeMenu);

            //Debug
            DebugMenu = new GroupMenuGrid();
            DebugMenu.Key = "Debug";
            DebugMenu.Add(AddNewMenu("Log List", "7443 - Quality Assurance.png", "loglist"));
            DebugMenu.Add(AddNewMenu("Close Menu", "3672 - Diamond.png", "close"));
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
                        SetColorFilterNone.Execute();
                        break;
                    case "cbw":
                        SetColorFilterGrayscale.Execute();
                        break;
                    case "csepia":
                        SetColorFilterBurn.Execute();
                        break;
                    case "cretro":
                        SetColorFilterRetro.Execute();
                        break;

                    case "loglist":
                        SetShowLogsList.Execute();
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
