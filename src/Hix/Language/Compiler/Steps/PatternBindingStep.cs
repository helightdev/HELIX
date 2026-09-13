namespace Hix.Compiler.Steps;

/// <summary>Bind source patterns and select a declared signature for every call.</summary>
public sealed class PatternBindingStep : HixCompilerStep {
  public override void Run(HixCompilation compilation) => PatternBinding.Bind(compilation);
}
