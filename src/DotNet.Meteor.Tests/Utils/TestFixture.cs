using Xunit;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace DotNet.Meteor.Tests;

[Collection("Sequential")]
public abstract class TestFixture {
    protected readonly string MockDataDirectory;
    protected readonly string ProjectName = "TestApp";

    protected TestFixture() {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        MockDataDirectory = Path.GetFullPath(Path.Combine(assemblyLocation, "MockData"));
    }

    protected string CreateMockProject(string csprojContent) {
        string csprojPath = Path.Combine(MockDataDirectory, ProjectName, $"{ProjectName}.csproj");
        Directory.CreateDirectory(Path.Combine(MockDataDirectory, ProjectName));

        using var writer = File.CreateText(csprojPath);
        writer.WriteLine(csprojContent);

        return csprojPath;
    }
    protected string CreateCommonProps(string directory, string content) {
        if (!Directory.Exists(Path.Combine(MockDataDirectory, directory)))
            Directory.CreateDirectory(Path.Combine(MockDataDirectory, directory));
        string propsPath = Path.Combine(MockDataDirectory, directory, "Directory.Build.props");

        using var writer = File.CreateText(propsPath);
        writer.WriteLine(content);

        return propsPath;
    }
    protected string CreateCustomProps(string filePath, string content) {
        if (!Directory.Exists(Path.Combine(MockDataDirectory, Path.GetDirectoryName(filePath) ?? "")))
            Directory.CreateDirectory(Path.Combine(MockDataDirectory, Path.GetDirectoryName(filePath) ?? ""));
        string propsPath = Path.Combine(MockDataDirectory, filePath);

        using var writer = File.CreateText(propsPath);
        writer.WriteLine(content);

        return propsPath;
    }
    protected string CreateOutputAssembly(string target, string framework, string? runtime, string name, bool includeWinDir) {
        string projectDir = Path.Combine(MockDataDirectory, ProjectName);
        string assemblyDir = runtime == null
            ? Path.Combine(projectDir, "bin", target, framework)
            : Path.Combine(projectDir, "bin", target, framework, runtime);

        if (includeWinDir)
            assemblyDir = Path.Combine(assemblyDir, "win-x64");

        string assemblyPath = Path.Combine(assemblyDir, name);

        Directory.CreateDirectory(assemblyDir);
        using var writer = File.CreateText(assemblyPath);
        writer.WriteLine("bin-data...");

        return assemblyPath;
    }
    protected string CreateOutputBundle(string target, string framework, string runtime, string name) {
        string projectDir = Path.Combine(MockDataDirectory, ProjectName);
        string assemblyDir = runtime == null
            ? Path.Combine(projectDir, "bin", target, framework, name)
            : Path.Combine(projectDir, "bin", target, framework, runtime, name);
        string assemblyPath = Path.Combine(assemblyDir, "mock");

        Directory.CreateDirectory(assemblyDir);
        using var writer = File.CreateText(assemblyPath);
        writer.WriteLine("bin-data...");

        return assemblyDir;
    }

    protected string CreateOutputAssemblyFile(string path, string name) {
        string assemblyPath = Path.Combine(path, name);
        using var writer = File.CreateText(assemblyPath);
        writer.WriteLine("bin-data...");

        return assemblyPath;
    }
    protected void DeleteMockData() {
       Directory.Delete(MockDataDirectory, true);
    }
    protected List<string> FindAllXNames(StringBuilder stringBuilder) {
        var names = new List<string>();
        var xaml = XDocument.Parse(stringBuilder.ToString());
        var xElements = xaml.Descendants().ToList();
        foreach (var xElement in xElements) {
            var attribute = xElement.Attributes().FirstOrDefault(a => a.Name.LocalName == "Name");
            if (attribute != null)
                names.Add(attribute.Value);
        }

        return names;
    }
}