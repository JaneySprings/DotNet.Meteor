using Xunit;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Windows;
using DotNet.Meteor.Android;
using DotNet.Meteor.Apple;

namespace DotNet.Meteor.Tests;

public class OtherTests: TestFixture {

    [Fact]
    public void AndroidSdkDirectoryTests() {
        var sdkLocation = Android.PathUtils.SdkLocation();
        Assert.NotNull(sdkLocation);
        Assert.True(Directory.Exists(sdkLocation));
    }

    [Fact]
    public void HomeDirectoryValidationTest() {
        var homeDirectory = RuntimeSystem.HomeDirectory;
        if (RuntimeSystem.IsWindows)
            Assert.StartsWith("C:\\Users", homeDirectory);
        else
            Assert.StartsWith("/Users", homeDirectory);
    }

    [Fact]
    public void ProgramFilesDirectoryValidationTest() {
        if (!RuntimeSystem.IsWindows)
            return;
        
        var homeDirectory = RuntimeSystem.ProgramX86Directory;
        Assert.StartsWith("C:\\Program", homeDirectory);
    }
}