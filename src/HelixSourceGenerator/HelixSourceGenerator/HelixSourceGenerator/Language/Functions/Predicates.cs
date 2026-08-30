using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace HelixSourceGenerator.Language.Functions;

internal sealed class ExistsPredicate() : PredicateFunctionDefinition("exists", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(value is not NullMixinValue and not ErrorMixinValue);
  }
}

internal sealed class AndPredicate() : PredicateFunctionDefinition("and", 1, int.MaxValue) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(
      value.IsTruthy(context) && arguments.All(item => item.IsTruthy(context))
    );
  }
}

internal sealed class OrPredicate() : PredicateFunctionDefinition("or", 1, int.MaxValue) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(
      value.IsTruthy(context) || arguments.Any(item => item.IsTruthy(context))
    );
  }
}

internal sealed class IsPredicate() : PredicateFunctionDefinition("is", 1, 1) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(context.IsType(value, arguments[0].Render(context)));
  }
}

internal sealed class HasPredicate() : PredicateFunctionDefinition("has", 1, 1) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    if (value is not MixinTableValue table)
      return Result(value.Select(context, arguments[0].Render(context)) is not NullMixinValue);
    var expected = Comparable(arguments[0], context);
    return Result(
      table.Entries.Any(item => item.Value.Equals(arguments[0]) ||
        string.Equals(Comparable(item.Value, context), expected, StringComparison.OrdinalIgnoreCase)
      )
    );
  }
}

internal sealed class EqualPredicate() : PredicateFunctionDefinition("eq", 1, 1) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(
      string.Equals(
        Comparable(value, context), Comparable(arguments[0], context),
        StringComparison.OrdinalIgnoreCase
      )
    );
  }
}

internal sealed class MatchesPredicate() : PredicateFunctionDefinition("matches", 1, 1) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(
      Regex.IsMatch(
        value.Render(context).Resolve(context.Strings), arguments[0].Render(context).Resolve(context.Strings)
      )
    );
  }
}

internal sealed class TraitPredicate(string name) : PredicateFunctionDefinition(name, 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    if (context.HasTrait(value, context.Intern(Name))) return Result(true);
    if (value is not RoslynMixinValue roslyn) return Result(false);
    var symbol = roslyn.Value as ISymbol;
    var type = RoslynMixinContext.TypeOf(roslyn.Value);
    return Result(
      Name switch {
        "isSelf" => context is RoslynMixinContext owner &&
          SymbolEqualityComparer.Default.Equals(type, owner.CurrentType),
        "ref" => symbol is IParameterSymbol { RefKind: RefKind.Ref },
        "in" => symbol is IParameterSymbol { RefKind: RefKind.In },
        "out" => symbol is IParameterSymbol { RefKind: RefKind.Out },
        "inout" => symbol is IParameterSymbol { RefKind: RefKind.In or RefKind.Out },
        "argument" => symbol is IParameterSymbol { RefKind: RefKind.None },
        "static" => symbol?.IsStatic == true,
        "async" => symbol is IMethodSymbol { IsAsync: true },
        "public" => symbol?.DeclaredAccessibility == Accessibility.Public,
        "exposed" => symbol?.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal
          or Accessibility.ProtectedOrInternal,
        "top" => type?.ContainingType is null,
        "generic" => type is INamedTypeSymbol { Arity: > 0 },
        "struct" => type?.TypeKind == TypeKind.Struct,
        "class" => type?.IsReferenceType == true,
        "concrete" => type is not { TypeKind: TypeKind.Interface } && type?.IsAbstract != true,
        _ => false
      }
    );
  }
}