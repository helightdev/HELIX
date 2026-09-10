using System.Collections.Generic;

namespace Hix.Compiler;

/// <summary>Compiler-only catalog. Runtime programs never retain these declarations.</summary>
public sealed class HixCompilerCatalog {
  public HixCompilerCatalog(HixStringPool strings,
    IReadOnlyList<FunctionDeclarationIr> functions, IReadOnlyList<MixinDeclarationIr> derivations, HixBackend backend = null,
    IReadOnlyDictionary<string, HixPattern> patterns = null) {
    Backend = backend ?? HixCoreBackend.Instance;
    StringPool = strings;
    Functions = functions;
    Derivations = derivations;
    Patterns = patterns ?? new Dictionary<string, HixPattern>();
  }
  public HixBackend Backend { get; }
  public HixStringPool StringPool { get; }
  public IReadOnlyList<FunctionDeclarationIr> Functions { get; }
  public IReadOnlyList<MixinDeclarationIr> Derivations { get; }
  public IReadOnlyDictionary<string, HixPattern> Patterns { get; }
}
