## How to work with the repository

### Prerequisites
- [Node.js](https://nodejs.org/en/) (latest)
- [.NET SDK](https://dotnet.microsoft.com/download) (latest)

### Clone the repository
This project uses submodules, so you need to clone it with the `--recursive` flag:
```bash
git clone --recursive
```

### Project structure
The `src/` folder contains the source code of the repository. It has the following projects:
- `DotNet.Diagnostics`: Submodule that contains the .NET Diagnostic Tools for profiling.
- `Mono.Debugger`: Submodule that contains my fork of the Mono Debugger.
- `DotNet.Meteor.Common`: Common tools and utilities for all projects.
- `DotNet.Meteor.Debug`: Debugger and profiler implementation for Xamarin projects (DAP).
- `DotNet.Meteor.HotReload`: Hot reload client that communicates with the application.
- `DotNet.Meteor.HotReload.Plugin`: NuGet package that contains the hot reload server for .NET MAUI projects.
- `DotNet.Meteor.Workspace`: Tools for managing the workspace and the devices.
- `DotNet.Meteor.Xaml`: Submodule that contains XAML Language Server (LSP) for .NET MAUI project.
- `DotNet.Meteor.Tests`: Unit tests for the projects.
- `VSCode`: Visual Studio Code extension.

### Build the projects
To build the projects, you can use the [build.cake](https://github.com/JaneySprings/DotNet.Meteor/blob/main/build.cake) script in the root folder. It will run the `vsix` target in the `debug` configuration by default:
```bash
dotnet tool restore
dotnet cake
```
After running the script, you can find the VSIX extension in the `artifacts` folder.

### Debug the projects
To debug the projects, you can use the `launch.json` file in the `.vscode` folder. It has the following configurations:
- `Run Extension`: Launches the Visual Studio Code extension in a new window. You can debug the _typescipt_ code.
- `.NET Core Debugger (attach)`: Attaches the debugger to the any .NET process. You can debug the _C#_ code (for example, you can start a new debug session and attach to the `DotNet.Meteor.Debug` process).
- `.NET Core Debugger (launch)`: Launches any console application. You can debug the _C#_ code (for example, you can start the `DotNet.Meteor.Workspace` project).

### Code guidelines
This repository uses an `.editorconfig` file to enforce the code style. You can find the rules in the `.editorconfig` file.