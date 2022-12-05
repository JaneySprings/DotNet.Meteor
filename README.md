# .NET Meteor

This VSCode extension allows you to build, debug .NET 7 / .NET 6 apps, and deploy them to Android/iOS devices/emulators.

* Fast and responsive
* Do not required to install the OmniSharp VSCode Extension
* Shows all projects that are exist in the opened workspace

# Run the Application

1. Open a project's folder.
1. Open the Run and Debug VSCode tab and click the 'create a launch.json file'.

    ![image](https://user-images.githubusercontent.com/12169834/205598333-1987f55f-a70c-402a-8986-1df2f256d9a0.png)
    
1. In the opened panel, select the '.NET Meteor Debugger'.

    ![image](https://user-images.githubusercontent.com/12169834/205598820-9767ff89-f64a-4c71-bbb3-9614a0aad254.png)
    
1. In the status bar, select a project (if there are several project in your opened folder), select a configuration (the debug is the default), and select a device. 
1. In the status bar, click the device name and select a target device/emulator from the opened panel.

    ![image](https://user-images.githubusercontent.com/12169834/205599557-9a9a3981-3e39-4d9d-9b0e-c1146f9df21e.png)

1. Press F5 to launch the application in the selected configuration (debug, release, etc). 
1. Enjoy!

## Limitations

* The application's Hot Reload is not available. We are working to implement feature.
* XAML Intellisence is not available.
