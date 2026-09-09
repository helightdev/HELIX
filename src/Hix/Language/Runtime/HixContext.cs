namespace Hix.Runtime;

/// <summary>Subclassable host data and committed target storage. Execution state belongs to HixThread.</summary>
public class HixContext {
  private static readonly HixStringPool EmptyStrings = new HixStringPoolBuilder().Freeze();

  public HixContext(HixBackend backend = null, HixStringPool strings = null) {
    Backend = backend ?? HixCoreBackend.Instance;
    Strings = strings ?? EmptyStrings;
  }

  public HixBackend Backend { get; }
  public HixStringPool Strings { get; }
  public HixValueDictionary TargetVariables { get; } = new();
  /// <summary>Reset host effect buffers for a new invocation; keep persistent host data.</summary>
  public virtual void BeginExecution() { }
  /// <summary>Capture an allocation-free rollback token for buffered host effects.</summary>
  public virtual int CaptureEffects() => 0;
  public virtual void RollbackEffects(int checkpoint) { }

  public virtual IHixValue ResolveHost(HixThread thread, HixExpressionRoot root, HixString member) =>
    Backend.ResolveRoot(thread, root.ToString().ToLowerInvariant()).Select(thread, member);
}
