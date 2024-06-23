# RetriXGold III

There are few stuff has no logic, those were tests for older devices like changing the dll to dat

so don't spend time figuring out what is this,

skip whatever is not clear, in anyway modification should be easy as all things now are in one project

you should be able to build it for ARM, x86, x64 (ARM64 prepared but not actually fully ready)

## Building

You need the following:

- SDK: 10.0.19041.0 and 10.0.17134.0
- Visual Studio 2022


## Legacy support

- Refer to the Legacy folder
- Downgrade Win2D to compatible version
- Keep project target at 16299
- After building the package, re-target it to 15035