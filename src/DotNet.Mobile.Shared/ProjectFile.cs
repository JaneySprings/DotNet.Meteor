using System.Text.RegularExpressions;
using System.IO;

namespace DotNet.Mobile.Shared {
    public class ProjectFile {
        private string content;

        public static ProjectFile FromPath(string path) {
            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find file", path);
            return new ProjectFile(path);
        }
        private ProjectFile(string path) {
            content = File.ReadAllText(path);
        }

        public string ValueFromProperty(string name, string defaultValue = null) {
            var regex = new Regex($@"<{name}>(.*?)<\/{name}>", RegexOptions.Singleline);
            var match = regex.Match(content);

            if (!match.Success)
                return defaultValue;

            return match.Groups[1].Value;
        }

        public void Free() {
            content = null;
        }
    }
}