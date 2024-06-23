# RetriXGold III

There are few stuff has no logic, those were tests for older devices like changing the dll to dat

so don't spend time figuring out what is this,

skip whatever is not clear, in anyway modification should be easy as all things now are in one project

you should be able to build it for ARM, x86, x64 (ARM64 prepared but not actually fully ready)

## Building

You need the following:

- SDK: 10.0.19041.0 and 10.0.17134.0
- Visual Studio 2022

## Built-in cores

- Cores are located at `Components\InjectedFiles\Cores`
- Those cores will automaticlly copied with the package

## Legacy support

- Refer to the Legacy folder
- Ensure Win2D is compatible with older builds (1.21 current)
- Keep project min target at 16299
- After building the package, re-target it to 15035