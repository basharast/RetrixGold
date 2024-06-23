using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    internal class LibretroCoreOptions
    {
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct retro_core_option_value
    {
        /* Expected option value */
        public IntPtr value;

        /* Human-readable value label. If NULL, value itself
         * will be displayed by the frontend */
        IntPtr label;
        public string Value
        {
            get
            {
                return Marshal.PtrToStringAnsi(value);
            }
        }
        public string Label
        {
            get
            {
                return Marshal.PtrToStringAnsi(label);
            }
        }
    };
    [StructLayout(LayoutKind.Sequential)]
    struct retro_core_option_definition
    {
        /* Variable to query in RETRO_ENVIRONMENT_GET_VARIABLE. */
        public IntPtr key;

        /* Human-readable core option description (used as menu label) */
        IntPtr desc;

        /* Human-readable core option information (used as menu sublabel) */
        IntPtr info;

        /* Array of retro_core_option_value structs, terminated by NULL */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public retro_core_option_value[] values;

     /* Default core option value. Must match one of the values
      * in the retro_core_option_value array, otherwise will be
      * ignored */
     IntPtr default_value;
        public string Key { 
            get { 
                return Marshal.PtrToStringAnsi(key); 
            } 
        }
        public string Desc
        { 
            get { 
                return Marshal.PtrToStringAnsi(desc); 
            } 
        }
        public string Info
        { 
            get { 
                return Marshal.PtrToStringAnsi(info); 
            } 
        }
        public string DefaultValue
        { 
            get { 
                return Marshal.PtrToStringAnsi(default_value); 
            } 
        }
        public retro_core_option_value[] ValuesArray
        {
            get
            {
                return null;
            }
        }
    };
}
