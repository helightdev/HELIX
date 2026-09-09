using System.Collections.Generic;
using System.Linq;
using Hix.Compiler;
using Hix.Runtime;

namespace Hix;

public sealed record LiteralHixValue(HixString Value) : IHixValue {
   public HixValueKind Kind => HixValueKind.String;
  public bool IsTruthy(HixThread context) {
    return !string.IsNullOrEmpty(Value.Resolve(context.Strings));
  }

  public HixString Render(HixThread context) {
    return Value;
  }

  public void Fingerprint(HixFingerprintBuilder builder, HixThread context) {
    builder.Append(nameof(LiteralHixValue));
    builder.Append(Value, context.Strings);
  }

  public IHixValue Select(HixThread context, HixString member) {
    return NullHixValue.Instance;
  }

  public object Unlink(HixThread context) {
    return Value.Resolve(context.Strings);
  }

  public bool Equals(IHixValue other) {
    return other is LiteralHixValue value && Equals(value);
  }
}


/// <summary>Compiler-only catalog. Runtime programs never retain these declarations.</summary>
public sealed class HixExpressionPreparedState {
  public HixExpressionPreparedState(HixStringPool strings,
    IReadOnlyList<FunctionDeclarationAst> functions, IReadOnlyList<MixinDeclarationAst> derivations, HixBackend backend = null,
    IReadOnlyDictionary<string, HixPattern> patterns = null) {
    Backend = backend ?? HixCoreBackend.Instance;
    StringPool = strings;
    Functions = functions;
    Derivations = derivations;
    Patterns = patterns ?? new Dictionary<string, HixPattern>();
  }
  public HixBackend Backend { get; }
  public HixStringPool StringPool { get; }
  public IReadOnlyList<FunctionDeclarationAst> Functions { get; }
  public IReadOnlyList<MixinDeclarationAst> Derivations { get; }
  public IReadOnlyDictionary<string, HixPattern> Patterns { get; }
}
