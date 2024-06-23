using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    internal class InputDesc
    {
    }
    /* Describes how the libretro implementation maps a libretro input bind
    * to its internal input system through a human readable string.
    * This string can be used to better let a user configure input. */
    [StructLayout(LayoutKind.Sequential)]
    public struct retro_input_descriptor
    {
        /* Associates given parameters with a description. */
        uint port;
        uint device;
        uint index;
        uint id;

        /* Human readable description for parameters.
         * The pointer must remain valid until
         * retro_unload_game() is called. */
        public IntPtr description;

        public uint Port
        {
            get { return port; }
        }
        public uint Device
        {
            get { return device; }
        }
        public uint Index
        {
            get { return index; }
        }
        public uint Id
        {
            get { return id; }
        }
        public string Description
        {
            get
            {
                return Marshal.PtrToStringAnsi(description);
            }
        }
    };
}
