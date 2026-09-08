using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler;
using Mixins.Runtime;

namespace Mixins;

internal sealed record LiteralMixinValue(MixinString Value) : IMixinValue {
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


/// <summary>Immutable declarations prepared once for a catalog of compilation units.</summary>
public sealed class MixinExpressionPreparedState {
  internal MixinExpressionPreparedState(MixinStringPool strings,
    IReadOnlyList<FunctionDeclarationAst> functions, IReadOnlyList<MixinDeclarationAst> derivations,
    LanguageProgramBindings bindings) {
    StringPool = strings;
    Functions = functions;
    Derivations = derivations;
    Bindings = bindings;
    GlobalScope = new LanguageFunctionScope(functions);
    DerivationScopes = derivations.ToDictionary(declaration => declaration,
      declaration => new LanguageFunctionScope(declaration.Declarations.OfType<FunctionDeclarationAst>(), GlobalScope));
  }
  public MixinStringPool StringPool { get; }
  internal IReadOnlyList<FunctionDeclarationAst> Functions { get; }
  internal IReadOnlyList<MixinDeclarationAst> Derivations { get; }
  internal LanguageFunctionScope GlobalScope { get; }
  internal LanguageProgramBindings Bindings { get; }
  internal IReadOnlyDictionary<MixinDeclarationAst, LanguageFunctionScope> DerivationScopes { get; }
}

internal sealed record MixinExpressionExecutionProgram(
  MixinStringPool StringPool, IReadOnlyList<ExpressionDeclarationAst> Expressions,
  IReadOnlyList<FunctionDeclarationAst> Functions, IReadOnlyList<FunctionDeclarationAst> LocalFunctions,
  IReadOnlyList<MixinDeclarationAst> Derivations,
  LanguageFunctionScope GlobalScope,
  IReadOnlyDictionary<MixinDeclarationAst, LanguageFunctionScope> DerivationScopes,
  LanguageProgramBindings Bindings
) {
  internal LanguageFunctionScope Scope { get; } = new(LocalFunctions, GlobalScope);
  internal string ExecutableIr => Bindings.RenderExecutableIr(Expressions);
}
