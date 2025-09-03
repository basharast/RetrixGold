> ⚠️⚠️⚠️

> I'm moving toward [ImMobile](https://github.com/basharast/ImMobile) environment and [ImLibretro](https://github.com/basharast/ImLibretro).<br/>

---

<p align="center">
  <img src="assets/logo.png" width="176"><br>
  <b>Universal Libretro Frontend</b><br>
  <a href="./src">Source</a> |
  <a href="https://github.com/Aftnet/RetriX">Original Project</a> 
  <br><br>
  <img src="assets/screen.jpg"><br><br>
</p>

<p style="padding: 0px;margin-left: 6px;">Get it from itch.io</p>
<a href="https://basharast.itch.io/retrixgold">
<img src="assets/itchio.png" style="width: 150px;margin-left: 5px;border-radius: 10px;border: 1px white solid;">
</a>


App notice:

- This app provided for free without any warranty

- You cannot sell this app or any of it's components, it should be free

- Not to be part of any commercial action

- No tracking or analytics should be added at all


# ARM64
- RetrixGold is a libretro frontend
- As long there are no ARM64 cores ready, any ARM64 release will be pointless
- For ARM64 please refer to libretro for any update [Click here](https://retroarch.com/?page=platforms)
- Try the x86 release it may work

# Roadmap
~~When I started this modified version, I had early understanding in UWP~~

~~many things, in my view can be better, for certain things understanding C/C++ is important to solve some important issues~~

~~good now I have both side covered, UWP and C/C++ even I got to understand more about DirectX stuff~~

~~I know the latest release was in Sep.2022, but will try to pick this again soon~~

~~some things has to be translated to C++, and we can get benfits from the UWP APIs (...FromApp)~~

~~those APIs can greatly improve loading perfomance.~~

# Download

- <a href="https://github.com/basharast/RetrixGold/releases/latest">RetriXGold Latest (GitHub) +Cores</a>

- <a href="http://retrix.astifan.online/cores.html">Supported cores</a>


# What's new in RetriXGold 3.0

- New Layouts

- New Libretro VFS layer

- New Cores

- Online cores updater

- Easy BIOS management

- Games lists cache

- Smart roms resolver (with 7z, rar, zip support)

- Support start core without content

- Play statisitics (Internal usage only)

- Major base code changes

- Advanced customization for Gamepad and options

- Onscreen Keyboard

- Custom Saves/System folder (if supported)

- Pre-Configuration for cores with (free to use content)

- Improved & accurate touch functions

- Controllers ports mapping

- URI integration (RetroPass)

- Bugs fixes

- Much more


## Libretro VFS layer (new)?

This means you can use any Libretro core with Retrix directly

the new VFS layer is much smarter and provides compatiblity with archived roms (even if the core doesn't support that)

it can also solve many issues that prevent UWP app from accessing to the content


# Limitations

- [x] HW Render Configuration
- [ ] OpenGL
- [ ] OpenGLES
- [ ] DirectX


### Easy BIOS Import
<img src="assets/EasyBIOS.gif"><br><br>


### Effects System
<img src="assets/Effects.gif"><br><br>


# Target

## Desktop & XBOX

It will work with build 17763 and above

## WindowsRT (ARM)

It will support only 15035 build

## Windows Phone (W10M)

The attached releases already retargeted to support Windows 10 Mobile 15063+

# Credits

RetrixGold Developement by Bashar Astifan (Since 2019)

Based on RetriX by <a href="https://github.com/albertofustinoni">Alberto Fustinoni</a> 


# Support

Yo can help me to keep this kind of projects a live by supporting my projects like:

- <a href="https://github.com/basharast/wut">W.U.T</a>

# Thanks

The for the following for their support and ideas:

- Khanlend Theinoend
- Dom's & Gamr13
- Danprice142 
- Ranomez
- DekuDesu
