using System.IO;
using System.Text.RegularExpressions;

namespace DotNet.Meteor.Apple {
    public class PropertyExtractor {
        private string content;

        public PropertyExtractor(string plist) {
            this.content = File.ReadAllText(plist);
        }

        public string Extract(string key, string valueType = "string") {
            string pattern = $@"<key>{key}</key>.*<{valueType}>(?<val>.+?)</{valueType}>";
            var regex = new Regex(pattern);
            var match = regex.Match(this.content);

            if (!match.Success)
                return null;

            return match.Groups["val"]?.Value;
        }

        public bool ExtractBoolean(string key) {
            string pattern = $@"<key>{key}</key>.*<(?<val>\S+?)/>";
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