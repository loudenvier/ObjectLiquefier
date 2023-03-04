# ObjectLiquefier

A (very) tiny library to help pretty-printing dotnet objects (for logging, auditing, visualization, etc. purposes). It uses Liquid templates powered by the nice [Fluid](https://github.com/sebastienros/fluid) library. 

## Strongly-typed Templates

The library uses a convention-based template resolution mechanism which automatically resolves templates on disk based on an object's type name. When a suitable template is found it is read from disk, compiled and the compiled template is cached using the object's type name as the cache key.

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

The `TemplateResolver` will search for a file named `some.test.person.liquid`, then for `test.person.liquid` and, finally, simply for `person.liquid`. Files are searched in the configured `TemplateSettings.TemplateFolder` (which defaults to `"liquefier"`). 

_Note: the `.liquid` extension is hard-coded and cannot be changed. Templates **MUST** have this extension._

## Ad-hoc Templates

Given a simple class (or record) you can pretty-print it using an ad-hoc liquid template with minimal code:
```csharp
const string template = "Hello {{What}}";
var liquefier = new Liquefier();
var helloWorld = liquefier.Liquefy(new { What = "World" }, template);
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

Like strongly-typed templates, ad-hoc templates are compiled at first use and the compiled template is cached by its 128-bit cache key.

To genate the 128-bit cache key the library uses Jon Hanna's donet implementation ([Spookily Sharp](https://github.com/JonHanna/SpookilySharp/)) of [Bob Jenkins’ SpookyHash version 2](http://burtleburtle.net/bob/hash/spooky.html). While collisions are _possible_ they are very improbable so the library does nothing at all to prevent it as 1 in 16 quintillion chances is low enough for me.


