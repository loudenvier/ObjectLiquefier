using Fluid;
using SpookilySharp;
using System;
using System.IO;

namespace ObjectLiquefier
{
    public class Liquefier
    {
        private readonly TemplateCache cache = new();
        private readonly TemplateResolver resolver;
        private readonly FluidParser parser;

        public Liquefier(Action<LiquefierSettings>? configAction=null) {
            Settings = new LiquefierSettings();
            configAction?.Invoke(Settings);
            resolver = new(Settings.TemplateFolder);
            parser = new FluidParser(Settings.ParserOptions);
        }

        public LiquefierSettings Settings { get; }

        public string Liquefy<T>(T obj, string? template=null) where T : class {
            var templateKey = template == null ? typeof(T).FullName : GetAdHocTemplateKey(template);
            var parsedTemplate = FindTemplate<T>(templateKey, template);
            var context = new TemplateContext(obj, Settings.TemplateOptions);
            return parsedTemplate.Render(context);
        }

        public string GetAdHocTemplateKey(string template) => template.SpookyHash128().ToString();

        public IFluidTemplate FindTemplate<T>(string key, string? template) where T : class {
            // tries to get a parsed template from the cache or parses and caches it from the resolver
            // a run condition may happen but in the worst case the same template gets compiled more than once
            var parsedTemplate = cache[key];
            if (parsedTemplate == null) {
                template ??= File.ReadAllText(resolver.ResolveTemplate<T>());
                cache[key] = parsedTemplate = parser.Parse(template);
            }
            return parsedTemplate;
        }

        public sealed class LiquefierSettings
        {
            public FluidParserOptions ParserOptions { get; set; } = new();
            public TemplateOptions TemplateOptions { get; set; } = new();
            public string TemplateFolder { get; set; } = "liquefier";
        }

    }

}
