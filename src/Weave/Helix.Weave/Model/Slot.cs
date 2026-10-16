namespace Helix.Weave;

public enum Scope {
  /// <summary>One value per generated type. Survives across mixins on that type.</summary>
  Type,
  /// <summary>One value per annotated symbol.</summary>
  Target,
}

/// <summary>
/// Typed state slot, replacing Hix's untyped <c>var#</c> / <c>tar#</c> bags. Values must be
/// detached (see the purity rules); the slot identity is the instance itself.
/// </summary>
public sealed class Slot<T> {
  public Slot(Scope scope, string name = null) {
    Scope = scope;
    Id = SlotId.Next();
    Name = name ?? typeof(T).Name + "#" + Id;
  }

  public Scope Scope { get; }
  public int Id { get; }
  public string Name { get; }
  public T Default { get; init; }
  public override string ToString() => Name;
}

/// <summary>
/// Slot ids come from here rather than a static on Slot&lt;T&gt;: statics on a generic type are
/// per closed type, so Slot&lt;bool&gt; and Slot&lt;string&gt; would both start at 1 and collide.
/// </summary>
internal static class SlotId {
  private static int _next;
  internal static int Next() => System.Threading.Interlocked.Increment(ref _next);
}
