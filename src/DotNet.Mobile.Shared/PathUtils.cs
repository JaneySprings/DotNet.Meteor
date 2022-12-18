using System.IO;

namespace DotNet.Mobile.Shared {
    public static class PathUtils {
        public static string Invariant(string basePath, params string[] tokens) {
            foreach (var token in tokens) {
                string path = Path.Combine(basePath, token);
                if (Directory.Exists(path))
                    return path;
            }
            return null;
        }
    }
}