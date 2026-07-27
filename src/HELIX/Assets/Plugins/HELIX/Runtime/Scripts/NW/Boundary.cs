using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HELIX.Extensions;
using HELIX.NW.Forms;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.NW {
  public interface IBoundary : IComposable {
    IBoundary Parent { get; }
    BoundaryCell Cell { get; }

    int TreeDepth { get; }

    SparseContextMap AcquireContext();
    void ContributeContext(Dictionary<int, ContextData> context);

    void RefreshHierarchy();
    void CheckModified();

    void Recompose();
  }

  public static class BoundaryExtensions {
    public static T LookupData<T>(this IBoundary boundary, bool includeHost = true) {
      var current = includeHost ? boundary : boundary.Parent;
      while (current != null) {
        if (current is CompositionBoundaryNodeBase { Data: T data }) return data;
        current = current.Parent;
      }
      return default;
    }

    public static T LookupComposable<T>(this IBoundary boundary, bool includeHost = true) {
      var current = includeHost ? boundary : boundary.Parent;
      while (current != null) {
        if (current is CompositionBoundaryNodeBase { BoundaryComposable: T attachment }) return attachment;
        current = current.Parent;
      }
      return default;
    }

    public static T LookupBoundary<T>(this IBoundary boundary, bool includeHost = true) {
      var current = includeHost ? boundary : boundary.Parent;
      while (current != null) {
        if (current is T typed) return typed;
        current = current.Parent;
      }
      return default;
    }

  }

  public class BoundaryCell {
    public static readonly BoundaryCell Shared = new();

    private static readonly ProfilerCounterValue<int> _hierarchyDeletions = new(
      HelixProfiling.HelixCategory,
      "Hierarchy Deletions",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );

    public IComposable current;
    public int cursor;
    public LocalId localId;
    public HxSlot slot;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VisualElement ReadCursor() {
      var element = current.Element;
      return element.childCount <= cursor ? null : element.ElementAt(cursor);
    }

    public VisualElement ReadCursorOrFind(ulong typeId) {
      // Get the current cursor or finds the element after the current curser if it already exists
      var element = ReadCursor();
      if (element == null) return null;
      if (GetTypeId(element) == typeId) return element;
      for (var i = cursor + 1; i < current.Element.childCount; i++) {
        var child = current.Element.ElementAt(i);
        if (GetTypeId(child) == typeId) return child;
      }

      return element;
    }

    public ulong GetTypeId(VisualElement element) {
      if (element is IComposable composable) {
        return composable.TypeId;
      } else {
        if (element.userData is UserdataTracker tracker) return tracker.TypeId;
        return 0;
      }
    }

    public void TrimChildren() {
      var element = current.Element;
      var overflow = element.childCount - cursor;
      for (var i = 0; i < overflow; i++) {
        //Debug.Log($"Removing child {i} from {element.name}");
        element.RemoveAt(element.childCount - 1);
      }
      _hierarchyDeletions.Value += overflow;
      //if (overflow > 0) Debug.LogWarning($"Removed {overflow} children");
    }
  }

  public abstract class BoundaryElementBase : VisualElement, IBoundary {
    public VisualElement Element => this;
    public SparseContextMap Context { get; protected set; }
    public int TreeDepth { get; protected set; }
    public BoundaryCell Cell { get; } = BoundaryCell.Shared;

    public IBoundary Parent { get; protected set; }
    public UssFlag Flag { get; set; }
    public ulong TypeId { get; set; }
    private bool _initialAttachment = true;

    protected BoundaryElementBase() {
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
      RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
      RecompositionScope.RegisterBoundary(this);
      //name = $"Boundary{this.ShortHash()}";
      //generateVisualContent += GenerateDebugVisuals;

      if (RecompositionScope.IsProcessing) {
        _initialAttachment = false;
        RecompositionScope.MarkDirty(this);
      }
    }

    public void RefreshHierarchy() {
      Parent = GetFirstAncestorOfType<IBoundary>();
    }

    public void CheckModified() {
      if (Context != null && Context.CheckSubscriptionsModified()) {
        RecompositionScope.EnqueueDirty(this);
      }
    }

    public SparseContextMap AcquireContext() {
      Context ??= SparseContextMap.Get();
      return Context;
    }

    public void ContributeContext(Dictionary<int, ContextData> context) {
      Context?.LoadInto(context);
    }

    public virtual void Reset() {
      RecompositionScope.RemoveDirty(this);
    }

    protected virtual void BeforeCompose() {
      Context?.ResetMarkers();
    }

    protected virtual void AfterCompose() {
      if (Context == null) return;
      Context.Prune();
      if (!Context.IsUnused) return;
      SparseContextMap.Release(Context);
      Context = null;
    }

    public virtual void Recompose() {
      try {
        //rebuildCount++;
        RecompositionScope.RemoveDirty(this);
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
        RecompositionScope.RemoveDirty(this);
        //MarkDirtyRepaint();
      }
    }
    public abstract void PerformCompose(ref Composition cx);

    protected virtual void OnAttachToPanel(AttachToPanelEvent evt) {
      RecompositionScope.RegisterBoundary(this);
      TreeDepth = this.GetDepth();
      RefreshHierarchy();

      if (_initialAttachment) {
        _initialAttachment = false;
        //Debug.Log("Marking dirty");
        RecompositionScope.MarkDirty(this);
      }
    }

    protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt) {
      _initialAttachment = true;
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

    public void SetData(BoundaryData data) {
      if (Data != null) DisposeState();
      Data = data;
    }

    public void SetData(BoundaryData data, IBoundaryComposable boundaryComposable) {
      if (Data != null) DisposeState();
      Data = data;
      BoundaryComposable = boundaryComposable;
      boundaryComposable.OnAttach(data, this);
    }

    public void SwapState(IBoundaryComposable boundaryComposable) {
      if (Data == null) throw new InvalidOperationException("Cannot swap attachment on boundary with no state");
      BoundaryComposable?.OnDetach(Data, this);
      BoundaryComposable = boundaryComposable;
      boundaryComposable.OnAttach(Data, this);
    }

    public override void Reset() {
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

      Context?.Clear();
      if (Context != null) SparseContextMap.Release(Context);
      Context = null;

      RecompositionScope.UnregisterBoundary(this);
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

  public interface IBoundaryComposable {
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

    public void MarkDirty() => Node?.MarkDirty();
  }

  public abstract class PropsBoundaryComposable<TProps> : BoundaryComposable<BoundaryData<TProps>>
    where TProps : struct {
    public TProps Props {
      get => Data.props;
      set => Data.props = value;
    }

    public virtual void ReceiveProps(TProps props) {
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

  [UxmlElement]
  public partial class CustomVisualElement : BoundaryVisualElement {
    public override void Compose(ref Composition cx) {

    }
  }

  // public partial class CustomVisualElement {
  //   private static readonly CompositionId _compositionId = new() {
  //     composition = CompositionId.GetCompositionId(),
  //     type = CompositionId.GetTypeId(),
  //     local = LocalId.Initial
  //   };
  //
  //   public override void PerformCompose(ref Composition cx) {
  //     cx.AUTHORING.SetId(_compositionId);
  //     Compose(ref cx);
  //   }
  // }
}