using System;
using System.Collections.Immutable;
using System.Linq;
using Helix.Weave;

namespace Fixtures;

[Mixin(Unit.Method)]
public sealed class CommandAttribute : Attribute {
  public CommandAttribute(string name = null) => Name = name;
  public string Name { get; init; }
  public string Description { get; init; }

  /// <summary>Everything the late pass is allowed to know: detached, equatable, no symbols.</summary>
  public sealed record Carry(string Method, string Name, string Description, ImmutableArray<PureTypeRef> Params);

  [Prelude]
  static Carry Prelude(MethodTarget t, CommandAttribute a) {
    var parameters = t.Params.Select(p => p.Type.Pure()).ToImmutableArray();
    return new Carry(t.Name, Lit.Of(a.Name ?? t.Name), Lit.Of(a.Description), parameters);
  }

  [Late]
  static void Late(LateTarget t, Carry c) {
    var types = Code.Join(c.Params, p => $"typeof({p.Qualified})");
    t.Inject(Targets.Configure, 0,
      $"registry.Add({c.Name}, {c.Description}, new System.Type[] {{ {types} }}, args => {c.Method}());");
  }
}
