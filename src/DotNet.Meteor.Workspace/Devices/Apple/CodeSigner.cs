using DotNet.Debugging.Common.Interop;

namespace DotNet.Meteor.Workspace.Devices;

public static class CodeSigner {
    public static bool SignFiles(IEnumerable<string> filePaths) {
        var result = true;
        foreach (var filePath in filePaths)
            result &= SignFile(filePath);

        return result;
    }
    public static bool SignFile(string filePath) {
        if (!RuntimeInfo.IsMacOS)
            throw new NotSupportedException();

        var result = new ProcessRunner("codesign", new ProcessArgumentBuilder()
            .Append("--force", "--sign", "-")
            .AppendQuoted(filePath))
            .WaitForExit();

        return result.Success;
    }
}