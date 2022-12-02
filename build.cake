#addin nuget:?package=Cake.FileHelpers&version=5.0.0
#addin nuget:?package=Cake.VsCode&version=0.11.1
#load "env.cake"

using _Path = System.IO.Path;

var target = Argument("target", "vsix");
var version = Argument("release-version", "1.1.4");
var configuration = Argument("configuration", "release");

///////////////////////////////////////////////////////////////////////////////
// DOTNET
///////////////////////////////////////////////////////////////////////////////

Task("build-debugger")
   .Does(() => CleanDirectory(ExtensionAssembliesDirectory))
   .DoesForEach<FilePath>(GetFiles(_Path.Combine(MonoDebuggerDirectory, "**", "*.csproj")), file => {
      var regex = @"(<NuGetVersionRoslyn\s+Condition=""\$\(NuGetVersionRoslyn\)\s*==\s*''"")(>.+<)(/NuGetVersionRoslyn>)";
      ReplaceRegexInFiles(file.ToString(), regex, $"$1>{NuGetVersionRoslyn}<$3");
   })
   .Does(() => DotNetBuild(MobileDebugProjectPath, new DotNetBuildSettings {
      MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
      OutputDirectory = ExtensionAssembliesDirectory,
      Configuration = configuration,
   }))
   .Does(() => {
      DeleteFiles(GetFiles(_Path.Combine(ExtensionAssembliesDirectory, "*.pdb")));
      DeleteFiles(GetFiles(_Path.Combine(ExtensionAssembliesDirectory, "*.deps.json")));
      DeleteFiles(GetFiles(_Path.Combine(ExtensionAssembliesDirectory, "*.xml")));
   });

Task("build-reload")
   .Does(() => DotNetBuild(MobileHotReloadProjectPath, new DotNetBuildSettings {
      MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
      Configuration = configuration,
   }));

///////////////////////////////////////////////////////////////////////////////
// TYPESCRIPT
///////////////////////////////////////////////////////////////////////////////

Task("up-version")
   .DoesForEach<FilePath>(GetFiles(_Path.Combine(RootDirectory, "*.json")), file => {
      var regex = @"^\s\s(""version"":\s+)("".+"")(,)";
      var options = System.Text.RegularExpressions.RegexOptions.Multiline;
      ReplaceRegexInFiles(file.ToString(), regex, $"  $1\"{version}\"$3", options);
   });

Task("vsix")
   .IsDependentOn("up-version")
   .IsDependentOn("build-debugger")
   .Does(() => {
      CleanDirectory(ArtifactsDirectory);
      VscePackage(new VscePackageSettings {
         OutputFilePath = _Path.Combine(ArtifactsDirectory, $"DotNet.Meteor.{version}.vsix"),
         WorkingDirectory = RootDirectory
      });
   });


RunTarget(target);