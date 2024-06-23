using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    internal class RetroMessage
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    struct retro_message
    {
        IntPtr msg;       /* Message to be displayed. */
        uint frames;     /* Duration in frames of message. */

        public string Message
        {
            get
            {
               return Marshal.PtrToStringAnsi(msg);
            }
        }
        public uint Frames
        {
            get
            {
                return frames;
            }
        }
    };
}
