using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

/// <summary>Compiler-only catalog. Runtime programs never retain these declarations.</summary>
public sealed class HixCompilerCatalog {
  public HixCompilerCatalog(HixStringPool strings,
    IReadOnlyList<FunctionDeclarationIr> functions, IReadOnlyList<MixinDeclarationIr> derivations, HixBackend backend = null,
    IReadOnlyDictionary<string, HixPattern> patterns = null,
    IReadOnlyList<MetadataDeclarationIr> metadataDefinitions = null,
    IReadOnlyList<MetadataIr> metadata = null,
    IReadOnlyList<TypeDeclarationIr> types = null) {
    Backend = backend ?? HixCoreBackend.Instance;
    StringPool = strings;
    Functions = functions;
    Derivations = derivations;
    var visiblePatterns = Backend.Functions.Types.ToDictionary(type => type.Name, type => type.Pattern);
    foreach (var item in patterns ?? new Dictionary<string, HixPattern>())
      visiblePatterns[item.Key] = item.Value;
    Patterns = visiblePatterns;
    MetadataDefinitions = metadataDefinitions ?? [];
    Metadata = metadata ?? [];
    Types = Backend.Functions.Types.Concat(types ?? []).GroupBy(type => type.Name, System.StringComparer.Ordinal)
      .Select(group => group.Last()).ToArray();
  }
  public IReadOnlyList<MetadataDeclarationIr> MetadataDefinitions { get; }
  public IReadOnlyList<MetadataIr> Metadata { get; }
  public IReadOnlyList<TypeDeclarationIr> Types { get; }
  public HixBackend Backend { get; }
  public HixStringPool StringPool { get; }
  public IReadOnlyList<FunctionDeclarationIr> Functions { get; }
  public IReadOnlyList<MixinDeclarationIr> Derivations { get; }
  public IReadOnlyDictionary<string, HixPattern> Patterns { get; }
}
