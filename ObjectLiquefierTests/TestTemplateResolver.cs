public class TestTemplateResolver {
    const string TestTemplateFolder = nameof(TestTemplateFolder);
    public TestTemplateResolver() { 
        Directory.CreateDirectory(TestTemplateFolder);
    }
    static string GetTemplatePath(string templaName) => Path.Combine(TestTemplateFolder, templaName);
    static void WriteTemplate(string templateName, string template) 
        => File.WriteAllText(GetTemplatePath(templateName), template);
    static void WriteAllPossibleTemplates(ObjectTemplateName name) {
        foreach (var possibility in name.PossibleNames)
            WriteTemplate(possibility, possibility);
    }
    static void DeleteAllPossibleTemplates(ObjectTemplateName name) {
        foreach (var possibility in name.PossibleNames)
            File.Delete(GetTemplatePath(possibility));
    }

    [Fact]
    public void ResolveFullyQualifiedTemplateFirst() {
        var resolver = new TemplateResolver(TestTemplateFolder);
        var name = new ObjectTemplateName(typeof(Path));
        WriteAllPossibleTemplates(name);
        try {
            var resolved = resolver.ResolveTemplate(name);
            Assert.Equal($"{TestTemplateFolder}\\{name.PossibleNames[0]}", resolved);
        } finally {
            DeleteAllPossibleTemplates(name);
        }
    }
    [Fact]
    public void ResolveSecondQualifiedTemplateIfFullyQualifiedTemplateIsMissing() {
        var resolver = new TemplateResolver(TestTemplateFolder);
        var name = new ObjectTemplateName(typeof(Path));
        WriteAllPossibleTemplates(name);
        File.Delete(GetTemplatePath(name.PossibleNames[0]));
        try {
            var resolved = resolver.ResolveTemplate(name);
            Assert.Equal($"{TestTemplateFolder}\\{name.PossibleNames[1]}", resolved);
        } finally {
            DeleteAllPossibleTemplates(name);
        }
    }
    [Fact]
    public void ResolveNonQualifiedTemplateIfAllMoreQualifiedTemplatesAreMissing() {
        var resolver = new TemplateResolver(TestTemplateFolder);
        var name = new ObjectTemplateName(typeof(Path));
        WriteAllPossibleTemplates(name);
        File.Delete(GetTemplatePath(name.PossibleNames[0]));
        File.Delete(GetTemplatePath(name.PossibleNames[1]));
        try {
            var resolved = resolver.ResolveTemplate(name);
            Assert.Equal($"{TestTemplateFolder}\\{name.PossibleNames[2]}", resolved);
        } finally {
            DeleteAllPossibleTemplates(name);
        }
    }
    [Fact]
    public void AllResolveTemplateOverloadsAreEquivalent() {
        var resolver = new TemplateResolver(TestTemplateFolder);
        var name = new ObjectTemplateName(typeof(object));
        WriteAllPossibleTemplates(name);
        try {
            var resolved = resolver.ResolveTemplate(name);
            Assert.Equal($"{TestTemplateFolder}\\{name.PossibleNames[0]}", resolved);
            var resolved2 = resolver.ResolveTemplate<object>();
            var resolved3 = resolver.ResolveTemplate(typeof(object));
            Assert.Equal(resolved, resolved2);
            Assert.Equal(resolved2, resolved3);
        } finally {
            DeleteAllPossibleTemplates(name);
        }
    }
}
