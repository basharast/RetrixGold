[![Build status](https://ci.appveyor.com/api/projects/status/bgy3sv7n8mmndlcj?svg=true)](https://ci.appveyor.com/project/Aftnet/libretrix)
[![Nuget](https://img.shields.io/nuget/v/LibRetrix.svg)](https://www.nuget.org/packages?q=LibRetriX)

# LibRetriX

[RetriX](https://www.retrix.me)'s emulation core interface

## TL;DR

- [RetriX](https://www.retrix.me) will switch to using LibRetriX to interface with emulator cores
- Want a new Libretro core in LibRetriX? Use the RetroBindings to expose it as a LibRetriX core and it will work
- Want to write an emulator in a .net language? You can use RetriX as your UI and get a lot of functionality for free by following the LibRetriX API contract
- Want to write a front end in .net? You have several cores uaing a standard interface available as [Nuget packages](https://www.nuget.org/packages?q=LibRetriX)

## Overview

LibRetriX is a standardized interface for game and computer systems emulators development.
It allows decoupling of emulation code from the shell that provides UI, input etc. in the host operating system.
It's basically a rewrite of Libretro in .net.

LibRetriX serves four main purposes:

1. Allows developing front ends that work with any LibRetriX compliant core
2. Allows developing emulator cores without having to develop one's GUI, abstracting the platform the emulator runs on. Much like developing games in Unity or a similar game engine
3. Provides bindings for Libretro cores so that they can be exposed as LibRetriX cores and used by .net languages.
4. Serve as a standardized, separate interface between RetriX and its cores. Decouple RetriX from the emulator cores it uses and allow it to potentially target all Xamarin platforms.

This repository also contains selected Libretro cores bound as LibRetriX cores, which are packaged and distributed via Nuget. These currently support:

- Windows Desktop (x86/x64)
- UWP (x86/x64/ARM)
- macOS (x86/x64)

Android is currently not working, while iOS support is not planned ATM (no reasonable way of sideloading there).

## Available Libretro cores

Libretro core used in parentheses

- BeetlePCEFast (PC Engine)
- BeetlePCFX (PC-FX)
- BeetleNGP (Neo Geo Pocket)
- BeetlePSX (PlayStation)
- BeetleWSwan (Wonderswan)
- FCEUMM (NES)
- Final Burn Alpha (Arcades, Neo Geo)
- Ganbatte (Game Boy)
- GenesisPlusGX (SG1000, Master System, Game Gear, Mega Drive, Mega CD)
- MelonDS (Nintendo DS)
- Nestopia (NES)
- ParallelN64 (Nintendo 64)
- Snes9x (SNES)
- Visual Boy Advance (Game Boy Advance)