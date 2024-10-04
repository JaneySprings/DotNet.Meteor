using Xunit;

namespace DotNet.Meteor.Tests;

public static class Assertion {
    public static void CollectionsAreEqual<TValue>(IEnumerable<TValue> expected, IEnumerable<TValue> actual) {
        Assert.Equal(expected.Count(), actual.Count());
        foreach (var item in expected)
            Assert.Contains(item, actual);
        foreach (var item in actual)
            Assert.Contains(item, expected);
    }
}