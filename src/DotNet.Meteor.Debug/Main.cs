using System;

namespace DotNet.Meteor.Debug;

public class Program {
    public static void Main(string[] args) {
        Logging.LogConfig.InitializeLog();
        var debugSession = new DebugSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        debugSession.Start();
    }
}