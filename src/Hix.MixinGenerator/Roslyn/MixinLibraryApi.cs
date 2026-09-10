using Hix.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Hix.Compiler;
using Hix.Diagnostics;
using Hix.Env;
using static Hix.Roslyn.GeneratorAnalysis;
using static Hix.Roslyn.GeneratorDiagnostics.Mixins;

#pragma warning disable RS1035

namespace Hix.Roslyn;

internal static class MixinLibraryApi {
  internal const string AdditionalFileSuffix = ".HelixSourceGenerator.additionalfile";

  internal static MixinCompilation CompileCached(MixinLibraryCatalog catalog) {
    HixCompilerProfiler.Configure(catalog.HasFlag("pragma", "PROFILE"), catalog.ProjectPath, catalog.Key);
    HixProfiler.Configure(catalog.HasFlag("vm", "PROFILE"), catalog.ProjectPath, catalog.Key);
    var writeDisassembly = HixDebugReporter.Configure(
      catalog.HasFlag("pragma", "DISASSEMBLE"), catalog.ProjectPath, catalog.Key
    );
    var compilation = MixinCompilationCache.GetOrCreate(catalog.Key, () => Compile(catalog));
    if (writeDisassembly) HixDebugReporter.Write(BuildDisassembly(compilation));
    return compilation;
  }

  private static string BuildDisassembly(MixinCompilation compilation) {
    using var profile = HixCompilerProfiler.Measure("compiler.disassemble");
    var expressions = compilation.Annotations.Values
      .GroupBy(annotation => annotation.Program.Prelude.Identity + "\u001f" + annotation.Program.Late.Identity,
        StringComparer.Ordinal)
      .Select(group => {
        var annotation = group.First();
        return new HixDebugExpression(
          annotation.Program.Prelude, annotation.Program.Late,
          ImmutableDictionary<string, object>.Empty, ImmutableDictionary<string, object>.Empty,
          string.Join(", ", group.Select(item => item.Definition.Name).OrderBy(name => name, StringComparer.Ordinal)),
          "", "", 0, 0
        );
      }).ToImmutableArray();
    return HixDebugRenderer.BuildTrace(new HixDebugRenderData(
      compilation.StringPool, expressions, false, 0, default, default, default
    ));
  }

  internal static MixinCompilation Compile(MixinLibraryCatalog catalog) {
    var diagnostics = new List<Diagnostic>();
    foreach (var file in catalog.Files.Where(file => !file.Success))
      diagnostics.Add(Diagnostic.Create(InvalidPreparedExpression, Location.None, file.Key,
        file.ErrorLine.ToString(CultureInfo.InvariantCulture), file.Error));
    HixCompilerCatalog prepared;
    try {
      prepared = HixCompiler.PrepareGlobals(catalog.Files.Where(file => file.Success).Select(file => file.Program), HixMixinBackend.Instance);
    } catch (ArgumentException exception) {
      diagnostics.Add(Diagnostic.Create(InvalidLibraryImport, Location.None, "import set", exception.Message));
      prepared = HixCompiler.PrepareGlobals(Array.Empty<CompilationUnitIr>(), HixMixinBackend.Instance);
    }
    var annotations = new Dictionary<string, CompiledHixAnnotation>(StringComparer.Ordinal);
    if (diagnostics.Count == 0) {
      foreach (var annotation in catalog.AnnotationDefinitions) {
        try {
          var syntax = HixCompiler.PrepareIr(annotation.Declaration, prepared);
          var image = new HixBytecodeCompiler(prepared.StringPool).Compile(
            syntax.Prelude.Concat(syntax.Late).ToArray(), syntax.Functions, prepared);
          annotations.Add(annotation.Name, new CompiledHixAnnotation(annotation,
            new CompiledHixProgram(image.ForPass(true), image.ForPass(false), CollectTargets(syntax.Late))));
        } catch (ArgumentException exception) {
          diagnostics.Add(Diagnostic.Create(InvalidPreparedExpression, Location.None, annotation.Name,
            annotation.Declaration.Line.ToString(CultureInfo.InvariantCulture), exception.Message));
        }
      }
    }
    return new MixinCompilation(catalog, prepared.StringPool, prepared, annotations, diagnostics.ToImmutableArray());
  }

  private static IReadOnlyList<MixinTargetReference> CollectTargets(IEnumerable<ExpressionDeclarationIr> expressions) {
    var targets = new List<MixinTargetReference>();
    void Visit(HixIrNode node) {
      if (node is CallExpressionIr {Name: "inject"} call && call.Arguments.Count >= 2) {
        if (call.Arguments[0] is StringExpressionIr target) targets.Add(new(target.Value, false));
        else if (call.Arguments[0] is MemberExpressionIr {Receiver: RootExpressionIr {Name: "carry"}, Member: var carry})
          targets.Add(new(carry, true));
      }
      foreach (var child in node.SemanticChildren) Visit(child);
    }
    foreach (var expression in expressions) Visit(expression);
    return targets.ToArray();
  }

  internal static MixinLibraryFile ReadAdditionalFile(AdditionalText file, CancellationToken cancellationToken) {
    if (!file.Path.EndsWith(AdditionalFileSuffix, StringComparison.OrdinalIgnoreCase)) return null;
    var name = Path.GetFileName(file.Path);
    var key = name.Substring(0, name.Length - AdditionalFileSuffix.Length);
    var source = file.GetText(cancellationToken)?.ToString() ?? "";
    return MixinCompilationCache.GetFile(file.Path, source, () => ParseFile(file.Path, key, source));
  }

  private static MixinLibraryFile ParseFile(string path, string key, string source) {
    var unit = AntlrSyntax.Parse(source, HixMixinBackend.Instance);
    var annotations = new Dictionary<string, MixinAnnotationDefinition>(StringComparer.Ordinal);
    var derivations = new List<MixinDeclarationIr>();
    var configuration = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var metadata in unit.Metadata.Where(metadata => metadata.Name is "pragma" or "vm"))
      foreach (var value in metadata.Values.OfType<StringExpressionIr>())
        configuration[metadata.Name + ":" + value.Value] = "";
    foreach (var declaration in unit.Declarations.OfType<MixinDeclarationIr>()) {
      if (declaration.IsDerivation) { derivations.Add(declaration); continue; }
      var targets = new Dictionary<string, string>(StringComparer.Ordinal);
      foreach (var call in declaration.Declarations.OfType<ExpressionDeclarationIr>()
        .Where(expression => expression.IsPrelude).SelectMany(expression => expression.Body.Statements)
        .OfType<InvocationStatementIr>().Select(statement => statement.Call)) {
        if (call.Arguments.Count != 2 || call.Arguments[0] is not StringExpressionIr first ||
            call.Arguments[1] is not StringExpressionIr second) continue;
        if (call.Name == "defineTarget") targets["$" + first.Value.TrimStart('$')] = second.Value;
      }
      annotations[declaration.Name] = new MixinAnnotationDefinition(declaration.Name, declaration, source, targets);
    }
    var error = unit.Diagnostics.FirstOrDefault();
    return new MixinLibraryFile(key, path, source, error == null, error?.Message, error?.Line ?? 0,
      unit, annotations, derivations, configuration);
  }

  internal static string NormalizeProviderName(string name) {
    var normalized = (name ?? "").Trim();
    return normalized.StartsWith("global::", StringComparison.Ordinal) ? normalized.Substring(8) : normalized;
  }

  internal static IEnumerable<INamedTypeSymbol> AttributeOwners(IEnumerable<ISymbol> symbols) {
    foreach (var symbol in symbols) {
      if (symbol is INamedTypeSymbol type) yield return type;
      foreach (var attribute in symbol.GetAttributes()
        .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
        .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue))
        if (attribute.AttributeClass is { } attributeType) yield return attributeType;
    }
  }

  internal static IEnumerable<MixinAnnotationDefinition> Annotations(INamedTypeSymbol type, MixinLibraryCatalog catalog) {
    if (type != null && catalog != null &&
        catalog.TryGetAnnotation(type.ToDisplayString(TypeDisplayFormat), out var annotation)) yield return annotation;
  }
}

internal sealed record MixinLibraryFile(
  string Key, string Path, string Content, bool Success, string Error, int ErrorLine,
  CompilationUnitIr Program, IReadOnlyDictionary<string, MixinAnnotationDefinition> Annotations,
  IReadOnlyList<MixinDeclarationIr> Derivations, IReadOnlyDictionary<string, string> Configuration
);

internal sealed record MixinAnnotationDefinition(
  string Name, MixinDeclarationIr Declaration, string Source, IReadOnlyDictionary<string, string> TargetDefinitions
);

internal sealed class MixinLibraryCatalog {
  private readonly Dictionary<string, MixinAnnotationDefinition> _annotations;
  private readonly Dictionary<string, MixinLibraryFile> _files;
  private readonly IReadOnlyList<MixinLibraryFile> _orderedFiles;

  internal MixinLibraryCatalog(IEnumerable<MixinLibraryFile> files) {
    using var profile = HixProfiler.Measure("model.library_catalog.create");
    _orderedFiles = [.. (files ?? []).Where(item => item is not null)];
    _files = _orderedFiles
      .GroupBy(item => item.Key, StringComparer.Ordinal)
      .ToDictionary(item => item.Key, item => item.Last(), StringComparer.Ordinal);
    _annotations = _files.Values
      .Where(file => file.Success)
      .SelectMany(file => file.Annotations.Values)
      .GroupBy(annotation => annotation.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.Last(), StringComparer.Ordinal);
    Key = string.Join(
      "\u001e", _orderedFiles.Select(item =>
        item.Key + "\u001f" + item.Path + "\u001f" + item.Content
      )
    );
  }

  internal IEnumerable<MixinLibraryFile> Files => _orderedFiles;
  internal IEnumerable<MixinAnnotationDefinition> Annotations => _annotations.Values;
  internal IEnumerable<MixinAnnotationDefinition> AnnotationDefinitions =>
    _orderedFiles.Where(file => file.Success).SelectMany(file => file.Annotations.Values);
  internal IEnumerable<MixinDeclarationIr> Derivations =>
    _orderedFiles.Where(file => file.Success).SelectMany(file => file.Derivations ?? []);

  internal string ProjectPath {
    get {
      var path = _files.Values.Select(file => file.Path).FirstOrDefault(item => !string.IsNullOrEmpty(item));
      return ProjectPathFrom(path);
    }
  }

  internal string Key { get; }
  internal string AvailableKeys => _files.Count == 0
    ? "<none>"
    : string.Join(", ", _files.Keys.OrderBy(item => item, StringComparer.Ordinal));

  internal static string ProjectPathFrom(string path) {
    if (string.IsNullOrEmpty(path)) return Directory.GetCurrentDirectory();
    var fullPath = Path.GetFullPath(path);
    var marker = Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar;
    var index = fullPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
    return index < 0 ? Directory.GetCurrentDirectory() : fullPath.Substring(0, index);
  }

  internal bool HasFlag(string scope, string flag) => _files.Values.Any(file => file.Success &&
    file.Configuration.ContainsKey((scope ?? "") + ":" + (flag ?? "")));

  internal bool TryGet(string key, out MixinLibraryFile file) {
    return _files.TryGetValue(key ?? "", out file);
  }

  internal bool TryGetAnnotation(string name, out MixinAnnotationDefinition annotation) {
    return _annotations.TryGetValue(MixinLibraryApi.NormalizeProviderName(name), out annotation);
  }
}

internal sealed record CompiledHixProgram(
  HixProgramImage Prelude,
  HixProgramImage Late,
  IReadOnlyList<MixinTargetReference> Targets
);

internal readonly record struct MixinTargetReference(string Value, bool IsCarry);

internal sealed record CompiledHixAnnotation(
  MixinAnnotationDefinition Definition,
  CompiledHixProgram Program
);

internal sealed record MixinCompilation(
  MixinLibraryCatalog Catalog,
  HixStringPool StringPool,
  HixCompilerCatalog PreparedState,
  IReadOnlyDictionary<string, CompiledHixAnnotation> Annotations,
  ImmutableArray<Diagnostic> Diagnostics
) {
  internal bool TryGetAnnotation(string name, out CompiledHixAnnotation annotation) {
    return Annotations.TryGetValue(MixinLibraryApi.NormalizeProviderName(name), out annotation);
  }
}

internal sealed class HixLibraryCatalogComparer : IEqualityComparer<MixinLibraryCatalog> {
  internal static readonly HixLibraryCatalogComparer Instance = new();

  public bool Equals(MixinLibraryCatalog x, MixinLibraryCatalog y) {
    using var profile = HixProfiler.Measure("comparer.library_catalog.equals");
    return ReferenceEquals(x, y) || (x is not null && y is not null && x.Key == y.Key);
  }

  public int GetHashCode(MixinLibraryCatalog value) {
    using var profile = HixProfiler.Measure("comparer.library_catalog.hash");
    return StringComparer.Ordinal.GetHashCode(value?.Key ?? "");
  }
}

internal sealed class HixCompilationComparer : IEqualityComparer<MixinCompilation> {
  internal static readonly HixCompilationComparer Instance = new();

  public bool Equals(MixinCompilation x, MixinCompilation y) {
    using var profile = HixProfiler.Measure("comparer.compilation.equals");
    return ReferenceEquals(x, y) || (x is not null && y is not null && x.Catalog.Key == y.Catalog.Key);
  }

  public int GetHashCode(MixinCompilation value) {
    using var profile = HixProfiler.Measure("comparer.compilation.hash");
    return StringComparer.Ordinal.GetHashCode(value?.Catalog.Key ?? "");
  }
}
