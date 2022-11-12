namespace DotNet.Mobile.Debug;

public class LaunchData {
    public string AppName { get; set; }
    public string DeviceId { get; set; }
    public string Project { get; set; }
    public string Configuration { get; set; }
    public string Platform { get; set; }
    public string RuntimeIdentifier { get; set; }
    public ProjectType ProjectType { get; set; }
    public string ProjectTargetFramework { get; set; }
    public string OutputDirectory { get; set; }
    public string WorkspaceDirectory { get; set; }
    public int DebugPort { get; set; }

    public LaunchData() {}
    public LaunchData(dynamic args) {
        // Project = getString(args, VSCodeKeys.LaunchConfig.ProjectPath);
        // Configuration = getString(args, VSCodeKeys.LaunchConfig.Configuration);
        // Platform = getString(args, VSCodeKeys.LaunchConfig.Platform, "AnyCPU");
        // DeviceId = getString(args, VSCodeKeys.LaunchConfig.DeviceId);
        // RuntimeIdentifier = getString(args, VSCodeKeys.LaunchConfig.RuntimeIdentifier);
        // OutputDirectory = cleanseStringPaths(getString(args, VSCodeKeys.LaunchConfig.Output));
        // var projectTypeString = getInt(args, VSCodeKeys.LaunchConfig.ProjectType, 0);
        // ProjectType = (ProjectType)projectTypeString;
        // ProjectTargetFramework = getString(args, VSCodeKeys.LaunchConfig.ProjectTargetFramework);
        // DebugPort = getInt(args, VSCodeKeys.LaunchConfig.DebugPort, 55555);
        // WorkspaceDirectory = getString(args, VSCodeKeys.LaunchConfig.WorkspaceDirectory);
        //if(string.IsNullOrWhiteSpace(projectTypeString))
        //	ProjectType = Enum.Parse (typeof(ProjectType), projectTypeString,true);
    }

    static string cleanseStringPaths(string path) {
        // if (Util.IsWindows)
            return path;
        //return path.Replace("\\", "/");
    }
}