using System;
using System.IO;
using System.Linq;

namespace DotNet.Mobile.Shared {
    public static class BundleLocator {
        public static string FindAndroidPackage(string root, string target, string framework) {
            var binDirectory = Path.Combine(root, "bin");
            var files = Directory
                .GetFiles(binDirectory, "*-Signed.apk", SearchOption.AllDirectories)
                .Where(it =>
                    it.Contains(target, StringComparison.OrdinalIgnoreCase) &&
                    it.Contains(framework, StringComparison.OrdinalIgnoreCase)
                );

            if (!files.Any())
                throw new Exception("Could not find Android package");

            return files.FirstOrDefault();
        }

        public static string FindAppleBundle(string rootDirectory, string target, string framework, bool isSimulator) {
            var binDirectory = Path.Combine(rootDirectory, "bin");
            var directories = Directory
                .GetDirectories(binDirectory, "*.app", SearchOption.AllDirectories)
                .Where(it =>
                    it.Contains(target, StringComparison.OrdinalIgnoreCase) &&
                    it.Contains(framework, StringComparison.OrdinalIgnoreCase)
                );

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