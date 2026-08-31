using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using MixinLanguage.Compiler;
using static MixinLanguage.GeneratorAnalysis;
using static MixinLanguage.GeneratorDiagnostics.Mixins;

#pragma warning disable RS1035 // Resolving the output path is used only by the explicit PROFILE configuration.

namespace MixinLanguage;

internal static class MixinLibraryApi {
  internal const string AdditionalFileSuffix = ".HelixSourceGenerator.additionalfile";

  internal static MixinCompilation CompileCached(MixinLibraryCatalog catalog) {
    MixinProfiler.Configure(catalog.HasConfiguration("PROFILE"), catalog.ProjectPath);
    using var profile = MixinProfiler.Measure("library.compile_cached");
    return MixinCompilationCache.GetOrCreate(catalog.Key, () => Compile(catalog));
  }

  internal static MixinCompilation Compile(MixinLibraryCatalog catalog) {
    using var profile = MixinProfiler.Measure("library.compile");
    var diagnostics = new List<Diagnostic>();
    var prepared = Prepare(diagnostics.Add, catalog);
    var stringPool = prepared.StringPool;
    var annotations = new Dictionary<string, CompiledMixinAnnotation>(StringComparer.Ordinal);
    foreach (var group in catalog.AnnotationDefinitions.GroupBy(item => item.Name, StringComparer.Ordinal)) {
      if (group.Count() != 1 || catalog.Derivations.Any(item => item.Name == group.Key)) {
        diagnostics.Add(
          Diagnostic.Create(
            InvalidPreparedExpression, Location.None, group.Key, "1",
            group.Count() != 1
              ? "annotation provider is defined more than once"
              : "a provider cannot be both ANNOTATION and DERIVATION"
          )
        );
        continue;
      }
      var annotation = group.Single();
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
    var derivations = new List<CompiledMixinDerivation>();
    foreach (var group in catalog.Derivations.GroupBy(item => item.Name, StringComparer.Ordinal)) {
      if (group.Count() != 1 || catalog.AnnotationDefinitions.Any(item => item.Name == group.Key)) {
        diagnostics.Add(
          Diagnostic.Create(
            InvalidPreparedExpression, Location.None, group.Key, "1",
            catalog.AnnotationDefinitions.Any(item => item.Name == group.Key)
              ? "a provider cannot be both ANNOTATION and DERIVATION"
              : "derivation provider is defined more than once"
          )
        );
        continue;
      }
      var definition = group.Single();
      if (prepared.FunctionEntries.TryGetValue(stringPool.Get(definition.Function), out var entry))
        derivations.Add(new CompiledMixinDerivation(definition, new ProgramFunctionMixinValue(entry)));
    }
    return new MixinCompilation(
      catalog, stringPool, prepared, annotations, derivations, diagnostics.ToImmutableArray()
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
    var parsed = ParseAdditionalFile(content, key);
    if (parsed.Configuration.ContainsKey("PROFILE"))
      MixinProfiler.Configure(true, MixinLibraryCatalog.ProjectPathFrom(file.Path));
    return new MixinLibraryFile(
      key, file.Path, content, parsed.Success,
      parsed.Error, parsed.ErrorLine,
      parsed.Success ? MixinExpressionParser.Parse(parsed.Functions) : null,
      parsed.Annotations, parsed.Derivations, parsed.Configuration
    );
  }

  private static ParsedAdditionalFile ParseAdditionalFile(string content, string libraryKey = "inline") {
    var functions = new StringBuilder();
    var annotations = new Dictionary<string, MixinAnnotationDefinition>(StringComparer.Ordinal);
    var derivations = new List<MixinDerivationDefinition>();
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
    var isDerivation = false;

    ParsedAdditionalFile Failure(string error, int line) {
      return new ParsedAdditionalFile(
        false, functions.ToString(), annotations, derivations, configuration, error, line
      );
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
        else Append(isDerivation ? expression : inPrelude ? prelude : expression, line);
        continue;
      }

      if (annotationName is null) {
        if (command == "CONFIG") {
          if (arguments.Count != 1 || string.IsNullOrWhiteSpace(arguments[0]))
            return Failure("CONFIG requires a key", index + 1);
          configuration[arguments[0]] = ReadConfigurationValue(line);
          continue;
        }
        if (command is not ("ANNOTATION" or "DERIVATION")) {
          Append(functions, line);
          continue;
        }
        if (arguments.Count != 1 || string.IsNullOrWhiteSpace(arguments[0]))
          return Failure(command + " requires a qualified type name", index + 1);
        if (command == "DERIVATION" && !IsQualifiedProviderName(arguments[0]))
          return Failure("DERIVATION requires a qualified type name", index + 1);
        annotationName = NormalizeProviderName(arguments[0]);
        isDerivation = command == "DERIVATION";
        if (annotations.ContainsKey(annotationName) || derivations.Any(item => item.Name == annotationName))
          return Failure("provider '" + annotationName + "' is defined more than once", index + 1);
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

      if (command is "ANNOTATION" or "DERIVATION")
        return Failure("annotation and derivation blocks may not be nested", index + 1);
      if (isDerivation && command == "PRELUDE")
        return Failure("DERIVATION is already a prelude expression", index + 1);
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
        Append(isDerivation ? expression : inPrelude ? prelude : expression, line);
        continue;
      }
      if (command == "SCOPE") {
        if (inFunction) functionScope = true;
        else sectionScope = true;
        Append(isDerivation ? expression : inPrelude ? prelude : expression, line);
        continue;
      }
      if (command != "END") {
        Append(isDerivation ? expression : inPrelude ? prelude : expression, line);
        continue;
      }
      if (inFunction && functionScope) {
        functionScope = false;
        Append(isDerivation ? expression : inPrelude ? prelude : expression, line);
        continue;
      }
      if (inFunction) {
        inFunction = false;
        Append(isDerivation ? expression : inPrelude ? prelude : expression, line);
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

      var preludeText = prelude.ToString();
      var expressionText = expression.ToString();
      foreach (var part in new[] { preludeText, expressionText }) {
        var validation = MixinExpressionParser.ValidateSyntax(part, false);
        if (!validation.Success)
          return Failure(validation.Error, annotationLine + validation.ErrorLine);
      }
      if (isDerivation) {
        var function = "__derive::" + libraryKey + "::" + annotationName;
        functions.Append("@FUNC<").Append(function).AppendLine(">");
        functions.Append(expressionText);
        functions.AppendLine("@END");
        derivations.Add(
          new MixinDerivationDefinition(
            annotationName, function, libraryKey, annotationLine
          )
        );
      } else {
        annotations.Add(
          annotationName, new MixinAnnotationDefinition(
            annotationName, preludeText, expressionText, targets
          )
        );
      }
      annotationName = null;
      isDerivation = false;
    }
    if (annotationName is not null) {
      return Failure(
        "unterminated " + (isDerivation ? "derivation" : "annotation") + " '" + annotationName + "'",
        annotationLine
      );
    }
    var functionText = functions.ToString();
    var functionValidation = MixinExpressionCompiler.ValidateFunctionLibrary(functionText);
    return functionValidation.Success
      ? new ParsedAdditionalFile(true, functionText, annotations, derivations, configuration, null, 0)
      : Failure(functionValidation.Error, functionValidation.ErrorLine);
  }

  private static bool IsQualifiedProviderName(string name) {
    var normalized = NormalizeProviderName(name);
    return normalized.IndexOf('.') > 0 || normalized.IndexOf('+') > 0;
  }

  internal static string NormalizeProviderName(string name) {
    var normalized = (name ?? "").Trim();
    return normalized.StartsWith("global::", StringComparison.Ordinal)
      ? normalized.Substring("global::".Length)
      : normalized;
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
    return PrepareLibraries(
      reportDiagnostic, libraries,
      catalog.Annotations.SelectMany(annotation =>
        new[] { annotation.Name }.Concat(
          annotation.TargetDefinitions.SelectMany(definition =>
            new[] { definition.Key, definition.Value }
          )
        )
      )
    );
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
    IReadOnlyList<Library> libraries,
    IEnumerable<string> additionalConstants = null
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
      return MixinExpressionCompiler.PrepareGlobals(valid, additionalConstants);
    } catch (ArgumentException exception) {
      reportDiagnostic(
        Diagnostic.Create(
          InvalidLibraryImport, Location.None, "import set", exception.Message
        )
      );
      return MixinExpressionCompiler.PrepareGlobals(
        Array.Empty<MixinProgramSyntax>(), additionalConstants
      );
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
  IReadOnlyList<MixinDerivationDefinition> Derivations,
  IReadOnlyDictionary<string, string> Configuration
);

internal sealed record MixinDerivationDefinition(
  string Name, string Function, string Source, int SourceLine
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
  IReadOnlyList<MixinDerivationDefinition> Derivations,
  IReadOnlyDictionary<string, string> Configuration,
  string Error,
  int ErrorLine
);

internal sealed class MixinLibraryCatalog {
  private readonly Dictionary<string, MixinAnnotationDefinition> _annotations;
  private readonly Dictionary<string, MixinLibraryFile> _files;
  private readonly IReadOnlyList<MixinLibraryFile> _orderedFiles;

  internal MixinLibraryCatalog(IEnumerable<MixinLibraryFile> files) {
    using var profile = MixinProfiler.Measure("model.library_catalog.create");
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
  internal IEnumerable<MixinDerivationDefinition> Derivations =>
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

internal sealed record CompiledMixinDerivation(
  MixinDerivationDefinition Definition, ProgramFunctionMixinValue Function
);

internal sealed record MixinCompilation(
  MixinLibraryCatalog Catalog,
  MixinStringPool StringPool,
  MixinExpressionPreparedState PreparedState,
  IReadOnlyDictionary<string, CompiledMixinAnnotation> Annotations,
  IReadOnlyList<CompiledMixinDerivation> Derivations,
  ImmutableArray<Diagnostic> Diagnostics
) {
  internal bool TryGetAnnotation(string name, out CompiledMixinAnnotation annotation) {
    return Annotations.TryGetValue(MixinLibraryApi.NormalizeProviderName(name), out annotation);
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
