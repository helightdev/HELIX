namespace Hix.Standalone;

/// <summary>Standalone services plus metadata understood by the Hix test runner.</summary>
public sealed class HixTestBackend : HixStandaloneBackend {
  public HixTestBackend(TextWriter output = null, string workingDirectory = null) : base(output, workingDirectory) { }

  protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    base.RegisterFunctions(functions);
    Hix.Functions.TestMetadataFunctions.Register(functions);
  }
}
