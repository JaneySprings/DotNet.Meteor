using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DotNet.Meteor.HotReload.Extensions;

public static class MarkupExtensions {
    private const string xReferenceNameRegex = @":Reference\s+([a-zA-Z_][a-zA-Z0-9_]*)";

    public static string? GetClassDefinition(StringBuilder xamlContent) {
        var xaml = XDocument.Parse(xamlContent.ToString());
        var xClassAttribute = xaml.Root?.Attributes().FirstOrDefault(a => a.Name.LocalName == "Class");
        return xClassAttribute?.Value;
    }

    public static Dictionary<string, string> TransformReferenceNames(StringBuilder xamlContent) {
        var transformations = new Dictionary<string, string>();
        var xaml = XDocument.Parse(xamlContent.ToString());
        var xElements = xaml.Descendants().ToList();
        foreach (var xElement in xElements) {
            var xNameAttribute = xElement.Attributes().FirstOrDefault(a => a.Name.LocalName == "Name");
            if (xNameAttribute != null) {
                var oldName = xNameAttribute.Value;
                var newName = oldName + $"_{DateTime.UtcNow.Ticks.ToString("X")}";
                xNameAttribute.Value = newName;
                transformations.Add(oldName, newName);

                foreach (var xElement2 in xElements) {
                    var xAttributes = xElement2.Attributes().ToList();
                    foreach (var xAttribute in xAttributes) {
                        var match = Regex.Match(xAttribute.Value, xReferenceNameRegex);
                        if (!match.Success || match.Groups.Count < 2) 
                            continue;
        
                        var referenceName = match.Groups[1].Value;
                        if (referenceName == oldName)
                            xAttribute.Value = xAttribute.Value.ReplaceFirst(oldName, newName);

                        /* TODO: Handle other scenarios" */
                    }
                }

            }
        }

        xamlContent.Clear();
        xamlContent.Append(xaml.ToString());
        return transformations;
    }

    private static string ReplaceFirst(this string text, string oldValue, string newValue) {
      int pos = text.IndexOf(oldValue);
      if (pos < 0)
        return text;

      return text.Substring(0, pos) + newValue + text.Substring(pos + oldValue.Length);
    }
}