using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Helix.Weave.Lowering;

public sealed class LoweringError {
  public LoweringError(string message, Location location) {
    Message = message;
    Location = location;
  }

  public string Message { get; }
  public Location Location { get; }
  public override string ToString() {
    var span = Location?.GetLineSpan();
    return span is null || Location.SourceTree is null
      ? Message
      : $"{span.Value.Path}({span.Value.StartLinePosition.Line + 1}): {Message}";
  }
}

/// <summary>Reads <c>[Mixin]</c> dataclasses out of the authoring compilation.</summary>
public static class MixinDiscovery {
  public static IReadOnlyList<MixinDeclaration> Discover(Compilation compilation, List<LoweringError> errors) {
    var result = new List<MixinDeclaration>();

    foreach (var tree in compilation.SyntaxTrees) {
      if (tree.FilePath.EndsWith(".lib.cs", System.StringComparison.OrdinalIgnoreCase)) continue;
      var model = compilation.GetSemanticModel(tree);

      foreach (var syntax in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()) {
        if (model.GetDeclaredSymbol(syntax) is not INamedTypeSymbol symbol) continue;
        var mixinAttribute = symbol.GetAttributes().FirstOrDefault(
          a => a.AttributeClass?.ToDisplayString() == "Helix.Weave.MixinAttribute");
        if (mixinAttribute is null) continue;

        var declaration = Build(symbol, syntax, mixinAttribute, model, errors);
        if (declaration is not null) result.Add(declaration);
      }
    }

    return result.OrderBy(d => d.AttributeMetadataName, System.StringComparer.Ordinal).ToList();
  }

  private static MixinDeclaration Build(INamedTypeSymbol symbol, ClassDeclarationSyntax syntax,
                                        AttributeData mixinAttribute, SemanticModel model,
                                        List<LoweringError> errors) {
    var unit = mixinAttribute.ConstructorArguments.Length > 0 && mixinAttribute.ConstructorArguments[0].Value is int raw
      ? (Unit)raw
      : Unit.Type;
    var attributeName = Named(mixinAttribute, "Attribute") as string
                        ?? symbol.ToDisplayString(SymbolFacts.MetadataFormat);
    var order = Named(mixinAttribute, "Order") is int value ? value : 0;

    var declaration = new MixinDeclaration {
      Syntax = syntax,
      Symbol = symbol,
      Namespace = symbol.ContainingNamespace is { IsGlobalNamespace: false } ns ? ns.ToDisplayString() : null,
      Name = symbol.Name,
      AttributeMetadataName = attributeName,
      Unit = unit,
      Order = order,
      WholeCompilation = Named(mixinAttribute, "WholeCompilation") is true,
      SourcePath = syntax.SyntaxTree.FilePath,
    };

    foreach (var member in syntax.Members) {
      switch (member) {
        case MethodDeclarationSyntax method when PassOf(method, model) is { } pass:
          var methodSymbol = model.GetDeclaredSymbol(method);
          declaration.Methods.Add(new WeaveMethod {
            Syntax = method,
            Symbol = methodSymbol,
            Pass = pass,
            Carry = pass == WeavePass.Prelude ? methodSymbol?.ReturnType as INamedTypeSymbol : null,
            TargetParameterType = methodSymbol?.Parameters.FirstOrDefault()?.Type.ToDisplayString(),
          });
          break;

        case PropertyDeclarationSyntax property:
          if (model.GetDeclaredSymbol(property) is IPropertySymbol { IsStatic: false } propertySymbol)
            declaration.Decodable.Add(propertySymbol);
          declaration.RelocatedMembers.Add(property);
          break;

        case FieldDeclarationSyntax field:
          foreach (var variable in field.Declaration.Variables)
            if (model.GetDeclaredSymbol(variable) is IFieldSymbol { IsStatic: false, IsConst: false } fieldSymbol)
              declaration.Decodable.Add(fieldSymbol);
          declaration.RelocatedMembers.Add(field);
          break;

        // Nested carry records and helper types travel with the mixin.
        case RecordDeclarationSyntax:
        case ClassDeclarationSyntax:
        case StructDeclarationSyntax:
        case EnumDeclarationSyntax:
          declaration.RelocatedMembers.Add(member);
          break;

        case MethodDeclarationSyntax plain:
          // Un-annotated helpers stay with the mixin.
          declaration.RelocatedMembers.Add(plain);
          break;

        case ConstructorDeclarationSyntax:
          // Deliberately not relocated: a constructor exists to declare the attribute's
          // positional argument order, which the decoder reads off the symbol. The relocated
          // class needs its parameterless form instead.
          break;
      }
    }

    Validate(declaration, errors);
    return declaration;
  }

  private static void Validate(MixinDeclaration declaration, List<LoweringError> errors) {
    var aliases = declaration.Methods.Where(m => m.Pass == WeavePass.Aliases).ToList();
    if (aliases.Count > 1)
      errors.Add(new LoweringError($"{declaration.Name} declares more than one [Aliases] method",
        aliases[1].Syntax.Identifier.GetLocation()));

    var single = declaration.Methods.Where(m => m.Pass == WeavePass.Single).ToList();
    var prelude = declaration.Methods.Where(m => m.Pass == WeavePass.Prelude).ToList();
    var late = declaration.Methods.Where(m => m.Pass == WeavePass.Late).ToList();

    if (single.Count + prelude.Count == 0)
      errors.Add(new LoweringError($"{declaration.Name} has no [Weave] or [Prelude] method",
        declaration.Syntax.Identifier.GetLocation()));

    if (single.Count > 0 && prelude.Count > 0)
      errors.Add(new LoweringError($"{declaration.Name} declares both [Weave] and [Prelude]; pick one",
        declaration.Syntax.Identifier.GetLocation()));

    if (single.Count > 1 || prelude.Count > 1 || late.Count > 1)
      errors.Add(new LoweringError($"{declaration.Name} declares more than one method per pass",
        declaration.Syntax.Identifier.GetLocation()));

    if (late.Count > 0 && prelude.Count == 0)
      errors.Add(new LoweringError($"{declaration.Name} has a [Late] pass but no [Prelude] to feed it",
        late[0].Syntax.Identifier.GetLocation()));

    foreach (var preludeMethod in prelude) {
      var hasLate = late.Count > 0;
      var returnsCarry = preludeMethod.Symbol is { ReturnsVoid: false };
      if (hasLate && !returnsCarry)
        errors.Add(new LoweringError(
          $"{declaration.Name}'s [Prelude] must return the carry record its [Late] takes",
          preludeMethod.Syntax.Identifier.GetLocation()));
      if (!hasLate && returnsCarry)
        errors.Add(new LoweringError(
          $"{declaration.Name}'s [Prelude] returns a carry but there is no [Late] pass to receive it",
          preludeMethod.Syntax.Identifier.GetLocation()));
    }

    if (declaration.Unit == Unit.Derivation) ValidateDerivation(declaration, errors);

    PurityCheck.Verify(declaration, errors);
  }

  /// <summary>A derivation step takes the property so far and returns the next one.</summary>
  private static void ValidateDerivation(MixinDeclaration declaration, List<LoweringError> errors) {
    foreach (var method in declaration.Methods) {
      if (method.Pass == WeavePass.Aliases) continue;
      if (method.Pass != WeavePass.Single) {
        errors.Add(new LoweringError(
          $"{declaration.Name} is a derivation step, so it declares a single [Weave] method",
          method.Syntax.Identifier.GetLocation()));
        continue;
      }

      var parameters = method.Symbol?.Parameters;
      var shapeIsRight =
        parameters is { Length: 3 } &&
        parameters.Value[0].Type.ToDisplayString() == "Helix.Weave.DerivationTarget" &&
        parameters.Value[2].Type.ToDisplayString() == "Helix.Weave.DerivedProperty" &&
        method.Symbol.ReturnType.ToDisplayString() == "Helix.Weave.DerivedProperty";

      if (!shapeIsRight)
        errors.Add(new LoweringError(
          $"{declaration.Name}'s derivation step must be " +
          "DerivedProperty (DerivationTarget, <attribute>, DerivedProperty)",
          method.Syntax.ParameterList.GetLocation()));
    }
  }

  private static WeavePass? PassOf(MethodDeclarationSyntax method, SemanticModel model) {
    foreach (var list in method.AttributeLists)
      foreach (var attribute in list.Attributes) {
        var name = model.GetTypeInfo(attribute).Type?.ToDisplayString();
        switch (name) {
          case "Helix.Weave.WeaveAttribute": return WeavePass.Single;
          case "Helix.Weave.PreludeAttribute": return WeavePass.Prelude;
          case "Helix.Weave.LateAttribute": return WeavePass.Late;
          case "Helix.Weave.AliasesAttribute": return WeavePass.Aliases;
        }
      }
    return null;
  }

  private static object Named(AttributeData attribute, string name) {
    foreach (var pair in attribute.NamedArguments)
      if (pair.Key == name) return pair.Value.Value;
    return null;
  }
}
