﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP.RetroBindings.Structs
{
    internal class MMAP
    {
    }
    /* The frontend may use the largest value of 'start'+'select' in a
 * certain namespace to infer the size of the address space.
 *
 * If the address space is larger than that, a mapping with .ptr=NULL
 * should be at the end of the array, with .select set to all ones for
 * as long as the address space is big.
 *
 * Sample descriptors (minus .ptr, and RETRO_MEMFLAG_ on the flags):
 * SNES WRAM:
 * .start=0x7E0000, .len=0x20000
 * (Note that this must be mapped before the ROM in most cases; some of the
 * ROM mappers
 * try to claim $7E0000, or at least $7E8000.)
 * SNES SPC700 RAM:
 * .addrspace="S", .len=0x10000
 * SNES WRAM mirrors:
 * .flags=MIRROR, .start=0x000000, .select=0xC0E000, .len=0x2000
 * .flags=MIRROR, .start=0x800000, .select=0xC0E000, .len=0x2000
 * SNES WRAM mirrors, alternate equivalent descriptor:
 * .flags=MIRROR, .select=0x40E000, .disconnect=~0x1FFF
 * (Various similar constructions can be created by combining parts of
 * the above two.)
 * SNES LoROM (512KB, mirrored a couple of times):
 * .flags=CONST, .start=0x008000, .select=0x408000, .disconnect=0x8000, .len=512*1024
 * .flags=CONST, .start=0x400000, .select=0x400000, .disconnect=0x8000, .len=512*1024
 * SNES HiROM (4MB):
 * .flags=CONST,                 .start=0x400000, .select=0x400000, .len=4*1024*1024
 * .flags=CONST, .offset=0x8000, .start=0x008000, .select=0x408000, .len=4*1024*1024
 * SNES ExHiROM (8MB):
 * .flags=CONST, .offset=0,                  .start=0xC00000, .select=0xC00000, .len=4*1024*1024
 * .flags=CONST, .offset=4*1024*1024,        .start=0x400000, .select=0xC00000, .len=4*1024*1024
 * .flags=CONST, .offset=0x8000,             .start=0x808000, .select=0xC08000, .len=4*1024*1024
 * .flags=CONST, .offset=4*1024*1024+0x8000, .start=0x008000, .select=0xC08000, .len=4*1024*1024
 * Clarify the size of the address space:
 * .ptr=NULL, .select=0xFFFFFF
 * .len can be implied by .select in many of them, but was included for clarity.
 */
    [StructLayout(LayoutKind.Sequential)]
    struct retro_memory_map
    {
        public IntPtr descriptors;
        public uint num_descriptors;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct rarch_memory_descriptor
    {
        public retro_memory_descriptor core;        /* uint64_t alignment */
        public long disconnect_mask;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct retro_memory_descriptor
    {
        public long flags;

        /* Pointer to the start of the relevant ROM or RAM chip.
         * It's strongly recommended to use 'offset' if possible, rather than
         * doing math on the pointer.
         *
         * If the same byte is mapped my multiple descriptors, their descriptors
         * must have the same pointer.
         * If 'start' does not point to the first byte in the pointer, put the
         * difference in 'offset' instead.
         *
         * May be NULL if there's nothing usable here (e.g. hardware registers and
         * open bus). No flags should be set if the pointer is NULL.
         * It's recommended to minimize the number of descriptors if possible,
         * but not mandatory. */
        public IntPtr ptr;
        public long offset;

        /* This is the location in the emulated address space
         * where the mapping starts. */
        public long start;

        /* Which bits must be same as in 'start' for this mapping to apply.
         * The first memory descriptor to claim a certain byte is the one
         * that applies.
         * A bit which is set in 'start' must also be set in this.
         * Can be zero, in which case each byte is assumed mapped exactly once.
         * In this case, 'len' must be a power of two. */
        public long select;

        /* If this is nonzero, the set bits are assumed not connected to the
         * memory chip's address pins. */
        public long disconnect;

        /* This one tells the size of the current memory area.
         * If, after start+disconnect are applied, the address is higher than
         * this, the highest bit of the address is cleared.
         *
         * If the address is still too high, the next highest bit is cleared.
         * Can be zero, in which case it's assumed to be infinite (as limited
         * by 'select' and 'disconnect'). */
        public long len;

        /* To go from emulated address to physical address, the following
         * order applies:
         * Subtract 'start', pick off 'disconnect', apply 'len', add 'offset'. */

        /* The address space name must consist of only a-zA-Z0-9_-,
         * should be as short as feasible (maximum length is 8 plus the NUL),
         * and may not be any other address space plus one or more 0-9A-F
         * at the end.
         * However, multiple memory descriptors for the same address space is
         * allowed, and the address space name can be empty. NULL is treated
         * as empty.
         *
         * Address space names are case sensitive, but avoid lowercase if possible.
         * The same pointer may exist in multiple address spaces.
         *
         * Examples:
         * blank+blank - valid (multiple things may be mapped in the same namespace)
         * 'Sp'+'Sp' - valid (multiple things may be mapped in the same namespace)
         * 'A'+'B' - valid (neither is a prefix of each other)
         * 'S'+blank - valid ('S' is not in 0-9A-F)
         * 'a'+blank - valid ('a' is not in 0-9A-F)
         * 'a'+'A' - valid (neither is a prefix of each other)
         * 'AR'+blank - valid ('R' is not in 0-9A-F)
         * 'ARB'+blank - valid (the B can't be part of the address either, because
         *                      there is no namespace 'AR')
         * blank+'B' - not valid, because it's ambigous which address space B1234
         *             would refer to.
         * The length can't be used for that purpose; the frontend may want
         * to append arbitrary data to an address, without a separator. */
        public IntPtr addrspace;

        /* TODO: When finalizing this one, add a description field, which should be
         * "WRAM" or something roughly equally long. */

        /* TODO: When finalizing this one, replace 'select' with 'limit', which tells
         * which bits can vary and still refer to the same address (limit = ~select).
         * TODO: limit? range? vary? something else? */

        /* TODO: When finalizing this one, if 'len' is above what 'select' (or
         * 'limit') allows, it's bankswitched. Bankswitched data must have both 'len'
         * and 'select' != 0, and the mappings don't tell how the system switches the
         * banks. */

        /* TODO: When finalizing this one, fix the 'len' bit removal order.
         * For len=0x1800, pointer 0x1C00 should go to 0x1400, not 0x0C00.
         * Algorithm: Take bits highest to lowest, but if it goes above len, clear
         * the most recent addition and continue on the next bit.
         * TODO: Can the above be optimized? Is "remove the lowest bit set in both
         * pointer and 'len'" equivalent? */

        /* TODO: Some emulators (MAME?) emulate big endian systems by only accessing
         * the emulated memory in 32-bit chunks, native endian. But that's nothing
         * compared to Darek Mihocka <http://www.emulators.com/docs/nx07_vm101.htm>
         * (section Emulation 103 - Nearly Free Byte Reversal) - he flips the ENTIRE
         * RAM backwards! I'll want to represent both of those, via some flags.
         *
         * I suspect MAME either didn't think of that idea, or don't want the #ifdef.
         * Not sure which, nor do I really care. */

        /* TODO: Some of those flags are unused and/or don't really make sense. Clean
         * them up. */
    };
}
