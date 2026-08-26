using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.Mixins;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

internal static class MixinLibraryApi {
  private const int MaximumCachedLibrarySets = 128;
  private const int MaximumCachedCharacters = 1024 * 1024;
  private static readonly object CacheLock = new();
  private static readonly Dictionary<string, CacheEntry> StateCache = new(StringComparer.Ordinal);
  private static readonly LinkedList<string> StateLru = new();
  private static int _cachedCharacters;

  internal static MixinExpressionPreparedState Prepare(
    SourceProductionContext context,
    IEnumerable<INamedTypeSymbol> owners
  ) {
    if (!TryCollect(owners, out var libraries, out var failures)) {
      foreach (var failure in failures)
        context.ReportDiagnostic(Diagnostic.Create(
          InvalidLibraryImport, failure.Location, failure.Owner, failure.Message
        ));
      return new MixinExpressionInterpreter().PrepareGlobals(Array.Empty<string>());
    }

    foreach (var failure in failures)
      context.ReportDiagnostic(Diagnostic.Create(
        InvalidLibraryImport, failure.Location, failure.Owner, failure.Message
      ));
    return PrepareLibraries(context, libraries);
  }

  internal static bool TryPrepare(
    IEnumerable<INamedTypeSymbol> owners,
    out MixinExpressionPreparedState state,
    out string error
  ) {
    state = null;
    if (!TryCollect(owners, out var libraries, out var failures) || failures.Count != 0) {
      error = failures.Count == 0 ? "mixin library import is invalid" : failures[0].Message;
      return false;
    }
    var interpreter = new MixinExpressionInterpreter();
    foreach (var library in libraries) {
      var validation = interpreter.ValidateFunctionLibrary(library.Content);
      if (!validation.Success) {
        error = "mixin library '" + library.Name + "' is invalid at line " +
          validation.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + validation.Error;
        return false;
      }
    }
    try {
      state = PrepareCached(libraries.Select(item => item.Content).ToArray(), interpreter);
      error = null;
      return true;
    } catch (ArgumentException exception) {
      error = exception.Message;
      return false;
    }
  }

  internal static IEnumerable<INamedTypeSymbol> AttributeOwners(IEnumerable<ISymbol> symbols) {
    foreach (var symbol in symbols) {
      if (symbol is INamedTypeSymbol type) yield return type;
      foreach (var attribute in OrderedAttributes(symbol))
        if (attribute.AttributeClass is { } attributeType)
          yield return attributeType;
    }
  }

  private static MixinExpressionPreparedState PrepareLibraries(
    SourceProductionContext context,
    IReadOnlyList<Library> libraries
  ) {
    var interpreter = new MixinExpressionInterpreter();
    var valid = new List<string>(libraries.Count);
    foreach (var library in libraries) {
      var validation = interpreter.ValidateFunctionLibrary(library.Content);
      if (!validation.Success) {
        context.ReportDiagnostic(Diagnostic.Create(
          InvalidPreparedExpression,
          library.Location,
          library.Name,
          validation.ErrorLine.ToString(CultureInfo.InvariantCulture),
          validation.Error
        ));
        continue;
      }
      valid.Add(library.Content);
    }
    try {
      return PrepareCached(valid, interpreter);
    } catch (ArgumentException exception) {
      context.ReportDiagnostic(Diagnostic.Create(
        InvalidLibraryImport, Location.None, "import set", exception.Message
      ));
      return interpreter.PrepareGlobals(Array.Empty<string>());
    }
  }

  private static bool TryCollect(
    IEnumerable<INamedTypeSymbol> owners,
    out IReadOnlyList<Library> libraries,
    out List<ImportFailure> failures
  ) {
    var result = new List<Library>();
    failures = new List<ImportFailure>();
    var visitedOwners = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
    var visitedLibraries = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
    var queue = new Queue<INamedTypeSymbol>(owners.Where(item => item is not null));
    while (queue.Count != 0) {
      var owner = queue.Dequeue();
      if (!visitedOwners.Add(owner)) continue;
      for (var current = owner; current is not null; current = current.BaseType) {
        foreach (var import in OrderedAttributes(current).Where(item =>
          IsAttribute(item, Attributes.MixinImport)
        )) {
          var location = import.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? LocationOf(owner);
          if (import.ConstructorArguments.Length != 1 ||
            import.ConstructorArguments[0].Value is not INamedTypeSymbol libraryType) {
            failures.Add(new ImportFailure(owner.Name, location, "the imported library type is missing"));
            continue;
          }
          if (!visitedLibraries.Add(libraryType)) continue;
          var declaration = Attribute(libraryType, Attributes.MixinLibrary);
          if (declaration is null || declaration.ConstructorArguments.Length != 1 ||
            declaration.ConstructorArguments[0].Value is not string content) {
            failures.Add(new ImportFailure(
              owner.Name, location,
              "'" + libraryType.ToDisplayString() + "' is not a valid [MixinLibrary]"
            ));
            continue;
          }
          result.Add(new Library(
            libraryType.ToDisplayString(), content,
            declaration.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? location
          ));
          queue.Enqueue(libraryType);
        }
      }
    }
    libraries = result;
    return failures.Count == 0;
  }

  private static MixinExpressionPreparedState PrepareCached(
    IReadOnlyList<string> contents,
    MixinExpressionInterpreter interpreter
  ) {
    if (contents.Count == 0) return interpreter.PrepareGlobals(Array.Empty<string>());
    var key = string.Concat(contents.Select(content => content.Length + ":" + content));
    lock (CacheLock) {
      if (StateCache.TryGetValue(key, out var cached)) {
        StateLru.Remove(cached.Node);
        StateLru.AddFirst(cached.Node);
        return cached.State;
      }
    }

    var prepared = interpreter.PrepareGlobals(contents);
    if (key.Length > MaximumCachedCharacters) return prepared;
    lock (CacheLock) {
      if (StateCache.TryGetValue(key, out var existing)) return existing.State;
      var node = StateLru.AddFirst(key);
      StateCache.Add(key, new CacheEntry(prepared, node));
      _cachedCharacters += key.Length;
      while (StateCache.Count > MaximumCachedLibrarySets ||
        _cachedCharacters > MaximumCachedCharacters) {
        var last = StateLru.Last;
        if (last is null) break;
        StateLru.RemoveLast();
        if (!StateCache.ContainsKey(last.Value)) continue;
        StateCache.Remove(last.Value);
        _cachedCharacters -= last.Value.Length;
      }
    }
    return prepared;
  }

  private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) =>
    symbol.GetAttributes()
      .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
      .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
      .ToArray();

  private static bool IsAttribute(AttributeData attribute, string metadataName) =>
    attribute.AttributeClass?.ToDisplayString() == metadataName;

  private sealed record Library(string Name, string Content, Location Location);
  private sealed record ImportFailure(string Owner, Location Location, string Message);
  private sealed record CacheEntry(
    MixinExpressionPreparedState State,
    LinkedListNode<string> Node
  );
}
