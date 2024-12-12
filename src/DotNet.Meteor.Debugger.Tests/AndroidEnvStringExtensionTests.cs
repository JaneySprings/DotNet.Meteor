using DotNet.Meteor.Debugger.Extensions;
using NUnit.Framework;

namespace DotNet.Meteor.Debugger.Tests;

public class AndroidEnvStringExtensionTests: TestFixture {
    const int AndroidMaxEnvLength = 49;

    [Test]
    public void AndroidEnvExtensionTest() {
        var env = new Dictionary<string, string> {{ "MY_VAR", "123456" }};
        var envString = env.ToAndroidEnvString();

        Assert.That(envString, Is.EqualTo("'MY_VAR=123456'"));
    }
    [Test]
    public void AndroidEnvExtensionTest2() {
        var env = new Dictionary<string, string> {{ "MY_VAR", "123456" }, { "MY_VAR2", "54321" }};
        var envString = env.ToAndroidEnvString();

        Assert.That(envString, Is.EqualTo("'MY_VAR=123456|MY_VAR2=54321'"));
    }
    [Test]
    public void AndroidEnvExtensionTest3() {
        var env = new Dictionary<string, string> {{ "MY_VAR", "123456" }, { "MY_VAR2", "54321" }, { "MY_VAR3", "6543210" }};
        var envString = env.ToAndroidEnvString();

        Assert.That(envString, Is.EqualTo("'MY_VAR=123456|MY_VAR2=54321|MY_VAR3=6543210'"));
    }
    [Test]
    public void AndroidEnvExtensionTest4() {
        var env = new Dictionary<string, string> {{ "VAR", new string('a', AndroidMaxEnvLength - 4) }};
        var envString = env.ToAndroidEnvString().Trim('\'');

        Assert.That(envString, Has.Length.EqualTo(AndroidMaxEnvLength));
        Assert.That(envString, Is.EqualTo($"VAR={new string('a', AndroidMaxEnvLength - 4)}"));
    }
    [Test]
    public void AndroidEnvExtensionTest5() {
        var env = new Dictionary<string, string> {{ "VAR", new string('a', AndroidMaxEnvLength) }};
        var envString = env.ToAndroidEnvString().Trim('\'');

        Assert.That(envString, Has.Length.EqualTo(AndroidMaxEnvLength));
        Assert.That(envString, Is.EqualTo($"VAR={new string('a', AndroidMaxEnvLength - 4)}"));
    }
    [Test]
    public void AndroidEnvExtensionTest6() {
        var env = new Dictionary<string, string> {{ "VAR", "123456"}, { "VAR2", new string('a', AndroidMaxEnvLength) }};
        var envString = env.ToAndroidEnvString().Trim('\'');

        Assert.That(envString, Has.Length.EqualTo(AndroidMaxEnvLength));
        Assert.That(envString, Is.EqualTo($"VAR=123456|VAR2={new string('a', AndroidMaxEnvLength - 16)}"));
    }
    [Test]
    public void AndroidEnvExtensionTest7() {
        var env = new Dictionary<string, string> {{ "VAR", "123456"}, { "VARIABLE", "VALUE"}, { "VAR2", new string('a', 100) }};
        var envString = env.ToAndroidEnvString().Trim('\'');

        Assert.That(envString, Has.Length.EqualTo(AndroidMaxEnvLength));
        Assert.That(envString, Is.EqualTo($"VAR=123456|VARIABLE=VALUE|VAR2={new string('a', AndroidMaxEnvLength - 31)}"));
    }
    [Test]
    public void AndroidEnvExtensionTest8() {
        var env = new Dictionary<string, string> {{ "VAR", "123456789101112131415161718192021222324252627282930"}};
        var envString = env.ToAndroidEnvString().Trim('\'');

        Assert.That(envString, Has.Length.EqualTo(AndroidMaxEnvLength));
        Assert.That(envString, Is.EqualTo($"VAR=789101112131415161718192021222324252627282930"));
    }
    [Test]
    public void AndroidEnvExtensionTest9() {
        var env = new Dictionary<string, string> {{ "VAR", "123456"}, { "VARIABLE", "VALUE"}, { "VAR2", "12345678910111213141516171819202122232425" }};
        var envString = env.ToAndroidEnvString().Trim('\'');

        Assert.That(envString, Has.Length.EqualTo(AndroidMaxEnvLength));
        Assert.That(envString, Is.EqualTo($"VAR=123456|VARIABLE=VALUE|VAR2=171819202122232425"));
    }
}