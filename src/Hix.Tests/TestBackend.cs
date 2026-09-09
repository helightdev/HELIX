using Hix;
using Hix.Runtime;
namespace HELIX.SourceGen.Tests;
internal sealed class TestBackend : HixBackend {
  internal static TestBackend Instance { get; } = new();
  protected override void RegisterRoots(System.Collections.Generic.IDictionary<string,HixBackendRoot> roots) {
    foreach (var name in new[] {"this", "target", "attr"}) roots.Add(name, new(name, HixValueKind.Symbol, true));
  }
  public override IHixValue ResolveRoot(HixExecutionContext context, string name) => context.Resolve(name switch {
    "this" => HixExpressionRoot.This, "target" => HixExpressionRoot.Target, _ => HixExpressionRoot.Attribute
  }, HixString.Dynamic(""));
}
