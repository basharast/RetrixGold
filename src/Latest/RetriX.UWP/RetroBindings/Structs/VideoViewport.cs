using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    internal class VideoViewport
    {
    }
    public struct video_viewport
    {
        public int x;
        public int y;
        public uint width;
        public uint height;
        public uint full_width;
        public uint full_height;
    }
}
