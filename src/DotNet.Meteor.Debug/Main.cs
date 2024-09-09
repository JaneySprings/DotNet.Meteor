namespace DotNet.Meteor.Debug;

public class Program {
    private static void Main(string[] args) {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("METEOR_DEBUG_WAIT"))) {
            while (!System.Diagnostics.Debugger.IsAttached) {
                Thread.Sleep(500);
            }
        }

        var debugSession = new DebugSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        debugSession.Start();
    }
}
