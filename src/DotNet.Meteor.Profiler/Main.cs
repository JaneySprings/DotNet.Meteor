namespace DotNet.Meteor.Profiler;

public class Program {
    private static void Main(string[] args) {
        var profileSession = new ProfileSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        profileSession.Start();
    }
}
