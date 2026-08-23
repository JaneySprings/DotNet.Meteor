using System.Text;
using System.Xml.Linq;

namespace DotNet.Meteor.Tests;

public abstract class TestFixture {
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
}