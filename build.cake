#addin nuget:?package=Cake.FileHelpers&version=6.1.2

using _Path = System.IO.Path;

public string RootDirectory => MakeAbsolute(Directory("./")).ToString();
public string ArtifactsDirectory => _Path.Combine(RootDirectory, "artifacts");
public string ExtensionStagingDirectory => _Path.Combine(RootDirectory, "extension");
public string ExtensionBinariesDirectory => _Path.Combine(ExtensionStagingDirectory, "bin");

public string MeteorWorkspaceProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Workspace", "DotNet.Meteor.Workspace.csproj");
public string MeteorHotReloadProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.HotReload", "DotNet.Meteor.HotReload.csproj");
public string MeteorXamlProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Xaml", "DotNet.Meteor.Xaml.csproj");
public string MeteorDebugProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Debug", "DotNet.Meteor.Debug.csproj");
public string MeteorTestsProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Tests", "DotNet.Meteor.Tests.csproj");
public string MeteorPluginProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.HotReload.Plugin", "DotNet.Meteor.HotReload.Plugin.csproj");
public string DotNetDSRouterProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Diagnostics", "src", "Tools", "dotnet-dsrouter", "dotnet-dsrouter.csproj");

var target = Argument("target", "vsix");
var runtime = Argument("arch", "osx-arm64");
var version = Argument("release-version", "1.0.0");
var configuration = Argument("configuration", "debug");

///////////////////////////////////////////////////////////////////////////////
// COMMON
///////////////////////////////////////////////////////////////////////////////

Task("clean").Does(() => {
	EnsureDirectoryExists(ArtifactsDirectory);
	CleanDirectory(ExtensionStagingDirectory);
});

///////////////////////////////////////////////////////////////////////////////
// DOTNET
///////////////////////////////////////////////////////////////////////////////

Task("workspace").Does(() => DotNetPublish(MeteorWorkspaceProjectPath, new DotNetPublishSettings {
	MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
	Configuration = configuration,
	Runtime = runtime,
}));

Task("hotreload").Does(() => DotNetPublish(MeteorHotReloadProjectPath, new DotNetPublishSettings {
	MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
	Configuration = configuration,
	Runtime = runtime,
}));

Task("xaml").Does(() => DotNetBuild(MeteorXamlProjectPath, new DotNetBuildSettings {
	MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
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
	OutputDirectory = _Path.Combine(ExtensionBinariesDirectory, "Debug"),
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


///////////////////////////////////////////////////////////////////////////////
// TYPESCRIPT
///////////////////////////////////////////////////////////////////////////////

Task("vsix")
	.IsDependentOn("clean")
	.IsDependentOn("workspace")
	.IsDependentOn("hotreload")
	.IsDependentOn("xaml")
	.IsDependentOn("debugger")
	.IsDependentOn("dsrouter")
	.Does(() => {
		var package = _Path.Combine(RootDirectory, "package.json");
		var regex = @"^\s\s(""version"":\s+)("".+"")(,)";
		var options = System.Text.RegularExpressions.RegexOptions.Multiline;
		ReplaceRegexInFiles(package, regex, $"  $1\"{version}\"$3", options);
	})
	.Does(() => {
		switch (runtime) {
			case "win-x64": runtime = "win32-x64"; break;
			case "win-arm64": runtime = "win32-arm64"; break;
			case "osx-x64": runtime = "darwin-x64"; break;
			case "osx-arm64": runtime = "darwin-arm64"; break;
		}
		var output = _Path.Combine(ArtifactsDirectory, $"DotNet.Meteor.v{version}_{runtime}.vsix");
		ExecuteCommand("vsce", $"package --target {runtime} --out {output}");
	});


void ExecuteCommand(string command, string arguments) {
	if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
		arguments = $"/c \"{command} {arguments}\"";
		command = "cmd";
	}
	if (StartProcess(command, arguments) != 0)
		throw new Exception("Command exited with non-zero exit code.");
}

RunTarget(target);