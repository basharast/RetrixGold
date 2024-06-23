using System;
using System.Runtime.InteropServices;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace LibRetriX.RetroBindings
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void context_resetDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr get_current_framebufferDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr get_proc_addressDelegate(IntPtr sym);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void context_destroyDelegate();
}
