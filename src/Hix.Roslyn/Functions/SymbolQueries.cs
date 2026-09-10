using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Hix.Roslyn;
using Hix.Runtime;

namespace Hix.Functions;

public sealed class IsTypeFunction() : RoslynFunctionDefinition("is", 1, resultType: HixValueKind.Bool,
  argumentTypes: new[] {HixValueKind.String}) {
  public override string Documentation => "Tests whether the semantic value matches the specified C# type name.";
  public override IReadOnlyDictionary<int, string> ArgumentReferences => new Dictionary<int, string> { {1, "CSharpType"} };
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return BooleanHixValue.From(context.IsType(value, arguments[0].Render(context)));
  }
}

public sealed class HasMemberFunction() : RoslynFunctionDefinition("has", 1, resultType: HixValueKind.Bool) {
  public override string Documentation => "Tests whether a semantic value exposes the specified member.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return BooleanHixValue.From(value.Select(context, arguments[0].Render(context)) is not NullHixValue);
  }
}

public sealed class TraitFunction(string name) : RoslynFunctionDefinition(name, 0, resultType: HixValueKind.Bool) {
  public override string Documentation => Name switch {
    "isSelf" => "Tests whether the value refers to the current target type.",
    "ref" => "Tests whether a parameter is passed by ref.",
    "in" => "Tests whether a parameter is passed by in.",
    "out" => "Tests whether a parameter is passed by out.",
    "inout" => "Tests whether a parameter is passed by in or out.",
    "argument" => "Tests whether a parameter is passed by value.",
    "static" => "Tests whether a symbol is static.",
    "async" => "Tests whether a method is async.",
    "public" => "Tests whether a symbol is public.",
    "exposed" => "Tests whether a symbol is public, internal, or protected internal.",
    "top" => "Tests whether the associated type is not nested in another type.",
    "generic" => "Tests whether the associated type contains type parameters.",
    "genericMethod" => "Tests whether a method has generic type parameters.",
    "accessible" => "Tests whether the symbol is accessible from the current target.",
    "struct" => "Tests whether the associated type is a struct.",
    "class" => "Tests whether the associated type is a reference type.",
    "concrete" => "Tests whether the associated type is neither an interface nor abstract.",
    "partial" => "Tests whether the associated declaration is partial.",
    "field" => "Tests whether the symbol is a field.",
    "property" => "Tests whether the symbol is a property.",
    "method" => "Tests whether the symbol is a method.",
    "event" => "Tests whether the symbol is an event.",
    "parameter" => "Tests whether the symbol is a parameter.",
    "typeSymbol" => "Tests whether the value represents a type symbol.",
    "referenceType" => "Tests whether the associated type is a reference type.",
    "valueType" => "Tests whether the associated type is a value type.",
    "nullable" => "Tests whether the associated type is nullable.",
    "pointer" => "Tests whether the associated type is a pointer.",
    "containsPointer" => "Tests whether the type contains pointer data.",
    "enum" => "Tests whether the associated type is an enum.",
    "primitive" => "Tests whether the associated type is a supported primitive type.",
    "parameterDefault" => "Tests whether a parameter has an explicit default value; null also satisfies this predicate.",
    "nonEmptyStringConstant" => "Tests whether the value is a nonempty string constant.",
    "equatableSelf" => "Tests whether the type implements IEquatable of itself.",
    "typedEqualsSelf" => "Tests whether the type exposes a typed Equals method for itself.",
    "ordinaryTypedEqualsSelf" => "Tests whether the type declares an ordinary typed Equals method for itself.",
    "objectEquals" => "Tests whether the type provides an object-based Equals implementation.",
    "hashCode" => "Tests whether the type provides a GetHashCode implementation.",
    _ => ""
  };

  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    if (context.HasTrait(value, context.ResolveString(Name))) return BooleanHixValue.From(true);
    if (value is NullHixValue) return BooleanHixValue.From(Name == "parameterDefault");
    if (value is not RoslynHixValue roslyn) return BooleanHixValue.From(false);
    var symbol = roslyn.Value as ISymbol;
    var type = HixRoslynContext.TypeOf(roslyn.Value);
    return BooleanHixValue.From(
      Name switch {
        "isSelf" => context.Context is HixRoslynContext owner &&
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
        "generic" => type is INamedTypeSymbol namedType && GeneratorAnalysis.HasTypeParameters(namedType),
        "accessible" => context.Context is HixRoslynContext access && symbol != null && access.IsAccessible(symbol),
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
