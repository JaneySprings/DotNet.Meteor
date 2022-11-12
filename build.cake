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
   .Does(() => DotNetBuild(MobileDebugProjectPath, new DotNetBuildSettings {
      OutputDirectory = ExtensionAssembliesDirectory,
      Configuration = configuration,
      MSBuildSettings = new DotNetMSBuildSettings {
         AssemblyVersion = version
      }
   }));
   // .DoesForEach<DirectoryPath>(GetDirectories($"{ExtensionAssembliesDirectory}/*"), dir => {
   //    DeleteDirectory(dir, new DeleteDirectorySettings { Recursive = true });
   // });

Task("vsix").Does(() => {
   Information("Hello Cake!");
});


RunTarget(target);