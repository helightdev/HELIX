using System;
using HELIX.Abstractions;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [AttributeUsage(AttributeTargets.Method)]
  public class CompositionAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Class)]
  public class BoundaryComposableAttribute : Attribute {
    public Type Base { get; set; }
    public bool Extension { get; set; } = false;
    public string Name { get; set; } // Defaults to target type name
    public bool UseLookupCache { get; set; } = false;
  }

  [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
  public class ComposableProxyAttribute : Attribute {
    public Type Target { get; set; }

    public ComposableKind Kind { get; set; } = ComposableKind.Element;
    public string Name { get; set; } // Defaults to target type name
    public bool RequiresTracking { get; set; } = true;
    public bool Extension { get; set; } = true;
    public string CreateSyntax { get; set; } = "instance = new {TYPE}();";
    public string PrepareSyntax { get; set; } = "/* Skip Prepare */";
    public string PreYieldSyntax { get; set; } = "/* Skip Before Yield */";
    public string PostYieldSyntax { get; set; } = "/* Skip Post Yield */";
    public string ScopeCallbackSyntax { get; set; } = "cell.TrimChildren();";
  }

  public enum ComposableKind { ScopeElement, Element }

  // [ComposableProxy(
  //   Target = typeof(ScrollView),
  //   Kind = ComposableKind.ScopeElement,
  //   Name = "NativeScrollView",
  //   CreateSyntax = "instance = new {TYPE}();"
  // )]
  // public partial struct ScrollViewProxy {
  //   public static readonly PlainEventListener<float>.Binding SliderValueBinding = new(
  //     accessor: PlainEventAccessor<float>.Casting<Scroller>(
  //       subscribe: (scroller, action) => scroller.valueChanged += action,
  //       unsubscribe: (scroller, action) => scroller.valueChanged -= action
  //     ),
  //     targetSelector: static root => root.Q<Scroller>()
  //   );
  //
  //   [Prop(ScrollViewMode.Vertical)]
  //   public ScrollViewMode mode;
  //
  //   [Prop(ScrollView.NestedInteractionKind.Default)]
  //   public ScrollView.NestedInteractionKind nestedInteractionKind;
  //
  //   [Prop(ScrollerVisibility.Hidden, ProxySetter = "horizontalScrollerVisibility")]
  //   public ScrollerVisibility horizontalScroller;
  //
  //   [Prop(ScrollerVisibility.Hidden, ProxySetter = "verticalScrollerVisibility")]
  //   public ScrollerVisibility verticalScroller;
  //
  //   [Prop(null, ProxyFunction = "SliderValueBinding.Bind(instance, {VALUE});")]
  //   public CompositionAction<float> onVerticalScroll;
  // }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
  public class PropAttribute : Attribute {
    public object defaultValue;
    public PropInit defaultInit;

    public bool Equatable { get; set; } = true;
    public string EqualitySyntax { get; set; } = "{0} == {1}";
    public string HashCodeSyntax { get; set; } = "{0}";

    public string ProxyFunction { get; set; }
    public string ProxySetter { get; set; }
    public string ProxyGetter { get; set; }
    public bool ProxyEquality { get; set; } = false;

    public PropAttribute(
      object defaultValue,
      PropInit defaultInit = PropInit.Literal
    ) {
      this.defaultValue = defaultValue;
      this.defaultInit = defaultInit;
    }

    public PropAttribute() {
      defaultValue = null;
      defaultInit = PropInit.None;
    }

  }

  [AttributeUsage(AttributeTargets.Struct)]
  public class PropStructAttribute : Attribute { }

  public enum PropInit {
    /// <summary>
    /// The literal object value of this will be used as the constructor parameter initializer.
    /// </summary>
    Literal,

    /// <summary>
    /// The constructor parameter will have the string content of this as the constant initializer.
    /// </summary>
    Constant,

    /// <summary>
    /// The constructor parameter will be nullable and default to null.
    /// The string content of the annotation is used as the initializer if the parameter is null.
    /// </summary>
    Deferred,

    None,
  }

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
    /// <summary>
    /// The owning boundary of this context.
    /// </summary>
    ///
    public readonly IBoundary boundary;

    /// <summary>
    /// The closest composable in the owner chain.
    /// </summary>
    public readonly IComposable composable;

    /// <summary>
    /// The target element to which the context instance belongs.
    /// </summary>
    public readonly VisualElement element;

    public CompositionContext(IBoundary boundary) {
      this.boundary = boundary;
      composable = boundary;
      element = boundary.Element;
    }

    public CompositionContext(IComposable composable, VisualElement element) : this() {
      this.composable = composable;
      this.element = element;
      if (composable is IBoundary selfBoundary) {
        boundary = selfBoundary;
      } else {
        boundary = composable.Element.GetFirstAncestorOfType<IBoundary>();
      }
    }

    public CompositionContext(IComposable composable) : this(composable, composable.Element) { }

    public static implicit operator CompositionContext(Composition cx) => new(cx.boundary);
    public static implicit operator CompositionContext(BoundaryElementBase boundary) => new(boundary);
  }

  public interface IComposable : IElement {
    UssFlag Flag { get; set; }
    ulong PackedId { get; set; }
    void Reset();
    void MarkFlag(UssFlag flag);
  }

  public interface IDirty {
    void MarkDirty();
  }

  public interface IStateAttachmentHolder : IComposable {
    ref StateAttachmentStore StateAttachmentStore { get; }
  }

  public sealed class UserdataTracker : IComposable, IStateAttachmentHolder {
    public VisualElement Element { get; set; }
    public UssFlag Flag { get; set; }
    public ulong PackedId { get; set; }

    public Action OnReset { get; set; }

    private StateAttachmentStore _stateAttachmentStore;

    public ref StateAttachmentStore StateAttachmentStore => ref _stateAttachmentStore;

    public void Reset() {
      OnReset?.Invoke();
      OnReset = null;
      StateAttachmentStore.Dispose();
    }

    public void MarkFlag(UssFlag flag) {
      Flag |= flag;
    }
  }

  public abstract class ComposableElement : VisualElement, IStateAttachmentHolder {
    public VisualElement Element => this;

    public UssFlag Flag { get; set; }
    public ulong PackedId { get; set; }
    public object ComposableUserData { get; set; }
    private StateAttachmentStore _stateAttachmentStore;

    public ref StateAttachmentStore StateAttachmentStore => ref _stateAttachmentStore;

    public virtual void Reset() {
      _stateAttachmentStore.Dispose();
    }

    public void MarkFlag(UssFlag flag) {
      Flag |= flag;
    }
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
      var scope = HXComposer.BeginBatch();
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
      var scope = HXComposer.BeginBatch();
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
      using (HXComposer.BeginBatch()) action.Invoke(new CompositionContext(boundary));
    }

    public static void Call<T>(this CompositionAction<T> action, IBoundary boundary, T arg) {
      if (action == null) return;
      using (HXComposer.BeginBatch()) action.Invoke(new CompositionContext(boundary), arg);
    }

    public static void Call<T0, T1>(this CompositionAction<T0, T1> action, IBoundary boundary, T0 arg0, T1 arg1) {
      if (action == null) return;
      using (HXComposer.BeginBatch()) action.Invoke(new CompositionContext(boundary), arg0, arg1);
    }

    public static void Call<T0, T1, T2>(
      this CompositionAction<T0, T1, T2> action, IBoundary boundary, T0 arg0, T1 arg1, T2 arg2
    ) {
      using (HXComposer.BeginBatch()) action?.Invoke(new CompositionContext(boundary), arg0, arg1, arg2);
    }

    public static void Call<T0, T1, T2, T3>(
      this CompositionAction<T0, T1, T2, T3> action, IBoundary boundary, T0 arg0, T1 arg1, T2 arg2, T3 arg3
    ) {
      using (HXComposer.BeginBatch()) action?.Invoke(new CompositionContext(boundary), arg0, arg1, arg2, arg3);
    }
  }
}
