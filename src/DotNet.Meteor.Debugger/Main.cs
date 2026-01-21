namespace DotNet.Meteor.Debugger;

public class Program {
    private static void Main(string[] args) {
        Console.SetIn(TextReader.Null);
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("METEOR_DEBUG_WAIT"))) {
            while (!System.Diagnostics.Debugger.IsAttached) {
                Thread.Sleep(500);
            }
        }

        var debugSession = new DebugSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        debugSession.Start();
    }
}
