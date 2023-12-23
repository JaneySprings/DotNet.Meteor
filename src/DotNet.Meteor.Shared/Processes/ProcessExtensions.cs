using System.Diagnostics;

namespace DotNet.Meteor.Shared {

    public static class ProcessExtensions {
        public static void Terminate(this Process process) {
            if (!process.HasExited) {
                process.Kill();
                process.WaitForExit();
            }
            process.Close();
        }
    }
}