using LibRetriX;
using RetriX.Shared.Services;
using RetriX.UWP.Pages;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Gaming.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace RetriX.UWP
{
    public sealed class InputService : IInputService
    {
        private const uint InjectedInputFramePermamence = 4;
        private const double GamepadAnalogDeadZoneSquareRadius = 0.0;

        public static IReadOnlyDictionary<InputTypes, VirtualKey> LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
        {
            { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
            { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
            { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
            { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
            { InputTypes.DeviceIdJoypadA, VirtualKey.Z },
            { InputTypes.DeviceIdJoypadB, VirtualKey.X },
            { InputTypes.DeviceIdJoypadX, VirtualKey.A },
            { InputTypes.DeviceIdJoypadY, VirtualKey.S },
            { InputTypes.DeviceIdJoypadL, VirtualKey.C },
            { InputTypes.DeviceIdJoypadR, VirtualKey.D },
            { InputTypes.DeviceIdJoypadL2, VirtualKey.L },
            { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
            { InputTypes.DeviceIdJoypadL3, VirtualKey.J },
            { InputTypes.DeviceIdJoypadR3, VirtualKey.W },
            { InputTypes.DeviceIdJoypadSelect, VirtualKey.P },
            { InputTypes.DeviceIdJoypadStart, VirtualKey.O },
        };

        private static readonly ISet<InputTypes> LibretroGamepadAnalogTypes = new HashSet<InputTypes>()
        {
            { InputTypes.DeviceIdAnalogLeftX },
            { InputTypes.DeviceIdAnalogLeftY },
            { InputTypes.DeviceIdAnalogRightX },
            { InputTypes.DeviceIdAnalogRightY },
        };

        public static Dictionary<string, GamepadButtons> GamepadMap = new Dictionary<string, GamepadButtons>()
        {
            {"B", GamepadButtons.B },
            {"A", GamepadButtons.A },
            {"Y", GamepadButtons.Y },
            {"X", GamepadButtons.X },
            {"LeftShoulder", GamepadButtons.LeftShoulder },
            {"RightShoulder", GamepadButtons.RightShoulder },
            {"Paddle1", GamepadButtons.Paddle1 },
            {"Paddle2", GamepadButtons.Paddle2 },
            {"LeftThumbstick", GamepadButtons.LeftThumbstick },
            {"RightThumbstick", GamepadButtons.RightThumbstick },
            {"View", GamepadButtons.View },
            {"Menu", GamepadButtons.Menu },
        };
        public static GamepadButtons[] GamepadMapArray = new GamepadButtons[]
        {
            GamepadButtons.B ,
            GamepadButtons.A ,
            GamepadButtons.Y ,
            GamepadButtons.X ,
            GamepadButtons.LeftShoulder ,
            GamepadButtons.RightShoulder ,
            GamepadButtons.Paddle1 ,
            GamepadButtons.Paddle2 ,
            GamepadButtons.LeftThumbstick ,
            GamepadButtons.RightThumbstick,
            GamepadButtons.View ,
            GamepadButtons.Menu
        };
        public static Dictionary<InjectedInputTypes, string> CurrentButtons = new Dictionary<InjectedInputTypes, string>();
        private static Dictionary<InputTypes, GamepadButtons> LibretroGamepadToWindowsGamepadButtonMapping = new Dictionary<InputTypes, GamepadButtons>()
        {
            { InputTypes.DeviceIdJoypadUp, GamepadButtons.DPadUp },
            { InputTypes.DeviceIdJoypadDown, GamepadButtons.DPadDown },
            { InputTypes.DeviceIdJoypadLeft, GamepadButtons.DPadLeft },
            { InputTypes.DeviceIdJoypadRight, GamepadButtons.DPadRight },
            { InputTypes.DeviceIdJoypadA, GamepadButtons.B },
            { InputTypes.DeviceIdJoypadB, GamepadButtons.A },
            { InputTypes.DeviceIdJoypadX, GamepadButtons.Y },
            { InputTypes.DeviceIdJoypadY, GamepadButtons.X },
            { InputTypes.DeviceIdJoypadL, GamepadButtons.LeftShoulder },
            { InputTypes.DeviceIdJoypadR, GamepadButtons.RightShoulder },
            { InputTypes.DeviceIdJoypadL2, GamepadButtons.Paddle1 },
            { InputTypes.DeviceIdJoypadR2, GamepadButtons.Paddle2 },
            { InputTypes.DeviceIdJoypadL3, GamepadButtons.LeftThumbstick },
            { InputTypes.DeviceIdJoypadR3, GamepadButtons.RightThumbstick },
            { InputTypes.DeviceIdJoypadSelect, GamepadButtons.View },
            { InputTypes.DeviceIdJoypadStart, GamepadButtons.Menu },
        };
        public static Dictionary<int, int> GamepadMapWithInput = new Dictionary<int, int>()
        {
            { (int)InputTypes.DeviceIdJoypadUp, (int)GamepadButtons.DPadUp },
            { (int)InputTypes.DeviceIdJoypadDown, (int)GamepadButtons.DPadDown },
            { (int)InputTypes.DeviceIdJoypadLeft, (int)GamepadButtons.DPadLeft },
            { (int)InputTypes.DeviceIdJoypadRight, (int)GamepadButtons.DPadRight },
            { (int)InputTypes.DeviceIdJoypadA, (int)GamepadButtons.B },
            { (int)InputTypes.DeviceIdJoypadB, (int)GamepadButtons.A },
            { (int)InputTypes.DeviceIdJoypadX, (int)GamepadButtons.Y },
            { (int)InputTypes.DeviceIdJoypadY, (int)GamepadButtons.X },
            { (int)InputTypes.DeviceIdJoypadL, (int)GamepadButtons.LeftShoulder },
            { (int)InputTypes.DeviceIdJoypadR, (int)GamepadButtons.RightShoulder },
            { (int)InputTypes.DeviceIdJoypadL2, (int)GamepadButtons.Paddle1 },
            { (int)InputTypes.DeviceIdJoypadR2, (int)GamepadButtons.Paddle2 },
            { (int)InputTypes.DeviceIdJoypadL3, (int)GamepadButtons.LeftThumbstick },
            { (int)InputTypes.DeviceIdJoypadR3, (int)GamepadButtons.RightThumbstick },
            { (int)InputTypes.DeviceIdJoypadSelect, (int)GamepadButtons.View },
            { (int)InputTypes.DeviceIdJoypadStart, (int)GamepadButtons.Menu },
        };
        public readonly static Dictionary<int, int> GamepadMapWithInputTemp = new Dictionary<int, int>()
        {
            { (int)InputTypes.DeviceIdJoypadUp, (int)GamepadButtons.DPadUp },
            { (int)InputTypes.DeviceIdJoypadDown, (int)GamepadButtons.DPadDown },
            { (int)InputTypes.DeviceIdJoypadLeft, (int)GamepadButtons.DPadLeft },
            { (int)InputTypes.DeviceIdJoypadRight, (int)GamepadButtons.DPadRight },
            { (int)InputTypes.DeviceIdJoypadA, (int)GamepadButtons.B },
            { (int)InputTypes.DeviceIdJoypadB, (int)GamepadButtons.A },
            { (int)InputTypes.DeviceIdJoypadX, (int)GamepadButtons.Y },
            { (int)InputTypes.DeviceIdJoypadY, (int)GamepadButtons.X },
            { (int)InputTypes.DeviceIdJoypadL, (int)GamepadButtons.LeftShoulder },
            { (int)InputTypes.DeviceIdJoypadR, (int)GamepadButtons.RightShoulder },
            { (int)InputTypes.DeviceIdJoypadL2, (int)GamepadButtons.Paddle1 },
            { (int)InputTypes.DeviceIdJoypadR2, (int)GamepadButtons.Paddle2 },
            { (int)InputTypes.DeviceIdJoypadL3, (int)GamepadButtons.LeftThumbstick },
            { (int)InputTypes.DeviceIdJoypadR3, (int)GamepadButtons.RightThumbstick },
            { (int)InputTypes.DeviceIdJoypadSelect, (int)GamepadButtons.View },
            { (int)InputTypes.DeviceIdJoypadStart, (int)GamepadButtons.Menu },
        };

        private readonly static Dictionary<InputTypes, GamepadButtons> LibretroGamepadToWindowsGamepadButtonMappingTemp = new Dictionary<InputTypes, GamepadButtons>()
        {
            { InputTypes.DeviceIdJoypadUp, GamepadButtons.DPadUp },
            { InputTypes.DeviceIdJoypadDown, GamepadButtons.DPadDown },
            { InputTypes.DeviceIdJoypadLeft, GamepadButtons.DPadLeft },
            { InputTypes.DeviceIdJoypadRight, GamepadButtons.DPadRight },
            { InputTypes.DeviceIdJoypadA, GamepadButtons.B },
            { InputTypes.DeviceIdJoypadB, GamepadButtons.A },
            { InputTypes.DeviceIdJoypadX, GamepadButtons.Y },
            { InputTypes.DeviceIdJoypadY, GamepadButtons.X },
            { InputTypes.DeviceIdJoypadL, GamepadButtons.LeftShoulder },
            { InputTypes.DeviceIdJoypadR, GamepadButtons.RightShoulder },
            { InputTypes.DeviceIdJoypadL2, GamepadButtons.Paddle1 },
            { InputTypes.DeviceIdJoypadR2, GamepadButtons.Paddle2 },
            { InputTypes.DeviceIdJoypadL3, GamepadButtons.LeftThumbstick },
            { InputTypes.DeviceIdJoypadR3, GamepadButtons.RightThumbstick },
            { InputTypes.DeviceIdJoypadSelect, GamepadButtons.View },
            { InputTypes.DeviceIdJoypadStart, GamepadButtons.Menu },
        };
        public static void ChangeGamepadButton(InjectedInputTypes injectedInputTypes, int ButtonIndex)
        {
            try
            {
                lock (LibretroGamepadToWindowsGamepadButtonMapping)
                {
                    GamepadButtons gamepadButtons = GamepadMapArray[ButtonIndex];
                    GamepadMapWithInput[(int)injectedInputTypes] = (int)gamepadButtons;
                    LibretroGamepadToWindowsGamepadButtonMapping[(InputTypes)injectedInputTypes] = gamepadButtons;
                }
            }
            catch (Exception e)
            {

            }
        }
        public static void ResetGamepadButtons()
        {
            try
            {
                lock (LibretroGamepadToWindowsGamepadButtonMapping)
                {
                    GamepadMapWithInput = GamepadMapWithInputTemp.ToDictionary(entry => entry.Key, entry => entry.Value);
                    LibretroGamepadToWindowsGamepadButtonMapping = LibretroGamepadToWindowsGamepadButtonMappingTemp.ToDictionary(entry => entry.Key, entry => entry.Value);
                }
            }
            catch (Exception ex)
            {
            }
        }
        public static void ReSyncGamepadButtons()
        {
            try
            {
                lock (LibretroGamepadToWindowsGamepadButtonMapping)
                {
                    foreach (var GamepadMapItem in GamepadMapWithInput)
                    {
                        LibretroGamepadToWindowsGamepadButtonMapping[(InputTypes)GamepadMapItem.Key] = (GamepadButtons)GamepadMapItem.Value;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        public static int GetGamePadSelectedIndex(InjectedInputTypes injectedInputTypes)
        {
            try
            {
                GamepadButtons gameButton;
                if (LibretroGamepadToWindowsGamepadButtonMapping.TryGetValue((InputTypes)injectedInputTypes, out gameButton))
                {
                    int index = 0;
                    foreach (var GamepadButtonItem in GamepadMap.Values)
                    {
                        if (GamepadButtonItem == gameButton)
                        {
                            return index;
                        }
                        index++;
                    }
                }
            }
            catch (Exception e)
            {

            }
            return -1;
        }

        private readonly Dictionary<InputTypes, uint> InjectedInput = new Dictionary<InputTypes, uint>();
        private readonly Dictionary<VirtualKey, bool> KeyStates = new Dictionary<VirtualKey, bool>();
        private readonly Dictionary<VirtualKey, bool> KeySnapshot = new Dictionary<VirtualKey, bool>();

        private readonly object GamepadReadingsLock = new object();
        private GamepadReading[] GamepadReadings;

        public InputService()
        {
            try
            {
                var window = CoreWindow.GetForCurrentThread();
                window.KeyDown -= WindowKeyDownHandler;
                window.KeyDown += WindowKeyDownHandler;
                window.KeyUp -= WindowKeyUpHandler;
                window.KeyUp += WindowKeyUpHandler;
            }
            catch (Exception ex)
            {

            }
        }

        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public Task DeinitAsync()
        {
            return Task.CompletedTask;
        }

        public void InjectInputPlayer1(InputTypes inputType)
        {
            try
            {
                lock (InjectedInput)
                {
                    InjectedInput[inputType] = InjectedInputFramePermamence;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void PollInput()
        {
            try
            {
                lock (KeyStates)
                    lock (KeySnapshot)
                    {
                        foreach (var i in KeyStates.Keys)
                        {
                            KeySnapshot[i] = KeyStates[i];
                        }
                    }

                lock (GamepadReadingsLock)
                {
                    GamepadReadings = Gamepad.Gamepads.Select(d => d.GetCurrentReading()).ToArray();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static double[] CanvasPointerPosition = new double[] { 0, 0 };
        public static double[] MousePointerPosition = new double[] { 0, 0 };


        public short GetInputState(uint port, InputTypes inputType)
        {
            try
            {
                if (!Enum.IsDefined(typeof(InputTypes), inputType))
                {
                    return 0;
                }
                lock (GamepadReadingsLock)
                {
                    if (LibretroGamepadAnalogTypes.Contains(inputType) && port < GamepadReadings.Length)
                    {
                        var reading = GamepadReadings[port];
                        switch (inputType)
                        {
                            case InputTypes.DeviceIdAnalogLeftX:
                                return ConvertAxisReading(reading.LeftThumbstickX, reading.LeftThumbstickY);
                            case InputTypes.DeviceIdAnalogLeftY:
                                return ConvertAxisReading(-reading.LeftThumbstickY, reading.LeftThumbstickX);
                            case InputTypes.DeviceIdAnalogRightX:
                                return ConvertAxisReading(reading.RightThumbstickX, reading.RightThumbstickY);
                            case InputTypes.DeviceIdAnalogRightY:
                                return ConvertAxisReading(-reading.RightThumbstickY, reading.RightThumbstickX);
                        }
                    }

                    //Detect Analog Input
                    var LeftThumbstickX = CanvasPointerPosition[0];
                    var LeftThumbstickY = CanvasPointerPosition[1];
                    if (PlatformService.RotatedRight)
                    {
                        LeftThumbstickX = -CanvasPointerPosition[1];
                        LeftThumbstickY = CanvasPointerPosition[0];
                    }
                    else if (PlatformService.RotatedLeft)
                    {
                        LeftThumbstickX = CanvasPointerPosition[1];
                        LeftThumbstickY = -CanvasPointerPosition[0];
                    }
                    var RightThumbstickX = 0;
                    var RightThumbstickY = 0;
                    short AnalogValue;
                    switch (inputType)
                    {
                        case InputTypes.DeviceIdAnalogLeftX:
                            AnalogValue = ConvertAxisReading(LeftThumbstickX, LeftThumbstickY);
                            return AnalogValue;
                        case InputTypes.DeviceIdAnalogLeftY:
                            AnalogValue = ConvertAxisReading(-LeftThumbstickY, LeftThumbstickX);
                            return AnalogValue;
                        case InputTypes.DeviceIdAnalogRightX:
                            AnalogValue = ConvertAxisReading(RightThumbstickX, RightThumbstickY);
                            return AnalogValue;
                        case InputTypes.DeviceIdAnalogRightY:
                            AnalogValue = ConvertAxisReading(-RightThumbstickY, RightThumbstickX);
                            return AnalogValue;
                    }

                    /*short PointerValue;
                    switch (inputType)
                    {
                        case InputTypes.DeviceIdPointerX:
                            PointerValue = ConvertAxisReading(LeftThumbstickX, LeftThumbstickY);
                            return PointerValue;
                        case InputTypes.DeviceIdPointerY:
                            PointerValue = ConvertAxisReading(-LeftThumbstickY, LeftThumbstickX);
                            return PointerValue;
                    }*/

                    try
                    {
                        var MouseX = MousePointerPosition[0];
                        var MouseY = MousePointerPosition[1];
                        short MouseValue;
                        switch (inputType)
                        {
                            case InputTypes.DeviceIdMouseX:
                            case InputTypes.DeviceIdPointerX:
                                //MouseValue = ConvertAxisReading(MouseX, MouseY);
                                MouseValue = (short)MouseX;
                                return MouseValue;
                            case InputTypes.DeviceIdMouseY:
                            case InputTypes.DeviceIdPointerY:
                                //MouseValue = ConvertAxisReading(-MouseY, MouseX);
                                MouseValue = (short)MouseY;
                                return MouseValue;
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    var output = false;
                    if (port == 0)
                    {
                        lock (KeySnapshot)
                        {
                            output = GetKeyboardKeyState(KeySnapshot, inputType);
                        }
                        output = output || GetInjectedInputState(inputType);
                    }

                    if (port < GamepadReadings.Length)
                    {
                        output = output || GetGamepadButtonState(GamepadReadings[port], inputType);
                    }

                    return output ? (short)1 : (short)0;
                }
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        private bool GetInjectedInputState(InputTypes inputType)
        {
            try
            {
                lock (InjectedInput)
                {
                    var output = InjectedInput.Keys.Contains(inputType);
                    if (output)
                    {
                        output = InjectedInput[inputType] > 0;
                        if (output)
                        {
                            InjectedInput[inputType] -= 1;
                        }
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        private static bool GetKeyboardKeyState(Dictionary<VirtualKey, bool> keyStates, InputTypes button)
        {
            try
            {
                if (!LibretroGamepadToWindowsGamepadButtonMapping.ContainsKey(button))
                {
                    return false;
                }

                var nativeKey = LibretroGamepadToKeyboardKeyMapping[button];
                var output = keyStates.ContainsKey(nativeKey) && keyStates[nativeKey];
                if (output)
                {
                    if (!PlatformService.XBoxModeState) PlatformService.XBoxModeState = true;
                }
                return output;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        private static bool GetGamepadButtonState(GamepadReading reading, InputTypes button)
        {
            try
            {
                if (!LibretroGamepadToWindowsGamepadButtonMapping.ContainsKey(button))
                {
                    return false;
                }

                var nativeButton = LibretroGamepadToWindowsGamepadButtonMapping[button];
                var output = (reading.Buttons & nativeButton) == nativeButton;
                if (output)
                {
                    if (!PlatformService.XBoxModeState) PlatformService.XBoxModeState = true;
                }
                return output;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public static short ConvertAxisReading(double mainValue, double transverseValue)
        {
            try
            {
                var isInDeadZone = (mainValue * mainValue) + (transverseValue * transverseValue) < GamepadAnalogDeadZoneSquareRadius;
                var output = isInDeadZone ? 0 : mainValue * short.MaxValue;
                return (short)output;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        private void WindowKeyUpHandler(CoreWindow sender, KeyEventArgs args)
        {
            try
            {
                var key = args.VirtualKey;
                if (Enum.IsDefined(typeof(VirtualKey), key))
                {
                    lock (KeyStates)
                    {
                        KeyStates[args.VirtualKey] = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void WindowKeyDownHandler(CoreWindow sender, KeyEventArgs args)
        {
            try
            {
                var key = args.VirtualKey;
                if (Enum.IsDefined(typeof(VirtualKey), key))
                {
                    lock (KeyStates)
                    {
                        KeyStates[args.VirtualKey] = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }



        public static Dictionary<string, CompositeTransform> ButtonsCompositeTransform = new Dictionary<string, CompositeTransform>();
        public static Dictionary<string, double[]> ButtonsPositions = new Dictionary<string, double[]>();
        public static void ResetButtonsPositions()
        {
            try
            {
                foreach (var ButtonsNameItem in ButtonsCompositeTransform.Keys)
                {
                    SaveButtonPosition(ButtonsNameItem, 0.0, 0.0, true);
                }
                RetriveButtonsPositions();
            }
            catch (Exception ex)
            {

            }
        }
        public static void SetButtonPosition(string buttonName)
        {
            try
            {
                double[] testValue;
                if (ButtonsPositions.TryGetValue(buttonName, out testValue))
                {
                    double BX = testValue[0];
                    double BY = testValue[1];
                    ButtonsCompositeTransform[buttonName].TranslateX = BX;
                    ButtonsCompositeTransform[buttonName].TranslateY = BY;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static void SaveButtonPosition(string buttonName, double buttonX, double buttonY, bool resetData = false)
        {
            try
            {
                double[] testValue;
                if (ButtonsPositions.TryGetValue(buttonName, out testValue))
                {
                    if (!resetData)
                    {
                        ButtonsPositions[buttonName][0] += buttonX;
                        ButtonsPositions[buttonName][1] += buttonY;
                    }
                    else
                    {
                        ButtonsPositions[buttonName][0] = buttonX;
                        ButtonsPositions[buttonName][1] = buttonY;
                    }
                }
                else
                {
                    ButtonsPositions.Add(buttonName, new double[] { buttonX, buttonY });
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void RetriveButtonsPositions()
        {
            try
            {
                foreach (var ButtonsNameItem in ButtonsCompositeTransform.Keys)
                {
                    SetButtonPosition(ButtonsNameItem);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
