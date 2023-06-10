#addin nuget:?package=Cake.FileHelpers&version=6.1.2
#addin nuget:?package=Cake.VsCode&version=0.11.1

using _Path = System.IO.Path;

public string RootDirectory => MakeAbsolute(Directory("./")).ToString();

public string ArtifactsDirectory => _Path.Combine(RootDirectory, "artifacts");
public string ExtensionStagingDirectory => _Path.Combine(RootDirectory, "extension");
public string ExtensionAssembliesDirectory => _Path.Combine(ExtensionStagingDirectory, "bin");

public string MeteorMainProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.CommandLine", "DotNet.Meteor.CommandLine.csproj");
public string MeteorTestsProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.Tests", "DotNet.Meteor.Tests.csproj");
public string MeteorPluginProjectPath => _Path.Combine(RootDirectory, "src", "DotNet.Meteor.HotReload.Plugin", "DotNet.Meteor.HotReload.Plugin.csproj");

var target = Argument("target", "vsix");
var version = Argument("release-version", "");
var configuration = Argument("configuration", "debug");

///////////////////////////////////////////////////////////////////////////////
// COMMON
///////////////////////////////////////////////////////////////////////////////

Setup(context => {
   if (string.IsNullOrEmpty(version)) {
      var major = DateTime.Now.ToString("yy");
      var minor = DateTime.Now.Month < 7 ? "1" : "2";
      var build = DateTime.Now.DayOfYear;
      version = $"{major}.{minor}.{major}{build:000}";
   }
   Information("Building DotNet.Meteor {0}-{1}", version, configuration);
});

Task("clean").Does(() => {
   CleanDirectory(ArtifactsDirectory);
   CleanDirectory(ExtensionStagingDirectory);
   CleanDirectories(_Path.Combine(RootDirectory, "**", "bin"));
   CleanDirectories(_Path.Combine(RootDirectory, "**", "obj"));
});

///////////////////////////////////////////////////////////////////////////////
// DOTNET
///////////////////////////////////////////////////////////////////////////////

Task("build-debugger")
   .Does(() => {
      DotNetBuild(MeteorMainProjectPath, new DotNetBuildSettings {
         MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
         OutputDirectory = ExtensionAssembliesDirectory,
         Configuration = configuration,
      });
      DeleteFiles(GetFiles(_Path.Combine(ExtensionAssembliesDirectory, "*.deps.json")));
      DeleteFiles(GetFiles(_Path.Combine(ExtensionAssembliesDirectory, "*.xml")));
   });

Task("build-plugin")
   .Does(() => DotNetPack(MeteorPluginProjectPath, new DotNetPackSettings {
      OutputDirectory = ArtifactsDirectory,
      Configuration = configuration,
      MSBuildSettings = new DotNetMSBuildSettings { 
         AssemblyVersion = version, 
         Version = version
      },
   }));

Task("build-tests")
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
   .IsDependentOn("build-debugger")
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