using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HELIX.Compose.Collections;
using HELIX.Extensions;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace HELIX.Compose {
  public interface IBoundary : IContextComposable, IContextWriteable, IDirty, IDisposable, IPossiblyDisposed {
    IBoundary Parent { get; }
    BoundaryCell Cell { get; }

    int TreeDepth { get; }

    void RefreshHierarchy();
    void CheckModified();
    void UseLookupCache();

    void Recompose();
    void SubscribeToContextData(int key, ContextData data);
  }

  public class BoundaryCell {
    public static readonly BoundaryCell Shared = new();

    public IComposable scope;
    public int cursor;
    public LocalId localId;
    public ComposableSlot slot;
    public bool skip;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VisualElement ReadCursor() {
      var element = scope.Element;
      return element.childCount <= cursor ? null : element.ElementAt(cursor);
    }

    public VisualElement ReadCursorOrFind(ulong typeId) {
      // Get the current cursor or finds the element after the current curser if it already exists
      var element = ReadCursor();
      if (element == null) return null;
      if (GetTypeId(element) == typeId) return element;
      for (var i = cursor + 1; i < scope.Element.childCount; i++) {
        var child = scope.Element.ElementAt(i);
        if (GetTypeId(child) == typeId) return child;
      }
      return element;
    }

    public ulong GetTypeId(VisualElement element) {
      if (element is IComposable composable) {
        return composable.PackedId;
      } else {
        if (element.userData is UserdataTracker tracker) return tracker.PackedId;
        return 0;
      }
    }

    public void TrimChildren() {
      var element = scope.Element;
      var overflow = element.childCount - cursor;
      for (var i = 0; i < overflow; i++) {
        //Debug.Log($"Removing child {i} from {element.name}");
        element.RemoveAt(element.childCount - 1);
      }
      HXProfiling.TrackHierarchyDeletion(overflow);
      //if (overflow > 0) Debug.LogWarning($"Removed {overflow} children");
    }
  }

  public abstract class BoundaryElementBase : VisualElement, IBoundary {
    public VisualElement Element => this;
    public BoundaryCell Cell { get; } = BoundaryCell.Shared;
    public SparseContextMap WrittenContext { get; protected set; }
    public int TreeDepth { get; protected set; }

    public IBoundary Parent { get; protected set; }
    public IContextComposable ContextParent { get; protected set; }

    public bool IsDisposed { get; protected set; }
    public UssFlag Flag { get; set; }

    public ulong PackedId { get; set; }


    private bool _initialAttachment = true;
    private LookupCache _lookupCache;


    public virtual void Dispose() {
      if (IsDisposed) return;
      IsDisposed = true;
      Reset();
    }

    protected BoundaryElementBase() {
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
      RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
      HXComposer.RegisterActiveBoundary(this);
      //name = $"Boundary{this.ShortHash()}";
      //generateVisualContent += GenerateDebugVisuals;

      if (HXComposer.IsProcessing) {
        _initialAttachment = false;
        HXComposer.MarkDirty(this);
      }
    }

    public void RefreshHierarchy() {
      Parent = GetFirstAncestorOfType<IBoundary>();
      ContextParent = GetFirstAncestorOfType<IContextComposable>();
    }

    public void CheckModified() {
      if (WrittenContext != null && WrittenContext.CheckSubscriptionsModified()) {
        HXComposer.EnqueueDirty(this);
      }
    }

    public void UseLookupCache() {
      _lookupCache.Claim();
    }

    public void ContributeContext(Dictionary<int, ContextData> context) {
      WrittenContext?.LoadInto(context);
    }

    public bool TryLookupContext(int key, out ContextData data) {
      data = null;
      if (WrittenContext != null && WrittenContext.TryGet(key, out data)) return true;
      return _lookupCache.TryLookup(ContextParent, key, out data);
    }

    public virtual void Reset() {
      HXComposer.RemoveDirty(this);
      _lookupCache.Release();
    }

    protected virtual void BeforeCompose() {
      _lookupCache.Clear();
      WrittenContext?.ResetSubscriptionMarkers();
    }

    protected virtual void AfterCompose() {
      if (WrittenContext == null) return;
      WrittenContext.PruneSubscriptions();
      if (!WrittenContext.IsUnused) return;
      SparseContextMap.Release(WrittenContext);
      WrittenContext = null;
    }

    public virtual void Recompose() {
      try {
        //rebuildCount++;
        HXComposer.RemoveDirty(this);
        BeforeCompose();
        try {
          var composition = new Composition(this);
          PerformCompose(ref composition);
        } catch (Exception e) {
          Debug.LogException(e);
        }
        Cell.TrimChildren(); // TODO: Maybe?
      } finally {
        AfterCompose();
        HXComposer.RemoveDirty(this);
        //MarkDirtyRepaint();
      }
    }

    public void SubscribeToContextData(int key, ContextData data) {
      AcquireWriteableContext().Subscribe(key, data);
    }

    public abstract void PerformCompose(ref Composition cx);

    protected virtual void OnAttachToPanel(AttachToPanelEvent evt) {
      if (IsDisposed) {
        Debug.LogError("Attaching already disposed boundary to panel!");
        HXComposer.RemoveDirty(this);
        HXComposer.UnregisterBoundary(this);
        RemoveFromHierarchy();
        return;
      }

      HXComposer.RegisterActiveBoundary(this);
      TreeDepth = this.GetDepth();
      RefreshHierarchy();

      if (_initialAttachment) {
        _initialAttachment = false;
        //Debug.Log("Marking dirty");
        HXComposer.MarkDirty(this);
      }
    }

    protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt) {
      _initialAttachment = true;
      _lookupCache.Release();
      HXComposer.NotifyDetach(this);
    }

    public SparseContextMap AcquireWriteableContext() {
      return WrittenContext ??= SparseContextMap.Get();
    }

    public void BeginContextModification() {
      WrittenContext?.ResetPublicationMarkers();
    }

    public void EndContextModification() {
      if (WrittenContext == null) return;
      WrittenContext.PrunePublications();
    }

    public void MarkDirty() {
      HXComposer.MarkDirty(this, false);
    }
  }

  public abstract class CompositionBoundaryNodeBase : BoundaryElementBase {
    public IBoundaryComposable BoundaryComposable { get; private set; }

    public BoundaryData Data { get; private set; }

    //private int rebuildCount = 0;

    // private void GenerateDebugVisuals(MeshGenerationContext obj) {
    //   obj.painter2D.PathRect(layout.WithPosition(Vector2.zero));
    //   var color = Colors.Hsv(rebuildCount % 60 / 60f, 0.5f, 1f);
    //   obj.painter2D.strokeColor = color;
    //   obj.painter2D.Stroke();
    //
    //   // Draw id as text
    //   obj.DrawText(new CompositionId() {
    //     packed = TypeId
    //   }.ToString(), new Vector2(0, -10), 12, color);
    //
    // }

    public void SetDataOnly(BoundaryData data) {
      if (Data != null) DisposeState();
      Data = data;
    }

    public void SetComposable(BoundaryData data, IBoundaryComposable boundaryComposable) {
      if (Data != null) DisposeState();
      Data = data;
      BoundaryComposable = boundaryComposable;
      boundaryComposable.OnAttach(data, this);
    }

    public void SwapComposable(IBoundaryComposable boundaryComposable) {
      if (Data == null) throw new InvalidOperationException("Cannot swap attachment on boundary with no state");
      BoundaryComposable?.OnDetach(Data, this);
      BoundaryComposable = boundaryComposable;
      boundaryComposable.OnAttach(Data, this);
    }

    public override void Reset() {
      base.Reset();
      if (Data == null) return;
      BoundaryComposable?.OnDetach(Data, this);
      BoundaryComposable = null;
      Data.Clear();
    }

    public void DisposeState() {
      if (Data != null) {
        BoundaryComposable?.OnDetach(Data, this);
        BoundaryComposable = null;
        Data.Dispose();
        Data = null;
      }

      WrittenContext?.Clear();
      if (WrittenContext != null) SparseContextMap.Release(WrittenContext);
      WrittenContext = null;
    }

    protected override void OnDetachFromPanel(DetachFromPanelEvent evt) {
      base.OnDetachFromPanel(evt);
      DisposeState();
    }
  }

  public sealed class CompositionBoundaryNode : CompositionBoundaryNodeBase {
    public Composable composable;

    public override void PerformCompose(ref Composition cx) {
      Data?.OnRecompose(ref cx, this);
      BoundaryComposable?.OnRecompose(ref cx, Data, this);
      composable?.Invoke(ref cx);
    }
  }

  public abstract class BoundaryData : IDisposable {
    //protected INodeStateAttachment attachment;

    protected BoundaryData() { }

    // public void SetAttachment(INodeStateAttachment updated, IBoundary boundary) {
    //   attachment = updated;
    //   updated.OnAttach(this, boundary);
    // }
    //
    // public INodeStateAttachment GetAttachment() => attachment;

    public void OnRecompose(ref Composition cx, IBoundary boundary) {
      //attachment?.OnRecompose(ref cx, this, boundary);
    }

    // May be called multiple times
    public virtual void Clear() {
      // attachment?.OnDetach(this);
      // attachment = null;
    }

    public virtual void Dispose() {
      //if (attachment == null) return;
      Clear();
    }
  }

  public interface IBoundaryComposable : IDirty {
    void OnAttach(BoundaryData data, IBoundary boundary);
    void OnDetach(BoundaryData data, IBoundary boundary);
    void OnRecompose(ref Composition cx, BoundaryData data, IBoundary boundary);
  }

  public abstract class BoundaryComposable : IBoundaryComposable {
    public CompositionBoundaryNodeBase Node { get; protected set; }

    protected virtual void OnAttach() { }

    protected virtual void OnDetach() { }

    protected virtual void OnRecompose(ref Composition cx) { }
    public abstract void OnAttach(BoundaryData data, IBoundary boundary);
    public abstract void OnDetach(BoundaryData data, IBoundary boundary);
    public abstract void OnRecompose(ref Composition cx, BoundaryData data, IBoundary boundary);

    public void MarkDirty() => Node?.MarkDirty();
  }

  public abstract class BoundaryComposable<TData> : BoundaryComposable where TData : BoundaryData {
    public TData Data { get; protected set; }

    public override void OnAttach(BoundaryData data, IBoundary boundary) {
      if (data is not TData typedState) throw new InvalidOperationException();
      Data = typedState;
      Node = boundary as CompositionBoundaryNodeBase;
      if (Node == null) throw new InvalidOperationException();
      OnAttach();
    }

    public override void OnDetach(BoundaryData data, IBoundary boundary) {
      if (Node != null && Data != null) OnDetach();
      Data = null;
      Node = null;
    }

    public override void OnRecompose(ref Composition cx, BoundaryData data, IBoundary boundary) {
      if (Data == null || Node == null) return;
      OnRecompose(ref cx);
    }
  }

  public abstract class PropsBoundaryComposable<TProps> : BoundaryComposable<BoundaryData<TProps>>
    where TProps : struct {
    public ref TProps props { get => ref Data.props; }

    public virtual void ReceiveProps(in TProps props) {
      Data.props = props;
    }
  }

  public abstract class BoundaryData<T> : BoundaryData where T : struct {
    public T props;
  }

  public sealed class GenericPropsData<T> : BoundaryData<T> where T : struct { }

  public sealed class AnonymousBoundaryData : BoundaryData { }

  public abstract class BoundaryVisualElement : BoundaryElementBase {
    public abstract void Compose(ref Composition cx);
  }
}