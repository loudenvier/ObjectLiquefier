using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ObjectLiquefier
{
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public class ObjectTemplateName {
        /// <summary>
        /// Instantiates a new <see cref="ObjectTemplateName"/> with the <see cref="ObjectType"/> set to 
        /// the <see cref="Type"/> passed in <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for <see cref="ObjectType"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> must not be null</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> cannot be <see cref="System.Object"/></exception>
        public ObjectTemplateName(Type type) {
            if (type == null) throw new ArgumentNullException("type");
            if (type == typeof(object)) throw new ArgumentException("type can't be System.Object");
            ObjectType = type;
            TypeName = type.FullName.ToLowerInvariant();
            PossibleNames = ResolvePossibleNames();
        }

        /// <summary>
        /// Resolves all possible template names, from most to least qualified. Considers the whole 
        /// inheritance hierarchy bellow <see cref="System.Object"/>.
        /// </summary>
        /// <returns></returns>
        private string[] ResolvePossibleNames() => ObjectType.GetClassHierarchy()
            .Where(t => t != typeof(object))
            .Select(t => {
                var parts = t.FullName.ToLowerInvariant().Split('.', '+');
                return parts.Select((_, i) =>
                    $"{string.Join(".", parts.Skip(i))}{TemplateResolver.DefaultExtension}");
            }).SelectMany(s => s)
            .ToArray();

        /// <summary>
        /// The <see cref="Type"/> used to create this instance.
        /// </summary>
        public Type ObjectType { get; }
        /// <summary>
        /// The normalized (lowecase) name of the <see cref="ObjectType"/> used to create this instance.
        /// </summary>
        public string TypeName { get; }
        /// <summary>
        /// All possible template names already in resolution order (higher priority first).
        /// </summary>
        public string[] PossibleNames { get; }
        /// <summary>
        /// Converts the object to a <see cref="string"/> with all possible template names concatenated.
        /// </summary>
        /// <returns>Possible template names separated by '>'</returns>
        public override string ToString() => string.Join(">", PossibleNames);
    }
}
