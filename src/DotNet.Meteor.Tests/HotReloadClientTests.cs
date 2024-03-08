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
        var classDefinition = MarkupExtensions.GetClassDefinition(xamlContent);
        Assert.Equal(actualClassDefinition, classDefinition);
    }

    [Fact]
    public void ElementNameChangingTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout x:Name=""element1"">
        <Label Text=""Welcome to .NET MAUI!"" />
        <Label Text=""This is a test"" />
    </StackLayout>
</ContentPage>
");
        MarkupExtensions.ModifyReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Single(names);
        Assert.DoesNotContain("element1", names);
        Assert.Contains("element1_", names.First());
    }

    [Fact]
    public void ElementMultipleNamesChangingTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout x:Name=""element1"">
        <Label x:Name=""element2"" Text=""Welcome to .NET MAUI!"" />
        <Label x:Name=""element3"" Text=""This is a test"" />
    </StackLayout>
</ContentPage>
");
        MarkupExtensions.ModifyReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Equal(3, names.Count);
        Assert.DoesNotContain("element1", names);
        Assert.DoesNotContain("element2", names);
        Assert.DoesNotContain("element3", names);
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element1_")));
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element2_")));
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element3_")));
    }

    [Fact]
    public void ElementNameChangingWithReferenceTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout Command=""{Binding Source={x:Reference element1}, Path=Commands.element1.Some}"">
        <Label Text=""Welcome to .NET MAUI!"" />
        <Label x:Name=""element1"" Style=""{StaticResource element1}"" />
    </StackLayout>
</ContentPage>
");
        MarkupExtensions.ModifyReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Single(names);
        Assert.DoesNotContain("element1", names);
        Assert.Contains("element1_", names.First());
        Assert.Contains("x:Reference element1_", xamlContent.ToString());
        Assert.Equal(2, xamlContent.ToString().Split(names.First()).Length - 1);
    }

    [Fact]
    public void ElementMultipleNameChangingWithReferencesTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout x:Name=""element2"" Command=""{Binding Source={x:Reference element1}, Path=Commands.element1.Some}"">
        <Label x:Name=""element1"" Style=""{StaticResource element1}"" />
        <Button Command=""{Binding Source={x:Reference element2}, Path=Commands.element2.Some}"" />
    </StackLayout>
</ContentPage>
");
        MarkupExtensions.ModifyReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Equal(2, names.Count);
        Assert.DoesNotContain("element1", names);
        Assert.DoesNotContain("element2", names);
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element1_")));
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element2_")));
        Assert.Contains("x:Reference element1_", xamlContent.ToString());
        Assert.Contains("x:Reference element2_", xamlContent.ToString());
        Assert.Equal(2, xamlContent.ToString().Split(names[0]).Length - 1);
        Assert.Equal(2, xamlContent.ToString().Split(names[1]).Length - 1);
    }
}