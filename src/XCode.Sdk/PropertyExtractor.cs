using System.IO;
using System.Text.RegularExpressions;

namespace XCode.Sdk {
    public class PropertyExtractor {
        private string content;

        public static PropertyExtractor FromFile(string plist) {
            if (!File.Exists(plist))
                throw new FileNotFoundException("Could not find plist file", plist);

            return new PropertyExtractor(plist);
        }

        private PropertyExtractor(string plist) {
            this.content = File.ReadAllText(plist);
        }


        public string Extract(string key, string valueType = "string") {
            string pattern = $@"<key>{key}</key>\n+\t+<{valueType}>(?<val>.+)</{valueType}>";
            var regex = new Regex(pattern);
            var match = regex.Match(this.content);

            if (!match.Success)
                return null;

            return match.Groups["val"]?.Value;
        }

        public bool ExtractBoolean(string key) {
            string pattern = $@"<key>{key}</key>\n+\t+<(?<val>\S+)/>";
            var regex = new Regex(pattern);
            var match = regex.Match(this.content);

            if (!match.Success)
                return false;

            string value = match.Groups["val"]?.Value;
            return value?.Equals("true") ?? false;
        }

        public void Free() {
            this.content = null;
        }
    }
}