using Fluid;

namespace ObjectLiquefier
{
    public sealed class LiquefierSettings
    {
        public FluidParserOptions ParserOptions { get; } = new();
        public TemplateOptions TemplateOptions { get; } = new();
        public string TemplateFolder { get; set; } = "liquefier";
    }
}
