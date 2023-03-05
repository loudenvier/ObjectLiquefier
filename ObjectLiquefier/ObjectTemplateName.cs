using System;
using System.Diagnostics;
using System.Linq;

namespace ObjectLiquefier
{
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public class ObjectTemplateName
    {
        public ObjectTemplateName(Type t) {
            ObjectType = t;
            TypeName = t.FullName.ToLowerInvariant();
            NameParts = ParseNameParts();
            PossibleNames = ResolvePossibleNames();
        }
        private string[] ParseNameParts() => TypeName.Split('.', '+');

        private string[] ResolvePossibleNames() => NameParts.Select((_, i) 
            => $"{string.Join('.', NameParts.Skip(i))}{TemplateResolver.DefaultExtension}").ToArray();

        public Type ObjectType { get; }
        public string TypeName { get; }
        public string[] NameParts { get; }
        public string[] PossibleNames { get; }
        
        public override string ToString() => string.Join(">", PossibleNames);
    }
}
