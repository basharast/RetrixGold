using LibRetriX.RetroBindings.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace LibRetriX.RetroBindings
{
    internal sealed class LibretroCore : ICore, IDisposable
    {
        private const int AudioSamplesPerFrame = 2;

        public string Name { get; set; }
        public string SystemName { get; set; }
        public string OriginalSystemName { get; set; }
        public string Version { get; set; }
        public IList<string> SupportedExtensions { get; set;}
        public bool FailedToLoad { get; set;}
        public bool NativeArchiveSupport { get; set; }
        public bool SubSystemSupport { get; set; }
        public uint RetroGameType { get; set; }

        private bool RequiresFullPath { get; set; }

        private IntPtr CurrentlyResolvedCoreOptionValue { get; set; }
        public IDictionary<string, CoreOption> Options { get; private set; }

        private IList<Tuple<string, uint>> OptionSetters { get; set; }

        public IList<FileDependency> FileDependencies { get; set;}

        private IntPtr systemRootPathUnmanaged;
        private string systemRootPath;
        public string SystemRootPath
        {
            get => systemRootPath;
            set { SetStringAndUnmanagedMemory(value, ref systemRootPath, ref systemRootPathUnmanaged); }
        }

        private IntPtr saveRootPathUnmanaged;
        private string saveRootPath;
        public string SaveRootPath
        {
            get => saveRootPath;
            set { SetStringAndUnmanagedMemory(value, ref saveRootPath, ref saveRootPathUnmanaged); }
        }

        private PixelFormats pixelFormat;
        public PixelFormats PixelFormat
        {
            get => pixelFormat;
            private set { pixelFormat = value; PixelFormatChanged?.Invoke(pixelFormat); }
        }

        private GameGeometry geometry;
        public GameGeometry Geometry
        {
            get => geometry;
            private set { geometry = value; GeometryChanged?.Invoke(geometry); }
        }

        private SystemTimings timings;
        public SystemTimings Timings
        {
            get => timings;
            private set { timings = value; TimingsChanged?.Invoke(timings); }
        }

        private Rotations rotation;
        public Rotations Rotation
        {
            get => rotation;
            private set { rotation = value; RotationChanged?.Invoke(rotation); }
        }

        public ulong SerializationSize => (ulong)LibretroAPI.GetSerializationSize();

        public PollInputDelegate PollInput { get; set; }
        public GetInputStateDelegate GetInputState { get; set; }
        public OpenFileStreamDelegate OpenFileStream
        {
            get => VFSHandler.OpenFileStream;
            set { VFSHandler.OpenFileStream = value; }
        }

        public CloseFileStreamDelegate CloseFileStream
        {
            get => VFSHandler.CloseFileStream;
            set { VFSHandler.CloseFileStream = value; }
        }

        public event RenderVideoFrameDelegate RenderVideoFrame;
        public event RenderAudioFramesDelegate RenderAudioFrames;
        public event PixelFormatChangedDelegate PixelFormatChanged;
        public event GeometryChangedDelegate GeometryChanged;
        public event TimingsChangedDelegate TimingsChanged;
        public event RotationChangedDelegate RotationChanged;

        private static LogCallbackDescriptor LogCBDescriptor { get; } = new LogCallbackDescriptor { LogCallback = LogHandler };

        private List<List<ControllerDescription>> SupportedInputsPerPort { get; } = new List<List<ControllerDescription>>();
        private Lazy<uint[]> InputTypesToUse { get; set; }
        private IEnumerable<uint> PreferredInputTypes { get; set; }

        private bool IsInitialized { get; set; }
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
        public bool NativeSkipFrames { get; set; }
        public bool NativeSkipFramesRandom { get; set; }

        IntPtr DLLModule;
        public string DLLName { get; set; }

        public void FreeLibretroCore()
        {
            try
            {
                if (DLLModule != IntPtr.Zero)
                {
                    UnloadGame();
                }
            }
            catch (Exception e)
            {

            }
        }
        bool loadCustomInput = true;
        public void ReInitialCore(bool LoadCustomInput=true)
        {
            try
            {
                loadCustomInput = LoadCustomInput;
                DeInitialize();
                FreeLibretroCore();
                Dispose();
                LogsList = new ObservableCollection<string>();
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
            try
            {
                //Load DLL Library
                DLLModule = LibretroAPI.LoadLibrary(DLLName);
                if (DLLModule != IntPtr.Zero) // error handling
                {
                    LibretroAPI.LoadLibraryFunctions(DLLModule);
                    if (LibretroAPI.UpdateOptionsInGame == null && LibretroAPI.UpdateVariables==null)
                    {
                        IsInGameOptionsActive = false;
                    }
                    else
                    {
                        IsInGameOptionsActive = true;
                    }
                }
                else
                {
                    throw new Exception($"Could not load library: {Marshal.GetLastWin32Error()}");
                }
            }
            catch (Exception e)
            {

            }
        }

        IList<FileDependency> dependenciesGlobal = null;
        IList<Tuple<string, uint>> optionSettersGlobal = null;
        uint? inputTypeIdGlobal = null;
        public LibretroCore(IReadOnlyList<FileDependency> dependencies = null, IReadOnlyList<Tuple<string, uint>> optionSetters = null, uint? inputTypeId = null, string AnyCoreName="")
        {
            dependenciesGlobal = (IList<FileDependency>)dependencies;
            optionSettersGlobal = (IList<Tuple<string, uint>>)optionSetters;
            inputTypeIdGlobal = loadCustomInput ? inputTypeId : null;
            DLLName = $"{LibretroAPI.LibraryName}.dll";
            if (AnyCoreName.Length > 0)
            {
                DLLName = AnyCoreName;
            }
            try {
                FreeLibretroCore();
                LoadCoreLibrary();
                PrepareCore();
            }catch(Exception e)
            {

            }
        }

        public void PrepareCore()
        {
            try
            {
                if (DLLModule == IntPtr.Zero)
                {
                    throw new Exception($"Could not load library: {Marshal.GetLastWin32Error()}");
                }
                FileDependencies = dependenciesGlobal == null ? Array.Empty<FileDependency>() : dependenciesGlobal;
                OptionSetters = optionSettersGlobal == null ? Array.Empty<Tuple<string, uint>>() : optionSettersGlobal;

                InputTypesToUse = new Lazy<uint[]>(DetermineInputTypesToUse, LazyThreadSafetyMode.PublicationOnly);
                var preferredInputTypes = new List<uint> { Constants.RETRO_DEVICE_ANALOG, Constants.RETRO_DEVICE_JOYPAD };
                PreferredInputTypes = preferredInputTypes;
                if (inputTypeIdGlobal.HasValue && loadCustomInput)
                {
                    preferredInputTypes.Insert(0, inputTypeIdGlobal.Value);
                }

                var systemInfo = new SystemInfo();
                LibretroAPI.GetSystemInfo(ref systemInfo);
                Name = systemInfo.LibraryName;
                Version = systemInfo.LibraryVersion;
                SupportedExtensions = systemInfo.ValidExtensions.Split('|').Select(d => $".{d}").ToArray();
                NativeArchiveSupport = systemInfo.BlockExtract;
                RequiresFullPath = systemInfo.NeedFullpath;

                Options = new Dictionary<string, CoreOption>();

                AudioOnly = false;
                VideoOnly = false;
                FrameRate = 0;
                ShowFPSCounter = false;
            }
            catch (Exception e)
            {
                SupportedExtensions = new string[] { ".null" };
                NativeArchiveSupport = true;
                RequiresFullPath = false;
                FailedToLoad = true;
            }
        }

        public void Dispose()
        {
            try { 
            SystemRootPath = null;
            SaveRootPath = null;

            if (CurrentlyResolvedCoreOptionValue != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(CurrentlyResolvedCoreOptionValue);
                CurrentlyResolvedCoreOptionValue = IntPtr.Zero;
            }

                GC.SuppressFinalize(this);
            }
            catch (Exception e)
            {

            }
        }

        public void Initialize(bool isSubSystem=false)
        {
            try {
            SubSystemSupport = isSubSystem;
            LibretroAPI.EnvironmentCallback = EnvironmentHandler;
            LibretroAPI.RenderVideoFrameCallback = RenderVideoFrameHandler;
            LibretroAPI.RenderAudioFrameCallback = RenderAudioFrameHandler;
            LibretroAPI.RenderAudioFramesCallback = RenderAudioFramesHandler;
            LibretroAPI.PollInputCallback = PollInputHandler;
            LibretroAPI.GetInputStateCallback = GetInputStateHandler;
            }catch(Exception e)
            {

            }
        }

        public void DeInitialize()
        {
            try
            {
                LibretroAPI.SetEnvironmentDelegate = null;
                LibretroAPI.SetRenderVideoFrameDelegate = null;
                LibretroAPI.SetRenderAudioFrameDelegate = null;
                LibretroAPI.SetRenderAudioFramesDelegate = null;
                LibretroAPI.SetPollInputDelegate = null;
                LibretroAPI.SetGetInputStateDelegate = null;
            }
            catch (Exception e)
            {

            }
        }

        private static ObservableCollection<string> LogsList = new ObservableCollection<string>();
        public ObservableCollection<string> GetLogsList()
        {
            return LogsList;
        }
        private static void LogHandler(LogLevels level, IntPtr format, IntPtr argAddresses)
        {
        //#if DEBUG
            var message = Marshal.PtrToStringAnsi(format);
            /*var messageArgs = Marshal.PtrToStringAnsi(argAddresses);
            if (messageArgs != null) { 
            LogsList.Insert(1,String.Format(message,messageArgs));
            }
            else
            {
                LogsList.Insert(1, message);
            }*/
            LogsList.Insert(1, message);
            //System.Diagnostics.Debug.WriteLine($"{NativeDllInfo.DllName}: {level} - {message}");
            //#endif
        }

        public bool LoadGame(string mainGameFilePath)
        {
            LogsList.Clear();
            LogsList.Insert(0,"*****************************");
            try { 
            if (!IsInitialized)
            {
                LibretroAPI.Initialize();
                IsInitialized = true;
            }

            if (CurrentGameInfo.HasValue)
            {
                UnloadGameNoDeinit();
            }

            var gameInfo = new GameInfo()
            {
                Path = mainGameFilePath
            };

            if (!RequiresFullPath)
            {
                var stream = OpenFileStream?.Invoke(mainGameFilePath, FileAccess.Read);
                if (stream == null)
                {
                    return false;
                }

                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                GameDataHandle = gameInfo.SetGameData(data);
                CloseFileStream(stream);
            }

            Rotation = Rotations.CCW0;
            var loadSuccessful = false;

                if (SubSystemSupport && RetroGameType > 0) {
                    loadSuccessful = LibretroAPI.LoadGameSpecial((uint)RetroGameType, ref gameInfo);
                }
                else
                {
                    loadSuccessful = LibretroAPI.LoadGame(ref gameInfo);
                }

                if (loadSuccessful)
                {
                var avInfo = new SystemAVInfo();
                LibretroAPI.GetSystemAvInfo(ref avInfo);

                Geometry = avInfo.Geometry;
                Timings = avInfo.Timings;

                var inputTypesToUse = InputTypesToUse.Value;
                for (var i = 0; i < inputTypesToUse.Length; i++)
                {
                    LibretroAPI.SetControllerPortDevice((uint)i, inputTypesToUse[i]);
                }

                CurrentGameInfo = gameInfo;
                }
                else
                {
                    LogsList.Insert(1, "Error, failed to load the game");
                    FailedToLoadGame = true;

                    //Because we handled the error, the return result should be true so the emulation can keep going
                    return true;
                }

            return CurrentGameInfo.HasValue;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void UnloadGame()
        {
            try {
            RetroGameType = 0U;
            FailedToLoadGame = false;
            UnloadGameNoDeinit();

            if (IsInitialized)
            {
                LibretroAPI.Cleanup();
                IsInitialized = false;
            }
            }
            catch (Exception e)
            {

            }
            try
            {
                LibretroAPI.FreeLibrary(DLLModule);
            }
            catch (Exception e)
            {

            }
        }

        public void Reset()
        {
            try {
                if (!FailedToLoadGame) { 
            LibretroAPI.Reset();
                }
                else
                {
                    LogsList.Insert(1, "Can't reset the game didn't loaded");
                }
            }
            catch (Exception e)
            {

            }
        }

        public void RunFrame()
        {
            unsafe { 
            try {
            if (!FailedToLoadGame) { 
            LibretroAPI.RunFrame();
            }
            }
           catch(Exception e)
            {

            }
            }
        }

        public void UpdateOptionsInGame()
        {
            unsafe
            {
                try
                {
                    if (!FailedToLoadGame) { 
                    if (LibretroAPI.UpdateOptionsInGame != null) { 
                    LibretroAPI.UpdateOptionsInGame();
                    }
                    else
                    {
                    LibretroAPI.UpdateVariables(false);
                    }
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        public bool SaveState(Stream outputStream)
        {
            try {
                if (!FailedToLoadGame) { 
            var size = LibretroAPI.GetSerializationSize();
            var stateData = new byte[(int)size];

            var handle = GCHandle.Alloc(stateData, GCHandleType.Pinned);
            var result = LibretroAPI.SaveState(handle.AddrOfPinnedObject(), (IntPtr)stateData.Length);
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
                if (!FailedToLoadGame)
                {
                    var stateData = new byte[inputStream.Length];
                    inputStream.Position = 0;
                    inputStream.Read(stateData, 0, stateData.Length);

                    var handle = GCHandle.Alloc(stateData, GCHandleType.Pinned);
                    var result = LibretroAPI.LoadState(handle.AddrOfPinnedObject(), (IntPtr)stateData.Length);
                    handle.Free();

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
            try { 
            if (!CurrentGameInfo.HasValue)
            {
                return;
            }

            LibretroAPI.UnloadGame();
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

        IntPtr dataPtrShared = new IntPtr();
        private bool EnvironmentHandler(uint command, IntPtr dataPtr)
        {
            try {
            dataPtrShared = dataPtr;
            switch (command)
            {
                case Constants.RETRO_ENVIRONMENT_GET_LOG_INTERFACE:
                    {
                        Marshal.StructureToPtr(LogCBDescriptor, dataPtr, false);
                        return true;
                    }
                case Constants.RETRO_ENVIRONMENT_SET_VARIABLES:
                    {
                        var newOptions = new Dictionary<string, CoreOption>();
                        Options = newOptions;

                        var data = Marshal.PtrToStructure<LibretroVariable>(dataPtr);
                        while (data.KeyPtr != IntPtr.Zero)
                        {
                            var key = Marshal.PtrToStringAnsi(data.KeyPtr);
                            var rawValue = Marshal.PtrToStringAnsi(data.ValuePtr);

                            var split = rawValue.Split(';');
                            var description = split[0];

                            rawValue = rawValue.Substring(description.Length + 2);
                            split = rawValue.Split('|');

                            newOptions.Add(key, new CoreOption(description, split));

                            dataPtr += Marshal.SizeOf<LibretroVariable>();
                            data = Marshal.PtrToStructure<LibretroVariable>(dataPtr);
                        }

                        foreach(var i in OptionSetters)
                        {
                            Options[i.Item1].SelectedValueIx = i.Item2;
                        }

                        return true;
                    }
                case Constants.RETRO_ENVIRONMENT_GET_VARIABLE:
                    {
                        var data = Marshal.PtrToStructure<LibretroVariable>(dataPtr);
                        var key = Marshal.PtrToStringAnsi(data.KeyPtr);
                        var valueFound = false;
                        data.ValuePtr = IntPtr.Zero;

                        if (Options.ContainsKey(key))
                        {
                            valueFound = true;
                            var coreOption = Options[key];
                            var value = coreOption.Values[(int)coreOption.SelectedValueIx];
                            if (CurrentlyResolvedCoreOptionValue != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(CurrentlyResolvedCoreOptionValue);
                            }

                            CurrentlyResolvedCoreOptionValue = Marshal.StringToHGlobalAnsi(value);
                            data.ValuePtr = CurrentlyResolvedCoreOptionValue;
                        }

                        Marshal.StructureToPtr(data, dataPtr, false);
                        return valueFound;
                    }
                case Constants.RETRO_ENVIRONMENT_GET_OVERSCAN:
                    {
                        Marshal.WriteByte(dataPtr, 0);
                        return true;
                    }
                case Constants.RETRO_ENVIRONMENT_GET_CAN_DUPE:
                    {
                        Marshal.WriteByte(dataPtr, 1);
                        return true;
                    }
                case Constants.RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY:
                    {
                        Marshal.WriteIntPtr(dataPtr, systemRootPathUnmanaged);
                        return true;
                    }
                case Constants.RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:
                    {
                        Marshal.WriteIntPtr(dataPtr, saveRootPathUnmanaged);
                        return true;
                    }
                case Constants.RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:
                    {
                        var data = (PixelFormats)Marshal.ReadInt32(dataPtr);
                        PixelFormat = data;
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
                        return true;
                    }
                case Constants.RETRO_ENVIRONMENT_SET_SYSTEM_AV_INFO:
                    {
                        var data = Marshal.PtrToStructure<SystemAVInfo>(dataPtr);
                        Geometry = data.Geometry;
                        Timings = data.Timings;
                        return true;
                    }
                case Constants.RETRO_ENVIRONMENT_SET_CONTROLLER_INFO:
                    {
                        IntPtr portDescriptionsPtr;
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
                            data.Interface = VFSHandler.VFSInterfacePtr;
                            Marshal.StructureToPtr(data, dataPtr, false);
                        }

                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public void UpdateCoreOptions(string key)
        {
            var data = Marshal.PtrToStructure<LibretroVariable>(dataPtrShared);
            data.ValuePtr = IntPtr.Zero;
            if (Options.ContainsKey(key))
            {
                var coreOption = Options[key];
                var value = coreOption.Values[(int)coreOption.SelectedValueIx];
                if (CurrentlyResolvedCoreOptionValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(CurrentlyResolvedCoreOptionValue);
                }

                CurrentlyResolvedCoreOptionValue = Marshal.StringToHGlobalAnsi(value);
                data.ValuePtr = CurrentlyResolvedCoreOptionValue;
            }

            Marshal.StructureToPtr(data, dataPtrShared, true);
        }
        

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
                    if (SkippedFramesCount > GetSkipFrameValue())
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
                    if(SkipState == 5)
                    {
                        return;
                    }
                }catch(Exception ex)
                {

                }
            }
            try { 
            var size = (int)height * (int)pitch;

            var payload = default(ReadOnlySpan<byte>);
            if (data != IntPtr.Zero)
            {
                payload = new ReadOnlySpan<byte>(data.ToPointer(), size);
            }
            UpdateFrameRate();
            RenderVideoFrame?.Invoke(payload, width, height, (uint)pitch);
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

        private  bool SkipFrameRelay = false;
        private  int SkippedFramesCount = 0;
        private   Random RandomSkipFrames = new Random();
        private static int GetSkipFrameValue()
        {
            //int SkipFrameValue = RandomSkipFrames.Next(1);
            int SkipFrameValue = 1;
            return SkipFrameValue;
        }

        private unsafe void RenderAudioFrameHandler(short left, short right)
        {
            try { 
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
            try { 
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
            try { 
            PollInput?.Invoke();
            }
            catch (Exception e)
            {

            }
        }

        private short GetInputStateHandler(uint port, uint device, uint index, uint id)
        {
            try { 
            var inputType = Converter.ConvertToInputType(device, index, id);
            var result = GetInputState?.Invoke(port, inputType);
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
            try { 
            var result = SupportedInputsPerPort.Select(supportedInputs =>
            {
                var output = (uint)Constants.RETRO_DEVICE_NONE;
                if (!supportedInputs.Any())
                {
                    return output;
                }

                output = supportedInputs.First().Id;
                var currentPortSupportedInputsIds = new HashSet<uint>(supportedInputs.Select(e => e.Id));
                foreach (var j in PreferredInputTypes)
                {
                    if (currentPortSupportedInputsIds.Contains(j))
                    {
                        output = j;
                        break;
                    }
                }

                return output;
            }).ToArray();

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
    }
}
