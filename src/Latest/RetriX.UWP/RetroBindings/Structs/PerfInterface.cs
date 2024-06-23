using LibRetriX.RetroBindings;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    public class PerfInterface : IDisposable
    {
        public static bool runloop_perfcnt_enable = false;
        public PerfInterface()
        {
            //stopWatch = Stopwatch.StartNew();
        }
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct LARGE_INTEGER
        {
            [FieldOffset(0)]
            public uint LowPart;
            [FieldOffset(4)]
            public int HighPart;

            [FieldOffset(0)]
            public long QuadPart;
        }
        Stopwatch stopWatch = Stopwatch.StartNew();
        public List<retro_perf_counter> registers = new List<retro_perf_counter>();
        static LARGE_INTEGER freq;
        public long cpu_features_get_time_usec()
        {
            try
            {
                //QueryPerformanceFrequency 
                long frequency = Stopwatch.Frequency;

                //QueryPerformanceCounter
                long ticks = Stopwatch.GetTimestamp();
                var result = (ticks / frequency * 1000000) + (ticks % frequency * 1000000 / frequency);
               
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public long cpu_features_get()
        {
            long cpu = 0;
            if (PlatformService.isXBOX)
            {
                cpu |= Constants.RETRO_SIMD_MMX;
                cpu |= Constants.RETRO_SIMD_SSE;
                cpu |= Constants.RETRO_SIMD_MMXEXT;
            }
            return cpu;
        }

        public long cpu_features_get_perf_counter()
        {
            long ticks = Stopwatch.GetTimestamp();
            return ticks;
        }

        public void runloop_performance_counter_register(IntPtr counter)
        {
            if (counter != IntPtr.Zero)
            {
                var counterData = Marshal.PtrToStructure<retro_perf_counter>(counter);
                if (counterData.registered)
                {
                    return;
                }
                registers.Add(counterData);
                counterData.registered = true;
                Marshal.StructureToPtr(counterData, counter, false);
            }
        }

        public void core_performance_counter_start(IntPtr counter)
        {
            if (runloop_perfcnt_enable)
            {
                if (counter != IntPtr.Zero)
                {
                    var counterData = Marshal.PtrToStructure<retro_perf_counter>(counter);
                    counterData.call_cnt++;
                    counterData.start = cpu_features_get_perf_counter();
                    Marshal.StructureToPtr(counterData, counter, false);
                }
            }
        }

        public void core_performance_counter_stop(IntPtr counter)
        {
            if (runloop_perfcnt_enable)
            {
                if (counter != IntPtr.Zero)
                {
                    var counterData = Marshal.PtrToStructure<retro_perf_counter>(counter);
                    counterData.total += cpu_features_get_perf_counter() - counterData.start;
                    Marshal.StructureToPtr(counterData, counter, false);
                    try
                    {
                        var ident = Marshal.PtrToStringAnsi(counterData.ident);
                        var rIndex = 0;
                        foreach (var rItem in registers)
                        {
                            var rItemIdent = Marshal.PtrToStringAnsi(rItem.ident);
                            if (ident.Equals(rItemIdent))
                            {
                                break;
                            }
                            rIndex++;
                        }
                        registers.RemoveAt(rIndex);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }

        public void runloop_perf_log()
        {

        }

        public void Dispose()
        {
            //stopWatch.Stop();
            //stopWatch = null;
        }
    }

    /* Returns current time in microseconds.
    * Tries to use the most accurate timer available.
    */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long retro_perf_get_time_usec_t();

    /* Returns a bit-mask of detected CPU features (RETRO_SIMD_*). */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long retro_get_cpu_features_t();

    /* A simple counter. Usually nanoseconds, but can also be CPU cycles.
     * Can be used directly if desired (when creating a more sophisticated
     * performance counter system).
     * */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long retro_perf_get_counter_t();

    [StructLayout(LayoutKind.Sequential)]
    public struct retro_perf_counter
    {
        public IntPtr ident;
        public long start;
        public long total;
        public long call_cnt;

        public bool registered;
    };

    /* Register a performance counter.
     * ident field must be set with a discrete value and other values in
     * retro_perf_counter must be 0.
     * Registering can be called multiple times. To avoid calling to
     * frontend redundantly, you can check registered field first. */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void retro_perf_register_t(IntPtr counter);

    /* Starts a registered counter. */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void retro_perf_start_t(IntPtr counter);

    /* Stops a registered counter. */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void retro_perf_stop_t(IntPtr counter);

    /* Asks frontend to log and/or display the state of performance counters.
     * Performance counters can always be poked into manually as well.
     */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void retro_perf_log_t();

    [StructLayout(LayoutKind.Sequential)]
    struct retro_perf_callback
    {
        public retro_perf_get_time_usec_t get_time_usec;
        public retro_get_cpu_features_t get_cpu_features;

        public retro_perf_get_counter_t get_perf_counter;
        public retro_perf_register_t perf_register;
        public retro_perf_start_t perf_start;
        public retro_perf_stop_t perf_stop;
        public retro_perf_log_t perf_log;
    }
}
