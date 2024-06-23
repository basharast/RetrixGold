using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using RetriX.UWP.Services;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;
using LibRetriX;
using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.Gaming.Input;
using Windows.UI;
using System.Reflection;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RetriX.UWP.Controls
{
    public sealed partial class VirtualPadActions : UserControl
    {
        public static Dictionary<string, Dictionary<string, InjectedInputTypes>> ButtonsDictionary = new Dictionary<string, Dictionary<string, InjectedInputTypes>>();
        Dictionary<string, Func<bool>> GetStatsMap = new Dictionary<string, Func<bool>>();
        public VirtualPadActions()
        {
            this.InitializeComponent();
            PlatformService.AddNewActionButton = AddNewActionButton;
            PlatformService.UpdateButtonsMap = UpdateButtonsMap;

            try
            {
                GetStatsMap = new Dictionary<string, Func<bool>>(){
                {"GPAButton", Get_A_State },
                {"GPBButton", Get_B_State },
                {"GPCButton", Get_C_State },
                {"GPXButton", Get_X_State },
                {"GPYButton", Get_Y_State },
                {"GPZButton", Get_Z_State },
                {"GPL2Button", Get_L1_State },
                {"GPR2Button", Get_R1_State },
                {"GPStartButton", Get_START_State },
                {"GPSelectButton", Get_SELECT_State },
                };
            }
            catch (Exception ex)
            {

            }
            PrepareButtonsDictionary();
            PlatformService.UseL3R3InsteadOfX1X2Updater = UpdateL3R3;
            if (PlatformService.UseL3R3InsteadOfX1X2)
            {
                UpdateL3R3(null, null);
            }
        }

        #region START
        bool START_ActiveState = false;
        public void START_Active()
        {
            START_ActiveState = true;
            UpdateTopBarState();
        }
        public void START_InActive()
        {
            START_ActiveState = false;
        }
        public bool Get_START_State()
        {
            return START_ActiveState;
        }
        #endregion

        #region SELECT
        bool SELECT_ActiveState = false;
        public void SELECT_Active()
        {
            SELECT_ActiveState = true;
            UpdateTopBarState();
        }
        public void SELECT_InActive()
        {
            SELECT_ActiveState = false;
        }
        public bool Get_SELECT_State()
        {
            return SELECT_ActiveState;
        }
        #endregion

        #region A
        bool A_ActiveState = false;
        public void A_Active()
        {
            A_ActiveState = true;
            UpdateTopBarState();
        }
        public void A_InActive()
        {
            A_ActiveState = false;
        }
        public bool Get_A_State()
        {
            return A_ActiveState;
        }
        #endregion

        #region B
        bool B_ActiveState = false;
        public void B_Active()
        {
            B_ActiveState = true;
            UpdateTopBarState();
        }
        public void B_InActive()
        {
            B_ActiveState = false;
        }
        public bool Get_B_State()
        {
            return B_ActiveState;
        }
        #endregion

        #region C
        bool C_ActiveState = false;
        public void C_Active()
        {
            C_ActiveState = true;
            UpdateTopBarState();
        }
        public void C_InActive()
        {
            C_ActiveState = false;
        }
        public bool Get_C_State()
        {
            return C_ActiveState;
        }
        #endregion


        #region X
        bool X_ActiveState = false;
        public void X_Active()
        {
            X_ActiveState = true;
            UpdateTopBarState();
        }
        public void X_InActive()
        {
            X_ActiveState = false;
        }
        public bool Get_X_State()
        {
            return X_ActiveState;
        }
        #endregion

        #region Y
        bool Y_ActiveState = false;
        public void Y_Active()
        {
            Y_ActiveState = true;
            UpdateTopBarState();
        }
        public void Y_InActive()
        {
            Y_ActiveState = false;
        }
        public bool Get_Y_State()
        {
            return Y_ActiveState;
        }
        #endregion

        #region Z
        bool Z_ActiveState = false;
        public void Z_Active()
        {
            Z_ActiveState = true;
            UpdateTopBarState();
        }
        public void Z_InActive()
        {
            Z_ActiveState = false;
        }
        public bool Get_Z_State()
        {
            return Z_ActiveState;
        }
        #endregion


        #region L1
        bool L1_ActiveState = false;
        public void L1_Active()
        {
            L1_ActiveState = true;
            UpdateTopBarState();
        }
        public void L1_InActive()
        {
            L1_ActiveState = false;
        }
        public bool Get_L1_State()
        {
            return L1_ActiveState;
        }
        #endregion

        #region R1
        bool R1_ActiveState = false;
        public void R1_Active()
        {
            R1_ActiveState = true;
            UpdateTopBarState();
        }
        public void R1_InActive()
        {
            R1_ActiveState = false;
        }
        public bool Get_R1_State()
        {
            return R1_ActiveState;
        }
        #endregion

        #region L2
        public void L1R1_Active()
        {
            
            {
                if (LRButtons[0] == AButtonInputType)
                {
                    A_Active();
                }
                else
                if (LRButtons[0] == BButtonInputType)
                {
                    B_Active();
                }
                else
                if (LRButtons[0] == CButtonInputType)
                {
                    C_Active();
                }
                else
                if (LRButtons[0] == XButtonInputType)
                {
                    X_Active();
                }
                else
                if (LRButtons[0] == YButtonInputType)
                {
                    Y_Active();
                }
                else
                if (LRButtons[0] == ZButtonInputType)
                {
                    Z_Active();
                }
            }
            {
                if (LRButtons[1] == AButtonInputType)
                {
                    A_Active();
                }
                else
                if (LRButtons[1] == BButtonInputType)
                {
                    B_Active();
                }
                else
                if (LRButtons[1] == CButtonInputType)
                {
                    C_Active();
                }
                else
                if (LRButtons[1] == XButtonInputType)
                {
                    X_Active();
                }
                else
                if (LRButtons[1] == YButtonInputType)
                {
                    Y_Active();
                }
                else
                if (LRButtons[1] == ZButtonInputType)
                {
                    Z_Active();
                }
            }
            try
            {
                (LRButtonsObjects[0] as Button).Background = (SolidColorBrush)Resources["ActiveButton"];
                (LRButtonsObjects[1] as Button).Background = (SolidColorBrush)Resources["ActiveButton"];
            }
            catch(Exception ex)
            {

            }
            UpdateTopBarState();
        }
        public void L1R1_InActive()
        {
            {
                if (LRButtons[0] == AButtonInputType)
                {
                    A_InActive();
                }
                else
                if (LRButtons[0] == BButtonInputType)
                {
                    B_InActive();
                }
                else
                if (LRButtons[0] == CButtonInputType)
                {
                    C_InActive();
                }
                else
                if (LRButtons[0] == XButtonInputType)
                {
                    X_InActive();
                }
                else
                if (LRButtons[0] == YButtonInputType)
                {
                    Y_InActive();
                }
                else
                if (LRButtons[0] == ZButtonInputType)
                {
                    Z_InActive();
                }
            }
            {
                if (LRButtons[1] == AButtonInputType)
                {
                    A_InActive();
                }
                else
                if (LRButtons[1] == BButtonInputType)
                {
                    B_InActive();
                }
                else
                if (LRButtons[1] == CButtonInputType)
                {
                    C_InActive();
                }
                else
                if (LRButtons[1] == XButtonInputType)
                {
                    X_InActive();
                }
                else
                if (LRButtons[1] == YButtonInputType)
                {
                    Y_InActive();
                }
                else
                if (LRButtons[1] == ZButtonInputType)
                {
                    Z_InActive();
                }
            }
            try
            {
                (LRButtonsObjects[0] as Button).Background = (SolidColorBrush)Resources["InActive"];
                (LRButtonsObjects[1] as Button).Background = (SolidColorBrush)Resources["InActive"];
            }
            catch(Exception ex)
            {

            }
        }
        bool L2_ActiveState = false;
        public void L2_Active()
        {
            if (PlatformService.UseL3R3InsteadOfX1X2)
            {
                L2_ActiveState = true;
            }
            else
            {
                L1R1_Active();
            }
            UpdateTopBarState();
        }
        public void L2_InActive()
        {
            if (PlatformService.UseL3R3InsteadOfX1X2)
            {
                L2_ActiveState = false;
            }
            else
            {
                L1R1_InActive();
            }
        }
        public bool Get_L2_State()
        {
            return L2_ActiveState;
        }
        #endregion

        #region R2
        public void L2R2_Active()
        {
            L1_Active();
            R1_Active();
            try
            {
                GPL2Button.Background = (SolidColorBrush)Resources["ActiveButton"];
                GPR2Button.Background = (SolidColorBrush)Resources["ActiveButton"];
            }
            catch (Exception ex)
            {

            }
            UpdateTopBarState();
        }
        public void L2R2_InActive()
        {
            L1_InActive();
            R1_InActive();
            try
            {
                GPL2Button.Background = (SolidColorBrush)Resources["InActive"];
                GPR2Button.Background = (SolidColorBrush)Resources["InActive"];
            }
            catch (Exception ex)
            {

            }
        }
        bool R2_ActiveState = false;
        public void R2_Active()
        {
            if (PlatformService.UseL3R3InsteadOfX1X2)
            {
                R2_ActiveState = true;
            }
            else
            {
                L2R2_Active();
            }
            UpdateTopBarState();
        }
        public void R2_InActive()
        {
            if (PlatformService.UseL3R3InsteadOfX1X2)
            {
                R2_ActiveState = false;
            }
            else
            {
                L2R2_InActive();
            }
        }
        public bool Get_R2_State()
        {
            return R2_ActiveState;
        }
        #endregion
        private void UpdateTopBarState()
        {
            try
            {
                if(ViewModel !=null && ViewModel.TopBarOpacity != 0.0f)
                {
                    ViewModel.TopBarOpacity = 0.0f;
                }
            }catch(Exception ex)
            {

            }
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
        public bool IsSegaSystem
        {
            get
            {
                return IsSegaSystemState;
            }
            set
            {
                IsSegaSystemState = value;
            }
        }

        InjectedInputTypes[] LRButtons = new InjectedInputTypes[] { };
        object[] LRButtonsObjects = new object[] { };
        public void PrepareButtonsDictionary()
        {
            ButtonsDictionary.Clear();
            for (int i = 0; i < 12; i++)
            {
                InputService.CurrentButtonsMaps[i].Clear();
            }

            AddNewInputTypeToDictionary(AButtonInputType, "GPAButton", "A");
            AddNewInputTypeToDictionary(BButtonInputType, "GPBButton", "B");
            AddNewInputTypeToDictionary(CButtonInputType, "GPCButton", "C");
            AddNewInputTypeToDictionary(XButtonInputType, "GPXButton", "X");
            AddNewInputTypeToDictionary(YButtonInputType, "GPYButton", "Y");
            AddNewInputTypeToDictionary(ZButtonInputType, "GPZButton", "Z");
            AddNewInputTypeToDictionary(L2ButtonInputType, "GPL2Button", "L2");
            AddNewInputTypeToDictionary(R2ButtonInputType, "GPR2Button", "R2");
            AddNewInputTypeToDictionary(StartButtonInputType, "GPStartButton", "Start");
            AddNewInputTypeToDictionary(SelectButtonInputType, "GPSelectButton", "Select");
            LRButtons = new InjectedInputTypes[] { LButtonInputType, RButtonInputType };
            LRButtonsObjects = new object[] { GPAButton, GPXButton };

            PrepareRightControls();
        }

        public void PrepareRightControls()
        {
            try
            {
                lock (InputService.RightControls)
                {
                    InputService.RightControls.Clear();

                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = StartButtonInputType,
                        GetState = Get_START_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = SelectButtonInputType,
                        GetState = Get_SELECT_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = AButtonInputType,
                        GetState = Get_A_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = BButtonInputType,
                        GetState = Get_B_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = CButtonInputType,
                        GetState = Get_C_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = XButtonInputType,
                        GetState = Get_X_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = YButtonInputType,
                        GetState = Get_Y_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = ZButtonInputType,
                        GetState = Get_Z_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = LButtonInputType,
                        GetState = Get_L1_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = RButtonInputType,
                        GetState = Get_R1_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = L2ButtonInputType,
                        GetState = Get_L2_State
                    });
                    InputService.RightControls.Add(new TouchControl()
                    {
                        InjectedInput = R2ButtonInputType,
                        GetState = Get_R2_State
                    });
                }
            }
            catch (Exception ex)
            {

            }
            if (PlatformService.PrepareLeftControls != null)
            {
                try
                {
                    PlatformService.PrepareLeftControls.Invoke(null, null);
                }
                catch (Exception ex)
                {

                }
            }
        }
        public static Dictionary<VirtualKey, string> VirtualButtonsMap = new Dictionary<VirtualKey, string>()
        {
            {VirtualKey.GamepadDPadDown, "DownButton" },
            {VirtualKey.GamepadDPadUp, "UpButton" },
            {VirtualKey.GamepadDPadLeft, "LeftButton" },
            {VirtualKey.GamepadDPadRight, "RightButton" },
            {VirtualKey.GamepadA, "GPCButton" },
            {VirtualKey.GamepadB, "GPBButton" },
            {VirtualKey.GamepadX, "GPZButton" },
            {VirtualKey.GamepadY, "GPYButton" },
            {VirtualKey.GamepadLeftShoulder, "GPXButton" },
            {VirtualKey.GamepadLeftThumbstickButton, "" },
            {VirtualKey.GamepadLeftThumbstickDown, "" },
            {VirtualKey.GamepadLeftThumbstickLeft, "" },
            {VirtualKey.GamepadLeftThumbstickUp, "" },
            {VirtualKey.GamepadLeftThumbstickRight, "" },
            {VirtualKey.GamepadRightThumbstickButton, "" },
            {VirtualKey.GamepadRightThumbstickDown, "" },
            {VirtualKey.GamepadRightThumbstickLeft, "" },
            {VirtualKey.GamepadRightThumbstickUp, "" },
            {VirtualKey.GamepadRightThumbstickRight, "" },
            {VirtualKey.GamepadLeftTrigger, "GPL2Button" },
            {VirtualKey.GamepadRightTrigger, "GPR2Button" },
            {VirtualKey.GamepadRightShoulder, "GPAButton" },
            {VirtualKey.GamepadView, "GPSelectButton" },
            {VirtualKey.GamepadMenu, "GPStartButton" },
        };



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
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(GamepadSaveLocation);
                if (localFolder == null)
                {
                    ViewModel.SetButtonsIsLoadingState(false);
                    return;
                }

                var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{SystemName}.rbm");
                if (targetFileTest != null)
                {
                    ViewModel.SetButtonsIsLoadingState(true);
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
        public void UpdateButtonInDictionary(string ButtonKey, InjectedInputTypes ButtonNewType, string ButtonTitle)
        {
            Dictionary<string, InjectedInputTypes> AddNewType = new Dictionary<string, InjectedInputTypes>();
            AddNewType.Add(ButtonTitle, ButtonNewType);
            ButtonsDictionary[ButtonKey] = AddNewType.ToDictionary(entry => entry.Key, entry => entry.Value);
        }
        public void AddNewInputTypeToDictionary(InjectedInputTypes TargetType, string Buttonkey, string TypeTitle)
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

        public InjectedInputTypes UpButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(UpButtonInputTypeProperty); }
            set { SetValue(UpButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty UpButtonInputTypeProperty = DependencyProperty.Register(nameof(UpButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadUp));

        public InjectedInputTypes DownButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(DownButtonInputTypeProperty); }
            set { SetValue(DownButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DownButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty DownButtonInputTypeProperty = DependencyProperty.Register(nameof(DownButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadDown));

        public InjectedInputTypes LeftButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(LeftButtonInputTypeProperty); }
            set { SetValue(LeftButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty LeftButtonInputTypeProperty = DependencyProperty.Register(nameof(LeftButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadLeft));

        public InjectedInputTypes RightButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(RightButtonInputTypeProperty); }
            set { SetValue(RightButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty RightButtonInputTypeProperty = DependencyProperty.Register(nameof(RightButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadRight));

        public InjectedInputTypes PointerPressedInputType
        {
            get { return (InjectedInputTypes)GetValue(PointerPressedTypeProperty); }
            set { SetValue(PointerPressedTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty PointerPressedTypeProperty = DependencyProperty.Register(nameof(PointerPressedInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdPointerPressed));

        public InjectedInputTypes MouseRightInputType
        {
            get { return (InjectedInputTypes)GetValue(MouseRightTypeProperty); }
            set { SetValue(MouseRightTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty MouseRightTypeProperty = DependencyProperty.Register(nameof(MouseRightInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdMouseRight));

        public InjectedInputTypes MouseLeftInputType
        {
            get { return (InjectedInputTypes)GetValue(MouseLeftTypeProperty); }
            set { SetValue(MouseLeftTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty MouseLeftTypeProperty = DependencyProperty.Register(nameof(MouseLeftInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdMouseLeft));

        private void Button01Clicked(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-01");
        }
        private void Button02Clicked(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-02");
        }
        private void Button03Clicked(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-03");
        }

        private void Button04Clicked(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-04");
        }
        private void Button04Clicked()
        {
            if (ViewModel.TabSoundEffect)
            {
                PlatformService.PlayNotificationSoundDirect("button-04");
            }
        }
        public void AddNewActionButton(object sender)
        {
            try
            {
                string ButtonKey = ((Button)sender).Name;
                string ButtonTitle = ButtonsDictionary[ButtonKey].Keys.First();
                InjectedInputTypes ButtonAction = ButtonsDictionary[ButtonKey][ButtonTitle];
                ViewModel.AddActionButton(ButtonKey, ButtonTitle, ButtonAction, ViewModel.ActionsCustomDelay ? "+" : ",");
                Button04Clicked();
                ActiveButton(sender);
            }
            catch (Exception ee)
            {

            }
        }
        public void AddNewActionButton(object sender, EventArgs args)
        {
            try
            {
                string ButtonKey = ((string)sender);
                switch (ButtonKey)
                {
                    case "UpButton":
                        ViewModel.AddActionButton("Up", "Up", UpButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                        break;

                    case "DownButton":
                        ViewModel.AddActionButton("Down", "Down", DownButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                        break;

                    case "LeftButton":
                        ViewModel.AddActionButton("Left", "Left", LeftButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                        break;

                    case "RightButton":
                        ViewModel.AddActionButton("Right", "Right", RightButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                        break;

                    case "GPSelectButton":
                    case "GPStartButton":

                        break;

                    default:
                        string ButtonTitle = ButtonsDictionary[ButtonKey].Keys.First();
                        InjectedInputTypes ButtonAction = ButtonsDictionary[ButtonKey][ButtonTitle];
                        ViewModel.AddActionButton(ButtonKey, ButtonTitle, ButtonAction, ViewModel.ActionsCustomDelay ? "+" : ",");
                        break;
                }
            }
            catch (Exception ee)
            {

            }
        }
        private void XButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(XButtonInputType);
            PlatformService.DPadActive = false;
        }
        private void XButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void BButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(BButtonInputType);
            PlatformService.DPadActive = false;
        }
        private void BButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void YButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(YButtonInputType);
            PlatformService.DPadActive = false;
        }
        private void YButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }

        private void AButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(AButtonInputType);
            PlatformService.DPadActive = false;
        }
        private void AButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }

        private void CButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(CButtonInputType);
            PlatformService.DPadActive = false;
        }
        private void CButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void ZButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(ZButtonInputType);
            PlatformService.DPadActive = false;
        }
        private void ZButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void LButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(LButtonInputType);
            PlatformService.DPadActive = false;
        }
        private void LButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void RButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(RButtonInputType);
            PlatformService.DPadActive = false;
        }
        private void RButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(StartButtonInputType);
            PlatformService.DPadActive = false;
        }
        private void StartButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void SelectButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(SelectButtonInputType);
            PlatformService.DPadActive = false;

        }
        private void SelectButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            AddNewActionButton(sender);
        }
        private void ToggleBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            PlatformService.PlayNotificationSoundDirect("button-01");
            ViewModel.TogglePauseCommand.Execute(null);
            //ViewModel.TappedCommand.Execute();
        }


        private async void Save9ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.QuickSaveState();
        }

        private async void Save8ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            ViewModel.QuickLoadState();
        }

        private async void Save7ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            bool MuteState = ViewModel.ToggleMuteAudioCall();
            PlatformService.PlayNotificationSoundDirect("button-01");
            SetAudioIcon(MuteState);
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

        private void FitScreenButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SetScreenFit.Execute(null);
        }

        private DependencyProperty changeButtonAction(InjectedInputTypes targetType, InjectedInputTypes targetAction, TextBlock targetTextBlock, string targetText, int targetTextMargin, string originalText = "", Type type = null)
        {
            if (type == null)
            {
                type = typeof(VirtualPadActions);
            }
            var targetProperty = DependencyProperty.Register(nameof(targetType), typeof(InjectedInputTypes), type, new PropertyMetadata(targetAction));
            try
            {
                string ButtonKey = targetTextBlock != null ? targetTextBlock.Name.Replace("Text", "") : originalText;

                var buttonLookup = targetText;
                var descTest = "";
                if (originalText.Length == 0)
                {
                    var buttonLookupTest = "";
                    if (DeviceIdsMap.TryGetValue(targetAction, out buttonLookupTest))
                    {
                        buttonLookup = buttonLookupTest;
                    }
                }

                for (int i = 0; i < 12; i++)
                {
                    var buttonText = targetText;
                    if (InputService.Descriptors[i].TryGetValue((originalText.Length > 0 ? originalText : buttonLookup), out descTest))
                    {
                        if (PlatformService.isXBOX)
                        {
                            buttonText = $"{descTest}";
                        }
                        else
                        {
                            buttonText = $"{descTest} (Touch: {targetText})";
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            if (targetTextBlock != null)
                            {
                                targetTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
                            }
                        }
                    }
                    try
                    {
                        var KeyboardButton = VirtualKey.None;
                        if (InputService.LibretroGamepadToKeyboardKeyMapping.TryGetValue((InputTypes)targetAction, out KeyboardButton))
                        {
                            try
                            {
                                KeyboardButton = InputService.GetKeyByRules(KeyboardButton);
                            }
                            catch (Exception ex)
                            {

                            }
                            buttonText += $" [Keyboard: {KeyboardButton}]";
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    var testValue = "";

                    if (InputService.CurrentButtonsMaps[i].TryGetValue(targetAction, out testValue))
                    {
                        InputService.CurrentButtonsMaps[i][targetAction] = buttonText;
                    }
                    else
                    {
                        InputService.CurrentButtonsMaps[i].Add(targetAction, buttonText);
                    }
                }



                UpdateButtonInDictionary(ButtonKey, targetAction, targetText);
                if (targetTextBlock != null)
                {
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
            }
            catch (Exception e)
            {

            }
            return targetProperty;
        }

        Dictionary<InjectedInputTypes, string> DeviceIdsMap = new Dictionary<InjectedInputTypes, string>()
        {
            {InjectedInputTypes.DeviceIdJoypadB, "B" },
            {InjectedInputTypes.DeviceIdJoypadA, "A" },
            {InjectedInputTypes.DeviceIdJoypadSelect, "Select" },
            {InjectedInputTypes.DeviceIdJoypadStart, "Start" },
            {InjectedInputTypes.DeviceIdJoypadUp, "Up" },
            {InjectedInputTypes.DeviceIdJoypadDown, "Down" },
            {InjectedInputTypes.DeviceIdJoypadLeft, "Left" },
            {InjectedInputTypes.DeviceIdJoypadRight, "Right" },
            {InjectedInputTypes.DeviceIdJoypadX, "X" },
            {InjectedInputTypes.DeviceIdJoypadY, "Y" },
            {InjectedInputTypes.DeviceIdJoypadL, "L2" },
            {InjectedInputTypes.DeviceIdJoypadR, "R2" },
            {InjectedInputTypes.DeviceIdJoypadL2, "L3" },
            {InjectedInputTypes.DeviceIdJoypadR2, "R3" },
            {InjectedInputTypes.DeviceIdJoypadC, "L" },
            {InjectedInputTypes.DeviceIdJoypadZ, "R" },
        };

        private void UpdateButtonsMap(object sender, EventArgs args)
        {
            try
            {
                var name = (string)sender;
                GetButtonMap(name);
            }
            catch (Exception ex)
            {

            }
        }

        //NOTE:
        //InjectedInputTypes.DeviceIdJoypadC means mostly "L" with exceptions for SEGA
        //InjectedInputTypes.DeviceIdJoypadZ means mostly "R" with exceptions for SEGA

        //NOTE: This function for touch pad and keyboard mapping, it's not related to real gamepad mapping
        public async void GetButtonMap(string forceName = "")
        {

            try
            {
                await Task.Delay(500);
                while (ViewModel == null || ViewModel.CoreName == null)
                {
                    await Task.Delay(650);
                }


                string CoreName = ViewModel.CoreName;

                string SystemName = ViewModel.SystemName.Replace("RACore", "");

                if (forceName != null && forceName.Length > 0)
                {
                    CoreName = forceName;
                }

                try
                {
                    await GetAndSaveButtonsPositions(ViewModel.SystemName);
                }
                catch (Exception er)
                {

                }
                SetAudioIcon(ViewModel.AudioMuteLevel);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        try
                        {
                            //Set default state for all buttons
                            L3R3CustomTest = false;
                            GPXButton.Visibility = Visibility.Visible;
                            GPYButton.Visibility = Visibility.Visible;
                            GPZButton.Visibility = Visibility.Visible;
                            GPAButton.Visibility = Visibility.Visible;
                            GPBButton.Visibility = Visibility.Visible;
                            GPCButton.Visibility = Visibility.Visible;

                            ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                            YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                            BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                            CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                            XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                            AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                            SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                            StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                            UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                            DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                            LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                            RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                            //L2R2
                            LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                            RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                            //L3R3
                            L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                            R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));
                        }
                        catch (Exception ex)
                        {

                        }
                        switch (CoreName.Replace("'", ""))
                        {
                            case "Beetle PSX":
                            case "Beetle PSX HW":
                            case "Playstation":
                            case "DuckStation":
                            case "PCSX ReARMed":
                            case "PCSX-ReARMed":
                                //GPAButton.Visibility = Visibility.Collapsed;
                                //GPXButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "\u25B3", 10, "X");
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "\u25FB", 10, "Y");
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "\u2715", 10, "B");
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPCButtonText, "\u25CE", 10, "A");
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "PCSX2":
                                //GPAButton.Visibility = Visibility.Collapsed;
                                //GPXButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "\u25B3", 10, "X");
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "\u25FB", 10, "Y");
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "\u2715", 10, "B");
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPCButtonText, "\u25CE", 10, "A");
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10, "L");
                                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPAButtonText, "R", 10, "R");

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25CD", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10, "L");
                                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPAButtonText, "R", 10, "R");

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25CD", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                        break;

                                    default:
                                        ViewModel.ShowXYZ = false;
                                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "D", 10);
                                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "C", 10);
                                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "A", 10);
                                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "B", 10);
                                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10, "L");
                                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPAButtonText, "R", 10, "R");

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25CD", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                        break;
                                }
                                break;

                            case "MAME 2000":
                            case "MAME2000":
                            case "MAME2003Plus":
                            case "MAME2003 Plus":
                            case "MAME 2003 Plus":
                            case "MAME 2003-Plus":
                            case "MAME":
                                //GPAButton.Visibility = Visibility.Collapsed;
                                //GPXButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "D", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "C", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "A", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "B", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25CD", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
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

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Gambatte":
                            case "Gearboy":
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Genesis Plus GX":
                            case "Genesis Plus GX Wide":
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
                                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10, "L");
                                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10, "R");

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                        break;
                                    case "Mega Drive":
                                    case "SEGA Wide":
                                        if (!ShowXYZ)
                                        {
                                            GPXButton.Visibility = Visibility.Collapsed;
                                            GPYButton.Visibility = Visibility.Collapsed;
                                            GPZButton.Visibility = Visibility.Collapsed;
                                        }
                                        AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPAButtonText, "A", 10);
                                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "C", 10);
                                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10, "L");
                                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10, "R");

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "1", 10);
                                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "2", 10);
                                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10, "L");
                                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10, "R");

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                        XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10, "L");
                                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "Z", 10, "R");

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                        break;
                                }
                                break;
                            case "Mednafen NeoPop":
                            case "Beetle NeoPop":
                            case "RACE":
                                ViewModel.ShowXYZ = false;
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "A", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "B", 10);

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Mednafen PCE Fast":
                            case "Mednafen PCE":
                            case "Beetle PCE Fast":
                            case "Beetle PCE":
                                ViewModel.ShowXYZ = false;
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "2", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPCButtonText, "1", 10);

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "V", 10, "L");
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "VI", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "VBA-M":
                            case "VBA Next":
                            case "Meteor GBA":
                                switch (SystemName)
                                {
                                    case "Game Boy Advance SP":
                                    case "Game Boy Advance":
                                    case "Game Boy Micro":
                                    case "Game Boy Advance (Meteor)":
                                        ViewModel.ShowXYZ = false;
                                        GPAButton.Visibility = Visibility.Collapsed;
                                        GPXButton.Visibility = Visibility.Collapsed;
                                        GPYButton.Visibility = Visibility.Visible;
                                        GPZButton.Visibility = Visibility.Visible;
                                        BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                        CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10, "R");
                                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10, "L");

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        LRButtons[0] = YButtonInputType;
                                        LRButtons[1] = ZButtonInputType;
                                        LRButtonsObjects[0] = GPYButton;
                                        LRButtonsObjects[1] = GPZButton;

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                        ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10, "R");
                                        YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10, "L");

                                        SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                        StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                        UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                        DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                        LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                        RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                        LRButtons[0] = YButtonInputType;
                                        LRButtons[1] = ZButtonInputType;
                                        LRButtonsObjects[0] = GPYButton;
                                        LRButtonsObjects[1] = GPZButton;

                                        //L2R2
                                        LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                        RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                        //L3R3
                                        L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                        R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                        break;
                                }
                                break;

                            case "TGB Dual":
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "NXEngine":
                            case "Cave Story":
                                ViewModel.ShowXYZ = false;
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Visible;
                                GPZButton.Visibility = Visibility.Visible;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10, "R");
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10, "L");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = YButtonInputType;
                                LRButtons[1] = ZButtonInputType;
                                LRButtonsObjects[0] = GPYButton;
                                LRButtonsObjects[1] = GPZButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
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

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = YButtonInputType;
                                LRButtons[1] = ZButtonInputType;
                                LRButtonsObjects[0] = GPYButton;
                                LRButtonsObjects[1] = GPZButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10, "R");
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10, "L");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "ZX81":
                            case "Sinclair ZX81":
                            case "EightyOne":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Game & Watch":
                            case "GW":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Intellivision":
                            case "FreeIntv":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Amstrad CPC":
                            case "Caprice32":
                            case "cap32":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Opera":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPAButtonText, "A", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "C", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPXButtonText, "P", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "L", 10, "L");
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = YButtonInputType;
                                LRButtons[1] = ZButtonInputType;
                                LRButtonsObjects[0] = GPYButton;
                                LRButtonsObjects[1] = GPZButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "ProSystem":
                                //GPAButton.Visibility = Visibility.Collapsed;
                                //GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "1", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "2", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Virtual Jaguar":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPAButtonText, "A", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPCButtonText, "C", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPXButtonText, "0", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "1", 10, "L");
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "2", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPCButtonText, "C", 10, "R");
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPXButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPZButtonText, "Z", 10, "L");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "Atari 5200":
                            case "Atari800":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "F2", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "H", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "R", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "F1", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "O", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "M", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "MSX Computer":
                            case "BlueMSX":
                            case "fMSX":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "Doom":
                            case "PrBoom":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "Cannonball":
                            case "OutRun":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                //NOTE: these overrides mainly tested on touch controls and they could be deferent from joypad
                                if (PlatformService.isXBOX || PlatformService.DPadActive)
                                {
                                    ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPZButtonText, "X", 10, "X");
                                    YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPYButtonText, "Y", 10, "Y");
                                    BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "B", 10, "B");
                                    CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPCButtonText, "A", 10, "A");
                                    XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                    AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                    SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25CD", 10, "Select");
                                    StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                    UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                    DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                    LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                    RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));
                                }
                                else
                                {
                                    ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, GPZButtonText, "X", 10, "X");
                                    YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, GPYButtonText, "Y", 10, "Y");
                                    BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, GPBButtonText, "B", 10, "B");
                                    CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, GPCButtonText, "A", 10, "A");
                                    XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                    AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                    SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, GPSelectButtonText, "\u25CD", 10, "Select");
                                    StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, GPStartButtonText, "\u25B7", 10, "Start");

                                    UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadB, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                    DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadY, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                    LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadX, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                    RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadA, null, "\u25B6", 10, "Right", typeof(VirtualPad));
                                }
                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "FreeChaF":
                                ViewModel.ShowXYZ = false;
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Visible;
                                GPZButton.Visibility = Visibility.Visible;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "O2EM":
                            case "o2em":
                            case "Magnavox Odyssey 2":
                                ViewModel.ShowXYZ = false;
                                //GPAButton.Visibility = Visibility.Collapsed;
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Visible;
                                //GPZButton.Visibility = Visibility.Visible;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "0", 10, "X");
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10, "Y");
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10, "B");
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadL, GPCButtonText, "3", 10, "A"); //JOYPAD_A is not required by this core so it's better to replace it with 3<>L2
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "1", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "2", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, GPL2ButtonText, "3", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, GPR2ButtonText, "4", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, GPL1R1ButtonText, "5", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, GPL2R2ButtonText, "6", 10, "R3", typeof(VirtualPadActions));

                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "Game MUsic":
                            case "GME":
                            case "Game Music Emu":
                                ViewModel.ShowXYZ = false;
                                ViewModel.RaisePropertyChanged(nameof(ViewModel.ShowXYZ));
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPBButtonText, "L", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPCButtonText, "R", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "X", 10, "L");
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPYButtonText, "Y", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPZButtonText, "Z", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "PokeMini":
                                ViewModel.ShowXYZ = false;
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Visible;
                                GPZButton.Visibility = Visibility.Visible;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "C", 10, "R");
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "S", 10, "L");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = YButtonInputType;
                                LRButtons[1] = ZButtonInputType;
                                LRButtonsObjects[0] = GPYButton;
                                LRButtonsObjects[1] = GPZButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "Stella":
                            case "Stella 2014":
                                ViewModel.ShowXYZ = false;
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Visible;
                                GPZButton.Visibility = Visibility.Visible;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPZButtonText, "L", 10, "R");
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPYButtonText, "R", 10, "L");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                LRButtons[0] = YButtonInputType;
                                LRButtons[1] = ZButtonInputType;
                                LRButtonsObjects[0] = GPYButton;
                                LRButtonsObjects[1] = GPZButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "2048":
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPBButton.Visibility = Visibility.Collapsed;
                                GPCButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "JNB":
                            case "Jump n Bump":
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                            case "TIC-80":
                                //GPAButton.Visibility = Visibility.Collapsed;
                                //GPXButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));


                                LRButtons[0] = XButtonInputType;
                                LRButtons[1] = AButtonInputType;
                                LRButtonsObjects[0] = GPXButton;
                                LRButtonsObjects[1] = GPAButton;

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

                                InputService.LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
                                  {
                                  { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
                                  { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
                                  { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
                                  { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
                                  { InputTypes.DeviceIdJoypadA, VirtualKey.C },
                                  { InputTypes.DeviceIdJoypadB, VirtualKey.X },
                                  { InputTypes.DeviceIdJoypadX, VirtualKey.S },
                                  { InputTypes.DeviceIdJoypadY, VirtualKey.D },
                                  { InputTypes.DeviceIdJoypadR, VirtualKey.Z },
                                  { InputTypes.DeviceIdJoypadL, VirtualKey.A },
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "LowResNX":
                            case "LowRes NX":
                            case "Lowres NX":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "PC 8000-8800":
                            case "Quasi88":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "REminiscence":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "TyrQuake":
                                //GPXButton.Visibility = Visibility.Collapsed;
                                //GPYButton.Visibility = Visibility.Collapsed;
                                //GPZButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "xrick":
                                GPXButton.Visibility = Visibility.Collapsed;
                                GPYButton.Visibility = Visibility.Collapsed;
                                GPZButton.Visibility = Visibility.Collapsed;
                                GPAButton.Visibility = Visibility.Collapsed;
                                GPBButton.Visibility = Visibility.Collapsed;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;



                            case "Gearcoleco":
                            case "GearColeco":
                                L3R3CustomTest = true;
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "1", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "2", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPCButtonText, "R", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "L", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "4", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "3", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "*", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "#", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, GPL2ButtonText, "6", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, GPR2ButtonText, "5", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, GPL2R2ButtonText, "8", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, GPL1R1ButtonText, "7", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "VICE x64":
                            case "VICE x64sc":
                            case "VICE xscpu64":
                            case "VICE x128":
                            case "VICE xcbm2":
                            case "VICE xcbm5x0":
                            case "VICE xpet":
                            case "VICE xplus4":
                            case "VICE xvic":
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "scummvm":
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "\u25C1", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, ".", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPBButtonText, "L", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPCButtonText, "R", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "E", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "5", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "DOSBox-pure":
                            case "DOSBox-svn":
                            case "DOSBox-core":
                            case "DOSBox":
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, GPL2ButtonText, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, GPR2ButtonText, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, GPL2R2ButtonText, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, GPL1R1ButtonText, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            case "NeoCD":
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "D", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "C", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "A", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "B", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;

                            default:
                                ZButtonInputTypeProperty = changeButtonAction(ZButtonInputType, InjectedInputTypes.DeviceIdJoypadX, GPZButtonText, "X", 10);
                                YButtonInputTypeProperty = changeButtonAction(YButtonInputType, InjectedInputTypes.DeviceIdJoypadY, GPYButtonText, "Y", 10);
                                BButtonInputTypeProperty = changeButtonAction(BButtonInputType, InjectedInputTypes.DeviceIdJoypadB, GPBButtonText, "B", 10);
                                CButtonInputTypeProperty = changeButtonAction(CButtonInputType, InjectedInputTypes.DeviceIdJoypadA, GPCButtonText, "A", 10);
                                XButtonInputTypeProperty = changeButtonAction(XButtonInputType, InjectedInputTypes.DeviceIdJoypadC, GPXButtonText, "L", 10, "L");
                                AButtonInputTypeProperty = changeButtonAction(AButtonInputType, InjectedInputTypes.DeviceIdJoypadZ, GPAButtonText, "R", 10, "R");

                                SelectButtonInputTypeProperty = changeButtonAction(SelectButtonInputType, InjectedInputTypes.DeviceIdJoypadSelect, GPSelectButtonText, "\u25F8", 10, "Select");
                                StartButtonInputTypeProperty = changeButtonAction(StartButtonInputType, InjectedInputTypes.DeviceIdJoypadStart, GPStartButtonText, "\u25B7", 10, "Start");

                                UpButtonInputTypeProperty = changeButtonAction(UpButtonInputType, InjectedInputTypes.DeviceIdJoypadUp, null, "\u25B2", 10, "Up", typeof(VirtualPad));
                                DownButtonInputTypeProperty = changeButtonAction(DownButtonInputType, InjectedInputTypes.DeviceIdJoypadDown, null, "\u25BC", 10, "Down", typeof(VirtualPad));
                                LeftButtonInputTypeProperty = changeButtonAction(LeftButtonInputType, InjectedInputTypes.DeviceIdJoypadLeft, null, "\u25C0", 10, "Left", typeof(VirtualPad));
                                RightButtonInputTypeProperty = changeButtonAction(RightButtonInputType, InjectedInputTypes.DeviceIdJoypadRight, null, "\u25B6", 10, "Right", typeof(VirtualPad));

                                //L2R2
                                LButtonInputTypeProperty = changeButtonAction(LButtonInputType, InjectedInputTypes.DeviceIdJoypadL, null, "L2", 10, "L2", typeof(VirtualPadActions));
                                RButtonInputTypeProperty = changeButtonAction(RButtonInputType, InjectedInputTypes.DeviceIdJoypadR, null, "R2", 10, "R2", typeof(VirtualPadActions));

                                //L3R3
                                L2ButtonInputTypeProperty = changeButtonAction(L2ButtonInputType, InjectedInputTypes.DeviceIdJoypadL2, null, "L3", 10, "L3", typeof(VirtualPadActions));
                                R2ButtonInputTypeProperty = changeButtonAction(R2ButtonInputType, InjectedInputTypes.DeviceIdJoypadR2, null, "R3", 10, "R3", typeof(VirtualPadActions));

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
                                  { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
                                  { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
                                  { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
                                  { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
                                  { InputTypes.DeviceIdJoypadSelect, VirtualKey.Shift },
                                  { InputTypes.DeviceIdJoypadStart, VirtualKey.Enter },
                                  };
                                break;
                        }
                        InputService.AssignKeyboardMapRules(CoreName, SystemName);
                        PrepareRightControls();
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception e)
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
            //ViewModel.InjectInputCommand(LButtonInputType);
        }

        private void GPR2Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            //ViewModel.InjectInputCommand(RButtonInputType);
        }

        bool L3R3CustomTest = false;
        private void UpdateL3R3(object sender, EventArgs args)
        {
            try
            {
                if (!L3R3CustomTest)
                {
                    if (PlatformService.UseL3R3InsteadOfX1X2)
                    {
                        GPL1R1ButtonText.Inlines.Clear();
                        GPL2R2ButtonText.Inlines.Clear();

                        GPL1R1ButtonText.Inlines.Add(new Windows.UI.Xaml.Documents.Run { Text = "R3" });
                        GPL2R2ButtonText.Inlines.Add(new Windows.UI.Xaml.Documents.Run { Text = "L3" });
                    }
                    else
                    {
                        GPL1R1ButtonText.Inlines.Clear();
                        GPL2R2ButtonText.Inlines.Clear();

                        GPL1R1ButtonText.Inlines.Add(new Windows.UI.Xaml.Documents.Run { Text = "1X" });
                        GPL2R2ButtonText.Inlines.Add(new Windows.UI.Xaml.Documents.Run { Text = "2X" });
                    }
                }
                GPL1R1Button.FontSize = 30;
                GPL2R2Button.FontSize = 30;
            }
            catch (Exception ex)
            {

            }
        }
        private void GPL1R1Button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
                if (PlatformService.UseL3R3InsteadOfX1X2)
                {
                    string ButtonKey = ((Button)sender).Name;
                    string ButtonTitle = "R3";
                    InjectedInputTypes ButtonAction = R2ButtonInputType;
                    ViewModel.AddActionButton(ButtonKey, ButtonTitle, ButtonAction, ViewModel.ActionsCustomDelay ? "+" : ",");
                    Button04Clicked();
                    ActiveButton(sender);
                }
                else
                {
                    ViewModel.ActionsCustomDelay = true;
                    AddNewActionButton(LRButtonsObjects[0]);
                    AddNewActionButton(LRButtonsObjects[1]);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void GPL1R1Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            if (PlatformService.UseL3R3InsteadOfX1X2)
            {
                //ViewModel.InjectInputCommand(R2ButtonInputType);
            }
            else
            {
                //ViewModel.InjectInputCommand(LRButtons[0]);
                //ViewModel.InjectInputCommand(LRButtons[1]);
            }
        }

        private void GPL2R2Button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
                if (PlatformService.UseL3R3InsteadOfX1X2)
                {
                    string ButtonKey = ((Button)sender).Name;
                    string ButtonTitle = "L3";
                    InjectedInputTypes ButtonAction = L2ButtonInputType;
                    ViewModel.AddActionButton(ButtonKey, ButtonTitle, ButtonAction, ViewModel.ActionsCustomDelay ? "+" : ",");
                    Button04Clicked();
                    ActiveButton(sender);
                }
                else
                {
                    ViewModel.ActionsCustomDelay = true;
                    AddNewActionButton(GPL2Button);
                    AddNewActionButton(GPR2Button);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void GPL2R2Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible || ViewModel.ButtonsCustomization) return;
            if (PlatformService.UseL3R3InsteadOfX1X2)
            {
                //ViewModel.InjectInputCommand(L2ButtonInputType);
            }
            else
            {
                //ViewModel.InjectInputCommand(LButtonInputType);
                //ViewModel.InjectInputCommand(RButtonInputType);
            }
        }
        private async void ActiveButton(object sender)
        {
            try
            {
                /*
                var ObjectName = (sender as Button).Name;
                (sender as RepeatButton).Background = (SolidColorBrush)Resources["ActiveButton"];
                await Task.Delay(10);
                (sender as RepeatButton).Background = (SolidColorBrush)Resources["InActive"];
                ReleaseActionButton(sender);
                */
            }
            catch (Exception ee)
            {

            }
        }
        private void ButtonControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
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
            }
            catch (Exception ex)
            {

            }
        }
        public void ReleaseActionButton(object sender)
        {
            try
            {
                /*
                string ButtonKey = ((RepeatButton)sender).Name;
                string ButtonTitle = ButtonsDictionary[ButtonKey].Keys.First();
                InjectedInputTypes ButtonAction = ButtonsDictionary[ButtonKey][ButtonTitle];
                lock (InputService.InjectedInputPressed)
                {
                    InputService.InjectedInputPressed.Remove((uint)ButtonAction);
                }
                */
            }
            catch (Exception ee)
            {

            }
        }
        private void GPAButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPAButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPBButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPBButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPCButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPCButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPXButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPXButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPYButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPYButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPZButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPZButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPStartButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPStartButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPSelectButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPSelectButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPL2Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPL2Button_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPR2Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPR2Button_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPL1R1Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPL1R1Button_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPL2R2Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }

        private void GPL2R2Button_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseActionButton(sender);
        }
    }
}
