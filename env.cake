using _Path = System.IO.Path;

public string RootDirectory => MakeAbsolute(Directory("./")).ToString();

public string ArtifactsDirectory => _Path.Combine(RootDirectory, "artifacts");
public string ExtensionStagingDirectory => _Path.Combine(RootDirectory, "extension");
public string ExtensionAssembliesDirectory => _Path.Combine(ExtensionStagingDirectory, "bin");

public string MeteorDebugProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Debug", "DotNet.Meteor.Debug.csproj");
public string MeteorTestsProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Tests", "DotNet.Meteor.Tests.csproj");