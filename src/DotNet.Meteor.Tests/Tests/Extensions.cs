using System.Linq;

namespace DotNet.Meteor.Tests;

public static class Extensions {
    public static string Join(this IEnumerable<string> str, string separator = ";") {
        return string.Join(separator, str);
    }

    public static string RemoveEmptyEntries(this string property) {
        return property.Split(';').Where(it => !string.IsNullOrEmpty(it)).Join();
    }
}