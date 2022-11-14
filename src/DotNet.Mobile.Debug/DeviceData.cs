using System;
using Newtonsoft.Json;

namespace VsCodeMobileUtil {
    public class DeviceData {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("serial")]
        public string Serial { get; set; }

        [JsonProperty("platforms")]
        public string[] Platforms { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("isEmulator")]
        public bool IsEmulator { get; set; }

        [JsonProperty("isRunning")]
        public bool IsRunning { get; set; }

        [JsonProperty("rid")]
        public string RuntimeIdentifier { get; set; }
    }

    public class SimpleResult {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
    public enum ProjectType {
        Mono,
        Android,
        iOS,
        MacCatalyst,
        Mac,
        UWP,
        Unknown,
        WPF,
        Blazor,

    }
    public class LaunchData {
        public string AppName { get; set; } = "";
        public string DeviceId { get; set; }
        public string Project { get; set; }
        public string Configuration { get; set; }
        public string Platform { get; set; }
        public string RuntimeIdentifier { get; set; }
        public ProjectType ProjectType { get; set; }
        public string ProjectTargetFramework { get; set; }
        public bool ProjectIsCore { get; set; }
        public string OutputDirectory { get; set; }
        public bool EnableHotReload { get; set; }
        public string AdbDeviceName { get; set; }
        public string WorkspaceDirectory { get; set; }

        public int DebugPort { get; set; }

        public LaunchData() {
        }

        public LaunchData(dynamic args) {
            Project = getString(args, VSCodeKeys.LaunchConfig.ProjectPath);
            Configuration = getString(args, VSCodeKeys.LaunchConfig.Configuration);
            Platform = getString(args, VSCodeKeys.LaunchConfig.Platform, "AnyCPU");
            DeviceId = getString(args, VSCodeKeys.LaunchConfig.DeviceId);
            RuntimeIdentifier = getString(args, VSCodeKeys.LaunchConfig.RuntimeIdentifier);
            OutputDirectory = cleanseStringPaths(getString(args, VSCodeKeys.LaunchConfig.Output));
            EnableHotReload = getBool(args, nameof(EnableHotReload));
            AdbDeviceName = getString(args, VSCodeKeys.LaunchConfig.AdbEmulatorName);
            var projectTypeString = getInt(args, VSCodeKeys.LaunchConfig.ProjectType, 0);
            ProjectType = (ProjectType)projectTypeString;
            ProjectTargetFramework = getString(args, VSCodeKeys.LaunchConfig.ProjectTargetFramework);
            ProjectIsCore = getBool(args, VSCodeKeys.LaunchConfig.ProjectIsCore, false);
            DebugPort = getInt(args, VSCodeKeys.LaunchConfig.DebugPort, 55555);
            WorkspaceDirectory = getString(args, VSCodeKeys.LaunchConfig.WorkspaceDirectory);
            //if(string.IsNullOrWhiteSpace(projectTypeString))
            //	ProjectType = Enum.Parse (typeof(ProjectType), projectTypeString,true);
        }

        public (bool success, string message) Validate() {
            (bool success, string message) validateString(string value, string name) {
                return string.IsNullOrWhiteSpace(value) ? (false, $"{name} is not valid") : (true, "");
            }

            var checks = new[] {
                validateString(Project,nameof(Project)),
                validateString(Configuration,nameof(Configuration)),
                validateString(OutputDirectory,nameof(OutputDirectory)),
            };
            foreach (var check in checks) {
                if (!check.success)
                    return check;
            }

            if (ProjectType == ProjectType.iOS) {
                if (string.IsNullOrWhiteSpace(DeviceId))
                    return (false, "iOS simulator is not valid");
            } else if (ProjectType == ProjectType.Android) {
                if (string.IsNullOrWhiteSpace(DeviceId) && string.IsNullOrWhiteSpace(AdbDeviceName))
                    return (false, "Android device is not valid");
            }

            return (true, "");
        }

        static string cleanseStringPaths(string path) {
            if (Util.IsWindows)
                return path;
            return path.Replace("\\", "/");
        }

        private static bool getBool(dynamic container, string propertyName, bool dflt = false) {
            try {
                return (bool)container[propertyName];
            } catch (Exception) {
                // ignore and return default value
            }
            return dflt;
        }

        private static int getInt(dynamic container, string propertyName, int dflt = 0) {
            try {
                return (int)container[propertyName];
            } catch (Exception) {
                // ignore and return default value
            }
            return dflt;
        }

        private static string getString(dynamic args, string property, string dflt = null) {
            var s = (string)args[property];
            if (s == null) {
                return dflt;
            }
            s = s.Trim();
            if (s.Length == 0) {
                return dflt;
            }
            return s;
        }

    }
}