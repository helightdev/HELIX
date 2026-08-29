using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HelixSourceGenerator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace HELIX.SourceGen.Tests;

internal static class MixinTestDriver {
  internal static GeneratorDriver Create(
    CSharpCompilation compilation,
    IEnumerable<ISourceGenerator> generators = null,
    IEnumerable<AdditionalText> additionalTexts = null
  ) {
    var files = (additionalTexts ?? []).ToList();
    var legacy = LegacyAnnotations(compilation);
    if (!string.IsNullOrWhiteSpace(legacy))
      files.Add(new TextFile("/tests/Legacy.HelixSourceGenerator.additionalfile", legacy));
    return CSharpGeneratorDriver.Create(
      generators ?? [new MixinGenerator().AsSourceGenerator()],
      files,
      (CSharpParseOptions)compilation.SyntaxTrees.First().Options
    );
  }

  private static string LegacyAnnotations(CSharpCompilation compilation) {
    var result = new StringBuilder();
    foreach (var type in Types(compilation.Assembly.GlobalNamespace)) {
      var expressions = type.GetAttributes().Where(attribute =>
        attribute.AttributeClass?.ToDisplayString() == "HELIX.MixinExpressionAttribute"
      ).ToArray();
      var targetDefinitions = type.GetAttributes().Where(attribute =>
        attribute.AttributeClass?.ToDisplayString() == "HELIX.MixinDefineTargetAttribute"
      ).ToArray();
      if (expressions.Length == 0 && targetDefinitions.Length == 0) continue;
      result.Append("@ANNOTATION<").Append(type.ToDisplayString()).AppendLine(">");
      foreach (var definition in targetDefinitions) {
        if (definition.ConstructorArguments.Length < 2) continue;
        result.Append("@DEFINE_TARGET<")
          .Append(Convert.ToString(definition.ConstructorArguments[0].Value)?.TrimStart('$'))
          .Append("><").Append(Convert.ToString(definition.ConstructorArguments[1].Value))
          .AppendLine(">");
      }
      var preludes = new StringBuilder();
      var late = new StringBuilder();
      foreach (var attribute in expressions) AppendExpression(preludes, late, attribute);
      if (preludes.Length != 0)
        result.AppendLine("@PRELUDE").Append(preludes).AppendLine("@END");
      result.Append(late);
      result.AppendLine("@END");
    }
    return result.ToString();
  }

  private static void AppendExpression(
    StringBuilder preludes,
    StringBuilder lateExpressions,
    AttributeData attribute
  ) {
    var targets = Array.Empty<string>();
    var orders = Array.Empty<int>();
    string expression;
    if (attribute.ConstructorArguments.Length == 1) {
      expression = attribute.ConstructorArguments[0].Value as string ?? "";
    } else if (attribute.ConstructorArguments.Length == 3) {
      targets = Values(attribute.ConstructorArguments[0]).Select(value => value.Value as string ?? "").ToArray();
      orders = Values(attribute.ConstructorArguments[1]).Select(value => Convert.ToInt32(value.Value)).ToArray();
      expression = attribute.ConstructorArguments[2].Value as string ?? "";
    } else return;

    var targetOrders = new Dictionary<string, int>(StringComparer.Ordinal);
    for (var index = 0; index < Math.Min(targets.Length, orders.Length); index++)
      targetOrders[targets[index]] = orders[index];
    expression = ConvertTargets(expression, targetOrders);
    var preludeMember = attribute.AttributeClass?.GetMembers("Prelude").Length > 0;
    var configuredPrelude = attribute.NamedArguments.FirstOrDefault(item => item.Key == "Prelude").Value.Value as string;
    var configuredLate = attribute.NamedArguments.FirstOrDefault(item => item.Key == "LateExpression").Value.Value as string;
    string prelude;
    string late;
    if (configuredLate is not null) {
      prelude = expression;
      late = ConvertTargets(configuredLate, targetOrders);
    } else if (preludeMember) {
      prelude = configuredPrelude ?? "";
      late = expression;
    } else {
      prelude = expression;
      late = "";
    }
    if (!string.IsNullOrWhiteSpace(prelude)) preludes.AppendLine(CloseOpenScope(prelude));
    if (!string.IsNullOrWhiteSpace(late)) lateExpressions.AppendLine(CloseOpenScope(late));
  }

  private static string CloseOpenScope(string expression) {
    var scopeOpen = false;
    foreach (var line in expression.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')) {
      var trimmed = line.TrimStart();
      if (trimmed.StartsWith("@SCOPE", StringComparison.Ordinal)) scopeOpen = true;
      else if (trimmed.StartsWith("@END", StringComparison.Ordinal)) scopeOpen = false;
    }
    return scopeOpen ? expression + "\n@END" : expression;
  }

  private static string ConvertTargets(string expression, IReadOnlyDictionary<string, int> targets) {
    if (string.IsNullOrEmpty(expression) || targets.Count == 0) return expression ?? "";
    var single = targets.Count == 1 ? targets.First() : default;
    return Regex.Replace(
      expression,
      @"(?m)^(?<indent>\s*)@CODE(?:<(?<target>[^>]+)>)?(?<rest>\s.*|$)",
      match => {
        var target = match.Groups["target"].Success ? match.Groups["target"].Value : null;
        KeyValuePair<string, int> selected;
        if (target is null || target == "TARGET") selected = single;
        else if (!targets.TryGetValue(target, out var order)) return match.Value;
        else selected = new KeyValuePair<string, int>(target, order);
        if (string.IsNullOrEmpty(selected.Key)) return match.Value;
        return match.Groups["indent"].Value + "@MIXIN<" + selected.Key + "><" + selected.Value + ">" +
          match.Groups["rest"].Value;
      }
    );
  }

  private static IReadOnlyList<TypedConstant> Values(TypedConstant value) =>
    value.Kind == TypedConstantKind.Array ? value.Values : new[] { value };

  private static IEnumerable<INamedTypeSymbol> Types(INamespaceSymbol root) {
    foreach (var nested in root.GetTypeMembers().SelectMany(Types)) yield return nested;
    foreach (var child in root.GetNamespaceMembers())
      foreach (var type in Types(child)) yield return type;
  }

  private static IEnumerable<INamedTypeSymbol> Types(INamedTypeSymbol root) {
    yield return root;
    foreach (var type in root.GetTypeMembers().SelectMany(Types)) yield return type;
  }

  private sealed class TextFile : AdditionalText {
    private readonly SourceText _text;
    internal TextFile(string path, string text) { Path = path; _text = SourceText.From(text); }
    public override string Path { get; }
    public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default) => _text;
  }
}
