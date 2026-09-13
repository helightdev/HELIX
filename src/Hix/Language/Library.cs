using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Compiler;
using Hix.Env;
using Hix.Functions;

namespace Hix;

public sealed class FunctionLibrary {
  private FunctionSignatureRegistry Definitions;
  private readonly IReadOnlyList<HixFunctionEntry> constructorMetadata;
  public FunctionLibrary(FunctionSignatureRegistryBuilder definitions) {
    Definitions = new FunctionSignatureRegistry(Array.Empty<HixFunctionEntry>());
    OpaqueTypes = definitions.OpaqueTypes.ToArray();
    PatternConstructors = definitions.PatternConstructors.Concat(OpaqueTypes.Select(type =>
      new HixPatternConstructor(type.Name, type.Arity, arguments => type.Construct(arguments.ToArray()), type.Documentation))).ToArray();
    constructorMetadata = PatternConstructors.Select(constructor => new HixFunctionEntry(constructor.Name,
      new[] {new FunctionSignature(HixValueKind.Pattern, Enumerable.Repeat(HixValueKind.Pattern, constructor.Arity).ToArray())},
      constructor.Documentation, HixMetadataKind.Pattern | HixMetadataKind.PatternField)).ToArray();
  }
  public IReadOnlyList<HixOpaqueTypeDefinition> OpaqueTypes { get; }
  public IReadOnlyList<HixPatternConstructor> PatternConstructors { get; }

  internal void BindLibraries(FunctionSignatureRegistryBuilder builder, HixBackend backend) {
    var implementations = builder.Handlers
      .GroupBy(item => item.Name, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    var entries = Definitions.Enumerate().ToList();
    foreach (var unit in builder.Libraries.Select(source => AntlrSyntax.Parse(source, backend)).Concat(builder.LibraryUnits)) {
      if (unit.Diagnostics.Count != 0)
        throw new InvalidOperationException("Invalid embedded Hix library: " + string.Join("; ", unit.Diagnostics.Select(item => item.Message)));
      foreach (var declaration in unit.Declarations) {
        if (declaration is MetadataDeclarationIr metadata) {
          entries.Add(new HixFunctionEntry(metadata.Name, [metadata.Signature], string.Join("\n",
            metadata.Metadata.Where(item => item.Name is "description" or "doc").SelectMany(item => item.Values)
              .OfType<StringExpressionIr>().Select(item => item.Value)), metadata.Targets, metadata.Retention));
          continue;
        }
        if (declaration is not FunctionDeclarationIr {IsExternal: true} function || function.Signatures.Count == 0)
          throw new InvalidOperationException("Embedded host libraries require typed external function declarations.");
        var hostName = function.ExternalName ?? function.Name;
        if (!implementations.TryGetValue(hostName, out var handlers))
          throw new InvalidOperationException("External function '" + function.Name + "' requires one handler named '" + hostName + "'.");
        if (handlers.Length != 1)
          throw new InvalidOperationException("External function '" + function.Name + "' requires one handler named '" + hostName + "'.");
        entries.Add(new LibraryFunction(function, handlers[0], builder));
      }
      Definitions = new FunctionSignatureRegistry(entries);
    }
    foreach (var handler in implementations.Values.SelectMany(items => items))
      if (!entries.OfType<LibraryFunction>().Any(entry => ReferenceEquals(entry.Handler, handler)))
        throw new InvalidOperationException("Host handler '" + handler.Name + "' has no external Hix declaration.");
    ValidateMetadata(entries);
    Definitions = new FunctionSignatureRegistry(entries.Where(entry => entry is not LibraryFunction && entry.Metadata == HixMetadataKind.None)
      .Concat(entries.Where(entry => entry.Metadata != HixMetadataKind.None).GroupBy(entry => (entry.Name, entry.Metadata, entry.Retention))
        .Select(group => new HixFunctionEntry(group.Key.Name, group.SelectMany(entry => entry.Signatures).ToArray(),
          group.First().Documentation, group.Key.Metadata, group.Key.Retention))).Concat(
      entries.OfType<LibraryFunction>().GroupBy(entry => (entry.Name, entry.Handler, entry.HasEffects, entry.Metadata, entry.RequiresPrelude))
        .Select(group => new LibraryFunction(group.ToArray()))));
  }

  private sealed class LibraryFunction : HixFunctionEntry {
    public LibraryFunction(LibraryFunction[] overloads)
      : base(overloads[0].Name, overloads.SelectMany(entry => entry.Signatures).ToArray(), overloads[0].Documentation, overloads[0].Metadata) {
      Handler = overloads[0].Handler;
      hasEffects = overloads[0].HasEffects;
      RequiresPrelude = overloads[0].RequiresPrelude;
      ArgumentReferences = overloads[0].ArgumentReferences;
    }
    public LibraryFunction(FunctionDeclarationIr declaration, FunctionDefinition handler, FunctionSignatureRegistryBuilder builder)
      : base(declaration.Name, declaration.Signatures.Select(signature => {
        var pattern = signature.Constant(declaration.Name);
        var result = new FunctionSignature(pattern.Result, pattern.Parameters.Select(field => field.Pattern).ToArray(),
          signature.Inputs?.LastOrDefault()?.Variadic == true,
          pattern.Parameters.Select(field => field.Name).ToArray(),
          signature.Inputs?.Select(field => field.DefaultValue).ToArray(), declaration.TypeParameters);
        return builder.ConfigureInference(declaration.Name, result);
      }).ToArray(), string.Join("\n", declaration.Metadata.Where(item => item.Name is "description" or "doc")
        .SelectMany(item => item.Values).OfType<StringExpressionIr>().Select(item => item.Value))) {
      Handler = handler;
      hasEffects = !declaration.IsPure;
      RequiresPrelude = declaration.Metadata.Any(item => item.Name == "requiresPrelude");
      ArgumentReferences = (declaration.Signatures[0].Inputs ?? []).Select((field, index) => (
        Index: index, Kind: field.Metadata.Where(item => item.Name == "reference")
          .SelectMany(item => item.Values).OfType<StringExpressionIr>().Select(item => item.Value).FirstOrDefault()))
        .Where(item => item.Kind != null).ToDictionary(item => item.Index, item => item.Kind);
    }
    private readonly bool hasEffects;
    public override bool HasEffects => hasEffects;

  }

  public bool TryGet(string name, int argumentCount, out HixFunctionEntry definition) =>
    TryExecutable(name, argumentCount, out definition);

  public bool TryResolve(string name, int argumentCount, out HixFunctionEntry definition) =>
    TryExecutable(name, argumentCount, out definition);

  public IReadOnlyList<HixFunctionEntry> Resolve(string name, int count) => Definitions.Resolve(name, count)
    .Where(definition => definition.Metadata == HixMetadataKind.None).ToArray();
  public IReadOnlyList<HixFunctionEntry> Resolve(string name) => Definitions.Resolve(name)
    .Where(definition => definition.Metadata == HixMetadataKind.None).ToArray();

  public IEnumerable<HixFunctionEntry> Enumerate() => Definitions.Enumerate()
    .Where(definition => definition.Metadata == HixMetadataKind.None);
  public IEnumerable<HixFunctionEntry> EnumerateAll() => Definitions.Enumerate().Concat(constructorMetadata);
  public IReadOnlyList<HixFunctionEntry> ResolveMetadata(string name, HixMetadataKind metadata) =>
    EnumerateAll().Where(definition => definition.Name == name && (definition.Metadata & metadata) != 0).ToArray();
  public IReadOnlyList<HixFunctionEntry> ResolveMetadata(string name, HixMetadataKind metadata, int argumentCount) =>
    ResolveMetadata(name, metadata).Where(definition => definition.MatchesArgumentCount(argumentCount)).ToArray();

  private bool TryExecutable(string name, int argumentCount, out HixFunctionEntry definition) {
    definition = Resolve(name, argumentCount).FirstOrDefault();
    return definition != null;
  }

  private static void ValidateMetadata(IEnumerable<HixFunctionEntry> definitions) {
    var entries = definitions.ToArray();
    foreach (var group in entries.Where(entry => entry.Metadata != HixMetadataKind.None).GroupBy(entry => entry.Name))
      if (group.Select(entry => entry.Retention).Distinct().Count() > 1)
        throw new InvalidOperationException("Metadata overloads '" + group.Key + "' must use the same retention modifier.");
    foreach (var definition in entries) {
      if (definition.Signatures is null || definition.Signatures.Count == 0 ||
        definition.Signatures.Any(signature => signature.ArgumentTypes is null))
        throw new InvalidOperationException("Function ':" + definition.Name + "' must provide language metadata.");
    }
  }

  public void CollectConstants(HixStringPoolBuilder pool) {
    foreach (var name in Definitions.Enumerate().Select(item => item.Name).Distinct()) pool.Intern(name);
  }
}

public static class HixRootLibrary {
  private static readonly IReadOnlyList<HixRootDefinition> Definitions = [
    new(
      Name: "table", Root: HixExpressionRoot.Table, Kind: HixValueKind.Kind,
      Documentation: "The table kind. Construct a table with table() or a table literal."
    ),
    new(
      Name: "var", Root: HixExpressionRoot.Variable, Kind: HixValueKind.Table,
      Documentation: "A named Hix variable."
    ),
    new(
      Name: "tar", Root: HixExpressionRoot.TargetVariable, Kind: HixValueKind.Table,
      Documentation: "A named variable stored on the current target."
    ),
    new(
      Name: "local", Root: HixExpressionRoot.Local, Kind: HixValueKind.Table,
      Documentation: "A compiler-generated local Hix value."
    ),
    new(
      Name: "param", Root: HixExpressionRoot.Parameter, Kind: HixValueKind.Any,
      Documentation: "A named parameter of the current function scope."
    )
  ];

  private static readonly IReadOnlyDictionary<string, HixRootDefinition> ByName = Definitions.ToDictionary(
    item => item.Name, StringComparer.Ordinal
  );

  private static readonly IReadOnlyDictionary<HixExpressionRoot, HixRootDefinition> ByRoot =
    Definitions.ToDictionary(item => item.Root);

  public static bool TryGet(string name, out HixRootDefinition definition) => ByName.TryGetValue(
    name ?? "",
    out definition
  );

  public static bool TryGet(HixExpressionRoot root, out HixRootDefinition definition) => ByRoot.TryGetValue(
    root,
    out definition
  );

  public static IEnumerable<HixRootDefinition> Enumerate() => Definitions;
}
