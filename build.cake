#addin nuget:?package=Cake.FileHelpers&version=5.0.0
#load "env.cake"

var target = Argument("target", "vsix");
var version = Argument("release-version", "1.0.1");
var configuration = Argument("configuration", "release");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("prepare").Does(() => {
   CleanDirectory(ExtensionStagingDirectory);
});

Task("build-dotnet")
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
   
Task("vsix").Does(() => {
   Information("Hello Cake!");
});


RunTarget(target);