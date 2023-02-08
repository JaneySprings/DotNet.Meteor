# .NET Meteor

This VSCode extension allows you to build, debug .NET 6 / .NET 7 apps and deploy them to devices or emulators. The following table lists supported .NET MAUI target platforms and their capabilities:

| Application Type | Build and Run (including Emulators) | Debugging |
|-|-|-|
| WinUI | ✅ | ❌ |
| Android | ✅ | ✅ |
| iOS | ✅ | ✅ |
| MacCatalyst | ✅ | ✅ |

## Included Features

* Fast and responsive.
* Do not require to install the OmniSharp VSCode Extension.
* Shows all projects that exist in the opened workspace.
* Works on Windows, MacOS, and Linux (tested on Ubuntu).

## Run the Application

1. Open a project's folder.
1. Open the Run and Debug VSCode tab and click the '_create a launch.json file_'.

    ![image](./img/build-file.png)
    
1. In the opened panel, select the '.NET Meteor Debugger'.

    ![image](./img/debugger.png)
    
1. In the status bar, select a project (if your opened folder contains several projects) and a configuration (the debug is the default).

    ![image](./img/status-1.png)

    
3. In the status bar, click the device name and select a target device/emulator from the opened panel.

    ![image](./img/devices.png)

1. Press F5 to launch the application in the selected configuration (debug, release, etc.). 
1. Enjoy!

## Limitations

* The application's Hot Reload is not available. We are working on implementing this feature.
* XAML IntelliSense is not available.

## More Topics

* [Getting Started for MAUI Development with DotNet.Meteor Extension](https://github.com/JaneySprings/DotNet.Meteor/wiki/Getting-started-for-MAUI-development-with-DotNet.Meteor-extension)
* [Customize Predefined Tasks](https://github.com/JaneySprings/DotNet.Meteor/wiki/Predefined-task-customization)
