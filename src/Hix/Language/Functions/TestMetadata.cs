namespace Hix.Functions;

/// <summary>Compiler declarations shared by standalone test hosts and IDE analyzers.</summary>
public static class TestMetadataFunctions {
  public static void Register(FunctionSignatureRegistryBuilder functions) => functions.AddLibrary("""
    %description("Marks a function as a standalone Hix test.")
    meta test() for function;
    %description("Adds an input tuple and optional expected output to a Hix test.")
    meta testcase(any in) for function;
    meta testcase(any in, any out) for function;
    """);
}
