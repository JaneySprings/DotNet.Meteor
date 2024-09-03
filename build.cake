using System.Runtime.InteropServices;
using _Path = System.IO.Path;

public string RootDirectory => MakeAbsolute(Directory("./")).ToString();
public string ArtifactsDirectory => _Path.Combine(RootDirectory, "artifacts");
public string ExtensionStagingDirectory => _Path.Combine(RootDirectory, "extension");
public string MeteorWorkspaceProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Workspace", "DotNet.Meteor.Workspace.csproj");
public string MeteorXamlProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Xaml", "DotNet.Meteor.Xaml.LanguageServer", "DotNet.Meteor.Xaml.LanguageServer.csproj");
public string MeteorDebugProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Debug", "DotNet.Meteor.Debug.csproj");
public string MeteorTestsProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Tests", "DotNet.Meteor.Tests.csproj");
public string MeteorPluginProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.HotReload.Plugin", "DotNet.Meteor.HotReload.Plugin.csproj");
public string DotNetDSRouterProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Diagnostics", "src", "Tools", "dotnet-dsrouter", "dotnet-dsrouter.csproj");
public string DotNetGCDumpProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Diagnostics", "src", "Tools", "dotnet-gcdump", "dotnet-gcdump.csproj");

var target = Argument("target", "vsix");
var version = Argument("release-version", "1.0.0");
var configuration = Argument("configuration", "debug");
var runtime = Argument("arch", RuntimeInformation.RuntimeIdentifier);


Task("clean").Does(() => {
	EnsureDirectoryExists(ArtifactsDirectory);
	CleanDirectory(ExtensionStagingDirectory);
	CleanDirectories(_Path.Combine(RootDirectory, "src", "**", "bin"));
	CleanDirectories(_Path.Combine(RootDirectory, "src", "**", "obj"));
});

Task("workspace").Does(() => DotNetPublish(MeteorWorkspaceProjectPath, new DotNetPublishSettings {
	MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
	OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Workspace"),
	Configuration = configuration,
	Runtime = runtime,
}));
Task("xaml").Does(() => DotNetPublish(MeteorXamlProjectPath, new DotNetPublishSettings {
	MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
	OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Xaml"),
	Configuration = configuration,
	Runtime = runtime,
}));
Task("debugger").Does(() => DotNetPublish(MeteorDebugProjectPath, new DotNetPublishSettings {
	Runtime = runtime,
	Configuration = configuration,
	MSBuildSettings = new DotNetMSBuildSettings { 
		ArgumentCustomization = args => args.Append("/p:NuGetVersionRoslyn=4.5.0"),
		AssemblyVersion = version
	},
}));
Task("dsrouter").Does(() => DotNetPublish(DotNetDSRouterProjectPath, new DotNetPublishSettings {
	OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Debug"),
	Configuration = configuration,
	Runtime = runtime,
}));
Task("gcdump").Does(() => DotNetPublish(DotNetGCDumpProjectPath, new DotNetPublishSettings {
	OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Debug"),
	Configuration = configuration,
	Runtime = runtime,
}));
Task("plugin").Does(() => DotNetPack(MeteorPluginProjectPath, new DotNetPackSettings {
	Configuration = configuration,
	MSBuildSettings = new DotNetMSBuildSettings { 
		AssemblyVersion = version, 
		Version = version
	},
}));
Task("test").Does(() => DotNetTest(MeteorTestsProjectPath, new DotNetTestSettings {  
	Configuration = configuration,
	Verbosity = DotNetVerbosity.Quiet,
	ResultsDirectory = ArtifactsDirectory,
	Loggers = new[] { "trx" }
}));

Task("vsix")
	.IsDependentOn("clean")
	.IsDependentOn("workspace")
	.IsDependentOn("xaml")
	.IsDependentOn("debugger")
	.IsDependentOn("dsrouter")
	.IsDependentOn("gcdump")
	.Does(() => {
		var vsruntime = runtime.Replace("win-", "win32-").Replace("osx-", "darwin-");
		var output = _Path.Combine(ArtifactsDirectory, $"DotNet.Meteor.v{version}_{vsruntime}.vsix");
		ExecuteCommand("npm", "install");
		ExecuteCommand("vsce", $"package --target {vsruntime} --out {output} --no-git-tag-version {version}");
	});


void ExecuteCommand(string command, string arguments) {
	if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
		arguments = $"/c \"{command} {arguments}\"";
		command = "cmd";
	}
	if (StartProcess(command, arguments) != 0)
		throw new Exception($"{command} exited with non-zero exit code.");
}

RunTarget(target);