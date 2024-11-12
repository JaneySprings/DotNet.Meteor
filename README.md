<img src="https://github.com/JaneySprings/DotNet.Meteor/raw/main/assets/header.jpg" width="1180px" alt=".NET Meteor" align="center" />

## Overview

&emsp;The .NET Meteor extension allows you to build, debug and deploy **.NET apps** to devices or emulators.

- **Cross-Platform** </br>
You can use this extension in the `Windows`, `MacOS`, and `Linux` operation systems.

- **No additional dependencies** </br>
The extension doesn't require any additional extensions to work. You can use it out of the box.

- **Performance and Memory Profiling** </br>
You can profile your application to find performance bottlenecks and undisposed objects that persist in the memory. See the instruction below to enable profiling in your project.

- **Debug iOS Devices on Windows with Pair to Mac** </br>
Build your project on a remote Mac and deploy it to an iOS device connected to your Windows machine using the Pair to Mac feature. Check these [instructions](https://github.com/JaneySprings/DotNet.Meteor/wiki/Build-and-debug-iOS-application-on-Windows-with-Pair-to-Mac) for more details. Please note that only physical devices (not simulators) are currently supported.

- **Enhanced MAUI support** </br>
The extension provides you with a `XAML intellisense` and `XAML Hot Reload` for any platform. See the instruction below to enable Hot Reload in your project.

- **Multi-root Workspaces support** </br>
You can use muliple folders in your workspace and change the current running project.

- **F# support** </br>
Your can build and debug projects, written in the `F#` language.


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


## Troubleshooting

&emsp;**.NET Meteor** creates log files in the `~/.vscode/extensions/dotnet-meteor-*/extension/bin` folder. You can find the following logs:
- `Workspace/Logs` - logs from the current workspace.
- `Xaml/Logs` - logs from the XAML IntelliSense server.
- `HotReload/Logs` - logs from the Hot Reload server.
- `Debug/Logs` - log from the debugger and profiler.

If checking the logs didn’t solve the issue, please open a new issue in this GitHub repository. Be sure to include a description of the problem along with the log files.

&emsp;**.NET Meteor** uses the `.NET Diagnostics` tools to profile applications. If you encounter any issues, please check the following:

- VSCode **Debug Console** tab should display a message about the successful connection. If you see the `Router stopped` message or something similar, the connection is not established. You can try to change the **profiler port** in the **.NET Meteor** settings.

- When profiling is started, the **Debug Console** tab should display the `Output File:` message. If you don't see this message after running the app and displaying the first view (after the splash screen), try deleting the `bin` and `obj` folders and rerunning the project. *Sometimes the issue occurs when you frequently switch between the profiling and debugging modes.*
