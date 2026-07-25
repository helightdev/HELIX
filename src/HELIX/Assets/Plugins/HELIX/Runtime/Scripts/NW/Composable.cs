using System;
using HELIX.Abstractions;
using Unity.Mathematics;
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

  public delegate void Composable<in T>(ref Composition ctx, T value);

  public interface IComposable : IElement {
    UssFlag Flag { get; set; }
    ulong TypeId { get; set; }
    void Reset();
  }

  public sealed class UserdataTracker : IComposable {
    public VisualElement Element { get; set; }
    public UssFlag Flag { get; set; }
    public ulong TypeId { get; set; }

    public Action OnReset { get; set; }

    public void Reset() {
      OnReset?.Invoke();
      OnReset = null;
    }
  }

  public sealed class CompositionNode : VisualElement, IComposable {
    public VisualElement Element => this;

    public UssFlag Flag { get; set; }
    public ulong TypeId { get; set; }

    public void Reset() { }

    public CompositionNode() {
      //name = $"Node{this.ShortHash()}";
    }
  }
}