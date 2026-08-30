using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HelixSourceGenerator.Shared;

internal static class GeneratorAnalysis {
  internal static readonly SymbolDisplayFormat TypeDisplayFormat =
    SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
      SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
      SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );
  internal static readonly SymbolDisplayFormat TypeDisplayFormatWithoutGlobal =
    TypeDisplayFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

  internal static Location LocationOf(ISymbol symbol) {
    return symbol.Locations.FirstOrDefault(location => location.IsInSource) ??
      symbol.Locations.FirstOrDefault() ?? Location.None;
  }

  internal static bool IsPartial(INamedTypeSymbol type) {
    return type.DeclaringSyntaxReferences.Any(reference =>
      reference.GetSyntax() is TypeDeclarationSyntax declaration &&
      declaration.Modifiers.Any(SyntaxKind.PartialKeyword)
    );
  }

  internal static INamedTypeSymbol FirstNonPartialContainingType(INamedTypeSymbol type) {
    for (var containing = type.ContainingType;
      containing is not null;
      containing = containing.ContainingType) {
      if (!IsPartial(containing))
        return containing;
    }
    return null;
  }

  internal static bool HasTypeParameters(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.ContainingType) {
      if (current.TypeParameters.Length != 0)
        return true;
    }
    return false;
  }

  internal static IReadOnlyList<IFieldSymbol> InstanceFields(INamedTypeSymbol type) {
    using var profile = MixinProfiler.Measure("roslyn.analysis.instance_fields");
    return [
      .. type.GetMembers()
        .OfType<IFieldSymbol>()
        .Where(field => !field.IsStatic && !field.IsImplicitlyDeclared)
        .OrderBy(SourceOrder)
    ];
  }

  internal static int SourceOrder(ISymbol symbol) {
    var location = symbol.Locations.FirstOrDefault(item => item.IsInSource);
    return location?.SourceSpan.Start ?? int.MaxValue;
  }

  internal static bool InheritsFrom(INamedTypeSymbol type, string metadataName) {
    for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType) {
      if (baseType.ToDisplayString() == metadataName)
        return true;
    }
    return false;
  }

  internal static bool Implements(INamedTypeSymbol type, string metadataName) {
    return type.AllInterfaces.Any(candidate => candidate.ToDisplayString() == metadataName);
  }

  internal static AttributeData Attribute(ISymbol symbol, string metadataName) {
    using var profile = MixinProfiler.Measure("roslyn.analysis.attribute");
    ImmutableArray<AttributeData> attributes;
    using (MixinProfiler.Measure("roslyn.analysis.attribute.get"))
      attributes = symbol.GetAttributes();
    using (MixinProfiler.Measure("roslyn.analysis.attribute.match"))
      return attributes.FirstOrDefault(item => item.AttributeClass?.ToDisplayString() == metadataName);
  }

  internal static ITypeSymbol TypeArgument(AttributeData attribute, string name) {
    return attribute.NamedArguments.FirstOrDefault(item => item.Key == name).Value.Value as ITypeSymbol;
  }

  internal static string StringArgument(
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

  internal static bool BooleanArgument(
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

  internal static int Int32Argument(AttributeData attribute, string name, int defaultValue) {
    foreach (var argument in attribute.NamedArguments) {
      if (argument.Key != name) continue;
      return TryConvertToInt32(argument.Value.Value, out var result) ? result : int.MinValue;
    }
    return defaultValue;
  }

  internal static bool TryConvertToInt32(object value, out int result) {
    try {
      result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
      return true;
    } catch (Exception) {
      result = 0;
      return false;
    }
  }

  internal static Accessibility EffectiveAccessibility(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.ContainingType) {
      if (current.DeclaredAccessibility != Accessibility.Public)
        return Accessibility.Internal;
    }
    return Accessibility.Public;
  }

  internal static string AccessibilityText(Accessibility accessibility) {
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

  internal static bool IsValidIdentifier(string name) {
    return !string.IsNullOrWhiteSpace(name) &&
      (SyntaxFacts.IsValidIdentifier(name) ||
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
        SyntaxFacts.GetContextualKeywordKind(name) !=
        SyntaxKind.None);
  }

  internal static string EscapeIdentifier(string identifier) {
    return SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None ||
      SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None
        ? "@" + identifier
        : identifier;
  }

  internal static bool ContainsPointer(ITypeSymbol type) {
    if (type.TypeKind == TypeKind.Pointer || type.TypeKind == TypeKind.FunctionPointer) return true;
    if (type is IArrayTypeSymbol array) return ContainsPointer(array.ElementType);
    return type is INamedTypeSymbol named && named.TypeArguments.Any(ContainsPointer);
  }

  internal static IReadOnlyList<string> CollectUsings(INamedTypeSymbol type) {
    using var profile = MixinProfiler.Measure("roslyn.analysis.collect_usings");
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

internal static class GeneratorSource {
  internal static string GetCompositionId(string name = null) {
    return GetId("GetCompositionId", name);
  }

  internal static string GetTypeId(string name = null) {
    return GetId("GetTypeId", name);
  }

  internal static TypeWrapper WrapType(
    INamedTypeSymbol type,
    string hintSuffix,
    IReadOnlyList<string> usings = null,
    string hintDiscriminator = null,
    string baseType = null,
    IReadOnlyList<string> typeAttributes = null
  ) {
    using var profile = MixinProfiler.Measure("roslyn.source.wrap_type");
    var chain = ContainingTypes(type);
    var namespaceName = type.ContainingNamespace is { IsGlobalNamespace: false } ns
      ? ns.ToDisplayString()
      : null;
    var hintParts = new List<string> { namespaceName ?? "global" };
    hintParts.AddRange(chain.Select(item => item.MetadataName));
    if (!string.IsNullOrEmpty(hintDiscriminator)) hintParts.Add(hintDiscriminator);
    return new TypeWrapper(
      type,
      chain,
      namespaceName,
      usings,
      baseType,
      typeAttributes,
      Sanitize(string.Join(".", hintParts)) + "." + hintSuffix + ".g.cs"
    );
  }

  internal static string BuildSource(Action<SharpStringBuilder> build) {
    var builder = new SharpStringBuilder();
    builder.GeneratedFile();
    build(builder);
    return builder.ToString();
  }

  private static string GetId(string method, string name) {
    var argument = name is null ? "" : SyntaxFactory.Literal(name).ToFullString();
    return $"global::{GeneratorStrings.Types.CompositionId}.{method}({argument})";
  }

  private static List<INamedTypeSymbol> ContainingTypes(INamedTypeSymbol type) {
    var chain = new List<INamedTypeSymbol>();
    for (var current = type; current is not null; current = current.ContainingType) chain.Add(current);
    chain.Reverse();
    return chain;
  }

  internal static string TypeKeyword(INamedTypeSymbol type) {
    if (type.IsRecord) return type.TypeKind == TypeKind.Struct ? "record struct" : "record";
    return type.TypeKind == TypeKind.Struct ? "struct" :
      type.TypeKind == TypeKind.Interface ? "interface" : "class";
  }

  internal static string TypeParameters(INamedTypeSymbol type) {
    return type.TypeParameters.Length == 0
      ? ""
      : "<" + string.Join(
        ", ", type.TypeParameters.Select(parameter =>
          GeneratorAnalysis.EscapeIdentifier(parameter.Name)
        )
      ) + ">";
  }

  private static string Sanitize(string value) {
    var characters = value.ToCharArray();
    for (var index = 0; index < characters.Length; index++) {
      if (!char.IsLetterOrDigit(characters[index]))
        characters[index] = '_';
    }
    return new string(characters);
  }
}

internal sealed class TypeWrapper {
  private readonly string _baseType;
  private readonly IReadOnlyList<INamedTypeSymbol> _chain;
  private readonly string _namespaceName;
  private readonly INamedTypeSymbol _type;
  private readonly IReadOnlyList<string> _typeAttributes;
  private readonly IReadOnlyList<string> _usings;

  internal TypeWrapper(
    INamedTypeSymbol type,
    IReadOnlyList<INamedTypeSymbol> chain,
    string namespaceName,
    IReadOnlyList<string> usings,
    string baseType,
    IReadOnlyList<string> typeAttributes,
    string hintName
  ) {
    _type = type;
    _chain = chain;
    _namespaceName = namespaceName;
    _usings = usings;
    _baseType = baseType;
    _typeAttributes = typeAttributes;
    HintName = hintName;
  }

  internal string HintName { get; }

  internal DetachedTypeWrapper Detach() {
    using var profile = MixinProfiler.Measure("roslyn.source.detach_wrapper");
    return new DetachedTypeWrapper(
      _namespaceName,
      [
        .. _chain.Select(current => (current.IsStatic ? "static " : "") +
          "partial " + GeneratorSource.TypeKeyword(current) + " " +
          GeneratorAnalysis.EscapeIdentifier(current.Name) +
          GeneratorSource.TypeParameters(current)
        )
      ],
      HintName
    );
  }

  internal string Build(
    Action<SharpStringBuilder> build,
    Action<SharpStringBuilder> after = null,
    bool afterInNamespace = false
  ) {
    return GeneratorSource.BuildSource(builder => {
        if (_usings is not null) {
          foreach (var directive in _usings) builder.AppendLine(directive);
          if (_usings.Count > 0) builder.BlankLine();
        }

        if (afterInNamespace) {
          using (builder.Namespace(_namespaceName)) {
            AppendContainingType(builder, 0, build);
            after?.Invoke(builder);
          }
        } else {
          using (builder.Namespace(_namespaceName)) AppendContainingType(builder, 0, build);
          after?.Invoke(builder);
        }
      }
    );
  }

  private void AppendContainingType(
    SharpStringBuilder builder,
    int index,
    Action<SharpStringBuilder> build
  ) {
    if (index == _chain.Count) {
      build(builder);
      return;
    }

    var current = _chain[index];
    if (_typeAttributes is not null && SymbolEqualityComparer.Default.Equals(current, _type)) {
      foreach (var attribute in _typeAttributes)
        builder.Attribute(attribute);
    }
    var declaration = (current.IsStatic ? "static " : "") +
      "partial " + GeneratorSource.TypeKeyword(current) + " " +
      GeneratorAnalysis.EscapeIdentifier(current.Name) +
      GeneratorSource.TypeParameters(current);
    if (_baseType is not null && SymbolEqualityComparer.Default.Equals(current, _type))
      declaration += " : " + _baseType;
    using (builder.Type(declaration)) AppendContainingType(builder, index + 1, build);
  }
}

internal sealed record DetachedTypeWrapper(
  string NamespaceName,
  IReadOnlyList<string> Declarations,
  string HintName
) {
  internal string Build(
    IReadOnlyList<string> usings,
    IReadOnlyList<string> interfaces,
    IReadOnlyList<string> annotations,
    Action<SharpStringBuilder> build,
    Action<SharpStringBuilder> after = null
  ) {
    return GeneratorSource.BuildSource(builder => {
        if (usings is not null) {
          foreach (var directive in usings) builder.AppendLine(directive);
          if (usings.Count != 0) builder.BlankLine();
        }
        using (builder.Namespace(NamespaceName)) {
          Append(builder, 0, interfaces, annotations, build);
          after?.Invoke(builder);
        }
      }
    );
  }

  private void Append(
    SharpStringBuilder builder,
    int index,
    IReadOnlyList<string> interfaces,
    IReadOnlyList<string> annotations,
    Action<SharpStringBuilder> build
  ) {
    if (index == Declarations.Count) {
      build(builder);
      return;
    }
    var target = index == Declarations.Count - 1;
    if (target && annotations is not null) {
      foreach (var annotation in annotations)
        builder.Attribute(annotation);
    }
    var declaration = Declarations[index];
    if (target && interfaces is { Count: > 0 }) declaration += " : " + string.Join(", ", interfaces);
    using (builder.Type(declaration)) Append(builder, index + 1, interfaces, annotations, build);
  }
}
