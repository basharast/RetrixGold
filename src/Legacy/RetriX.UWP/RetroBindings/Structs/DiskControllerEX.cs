using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    public class DiskControllerEX
    {
        public retro_set_eject_state_t retro_set_eject_state { get; set; }

        public retro_get_eject_state_t retro_get_eject_state { get; set; }

        public retro_get_image_index_t retro_get_image_index { get; set; }
        public retro_set_image_index_t retro_set_image_index { get; set; }
        public retro_get_num_images_t retro_get_num_images { get; set; }
        public retro_replace_image_index_t retro_replace_image_index { get; set; }
        public retro_add_image_index_t retro_add_image_index { get; set; }
        public retro_set_initial_image_t retro_set_initial_image { get; set; }
        public retro_get_image_path_t retro_get_image_path { get; set; }
        public retro_get_image_label_t retro_get_image_label { get; set; }
    }

    /* Callbacks for RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE &
 * RETRO_ENVIRONMENT_SET_DISK_CONTROL_EXT_INTERFACE.
 * Should be set for implementations which can swap out multiple disk
 * images in runtime.
 *
 * If the implementation can do this automatically, it should strive to do so.
 * However, there are cases where the user must manually do so.
 *
 * Overview: To swap a disk image, eject the disk image with
 * set_eject_state(true).
 * Set the disk index with set_image_index(index). Insert the disk again
 * with set_eject_state(false).
 */

    /* If ejected is true, "ejects" the virtual disk tray.
     * When ejected, the disk image index can be set.
     */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_set_eject_state_t(bool ejected);

    /* Gets current eject state. The initial state is 'not ejected'. */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_get_eject_state_t();

    /* Gets current disk index. First disk is index 0.
 * If return value is >= get_num_images(), no disk is currently inserted.
 */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint retro_get_image_index_t();

    /* Sets image index. Can only be called when disk is ejected.
 * The implementation supports setting "no disk" by using an
 * index >= get_num_images().
 */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_set_image_index_t(uint index);

    /* Gets total number of images which are available to use. */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint retro_get_num_images_t();

    /* Replaces the disk image associated with index.
 * Arguments to pass in info have same requirements as retro_load_game().
 * Virtual disk tray must be ejected when calling this.
 *
 * Replacing a disk image with info = NULL will remove the disk image
 * from the internal list.
 * As a result, calls to get_image_index() can change.
 *
 * E.g. replace_image_index(1, NULL), and previous get_image_index()
 * returned 4 before.
 * Index 1 will be removed, and the new index is 3.
 */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_replace_image_index_t(uint index, IntPtr info);

    /* Adds a new valid index (get_num_images()) to the internal disk list.
 * This will increment subsequent return values from get_num_images() by 1.
 * This image index cannot be used until a disk image has been set
 * with replace_image_index. */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_add_image_index_t();

    /* Sets initial image to insert in drive when calling
 * core_load_game().
 * Since we cannot pass the initial index when loading
 * content (this would require a major API change), this
 * is set by the frontend *before* calling the core's
 * retro_load_game()/retro_load_game_special() implementation.
 * A core should therefore cache the index/path values and handle
 * them inside retro_load_game()/retro_load_game_special().
 * - If 'index' is invalid (index >= get_num_images()), the
 *   core should ignore the set value and instead use 0
 * - 'path' is used purely for error checking - i.e. when
 *   content is loaded, the core should verify that the
 *   disk specified by 'index' has the specified file path.
 *   This is to guard against auto selecting the wrong image
 *   if (for example) the user should modify an existing M3U
 *   playlist. We have to let the core handle this because
 *   set_initial_image() must be called before loading content,
 *   i.e. the frontend cannot access image paths in advance
 *   and thus cannot perform the error check itself.
 *   If set path and content path do not match, the core should
 *   ignore the set 'index' value and instead use 0
 * Returns 'false' if index or 'path' are invalid, or core
 * does not support this functionality
 */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_set_initial_image_t(uint index, IntPtr path);

    /* Fetches the path of the specified disk image file.
 * Returns 'false' if index is invalid (index >= get_num_images())
 * or path is otherwise unavailable.
 */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_get_image_path_t(uint index, IntPtr path, long len);

    /* Fetches a core-provided 'label' for the specified disk
 * image file. In the simplest case this may be a file name
 * (without extension), but for cores with more complex
 * content requirements information may be provided to
 * facilitate user disk swapping - for example, a core
 * running floppy-disk-based content may uniquely label
 * save disks, data disks, level disks, etc. with names
 * corresponding to in-game disk change prompts (so the
 * frontend can provide better user guidance than a 'dumb'
 * disk index value).
 * Returns 'false' if index is invalid (index >= get_num_images())
 * or label is otherwise unavailable.
 */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool retro_get_image_label_t(uint index, IntPtr path, long len);


    [StructLayout(LayoutKind.Sequential)]
    struct retro_game_info
    {
        IntPtr path;       /* Path to game, UTF-8 encoded.
                            * Sometimes used as a reference for building other paths.
                            * May be NULL if game was loaded from stdin or similar,
                            * but in this case some cores will be unable to load `data`.
                            * So, it is preferable to fabricate something here instead
                            * of passing NULL, which will help more cores to succeed.
                            * retro_system_info::need_fullpath requires
                            * that this path is valid. */
        IntPtr data;       /* Memory buffer of loaded game. Will be NULL
                            * if need_fullpath was set. */
        long size;         /* Size of memory buffer. */
        IntPtr meta;       /* String of implementation specific meta-data. */
        public string Path
        {
            get
            {
                return Marshal.PtrToStringAnsi(path);
            }
        }
        public string Data
        {
            get
            {
                return Marshal.PtrToStringAnsi(data);
            }
        }
        public long Size
        {
            get
            {
                return size;
            }
        }
        public string Meta
        {
            get
            {
                return Marshal.PtrToStringAnsi(meta);
            }
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    struct retro_disk_control_ext_callback
    {
        public retro_set_eject_state_t set_eject_state;
        public retro_get_eject_state_t get_eject_state;

        public retro_get_image_index_t get_image_index;
        public retro_set_image_index_t set_image_index;
        public retro_get_num_images_t get_num_images;

        public retro_replace_image_index_t replace_image_index;
        public retro_add_image_index_t add_image_index;

        /* NOTE: Frontend will only attempt to record/restore
         * last used disk index if both set_initial_image()
         * and get_image_path() are implemented */
        public retro_set_initial_image_t set_initial_image; /* Optional - may be NULL */

        public retro_get_image_path_t get_image_path;       /* Optional - may be NULL */
        public retro_get_image_label_t get_image_label;     /* Optional - may be NULL */
    };
}
