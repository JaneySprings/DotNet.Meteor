using System.Runtime.InteropServices;
using _Path = System.IO.Path;

public string RootDirectory => MakeAbsolute(Directory("./")).ToString();
public string ArtifactsDirectory => _Path.Combine(RootDirectory, "artifacts");
public string ExtensionStagingDirectory => _Path.Combine(RootDirectory, "extension");

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


Task("workspace").Does(() => DotNetPublish(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Workspace", "DotNet.Meteor.Workspace.csproj"), new DotNetPublishSettings {
	MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
	OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Workspace"),
	Configuration = configuration,
	Runtime = runtime,
}));
Task("xaml").Does(() => DotNetPublish(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Xaml", "DotNet.Meteor.Xaml.LanguageServer", "DotNet.Meteor.Xaml.LanguageServer.csproj"), new DotNetPublishSettings {
	MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
	OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Xaml"),
	Configuration = configuration,
	Runtime = runtime,
}));
Task("hotreload").Does(() => DotNetPublish(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.HotReload", "DotNet.Meteor.HotReload.csproj"), new DotNetPublishSettings {
	MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
	OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "HotReload"),
	Configuration = configuration,
	Runtime = runtime,
}));
Task("plugin").Does(() => DotNetPack(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.HotReload.Plugin", "DotNet.Meteor.HotReload.Plugin.csproj"), new DotNetPackSettings {
	Configuration = configuration,
	MSBuildSettings = new DotNetMSBuildSettings { 
		AssemblyVersion = version, 
		Version = version
	},
}));


Task("debugger")
	.Does(() => DotNetPublish(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Debugger", "DotNet.Meteor.Debugger.csproj"), new DotNetPublishSettings {
		MSBuildSettings = new DotNetMSBuildSettings { 
			ArgumentCustomization = args => args.Append("/p:NuGetVersionRoslyn=4.5.0"),
			AssemblyVersion = version
		},
		OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Debugger"),
		Configuration = configuration,
		Runtime = runtime,
	}));
Task("profiler")
	.Does(() => DotNetPublish(_Path.Combine(RootDirectory, "src", "DotNet.Diagnostics", "src", "Tools", "dotnet-dsrouter", "dotnet-dsrouter.csproj"), new DotNetPublishSettings {
		OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Profiler"),
		Configuration = configuration,
		Runtime = runtime,
	})).Does(() => DotNetPublish(_Path.Combine(RootDirectory, "src", "DotNet.Diagnostics", "src", "Tools", "dotnet-gcdump", "dotnet-gcdump.csproj"), new DotNetPublishSettings {
		OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Profiler"),
		Configuration = configuration,
		Runtime = runtime,
	})).Does(() => DotNetPublish(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Profiler", "DotNet.Meteor.Profiler.csproj"), new DotNetPublishSettings {
		MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
		OutputDirectory = _Path.Combine(ExtensionStagingDirectory, "bin", "Profiler"),
		Configuration = configuration,
		Runtime = runtime,
	}));


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
	)).Does(() => DotNetTest(_Path.Combine(RootDirectory, "src", "DotNet.Meteor.Profiler.Tests", "DotNet.Meteor.Profiler.Tests.csproj"),
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
	.IsDependentOn("debugger")
	.IsDependentOn("profiler")
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