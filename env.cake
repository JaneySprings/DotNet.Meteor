using _Path = System.IO.Path;

public string RootDirectory => MakeAbsolute(Directory("./")).ToString();

public string ArtifactsDirectory => _Path.Combine(RootDirectory, "artifacts");
public string ExtensionStagingDirectory => _Path.Combine(RootDirectory, "extension");
public string ExtensionAssembliesDirectory => _Path.Combine(ExtensionStagingDirectory, "bin");
public string InjectFilesDirectory => _Path.Combine(RootDirectory, "inject");

public string MonoDebuggerDirectory => _Path.Combine(RootDirectory, "src", "Mono.Debugger");
public string MeteorDebugProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Debug", "DotNet.Meteor.Debug.csproj");
public string MeteorTestsProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Tests", "DotNet.Meteor.Tests.csproj");


public void InjectSourceCode() {
    using var sha256 = System.Security.Cryptography.SHA256.Create();
    foreach (var line in System.IO.File.ReadAllLines(_Path.Combine(InjectFilesDirectory, "inject.txt"))) {
        var parts = line.Split(' ');
        var sourceFile = _Path.Combine(InjectFilesDirectory, parts[0]);
        var targetFile = _Path.Combine(parts[1]);
        var expectedHash = parts[2];
        var newHash = Convert.ToHexString(sha256.ComputeHash(System.IO.File.ReadAllBytes(sourceFile)));
        var actualHash = Convert.ToHexString(sha256.ComputeHash(System.IO.File.ReadAllBytes(targetFile)));

        if (actualHash == newHash)
            continue;
        if (actualHash != expectedHash)
            throw new Exception($"Hash mismatch for {targetFile}. Target file has been modified. Validation required.");

        CopyFile(sourceFile, targetFile);
    }
}