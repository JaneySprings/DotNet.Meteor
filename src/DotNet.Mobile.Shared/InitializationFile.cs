using System.Linq;
using System.IO;

namespace DotNet.Mobile.Shared {
    public class IniFile {
        private string[] content;

        public IniFile(string path) {
            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find file", path);
            content = File.ReadAllLines(path);
        }

        public string GetField(string name) {
            var record = content.FirstOrDefault(it => it.StartsWith(name));

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