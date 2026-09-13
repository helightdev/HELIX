using Hix.Compiler;
using System;
using System.Collections.Generic;
using Hix.Runtime;
namespace Hix;

public sealed record HixBackendRoot(string Name, HixValueKind Kind, bool RequiresPrelude = false, bool HasEffects = true) {
  public HixPattern Pattern { get; init; } = Kind == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(Kind);
}

/// <summary>Host services and immutable language definitions. Execution state belongs to threads.</summary>
public abstract class HixBackend {
  private readonly Lazy<FunctionLibrary> functions;
  private FunctionLibrary initializingFunctions;
  private readonly object functionGate = new();
  private readonly Lazy<IReadOnlyDictionary<string, HixBackendRoot>> roots;
  protected HixBackend() {
    functions = new Lazy<FunctionLibrary>(() => {
      var definitions = new FunctionSignatureRegistryBuilder();
      RegisterFunctions(definitions);
      var library = new FunctionLibrary(definitions);
      initializingFunctions = library;
      try {
        library.BindLibraries(definitions, this);
        return library;
      } finally {
        initializingFunctions = null;
      }
    });
    roots = new Lazy<IReadOnlyDictionary<string, HixBackendRoot>>(() => {
      var definitions = new Dictionary<string, HixBackendRoot>(StringComparer.Ordinal);
      RegisterRoots(definitions);
      return new System.Collections.ObjectModel.ReadOnlyDictionary<string, HixBackendRoot>(definitions);
    });
  }
  /// <summary>Ordered compiler transforms used for globals, mixins, and derivations.</summary>
  public virtual IReadOnlyList<HixCompilerStep> CompilerSteps => HixCompiler.DefaultSteps;

  public FunctionLibrary Functions {
    get {
      if (functions.IsValueCreated) return functions.Value;
      lock (functionGate) return initializingFunctions ?? functions.Value;
    }
  }
  public IReadOnlyDictionary<string, HixBackendRoot> Roots => roots.Value;
  protected virtual void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    Hix.Functions.MetadataFunctions.Register(functions);
    Hix.Functions.Builtins.Register(functions);
    functions.Add(new Hix.Functions.NameFunction(), new Hix.Functions.UnwrapFunction());
    RegisterEmissionFunctions(functions);
  }
  protected virtual void RegisterEmissionFunctions(FunctionSignatureRegistryBuilder functions) {
    functions.AddLibrary("""
      %description<Emits a detached Hix value.>
      func emit(any value) -> null external;
      %description<Emits a detached Hix value to a named destination.>
      func emit(string destination, any value) -> null external;
      """);
    functions.Add(new SimpleFunction("emit", (thread, values) => {
      thread.Context.Emit(thread, values[values.Length - 1], values.Length == 2 ? thread.Text(values[0]) : default);
      return NullHixValue.Instance;
    }));
  }

  protected virtual void RegisterRoots(IDictionary<string, HixBackendRoot> roots) { }
  /// <summary>Optional host constraints on derivation records, beyond the language's required value field.</summary>
  public virtual ErrorHixValue ValidateDerivationRecord(HixCoroutine thread, HixTableValue record) => null;

  public virtual HixContext CreateContext() => new HixExpressionContext(this);
  public virtual HixContext CreateContext(HixStringPool strings) => strings == null ? CreateContext() : new HixExpressionContext(this, strings);
  public HixCoroutine CreateCoroutine() => new(CreateContext());
  public virtual IHixValue ResolveRoot(HixCoroutine thread, string name) => thread.Error("unknown root '" + name + "'");
  public virtual HixString Render(HixCoroutine thread, IHixValue value) => value.Render(thread);
  public virtual object UnlinkSnapshot(HixCoroutine thread, IHixValue value) => value.Unlink(thread);
  public virtual IHixValue DetachValue(HixCoroutine thread, IHixValue value) => thread.DetachCore(value);
  public virtual HixString NameOf(HixCoroutine thread, IHixValue value) => value.Render(thread);
  public virtual IHixValue Import(HixCoroutine thread, object value) => new ObjectHixValue(value);
  public virtual IHixValue Unwrap(HixCoroutine thread, IHixValue value) => value;
  public virtual bool IsType(HixCoroutine thread, IHixValue value, HixString type) => false;
  public virtual bool HasTrait(HixCoroutine thread, IHixValue value, HixString trait) => false;
  public virtual IHixValue Attributes(HixCoroutine thread, IHixValue value, HixString type, bool exact, bool first) => first ? NullHixValue.Instance : HixTableValue.Empty;
}
public sealed class HixCoreBackend : HixBackend {
  public static HixCoreBackend Instance { get; } = new();
}
