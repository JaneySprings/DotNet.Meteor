using DotNet.Debugging.Common.Android;
using DotNet.Debugging.Common.Apple;
using DotNet.Meteor.Workspace;
using NUnit.Framework;

namespace DotNet.Meteor.Tests;

public class OtherTests : TestFixture {

    [Test]
    public void AndroidSdkDirectoryTests() {
        var sdkLocation = AndroidSdkLocator.GetSdkDirecotry();
        Assert.Multiple(() => {
            Assert.That(sdkLocation, Is.Not.Null.Or.Empty);
            Assert.That(Directory.Exists(sdkLocation));
        });
    }
    [Test]
    public void HomeDirectoryValidationTest() {
        var homeDirectory = RuntimeInfo.HomeDirectory;
        if (RuntimeInfo.IsWindows)
            Assert.That(homeDirectory, Does.StartWith("C:\\Users"));
        else if (RuntimeInfo.IsMacOS)
            Assert.That(homeDirectory, Does.StartWith("/Users"));
        else
            Assert.That(homeDirectory, Does.StartWith("/home"));
    }
    [Test]
    public void ProgramFilesDirectoryValidationTest() {
        if (!RuntimeInfo.IsWindows)
            Assert.Ignore("Windows only");

        var programsDirectory = RuntimeInfo.ProgramX86Directory;
        Assert.That(programsDirectory, Does.StartWith("C:\\Program"));
    }

    [Test]
    public void ToolingDefaultsTest() {
        Assert.That(MonoLauncher.UseDeviceCtl, Is.False, "UseDeviceCtl should be false: https://github.com/xamarin/xamarin-macios/issues/21664");
    }
}