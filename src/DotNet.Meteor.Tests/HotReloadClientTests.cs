using System.Text;
using DotNet.Meteor.HotReload.Extensions;
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
        var transformations = MarkupExtensions.TransformReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Single(names);
        Assert.Single(transformations);
        Assert.DoesNotContain("element1", names);
        Assert.Equal("element1", transformations.Single().Key);
        Assert.Contains("element1_", names.First());
        Assert.Contains("element1_", transformations.Single().Value);
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
        var transformations = MarkupExtensions.TransformReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Equal(3, names.Count);
        Assert.Equal(3, transformations.Count);
        Assert.DoesNotContain("element1", names);
        Assert.DoesNotContain("element2", names);
        Assert.DoesNotContain("element3", names);
        Assert.Contains("element1", transformations.Keys);
        Assert.Contains("element2", transformations.Keys);
        Assert.Contains("element3", transformations.Keys);
        Assert.Contains("element1_", transformations["element1"]);
        Assert.Contains("element2_", transformations["element2"]);
        Assert.Contains("element3_", transformations["element3"]);
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element1_")));
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element2_")));
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element3_")));
    }

    [Fact]
    public void ElementNameChangingWithReferenceTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    lg:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:lg=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout Command=""{Binding Source={lg:Reference element1}, Path=Commands.element1.Some}"">
        <Label Text=""Welcome to .NET MAUI!"" />
        <Label lg:Name=""element1"" Style=""{StaticResource element1}"" />
    </StackLayout>
</ContentPage>
");
        MarkupExtensions.TransformReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Single(names);
        Assert.DoesNotContain("element1", names);
        Assert.Contains("element1_", names.First());
        Assert.Contains("lg:Reference element1_", xamlContent.ToString());
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
        MarkupExtensions.TransformReferenceNames(xamlContent);
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

    [Fact]
    public void ElementMultipleNamesWithSamePartTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout>
        <Label x:Name=""element"" Text=""Welcome to .NET MAUI!"" />
        <Label x:Name=""elementTwo"" Text=""This is a test"" />
    </StackLayout>
</ContentPage>
");
        var transformations = MarkupExtensions.TransformReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Equal(2, names.Count);
        Assert.Equal(2, transformations.Count);
        Assert.DoesNotContain("element", names);
        Assert.DoesNotContain("elementTwo", names);
        Assert.Contains("element", transformations.Keys);
        Assert.Contains("elementTwo", transformations.Keys);
        Assert.Contains("element_", transformations["element"]);
        Assert.Contains("elementTwo_", transformations["elementTwo"]);
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element")));
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("elementTwo")));
    }

    [Fact]
    public void ElementNameWithSamePartWithReferenceTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout Command=""{Binding Source={x:Reference element}"">
        <Label x:Name=""element"" />
    </StackLayout>
    <StackLayout Command=""{Binding Source={x:Reference elementTwo}"">
        <Label x:Name=""elementTwo"" />
    </StackLayout>
</ContentPage>
");
        var transformations = MarkupExtensions.TransformReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Equal(2, names.Count);
        Assert.DoesNotContain("element", names);
        Assert.DoesNotContain("elementTwo", names);
        Assert.Contains("element", transformations.Keys);
        Assert.Contains("elementTwo", transformations.Keys);
        Assert.Contains("element_", transformations["element"]);
        Assert.Contains("elementTwo_", transformations["elementTwo"]);
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("element")));
        Assert.NotNull(names.FirstOrDefault(it => it.StartsWith("elementTwo")));
        foreach (var transformation in transformations) 
            Assert.Contains($"x:Reference {transformation.Value}", xamlContent.ToString());
    }

    [Fact]
    public void ElementNameWithFullQualifiedReferenceTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout Command=""{Binding Source={x:Reference Name=element}"">
        <Label x:Name=""element"" />
    </StackLayout>
</ContentPage>
");
        var transformations = MarkupExtensions.TransformReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        Assert.Single(names);
        Assert.DoesNotContain("element", names);
        Assert.Contains("element", transformations.Keys);
        Assert.Contains("element_", transformations["element"]);
        foreach (var transformation in transformations) 
            Assert.Contains($"x:Reference Name={transformation.Value}", xamlContent.ToString());
    }

    [Fact]
    public void TransformOnlyPortableXamlTypesTest() {
        var xamlContent = new StringBuilder(@"
<ContentPage
    x:Class=""DotNet.Meteor.MainPage""
    xmlns=""http://microsoft.com/schemas/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <StackLayout>
        <Label x:Name=""element"" />
        <Label Name=""element2"" />
    </StackLayout>
</ContentPage>
");
        var transformations = MarkupExtensions.TransformReferenceNames(xamlContent);
        var names = FindAllXNames(xamlContent);
        
        Assert.Single(names);
        Assert.Contains("element", transformations.Keys);
        Assert.Contains("element_", transformations["element"]);
        Assert.DoesNotContain("element2", transformations.Keys);
        Assert.DoesNotContain("element2_", transformations.Values);
    }
}