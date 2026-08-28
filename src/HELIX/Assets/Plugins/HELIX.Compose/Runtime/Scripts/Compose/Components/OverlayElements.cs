using System;
using HELIX.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using static HELIX.Compose.OverlayPlacementResolver;

namespace HELIX.Compose {
  [EnableMixins]
  [BoundaryElementMixin(composable: false)]
  internal sealed partial class OverlayContentBoundary {
    private readonly Func<float> _resolveAnchorWidth;
    private OverlayEntry _entry;
    private uint _contentRevision;
    private float _anchorWidth = float.NaN;

    public OverlayContentBoundary(Func<float> resolveAnchorWidth) : this() {
      _resolveAnchorWidth = resolveAnchorWidth;
    }

    [Hook]
    private void OnInit() {
      this.MakeAbsolute().Tight();
      pickingMode = PickingMode.Position;
    }

    internal OverlayHandle Handle => _entry?.Handle;
    internal bool HasComposed { get; private set; }

    internal void Bind(OverlayEntry entry) {
      if (ReferenceEquals(_entry, entry) && _contentRevision == entry.ContentRevision) return;
      _anchorWidth = float.NaN;
      _entry = entry;
      _contentRevision = entry.ContentRevision;
      HasComposed = false;
      if (panel != null) HXComposer.MarkDirty(this, false);
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      if (_entry == null) return;
      var identity = unchecked((int)_entry.Id ^ (int)(_entry.Id >> 32));
      cx.AUTHORING.SetId(CompositionId.Generated(identity));
      var contextData = new OverlayContextData(_entry.Handle.Controller, _entry.Handle);
      using (cx.WriteContext(out var context)) {
        OverlayContextData.Key[in context] = contextData;
      }
      _entry.Content?.Invoke(ref cx, contextData);
      HasComposed = true;
      _entry.Options.constraints?.Apply(this);
      SynchronizeAnchorWidth();
    }

    internal bool SynchronizeAnchorWidth() {
      if (_entry == null) return false;
      var options = _entry.Options;
      if (!options.Has(OverlayBehavior.MatchAnchorWidth) || options.anchor?.panel == null) return false;
      var width = _resolveAnchorWidth?.Invoke() ?? float.NaN;
      if (!IsFinitePositive(width) || Mathf.Approximately(_anchorWidth, width)) return false;
      _anchorWidth = width;
      style.width = width;
      return true;
    }

    private static bool IsFinitePositive(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
  }

  internal sealed class OverlayEntryElement : VisualElement, IDisposable {
    private readonly OverlayContentBoundary _content;
    private readonly Action _requestPlacement;
    private IVisualElementScheduledItem _anchorPoll;
    private IVisualElementScheduledItem _timeout;
    private IVisualElementScheduledItem _focusRequest;
    private VisualElement _previousFocus;
    private bool _hasCapturedPreviousFocus;
    private OverlayEntry _entry;
    private bool _hasContentGeometry;
    private bool _positioned;
    private Vector2 _translation = new(float.NaN, float.NaN);

    public float PlacementHeight { get; private set; }

    public OverlayEntryElement(Action requestPlacement) {
      _requestPlacement = requestPlacement;
      this.Stretched();
      pickingMode = PickingMode.Ignore;
      tabIndex = -1;
      _content = new OverlayContentBoundary(ResolveAnchorWidth).AddTo(hierarchy);
      _content.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
      RegisterCallback<PointerDownEvent>(OnPointerDown);
      RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
      RegisterCallback<FocusOutEvent>(OnFocusOut);
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
    }

    public void Bind(OverlayEntry entry) {
      var changed = !ReferenceEquals(_entry, entry);
      _entry = entry;
      _content.Bind(entry);
      ConfigureContentLayout(entry.Options);
      if (changed && !_positioned) {
        _hasContentGeometry = false;
        _content.visible = !entry.Options.Has(OverlayBehavior.Prelayout);
      }
      var hasBarrier = entry.Options.Has(OverlayBehavior.Barrier);
      pickingMode = hasBarrier ? PickingMode.Position : PickingMode.Ignore;
      style.backgroundColor = hasBarrier ? entry.Options.barrierColor : Color.clear;
      focusable = entry.Options.Has(OverlayBehavior.CaptureFocus);
      _content.focusable = false;
      ConfigureSchedules(changed);
      _requestPlacement?.Invoke();
    }

    public bool ApplyPlacement(float stackOffset) {
      if (_entry == null || parent == null) return false;
      var options = _entry.Options;
      if (options.placement == OverlayPlacement.Fill) {
        PlacementHeight = FiniteOrZero(contentRect.height);
        SetTranslation(Vector2.zero);
        RevealPositionedContent();
        return true;
      }

      var prelayout = options.Has(OverlayBehavior.Prelayout);
      if (prelayout && !_hasContentGeometry) return false;
      if (prelayout && options.anchor is { panel: null }) return false;

      var hostRect = contentRect;
      var contentSize = prelayout
        ? new Vector2(FiniteOrZero(_content.layout.width), FiniteOrZero(_content.layout.height))
        : options.constraints?.ResolveSize(hostRect.size) ?? Vector2.zero;
      var width = contentSize.x;
      var height = contentSize.y;
      var margin = Mathf.Max(0f, options.margin);

      float x, y;
      if (TryResolveAnchorRect(out var anchor)) {
        if (options.Has(OverlayBehavior.MatchAnchorWidth)) width = anchor.width;
        ResolveAnchored(options.placement, anchor, width, height, out x, out y);
        if (options.Has(OverlayBehavior.FlipToFit)) {
          FlipToFit(options.placement, anchor, hostRect, width, height, ref x, ref y);
        }
      } else {
        ResolveViewport(options.placement, hostRect, width, height, margin, out x, out y);
      }

      x += options.offset.x;
      y += options.offset.y + StackDirection(options.placement) * stackOffset;
      if (options.Has(OverlayBehavior.ClampToHost)) {
        x = Mathf.Clamp(x, margin, Mathf.Max(margin, hostRect.width - width - margin));
        y = Mathf.Clamp(y, margin, Mathf.Max(margin, hostRect.height - height - margin));
      }
      SetTranslation(new Vector2(x, y));
      PlacementHeight = height;
      RevealPositionedContent();
      return true;
    }

    private float ResolveAnchorWidth() => TryResolveAnchorRect(out var anchor) ? anchor.width : float.NaN;

    private bool TryResolveAnchorRect(out Rect anchor) {
      anchor = default;
      var element = _entry?.Options.anchor;
      if (element?.panel == null || panel == null) return false;
      anchor = this.WorldToLocal(element.worldBound);
      return true;
    }

    public void Dispose() {
      _anchorPoll?.Pause();
      _timeout?.Pause();
      _focusRequest?.Pause();
      RestoreFocus();
      _content.RemoveFromHierarchy();
      _content.Dispose();
      _entry = null;
    }

    private void ConfigureSchedules(bool changed) {
      _anchorPoll?.Pause();
      _anchorPoll = null;
      _timeout?.Pause();
      _timeout = null;
      if (_entry == null) return;

      var options = _entry.Options;
      if (options.anchor != null && options.Has(OverlayBehavior.FollowAnchor)) {
        _anchorPoll = schedule.Execute(PollAnchor).Every(16);
      }
      if (options.timeoutMs > 0) {
        _timeout = schedule.Execute(() => { _entry?.Handle.Dismiss(OverlayDismissReason.Timeout); });
        _timeout.ExecuteLater(options.timeoutMs);
      }
      if (changed && panel != null) RequestFocus();
    }

    private void PollAnchor() {
      if (_entry == null) return;
      var options = _entry.Options;
      if (options.anchor == null) return;
      if (options.anchor.panel == null) {
        if (options.Has(OverlayBehavior.DismissWhenAnchorDetached)) {
          _entry.Handle.Dismiss(OverlayDismissReason.AnchorDetached);
        }
        return;
      }
      _content.SynchronizeAnchorWidth();
      _requestPlacement?.Invoke();
    }

    private void OnPointerDown(PointerDownEvent evt) {
      if (_entry == null || !_entry.Handle.Controller.IsTopOrAncestor(_entry)) return;
      if (!_entry.Options.Has(OverlayBehavior.Barrier | OverlayBehavior.DismissOnOutsidePointer) ||
          _content.worldBound.Contains(evt.position)) return;
      _entry.Handle.Dismiss(OverlayDismissReason.OutsidePointer);
      evt.StopImmediatePropagation();
    }

    private void OnNavigationCancel(NavigationCancelEvent evt) {
      if (!DismissFromCancel()) return;
      evt.StopImmediatePropagation();
    }

    private bool DismissFromCancel() {
      return _entry?.Handle.Controller.DismissFromCancel(_entry) == true;
    }

    private void OnFocusOut(FocusOutEvent evt) {
      if (_entry == null || !_entry.Options.Has(OverlayBehavior.CaptureFocus)) return;
      var next = evt.relatedTarget as VisualElement;
      if (IsInsideOverlayFamily(next)) return;
      if (next == null) {
        RequestFocus();
        return;
      }

      var controller = _entry.Handle.Controller;
      var scopeRoot = controller.FocusScopeRoot(_entry);
      if (IsInsideOverlayFamily(next, scopeRoot)) {
        if (_entry.Options.parent != null) _entry.Handle.Dismiss(OverlayDismissReason.FocusLost);
        return;
      }
      if (!scopeRoot.Options.Has(OverlayBehavior.DismissOnOutsidePointer)) {
        RequestFocus();
        return;
      }

      scopeRoot.Handle.Dismiss(OverlayDismissReason.FocusLost);
    }

    private bool IsInsideOverlayFamily(VisualElement element) => IsInsideOverlayFamily(element, _entry);

    private static bool IsInsideOverlayFamily(VisualElement element, OverlayEntry entry) {
      if (element == null || entry == null) return false;
      var targetOverlay = element as OverlayEntryElement ?? element.GetFirstAncestorOfType<OverlayEntryElement>();
      return targetOverlay?._entry != null &&
             entry.Handle.Controller.IsDescendantOf(targetOverlay._entry, entry);
    }

    private void OnAttachToPanel(AttachToPanelEvent evt) => RequestFocus();

    private void RequestFocus() {
      if (_entry == null || !_entry.Options.Has(OverlayBehavior.CaptureFocus) || panel == null) return;
      if (_entry.Options.Has(OverlayBehavior.Prelayout) && !_positioned) return;
      CapturePreviousFocus();
      _focusRequest?.Pause();
      _focusRequest = schedule.Execute(() => {
          _focusRequest = null;
          if (_entry == null || panel == null || !_entry.Handle.IsOpen) return;
          if (!ReferenceEquals(_entry.Handle.Controller.Top, _entry)) return;
          var focused = panel.focusController.focusedElement as VisualElement;
          if (IsInsideOverlayFamily(focused)) return;
          Focus();
        }
      );
      _focusRequest.ExecuteLater(1);
    }

    private void CapturePreviousFocus() {
      if (!_hasCapturedPreviousFocus) {
        _previousFocus = panel.focusController.focusedElement as VisualElement;
        _hasCapturedPreviousFocus = true;
      }
    }

    private void RestoreFocus() {
      _focusRequest?.Pause();
      _focusRequest = null;
      if (_entry != null && _entry.Options.Has(OverlayBehavior.RestoreFocus) &&
          _entry.DismissReason != OverlayDismissReason.FocusLost &&
          _previousFocus?.panel != null) _previousFocus.Focus();
      ForgetPreviousFocus();
    }

    private void ForgetPreviousFocus() {
      _previousFocus = null;
      _hasCapturedPreviousFocus = false;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) {
      if (_entry == null || !_entry.Options.Has(OverlayBehavior.Prelayout) || !_content.HasComposed) return;
      _hasContentGeometry = true;
      _requestPlacement?.Invoke();
    }

    private void ConfigureContentLayout(in OverlayOptions options) {
      _translation = new Vector2(float.NaN, float.NaN);
      _content.style.translate = new Translate();
      if (options.placement == OverlayPlacement.Fill) {
        _content.Stretched();
        return;
      }

      _content.MakeAbsolute();
      _content.style.left = 0f;
      _content.style.top = 0f;
      _content.style.right = StyleKeyword.Auto;
      _content.style.bottom = StyleKeyword.Auto;
    }

    private void SetTranslation(Vector2 translation) {
      if (Approximately(_translation, translation)) return;
      _translation = translation;
      _content.style.translate = new Translate(translation.x, translation.y);
    }

    private static bool Approximately(Vector2 a, Vector2 b) =>
      Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);

    private void RevealPositionedContent() {
      if (_positioned) return;
      _positioned = true;
      _content.visible = true;
      RequestFocus();
    }
  }
}
