using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static LibRetriX.RetroBindings.Constants;

namespace LibRetriX.RetroBindings
{
    public class HWHandler
    {
        public retro_hw_context_type context_type;
        public bool depth;
        public bool stencil;
        public uint version_major;
        public uint version_minor;
        public bool cache_context;
        public bool debug_context;

        public void Reset()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }
        public void Destroy()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }
        public IntPtr CurrentFrameBuffer()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
            return IntPtr.Zero;
        }
        public IntPtr ProcessAddress(IntPtr sym)
        {
            try
            {
                var symbol = Marshal.PtrToStringAnsi(sym);
                if (symbol != null)
                {
                    LibretroCore.AddVFSLog($"Core request symbol pointer: {symbol}");
                }
            }
            catch (Exception ex)
            {

            }
            return IntPtr.Zero;
        }
    }
}
