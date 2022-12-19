using System;
using System.IO;
using System.Linq;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.Session;

public class LaunchData {
    public string AppId { get; }
    public string AppName { get; }
    public string Target { get; }
    public string Framework { get; }
    public string ExecutablePath { get; }
    public DeviceData Device { get; }
    public Project Project { get; }

    public bool IsDebug => Target.Equals("debug", StringComparison.OrdinalIgnoreCase);

    public LaunchData(Project project, DeviceData device, string target) {
        var projectFile = ProjectFile.FromPath(project.Path);
        Project = project;
        Device = device;
        Target = target;
        AppId = projectFile.ValueFromProperty("ApplicationId");
        AppName = projectFile.ValueFromProperty("ApplicationTitle");
        Framework = Project.Frameworks.First(it =>
            it.Contains(Device.Platform, StringComparison.OrdinalIgnoreCase)
        );

        projectFile.Free();
        ExecutablePath = LocateExecutable();
    }

    private string LocateExecutable() {
        var rootDirectory = Path.GetDirectoryName(Project.Path);
        var frameworkDirectory = IsDebug
            ? Path.Combine(rootDirectory, "bin", "Debug", Framework)
            : Path.Combine(rootDirectory, "bin", "Release", Framework);

        if (!Directory.Exists(frameworkDirectory))
            throw new Exception($"Framework directory not found: {frameworkDirectory}");

        if (Device.IsAndroid) {
            var files = Directory.GetFiles(frameworkDirectory,  "*-Signed.apk", SearchOption.TopDirectoryOnly);
            if (!files.Any())
                throw new FileNotFoundException($"Could not find adnroid package in {frameworkDirectory}");
            return files.FirstOrDefault();
        }

        if (Device.IsWindows) {
            var files = Directory.GetFiles(frameworkDirectory, $"{AppName}.exe", SearchOption.AllDirectories);
            if (!files.Any())
                throw new FileNotFoundException($"Could not find windows program in {frameworkDirectory}");
            return files.FirstOrDefault();
        }

        if (Device.IsIPhone) {
            var archDirectories = Directory.GetDirectories(frameworkDirectory);
            var armDirectory = archDirectories.FirstOrDefault(it => it.Contains("arm64", StringComparison.OrdinalIgnoreCase));
            var intelDirectory = archDirectories.FirstOrDefault(it => !it.Contains("arm64", StringComparison.OrdinalIgnoreCase));

            if (!Device.IsEmulator) {
                if (armDirectory == null)
                    throw new DirectoryNotFoundException("Could not find arm64 directory");

                var armBundleDirectories = Directory.GetDirectories(armDirectory, "*.app", SearchOption.TopDirectoryOnly);
                if (!armBundleDirectories.Any())
                    throw new DirectoryNotFoundException($"Could not find iOS bundle in {armDirectory}");

                return armBundleDirectories.FirstOrDefault();
            }

            if (intelDirectory == null)
                throw new DirectoryNotFoundException("Could not find x86-64 directory");

            var intelBundledirectories = Directory.GetDirectories(intelDirectory, "*.app", SearchOption.TopDirectoryOnly);
            if (!intelBundledirectories.Any())
                throw new DirectoryNotFoundException($"Could not find iOS bundle in {intelDirectory}");

            return intelBundledirectories.FirstOrDefault();
        }

        if (Device.IsMacCatalyst) {
            var archDirectories = Directory.GetDirectories(frameworkDirectory);
            var armDirectory = archDirectories.FirstOrDefault(it => it.Contains("arm64", StringComparison.OrdinalIgnoreCase));
            var intelDirectory = archDirectories.FirstOrDefault(it => !it.Contains("arm64", StringComparison.OrdinalIgnoreCase));

            if (Device.IsArm && armDirectory != null) {
                var armBundleDirectories = Directory.GetDirectories(armDirectory, "*.app", SearchOption.TopDirectoryOnly);
                if (armBundleDirectories.Any())
                    return armBundleDirectories.FirstOrDefault();
            }

            if (intelDirectory == null)
                throw new DirectoryNotFoundException("Could not find x86-64 directory");

            var intelBundledirectories = Directory.GetDirectories(intelDirectory, "*.app", SearchOption.TopDirectoryOnly);
            if (!intelBundledirectories.Any())
                throw new DirectoryNotFoundException($"Could not find Mac bundle in {intelDirectory}");

            return intelBundledirectories.FirstOrDefault();
        }

        return null;
    }
}