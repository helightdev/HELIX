using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Helix.Weave.Lowering;

/// <summary>
/// Enforces the detachment boundary: the late pass sees only its carry, and a carry holds only
/// detached data. Types catch most of this on their own (there is no way back from a
/// PureTypeRef); this is the backstop for what types can't express.
/// </summary>
public static class PurityCheck {
  private static readonly HashSet<string> Attached = new(System.StringComparer.Ordinal) {
    "Microsoft.CodeAnalysis.ISymbol",
    "Microsoft.CodeAnalysis.ITypeSymbol",
    "Microsoft.CodeAnalysis.INamedTypeSymbol",
    "Microsoft.CodeAnalysis.IMethodSymbol",
    "Microsoft.CodeAnalysis.IFieldSymbol",
    "Microsoft.CodeAnalysis.IPropertySymbol",
    "Microsoft.CodeAnalysis.IParameterSymbol",
    "Microsoft.CodeAnalysis.AttributeData",
    "Microsoft.CodeAnalysis.Compilation",
    "Microsoft.CodeAnalysis.SemanticModel",
    "Microsoft.CodeAnalysis.SyntaxNode",
    "Microsoft.CodeAnalysis.Location",
    "Helix.Weave.TypeRef",
    "Helix.Weave.SymbolTarget",
    "Helix.Weave.TypeTarget",
    "Helix.Weave.MethodTarget",
    "Helix.Weave.MemberTarget",
    "Helix.Weave.ParamRef",
    "Helix.Weave.SymbolFacts",
  };

  public static void Verify(MixinDeclaration declaration, List<LoweringError> errors) {
    var late = declaration.Methods.FirstOrDefault(m => m.Pass == WeavePass.Late);
    if (late?.Symbol is null) return;

    VerifyLateSignature(declaration, late, errors);

    var carry = declaration.Methods.FirstOrDefault(m => m.Pass == WeavePass.Prelude)?.Carry;
    if (carry is not null) VerifyCarry(declaration, carry, late, errors, new HashSet<string>(System.StringComparer.Ordinal));
  }

  private static void VerifyLateSignature(MixinDeclaration declaration, WeaveMethod late,
                                          List<LoweringError> errors) {
    var parameters = late.Symbol.Parameters;
    if (parameters.Length != 2 || parameters[0].Type.ToDisplayString() != "Helix.Weave.LateTarget") {
      errors.Add(new LoweringError(
        $"{declaration.Name}'s [Late] must take (LateTarget, <carry>) — the late pass may not see " +
        "the attribute or any symbol, or it becomes an incremental dependency on analyzed source",
        late.Syntax.ParameterList.GetLocation()));
      return;
    }

    foreach (var parameter in parameters.Skip(1))
      if (IsAttached(parameter.Type, out var offender))
        errors.Add(new LoweringError(
          $"{declaration.Name}'s [Late] parameter '{parameter.Name}' is attached ({offender})",
          late.Syntax.ParameterList.GetLocation()));
  }

  private static void VerifyCarry(MixinDeclaration declaration, INamedTypeSymbol carry, WeaveMethod late,
                                  List<LoweringError> errors, HashSet<string> visited) {
    var key = carry.ToDisplayString();
    if (!visited.Add(key)) return;

    foreach (var member in carry.GetMembers()) {
      ITypeSymbol type = member switch {
        IPropertySymbol { IsStatic: false } property => property.Type,
        IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false } field => field.Type,
        _ => null,
      };
      if (type is null) continue;

      if (IsAttached(type, out var offender)) {
        errors.Add(new LoweringError(
          $"carry '{carry.Name}' of {declaration.Name} holds attached state in '{member.Name}' ({offender}); " +
          "detach it in the prelude — e.g. a TypeRef converts to PureTypeRef",
          member.Locations.FirstOrDefault() ?? late.Syntax.GetLocation()));
        continue;
      }

      if (type is INamedTypeSymbol nested && nested.SpecialType == SpecialType.None &&
          nested.ContainingAssembly is not null && !IsKnownDetached(nested))
        VerifyCarry(declaration, nested, late, errors, visited);
    }
  }

  private static bool IsKnownDetached(INamedTypeSymbol type) {
    var name = type.ToDisplayString();
    if (name.StartsWith("Helix.Weave.PureTypeRef", System.StringComparison.Ordinal)) return true;
    if (name.StartsWith("System.", System.StringComparison.Ordinal)) return true;
    return false;
  }

  /// <summary>True when the type, or anything it carries, is bound to the analyzed compilation.</summary>
  public static bool IsAttached(ITypeSymbol type, out string offender) {
    offender = null;
    switch (type) {
      case null:
        return false;
      case IArrayTypeSymbol array:
        return IsAttached(array.ElementType, out offender);
      case INamedTypeSymbol named: {
        var name = named.ConstructedFrom.ToDisplayString();
        var bare = name.Contains("<") ? name.Substring(0, name.IndexOf('<')) : name;
        if (Attached.Contains(bare)) { offender = bare; return true; }
        foreach (var argument in named.TypeArguments)
          if (IsAttached(argument, out offender)) return true;
        foreach (var iface in named.AllInterfaces) {
          var interfaceName = iface.ConstructedFrom.ToDisplayString();
          var bareInterface = interfaceName.Contains("<")
            ? interfaceName.Substring(0, interfaceName.IndexOf('<'))
            : interfaceName;
          if (Attached.Contains(bareInterface)) { offender = bareInterface; return true; }
        }
        for (var current = named.BaseType; current is not null; current = current.BaseType)
          if (Attached.Contains(current.ToDisplayString())) { offender = current.ToDisplayString(); return true; }
        return false;
      }
      default:
        return false;
    }
  }
}
