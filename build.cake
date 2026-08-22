using System.Runtime.InteropServices;
using _Path = System.IO.Path;

public string RootDirectory => MakeAbsolute(Directory("./")).ToString();
public string ArtifactsDirectory => _Path.Combine(RootDirectory, "artifacts");
public string ExtensionStagingDirectory => _Path.Combine(RootDirectory, "extension");

// All platform targets shipped inside one universal VSIX.
var platforms = new[] { "win-x64", "win-arm64", "osx-x64", "osx-arm64", "linux-x64", "linux-arm64" };

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


void PublishTool(string project, string tool) {
	foreach (var rid in platforms) {
		DotNetPublish(project, new DotNetPublishSettings {
			MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
			OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", rid, tool),
			Configuration = configuration,
			Runtime = rid,
		});
	}
}

Task("workspace").Does(() => PublishTool(
	_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Workspace", "DotNet.Meteor.Workspace.csproj"), "Workspace"));
Task("xaml").Does(() => PublishTool(
	_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Xaml", "DotNet.Meteor.Xaml.LanguageServer", "DotNet.Meteor.Xaml.LanguageServer.csproj"), "Xaml"));
Task("hotreload").Does(() => PublishTool(
	_Path.Combine(RootDirectory, "src", "DotNet.Meteor.HotReload", "DotNet.Meteor.HotReload.csproj"), "HotReload"));

Task("plugin").Does(() => DotNetPack(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.HotReload.Plugin", "DotNet.Meteor.HotReload.Plugin.csproj"), new DotNetPackSettings {
	Configuration = configuration,
	MSBuildSettings = new DotNetMSBuildSettings { 
		AssemblyVersion = version, 
		Version = version
	},
}));


Task("debugger")
	.Does(() => PublishTool(_Path.Combine(RootDirectory, "src", "DotNet.Diagnostics", "src", "Tools", "dotnet-dsrouter", "dotnet-dsrouter.csproj"), "Debugger"))
	.Does(() => PublishTool(_Path.Combine(RootDirectory, "src", "DotNet.Diagnostics", "src", "Tools", "dotnet-gcdump", "dotnet-gcdump.csproj"), "Debugger"))
	.Does(() => DotNetPublish(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Debugger", "DotNet.Meteor.Debugger.csproj"), new DotNetPublishSettings {
		MSBuildSettings = new DotNetMSBuildSettings { 
			ArgumentCustomization = args => args.Append("/p:NuGetVersionRoslyn=4.5.0"),
			AssemblyVersion = version
		},
		OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", runtime, "Debugger"),
		Configuration = configuration,
		Runtime = runtime,
	}));

Task("debugger-all")
	.IsDependentOn("debugger")
	.Does(() => PublishTool(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Debugger", "DotNet.Meteor.Debugger.csproj"), "Debugger"));


Task("test")
	.Does(() => DotNetTest(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Common.Tests", "DotNet.Meteor.Common.Tests.csproj"),
		new DotNetTestSettings {  
			Configuration = configuration,
			Verbosity = DotNetVerbosity.Quiet,
			ResultsDirectory = ArtifactsDirectory,
			Loggers = new[] { "trx" }
		}
	)).Does(() => DotNetTest(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Debugger.Tests", "DotNet.Meteor.Debugger.Tests.csproj"),
		new DotNetTestSettings {  
			Configuration = configuration,
			Verbosity = DotNetVerbosity.Quiet,
			ResultsDirectory = ArtifactsDirectory,
			Loggers = new[] { "trx" }
		}
	));


Task("vsix")
	.IsDependentOn("clean")
	.IsDependentOn("workspace")
	.IsDependentOn("xaml")
	.IsDependentOn("hotreload")
	.IsDependentOn("debugger-all")
	.Does(() => {
		var output = _Path.Combine(ArtifactsDirectory, $"DotNet.Meteor.Fork.v{version}.vsix");
		ExecuteCommand("npm", "install --include=dev");
		ExecuteCommand("vsce", $"package --out {output} --no-git-tag-version {version}");
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
