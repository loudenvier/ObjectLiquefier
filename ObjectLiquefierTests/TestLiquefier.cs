using Fluid.Values;

public class TestLiquefier {
    public class Person {
        public string Name { get; set; } = "";
        public DateTime Birth { get; set; }
        public string? Bio { get; set; }
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
    public void CanInstantiateWithDefaultConfigButItsNotTheSame() {
        // if it was the same then a change in an instance settings would affect the default's
        var liquefier = new Liquefier();
        Assert.NotSame(Liquefier.DefaultSettings(), liquefier.Settings);
    }

    [Fact]
    public void CanConfigureLiquefierSettings() {
        var liquefier = new Liquefier(cfg => {
            cfg.TemplateFolder = "test";
            cfg.ParserOptions.AllowFunctions = false; // = new Fluid.FluidParserOptions { AllowFunctions = false };
            cfg.TemplateOptions.MaxSteps = 567;
            cfg.TemplateOptions.MaxRecursion = 2;
        });
        Assert.Equal("test", liquefier.Settings.TemplateFolder);   
        Assert.False(liquefier.Settings.ParserOptions.AllowFunctions);
        Assert.Equal(567, liquefier.Settings.TemplateOptions.MaxSteps);
    }
    [Fact]
    public void CanConfigureLiquefierSettingsWithNewFilters() {
        var liquefier = new Liquefier(cfg => {
            cfg.TemplateOptions.Filters.AddFilter("tagorvalue", (input, args, ctx) => {
                if (input.IsNil())
                    return NilValue.Instance;
                return input;
            });
        });
        var t = liquefier.Liquefy(new { Name = "Felipe" }, "{{ Name | tagorvalue }}");
        Assert.Equal("Felipe", t);
    }
    [Fact]
    public void LiquefyWithoutTemplateReturnsEmptyString() {
        var liquefier = new Liquefier();
        Assert.Equal("", liquefier.Liquefy(Felipe));
    }
    [Fact]
    public void TryLiquefyReturnsFalseWithoutTemplate() {
        Assert.False(new Liquefier().TryLiquefy(Felipe, out _));
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
        File.WriteAllText(Path.Combine(liquefier.Settings.TemplateFolder, "person.liquid"), personTemplate);
        try {
            var liquefied = liquefier.Liquefy(Felipe);
            Assert.Equal(felipeLiquefied, liquefied);
        } finally {
            Directory.Delete(liquefier.Settings.TemplateFolder, true);
        }
    }
    [Fact]
    public void LiquefyUsesProperTemplateEvenIfReferenceIsToSystemObject() {
        var liquefier = new Liquefier();
        Directory.CreateDirectory(liquefier.Settings.TemplateFolder);
        File.WriteAllText(Path.Combine(liquefier.Settings.TemplateFolder, "person.liquid"), personTemplate);
        try {
            object obj = Felipe;
            var liquefied = liquefier.Liquefy(obj);
            Assert.Equal(felipeLiquefied, liquefied);
        } finally {
            Directory.Delete(liquefier.Settings.TemplateFolder, true);
        }
    }

    [Fact]
    public void LiquefyPersonAdHocDoesNotOverwriteTemplateOnDisk() {
        var liquefier = new Liquefier();
        Directory.CreateDirectory(liquefier.Settings.TemplateFolder);
        File.WriteAllText(Path.Combine(liquefier.Settings.TemplateFolder, "person.liquid"), personTemplate+"disk");
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
        File.WriteAllText(Path.Combine(liquefier.Settings.TemplateFolder, "person.liquid"), personTemplate + "disk");
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
        try {
            File.WriteAllText(Path.Combine(liquefier.Settings.TemplateFolder, "person.liquid"), personTemplate);
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

    public class Nested { 
        public string? Name { get; set; } 
        public DateTime Birth { get; set; }
        public class Person {
            public string? Name { get; set; }
            public DateTime Birth { get; set; }

        }
    }

    [Fact]
    public void LiquefyCachesTemplateFromDiskForNestedClasses() {
        var liquefier = new Liquefier();
        var folder = liquefier.Settings.TemplateFolder;
        Directory.CreateDirectory(folder);
        try {
            File.WriteAllText(Path.Combine(folder, "nested.liquid"), personTemplate);
            var nested = new Nested { Name = "Felipe", Birth = myBirth };
            var liquefied = liquefier.Liquefy(nested);
            Assert.Equal(felipeLiquefied, liquefied);
            Directory.Delete(folder, true);
            // since template dir is deleted, the compiled template must come from the cache
            var liquefiedByCachedTemplate = liquefier.Liquefy(nested);
            Assert.Equal(liquefied, liquefiedByCachedTemplate);
        } finally {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void LiquefyCachesTemplatesByFullTypeName() {
        var liquefier = new Liquefier();
        var folder = liquefier.Settings.TemplateFolder;
        Directory.CreateDirectory(folder);
        try {
            File.WriteAllText(Path.Combine(folder, "person.liquid"), personTemplate);
            var nested = new Nested.Person { Name = "Felipe", Birth = myBirth };
            var liquefied = liquefier.Liquefy(nested);
            Assert.Equal(felipeLiquefied, liquefied);
            Directory.Delete(folder, true);
            // since template dir is deleted, the compiled template must come from the cache
            Assert.Equal("", liquefier.Liquefy(Felipe));
        } finally {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }

    public class Child : Person { }

    [Fact]
    public void LiquefyUsesParentTemplateForChildClassWithoutTemplate() {
        var liquefier = new Liquefier();
        var folder = liquefier.Settings.TemplateFolder;
        Directory.CreateDirectory(folder);
        try {
            File.WriteAllText(Path.Combine(folder, "person.liquid"), personTemplate);
            var child = new Child { Name = "Felipe", Birth = myBirth };
            var liquefied = liquefier.Liquefy(child);
            Assert.Equal(felipeLiquefied, liquefied);
        } finally {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void LiquefyObjectUsesDefaultSettings() {
        const string otherFolder = "other_folder";
        Liquefier.DefaultSettings = () => new LiquefierSettings {
            TemplateFolder = otherFolder,
        };
        Directory.CreateDirectory(otherFolder);
        try {
            File.WriteAllText(Path.Combine(otherFolder, "person.liquid"), personTemplate);
            var liquefied = Liquefier.LiquefyObject(Felipe);
            Assert.Equal(felipeLiquefied, liquefied);
        } finally {
            Directory.Delete(otherFolder, true);
        }
    }

    [Fact]
    public void LiquefyObjectCachesTemplateFromDisk() {
        var folder = Liquefier.Instance.Settings.TemplateFolder;
        Directory.CreateDirectory(folder);
        try {
            File.WriteAllText(Path.Combine(folder, "person.liquid"), personTemplate);
            var liquefied = Liquefier.LiquefyObject(Felipe);
            Assert.Equal(felipeLiquefied, liquefied);
            Directory.Delete(folder, true);
            // since template dir is deleted, the compiled template must come from the cache
            var liquefiedByCachedTemplate = Liquefier.LiquefyObject(Felipe);
            Assert.Equal(liquefied, liquefiedByCachedTemplate);
        } finally {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }


}
