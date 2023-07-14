
namespace DotNet.Meteor.Workspace.Utilities;

public class PropertyExtractor {
    private string[] content;

    public PropertyExtractor(string plist) {
        this.content = File.ReadAllLines(plist);
    }

    public string? Extract(string key, string valueType = "string") {
        for (int i = 0; i < content.Length; i++) {
            if (!content[i].Contains($"<key>{key}</key>"))
                continue;

            if (i >= content.Length - 1)
                return null;

            string value = content[i + 1].Trim();
            if (!value.Contains($"<{valueType}>"))
                return null;

            return value.Replace($"<{valueType}>", "").Replace($"</{valueType}>", "");
        }

        return null;
    }

    public bool ExtractBoolean(string key) {
        for (int i = 0; i < content.Length; i++) {
            if (!content[i].Contains($"<key>{key}</key>"))
                continue;

            if (i >= content.Length - 1)
                return false;

            string value = content[i + 1].Trim();
            return value.Replace("</", "").Replace(">", "").Equals("true", System.StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public void Free() {
        this.content = Array.Empty<string>();
    }
}