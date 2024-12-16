using System.Reflection;
using DotNet.Meteor.Common;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotNet.Meteor.Debugger.Tests;

public class LaunchConfigurationTests : TestFixture {
    private readonly JToken TestProjectJToken = new JObject {
        { "name", "TestProject" },
        { "path", $"{Root}Documents{APS}TestProject{APS}TestProject.csproj" },
        { "frameworks", new JArray { "net8.0-android", "net8.0-ios" } },
        { "configurations", new JArray { "Debug", "Release" } }
    };
    private readonly JToken TestDeviceJToken = new JObject {
        { "name", "TestDevice" },
        { "serial", "1234567890" },
        { "platform", Platforms.Android },
        { "is_emulator", false },
        { "is_running", true },
        { "is_mobile", true }
    };

    private static char APS => Path.DirectorySeparatorChar;
    private static char IPS {
        get {
            if (RuntimeSystem.IsWindows)
                return '/';
            return '\\';
        }
    }
    private static string Root {
        get {
            if (RuntimeSystem.IsWindows)
                return $"C:{APS}";
            return "/";
        }
    }

    [Test]
    public void LoadLaunchConfigurationTest() {
        var properties = new Dictionary<string, JToken>();
        Assert.Throws<KeyNotFoundException>(() => _ = new LaunchConfiguration(properties));

        properties.Add("project", TestProjectJToken);
        Assert.Throws<KeyNotFoundException>(() => _ = new LaunchConfiguration(properties));

        properties.Add("device", TestDeviceJToken);
        Assert.Throws<ProtocolException>(() => _ = new LaunchConfiguration(properties));

        properties.Add("program", Assembly.GetExecutingAssembly().Location);
        var configuration = new LaunchConfiguration(properties);

        Assert.Multiple(() => {
            Assert.That(configuration.Project, Is.Not.Null);
            Assert.That(configuration.Device, Is.Not.Null);
            Assert.That(configuration.ProgramPath, Is.Not.Null);
            Assert.That(configuration.AssetsPath, Is.Null.Or.Empty);

            Assert.That(configuration.UninstallApp, Is.False);
            Assert.That(configuration.DebugPort, Is.Not.Zero);
            Assert.That(configuration.ReloadHostPort, Is.Zero);
            Assert.That(configuration.TransportId, Is.Null.Or.Empty);
            Assert.That(configuration.DebuggerSessionOptions, Is.Not.Null);
            Assert.That(configuration.EnvironmentVariables, Is.Not.Null);

            Assert.That(Directory.Exists(configuration.GetAssembliesPath()), Is.True, "Assemblies path does not exist");
            Assert.That(configuration.GetApplicationName(), Is.Not.Null.Or.Empty);
        });
    }
    [Test]
    public void LaunchConfigurationAgentTest() {
        var properties = new Dictionary<string, JToken>();
        properties.Add("project", TestProjectJToken);
        properties.Add("device", TestDeviceJToken);
        properties.Add("program", Assembly.GetExecutingAssembly().Location);

        var configuration = new LaunchConfiguration(properties);
        Assert.That(configuration.GetLaunchAgent(), Is.InstanceOf<DebugLaunchAgent>());

        properties["skipDebug"] = true;
        configuration = new LaunchConfiguration(properties);
        Assert.That(configuration.GetLaunchAgent(), Is.InstanceOf<NoDebugLaunchAgent>());

        properties["profilerMode"] = "none";
        configuration = new LaunchConfiguration(properties);
        Assert.That(configuration.GetLaunchAgent(), Is.InstanceOf<NoDebugLaunchAgent>());

        properties["profilerMode"] = "trace";
        configuration = new LaunchConfiguration(properties);
        Assert.That(configuration.GetLaunchAgent(), Is.InstanceOf<NoDebugLaunchAgent>());

        properties["profilerMode"] = "gcdump";
        configuration = new LaunchConfiguration(properties);
        Assert.That(configuration.GetLaunchAgent(), Is.InstanceOf<NoDebugLaunchAgent>());
    }
    
    [Test]
    public void StrangeMSBuildPathFormatTest() {
        var incorrectPath = $"{Root}Documents{IPS}TestProject{IPS}obj{IPS}Debug{IPS}net8.0-android{IPS}TestProject.dll";
        var correctPath = incorrectPath.Replace(IPS, APS);

        var properties = new Dictionary<string, JToken>();
        properties.Add("project", TestProjectJToken);
        properties.Add("device", TestDeviceJToken);
        properties.Add("program", incorrectPath);
        var exception = Assert.Catch<ProtocolException>(() => _ = new LaunchConfiguration(properties));
        Assert.That(exception.Message, Does.StartWith("Incorrect path to program: '"));

        var path = exception.Message.Substring(28, exception.Message.Length - 29);
        Assert.That(path, Does.Not.Contain(IPS));
        Assert.That(path, Is.EqualTo(correctPath));
    }
    [Test]
    public void StrangeMSBuildPathFormat2Test() {
        var incorrectPath = $"{Root}Documents{APS}TestProject{IPS}obj{IPS}Debug{IPS}net8.0-android{APS}TestProject.dll";
        var correctPath = incorrectPath.Replace(IPS, APS);

        var properties = new Dictionary<string, JToken>();
        properties.Add("project", TestProjectJToken);
        properties.Add("device", TestDeviceJToken);
        properties.Add("program", incorrectPath);
        var exception = Assert.Catch<ProtocolException>(() => _ = new LaunchConfiguration(properties));
        Assert.That(exception.Message, Does.StartWith("Incorrect path to program: '"));

        var path = exception.Message.Substring(28, exception.Message.Length - 29);
        Assert.That(path, Does.Not.Contain(IPS));
        Assert.That(path, Is.EqualTo(correctPath));
    }
    [Test]
    public void StrangeMSBuildPathFormat3Test() {
        var incorrectPath = $"obj{IPS}Debug{IPS}net8.0-android{APS}TestProject.dll";
        var correctPath =  $"{Root}Documents{APS}TestProject{APS}obj{APS}Debug{APS}net8.0-android{APS}TestProject.dll";

        var properties = new Dictionary<string, JToken>();
        properties.Add("project", TestProjectJToken);
        properties.Add("device", TestDeviceJToken);
        properties.Add("program", incorrectPath);
        var exception = Assert.Catch<ProtocolException>(() => _ = new LaunchConfiguration(properties));
        Assert.That(exception.Message, Does.StartWith("Incorrect path to program: '"));

        var path = exception.Message.Substring(28, exception.Message.Length - 29);
        Assert.That(path, Does.Not.Contain(IPS));
        Assert.That(path, Is.EqualTo(correctPath));
    }
    [Test]
    public void StrangeMSBuildPathFormat4Test() {
        var incorrectPath = $"obj{IPS}{IPS}Debug{IPS}net8.0-android{APS}{APS}TestProject.dll";
        var correctPath =  $"{Root}Documents{APS}TestProject{APS}obj{APS}Debug{APS}net8.0-android{APS}TestProject.dll";

        var properties = new Dictionary<string, JToken>();
        properties.Add("project", TestProjectJToken);
        properties.Add("device", TestDeviceJToken);
        properties.Add("program", incorrectPath);
        var exception = Assert.Catch<ProtocolException>(() => _ = new LaunchConfiguration(properties));
        Assert.That(exception.Message, Does.StartWith("Incorrect path to program: '"));

        var path = exception.Message.Substring(28, exception.Message.Length - 29);
        Assert.That(path, Does.Not.Contain(IPS));
        Assert.That(path, Is.EqualTo(correctPath));
    }

    [Test]
    public void StrangeMSBuildPathFormatAssetsTest() {
        var incorrectPath = $"{Root}Documents{IPS}TestProject{IPS}obj{IPS}Debug{IPS}net8.0-android";
        var correctPath = incorrectPath.Replace(IPS, APS);

        var properties = new Dictionary<string, JToken>();
        properties.Add("project", TestProjectJToken);
        properties.Add("device", TestDeviceJToken);
        properties.Add("program", Assembly.GetExecutingAssembly().Location);
        properties.Add("assets", incorrectPath);
        var configuration = new LaunchConfiguration(properties);
        Assert.That(configuration.AssetsPath, Does.Not.Contain(IPS));
        Assert.That(configuration.AssetsPath, Is.EqualTo(correctPath));
    }
    [Test]
    public void StrangeMSBuildPathFormatAssets2Test() {
        var incorrectPath = $"{Root}Documents{APS}TestProject{IPS}obj{IPS}Debug{IPS}net8.0-android";
        var correctPath = incorrectPath.Replace(IPS, APS);

        var properties = new Dictionary<string, JToken>();
        properties.Add("project", TestProjectJToken);
        properties.Add("device", TestDeviceJToken);
        properties.Add("program", Assembly.GetExecutingAssembly().Location);
        properties.Add("assets", incorrectPath);
        var configuration = new LaunchConfiguration(properties);
        Assert.That(configuration.AssetsPath, Does.Not.Contain(IPS));
        Assert.That(configuration.AssetsPath, Is.EqualTo(correctPath));
    }
    [Test]
    public void StrangeMSBuildPathFormatAssets3Test() {
        var incorrectPath = $"obj{IPS}Debug{IPS}net8.0-android{APS}";
        var correctPath =  $"{Root}Documents{APS}TestProject{APS}obj{APS}Debug{APS}net8.0-android";

        var properties = new Dictionary<string, JToken>();
        properties.Add("project", TestProjectJToken);
        properties.Add("device", TestDeviceJToken);
        properties.Add("program", Assembly.GetExecutingAssembly().Location);
        properties.Add("assets", incorrectPath);
        var configuration = new LaunchConfiguration(properties);
        Assert.That(configuration.AssetsPath, Does.Not.Contain(IPS));
        Assert.That(configuration.AssetsPath, Is.EqualTo(correctPath));
    }
    [Test]
    public void StrangeMSBuildPathFormatAssets4Test() {
        var incorrectPath = $"obj{IPS}{IPS}Debug{IPS}net8.0-android{APS}{APS}";
        var correctPath =  $"{Root}Documents{APS}TestProject{APS}obj{APS}Debug{APS}net8.0-android";

        var properties = new Dictionary<string, JToken>();
        properties.Add("project", TestProjectJToken);
        properties.Add("device", TestDeviceJToken);
        properties.Add("program", Assembly.GetExecutingAssembly().Location);
        properties.Add("assets", incorrectPath);
        var configuration = new LaunchConfiguration(properties);
        Assert.That(configuration.AssetsPath, Does.Not.Contain(IPS));
        Assert.That(configuration.AssetsPath, Is.EqualTo(correctPath));
    }
}