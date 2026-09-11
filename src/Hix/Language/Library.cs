using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Compiler;
using Hix.Env;
using Hix.Functions;

namespace Hix;

public sealed class FunctionLibrary {
  private readonly FunctionSignatureRegistry Definitions;
  public FunctionLibrary(FunctionSignatureRegistryBuilder definitions) {
    Definitions = definitions.Build(ValidateMetadata);
  }

  public bool TryGet(string name, int argumentCount, out FunctionDefinition definition) =>
    TryExecutable(name, argumentCount, out definition);

  public bool TryResolve(string name, int argumentCount, out FunctionDefinition definition) =>
    TryExecutable(name, argumentCount, out definition);

  public IReadOnlyList<FunctionDefinition> Resolve(string name, int count) => Definitions.Resolve(name, count)
    .Where(definition => definition.Metadata == HixMetadataKind.None).ToArray();
  public IReadOnlyList<FunctionDefinition> Resolve(string name) => Definitions.Resolve(name)
    .Where(definition => definition.Metadata == HixMetadataKind.None).ToArray();

  public IEnumerable<FunctionDefinition> Enumerate() => Definitions.Enumerate()
    .Where(definition => definition.Metadata == HixMetadataKind.None);
  public IEnumerable<FunctionDefinition> EnumerateAll() => Definitions.Enumerate();
  public IReadOnlyList<FunctionDefinition> ResolveMetadata(string name, HixMetadataKind metadata) =>
    Definitions.Enumerate().Where(definition => definition.Name == name && (definition.Metadata & metadata) != 0).ToArray();
  public IReadOnlyList<FunctionDefinition> ResolveMetadata(string name, HixMetadataKind metadata, int argumentCount) =>
    ResolveMetadata(name, metadata).Where(definition => definition.MatchesArgumentCount(argumentCount)).ToArray();

  private bool TryExecutable(string name, int argumentCount, out FunctionDefinition definition) {
    definition = Resolve(name, argumentCount).FirstOrDefault();
    return definition != null;
  }

  private static void ValidateMetadata(IEnumerable<FunctionDefinition> definitions) {
    foreach (var definition in definitions) {
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
