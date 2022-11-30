using _Path = System.IO.Path;

public string RootDirectory => MakeAbsolute(Directory("./")).ToString();

public string ArtifactsDirectory => _Path.Combine(RootDirectory, "artifacts");
public string ExtensionStagingDirectory => _Path.Combine(RootDirectory, "extension");
public string ExtensionAssembliesDirectory => _Path.Combine(ExtensionStagingDirectory, "bin");

public string MonoDebuggerDirectory => _Path.Combine(RootDirectory, "src", "Mono.Debugger");
public string MobileDebugProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Mobile.Debug", "DotNet.Mobile.Debug.csproj");
public string MobileHotReloadProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Mobile.HotReload", "DotNet.Mobile.HotReload.csproj");

public string NuGetVersionRoslyn => "4.3.1";
