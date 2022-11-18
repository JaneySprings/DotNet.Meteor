#addin nuget:?package=Cake.FileHelpers&version=5.0.0
#addin nuget:?package=Cake.VsCode&version=0.11.1
#load "env.cake"

var target = Argument("target", "vsix");
var version = Argument("release-version", "1.0.1");
var configuration = Argument("configuration", "release");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("up-version")
   .DoesForEach<FilePath>(GetFiles($"{RootDirectory}/*.json"), file => {
      var regex = @"^\s\s(""version"":\s+)("".+"")(,)";
      var options = System.Text.RegularExpressions.RegexOptions.Multiline;
      ReplaceRegexInFiles(file.ToString(), regex, $"  $1\"{version}\"$3", options);
   });

Task("build-dotnet")
   .Does(() => CleanDirectory(ExtensionAssembliesDirectory))
   .DoesForEach<FilePath>(GetFiles($"{MonoDebuggerDirectory}/**/*.csproj"), file => {
      var regex = @"(<NuGetVersionRoslyn\s+Condition=""\$\(NuGetVersionRoslyn\)\s*==\s*''"")(>.+<)(/NuGetVersionRoslyn>)";
      ReplaceRegexInFiles(file.ToString(), regex, $"$1>{NuGetVersionRoslyn}<$3");
   })
   .Does(() => DotNetBuild(MobileDebugProjectPath, new DotNetBuildSettings {
      OutputDirectory = ExtensionAssembliesDirectory,
      Configuration = configuration,
      MSBuildSettings = new DotNetMSBuildSettings {
         AssemblyVersion = version
      }
   }));
   
Task("vsix")
   .IsDependentOn("up-version")
   .IsDependentOn("build-dotnet")
   .Does(() => VscePackage(new VscePackageSettings {
      OutputFilePath = ArtifactsDirectory,
      WorkingDirectory = RootDirectory
   }));


RunTarget(target);