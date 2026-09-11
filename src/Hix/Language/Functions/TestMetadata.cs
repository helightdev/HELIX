using System.Collections.Generic;

namespace Hix.Functions;

/// <summary>Compiler declarations shared by standalone test hosts and IDE analyzers.</summary>
public static class TestMetadataFunctions {
  public static void Register(FunctionSignatureRegistryBuilder functions) {
    functions.Add(
      Metadata("test", HixMetadataKind.FunctionDefinition, [new FunctionSignature(HixValueKind.Pattern, [])],
        "Marks a function as a standalone Hix test."),
      Metadata("testcase", HixMetadataKind.FunctionDefinition, [
          new FunctionSignature(HixValueKind.Pattern, [HixValueKind.Any], ArgumentNames: ["in"]),
          new FunctionSignature(HixValueKind.Pattern, [HixValueKind.Any, HixValueKind.Any],
            ArgumentNames: ["in", "out"])
        ], "Adds an input tuple and optional expected output to a Hix test."));
  }

  private static FunctionDefinition Metadata(string name, HixMetadataKind target,
    IReadOnlyList<FunctionSignature> signatures, string documentation) =>
    new(name, signatures, documentation, target);
}
