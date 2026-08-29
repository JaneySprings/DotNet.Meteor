## Overview

&emsp;The .NET Meteor extension allows you to build, debug and deploy **.NET MAUI apps** to devices or emulators. The extension provides you with a `XAML intellisense` and `XAML Hot Reload` for any platform. See the instruction below to enable Hot Reload in your project.

## Limitations

- **.NET 10+ (CoreCLR) only** </br>
Starting with version 10, the extension targets **.NET MAUI on .NET 10+** running on the CoreCLR runtime. If your project still uses the Mono runtime, install **.NET Meteor 6.x**, the last release line with Mono support.

- **Requires the `VsdbgRemoteCoreclr` native libraries** </br>
Debugging CoreCLR apps relies on the `VsdbgRemoteCoreclrHost` and `VsdbgRemoteCoreclrTarget` native libraries. These libraries are **closed source** and are **not** distributed with this extension: they ship with the official [.NET MAUI extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-maui) from Microsoft, and no standalone download is known. Install that extension, then point the following settings to the folders bundled with it:
  - `dotnetMeteor.debuggerOptions.remoteCoreclrHost`
  - `dotnetMeteor.debuggerOptions.remoteCoreclrTarget`


- **Requires the `DotRush` extension** </br>
This version requires the [DotRush](https://github.com/JaneySprings/DotRush) extension — a Roslyn-based language server that provides better integration with the project system, IntelliSense, and other C# editor features. It is listed as an extension dependency, so VSCode will install it automatically alongside .NET Meteor.


## Run the Application

1. Open the project folder.
2. Open the `Run and Debug` VSCode tab and click the `create a launch.json file`.
3. In the opened panel, select the `.NET Meteor Debugger`.
4. In the status bar, select a project (if your opened folder contains several projects) and a configuration (the debug is the default).
5. In the status bar, click the device name and select a target device/emulator from the opened panel.
6. Press `F5` to debug the application or `ctrl + F5` to launch the application without debugging.
7. Enjoy!

![image](https://github.com/JaneySprings/DotNet.Meteor/raw/main/assets/demo_dbg.gif)


## Enable XAML Hot Reload

1. Open the `.csproj` file of your project and add the following package reference:

```xml
<ItemGroup>
	<PackageReference Include="DotNetMeteor.HotReload.Plugin" Version="3.*"/>
</ItemGroup>
```

2. Enable Hot Reload Server in your `MauiProgram.cs`:
```cs
using DotNet.Meteor.HotReload.Plugin;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
#if DEBUG
            .EnableHotReload()
#endif
        ...
        return builder.Build();
    }
}
```
3. Now you can run your project, update XAML and see updates in real-time!

![image](https://github.com/JaneySprings/DotNet.Meteor/raw/main/assets/demo_hr.gif)
