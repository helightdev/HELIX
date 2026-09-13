using System.Collections.Generic;

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
    Patterns = patterns ?? new Dictionary<string, HixPattern>();
    MetadataDefinitions = metadataDefinitions ?? [];
    Metadata = metadata ?? [];
    Types = types ?? [];
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
