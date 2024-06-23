using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    public class MidiInterface
    {
        public bool retro_midi_input_enabled()
        {

            return false;
        }
        public bool retro_midi_output_enabled()
        {

            return false;
        }
        public bool retro_midi_read(IntPtr bytes)
        {

            return false;
        }
        public bool retro_midi_write(IntPtr bytes, uint delta_time)
        {

            return false;
        }
        public bool retro_midi_flush()
        {

            return false;
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_midi_input_enabled_t();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_midi_output_enabled_t();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_midi_read_t(IntPtr bytes);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_midi_write_t(IntPtr bytes, uint delta_time);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_midi_flush_t();

    [StructLayout(LayoutKind.Sequential)]
    struct retro_midi_interface
    {
        public retro_midi_input_enabled_t input_enabled;
        public retro_midi_output_enabled_t output_enabled;
        public retro_midi_read_t read;
        public retro_midi_write_t write;
        public retro_midi_flush_t flush;
    };
}
