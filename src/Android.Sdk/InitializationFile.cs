using System.Linq;
using System.IO;

namespace Android.Sdk {
    public class IniFile {
        private string[] content;

        public static IniFile FromPath(string path) {
            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find file", path);
            return new IniFile(path);
        }
        private IniFile(string path) {
            content = File.ReadAllLines(path);
        }

        public string GetField(string name) {
            var record = content.First(it => it.StartsWith(name));

            if (record == null)
                return null;

            var parts = record.Split('=');

            if (parts.Length < 2)
                return null;

            return parts[1];
        }

        public void Free() {
            content = null;
        }
    }
}