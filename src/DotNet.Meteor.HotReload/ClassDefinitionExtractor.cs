using System.Xml.Linq;

namespace DotNet.Meteor.HotReload;

public static class ClassDefinitionExtractor {
    public static string? GetClassDefinition(string xamlContent) {
        var xaml = XDocument.Parse(xamlContent);
        var xClassAttribute = xaml.Root?.Attributes().FirstOrDefault(a => a.Name.LocalName == "Class");
        return xClassAttribute?.Value;
    }
}