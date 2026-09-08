using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler;
using Mixins.Runtime;

namespace Mixins;

internal sealed record LiteralMixinValue(MixinString Value) : IMixinValue {
   public MixinValueKind Kind => MixinValueKind.String;
  public bool IsTruthy(ExecutionContext context) {
    return !string.IsNullOrEmpty(Value.Resolve(context.Strings));
  }

  public MixinString Render(ExecutionContext context) {
    return Value;
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(LiteralMixinValue));
    builder.Append(Value.Resolve(context.Strings));
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return Value.Resolve(context.Strings);
  }

  public bool Equals(IMixinValue other) {
    return other is LiteralMixinValue value && Equals(value);
  }
}


/// <summary>Compiler-only catalog. Runtime programs never retain these declarations.</summary>
public sealed class MixinExpressionPreparedState {
  internal MixinExpressionPreparedState(MixinStringPool strings,
    IReadOnlyList<FunctionDeclarationAst> functions, IReadOnlyList<MixinDeclarationAst> derivations) {
    StringPool = strings;
    Functions = functions;
    Derivations = derivations;
  }
  public MixinStringPool StringPool { get; }
  internal IReadOnlyList<FunctionDeclarationAst> Functions { get; }
  internal IReadOnlyList<MixinDeclarationAst> Derivations { get; }
}
