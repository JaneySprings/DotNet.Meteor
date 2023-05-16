using Xunit;
using DotNet.Meteor.Shared;

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
        else if (RuntimeSystem.IsMacOS)
            Assert.StartsWith("/Users", homeDirectory);
        else
            Assert.StartsWith("/home", homeDirectory);
    }

    [Fact]
    public void ProgramFilesDirectoryValidationTest() {
        if (!RuntimeSystem.IsWindows)
            return;
        
        var homeDirectory = RuntimeSystem.ProgramX86Directory;
        Assert.StartsWith("C:\\Program", homeDirectory);
    }
}