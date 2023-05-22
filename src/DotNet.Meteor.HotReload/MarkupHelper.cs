using System.Text;
using System.Xml.Linq;

namespace DotNet.Meteor.HotReload;

public static class MarkupHelper {
    public static string? GetClassDefinition(StringBuilder xamlContent) {
        var xaml = XDocument.Parse(xamlContent.ToString());
        var xClassAttribute = xaml.Root?.Attributes().FirstOrDefault(a => a.Name.LocalName == "Class");
        return xClassAttribute?.Value;
    }

    public static void RemoveReferenceNames(StringBuilder xamlContent) {
        var xaml = XDocument.Parse(xamlContent.ToString());
        var xElements = xaml.Descendants().ToList();
        foreach (var xElement in xElements) {
            var xNameAttribute = xElement.Attributes().FirstOrDefault(a => a.Name.LocalName == "Name");
            if (xNameAttribute != null) {
                xNameAttribute.Remove();
            }
        }

        xamlContent.Clear();
        xamlContent.Append(xaml.ToString());
    }
}