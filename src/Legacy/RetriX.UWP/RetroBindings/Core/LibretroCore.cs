using LibRetriX.RetroBindings.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static LibRetriX.RetroBindings.Constants;
using System.Runtime.CompilerServices;
using RetriX.Shared.Services;
using Windows.Storage;
using RetriX.UWP.RetroBindings.Structs;
using RetriX.UWP.Services;
using RetriX.UWP;
using Windows.Graphics.Display.Core;
using RetriX.Shared.ViewModels;
using Windows.UI.Xaml.Media;
using WinUniversalTool;
using Windows.UI;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using RetriX.UWP.Pages;
using Windows.ApplicationModel;
using SharpDX.Direct3D;
using Windows.Devices.HumanInterfaceDevice;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.D3DCompiler;

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
    #region Delegates Main
    /// <summary>
    /// Video frame render callbacks
    /// </summary>
    /// <param name="data">Framebuffer data. Only valid while inside the callback</param>
    /// <param name="width">Framebufer width in pixels</param>
    /// <param name="height">Framebuffer height in pixels</param>
    /// <param name="pitch">Number of bytes between horizontal lines (framebuffer is not always packed in memory)</param>
    public delegate void RenderVideoFrameDelegate(IntPtr data, uint width, uint height, uint pitch);

    /// <summary>
    /// Audio data render callback. Use to fill audio buffers of whatever playback mechanism the front end uses
    /// </summary>
    /// <param name="data">Audio data. Only valid while inside the callback</param>
    /// <param name="numFrames">The number of audio frames to render</param>
    /// <returns>The number of audio frames enqueued for rendering. Can be less than numFrames</returns>
    public delegate uint RenderAudioFramesDelegate(ReadOnlySpan<short> data, uint numFrames);

    public delegate void PollInputDelegate();
    public delegate short GetInputStateDelegate(uint device, uint port, InputTypes inputType);

    public delegate void GeometryChangedDelegate(GameGeometry geometry);
    public delegate void TimingsChangedDelegate(SystemTimings timing);
    public delegate void RotationChangedDelegate(Rotations rotation);
    public delegate void PixelFormatChangedDelegate(PixelFormats format);

    public delegate Stream OpenFileStreamDelegate(string path, FileAccessMode fileAccess);
    public delegate void CloseFileStreamDelegate(Stream stream);
    public delegate int DeleteHandlerDelegate(string path);
    public delegate int RenameHandlerDelegate(string path, string newPath);
    public delegate long TruncateHandlerDelegate(Stream stream, long length);
    public delegate int StatHandlerDelegate(string path, IntPtr size);
    public delegate int MkdirHandlerDelegate(string dir);
    public delegate VFSDir OpendirHandlerDelegate(string path, bool include_hidden);
    public delegate bool ReaddirHandlerDelegate(VFSDir rdir);
    public delegate string DirentGetNameHandlerDelegate(VFSDir rdir);
    public delegate bool DirentIsDirHandlerDelegate(VFSDir rdir);
    public delegate int ClosedirHandlerDelegate(VFSDir rdir);
    public delegate void StopGameDelegate();
    #endregion
    public class LibretroCore : IDisposable
    {
        private const int AudioSamplesPerFrame = 2;

        public string Name { get; set; }
        [JsonIgnore]
        public string SystemName
        {
            get
            {
                return SystemNameStatic;
            }
            set
            {
                SystemNameStatic = value;
            }
        }
        [JsonIgnore]
        public LibretroAPI coreAPI = new LibretroAPI();
        public string SystemNameStatic { get; set; }
        public string OriginalSystemName { get; set; }
        public string Version { get; set; }
        public IList<string> SupportedExtensions { get; set; }
        public bool FailedToLoad = false;
        public bool IsNewCore { get; set; }
        public bool SkippedCore = false;
        public bool VFSSupport = false;
        public bool NativeArchiveSupport = false;
        public bool NativeArchiveNonZipSupport = false;
        public bool SubSystemSupport = false;
        public bool RestartRequired { get; set; }
        public bool AnyCore { get; set; }
        public bool ImportedCore { get; set; }

        public uint RetroGameType { get; set; }

        private bool RequiresFullPath { get; set; }

        private IntPtr CurrentlyResolvedCoreOptionValue { get; set; }
        public IDictionary<string, CoreOption> Options = new Dictionary<string, CoreOption>();

        private IList<Tuple<string, uint>> OptionSetters = new List<Tuple<string, uint>>();

        public IList<FileDependency> FileDependencies = new List<FileDependency>();

        #region Locations
        [JsonIgnore]
        private IntPtr systemRootPathUnmanaged;
        [JsonIgnore]
        private IntPtr systemGamesPathUnmanaged;
        private string systemGamesPath;
        private string systemRootPath;

        [JsonIgnore]
        public StorageFolder savesRootFolder;

        [JsonIgnore]
        public StorageFolder systemRootFolder;
        [JsonIgnore]
        public string SystemRootPath
        {
            get => systemRootPath;
            set { SetStringAndUnmanagedMemory(value, ref systemRootPath, ref systemRootPathUnmanaged); }
        }
        [JsonIgnore]
        public string SystemGamesPath
        {
            get => systemGamesPath;
            set { SetStringAndUnmanagedMemory(value, ref systemGamesPath, ref systemGamesPathUnmanaged); }
        }
        [JsonIgnore]
        private IntPtr saveRootPathUnmanaged;
        private string saveRootPath;
        [JsonIgnore]
        public string SaveRootPath
        {
            get => saveRootPath;
            set { SetStringAndUnmanagedMemory(value, ref saveRootPath, ref saveRootPathUnmanaged); }
        }
        #endregion

        private PixelFormats pixelFormat;
        [JsonIgnore]
        public PixelFormats PixelFormat
        {
            get => pixelFormat;
            private set { pixelFormat = value; PixelFormatChanged?.Invoke(pixelFormat); }
        }

        private GameGeometry geometry;
        [JsonIgnore]
        public GameGeometry Geometry
        {
            get => geometry;
            private set { geometry = value; GeometryChanged?.Invoke(geometry); }
        }

        private SystemTimings timings;
        [JsonIgnore]
        public SystemTimings Timings
        {
            get => timings;
            private set { timings = value; TimingsChanged?.Invoke(timings); }
        }

        private Rotations rotation;
        [JsonIgnore]
        public Rotations Rotation
        {
            get => rotation;
            private set { rotation = value; RotationChanged?.Invoke(rotation); }
        }

        [JsonIgnore]
        public ulong SerializationSize => (ulong)coreAPI.GetSerializationSize();

        #region Delegates
        [JsonIgnore]
        public PollInputDelegate PollInput { get; set; }
        [JsonIgnore]
        public GetInputStateDelegate GetInputState { get; set; }

        [JsonIgnore]
        public VFSHandler vfsHandler = new VFSHandler();
        [JsonIgnore]
        public OpenFileStreamDelegate OpenFileStream
        {
            get => vfsHandler.OpenFileStream;
            set { vfsHandler.OpenFileStream = value; }
        }

        [JsonIgnore]
        public CloseFileStreamDelegate CloseFileStream
        {
            get => vfsHandler.CloseFileStream;
            set { vfsHandler.CloseFileStream = value; }
        }
        [JsonIgnore]
        public DeleteHandlerDelegate DeleteHandler
        {
            get => vfsHandler.DeleteHandler;
            set { vfsHandler.DeleteHandler = value; }
        }
        [JsonIgnore]
        public RenameHandlerDelegate RenameHandler
        {
            get => vfsHandler.RenameHandler;
            set { vfsHandler.RenameHandler = value; }
        }
        [JsonIgnore]
        public TruncateHandlerDelegate TruncateHandler
        {
            get => vfsHandler.TruncateHandler;
            set { vfsHandler.TruncateHandler = value; }
        }
        [JsonIgnore]
        public StatHandlerDelegate StatHandler
        {
            get => vfsHandler.StatHandler;
            set { vfsHandler.StatHandler = value; }
        }
        [JsonIgnore]
        public MkdirHandlerDelegate MkdirHandler
        {
            get => vfsHandler.MkdirHandler;
            set { vfsHandler.MkdirHandler = value; }
        }
        [JsonIgnore]
        public OpendirHandlerDelegate OpendirHandler
        {
            get => vfsHandler.OpendirHandler;
            set { vfsHandler.OpendirHandler = value; }
        }
        [JsonIgnore]
        public ReaddirHandlerDelegate ReaddirHandler
        {
            get => vfsHandler.ReaddirHandler;
            set { vfsHandler.ReaddirHandler = value; }
        }
        [JsonIgnore]
        public DirentGetNameHandlerDelegate DirentGetNameHandler
        {
            get => vfsHandler.DirentGetNameHandler;
            set { vfsHandler.DirentGetNameHandler = value; }
        }
        [JsonIgnore]
        public DirentIsDirHandlerDelegate DirentIsDirHandler
        {
            get => vfsHandler.DirentIsDirHandler;
            set { vfsHandler.DirentIsDirHandler = value; }
        }
        [JsonIgnore]
        public ClosedirHandlerDelegate ClosedirHandler
        {
            get => vfsHandler.ClosedirHandler;
            set { vfsHandler.ClosedirHandler = value; }
        }

        public StopGameDelegate StopGameHandler { get; set; }
        [JsonIgnore]
        public StopGameDelegate StopGame
        {
            get => StopGameHandler;
            set { StopGameHandler = value; }
        }

        [JsonIgnore]
        public RenderVideoFrameDelegate RenderVideoFrame;
        [JsonIgnore]
        public RenderAudioFramesDelegate RenderAudioFrames;
        [JsonIgnore]
        public PixelFormatChangedDelegate PixelFormatChanged;
        [JsonIgnore]
        public GeometryChangedDelegate GeometryChanged;
        [JsonIgnore]
        public TimingsChangedDelegate TimingsChanged;
        [JsonIgnore]
        public RotationChangedDelegate RotationChanged;

        [JsonIgnore]
        private LogCallbackDescriptor LogCBDescriptor { get; set; }
        #endregion

        //This list will handle all the sub dll loading calls from the core
        public List<InternalLibrary> InternalLibraries = new List<InternalLibrary>();
        [JsonIgnore]
        private List<List<ControllerDescription>> SupportedInputsPerPort { get; } = new List<List<ControllerDescription>>();
        private Lazy<uint[]> InputTypesToUse { get; set; }
        private IEnumerable<uint> PreferredInputTypes { get; set; }

        private bool IsInitialized { get; set; }
        public bool StartupLoading { get; set; }
        public bool FailedToLoadGame { get; set; }
        public bool IsInGameOptionsActive { get; set; }
        private GameInfo? CurrentGameInfo { get; set; }
        private GCHandle GameDataHandle { get; set; }

        private readonly short[] RenderAudioFrameBuffer = new short[2];

        public bool AudioOnly { get; set; }
        public bool VideoOnly { get; set; }

        int frameRate = 0;
        public int FrameRate { get { int tempValue = frameRate; frameRate = 0; return tempValue; } set { frameRate = value; } }

        public bool ShowFPSCounter { get; set; }
        public bool SupportNoGame = false;
        public bool RequestNoGame { get; set; }
        public bool NativeSkipFrames { get; set; }
        public bool NativeSkipFramesRandom { get; set; }
        public bool SupportUnload = true;

        IntPtr DLLModule;
        public string DLLName { get; set; }
        public string CoreFileName { get; set; }

        public void FreeLibretroCore()
        {
            RequestVariablesUpdate = false;
            if (DLLModule != IntPtr.Zero)
            {
                PlatformService.KeyboardEvent = null;
                PlatformService.useNativeKeyboard = false;
                UnloadGame();
            }
        }
        bool loadCustomInput = true;
        public void ReInitialCore(bool LoadCustomInput = true)
        {
            if (SkippedCore)
            {
                return;
            }
            try
            {
                RequestVariablesUpdate = false;
                forceLogOnHold = false;
                WriteLogInProgress = false;
                loadCustomInput = LoadCustomInput;
                DeInitialize();
                FreeLibretroCore();
                Dispose();
                LogsList = new ObservableCollection<CoreLogItem>();
                LoadCoreLibrary();
                PrepareCore();
                Initialize(SubSystemSupport);
            }
            catch (Exception e)
            {

            }
        }
        public void LoadCoreLibrary()
        {
            if (SkippedCore)
            {
                return;
            }
            try
            {
                //Load DLL Library
                //If the core already loaded don't load again (check issue comment in FreeLibrary section below)
                if (DLLModule == IntPtr.Zero)
                {
                    while (!DLLLoader.loaderReady)
                    {
                        Task.Delay(100).Wait();
                    }
                    var mainPath = DLLName;
                    if (!mainPath.Contains("\\"))
                    {
                        mainPath = $@"Assets\Libraries\{DLLName}";
                    }
                    //DLLModule = (IntPtr)Retrix.UWP.Native.DLLLoader.LoadLibraryCall(mainPath.Replace(".dll", ".dat"));
                    DLLModule = DLLLoader.LoadLibrary(mainPath.Replace(".dll", ".dat"));
                    if (DLLModule == IntPtr.Zero)
                    {
                        DLLModule = DLLLoader.LoadLibrary(mainPath);
                    }
                }
                if (DLLModule != IntPtr.Zero) // error handling
                {
                    switch (CoreFileName)
                    {
                        case "dosbox_core_libretro":
                        case "dosbox_svn_libretro":
                            SupportUnload = false;
                            break;
                    }
                    coreAPI.LibraryState = true;
                    coreAPI.LoadLibraryFunctions(DLLModule);
                }
                else
                {
                    coreAPI.LibraryState = false;
                    var ErrorMessage = $"Could not load library {Path.GetFileName(DLLName)}: {Marshal.GetLastWin32Error()}";
                }
            }
            catch (Exception e)
            {

            }
        }

        IList<FileDependency> dependenciesGlobal = null;
        IList<Tuple<string, uint>> optionSettersGlobal = null;
        uint?[] inputTypeIdGlobal = null;
        [JsonConstructor]
        public LibretroCore(bool startupLoading, string LibraryName, IReadOnlyList<FileDependency> dependencies = null, IReadOnlyList<Tuple<string, uint>> optionSetters = null, uint?[] inputTypeId = null, string AnyCoreName = "", bool skippedCore = false, bool anyCore = false, bool importedCore = false, bool loadLibrary = true)
        {
            RequestVariablesUpdate = false;
            if (LibraryName != null)
            {
                LibretroCoreCall(startupLoading, LibraryName, dependencies, optionSetters, inputTypeId, AnyCoreName, skippedCore, anyCore, importedCore, loadLibrary);
            }
        }
        public void LibretroCoreCall(bool startupLoading, string LibraryName, IReadOnlyList<FileDependency> dependencies = null, IReadOnlyList<Tuple<string, uint>> optionSetters = null, uint?[] inputTypeId = null, string AnyCoreName = "", bool skippedCore = false, bool anyCore = false, bool importedCore = false, bool loadLibrary = true)
        {
            RequestVariablesUpdate = false;
            StartupLoading = startupLoading;
            AnyCore = anyCore;
            ImportedCore = importedCore;

            coreAPI = new LibretroAPI();
            coreAPI.RetriXGoldLogFileLocation = RetriXGoldLogFileLocation;

            vfsHandler = new VFSHandler();
            LogCBDescriptor = new LogCallbackDescriptor { LogCallback = LogHandler };

            hwHandler = new HWHandler();

            perfFunctions = new PerfInterface();
            diskController = new DiskController();
            diskControllerEX = new DiskControllerEX();
            midiInterfaceDelegates = new MidiInterface();
            ledInterfaceDelegates = new LEDInterface();

            InternalLibraries = new List<InternalLibrary>();

            SkippedCore = skippedCore;
            dependenciesGlobal = (IList<FileDependency>)dependencies;
            optionSettersGlobal = (IList<Tuple<string, uint>>)optionSetters;
            inputTypeIdGlobal = inputTypeId;

            if (!LibraryName.ToLower().EndsWith(".dll"))
            {
                DLLName = $"{LibraryName}.dll";
            }
            else
            {
                DLLName = LibraryName;
            }
            if (AnyCoreName.Length > 0)
            {
                DLLName = AnyCoreName;
            }
            if (!loadLibrary)
            {
                SkippedCore = false;
                FailedToLoad = false;
            }
            if (!SkippedCore)
            {
                try
                {
                    CoreFileName = Path.GetFileNameWithoutExtension(DLLName);
                    if (loadLibrary)
                    {
                        FreeLibretroCore();
                        LoadCoreLibrary();
                        PrepareCore();
                    }
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                if (DLLName.Contains("\\"))
                {
                    Name = Path.GetFileName(DLLName);
                }
                else
                {
                    Name = DLLName;
                }
                SupportedExtensions = new string[] { ".null" };
                NativeArchiveSupport = true;
                RequiresFullPath = false;
                FailedToLoad = true;
            }
        }

        public void PrepareCore()
        {
            try
            {
                RequestVariablesUpdate = false;
                if (DLLModule == IntPtr.Zero)
                {
                    throw new Exception($"Could not load library: {Marshal.GetLastWin32Error()}");
                }
                FileDependencies = dependenciesGlobal == null ? Array.Empty<FileDependency>() : dependenciesGlobal;
                OptionSetters = optionSettersGlobal == null ? Array.Empty<Tuple<string, uint>>() : optionSettersGlobal;

                var preferredInputTypes = new List<uint> { Constants.RETRO_DEVICE_ANALOG, Constants.RETRO_DEVICE_JOYPAD, Constants.RETRO_DEVICE_KEYBOARD };
                PreferredInputTypes = preferredInputTypes;
                if (loadCustomInput && inputTypeIdGlobal != null && inputTypeIdGlobal.Length > 0)
                {
                    foreach (var iItem in inputTypeIdGlobal)
                    {
                        preferredInputTypes.Insert(0, iItem.Value);
                    }
                }

                InputTypesToUse = new Lazy<uint[]>(DetermineInputTypesToUse, LazyThreadSafetyMode.PublicationOnly);

                var systemInfo = new SystemInfo();
                coreAPI.GetSystemInfo(ref systemInfo);
                Name = systemInfo.LibraryName;
                _ = WriteLog($"Core name: {Name}");

                Version = systemInfo.LibraryVersion;
                _ = WriteLog($"Core version: {Version}");

                if (systemInfo.ValidExtensions == null)
                {
                    _ = WriteLog($"Core extensions: none");
                    SupportedExtensions = new string[] { ".null" };
                }
                else
                {
                    _ = WriteLog($"Core extensions: {systemInfo.ValidExtensions}");
                    SupportedExtensions = systemInfo.ValidExtensions.Split('|').Select(d => $".{d}").ToArray();
                }

                NativeArchiveSupport = systemInfo.BlockExtract;
                _ = WriteLog($"Core NativeArchiveSupport: {NativeArchiveSupport}");

                RequiresFullPath = systemInfo.NeedFullpath;
                _ = WriteLog($"Core RequiresFullPath: {RequiresFullPath}");

                Options = new Dictionary<string, CoreOption>();

                AudioOnly = false;
                VideoOnly = false;
                FrameRate = 0;
                ShowFPSCounter = false;

                //Some cores supports VFS but it will not be called until load game
                //I should change VFSSupport value manually
                switch (CoreFileName)
                {
                    //Options for these cores causing crash, I think because of missing DLLs or restricted commands in UWP
                    //Need to investigate about this issue later
                    case "dosbox_pure_libretro":
                        //Still need more tests, it works fine on ARM but on x64 the VFS is not active!
                        {
#if TARGET_X64
                            VFSSupport = false;
                            _ = WriteLog($"Force VFS-OFF on x64 by default");
#elif TARGET_ARM
#elif TARGET_X86
                           VFSSupport = false;
                           _ = WriteLog($"Force VFS-OFF on x86 by default");
#endif
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                _ = WriteLog(e.Message);
                SupportedExtensions = new string[] { ".null" };
                NativeArchiveSupport = true;
                RequiresFullPath = false;
                FailedToLoad = true;
            }

        }

        public void Dispose()
        {
            try
            {
                SystemRootPath = null;
                SaveRootPath = null;

                if (CurrentlyResolvedCoreOptionValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(CurrentlyResolvedCoreOptionValue);
                    CurrentlyResolvedCoreOptionValue = IntPtr.Zero;
                }
                //GC.Collect();
            }
            catch (Exception e)
            {

            }
            callLogTimer();
        }

        public void Initialize(bool isSubSystem = false)
        {
            try
            {
                SubSystemSupport = isSubSystem;
                coreAPI.EnvironmentCallback = EnvironmentHandler;
                coreAPI.RenderVideoFrameCallback = RenderVideoFrameHandler;
                coreAPI.RenderAudioFrameCallback = RenderAudioFrameHandler;
                coreAPI.RenderAudioFramesCallback = RenderAudioFramesHandler;
                coreAPI.PollInputCallback = PollInputHandler;
                coreAPI.GetInputStateCallback = GetInputStateHandler;
            }
            catch (Exception e)
            {
                _ = WriteLog($"{e.Message}");
            }
            if (coreAPI.CoreFailed)
            {
                SkippedCore = true;
                Name = Path.GetFileName(DLLName);
                SupportedExtensions = new string[] { ".null" };
                NativeArchiveSupport = true;
                RequiresFullPath = false;
                FailedToLoad = true;
            }
        }

        public void DeInitialize()
        {
            try
            {
                coreAPI.SetEnvironmentDelegate = null;
                coreAPI.SetRenderVideoFrameDelegate = null;
                coreAPI.SetRenderAudioFrameDelegate = null;
                coreAPI.SetRenderAudioFramesDelegate = null;
                coreAPI.SetPollInputDelegate = null;
                coreAPI.SetGetInputStateDelegate = null;
            }
            catch (Exception e)
            {

            }
        }

        #region Logger
        [JsonIgnore]
        public bool EnabledDebugLog
        {
            get
            {
                return DebugLogState;
            }
            set
            {
                DebugLogState = value;
            }
        }
        [JsonIgnore]
        public bool EnabledLogFile
        {
            get
            {
                return LogFileState;
            }
            set
            {
                LogFileState = value;
            }
        }


        //There are few cases to ignore game folder selection
        //1-Core supports start without content
        //2-Core started with external single game
        //3-Core started with test content
        [JsonIgnore]
        public bool ignoreGamesFolderSelection = false;

        [JsonIgnore]
        public static StorageFile fileLogLocation;
        [JsonIgnore]
        public static StorageFile fileLogVFSLocation;

        [JsonIgnore]
        string RetriXGoldLogFileLocation
        {
            get
            {
                try
                {
                    var localFolder = ApplicationData.Current.LocalFolder.Path;
                    return $"{localFolder}\\RetriXGoldLog.txt";
                }
                catch { return null; }
            }
        }

        public bool DebugLogState = false;
        public bool LogFileState = false;
        private ObservableCollection<CoreLogItem> LogsList = new ObservableCollection<CoreLogItem>();
        public ObservableCollection<CoreLogItem> GetLogsList()
        {
            return LogsList;
        }
        public void ClearLogs()
        {
            try
            {
                LogsList.Clear();
                forceLogOnHold = false;
                string DateText = DateTime.Now.ToString();
                _ = InsertLog(0, $"**************{DateText}***************");
            }
            catch (Exception ex)
            {

            }
        }
        /*
         * Built with help of the solution below
         * https://github.com/KimNynxx/prueba2/blob/a2ca9257d349f56295bda48010ff33ee171e808b/src/packages/SK.Libretro/Wrapper/LibretroLog.cs
         * 
         */
        bool forceLogOnHold = false;
        private readonly Regex _argumentsRegex = new Regex(@"%(?:\d+\$)?[+-]?(?:[ 0]|'.{1})?-?\d*(?:\.\d+)?([bcdeEufFgGosxX])", RegexOptions.Compiled);
        private void LogHandler(LogLevels level, string format, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12)
        {
            //In 3.0.21 DebugLogState will disable all log
            //I have few suspesios that the log is cause crash in some cases
            //So it's better to still off until user choose to activate it
            if (forceLogOnHold || !DebugLogState)
            {
                return;
            }

            //Some core causing crash becuase of the log, it should be disabled until I can solve the issue
            //Hints: the exceptions related to null reference or memory access violation
            switch (Name)
            {
                case "scummvm":
                    return;
            }
            return;
            try
            {
                var data = GetFormatArgumentCount(format);
                var argumentsToPush = (int)data[0];
                if (argumentsToPush > 12)
                {
                    _ = InsertLog(1, $"Too many arguments ({argumentsToPush}) supplied to retroLogCallback");
                    return;
                }

                switch (level)
                {
                    case LogLevels.Debug:
                        {
                            //Debug logs can be very long and heavy
                            //User can enabled debug log from logs list in-game
                            if (DebugLogState)
                            {
                                var formates = (List<string>)data[1];
                                Sprintf(out string formattedString, format, formates, argumentsToPush >= 1 ? arg1 : IntPtr.Zero, argumentsToPush >= 2 ? arg2 : IntPtr.Zero, argumentsToPush >= 3 ? arg3 : IntPtr.Zero, argumentsToPush >= 4 ? arg4 : IntPtr.Zero, argumentsToPush >= 5 ? arg5 : IntPtr.Zero, argumentsToPush >= 6 ? arg6 : IntPtr.Zero, argumentsToPush >= 7 ? arg7 : IntPtr.Zero, argumentsToPush >= 8 ? arg8 : IntPtr.Zero, argumentsToPush >= 9 ? arg9 : IntPtr.Zero, argumentsToPush >= 10 ? arg10 : IntPtr.Zero, argumentsToPush >= 11 ? arg11 : IntPtr.Zero, argumentsToPush >= 12 ? arg12 : IntPtr.Zero);
                                if (formattedString != null)
                                {
                                    _ = InsertLog(1, $"DEBUG: {formattedString}");
                                    if (LogsList.Count > 500)
                                    {
                                        _ = InsertLog(1, $"WARN: Log suspended due too many requests, clear list to release it");
                                        forceLogOnHold = true;
                                    }
                                }
                            }
                        }
                        break;
                    case LogLevels.Info:
                        {
                            var formates = (List<string>)data[1];
                            Sprintf(out string formattedString, format, formates, argumentsToPush >= 1 ? arg1 : IntPtr.Zero, argumentsToPush >= 2 ? arg2 : IntPtr.Zero, argumentsToPush >= 3 ? arg3 : IntPtr.Zero, argumentsToPush >= 4 ? arg4 : IntPtr.Zero, argumentsToPush >= 5 ? arg5 : IntPtr.Zero, argumentsToPush >= 6 ? arg6 : IntPtr.Zero, argumentsToPush >= 7 ? arg7 : IntPtr.Zero, argumentsToPush >= 8 ? arg8 : IntPtr.Zero, argumentsToPush >= 9 ? arg9 : IntPtr.Zero, argumentsToPush >= 10 ? arg10 : IntPtr.Zero, argumentsToPush >= 11 ? arg11 : IntPtr.Zero, argumentsToPush >= 12 ? arg12 : IntPtr.Zero);
                            if (formattedString != null)
                            {
                                _ = InsertLog(1, $"INFO: {formattedString}");
                            }
                        }
                        break;
                    case LogLevels.Warning:
                        {
                            var formates = (List<string>)data[1];
                            Sprintf(out string formattedString, format, formates, argumentsToPush >= 1 ? arg1 : IntPtr.Zero, argumentsToPush >= 2 ? arg2 : IntPtr.Zero, argumentsToPush >= 3 ? arg3 : IntPtr.Zero, argumentsToPush >= 4 ? arg4 : IntPtr.Zero, argumentsToPush >= 5 ? arg5 : IntPtr.Zero, argumentsToPush >= 6 ? arg6 : IntPtr.Zero, argumentsToPush >= 7 ? arg7 : IntPtr.Zero, argumentsToPush >= 8 ? arg8 : IntPtr.Zero, argumentsToPush >= 9 ? arg9 : IntPtr.Zero, argumentsToPush >= 10 ? arg10 : IntPtr.Zero, argumentsToPush >= 11 ? arg11 : IntPtr.Zero, argumentsToPush >= 12 ? arg12 : IntPtr.Zero);
                            if (formattedString != null)
                            {
                                _ = InsertLog(1, $"WARN: {formattedString}");
                            }
                        }
                        break;
                    case LogLevels.Error:
                        {
                            var formates = (List<string>)data[1];
                            Sprintf(out string formattedString, format, formates, argumentsToPush >= 1 ? arg1 : IntPtr.Zero, argumentsToPush >= 2 ? arg2 : IntPtr.Zero, argumentsToPush >= 3 ? arg3 : IntPtr.Zero, argumentsToPush >= 4 ? arg4 : IntPtr.Zero, argumentsToPush >= 5 ? arg5 : IntPtr.Zero, argumentsToPush >= 6 ? arg6 : IntPtr.Zero, argumentsToPush >= 7 ? arg7 : IntPtr.Zero, argumentsToPush >= 8 ? arg8 : IntPtr.Zero, argumentsToPush >= 9 ? arg9 : IntPtr.Zero, argumentsToPush >= 10 ? arg10 : IntPtr.Zero, argumentsToPush >= 11 ? arg11 : IntPtr.Zero, argumentsToPush >= 12 ? arg12 : IntPtr.Zero);
                            if (formattedString != null)
                            {
                                _ = InsertLog(1, $"ERROR: {formattedString}");
                            }
                        }
                        break;
                    default:
                        {
                            var formates = (List<string>)data[1];
                            Sprintf(out string formattedString, format, formates, argumentsToPush >= 1 ? arg1 : IntPtr.Zero, argumentsToPush >= 2 ? arg2 : IntPtr.Zero, argumentsToPush >= 3 ? arg3 : IntPtr.Zero, argumentsToPush >= 4 ? arg4 : IntPtr.Zero, argumentsToPush >= 5 ? arg5 : IntPtr.Zero, argumentsToPush >= 6 ? arg6 : IntPtr.Zero, argumentsToPush >= 7 ? arg7 : IntPtr.Zero, argumentsToPush >= 8 ? arg8 : IntPtr.Zero, argumentsToPush >= 9 ? arg9 : IntPtr.Zero, argumentsToPush >= 10 ? arg10 : IntPtr.Zero, argumentsToPush >= 11 ? arg11 : IntPtr.Zero, argumentsToPush >= 12 ? arg12 : IntPtr.Zero);
                            if (formattedString != null)
                            {
                                _ = InsertLog(1, $"NORMAL: {formattedString}");
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                _ = InsertLog(1, $"EXCEPTION: {ex.Message}");
            }
        }

        //This function made as Task because it was affecting on VFS loading for some reason
        [JsonIgnore]
        bool loggerInProgress = false;
        [JsonIgnore]
        static List<Action> loggerActions = new List<Action>();
        [JsonIgnore]
        static bool loggerQueueInProgress = false;

        public static void AddNewTask(Action action, LibretroCore core = null)
        {
            try
            {
                if (action != null)
                {
                    loggerActions.Add(action);
                }
                if (!timerState && core != null)
                {
                    var fileName = $"{core.SystemNameStatic}'s Log.txt";
                    fileLogLocation = core.systemRootFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting).AsTask().Result;
                    FileIO.WriteTextAsync(fileLogLocation, $"").AsTask().Wait();

                    var fileVFSName = $"{core.SystemNameStatic}'s VFSLog.txt";
                    fileLogVFSLocation = core.systemRootFolder.CreateFileAsync(fileVFSName, CreationCollisionOption.ReplaceExisting).AsTask().Result;
                    FileIO.WriteTextAsync(fileLogVFSLocation, $"").AsTask().Wait();

                    callLogTimer(true);
                }
            }
            catch (Exception ex)
            {

            }
        }

        [JsonIgnore]
        private static Timer LogTimer;
        [JsonIgnore]
        static bool timerState = false;
        private static void callLogTimer(bool startState = false)
        {
            try
            {
                LogTimer?.Dispose();
                timerState = false;
                if (startState)
                {
                    timerState = true;
                    LogTimer = new Timer(delegate { LoggerQueueCall(); }, null, 0, 1000);
                }
            }
            catch (Exception e)
            {

            }
        }

        public static void AddVFSLog(string message, string memberName = "", int sourceLineNumber = 0)
        {
            try
            {
                var task = new Action(() =>
                {
                    if (fileLogVFSLocation != null)
                    {
                        try
                        {
                            FileIO.AppendTextAsync(fileLogVFSLocation, $"{memberName} ({sourceLineNumber}): {message}\n").AsTask().Wait();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                });
                LibretroCore.AddNewTask(task);
            }
            catch (Exception ex)
            {

            }
        }
        private static async void LoggerQueueCall()
        {
            try
            {
                if (loggerQueueInProgress || loggerActions.Count == 0)
                {
                    return;
                }
                loggerQueueInProgress = true;
                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Action[] actions;
                        lock (loggerActions)
                        {
                            actions = loggerActions.ToArray();
                            loggerActions.Clear();
                        }
                        if (actions != null)
                        {
                            foreach (var action in actions)
                            {
                                await Task.Delay(10);
                                await Task.Run(action);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    taskCompletionSource.SetResult(true);
                });
                await taskCompletionSource.Task;
            }
            catch (Exception ex)
            {

            }
            loggerQueueInProgress = false;
        }
        public async Task<StorageFile> GetLogFile(bool VFS = false, GameSystemViewModel system = null)
        {
            StorageFile logFile = null;
            try
            {
                var fileName = !VFS ? $"{SystemNameStatic}'s Log.txt" : $"{SystemNameStatic}'s VFSLog.txt";
                if (systemRootFolder == null)
                {
                    logFile = (StorageFile)await (await system.GetSystemDirectoryAsync()).TryGetItemAsync(fileName);
                }
                else
                {
                    logFile = (StorageFile)await systemRootFolder.TryGetItemAsync(fileName);
                }
            }
            catch (Exception ex)
            {

            }
            return logFile;
        }
        public async Task InsertLog(int position, string message, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                if (forceLogOnHold)
                {
                    return;
                }

                loggerInProgress = true;
                string type = "info";
                string cleanMessage = message;
                string[] tags = new[] { "error", "vfs error", "retrix", "normal", "info", "debug", "warn", "exception" };
                if (message.Contains("****"))
                {
                    type = "none";
                }
                else
                {
                    foreach (var iItem in tags)
                    {
                        if (message.ToLower().StartsWith($"{iItem}:"))
                        {
                            message = message.Replace($"{iItem.ToUpper()}: ", "");
                            type = iItem;
                            break;
                        }
                    }
                }

                if (LogsList != null || position == -1)
                {
                    if (position != -1)
                    {
                        LogsList.Insert(position, new CoreLogItem(type, $"{message}"));
                    }
                    if (LogFileState)
                    {
                        if (!cleanMessage.EndsWith("\n") && !cleanMessage.EndsWith("\\n") && !cleanMessage.EndsWith("\r") && !cleanMessage.EndsWith("\\r"))
                        {
                            cleanMessage += "\n";
                        }

                        //Ensure files are generated and cleaned
                        AddNewTask(null, this);

                        var task = new Action(() =>
                        {
                            try
                            {
                                if (fileLogLocation != null && position != -1)
                                {
                                    FileIO.AppendTextAsync(fileLogLocation, $"{memberName} ({sourceLineNumber}): {cleanMessage}").AsTask().Wait();
                                    if (PlatformService.LogIndicatorHandler != null)
                                    {
                                        PlatformService.LogIndicatorHandler.Invoke(message, null);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogsList.Insert(position, new CoreLogItem("exception", $"EXCEPTION: {ex.Message}"));
                            }
                        });
                        AddNewTask(task);
                    }
                }
            }
            catch (Exception ex)
            {
                if (position != -1)
                {
                    LogsList.Insert(position, new CoreLogItem("exception", $"EXCEPTION: {ex.Message}"));
                    if (PlatformService.LogIndicatorHandler != null)
                    {
                        PlatformService.LogIndicatorHandler.Invoke(ex.Message, null);
                    }
                }
            }
            loggerInProgress = false;
        }

        bool WriteLogInProgress = false;
        private async Task WriteLog(string message, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (forceLogOnHold)
            {
                return;
            }
            if (!StartupLoading)
            {
                //This log only to track startup issues 
                //there is no need to log anything after that
                return;
            }
            WriteLogInProgress = true;

            try
            {
                if (RetriXGoldLogFileLocation != null)
                {
                    var task = new Action(() =>
                    {
                        try
                        {
                            File.AppendAllText(RetriXGoldLogFileLocation, $"{memberName} ({sourceLineNumber}): {message}\n");
                            if (PlatformService.LogIndicatorHandler != null)
                            {
                                PlatformService.LogIndicatorHandler.Invoke(message, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogsList.Insert(1, new CoreLogItem("exception", $"EXCEPTION: {ex.Message}"));
                        }
                    });
                    AddNewTask(task);
                }
            }
            catch (Exception e)
            {

            }
            WriteLogInProgress = false;
        }
        private object[] GetFormatArgumentCount(string format)
        {
            int argumentsToPush = 0;
            List<string> formates = new List<string>();
            MatchCollection matches = _argumentsRegex.Matches(format);

            foreach (Match match in matches)
            {
                formates.Add($"{match.Value}");
                switch (match.Groups[1].Value)
                {
                    case "b":
                    case "d":
                    case "x":
                    case "s":
                    case "u":
                        argumentsToPush += 1;
                        break;
                    case "f":
                    case "m":
                        argumentsToPush += 2;
                        break;
                    default:
                        _ = InsertLog(1, $"Placeholder '{match.Value}' not implemented");
                        break;
                }
            }

            return new object[] { argumentsToPush, formates };
        }
        private void Sprintf(out string buffer, string format, List<string> formates, params IntPtr[] args)
        {
            var finalMessage = format;
            try
            {
                if (finalMessage != null && formates != null)
                {
                    for (int i = 0; i < formates.Count; i++)
                    {
                        var formatItem = formates[i];
                        if (formatItem != null)
                        {
                            var regex = new Regex(Regex.Escape(formatItem));
                            var replaceValue = args[i].ToString();
                            try
                            {
                                //Still there is crash happens when pointer is number, hope this will prevent that totally
                                var testNumber = long.Parse(replaceValue);
                            }
                            catch (Exception ex)
                            {
                                if (formatItem.StartsWith("%") && formatItem.EndsWith("s"))
                                {
                                    replaceValue = Marshal.PtrToStringAnsi(args[i]);
                                    if (replaceValue == null)
                                    {
                                        replaceValue = args[i].ToString();
                                    }
                                }
                            }
                            if (regex != null && finalMessage != null && replaceValue != null)
                            {
                                finalMessage = regex.Replace(finalMessage, replaceValue, 1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = InsertLog(1, ex.Message);
            }
            buffer = finalMessage;
        }
        #endregion


        public bool LoadGame(string mainGameFilePath, bool vfsIssueRecall = false)
        {
            if (!vfsIssueRecall)
            {
                LogsList.Clear();
            }
            else
            {
                _ = InsertLog(1, "WARN: Emulation Service want to try without VFS as second chance");
            }
            forceLogOnHold = false;
            WriteLogInProgress = false;
            string DateText = DateTime.Now.ToString();
            _ = InsertLog(0, $"**************{DateText}***************");

            try
            {
                if (!IsInitialized)
                {
                    coreAPI.Initialize();
                    IsInitialized = true;
                }

                if (CurrentGameInfo != null && CurrentGameInfo.HasValue)
                {
                    UnloadGameNoDeinit();
                }

                var gameInfo = new GameInfo()
                {
                    Path = mainGameFilePath
                };

                if (!RequiresFullPath)
                {
                    if (mainGameFilePath != null && mainGameFilePath.Length > 0)
                    {
                        var stream = OpenFileStream?.Invoke(mainGameFilePath, FileAccessMode.Read);
                        if (stream == null)
                        {
                            _ = InsertLog(1, "VFS ERROR: Failed to load the game");
                            _ = InsertLog(1, $"VFS ERROR: Cannot open stream for {mainGameFilePath}");
                            FailedToLoadGame = true;
                            return true;
                        }

                        var data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        GameDataHandle = gameInfo.SetGameData(data);
                        CloseFileStream(stream);
                    }
                    else
                    {
                        //There is chance that the core started without content
                        if (!RequestNoGame)
                        {
                            _ = InsertLog(1, "VFS ERROR: Failed to load the game");
                            _ = InsertLog(1, $"VFS ERROR: Cannot open stream null");
                            FailedToLoadGame = true;
                            return true;
                        }
                    }
                }

                Rotation = Rotations.CCW0;
                var loadSuccessful = false;

                if (SubSystemSupport && RetroGameType > 0)
                {
                    loadSuccessful = coreAPI.LoadGameSpecial((uint)RetroGameType, ref gameInfo);
                }
                else
                {
                    if (RequestNoGame && SupportNoGame)
                    {
                        loadSuccessful = coreAPI.StartCore(IntPtr.Zero);
                    }
                    else
                    {
                        loadSuccessful = coreAPI.LoadGame(ref gameInfo);
                    }
                }

                if (loadSuccessful)
                {
                    var avInfo = new SystemAVInfo();
                    coreAPI.GetSystemAvInfo(ref avInfo);

                    Geometry = avInfo.Geometry;
                    Timings = avInfo.Timings;

                    var inputTypesToUse = InputTypesToUse.Value;
                    for (var i = 0; i < inputTypesToUse.Length; i++)
                    {
                        coreAPI.SetControllerPortDevice((uint)i, inputTypesToUse[i]);
                    }

                    CurrentGameInfo = gameInfo;
                }
                else
                {
                    _ = InsertLog(1, "ERROR: Failed to load the game");
                    FailedToLoadGame = true;

                    //Because we handled the error, the return result should be true so the emulation can keep going
                    return true;
                }

                return CurrentGameInfo.HasValue;
            }
            catch (Exception e)
            {
                _ = InsertLog(1, $"EXCEPTION: {e.Message}");
                return false;
            }
        }

        public void UnloadGame()
        {
            try
            {
                RetroGameType = 0U;
                UnloadGameNoDeinit();

                if (IsInitialized)
                {
                    if (!FailedToLoadGame && coreAPI.Cleanup != null)
                    {
                        coreAPI.Cleanup();
                    }
                    IsInitialized = false;
                }
                FailedToLoadGame = false;
            }
            catch (Exception e)
            {

            }

            try
            {
                //VERY IMPORTANT
                //Some cores like (DOSBox-Core) will prevent them self from unload (I don't understand that technically)
                //so in this case the (free command) will cause crash, it's better to avoid that
                //I will keep it loaded always
                if (DLLModule != IntPtr.Zero && SupportUnload)
                {
                    foreach (var internalLibrary in InternalLibraries)
                    {
                        try
                        {
                            internalLibrary.FreeLibrary();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    var releaseState = DLLLoader.FreeLibrary(DLLModule);
                    if (releaseState)
                    {
                        //Reset dll variable so we can load it again
                        DLLModule = IntPtr.Zero;
                    }
                }
            }
            catch (Exception e)
            {

            }
            try
            {
                if (libEGL != IntPtr.Zero)
                {
                    var releaseState = DLLLoader.FreeLibrary(libEGL);
                }
            }
            catch (Exception e)
            {

            }
            try
            {
                if (libGLESv2 != IntPtr.Zero)
                {
                    var releaseState = DLLLoader.FreeLibrary(libGLESv2);
                }
            }
            catch (Exception e)
            {

            }
            Dispose();
        }

        public void Reset()
        {
            try
            {
                if (!FailedToLoadGame)
                {
                    coreAPI.Reset();
                }
                else
                {
                    _ = InsertLog(1, "WARN: Can't reset, the game didn't loaded");
                }
            }
            catch (Exception e)
            {

            }
        }

        public void RunFrame()
        {
            unsafe
            {
                try
                {
                    if (!FailedToLoadGame)
                    {
                        coreAPI.RunFrame();
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        public void UpdateOptionsInGame()
        {
            UpdateCoreOptions();
        }

        public bool SaveState(Stream outputStream)
        {
            try
            {
                if (!FailedToLoadGame)
                {
                    var size = coreAPI.GetSerializationSize();
                    var stateData = new byte[(int)size];

                    var handle = GCHandle.Alloc(stateData, GCHandleType.Pinned);
                    var result = coreAPI.SaveState(handle.AddrOfPinnedObject(), (IntPtr)stateData.Length);
                    handle.Free();

                    if (result == true)
                    {
                        outputStream.Position = 0;
                        outputStream.Write(stateData, 0, stateData.Length);
                        outputStream.SetLength(stateData.Length);
                    }

                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool LoadState(Stream inputStream)
        {
            try
            {
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            try
            {
                if (!FailedToLoadGame)
                {
                    var stateData = new byte[inputStream.Length];
                    inputStream.Position = 0;
                    inputStream.Read(stateData, 0, stateData.Length);

                    var handle = GCHandle.Alloc(stateData, GCHandleType.Pinned);
                    var result = coreAPI.LoadState(handle.AddrOfPinnedObject(), (IntPtr)stateData.Length);
                    handle.Free();

                    try
                    {
                        if (PlatformService.PreventGCAlways)
                        {
                            GC.SuppressFinalize(stateData);
                            stateData = null;

                            inputStream.Dispose();
                            GC.SuppressFinalize(inputStream);
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        if (PlatformService.PreventGCAlways)
                        {
                            GC.EndNoGCRegion();
                        }
                    }
                    catch (Exception ea)
                    {
                    }
                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void UnloadGameNoDeinit()
        {
            try
            {
                if (CurrentGameInfo == null || !CurrentGameInfo.HasValue)
                {
                    return;
                }

                if (!RequestNoGame)
                {
                    coreAPI.UnloadGame();
                }
                if (GameDataHandle.IsAllocated)
                {
                    GameDataHandle.Free();
                }

                CurrentGameInfo = null;
            }
            catch (Exception e)
            {

            }


        }

        #region OpenGLES Libraries
        IntPtr libEGL = new IntPtr();
        IntPtr libGLESv2 = new IntPtr();
        private bool TargetIsARM()
        {
#if _M_ARM || TARGET_ARM
            return true;
#endif
            return false;
        }
        private void LoadGLESLibraries()
        {
            try
            {
                //Don't load again if already loaded
                if (libEGL == IntPtr.Zero)
                {
                    //Load libEGL.dll
                    //In some older builds the new release of ANGLE libraries will fail
                    //so I will try to get the older release in this case
                    var libEGLDLL = "libEGL.dll";
                    libEGL = DLLLoader.LoadLibrary(libEGLDLL);
                    if (libEGL != IntPtr.Zero) // error handling
                    {
                        _ = InsertLog(1, $"ANGLE (GLES): libEGL.dll loaded");
                    }
                    else
                    {
                        if (TargetIsARM())
                        {
                            //Try older version
                            _ = InsertLog(1, $"Failed to load: libEGL.dll");
                            _ = InsertLog(1, $"Could not load library: {Marshal.GetLastWin32Error()}");
                            _ = InsertLog(1, $"Trying older release libEGL_old.dll");
                            libEGLDLL = "libEGL_old.dll";
                            libEGL = DLLLoader.LoadLibrary(libEGLDLL);
                            if (libEGL != IntPtr.Zero) // error handling
                            {

                            }
                            else
                            {
                                _ = InsertLog(1, $"Failed to load: libEGL_old.dll");
                                _ = InsertLog(1, $"Could not load library: {Marshal.GetLastWin32Error()}");
                            }
                        }
                        else
                        {
                            _ = InsertLog(1, $"Failed to load: libEGL.dll");
                            _ = InsertLog(1, $"Could not load library: {Marshal.GetLastWin32Error()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = InsertLog(1, $"EXCEPTION: {ex.Message}");
                _ = InsertLog(1, $"ERROR: Could not load library: {Marshal.GetLastWin32Error()}");
            }

            try
            {
                //Don't load again if already loaded
                if (libGLESv2 == IntPtr.Zero)
                {
                    //Load libGLESv2.dll
                    //In some older builds the new release of ANGLE libraries will fail
                    //so I will try to get the older release in this case
                    var libGLESv2DLL = "libGLESv2.dll";
                    libGLESv2 = DLLLoader.LoadLibrary(libGLESv2DLL);
                    if (libGLESv2 != IntPtr.Zero) // error handling
                    {
                        _ = InsertLog(1, $"ANGLE (GLES): libGLESv2.dll loaded");
                    }
                    else
                    {
                        if (TargetIsARM())
                        {
                            //Try older version
                            _ = InsertLog(1, $"Failed to load: libGLESv2.dll");
                            _ = InsertLog(1, $"Could not load library: {Marshal.GetLastWin32Error()}");
                            _ = InsertLog(1, $"Trying older release libGLESv2_old.dll");
                            libGLESv2DLL = "libGLESv2_old.dll";
                            libGLESv2 = DLLLoader.LoadLibrary(libGLESv2DLL);
                            if (libGLESv2 != IntPtr.Zero) // error handling
                            {

                            }
                            else
                            {
                                _ = InsertLog(1, $"Failed to load: libGLESv2_old.dll");
                                _ = InsertLog(1, $"Could not load library: {Marshal.GetLastWin32Error()}");
                            }
                        }
                        else
                        {
                            _ = InsertLog(1, $"Failed to load: libGLESv2.dll");
                            _ = InsertLog(1, $"Could not load library: {Marshal.GetLastWin32Error()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = InsertLog(1, $"EXCEPTION: {ex.Message}");
                _ = InsertLog(1, $"ERROR: Could not load library: {Marshal.GetLastWin32Error()}");
            }
        }
        #endregion

        IntPtr dataPtrShared = new IntPtr();
        retro_hw_context_type preferred_hw = retro_hw_context_type.RETRO_HW_CONTEXT_DIRECT3D;
        [JsonIgnore]
        HWHandler hwHandler = new HWHandler();
        [JsonIgnore]
        DiskControllerEX diskControllerEX = new DiskControllerEX();
        [JsonIgnore]
        DiskController diskController = new DiskController();
        [JsonIgnore]
        PerfInterface perfFunctions = new PerfInterface();
        [JsonIgnore]
        MidiInterface midiInterfaceDelegates = new MidiInterface();
        [JsonIgnore]
        LEDInterface ledInterfaceDelegates = new LEDInterface();

        [JsonIgnore]
        bool PSXInWideMode = false;
        [JsonIgnore]
        bool PSXCurrentAspectBackup = false;
        [JsonIgnore]
        int[] PSXCurrentAspect = new int[] { 4, 3 };

        [JsonIgnore]
        private Device d3dDevice;
        private DeviceContext d3dContext;
        private bool EnvironmentHandler(uint command, IntPtr dataPtr)
        {
            try
            {
                dataPtrShared = dataPtr;
                _ = WriteLog($"command: {command}->Called, dataPtr: {dataPtr}");

                switch (command)
                {
                    case Constants.RETRO_ENVIRONMENT_GET_LOG_INTERFACE:
                        {
                            Marshal.StructureToPtr(LogCBDescriptor, dataPtr, false);
                            _ = InsertLog(1, "RETRIX: GET_LOG_INTERFACE->Done");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_CORE_OPTIONS:
                        {
                            var newOptions = new Dictionary<string, CoreOption>();
                            Options = newOptions;

                            var data = Marshal.PtrToStructure<retro_core_option_definition>(dataPtr);
                            while (data.key != IntPtr.Zero)
                            {
                                try
                                {
                                    var key = data.Key;
                                    var description = data.Desc;
                                    string defaultValue = data.DefaultValue;
                                    List<string> values = new List<string>();
                                    foreach (var oItem in data.values)
                                    {
                                        if (oItem.Value == null)
                                        {
                                            break;
                                        }
                                        values.Add(oItem.Value);
                                    }
                                    newOptions.Add(key, new CoreOption(description, values));

                                    dataPtr += Marshal.SizeOf<retro_core_option_definition>();
                                    data = Marshal.PtrToStructure<retro_core_option_definition>(dataPtr);
                                }
                                catch (Exception ex)
                                {
                                    _ = InsertLog(1, $"RETRIX: {ex.Message}");
                                    _ = WriteLog($"RETRO_ENVIRONMENT_SET_CORE_OPTIONS-> {ex.Message}");
                                }
                            }

                            foreach (var i in OptionSetters)
                            {
                                Options[i.Item1].SelectedValueIx = i.Item2;
                            }
                            if (DebugLogState)
                            {
                                _ = InsertLog(1, "RETRIX: SET_CORE_OPTIONS->Done");
                            }
                            if (Options != null && Options.Count > 0)
                            {
                                IsInGameOptionsActive = true;
                                var changesState = GameSystemViewModel.SetCoreOptions(this, SystemName);
                                //Some cores the options will be called during initialization
                                //So I have to always call update after filling the values
                                //Usually these values will get updated at EmulationService.cs before game loaded
                                UpdateCoreOptionsValues(changesState);
                            }
                            else
                            {
                                IsInGameOptionsActive = false;
                            }
                            if (PlatformService.raiseInGameOptionsActiveState != null)
                            {
                                PlatformService.raiseInGameOptionsActiveState.Invoke(IsInGameOptionsActive, null);
                            }
                            _ = InsertLog(1, "RETRIX: RETRO_ENVIRONMENT_SET_CORE_OPTIONS->Done");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_VARIABLES:
                        {
                            var newOptions = new Dictionary<string, CoreOption>();
                            Options = newOptions;

                            var data = Marshal.PtrToStructure<LibretroVariable>(dataPtr);
                            while (data.KeyPtr != IntPtr.Zero)
                            {
                                try
                                {
                                    var key = Marshal.PtrToStringAnsi(data.KeyPtr);
                                    var rawValue = Marshal.PtrToStringAnsi(data.ValuePtr);

                                    var split = rawValue.Split(';');
                                    var description = split[0];

                                    rawValue = rawValue.Substring(description.Length + 2);
                                    split = rawValue.Split('|');
                                    var valuesList = split.ToList();
                                    switch (Name)
                                    {
                                        case "Opera":
                                            switch (key)
                                            {
                                                case "opera_bios":
                                                    if (valuesList.Count == 1)
                                                    {
                                                        //Opera BIOS list is not updated when BIOS available 
                                                        //I will add the values here manually
                                                        var OperaBIOSFiles = new Dictionary<string, string>()
                                                        {
                                                            { "panafz1.bin", "Panasonic FZ-1 (U)" },
                                                            { "panafz1j.bin", "Panasonic FZ-1 (J)" },
                                                            { "panafz1j-norsa.bin", "Panasonic FZ-1 (J) [No RSA]" },
                                                            { "panafz10.bin", "Panasonic FZ-10 (U)" },
                                                            { "panafz10-norsa.bin", "Panasonic FZ-10 (U) [No RSA]" },
                                                            { "panafz10e-anvil-norsa.bin", "Panasonic FZ-10 (E) ANVIL [No RSA]" },
                                                            { "goldstar.bin", "Goldstar GDO-101M" },
                                                            { "sanyotry.bin", "Sanyo Try IMP-21J" },
                                                            { "3do_arcade_saot.bin", "3DO Arcade - SAOT" },
                                                        };
                                                        //if (systemRootFolder != null)
                                                        {
                                                            foreach (var oItem in OperaBIOSFiles.Reverse())
                                                            {
                                                                //var testFile = Task.Run(() => systemRootFolder.TryGetItemAsync(oItem.Key).AsTask()).Result;
                                                                //if (testFile != null)
                                                                {
                                                                    valuesList.Insert(0, oItem.Value);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;

                                                case "opera_font":
                                                    if (valuesList.Count == 1)
                                                    {
                                                        //Opera BIOS list is not updated when BIOS available 
                                                        //I will add the values here manually
                                                        var OperaBIOSFiles = new Dictionary<string, string>()
                                                        {
                                                            { "panafz1-kanji.bin", "Panasonic FZ-1 (J) Kanji ROM" },
                                                            { "panafz1j-kanji.bin", "Panasonic FZ-1 (J) Kanji ROM" },
                                                            { "panafz10ja-anvil-kanji.bin", "Panasonic FZ-10 (J) ANVIL Kanji ROM" },
                                                        };
                                                        //if (systemRootFolder != null)
                                                        {
                                                            foreach (var oItem in OperaBIOSFiles.Reverse())
                                                            {
                                                                //var testFile = Task.Run(() => systemRootFolder.TryGetItemAsync(oItem.Key).AsTask()).Result;
                                                                //if (testFile != null)
                                                                {
                                                                    valuesList.Add(oItem.Value);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                            break;

                                    }
                                    newOptions.Add(key, new CoreOption(description, valuesList));

                                    dataPtr += Marshal.SizeOf<LibretroVariable>();
                                    data = Marshal.PtrToStructure<LibretroVariable>(dataPtr);
                                }
                                catch (Exception ex)
                                {
                                    _ = InsertLog(1, $"RETRIX: {ex.Message}");
                                    _ = WriteLog($"RETRO_ENVIRONMENT_SET_VARIABLES-> {ex.Message}");
                                }
                            }

                            foreach (var i in OptionSetters)
                            {
                                Options[i.Item1].SelectedValueIx = i.Item2;
                            }
                            if (DebugLogState)
                            {
                                _ = InsertLog(1, "RETRIX: SET_VARIABLES->Done");
                            }
                            if (Options != null && Options.Count > 0)
                            {
                                IsInGameOptionsActive = true;
                                var changesState = GameSystemViewModel.SetCoreOptions(this, SystemName);
                                //Some cores the options will be called during initialization
                                //So I have to always call update after filling the values
                                //Usually these values will get updated at EmulationService.cs before game loaded
                                UpdateCoreOptionsValues(changesState);
                            }
                            else
                            {
                                IsInGameOptionsActive = false;
                            }
                            if (PlatformService.raiseInGameOptionsActiveState != null)
                            {
                                PlatformService.raiseInGameOptionsActiveState.Invoke(IsInGameOptionsActive, null);
                            }
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_VARIABLE:
                        {
                            var data = Marshal.PtrToStructure<LibretroVariable>(dataPtr);
                            var key = Marshal.PtrToStringAnsi(data.KeyPtr);

                            switch (CoreFileName)
                            {
                                //Options for these cores causing crash, I think because of missing DLLs or restricted commands in UWP
                                //Need to investigate about this issue later
                                case "dosbox_svn_libretro":
                                    //Ignore
                                    if (DebugLogState)
                                    {
                                        _ = InsertLog(1, $"RETRIX: GET_VARIABLE ({key}) Ignored because of UWP issue");
                                    }
                                    switch (key)
                                    {
                                        case "KEY TO ALLOW":
                                            //Allow
                                            break;

                                        default:
                                            //Ignore
                                            return false;
                                    }
                                    break;
                            }
                            switch (key)
                            {
                                case "KEY TO IGNORE":
                                    //Ignore
                                    if (DebugLogState)
                                    {
                                        _ = InsertLog(1, $"RETRIX: GET_VARIABLE ({key}) Ignored because of UWP issue");
                                    }
                                    return false;
                            }
                            Task.Delay(10).Wait();
                            var valueFound = false;
                            var valueNull = false;
                            data.ValuePtr = IntPtr.Zero;
                            string value = null;
                            if (Options.ContainsKey(key))
                            {
                                try
                                {
                                    valueFound = true;
                                    var coreOption = Options[key];
                                    value = coreOption.Values[(int)coreOption.SelectedValueIx];
                                    try
                                    {
                                        switch (Name)
                                        {
                                            case "Beetle PSX":
                                                {
                                                    switch (key)
                                                    {
                                                        case "beetle_psx_widescreen_hack":
                                                            //PSXInWideMode = value == "enabled";
                                                            break;
                                                        case "beetle_psx_widescreen_hack_aspect_ratio":
                                                            /*if (PSXInWideMode)
                                                            {
                                                                var targetAspect = GamePlayerView.aspects[value];
                                                                if (!PSXCurrentAspectBackup)
                                                                {
                                                                    PSXCurrentAspect = targetAspect;
                                                                    PSXCurrentAspectBackup = true;
                                                                }
                                                                GamePlayerView.ASR = targetAspect;
                                                            }
                                                            else
                                                            {
                                                                if (PSXCurrentAspectBackup)
                                                                {
                                                                    GamePlayerView.ASR = PSXCurrentAspect;
                                                                    PSXCurrentAspectBackup = false;
                                                                }
                                                            }
                                                            PlatformService.ResolveCanvasSizeHandler?.Invoke(null, null);*/
                                                            break;

                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    catch (Exception e)
                                    {

                                    }
                                    if (value != null && value.Length > 0)
                                    {
                                        if (CurrentlyResolvedCoreOptionValue != IntPtr.Zero)
                                        {
                                            Marshal.FreeHGlobal(CurrentlyResolvedCoreOptionValue);
                                        }

                                        CurrentlyResolvedCoreOptionValue = Marshal.StringToHGlobalAnsi(value);
                                        data.ValuePtr = CurrentlyResolvedCoreOptionValue;
                                    }
                                    else
                                    {
                                        valueNull = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _ = InsertLog(1, $"RETRIX: {ex.Message}");
                                }
                            }

                            Marshal.StructureToPtr(data, dataPtr, false);
                            if (DebugLogState)
                            {
                                if (valueNull)
                                {
                                    _ = InsertLog(1, $"RETRIX: GET_VARIABLE ({key})->value is null or empty!");
                                }
                                else
                                {
                                    _ = InsertLog(1, $"RETRIX: GET_VARIABLE ({key})->{(valueFound ? value : "Not found!")}");
                                }
                            }
                            return valueFound && !valueNull;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_OVERSCAN:
                        {
                            Marshal.WriteByte(dataPtr, 0);
                            _ = InsertLog(1, "RETRIX: GET_OVERSCAN->0");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_CAN_DUPE:
                        {
                            Marshal.WriteByte(dataPtr, 1);
                            _ = InsertLog(1, "RETRIX: GET_CAN_DUPE->1");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY:
                        {
                            Marshal.WriteIntPtr(dataPtr, systemRootPathUnmanaged);
                            _ = InsertLog(1, $"RETRIX: GET_SYSTEM_DIRECTORY->{systemRootPath}");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_CONTENT_DIRECTORY:
                        {
                            Marshal.WriteIntPtr(dataPtr, systemGamesPathUnmanaged);
                            _ = InsertLog(1, $"RETRIX: GET_CONTENT_DIRECTORY->{systemGamesPath}");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:
                        {
                            Marshal.WriteIntPtr(dataPtr, saveRootPathUnmanaged);
                            _ = InsertLog(1, $"RETRIX: GET_SAVE_DIRECTORY->{saveRootPath}");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_LIBRETRO_PATH:
                        {
                            Marshal.WriteIntPtr(dataPtr, systemRootPathUnmanaged);
                            _ = InsertLog(1, $"RETRIX: GET_LIBRETRO_PATH->{systemRootPath}");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:
                        {
                            var data = (PixelFormats)Marshal.ReadInt32(dataPtr);
                            PixelFormat = data;
                            _ = InsertLog(1, $"RETRIX: SET_PIXEL_FORMAT->{PixelFormat.ToString()}");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_GEOMETRY:
                        {
                            var data = Marshal.PtrToStructure<GameGeometry>(dataPtr);
                            Geometry = data;
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_ROTATION:
                        {
                            var data = (Rotations)Marshal.ReadInt32(dataPtr);
                            Rotation = data;
                            _ = InsertLog(1, $"RETRIX: SET_ROTATION->{Rotation.ToString()}");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME:
                        {
                            var data = Marshal.PtrToStructure<bool>(dataPtr);
                            SupportNoGame = data;
                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME->Called ({SupportNoGame})");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_SYSTEM_AV_INFO:
                        {
                            var data = Marshal.PtrToStructure<SystemAVInfo>(dataPtr);
                            Geometry = data.Geometry;
                            Timings = data.Timings;
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK:
                        {
                            var info = Marshal.PtrToStructure<retro_keyboard_callback>(dataPtr);
                            PlatformService.KeyboardEvent = info.Callback;
                            PlatformService.useNativeKeyboard = PlatformService.useNativeKeyboardByDefault;
                            if (PlatformService.GamePlayPageUpdateBindings != null)
                            {
                                PlatformService.GamePlayPageUpdateBindings.Invoke(null, null);
                            }
                            if (DebugLogState)
                            {
                                _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK->Called");
                            }
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_CONTROLLER_INFO:
                        {
                            IntPtr portDescriptionsPtr;
                            SupportedInputsPerPort.Clear();
                            do
                            {
                                var portControllerData = Marshal.PtrToStructure<ControllerInfo>(dataPtr);
                                portDescriptionsPtr = portControllerData.DescriptionsPtr;
                                if (portDescriptionsPtr != IntPtr.Zero)
                                {
                                    var currentPortDescriptions = new List<ControllerDescription>();
                                    SupportedInputsPerPort.Add(currentPortDescriptions);
                                    for (var i = 0U; i < portControllerData.NumDescriptions; i++)
                                    {
                                        var nativeDescription = Marshal.PtrToStructure<ControllerDescription.NativeForm>(portDescriptionsPtr);
                                        currentPortDescriptions.Add(new ControllerDescription(nativeDescription));
                                        portDescriptionsPtr += Marshal.SizeOf<ControllerDescription.NativeForm>();
                                    }

                                    dataPtr += Marshal.SizeOf<ControllerInfo>();
                                }
                            }
                            while (portDescriptionsPtr != IntPtr.Zero);
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_VFS_INTERFACE:
                        {
                            var data = Marshal.PtrToStructure<VFSInterfaceInfo>(dataPtr);
                            if (data.RequiredInterfaceVersion <= VFSHandler.SupportedVFSVersion)
                            {
                                data.RequiredInterfaceVersion = VFSHandler.SupportedVFSVersion;
                                data.Interface = vfsHandler.VFSInterfacePtr;
                                Marshal.StructureToPtr(data, dataPtr, false);
                            }
                            _ = InsertLog(1, $"RETRIX: GET_VFS_INTERFACE->Done (v{VFSHandler.SupportedVFSVersion})");
                            VFSSupport = true;
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS:
                        {
                            var inputDesc = Marshal.PtrToStructure<retro_input_descriptor>(dataPtr);
                            while (inputDesc.description != IntPtr.Zero)
                            {
                                try
                                {
                                    InputService.UpdateInputDescriptionByIndex(inputDesc);
                                    if (inputDesc.Port + 1 <= InputService.supportedPorts)
                                    {
                                        try
                                        {
                                            if (DebugLogState)
                                            {
                                                var index = inputDesc.Id;
                                                if (index < InputService.libretro_btn_desc.Length)
                                                {
                                                    var button = InputService.libretro_btn_desc[index];
                                                    _ = InsertLog(1, $"RETRIX: DESCRIPTORS ({inputDesc.Port})-> {button} -> {inputDesc.Description}");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    dataPtr += Marshal.SizeOf<retro_input_descriptor>();
                                    inputDesc = Marshal.PtrToStructure<retro_input_descriptor>(dataPtr);
                                }
                                catch (Exception ex)
                                {
                                    _ = WriteLog(ex.Message);
                                    _ = InsertLog(1, $"{ex.Message}");
                                    break;
                                }
                            }
                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS->Called");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SHUTDOWN:
                        {
                            _ = InsertLog(1, $"RETRIX: ENVIRONMENT_SHUTDOWN->Called");
                            if (StopGame != null)
                            {
                                StopGame.Invoke();
                            }
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_SUPPORT_ACHIEVEMENTS:
                        {
                            var data = Marshal.PtrToStructure<bool>(dataPtr);
                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_SUPPORT_ACHIEVEMENTS->{data}");
                            return false;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_PERF_INTERFACE:
                        {
                            //This code below causing crash, I need to understand the full process in better way
                            /*var perfData = Marshal.PtrToStructure<retro_perf_callback>(dataPtr);
                            perfData.get_time_usec = perfFunctions.cpu_features_get_time_usec;
                            perfData.get_cpu_features = perfFunctions.cpu_features_get;
                            perfData.get_perf_counter = perfFunctions.cpu_features_get_perf_counter;

                            perfData.perf_register = perfFunctions.runloop_performance_counter_register;
                            perfData.perf_start = perfFunctions.core_performance_counter_start;
                            perfData.perf_stop = perfFunctions.core_performance_counter_stop;
                            perfData.perf_log = perfFunctions.runloop_perf_log;

                            Marshal.StructureToPtr(perfData, dataPtr, false);
                            _ = InsertLog(1, "RETRIX: RETRO_ENVIRONMENT_GET_PERF_INTERFACE->Done");*/
                            return false;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE:
                        {
                            if (RequestVariablesUpdate)
                            {
                                RequestVariablesUpdate = false;
                                Marshal.WriteByte(dataPtr, 1);
                                _ = InsertLog(1, $"RETRIX: VARIABLE_UPDATE->Called");
                            }
                            else
                            {
                                Marshal.WriteByte(dataPtr, 0);
                            }
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_MEMORY_MAPS:
                        {
                            var mmapStruct = Marshal.PtrToStructure<retro_memory_map>(dataPtr);
                            var mmapDescPtr = mmapStruct.descriptors;
                            var mmapDesc = Marshal.PtrToStructure<retro_memory_descriptor>(mmapDescPtr);

                            for (var i = 0; i < mmapStruct.num_descriptors; i++)
                            {
                                try
                                {
                                    mmapDescPtr += Marshal.SizeOf<retro_memory_descriptor>();
                                    mmapDesc = Marshal.PtrToStructure<retro_memory_descriptor>(mmapDescPtr);
                                }
                                catch (Exception ex)
                                {
                                    _ = WriteLog(ex.Message);
                                    _ = InsertLog(1, $"{ex.Message}");
                                }
                            }
                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_MEMORY_MAPS->false");
                            return false;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_TARGET_REFRESH_RATE:
                        {
                            float rate = 60.0f;
                            Marshal.WriteByte(dataPtr, Convert.ToByte(rate));
                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_GET_TARGET_REFRESH_RATE->{rate}");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_FASTFORWARDING:
                        {
                            if (fastForwardState)
                            {
                                Marshal.WriteByte(dataPtr, 1);
                            }
                            else
                            {
                                Marshal.WriteByte(dataPtr, 0);
                            }
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_MIDI_INTERFACE:
                        {
                            var midiInterface = Marshal.PtrToStructure<retro_midi_interface>(dataPtr);
                            midiInterface.input_enabled = midiInterfaceDelegates.retro_midi_input_enabled;
                            midiInterface.output_enabled = midiInterfaceDelegates.retro_midi_output_enabled;
                            midiInterface.read = midiInterfaceDelegates.retro_midi_read;
                            midiInterface.write = midiInterfaceDelegates.retro_midi_write;
                            midiInterface.flush = midiInterfaceDelegates.retro_midi_flush;
                            Marshal.StructureToPtr(midiInterface, dataPtr, false);
                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_GET_MIDI_INTERFACE->Done");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE:
                        {
                            var controller = Marshal.PtrToStructure<retro_disk_control_callback>(dataPtr);
                            diskController.retro_set_eject_state = controller.set_eject_state;
                            diskController.retro_get_eject_state = controller.get_eject_state;
                            diskController.retro_get_image_index = controller.get_image_index;
                            diskController.retro_set_image_index = controller.set_image_index;
                            diskController.retro_get_num_images = controller.get_num_images;
                            diskController.retro_replace_image_index = controller.replace_image_index;
                            diskController.retro_add_image_index = controller.add_image_index;

                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE->Done");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_DISK_CONTROL_EXT_INTERFACE:
                        {
                            var controllerEx = Marshal.PtrToStructure<retro_disk_control_ext_callback>(dataPtr);
                            diskControllerEX.retro_set_eject_state = controllerEx.set_eject_state;
                            diskControllerEX.retro_get_eject_state = controllerEx.get_eject_state;
                            diskControllerEX.retro_get_image_index = controllerEx.get_image_index;
                            diskControllerEX.retro_set_image_index = controllerEx.set_image_index;
                            diskControllerEX.retro_get_num_images = controllerEx.get_num_images;
                            diskControllerEX.retro_replace_image_index = controllerEx.replace_image_index;
                            diskControllerEX.retro_add_image_index = controllerEx.add_image_index;
                            diskControllerEX.retro_set_initial_image = controllerEx.set_initial_image;
                            diskControllerEX.retro_get_image_path = controllerEx.get_image_path;
                            diskControllerEX.retro_get_image_label = controllerEx.get_image_label;

                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_DISK_CONTROL_EXT_INTERFACE->Done");
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_SET_HW_RENDER:
                        {
                            var data = Marshal.PtrToStructure<retro_hw_render_callback>(dataPtr);
                            var type = data.context_type;
                            switch (type)
                            {
                                case retro_hw_context_type.RETRO_HW_CONTEXT_DIRECT3D:
                                    _ = InsertLog(1, $"RETRIX: SET_HW_RENDER->DIRECT3D");

                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL:
                                    _ = InsertLog(1, $"RETRIX: SET_HW_RENDER->OPENGL");
                                    LoadGLESLibraries();
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_OPENGLES2:
                                    _ = InsertLog(1, $"RETRIX: SET_HW_RENDER->OPENGLES2");
                                    LoadGLESLibraries();
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_OPENGLES3:
                                    _ = InsertLog(1, $"RETRIX: SET_HW_RENDER->OPENGLES3");
                                    LoadGLESLibraries();
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_VULKAN:
                                    _ = InsertLog(1, $"RETRIX: SET_HW_RENDER->VULKAN");
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_DUMMY:
                                    _ = InsertLog(1, $"RETRIX: SET_HW_RENDER->DUMMY");
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_NONE:
                                    _ = InsertLog(1, $"RETRIX: SET_HW_RENDER->NONE");
                                    break;
                            }
                            /*retro_hw_render_callback context = new retro_hw_render_callback()
                            {
                                context_type = type,
                                version_major = data.version_major,
                                version_minor = data.version_minor,
                                depth = data.depth,
                                stencil = data.stencil,
                                context_reset = data.context_reset,
                                context_destroy = data.context_destroy,
                                get_current_framebuffer = hwHandler.CurrentFrameBuffer,
                                get_proc_address = hwHandler.ProcessAddress
                            };*/
                            data.get_proc_address = hwHandler.ProcessAddress;
                            data.get_current_framebuffer = hwHandler.CurrentFrameBuffer;
                            Marshal.StructureToPtr(data, dataPtr, false);
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE:
                        {
                            var data = Marshal.PtrToStructure<HWInterfaceD3D>(dataPtr);
                           
                            var creationFlags = DeviceCreationFlags.BgraSupport;

#if DEBUG
                            // If the project is in a debug build, enable debugging via SDK Layers.
                            creationFlags |= DeviceCreationFlags.Debug;
#endif

                            FeatureLevel[] featureLevels =
                            {
                               FeatureLevel.Level_11_1,
                               FeatureLevel.Level_11_0,
                               FeatureLevel.Level_10_1,
                               FeatureLevel.Level_10_0,
                               FeatureLevel.Level_9_3,
                               FeatureLevel.Level_9_2,
                               FeatureLevel.Level_9_1,
                             };

                            Utilities.Dispose(ref d3dDevice);
                            Utilities.Dispose(ref d3dContext);

                            SharpDX.D3DCompiler.Linker linker = new SharpDX.D3DCompiler.Linker();

                            // Create the Direct3D 11 API device object.
                            d3dDevice = new Device(DriverType.Hardware, creationFlags, featureLevels);
                            d3dContext = d3dDevice.ImmediateContext;

                            data.interface_type = retro_hw_render_interface_type.RETRO_HW_RENDER_INTERFACE_D3D11;
                            data.interface_version = RETRO_HW_RENDER_INTERFACE_D3D11_VERSION;
                            data.handle = IntPtr.Zero;
                            data.device = ((IntPtr)d3dDevice);
                            data.context = ((IntPtr)d3dContext);
                            data.featureLevel = FeatureLevel.Level_11_1;
                            //data.D3DCompile = ;

                            Marshal.StructureToPtr(data, dataPtr, false);
                            return true;
                        }
                    case Constants.RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER:
                        {
                            switch (preferred_hw)
                            {
                                case retro_hw_context_type.RETRO_HW_CONTEXT_DIRECT3D:
                                    _ = InsertLog(1, $"RETRIX: GET_PREFERRED_HW->DIRECT3D");
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL:
                                    _ = InsertLog(1, $"RETRIX: GET_PREFERRED_HW->OPENGL");
                                    LoadGLESLibraries();
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_OPENGLES2:
                                    _ = InsertLog(1, $"RETRIX: GET_PREFERRED_HW->OPENGLES2");
                                    LoadGLESLibraries();
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_OPENGLES3:
                                    _ = InsertLog(1, $"RETRIX: GET_PREFERRED_HW->OPENGLES3");
                                    LoadGLESLibraries();
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_VULKAN:
                                    _ = InsertLog(1, $"RETRIX: GET_PREFERRED_HW->VULKAN");
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_DUMMY:
                                    _ = InsertLog(1, $"RETRIX: GET_PREFERRED_HW->DUMMY");
                                    break;
                                case retro_hw_context_type.RETRO_HW_CONTEXT_NONE:
                                    _ = InsertLog(1, $"RETRIX: GET_PREFERRED_HW->NONE");
                                    break;
                            }
                            Marshal.WriteInt32(dataPtr, (int)preferred_hw);
                            return true;
                        }
                    case RETRO_ENVIRONMENT_GET_MESSAGE_INTERFACE_VERSION:
                        {
                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_GET_MESSAGE_INTERFACE_VERSION->1");
                            Marshal.WriteInt32(dataPtr, 1);
                            return true;
                        }
                    case RETRO_ENVIRONMENT_GET_LED_INTERFACE:
                        {
                            var ledInterface = Marshal.PtrToStructure<retro_led_interface>(dataPtr);
                            ledInterface.set_led_state = ledInterfaceDelegates.retro_set_led_state;
                            Marshal.StructureToPtr(ledInterface, dataPtr, false);
                            _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_GET_LED_INTERFACE");
                            return true;
                        }
                    case RETRO_ENVIRONMENT_SET_MESSAGE:
                        {
                            try
                            {
                                var data = Marshal.PtrToStructure<retro_message>(dataPtr);
                                string message = data.Message;
                                if (message.EndsWith('\n'))
                                {
                                    message = message.TrimEnd('\n');
                                }
                                var frames = Math.Round((double)(data.Frames / 60.0f));
                                PlatformService.ShowNotificationDirect(message, (int)frames);
                                _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_MESSAGE->{message}");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_MESSAGE->Exception");
                                _ = InsertLog(1, $"RETRIX: {ex.Message}");
                                return false;
                            }
                        }

                    case RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL:
                        {
                            try
                            {
                                var performanceLevel = Marshal.ReadInt32(dataPtr);
                                _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL->{performanceLevel}");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                _ = InsertLog(1, $"RETRIX: RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL->{ex.Message}");
                                return false;
                            }
                        }
                    default:
                        {
                            _ = WriteLog($"command: {command}->Not found!");
                            return false;
                        }
                }
            }
            catch (Exception e)
            {
                RequestVariablesUpdate = false;
                return false;
            }
        }

        public void UpdateCoreOptionsValues(bool changesState)
        {
            try
            {
                if (SystemName == null)
                {
                    //SystemName can be null during RetriXGold startup
                    //in this case there is no need to update any option
                    return;
                }
                var expectedName = $"{Name}_{SystemName}";
                CoresOptions TargetSystem;
                if (!GameSystemSelectionViewModel.SystemsOptions.TryGetValue(expectedName, out TargetSystem))
                {
                    if (!IsNewCore)
                    {
                        GameSystemSelectionViewModel.SystemsOptions.TryGetValue(SystemName, out TargetSystem);
                    }
                }

                if (TargetSystem != null)
                {
                    foreach (var optionItem in TargetSystem.OptionsList.Keys)
                    {
                        try
                        {
                            var optionObject = TargetSystem.OptionsList[optionItem];
                            CoreOption coreOption;
                            if (Options.TryGetValue(optionObject.OptionsKey, out coreOption))
                            {
                                Options[optionObject.OptionsKey].SelectedValueIx = optionObject.SelectedIndex;
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }
                    }
                }
                _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await GameSystemSelectionViewModel.CoreOptionsStoreAsyncDirect(Name, SystemName, IsNewCore, "", !changesState);
                });
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        public static bool RequestVariablesUpdate = false;
        public bool fastForwardState = false;
        public void UpdateCoreOptions()
        {
            RequestVariablesUpdate = true;
        }

        #region Handlers
        unsafe private void RenderVideoFrameHandler(IntPtr data, uint width, uint height, UIntPtr pitch)
        {
            if (AudioOnly)
            {
                return;
            }

            if (NativeSkipFrames)
            {
                if (SkipFrameRelay)
                {
                    SkippedFramesCount++;
                    if (SkippedFramesCount == GetSkipFrameValue())
                    {
                        SkipFrameRelay = false;
                    }
                    return;
                }
                else
                {
                    SkippedFramesCount = 0;
                    SkipFrameRelay = true;
                }
            }
            if (NativeSkipFramesRandom)
            {
                try
                {
                    var SkipState = new Random().Next(0, 10);
                    if (SkipState == 5)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            try
            {
                /*var size = (int)height * (int)pitch;

                var payload = default(ReadOnlySpan<byte>);
                if (data != IntPtr.Zero)
                {
                    payload = new ReadOnlySpan<byte>(data.ToPointer(), size);
                }*/
                UpdateFrameRate();
                RenderVideoFrame?.Invoke(data, width, height, (uint)pitch);
            }
            catch (Exception e)
            {

            }
        }

        private void UpdateFrameRate()
        {
            if (ShowFPSCounter)
            {
                Interlocked.Increment(ref frameRate);
            }
            else
            {
                Interlocked.Exchange(ref frameRate, 0);
                ;
            }
        }

        private bool SkipFrameRelay = false;
        private int SkippedFramesCount = 0;
        private Random RandomSkipFrames = new Random();
        private int GetSkipFrameValue()
        {
            int SkipFrameValue = 1;
            return SkipFrameValue;
        }

        private unsafe void RenderAudioFrameHandler(short left, short right)
        {
            try
            {
                RenderAudioFrameBuffer[0] = left;
                RenderAudioFrameBuffer[1] = right;
                RenderAudioFrames?.Invoke(RenderAudioFrameBuffer.AsSpan(), 1);
            }
            catch (Exception e)
            {

            }
        }

        private unsafe UIntPtr RenderAudioFramesHandler(IntPtr data, UIntPtr numFrames)
        {
            if (VideoOnly)
            {
                return UIntPtr.Zero;
            }
            try
            {
                var payload = new ReadOnlySpan<short>(data.ToPointer(), (int)numFrames * AudioSamplesPerFrame);
                var output = RenderAudioFrames?.Invoke(payload, ((uint)numFrames));
                return (UIntPtr)output;
            }
            catch (Exception e)
            {
                return UIntPtr.Zero;
            }
        }

        private void PollInputHandler()
        {
            try
            {
                PollInput?.Invoke();
            }
            catch (Exception e)
            {

            }
        }

        private short GetInputStateHandler(uint port, uint device, uint index, uint id)
        {
            try
            {
                var inputType = Converter.ConvertToInputType(device, index, id);
                var result = GetInputState?.Invoke(device, port, inputType);
                return result ?? 0;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private void SetStringAndUnmanagedMemory(string newValue, ref string store, ref IntPtr unmanagedPtr)
        {
            try
            {
                store = newValue;
                if (unmanagedPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(unmanagedPtr);
                    unmanagedPtr = IntPtr.Zero;
                }

                if (newValue != null)
                {
                    unmanagedPtr = Marshal.StringToHGlobalAnsi(newValue);
                }
            }
            catch (Exception e)
            {

            }
        }

        private uint[] DetermineInputTypesToUse()
        {
            try
            {
                uint[] result = new uint[SupportedInputsPerPort.Count];

                for (var i = 0; i < SupportedInputsPerPort.Count; i++)
                {
                    result[i] = Constants.RETRO_DEVICE_NONE;
                    var iItem = SupportedInputsPerPort[i];

                    var currentPortSupportedInputsIds = new HashSet<uint>(iItem.Select(e => e.Id));

                    bool deviceSelected = false;
                    try
                    {
                        //Check first mapped ports
                        //This step important when user specify one input, so I have to tell the core that it's preferred
                        var targetPort = InputService.ControllersPortsMap[i];
                        if (targetPort.controllers.Count == 1 || (targetPort.controllers.Count == 2 && targetPort.controllers.Contains("Touch")))
                        {
                            var targetWithoutTouch = targetPort.controllers.Where(a => !a.Equals("Touch"));
                            var targetDevice = InputService.ControllersList[targetWithoutTouch.FirstOrDefault()];
                            if (currentPortSupportedInputsIds.Contains((uint)targetDevice))
                            {
                                result[i] = (uint)targetDevice;
                                deviceSelected = true;
                            }
                            if (!deviceSelected && targetDevice == Constants.RETRO_DEVICE_ANALOG)
                            {
                                //Analog not always supported by core so I have to check without it also
                                targetDevice = Constants.RETRO_DEVICE_JOYPAD;
                                if (currentPortSupportedInputsIds.Contains((uint)targetDevice))
                                {
                                    result[i] = (uint)targetDevice;
                                    deviceSelected = true;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    if (!deviceSelected)
                    {
                        foreach (var j in PreferredInputTypes)
                        {
                            if (currentPortSupportedInputsIds.Contains(j))
                            {
                                result[i] = j;
                                break;
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                return new uint[] { };
            }
        }

        //Empty Handlers
        unsafe private void RenderVideoFrameHandlerEmpty(IntPtr data, uint width, uint height, UIntPtr pitch)
        {
        }
        private unsafe void RenderAudioFrameHandlerEmpty(short left, short right)
        {
        }

        private unsafe UIntPtr RenderAudioFramesHandlerEmpty(IntPtr data, UIntPtr numFrames)
        {
            return UIntPtr.Zero;
        }

        private void PollInputHandlerEmpty()
        {
        }

        private short GetInputStateHandlerEmpty(uint port, uint device, uint index, uint id)
        {
            return 0;
        }
        private bool EnvironmentHandlerEmpty(uint command, IntPtr dataPtr)
        {
            return false;
        }
        #endregion

        public IntPtr LoadInternalLibrary(IntPtr name)
        {
            try
            {
                var libraryName = Marshal.PtrToStringAnsi(name);
                _ = WriteLog($"Requesting internal DLL {libraryName}");
                var internalLibrary = new InternalLibrary(libraryName);
                internalLibrary.LoadLibrary();
                InternalLibraries.Add(internalLibrary);
                return internalLibrary.LibraryPointer;
            }
            catch (Exception e)
            {
                _ = WriteLog(e.Message);
            }
            return IntPtr.Zero;
        }
        public bool FreeInternalLibrary(IntPtr pointer)
        {
            try
            {
                _ = WriteLog($"Free internal DLL at {pointer}");
                for (var i = 0; i < InternalLibraries.Count; i++)
                {
                    var internalLibrary = InternalLibraries[i];
                    if (internalLibrary.LibraryPointer == pointer)
                    {
                        internalLibrary.FreeLibrary();
                        InternalLibraries.RemoveAt(i);
                        break;
                    }
                }
                //GC.Collect();
            }
            catch (Exception e)
            {
                _ = WriteLog(e.Message);
            }
            return true;
        }
    }
    public static class StringExtensionMethods
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
    public class InternalLibrary
    {
        public IntPtr LibraryPointer = IntPtr.Zero;
        public string LibraryName = string.Empty;

        public InternalLibrary(string libraryName)
        {
            LibraryName = libraryName;
        }
        public void LoadLibrary()
        {
            LibraryPointer = DLLLoader.LoadLibrary(LibraryName);
        }
        public void FreeLibrary()
        {
            DLLLoader.FreeLibrary(LibraryPointer);
            LibraryPointer = IntPtr.Zero;
        }
    }
    public class CoreLogItem : BindableBase
    {
        public string Type;
        public Visibility TypeVisible = Visibility.Visible;
        public SolidColorBrush backColor
        {
            get
            {
                var color = Colors.Gray;
                switch (Type.ToLower())
                {
                    case "error":
                        color = Colors.Red;
                        break;

                    case "normal":
                    case "info":
                        color = Colors.DodgerBlue;
                        break;

                    case "warn":
                        color = Colors.DarkOrange;
                        break;

                    case "debug":
                        color = Colors.Purple;
                        break;

                    case "retrix":
                        color = Colors.Green;
                        break;

                    case "exception":
                        color = Colors.OrangeRed;
                        break;

                    case "effects":
                        color = Colors.DarkOrange;
                        break;

                    case "pixels":
                        color = Colors.DodgerBlue;
                        break;

                    case "memory":
                        color = Colors.ForestGreen;
                        break;

                    case "render":
                        color = Colors.BlueViolet;
                        break;

                    case "resolution":
                        color = Colors.Gold;
                        break;

                    case "cbuffer":
                        color = Colors.PaleVioletRed;
                        break;

                    case "aspect":
                        color = Colors.Purple;
                        break;
                }
                return new SolidColorBrush(color);
            }
        }
        public string Message;
        public string Time;

        public CoreLogItem(string type, string message)
        {
            Type = type.ToUpper();
            Message = message.Replace("\n", "").Replace("\r", "");
            Time = DateTime.Now.ToString();
            if (type.ToLower().Equals("none"))
            {
                TypeVisible = Visibility.Collapsed;
            }
        }

    }
}
