public class TestObjectTemplateName
{
    [Fact]
    public void ObjectTypeEqualsTheType() {
        var type = typeof(Path);
        var templateName = new ObjectTemplateName(type);
        Assert.Equal(typeof(Path), templateName.ObjectType);
    }
    [Fact]
    public void TypeNameEqualsTheFullNameOfTheTypeInLowerCase() {
        var type = typeof(Path);
        var templateName = new ObjectTemplateName(type);
        Assert.Equal("system.io.path", templateName.TypeName);
    }
    [Fact]
    public void PossibleNamesGoesFromMoreQualifiedToSimpleObjectName_IncludingLiquidFileExtension() {
        var type = typeof(Path);
        var templateName = new ObjectTemplateName(type);
        Assert.Equal("system.io.path.liquid", templateName.PossibleNames[0]);
        Assert.Equal("io.path.liquid", templateName.PossibleNames[1]);
        Assert.Equal("path.liquid", templateName.PossibleNames[2]);
        Assert.Equal(3, templateName.PossibleNames.Length);
    }

    public class Nested
    {
        public string? Dummy { get; set; }
        public class Nested2 { }
    }
    [Fact]
    public void PossibleNamesConsidersNestedClassesToo() {
        var templateName = new ObjectTemplateName(typeof(Nested));
        Assert.Equal("testobjecttemplatename.nested.liquid", templateName.PossibleNames[0]);
        Assert.Equal("nested.liquid", templateName.PossibleNames[1]);
    }
    [Fact]
    public void PossibleNamesConsidersMultipleNestedClasses() {
        var templateName = new ObjectTemplateName(typeof(Nested.Nested2));
        Assert.Equal("testobjecttemplatename.nested.nested2.liquid", templateName.PossibleNames[0]);
        Assert.Equal("nested.nested2.liquid", templateName.PossibleNames[1]);
        Assert.Equal("nested2.liquid", templateName.PossibleNames[2]);
    }

    public class Parent { }
    public class Child : Parent { }
    public class GrandChild : Child { }
    [Fact]
    public void PossibleNamesIncludeTheWholeClassHierarchyExceptForSystemObject() {
        var templateName = new ObjectTemplateName(typeof(GrandChild));
        Assert.Equal("testobjecttemplatename.grandchild.liquid", templateName.PossibleNames[0]);
        Assert.Equal("grandchild.liquid", templateName.PossibleNames[1]);
        Assert.Equal("testobjecttemplatename.child.liquid", templateName.PossibleNames[2]);
        Assert.Equal("child.liquid", templateName.PossibleNames[3]);
        Assert.Equal("testobjecttemplatename.parent.liquid", templateName.PossibleNames[4]);
        Assert.Equal("parent.liquid", templateName.PossibleNames[5]);
        Assert.Equal(6, templateName.PossibleNames.Length);
    }

}

