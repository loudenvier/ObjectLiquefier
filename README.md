# ObjectLiquefier
A (very) tiny library to help pretty-printing dotnet objects (for logging, auditing, visualization, etc. purposes). It uses Liquid templates powered by the nice [Fluid](https://github.com/sebastienros/fluid) library. 

## Ad-hoc templates

Given a simple class (or record) you can pretty-print it using an ad-hoc liquid template with minimal code:
```
const string template = "Hello {{What}}";
var liquefier = new Liquefier();
var helloWorld = liquefier.Liquefy(new { What = "World" }, template);
Console.WriteLine(helloWorld);
```
Which yields `Hello World` in the Console output.

The following unit test asserts the correctness of a more complex liquid template.

```
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
    var liquefied = liquefier.Liquefy(Felipe, personTemplate);
    Assert.Equal(felipeLiquefied, liquefied);
}
```

### Ad-hoc template caching

Ad-hoc templates are compiled and cached based on a hash of the template string. The library uses Jon Hanna's donet implementation ([Spookily Sharp](https://github.com/JonHanna/SpookilySharp/)) of [Bob Jenkins’ SpookyHash version 2](http://burtleburtle.net/bob/hash/spooky.html). While collisions are _possible_ they are very improbable so the library does nothing at all to prevent it as 1 in 16 quintillion chances is low enough for me.

Like strongly-typed templates, ad-hoc templates are compiled at first use and the compiled template is cached by its 128-bit cache key.
