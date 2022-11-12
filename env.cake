public string RootDirectory => MakeAbsolute(Directory("./")).ToString();

public string ArtifactsDirectory => $"{RootDirectory}/artifacts";
public string ExtensionStagingDirectory => $"{RootDirectory}/extension";
public string ExtensionAssembliesDirectory => $"{ExtensionStagingDirectory}/bin";

public string MobileDebugProjectPath => $"{RootDirectory}/src/DotNet.Mobile.Debug/DotNet.Mobile.Debug.csproj";
