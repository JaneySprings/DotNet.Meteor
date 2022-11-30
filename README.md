# .NET Meteor

This extension can build and debug .NET 7 / .NET 6 apps for Android and iOS.

## Features

* Fast, very responsive
* No dependencies required
* Analyzing workspace projects
* Run and Debug Android/iOS apps on emulator or physical devices

# Usage

1. Create `launch.json` from template or manual:
```
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Debug .NET Mobile App",
            "type": "dotnet-meteor.debug",
            "request": "launch",
            "preLaunchTask": "dotnet-meteor: Build"
        }
    ]
}
```
2. In status bar, select project (if there are several project), select configuration (debug by default) and select device. 
3. Launch debug by pressing F5. 
4. Enjoy!