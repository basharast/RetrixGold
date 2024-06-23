using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    internal class KeyboardInput
    {
    }
    public delegate void retro_keyboard_event_t (bool down, uint keycode, uint character, uint key_modifiers);
    
    [StructLayout(LayoutKind.Sequential)]
    struct retro_keyboard_callback
    {
        retro_keyboard_event_t callback;
        public retro_keyboard_event_t Callback
        {
            get
            {
                return callback;
            }
        }
    };
}
