using System;
using System.IO;
using System.Linq;

namespace DotNet.Mobile.Shared {
    public static class BundleFinder {
        public static string FindBundle(string rootDirectory, DeviceData device, string target) {
            if (device.IsAndroid)
                return FindAndroidPackage(rootDirectory, target);
            if (device.IsIPhone)
                return FindApplePackage(rootDirectory, target, device.IsEmulator);
            throw new Exception("Could not find bundle");
        }

        private static string FindAndroidPackage(string rootDirectory, string target) {
            var binDirectory = Path.Combine(rootDirectory, "bin");
            var files = Directory
                .GetFiles(binDirectory, "*-Signed.apk", SearchOption.AllDirectories)
                .Where(it => it.Contains(target, StringComparison.OrdinalIgnoreCase));

            if (!files.Any())
                throw new Exception("Could not find Android package");

            return files.FirstOrDefault();
        }

        private static string FindApplePackage(string rootDirectory, string target, bool isSimulator) {
            var binDirectory = Path.Combine(rootDirectory, "bin");
            var directories = Directory
                .GetDirectories(binDirectory, "*.app", SearchOption.AllDirectories)
                .Where(it => it.Contains(target, StringComparison.OrdinalIgnoreCase));

            if (!directories.Any())
                throw new Exception("Could not find ios bundle");

            var armApp = directories.FirstOrDefault(it => it.Contains("arm64", StringComparison.OrdinalIgnoreCase));
            var otherApp = directories.FirstOrDefault(it => !it.Contains("arm64", StringComparison.OrdinalIgnoreCase));

            if (isSimulator) {
                if (otherApp == null)
                    throw new Exception("Could not find bundle for iossimulator");
                return otherApp;
            }

            if (armApp == null)
                throw new Exception("Could not find bundle for ios-arm");
            return armApp;
        }
    }
}