using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HelixSourceGenerator.Language;
using HelixSourceGenerator.Language.Compiler;
using Microsoft.CodeAnalysis;
using static HelixSourceGenerator.Shared.GeneratorAnalysis;
using static HelixSourceGenerator.Shared.GeneratorDiagnostics.Mixins;

#pragma warning disable RS1035 // Resolving the output path is used only by the explicit PROFILE configuration.

namespace HelixSourceGenerator.Shared;

internal static class MixinLibraryApi {
  internal const string AdditionalFileSuffix = ".HelixSourceGenerator.additionalfile";
  private static readonly object CompilationCacheGate = new();
  private static string _compiledCatalogKey;
  private static MixinCompilation _compiledCatalog;

  internal static MixinCompilation CompileCached(MixinLibraryCatalog catalog) {
    MixinProfiler.Configure(catalog.HasConfiguration("PROFILE"), catalog.ProjectPath);
    using var profile = MixinProfiler.Measure("library.compile_cached");
    lock (CompilationCacheGate) {
      if (_compiledCatalog is not null &&
        string.Equals(_compiledCatalogKey, catalog.Key, StringComparison.Ordinal))
        return _compiledCatalog;
      _compiledCatalog = Compile(catalog);
      _compiledCatalogKey = catalog.Key;
      return _compiledCatalog;
    }
  }

  internal static MixinCompilation Compile(MixinLibraryCatalog catalog) {
    using var profile = MixinProfiler.Measure("library.compile");
    var diagnostics = new List<Diagnostic>();
    var prepared = Prepare(diagnostics.Add, catalog);
    var stringPool = prepared.StringPool;
    foreach (var annotation in catalog.Annotations) {
      stringPool.Intern(annotation.Name);
      foreach (var definition in annotation.TargetDefinitions) {
        stringPool.Intern(definition.Key);
        stringPool.Intern(definition.Value);
      }
    }
    var annotations = new Dictionary<string, CompiledMixinAnnotation>(StringComparer.Ordinal);
    foreach (var annotation in catalog.Annotations) {
      if (!TryCompileAnnotation(annotation, prepared, false, out var member, out var error, out var line) ||
        !TryCompileAnnotation(annotation, prepared, true, out var type, out error, out line)) {
        diagnostics.Add(
          Diagnostic.Create(
            InvalidPreparedExpression, Location.None, annotation.Name,
            line.ToString(CultureInfo.InvariantCulture), error
          )
        );
        continue;
      }
      annotations[annotation.Name] = new CompiledMixinAnnotation(annotation, member, type);
    }
    return new MixinCompilation(
      catalog, stringPool, prepared, annotations, diagnostics.ToImmutableArray()
    );
  }

  private static bool TryCompileAnnotation(
    MixinAnnotationDefinition annotation,
    MixinExpressionPreparedState prepared,
    bool typeLevel,
    out CompiledMixinProgram program,
    out string error,
    out int errorLine
  ) {
    using var profile = MixinProfiler.Measure(
      typeLevel
        ? "library.annotation.type"
        : "library.annotation.member"
    );
    var prelude = MixinExpressionParser.Parse(annotation.Prelude);
    var expression = MixinExpressionParser.Parse(annotation.Expression);
    if (typeLevel) {
      prelude = MixinExpressionCompiler.RewriteTargetAsThis(prelude);
      expression = MixinExpressionCompiler.RewriteTargetAsThis(expression);
    }
    if (!MixinExpressionCompiler.TryCompileSyntax(
      prelude, expression, prepared, out var compiledPrelude, out var compiledLate,
      out error, out errorLine
    )) {
      program = null;
      return false;
    }
    if (!MixinExpressionCompiler.TryCompileExecution(
      compiledPrelude, prepared, out var preludeIr, out error, out errorLine
    ) || !MixinExpressionCompiler.TryCompileExecution(
      compiledLate, prepared, out var lateIr, out error, out errorLine
    )) {
      program = null;
      return false;
    }
    program = new CompiledMixinProgram(
      preludeIr, lateIr, MixinSyntaxRenderer.RenderProgram(compiledPrelude),
      MixinSyntaxRenderer.RenderProgram(compiledLate)
    );
    return true;
  }

  internal static MixinLibraryFile ReadAdditionalFile(
    AdditionalText file,
    CancellationToken cancellationToken
  ) {
    if (!file.Path.EndsWith(AdditionalFileSuffix, StringComparison.OrdinalIgnoreCase))
      return null;
    var name = Path.GetFileName(file.Path);
    var key = name.EndsWith(AdditionalFileSuffix, StringComparison.OrdinalIgnoreCase)
      ? name.Substring(0, name.Length - AdditionalFileSuffix.Length)
      : name;
    var content = file.GetText(cancellationToken)?.ToString() ?? "";
    var parsed = ParseAdditionalFile(content);
    if (parsed.Configuration.ContainsKey("PROFILE"))
      MixinProfiler.Configure(true, MixinLibraryCatalog.ProjectPathFrom(file.Path));
    return new MixinLibraryFile(
      key, file.Path, content, parsed.Success,
      parsed.Error, parsed.ErrorLine,
      parsed.Success ? MixinExpressionParser.Parse(parsed.Functions) : null,
      parsed.Annotations, parsed.Configuration
    );
  }

  private static ParsedAdditionalFile ParseAdditionalFile(string content) {
    var functions = new StringBuilder();
    var annotations = new Dictionary<string, MixinAnnotationDefinition>(StringComparer.Ordinal);
    var configuration = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var lines = (content ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    string annotationName = null;
    var prelude = new StringBuilder();
    var expression = new StringBuilder();
    Dictionary<string, string> targets = null;
    var inPrelude = false;
    var sawPrelude = false;
    var sectionScope = false;
    var functionScope = false;
    var inFunction = false;
    var annotationLine = 0;

    ParsedAdditionalFile Failure(string error, int line) {
      return new ParsedAdditionalFile(false, functions.ToString(), annotations, configuration, error, line);
    }

    void Append(StringBuilder target, string line) {
      target.AppendLine(line);
    }

    for (var index = 0; index < lines.Length; index++) {
      var line = lines[index];
      if (!MixinExpressionParser.TryReadDirective(
        line, out var command, out var arguments, out _
      )) {
        if (annotationName is null) Append(functions, line);
        else Append(inPrelude ? prelude : expression, line);
        continue;
      }

      if (annotationName is null) {
        if (command == "CONFIG") {
          if (arguments.Count != 1 || string.IsNullOrWhiteSpace(arguments[0]))
            return Failure("CONFIG requires a key", index + 1);
          configuration[arguments[0]] = ReadConfigurationValue(line);
          continue;
        }
        if (command != "ANNOTATION") {
          Append(functions, line);
          continue;
        }
        if (arguments.Count != 1 || string.IsNullOrWhiteSpace(arguments[0]))
          return Failure("ANNOTATION requires a qualified type name", index + 1);
        annotationName = arguments[0];
        annotationLine = index + 1;
        prelude.Clear();
        expression.Clear();
        targets = new Dictionary<string, string>(StringComparer.Ordinal);
        inPrelude = false;
        sawPrelude = false;
        sectionScope = false;
        functionScope = false;
        inFunction = false;
        continue;
      }

      if (command == "ANNOTATION")
        return Failure("annotations may not be nested", index + 1);
      if (command == "PRELUDE" && !inPrelude && !inFunction && !sectionScope) {
        if (sawPrelude) return Failure("PRELUDE may only be declared once", index + 1);
        inPrelude = true;
        sawPrelude = true;
        continue;
      }
      if (command == "DEFINE_TARGET" && !inPrelude && !inFunction && !sectionScope) {
        if (arguments.Count != 2 || arguments.Any(string.IsNullOrWhiteSpace))
          return Failure("DEFINE_TARGET requires a name and value", index + 1);
        var targetKey = "$" + arguments[0].TrimStart('$');
        if (targets.ContainsKey(targetKey))
          return Failure("target '" + targetKey + "' is defined more than once", index + 1);
        targets.Add(targetKey, arguments[1]);
        continue;
      }
      if (command == "FUNC") {
        if (inFunction) return Failure("functions may not be nested", index + 1);
        inFunction = true;
        functionScope = false;
        Append(inPrelude ? prelude : expression, line);
        continue;
      }
      if (command == "SCOPE") {
        if (inFunction) functionScope = true;
        else sectionScope = true;
        Append(inPrelude ? prelude : expression, line);
        continue;
      }
      if (command != "END") {
        Append(inPrelude ? prelude : expression, line);
        continue;
      }
      if (inFunction && functionScope) {
        functionScope = false;
        Append(inPrelude ? prelude : expression, line);
        continue;
      }
      if (inFunction) {
        inFunction = false;
        Append(inPrelude ? prelude : expression, line);
        continue;
      }
      if (sectionScope) {
        sectionScope = false;
        Append(inPrelude ? prelude : expression, line);
        continue;
      }
      if (inPrelude) {
        inPrelude = false;
        continue;
      }

      if (annotations.ContainsKey(annotationName))
        return Failure("annotation '" + annotationName + "' is defined more than once", annotationLine);
      var preludeText = prelude.ToString();
      var expressionText = expression.ToString();
      foreach (var part in new[] { preludeText, expressionText }) {
        var validation = MixinExpressionParser.ValidateSyntax(part, false);
        if (!validation.Success)
          return Failure(validation.Error, annotationLine + validation.ErrorLine);
      }
      annotations.Add(
        annotationName, new MixinAnnotationDefinition(
          annotationName, preludeText, expressionText, targets
        )
      );
      annotationName = null;
    }
    if (annotationName is not null)
      return Failure("unterminated annotation '" + annotationName + "'", annotationLine);
    var functionText = functions.ToString();
    var functionValidation = MixinExpressionCompiler.ValidateFunctionLibrary(functionText);
    return functionValidation.Success
      ? new ParsedAdditionalFile(true, functionText, annotations, configuration, null, 0)
      : Failure(functionValidation.Error, functionValidation.ErrorLine);
  }

  private static string ReadConfigurationValue(string line) {
    var closing = line?.IndexOf('>') ?? -1;
    if (closing < 0 || closing + 1 >= line.Length) return "";
    var value = line.Substring(closing + 1).Trim();
    return value.StartsWith("|", StringComparison.Ordinal) ? value.Substring(1).Trim() : value;
  }

  internal static MixinExpressionPreparedState Prepare(
    Action<Diagnostic> reportDiagnostic,
    MixinLibraryCatalog catalog
  ) {
    var libraries = new List<Library>();
    foreach (var file in catalog.Files) {
      if (!file.Success) {
        reportDiagnostic(
          Diagnostic.Create(
            InvalidPreparedExpression, Location.None, file.Key,
            file.ErrorLine.ToString(CultureInfo.InvariantCulture), file.Error
          )
        );
        continue;
      }
      libraries.Add(new Library(file.Key, "", Location.None, file.Program));
    }
    return PrepareLibraries(reportDiagnostic, libraries);
  }

  internal static IEnumerable<INamedTypeSymbol> AttributeOwners(IEnumerable<ISymbol> symbols) {
    foreach (var symbol in symbols) {
      if (symbol is INamedTypeSymbol type) yield return type;
      foreach (var attribute in OrderedAttributes(symbol)) {
        if (attribute.AttributeClass is { } attributeType)
          yield return attributeType;
      }
    }
  }

  internal static IEnumerable<MixinAnnotationDefinition> Annotations(
    INamedTypeSymbol type,
    MixinLibraryCatalog catalog
  ) {
    if (type is null || catalog is null) yield break;
    var name = type.ToDisplayString(TypeDisplayFormat);
    if (catalog.TryGetAnnotation(name, out var annotation)) yield return annotation;
  }

  private static MixinExpressionPreparedState PrepareLibraries(
    Action<Diagnostic> reportDiagnostic,
    IReadOnlyList<Library> libraries
  ) {
    var valid = new List<MixinProgramSyntax>(libraries.Count);
    foreach (var library in libraries) {
      if (library.Program is not null) {
        valid.Add(library.Program);
        continue;
      }
      var validation = MixinExpressionCompiler.ValidateFunctionLibrary(library.Content);
      if (!validation.Success) {
        reportDiagnostic(
          Diagnostic.Create(
            InvalidPreparedExpression,
            library.Location,
            library.Name,
            validation.ErrorLine.ToString(CultureInfo.InvariantCulture),
            validation.Error
          )
        );
        continue;
      }
      valid.Add(MixinExpressionParser.Parse(library.Content));
    }
    try {
      return MixinExpressionCompiler.PrepareGlobals(valid);
    } catch (ArgumentException exception) {
      reportDiagnostic(
        Diagnostic.Create(
          InvalidLibraryImport, Location.None, "import set", exception.Message
        )
      );
      return MixinExpressionCompiler.PrepareGlobals(Array.Empty<string>());
    }
  }

  private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) {
    return [
      .. symbol.GetAttributes()
        .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
        .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
    ];
  }

  private sealed record Library(
    string Name,
    string Content,
    Location Location,
    MixinProgramSyntax Program
  );
}

internal sealed record MixinLibraryFile(
  string Key,
  string Path,
  string Content,
  bool Success,
  string Error,
  int ErrorLine,
  MixinProgramSyntax Program,
  IReadOnlyDictionary<string, MixinAnnotationDefinition> Annotations,
  IReadOnlyDictionary<string, string> Configuration
);

internal sealed record MixinAnnotationDefinition(
  string Name,
  string Prelude,
  string Expression,
  IReadOnlyDictionary<string, string> TargetDefinitions
);

internal sealed record ParsedAdditionalFile(
  bool Success,
  string Functions,
  IReadOnlyDictionary<string, MixinAnnotationDefinition> Annotations,
  IReadOnlyDictionary<string, string> Configuration,
  string Error,
  int ErrorLine
);

internal sealed class MixinLibraryCatalog {
  private readonly Dictionary<string, MixinAnnotationDefinition> _annotations;
  private readonly Dictionary<string, MixinLibraryFile> _files;

  internal MixinLibraryCatalog(IEnumerable<MixinLibraryFile> files) {
    using var profile = MixinProfiler.Measure("model.library_catalog.create");
    _files = (files ?? [])
      .Where(item => item is not null)
      .GroupBy(item => item.Key, StringComparer.Ordinal)
      .ToDictionary(item => item.Key, item => item.Last(), StringComparer.Ordinal);
    _annotations = _files.Values
      .Where(file => file.Success)
      .SelectMany(file => file.Annotations.Values)
      .GroupBy(annotation => annotation.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.Last(), StringComparer.Ordinal);
    Key = string.Join(
      "\u001e", _files.OrderBy(item => item.Key, StringComparer.Ordinal)
        .Select(item => item.Key + "\u001f" + item.Value.Content)
    );
  }

  internal IEnumerable<MixinLibraryFile> Files => _files.Values;
  internal IEnumerable<MixinAnnotationDefinition> Annotations => _annotations.Values;

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
    return _annotations.TryGetValue((name ?? "").Replace("global::", ""), out annotation);
  }
}

internal sealed record CompiledMixinProgram(
  MixinExpressionExecutionProgram Prelude,
  MixinExpressionExecutionProgram Late,
  string PreludeSource,
  string LateSource
);

internal sealed record CompiledMixinAnnotation(
  MixinAnnotationDefinition Definition,
  CompiledMixinProgram MemberProgram,
  CompiledMixinProgram TypeProgram
);

internal sealed record MixinCompilation(
  MixinLibraryCatalog Catalog,
  MixinStringPool StringPool,
  MixinExpressionPreparedState PreparedState,
  IReadOnlyDictionary<string, CompiledMixinAnnotation> Annotations,
  ImmutableArray<Diagnostic> Diagnostics
) {
  internal bool TryGetAnnotation(string name, out CompiledMixinAnnotation annotation) {
    return Annotations.TryGetValue((name ?? "").Replace("global::", ""), out annotation);
  }
}

internal sealed class MixinLibraryCatalogComparer : IEqualityComparer<MixinLibraryCatalog> {
  internal static readonly MixinLibraryCatalogComparer Instance = new();

  public bool Equals(MixinLibraryCatalog x, MixinLibraryCatalog y) {
    using var profile = MixinProfiler.Measure("comparer.library_catalog.equals");
    return ReferenceEquals(x, y) || (x is not null && y is not null && x.Key == y.Key);
  }

  public int GetHashCode(MixinLibraryCatalog value) {
    using var profile = MixinProfiler.Measure("comparer.library_catalog.hash");
    return StringComparer.Ordinal.GetHashCode(value?.Key ?? "");
  }
}

internal sealed class MixinCompilationComparer : IEqualityComparer<MixinCompilation> {
  internal static readonly MixinCompilationComparer Instance = new();

  public bool Equals(MixinCompilation x, MixinCompilation y) {
    using var profile = MixinProfiler.Measure("comparer.compilation.equals");
    return ReferenceEquals(x, y) || (x is not null && y is not null && x.Catalog.Key == y.Catalog.Key);
  }

  public int GetHashCode(MixinCompilation value) {
    using var profile = MixinProfiler.Measure("comparer.compilation.hash");
    return StringComparer.Ordinal.GetHashCode(value?.Catalog.Key ?? "");
  }
}
