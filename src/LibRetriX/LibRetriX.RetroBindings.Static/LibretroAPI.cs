using System;
using System.Runtime.InteropServices;

namespace LibRetriX.RetroBindings
{
    internal static class LibretroAPI
    {
        #region Static callback delegates
        //This is to ensure native code has references to static members of a static class, which won't be moved by GC

        /*private static LibretroLogDelegate logCallback;
        public static LibretroLogDelegate LogCallback
        {
            get => logCallback;
            set { logCallback = value; SetLogDelegate(logCallback); }
        }*/

        public static string LibraryName=@NativeDllInfo.DllName;

        private static LibretroEnvironmentDelegate environmentCallback;
        public static LibretroEnvironmentDelegate EnvironmentCallback
        {
            get => environmentCallback;
            set { environmentCallback = value; try { SetEnvironmentDelegate(environmentCallback); } catch (Exception e) { } }
        }

        private static LibretroRenderVideoFrameDelegate renderVideoFrameCallback;
        public static LibretroRenderVideoFrameDelegate RenderVideoFrameCallback
        {
            get => renderVideoFrameCallback;
            set { renderVideoFrameCallback = value; try{SetRenderVideoFrameDelegate(renderVideoFrameCallback); } catch (Exception e) { } }
        }

        private static LibretroRenderAudioFrameDelegate renderAudioFrameCallback;
        public static LibretroRenderAudioFrameDelegate RenderAudioFrameCallback
        {
            get => renderAudioFrameCallback;
            set { renderAudioFrameCallback = value; try{SetRenderAudioFrameDelegate(renderAudioFrameCallback); } catch (Exception e) { } }
        }

        private static LibretroRenderAudioFramesDelegate renderAudioFramesCallback;
        public static LibretroRenderAudioFramesDelegate RenderAudioFramesCallback
        {
            get => renderAudioFramesCallback;
            set { renderAudioFramesCallback = value; try{SetRenderAudioFramesDelegate(renderAudioFramesCallback); } catch (Exception e) { } }
        }

        private static LibretroPollInputDelegate pollInputCallback;
        public static LibretroPollInputDelegate PollInputCallback
        {
            get => pollInputCallback;
            set { pollInputCallback = value; try{SetPollInputDelegate(pollInputCallback); } catch (Exception e) { } }
        }

        private static LibretroGetInputStateDelegate getInputStateCallback;
        public static LibretroGetInputStateDelegate GetInputStateCallback
        {
            get => getInputStateCallback;
            set { getInputStateCallback = value; try{SetGetInputStateDelegate(getInputStateCallback); } catch (Exception e) { } }
        }
        #endregion

        // import necessary API as shown in other examples
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lib);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void FreeLibrary(IntPtr module);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr module, string proc);


        #region Old API
        /*
        #region Internal callback setters
        //[DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_log")]
        //private extern static void SetLogDelegate(LibretroLogDelegate f);

        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_environment")]
        private extern static void SetEnvironmentDelegate(LibretroEnvironmentDelegate f);

        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_video_refresh")]
        private extern static void SetRenderVideoFrameDelegate(LibretroRenderVideoFrameDelegate f);

        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_audio_sample")]
        private extern static void SetRenderAudioFrameDelegate(LibretroRenderAudioFrameDelegate f);

        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_audio_sample_batch")]
        private extern static void SetRenderAudioFramesDelegate(LibretroRenderAudioFramesDelegate f);

        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_input_poll")]
        private extern static void SetPollInputDelegate(LibretroPollInputDelegate f);

        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_input_state")]
        private extern static void SetGetInputStateDelegate(LibretroGetInputStateDelegate f);
        #endregion

        #region Libretro API methods
        /// <summary>
        /// Used to validate ABI compatibility when the API is revised.
        /// </summary>
        /// <returns>The current API version</returns>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_api_version")]
        public extern static uint GetAPIVersion();

        /// <summary>
        ///  Gets statically known system info.
        ///  Pointers provided in *info must be statically allocated.
        ///  Can be called at any time, even before retro_init().
        /// </summary>
        /// <param name="info">Core system info</param>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_get_system_info")]
        public extern static void GetSystemInfo(ref SystemInfo info);

        /// <summary>
        /// Gets information about system audio/video timings and geometry.
        /// Can be called only after retro_load_game() has successfully completed.
        /// The implementation of this function might not initialize every variable if needed.
        /// E.g.geom.aspect_ratio might not be initialized if core doesn't desire a particular aspect ratio.
        /// </summary>
        /// <param name="info">Core AV info</param>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_get_system_av_info")]
        public extern static void GetSystemAvInfo(ref SystemAVInfo info);

        /// <summary>
        /// Gets region of game.
        /// </summary>
        /// <returns>The game region</returns>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_get_region")]
        public extern static uint GetGameRegion();

        /// <summary>
        /// Core Initialization
        /// </summary>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retrix_init")]
        public extern static void Initialize();

        /// <summary>
        /// Core deinitialization
        /// </summary>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_deinit")]
        public extern static void Cleanup();

        /// <summary>
        /// Sets device to be used for player 'port'.
        /// By default, RETRO_DEVICE_JOYPAD is assumed to be plugged into all available ports.
        /// Setting a particular device type is not a guarantee that libretro cores will only poll input based on that particular device type.
        /// It is only a hint to the libretro core when a core cannot automatically detect the appropriate input device type on its own.
        /// It is also relevant when a core can change its behavior depending on device type.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="device"></param>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_controller_port_device")]
        public extern static void SetControllerPortDevice(uint port, uint device);

        /// <summary>
        /// Loads a game
        /// </summary>
        /// <param name="game">Game info struct</param>
        /// <returns>Whether the game was successfully loaded</returns>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retrix_load_game")]
        [return: MarshalAs(UnmanagedType.I1)]
        public extern static bool LoadGame([In] ref GameInfo game);

        

        /// <summary>
        /// Unloads a currently loaded game
        /// </summary>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_unload_game")]
        public extern static void UnloadGame();

        /// <summary>
        /// Resets the current game
        /// </summary>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_reset")]
        public extern static void Reset();

        /// <summary>
        /// Resets the current game
        /// </summary>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "update_core_options_in_game")]
        public extern static void UpdateOptionsInGame();

        /// <summary>
        /// Runs the game for one video frame.
        /// During retro_run(), input_poll callback must be called at least once.
        /// If a frame is not rendered for reasons where a game "dropped" a frame, this still counts as a frame, and retro_run() should explicitly dupe a frame if GET_CAN_DUPE returns true.
        /// In this case, the video callback can take a NULL argument for data.
        /// </summary>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retrix_run")]
        public extern static void RunFrame();


        //[DllImport("vulkan-1.dll", SetLastError = true)]
        //public static extern IntPtr LoadPackagedLibrary([MarshalAs(UnmanagedType.LPWStr)] string libraryName, int reserved = 0);

        /// <summary>
        /// Returns the amount of data the implementation requires to serialize internal state(save states).
        /// Between calls to retro_load_game() and retro_unload_game(), the returned size is never allowed to be larger than a previous returned value,
        /// to ensure that the frontend can allocate a save state buffer once.
        /// </summary>
        /// <returns>Size of serialization buffer</returns>
        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_serialize_size")]
        public extern static IntPtr GetSerializationSize();

        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_serialize")]
        [return: MarshalAs(UnmanagedType.I1)]
        public extern static bool SaveState(IntPtr data, IntPtr size);

        [DllImport(@NativeDllInfo.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_unserialize")]
        [return: MarshalAs(UnmanagedType.I1)]
        public extern static bool LoadState(IntPtr data, IntPtr size);
        #endregion
        */
        #endregion

        #region Internal callback setters
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetEnvironmentDelegateRetrix(LibretroEnvironmentDelegate f);
        public static SetEnvironmentDelegateRetrix SetEnvironmentDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetRenderVideoFrameDelegateRetrix(LibretroRenderVideoFrameDelegate f);
        public static SetRenderVideoFrameDelegateRetrix SetRenderVideoFrameDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetRenderAudioFrameDelegateRetrix(LibretroRenderAudioFrameDelegate f);
        public static SetRenderAudioFrameDelegateRetrix SetRenderAudioFrameDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetRenderAudioFramesDelegateRetrix(LibretroRenderAudioFramesDelegate f);
        public static SetRenderAudioFramesDelegateRetrix SetRenderAudioFramesDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetPollInputDelegateRetrix(LibretroPollInputDelegate f);
        public static SetPollInputDelegateRetrix SetPollInputDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetGetInputStateDelegateRetrix(LibretroGetInputStateDelegate f);
        public static SetGetInputStateDelegateRetrix SetGetInputStateDelegate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint GetAPIVersionRetrix();
        public static GetAPIVersionRetrix GetAPIVersion;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetSystemInfoRetrix(ref SystemInfo info);
        public static GetSystemInfoRetrix GetSystemInfo;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetSystemAvInfoRetrix(ref SystemAVInfo info);
        public static GetSystemAvInfoRetrix GetSystemAvInfo;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint GetGameRegionRetrix();
        public static GetGameRegionRetrix GetGameRegion;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void InitializeRetrix();
        public static InitializeRetrix Initialize;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CleanupRetrix();
        public static CleanupRetrix Cleanup;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetControllerPortDeviceRetrix(uint port, uint device);
        public static SetControllerPortDeviceRetrix SetControllerPortDevice;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool LoadGameRetrix([In] ref GameInfo game);
        public static LoadGameRetrix LoadGame;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool LoadGameSpecialRetrix(uint type, [In] ref GameInfo game);
        public static LoadGameSpecialRetrix LoadGameSpecial;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UnloadGameRetrix();
        public static UnloadGameRetrix UnloadGame;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ResetRetrix();
        public static ResetRetrix Reset;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UpdateOptionsInGameRetrix();
        public static UpdateOptionsInGameRetrix UpdateOptionsInGame;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UpdateVariablesRetrix(bool startup);
        public static UpdateVariablesRetrix UpdateVariables;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void RunFrameRetrix();
        public static RunFrameRetrix RunFrame;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetSerializationSizeRetrix();
        public static GetSerializationSizeRetrix GetSerializationSize;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool SaveStateRetrix(IntPtr data, IntPtr size);
        public static SaveStateRetrix SaveState;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool LoadStateRetrix(IntPtr data, IntPtr size);
        public static LoadStateRetrix LoadState;

        public static void LoadLibraryFunctions(IntPtr DLLModule)
        {
            try { 
                IntPtr SetEnvironmentDelegatePointer = GetProcAddress(DLLModule, "retro_set_environment");
                if (SetEnvironmentDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetEnvironmentDelegate = Marshal.GetDelegateForFunctionPointer<SetEnvironmentDelegateRetrix>(SetEnvironmentDelegatePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }
                IntPtr SetRenderVideoFrameDelegatePointer = GetProcAddress(DLLModule, "retro_set_video_refresh");
                if (SetRenderVideoFrameDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetRenderVideoFrameDelegate = Marshal.GetDelegateForFunctionPointer<SetRenderVideoFrameDelegateRetrix>(SetRenderVideoFrameDelegatePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetRenderAudioFrameDelegatePointer = GetProcAddress(DLLModule, "retro_set_audio_sample");
                if (SetRenderAudioFrameDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetRenderAudioFrameDelegate = Marshal.GetDelegateForFunctionPointer<SetRenderAudioFrameDelegateRetrix>(SetRenderAudioFrameDelegatePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetRenderAudioFramesDelegatePointer = GetProcAddress(DLLModule, "retro_set_audio_sample_batch");
                if (SetRenderAudioFramesDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetRenderAudioFramesDelegate = Marshal.GetDelegateForFunctionPointer<SetRenderAudioFramesDelegateRetrix>(SetRenderAudioFramesDelegatePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetPollInputDelegatePointer = GetProcAddress(DLLModule, "retro_set_input_poll");
                if (SetPollInputDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetPollInputDelegate = Marshal.GetDelegateForFunctionPointer<SetPollInputDelegateRetrix>(SetPollInputDelegatePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetGetInputStateDelegatePointer = GetProcAddress(DLLModule, "retro_set_input_state");
                if (SetGetInputStateDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetGetInputStateDelegate = Marshal.GetDelegateForFunctionPointer<SetGetInputStateDelegateRetrix>(SetGetInputStateDelegatePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------

                IntPtr GetAPIVersionPointer = GetProcAddress(DLLModule, "retro_api_version");
                if (GetAPIVersionPointer != IntPtr.Zero) // error handling
                {
                    GetAPIVersion = Marshal.GetDelegateForFunctionPointer<GetAPIVersionRetrix>(GetAPIVersionPointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr GetSystemInfoPointer = GetProcAddress(DLLModule, "retro_get_system_info");
                if (GetSystemInfoPointer != IntPtr.Zero) // error handling
                {
                    GetSystemInfo = Marshal.GetDelegateForFunctionPointer<GetSystemInfoRetrix>(GetSystemInfoPointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr GetSystemAvInfoPointer = GetProcAddress(DLLModule, "retro_get_system_av_info");
                if (GetSystemAvInfoPointer != IntPtr.Zero) // error handling
                {
                    GetSystemAvInfo = Marshal.GetDelegateForFunctionPointer<GetSystemAvInfoRetrix>(GetSystemAvInfoPointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr GetGameRegionPointer = GetProcAddress(DLLModule, "retro_get_region");
                if (GetGameRegionPointer != IntPtr.Zero) // error handling
                {
                    GetGameRegion = Marshal.GetDelegateForFunctionPointer<GetGameRegionRetrix>(GetGameRegionPointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr InitializePointer = GetProcAddress(DLLModule, "retrix_init");
                if (InitializePointer != IntPtr.Zero) // error handling
                {
                    Initialize = Marshal.GetDelegateForFunctionPointer<InitializeRetrix>(InitializePointer);
                }
                else
                {
                    InitializePointer = GetProcAddress(DLLModule, "retro_init");
                    if (InitializePointer != IntPtr.Zero) // error handling
                    {
                        Initialize = Marshal.GetDelegateForFunctionPointer<InitializeRetrix>(InitializePointer);
                    }
                    else
                    {
                        throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                    }
                }

                IntPtr CleanupPointer = GetProcAddress(DLLModule, "retro_deinit");
                if (CleanupPointer != IntPtr.Zero) // error handling
                {
                    Cleanup = Marshal.GetDelegateForFunctionPointer<CleanupRetrix>(CleanupPointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetControllerPortDevicePointer = GetProcAddress(DLLModule, "retro_set_controller_port_device");
                if (SetControllerPortDevicePointer != IntPtr.Zero) // error handling
                {
                    SetControllerPortDevice = Marshal.GetDelegateForFunctionPointer<SetControllerPortDeviceRetrix>(SetControllerPortDevicePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------

                IntPtr LoadGamePointer = GetProcAddress(DLLModule, "retrix_load_game");
                if (LoadGamePointer != IntPtr.Zero) // error handling
                {
                    LoadGame = Marshal.GetDelegateForFunctionPointer<LoadGameRetrix>(LoadGamePointer);
                }
                else
                {
                    LoadGamePointer = GetProcAddress(DLLModule, "retro_load_game");
                    if (LoadGamePointer != IntPtr.Zero) // error handling
                    {
                        LoadGame = Marshal.GetDelegateForFunctionPointer<LoadGameRetrix>(LoadGamePointer);
                    }
                    else
                    {
                        throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                    }
                }

                IntPtr LoadGameSpecialPointer = GetProcAddress(DLLModule, "retro_load_game_special");
                if (LoadGameSpecialPointer != IntPtr.Zero) // error handling
                {
                    LoadGameSpecial = Marshal.GetDelegateForFunctionPointer<LoadGameSpecialRetrix>(LoadGameSpecialPointer);
                }
                else
                {
                    //throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr UnloadGamePointer = GetProcAddress(DLLModule, "retro_unload_game");
                if (UnloadGamePointer != IntPtr.Zero) // error handling
                {
                    UnloadGame = Marshal.GetDelegateForFunctionPointer<UnloadGameRetrix>(UnloadGamePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr ResetPointer = GetProcAddress(DLLModule, "retro_reset");
                if (ResetPointer != IntPtr.Zero) // error handling
                {
                    Reset = Marshal.GetDelegateForFunctionPointer<ResetRetrix>(ResetPointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr UpdateOptionsInGamePointer = GetProcAddress(DLLModule, "update_core_options_in_game");
                if (UpdateOptionsInGamePointer != IntPtr.Zero) // error handling
                {
                    UpdateOptionsInGame = Marshal.GetDelegateForFunctionPointer<UpdateOptionsInGameRetrix>(UpdateOptionsInGamePointer);
                }
                else
                {
                    UpdateOptionsInGamePointer = GetProcAddress(DLLModule, "update_variables");
                    if (UpdateOptionsInGamePointer != IntPtr.Zero) // error handling
                    {
                        UpdateOptionsInGame = Marshal.GetDelegateForFunctionPointer<UpdateOptionsInGameRetrix>(UpdateOptionsInGamePointer);
                    }
                    else
                    {
                        UpdateOptionsInGamePointer = GetProcAddress(DLLModule, "check_variables");
                        if (UpdateOptionsInGamePointer != IntPtr.Zero) // error handling
                        {
                            UpdateOptionsInGame = Marshal.GetDelegateForFunctionPointer<UpdateOptionsInGameRetrix>(UpdateOptionsInGamePointer);
                        }
                        else
                        {
                            //throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                        }
                     }
                }

                IntPtr UpdateVariablesPointer = GetProcAddress(DLLModule, "update_variables");
                if (UpdateVariablesPointer != IntPtr.Zero) // error handling
                {
                    UpdateVariables = Marshal.GetDelegateForFunctionPointer<UpdateVariablesRetrix>(UpdateVariablesPointer);
                }
                else
                {
                    UpdateVariablesPointer = GetProcAddress(DLLModule, "check_variables");
                    if (UpdateVariablesPointer != IntPtr.Zero) // error handling
                    {
                        UpdateVariables = Marshal.GetDelegateForFunctionPointer<UpdateVariablesRetrix>(UpdateVariablesPointer);
                    }
                    else
                    {
                      //throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                    }
                }
                
                IntPtr RunFramePointer = GetProcAddress(DLLModule, "retrix_run");
                if (RunFramePointer != IntPtr.Zero) // error handling
                {
                    RunFrame = Marshal.GetDelegateForFunctionPointer<RunFrameRetrix>(RunFramePointer);
                }
                else
                {
                    RunFramePointer = GetProcAddress(DLLModule, "retro_run");
                    if (RunFramePointer != IntPtr.Zero) // error handling
                    {
                        RunFrame = Marshal.GetDelegateForFunctionPointer<RunFrameRetrix>(RunFramePointer);
                    }
                    else
                    {
                        throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                    }
                }

                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------

                IntPtr GetSerializationSizePointer = GetProcAddress(DLLModule, "retro_serialize_size");
                if (GetSerializationSizePointer != IntPtr.Zero) // error handling
                {
                    GetSerializationSize = Marshal.GetDelegateForFunctionPointer<GetSerializationSizeRetrix>(GetSerializationSizePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SaveStatePointer = GetProcAddress(DLLModule, "retro_serialize");
                if (SaveStatePointer != IntPtr.Zero) // error handling
                {
                    SaveState = Marshal.GetDelegateForFunctionPointer<SaveStateRetrix>(SaveStatePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

                IntPtr LoadStatePointer = GetProcAddress(DLLModule, "retro_unserialize");
                if (LoadStatePointer != IntPtr.Zero) // error handling
                {
                    LoadState = Marshal.GetDelegateForFunctionPointer<LoadStateRetrix>(LoadStatePointer);
                }
                else
                {
                    throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                }

            }catch(Exception e)
            {

            }

        }
        #endregion
    }
}
