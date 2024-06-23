using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    public class LEDInterface
    {
        public void retro_set_led_state(int led, int state){
            try
            {
                if (PlatformService.LEDIndicatorHandler !=null)
                {
                    PlatformService.LEDIndicatorHandler.Invoke(led, null);
                }
            }catch(Exception ex)
            {

            }
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void retro_set_led_state_t(int led, int state);

    [StructLayout(LayoutKind.Sequential)]
    struct retro_led_interface
    {
        public retro_set_led_state_t set_led_state;
    };
}
