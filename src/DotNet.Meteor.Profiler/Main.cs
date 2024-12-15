namespace DotNet.Meteor.Profiler;

public class Program {
    private static void Main(string[] args) {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("METEOR_DEBUG_WAIT"))) {
            while (!System.Diagnostics.Debugger.IsAttached) {
                Thread.Sleep(500);
            }
        }

        var debugSession = new ProfileSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        debugSession.Start();
    }
}
