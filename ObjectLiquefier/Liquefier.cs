using Fluid;
using SpookilySharp;
using System;
using System.IO;

namespace ObjectLiquefier
{
    public partial class Liquefier
    {
        private readonly TemplateCache cache = new();
        private readonly FluidParser parser;

        public static Func<LiquefierSettings> DefaultSettings { get; set; } = () => new LiquefierSettings();

        private readonly static Lazy<Liquefier> instance = new(() => new Liquefier());
        public static Liquefier Instance => instance.Value;

        public Liquefier(Action<LiquefierSettings>? configAction = null) {
            Settings = DefaultSettings?.Invoke() ?? new();
            configAction?.Invoke(Settings);
            parser = new(Settings.ParserOptions);
            parser.RegisterExpressionTag("liquefy", new LiquefyExpressionTag(this).Tag);
        }

        public LiquefierSettings Settings { get; }

        /// <summary>
        /// Liquefies the <paramref name="obj"/> with default settings (<see cref="DefaultSettings"/>) 
        /// using a liquid template based on the object's type and inheritance hierarchy,
        /// or an ad-hoc template passed in <paramref name="template"/>.
        /// </summary>
        /// <typeparam name="T">The type that will be used for template resolution</typeparam>
        /// <param name="obj">The object passed to the liguid template engine</param>
        /// <param name="template">An optional ad-hoc template to be used</param>
        /// <returns> A <see cref="string"/> with the result of applying the found or ad-hoc <paramref name="template"/> to <paramref name="obj"/>,
        /// or an empty string (<see cref="string.Empty"/>) if no template is found.</returns>
        public static string LiquefyObject<T>(T obj, string? template = null) where T : class 
            => Instance.Liquefy(obj, template);

        /// <summary>
        /// Liquefies the <paramref name="obj"/> using a liquid template based on the object's type and inheritance hierarchy,
        /// or an ad-hoc template passed in <paramref name="template"/>.
        /// </summary>
        /// <typeparam name="T">The type that will be used for template resolution</typeparam>
        /// <param name="obj">The object passed to the liguid template engine</param>
        /// <param name="template">An optional ad-hoc template to be used</param>
        /// <returns> A <see cref="string"/> with the result of applying the found or ad-hoc <paramref name="template"/> to <paramref name="obj"/>,
        /// or an empty string (<see cref="string.Empty"/>) if no template is found.</returns>
        public string Liquefy<T>(T obj, string? template=null) where T : class {
            var templateKey = GetTemplateKey(obj, template);
            var parsedTemplate = FindTemplate(obj.GetType(),templateKey, template);
            if (parsedTemplate is null) return string.Empty;
            var context = new TemplateContext(obj, Settings.TemplateOptions);
            return parsedTemplate.Render(context);
        }
        public bool TryLiquefy<T>(T obj, out string liquefied, string? template = null) where T : class
            => (liquefied = Liquefy(obj, template)) != string.Empty;

        public string GetTemplateKey(object obj, string? template = null) 
            => template == null ? obj.GetType().FullName : GetAdHocTemplateKey(template);
        public string GetAdHocTemplateKey(string template) => template.SpookyHash128().ToString();

        public IFluidTemplate? FindTemplate<T>(string key, string? template) where T : class 
            => FindTemplate(typeof(T), key, template);

        public IFluidTemplate? FindTemplate(Type type, string key, string? template) {
            // tries to get a parsed template from the cache or parses and caches it from the resolver
            // a run condition may happen but in the worst case the same template gets compiled more than once
            var resolver = new TemplateResolver(Settings.TemplateFolder);
            var parsedTemplate = cache[key];
            if (parsedTemplate == null) {
                if (template == null) {
                    if (!resolver.TryResolveTemplate(type, out var templateFilename))
                        return null;
                    template = File.ReadAllText(templateFilename);
                }
                cache[key] = parsedTemplate = parser.Parse(template);
            }
            return parsedTemplate;
        }
    }

}
