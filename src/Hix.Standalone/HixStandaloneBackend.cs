using Hix.Runtime;
namespace Hix.Standalone;

public class HixStandaloneBackend(TextWriter output = null) : HixBackend {
  public TextWriter Output { get; } = output ?? Console.Out;
  protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    base.RegisterFunctions(functions);
    functions.Add(new PrintFunction());
  }
  private sealed class PrintFunction() : FunctionDefinition("print", [new(HixValueKind.Null, [HixValueKind.Any], true)]) {
    public override bool HasEffects => true;
    public override IHixValue Execute(HixExecutionContext context, IHixValue[] arguments, int line) {
      var output = ((HixStandaloneBackend)context.Backend).Output;
      for (var i = 0; i < arguments.Length; i++) {
        if (i != 0) output.Write(" ");
        output.Write(context.Render(arguments[i]).Resolve(context.Strings));
      }
      output.WriteLine();
      return NullHixValue.Instance;
    }
  }
}
