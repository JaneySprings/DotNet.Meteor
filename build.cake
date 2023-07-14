#addin nuget:?package=Cake.FileHelpers&version=6.1.2
#addin nuget:?package=Cake.VsCode&version=0.11.1

using _Path = System.IO.Path;

public string RootDirectory => MakeAbsolute(Directory("./")).ToString();

public string ArtifactsDirectory => _Path.Combine(RootDirectory, "artifacts");
public string ExtensionStagingDirectory => _Path.Combine(RootDirectory, "extension");
public string ExtensionBinariesDirectory => _Path.Combine(ExtensionStagingDirectory, "bin");

public string MeteorWorkspaceProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Workspace", "DotNet.Meteor.Workspace.csproj");
public string MeteorDebugProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Debug", "DotNet.Meteor.Debug.csproj");
public string MeteorTestsProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Tests", "DotNet.Meteor.Tests.csproj");
public string MeteorPluginProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.HotReload.Plugin", "DotNet.Meteor.HotReload.Plugin.csproj");

var target = Argument("target", "vsix");
var version = Argument("release-version", "23.2.0");
var configuration = Argument("configuration", "debug");

///////////////////////////////////////////////////////////////////////////////
// COMMON
///////////////////////////////////////////////////////////////////////////////

Task("clean").Does(() => {
   CleanDirectory(ArtifactsDirectory);
   CleanDirectory(ExtensionStagingDirectory);
   CleanDirectories(_Path.Combine(RootDirectory, "**", "bin"));
   CleanDirectories(_Path.Combine(RootDirectory, "**", "obj"));
});

///////////////////////////////////////////////////////////////////////////////
// DOTNET
///////////////////////////////////////////////////////////////////////////////

Task("debugger")
   .Does(() => {
      DotNetBuild(MeteorDebugProjectPath, new DotNetBuildSettings {
         MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
         Configuration = configuration,
      });
      DotNetBuild(MeteorWorkspaceProjectPath, new DotNetBuildSettings {
         MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
         Configuration = configuration,
      });
      DeleteFiles(GetFiles(_Path.Combine(ExtensionBinariesDirectory, "**", "*.xml")));
      DeleteDirectories(GetDirectories(
         _Path.Combine(ExtensionBinariesDirectory, "**", "runtimes", "android-*")), 
         new DeleteDirectorySettings { Recursive = true }
      );
   });

Task("plugin")
   .Does(() => DotNetPack(MeteorPluginProjectPath, new DotNetPackSettings {
      Configuration = configuration,
      MSBuildSettings = new DotNetMSBuildSettings { 
         AssemblyVersion = version, 
         Version = version
      },
   }));

Task("test")
   .Does(() => DotNetTest(MeteorTestsProjectPath, new DotNetTestSettings {  
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
   .IsDependentOn("debugger")
   .DoesForEach<FilePath>(GetFiles(_Path.Combine(RootDirectory, "*.json")), file => {
      var regex = @"^\s\s(""version"":\s+)("".+"")(,)";
      var options = System.Text.RegularExpressions.RegexOptions.Multiline;
      ReplaceRegexInFiles(file.ToString(), regex, $"  $1\"{version}\"$3", options);
   })
   .Does(() => {
      var options = System.Text.RegularExpressions.RegexOptions.Multiline;
      var packageFile = _Path.Combine(RootDirectory, "package.json");
      var includes = FindRegexMatchesInFile(packageFile, @"""include"": ""(.+)""", options);
      foreach (string include in includes) {
         var includePath = include.Split(':')[1].Trim().Replace("\"", string.Empty);
         var includeFile = _Path.Combine(RootDirectory, includePath);
         var includeContent = FileReadText(includeFile);
         includeContent = includeContent.Substring(8, includeContent.Length - 12);
         ReplaceTextInFiles(packageFile, include, includeContent);
      }
   })
   .Does(() => VscePackage(new VscePackageSettings {
      OutputFilePath = _Path.Combine(ArtifactsDirectory, $"DotNet.Meteor.{version}-{configuration}.vsix"),
      WorkingDirectory = RootDirectory
   }));


RunTarget(target);