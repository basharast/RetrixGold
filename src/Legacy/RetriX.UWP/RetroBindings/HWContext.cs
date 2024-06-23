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
    struct retro_hw_render_callback
    {
        /* Which API to use. Set by libretro core. */
       public retro_hw_context_type context_type;

        /* Called when a context has been created or when it has been reset.
         * An OpenGL context is only valid after context_reset() has been called.
         *
         * When context_reset is called, OpenGL resources in the libretro
         * implementation are guaranteed to be invalid.
         *
         * It is possible that context_reset is called multiple times during an
         * application lifecycle.
         * If context_reset is called without any notification (context_destroy),
         * the OpenGL context was lost and resources should just be recreated
         * without any attempt to "free" old resources.
         */
        public context_resetDelegate context_reset;

        /* Set by frontend.
         * TODO: This is rather obsolete. The frontend should not
         * be providing preallocated framebuffers. */
        public get_current_framebufferDelegate get_current_framebuffer;

        /* Set by frontend.
         * Can return all relevant functions, including glClear on Windows. */
        public get_proc_addressDelegate get_proc_address;

        /* Set if render buffers should have depth component attached.
         * TODO: Obsolete. */
        public bool depth;

        /* Set if stencil buffers should be attached.
         * TODO: Obsolete. */
        public bool stencil;

        /* If depth and stencil are true, a packed 24/8 buffer will be added.
         * Only attaching stencil is invalid and will be ignored. */

        /* Use conventional bottom-left origin convention. If false,
         * standard libretro top-left origin semantics are used.
         * TODO: Move to GL specific interface. */
        public bool bottom_left_origin;

        /* Major version number for core GL context or GLES 3.1+. */
        public uint version_major;

        /* Minor version number for core GL context or GLES 3.1+. */
        public uint version_minor;

        /* If this is true, the frontend will go very far to avoid
         * resetting context in scenarios like toggling fullscreen, etc.
         * TODO: Obsolete? Maybe frontend should just always assume this ...
         */
        public bool cache_context;

        /* The reset callback might still be called in extreme situations
         * such as if the context is lost beyond recovery.
         *
         * For optimal stability, set this to false, and allow context to be
         * reset at any time.
         */

        /* A callback to be called before the context is destroyed in a
         * controlled way by the frontend. */
        public context_destroyDelegate context_destroy;

        /* OpenGL resources can be deinitialized cleanly at this step.
         * context_destroy can be set to NULL, in which resources will
         * just be destroyed without any notification.
         *
         * Even when context_destroy is non-NULL, it is possible that
         * context_reset is called without any destroy notification.
         * This happens if context is lost by external factors (such as
         * notified by GL_ARB_robustness).
         *
         * In this case, the context is assumed to be already dead,
         * and the libretro implementation must not try to free any OpenGL
         * resources in the subsequent context_reset.
         */

        /* Creates a debug context. */
        public bool debug_context;
    };

}
