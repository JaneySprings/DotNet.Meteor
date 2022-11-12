using System.IO;
using System.Linq;

namespace DotNet.Mobile.Debug;

public static class Extensions {
    private static readonly string[] MonoFileExtensions = new string[] {
        ".cs",
        ".csx",
        ".cake",
        ".fs",
        ".fsi",
        ".ml",
        ".mli",
        ".fsx",
        ".fsscript",
        ".hx"
    };
    public static bool HasMonoExtension(this string path) {
        var extension = Path.GetExtension(path);

        if (extension == null)
            return false;

        return MonoFileExtensions.Contains(extension);
    }
}
