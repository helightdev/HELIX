using System.Collections.Generic;
using K = Hix.HixValueKind;

namespace Hix.Functions;

public static class MetadataFunctions {
  private const HixMetadataKind Pattern = HixMetadataKind.Pattern | HixMetadataKind.PatternField;
  public static void Register(FunctionSignatureRegistryBuilder definitions) {
    definitions.Add(
      Meta("optional", Pattern, [Signature()], "Makes a pattern field optional."),
      Meta("many", Pattern, [Signature(), Signature(K.Pattern)], "Matches repeated tuple elements."),
      Meta("map", Pattern, [Signature(K.String), Signature(K.Pattern, K.Pattern)], "Matches table keys and values."),
      Meta("union", Pattern, [new FunctionSignature(K.Pattern, [K.Pattern, K.Pattern], true)], "Matches any supplied pattern."),
      Meta("const", Pattern, [Signature(K.Any)], "Matches one constant value."),
      Meta("enum", Pattern, [new FunctionSignature(K.Pattern, [K.Any, K.Any], true)], "Restricts a pattern to constant values."),
      Meta("min", Pattern, [Signature(K.Any), Signature(K.Any, K.Bool)], "Adds an inclusive or exclusive minimum constraint."),
      Meta("max", Pattern, [Signature(K.Any), Signature(K.Any, K.Bool)], "Adds an inclusive or exclusive maximum constraint."),
      Meta("length", Pattern, [Signature(K.Any)], "Adds an exact-length constraint."),
      Meta("matches", Pattern, [Signature(K.String)], "Adds a regular-expression constraint."),
      Meta("title", Pattern, [Signature(K.String)], "Adds a display title."),
      Meta("description", Pattern, [Signature(K.String)], "Adds descriptive documentation."),
      Meta("default", HixMetadataKind.PatternField, [Signature(K.Any)], "Sets a table-field default."),
      Meta("import", HixMetadataKind.File, [Signature(K.String)], "Imports relative Hix path globs."),
      Meta("backend", HixMetadataKind.File, [Signature(K.String)], "Selects the analyzer backend."),
      Meta("pragma", HixMetadataKind.File, [Signature(K.String)], "Configures compiler diagnostics and artifacts."),
      Meta("vm", HixMetadataKind.File, [Signature(K.String)], "Configures virtual-machine diagnostics.")
    );
  }
  private static FunctionDefinition Meta(string name, HixMetadataKind metadata,
    IReadOnlyList<FunctionSignature> signatures, string documentation) => new(name, signatures, documentation, metadata);
  private static FunctionSignature Signature(params K[] arguments) => new(K.Pattern, arguments);
}
