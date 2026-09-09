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
using Hix.Env;
using static Hix.Roslyn.GeneratorAnalysis;
using static Hix.Roslyn.GeneratorDiagnostics.Mixins;

#pragma warning disable RS1035

namespace Hix.Roslyn;

internal static class MixinLibraryApi {
  internal const string AdditionalFileSuffix = ".HelixSourceGenerator.additionalfile";

  internal static MixinCompilation CompileCached(MixinLibraryCatalog catalog) {
    HixProfiler.Configure(catalog.HasConfiguration("PROFILE"), catalog.ProjectPath);
    return MixinCompilationCache.GetOrCreate(catalog.Key, () => Compile(catalog));
  }

  internal static MixinCompilation Compile(MixinLibraryCatalog catalog) {
    var diagnostics = new List<Diagnostic>();
    foreach (var file in catalog.Files.Where(file => !file.Success))
      diagnostics.Add(Diagnostic.Create(InvalidPreparedExpression, Location.None, file.Key,
        file.ErrorLine.ToString(CultureInfo.InvariantCulture), file.Error));
    HixExpressionPreparedState prepared;
    try {
      prepared = HixCompiler.PrepareGlobals(catalog.Files.Where(file => file.Success).Select(file => file.Program), HixMixinBackend.Instance);
    } catch (ArgumentException exception) {
      diagnostics.Add(Diagnostic.Create(InvalidLibraryImport, Location.None, "import set", exception.Message));
      prepared = HixCompiler.PrepareGlobals(Array.Empty<CompilationUnitAst>(), HixMixinBackend.Instance);
    }
    var annotations = new Dictionary<string, CompiledHixAnnotation>(StringComparer.Ordinal);
    if (diagnostics.Count == 0) {
      foreach (var annotation in catalog.AnnotationDefinitions) {
        try {
          var (prelude, late) = HixCompiler.PreparePrograms(annotation.Declaration, prepared);
          annotations.Add(annotation.Name, new CompiledHixAnnotation(annotation,
            new CompiledHixProgram(prelude, late)));
        } catch (ArgumentException exception) {
          diagnostics.Add(Diagnostic.Create(InvalidPreparedExpression, Location.None, annotation.Name,
            annotation.Declaration.Line.ToString(CultureInfo.InvariantCulture), exception.Message));
        }
      }
    }
    return new MixinCompilation(catalog, prepared.StringPool, prepared, annotations, diagnostics.ToImmutableArray());
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
    var derivations = new List<MixinDeclarationAst>();
    var configuration = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var declaration in unit.Declarations.OfType<MixinDeclarationAst>()) {
      if (declaration.IsDerivation) { derivations.Add(declaration); continue; }
      var targets = new Dictionary<string, string>(StringComparer.Ordinal);
      foreach (var call in declaration.Declarations.OfType<ExpressionDeclarationAst>()
        .Where(expression => expression.IsPrelude).SelectMany(expression => expression.Body.Statements)
        .OfType<InvocationStatementAst>().Select(statement => statement.Call)) {
        if (call.Arguments.Count != 2 || call.Arguments[0] is not StringExpressionAst first ||
            call.Arguments[1] is not StringExpressionAst second) continue;
        if (call.Name == "defineTarget") targets["$" + first.Value.TrimStart('$')] = second.Value;
        if (call.Name == "config") configuration[first.Value] = second.Value;
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
  CompilationUnitAst Program, IReadOnlyDictionary<string, MixinAnnotationDefinition> Annotations,
  IReadOnlyList<MixinDeclarationAst> Derivations, IReadOnlyDictionary<string, string> Configuration
);

internal sealed record MixinAnnotationDefinition(
  string Name, MixinDeclarationAst Declaration, string Source, IReadOnlyDictionary<string, string> TargetDefinitions
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
  internal IEnumerable<MixinDeclarationAst> Derivations =>
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

  internal bool HasConfiguration(string key) {
    return _files.Values.Any(file =>
      file.Success && file.Configuration.ContainsKey(key ?? "")
    );
  }

  internal bool HasConfigurationOption(string key, string option) {
    return _files.Values.Any(file =>
      file.Success && file.Configuration.TryGetValue(key ?? "", out var value) &&
      (value ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
      .Contains(option ?? "", StringComparer.OrdinalIgnoreCase)
    );
  }

  internal bool TryGet(string key, out MixinLibraryFile file) {
    return _files.TryGetValue(key ?? "", out file);
  }

  internal bool TryGetAnnotation(string name, out MixinAnnotationDefinition annotation) {
    return _annotations.TryGetValue(MixinLibraryApi.NormalizeProviderName(name), out annotation);
  }
}

internal sealed record CompiledHixProgram(
  HixExpressionExecutionProgram Prelude,
  HixExpressionExecutionProgram Late
);

internal sealed record CompiledHixAnnotation(
  MixinAnnotationDefinition Definition,
  CompiledHixProgram Program
);

internal sealed record MixinCompilation(
  MixinLibraryCatalog Catalog,
  HixStringPool StringPool,
  HixExpressionPreparedState PreparedState,
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
