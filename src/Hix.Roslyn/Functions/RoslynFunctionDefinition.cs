using System.Collections.Generic;
using System.Linq;
using Hix.Runtime;
using Hix.Roslyn;
namespace Hix.Functions;
public abstract class RoslynFunctionDefinition : EvaluatedFunctionDefinition {
  protected RoslynFunctionDefinition(string name) : base(name) { }
  protected override IHixValue EvaluateValue(HixCoroutine context, IHixValue value, IReadOnlyList<IHixValue> arguments) {
    if (context.Context is not HixRoslynContext roslyn || value is not RoslynHixValue source) return Apply(context, value, arguments);
    var key = ":" + Name + (arguments.Count == 0 ? "" : "\u001f" + string.Join("\u001f", arguments.Select(item => item.GetType().FullName + "=" + item.Render(context).Resolve(context.Strings))));
    if (roslyn.TryGetDerived(source, key, out var result)) return result;
    result = Apply(context, value, arguments);
    roslyn.StoreDerived(source, key, result);
    return result;
  }
}
