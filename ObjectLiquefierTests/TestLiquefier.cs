public class TestLiquefier {
    public class Person {
        public string Name { get; set; } = "";
        public DateTime Birth { get; set; }
        public int Age => (int)((DateTime.Today - Birth).TotalDays / 365.2425); 
    }
    public record PersonRec(string Name, DateTime Birth) {
        public int Age => (int)((DateTime.Today - Birth).TotalDays / 365.2425);
    }
    static readonly DateTime myBirth = new(1976, 03, 31);
    static readonly Person Felipe = new() { Name = "Felipe", Birth = myBirth };

    const string personTemplate =
        """
        Name: {{ Name }}
        Birth: {{ Birth | date: "%d/%m/%Y" }}
        """;
    const string felipeLiquefied =
        """
        Name: Felipe
        Birth: 31/03/1976
        """;

    [Fact]
    public void CanInstantiateWithDefaultConfig() {
        var liquefier = new Liquefier();
        Assert.NotNull(liquefier.Settings);
    }
    [Fact]
    public void CanConfigureLiquefierSettings() {
        var liquefier = new Liquefier(cfg => {
            cfg.TemplateFolder = "test";
            cfg.ParserOptions = new Fluid.FluidParserOptions { AllowFunctions = false };
            cfg.TemplateOptions.MaxSteps = 567;
        });
        Assert.Equal("test", liquefier.Settings.TemplateFolder);   
        Assert.False(liquefier.Settings.ParserOptions.AllowFunctions);
        Assert.Equal(567, liquefier.Settings.TemplateOptions.MaxSteps);
    }
    [Fact]
    public void LiquefyWithoutTemplateThrowsTemplateNotFound() {
        var liquefier = new Liquefier();
        Assert.Throws<LiquefyTemplateNotFoundException>(() 
            => liquefier.Liquefy(Felipe));
    }
    [Fact]
    public void LiquefyCanAcceptAdHocTemplate() {
        var liquefier = new Liquefier();
        var liquefied = liquefier.Liquefy(Felipe, personTemplate);
        Assert.Equal(felipeLiquefied, liquefied);
    }
    [Fact]
    public void AdHocTemplateGetsCachedByTheKeyReturnedFromGetAdHocTemplateKey() {
        var liquefier = new Liquefier();
        var cacheKey = liquefier.GetAdHocTemplateKey(personTemplate);
        liquefier.Liquefy(Felipe, personTemplate);
        // since FindCache template param is null it would go to disk if it wasn't cached!
        var template = liquefier.FindTemplate<Person>(cacheKey, null);
        Assert.NotNull(template);
    }
    [Fact]
    public void LiquefyPersonResolvesTemplateOnDisk() {
        var liquefier = new Liquefier();
        Directory.CreateDirectory(liquefier.Settings.TemplateFolder);
        File.WriteAllText(Path.Combine(liquefier.Settings.TemplateFolder, "testliquefier+person.liquid"), personTemplate);
        try {
            var liquefied = liquefier.Liquefy(Felipe);
            Assert.Equal(felipeLiquefied, liquefied);
        } finally {
            Directory.Delete(liquefier.Settings.TemplateFolder, true);
        }
    }
    [Fact]
    public void LiquefyPersonAdHocDoesNotOverwriteTemplateOnDisk() {
        var liquefier = new Liquefier();
        Directory.CreateDirectory(liquefier.Settings.TemplateFolder);
        File.WriteAllText(Path.Combine(liquefier.Settings.TemplateFolder, "testliquefier+person.liquid"), personTemplate+"disk");
        try {
            var liquefied = liquefier.Liquefy(Felipe, personTemplate);
            Assert.Equal(felipeLiquefied, liquefied);
            liquefied = liquefier.Liquefy(Felipe);
            Assert.Equal(felipeLiquefied + "disk", liquefied);
        } finally {
            Directory.Delete(liquefier.Settings.TemplateFolder, true);
        }
    }
    [Fact]
    public void LiquefyPersonWithDiskTemplateStillAllowsAdHocTemplates() {
        var liquefier = new Liquefier();
        Directory.CreateDirectory(liquefier.Settings.TemplateFolder);
        File.WriteAllText(Path.Combine(liquefier.Settings.TemplateFolder, "testliquefier+person.liquid"), personTemplate + "disk");
        try {
            var liquefied = liquefier.Liquefy(Felipe);
            Assert.Equal(felipeLiquefied + "disk", liquefied);
            liquefied = liquefier.Liquefy(Felipe, personTemplate);
            Assert.Equal(felipeLiquefied, liquefied);
        } finally {
            Directory.Delete(liquefier.Settings.TemplateFolder, true);
        }
    }
    [Fact]
    public void LiquefyCanAcceptCShardRecords() {
        var liquefier = new Liquefier();
        var felipeRec = new PersonRec("Felipe", myBirth);
        var liquefied = liquefier.Liquefy(felipeRec, personTemplate);
        Assert.Equal(felipeLiquefied, liquefied);
    }
    [Fact]
    public void LiquefyCachesTemplateFromDisk() {
        var liquefier = new Liquefier();
        Directory.CreateDirectory(liquefier.Settings.TemplateFolder);
        File.WriteAllText(Path.Combine(liquefier.Settings.TemplateFolder, "testliquefier+person.liquid"), personTemplate);
        try {
            var liquefied = liquefier.Liquefy(Felipe);
            Assert.Equal(felipeLiquefied, liquefied);
            Directory.Delete(liquefier.Settings.TemplateFolder, true);
            // since template dir is deleted, the compiled template must come from the cache
            var liquefiedByCachedTemplate = liquefier.Liquefy(Felipe);
            Assert.Equal(liquefied, liquefiedByCachedTemplate);
        } finally {
            if (Directory.Exists(liquefier.Settings.TemplateFolder))
                Directory.Delete(liquefier.Settings.TemplateFolder, true);
        }
    }

}
