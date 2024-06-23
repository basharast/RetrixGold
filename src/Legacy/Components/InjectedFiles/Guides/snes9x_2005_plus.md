# Nintendo - SNES / Famicom (Snes9x 2005 Plus)

## Background

Port of SNES9x 1.43 for libretro. It was previously called CAT SFC.

The Snes9x 2005 Plus core has been compiled with Blargg's APU. An accurate audio processing unit.

Provides more favorable performance thresholds at the cost of accuracy. **DO NOT use this core unless you have underpowered hardware and the mainline Snes9x core and the bsnes/higan/bsnes-mercury cores aren't fast enough**

This core is part of a group of Snes9x cores that are snapshots from the year their code is based on. (Snes9x 2002, Snes9x 2005, Snes9x 2005 Plus, Snes9x 2010)

### Author/License

The Snes9x 2005 Plus core has been authored by

- Snes9x Team
- dking
- BassAceGold
- ShadauxCat
- Nebuleon

The Snes9x 2005 Plus core is licensed under

- [Non-commercial](https://github.com/libretro/snes9x/blob/master/docs/snes9x-license.txt)

A summary of the licenses behind RetroArch and its cores can be found [here](../development/licenses.md).

## Extensions

Content that can be loaded by the Snes9x 2005 Plus core have the following file extensions:

- .smc
- .fig
- .sfc
- .gd3
- .gd7
- .dx2
- .bsx
- .swc

## Databases

RetroArch database(s) that are associated with the Snes9x 2005 Plus core:

- [Nintendo - Super Nintendo Entertainment System](https://github.com/libretro/libretro-database/blob/master/rdb/Nintendo%20-%20Super%20Nintendo%20Entertainment%20System.rdb)
- [Nintendo - Super Nintendo Entertainment System Hacks](https://github.com/libretro/libretro-database/blob/master/rdb/Nintendo%20-%20Super%20Nintendo%20Entertainment%20System%20Hacks.rdb)
- [Nintendo - Sufami Turbo](https://github.com/libretro/libretro-database/blob/master/rdb/Nintendo%20-%20Sufami%20Turbo.rdb)

## Features

Frontend-level settings or features that the Snes9x 2005 Plus core respects.

| Feature           | Supported |
|-------------------|:---------:|
| Restart           | ✔         |
| Screenshots       | ✔         |
| Saves             | ✔         |
| States            | ✔         |
| Rewind            | ✔         |
| Netplay           | ✔         |
| Core Options      | ✔         |
| RetroAchievements | ✔         |
| RetroArch Cheats  | ✔         |
| Native Cheats     | ✕         |
| Controls          | ✔         |
| Remapping         | ✔         |
| Multi-Mouse       | ✕         |
| Rumble            | ✕         |
| Sensors           | ✕         |
| Camera            | ✕         |
| Location          | ✕         |
| Subsystem         | ✕         |
| [Softpatching](../guides/softpatching.md) | ✔         |
| Disk Control      | ✕         |
| Username          | ✕         |
| Language          | ✕         |
| Crop Overscan     | ✕         |
| LEDs              | ✕         |

### Directories

The Snes9x 2005 Plus core's internal core name is 'Snes9x 2005 Plus'

The Snes9x 2005 Plus core saves/loads to/from these directories.

**Frontend's Save directory**

- 'content-name'.srm (Cartridge battery save)

**Frontend's State directory**

- 'content-name'.state# (State)

### Geometry and timing

- The Snes9x 2005 Plus core's core provided FPS is 59.9010812533 for NTSC games and 50.3015490011 for PAL games.
- The Snes9x 2005 Plus core's core provided sample rate is 32040 Hz
- The Snes9x 2005 Plus core's core provided base width is 256
- The Snes9x 2005 Plus core's core provided base height is 224
- The Snes9x 2005 Plus core's core provided max width is 512
- The Snes9x 2005 Plus core's core provided max height is 512
- The Snes9x 2005 Plus core's core provided aspect ratio is 4/3

## Core options

The Snes9x 2005 Plus core has the following option(s) that can be tweaked from the core options menu. The default setting is bolded.

Settings with (Restart) means that core has to be closed for the new setting to be applied on next launch.

- **Video Mode** [catsfc_VideoMode] (**auto**|NTSC|PAL)

	Awaiting description.

- **Reduce Slowdown (Hack, Unsafe, Restart)** [catsfc_overclock_cycles] (**disabled**|compatible|max)

	Many games for the SNES suffered from slowdown due to the weak main CPU. This option helps allievate that at the cost of possible bugs.

	[Example video here](https://www.youtube.com/watch?v=8xA9fosum4Q)

	compatible: Reduce slowdown but keep as much game compatibility as much as possible.

	max: Reduce slowdown as much as possible but will break more games.

- **Reduce Flickering (Hack, Unsafe)** [catsfc_reduce_sprite_flicker] (**disabled**|enabled)

	Rises sprite limit to reduce flickering in games.

## Controllers

The Snes9x 2005 Plus core supports the following device type(s) in the controls menu, bolded device types are the default for the specified user(s):

### User 1 - 5 device types

- None - Doesn't disable input. There's no reason to switch to this.
- **RetroPad - Joypad - Stay on this.
- RetroPad w/Analog - Joypad - Same as RetroPad. There's no reason to switch to this.

### Controller tables

#### Joypad

![](../image/controller/snes.png)

| User 1 - 5 Remap descriptors | RetroPad Inputs                           |
|------------------------------|-------------------------------------------|
| B                            | ![](../image/retropad/retro_b.png)    |
| Y                            | ![](../image/retropad/retro_y.png)    |
| Select                       | ![](../image/retropad/retro_select.png)     |
| Start                        | ![](../image/retropad/retro_start.png)      |
| D-Pad Up                     | ![](../image/retropad/retro_dpad_up.png)    |
| D-Pad Down                   | ![](../image/retropad/retro_dpad_down.png)  |
| D-Pad Left                   | ![](../image/retropad/retro_dpad_left.png)  |
| D-Pad Right                  | ![](../image/retropad/retro_dpad_right.png) |
| A                            | ![](../image/retropad/retro_a.png)    |
| X                            | ![](../image/retropad/retro_x.png)    |
| L                            | ![](../image/retropad/retro_l1.png)         |
| R                            | ![](../image/retropad/retro_r1.png)         |

## Compatibility

| Game                                             | Issue                                                                                |
|--------------------------------------------------|--------------------------------------------------------------------------------------|
| A.S.P. Air Strike Patrol                         | The shadow below the aircraft is missing. Glitched graphics on the briefing screens. |
| Bass Masters Classic - Pro Edition               | Only shows a black screen.                                                           |
| Funaki Masakatsu Hybrid Wrestler – Tougi Denshou | Corrupted graphics on the Pancrase logo screen.                                      |
| Hayazashi Nidan Morita Shougi 2                  | Matches won’t start.                                                                 |
| Madden NFL 96                                    | Only shows a black screen.                                                           |
| Masters New – Harukanaru Augusta 3               | Black screen after selecting game.                                                   |
| Mecarobot Golf                                   | The ground "wobbles" during gameplay.                                                |
| Mechwarrior 3050                                 | Black screen after the Activision logo.                                              |

## External Links

- [Official Snes9x 2005 Plus Github Repository](https://github.com/ShadauxCat/CATSFC)
- [Libretro Snes9x 2005 Plus Core info file](https://github.com/libretro/libretro-super/blob/master/dist/info/snes9x2005_plus_libretro.info)
- [Libretro Snes9x 2005 Plus Github Repository](https://github.com/libretro/snes9x2005)
- [Report Libretro Snes9x 2005 Plus Core Issues Here](https://github.com/libretro/snes9x2005/issues)

### See also

#### Nintendo - Sufami Turbo

- [Nintendo - SNES / Famicom (Beetle bsnes)](beetle_bsnes.md)
- [Nintendo - SNES / Famicom (bsnes-mercury Accuracy)](bsnes_mercury_accuracy.md)
- [Nintendo - SNES / Famicom (bsnes-mercury Balanced)](bsnes_mercury_balanced.md)
- [Nintendo - SNES / Famicom (bsnes-mercury Performance)](bsnes_mercury_performance.md)
- [Nintendo - SNES / Famicom (bsnes Accuracy)](bsnes_accuracy.md)
- [Nintendo - SNES / Famicom (bsnes Balanced)](bsnes_balanced.md)
- [Nintendo - SNES / Famicom (bsnes C++98 (v085))](bsnes_cplusplus98.md)
- [Nintendo - SNES / Famicom (bsnes Performance)](bsnes_performance.md)
- [Nintendo - SNES / Famicom (Snes9x)](snes9x.md)
- [Nintendo - SNES / Famicom (Snes9x 2002)](snes9x_2002.md)
- [Nintendo - SNES / Famicom (Snes9x 2005)](snes9x_2005.md)
- [Nintendo - SNES / Famicom (Snes9x 2010)](snes9x_2010.md)

#### Nintendo - Super Nintendo Entertainment System (+ Hacks)

- [Nintendo - SNES / Famicom (Beetle bsnes)](beetle_bsnes.md)
- [Nintendo - SNES / Famicom (bsnes-mercury Accuracy)](bsnes_mercury_accuracy.md)
- [Nintendo - SNES / Famicom (bsnes-mercury Balanced)](bsnes_mercury_balanced.md)
- [Nintendo - SNES / Famicom (bsnes-mercury Performance)](bsnes_mercury_performance.md)
- [Nintendo - SNES / Famicom (bsnes Accuracy)](bsnes_accuracy.md)
- [Nintendo - SNES / Famicom (bsnes Balanced)](bsnes_balanced.md)
- [Nintendo - SNES / Famicom (bsnes C++98 (v085))](bsnes_cplusplus98.md)
- [Nintendo - SNES / Famicom (bsnes Performance)](bsnes_performance.md)
- [Nintendo - SNES / Famicom (higan Accuracy)](higan_accuracy.md)
- [Nintendo - SNES / Famicom (nSide Balanced)](nside_balanced.md)
- [Nintendo - SNES / Famicom (Mesen-S)](mesen-s.md)
- [Nintendo - SNES / Famicom (Snes9x)](snes9x.md)
- [Nintendo - SNES / Famicom (Snes9x 2002)](snes9x_2002.md)
- [Nintendo - SNES / Famicom (Snes9x 2005)](snes9x_2005.md)
- [Nintendo - SNES / Famicom (Snes9x 2010)](snes9x_2010.md)
