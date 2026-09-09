using System;
using System.Collections.Generic;
using Hix.Runtime;
namespace Hix;

public sealed record HixBackendRoot(string Name, HixValueKind Kind, bool RequiresPrelude = false, bool HasEffects = true);

/// <summary>Host services and immutable language definitions. Execution state belongs to contexts.</summary>
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
  public FunctionLibrary Functions => functions.Value;
  public IReadOnlyDictionary<string, HixBackendRoot> Roots => roots.Value;
  protected virtual void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    Hix.Functions.Builtins.Register(functions);
    functions.Add(new Hix.Functions.NameFunction(), new Hix.Functions.UnwrapFunction());
  }
  protected virtual void RegisterRoots(IDictionary<string, HixBackendRoot> roots) { }
  public virtual HixExecutionContext CreateContext() => new(this);
  public virtual IHixValue ResolveRoot(HixExecutionContext context, string name) => context.Error("unknown root '" + name + "'");
  public virtual HixString Render(HixExecutionContext context, IHixValue value) => value.Render(context);
  public virtual object UnlinkSnapshot(HixExecutionContext context, IHixValue value) => value.Unlink(context);
  public virtual IHixValue DetachValue(HixExecutionContext context, IHixValue value) => context.DetachCore(value);
  public virtual HixString NameOf(HixExecutionContext context, IHixValue value) => value.Render(context);
  public virtual IHixValue Import(HixExecutionContext context, object value) => new ObjectHixValue(value);
  public virtual IHixValue Unwrap(HixExecutionContext context, IHixValue value) => value;
  public virtual bool IsType(HixExecutionContext context, IHixValue value, HixString type) => false;
  public virtual bool HasTrait(HixExecutionContext context, IHixValue value, HixString trait) => false;
  public virtual IHixValue Attributes(HixExecutionContext context, IHixValue value, HixString type, bool exact, bool first) => first ? NullHixValue.Instance : HixTableValue.Empty;
  public virtual IHixValue ResolveMixin(HixExecutionContext context, HixString local, IHixValue operand) => context.Error("mixin resolution is not supported by this backend");
  public virtual IHixValue DefineTarget(HixExecutionContext context, string name, string descriptor) => context.Error("target aliases are not supported by this backend");
  public virtual string ResolveInjectionTarget(HixExecutionContext context, string target) => target;
}
public sealed class HixCoreBackend : HixBackend {
  public static HixCoreBackend Instance { get; } = new();
}
