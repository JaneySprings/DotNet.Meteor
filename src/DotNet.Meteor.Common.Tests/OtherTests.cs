using DotNet.Meteor.Common.Extensions;
using DotNet.Meteor.Common.Android;
using NUnit.Framework;

namespace DotNet.Meteor.Common.Tests;

public class OtherTests : TestFixture {

    [Test]
    public void AndroidSdkDirectoryTests() {
        var sdkLocation = AndroidSdkLocator.SdkLocation();
        Assert.Multiple(() => {
            Assert.That(sdkLocation, Is.Not.Null.Or.Empty);
            Assert.That(Directory.Exists(sdkLocation));
        });
    }
    [Test]
    public void HomeDirectoryValidationTest() {
        var homeDirectory = RuntimeSystem.HomeDirectory;
        if (RuntimeSystem.IsWindows)
            Assert.That(homeDirectory, Does.StartWith("C:\\Users"));
        else if (RuntimeSystem.IsMacOS)
            Assert.That(homeDirectory, Does.StartWith("/Users"));
        else
            Assert.That(homeDirectory, Does.StartWith("/home"));
    }
    [Test]
    public void ProgramFilesDirectoryValidationTest() {
        if (!RuntimeSystem.IsWindows)
            return;

        var programsDirectory = RuntimeSystem.ProgramX86Directory;
        Assert.That(programsDirectory, Does.StartWith("C:\\Program"));
    }
    [Test]
    public void ToolingDefaultsTest() {
        Assert.That(Common.Apple.MonoLauncher.UseDeviceCtl, Is.False, "UseDeviceCtl should be false: https://github.com/xamarin/xamarin-macios/issues/21664");
    }
}