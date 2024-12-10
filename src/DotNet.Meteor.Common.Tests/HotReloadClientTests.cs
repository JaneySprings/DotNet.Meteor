using System.Text;
using DotNet.Meteor.HotReload.Extensions;
using NUnit.Framework;

namespace DotNet.Meteor.Common.Tests;

public class HotReloadClientTests: TestFixture {

    [Test]
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
        Assert.That(classDefinition, Is.EqualTo(actualClassDefinition));
    }
    [Test]
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
        Assert.Multiple(() => {
            Assert.That(names, Has.Count.EqualTo(1));
            Assert.That(transformations, Has.Count.EqualTo(1));
            Assert.That(names, Has.No.Member("element1"));
            Assert.That(transformations.Single().Key, Is.EqualTo("element1"));
            Assert.That(names.First(), Does.Contain("element1_"));
            Assert.That(transformations.Single().Value, Does.Contain("element1_"));
        });
    }
    [Test]
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
        Assert.Multiple(() => {
            Assert.That(names, Has.Count.EqualTo(3));
            Assert.That(transformations, Has.Count.EqualTo(3));
            Assert.That(names, Has.No.Member("element1"));
            Assert.That(names, Has.No.Member("element2"));
            Assert.That(names, Has.No.Member("element3"));
            Assert.That(transformations.Keys, Does.Contain("element1"));
            Assert.That(transformations.Keys, Does.Contain("element2"));
            Assert.That(transformations.Keys, Does.Contain("element3"));
            Assert.That(transformations["element1"], Does.Contain("element1_"));
            Assert.That(transformations["element2"], Does.Contain("element2_"));
            Assert.That(transformations["element3"], Does.Contain("element3_"));
            Assert.That(names.FirstOrDefault(it => it.StartsWith("element1_")), Is.Not.Null);
            Assert.That(names.FirstOrDefault(it => it.StartsWith("element2_")), Is.Not.Null);
            Assert.That(names.FirstOrDefault(it => it.StartsWith("element3_")), Is.Not.Null);
        });
    }
    [Test]
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
        Assert.Multiple(() => {
            Assert.That(names, Has.Count.EqualTo(1));
            Assert.That(names, Has.No.Member("element1"));
            Assert.That(names.First(), Does.Contain("element1_"));
            Assert.That(xamlContent.ToString(), Does.Contain("lg:Reference element1_"));
            Assert.That(xamlContent.ToString().Split(names.First()).Length - 1, Is.EqualTo(2));
        });
    }
    [Test]
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
        Assert.Multiple(() => {
            Assert.That(names, Has.Count.EqualTo(2));
            Assert.That(names, Has.No.Member("element1"));
            Assert.That(names, Has.No.Member("element2"));
            Assert.That(names.FirstOrDefault(it => it.StartsWith("element1_")), Is.Not.Null);
            Assert.That(names.FirstOrDefault(it => it.StartsWith("element2_")), Is.Not.Null);
            Assert.That(xamlContent.ToString(), Does.Contain("x:Reference element1_"));
            Assert.That(xamlContent.ToString(), Does.Contain("x:Reference element2_"));
            Assert.That(xamlContent.ToString().Split(names[0]).Length - 1, Is.EqualTo(2));
            Assert.That(xamlContent.ToString().Split(names[1]).Length - 1, Is.EqualTo(2));
        });
    }
    [Test]
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
        Assert.Multiple(() => {
            Assert.That(names, Has.Count.EqualTo(2));
            Assert.That(transformations, Has.Count.EqualTo(2));
            Assert.That(names, Has.No.Member("element"));
            Assert.That(names, Has.No.Member("elementTwo"));
            Assert.That(transformations.Keys, Does.Contain("element"));
            Assert.That(transformations.Keys, Does.Contain("elementTwo"));
            Assert.That(transformations["element"], Does.Contain("element_"));
            Assert.That(transformations["elementTwo"], Does.Contain("elementTwo_"));
            Assert.That(names.FirstOrDefault(it => it.StartsWith("element")), Is.Not.Null);
            Assert.That(names.FirstOrDefault(it => it.StartsWith("elementTwo")), Is.Not.Null);
        });
    }
    [Test]
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
        Assert.Multiple(() => {
            Assert.That(names, Has.Count.EqualTo(2));
            Assert.That(names, Has.No.Member("element"));
            Assert.That(names, Has.No.Member("elementTwo"));
            Assert.That(transformations.Keys, Has.Member("element"));
            Assert.That(transformations.Keys, Has.Member("elementTwo"));
            Assert.That(transformations["element"], Does.Contain("element_"));
            Assert.That(transformations["elementTwo"], Does.Contain("elementTwo_"));
            Assert.That(names.FirstOrDefault(it => it.StartsWith("element")), Is.Not.Null);
            Assert.That(names.FirstOrDefault(it => it.StartsWith("elementTwo")), Is.Not.Null);
        });
        foreach (var transformation in transformations) 
            Assert.That(xamlContent.ToString(), Does.Contain($"x:Reference {transformation.Value}"));
    }
    [Test]
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
        Assert.Multiple(() => {
            Assert.That(names, Has.Count.EqualTo(1));
            Assert.That(names, Has.No.Member("element"));
            Assert.That(transformations.Keys, Does.Contain("element"));
            Assert.That(transformations["element"], Does.Contain("element_"));
        });
        foreach (var transformation in transformations) 
            Assert.That(xamlContent.ToString(), Does.Contain($"x:Reference Name={transformation.Value}"));
    }
    [Test]
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
        Assert.Multiple(() => {
            Assert.That(names, Has.Count.EqualTo(1));
            Assert.That(transformations.Keys, Has.Member("element"));
            Assert.That(transformations["element"], Does.Contain("element_"));
            Assert.That(transformations.Keys, Has.No.Member("element2"));
            Assert.That(transformations.Values, Has.No.Member("element2_"));
        });
    }
}