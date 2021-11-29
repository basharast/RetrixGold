using MvvmCross.Platform;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Pipelines.Sockets.Unofficial;
using RetriX.Shared.Services;
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
        public static int CoresCount = 1;
        private const uint LookupTableSize = ushort.MaxValue + 1;

        private static uint[] RGB0555LookupTable = new uint[LookupTableSize];
        private static uint[] RGB565LookupTable = new uint[LookupTableSize];
        public static IPlatformService PlatformService { get; set; }

        public static int CurrentColorFilter = 0;
        public static bool isGameStarted = false;
        public static bool AudioOnly = false;
        static FramebufferConverter()
        {
            SetRGB0555LookupTable();
        }
        public static void ClearBuffer()
        {
            RGB0555LookupTable = new uint[LookupTableSize];
            RGB565LookupTable = new uint[LookupTableSize];
        }
        public static void SetRGB0555LookupTable()
        {
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

                    RGB565LookupTable[i] = GetPixel(0xFF000000 | r565 << 16 | g565 << 8 | b565);
                    RGB0555LookupTable[i] = GetPixel(0xFF000000 | r0555 << 16 | g0555 << 8 | b0555);
                }
                LookupTableUint.Clear();
            }
            catch (Exception e)
            {

            }
        }
        public static void ConvertFrameBufferRGB0555ToXRGB8888(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch)
        {
            try
            {
                ConvertFrameBufferUshortToXRGB8888WithLUT(width, height, input, inputPitch, output, outputPitch, false);
            }
            catch (Exception e)
            {

            }
        }

        public static void ConvertFrameBufferRGB565ToXRGB8888(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch)
        {
            try
            {
                ConvertFrameBufferUshortToXRGB8888WithLUT(width, height, input, inputPitch, output, outputPitch, true);
            }
            catch (Exception e)
            {

            }
        }

        public static void ConvertFrameBufferToXRGB8888(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch)
        {
            try
            {
                ConvertFrameBufferXRGB8888(width, height, input, inputPitch, output, outputPitch);
            }
            catch (Exception e)
            {

            }
        }

        private static bool UseMarshalCopy = false;

        public unsafe static void ConvertFrameBufferXRGB8888(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch)
        {
            try
            {
                if (!isGameStarted || AudioOnly)
                {
                    return;
                }
                //Stopwatch ss = new Stopwatch();
                //ss.Start();

                var castInput = MemoryMarshal.Cast<byte, uint>(input);
                var castInputPitch = inputPitch / sizeof(uint);
                var castOutput = MemoryMarshal.Cast<byte, uint>(output);
                var castOutputPitch = outputPitch / sizeof(uint);

                if (UseMarshalCopy)
                {
                    int DataLength = castInput.Length * sizeof(uint);
                    byte[] FinalResult = new byte[DataLength];
                    fixed (uint* currentByte = &castInput[0])
                    {
                        Marshal.Copy((IntPtr)currentByte, FinalResult, 0, DataLength);
                    }
                    AsSpanArray<byte>(FinalResult).CopyTo(output);
                }
                else
                {
                    RenderHelperCallInstant(height, width, castInput, castOutput, castInputPitch, castOutputPitch);
                }
                //ss.Stop();
                //tt.Add(ss.Elapsed);
            }
            catch (Exception e)
            {

            }
        }

        public static bool DontWaitThreads = false;
        public static bool NativeDoublePixel = false;
        public static bool NativeSpeedup = false;
        public static bool NativeScanlines = false;
        public static int NativePixelStep = 0;
        private static int StartIndex = -1;
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
        private unsafe static void RenderHelperCallInstant(uint height, uint width, ReadOnlySpan<uint> castInput, Span<uint> castOutput, int castInputPitch, int castOutputPitch)
        {
            //Stopwatch ss = new Stopwatch();
            //ss.Start();
            if (!isGameStarted || AudioOnly)
            {
                return;
            }
            try
            {
                cancellationTokenSource.Cancel();
            }
            catch (Exception x)
            {

            }
            cancellationTokenSource = new CancellationTokenSource();
            castOutput.Fill(0);
            StartIndex = GetStartIndexValue();
            List<Task> coresTasks = new List<Task>();
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
                                        var hightPerCore = height / CoresCount;
                                        //for (var t = 0; t < CoresCount; t++)
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
                                                //for (var i = TStartIndex; i < hightSub;)
                                                Parallel.For(TStartIndex, (int)hightSub, (i) =>
                                                {
                                                    if (cancellationTokenSource.IsCancellationRequested)
                                                    {
                                                        return;
                                                    }
                                                    var inputLine = umemory.Slice(i * castInputPitch, castInputPitch);
                                                    var outputLine = memory.Slice(i * castOutputPitch, castOutputPitch);
                                                    inputLine.CopyTo(outputLine);
                                                    i += NativePixelStep;

                                                    GetPixel(outputLine.Span, width);
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
                }
            }
            else
            {
                for (var i = StartIndex; i < height;)
                {
                    try
                    {
                        var inputLine = castInput.Slice(i * castInputPitch, castInputPitch);
                        var outputLine = castOutput.Slice(i * castOutputPitch, castOutputPitch);

                        inputLine.CopyTo(outputLine);
                        i += NativePixelStep;

                        GetPixel(outputLine, width);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            //ss.Stop();
            //tt.Add(ss.Elapsed);
        }


        static uint getPixelPointer(ushort x) => getPattern(x);
        private static bool isRG565 = false;
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        static byte[] tempHash = new byte[] { };
        //static ushort[][] tempLine = new ushort[4096][];
        //static uint[][] cacheLine = new uint[4096][];
        private static void ConvertFrameBufferUshortToXRGB8888WithLUT(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch, bool is565)
        {
            if (!isGameStarted || AudioOnly)
            {
                return;
            }
            try
            {
                //GC.TryStartNoGCRegion(1000000, true);
            }
            catch (Exception ex)
            {

            }
            
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
                castOutput.Fill(0);
                StartIndex = GetStartIndexValue();
                isRG565 = is565;
                List<Task> coresTasks = new List<Task>();
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
                                            var hightPerCore = height / CoresCount;
                                            //for (var t = 0; t < CoresCount; t++)
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
                                                    //for (var i = TStartIndex; i < hightSub;)
                                                    Parallel.For(TStartIndex, (int)hightSub, (i) =>
                                                     {
                                                         if (cancellationTokenSource.IsCancellationRequested)
                                                         {
                                                             return;
                                                         }
                                                         var inputLine = umemory.Slice(i * castInputPitch, castInputPitch);
                                                         var outputLine = memory.Slice(i * castOutputPitch, castOutputPitch);

                                                         try
                                                         {
                                                             /*if (tempLine[i].SequenceEqual(inputLine.Span.ToArray()))
                                                             {
                                                                 cacheLine[i].CopyTo(outputLine);
                                                                 return;
                                                             }
                                                             tempLine[i] = inputLine.ToArray();*/
                                                         }
                                                         catch (Exception ex)
                                                         {

                                                         }

                                                         for (var j = 0; j < width;)
                                                        //Parallel.For(0, (int)width, parallelOptions, (j) =>
                                                        {
                                                             if (cancellationTokenSource.IsCancellationRequested)
                                                             {
                                                                 break;
                                                             }
                                                             outputLine.Span[j] = getPattern(inputLine.Span[j]);
                                                             j++;
                                                         }//);
                                                         //cacheLine[i] = outputLine.ToArray();
                                                        i += NativePixelStep;
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
                    }
                }
                else
                {
                    for (var i = StartIndex; i < height;)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        var inputLine = castInput.Slice(i * castInputPitch, castInputPitch);
                        var outputLine = castOutput.Slice(i * castOutputPitch, castOutputPitch);
                        for (var j = 0; j < width;)
                        {
                            if (cancellationTokenSource.IsCancellationRequested)
                            {
                                break;
                            }
                            outputLine[j] = getPattern(inputLine[j]);
                            j++;
                        }
                        i += NativePixelStep;
                    }
                }

                /*unsafe
                {
                    fixed (byte* currentByte = &input[0])
                    {
                        var tempArray = new Span<byte>((void*)getPixelPointer((ushort)currentByte), input.Length);
                        tempArray.CopyTo(output);
                    }
                }*/
            }
            catch (Exception e)
            {

            }
            try
            {
                //GC.EndNoGCRegion();
            }
            catch (Exception ex)
            {

            }
        }

        public static void DataToBitmap(ReadOnlySpan<byte> data, long bitmapPointer, int mapPitch, int size, int width, int height)
        {
           unsafe
            {
                fixed (byte* currentByte = &data[0])
                {
                    var tempArray = new Span<byte>((void*)getPixelPointer((ushort)currentByte), data.Length);
                    var mapData = new Span<byte>(new IntPtr(bitmapPointer).ToPointer(), size);
                    tempArray.CopyTo(mapData);
                }
            }
        }
        private static uint getPattern(ushort x)
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
        private static int RedSpace = 16;
        private static int GreenSpace = 8;
        private static uint RedRange = 0xFF;
        private static uint GreenRange = 0xFF;
        private static uint BlueRange = 0xFF;
        private static uint GetPixel(uint buffer)
        {
            uint BufferPixel = 0;
            switch (CurrentColorFilter)
            {
                case 0:
                    //"None"
                    BufferPixel = buffer;
                    break;
                case 1:
                    //"Grayscale"
                    BufferPixel = GrayscaleFilter(buffer);
                    break;
                case 2:
                    //"Cool"
                    BufferPixel = CoolFilter(buffer);
                    break;
                case 3:
                    //"Warm"
                    BufferPixel = WarmFilter(buffer);
                    break;
                case 4:
                    //"Sepia"
                    BufferPixel = SepiaFilter(buffer);
                    break;
                case 5:
                    //"Retro"
                    BufferPixel = RetroFilter(buffer);
                    break;
                case 6:
                    //"Blue"
                    BufferPixel = BlueFilter(buffer);
                    break;
                case 7:
                    //"Green"
                    BufferPixel = GreenFilter(buffer);
                    break;
                case 8:
                    //"Red"
                    BufferPixel = RedFilter(buffer);
                    break;
            }
            return BufferPixel;
        }
        static List<TimeSpan> tt = new List<TimeSpan>();
        static Dictionary<uint, uint> LookupTableUint = new Dictionary<uint, uint>();
        private static void GetPixel(Span<uint> outputLine, uint width)
        {
            if (CurrentColorFilter > 0)
            {
                //Stopwatch ss = new Stopwatch();
                //ss.Start();

                for (var j = 0; j < width;)
                {
                    outputLine[j] = GetPixel(outputLine[j]);
                    j += 1;
                }

                //ss.Stop();
                //tt.Add(ss.Elapsed);
            }
        }

        private static uint GrayscaleFilter(uint pixel)
        {
            uint red = (pixel >> RedSpace) & RedRange;
            uint green = (pixel >> GreenSpace) & GreenRange;
            uint blue = (pixel & BlueRange);
            uint grayscale = (uint)Math.Round((red + blue + green) / 3.0);
            var buffer = (grayscale << 16) + (grayscale << 8) + grayscale;
            return buffer;
        }
        private static uint CoolFilter(uint pixel)
        {
            uint red = (pixel >> RedSpace) & RedRange;
            uint green = (pixel >> GreenSpace) & GreenRange;
            uint blue = (pixel & BlueRange);

            red = (uint)Math.Round((red * 0.60));
            green = (uint)Math.Round((green * 0.80));
            blue = (uint)Math.Round((blue * 1.0));

            var buffer = (red << 16) + (green << 8) + blue;
            return buffer;
        }
        private static uint WarmFilter(uint pixel)
        {
            uint red = (pixel >> RedSpace) & RedRange;
            uint green = (pixel >> GreenSpace) & GreenRange;
            uint blue = (pixel & BlueRange);

            red = (uint)Math.Round((red * 1.0));
            green = (uint)Math.Round((green * 0.80));
            blue = (uint)Math.Round((blue * 0.60));

            var buffer = (red << 16) + (green << 8) + blue;
            return buffer;
        }

        private static uint SepiaFilter(uint pixel)
        {
            uint red = (pixel >> RedSpace) & RedRange;
            uint green = (pixel >> GreenSpace) & GreenRange;
            uint blue = (pixel & BlueRange);

            double grayscale = (red * 0.11 + green * 0.30 + blue * 0.59);
            red = (uint)Math.Round(grayscale * 1);
            green = (uint)Math.Round(grayscale * 0.85);
            blue = (uint)Math.Round(grayscale * 0.72);

            var buffer = (red << 16) + (green << 8) + blue;
            return buffer;
        }
        private static uint RetroFilter(uint pixel)
        {
            uint red = (pixel >> RedSpace) & RedRange;
            uint green = (pixel >> GreenSpace) & GreenRange;
            uint blue = (pixel & BlueRange);

            uint grayscale = (uint)Math.Round((red + blue + green) / 3.0);

            red = (uint)Math.Round((red * 0.11 + red * 0.30 + red * 0.59) * 1);
            green = (uint)Math.Round((green * 0.11 + green * 0.30 + green * 0.59) * 0.85);
            blue = (uint)Math.Round((blue * 0.11 + blue * 0.30 + blue * 0.59) * 0.72);

            var buffer = (red << 16) + (grayscale << 8) + grayscale;
            return buffer;
        }
        private static uint BlueFilter(uint pixel)
        {
            uint red = (pixel >> RedSpace) & RedRange;
            uint green = (pixel >> GreenSpace) & GreenRange;
            uint blue = (pixel & BlueRange);

            red = (uint)Math.Round((red + (255 - red) * 0.01));
            green = (uint)Math.Round((green + (255 - green) * 0.01));
            blue = (uint)Math.Round((blue + (255 - blue) * 0.1));

            var buffer = (red << 16) + (green << 8) + blue;
            return buffer;
        }

        private static uint GreenFilter(uint pixel)
        {
            uint red = (pixel >> RedSpace) & RedRange;
            uint green = (pixel >> GreenSpace) & GreenRange;
            uint blue = (pixel & BlueRange);

            red = (uint)Math.Round((red + (255 - red) * 0.01));
            green = (uint)Math.Round((green + (255 - green) * 0.1));
            blue = (uint)Math.Round((blue + (255 - blue) * 0.01));

            var buffer = (red << 16) + (green << 8) + blue;
            return buffer;
        }

        private static uint RedFilter(uint pixel)
        {
            uint red = (pixel >> RedSpace) & RedRange;
            uint green = (pixel >> GreenSpace) & GreenRange;
            uint blue = (pixel & BlueRange);

            red = (uint)Math.Round((red + (255 - red) * 0.1));
            green = (uint)Math.Round((green + (255 - green) * 0.01));
            blue = (uint)Math.Round((blue + (255 - blue) * 0.01));

            var buffer = (red << 16) + (green << 8) + blue;
            return buffer;
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



    }
    public class PinnedBuffer : IDisposable
    {
        public GCHandle Handle { get; }
        public byte[] Data { get; private set; }

        public IntPtr Ptr
        {
            get
            {
                return Handle.AddrOfPinnedObject();
            }
        }

        public PinnedBuffer(byte[] bytes)
        {
            Data = bytes;
            Handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Handle.Free();
                Data = null;
            }
        }
    }

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
