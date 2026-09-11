using Hix.Runtime;
namespace Hix.Standalone;

public class HixStandaloneBackend : HixBackend {
  public HixStandaloneBackend(TextWriter output = null, string workingDirectory = null) {
    Output = output ?? Console.Out;
    WorkingDirectory = Path.GetFullPath(workingDirectory ?? Directory.GetCurrentDirectory());
  }
  public TextWriter Output { get; }
  public string WorkingDirectory { get; }
  public IReadOnlyList<string> CompilationSources { get; private set; } = [];
  public void SetCompilationSources(IEnumerable<string> sources) => CompilationSources = sources?.ToArray() ?? [];
  protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    base.RegisterFunctions(functions);
    functions.Add(new PrintFunction());
    JsonFunctions.Register(functions);
    HalFunctions.Register(functions);
    IoFunctions.Register(functions);
    CompilationFunctions.Register(functions);
  }
  private sealed class PrintFunction() : FunctionDefinition("print", [new(HixValueKind.Null, [HixValueKind.Any], true)]) {
    public override bool HasEffects => true;
    public override IHixValue Execute(HixThread context, IHixValue[] arguments, int line) {
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
