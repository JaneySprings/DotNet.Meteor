using System.Diagnostics;

namespace DotNet.Meteor.Shared {

    public static class ProcessExtensions {
        private const int ExitTimeout = 5000;
    
        public static void Terminate(this Process process) {
            if (!process.HasExited) {
                process.Kill();
                process.WaitForExit(ExitTimeout);
            }
            process.Close();
        }
    }
}