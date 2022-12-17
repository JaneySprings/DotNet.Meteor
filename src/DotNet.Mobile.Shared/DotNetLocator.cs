using System;
using System.IO;
using System.Linq;
using DotNet.Mobile.Shared;

namespace Microsoft.Sdk {
    public abstract class DotNetLocator {
        public static string BuildedAppPath(string root, string framework, bool isDebug, DeviceData device) {
            string basePath = Path.Combine(root, "bin");
            string targetPath = isDebug
                ? PathUtils.Invariant(basePath, "Debug", "debug")
                : PathUtils.Invariant(basePath, "Release", "release");

            if (targetPath == null)
                throw new DirectoryNotFoundException("Could not find target directory");

            targetPath = Path.Combine(targetPath, framework);

            if (!Directory.Exists(targetPath))
                throw new DirectoryNotFoundException("Could not find framework directory");

            if (device.IsAndroid)
                return AndroidPackage(targetPath);
            if (device.IsIPhone)
                return AppleBundle(targetPath, !device.IsEmulator);
            if (device.IsMacCatalyst)
                return MacCatalystBundle(targetPath, device.IsArm);

            throw new Exception("Not supported device");
        }

        private static string AndroidPackage(string targetDirectory) {
            var files = Directory.GetFiles(targetDirectory,  "*-Signed.apk", SearchOption.TopDirectoryOnly);
            if (!files.Any())
                throw new FileNotFoundException($"Could not find adnroid package in {targetDirectory}");
            return files.FirstOrDefault();
        }

        private static string AppleBundle(string targetDirectory, bool IsArm) {
            var archDirectories = Directory.GetDirectories(targetDirectory);

            if (!archDirectories.Any())
                throw new DirectoryNotFoundException($"Could not find iOS bundle in {targetDirectory}");

            var armDirectory = archDirectories.FirstOrDefault(it => it.Contains("arm64", StringComparison.OrdinalIgnoreCase));
            var intelDirectory = archDirectories.FirstOrDefault(it => !it.Contains("arm64", StringComparison.OrdinalIgnoreCase));

            if (IsArm) {
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

        private static string MacCatalystBundle(string targetDirectory, bool IsArm) {
            var archDirectories = Directory.GetDirectories(targetDirectory);

            if (!archDirectories.Any())
                throw new DirectoryNotFoundException("Could not find Mac bundle");

            var armDirectory = archDirectories.FirstOrDefault(it => it.Contains("arm64", StringComparison.OrdinalIgnoreCase));
            var intelDirectory = archDirectories.FirstOrDefault(it => !it.Contains("arm64", StringComparison.OrdinalIgnoreCase));

            if (IsArm && armDirectory != null) {
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
    }
}