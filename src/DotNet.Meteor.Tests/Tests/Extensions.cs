namespace DotNet.Meteor.Tests;

public static class Extensions {
    public static string Join(this IEnumerable<string> str, string separator = ";") {
        return string.Join(separator, str);
    }
}