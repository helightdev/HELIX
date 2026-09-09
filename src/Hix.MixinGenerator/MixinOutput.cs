using Hix.Runtime;
using System;
using System.Collections.Generic;

namespace Hix.Mixins;

public enum MixinEmissionTarget {
  Target,
  Class,
  File,
  Extends,
  Implements,
  Injection,
  Annotation,
  Using,
  Mixin
}

/// <summary>Output handles retain their immutable originating pool for later host rendering.</summary>
public readonly struct MixinOutput {
  public MixinOutput(
    MixinEmissionTarget target, HixString text, HixString injectionTarget = default,
    int injectionPriority = 0, HixStringPool strings = null
  ) {
    Strings = strings;
    Target = target;
    this.text = text;
    this.injectionTarget = injectionTarget;
    InjectionPriority = injectionPriority;
  }

  private readonly HixString text;
  private readonly HixString injectionTarget;
  public MixinEmissionTarget Target { get; }
  public HixString Text => text.IsNull ? HixString.Empty : text;
  public HixString InjectionTarget => injectionTarget.IsNull ? HixString.Empty : injectionTarget;
  public int InjectionPriority { get; }
  public HixStringPool Strings { get; }
  public string ResolveText() => Text.Resolve(Strings);
  public string ResolveInjectionTarget() => InjectionTarget.Resolve(Strings);
  public bool IsEmpty => Text.Length(Strings) == 0;
  public MixinOutput Retarget(MixinEmissionTarget target) =>
    new(target, text, injectionTarget, InjectionPriority, Strings);
}

public sealed class MixinOutputBuffer {
  private readonly List<MixinOutput> outputs = [];
  public IReadOnlyList<MixinOutput> Outputs => outputs;
  public int Count => outputs.Count;
  public void Add(MixinOutput output) => outputs.Add(output);
  public void Clear() => outputs.Clear();
  public void Rollback(int count) => outputs.RemoveRange(count, outputs.Count - count);
}

public interface IMixinOutputContext {
  MixinOutputBuffer Emissions { get; }
}

/// <summary>Buffered generator emissions for hosts without Roslyn semantic data.</summary>
public class MixinOutputContext(HixBackend backend, HixStringPool strings = null) : HixContext(backend, strings), IMixinOutputContext {
  public MixinOutputBuffer Emissions { get; } = new();
  public override void BeginExecution() => Emissions.Clear();
  public override int CaptureEffects() => Emissions.Count;
  public override void RollbackEffects(int checkpoint) => Emissions.Rollback(checkpoint);
}
