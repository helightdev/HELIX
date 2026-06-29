using System;
using HELIX.Abstractions;
using HELIX.Diagnostics;
using UnityEngine.UIElements;

namespace HELIX.NW {

  [AttributeUsage(AttributeTargets.Method)]
  public class CompositionAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Method)]
  public class CompositionBoundaryAttribute : Attribute {
    public Type Base { get; set; }
  }

  [AttributeUsage(AttributeTargets.Parameter)]
  public class PropAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Field)]
  public class ContextAttribute : Attribute { }

  public delegate void Composable(ref Composition ctx);

  public delegate void InlineComposable(ref Composition ctx);

  public delegate void InlineComposable<in T>(ref Composition ctx, T value);

  public interface IComposable : IElement {
    UssDirtyFlags DirtyFlags { get; set; }
    ulong TypeId { get; set; }
    void Reset();
  }

  public sealed class UserdataTracker : IComposable {
    public VisualElement Element { get; set; }
    public UssDirtyFlags DirtyFlags { get; set; }
    public ulong TypeId { get; set; }
    public void Reset() { }
  }

  public sealed class CompositionNode : VisualElement, IComposable {
    public VisualElement Element => this;

    public UssDirtyFlags DirtyFlags { get; set; }
    public ulong TypeId { get; set; }

    public void Reset() { }

    public CompositionNode() {
      //name = $"Node{this.ShortHash()}";
    }
  }
}