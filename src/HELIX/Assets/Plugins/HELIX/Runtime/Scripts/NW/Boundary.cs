using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HELIX.Extensions;
using UnityEngine.UIElements;

namespace HELIX.NW {
  public interface IBoundary : IComposable {
    IBoundary Parent { get; }
    BoundaryCell Cell { get; }
    NodeState State { get; }
    int TreeDepth { get; }

    SparseContextMap AcquireContext();
    void ContributeContext(Dictionary<Type, IContextData> context);

    void RefreshHierarchy();
    void SetState(NodeState state);
    void Recompose();
    void DisposeState();
  }

  public class BoundaryCell {
    public static readonly BoundaryCell Shared = new();

    public IComposable current;
    public int cursor;
    public ushort key;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VisualElement ReadCursor() {
      var element = current.Element;
      return element.childCount <= cursor ? null : element.ElementAt(cursor);
    }

    public void TrimChildren() {
      var element = current.Element;
      var overflow = element.childCount - cursor;
      for (var i = 0; i < overflow; i++) {
        element.RemoveAt(element.childCount - 1);
      }
      //if (overflow > 0) Debug.LogWarning($"Removed {overflow} children");
    }
  }

  public abstract class CompositionBoundaryNodeBase : VisualElement, IBoundary {
    public VisualElement Element => this;
    public Composable composable;
    public SparseContextMap Context { get; private set; }
    public int TreeDepth { get; private set; }

    public SparseContextMap AcquireContext() {
      Context ??= SparseContextMap.Get();
      return Context;
    }

    public void ContributeContext(Dictionary<Type, IContextData> context) {
      Context?.LoadInto(context);
    }

    protected virtual void BeginContext() {
      Context?.ResetMarkers();
    }

    protected virtual void EndContext() {
      if (Context == null) return;
      Context.Prune();
      if (Context.Count != 0) return;
      SparseContextMap.Release(Context);
      Context = null;
    }

    public IBoundary Parent { get; private set; }
    public NodeState State { get; private set; }
    public BoundaryCell Cell { get; } = BoundaryCell.Shared;

    public UssDirtyFlags DirtyFlags { get; set; }
    public ulong TypeId { get; set; }

    protected CompositionBoundaryNodeBase() {
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
      RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    public void RefreshHierarchy() {
      Parent = GetFirstAncestorOfType<IBoundary>();
    }

    public void SetState(NodeState state) {
      if (State != null) DisposeState();
      State = state;
    }

    public void Reset() {
      State?.Clear();
    }

    public void DisposeState() {
      State?.Dispose();
      State = null;

      Context?.Clear();
      if (Context != null) SparseContextMap.Release(Context);
      Context = null;
    }

    public void Recompose() {
      try {
        RecompositionScope.MarkClean(this);
        BeginContext();
        Compose();
      } finally {
        EndContext();
        RecompositionScope.MarkClean(this);
      }
    }

    public void Compose() {
      var composition = new Composition(this);
      State?.OnRecompose(ref composition, this);
      composable?.Invoke(ref composition);
    }

    protected virtual void OnAttachToPanel(AttachToPanelEvent evt) {
      TreeDepth = this.GetDepth();
      RefreshHierarchy();
    }

    protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt) {
      DisposeState();
    }
  }

  public sealed class CompositionBoundaryNode : CompositionBoundaryNodeBase { }

  public abstract class NodeState : IDisposable {
    protected INodeStateAttachment attachment;

    protected NodeState() { }

    public void SetAttachment(INodeStateAttachment updated, IBoundary boundary) {
      attachment = updated;
      updated.OnAttach(this, boundary);
    }

    public INodeStateAttachment GetAttachment() => attachment;

    public void OnRecompose(ref Composition cx, IBoundary boundary) {
      attachment?.OnRecompose(ref cx, this, boundary);
    }

    // May be called multiple times
    public virtual void Clear() {
      attachment?.OnDetach(this);
      attachment = null;
    }

    public virtual void Dispose() {
      if (attachment == null) return;
      Clear();
    }
  }

  public interface INodeStateAttachment {
    void OnAttach(NodeState state, IBoundary boundary);
    void OnDetach(NodeState state);
    void OnRecompose(ref Composition cx, NodeState state, IBoundary boundary);
  }

  public abstract class NodeStateAttachmentBase<TState> : INodeStateAttachment where TState : NodeState {
    public TState State { get; protected set; }
    public CompositionBoundaryNodeBase Node { get; protected set; }

    protected virtual void OnAttach() {}

    protected virtual void OnDetach() {}

    public void OnAttach(NodeState state, IBoundary boundary) {
      if (state is not TState typedState) throw new InvalidOperationException();
      State = typedState;
      Node = boundary as CompositionBoundaryNodeBase;
      if (Node == null) throw new InvalidOperationException();
      OnAttach();
    }

    public void OnDetach(NodeState state) {
      if (Node != null && State != null) OnDetach();
      State = null;
      Node = null;
    }
    public virtual void OnRecompose(ref Composition cx, NodeState state, IBoundary boundary) {
      if (State == null || Node == null) return;
      OnRecompose(ref cx);
    }

    protected virtual void OnRecompose(ref Composition cx) {}
  }

  public abstract class PropsNodeStateAttachmentBase<TProps> : NodeStateAttachmentBase<NodeState<TProps>>
    where TProps : struct {

    public TProps Props {
      get => State.props;
      set => State.props = value;
    }

    public virtual void ReceiveProps(TProps props) {
      State.props = props;
    }

  }

  public abstract class NodeState<T> : NodeState where T : struct {
    public T props;
  }

  public sealed class GenericPropsState<T> : NodeState<T> where T : struct { }
  public sealed class AnonymousNodeState : NodeState { }
}