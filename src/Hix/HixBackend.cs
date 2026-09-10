using Hix.Compiler;
using System;
using System.Collections.Generic;
using Hix.Runtime;
namespace Hix;

public sealed record HixBackendRoot(string Name, HixValueKind Kind, bool RequiresPrelude = false, bool HasEffects = true);

/// <summary>Host services and immutable language definitions. Execution state belongs to threads.</summary>
public abstract class HixBackend {
  private readonly Lazy<FunctionLibrary> functions;
  private readonly Lazy<IReadOnlyDictionary<string, HixBackendRoot>> roots;
  protected HixBackend() {
    functions = new Lazy<FunctionLibrary>(() => {
      var definitions = new FunctionSignatureRegistryBuilder();
      RegisterFunctions(definitions);
      return new FunctionLibrary(definitions);
    });
    roots = new Lazy<IReadOnlyDictionary<string, HixBackendRoot>>(() => {
      var definitions = new Dictionary<string, HixBackendRoot>(StringComparer.Ordinal);
      RegisterRoots(definitions);
      return new System.Collections.ObjectModel.ReadOnlyDictionary<string, HixBackendRoot>(definitions);
    });
  }
  /// <summary>Ordered compiler transforms used for globals, mixins, and derivations.</summary>
  public virtual IReadOnlyList<HixCompilerStep> CompilerSteps => HixCompiler.DefaultSteps;

  public FunctionLibrary Functions => functions.Value;
  public IReadOnlyDictionary<string, HixBackendRoot> Roots => roots.Value;
  protected virtual void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    Hix.Functions.Builtins.Register(functions);
    functions.Add(new Hix.Functions.NameFunction(), new Hix.Functions.UnwrapFunction());
    RegisterEmissionFunctions(functions);
  }
  protected virtual void RegisterEmissionFunctions(FunctionSignatureRegistryBuilder functions) {
    functions.Add(new SimpleFunction("emit", [new(HixValueKind.Null, [HixValueKind.Any]), new(HixValueKind.Null, [HixValueKind.String, HixValueKind.Any])], (thread, values) => {
      thread.Emit(values[values.Length - 1], values.Length == 2 ? thread.Text(values[0]) : default);
      return NullHixValue.Instance;
    }, effects: true, acceptsErrors: true, documentation: "Emits a detached Hix value, optionally addressed to an opaque destination."));
  }

  protected virtual void RegisterRoots(IDictionary<string, HixBackendRoot> roots) { }
  /// <summary>Optional host constraints on derivation records, beyond the language's required value field.</summary>
  public virtual ErrorHixValue ValidateDerivationRecord(HixThread thread, HixTableValue record) => null;

  public virtual HixContext CreateContext() => new(this);
  public HixThread CreateThread() => new(CreateContext());
  public virtual IHixValue ResolveRoot(HixThread thread, string name) => thread.Error("unknown root '" + name + "'");
  public virtual HixString Render(HixThread thread, IHixValue value) => value.Render(thread);
  public virtual object UnlinkSnapshot(HixThread thread, IHixValue value) => value.Unlink(thread);
  public virtual IHixValue DetachValue(HixThread thread, IHixValue value) => thread.DetachCore(value);
  public virtual HixString NameOf(HixThread thread, IHixValue value) => value.Render(thread);
  public virtual IHixValue Import(HixThread thread, object value) => new ObjectHixValue(value);
  public virtual IHixValue Unwrap(HixThread thread, IHixValue value) => value;
  public virtual bool IsType(HixThread thread, IHixValue value, HixString type) => false;
  public virtual bool HasTrait(HixThread thread, IHixValue value, HixString trait) => false;
  public virtual IHixValue Attributes(HixThread thread, IHixValue value, HixString type, bool exact, bool first) => first ? NullHixValue.Instance : HixTableValue.Empty;
}
public sealed class HixCoreBackend : HixBackend {
  public static HixCoreBackend Instance { get; } = new();
}
