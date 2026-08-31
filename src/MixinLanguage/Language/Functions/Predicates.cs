using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Mixins.Roslyn;
using Mixins.Runtime;

namespace Mixins.Functions;

internal sealed class ExistsPredicate() : PredicateFunctionDefinition("exists", 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(value is not NullMixinValue and not ErrorMixinValue);
  }
}

internal sealed class AndPredicate() : PredicateFunctionDefinition("and", 1,
  MixinLanguageValueKind.Boolean,
  [MixinLanguageValueKind.Boolean, MixinLanguageValueKind.Boolean], variadic: true) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(
      value.IsTruthy(context) && arguments.All(item => item.IsTruthy(context))
    );
  }
}

internal sealed class OrPredicate() : PredicateFunctionDefinition("or", 1,
  MixinLanguageValueKind.Boolean,
  [MixinLanguageValueKind.Boolean, MixinLanguageValueKind.Boolean], variadic: true) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(
      value.IsTruthy(context) || arguments.Any(item => item.IsTruthy(context))
    );
  }
}

internal sealed class IsPredicate() : PredicateFunctionDefinition("is", 1) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(context.IsType(value, arguments[0].Render(context)));
  }
}

internal sealed class HasPredicate() : PredicateFunctionDefinition("has", 1) {
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

internal sealed class EqualPredicate() : PredicateFunctionDefinition("eq", 1) {
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

internal sealed class MatchesPredicate() : PredicateFunctionDefinition("matches", 1,
  MixinLanguageValueKind.Text, [MixinLanguageValueKind.Text]) {
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

internal sealed class TraitPredicate(string name) : PredicateFunctionDefinition(name, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    if (context.HasTrait(value, context.ResolveString(Name))) return Result(true);
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
        "field" => symbol is IFieldSymbol,
        "property" => symbol is IPropertySymbol,
        "method" => symbol is IMethodSymbol,
        "event" => symbol is IEventSymbol,
        "parameter" => symbol is IParameterSymbol,
        "typeSymbol" => symbol is INamedTypeSymbol,
        "referenceType" => type?.IsReferenceType == true,
        "valueType" => type?.IsValueType == true,
        "nullable" => type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T },
        "pointer" => type?.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer,
        "containsPointer" => type is not null && GeneratorAnalysis.ContainsPointer(type),
        "enum" => type?.TypeKind == TypeKind.Enum,
        "primitive" => type?.TypeKind == TypeKind.Enum || type?.SpecialType is
          SpecialType.System_Boolean or SpecialType.System_Byte or SpecialType.System_SByte or
          SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or
          SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64 or
          SpecialType.System_Char or SpecialType.System_Single or SpecialType.System_Double or
          SpecialType.System_Decimal,
        "parameterDefault" => roslyn.Value is TypedConstant constant &&
          (constant.IsNull || constant.Kind is TypedConstantKind.Primitive or TypedConstantKind.Enum),
        "nonEmptyStringConstant" => roslyn.Value is TypedConstant {
          Kind: TypedConstantKind.Primitive, Value: string text
        } && !string.IsNullOrWhiteSpace(text),
        "equatableSelf" => type is INamedTypeSymbol named && named.AllInterfaces.Any(candidate =>
          candidate.OriginalDefinition.MetadataName == "IEquatable`1" &&
          candidate.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System" &&
          candidate.TypeArguments.Length == 1 &&
          SymbolEqualityComparer.Default.Equals(candidate.TypeArguments[0], named)
        ),
        "typedEqualsSelf" => type is INamedTypeSymbol typed && typed.GetMembers().OfType<IMethodSymbol>().Any(method =>
          IsTypedEquals(method, typed) &&
          (method is { Name: "Equals", DeclaredAccessibility: Accessibility.Public } ||
            method.ExplicitInterfaceImplementations.Length != 0)
        ),
        "ordinaryTypedEqualsSelf" => type is INamedTypeSymbol ordinary && ordinary.GetMembers("Equals")
          .OfType<IMethodSymbol>().Any(method => IsTypedEquals(method, ordinary)),
        "objectEquals" => type is INamedTypeSymbol objectOwner && objectOwner.GetMembers("Equals")
          .OfType<IMethodSymbol>().Any(method => !method.IsStatic &&
            method.ReturnType.SpecialType == SpecialType.System_Boolean && method.Parameters.Length == 1 &&
            method.Parameters[0].Type.SpecialType == SpecialType.System_Object
          ),
        "hashCode" => type is INamedTypeSymbol hashOwner && hashOwner.GetMembers("GetHashCode")
          .OfType<IMethodSymbol>().Any(method => !method.IsStatic &&
            method.ReturnType.SpecialType == SpecialType.System_Int32 && method.Parameters.Length == 0
          ),
        _ => false
      }
    );
  }

  private static bool IsTypedEquals(IMethodSymbol method, INamedTypeSymbol type) {
    return !method.IsStatic && !method.ReturnsByRef && !method.ReturnsByRefReadonly &&
      method.ReturnType.SpecialType == SpecialType.System_Boolean && method.Parameters.Length == 1 &&
      method.Parameters[0].RefKind == RefKind.None &&
      SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, type);
  }
}
