using System.Collections.Generic;

namespace Helix.Weave;

public enum EmissionTarget {
  /// <summary>Member of the generated partial type.</summary>
  Class,
  /// <summary>Top-level declaration in the generated file.</summary>
  File,
  /// <summary>Base type of the generated partial.</summary>
  Extends,
  /// <summary>Interface on the generated partial.</summary>
  Implements,
  /// <summary>Attribute on the generated partial.</summary>
  Annotation,
  /// <summary>Using directive.</summary>
  Using,
  /// <summary>Statement woven into a named injection target.</summary>
  Injection,
}

public readonly struct MixinOutput {
  public MixinOutput(EmissionTarget target, StringHandle text, StringHandle injectionTarget = default,
                     int priority = 0, int sequence = 0) {
    Target = target;
    Text = text;
    InjectionTarget = injectionTarget;
    Priority = priority;
    Sequence = sequence;
  }

  public EmissionTarget Target { get; }
  public StringHandle Text { get; }
  public StringHandle InjectionTarget { get; }
  public int Priority { get; }
  /// <summary>Insertion order, the tie-break within equal <see cref="Priority"/>.</summary>
  public int Sequence { get; }
}

/// <summary>Accumulates emissions and a rolling fingerprint so comparers can short-circuit.</summary>
public sealed class OutputBuffer {
  private const ulong Offset = 14695981039346656037UL;
  private const ulong Prime = 1099511628211UL;

  private readonly List<MixinOutput> _outputs = new();
  private ulong _hash = Offset;
  private int _sequence;

  public IReadOnlyList<MixinOutput> Outputs => _outputs;
  public int Count => _outputs.Count;
  public ulong Fingerprint => _hash;

  public void Add(EmissionTarget target, StringHandle text, StringHandle injectionTarget = default, int priority = 0) {
    var output = new MixinOutput(target, text, injectionTarget, priority, _sequence++);
    _outputs.Add(output);
    Mix((ulong)(int)target);
    Mix((ulong)text.Id);
    Mix((ulong)injectionTarget.Id);
    Mix(unchecked((ulong)priority));
  }

  public int Checkpoint() => _outputs.Count;

  /// <summary>Discards everything added since <paramref name="checkpoint"/>; the fingerprint is rebuilt.</summary>
  public void Rollback(int checkpoint) {
    if (checkpoint >= _outputs.Count) return;
    _outputs.RemoveRange(checkpoint, _outputs.Count - checkpoint);
    _hash = Offset;
    foreach (var output in _outputs) {
      Mix((ulong)(int)output.Target);
      Mix((ulong)output.Text.Id);
      Mix((ulong)output.InjectionTarget.Id);
      Mix(unchecked((ulong)output.Priority));
    }
  }

  private void Mix(ulong value) {
    unchecked {
      _hash ^= value;
      _hash *= Prime;
    }
  }
}
