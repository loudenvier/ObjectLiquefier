using System;
using System.IO;

namespace ObjectLiquefier
{
    public class TemplateResolver {
        public static string DefaultExtension { get; set; } = ".liquid";

        public TemplateResolver(string templateFolder) {
            TemplateFolder = templateFolder;
        }

        public string TemplateFolder { get; init; }

        public string ResolveTemplate<T>() where T : class => ResolveTemplate(typeof(T));
        public string ResolveTemplate(Type type) => ResolveTemplate(new ObjectTemplateName(type));
        public string ResolveTemplate(ObjectTemplateName templateName) => 
            TryResolveTemplate(templateName, out var template) 
                ? template : throw new LiquidTemplateNotFoundException(templateName);

        public bool TryResolveTemplate<T>(out string template) where T : class 
            => TryResolveTemplate(typeof(T), out template);
        public bool TryResolveTemplate(Type type, out string template) 
            => TryResolveTemplate(new ObjectTemplateName(type), out template);
        public bool TryResolveTemplate(ObjectTemplateName templateName, out string template) {
            foreach (var name in templateName.PossibleNames) {
                template = Path.Combine(TemplateFolder, name);
                if (File.Exists(template))
                    return true;
            }
            template = "";
            return false;
        }
    }


}
