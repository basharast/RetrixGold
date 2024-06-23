using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using static LibRetriX.RetroBindings.DLLLoader;
using static RetriX.UWP.RetroBindings.Structs.PerfInterface;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace LibRetriX.RetroBindings
{
    public static class DLLLoader
    {
        public static bool loaderReady = false;
        static DLLLoader()
        {
            if (!loaderReady)
            {
                GetPID();
            }
        }

        //[UnmanagedFunctionPointer(CallingConvention.Winapi, BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true)]
        //public delegate IntPtr LoadLibraryW(string lib);
        //public static LoadLibraryW LoadLibrary;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lib);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr module);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr module, string proc);

        public static void GetPID()
        {
            /*try
            {
                Retrix.UWP.Native.DLLLoader.InitialGetProcess();
                var LoadLibraryW = (IntPtr)Retrix.UWP.Native.DLLLoader.GetProcAddressCall("kernelbase.dll", "LoadLibraryW");
                if (LoadLibraryW != IntPtr.Zero) // error handling
                {
                    LoadLibrary = Marshal.GetDelegateForFunctionPointer<LoadLibraryW>(LoadLibraryW);
                }
            }
            catch(Exception ex)
            {

            }*/
            loaderReady = true;
        }
        /*[DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceFrequency(out LARGE_INTEGER lpFrequency);
       
        [DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out LARGE_INTEGER lpPerformanceCount);*/
    }

    public class LibretroAPI
    {
        //This class was static before
        //but because it caused many issues to AnyCore feature
        //it cannot be static any more, now each core will have new instance from LibretroAPI to avoid any conflicts

        #region Callback delegates

        public bool CoreFailed = false;
        public bool LibraryState = false;
        public string RetriXGoldLogFileLocation = "";

        //Checking delegates state if 'null' and 'LibraryState' very important
        //this step will prevent the app from crashing if the library failed to load
        //as RetriXGold already prepared to handle the failed core this step is highly important 

        private LibretroEnvironmentDelegate environmentCallback;
        public LibretroEnvironmentDelegate EnvironmentCallback
        {
            get => environmentCallback;
            set
            {
                environmentCallback = value;
                try
                {
                    if (SetEnvironmentDelegate != null && LibraryState)
                    {
                        SetEnvironmentDelegate(environmentCallback);
                    }
                    else
                    {
                        CoreFailed = true;
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        private LibretroRenderVideoFrameDelegate renderVideoFrameCallback;
        public LibretroRenderVideoFrameDelegate RenderVideoFrameCallback
        {
            get => renderVideoFrameCallback;
            set
            {
                renderVideoFrameCallback = value;
                try
                {
                    if (SetRenderVideoFrameDelegate != null && LibraryState)
                    {
                        SetRenderVideoFrameDelegate(renderVideoFrameCallback);
                    }
                    else
                    {
                        CoreFailed = true;
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        private LibretroRenderAudioFrameDelegate renderAudioFrameCallback;
        public LibretroRenderAudioFrameDelegate RenderAudioFrameCallback
        {
            get => renderAudioFrameCallback;
            set
            {
                renderAudioFrameCallback = value;
                try
                {
                    if (SetRenderAudioFrameDelegate != null && LibraryState)
                    {
                        SetRenderAudioFrameDelegate(renderAudioFrameCallback);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        private LibretroRenderAudioFramesDelegate renderAudioFramesCallback;
        public LibretroRenderAudioFramesDelegate RenderAudioFramesCallback
        {
            get => renderAudioFramesCallback;
            set
            {
                renderAudioFramesCallback = value;
                try
                {
                    if (SetRenderAudioFramesDelegate != null && LibraryState)
                    {
                        SetRenderAudioFramesDelegate(renderAudioFramesCallback);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        private LibretroPollInputDelegate pollInputCallback;
        public LibretroPollInputDelegate PollInputCallback
        {
            get => pollInputCallback;
            set
            {
                pollInputCallback = value;
                try
                {
                    if (SetPollInputDelegate != null && LibraryState)
                    {
                        SetPollInputDelegate(pollInputCallback);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        private LibretroGetInputStateDelegate getInputStateCallback;
        public LibretroGetInputStateDelegate GetInputStateCallback
        {
            get => getInputStateCallback;
            set
            {
                getInputStateCallback = value;
                try
                {
                    if (SetGetInputStateDelegate != null && LibraryState)
                    {
                        SetGetInputStateDelegate(getInputStateCallback);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        #endregion

        #region Internal callback setters
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetEnvironmentDelegateRetrix(LibretroEnvironmentDelegate f);
        public SetEnvironmentDelegateRetrix SetEnvironmentDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetRenderVideoFrameDelegateRetrix(LibretroRenderVideoFrameDelegate f);
        public SetRenderVideoFrameDelegateRetrix SetRenderVideoFrameDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetRenderAudioFrameDelegateRetrix(LibretroRenderAudioFrameDelegate f);
        public SetRenderAudioFrameDelegateRetrix SetRenderAudioFrameDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetRenderAudioFramesDelegateRetrix(LibretroRenderAudioFramesDelegate f);
        public SetRenderAudioFramesDelegateRetrix SetRenderAudioFramesDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetPollInputDelegateRetrix(LibretroPollInputDelegate f);
        public SetPollInputDelegateRetrix SetPollInputDelegate;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetGetInputStateDelegateRetrix(LibretroGetInputStateDelegate f);
        public SetGetInputStateDelegateRetrix SetGetInputStateDelegate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint GetAPIVersionRetrix();
        public GetAPIVersionRetrix GetAPIVersion;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetSystemInfoRetrix(ref SystemInfo info);
        public GetSystemInfoRetrix GetSystemInfo;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetSystemAvInfoRetrix(ref SystemAVInfo info);
        public GetSystemAvInfoRetrix GetSystemAvInfo;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint GetGameRegionRetrix();
        public GetGameRegionRetrix GetGameRegion;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void InitializeRetrix();
        public InitializeRetrix Initialize;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CleanupRetrix();
        public CleanupRetrix Cleanup;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetControllerPortDeviceRetrix(uint port, uint device);
        public SetControllerPortDeviceRetrix SetControllerPortDevice;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool StartCoreRetrix(IntPtr data);
        public StartCoreRetrix StartCore;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool LoadGameRetrix([In] ref GameInfo game);
        public LoadGameRetrix LoadGame;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool LoadGameSpecialRetrix(uint type, [In] ref GameInfo game);
        public LoadGameSpecialRetrix LoadGameSpecial;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UnloadGameRetrix();
        public UnloadGameRetrix UnloadGame;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ResetRetrix();
        public ResetRetrix Reset;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void RunFrameRetrix();
        public RunFrameRetrix RunFrame;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetSerializationSizeRetrix();
        public GetSerializationSizeRetrix GetSerializationSize;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool SaveStateRetrix(IntPtr data, IntPtr size);
        public SaveStateRetrix SaveState;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool LoadStateRetrix(IntPtr data, IntPtr size);
        public LoadStateRetrix LoadState;

        /*
        public void LoadLibraryFunctions(string DLLModule)
        {
            try
            {
                var SetEnvironmentDelegatePointer = (IntPtr)Retrix.UWP.Native.DLLLoader.GetProcAddressCall($"{DLLModule}", "retro_set_environment");
                if (SetEnvironmentDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetEnvironmentDelegate = Marshal.GetDelegateForFunctionPointer<SetEnvironmentDelegateRetrix>(SetEnvironmentDelegatePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_set_environment: {Marshal.GetLastWin32Error()}");
                }

            }
            catch (Exception ex)
            {

            }
        }
        */
        public void LoadLibraryFunctions(IntPtr DLLModule)
        {
            try
            {

                //IntPtr SetEnvironmentDelegatePointer = (IntPtr)Retrix.UWP.Native.DLLLoader.KGetProcAddressCall((long)DLLModule, "retro_set_environment");
                IntPtr SetEnvironmentDelegatePointer = DLLLoader.GetProcAddress(DLLModule, "retro_set_environment");
                if (SetEnvironmentDelegatePointer != IntPtr.Zero)
                {
                    SetEnvironmentDelegate = Marshal.GetDelegateForFunctionPointer<SetEnvironmentDelegateRetrix>(SetEnvironmentDelegatePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_set_environment: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetRenderVideoFrameDelegatePointer = DLLLoader.GetProcAddress(DLLModule, "retro_set_video_refresh");
                if (SetRenderVideoFrameDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetRenderVideoFrameDelegate = Marshal.GetDelegateForFunctionPointer<SetRenderVideoFrameDelegateRetrix>(SetRenderVideoFrameDelegatePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_set_video_refresh: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetRenderAudioFrameDelegatePointer = DLLLoader.GetProcAddress(DLLModule, "retro_set_audio_sample");
                if (SetRenderAudioFrameDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetRenderAudioFrameDelegate = Marshal.GetDelegateForFunctionPointer<SetRenderAudioFrameDelegateRetrix>(SetRenderAudioFrameDelegatePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_set_audio_sample: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetRenderAudioFramesDelegatePointer = DLLLoader.GetProcAddress(DLLModule, "retro_set_audio_sample_batch");
                if (SetRenderAudioFramesDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetRenderAudioFramesDelegate = Marshal.GetDelegateForFunctionPointer<SetRenderAudioFramesDelegateRetrix>(SetRenderAudioFramesDelegatePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_set_audio_sample_batch: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetPollInputDelegatePointer = DLLLoader.GetProcAddress(DLLModule, "retro_set_input_poll");
                if (SetPollInputDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetPollInputDelegate = Marshal.GetDelegateForFunctionPointer<SetPollInputDelegateRetrix>(SetPollInputDelegatePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_set_input_poll: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetGetInputStateDelegatePointer = DLLLoader.GetProcAddress(DLLModule, "retro_set_input_state");
                if (SetGetInputStateDelegatePointer != IntPtr.Zero) // error handling
                {
                    SetGetInputStateDelegate = Marshal.GetDelegateForFunctionPointer<SetGetInputStateDelegateRetrix>(SetGetInputStateDelegatePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_set_input_state: {Marshal.GetLastWin32Error()}");
                }

                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------

                IntPtr GetAPIVersionPointer = DLLLoader.GetProcAddress(DLLModule, "retro_api_version");
                if (GetAPIVersionPointer != IntPtr.Zero) // error handling
                {
                    GetAPIVersion = Marshal.GetDelegateForFunctionPointer<GetAPIVersionRetrix>(GetAPIVersionPointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_api_version: {Marshal.GetLastWin32Error()}");
                }

                IntPtr GetSystemInfoPointer = DLLLoader.GetProcAddress(DLLModule, "retro_get_system_info");
                if (GetSystemInfoPointer != IntPtr.Zero) // error handling
                {
                    GetSystemInfo = Marshal.GetDelegateForFunctionPointer<GetSystemInfoRetrix>(GetSystemInfoPointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_get_system_info: {Marshal.GetLastWin32Error()}");
                }

                IntPtr GetSystemAvInfoPointer = DLLLoader.GetProcAddress(DLLModule, "retro_get_system_av_info");
                if (GetSystemAvInfoPointer != IntPtr.Zero) // error handling
                {
                    GetSystemAvInfo = Marshal.GetDelegateForFunctionPointer<GetSystemAvInfoRetrix>(GetSystemAvInfoPointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_get_system_av_info: {Marshal.GetLastWin32Error()}");
                }

                IntPtr GetGameRegionPointer = DLLLoader.GetProcAddress(DLLModule, "retro_get_region");
                if (GetGameRegionPointer != IntPtr.Zero) // error handling
                {
                    GetGameRegion = Marshal.GetDelegateForFunctionPointer<GetGameRegionRetrix>(GetGameRegionPointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_get_region: {Marshal.GetLastWin32Error()}");
                }

                IntPtr InitializePointer = DLLLoader.GetProcAddress(DLLModule, "retro_init");
                if (InitializePointer != IntPtr.Zero) // error handling
                {
                    Initialize = Marshal.GetDelegateForFunctionPointer<InitializeRetrix>(InitializePointer);
                }
                else
                {
                    InitializePointer = DLLLoader.GetProcAddress(DLLModule, "retrix_init");
                    if (InitializePointer != IntPtr.Zero) // error handling
                    {
                        Initialize = Marshal.GetDelegateForFunctionPointer<InitializeRetrix>(InitializePointer);
                    }
                    else
                    {
                        _ = WriteLog($"Could not load method retro_init: {Marshal.GetLastWin32Error()}");
                    }
                }

                IntPtr CleanupPointer = DLLLoader.GetProcAddress(DLLModule, "retro_deinit");
                if (CleanupPointer != IntPtr.Zero) // error handling
                {
                    Cleanup = Marshal.GetDelegateForFunctionPointer<CleanupRetrix>(CleanupPointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_deinit: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SetControllerPortDevicePointer = DLLLoader.GetProcAddress(DLLModule, "retro_set_controller_port_device");
                if (SetControllerPortDevicePointer != IntPtr.Zero) // error handling
                {
                    SetControllerPortDevice = Marshal.GetDelegateForFunctionPointer<SetControllerPortDeviceRetrix>(SetControllerPortDevicePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_set_controller_port_device: {Marshal.GetLastWin32Error()}");
                }

                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------

                IntPtr LoadGamePointer = DLLLoader.GetProcAddress(DLLModule, "retro_load_game");
                if (LoadGamePointer != IntPtr.Zero) // error handling
                {
                    LoadGame = Marshal.GetDelegateForFunctionPointer<LoadGameRetrix>(LoadGamePointer);
                }
                else
                {
                    LoadGamePointer = DLLLoader.GetProcAddress(DLLModule, "retrix_load_game");
                    if (LoadGamePointer != IntPtr.Zero) // error handling
                    {
                        LoadGame = Marshal.GetDelegateForFunctionPointer<LoadGameRetrix>(LoadGamePointer);
                    }
                    else
                    {
                        _ = WriteLog($"Could not load method retro_load_game: {Marshal.GetLastWin32Error()}");
                    }
                }



                IntPtr StartCorePointer = DLLLoader.GetProcAddress(DLLModule, "retro_load_game");
                if (StartCorePointer != IntPtr.Zero) // error handling
                {
                    StartCore = Marshal.GetDelegateForFunctionPointer<StartCoreRetrix>(StartCorePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_load_game: {Marshal.GetLastWin32Error()}");
                }

                IntPtr LoadGameSpecialPointer = DLLLoader.GetProcAddress(DLLModule, "retro_load_game_special");
                if (LoadGameSpecialPointer != IntPtr.Zero) // error handling
                {
                    LoadGameSpecial = Marshal.GetDelegateForFunctionPointer<LoadGameSpecialRetrix>(LoadGameSpecialPointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_load_game_special: {Marshal.GetLastWin32Error()}");
                }

                IntPtr UnloadGamePointer = DLLLoader.GetProcAddress(DLLModule, "retro_unload_game");
                if (UnloadGamePointer != IntPtr.Zero) // error handling
                {
                    UnloadGame = Marshal.GetDelegateForFunctionPointer<UnloadGameRetrix>(UnloadGamePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_unload_game: {Marshal.GetLastWin32Error()}");
                }

                IntPtr ResetPointer = DLLLoader.GetProcAddress(DLLModule, "retro_reset");
                if (ResetPointer != IntPtr.Zero) // error handling
                {
                    Reset = Marshal.GetDelegateForFunctionPointer<ResetRetrix>(ResetPointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_reset: {Marshal.GetLastWin32Error()}");
                }

                IntPtr RunFramePointer = DLLLoader.GetProcAddress(DLLModule, "retro_run");
                if (RunFramePointer != IntPtr.Zero) // error handling
                {
                    RunFrame = Marshal.GetDelegateForFunctionPointer<RunFrameRetrix>(RunFramePointer);
                }
                else
                {
                    RunFramePointer = DLLLoader.GetProcAddress(DLLModule, "retrix_run");
                    if (RunFramePointer != IntPtr.Zero) // error handling
                    {
                        RunFrame = Marshal.GetDelegateForFunctionPointer<RunFrameRetrix>(RunFramePointer);
                    }
                    else
                    {
                        _ = WriteLog($"Could not load method retro_run: {Marshal.GetLastWin32Error()}");
                    }
                }

                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------
                //--------------------------------------------------------------------------------------------------------------------

                IntPtr GetSerializationSizePointer = DLLLoader.GetProcAddress(DLLModule, "retro_serialize_size");
                if (GetSerializationSizePointer != IntPtr.Zero) // error handling
                {
                    GetSerializationSize = Marshal.GetDelegateForFunctionPointer<GetSerializationSizeRetrix>(GetSerializationSizePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_serialize_size: {Marshal.GetLastWin32Error()}");
                }

                IntPtr SaveStatePointer = DLLLoader.GetProcAddress(DLLModule, "retro_serialize");
                if (SaveStatePointer != IntPtr.Zero) // error handling
                {
                    SaveState = Marshal.GetDelegateForFunctionPointer<SaveStateRetrix>(SaveStatePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_serialize: {Marshal.GetLastWin32Error()}");
                }

                IntPtr LoadStatePointer = DLLLoader.GetProcAddress(DLLModule, "retro_unserialize");
                if (LoadStatePointer != IntPtr.Zero) // error handling
                {
                    LoadState = Marshal.GetDelegateForFunctionPointer<LoadStateRetrix>(LoadStatePointer);
                }
                else
                {
                    _ = WriteLog($"Could not load method retro_unserialize: {Marshal.GetLastWin32Error()}");
                }

            }
            catch (Exception e)
            {

            }

        }
        #endregion

        private async Task WriteLog(string message, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                if (RetriXGoldLogFileLocation != null)
                {
                    File.AppendAllText(RetriXGoldLogFileLocation, $"{memberName} ({sourceLineNumber}): {message}\n");
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
