using DotNet.Mobile.Debug.Protocol;

namespace VsCodeMobileUtil {
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
        public ProjectType ProjectType { get; set; }
        public int DebugPort { get; set; }

        public LaunchData(Argument args) {
            var projectTypeString = args.ProjectType;
            ProjectType = (ProjectType)projectTypeString;
            DebugPort = args.DebugPort;
        }
    }
}