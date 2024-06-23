using LibRetriX;
using LibRetriX.RetroBindings;
using Newtonsoft.Json;
using RetriX.Shared.Services;
using RetriX.UWP.Controls;
using RetriX.UWP.Pages;
using RetriX.UWP.RetroBindings.Structs;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Gaming.Input;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace RetriX.UWP
{
    public sealed class InputService
    {
        public static uint InjectedInputFramePermamence = 4;
        private const double GamepadAnalogDeadZoneSquareRadius = 0.0;

        public static bool CustomRulesLoaded = false;
        public static List<KeyboardMapRule> KeyboardMapRulesList = new List<KeyboardMapRule>();
        public static void AssignKeyboardMapRules(string CoreName, string SystemName)
        {
            try
            {
                CustomRulesLoaded = false;
                if (KeyboardMapRulesList == null)
                {
                    KeyboardMapRulesList = new List<KeyboardMapRule>();
                }
                KeyboardMapRulesList.Clear();
                foreach (var key in LibretroGamepadToKeyboardKeyMapping.Values)
                {
                    new KeyboardMapRule(ref KeyboardMapRulesList, key, key);
                }
                //Finally check for any possible customizations
                //This process it can be in background no need to wait it
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var customRules = await RetriveKeyboardRules(CoreName, SystemName);
                        if (customRules != null)
                        {
                            KeyboardMapRulesList = customRules;
                            CustomRulesLoaded = true;
                        }
                        else
                        {
                            //Check for global rules
                            var globalRules = await RetriveKeyboardRules("all", "systems");
                            if (globalRules != null)
                            {
                                KeyboardMapRulesList = globalRules;
                                CustomRulesLoaded = true;
                            }
                        }
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
        public static VirtualKey GetKeyByRules(VirtualKey virtualKey)
        {
            var vKey = virtualKey;
            try
            {
                foreach (var kItem in KeyboardMapRulesList)
                {
                    if (((VirtualKey)kItem.OriginalKey).Equals(virtualKey))
                    {
                        vKey = (VirtualKey)kItem.NewKey;
                    }
                }
            }
            catch (Exception x)
            {

            }

            return vKey;
        }
        public static async Task ResetKeyboardRules(string CoreName, string SystemName)
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("KeyboardRules");

                if (localFolder != null)
                {
                    var targetFileName = $"{CoreName}_{SystemName}_KeyboardRules.rtk";
                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync(targetFileName);

                    if (targetFileTest != null)
                    {
                        await targetFileTest.DeleteAsync();
                        AssignKeyboardMapRules(CoreName, SystemName);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static async Task<List<KeyboardMapRule>> RetriveKeyboardRules(string CoreName, string SystemName)
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("KeyboardRules");

                if (localFolder != null)
                {
                    var targetFileName = $"{CoreName}_{SystemName}_KeyboardRules.rtk";
                    var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync(targetFileName);

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
                        var dictionaryList = JsonConvert.DeserializeObject<List<KeyboardMapRule>>(ActionsFileContent);
                        if (dictionaryList != null)
                        {
                            return dictionaryList;
                        }
                    }
                }

            }
            catch (Exception e)
            {
            }
            return null;
        }

        public static async Task SaveKeyboardRules(string CoreName, string SystemName)
        {
            try
            {
                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(KeyboardMapRulesList));
                if (dictionaryListBytes != null)
                {
                    var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("KeyboardRules", CreationCollisionOption.OpenIfExists);
                    var targetFileName = $"{CoreName}_{SystemName}_KeyboardRules.rtk";

                    var targetFile = await localFolder.CreateFileAsync(targetFileName, CreationCollisionOption.ReplaceExisting);
                    using (var outStream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var stream = outStream.AsStreamForWrite();
                        await stream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                        await stream.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

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
            { InputTypes.DeviceIdJoypadL2, VirtualKey.Q },
            { InputTypes.DeviceIdJoypadR2, VirtualKey.W },
            { InputTypes.DeviceIdJoypadL3, VirtualKey.E },
            { InputTypes.DeviceIdJoypadR3, VirtualKey.R },
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
            {"Up", GamepadButtons.DPadUp },
            {"Down", GamepadButtons.DPadDown },
            {"Left", GamepadButtons.DPadLeft },
            {"Right", GamepadButtons.DPadRight },
        };

        //public static Dictionary<string, string> Descriptor = new Dictionary<string, string>();
        public static List<Dictionary<string, string>> Descriptors = new List<Dictionary<string, string>>()
        {
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
        };

        public static string[] libretro_btn_desc = new string[]{
                                  "B", "Y", "Select", "Start",
                                  "Up", "Down", "Left", "Right",
                                  "A", "X",
                                  "L", "R", "L2", "R2", "L3", "R3",
                                  };
        public static int supportedPorts
        {
            get
            {
                try
                {
                    //Count keyboard as port (if available)
                    var keyboardKeyState = isKeyboardAvailable();
                    var ports = keyboardKeyState ? 1 : 0;

                    //Count Touch as port (if available)
                    if (isTouchAvailable())
                    {
                        ports++;
                    }
                    if (GamepadReadings != null && GamepadReadings.Length > 0)
                    {
                        ports += GamepadReadings.Length;
                    }

                    //At least give 1 port supported, otherwise the input will be in-active
                    return ports == 0 ? 1 : ports;
                }
                catch (Exception ex)
                {
                    return 1;
                }
            }
        }
        public static bool isKeyboardAvailable()
        {
            try
            {
                if (PlatformService.DetectInputs)
                {
                    KeyboardCapabilities keyboardCapabilities = new Windows.Devices.Input.KeyboardCapabilities();
                    var keyboardKeyState = keyboardCapabilities.KeyboardPresent != 0 ? true : false;
                    return keyboardKeyState;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return true;
            }
        }
        public static bool isTouchAvailable()
        {
            try
            {
                if (PlatformService.DetectInputs)
                {
                    TouchCapabilities touchCapabilities = new Windows.Devices.Input.TouchCapabilities();
                    var touchState = touchCapabilities.TouchPresent != 0 ? true : false;
                    return touchState;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return true;
            }
        }
        public static bool isMouseAvailable()
        {
            try
            {
                if (PlatformService.DetectInputs)
                {
                    MouseCapabilities mouseCapabilities = new Windows.Devices.Input.MouseCapabilities();
                    var mouseState = mouseCapabilities.MousePresent != 0 ? true : false;
                    return mouseState;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return true;
            }
        }
        public static void UpdateInputDescriptionByIndex(retro_input_descriptor descriptor)
        {
            var port = (int)descriptor.Port;
            if (port + 1 > supportedPorts)
            {
                return;
            }
            var index = descriptor.Id;
            if (descriptor.Device == Constants.RETRO_DEVICE_ANALOG)
            {
                return;
            }
            if (index < libretro_btn_desc.Length)
            {
                var button = libretro_btn_desc[index];
                var testValue = "";
                if (!Descriptors[port].TryGetValue(button, out testValue))
                {
                    Descriptors[port].Add(button, descriptor.Description);
                }
                else
                {
                    Descriptors[port][button] = descriptor.Description;
                }
                GamePlayerView.systemControlsChanged = true;
            }
        }
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
            GamepadButtons.Menu,
            GamepadButtons.DPadUp,
            GamepadButtons.DPadDown,
            GamepadButtons.DPadLeft,
            GamepadButtons.DPadRight,
        };

        public static List<Dictionary<InjectedInputTypes, string>> CurrentButtonsMaps = new List<Dictionary<InjectedInputTypes, string>>()
        {
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
            new Dictionary<InjectedInputTypes, string>(),
        };

        public static List<Dictionary<InputTypes, GamepadButtons>> LibretroGamepadToWindowsGamepadButtonMappings = new List<Dictionary<InputTypes, GamepadButtons>>()
        {
            new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },
        };

        public static List<Dictionary<InputTypes, GamepadButtons>> LibretroGamepadToWindowsGamepadButtonMappingsTemp = new List<Dictionary<InputTypes, GamepadButtons>>()
        {
            new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },new Dictionary<InputTypes, GamepadButtons>()
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
        },
        };

        public static List<Dictionary<int, int>> GamepadMapWithInputs = new List<Dictionary<int, int>>()
        {
            new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        },
        };
        public static List<Dictionary<int, int>> GamepadMapWithInputsTemp = new List<Dictionary<int, int>>()
        {
            new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        }, new Dictionary<int, int>()
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
        },
        };


        public static void ChangeGamepadButton(int port, InjectedInputTypes injectedInputTypes, int ButtonIndex)
        {
            try
            {
                lock (LibretroGamepadToWindowsGamepadButtonMappings[port])
                {
                    GamepadButtons gamepadButtons = GamepadMapArray[ButtonIndex];
                    GamepadMapWithInputs[port][(int)injectedInputTypes] = (int)gamepadButtons;
                    LibretroGamepadToWindowsGamepadButtonMappings[port][(InputTypes)injectedInputTypes] = gamepadButtons;
                }
            }
            catch (Exception e)
            {

            }
        }
        public static void ResetGamepadButtons(int port)
        {
            try
            {
                lock (LibretroGamepadToWindowsGamepadButtonMappings[port])
                {
                    GamepadMapWithInputs[port] = GamepadMapWithInputsTemp[port].ToDictionary(entry => entry.Key, entry => entry.Value);
                    LibretroGamepadToWindowsGamepadButtonMappings[port] = LibretroGamepadToWindowsGamepadButtonMappingsTemp[port].ToDictionary(entry => entry.Key, entry => entry.Value);
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
                for (int i = 0; i < 12; i++)
                {
                    lock (LibretroGamepadToWindowsGamepadButtonMappings[i])
                    {
                        foreach (var GamepadMapItem in GamepadMapWithInputs[i])
                        {
                            LibretroGamepadToWindowsGamepadButtonMappings[i][(InputTypes)GamepadMapItem.Key] = (GamepadButtons)GamepadMapItem.Value;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        public static int GetGamePadSelectedIndex(int port, InjectedInputTypes injectedInputTypes)
        {
            try
            {
                GamepadButtons gameButton;
                if (LibretroGamepadToWindowsGamepadButtonMappings[port].TryGetValue((InputTypes)injectedInputTypes, out gameButton))
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

        private readonly Dictionary<InputTypes, long> InjectedInput = new Dictionary<InputTypes, long>();
        public readonly Dictionary<uint, long> InjectedKeys = new Dictionary<uint, long>();
        private readonly Dictionary<VirtualKey, bool> KeyStates = new Dictionary<VirtualKey, bool>();
        private readonly Dictionary<VirtualKey, bool> KeySnapshot = new Dictionary<VirtualKey, bool>();

        private readonly object GamepadReadingsLock = new object();
        public static GamepadReading[] GamepadReadings;

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

        public void InjectInputPlayer1(InputTypes inputType, bool forceState)
        {
            try
            {
                //This only case case this function will be called is:
                //-Actions
                //Custom control like UpLeft
                //lock (InjectedInputPressed)
                var keys = new List<List<TouchControl>>()
                {
                    RightControls, LeftControls
                };
                for (var k = 0; k < keys.Count; k++)
                {
                    var InjectedInputPressed = keys[k];
                    for (var i = 0; i < InjectedInputPressed.Count; i++)
                    {
                        var tItem = InjectedInputPressed[i];
                        if (((uint)tItem.InjectedInput).Equals((uint)inputType))
                        {
                            tItem.ForceState = forceState;
                        }
                    }
                }
                //After 3.0.22, there will be direct check on the button instead of this way
                /*lock (InjectedInput)
                {
                    if (!InjectedInput.ContainsKey(inputType))
                    {
                        InjectedInput.Add(inputType, 0);
                    }
                    InjectedInput[inputType] += 1;
                }
                lock (InjectedInputPressed)
                {
                    if (!InjectedInputPressed.Contains((uint)inputType))
                    {
                        InjectedInputPressed.Add((uint)inputType);
                    }
                }*/
            }
            catch (Exception ex)
            {

            }
        }
        public void InjectInputKey(uint key)
        {
            try
            {
                if (!PlatformService.DPadActive)
                {
                    lock (InjectedKeys)
                    {
                        if (!InjectedKeys.ContainsKey(key))
                        {
                            InjectedKeys.Add(key, 0);
                        }
                        InjectedKeys[key] += 1;
                    }
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
        public static double[] MousePointerViewportPosition = new double[] { 0, 0 };
        public static double[] CanvasSize = new double[] { 0, 0 };
        public static bool PointerPressed = false;
        public static List<uint> vKeyboardKeys = new List<uint>();
        public static List<uint> RequestedKeys = new List<uint>();
        public static List<PortEntry> ControllersPortsMap = new List<PortEntry>()
        {
            {new PortEntry(0, new List<string>{ "Touch", "Keyboard", "GamePad 1" }, "Port 1", "port0")},
            {new PortEntry(1, new List<string>{ "GamePad 2" }, "Port 2", "port1")},
            {new PortEntry(2, new List<string>{ "GamePad 3" }, "Port 3", "port2")},
            {new PortEntry(3, new List<string>{ "GamePad 4" }, "Port 4", "port3")},
            {new PortEntry(4, new List<string>{ "GamePad 5" }, "Port 5", "port4")},
            {new PortEntry(5, new List<string>{ "GamePad 6" }, "Port 6", "port5")},
            {new PortEntry(6, new List<string>{ "GamePad 7" }, "Port 7", "port6")},
            {new PortEntry(7, new List<string>{ "GamePad 8" }, "Port 8", "port7")},
            {new PortEntry(8, new List<string>{ "GamePad 9" }, "Port 9", "port8")},
            {new PortEntry(9, new List<string>{ "GamePad 10" }, "Port 10", "port9")},
            {new PortEntry(10, new List<string>{ "GamePad 11" }, "Port 11", "port10")},
            {new PortEntry(11, new List<string>{ "GamePad 12" }, "Port 12", "port11")},
        };
        public static Dictionary<string, int> ControllersList = new Dictionary<string, int>()
        {
            { "Touch", Constants.RETRO_DEVICE_POINTER },
            { "Keyboard", Constants.RETRO_DEVICE_KEYBOARD },
            { "GamePad 1", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 2", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 3", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 4", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 5", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 6", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 7", Constants.RETRO_DEVICE_ANALOG},
            { "GamePad 8", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 9", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 10", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 11", Constants.RETRO_DEVICE_ANALOG },
            { "GamePad 12", Constants.RETRO_DEVICE_ANALOG },
        };

        public uint[] getDevicePortByName(string name)
        {
            var port = new List<uint>();

            try
            {
                foreach (var pItem in ControllersPortsMap)
                {
                    if (pItem.controllers.Contains(name))
                    {
                        port.Add((uint)pItem.port);
                    }
                }
            }
            catch (Exception ex)
            {
                port.Add(0);
            }

            return port.ToArray();
        }
        public uint[] getGamePadPort(int index)
        {
            var port = new List<uint>();
            try
            {
                foreach (var pItem in ControllersPortsMap)
                {
                    var name = $"GamePad {index + 1}";
                    if (pItem.controllers.Contains(name))
                    {
                        port.Add((uint)pItem.port);
                    }
                }
            }
            catch (Exception ex)
            {
                port.Add(0);
            }
            return port.ToArray();
        }
        public int gamePadIndexByName(string name)
        {
            switch (name)
            {
                case "GamePad 1":
                    return 0;
                case "GamePad 2":
                    return 1;
                case "GamePad 3":
                    return 2;
                case "GamePad 4":
                    return 3;
                case "GamePad 5":
                    return 4;
                case "GamePad 6":
                    return 5;
                case "GamePad 7":
                    return 6;
                case "GamePad 8":
                    return 7;
                case "GamePad 9":
                    return 8;
                case "GamePad 10":
                    return 9;
                case "GamePad 11":
                    return 10;
                case "GamePad 12":
                    return 11;
            }

            return 0;
        }

        public static bool CoreRequestingMousePosition = false;
        public short GetInputState(uint device, uint port, InputTypes inputType)
        {
            try
            {
                switch (device)
                {
                    case Constants.RETRO_DEVICE_KEYBOARD:
                        {
                            if (!PlatformService.CoreReadingRetroKeyboard)
                            {
                                PlatformService.CoreReadingRetroKeyboard = true;
                                PlatformService.useNativeKeyboard = PlatformService.useNativeKeyboardByDefault;
                            }
                            if (!RequestedKeys.Contains((uint)inputType))
                            {
                                RequestedKeys.Add((uint)inputType);
                            }
                            if (PlatformService.PressedKeys.Contains((VirtualKey)inputType))
                            {
                                return 1;
                            }
                            else
                            {
                                if (vKeyboardKeys.Contains((uint)inputType))
                                {
                                    return 1;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                        }

                    default:
                        {
                            {
                                //The best way to inject virtual keyboard keys is here
                                if (PlatformService.KeyboardEvent != null && vKeyboardKeys.Count > 0)
                                {
                                    foreach (var key in vKeyboardKeys)
                                    {
                                        PlatformService.InvokeEkeyboardEvent(true, (VirtualKey)key);
                                    }
                                    ResolveTouchKeyboardState();
                                }
                            }
                            if (!Enum.IsDefined(typeof(InputTypes), inputType))
                            {
                                return 0;
                            }
                            if (PlatformService.XBoxMenuActive || PlatformService.KeyboardMapVisible || PlatformService.ScaleMenuActive || PlatformService.SavesListActive || PlatformService.ShortcutsVisible || PlatformService.EffectsActive || PlatformService.CoreOptionsActive || PlatformService.ControlsActive || PlatformService.LogsVisibile || PlatformService.ActionVisibile || (PlatformService.KeyboardVisibleState && PlatformService.DPadActive))
                            {
                                return 0;
                            }

                            lock (GamepadReadingsLock)
                            {
                                if (PlatformService.DPadActive)
                                {
                                    if (LibretroGamepadAnalogTypes.Contains(inputType) && port < GamepadReadings.Length)
                                    {
                                        var reading = GamepadReadings[port];
                                        //Detect Analog Input
                                        switch (inputType)
                                        {
                                            case InputTypes.DeviceIdAnalogLeftX:
                                                if (PlatformService.MouseSate)
                                                {
                                                    //When mouse state is active this should report MouseX
                                                    var X = reading.LeftThumbstickX;
                                                    MousePointerPosition[0] = X * 4;
                                                }
                                                else
                                                {
                                                    return ConvertAxisReading(reading.LeftThumbstickX, reading.LeftThumbstickY);
                                                }
                                                break;
                                            case InputTypes.DeviceIdAnalogLeftY:
                                                if (PlatformService.MouseSate)
                                                {
                                                    //When mouse state is active this should report MouseY
                                                    var Y = reading.LeftThumbstickY;
                                                    MousePointerPosition[1] = -Y * 4;
                                                }
                                                else
                                                {
                                                    return ConvertAxisReading(-reading.LeftThumbstickY, reading.LeftThumbstickX);
                                                }
                                                break;
                                            case InputTypes.DeviceIdAnalogRightX:
                                                return ConvertAxisReading(reading.RightThumbstickX, reading.RightThumbstickY);
                                            case InputTypes.DeviceIdAnalogRightY:
                                                return ConvertAxisReading(-reading.RightThumbstickY, reading.RightThumbstickX);
                                        }
                                    }
                                }


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

                                if (PlatformService.ThumbstickSate)
                                {
                                    switch (inputType)
                                    {
                                        case InputTypes.DeviceIdAnalogRightX:
                                            return ConvertAxisReading(LeftThumbstickX, LeftThumbstickY);
                                        case InputTypes.DeviceIdAnalogRightY:
                                            return ConvertAxisReading(-LeftThumbstickY, LeftThumbstickX);
                                    }
                                }
                                else
                                {
                                    short AnalogValue;
                                    switch (inputType)
                                    {
                                        case InputTypes.DeviceIdAnalogLeftX:
                                            AnalogValue = ConvertAxisReading(LeftThumbstickX, LeftThumbstickY);
                                            return AnalogValue;
                                        case InputTypes.DeviceIdAnalogLeftY:
                                            AnalogValue = ConvertAxisReading(-LeftThumbstickY, LeftThumbstickX);
                                            return AnalogValue;
                                    }
                                }

                                try
                                {
                                    var MouseX = MousePointerPosition[0];
                                    var MouseY = MousePointerPosition[1];
                                    switch (inputType)
                                    {
                                        case InputTypes.DeviceIdMouseX:
                                            CoreRequestingMousePosition = true;
                                            return (short)MouseX;
                                        case InputTypes.DeviceIdMouseY:
                                            CoreRequestingMousePosition = true;
                                            return (short)MouseY;
                                    }

                                    if (PointerPressed)
                                    {
                                        switch (inputType)
                                        {
                                            case InputTypes.DeviceIdPointerX:
                                            case InputTypes.DeviceIdPointerY:
                                                {
                                                    video_viewport vp = new video_viewport();

                                                    /* convert from event coordinates to core and screen coordinates */
                                                    vp.x = 0;
                                                    vp.y = 0;
                                                    vp.width = (uint)(CanvasSize[0] * (PlatformService.ScreenScale / 100));
                                                    vp.height = (uint)(CanvasSize[1] * (PlatformService.ScreenScale / 100));
                                                    vp.full_width = (uint)Math.Round((CanvasSize[0] * (PlatformService.ScreenScale / 100)));
                                                    vp.full_height = (uint)Math.Round((CanvasSize[1] * (PlatformService.ScreenScale / 100)));

                                                    double x = MousePointerViewportPosition[0];
                                                    double y = MousePointerViewportPosition[1];
                                                    int res_x = 0;
                                                    int res_y = 0;
                                                    int res_screen_x = 0;
                                                    int res_screen_y = 0;
                                                    float dpi = PlatformService.DPI;

                                                    //Dips conversion caused issue on touch screens
                                                    //It works fine without it
                                                    var xDip = (double)ConvertDipsToPixels((float)x, dpi);
                                                    var yDip = (double)ConvertDipsToPixels((float)y, dpi);

                                                    if (video_driver_translate_coord_viewport(ref vp, (int)Math.Round(xDip), (int)Math.Round(yDip), ref res_x, ref res_y, ref res_screen_x, ref res_screen_y))
                                                    {
                                                        InputService.MousePointerPosition[0] = res_x;
                                                        InputService.MousePointerPosition[1] = res_y;
                                                        switch (inputType)
                                                        {
                                                            case InputTypes.DeviceIdPointerX:
                                                                return (short)(res_x);
                                                            case InputTypes.DeviceIdPointerY:
                                                                PointerPressed = false;
                                                                return (short)(res_y);
                                                        }
                                                    }

                                                }
                                                break;

                                            case InputTypes.DeviceIdPointerPressed:
                                                var pressedState = PointerPressed ? 1 : 0;
                                                return (short)pressedState;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    PointerPressed = false;
                                }

                                var output = false;
                                var keyboardPort = getDevicePortByName("Keyboard");
                                if (keyboardPort.Contains(port))
                                {
                                    lock (KeySnapshot)
                                    {
                                        output = GetKeyboardKeyState((int)port, KeySnapshot, inputType);
                                    }
                                }

                                var touchPort = getDevicePortByName("Touch");
                                if (touchPort.Contains(port))
                                {
                                    output = output || GetInjectedInputState(inputType);
                                }

                                if (GamepadReadings != null && GamepadReadings.Length > 0)
                                {
                                    for (int i = 0; i < GamepadReadings.Length; i++)
                                    {
                                        var gamePadPort = getGamePadPort(i);
                                        if (gamePadPort.Contains(port))
                                        {
                                            output = output || GetGamepadButtonState((int)port, GamepadReadings[i], inputType);
                                            break;
                                        }
                                    }
                                }

                                return output ? (short)1 : (short)0;
                            }

                        }
                }
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        static bool video_driver_translate_coord_viewport(ref video_viewport vp,
      int mouse_x, int mouse_y,
      ref int res_x, ref int res_y,
      ref int res_screen_x, ref int res_screen_y)
        {
            int norm_vp_width = (int)vp.width;
            int norm_vp_height = (int)vp.height;
            int norm_full_vp_width = (int)vp.full_width;
            int norm_full_vp_height = (int)vp.full_height;
            int scaled_screen_x = -0x8000; /* OOB */
            int scaled_screen_y = -0x8000; /* OOB */
            int scaled_x = -0x8000; /* OOB */
            int scaled_y = -0x8000; /* OOB */
            if (norm_vp_width <= 0 ||
                norm_vp_height <= 0 ||
                norm_full_vp_width <= 0 ||
                norm_full_vp_height <= 0)
                return false;

            if (mouse_x >= 0 && mouse_x <= norm_full_vp_width)
                scaled_screen_x = ((2 * mouse_x * 0x7fff)
                      / norm_full_vp_width) - 0x7fff;

            if (mouse_y >= 0 && mouse_y <= norm_full_vp_height)
                scaled_screen_y = ((2 * mouse_y * 0x7fff)
                      / norm_full_vp_height) - 0x7fff;

            mouse_x -= vp.x;
            mouse_y -= vp.y;

            if (mouse_x >= 0 && mouse_x <= norm_vp_width)
                scaled_x = ((2 * mouse_x * 0x7fff)
                      / norm_vp_width) - 0x7fff;
            else
                scaled_x = -0x8000; /* OOB */

            if (mouse_y >= 0 && mouse_y <= norm_vp_height)
                scaled_y = ((2 * mouse_y * 0x7fff)
                      / norm_vp_height) - 0x7fff;

            res_x = scaled_x;
            res_y = scaled_y;
            res_screen_x = scaled_screen_x;
            res_screen_y = scaled_screen_y;
            return true;
        }
        public static float ConvertDipsToPixels(float dips, float dpi)
        {
            float dipsPerInch = 96.0f;
            return (float)Math.Floor(dips * dpi / dipsPerInch + 0.5f);
        }

        public static List<TouchControl> RightControls = new List<TouchControl>();
        public static List<TouchControl> LeftControls = new List<TouchControl>();

        //This variable not in use anymore
        public static bool useTouchPadReleaseResolver = true;
        private bool GetInjectedInputState(InputTypes inputType)
        {
            var state = false;
            try
            {
                var keys = new List<List<TouchControl>>()
                {
                    RightControls, LeftControls
                };
                for (var k = 0; k < keys.Count; k++)
                {
                    var InjectedInputPressed = keys[k];
                    for (var i = 0; i < InjectedInputPressed.Count; i++)
                    {
                        var tItem = InjectedInputPressed[i];
                        if (((uint)tItem.InjectedInput).Equals((uint)inputType))
                        {
                            if (tItem.isActive())
                            {
                                if (tItem.ForceState)
                                {
                                    state = true;
                                    break;
                                }
                                try
                                {
                                    state = tItem.GetState();
                                }
                                catch (Exception ex)
                                {

                                }
                                if (state)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                //The code below is not active anymore due accuracy issue
                /*
                var state = InjectedInputPressed.Contains((uint)inputType);
                lock (InjectedInput)
                {
                    if (state)
                    {
                        InjectedInput[inputType] -= 1;

                        if (useTouchPadReleaseResolver && InjectedInput[inputType] < -InjectedInputFramePermamence)
                        {
                            lock (InjectedInputPressed)
                            {
                                InjectedInputPressed.Remove((uint)inputType);
                            }
                            InjectedInput[inputType] = 0;
                        }
                    }
                }*/
                return state;
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        private void ResolveTouchKeyboardState()
        {
            try
            {
                if (!PlatformService.DPadActive)
                {
                    for (int i = 0; i < vKeyboardKeys.Count; i++)
                    {
                        var key = vKeyboardKeys[i];
                        lock (InjectedKeys)
                        {
                            InjectedKeys[key] -= 1;

                            if (useTouchPadReleaseResolver && InjectedKeys[key] < -InjectedInputFramePermamence * 100)
                            {
                                lock (vKeyboardKeys)
                                {
                                    vKeyboardKeys.Remove(key);
                                    GamePlayerView.SwitchKeyOff(key);
                                }
                                InjectedKeys[key] = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static bool GetKeyboardKeyState(int port, Dictionary<VirtualKey, bool> keyStates, InputTypes button)
        {
            try
            {
                if (PlatformService.useNativeKeyboard || !LibretroGamepadToWindowsGamepadButtonMappings[port].ContainsKey(button))
                {
                    return false;
                }

                var nativeKey = LibretroGamepadToKeyboardKeyMapping[button];
                try
                {
                    nativeKey = GetKeyByRules(nativeKey);
                }
                catch (Exception ex)
                {

                }
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

        private static bool GetGamepadButtonState(int port, GamepadReading reading, InputTypes button)
        {
            try
            {
                if (!LibretroGamepadToWindowsGamepadButtonMappings[port].ContainsKey(button))
                {
                    return false;
                }

                var nativeButton = LibretroGamepadToWindowsGamepadButtonMappings[port][button];
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
    public class PortEntry
    {
        public List<string> controllers = new List<string>();
        public string name = "";
        public string key = "";
        public int port = 0;
        public PortEntry(int p, List<string> ks, string n, string k)
        {
            controllers = ks;
            name = n;
            key = k;
            port = p;
        }
    }

    public class KeyboardMapRule
    {
        public string KeyName;
        public uint OriginalKey;
        public uint NewKey;

        public KeyboardMapRule(ref List<KeyboardMapRule> keyboardMapRules, VirtualKey ok, VirtualKey nk)
        {
            try
            {
                if (keyboardMapRules != null)
                {
                    OriginalKey = (uint)ok;
                    NewKey = (uint)nk;
                    KeyName = ok.ToString();

                    keyboardMapRules.Add(this);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
    public class TouchControl
    {
        public Func<bool> GetState;
        public bool ForceState = false;
        public InjectedInputTypes InjectedInput;
        public string ActiveConditionVariable = "";

        public bool isActive()
        {
            var state = true;
            try
            {
                if (ActiveConditionVariable != null && ActiveConditionVariable.Length > 0)
                {
                    Type myType = typeof(PlatformService);
                    var defaultValue = (bool)myType.GetField(ActiveConditionVariable).GetValue(myType);
                    state = defaultValue;
                }
            }
            catch (Exception ex)
            {

            }
            return state;
        }
    }
}
