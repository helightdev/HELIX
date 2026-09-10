using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

public sealed record HixModuleIr(
  IReadOnlyList<ExpressionDeclarationIr> Prelude,
  IReadOnlyList<ExpressionDeclarationIr> Late,
  IReadOnlyList<FunctionDeclarationIr> Functions,
  IReadOnlyList<SignatureField> Parameters = null
);

/// <summary>Owns the mutable semantic IR for one compilation; parsed units and catalogs are not mutated.</summary>
public sealed class HixCompilation {
  public HixCompilation(HixModuleIr module, HixCompilerCatalog catalog) {
    Catalog = catalog;
    var clone = new HixIrRewriter();
    Module = new(module.Prelude.Select(clone.Rewrite).ToArray(), module.Late.Select(clone.Rewrite).ToArray(),
      module.Functions.Select(clone.Rewrite).ToArray(), module.Parameters ?? []);
  }
  public HixModuleIr Module { get; set; }
  public HixCompilerCatalog Catalog { get; }
  public HixBackend Backend => Catalog.Backend;
  public IList<HixParseDiagnostic> Diagnostics { get; } = new List<HixParseDiagnostic>();
}

public abstract class HixCompilerStep {
  public abstract void Run(HixCompilation compilation);
}

/// <summary>A structural pass which replaces IR; validation and binding use Run directly.</summary>
public abstract class HixLoweringStep : HixCompilerStep {
  public sealed override void Run(HixCompilation compilation) =>
    compilation.Module = Lower(compilation.Module, compilation.Catalog);
  public abstract HixModuleIr Lower(HixModuleIr input, HixCompilerCatalog globals);
}
