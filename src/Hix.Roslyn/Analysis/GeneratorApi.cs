using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Hix;
using Hix.Env;

namespace Hix.Roslyn;

public static partial class GeneratorAnalysis {
  public static readonly SymbolDisplayFormat TypeDisplayFormat =
    SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
      SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
      SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );
  public static readonly SymbolDisplayFormat TypeDisplayFormatWithoutGlobal =
    TypeDisplayFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

  private static readonly ConditionalWeakTable<ISymbol, IReadOnlyList<AttributeData>> _attributeCache = new();

  public static Location LocationOf(ISymbol symbol) {
    return symbol.Locations.FirstOrDefault(location => location.IsInSource) ??
      symbol.Locations.FirstOrDefault() ?? Location.None;
  }

  public static bool IsPartial(INamedTypeSymbol type) {
    return type.DeclaringSyntaxReferences.Any(reference =>
      reference.GetSyntax() is TypeDeclarationSyntax declaration &&
      declaration.Modifiers.Any(SyntaxKind.PartialKeyword)
    );
  }

  public static INamedTypeSymbol FirstNonPartialContainingType(INamedTypeSymbol type) {
    for (var containing = type.ContainingType;
      containing is not null;
      containing = containing.ContainingType) {
      if (!IsPartial(containing))
        return containing;
    }
    return null;
  }

  public static bool HasTypeParameters(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.ContainingType) {
      if (current.TypeParameters.Length != 0)
        return true;
    }
    return false;
  }

  public static IReadOnlyList<IFieldSymbol> InstanceFields(INamedTypeSymbol type) {
    using var profile = HixProfiler.Measure("roslyn.analysis.instance_fields");
    return [
      .. type.GetMembers()
        .OfType<IFieldSymbol>()
        .Where(field => !field.IsStatic && !field.IsImplicitlyDeclared)
        .OrderBy(SourceOrder)
    ];
  }

  public static int SourceOrder(ISymbol symbol) {
    var location = symbol.Locations.FirstOrDefault(item => item.IsInSource);
    return location?.SourceSpan.Start ?? int.MaxValue;
  }

  public static bool InheritsFrom(INamedTypeSymbol type, string metadataName) {
    for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType) {
      if (baseType.ToDisplayString() == metadataName)
        return true;
    }
    return false;
  }

  public static bool Implements(INamedTypeSymbol type, string metadataName) {
    return type.AllInterfaces.Any(candidate => candidate.ToDisplayString() == metadataName);
  }

  private static bool HasAnyAttributeInSyntaxList(SyntaxList<AttributeListSyntax> attributeLists) {
    return attributeLists.Any(static x => x.Attributes.Any());
  }

  private static bool HasAnyAttributeDeclared(ISymbol symbol) {
    using var profile = HixProfiler.Measure("generator_api.has_any_attribute_declared");

    var declarations = symbol.DeclaringSyntaxReferences;
    if (declarations.IsEmpty) {
      HixProfiler.Increment("generator_api.haad_declarations_empty");
      return true;
    }

    foreach (var reference in declarations) {
      var syntax = reference.GetSyntax();

      var attributeLists = syntax switch {
        VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax field } => field.AttributeLists,

        VariableDeclaratorSyntax { Parent.Parent: EventFieldDeclarationSyntax eventField } => eventField.AttributeLists,

        MemberDeclarationSyntax member => member.AttributeLists,
        BaseParameterSyntax parameter => parameter.AttributeLists,
        LambdaExpressionSyntax lambda => lambda.AttributeLists,
        StatementSyntax statement => statement.AttributeLists,
        _ => default
      };

      if (HasAnyAttributeInSyntaxList(attributeLists)) return true;
    }

    return false;
  }

  public static IReadOnlyList<AttributeData> AttributeList(ISymbol symbol) {
    if (_attributeCache.TryGetValue(symbol, out var cached)) {
      HixProfiler.Increment("generator_api.attribute_cache_hit");
      return cached;
    }
    using var analyzeProfile = HixProfiler.Measure("generator_api.attribute_cache_fill");
    if (!HasAnyAttributeDeclared(symbol)) {
      _attributeCache.Add(symbol, []);
      return [];
    }

    using var resolveProfile = HixProfiler.Measure("generator_api.attribute_cache_resolve");
    var attributes = symbol.GetAttributes();
    _attributeCache.Add(symbol, attributes);
    return attributes;
  }

  public static AttributeData Attribute(ISymbol symbol, string metadataName) {
    using var profile = HixProfiler.Measure("generator_api.attribute");
    var attributes = AttributeList(symbol);
    using (HixProfiler.Measure("generator_api.attribute.match"))
      return attributes.FirstOrDefault(item => item.AttributeClass?.ToDisplayString() == metadataName);
  }

  public static ITypeSymbol TypeArgument(AttributeData attribute, string name) {
    return attribute.NamedArguments.FirstOrDefault(item => item.Key == name).Value.Value as ITypeSymbol;
  }

  public static string StringArgument(
    AttributeData attribute,
    string name,
    string defaultValue = null
  ) {
    foreach (var argument in attribute.NamedArguments) {
      if (argument.Key == name)
        return argument.Value.Value as string;
    }
    return defaultValue;
  }

  public static bool BooleanArgument(
    AttributeData attribute,
    string name,
    bool defaultValue = false
  ) {
    foreach (var argument in attribute.NamedArguments) {
      if (argument.Key == name && argument.Value.Value is bool value)
        return value;
    }
    return defaultValue;
  }

  public static int Int32Argument(AttributeData attribute, string name, int defaultValue) {
    foreach (var argument in attribute.NamedArguments) {
      if (argument.Key != name) continue;
      return TryConvertToInt32(argument.Value.Value, out var result) ? result : int.MinValue;
    }
    return defaultValue;
  }

  public static bool TryConvertToInt32(object value, out int result) {
    try {
      result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
      return true;
    } catch (Exception) {
      result = 0;
      return false;
    }
  }

  public static Accessibility EffectiveAccessibility(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.ContainingType) {
      if (current.DeclaredAccessibility != Accessibility.Public)
        return Accessibility.Internal;
    }
    return Accessibility.Public;
  }

  public static string AccessibilityText(Accessibility accessibility) {
    return accessibility switch {
      Accessibility.Public => "public",
      Accessibility.Private => "private",
      Accessibility.Protected => "protected",
      Accessibility.Internal => "internal",
      Accessibility.ProtectedAndInternal => "private protected",
      Accessibility.ProtectedOrInternal => "protected internal",
      _ => "internal"
    };
  }

  public static bool IsValidIdentifier(string name) {
    return !string.IsNullOrWhiteSpace(name) &&
      (SyntaxFacts.IsValidIdentifier(name) ||
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
        SyntaxFacts.GetContextualKeywordKind(name) !=
        SyntaxKind.None);
  }

  public static string EscapeIdentifier(string identifier) {
    return SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None ||
      SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None
        ? "@" + identifier
        : identifier;
  }

  public static bool ContainsPointer(ITypeSymbol type) {
    if (type.TypeKind == TypeKind.Pointer || type.TypeKind == TypeKind.FunctionPointer) return true;
    if (type is IArrayTypeSymbol array) return ContainsPointer(array.ElementType);
    return type is INamedTypeSymbol named && named.TypeArguments.Any(ContainsPointer);
  }

  public static IReadOnlyList<string> CollectUsings(INamedTypeSymbol type) {
    using var profile = HixProfiler.Measure("roslyn.analysis.collect_usings");
    var result = new List<string>();
    var seen = new HashSet<string>(StringComparer.Ordinal);
    foreach (var syntaxReference in type.DeclaringSyntaxReferences) {
      if (syntaxReference.GetSyntax() is not TypeDeclarationSyntax declaration) continue;
      if (declaration.SyntaxTree.GetRoot() is CompilationUnitSyntax compilationUnit) {
        foreach (var directive in compilationUnit.Usings)
          AddUsing(directive, seen, result);
      }
      for (var node = declaration.Parent; node is not null; node = node.Parent) {
        if (node is not BaseNamespaceDeclarationSyntax namespaceDeclaration) continue;
        foreach (var directive in namespaceDeclaration.Usings) AddUsing(directive, seen, result);
      }
    }
    return result;
  }

  private static void AddUsing(
    UsingDirectiveSyntax directive,
    ISet<string> seen,
    ICollection<string> result
  ) {
    var text = directive.WithoutTrivia().NormalizeWhitespace().ToFullString();
    if (seen.Add(text)) result.Add(text);
  }
}

