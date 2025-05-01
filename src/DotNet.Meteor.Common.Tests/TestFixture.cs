using System.Reflection;
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;

namespace DotNet.Meteor.Common.Tests;

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

    protected static List<string> FindAllXNames(StringBuilder stringBuilder) {
        var names = new List<string>();
        var xaml = XDocument.Parse(stringBuilder.ToString());
        var xElements = xaml.Descendants().ToList();
        foreach (var xElement in xElements) {
            var attribute = xElement.Attributes().FirstOrDefault(a => a.Name.LocalName == "Name" 
                && a.Name.NamespaceName == "http://schemas.microsoft.com/winfx/2009/xaml");
            if (attribute != null)
                names.Add(attribute.Value);
        }

        return names;
    }
    protected static void CollectionsAreEqual<TValue>(IEnumerable<TValue> expected, IEnumerable<TValue> actual) {
        Assert.That(actual.Count(), Is.EqualTo(expected.Count()));
        foreach (var item in expected)
            Assert.That(actual, Does.Contain(item));
        foreach (var item in actual)
            Assert.That(expected, Does.Contain(item));
    }
}