using System;
using System.Runtime.Serialization;

namespace ObjectLiquefier
{
    public class LiquefyException : Exception
    {
        public LiquefyException() { }
        public LiquefyException(string message) : base(message) { }
        public LiquefyException(string message, Exception innerException) : base(message, innerException) { }
        protected LiquefyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class LiquefyTemplateNotFoundException : LiquefyException
    {
        static string Msg(ObjectTemplateName name) => $"Template not found: {name}";
        public LiquefyTemplateNotFoundException(ObjectTemplateName name) :
            base(Msg(name)) {
            TemplateName = name;
        }
        public LiquefyTemplateNotFoundException(ObjectTemplateName name, Exception innerException) :
            base(Msg(name), innerException) {
            TemplateName = name;
        }

        public ObjectTemplateName TemplateName { get; }
    }
}
