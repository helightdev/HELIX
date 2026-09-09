using System;
using System.Threading;

namespace Hix.Runtime;

/// <summary>Subclassable host data and persistent language storage. Active frames belong to HixThread.</summary>
public class HixContext {
  private static readonly HixStringPool EmptyStrings = new HixStringPoolBuilder().Freeze();

  public HixContext(HixBackend backend = null, HixStringPool strings = null) {
    Backend = backend ?? HixCoreBackend.Instance;
    Strings = strings ?? EmptyStrings;
  }

  public HixBackend Backend { get; }
  public HixStringPool Strings { get; }
  private int running;
  internal void Acquire() {
    if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
      throw new InvalidOperationException("A Hix context is already in use by another execution");
  }
  internal void Release() => Volatile.Write(ref running, 0);

  public HixValueDictionary Variables { get; } = new();
  public HixValueDictionary Carries { get; } = new();
  public HixValueDictionary TargetVariables { get; } = new();
  /// <summary>Reset host effect buffers for a new invocation; keep persistent host data.</summary>
  public virtual void BeginExecution() { }
  /// <summary>Capture an allocation-free rollback token for buffered host effects.</summary>
  public virtual int CaptureEffects() => 0;
  public virtual void RollbackEffects(int checkpoint) { }

  public virtual IHixValue ResolveHost(HixThread thread, HixExpressionRoot root, HixString member) =>
    Backend.ResolveRoot(thread, root.ToString().ToLowerInvariant()).Select(thread, member);
}
