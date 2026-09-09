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
internal static class GeneratorSource {
  private static string GetId(string method, string name) {
    var argument = name is null ? "" : SyntaxFactory.Literal(name).ToFullString();
    return $"global::{GeneratorStrings.Types.CompositionId}.{method}({argument})";
  }


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
    using var profile = HixProfiler.Measure("roslyn.source.wrap_type");
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
    using var profile = HixProfiler.Measure("roslyn.source.detach_wrapper");
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
