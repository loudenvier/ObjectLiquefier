using Fluid;
using Fluid.Ast;
using Fluid.Values;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ObjectLiquefier
{
    public class LiquefyExpressionTag {

        public LiquefyExpressionTag(Liquefier liquefier) => this.liquefier = liquefier;

        readonly Liquefier liquefier;
        public ValueTask<Completion> Tag(Expression exp, TextWriter w, TextEncoder e, TemplateContext c) {
            var value = exp.EvaluateAsync(c).GetAwaiter().GetResult();
            if (value?.Type == FluidValues.Object) {
                var liquified = liquefier.Liquefy(value.ToObjectValue());
                if (liquified != "") {
                    w.Write(liquified);
                    return Statement.Normal();
                }
            }
             (value ?? NilValue.Instance).WriteTo(w, e, c.CultureInfo);
            return Statement.Normal();
        }
    }

}
