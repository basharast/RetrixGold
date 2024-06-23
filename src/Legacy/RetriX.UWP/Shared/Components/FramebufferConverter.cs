#if TARGET_AMD64 || TARGET_ARM64 || (TARGET_32BIT && !TARGET_ARM)
#define HAS_CUSTOM_BLOCKS
#endif

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

using LibRetriX.RetroBindings;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Pipelines.Sockets.Unofficial;
using RetriX.Shared.Services;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace RetriX.Shared.Components
{
    public static class FramebufferConverter
    {
        //import necessary API as shown in other examples
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("kernel32", SetLastError = true)]
        public static extern void FreeLibrary(IntPtr module);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr module, string proc);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void* CopyMemory_S(void* dest, int destSize, void* src, int count);
        public static CopyMemory_S CopyMemory;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void* __Memmove_S(void* dest, int destSize, void* src, int count);
        public static __Memmove_S __Memmove;

        public static bool CopyMemoryAvailable = false;
        public static bool MoveMemoryAvailable = false;
        public static bool CrazyBufferActive = true;
        public static double crazyBufferPercentageHandle = 0;

        public static string MemoryHelper = "Buffer.CopyMemory";

        public static int CoresCount = 1;
        private const uint LookupTableSize = ushort.MaxValue + 1;

        private static uint[] RGB0555LookupTable = new uint[LookupTableSize];
        private static uint[] RGB565LookupTable = new uint[LookupTableSize];

        public static int CurrentColorFilter = 0;
        public static bool isGameStarted = false;
        public static bool AudioOnly = false;
        public static string renderFailedMessage = "";

        public static bool DontWaitThreads = false;
        public static bool NativeDoublePixel = false;
        public static bool NativeSpeedup = false;
        public static bool NativeScanlines = false;
        public static int NativePixelStep = 0;
        private static int StartIndex = -1;
        public static EventHandler RaiseSkippedCachedHandler = null;
        public static bool isRGB888 = false;
        public static bool SkipCached = true;
        public static bool requestToStopSkipCached = false;
        private static bool lookupTablesReady = false;

        private static IntPtr DLLModule = IntPtr.Zero;
        public static string memcpyErrorMessage = "";
        public static string memmovErrorMessage = "";
        public static ulong currentWidth = 0;
        public static ulong currentHeight = 0;

        #region Loading Archive Progress (Should be moved to separate file in future)
        public static string CurrentFileEntry = "";
        public static string currentFileEntry
        {
            get
            {
                return CurrentFileEntry;
            }
            set
            {
                CurrentFileEntry = value;
                if (UpdateProgressState != null)
                {
                    UpdateProgressState.Invoke(null, EventArgs.Empty);
                }
            }
        }
        public static double CurrentFileProgress = 0;
        public static double currentFileProgress
        {
            get
            {
                return CurrentFileProgress;
            }
            set
            {
                CurrentFileProgress = value;
                if (UpdateProgressState != null)
                {
                    UpdateProgressState.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static EventHandler UpdateProgressState;
        #endregion

        static FramebufferConverter()
        {
            SetRGBLookupTable();
            LoadMemcpyFunction();
        }

        //Below is very safe way to get external function
        public static void LoadMemcpyFunction()
        {
            try
            {
                if (DLLModule != IntPtr.Zero)
                {
                    FreeLibrary(DLLModule);
                }
            }
            catch (Exception ex)
            {

            }
            try
            {
                DLLModule = LoadLibrary("msvcrt.dll");
                if (DLLModule != IntPtr.Zero)
                {
                    IntPtr CopyMemoryPointer = GetProcAddress(DLLModule, "memcpy_s");
                    if (CopyMemoryPointer != IntPtr.Zero) // error handling
                    {
                        CopyMemory = Marshal.GetDelegateForFunctionPointer<CopyMemory_S>(CopyMemoryPointer);
                        CopyMemoryAvailable = true;
                    }
                    else
                    {
                        throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                    }
                }
                else
                {
                    throw new Exception($"Could not load msvcrt.dll: {Marshal.GetLastWin32Error()}");
                }
            }
            catch (Exception ex)
            {
                renderFailedMessage = ex.Message;
                memcpyErrorMessage = ex.Message;
                CopyMemoryAvailable = false;
            }
            try
            {
                if (DLLModule != IntPtr.Zero)
                {
                    IntPtr MovMemoryPointer = GetProcAddress(DLLModule, "memmove_s");
                    if (MovMemoryPointer != IntPtr.Zero) // error handling
                    {
                        __Memmove = Marshal.GetDelegateForFunctionPointer<__Memmove_S>(MovMemoryPointer);
                        MoveMemoryAvailable = true;
                    }
                    else
                    {
                        throw new Exception($"Could not load method: {Marshal.GetLastWin32Error()}");
                    }
                }
            }
            catch (Exception ex)
            {
                renderFailedMessage = ex.Message;
                memmovErrorMessage = ex.Message;
                MoveMemoryAvailable = false;
            }
            if (!CopyMemoryAvailable && !MoveMemoryAvailable)
            {
                try
                {
                    if (DLLModule != IntPtr.Zero)
                    {
                        FreeLibrary(DLLModule);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        public static void ClearBuffer()
        {
            //Disabled for now, I don't think we need to regenrate the tables each time
            /*
            RGB0555LookupTable = new uint[LookupTableSize];
            RGB565LookupTable = new uint[LookupTableSize];
            */
        }
        public static void SetRGBLookupTable(bool forceRegenrate = false)
        {
            if (lookupTablesReady && !forceRegenrate)
            {
                return;
            }
            try
            {
                uint r565, g565, b565;
                uint r0555, g0555, b0555;

                double red = 255.0;
                double green = 255.0;
                double blue = 255.0;
                for (uint i = 0; i < LookupTableSize; i++)
                {
                    //RGB565
                    r565 = (i >> 11) & 0x1F;
                    g565 = (i >> 5) & 0x3F;
                    b565 = (i & 0x1F);

                    r565 = (uint)Math.Round(r565 * red / 31.0);
                    g565 = (uint)Math.Round(g565 * green / 63.0);
                    b565 = (uint)Math.Round(b565 * blue / 31.0);


                    //RGB0555
                    r0555 = (i >> 10) & 0x1F;
                    g0555 = (i >> 5) & 0x1F;
                    b0555 = (i & 0x1F);

                    r0555 = (uint)Math.Round(r0555 * red / 31.0);
                    g0555 = (uint)Math.Round(g0555 * green / 31.0);
                    b0555 = (uint)Math.Round(b0555 * blue / 31.0);

                    RGB565LookupTable[i] = (0xFF000000 | r565 << 16 | g565 << 8 | b565);
                    RGB0555LookupTable[i] = (0xFF000000 | r0555 << 16 | g0555 << 8 | b0555);
                }
                lookupTablesReady = true;
            }
            catch (Exception e)
            {

            }
        }

        public static void ConvertFrameBufferRGB0555ToXRGB8888(uint width, uint height, IntPtr input, int inputPitch, IntPtr output, int length, int outputPitch)
        {
            try
            {
                renderFailedMessage = "";
                FallbackToOldWay = PlatformService.ForceOldBufferMethods;
                var size = (int)height * (int)inputPitch;
                if (PlatformService.SafeRender)
                {
                    ConvertExtremelyUnsafeWithSpan(height, width, input, (ulong)size, (ulong)inputPitch, output, (ulong)length, (ulong)outputPitch, false);
                }
                else
                {
                    ConvertExtremelyUnsafe(height, width, input, (ulong)size, (ulong)inputPitch, output, (ulong)length, (ulong)outputPitch, false);
                }
            }
            catch (Exception e)
            {

            }
        }

        public static void ConvertFrameBufferRGB565ToXRGB8888(uint width, uint height, IntPtr input, int inputPitch, IntPtr output, int length, int outputPitch)
        {
            try
            {
                renderFailedMessage = "";
                FallbackToOldWay = PlatformService.ForceOldBufferMethods;
                var size = (int)height * (int)inputPitch;
                if (PlatformService.SafeRender)
                {
                    ConvertExtremelyUnsafeWithSpan(height, width, input, (ulong)size, (ulong)inputPitch, output, (ulong)length, (ulong)outputPitch, true);
                }
                else
                {
                    ConvertExtremelyUnsafe(height, width, input, (ulong)size, (ulong)inputPitch, output, (ulong)length, (ulong)outputPitch, true);
                }
            }
            catch (Exception e)
            {

            }
        }

        public static void ConvertFrameBufferToXRGB8888(uint width, uint height, IntPtr input, int inputPitch, IntPtr output, int length, int outputPitch)
        {
            try
            {
                renderFailedMessage = "";
                FallbackToOldWay = PlatformService.ForceOldBufferMethods;
                var size = (int)height * (int)inputPitch;
                if (PlatformService.SafeRender)
                {
                    ConvertExtremelyUnsafe888WithSpan(height, width, input, (ulong)size, (ulong)inputPitch, output, (ulong)length, (ulong)outputPitch);
                }
                else
                {
                    ConvertExtremelyUnsafe888(height, width, input, (ulong)size, (ulong)inputPitch, output, (ulong)length, (ulong)outputPitch);
                }
            }
            catch (Exception e)
            {

            }
        }

        private static int GetStartIndexValue()
        {
            StartIndex = 0;
            /*
            if (StartIndex < NativePixelStep && NativeSpeedup)
            {
                StartIndex ++;
            }
            */
            return StartIndex;
        }


        //Below new memory solution thanks to DekuDesu
        //https://stackoverflow.com/q/70491483/1590588
        //This function only for 555, 565
        #region FROM 555,565 TO 888
        public static bool inputFillWithBlack = false;
        static bool fallbackToOldWay = false;
        public static bool FallbackToOldWay
        {
            get
            {
                return fallbackToOldWay;
            }
            set
            {
                fallbackToOldWay = value;
            }
        }
        public static unsafe void ConvertExtremelyUnsafe(ulong height, ulong width, IntPtr inputArray, ulong inputLength, ulong inputPitch, IntPtr outputArray, ulong outputLength, ulong outputPitch, bool isRG565State)
        {
            if ((NativePixelStep > 1 && inputFillWithBlack) || fillSpanRequired)
            {
                var mapData = new Span<byte>(outputArray.ToPointer(), (int)outputLength);
                mapData.Fill(0);

                fillSpanRequired = false;
                inputFillWithBlack = false;
            }
            if ((!isGameStarted || AudioOnly))
            {
                return;
            }
            currentWidth = width;
            currentHeight = height;
            totalSkipped = 0;
            isRG565 = isRG565State;
            isRGB888 = false;
            StartIndex = GetStartIndexValue();

            bool renderFailed = FallbackToOldWay;

            if (!FallbackToOldWay && CoresCount > 1)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception x)
                {

                }
                cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    List<Task> coresTasks = new List<Task>();
                    var hightPerCore = (int)height / CoresCount;
                    byte* inputPointer = (byte*)inputArray.ToPointer(), outputPointer = (byte*)outputArray.ToPointer();
                    //fixed (byte* inputPointer = &inputArray[0], outputPointer = &outputArray[0])
                    {
                        using (var inputManager = new UnmanagedMemoryManager<byte>(inputPointer, (int)inputLength))
                        using (var outputManager = new UnmanagedMemoryManager<byte>(outputPointer, (int)outputLength))
                        {
                            Parallel.For(0, CoresCount, (t) =>
                            {
                                var TStartIndex = (t * hightPerCore);
                                var hightSub = hightPerCore + TStartIndex;
                                if (hightSub > (int)height)
                                {
                                    hightSub = (int)height;
                                }
                                ParallelOptions parallelOptions = new ParallelOptions();
                                parallelOptions.CancellationToken = cancellationTokenSource.Token;
                                var coreTask = Task.Factory.StartNew(() =>
                                {
                                    Parallel.For(TStartIndex, hightSub, (iT) =>
                                    {
                                        try
                                        {
                                            if (LibretroCore.RequestVariablesUpdate)
                                            {
                                                cancellationTokenSource.Cancel();
                                            }
                                            var i = (ulong)iT;
                                            if (cancellationTokenSource.IsCancellationRequested)
                                            {
                                                return;
                                            }

                                            if (!SkipCached)
                                            {
                                                // get a pointer for the first byte of the line of the input
                                                byte* inputLinePointer = (byte*)inputManager.Pin().Pointer + (i * inputPitch);
                                                inputManager.Unpin();

                                                // get a pointer for the first byte of the line of the output
                                                byte* outputLinePointer = (byte*)outputManager.Pin().Pointer + (i * outputPitch);
                                                outputManager.Unpin();

                                                pixelCachesStates[i] = true;
                                                ulong restPixels = 0;
                                                if (CrazyBufferActive)
                                                {
                                                    restPixels = CrazyBufferMove.CrazyMove(outputLinePointer, inputLinePointer, sizeof(ushort), sizeof(uint), width);
                                                    if (iT == 0)
                                                    {
                                                        try
                                                        {
                                                            crazyBufferPercentageHandle = Math.Round(((restPixels * 1d) / (width * 1d) * 100.0));
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                    }
                                                }

                                                // traverse the input line by ushorts
                                                for (ulong j = restPixels; j < width; j++)
                                                {
                                                    if (LibretroCore.RequestVariablesUpdate)
                                                    {
                                                        cancellationTokenSource.Cancel();
                                                    }
                                                    if (cancellationTokenSource.IsCancellationRequested)
                                                    {
                                                        break;
                                                    }

                                                    // calculate the offset for i'th uint
                                                    ulong outputOffset = j * sizeof(uint);

                                                    // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                                    if (outputOffset >= outputLength)
                                                    {
                                                        renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                                        renderFailed = true;
                                                        break;
                                                    }

                                                    // get a pointer to the i'th uint
                                                    uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                                    // calculate the offset for the i'th ushort,
                                                    // becuase we loop based on the input and ushort we dont need an index check here
                                                    ulong inputOffset = j * sizeof(ushort);

                                                    // get a pointer to the i'th ushort
                                                    ushort* rgb565Pointer = (ushort*)(inputLinePointer + inputOffset);

                                                    ushort rgb565Value = *rgb565Pointer;

                                                    // convert the rgb to the other format
                                                    var rgb888Value = getPattern(rgb565Value);

                                                    // write the bytes of the rgb888 to the output array
                                                    *rgb888Pointer = rgb888Value;
                                                }
                                            }
                                            else
                                            {
                                                //IMPORTANT Anything below related to Pixels Update debug feature
                                                #region PIXELS UPDATES
                                                if (IsPixelsRowCached(iT))
                                                {
                                                    iT += NativePixelStep;
                                                }
                                                else
                                                {
                                                    // get a pointer for the first byte of the line of the input
                                                    byte* inputLinePointer = (byte*)inputManager.Pin().Pointer + (i * inputPitch);
                                                    inputManager.Unpin();

                                                    // get a pointer for the first byte of the line of the output
                                                    byte* outputLinePointer = (byte*)outputManager.Pin().Pointer + (i * outputPitch);
                                                    outputManager.Unpin();

                                                    pixelCachesStates[i] = true;
                                                    // traverse the input line by ushorts
                                                    for (ulong j = 0; j < width; j++)
                                                    {
                                                        if (LibretroCore.RequestVariablesUpdate)
                                                        {
                                                            break;
                                                        }
                                                        if (cancellationTokenSource.IsCancellationRequested)
                                                        {
                                                            break;
                                                        }

                                                        // calculate the offset for i'th uint
                                                        ulong outputOffset = j * sizeof(uint);

                                                        // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                                        if (outputOffset >= outputLength)
                                                        {
                                                            renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                                            renderFailed = true;
                                                            break;
                                                        }

                                                        // get a pointer to the i'th uint
                                                        uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                                        // calculate the offset for the i'th ushort,
                                                        // becuase we loop based on the input and ushort we dont need an index check here
                                                        ulong inputOffset = j * sizeof(ushort);

                                                        // get a pointer to the i'th ushort
                                                        ushort* rgb565Pointer = (ushort*)(inputLinePointer + inputOffset);

                                                        ushort rgb565Value = *rgb565Pointer;

                                                        if (SkipCached)
                                                        {
                                                            if (pixelCaches[i, j] != null && pixelCaches[i, j].input == rgb565Value)
                                                            {
                                                                *rgb888Pointer = 0;
                                                                pixelCachesStates[i] = pixelCachesStates[i] && true;
                                                                Interlocked.Add(ref totalSkipped, 1);
                                                                continue;
                                                            }
                                                        }

                                                        // convert the rgb to the other format
                                                        var rgb888Value = getPattern(rgb565Value);

                                                        // write the bytes of the rgb888 to the output array
                                                        *rgb888Pointer = rgb888Value;
                                                        if (SkipCached)
                                                        {
                                                            CachePixel(rgb565Value, rgb888Value, (int)i, (int)j);
                                                            pixelCachesStates[i] = false;
                                                        }
                                                    }
                                                }
                                                if (renderFailed)
                                                {
                                                    cancellationTokenSource.Cancel();
                                                }

                                                iT += NativePixelStep;
                                                #endregion
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                    });
                                }, cancellationTokenSource.Token);
                                coresTasks.Add(coreTask);
                            });
                        }
                    }
                    if (!DontWaitThreads) Task.WaitAll(coresTasks.ToArray());
                }
                catch (Exception ex)
                {
                    renderFailed = true;
                    renderFailedMessage = ex.Message;
                }
            }
            else if (!FallbackToOldWay)
            {
                try
                {
                    // pin down pointers so they dont move on the heap
                    byte* inputPointer = (byte*)inputArray.ToPointer(), outputPointer = (byte*)outputArray.ToPointer();
                    //fixed (byte* inputPointer = &inputArray[0], outputPointer = &outputArray[0])
                    {
                        // since we have to account for padding we should go line by line
                        for (ulong i = (ulong)StartIndex; i < height;)
                        {
                            if (LibretroCore.RequestVariablesUpdate)
                            {
                                break;
                            }
                            // get a pointer for the first byte of the line of the input
                            byte* inputLinePointer = inputPointer + (i * inputPitch);

                            // get a pointer for the first byte of the line of the output
                            byte* outputLinePointer = outputPointer + (i * outputPitch);

                            if (!SkipCached)
                            {
                                bool memoryHelperUsed = false;
                                //No memory helpers due to pixels conversion 

                                if (!memoryHelperUsed)
                                {
                                    ulong restPixels = 0;
                                    if (CrazyBufferActive)
                                    {
                                        restPixels = CrazyBufferMove.CrazyMove(outputLinePointer, inputLinePointer, sizeof(ushort), sizeof(uint), width);
                                        if (i == 0)
                                        {
                                            try
                                            {
                                                crazyBufferPercentageHandle = Math.Round(((restPixels * 1d) / (width * 1d) * 100.0));
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                    }
                                    // traverse the input line by ushorts
                                    for (ulong j = restPixels; j < width; j++)
                                    {
                                        if (LibretroCore.RequestVariablesUpdate)
                                        {
                                            break;
                                        }
                                        // calculate the offset for i'th uint
                                        ulong outputOffset = j * sizeof(uint);
                                        // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                        if (outputOffset >= outputLength)
                                        {
                                            renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                            renderFailed = true;
                                            break;
                                        }
                                        // get a pointer to the i'th uint
                                        uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                        // calculate the offset for the i'th ushort,
                                        // becuase we loop based on the input and ushort we dont need an index check here
                                        ulong inputOffset = j * sizeof(ushort);

                                        // get a pointer to the i'th ushort
                                        ushort* rgb565Pointer = (ushort*)(inputLinePointer + inputOffset);

                                        ushort rgb565Value = *rgb565Pointer;

                                        // convert the rgb to the other format
                                        var rgb888Value = getPattern(rgb565Value);

                                        // write the bytes of the rgb888 to the output array
                                        *rgb888Pointer = rgb888Value;
                                    }
                                }
                            }
                            else
                            {
                                //IMPORTANT Anything below related to Pixels Update debug feature
                                #region PIXELS UPDATES
                                if (SkipCached && IsPixelsRowCached((int)i))
                                {
                                    i += (ulong)NativePixelStep;
                                    continue;
                                }

                                pixelCachesStates[i] = true;
                                // traverse the input line by ushorts
                                for (ulong j = 0; j < width; j++)
                                {
                                    if (LibretroCore.RequestVariablesUpdate)
                                    {
                                        break;
                                    }
                                    // calculate the offset for i'th uint
                                    ulong outputOffset = j * sizeof(uint);

                                    // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                    if (outputOffset >= outputLength)
                                    {
                                        renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                        renderFailed = true;
                                        break;
                                    }

                                    // get a pointer to the i'th uint
                                    uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                    // calculate the offset for the i'th ushort,
                                    // becuase we loop based on the input and ushort we dont need an index check here
                                    ulong inputOffset = j * sizeof(ushort);

                                    // get a pointer to the i'th ushort
                                    ushort* rgb565Pointer = (ushort*)(inputLinePointer + inputOffset);

                                    ushort rgb565Value = *rgb565Pointer;

                                    if (SkipCached)
                                    {
                                        if (pixelCaches[i, j] != null && pixelCaches[i, j].input == rgb565Value)
                                        {
                                            *rgb888Pointer = 0;
                                            pixelCachesStates[i] = pixelCachesStates[i] && true;
                                            totalSkipped++;
                                            continue;
                                        }
                                    }

                                    // convert the rgb to the other format
                                    var rgb888Value = getPattern(rgb565Value);

                                    // write the bytes of the rgb888 to the output array
                                    *rgb888Pointer = rgb888Value;
                                    if (SkipCached)
                                    {
                                        CachePixel(rgb565Value, rgb888Value, (int)i, (int)j);
                                        pixelCachesStates[i] = false;
                                    }
                                }
                                #endregion
                            }

                            if (renderFailed)
                            {
                                break;
                            }

                            i += (ulong)NativePixelStep;
                        }
                    }

                }
                catch (Exception ex)
                {
                    renderFailedMessage = ex.Message;
                    renderFailed = true;
                }
            }

            if (SkipCached)
            {
                try
                {
                    var totalPixels = (height * width);
                    if (totalSkipped > (int)totalPixels)
                    {
                        totalPercentageSkipped = 100;
                    }
                    else
                    {
                        totalPercentageSkipped = Math.Round((double)(totalSkipped / (double)(height * width)) * 100.0);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            if (renderFailed)
            {
                var mapData = new Span<byte>(outputArray.ToPointer(), (int)outputLength);
                var payload = new ReadOnlySpan<byte>(inputArray.ToPointer(), (int)inputLength);
                ConvertFrameBufferUshortToXRGB8888WithLUT((uint)width, (uint)height, payload, (int)inputPitch, mapData, (int)outputPitch, isRG565State);
            }
        }

        public static unsafe void ConvertExtremelyUnsafeWithSpan(ulong height, ulong width, IntPtr inputArray, ulong inputLength, ulong inputPitch, IntPtr outputArray, ulong outputLength, ulong outputPitch, bool isRG565State)
        {
            var mapData = new Span<byte>(outputArray.ToPointer(), (int)outputLength);
            if ((NativePixelStep > 1 && inputFillWithBlack) || fillSpanRequired)
            {
                mapData.Fill(0);

                fillSpanRequired = false;
                inputFillWithBlack = false;
            }
            if ((!isGameStarted || AudioOnly))
            {
                return;
            }
            currentWidth = width;
            currentHeight = height;
            totalSkipped = 0;
            isRG565 = isRG565State;
            isRGB888 = false;
            StartIndex = GetStartIndexValue();

            bool renderFailed = FallbackToOldWay;
            var payload = new ReadOnlySpan<byte>(inputArray.ToPointer(), (int)inputLength);

            if (!FallbackToOldWay && CoresCount > 1)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception x)
                {

                }
                cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    List<Task> coresTasks = new List<Task>();
                    var hightPerCore = (int)height / CoresCount;
                    fixed (byte* inputPointer = &payload[0], outputPointer = &mapData[0])
                    {
                        using (var inputManager = new UnmanagedMemoryManager<byte>(inputPointer, (int)inputLength))
                        using (var outputManager = new UnmanagedMemoryManager<byte>(outputPointer, (int)outputLength))
                        {
                            Parallel.For(0, CoresCount, (t) =>
                            {
                                var TStartIndex = (t * hightPerCore);
                                var hightSub = hightPerCore + TStartIndex;
                                if (hightSub > (int)height)
                                {
                                    hightSub = (int)height;
                                }
                                ParallelOptions parallelOptions = new ParallelOptions();
                                parallelOptions.CancellationToken = cancellationTokenSource.Token;
                                var coreTask = Task.Factory.StartNew(() =>
                                {
                                    Parallel.For(TStartIndex, hightSub, (iT) =>
                                    {
                                        try
                                        {
                                            if (LibretroCore.RequestVariablesUpdate)
                                            {
                                                cancellationTokenSource.Cancel();
                                            }
                                            var i = (ulong)iT;
                                            if (cancellationTokenSource.IsCancellationRequested)
                                            {
                                                return;
                                            }

                                            if (!SkipCached)
                                            {
                                                // get a pointer for the first byte of the line of the input
                                                byte* inputLinePointer = (byte*)inputManager.Pin().Pointer + (i * inputPitch);
                                                inputManager.Unpin();

                                                // get a pointer for the first byte of the line of the output
                                                byte* outputLinePointer = (byte*)outputManager.Pin().Pointer + (i * outputPitch);
                                                outputManager.Unpin();

                                                pixelCachesStates[i] = true;
                                                ulong restPixels = 0;
                                                if (CrazyBufferActive)
                                                {
                                                    restPixels = CrazyBufferMove.CrazyMove(outputLinePointer, inputLinePointer, sizeof(ushort), sizeof(uint), width);
                                                    if (iT == 0)
                                                    {
                                                        try
                                                        {
                                                            crazyBufferPercentageHandle = Math.Round(((restPixels * 1d) / (width * 1d) * 100.0));
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                    }
                                                }

                                                // traverse the input line by ushorts
                                                for (ulong j = restPixels; j < width; j++)
                                                {
                                                    if (LibretroCore.RequestVariablesUpdate)
                                                    {
                                                        cancellationTokenSource.Cancel();
                                                    }
                                                    if (cancellationTokenSource.IsCancellationRequested)
                                                    {
                                                        break;
                                                    }

                                                    // calculate the offset for i'th uint
                                                    ulong outputOffset = j * sizeof(uint);

                                                    // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                                    if (outputOffset >= outputLength)
                                                    {
                                                        renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                                        renderFailed = true;
                                                        break;
                                                    }

                                                    // get a pointer to the i'th uint
                                                    uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                                    // calculate the offset for the i'th ushort,
                                                    // becuase we loop based on the input and ushort we dont need an index check here
                                                    ulong inputOffset = j * sizeof(ushort);

                                                    // get a pointer to the i'th ushort
                                                    ushort* rgb565Pointer = (ushort*)(inputLinePointer + inputOffset);

                                                    ushort rgb565Value = *rgb565Pointer;

                                                    // convert the rgb to the other format
                                                    var rgb888Value = getPattern(rgb565Value);

                                                    // write the bytes of the rgb888 to the output array
                                                    *rgb888Pointer = rgb888Value;
                                                }
                                            }
                                            else
                                            {
                                                //IMPORTANT Anything below related to Pixels Update debug feature
                                                #region PIXELS UPDATES
                                                if (IsPixelsRowCached(iT))
                                                {
                                                    iT += NativePixelStep;
                                                }
                                                else
                                                {
                                                    // get a pointer for the first byte of the line of the input
                                                    byte* inputLinePointer = (byte*)inputManager.Pin().Pointer + (i * inputPitch);
                                                    inputManager.Unpin();

                                                    // get a pointer for the first byte of the line of the output
                                                    byte* outputLinePointer = (byte*)outputManager.Pin().Pointer + (i * outputPitch);
                                                    outputManager.Unpin();

                                                    pixelCachesStates[i] = true;
                                                    // traverse the input line by ushorts
                                                    for (ulong j = 0; j < width; j++)
                                                    {
                                                        if (LibretroCore.RequestVariablesUpdate)
                                                        {
                                                            break;
                                                        }
                                                        if (cancellationTokenSource.IsCancellationRequested)
                                                        {
                                                            break;
                                                        }

                                                        // calculate the offset for i'th uint
                                                        ulong outputOffset = j * sizeof(uint);

                                                        // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                                        if (outputOffset >= outputLength)
                                                        {
                                                            renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                                            renderFailed = true;
                                                            break;
                                                        }

                                                        // get a pointer to the i'th uint
                                                        uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                                        // calculate the offset for the i'th ushort,
                                                        // becuase we loop based on the input and ushort we dont need an index check here
                                                        ulong inputOffset = j * sizeof(ushort);

                                                        // get a pointer to the i'th ushort
                                                        ushort* rgb565Pointer = (ushort*)(inputLinePointer + inputOffset);

                                                        ushort rgb565Value = *rgb565Pointer;

                                                        if (SkipCached)
                                                        {
                                                            if (pixelCaches[i, j] != null && pixelCaches[i, j].input == rgb565Value)
                                                            {
                                                                *rgb888Pointer = 0;
                                                                pixelCachesStates[i] = pixelCachesStates[i] && true;
                                                                Interlocked.Add(ref totalSkipped, 1);
                                                                continue;
                                                            }
                                                        }

                                                        // convert the rgb to the other format
                                                        var rgb888Value = getPattern(rgb565Value);

                                                        // write the bytes of the rgb888 to the output array
                                                        *rgb888Pointer = rgb888Value;
                                                        if (SkipCached)
                                                        {
                                                            CachePixel(rgb565Value, rgb888Value, (int)i, (int)j);
                                                            pixelCachesStates[i] = false;
                                                        }
                                                    }
                                                }
                                                if (renderFailed)
                                                {
                                                    cancellationTokenSource.Cancel();
                                                }

                                                iT += NativePixelStep;
                                                #endregion
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                    });
                                }, cancellationTokenSource.Token);
                                coresTasks.Add(coreTask);
                            });
                        }
                    }
                    if (!DontWaitThreads) Task.WaitAll(coresTasks.ToArray());
                }
                catch (Exception ex)
                {
                    renderFailed = true;
                    renderFailedMessage = ex.Message;
                }
            }
            else if (!FallbackToOldWay)
            {
                try
                {
                    // pin down pointers so they dont move on the heap
                    fixed (byte* inputPointer = &payload[0], outputPointer = &mapData[0])
                    {
                        // since we have to account for padding we should go line by line
                        for (ulong i = (ulong)StartIndex; i < height;)
                        {
                            if (LibretroCore.RequestVariablesUpdate)
                            {
                                break;
                            }
                            // get a pointer for the first byte of the line of the input
                            byte* inputLinePointer = inputPointer + (i * inputPitch);

                            // get a pointer for the first byte of the line of the output
                            byte* outputLinePointer = outputPointer + (i * outputPitch);

                            if (!SkipCached)
                            {
                                bool memoryHelperUsed = false;
                                //No memory helpers due to pixels conversion 

                                if (!memoryHelperUsed)
                                {
                                    ulong restPixels = 0;
                                    if (CrazyBufferActive)
                                    {
                                        restPixels = CrazyBufferMove.CrazyMove(outputLinePointer, inputLinePointer, sizeof(ushort), sizeof(uint), width);
                                        if (i == 0)
                                        {
                                            try
                                            {
                                                crazyBufferPercentageHandle = Math.Round(((restPixels * 1d) / (width * 1d) * 100.0));
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                    }
                                    // traverse the input line by ushorts
                                    for (ulong j = restPixels; j < width; j++)
                                    {
                                        if (LibretroCore.RequestVariablesUpdate)
                                        {
                                            break;
                                        }
                                        // calculate the offset for i'th uint
                                        ulong outputOffset = j * sizeof(uint);
                                        // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                        if (outputOffset >= outputLength)
                                        {
                                            renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                            renderFailed = true;
                                            break;
                                        }
                                        // get a pointer to the i'th uint
                                        uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                        // calculate the offset for the i'th ushort,
                                        // becuase we loop based on the input and ushort we dont need an index check here
                                        ulong inputOffset = j * sizeof(ushort);

                                        // get a pointer to the i'th ushort
                                        ushort* rgb565Pointer = (ushort*)(inputLinePointer + inputOffset);

                                        ushort rgb565Value = *rgb565Pointer;

                                        // convert the rgb to the other format
                                        var rgb888Value = getPattern(rgb565Value);

                                        // write the bytes of the rgb888 to the output array
                                        *rgb888Pointer = rgb888Value;
                                    }
                                }
                            }
                            else
                            {
                                //IMPORTANT Anything below related to Pixels Update debug feature
                                #region PIXELS UPDATES
                                if (SkipCached && IsPixelsRowCached((int)i))
                                {
                                    i += (ulong)NativePixelStep;
                                    continue;
                                }

                                pixelCachesStates[i] = true;
                                // traverse the input line by ushorts
                                for (ulong j = 0; j < width; j++)
                                {
                                    if (LibretroCore.RequestVariablesUpdate)
                                    {
                                        break;
                                    }
                                    // calculate the offset for i'th uint
                                    ulong outputOffset = j * sizeof(uint);

                                    // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                    if (outputOffset >= outputLength)
                                    {
                                        renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                        renderFailed = true;
                                        break;
                                    }

                                    // get a pointer to the i'th uint
                                    uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                    // calculate the offset for the i'th ushort,
                                    // becuase we loop based on the input and ushort we dont need an index check here
                                    ulong inputOffset = j * sizeof(ushort);

                                    // get a pointer to the i'th ushort
                                    ushort* rgb565Pointer = (ushort*)(inputLinePointer + inputOffset);

                                    ushort rgb565Value = *rgb565Pointer;

                                    if (SkipCached)
                                    {
                                        if (pixelCaches[i, j] != null && pixelCaches[i, j].input == rgb565Value)
                                        {
                                            *rgb888Pointer = 0;
                                            pixelCachesStates[i] = pixelCachesStates[i] && true;
                                            totalSkipped++;
                                            continue;
                                        }
                                    }

                                    // convert the rgb to the other format
                                    var rgb888Value = getPattern(rgb565Value);

                                    // write the bytes of the rgb888 to the output array
                                    *rgb888Pointer = rgb888Value;
                                    if (SkipCached)
                                    {
                                        CachePixel(rgb565Value, rgb888Value, (int)i, (int)j);
                                        pixelCachesStates[i] = false;
                                    }
                                }
                                #endregion
                            }

                            if (renderFailed)
                            {
                                break;
                            }

                            i += (ulong)NativePixelStep;
                        }
                    }

                }
                catch (Exception ex)
                {
                    renderFailedMessage = ex.Message;
                    renderFailed = true;
                }
            }

            if (SkipCached)
            {
                try
                {
                    var totalPixels = (height * width);
                    if (totalSkipped > (int)totalPixels)
                    {
                        totalPercentageSkipped = 100;
                    }
                    else
                    {
                        totalPercentageSkipped = Math.Round((double)(totalSkipped / (double)(height * width)) * 100.0);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            if (renderFailed)
            {
                ConvertFrameBufferUshortToXRGB8888WithLUT((uint)width, (uint)height, payload, (int)inputPitch, mapData, (int)outputPitch, isRG565State);
            }
        }
        #endregion


        #region FROM X8888 TO 888
        public static unsafe void ConvertExtremelyUnsafe888(ulong height, ulong width, IntPtr inputArray, ulong inputLength, ulong inputPitch, IntPtr outputArray, ulong outputLength, ulong outputPitch)
        {
            if ((NativePixelStep > 1 && inputFillWithBlack) || fillSpanRequired)
            {
                var mapData = new Span<byte>(outputArray.ToPointer(), (int)outputLength);
                mapData.Fill(0);

                fillSpanRequired = false;
                inputFillWithBlack = false;
            }
            if ((!isGameStarted || AudioOnly))
            {
                return;
            }
            currentWidth = width;
            currentHeight = height;

            if (!requestToStopSkipCached)
            {
                SkipCached = false;
                requestToStopSkipCached = true;
                if (RaiseSkippedCachedHandler != null)
                {
                    RaiseSkippedCachedHandler.Invoke(null, EventArgs.Empty);
                }
            }

            totalSkipped = 0;
            isRG565 = false;
            isRGB888 = true;
            StartIndex = GetStartIndexValue();

            bool renderFailed = FallbackToOldWay;

            if (!FallbackToOldWay && CoresCount > 1)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception x)
                {

                }
                cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    List<Task> coresTasks = new List<Task>();
                    var crashFix = (ulong)(height > 1000 ? 2 : 0);
                    var hightPerCore = (int)(height - crashFix) / CoresCount;
                    byte* inputPointer = (byte*)inputArray.ToPointer(), outputPointer = (byte*)outputArray.ToPointer();
                    //fixed (byte* inputPointer = &inputArray[0], outputPointer = &outputArray[0])
                    {
                        using (var inputManager = new UnmanagedMemoryManager<byte>(inputPointer, (int)inputLength))
                        using (var outputManager = new UnmanagedMemoryManager<byte>(outputPointer, (int)outputLength))
                        {
                            Parallel.For(0, CoresCount, (t) =>
                            {
                                var TStartIndex = (t * hightPerCore);
                                var hightSub = hightPerCore + TStartIndex;
                                if (hightSub > (int)height)
                                {
                                    hightSub = (int)height;
                                }
                                ParallelOptions parallelOptions = new ParallelOptions();
                                parallelOptions.CancellationToken = cancellationTokenSource.Token;
                                var coreTask = Task.Factory.StartNew(() =>
                                {
                                    Parallel.For(TStartIndex, hightSub, (iT) =>
                                    {
                                        try
                                        {
                                            if (LibretroCore.RequestVariablesUpdate)
                                            {
                                                cancellationTokenSource.Cancel();
                                            }
                                            var i = (ulong)iT;
                                            if (cancellationTokenSource.IsCancellationRequested)
                                            {
                                                return;
                                            }

                                            // get a pointer for the first byte of the line of the input
                                            byte* inputLinePointer = (byte*)inputManager.Pin().Pointer + (i * inputPitch);
                                            inputManager.Unpin();

                                            // get a pointer for the first byte of the line of the output
                                            byte* outputLinePointer = (byte*)outputManager.Pin().Pointer + (i * outputPitch);
                                            outputManager.Unpin();

                                            if (!SkipCached)
                                            {
                                                bool skipDueOffsetOutOfRange = false;
                                                ulong restPixels = 0;
                                                if (CrazyBufferActive)
                                                {
                                                    //restPixels = CrazyBufferMove8888.CrazyMove(outputLinePointer, inputLinePointer, sizeof(uint), sizeof(uint), width);
                                                    var pointersOffset = restPixels * sizeof(uint);

                                                    if (iT == 0)
                                                    {
                                                        try
                                                        {
                                                            crazyBufferPercentageHandle = Math.Round(((restPixels * 1d) / (width * 1d) * 100.0));
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                    }

                                                    if (pointersOffset >= outputLength)
                                                    {
                                                        iT += NativePixelStep;
                                                        skipDueOffsetOutOfRange = true;
                                                    }
                                                    else
                                                    {
                                                        inputLinePointer = (byte*)(((byte*)inputManager.Pin().Pointer + (i * inputPitch)) + pointersOffset);
                                                        inputManager.Unpin();
                                                        outputLinePointer = (byte*)(((byte*)outputManager.Pin().Pointer + (i * outputPitch)) + pointersOffset);
                                                        outputManager.Unpin();
                                                    }
                                                }
                                                if (!skipDueOffsetOutOfRange && restPixels < width)
                                                {
                                                    switch (MemoryHelper)
                                                    {
                                                        case "Buffer.CopyMemory":
                                                            if (MoveMemoryAvailable)
                                                            {
                                                                Buffer.MemoryCopy(inputLinePointer, outputLinePointer, outputPitch - restPixels, inputPitch - restPixels);
                                                            }
                                                            else
                                                            {
                                                                var SpanInputTemp = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                                                var SpanOutputTemp = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                                                SpanInputTemp.CopyTo(SpanOutputTemp);
                                                                renderFailedMessage = "Span.CopyTo in use due Buffer.CopyMemory failed!";
                                                            }
                                                            break;

                                                        case "memcpy (msvcrt.dll)":
                                                            if (CopyMemoryAvailable)
                                                            {
                                                                CopyMemory(outputLinePointer, (int)(outputPitch - restPixels), inputLinePointer, (int)(inputPitch - restPixels));
                                                            }
                                                            else
                                                            {
                                                                //When no memcpy use default option 
                                                                if (MoveMemoryAvailable)
                                                                {
                                                                    Buffer.MemoryCopy(inputLinePointer, outputLinePointer, outputPitch - restPixels, inputPitch - restPixels);
                                                                    renderFailedMessage = "Buffer.CopyMemory in use due memcpy failed!";
                                                                }
                                                                else
                                                                {
                                                                    var SpanInputTemp = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                                                    var SpanOutputTemp = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                                                    SpanInputTemp.CopyTo(SpanOutputTemp);
                                                                    renderFailedMessage = "Span.CopyTo in use due memcpy, memmov failed!";
                                                                }
                                                            }
                                                            break;

                                                        case "Marshal.CopyTo":
                                                            CopyFromPtrToPtr((IntPtr)inputLinePointer, (uint)(inputPitch - restPixels), (IntPtr)outputLinePointer, (uint)(outputPitch - restPixels));
                                                            break;

                                                        case "Span.CopyTo":
                                                            var SpanInput = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                                            var SpanOutput = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                                            SpanInput.CopyTo(SpanOutput);
                                                            break;
                                                    }
                                                    iT += NativePixelStep;
                                                }
                                            }
                                            else
                                            {
                                                //IMPORTANT Anything below related to Pixels Update debug feature
                                                #region PIXELS UPDATES
                                                if (SkipCached && IsPixelsRowCached(iT))
                                                {
                                                    iT += NativePixelStep;
                                                }
                                                else
                                                {
                                                    pixelCachesStates[i] = true;
                                                    // traverse the input line by uints
                                                    for (ulong j = 0; j < width; j++)
                                                    {
                                                        if (LibretroCore.RequestVariablesUpdate)
                                                        {
                                                            break;
                                                        }
                                                        if (cancellationTokenSource.IsCancellationRequested)
                                                        {
                                                            break;
                                                        }
                                                        // calculate the offset for i'th uint
                                                        ulong outputOffset = j * sizeof(uint);

                                                        // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                                        if (outputOffset >= outputLength)
                                                        {
                                                            renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                                            renderFailed = true;
                                                            break;
                                                        }

                                                        // get a pointer to the i'th uint
                                                        uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                                        // calculate the offset for the i'th uint,
                                                        // becuase we loop based on the input and uint we dont need an index check here
                                                        ulong inputOffset = j * sizeof(uint);

                                                        // get a pointer to the i'th uint
                                                        uint* xrgb888Pointer = (uint*)(inputLinePointer + inputOffset);

                                                        uint xrgb888Value = *xrgb888Pointer;

                                                        if (SkipCached)
                                                        {
                                                            if (pixelCaches[i, j] != null && pixelCaches[i, j].input64 == xrgb888Value)
                                                            {
                                                                *rgb888Pointer = 0;
                                                                pixelCachesStates[i] = pixelCachesStates[i] && true;
                                                                Interlocked.Add(ref totalSkipped, 1);
                                                                continue;
                                                            }
                                                        }

                                                        // convert the rgb to the other format
                                                        var rgb888Value = xrgb888Value;

                                                        // write the bytes of the rgb888 to the output array
                                                        *rgb888Pointer = rgb888Value;
                                                        if (SkipCached)
                                                        {
                                                            CachePixel(xrgb888Value, rgb888Value, (int)i, (int)j);
                                                            pixelCachesStates[i] = false;
                                                        }
                                                    }
                                                    if (renderFailed)
                                                    {
                                                        cancellationTokenSource.Cancel();
                                                    }

                                                    iT += NativePixelStep;
                                                }
                                                #endregion
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    });
                                }, cancellationTokenSource.Token);
                                coresTasks.Add(coreTask);
                            });
                        }
                    }
                    if (!DontWaitThreads) Task.WaitAll(coresTasks.ToArray());
                }
                catch (Exception ex)
                {
                    renderFailed = true;
                    renderFailedMessage = ex.Message;
                }
            }
            else if (!FallbackToOldWay)
            {
                try
                {
                    // pin down pointers so they dont move on the heap
                    byte* inputPointer = (byte*)inputArray.ToPointer(), outputPointer = (byte*)outputArray.ToPointer();
                    //fixed (byte* inputPointer = &inputArray[0], outputPointer = &outputArray[0])
                    {
                        // since we have to account for padding we should go line by line
                        var crashFix = (ulong)(height > 1000 ? 2 : 0);
                        for (ulong i = (ulong)StartIndex; i < height - crashFix;)
                        {
                            if (LibretroCore.RequestVariablesUpdate)
                            {
                                break;
                            }
                            // get a pointer for the first byte of the line of the input
                            byte* inputLinePointer = inputPointer + (i * inputPitch);

                            // get a pointer for the first byte of the line of the output
                            byte* outputLinePointer = outputPointer + (i * outputPitch);

                            if (!SkipCached)
                            {
                                ulong restPixels = 0;
                                if (CrazyBufferActive)
                                {
                                    //restPixels = CrazyBufferMove8888.CrazyMove(outputLinePointer, inputLinePointer, sizeof(uint), sizeof(uint), width);
                                    var pointersOffset = restPixels * sizeof(uint);

                                    if (i == 0)
                                    {
                                        try
                                        {
                                            crazyBufferPercentageHandle = Math.Round(((restPixels * 1d) / (width * 1d) * 100.0));
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }

                                    if (pointersOffset >= outputLength || restPixels == width)
                                    {
                                        i += (ulong)NativePixelStep;
                                        continue;
                                    }
                                    else
                                    {
                                        inputLinePointer = (byte*)((inputPointer + (i * inputPitch)) + pointersOffset);
                                        outputLinePointer = (byte*)((outputPointer + (i * outputPitch)) + pointersOffset);
                                    }
                                }

                                switch (MemoryHelper)
                                {
                                    case "Buffer.CopyMemory":
                                        //When no memmove use default option 
                                        if (MoveMemoryAvailable)
                                        {
                                            Buffer.MemoryCopy(inputLinePointer, outputLinePointer, outputPitch - restPixels, inputPitch - restPixels);
                                        }
                                        else
                                        {
                                            var SpanInputTemp = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                            var SpanOutputTemp = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                            SpanInputTemp.CopyTo(SpanOutputTemp);
                                            renderFailedMessage = "Span.CopyTo in use due Buffer.CopyMemory failed!";
                                        }
                                        break;

                                    case "memcpy (msvcrt.dll)":
                                        if (CopyMemoryAvailable)
                                        {
                                            CopyMemory(outputLinePointer, (int)(outputPitch - restPixels), inputLinePointer, (int)(inputPitch - restPixels));
                                        }
                                        else
                                        {
                                            //When no memcpy use default option 
                                            if (MoveMemoryAvailable)
                                            {
                                                Buffer.MemoryCopy(inputLinePointer, outputLinePointer, outputPitch - restPixels, inputPitch - restPixels);
                                                renderFailedMessage = "Buffer.CopyMemory in use due memcpy failed!";
                                            }
                                            else
                                            {
                                                var SpanInputTemp = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                                var SpanOutputTemp = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                                SpanInputTemp.CopyTo(SpanOutputTemp);
                                                renderFailedMessage = "Span.CopyTo in use due memcpy, memmov failed!";
                                            }
                                        }
                                        break;

                                    case "Marshal.CopyTo":
                                        CopyFromPtrToPtr((IntPtr)inputLinePointer, (uint)(inputPitch - restPixels), (IntPtr)outputLinePointer, (uint)(outputPitch - restPixels));
                                        break;

                                    case "Span.CopyTo":
                                        var SpanInput = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                        var SpanOutput = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                        SpanInput.CopyTo(SpanOutput);
                                        break;
                                }


                                i += (ulong)NativePixelStep;
                                continue;
                            }

                            if (SkipCached && IsPixelsRowCached((int)i))
                            {
                                i += (ulong)NativePixelStep;
                                continue;
                            }

                            //IMPORTANT Anything below related to Pixels Update debug feature
                            //TO-DO something is wrong wth the total skipped pixels
                            #region PIXELS UPDATES
                            pixelCachesStates[i] = true;

                            // traverse the input line by uints
                            for (ulong j = 0; j < width; j++)
                            {
                                if (LibretroCore.RequestVariablesUpdate)
                                {
                                    break;
                                }
                                // calculate the offset for i'th uint
                                ulong pointersOffset = j * sizeof(uint);

                                // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                if (pointersOffset >= outputLength)
                                {
                                    renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                    renderFailed = true;
                                    break;
                                }

                                // get a pointer to the i'th uint
                                uint* rgb888Pointer = (uint*)(outputLinePointer + pointersOffset);

                                // get a pointer to the i'th uint
                                uint* xrgb888Pointer = (uint*)(inputLinePointer + pointersOffset);

                                uint xrgb888Value = *xrgb888Pointer;

                                if (SkipCached)
                                {
                                    if (pixelCaches[i, j] != null && pixelCaches[i, j].input64 == xrgb888Value)
                                    {
                                        *rgb888Pointer = 0;
                                        pixelCachesStates[i] = pixelCachesStates[i] && true;
                                        totalSkipped++;
                                        continue;
                                    }
                                }

                                // convert the rgb to the other format
                                uint rgb888Value = xrgb888Value;

                                // write the bytes of the rgb888 to the output array
                                *rgb888Pointer = rgb888Value;
                                if (SkipCached)
                                {
                                    CachePixel(xrgb888Value, rgb888Value, (int)i, (int)j);
                                    pixelCachesStates[i] = false;
                                }
                            }
                            #endregion

                            if (renderFailed)
                            {
                                break;
                            }

                            i += (ulong)NativePixelStep;
                        }
                    }

                }
                catch (Exception ex)
                {
                    renderFailedMessage = ex.Message;
                    renderFailed = true;
                }
            }

            if (SkipCached)
            {
                try
                {
                    var totalPixels = (height * width);
                    if (totalSkipped > (int)totalPixels)
                    {
                        totalPercentageSkipped = 100;
                    }
                    else
                    {
                        totalPercentageSkipped = Math.Round((double)(totalSkipped / (double)(height * width)) * 100.0);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            if (renderFailed)
            {
                var mapData = new Span<byte>(outputArray.ToPointer(), (int)outputLength);
                var payload = new ReadOnlySpan<byte>(inputArray.ToPointer(), (int)inputLength);
                ConvertFrameBufferXRGB8888((uint)width, (uint)height, payload, (int)inputPitch, mapData, (int)outputPitch);
            }
        }
        public static unsafe void ConvertExtremelyUnsafe888WithSpan(ulong height, ulong width, IntPtr inputArray, ulong inputLength, ulong inputPitch, IntPtr outputArray, ulong outputLength, ulong outputPitch)
        {
            var mapData = new Span<byte>(outputArray.ToPointer(), (int)outputLength);

            if ((NativePixelStep > 1 && inputFillWithBlack) || fillSpanRequired)
            {
                mapData.Fill(0);

                fillSpanRequired = false;
                inputFillWithBlack = false;
            }
            if ((!isGameStarted || AudioOnly))
            {
                return;
            }
            currentWidth = width;
            currentHeight = height;

            if (!requestToStopSkipCached)
            {
                SkipCached = false;
                requestToStopSkipCached = true;
                if (RaiseSkippedCachedHandler != null)
                {
                    RaiseSkippedCachedHandler.Invoke(null, EventArgs.Empty);
                }
            }

            totalSkipped = 0;
            isRG565 = false;
            isRGB888 = true;
            StartIndex = GetStartIndexValue();

            bool renderFailed = FallbackToOldWay;

            var payload = new ReadOnlySpan<byte>(inputArray.ToPointer(), (int)inputLength);
            if (!FallbackToOldWay && CoresCount > 1)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception x)
                {

                }
                cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    List<Task> coresTasks = new List<Task>();
                    var crashFix = (ulong)(height > 1000 ? 2 : 0);
                    var hightPerCore = (int)(height - crashFix) / CoresCount;
                    fixed (byte* inputPointer = &payload[0], outputPointer = &mapData[0])
                    {
                        using (var inputManager = new UnmanagedMemoryManager<byte>(inputPointer, (int)inputLength))
                        using (var outputManager = new UnmanagedMemoryManager<byte>(outputPointer, (int)outputLength))
                        {
                            Parallel.For(0, CoresCount, (t) =>
                            {
                                var TStartIndex = (t * hightPerCore);
                                var hightSub = hightPerCore + TStartIndex;
                                if (hightSub > (int)height)
                                {
                                    hightSub = (int)height;
                                }
                                ParallelOptions parallelOptions = new ParallelOptions();
                                parallelOptions.CancellationToken = cancellationTokenSource.Token;
                                var coreTask = Task.Factory.StartNew(() =>
                                {
                                    Parallel.For(TStartIndex, hightSub, (iT) =>
                                    {
                                        try
                                        {
                                            if (LibretroCore.RequestVariablesUpdate)
                                            {
                                                cancellationTokenSource.Cancel();
                                            }
                                            var i = (ulong)iT;
                                            if (cancellationTokenSource.IsCancellationRequested)
                                            {
                                                return;
                                            }

                                            // get a pointer for the first byte of the line of the input
                                            byte* inputLinePointer = (byte*)inputManager.Pin().Pointer + (i * inputPitch);
                                            inputManager.Unpin();

                                            // get a pointer for the first byte of the line of the output
                                            byte* outputLinePointer = (byte*)outputManager.Pin().Pointer + (i * outputPitch);
                                            outputManager.Unpin();

                                            if (!SkipCached)
                                            {
                                                bool skipDueOffsetOutOfRange = false;
                                                ulong restPixels = 0;
                                                if (CrazyBufferActive)
                                                {
                                                    //restPixels = CrazyBufferMove8888.CrazyMove(outputLinePointer, inputLinePointer, sizeof(uint), sizeof(uint), width);
                                                    var pointersOffset = restPixels * sizeof(uint);

                                                    if (iT == 0)
                                                    {
                                                        try
                                                        {
                                                            crazyBufferPercentageHandle = Math.Round(((restPixels * 1d) / (width * 1d) * 100.0));
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                    }

                                                    if (pointersOffset >= outputLength)
                                                    {
                                                        iT += NativePixelStep;
                                                        skipDueOffsetOutOfRange = true;
                                                    }
                                                    else
                                                    {
                                                        inputLinePointer = (byte*)(((byte*)inputManager.Pin().Pointer + (i * inputPitch)) + pointersOffset);
                                                        inputManager.Unpin();
                                                        outputLinePointer = (byte*)(((byte*)outputManager.Pin().Pointer + (i * outputPitch)) + pointersOffset);
                                                        outputManager.Unpin();
                                                    }
                                                }
                                                if (!skipDueOffsetOutOfRange && restPixels < width)
                                                {
                                                    switch (MemoryHelper)
                                                    {
                                                        case "Buffer.CopyMemory":
                                                            if (MoveMemoryAvailable)
                                                            {
                                                                Buffer.MemoryCopy(inputLinePointer, outputLinePointer, outputPitch - restPixels, inputPitch - restPixels);
                                                            }
                                                            else
                                                            {
                                                                var SpanInputTemp = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                                                var SpanOutputTemp = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                                                SpanInputTemp.CopyTo(SpanOutputTemp);
                                                                renderFailedMessage = "Span.CopyTo in use due Buffer.CopyMemory failed!";
                                                            }
                                                            break;

                                                        case "memcpy (msvcrt.dll)":
                                                            if (CopyMemoryAvailable)
                                                            {
                                                                CopyMemory(outputLinePointer, (int)(outputPitch - restPixels), inputLinePointer, (int)(inputPitch - restPixels));
                                                            }
                                                            else
                                                            {
                                                                //When no memcpy use default option 
                                                                if (MoveMemoryAvailable)
                                                                {
                                                                    Buffer.MemoryCopy(inputLinePointer, outputLinePointer, outputPitch - restPixels, inputPitch - restPixels);
                                                                    renderFailedMessage = "Buffer.CopyMemory in use due memcpy failed!";
                                                                }
                                                                else
                                                                {
                                                                    var SpanInputTemp = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                                                    var SpanOutputTemp = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                                                    SpanInputTemp.CopyTo(SpanOutputTemp);
                                                                    renderFailedMessage = "Span.CopyTo in use due memcpy, memmov failed!";
                                                                }
                                                            }
                                                            break;

                                                        case "Marshal.CopyTo":
                                                            CopyFromPtrToPtr((IntPtr)inputLinePointer, (uint)(inputPitch - restPixels), (IntPtr)outputLinePointer, (uint)(outputPitch - restPixels));
                                                            break;

                                                        case "Span.CopyTo":
                                                            var SpanInput = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                                            var SpanOutput = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                                            SpanInput.CopyTo(SpanOutput);
                                                            break;
                                                    }
                                                    iT += NativePixelStep;
                                                }
                                            }
                                            else
                                            {
                                                //IMPORTANT Anything below related to Pixels Update debug feature
                                                #region PIXELS UPDATES
                                                if (SkipCached && IsPixelsRowCached(iT))
                                                {
                                                    iT += NativePixelStep;
                                                }
                                                else
                                                {
                                                    pixelCachesStates[i] = true;
                                                    // traverse the input line by uints
                                                    for (ulong j = 0; j < width; j++)
                                                    {
                                                        if (LibretroCore.RequestVariablesUpdate)
                                                        {
                                                            break;
                                                        }
                                                        if (cancellationTokenSource.IsCancellationRequested)
                                                        {
                                                            break;
                                                        }
                                                        // calculate the offset for i'th uint
                                                        ulong outputOffset = j * sizeof(uint);

                                                        // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                                        if (outputOffset >= outputLength)
                                                        {
                                                            renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                                            renderFailed = true;
                                                            break;
                                                        }

                                                        // get a pointer to the i'th uint
                                                        uint* rgb888Pointer = (uint*)(outputLinePointer + outputOffset);

                                                        // calculate the offset for the i'th uint,
                                                        // becuase we loop based on the input and uint we dont need an index check here
                                                        ulong inputOffset = j * sizeof(uint);

                                                        // get a pointer to the i'th uint
                                                        uint* xrgb888Pointer = (uint*)(inputLinePointer + inputOffset);

                                                        uint xrgb888Value = *xrgb888Pointer;

                                                        if (SkipCached)
                                                        {
                                                            if (pixelCaches[i, j] != null && pixelCaches[i, j].input64 == xrgb888Value)
                                                            {
                                                                *rgb888Pointer = 0;
                                                                pixelCachesStates[i] = pixelCachesStates[i] && true;
                                                                Interlocked.Add(ref totalSkipped, 1);
                                                                continue;
                                                            }
                                                        }

                                                        // convert the rgb to the other format
                                                        var rgb888Value = xrgb888Value;

                                                        // write the bytes of the rgb888 to the output array
                                                        *rgb888Pointer = rgb888Value;
                                                        if (SkipCached)
                                                        {
                                                            CachePixel(xrgb888Value, rgb888Value, (int)i, (int)j);
                                                            pixelCachesStates[i] = false;
                                                        }
                                                    }
                                                    if (renderFailed)
                                                    {
                                                        cancellationTokenSource.Cancel();
                                                    }

                                                    iT += NativePixelStep;
                                                }
                                                #endregion
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    });
                                }, cancellationTokenSource.Token);
                                coresTasks.Add(coreTask);
                            });
                        }
                    }
                    if (!DontWaitThreads) Task.WaitAll(coresTasks.ToArray());
                }
                catch (Exception ex)
                {
                    renderFailed = true;
                    renderFailedMessage = ex.Message;
                }
            }
            else if (!FallbackToOldWay)
            {
                try
                {
                    // pin down pointers so they dont move on the heap
                    fixed (byte* inputPointer = &payload[0], outputPointer = &mapData[0])
                    {
                        // since we have to account for padding we should go line by line
                        var crashFix = (ulong)(height > 1000 ? 2 : 0);
                        for (ulong i = (ulong)StartIndex; i < height - crashFix;)
                        {
                            if (LibretroCore.RequestVariablesUpdate)
                            {
                                break;
                            }
                            // get a pointer for the first byte of the line of the input
                            byte* inputLinePointer = inputPointer + (i * inputPitch);

                            // get a pointer for the first byte of the line of the output
                            byte* outputLinePointer = outputPointer + (i * outputPitch);

                            if (!SkipCached)
                            {
                                ulong restPixels = 0;
                                if (CrazyBufferActive)
                                {
                                    //restPixels = CrazyBufferMove8888.CrazyMove(outputLinePointer, inputLinePointer, sizeof(uint), sizeof(uint), width);
                                    var pointersOffset = restPixels * sizeof(uint);

                                    if (i == 0)
                                    {
                                        try
                                        {
                                            crazyBufferPercentageHandle = Math.Round(((restPixels * 1d) / (width * 1d) * 100.0));
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }

                                    if (pointersOffset >= outputLength || restPixels == width)
                                    {
                                        i += (ulong)NativePixelStep;
                                        continue;
                                    }
                                    else
                                    {
                                        inputLinePointer = (byte*)((inputPointer + (i * inputPitch)) + pointersOffset);
                                        outputLinePointer = (byte*)((outputPointer + (i * outputPitch)) + pointersOffset);
                                    }
                                }

                                switch (MemoryHelper)
                                {
                                    case "Buffer.CopyMemory":
                                        //When no memmove use default option 
                                        if (MoveMemoryAvailable)
                                        {
                                            Buffer.MemoryCopy(inputLinePointer, outputLinePointer, outputPitch - restPixels, inputPitch - restPixels);
                                        }
                                        else
                                        {
                                            var SpanInputTemp = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                            var SpanOutputTemp = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                            SpanInputTemp.CopyTo(SpanOutputTemp);
                                            renderFailedMessage = "Span.CopyTo in use due Buffer.CopyMemory failed!";
                                        }
                                        break;

                                    case "memcpy (msvcrt.dll)":
                                        if (CopyMemoryAvailable)
                                        {
                                            CopyMemory(outputLinePointer, (int)(outputPitch - restPixels), inputLinePointer, (int)(inputPitch - restPixels));
                                        }
                                        else
                                        {
                                            //When no memcpy use default option 
                                            if (MoveMemoryAvailable)
                                            {
                                                Buffer.MemoryCopy(inputLinePointer, outputLinePointer, outputPitch - restPixels, inputPitch - restPixels);
                                                renderFailedMessage = "Buffer.CopyMemory in use due memcpy failed!";
                                            }
                                            else
                                            {
                                                var SpanInputTemp = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                                var SpanOutputTemp = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                                SpanInputTemp.CopyTo(SpanOutputTemp);
                                                renderFailedMessage = "Span.CopyTo in use due memcpy, memmov failed!";
                                            }
                                        }
                                        break;

                                    case "Marshal.CopyTo":
                                        CopyFromPtrToPtr((IntPtr)inputLinePointer, (uint)(inputPitch - restPixels), (IntPtr)outputLinePointer, (uint)(outputPitch - restPixels));
                                        break;

                                    case "Span.CopyTo":
                                        var SpanInput = new Span<byte>(inputLinePointer, (int)(inputPitch - restPixels));
                                        var SpanOutput = new Span<byte>(outputLinePointer, (int)(outputPitch - restPixels));
                                        SpanInput.CopyTo(SpanOutput);
                                        break;
                                }


                                i += (ulong)NativePixelStep;
                                continue;
                            }

                            if (SkipCached && IsPixelsRowCached((int)i))
                            {
                                i += (ulong)NativePixelStep;
                                continue;
                            }

                            //IMPORTANT Anything below related to Pixels Update debug feature
                            //TO-DO something is wrong wth the total skipped pixels
                            #region PIXELS UPDATES
                            pixelCachesStates[i] = true;

                            // traverse the input line by uints
                            for (ulong j = 0; j < width; j++)
                            {
                                if (LibretroCore.RequestVariablesUpdate)
                                {
                                    break;
                                }
                                // calculate the offset for i'th uint
                                ulong pointersOffset = j * sizeof(uint);

                                // at least attempt to avoid overflowing a buffer, not that the runtime would let you do that, i would hope..
                                if (pointersOffset >= outputLength)
                                {
                                    renderFailedMessage = new ArgumentOutOfRangeException().Message;
                                    renderFailed = true;
                                    break;
                                }

                                // get a pointer to the i'th uint
                                uint* rgb888Pointer = (uint*)(outputLinePointer + pointersOffset);

                                // get a pointer to the i'th uint
                                uint* xrgb888Pointer = (uint*)(inputLinePointer + pointersOffset);

                                uint xrgb888Value = *xrgb888Pointer;

                                if (SkipCached)
                                {
                                    if (pixelCaches[i, j] != null && pixelCaches[i, j].input64 == xrgb888Value)
                                    {
                                        *rgb888Pointer = 0;
                                        pixelCachesStates[i] = pixelCachesStates[i] && true;
                                        totalSkipped++;
                                        continue;
                                    }
                                }

                                // convert the rgb to the other format
                                uint rgb888Value = xrgb888Value;

                                // write the bytes of the rgb888 to the output array
                                *rgb888Pointer = rgb888Value;
                                if (SkipCached)
                                {
                                    CachePixel(xrgb888Value, rgb888Value, (int)i, (int)j);
                                    pixelCachesStates[i] = false;
                                }
                            }
                            #endregion

                            if (renderFailed)
                            {
                                break;
                            }

                            i += (ulong)NativePixelStep;
                        }
                    }

                }
                catch (Exception ex)
                {
                    renderFailedMessage = ex.Message;
                    renderFailed = true;
                }
            }

            if (SkipCached)
            {
                try
                {
                    var totalPixels = (height * width);
                    if (totalSkipped > (int)totalPixels)
                    {
                        totalPercentageSkipped = 100;
                    }
                    else
                    {
                        totalPercentageSkipped = Math.Round((double)(totalSkipped / (double)(height * width)) * 100.0);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            if (renderFailed)
            {
                ConvertFrameBufferXRGB8888((uint)width, (uint)height, payload, (int)inputPitch, mapData, (int)outputPitch);
            }
        }

        #endregion

        //Below will be used only if the new solution failed
        #region OLD X8888 TO 888
        public unsafe static void ConvertFrameBufferXRGB8888(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch)
        {
            try
            {
                if (!isGameStarted || AudioOnly)
                {
                    return;
                }
                var castInput = MemoryMarshal.Cast<byte, uint>(input);
                var castInputPitch = inputPitch / sizeof(uint);
                var castOutput = MemoryMarshal.Cast<byte, uint>(output);
                var castOutputPitch = outputPitch / sizeof(uint);

                if (!requestToStopSkipCached)
                {
                    SkipCached = false;
                    requestToStopSkipCached = true;
                    if (RaiseSkippedCachedHandler != null)
                    {
                        RaiseSkippedCachedHandler.Invoke(null, EventArgs.Empty);
                    }
                }
                RenderHelperCallInstant(height, width, castInput, castOutput, castInputPitch, castOutputPitch);
            }
            catch (Exception e)
            {

            }
        }

        private unsafe static void RenderHelperCallInstant(uint height, uint width, ReadOnlySpan<uint> castInput, Span<uint> castOutput, int castInputPitch, int castOutputPitch)
        {
            //Stopwatch ss = new Stopwatch();
            //ss.Start();
            if ((!isGameStarted || AudioOnly) && !fillSpanRequired)
            {
                return;
            }
            isRGB888 = true;

            try
            {
                cancellationTokenSource.Cancel();
            }
            catch (Exception x)
            {

            }
            cancellationTokenSource = new CancellationTokenSource();

            StartIndex = GetStartIndexValue();
            List<Task> coresTasks = new List<Task>();
            if ((NativePixelStep > 1 && inputFillWithBlack) || fillSpanRequired)
            {
                castOutput.Fill(0);
                fillSpanRequired = false;
                inputFillWithBlack = false;
            }
            if (CoresCount > 1)
            {
                unsafe
                {
                    // data is Span<T> or ReadOnlySpan<T>
                    fixed (uint* pointer = &castOutput.GetPinnableReference())
                    {
                        using (var memoryManager = new UnmanagedMemoryManager<uint>(pointer, castOutput.Length))
                        {
                            var memory = memoryManager.Memory;
                            fixed (uint* upointer = &castInput.GetPinnableReference())
                            {
                                using (var umemoryManager = new UnmanagedMemoryManager<uint>(upointer, castInput.Length))
                                {
                                    var umemory = umemoryManager.Memory;
                                    {
                                        totalSkipped = 0;
                                        var hightPerCore = height / CoresCount;
                                        Parallel.For(0, CoresCount, (t) =>
                                        {
                                            if (cancellationTokenSource.IsCancellationRequested)
                                            {
                                                return;
                                            }
                                            var TStartIndex = (int)(t * hightPerCore);
                                            var hightSub = hightPerCore + TStartIndex;
                                            if (hightSub > height)
                                            {
                                                hightSub = height;
                                            }
                                            var coreTask = Task.Factory.StartNew(() =>
                                            {
                                                Parallel.For(TStartIndex, (int)hightSub, (i) =>
                                                {
                                                    if (cancellationTokenSource.IsCancellationRequested)
                                                    {
                                                        return;
                                                    }
                                                    if (SkipCached && IsPixelsRowCached(i))
                                                    {
                                                        i += NativePixelStep;
                                                        Interlocked.Add(ref totalSkipped, (int)width);
                                                    }
                                                    else
                                                    {
                                                        var inputLine = umemory.Slice(i * castInputPitch, castInputPitch);
                                                        var outputLine = memory.Slice(i * castOutputPitch, castOutputPitch);

                                                        GetPixel(inputLine, outputLine, width, i);
                                                        i += NativePixelStep;
                                                    }
                                                });
                                            }, cancellationTokenSource.Token);
                                            coresTasks.Add(coreTask);
                                        });
                                        if (!DontWaitThreads) Task.WaitAll(coresTasks.ToArray());
                                    }
                                }
                                // you must use the memory object inside this fixed block!
                            }
                        }
                        // you must use the memory object inside this fixed block!
                    }
                    if (SkipCached)
                    {
                        try
                        {
                            totalPercentageSkipped = Math.Round((double)(totalSkipped / (double)(height * width)) * 100.0);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            else
            {
                totalSkipped = 0;
                for (var i = StartIndex; i < height;)
                {
                    if (SkipCached && IsPixelsRowCached(i))
                    {
                        i += NativePixelStep;
                        totalSkipped += (int)width;
                        continue;
                    }
                    try
                    {
                        var inputLine = castInput.Slice(i * castInputPitch, castInputPitch);
                        var outputLine = castOutput.Slice(i * castOutputPitch, castOutputPitch);
                        GetPixel(inputLine, outputLine, width, i);
                        i += NativePixelStep;
                    }
                    catch (Exception e)
                    {

                    }
                }
                if (SkipCached)
                {
                    try
                    {
                        totalPercentageSkipped = Math.Round((double)(totalSkipped / (double)(height * width)) * 100.0);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            //ss.Stop();
            //tt.Add(ss.Elapsed);
        }
        unsafe private static void GetPixel(ReadOnlySpan<uint> inputLine, Span<uint> outputLine, uint width, int i)
        {
            if (CurrentColorFilter > 0 || SkipCached)
            {
                pixelCachesStates[i] = true;
                for (var j = 0; j < width;)
                {
                    try
                    {
                        if (SkipCached)
                        {
                            if (pixelCaches[i, j] != null && inputLine[j] == pixelCaches[i, j].input64)
                            {
                                outputLine[j] = 0;
                                pixelCachesStates[i] = pixelCachesStates[i] && true;
                                j++;
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    outputLine[j] = inputLine[j];
                    CachePixel(inputLine[j], outputLine[j], i, j);
                    pixelCachesStates[i] = false;
                    j++;
                }
            }
            else
            {
                inputLine.CopyTo(outputLine);
            }
        }

        private static void GetPixel(Memory<uint> inputLine, Memory<uint> outputLine, uint width, int i)
        {
            if (CurrentColorFilter > 0 || SkipCached)
            {
                pixelCachesStates[i] = true;
                for (var j = 0; j < width;)
                {
                    try
                    {
                        if (SkipCached)
                        {
                            if (pixelCaches[i, j] != null && inputLine.Span[j] == pixelCaches[i, j].input64)
                            {
                                outputLine.Span[j] = 0;
                                pixelCachesStates[i] = pixelCachesStates[i] && true;
                                j++;
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    outputLine.Span[j] = inputLine.Span[j];
                    CachePixel(inputLine.Span[j], outputLine.Span[j], i, j);
                    pixelCachesStates[i] = false;
                    j++;
                }

            }
            else
            {
                if (pixelCachesConfirms[i] > 0)
                {
                    pixelCachesRelay[i] = 0;
                    pixelCachesConfirms[i] = 0;
                    pixelCachesStability[i] = 0;
                }
                inputLine.CopyTo(outputLine);
            }
        }
        #endregion

        #region OLD 565,555 to 888
        //This old conversion function, the above new one is more faster
        private static void ConvertFrameBufferUshortToXRGB8888WithLUT(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch, bool is565)
        {
            if ((!isGameStarted || AudioOnly) && !fillSpanRequired)
            {
                return;
            }
            isRGB888 = false;
            try
            {
                var castInput = MemoryMarshal.Cast<byte, ushort>(input);
                var castInputPitch = inputPitch / sizeof(ushort);
                var castOutput = MemoryMarshal.Cast<byte, uint>(output);
                var castOutputPitch = outputPitch / sizeof(uint);

                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception x)
                {

                }
                cancellationTokenSource = new CancellationTokenSource();
                StartIndex = GetStartIndexValue();
                isRG565 = is565;
                List<Task> coresTasks = new List<Task>();
                if ((NativePixelStep > 1 && inputFillWithBlack) || fillSpanRequired)
                {
                    castOutput.Fill(0);
                }
                if (CoresCount > 1)
                {
                    unsafe
                    {
                        // data is Span<T> or ReadOnlySpan<T>
                        fixed (uint* pointer = &castOutput.GetPinnableReference())
                        {
                            using (var memoryManager = new UnmanagedMemoryManager<uint>(pointer, castOutput.Length))
                            {
                                var memory = memoryManager.Memory;
                                fixed (ushort* upointer = &castInput.GetPinnableReference())
                                {
                                    using (var umemoryManager = new UnmanagedMemoryManager<ushort>(upointer, castInput.Length))
                                    {
                                        var umemory = umemoryManager.Memory;
                                        {
                                            totalSkipped = 0;
                                            var hightPerCore = height / CoresCount;
                                            Parallel.For(0, CoresCount, (t) =>
                                             {
                                                 var TStartIndex = (int)(t * hightPerCore);
                                                 var hightSub = hightPerCore + TStartIndex;
                                                 if (hightSub > height)
                                                 {
                                                     hightSub = height;
                                                 }
                                                 ParallelOptions parallelOptions = new ParallelOptions();
                                                 //parallelOptions.MaxDegreeOfParallelism = 1;
                                                 parallelOptions.CancellationToken = cancellationTokenSource.Token;
                                                 var coreTask = Task.Factory.StartNew(() =>
                                                 {
                                                     Parallel.For(TStartIndex, (int)hightSub, (i) =>
                                                     {
                                                         if (cancellationTokenSource.IsCancellationRequested)
                                                         {
                                                             return;
                                                         }
                                                         if (SkipCached && IsPixelsRowCached(i))
                                                         {
                                                             i += NativePixelStep;
                                                             Interlocked.Add(ref totalSkipped, (int)width);
                                                         }
                                                         else
                                                         {
                                                             var inputLine = umemory.Slice(i * castInputPitch, castInputPitch);
                                                             var outputLine = memory.Slice(i * castOutputPitch, castOutputPitch);

                                                             pixelCachesStates[i] = true;
                                                             for (var j = 0; j < width;)
                                                             {
                                                                 if (cancellationTokenSource.IsCancellationRequested)
                                                                 {
                                                                     break;
                                                                 }

                                                                 try
                                                                 {
                                                                     if (SkipCached)
                                                                     {
                                                                         if (pixelCaches[i, j] != null && inputLine.Span[j] == pixelCaches[i, j].input)
                                                                         {
                                                                             outputLine.Span[j] = 0;
                                                                             pixelCachesStates[i] = pixelCachesStates[i] && true;
                                                                             j++;
                                                                             continue;
                                                                         }
                                                                     }
                                                                 }
                                                                 catch (Exception ex)
                                                                 {

                                                                 }
                                                                 outputLine.Span[j] = getPattern(inputLine.Span[j]);
                                                                 CachePixel(inputLine.Span[j], outputLine.Span[j], i, j);
                                                                 pixelCachesStates[i] = false;

                                                                 j++;

                                                             }
                                                             i += NativePixelStep;
                                                         }
                                                     });
                                                 }, cancellationTokenSource.Token);
                                                 coresTasks.Add(coreTask);
                                             });
                                            if (!DontWaitThreads) Task.WaitAll(coresTasks.ToArray());
                                        }
                                    }
                                    // you must use the memory object inside this fixed block!
                                }
                            }
                            // you must use the memory object inside this fixed block!
                        }
                        if (SkipCached)
                        {
                            try
                            {
                                var totalPixels = (height * width);
                                if (totalSkipped > (int)totalPixels)
                                {
                                    totalPercentageSkipped = 100;
                                }
                                else
                                    totalPercentageSkipped = Math.Round((double)(totalSkipped / (double)(height * width)) * 100.0);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
                else
                {
                    totalSkipped = 0;
                    for (var i = StartIndex; i < height;)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        if (SkipCached && IsPixelsRowCached(i))
                        {
                            i += NativePixelStep;
                            totalSkipped += (int)width;
                            continue;
                        }
                        var inputLine = castInput.Slice(i * castInputPitch, castInputPitch);
                        var outputLine = castOutput.Slice(i * castOutputPitch, castOutputPitch);
                        pixelCachesStates[i] = true;
                        for (var j = 0; j < width;)
                        {
                            try
                            {
                                if (cancellationTokenSource.IsCancellationRequested)
                                {
                                    break;
                                }
                                if (SkipCached)
                                {
                                    if (pixelCaches[i, j] != null && inputLine[j] == pixelCaches[i, j].input)
                                    {
                                        outputLine[j] = 0;
                                        pixelCachesStates[i] = pixelCachesStates[i] && true;
                                        j++;
                                        continue;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                            outputLine[j] = getPattern(inputLine[j]);
                            CachePixel(inputLine[j], outputLine[j], i, j);
                            pixelCachesStates[i] = false;

                            j++;
                        }

                        i += NativePixelStep;
                    }
                    if (SkipCached)
                    {
                        try
                        {
                            var totalPixels = (height * width);
                            if (totalSkipped > (int)totalPixels)
                            {
                                totalPercentageSkipped = 100;
                            }
                            else
                                totalPercentageSkipped = Math.Round((double)(totalSkipped / (double)(height * width)) * 100.0);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        #endregion

        #region Pixels Updates
        static uint getPixelPointer(ushort x) => getPattern(x);
        public static bool isRG565 = false;
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        static PixelCache[,] pixelCaches = new PixelCache[2080, 2080];
        static bool[] pixelCachesStates = new bool[2080];
        static int[] pixelCachesRelay = new int[2080];
        static int[] pixelCachesConfirms = new int[2080];
        static int[] pixelCachesStability = new int[2080];
        public static bool showUpdatesOnly = false;
        public static bool fillSpanRequired = false;
        public static int totalSkipped = 0;
        public static double totalPercentageSkipped = 0;
        public static void ResetPixelCache()
        {
            pixelCaches = new PixelCache[2080, 2080];
            pixelCachesStates = new bool[2080];
            pixelCachesRelay = new int[2080];
            pixelCachesConfirms = new int[2080];
            pixelCachesStability = new int[2080];
            fillSpanRequired = true;
            totalPercentageSkipped = 0;
        }


        private static bool IsPixelsRowCached(int i)
        {
            if (pixelCachesRelay[i] >= pixelCachesStability[i])
            {
                if (pixelCachesRelay[i] >= (pixelCachesStability[i] + 1) && !pixelCachesStates[i])
                {
                    pixelCachesRelay[i] = 0;
                    pixelCachesConfirms[i] = 0;
                    pixelCachesStability[i] = 0;
                }
                else if (pixelCachesRelay[i] >= (pixelCachesStability[i] + 1))
                {
                    pixelCachesRelay[i] = 0;
                }
                else
                {
                    pixelCachesRelay[i]++;
                    return false;
                }
            }

            if (pixelCachesStates[i] && pixelCachesConfirms[i] < 120)
            {
                pixelCachesConfirms[i]++;
            }
            else if (pixelCachesStates[i])
            {
                pixelCachesRelay[i]++;
            }
            else
            {
                pixelCachesConfirms[i] = 0;
                pixelCachesStability[i] = 0;
            }

            if (pixelCachesConfirms[i] >= 120 && pixelCachesStability[i] == 0)
            {
                pixelCachesStability[i]++;
            }
            else if (pixelCachesConfirms[i] >= 1000 && pixelCachesStability[i] == 1)
            {
                pixelCachesStability[i]++;
            }
            else if (pixelCachesConfirms[i] >= 2000 && pixelCachesStability[i] == 2)
            {
                pixelCachesStability[i]++;
            }
            else if (pixelCachesConfirms[i] >= 3000 && pixelCachesStability[i] == 3)
            {
                pixelCachesStability[i]++;
            }
            else if (pixelCachesStates[i] && pixelCachesConfirms[i] <= 3000)
            {
                pixelCachesConfirms[i]++;
            }

            return pixelCachesConfirms[i] >= 120;
        }
        private static void CachePixel(ushort input, uint output, int i, int j)
        {
            if (!SkipCached)
            {
                return;
            }
            unsafe
            {
                if (pixelCaches[i, j] != null)
                {
                    pixelCaches[i, j].input = input;
                    pixelCaches[i, j].output = output;
                }
                else
                {
                    pixelCaches[i, j] = new PixelCache(input, output);
                }
            }
        }
        private static void CachePixel(uint input, uint output, int i, int j)
        {
            if (!SkipCached)
            {
                return;
            }
            unsafe
            {
                if (pixelCaches[i, j] != null)
                {
                    pixelCaches[i, j].input64 = input;
                    pixelCaches[i, j].output = output;
                }
                else
                {
                    pixelCaches[i, j] = new PixelCache(input, output);
                }
            }
        }
        #endregion

        #region Pixels Conversion
        public static uint getPattern(ushort x)
        {
            if (isRG565)
            {
                return RGB565LookupTable[x];
            }
            else
            {
                return RGB0555LookupTable[x];
            }
        }
        public static uint getPattern2(ref ushort x)
        {
            if (isRG565)
            {
                return RGB565LookupTable[x];
            }
            else
            {
                return RGB0555LookupTable[x];
            }
        }
        #endregion

        #region Span, Other memory helpers
        public static void CopyFromPtrToPtr(IntPtr src, uint srcLen, IntPtr dst, uint dstLen)
        {
            var buffer = new byte[srcLen];
            Marshal.Copy(src, buffer, 0, buffer.Length);
            Marshal.Copy(buffer, 0, dst, (int)Math.Min(buffer.Length, dstLen));
        }
        public unsafe static Span<byte> AsSpan<T>(in T val) where T : unmanaged
        {
            void* valPtr = Unsafe.AsPointer(ref Unsafe.AsRef(val));
            return new Span<byte>(valPtr, Marshal.SizeOf<T>());
        }
        // Alternatively, slightly easier when using 'ref' instead of 'in'
        public unsafe static Span<byte> AsSpanRef<T>(ref T val) where T : unmanaged
        {
            void* valPtr = Unsafe.AsPointer(ref val);
            return new Span<byte>(valPtr, Marshal.SizeOf<T>());
        }
        public static Span<byte> AsSpanArray<T>(Span<T> vals) where T : unmanaged
        {
            return MemoryMarshal.Cast<T, byte>(vals);
        }

        static byte[] ArrayUint2Byte(uint[] input)
        {

            Converter translate = new Converter();
            translate.Uint = input;
            return translate.Bytes;
        }
        static uint[] ArraByte2yUint(byte[] input)
        {

            Converter translate = new Converter();
            translate.Bytes = input;
            return translate.Uint;
        }
        [StructLayout(LayoutKind.Explicit)]
        struct Converter
        {
            [FieldOffset(0)]
            public byte[] Bytes;     //warning bytes are 8 bit ! dont use
            [FieldOffset(0)]
            public ushort[] Ushorts;  //16bit
            [FieldOffset(0)]
            public char[] Chars;      //16bit
            [FieldOffset(0)]
            public uint[] Uint;      //16bit
        }
        #endregion
    }

    #region Pixels Updates Classes
    class PixelCache
    {
        //64 added by me as quick rename, it's NOT related to 64bit
        public uint input64;
        public ushort input;
        public uint output;

        public PixelCache(ushort input, uint output)
        {
            this.input = input;
            this.output = output;
        }
        public PixelCache(uint input, uint output)
        {
            this.input64 = input;
            this.output = output;
        }
    }
    #endregion

    class TypedefUint
    {
        private uint Value = 0;
        public static implicit operator uint(TypedefUint ts)
        {
            return ((uint)(ts?.Value));
        }
        public static implicit operator TypedefUint(uint val)
        {
            return new TypedefUint { Value = val };
        }
    }

}
