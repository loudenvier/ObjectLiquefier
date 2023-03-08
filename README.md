# ObjectLiquefier

A (very) tiny library to help pretty-printing dotnet objects (for logging, auditing, visualization, etc. purposes). It uses Liquid templates powered by the nice [Fluid](https://github.com/sebastienros/fluid) library. 

## Installing

The fastest and simplest way to start using the library is from it's [nuget package](https://www.nuget.org/packages/ObjectLiquefier).
```powershell
NuGet\Install-Package ObjectLiquefier
```

It targets .NET Standard 2.0 which makes it compatible with all modern (and some legacy) dotnet projects.

## "Strongly"-typed Templates

The library uses a convention-based template resolution mechanism which automatically resolves templates on disk based on an object's type name and inheritance hierarchy. When a suitable template is found it is read from disk, compiled and the compiled template is cached using the object's type name as the cache key.

The resolution algorithm uses the `Type.FullName` to search for a template on disk. It start's with the most qualified name and descends down to the unqualified name. For example, take a `Person` class declared in the following namespace:

```csharp
namespace Some.Test
{
    public class Person {
        public int Id { get; set; } 
        public string Name { get; set; }
    }
}
```

The `TemplateResolver` will search for a file named `some.test.person.liquid`, then for `test.person.liquid` and, finally, simply for `person.liquid`. Files are searched in the configurable `TemplateSettings.TemplateFolder` (which defaults to `"liquefier"`). 

_Note: the `.liquid` extension is hard-coded and cannot be changed. Templates **MUST** have this extension._

### Template inheritance hierarchy

Template inheritance allows you to provide a single "base template" for a class hierarchy, and avoid the need to create one template for each class in the inheritance tree.  

If no suitable template is found for a given `Type` then the object hierarchy will be traversed backwards, up to the immediate `System.Object` descendant, checking for a suitable template match along the way. Given the following class hierarchy:

```csharp
namespace Game {
    public class Vehicle { }
    public class Car : Vehicle { }
    public class Truck : Car { }
}
```

The template resolution algorithm will search the following templates in this exact precedence order for the `Truck` class :

```csharp
game.truck.liquid >> truck.liquid >> game.car.liquid >> car.liquid >> game.vehicle.liquid >> vehicle.liquid
```

### Type name collisions

Templates are not actually strongly-typed since the engine does not care about assembly version, strong names, etc. Only the Type's Full Name is used for template resolution. Since the resolution mechanics do search for fully, partially and unqualified names, two classes with the same name can share the same template. For example:

```csharp
namespace Test.One {
    public record Person(string Name);
}
```
```csharp
namespace Test.Two {
    public class Person {
    	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Name => $"{LastName}, {FirstName}";
    }
}
```    

Both classes would share a template named `person.liquid`. If you need different templates for them you should qualify the template name further. For instance, a template named `two.person.liquid` would only satisfy the second `Person` class above.

_NOTE: while the `class` and `record` above could end up using the same template, the compiled template will be cached under different keys regardless as it uses the Type's full name as the cache key. This will result in a separate template compilation for each `Type` even though the template file is the same._

## Usage 

The easiest way to _liquefy_ an object is to use `Liquefier.LiquefyObject` static method which uses the default settings and a singleton `Liquefier` instance for template compilation, caching, resolution, etc.:

```csharp
var felipe = new Person { Name = "Felipe Machado", };
var liquefied = Liquefier.LiquefyObject(Felipe);

```

This will _liquefy_ the instance of the `Person` class above using a suitable liquid template (if none is available it returns an empty string). It will cache the compiled template, so subsequent calls with instances of the same class will get an already compiled liquid template from the cache. 

Liquid templates should be put inside a subfolder, under the current dir, named `liquefier` (can be configured). Considering the template bellow in the file  `.\liquefier\person.liquid`:

```liquid
<html>
<body>
	<h1>{{ Name }}</h1>
	<p><em>Birth:</em> {{ Birth | date: "%d/%m/%Y" }}
	<hr>
	<p>{{ Bio }}</p>
</body>
</html>
```

You can _liquefy_ classes named `Person`, as shown in the follwing code:

```csharp
public class Person {
    public string Name { get; set; } = "";
    public DateTime Birth { get; set; }
    public string? Bio { get; set; }
    public int Age => (int)((DateTime.Today - Birth).TotalDays / 365.2425);
}

var liquefied = Liquefier.LiquefyObject(new Person {
    Name = "Felipe Machado",
    Birth = new DateTime(1976, 03, 31),
    Bio = "Felipe was born in Volta Redonda, Rio de Janeiro, Brazil."
});
Console.WriteLine(liquefied);
```

Which will output this to the `Console`:

```html
<html>
<body>
	<h1>Felipe Machado</h1>
	<p><em>Birth:</em> 31/03/1976
	<hr>
	<p>Felipe was born in Volta Redonda, Rio de Janeiro, Brazil.</p>
</body>
</html>
```

### Default Settings

You can change the library's default settings by assigning a new `Func<LiquefierSettings>` to `Liquefier.DefaultSettings`:

```csharp
Liquefier.DefaultSettings = () => new Liquefier.LiquefierSettings {
    TemplateFolder = "another\\folder",
    // new liquefier instances will use "./another/folder" as the liquid source template directory
};

```

New `Liquefier` instances will automatically use the new default settings. The default liquefier instance used by `Liquefier.LiquefyObject()` will also use the new defaults if it was **NEVER** used prior to changing the settings (e.g.: you didn't call `Liquefier.LiquefyObject()` nor read the `Liquefier.Instance` property, as both would have instantiated the default `Liquefier` before the change).

#### Instance Settings

If you want to use a liquefier with custom settings without changing the defaults, you can configure the settings by passing an `Action<LiquefierSettings>` in it's constructor:

```csharp
var liquefier = new Liquefier(cfg => {
    cfg.TemplateFolder = "data\\templates";
    cfg.ParserOptions = new Fluid.FluidParserOptions { AllowFunctions = false };
    cfg.TemplateOptions.MaxSteps = 567;
    cfg.TemplateOptions.MaxRecursion = 2;
});
```

The `LiquefierSettings` passed onto the configuration `Action` will reflect the current default settings, so you can change only what is needed.

## Ad-hoc Templates

Given a simple class (or record) you can pretty-print it using an ad-hoc liquid template with minimal code:
```csharp
const string template = "Hello {{What}}";
var helloWorld = Liquefier.LiquefyObject(new { What = "World" }, template);
Console.WriteLine(helloWorld);
```
Which yields `Hello World` in the Console output.

The following unit test asserts the correctness of a more complex liquid template.

```csharp
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
public void LiquefyCanAcceptAdHocTemplate() {
    var liquefier = new Liquefier();
    var liquefied = liquefier.Liquefy(new { Name = "Felipe", Birth = new DateTime(1976, 03, 31) }, personTemplate);
    Assert.Equal(felipeLiquefied, liquefied);
}
```

### Ad-hoc template caching

Like strongly-typed templates, ad-hoc templates are compiled at first use and the compiled template is cached by a 128-bit cache key.

To generate the 128-bit cache key the library uses Jon Hanna's donet implementation ([Spookily Sharp](https://github.com/JonHanna/SpookilySharp/)) of [Bob Jenkins’ SpookyHash version 2](http://burtleburtle.net/bob/hash/spooky.html). While collisions are _possible_ they are very improbable so the library does nothing at all to prevent it as 1 in 16 quintillion chances is low enough for me.


