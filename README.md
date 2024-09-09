<p align="center">
<img src="https://github.com/JaneySprings/DotNet.Meteor/raw/main/assets/header.jpg" width="1180px" alt=".NET Meteor" />
<a href="https://dev.to/nromanov/boost-net-maui-development-productivity-6-powerful-features-of-net-meteor-for-vs-code-in-windows-mac-linux-d0b">Features</a> | <a href="https://github.com/JaneySprings/DotNet.Meteor/wiki">Documentation</a> | <a href="https://github.com/JaneySprings/DotNet.Meteor/issues">Support</a>

---

## Overview

&emsp;The .NET Meteor allows you to build, debug `.NET` apps and deploy them to devices or emulators.

- **Cross-Platform** </br>
You can use this extension in the `Windows`, `MacOS`, and `Linux` operation systems.

- **XAML IntelliSense** </br>
The extension provides you with a basic `XAML` syntax highlighting and shows snippets for .NET MAUI / third-party controls (it's necessary to build your project first).

- **XAML Hot Reload** </br>
Meteor support XAML Hot Reload for any platform. See the instruction below to enable Hot Reload in your project.

- **Performance and Memory Profiling** </br>
You can profile your application to find performance bottlenecks and undisposed objects that persist in the memory.

- **MAUI Blazor Support** </br>
The extension allows you to build and debug `MAUI Blazor` apps (including the `.razor` files).

- **Multiple Folders in a Workspace** </br>
You can use muliple folders in your workspace and change the current running project.

- **F# support** </br>
Your can build and debug projects, written in the `F#` language.

---

## Run the Application

1. Open the project folder.
2. Open the `Run and Debug` VSCode tab and click the `create a launch.json file`.
3. In the opened panel, select the `.NET Meteor Debugger`.
4. In the status bar, select a project (if your opened folder contains several projects) and a configuration (the debug is the default).
5. In the status bar, click the device name and select a target device/emulator from the opened panel.
6. Press `F5` to debug the application or `ctrl + F5` to launch the application without debugging.
7. Enjoy!

![image](https://github.com/JaneySprings/DotNet.Meteor/raw/main/assets/demo_dbg.gif)

---

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

---

## Profile the Application

1. Open the project folder.
2. Open the `Run and Debug` VSCode tab and click the `create a launch.json file`.
3. In the opened panel, select the `.NET Meteor Debugger`.
4. Specify a profiler mode option (`trace` or `gcdump`) in the generated configuration. For example:
```json
{
	"name": ".NET Meteor Profiler",
	"type": "dotnet-meteor.debugger",
	"request": "launch",
	"profilerMode": "trace",
	"preLaunchTask": "dotnet-meteor: Build"
}
```
5. In the status bar, select a project (if your opened folder contains several projects) and a configuration (the debug is the default). Click the device name and select a target device/emulator from the opened panel.
6. Press `ctrl + F5` to launch the application without debugging.
* If you use the `gcdump` mode, type a `/dump` command in the `Debug Console` to capture the report. You will see the message:
```
Writing gcdump to '/Users/You/.../Project/MauiProf.gcdump'...
command handled by DotNet.Meteor.Debug.GCDumpLaunchAgent
Finished writing 2759872 bytes.
```

* If you use the `trace` mode, click `Stop Debugging` in the VSCode to stop the profiling. **Don't close the application manually, because this may damage the report.** After completion, you will see the message:
```
Trace completed.
Writing:	/Users/You/.../Project/MauiProf.speedscope.json
Conversion complete
```
7. You can see the `speedscope.json` report in the root folder of your project. You can use the [Speedscope in VSCode](https://marketplace.visualstudio.com/items?itemName=sransara.speedscope-in-vscode) extension to view it. Alternatively, you can upload it directly to the [speedscope](https://www.speedscope.app) site. For the `gcdump` report, you can use the [dotnet-heapview](https://github.com/1hub/dotnet-heapview) or _Visual Studio for Windows_.

![image](https://github.com/JaneySprings/DotNet.Meteor/raw/main/assets/demo_trace.gif)

&emsp;*The profiler can capture and analyze functions executed within the Mono runtime. To profile native code, you can leverage platform-specific tools, such as Android Studio and Xcode.*

### Troubleshooting

**.NET Meteor** uses the `.NET Diagnostics` tools to profile applications. The process of profiling consists of two stages:

1) `dsrouter` creates a connection between the application and the profiler.
2) `dotnet-trace` or `dotnet-gcdump` captures the data.

If you encounter any issues, please check the following:

- VSCode **Debug Console** tab should display a message about the successful connection. If you see the `Router stopped` message or something similar, the connection is not established. You can try to change the **profiler port** in the **.NET Meteor** settings.

- When profiling is started, the **Debug Console** tab should display the `Output File:` message. If you don't see this message after running the app and displaying the first view (after the splash screen), try deleting the `bin` and `obj` folders and rerunning the project. *Sometimes the issue occurs when you frequently switch between the profiling and debugging modes.*

---

## Compatibility

&emsp;The following table lists supported .NET target platforms and their capabilities:

| Application Type | Build and Run | Hot Reload | Debugging | Profiling |
|-|:-:|:-:|:-:|:-:|
| **WinUI** | ✅ | ✅ | ❌ | ✅ |
| **Android** | ✅ | ✅ | ✅ | ✅ |
| **iOS** | ✅ | ✅ | ✅ | ✅ |
| **MacCatalyst** | ✅ | ✅ | ✅ | ✅ |

&emsp;*You can debug WinUI apps using the C# VSCode extension with attaching the .NET Core Debugger.*

