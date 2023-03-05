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

    public class LiquidTemplateNotFoundException : LiquefyException
    {
        static string Msg(ObjectTemplateName name) => $"Template not found: {name}";
        public LiquidTemplateNotFoundException(ObjectTemplateName name) :
            base(Msg(name)) {
            TemplateName = name;
        }
        public LiquidTemplateNotFoundException(ObjectTemplateName name, Exception innerException) :
            base(Msg(name), innerException) {
            TemplateName = name;
        }

        public ObjectTemplateName TemplateName { get; }
    }
}
