using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using RetriX.UWP.Services;
using Plugin.FileSystem.Abstractions;
using Acr.UserDialogs;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;
using LibRetriX;
using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Plugin.FileSystem;
using System.Text;
using System.IO;
using Newtonsoft.Json;



// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RetriX.UWP.Controls
{
    public sealed partial class VirtualPadActions : UserControl
    {
        public static Dictionary<string, Dictionary<string, InjectedInputTypes>> ButtonsDictionary = new Dictionary<string, Dictionary<string, InjectedInputTypes>>();
        public VirtualPadActions()
        {
            this.InitializeComponent();
            PrepareButtonsDictionary();
        }

        public Dictionary<string, InjectedInputTypes> GetButtonCurrentKey(string ButtonKey)
        {
            Dictionary<string, InjectedInputTypes> CurrentKey = ButtonsDictionary[ButtonKey];
            return CurrentKey;
        }
        public bool ShowXYZState = true;
        public bool ShowXYZ
        {
            get
            {
                return ShowXYZState;
            }
            set
            {
                ShowXYZState = value;
            }
        }
        public bool IsSegaSystemState = false;
        public bool IsSegaSystem {
            get
            {
                return IsSegaSystemState;
            }
            set
            {
                IsSegaSystemState = value;
            }
        }


        InjectedInputTypes[] LRButtons = new InjectedInputTypes[] {  };
        object[] LRButtonsObjects = new object[] { };
        public void PrepareButtonsDictionary()
        {
            ButtonsDictionary.Clear();
            InputService.CurrentButtons.Clear();
            AddNewInputTypeToDictionary(AButtonInputType, "GPAButton","A");
            AddNewInputTypeToDictionary(BButtonInputType, "GPBButton","B");
            AddNewInputTypeToDictionary(CButtonInputType, "GPCButton","C");
            AddNewInputTypeToDictionary(XButtonInputType, "GPXButton","X");
            AddNewInputTypeToDictionary(YButtonInputType, "GPYButton","Y");
            AddNewInputTypeToDictionary(ZButtonInputType, "GPZButton","Z");
            AddNewInputTypeToDictionary(L2ButtonInputType, "GPL2Button","L2");
            AddNewInputTypeToDictionary(R2ButtonInputType, "GPR2Button","R2");
            AddNewInputTypeToDictionary(StartButtonInputType, "GPStartButton","Start");
            AddNewInputTypeToDictionary(SelectButtonInputType, "GPSelectButton","Select");
            LRButtons = new InjectedInputTypes[] { LButtonInputType, RButtonInputType };
            LRButtonsObjects = new object[] { GPAButton, GPXButton };
        }

       

        
        private async Task GetAndSaveButtonsPositions(string SystemName)
        {
            InputService.ButtonsCompositeTransform.Clear();
            InputService.ButtonsCompositeTransform.Add("GPAButton", GPAButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPBButton", GPBButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPCButton", GPCButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPXButton", GPXButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPYButton", GPYButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPZButton", GPZButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPL2Button", GPL2ButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPR2Button", GPR2ButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPStartButton", GPStartButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPSelectButton", GPSelectButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPL1R1Button", GPL1R1ButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPL2R2Button", GPL2R2ButtonTransform);
            InputService.ButtonsCompositeTransform.Add("GPToggleBar", GPToggleBarTransform);
            InputService.ButtonsCompositeTransform.Add("GPQuickSave", GPQuickSaveTransform);
            InputService.ButtonsCompositeTransform.Add("GPQuickLoad", GPQuickLoadTransform);
            InputService.ButtonsCompositeTransform.Add("GPAudioState", GPAudioStateTransform);

            foreach (var ButtonsNameItem in InputService.ButtonsCompositeTransform.Keys)
            {
                var targetButtonCompositeTransform = InputService.ButtonsCompositeTransform[ButtonsNameItem];

                double ButtonX = targetButtonCompositeTransform.TranslateX;
                double ButtonY = targetButtonCompositeTransform.TranslateY;

                InputService.SaveButtonPosition(ButtonsNameItem, ButtonX, ButtonY);
            }
            await RetriveButtonsPositionsData(SystemName);
        }

        string GamepadSaveLocation = "GamepadMap";
        private async Task RetriveButtonsPositionsData(string SystemName)
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(GamepadSaveLocation);
                if (localFolder == null)
                {
                    ViewModel.SetButtonsIsLoadingState(false);
                    return;
                }

                var targetFileTest = await localFolder.GetFileAsync($"{SystemName}.rbm");
                if (targetFileTest != null)
                {
                    ViewModel.SetButtonsIsLoadingState(true);
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
                    var dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, double[]>>(CoreFileContent);

                    if (dictionaryList != null)
                    {
                        InputService.ButtonsPositions = dictionaryList;
                        InputService.RetriveButtonsPositions();
                    }

                }

            }
            catch (Exception e)
            {

            }
            ViewModel.SetButtonsIsLoadingState(false);
        }
        public void UpdateButtonInDictionary(string ButtonKey,InjectedInputTypes ButtonNewType,string ButtonTitle)
        {
            Dictionary<string, InjectedInputTypes> AddNewType = new Dictionary<string, InjectedInputTypes>();
            AddNewType.Add(ButtonTitle, ButtonNewType);
            ButtonsDictionary[ButtonKey] = AddNewType.ToDictionary(entry => entry.Key, entry => entry.Value);
        }
        public void AddNewInputTypeToDictionary(InjectedInputTypes TargetType,string Buttonkey, string TypeTitle)
        {
            Dictionary<string, InjectedInputTypes> AddNewType = new Dictionary<string, InjectedInputTypes>();
            AddNewType.Add(TypeTitle, TargetType);
            ButtonsDictionary.Add(Buttonkey, AddNewType.ToDictionary(entry => entry.Key, entry => entry.Value));
        }
        public GamePlayerViewModel ViewModel
        {
            get { return (GamePlayerViewModel)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }

        public static DependencyProperty VMProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GamePlayerViewModel), typeof(PlayerOverlay), new PropertyMetadata(null));
        
        public InjectedInputTypes XButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(XButtonInputTypeProperty); }
            set { SetValue(XButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for XButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty XButtonInputTypeProperty = DependencyProperty.Register(nameof(XButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadX));

        public InjectedInputTypes BButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(BButtonInputTypeProperty); }
            set { SetValue(BButtonInputTypeProperty, value); }
        }


        // Using a DependencyProperty as the backing store for CButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty BButtonInputTypeProperty = DependencyProperty.Register(nameof(BButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadB));

        public InjectedInputTypes CButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(CButtonInputTypeProperty); }
            set { SetValue(CButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty CButtonInputTypeProperty = DependencyProperty.Register(nameof(CButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadC));

        public InjectedInputTypes ZButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(ZButtonInputTypeProperty); }
            set { SetValue(ZButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty ZButtonInputTypeProperty = DependencyProperty.Register(nameof(ZButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadZ));

        public InjectedInputTypes YButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(YButtonInputTypeProperty); }
            set { SetValue(YButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty YButtonInputTypeProperty = DependencyProperty.Register(nameof(YButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadY));

        public InjectedInputTypes LButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(LButtonInputTypeProperty); }
            set { SetValue(LButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty LButtonInputTypeProperty = DependencyProperty.Register(nameof(LButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadL));

        public InjectedInputTypes RButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(RButtonInputTypeProperty); }
            set { SetValue(RButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty RButtonInputTypeProperty = DependencyProperty.Register(nameof(RButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadR));

        public InjectedInputTypes R2ButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(R2ButtonInputTypeProperty); }
            set { SetValue(R2ButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty R2ButtonInputTypeProperty = DependencyProperty.Register(nameof(R2ButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadR2));

        public InjectedInputTypes L2ButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(L2ButtonInputTypeProperty); }
            set { SetValue(L2ButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty L2ButtonInputTypeProperty = DependencyProperty.Register(nameof(L2ButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadL2));


        public InjectedInputTypes AButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(AButtonInputTypeProperty); }
            set { SetValue(AButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty AButtonInputTypeProperty = DependencyProperty.Register(nameof(AButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadA));

        public InjectedInputTypes StartButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(StartButtonInputTypeProperty); }
            set { SetValue(StartButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty SelectButtonInputTypeProperty = DependencyProperty.Register(nameof(SelectButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadSelect));

        public InjectedInputTypes SelectButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(SelectButtonInputTypeProperty); }
            set { SetValue(SelectButtonInputTypeProperty, value); }
        }


        // Using a DependencyProperty as the backing store for StartButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty StartButtonInputTypeProperty = DependencyProperty.Register(nameof(StartButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadStart));



        private void Button01Clicked(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");
        }
        private void Button02Clicked(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-02.mp3");
        }
        private void Button03Clicked(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-03.mp3");
        }

        private void Button04Clicked(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-04.mp3");
        }
        private void Button04Clicked()
        {
            if (ViewModel.TabSoundEffect) { 
            PlatformService.PlayNotificationSoundDirect("button-04.mp3");
            }
        }
        public void AddNewActionButton(object sender)
        {
            try { 
            ResetReleaseState();
            string ButtonKey = ((RepeatButton)sender).Name;
            string ButtonTitle=ButtonsDictionary[ButtonKey].Keys.First();
            InjectedInputTypes ButtonAction = ButtonsDictionary[ButtonKey][ButtonTitle];
            ViewModel.AddActionButton(ButtonKey,ButtonTitle, ButtonAction, ViewModel.ActionsCustomDelay ? "+" : ",");
            Button04Clicked();
            ActiveButton(sender);
            }catch(Exception ee)
            {

            }
        }
        private void XButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(XButtonInputType);
        }
        private void XButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void BButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(BButtonInputType);
        }
        private void BButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void YButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(YButtonInputType);
        }
        private void YButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }

        private void AButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(AButtonInputType);
        }
        private void AButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }

        private void CButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(CButtonInputType);
        }
        private void CButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void ZButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(ZButtonInputType);
        }
        private void ZButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void LButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(LButtonInputType);
        }
        private void LButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void RButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(RButtonInputType);
        }
        private void RButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(StartButtonInputType);
        }
        private void StartButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void SelectButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(SelectButtonInputType);

        }
        private void SelectButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void ToggleBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");
            ViewModel.TogglePauseCommand.Execute();
            //ViewModel.TappedCommand.Execute();
        }
        private void ResetLoadState()
        {
            State9Loaded = false;
            State8Loaded = false;
            State7Loaded = false;
        }
        private void ResetReleaseState()
        {
            State9LoadedReleased = true;
            State8LoadedReleased = true;
            State7LoadedReleased = true;
        }
        private async void ResetLoadState(object sender, RoutedEventArgs e)
        {
            await Task.Delay(500);
            ResetReleaseState();
        }
        private async Task<bool> OverwriteSloteAsync(bool TargetSlotState)
        {
            if (!TargetSlotState)
            {
                return true;
            }
            PlatformService.PlayNotificationSoundDirect("alert.wav");
            ConfirmConfig confirmLoadAuto = new ConfirmConfig();
            confirmLoadAuto.SetTitle("Overwrite Slot");
            confirmLoadAuto.SetMessage("You already loaded this slot, do you want to overwrite the saved state?");
            confirmLoadAuto.UseYesNo();
            var overwriteState = await UserDialogs.Instance.ConfirmAsync(confirmLoadAuto);
            return overwriteState;
        }
        private async void Save9ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.QuickSaveState();
            /*if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            if (State9LoadedReleased) { 
            bool saveState = await OverwriteSloteAsync(State9Loaded);
            if (saveState) { 
            PlatformService.PlayNotificationSoundDirect("button-03.mp3");
            ViewModel.SaveStateSlot9.Execute();
            ResetLoadState();
            ResetReleaseState();
            }
            }
            else
            {
                ResetReleaseState();
            }*/
        }
        bool State9Loaded = false;
        bool State9LoadedReleased = true;
        private void Load9ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            if (!State9LoadedReleased)
            {
                return;
            }
            ResetLoadState();
            ResetReleaseState();
            State9Loaded = true;
            State9LoadedReleased = false;
            ViewModel.LoadStateSlot9.Execute();
        }
        private async void Save8ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.QuickLoadState();
            /*if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            if (State8LoadedReleased) { 
            bool saveState = await OverwriteSloteAsync(State8Loaded);
            if (saveState)
            {
                PlatformService.PlayNotificationSoundDirect("button-03.mp3");
                ViewModel.SaveStateSlot8.Execute();
                ResetLoadState();
                ResetReleaseState();
            }
            }
            else
            {
                ResetReleaseState();
            }*/
        }
        bool State8Loaded = false;
        bool State8LoadedReleased = true;
        private void Load8ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            /*if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            if (!State8LoadedReleased)
            {
                return;
            }
            ResetLoadState();
            ResetReleaseState();
            State8Loaded = true;
            State8LoadedReleased = false;
            ViewModel.LoadStateSlot8.Execute();*/
        }
        private async void Save7ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            bool MuteState = ViewModel.ToggleMuteAudioCall();
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");
            SetAudioIcon(MuteState);
            /*if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            if (State7LoadedReleased) { 
            bool saveState = await OverwriteSloteAsync(State7Loaded);
            if (saveState)
            {
                PlatformService.PlayNotificationSoundDirect("button-03.mp3");
                ViewModel.SaveStateSlot7.Execute();
                ResetLoadState();
                ResetReleaseState();
            }
            }
            else
            {
                ResetReleaseState();
            }*/
        }
        private void SetAudioIcon(bool MuteState)
        {
            MuteText.Inlines.Clear();
            if (MuteState)
            {
                MuteText.Inlines.Add(new Windows.UI.Xaml.Documents.Run { Text = char.ConvertFromUtf32(0x1F568) });
            }
            else
            {
                MuteText.Inlines.Add(new Windows.UI.Xaml.Documents.Run { Text = char.ConvertFromUtf32(0x1F56A) });
            }
        }
        bool State7Loaded = false;
        bool State7LoadedReleased = true;
        private void Load7ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            if (!State7LoadedReleased)
            {
                return;
            }
            ResetLoadState();
            ResetReleaseState();
            State7Loaded = true;
            State7LoadedReleased = false;
            ViewModel.LoadStateSlot7.Execute();
        }
        private void FitScreenButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SetScreenFit.Execute();
        }
        private DependencyProperty changeButtonAction(InjectedInputTypes targetType, InjectedInputTypes targetAction, TextBlock targetTextBlock, string targetText, int targetTextMargin)
        {
            var targetProperty = DependencyProperty.Register(nameof(targetType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(targetAction));
            try
            {
                string ButtonKey = targetTextBlock.Name.Replace("Text", "");
                InputService.CurrentButtons.Add(targetAction, targetText);
                UpdateButtonInDictionary(ButtonKey, targetAction, targetText);
                targetTextBlock.Inlines.Clear();
                if (targetText.Contains("&#"))
                {
                    targetTextBlock.Inlines.Add(new Windows.UI.Xaml.Documents.Run { Text = targetText });
                }
                else
                {
                    targetTextBlock.Inlines.Add(new Windows.UI.Xaml.Documents.Run { Text = targetText });
                }
                int left = 0;
                int top = 0;
                int right = 0;
                int bottom = targetTextMargin;
                targetTextBlock.Margin = new Thickness(left, top, right, bottom);
            }
            catch (Exception e)
            {

            }
            return targetProperty;
        }
        private DependencyProperty changeButtonAction(InjectedInputTypes targetType, InjectedInputTypes targetAction, TextBlock targetTextBlock, string targetText)
        {
            var targetProperty = DependencyProperty.Register(nameof(targetType), typeof(InjectedInputTypes), typeof(VirtualPadActions), new PropertyMetadata(targetAction));
            try
            {
                string ButtonKey = targetTextBlock.Name.Replace("Text", "");
                InputService.CurrentButtons.Add(targetAction, targetText);
                UpdateButtonInDictionary(ButtonKey, targetAction, targetText);
            }
            catch (Exception e)
            {

            }
            return targetProperty;
        }

        public async void GetButtonMap()
        {

            try { 
            await Task.Delay(500);
                while (ViewModel == null || ViewModel.CoreName == null)
                {
                    await Task.Delay(650);
                }


                string CoreName = ViewModel.CoreName;
               
                string SystemName = ViewModel.SystemName.Replace("RACore","");

                try { 
                await GetAndSaveButtonsPositions(ViewModel.SystemName);
                }catch(Exception er)
                {

                }
                SetAudioIcon(ViewModel.AudioMuteLevel);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    try { 
                switch (CoreName)
                {
                    case "Beetle PSX":
                        //GPAButton.Visibility = Visibility.Collapsed;
                        //GPXButton.Visibility = Visibility.Collapsed;
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "\u25B3", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "\u25FB", 10);
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "\u2715", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPCButtonText, "\u25CE", 10);
                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10);
                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPAButtonText, "R", 10);
                        LRButtons[0] = XButtonInputType;
                        LRButtons[1] = AButtonInputType;
                        LRButtonsObjects[0] = GPXButton;
                        LRButtonsObjects[1] = GPAButton;
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "FB Alpha":
                    case "FinalBurn Neo":
                        switch (SystemName)
                        {
                            case "Neo Geo":
                                //GPAButton.Visibility = Visibility.Collapsed;
                                //GPXButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "D", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "C", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "A", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "B", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10);
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPAButtonText, "R", 10);
                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25CD", 10);
                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Arcade":
                                //GPAButton.Visibility = Visibility.Collapsed;
                                //GPXButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "D", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "C", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "A", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "B", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10);
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPAButtonText, "R", 10);
                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25CD", 10);
                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            default:
                                ViewModel.ShowXYZ = false;
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                        }
                        break;
                    case "FCEUmm":
                    case "Nestopia":
                    case "QuickNES":
                        GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Collapsed;
                        GPZButton.Visibility = Visibility.Collapsed;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "Gambatte":
                        GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Collapsed;
                        GPZButton.Visibility = Visibility.Collapsed;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "Genesis Plus GX":
                        ViewModel.IsSegaSystem = true;
                        switch (SystemName)
                        {
                            case "Mega CD":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ViewModel.ShowXYZ = true;
                                ViewModel.RaisePropertyChanged(nameof(ViewModel.ShowXYZ));
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPAButtonText, "A", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "C", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10);
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Mega Drive":
                                if (!ShowXYZ) {
                                    GPXButton.Visibility = Visibility.Collapsed;
                                    GPYButton.Visibility = Visibility.Collapsed;
                                    GPZButton.Visibility = Visibility.Collapsed;
                                }
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPAButtonText, "A", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "C", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10);
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Game Gear":
                            case "Master System":
                            case "SG-1000":
                                ViewModel.ShowXYZ = false;
                                ViewModel.RaisePropertyChanged(nameof(ViewModel.ShowXYZ));
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "1", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPCButtonText, "2", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10);
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            default:
                                if (!ShowXYZ)
                                {
                                    GPXButton.Visibility = Visibility.Collapsed;
                                    GPYButton.Visibility = Visibility.Collapsed;
                                    GPZButton.Visibility = Visibility.Collapsed;
                                }
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPAButtonText, "A", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "C", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10);
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                        }
                        break;
                    case "Mednafen NeoPop":
                    case "Beetle NeoPop":
                        ViewModel.ShowXYZ = false;
                        GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Collapsed;
                        GPZButton.Visibility = Visibility.Collapsed;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "A", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "B", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "Mednafen PCE Fast":
                    case "Beetle PCE Fast":
                        ViewModel.ShowXYZ = false;
                        GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Collapsed;
                        GPZButton.Visibility = Visibility.Collapsed;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "2", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPCButtonText, "1", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "Mednafen PC-FX":
                    case "Beetle PC-FX":
                        //GPXButton.Visibility = Visibility.Collapsed;
                        //GPYButton.Visibility = Visibility.Collapsed;
                        //GPZButton.Visibility = Visibility.Collapsed;
                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPAButtonText, "III", 10);
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "II", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "I", 10);
                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPXButtonText, "IV", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "V", 10);
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "VI", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "Mednafen WonderSwan":
                    case "Beetle WonderSwan":
                        ViewModel.ShowXYZ = false;
                        GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Collapsed;
                        GPZButton.Visibility = Visibility.Collapsed;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "melonDS":
                    case "DeSmuME":
                        //GPAButton.Visibility = Visibility.Collapsed;
                        //GPXButton.Visibility = Visibility.Collapsed;
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10);
                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10);
                        LRButtons[0] = XButtonInputType;
                        LRButtons[1] = AButtonInputType;
                        LRButtonsObjects[0] = GPXButton;
                        LRButtonsObjects[1] = GPAButton;
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "ParaLLEl N64":
                        //GPAButton.Visibility = Visibility.Collapsed;
                        //GPXButton.Visibility = Visibility.Collapsed;
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPYButtonText, "Y", 10);
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPCButtonText, "A", 10);
                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPXButtonText, "L", 10);
                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPAButtonText, "R", 10);
                        LRButtons[0] = XButtonInputType;
                        LRButtons[1] = AButtonInputType;
                        LRButtonsObjects[0] = GPXButton;
                        LRButtonsObjects[1] = GPAButton;
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "PicoDrive":
                        if (!ShowXYZ)
                        {
                            GPXButton.Visibility = Visibility.Collapsed;
                            GPYButton.Visibility = Visibility.Collapsed;
                            GPZButton.Visibility = Visibility.Collapsed;
                        }
                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPAButtonText, "A", 10);
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "C", 10);
                        //XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPXButtonText, "X",10);
                        //YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y",10);
                        //ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z",10);   
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "Snes9x":
                    case "Snes9x 2005":
                        //GPAButton.Visibility = Visibility.Collapsed;
                        //GPXButton.Visibility = Visibility.Collapsed;
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10);
                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10);
                        LRButtons[0] = XButtonInputType;
                        LRButtons[1] = AButtonInputType;
                        LRButtonsObjects[0] = GPXButton;
                        LRButtonsObjects[1] = GPAButton;
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "VBA-M":
                    case "VBA Next":
                        switch (SystemName)
                        {
                            case "Game Boy Advance SP":
                            case "Game Boy Advance":
                            case "Game Boy Micro":
                                ViewModel.ShowXYZ = false;
                                GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Visible;
                        GPZButton.Visibility = Visibility.Visible;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10);
                                LRButtons[0] = YButtonInputType;
                                LRButtons[1] = ZButtonInputType;
                                LRButtonsObjects[0] = GPYButton;
                                LRButtonsObjects[1] = GPZButton;
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Game Boy Color":
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Game Boy":
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            default:
                                ViewModel.ShowXYZ = false;
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Visible;
                                GPZButton.Visibility = Visibility.Visible;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10);
                                LRButtons[0] = YButtonInputType;
                                LRButtons[1] = ZButtonInputType;
                                LRButtonsObjects[0] = GPYButton;
                                LRButtonsObjects[1] = GPZButton;
                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                        }
                        break;

                    case "Beetle VB":
                    case "Mednafen VB":
                        ViewModel.ShowXYZ = false;
                        GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Visible;
                        GPZButton.Visibility = Visibility.Visible;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10);
                        LRButtons[0] = YButtonInputType;
                        LRButtons[1] = ZButtonInputType;
                        LRButtonsObjects[0] = GPYButton;
                        LRButtonsObjects[1] = GPZButton;
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;

                    case "Beetle Lynx":
                    case "Mednafen Lynx":
                    case "Handy":
                    case "Atari Lynx":
                        ViewModel.ShowXYZ = false;
                        GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Visible;
                        GPZButton.Visibility = Visibility.Visible;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "Opera":
                        GPXButton.Visibility = Visibility.Collapsed;
                        //GPYButton.Visibility = Visibility.Collapsed;
                        //GPZButton.Visibility = Visibility.Collapsed;
                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPAButtonText, "Y", 10);
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                        //XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPXButtonText, "X",10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "L",10);
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R",10);
                        LRButtons[0] = YButtonInputType;
                        LRButtons[1] = ZButtonInputType;
                        LRButtonsObjects[0] = GPYButton;
                        LRButtonsObjects[1] = GPZButton;
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "VecX":
                        ViewModel.ShowXYZ = false;
                        GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Visible;
                        GPZButton.Visibility = Visibility.Visible;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "2", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "1", 10);
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "3", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "4", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "ProSystem":
                        GPAButton.Visibility = Visibility.Collapsed;
                        GPXButton.Visibility = Visibility.Collapsed;
                        GPYButton.Visibility = Visibility.Collapsed;
                        GPZButton.Visibility = Visibility.Collapsed;
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "1", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "2", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "Virtual Jaguar":
                        //GPXButton.Visibility = Visibility.Collapsed;
                        //GPYButton.Visibility = Visibility.Collapsed;
                        //GPZButton.Visibility = Visibility.Collapsed;
                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPAButtonText, "A", 10);
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "C", 10);
                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                    case "Beetle Saturn":
                    case "Yabause":
                        //GPXButton.Visibility = Visibility.Collapsed;
                        //GPYButton.Visibility = Visibility.Collapsed;
                        //GPZButton.Visibility = Visibility.Collapsed;
                        //X -> Y, A -> B, C -> Z, B -> X, Z -> C, Y -> A 
                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPAButtonText, "A", 10);
                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPBButtonText, "B", 10);
                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPCButtonText, "C", 10);
                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPXButtonText, "X", 10);
                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPYButtonText, "Y", 10);
                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPZButtonText, "Z", 10);
                        InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                        break;
                        case "Atari800":

                            break;
                        case "FreeChaF":
                            ViewModel.ShowXYZ = false;
                            /*GPAButton.Visibility = Visibility.Collapsed;
                            GPXButton.Visibility = Visibility.Collapsed;
                            GPYButton.Visibility = Visibility.Collapsed;
                            GPZButton.Visibility = Visibility.Collapsed;
                            BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "2", 10);
                            CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPCButtonText, "1", 10);
                            InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };*/
                            break;
                        case "PokeMini":
                            ViewModel.ShowXYZ = false;
                            GPAButton.Visibility = Visibility.Collapsed;
                            GPXButton.Visibility = Visibility.Collapsed;
                            GPYButton.Visibility = Visibility.Visible;
                            GPZButton.Visibility = Visibility.Visible;
                            BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                            CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                            ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10);
                            YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10);
                            LRButtons[0] = YButtonInputType;
                            LRButtons[1] = ZButtonInputType;
                            LRButtonsObjects[0] = GPYButton;
                            LRButtonsObjects[1] = GPZButton;
                            InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                            break;
                        case "Potator":
                            GPAButton.Visibility = Visibility.Collapsed;
                            GPXButton.Visibility = Visibility.Collapsed;
                            GPYButton.Visibility = Visibility.Collapsed;
                            GPZButton.Visibility = Visibility.Collapsed;
                            BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                            CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                            InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                            break;
                        case "TIC-80":
                            //GPAButton.Visibility = Visibility.Collapsed;
                            //GPXButton.Visibility = Visibility.Collapsed;
                            ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                            YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                            BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "B", 10);
                            CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPCButtonText, "A", 10);
                            XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10);
                            AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10);
                            LRButtons[0] = XButtonInputType;
                            LRButtons[1] = AButtonInputType;
                            LRButtonsObjects[0] = GPXButton;
                            LRButtonsObjects[1] = GPAButton;
                            InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                            break;
                    default:
                        break;
                }
                    }catch(Exception ex)
                    {

                    }
                });
            }catch(Exception e)
            {

            }
        }

        private void GPYButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

        }

        private void GPL2Button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }

        private void GPR2Button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }

        private void GPL2Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(L2ButtonInputType);
        }

        private void GPR2Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(R2ButtonInputType);
        }

        private void GPL1R1Button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.ActionsCustomDelay = true;
            AddNewActionButton(LRButtonsObjects[0]);
            AddNewActionButton(LRButtonsObjects[1]);
        }

        private void GPL1R1Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(LRButtons[0]);
            ViewModel.InjectInputCommand.Execute(LRButtons[1]);
        }

        private void GPL2R2Button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.ActionsCustomDelay = true;
            AddNewActionButton(GPL2Button);
            AddNewActionButton(GPR2Button);
        }

        private void GPL2R2Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.InjectInputCommand.Execute(L2ButtonInputType);
            ViewModel.InjectInputCommand.Execute(R2ButtonInputType);
        }
        private async void ActiveButton(object sender)
        {
            try
            {
                var ObjectName = (sender as RepeatButton).Name;
                (sender as RepeatButton).Background = (SolidColorBrush)Resources["ActiveButton"];
                await Task.Delay(10);
                (sender as RepeatButton).Background = (SolidColorBrush)Resources["InActive"];
            }
            catch (Exception ee)
            {

            }
        }
        private void ButtonControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try { 
            if (ViewModel.GameStopStarted || !ViewModel.ButtonsCustomization) return;
            var PositionX = e.Delta.Translation.X / ViewModel.RightScaleFactorValue;
            var PositionY = e.Delta.Translation.Y / ViewModel.RightScaleFactorValue;
            var ObjectType = sender.GetType().Name;
            string ObjectName = "None";
            switch (ObjectType)
            {
                case "RepeatButton":
                    ObjectName = ((RepeatButton)sender).Name;
                    break;
                case "Button":
                    ObjectName = ((Button)sender).Name;
                    break;
            }
            InputService.SaveButtonPosition(ObjectName, PositionX, PositionY);
            InputService.SetButtonPosition(ObjectName);
            }catch(Exception ex)
            {

            }
        }

       
    }
}
