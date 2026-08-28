using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [InternalApi]
  public abstract class GeneratedBoundaryBase : VisualElement, IBoundary {
    private readonly bool _cacheLookups;
    private readonly bool _trimChildren;
    private bool _initialAttachment = true;
    private bool _initialized;

    private LookupCache _lookupCache;

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

    public VisualElement Element => this;
    public BoundaryCell Cell { get; } = BoundaryCell.Shared;
    public int TreeDepth { get; protected set; }
    public IBoundary Parent { get; protected set; }
    public IContextComposable ContextParent { get; protected set; }
    public bool IsDisposed { get; protected set; }
    public UssFlag Flag { get; set; }
    public ulong PackedId { get; set; }
    public SparseContextMap WrittenContext { get; set; }

    public void Recompose() {
      try {
        BoundaryHelper.ContextBefore(this);
        _lookupCache.Clear();
        if (!_initialized) Init();
        try {
          var cx = new Composition(this);
          if (_cacheLookups) UseLookupCache();
          ((IBoundaryHooks)this).MixinRecompose(ref cx);
        } catch (Exception e) { Debug.LogException(e); }
        BoundaryHelper.ContextAfter(this);
        if (_trimChildren) Cell.TrimChildren();
      } catch (Exception e) { Debug.LogException(e); }
    }

    public void Reset() {
      HXComposer.RemoveDirty(this);
      ((IBoundaryHooks)this).MixinReset();
      _lookupCache.Release();
      _initialized = false;
    }

    public void Dispose() {
      if (IsDisposed) return;
      IsDisposed = true;
      _initialized = false;
      ((IBoundaryHooks)this).MixinDispose();
      Reset();
    }

    public bool TryLookupContext(int key, out ContextData data) {
      data = null;
      if (WrittenContext != null && WrittenContext.TryGet(key, out data)) return true;
      return _lookupCache.TryLookup(ContextParent, key, out data);
    }

    public void RefreshHierarchy() {
      Parent = GetFirstAncestorOfType<IBoundary>();
      ContextParent = GetFirstAncestorOfType<IContextComposable>();
      TreeDepth = BoundaryHelper.GetDepth(this);
    }

    public void MarkDirty() {
      HXComposer.MarkDirty(this, false);
    }

    private void Init() {
      ((IBoundaryHooks)this).MixinInit();
      _initialized = true;
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

    public void UseLookupCache() {
      _lookupCache.Claim();
    }
  }
}