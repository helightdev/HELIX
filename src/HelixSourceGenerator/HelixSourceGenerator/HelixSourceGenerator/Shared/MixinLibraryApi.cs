using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.Mixins;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

internal static class MixinLibraryApi {
  internal const string AdditionalFileSuffix = ".HelixSourceGenerator.additionalfile";

  internal static MixinCompilation Compile(MixinLibraryCatalog catalog) {
    var diagnostics = new List<Diagnostic>();
    var prepared = Prepare(diagnostics.Add, catalog);
    var annotations = new Dictionary<string, CompiledMixinAnnotation>(StringComparer.Ordinal);
    foreach (var annotation in catalog.Annotations) {
      if (!TryCompileAnnotation(annotation, prepared, false, out var member, out var error, out var line) ||
        !TryCompileAnnotation(annotation, prepared, true, out var type, out error, out line)) {
        diagnostics.Add(Diagnostic.Create(
          InvalidPreparedExpression, Location.None, annotation.Name,
          line.ToString(CultureInfo.InvariantCulture), error
        ));
        continue;
      }
      annotations[annotation.Name] = new CompiledMixinAnnotation(annotation, member, type);
    }
    return new MixinCompilation(catalog, prepared, annotations, diagnostics.ToImmutableArray());
  }

  private static bool TryCompileAnnotation(
    MixinAnnotationDefinition annotation,
    MixinExpressionPreparedState prepared,
    bool typeLevel,
    out CompiledMixinProgram program,
    out string error,
    out int errorLine
  ) {
    var prelude = typeLevel
      ? MixinExpressionCompiler.RewriteTargetAsThis(annotation.Prelude)
      : annotation.Prelude;
    var expression = typeLevel
      ? MixinExpressionCompiler.RewriteTargetAsThis(annotation.Expression)
      : annotation.Expression;
    if (!MixinExpressionCompiler.TryHoistPrelude(
      prelude, expression, prepared, out var compiledPrelude, out var compiledLate,
      out error, out errorLine
    )) {
      program = null;
      return false;
    }
    program = new CompiledMixinProgram(
      MixinExpressionInterpreter.GetProgram(compiledPrelude, true),
      MixinExpressionInterpreter.GetProgram(compiledLate, true)
    );
    return true;
  }

  internal static MixinLibraryFile ReadAdditionalFile(
    AdditionalText file,
    System.Threading.CancellationToken cancellationToken
  ) {
    if (!file.Path.EndsWith(AdditionalFileSuffix, StringComparison.OrdinalIgnoreCase))
      return null;
    var name = Path.GetFileName(file.Path);
    var key = name.EndsWith(AdditionalFileSuffix, StringComparison.OrdinalIgnoreCase)
      ? name.Substring(0, name.Length - AdditionalFileSuffix.Length)
      : name;
    var content = file.GetText(cancellationToken)?.ToString() ?? "";
    var parsed = ParseAdditionalFile(content);
    return new MixinLibraryFile(
      key, file.Path, content, parsed.Success,
      parsed.Error, parsed.ErrorLine,
      parsed.Success ? MixinExpressionInterpreter.GetProgram(parsed.Functions, true) : null,
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

    ParsedAdditionalFile Failure(string error, int line) =>
      new(false, functions.ToString(), annotations, configuration, error, line);
    void Append(StringBuilder target, string line) => target.AppendLine(line);
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
        var validation = MixinExpressionCompiler.ValidateSyntax(part, false);
        if (!validation.Success)
          return Failure(validation.Error, annotationLine + validation.ErrorLine);
      }
      annotations.Add(annotationName, new MixinAnnotationDefinition(
        annotationName, preludeText, expressionText, targets
      ));
      annotationName = null;
    }
    if (annotationName is not null)
      return Failure("unterminated annotation '" + annotationName + "'", annotationLine);
    var functionText = functions.ToString();
    var functionValidation = new MixinExpressionInterpreter().ValidateFunctionLibrary(functionText);
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
        reportDiagnostic(Diagnostic.Create(
          InvalidPreparedExpression, Location.None, file.Key,
          file.ErrorLine.ToString(CultureInfo.InvariantCulture), file.Error
        ));
        continue;
      }
      libraries.Add(new Library(file.Key, "", Location.None, file.Program));
    }
    return PrepareLibraries(reportDiagnostic, libraries);
  }

  internal static IEnumerable<INamedTypeSymbol> AttributeOwners(IEnumerable<ISymbol> symbols) {
    foreach (var symbol in symbols) {
      if (symbol is INamedTypeSymbol type) yield return type;
      foreach (var attribute in OrderedAttributes(symbol))
        if (attribute.AttributeClass is { } attributeType)
          yield return attributeType;
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
    var interpreter = new MixinExpressionInterpreter();
    var valid = new List<MixinProgramSyntax>(libraries.Count);
    foreach (var library in libraries) {
      if (library.Program is not null) {
        valid.Add(library.Program);
        continue;
      }
      var validation = interpreter.ValidateFunctionLibrary(library.Content);
      if (!validation.Success) {
        reportDiagnostic(Diagnostic.Create(
          InvalidPreparedExpression,
          library.Location,
          library.Name,
          validation.ErrorLine.ToString(CultureInfo.InvariantCulture),
          validation.Error
        ));
        continue;
      }
      valid.Add(MixinExpressionInterpreter.GetProgram(library.Content, true));
    }
    try {
      return MixinExpressionCompiler.PrepareGlobals(valid);
    } catch (ArgumentException exception) {
      reportDiagnostic(Diagnostic.Create(
        InvalidLibraryImport, Location.None, "import set", exception.Message
      ));
      return interpreter.PrepareGlobals(Array.Empty<string>());
    }
  }

  private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) =>
    symbol.GetAttributes()
      .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
      .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
      .ToArray();

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
  private readonly Dictionary<string, MixinLibraryFile> _files;
  private readonly Dictionary<string, MixinAnnotationDefinition> _annotations;
  internal MixinLibraryCatalog(IEnumerable<MixinLibraryFile> files) {
    _files = (files ?? Array.Empty<MixinLibraryFile>())
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
  internal bool HasConfiguration(string key) => _files.Values.Any(file =>
    file.Success && file.Configuration.ContainsKey(key ?? "")
  );
  internal string Key { get; }
  internal string AvailableKeys => _files.Count == 0
    ? "<none>"
    : string.Join(", ", _files.Keys.OrderBy(item => item, StringComparer.Ordinal));
  internal bool TryGet(string key, out MixinLibraryFile file) => _files.TryGetValue(key ?? "", out file);
  internal bool TryGetAnnotation(string name, out MixinAnnotationDefinition annotation) =>
    _annotations.TryGetValue((name ?? "").Replace("global::", ""), out annotation);
}

internal sealed record CompiledMixinProgram(MixinProgramSyntax Prelude, MixinProgramSyntax Late);

internal sealed record CompiledMixinAnnotation(
  MixinAnnotationDefinition Definition,
  CompiledMixinProgram MemberProgram,
  CompiledMixinProgram TypeProgram
);

internal sealed record MixinCompilation(
  MixinLibraryCatalog Catalog,
  MixinExpressionPreparedState PreparedState,
  IReadOnlyDictionary<string, CompiledMixinAnnotation> Annotations,
  ImmutableArray<Diagnostic> Diagnostics
) {
  internal bool TryGetAnnotation(string name, out CompiledMixinAnnotation annotation) =>
    Annotations.TryGetValue((name ?? "").Replace("global::", ""), out annotation);
}

internal sealed class MixinLibraryCatalogComparer : IEqualityComparer<MixinLibraryCatalog> {
  internal static readonly MixinLibraryCatalogComparer Instance = new();
  public bool Equals(MixinLibraryCatalog x, MixinLibraryCatalog y) =>
    ReferenceEquals(x, y) || x is not null && y is not null && x.Key == y.Key;
  public int GetHashCode(MixinLibraryCatalog value) => StringComparer.Ordinal.GetHashCode(value?.Key ?? "");
}
