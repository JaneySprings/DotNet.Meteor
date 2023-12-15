using System.Text;
using System.Xml.Linq;

namespace DotNet.Meteor.HotReload;

public static class MarkupHelper {
    public static string? GetClassDefinition(StringBuilder xamlContent) {
        var xaml = XDocument.Parse(xamlContent.ToString());
        var xClassAttribute = xaml.Root?.Attributes().FirstOrDefault(a => a.Name.LocalName == "Class");
        return xClassAttribute?.Value;
    }

    public static void ModifyReferenceNames(StringBuilder xamlContent) {
        var xaml = XDocument.Parse(xamlContent.ToString());
        var xElements = xaml.Descendants().ToList();
        foreach (var xElement in xElements) {
            var xNameAttribute = xElement.Attributes().FirstOrDefault(a => a.Name.LocalName == "Name");
            if (xNameAttribute != null) {
                var oldName = xNameAttribute.Value;
                var newName = oldName + $"_{DateTime.UtcNow.Ticks.ToString("X")}";
                xNameAttribute.Value = newName;

                foreach (var xElement2 in xElements) {
                    var xAttributes = xElement2.Attributes().ToList();
                    foreach (var xAttribute in xAttributes) {
                        if (xAttribute.Value.Contains($"x:Reference {oldName}"))
                            xAttribute.Value = xAttribute.Value.Replace($"x:Reference {oldName}", $"x:Reference {newName}");
                        /* TODO: Handle other scenarios" */
                    }
                }

            }
        }

        xamlContent.Clear();
        xamlContent.Append(xaml.ToString());
    }
}