using System.Text;
using DotNet.Meteor.HotReload;
using Xunit;

namespace DotNet.Meteor.Tests;

public class HotReloadClientTests: TestFixture {

    [Fact]
    public void ClassDefinitionFinderTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout>
        <Label Text=""Welcome to .NET MAUI!"" />
        <Label Text=""This is a test"" />
    </StackLayout>
</ContentPage>
");
        var actualClassDefinition = "DotNet.Meteor.MainPage";
        var classDefinition = MarkupHelper.GetClassDefinition(xamlContent);
        Assert.Equal(actualClassDefinition, classDefinition);
    }

    [Fact]
    public void ElementNameRemovingTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout x:Name=""element1"">
        <Label Text=""Welcome to .NET MAUI!"" />
        <Label x:Name=""element2"" Text=""This is a test"" />
    </StackLayout>
</ContentPage>
");
        MarkupHelper.RemoveReferenceNames(xamlContent);
        Assert.DoesNotContain("x:Name", xamlContent.ToString());
    }
}