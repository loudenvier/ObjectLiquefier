using Fluid;
using SpookilySharp;
using System;
using System.IO;

namespace ObjectLiquefier
{
    public class Liquefier
    {
        private readonly TemplateCache cache = new();
        private readonly FluidParser parser;

        public static Func<LiquefierSettings> DefaultSettings { get; set; } = () => new LiquefierSettings();

        private readonly static Lazy<Liquefier> instance = new(() => new Liquefier());
        public static Liquefier Instance => instance.Value; 

        public Liquefier(Action<LiquefierSettings>? configAction=null) {
            Settings = DefaultSettings?.Invoke() ?? new LiquefierSettings();
            configAction?.Invoke(Settings);
            parser = new FluidParser(Settings.ParserOptions);
        }

        public LiquefierSettings Settings { get; }

        public static string LiquefyObject<T>(T obj, string? template = null) where T : class 
            => Instance.Liquefy(obj, template);

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
            var resolver = new TemplateResolver(Settings.TemplateFolder);
            var parsedTemplate = cache[key];
            if (parsedTemplate == null) {
                template ??= File.ReadAllText(resolver.ResolveTemplate<T>());
                cache[key] = parsedTemplate = parser.Parse(template);
            }
            return parsedTemplate;
        }

        public sealed class LiquefierSettings
        {
            public FluidParserOptions ParserOptions { get; } = new();
            public TemplateOptions TemplateOptions { get; } = new();
            public string TemplateFolder { get; set; } = "liquefier";
        }

    }

}
