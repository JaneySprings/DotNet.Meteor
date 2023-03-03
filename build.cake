#addin nuget:?package=Cake.FileHelpers&version=6.1.2
#addin nuget:?package=Cake.VsCode&version=0.11.1
#load "env.cake"

using _Path = System.IO.Path;

var target = Argument("target", "vsix");
var version = Argument("release-version", "");
var configuration = Argument("configuration", "debug");

///////////////////////////////////////////////////////////////////////////////
// COMMON
///////////////////////////////////////////////////////////////////////////////

Setup(context => {
   if (string.IsNullOrEmpty(version)) 
      version = DateTime.Now.ToString($"yy.{DateTime.Now.DayOfYear}");
   Information("Building DotNet.Meteor {0} ({1})", version, configuration);
});

Task("prepare")
   .Does(() => {
      CleanDirectory(ArtifactsDirectory);
      CleanDirectory(ExtensionStagingDirectory);
      CleanDirectories(_Path.Combine(RootDirectory, "**", "bin"));
      CleanDirectories(_Path.Combine(RootDirectory, "**", "obj"));
   })
   .DoesForEach<FilePath>(GetFiles(_Path.Combine(RootDirectory, "*.json")), file => {
      var regex = @"^\s\s(""version"":\s+)("".+"")(,)";
      var options = System.Text.RegularExpressions.RegexOptions.Multiline;
      ReplaceRegexInFiles(file.ToString(), regex, $"  $1\"{version}.0\"$3", options);
   });

///////////////////////////////////////////////////////////////////////////////
// DOTNET
///////////////////////////////////////////////////////////////////////////////

Task("build-debugger")
   .DoesForEach<FilePath>(GetFiles(_Path.Combine(MonoDebuggerDirectory, "**", "*.csproj")), file => {
      var roslynRegex = @"(<NuGetVersionRoslyn\s+Condition=""\$\(NuGetVersionRoslyn\)\s*==\s*''"")(>.+<)(/NuGetVersionRoslyn>)";
      var targetRegex = @"(<TargetFrameworks>)(.+)(</TargetFrameworks>)";
      ReplaceRegexInFiles(file.ToString(), roslynRegex, $"$1>{NuGetVersionRoslyn}<$3");
      ReplaceRegexInFiles(file.ToString(), targetRegex, $"$1{TargetFrameworkVersion}$3");
   })
   .Does(() => DotNetBuild(MeteorDebugProjectPath, new DotNetBuildSettings {
      MSBuildSettings = new DotNetMSBuildSettings { AssemblyVersion = version },
      OutputDirectory = ExtensionAssembliesDirectory,
      Configuration = configuration,
   }));

Task("build-tests")
   .ContinueOnError()
   .Does(() => DotNetTest(MeteorTestsProjectPath, new DotNetTestSettings {  
      Configuration = configuration,
      Verbosity = DotNetVerbosity.Quiet,
      ResultsDirectory = ArtifactsDirectory,
      Loggers = new[] { "trx" }
   }));

Task("clean-debugger")
   .WithCriteria(configuration.Equals("release"))
   .Does(() => {
      DeleteFiles(GetFiles(
         _Path.Combine(ExtensionAssembliesDirectory, 
         _Path.GetFileNameWithoutExtension(MeteorDebugProjectPath))
      ));
      DeleteFiles(GetFiles(_Path.Combine(ExtensionAssembliesDirectory, "*.pdb")));
      DeleteFiles(GetFiles(_Path.Combine(ExtensionAssembliesDirectory, "*.deps.json")));
      DeleteFiles(GetFiles(_Path.Combine(ExtensionAssembliesDirectory, "*.xml")));
   });

///////////////////////////////////////////////////////////////////////////////
// TYPESCRIPT
///////////////////////////////////////////////////////////////////////////////

Task("manifest").Does(() => {
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
});

Task("vsix")
   .IsDependentOn("prepare")
   .IsDependentOn("build-debugger")
   .IsDependentOn("clean-debugger")
   .IsDependentOn("manifest")
   .Does(() => VscePackage(new VscePackageSettings {
      OutputFilePath = _Path.Combine(ArtifactsDirectory, $"DotNet.Meteor.{version}.0.vsix"),
      WorkingDirectory = RootDirectory
   }));


RunTarget(target);