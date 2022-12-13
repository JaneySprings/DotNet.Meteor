using System;
using System.Linq;
using System.Diagnostics;
using Android.Sdk;
using Apple.Sdk;

namespace DotNet.Mobile.Debug.Tests;

internal static class Performance {
    public static void Test(string[] args) {
        AndroidEmulatorsFetchTest();
        AndroidDevicesFetchTest();

        AppleSimulatorsFetchTest();
        AppleDevicesFetchTest();

        if (args.Length != 2)
            return;

        WorkspaceFetchTest(args[1]);
    }

    private static void AndroidEmulatorsFetchTest() {
        DoTimed("Android Emulator Fast Fetch", () => {
            var r = AndroidTool.VirtualDevicesFast();
            Console.WriteLine("Found {0} emulators", r.Count);
        });
        DoTimed("Android Emulator AVD Fetch", () => {
            var r = VirtualDeviceManager.VirtualDevices();
            Console.WriteLine("Found {0} emulators", r.Count);
        });
    }
    private static void AndroidDevicesFetchTest() {
        DoTimed("Android Devices ADB Fetch", () => {
            var r = DeviceBridge.Devices();
            Console.WriteLine("Found {0} devices", r.Count);
        });
    }

    private static void AppleSimulatorsFetchTest() {
        DoTimed("Apple Simulators SimCtl Fetch", () => {
            var r = XCRun.Simulators();
            Console.WriteLine("Found {0} simulators", r.Count);
        });
        DoTimed("Apple Simulators SimCtl Fast Fetch", () => {
            var r = AppleTool.SimulatorsFast();
            Console.WriteLine("Found {0} simulators", r.Count);
        });
    }
    private static void AppleDevicesFetchTest() {
        DoTimed("Apple Devices Fast Fetch", () => {
            var r = AppleTool.PhysicalDevicesFast();
            Console.WriteLine("Found {0} devices", r.Count);
        });
        DoTimed("Apple Devices XCTrace Fetch", () => {
            var r = XCRun.PhysicalDevices();
            Console.WriteLine("Found {0} devices", r.Count);
        });
    }

    private static void WorkspaceFetchTest(string path) {
        DoTimed("Workspace Projects Fetch", () => {
            var r = WorkspaceAnalyzer.AnalyzeWorkspace(path);
            Console.WriteLine("Found {0} projects", r.Count());
        });
    }


    private static void DoTimed(string name, Action action) {
        Console.WriteLine($"\n==== {name} ====\n");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        action.Invoke();
        stopwatch.Stop();
        Console.WriteLine($"time elapsed: [{stopwatch.ElapsedMilliseconds}ms]\n");
    }
}