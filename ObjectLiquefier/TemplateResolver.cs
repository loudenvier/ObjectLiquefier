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
        public  string ResolveTemplate(ObjectTemplateName templateName) {
            foreach(var name in templateName.PossibleNames) {
                var templateFileName = Path.Combine(TemplateFolder, name);
                if (File.Exists(templateFileName))
                    return templateFileName;
            }
            throw new LiquidTemplateNotFoundException(templateName);
        }
    }


}
