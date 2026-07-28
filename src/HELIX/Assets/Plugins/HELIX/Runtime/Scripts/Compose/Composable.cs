using System;
using HELIX.Abstractions;
using UnityEngine.UIElements;

namespace HELIX.Compose {
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

  public delegate void Composable(ref Composition cx);

  public delegate void Composable<in T>(ref Composition cx, T value);

  public delegate void Composable<in T0, in T1>(ref Composition cx, T0 arg0, T1 arg1);

  public delegate void Composable<in T0, in T1, in T2>(ref Composition cx, T0 arg0, T1 arg1, T2 arg2);

  public delegate void Composable<in T0, in T1, in T2, in T3>(ref Composition cx, T0 arg0, T1 arg1, T2 arg2, T3 arg3);

  public delegate void ReadComposable<T>(ref Composition cx, in T value);

  public delegate void CompositionAction(CompositionContext ctx);

  public delegate void CompositionAction<in T>(CompositionContext ctx, T arg);

  public delegate void CompositionAction<in T0, in T1>(CompositionContext ctx, T0 arg0, T1 arg1);

  public delegate void CompositionAction<in T0, in T1, in T2>(CompositionContext ctx, T0 arg0, T1 arg1, T2 arg2);

  public delegate void CompositionAction<in T0, in T1, in T2, in T3>(
    CompositionContext ctx, T0 arg0, T1 arg1, T2 arg2, T3 arg3
  );

  public readonly ref struct CompositionContext {
    public readonly IBoundary boundary;

    public CompositionContext(IBoundary boundary) {
      this.boundary = boundary;
    }

    public static implicit operator CompositionContext(Composition cx) => new(cx.boundary);
    public static implicit operator CompositionContext(BoundaryElementBase boundary) => new(boundary);
  }

  public interface IComposable : IElement {
    UssFlag Flag { get; set; }
    ulong TypeId { get; set; }
    void Reset();
  }

  public interface IDirty {
    void MarkDirty();
  }

  public sealed class UserdataTracker : IComposable {
    public VisualElement Element { get; set; }
    public UssFlag Flag { get; set; }
    public ulong TypeId { get; set; }

    public Action OnReset { get; set; }
    public object Userdata { get; set; }

    public void Reset() {
      OnReset?.Invoke();
      OnReset = null;
      if (Userdata is IDisposable disposable) disposable.Dispose();
      Userdata = null;
    }
  }

  public abstract class ComposableElement : VisualElement, IComposable {
    public VisualElement Element => this;

    public UssFlag Flag { get; set; }
    public ulong TypeId { get; set; }
    public object ComposableUserData { get; set; }

    public virtual void Reset() { }
  }

  public sealed class CompositionNode : ComposableElement {
    public override void Reset() {
      if (ComposableUserData is IDisposable disposable) disposable.Dispose();
      ComposableUserData = null;
    }

    public CompositionNode() {
      //name = $"Node{this.ShortHash()}";
    }
  }

  public static class BoundaryLookupExtensions {
    public static T LookupBoundary<T>(this IBoundary boundary, bool includeHost = true) {
      var current = includeHost ? boundary : boundary.Parent;
      while (current != null) {
        if (current is T typed) return typed;
        current = current.Parent;
      }
      return default;
    }

    public static T Lookup<T>(this IBoundary boundary, bool includeHost = true) where T : IBoundary {
      return boundary.LookupBoundary<T>(includeHost);
    }

    public static T Lookup<T>(this CompositionContext context, bool includeHost = true) where T : IBoundary {
      return context.boundary.LookupBoundary<T>(includeHost);
    }

    public static T Lookup<T>(this Composition context, bool includeHost = true) where T : IBoundary {
      return context.boundary.LookupBoundary<T>(includeHost);
    }

    public static RecompositionScope Modify<T>(
      this IBoundary self, out T boundary, bool includeHost = true
    ) where T : IBoundary {
      boundary = self.LookupBoundary<T>(includeHost);
      if (boundary == null) {
        throw new InvalidOperationException($"Boundary of type {typeof(T)} not found in tree.");
      }
      var scope = HX.BatchScope();
      boundary.MarkDirty();
      return scope;
    }

    public static RecompositionScope Modify<T>(
      this CompositionContext self, out T boundary, bool includeHost = true
    ) where T : IBoundary => self.boundary.Modify(out boundary, includeHost);

    public static RecompositionScope Modify<T>(
      this Composition self, out T boundary, bool includeHost = true
    ) where T : IBoundary => self.boundary.Modify(out boundary, includeHost);
  }

  public static class ComposableLookupExtensions {
    public static T LookupComposable<T>(this IBoundary boundary, bool includeHost = true) {
      var current = includeHost ? boundary : boundary.Parent;
      while (current != null) {
        if (current is CompositionBoundaryNodeBase { BoundaryComposable: T attachment }) return attachment;
        current = current.Parent;
      }
      return default;
    }

    public static T Lookup<T>(this IBoundary boundary, bool includeHost = true) where T : IBoundaryComposable {
      return boundary.LookupComposable<T>(includeHost);
    }

    public static T Lookup<T>(this CompositionContext context, bool includeHost = true) where T : IBoundaryComposable {
      return context.boundary.LookupComposable<T>(includeHost);
    }

    public static T Lookup<T>(this Composition context, bool includeHost = true) where T : IBoundaryComposable {
      return context.boundary.LookupComposable<T>(includeHost);
    }

    public static RecompositionScope Modify<T>(
      this IBoundary self, out T composable, bool includeHost = true
    ) where T : IBoundaryComposable {
      composable = self.LookupComposable<T>(includeHost);
      if (composable == null) {
        throw new InvalidOperationException($"BoundaryComposable of type {typeof(T)} not found in tree.");
      }
      var scope = HX.BatchScope();
      composable.MarkDirty();
      return scope;
    }

    public static RecompositionScope Modify<T>(
      this CompositionContext self, out T composable, bool includeHost = true
    ) where T : IBoundaryComposable => self.boundary.Modify(out composable, includeHost);

    public static RecompositionScope Modify<T>(
      this Composition self, out T composable, bool includeHost = true
    ) where T : IBoundaryComposable => self.boundary.Modify(out composable, includeHost);
  }

  public static class DataLookupExtensions {
    public static T LookupData<T>(this IBoundary boundary, bool includeHost = true) {
      var current = includeHost ? boundary : boundary.Parent;
      while (current != null) {
        if (current is CompositionBoundaryNodeBase { Data: T data }) return data;
        current = current.Parent;
      }
      return default;
    }

    public static T Lookup<T>(this IBoundary boundary, bool includeHost = true) where T : BoundaryData {
      return boundary.LookupData<T>(includeHost);
    }

    public static T Lookup<T>(this CompositionContext context, bool includeHost = true) where T : BoundaryData {
      return context.boundary.LookupData<T>(includeHost);
    }

    public static T Lookup<T>(this Composition context, bool includeHost = true) where T : BoundaryData {
      return context.boundary.LookupData<T>(includeHost);
    }
  }

  public static class CompositionActionExtensions {
    public static void Call(this CompositionAction action, IBoundary boundary) {
      if (action == null) return;
      using (HX.BatchScope()) action.Invoke(new CompositionContext(boundary));
    }

    public static void Call<T>(this CompositionAction<T> action, IBoundary boundary, T arg) {
      if (action == null) return;
      using (HX.BatchScope()) action.Invoke(new CompositionContext(boundary), arg);
    }

    public static void Call<T0, T1>(this CompositionAction<T0, T1> action, IBoundary boundary, T0 arg0, T1 arg1) {
      if (action == null) return;
      using (HX.BatchScope()) action.Invoke(new CompositionContext(boundary), arg0, arg1);
    }

    public static void Call<T0, T1, T2>(
      this CompositionAction<T0, T1, T2> action, IBoundary boundary, T0 arg0, T1 arg1, T2 arg2
    ) {
      using (HX.BatchScope()) action?.Invoke(new CompositionContext(boundary), arg0, arg1, arg2);
    }

    public static void Call<T0, T1, T2, T3>(
      this CompositionAction<T0, T1, T2, T3> action, IBoundary boundary, T0 arg0, T1 arg1, T2 arg2, T3 arg3
    ) {
      using (HX.BatchScope()) action?.Invoke(new CompositionContext(boundary), arg0, arg1, arg2, arg3);
    }
  }
}