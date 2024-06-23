using SharpDX.Direct3D;
using System;
using System.Runtime.InteropServices;
using static LibRetriX.RetroBindings.Constants;

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
    [StructLayout(LayoutKind.Sequential)]
    public struct HWInterfaceD3D
    {
        public retro_hw_render_interface_type interface_type;

        public uint interface_version;

        public IntPtr handle;
        public IntPtr device;
        public IntPtr context;
        public FeatureLevel featureLevel;
        public IntPtr D3DCompile;
    };
}
