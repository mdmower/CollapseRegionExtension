## Collapse Region Extension for Visual Studio

Two new commands are exposed:

- `RegionManagement.Expand` - Default shortcut: CTRL+R, CTRL+Num +
- `RegionManagement.Collapse` - Default shortcut: CTRL+R, CTRL+Num -

These shortcuts can be remapped in Tools > Options > Environment > Keyboard.

Recognized regions:

- `#region` - .cs files
- `<!--region` - .xaml files
- `#pragma region` - .cpp files

### Compatibility

- Visual Studio 2022 (Version 17.3 or later)
- Visual Studio 2019 (Version 16.10 or later)
- Visual Studio 2017 (Any version)

### Build

Visual Studio 2022 is required to build the solution. Note that `Microsoft.VisualStudio.SDK` should have major version corresponding to the version of Visual Studio where the extension will run (e.g. nuget version 16.x for VS2019) and `Microsoft.VSSDK.BuildTools` should have major version corresponding to the version of Visual Studio that is used to _build_ the extension.

Release configuration builds can optionally be signed using a strong name keypair stored at `Metadata\Key.snk`.

### Additional Information

This extension was forked from [Vlad-Herus/CollapseRegionExtension](https://github.com/Vlad-Herus/CollapseRegionExtension).

Copyright (c) 2022 Matt Mower  
Copyright (c) 2018 VladimirUAZ
