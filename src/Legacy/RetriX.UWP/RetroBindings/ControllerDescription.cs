using System;
using System.Runtime.InteropServices;

namespace LibRetriX.RetroBindings
{
    public struct ControllerDescription
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeForm
        {
            private IntPtr descriptionStringPtr;
            /// <summary>
            /// Human-readable description of the controller.
            /// Even if using a generic input device type, this can be set to the particular device type the core uses.
            /// </summary>
            public IntPtr DescriptionStringPtr => descriptionStringPtr;

            private uint id;
            /// <summary>
            /// Device type passed to retro_set_controller_port_device().
            /// If the device type is a sub-class of a generic input device type,
            /// use the RETRO_DEVICE_SUBCLASS macro to create an ID.
            /// E.g. RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_JOYPAD, 1).
            /// </summary>
            public uint Id => id;
        };

        public string Description { get; set; }
        public uint Id { get; set; }

        public ControllerDescription(string description, uint id)
        {
            Description = description;
            Id = id;
        }

        public ControllerDescription(NativeForm nativeForm) : this(Marshal.PtrToStringAnsi(nativeForm.DescriptionStringPtr), nativeForm.Id)
        {

        }
    }
}
