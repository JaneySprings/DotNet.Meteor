using System;
using System.Linq;
using System.IO;

namespace DotNet.Mobile.Shared {
    public static class PathUtils {
        public static string Invariant(string path, params string[] tokens) {
            foreach (var token in tokens) {
                path = Path.Combine(path, token);
                if (Directory.Exists(path))
                    return path;
            }
            return null;
        }

        private static string FindFile(string directory, string pattertn) {
            var files = Directory.GetFiles(directory, pattertn, SearchOption.TopDirectoryOnly);
            if (!files.Any())
                throw new Exception($"Could not find package in {directory}");
            return files.FirstOrDefault();
        }

        private static string FindDirectory(string directory, string pattertn) {
            var directories = Directory.GetDirectories(directory, pattertn, SearchOption.TopDirectoryOnly);
            if (!directories.Any())
                throw new Exception($"Could not find bundle in {directory}");
            return directories.FirstOrDefault();
        }
    }
}