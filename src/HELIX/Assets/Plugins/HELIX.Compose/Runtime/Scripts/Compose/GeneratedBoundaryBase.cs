using HELIX.Compose.Collections;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public abstract class GeneratedBoundaryBase : VisualElement, IBoundary {
    public VisualElement Element => this;
    public BoundaryCell Cell { get; } = BoundaryCell.Shared;
    public int TreeDepth { get; protected set; }
    public IBoundary Parent { get; protected set; }
    public IContextComposable ContextParent { get; protected set; }
    public bool IsDisposed { get; protected set; }
    public UssFlag Flag { get; set; }
    public ulong PackedId { get; set; }
    public SparseContextMap WrittenContext { get; set; }

    private LookupCache _lookupCache;
    private bool _initialAttachment = true;
    private bool _initialized;
    private readonly bool _cacheLookups;
    private readonly bool _trimChildren;

    protected GeneratedBoundaryBase(bool cacheLookups, bool trimChildren) {
      _cacheLookups = cacheLookups;
      _trimChildren = trimChildren;
      RegisterCallback<AttachToPanelEvent>(_ => AttachBoundary());
      RegisterCallback<DetachFromPanelEvent>(_ => DetachBoundary());
      ((IBoundaryHooks)this).MixinPostConstruct();
      if (HXComposer.IsProcessing) {
        _initialAttachment = false;
        HXComposer.MarkDirty(this);
      }
    }

    public void Recompose() {
      try {
        BoundaryHelper.ContextBefore(this);
        _lookupCache.Clear();
        if (!_initialized) Init();
        try {
          var cx = new Composition(this);
          if (_cacheLookups) UseLookupCache();
          ((IBoundaryHooks)this).MixinRecompose(ref cx);
        } catch (System.Exception e) { UnityEngine.Debug.LogException(e); }
        BoundaryHelper.ContextAfter(this);
        if (_trimChildren) Cell.TrimChildren();
      } catch (System.Exception e) { UnityEngine.Debug.LogException(e); }
    }

    public void Reset() {
      HXComposer.RemoveDirty(this);
      ((IBoundaryHooks)this).MixinReset();
      _lookupCache.Release();
      _initialized = false;
    }

    private void Init() {
      ((IBoundaryHooks)this).MixinInit();
      _initialized = true;
    }

    public void Dispose() {
      if (IsDisposed) return;
      IsDisposed = true;
      _initialized = false;
      ((IBoundaryHooks)this).MixinDispose();
      Reset();
    }

    private void DetachBoundary() {
      _initialAttachment = true;
      _lookupCache.Release();
      HXComposer.NotifyDetach(this);
    }

    private void AttachBoundary() {
      IsDisposed = false;
      RefreshHierarchy();
      HXComposer.RegisterActiveBoundary(this);
      if (_initialAttachment) {
        _initialAttachment = false;
        HXComposer.MarkDirty(this);
      }
    }

    public void UseLookupCache() => _lookupCache.Claim();

    public bool TryLookupContext(int key, out ContextData data) {
      data = null;
      if (WrittenContext != null && WrittenContext.TryGet(key, out data)) return true;
      return _lookupCache.TryLookup(ContextParent, key, out data);
    }

    public void RefreshHierarchy() {
      Parent = GetFirstAncestorOfType<IBoundary>();
      ContextParent = GetFirstAncestorOfType<IContextComposable>();
    }

    public void MarkDirty() => HXComposer.MarkDirty(this, false);
  }
}