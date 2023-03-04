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
    public void NamePartsAreFormedFromTheFullNameOfTheType() {
        var type = typeof(Path);
        var templateName = new ObjectTemplateName(type);
        Assert.Equal(3, templateName.NameParts.Length);
        Assert.Equal("system", templateName.NameParts[0]);
        Assert.Equal("io", templateName.NameParts[1]);
        Assert.Equal("path", templateName.NameParts[2]);
    }
    [Fact]
    public void PossibleNamesGoesFromMoreQualifiedToSimpleObjectName_IncludingLiquidFileExtension() {
        var type = typeof(Path);
        var templateName = new ObjectTemplateName(type);
        Assert.Equal("system.io.path.liquid", templateName.PossibleNames[0]);
        Assert.Equal("io.path.liquid", templateName.PossibleNames[1]);
        Assert.Equal("path.liquid", templateName.PossibleNames[2]);
    }
    [Fact]
    public void NamePartsAndPossibleNamesAreTheSameLength() {
        var type = typeof(Path);
        var templateName = new ObjectTemplateName(type);
        Assert.Equal(templateName.NameParts.Length, templateName.PossibleNames.Length);
    }
}
