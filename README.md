## Toggle Regions Extension for Visual Studio

Quickly collapse, expand, and toggle all region sections in a code editor tab.

Recognized regions:

- `#region`
- `<!--#region` (`#` and whitespace are optional)
- `#pragma region`

Learning resources:

- [C# regions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives#defining-regions)
- [VB regions](https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/directives/region-directive)
- [XAML regions](https://learn.microsoft.com/en-us/visualstudio/xaml-tools/xaml-code-editor?view=vs-2022#xaml-region-support)
- [C/C++ regions](https://learn.microsoft.com/en-us/cpp/preprocessor/region-endregion?view=msvc-170)

### Installation

This extension is available through the Visual Studio Marketplace.

- [Toggle Regions for VS2017](https://marketplace.visualstudio.com/items?itemName=CMPhys.ToggleRegions2017)
- [Toggle Regions for VS2019](https://marketplace.visualstudio.com/items?itemName=CMPhys.ToggleRegions2019)
- [Toggle Regions for VS2022](https://marketplace.visualstudio.com/items?itemName=CMPhys.ToggleRegions2022)

Alternatively, a `.vsix` installation file can be downloaded from [Releases](https://github.com/mdmower/ToggleRegionsExtension/releases) and manually installed.

### Configurable Commands

Three commands are added to the Outlining menu: "Expand all regions", "Collapse all regions", and "Toggle all regions".

These commands are exposed to Visual Studio as:

- `RegionManagement.Expand` - Default shortcut: CTRL+R, CTRL+Num +
- `RegionManagement.Collapse` - Default shortcut: CTRL+R, CTRL+Num -
- `RegionManagement.Toggle` - Default shortcut: CTRL+R, CTRL+Num \*

... and can be remapped in Tools > Options > Environment > Keyboard.

### Compatibility

- Visual Studio 2022 (Version 17.3 or later)
- Visual Studio 2019 (Version 16.10 or later)
- Visual Studio 2017 (Any version)

### Build

Visual Studio 2022 is required to build the solution. Note that `Microsoft.VisualStudio.SDK` should have major version corresponding to the version of Visual Studio where the extension will run (e.g. nuget version 16.x for VS2019) and `Microsoft.VSSDK.BuildTools` should have major version corresponding to the version of Visual Studio that is used to _build_ the extension.

Release configuration builds can optionally be signed using a strong name keypair stored at `Metadata\Key.snk`.

### Additional Information

This extension was forked from [Vlad-Herus/CollapseRegionExtension](https://github.com/Vlad-Herus/CollapseRegionExtension).

MIT License  
Copyright (c) 2022 Matt Mower  
Copyright (c) 2018 VladimirUAZ
