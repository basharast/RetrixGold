using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    public class DiskController
    {
        public retro_set_eject_state_t retro_set_eject_state { get; set; }

        public retro_get_eject_state_t retro_get_eject_state { get; set; }

        public retro_get_image_index_t retro_get_image_index { get; set; }
        public retro_set_image_index_t retro_set_image_index { get; set; }
        public retro_get_num_images_t retro_get_num_images { get; set; }
        public retro_replace_image_index_t retro_replace_image_index { get; set; }
        public retro_add_image_index_t retro_add_image_index { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct retro_disk_control_callback
    {
        public retro_set_eject_state_t set_eject_state;
        public retro_get_eject_state_t get_eject_state;

        public retro_get_image_index_t get_image_index;
        public retro_set_image_index_t set_image_index;
        public retro_get_num_images_t get_num_images;

        public retro_replace_image_index_t replace_image_index;
        public retro_add_image_index_t add_image_index;
    };
}
