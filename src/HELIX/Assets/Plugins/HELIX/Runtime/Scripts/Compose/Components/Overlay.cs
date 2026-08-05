using System;
using System.Collections.Generic;
using HELIX.Extensions;
using HELIX.Signals;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public enum OverlayDismissReason : byte {
    Manual, OutsidePointer, Cancel, Timeout, AnchorDetached, ControllerDisposed, Action, Replaced, FocusLost
  }

  public enum OverlayPlacement : byte {
    Fill,
    Center,
    TopStart,
    Top,
    TopEnd,
    BottomStart,
    Bottom,
    BottomEnd,
    Start,
    End,
    AboveStart,
    Above,
    AboveEnd,
    BelowStart,
    Below,
    BelowEnd,
    Before,
    After
  }

  [PropStruct] public readonly partial struct OverlayOptions {
    // Force the generated constructor to run; an empty struct initializer zeroes every option.
    public static readonly OverlayOptions Default = new(placement: OverlayPlacement.Center);

    [Prop(OverlayPlacement.Center)] public readonly OverlayPlacement placement;
    [Prop(null)] public readonly OverlayHandle parent;
    [Prop(null)] public readonly VisualElement anchor;
    [Prop("default", PropInit.Constant)] public readonly Vector2 offset;
    [Prop(12f)] public readonly float margin;
    [Prop(true)] public readonly bool flipToFit;
    [Prop(true)] public readonly bool clampToHost;
    [Prop(true)] public readonly bool followAnchor;
    [Prop(false)] public readonly bool matchAnchorWidth;
    [Prop(true)] public readonly bool dismissWhenAnchorDetached;
    [Prop(false)] public readonly bool barrier;
    [Prop("new UnityEngine.Color(0f, 0f, 0f, 0.45f)", PropInit.Deferred)]
    public readonly Color barrierColor;
    [Prop(false)] public readonly bool dismissOnOutsidePointer;
    [Prop(true)] public readonly bool dismissOnCancel;
    [Prop(false)] public readonly bool captureFocus;
    [Prop(true)] public readonly bool restoreFocus;
    [Prop(true)] public readonly bool prelayout;
    [Prop(null)] public readonly BoxConstraints? constraints;
    [Prop(false)] public readonly bool stacked;
    [Prop(8f)] public readonly float stackSpacing;
    [Prop(0)] public readonly int timeoutMs;
    [Prop(null)] public readonly CompositionAction<OverlayDismissReason> onDismissed;
  }

  public readonly struct OverlayContextData : IEquatable<OverlayContextData> {
    public static readonly ContextKey<OverlayContextData> Key = new("OverlayContext");

    public readonly OverlayController controller;
    public readonly OverlayHandle handle;

    internal OverlayContextData(OverlayController controller, OverlayHandle handle) {
      this.controller = controller;
      this.handle = handle;
    }

    public bool isOpen => handle?.IsOpen ?? false;
    public bool Dismiss(OverlayDismissReason reason = OverlayDismissReason.Manual) => handle?.Dismiss(reason) ?? false;

    public bool Equals(OverlayContextData other) =>
      ReferenceEquals(controller, other.controller) && ReferenceEquals(handle, other.handle);

    public override bool Equals(object obj) => obj is OverlayContextData other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(controller, handle);
  }

  public sealed class OverlayEntry {
    internal OverlayEntry(
      long id,
      Composable<OverlayContextData> content,
      OverlayOptions options,
      OverlayHandle handle,
      IBoundary callbackBoundary
    ) {
      Id = id;
      Content = content;
      Options = options;
      Handle = handle;
      CallbackBoundary = callbackBoundary;
    }

    public long Id { get; }
    public Composable<OverlayContextData> Content { get; }
    public OverlayOptions Options { get; }
    public OverlayHandle Handle { get; }
    internal IBoundary CallbackBoundary { get; }
    internal OverlayDismissReason? DismissReason { get; set; }
  }

  public sealed class OverlayHandle {
    internal OverlayHandle(OverlayController controller, long id) {
      Controller = controller;
      Id = id;
    }

    public OverlayController Controller { get; }
    public long Id { get; }
    public bool IsOpen => Controller?.Contains(Id) ?? false;

    public bool Dismiss(OverlayDismissReason reason = OverlayDismissReason.Manual) =>
      Controller?.Dismiss(Id, reason) ?? false;

    public bool Replace(Composable<OverlayContextData> content) => Controller?.Replace(Id, content) ?? false;
  }

  public sealed class OverlayBuilder {
    private readonly Composable<OverlayContextData> _content;
    private OverlayPlacement _placement;
    private OverlayHandle _parent;
    private VisualElement _anchor;
    private Vector2 _offset;
    private float _margin;
    private bool _flipToFit;
    private bool _clampToHost;
    private bool _followAnchor;
    private bool _matchAnchorWidth;
    private bool _dismissWhenAnchorDetached;
    private bool _barrier;
    private Color _barrierColor;
    private bool _dismissOnOutsidePointer;
    private bool _dismissOnCancel;
    private bool _captureFocus;
    private bool _restoreFocus;
    private bool _prelayout;
    private BoxConstraints? _constraints;
    private bool _stacked;
    private float _stackSpacing;
    private int _timeoutMs;
    private CompositionAction<OverlayDismissReason> _onDismissed;

    internal OverlayBuilder(Composable<OverlayContextData> content)
      : this(content, OverlayOptions.Default) { }

    internal OverlayBuilder(Composable<OverlayContextData> content, in OverlayOptions options) {
      _content = content ?? throw new ArgumentNullException(nameof(content));
      _placement = options.placement;
      _parent = options.parent;
      _anchor = options.anchor;
      _offset = options.offset;
      _margin = options.margin;
      _flipToFit = options.flipToFit;
      _clampToHost = options.clampToHost;
      _followAnchor = options.followAnchor;
      _matchAnchorWidth = options.matchAnchorWidth;
      _dismissWhenAnchorDetached = options.dismissWhenAnchorDetached;
      _barrier = options.barrier;
      _barrierColor = options.barrierColor;
      _dismissOnOutsidePointer = options.dismissOnOutsidePointer;
      _dismissOnCancel = options.dismissOnCancel;
      _captureFocus = options.captureFocus;
      _restoreFocus = options.restoreFocus;
      _prelayout = options.prelayout;
      _constraints = options.constraints;
      _stacked = options.stacked;
      _stackSpacing = options.stackSpacing;
      _timeoutMs = options.timeoutMs;
      _onDismissed = options.onDismissed;
    }

    public OverlayBuilder Modal(
      bool dismissOnOutsidePointer = false,
      Color? barrierColor = null
    ) {
      _barrier = true;
      _captureFocus = true;
      _dismissOnOutsidePointer = dismissOnOutsidePointer;
      if (barrierColor.HasValue) _barrierColor = barrierColor.Value;
      return this;
    }

    public OverlayBuilder At(OverlayPlacement placement, Vector2 offset = default) {
      _placement = placement;
      _anchor = null;
      _offset = offset;
      return this;
    }

    public OverlayBuilder AnchorTo(
      VisualElement anchor,
      OverlayPlacement placement = OverlayPlacement.BelowStart,
      Vector2 offset = default
    ) {
      _anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
      _placement = placement;
      _offset = offset;
      return this;
    }

    /// <summary>
    /// Associates this entry with a parent overlay. Outside-pointer dismissal on the parent remains
    /// active while this entry is topmost, which is useful for nested popovers and menus.
    /// </summary>
    public OverlayBuilder Parent(OverlayHandle parent) {
      _parent = parent;
      return this;
    }

    public OverlayBuilder FollowAnchor(bool enabled = true) {
      _followAnchor = enabled;
      return this;
    }

    public OverlayBuilder MatchAnchorWidth(bool enabled = true) {
      _matchAnchorWidth = enabled;
      return this;
    }

    public OverlayBuilder FlipToFit(bool enabled = true) {
      _flipToFit = enabled;
      return this;
    }

    public OverlayBuilder ClampToHost(bool enabled = true, float margin = 12f) {
      _clampToHost = enabled;
      _margin = Mathf.Max(0f, margin);
      return this;
    }

    public OverlayBuilder DismissOnOutsidePointer(bool enabled = true) {
      _dismissOnOutsidePointer = enabled;
      return this;
    }

    public OverlayBuilder DismissOnCancel(bool enabled = true) {
      _dismissOnCancel = enabled;
      return this;
    }

    public OverlayBuilder DismissWhenAnchorDetaches(bool enabled = true) {
      _dismissWhenAnchorDetached = enabled;
      return this;
    }

    public OverlayBuilder CaptureFocus(bool enabled = true, bool restore = true) {
      _captureFocus = enabled;
      _restoreFocus = restore;
      return this;
    }

    /// <summary>
    /// Measures the composed surface while hidden before placing it. When disabled, content measurements
    /// never request placement updates and positioning uses the supplied constraints instead.
    /// </summary>
    public OverlayBuilder Prelayout(bool enabled = true) {
      _prelayout = enabled;
      return this;
    }

    /// <summary>
    /// Constrains the overlay surface and provides its expected size to placement without a prelayout pass.
    /// </summary>
    public OverlayBuilder Constraints(in BoxConstraints constraints) {
      _constraints = constraints;
      return this;
    }

    public OverlayBuilder Stacked(bool enabled = true, float spacing = 8f) {
      _stacked = enabled;
      _stackSpacing = Mathf.Max(0f, spacing);
      return this;
    }

    public OverlayBuilder Timeout(int milliseconds) {
      _timeoutMs = Mathf.Max(0, milliseconds);
      return this;
    }

    public OverlayBuilder OnDismissed(CompositionAction<OverlayDismissReason> callback) {
      _onDismissed = callback;
      return this;
    }

    public OverlayHandle Show(CompositionContext context) => Show(context.Overlays(), context.boundary);

    public OverlayHandle Show(OverlayController controller, IBoundary callbackBoundary = null) {
      if (controller == null) throw new ArgumentNullException(nameof(controller));
      if (_parent != null && !ReferenceEquals(_parent.Controller, controller)) {
        throw new ArgumentException("A child overlay must use the same controller as its parent.", nameof(controller));
      }
      if (_onDismissed != null && callbackBoundary == null) {
        throw new InvalidOperationException(
          "An overlay dismissal action requires a composition context. Use Show(context) or provide its boundary."
        );
      }
      return controller.Show(_content, BuildOptions(), callbackBoundary);
    }

    private OverlayOptions BuildOptions() => new(
      placement: _placement,
      parent: _parent,
      anchor: _anchor,
      offset: _offset,
      margin: _margin,
      flipToFit: _flipToFit,
      clampToHost: _clampToHost,
      followAnchor: _followAnchor,
      matchAnchorWidth: _matchAnchorWidth,
      dismissWhenAnchorDetached: _dismissWhenAnchorDetached,
      barrier: _barrier,
      barrierColor: _barrierColor,
      dismissOnOutsidePointer: _dismissOnOutsidePointer,
      dismissOnCancel: _dismissOnCancel,
      captureFocus: _captureFocus,
      restoreFocus: _restoreFocus,
      prelayout: _prelayout,
      constraints: _constraints,
      stacked: _stacked,
      stackSpacing: _stackSpacing,
      timeoutMs: _timeoutMs,
      onDismissed: _onDismissed
    );
  }

  public static class Overlay {
    public static OverlayBuilder Build(Composable<OverlayContextData> content) => new(content);

    public static OverlayBuilder Build(
      Composable<OverlayContextData> content,
      in OverlayOptions options
    ) => new(content, in options);
  }

  public sealed class OverlayController : Signal<int> {
    private readonly List<OverlayEntry> _entries = new();
    private int _revision;
    private long _nextId = 1;

    public OverlayController() : base("OverlayController", typeof(OverlayController)) { }

    public IReadOnlyList<OverlayEntry> Entries => _entries;
    public OverlayEntry Top => _entries.Count == 0 ? null : _entries[^1];

    public override int PeekValue() => _revision;

    public override void SetValue(int newValue) =>
      throw new NotSupportedException("Overlay state is changed with overlay operations.");

    public override void SetWithoutNotify(int newValue) =>
      throw new NotSupportedException("Overlay state is changed with overlay operations.");

    public OverlayHandle Show(
      Composable<OverlayContextData> content,
      OverlayOptions? options = null,
      IBoundary callbackBoundary = null
    ) {
      if (content == null) throw new ArgumentNullException(nameof(content));
      var id = _nextId++;
      var handle = new OverlayHandle(this, id);
      var resolvedOptions = options ?? OverlayOptions.Default;
      if (resolvedOptions.onDismissed != null && callbackBoundary == null) {
        throw new ArgumentNullException(
          nameof(callbackBoundary), "A dismissal action requires the boundary that owns its composition context."
        );
      }
      _entries.Add(new OverlayEntry(id, content, resolvedOptions, handle, callbackBoundary));
      NotifyChanged();
      return handle;
    }

    public OverlayHandle Show(OverlayBuilder builder, IBoundary callbackBoundary = null) {
      if (builder == null) throw new ArgumentNullException(nameof(builder));
      return builder.Show(this, callbackBoundary);
    }

    public bool Contains(long id) => FindIndex(id) >= 0;

    internal bool IsTopOrAncestor(OverlayEntry entry) {
      var current = Top;
      while (current != null) {
        if (ReferenceEquals(current, entry)) return true;
        current = Find(current.Options.parent?.Id ?? 0);
      }
      return false;
    }

    internal bool IsDescendantOf(OverlayEntry entry, OverlayEntry ancestor) {
      var current = entry;
      while (current != null) {
        if (ReferenceEquals(current, ancestor)) return true;
        current = Find(current.Options.parent?.Id ?? 0);
      }
      return false;
    }

    internal OverlayEntry FocusScopeRoot(OverlayEntry entry) {
      var root = entry;
      var current = entry;
      while (current != null && current.Options.parent != null) {
        current = Find(current.Options.parent.Id);
        if (current != null && current.Options.captureFocus) root = current;
      }
      return root;
    }

    public bool Replace(long id, Composable<OverlayContextData> content) {
      if (content == null) throw new ArgumentNullException(nameof(content));
      var index = FindIndex(id);
      if (index < 0) return false;
      var current = _entries[index];
      _entries[index] = new OverlayEntry(
        current.Id, content, current.Options, current.Handle, current.CallbackBoundary
      );
      NotifyChanged();
      return true;
    }

    public bool Dismiss(long id, OverlayDismissReason reason = OverlayDismissReason.Manual) {
      var index = FindIndex(id);
      if (index < 0) return false;
      var entry = _entries[index];
      _entries.RemoveAt(index);
      entry.DismissReason = reason;
      entry.Options.onDismissed?.Call(entry.CallbackBoundary, reason);
      NotifyChanged();
      return true;
    }

    public bool DismissTop(OverlayDismissReason reason = OverlayDismissReason.Manual) =>
      Top != null && Dismiss(Top.Id, reason);

    public void DismissAll(OverlayDismissReason reason = OverlayDismissReason.Manual) {
      if (_entries.Count == 0) return;
      var removed = _entries.ToArray();
      _entries.Clear();
      foreach (var entry in removed) entry.Options.onDismissed?.Call(entry.CallbackBoundary, reason);
      NotifyChanged();
    }

    public override void Dispose() {
      if (_entries.Count > 0) {
        var removed = _entries.ToArray();
        _entries.Clear();
        foreach (var entry in removed) {
          entry.Options.onDismissed?.Call(entry.CallbackBoundary, OverlayDismissReason.ControllerDisposed);
        }
      }
      base.Dispose();
    }

    private int FindIndex(long id) {
      for (var i = 0; i < _entries.Count; i++) {
        if (_entries[i].Id == id) return i;
      }
      return -1;
    }

    private OverlayEntry Find(long id) {
      var index = id == 0 ? -1 : FindIndex(id);
      return index < 0 ? null : _entries[index];
    }

    private void NotifyChanged() {
      unchecked { _revision++; }
      NotifyDirty();
      NotifyObservers();
    }
  }

  internal sealed class OverlayContentBoundary : BoundaryVisualElement {
    private readonly Func<float> _resolveAnchorWidth;
    private OverlayEntry _entry;
    private float _anchorWidth = float.NaN;

    public OverlayContentBoundary(Func<float> resolveAnchorWidth) {
      _resolveAnchorWidth = resolveAnchorWidth;
      this.MakeAbsolute().Tight();
      pickingMode = PickingMode.Position;
    }

    internal OverlayHandle handle => _entry?.Handle;
    internal bool hasComposed { get; private set; }

    internal void Bind(OverlayEntry entry) {
      if (ReferenceEquals(_entry, entry)) return;
      _anchorWidth = float.NaN;
      _entry = entry;
      hasComposed = false;
      if (panel != null) HXComposer.MarkDirty(this, false);
    }

    public override void PerformCompose(ref Composition cx) => Compose(ref cx);

    public override void Compose(ref Composition cx) {
      if (_entry == null) return;
      var identity = unchecked((int)_entry.Id ^ (int)(_entry.Id >> 32));
      cx.AUTHORING.SetId(CompositionId.Generated(identity));
      var contextData = new OverlayContextData(_entry.Handle.Controller, _entry.Handle);
      using (cx.WriteContext(out var context)) {
        OverlayContextData.Key[in context] = contextData;
      }
      _entry.Content?.Invoke(ref cx, contextData);
      hasComposed = true;
      _entry.Options.constraints?.Apply(this);
      SynchronizeAnchorWidth();
    }

    internal bool SynchronizeAnchorWidth() {
      if (_entry == null) return false;
      var options = _entry.Options;
      if (!options.matchAnchorWidth || options.anchor?.panel == null) return false;
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
    private IVisualElementScheduledItem _focusRecovery;
    private VisualElement _previousFocus;
    private bool _hasCapturedPreviousFocus;
    private OverlayEntry _entry;
    private bool _hasContentGeometry;
    private bool _positioned;
    private float _placementHeight;
    private Vector2 _translation = new(float.NaN, float.NaN);

    public OverlayEntryElement(Action requestPlacement) {
      _requestPlacement = requestPlacement;
      this.Stretched();
      pickingMode = PickingMode.Ignore;
      tabIndex = -1;
      _content = new OverlayContentBoundary(ResolveAnchorWidth).AddTo(hierarchy);
      _content.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
      RegisterCallback<PointerDownEvent>(OnPointerDown);
      RegisterCallback<KeyDownEvent>(OnKeyDown);
      RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
      RegisterCallback<FocusOutEvent>(OnFocusOut);
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
    }

    public float PlacementHeight => _placementHeight;

    public void Bind(OverlayEntry entry) {
      var changed = !ReferenceEquals(_entry, entry);
      _entry = entry;
      _content.Bind(entry);
      ConfigureContentLayout(entry.Options);
      if (changed && !_positioned) {
        _hasContentGeometry = false;
        _content.visible = !entry.Options.prelayout;
      }
      pickingMode = entry.Options.barrier ? PickingMode.Position : PickingMode.Ignore;
      style.backgroundColor = entry.Options.barrier ? entry.Options.barrierColor : Color.clear;
      focusable = entry.Options.captureFocus;
      _content.focusable = false;
      ConfigureSchedules(changed);
      _requestPlacement?.Invoke();
    }

    public bool ApplyPlacement(float stackOffset) {
      if (_entry == null || parent == null) return false;
      var options = _entry.Options;
      if (options.placement == OverlayPlacement.Fill) {
        _placementHeight = FiniteOrZero(contentRect.height);
        SetTranslation(Vector2.zero);
        RevealPositionedContent();
        return true;
      }

      if (options.prelayout && !_hasContentGeometry) return false;
      if (options.prelayout && options.anchor != null && options.anchor.panel == null) return false;

      var hostRect = contentRect;
      var contentSize = options.prelayout
        ? new Vector2(FiniteOrZero(_content.layout.width), FiniteOrZero(_content.layout.height))
        : ResolveConstraintSize(options.constraints, hostRect.size);
      var width = contentSize.x;
      var height = contentSize.y;
      var margin = Mathf.Max(0f, options.margin);
      var x = margin;
      var y = margin;

      if (TryResolveAnchorRect(out var anchor)) {
        if (options.matchAnchorWidth) width = anchor.width;
        ResolveAnchored(options.placement, anchor, width, height, out x, out y);
        if (options.flipToFit) FlipToFit(options.placement, anchor, hostRect, width, height, ref x, ref y);
      } else {
        ResolveViewport(options.placement, hostRect, width, height, margin, out x, out y);
      }

      x += options.offset.x;
      y += options.offset.y + StackDirection(options.placement) * stackOffset;
      if (options.clampToHost) {
        x = Mathf.Clamp(x, margin, Mathf.Max(margin, hostRect.width - width - margin));
        y = Mathf.Clamp(y, margin, Mathf.Max(margin, hostRect.height - height - margin));
      }
      SetTranslation(new Vector2(x, y));
      _placementHeight = height;
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
      _focusRecovery?.Pause();
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
      if (options.anchor != null && options.followAnchor) {
        _anchorPoll = schedule.Execute(PollAnchor).Every(16);
      }
      if (options.timeoutMs > 0) {
        _timeout = schedule.Execute(() => {
            _entry?.Handle.Dismiss(OverlayDismissReason.Timeout);
          }
        );
        _timeout.ExecuteLater(options.timeoutMs);
      }
      if (changed && panel != null) CaptureFocus();
    }

    private void PollAnchor() {
      if (_entry == null) return;
      var options = _entry.Options;
      if (options.anchor == null) return;
      if (options.anchor.panel == null) {
        if (options.dismissWhenAnchorDetached) {
          _entry.Handle.Dismiss(OverlayDismissReason.AnchorDetached);
        }
        return;
      }
      _content.SynchronizeAnchorWidth();
      _requestPlacement?.Invoke();
    }

    private void OnPointerDown(PointerDownEvent evt) {
      if (_entry == null || !_entry.Handle.Controller.IsTopOrAncestor(_entry)) return;
      if (!_entry.Options.barrier ||
          !_entry.Options.dismissOnOutsidePointer ||
          _content.worldBound.Contains(evt.position)) return;
      _entry.Handle.Dismiss(OverlayDismissReason.OutsidePointer);
      evt.StopImmediatePropagation();
    }

    private void OnKeyDown(KeyDownEvent evt) {
      if (evt.keyCode != KeyCode.Escape || !DismissFromCancel()) return;
      evt.StopImmediatePropagation();
    }

    private void OnNavigationCancel(NavigationCancelEvent evt) {
      if (!DismissFromCancel()) return;
      evt.StopImmediatePropagation();
    }

    private bool DismissFromCancel() {
      if (_entry == null || !_entry.Options.dismissOnCancel) return false;
      if (!ReferenceEquals(_entry.Handle.Controller.Top, _entry)) return false;
      return _entry.Handle.Dismiss(OverlayDismissReason.Cancel);
    }

    private void OnFocusOut(FocusOutEvent evt) {
      if (_entry?.Options.captureFocus != true) return;
      var next = evt.relatedTarget as VisualElement;
      if (IsInsideOverlayFamily(next)) return;
      if (next == null) {
        RecoverFocus();
        return;
      }

      var controller = _entry.Handle.Controller;
      var scopeRoot = controller.FocusScopeRoot(_entry);
      if (IsInsideOverlayFamily(next, scopeRoot)) {
        if (_entry.Options.parent != null) _entry.Handle.Dismiss(OverlayDismissReason.FocusLost);
        return;
      }
      if (!scopeRoot.Options.dismissOnOutsidePointer) {
        RecoverFocus();
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

    private void OnAttachToPanel(AttachToPanelEvent evt) => CaptureFocus();

    private void RecoverFocus() {
      _focusRecovery?.Pause();
      _focusRecovery = schedule.Execute(() => {
          _focusRecovery = null;
          if (_entry == null || panel == null || !_entry.Handle.IsOpen) return;
          if (!ReferenceEquals(_entry.Handle.Controller.Top, _entry)) return;
          var focused = panel.focusController.focusedElement as VisualElement;
          if (IsInsideOverlayFamily(focused)) return;
          Focus();
        }
      );
      _focusRecovery.ExecuteLater(1);
    }

    private void CaptureFocus() {
      if (_entry?.Options.captureFocus != true || panel == null) return;
      if (_entry.Options.prelayout && !_positioned) return;
      if (!_hasCapturedPreviousFocus) {
        _previousFocus = panel.focusController.focusedElement as VisualElement;
        _hasCapturedPreviousFocus = true;
      }
      RecoverFocus();
    }

    private void RestoreFocus() {
      _focusRecovery?.Pause();
      _focusRecovery = null;
      if (_entry?.Options.restoreFocus == true &&
          _entry.DismissReason != OverlayDismissReason.FocusLost &&
          _previousFocus?.panel != null) _previousFocus.Focus();
      ForgetPreviousFocus();
    }

    private void ForgetPreviousFocus() {
      _previousFocus = null;
      _hasCapturedPreviousFocus = false;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) {
      if (_entry?.Options.prelayout != true || !_content.hasComposed) return;
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
      CaptureFocus();
    }

    private static void ResolveViewport(
      OverlayPlacement placement, Rect host, float width, float height, float margin,
      out float x, out float y
    ) {
      x = (host.width - width) * 0.5f;
      y = (host.height - height) * 0.5f;
      switch (placement) {
        case OverlayPlacement.TopStart:
          x = margin;
          y = margin;
          break;
        case OverlayPlacement.Top: y = margin; break;
        case OverlayPlacement.TopEnd:
          x = host.width - width - margin;
          y = margin;
          break;
        case OverlayPlacement.BottomStart:
          x = margin;
          y = host.height - height - margin;
          break;
        case OverlayPlacement.Bottom: y = host.height - height - margin; break;
        case OverlayPlacement.BottomEnd:
          x = host.width - width - margin;
          y = host.height - height - margin;
          break;
        case OverlayPlacement.Start: x = margin; break;
        case OverlayPlacement.End: x = host.width - width - margin; break;
      }
    }

    private static void ResolveAnchored(
      OverlayPlacement placement, Rect anchor, float width, float height,
      out float x, out float y
    ) {
      x = anchor.xMin;
      y = anchor.yMax;
      switch (placement) {
        case OverlayPlacement.AboveStart:
          x = anchor.xMin;
          y = anchor.yMin - height;
          break;
        case OverlayPlacement.Above:
          x = anchor.center.x - width * 0.5f;
          y = anchor.yMin - height;
          break;
        case OverlayPlacement.AboveEnd:
          x = anchor.xMax - width;
          y = anchor.yMin - height;
          break;
        case OverlayPlacement.BelowStart:
          x = anchor.xMin;
          y = anchor.yMax;
          break;
        case OverlayPlacement.Below:
          x = anchor.center.x - width * 0.5f;
          y = anchor.yMax;
          break;
        case OverlayPlacement.BelowEnd:
          x = anchor.xMax - width;
          y = anchor.yMax;
          break;
        case OverlayPlacement.Before:
          x = anchor.xMin - width;
          y = anchor.center.y - height * 0.5f;
          break;
        case OverlayPlacement.After:
          x = anchor.xMax;
          y = anchor.center.y - height * 0.5f;
          break;
        default:
          x = anchor.xMin;
          y = anchor.yMax;
          break;
      }
    }

    private static void FlipToFit(
      OverlayPlacement placement, Rect anchor, Rect host, float width, float height,
      ref float x, ref float y
    ) {
      if (IsBelow(placement) && y + height > host.height) y = anchor.yMin - height;
      else if (IsAbove(placement) && y < 0f) y = anchor.yMax;
      if (placement == OverlayPlacement.After && x + width > host.width) x = anchor.xMin - width;
      else if (placement == OverlayPlacement.Before && x < 0f) x = anchor.xMax;
    }

    private static bool IsAbove(OverlayPlacement placement) =>
      placement is OverlayPlacement.AboveStart or OverlayPlacement.Above or OverlayPlacement.AboveEnd;

    private static bool IsBelow(OverlayPlacement placement) =>
      placement is OverlayPlacement.BelowStart or OverlayPlacement.Below or OverlayPlacement.BelowEnd;

    private static float StackDirection(OverlayPlacement placement) =>
      placement is OverlayPlacement.BottomStart or OverlayPlacement.Bottom or OverlayPlacement.BottomEnd ? -1f : 1f;

    private static Vector2 ResolveConstraintSize(BoxConstraints? constraints, Vector2 available) {
      if (!constraints.HasValue) return Vector2.zero;
      var value = constraints.Value;
      return new Vector2(
        ResolveConstraint(value.preferred.w, value.min.w, value.max.w, available.x),
        ResolveConstraint(value.preferred.h, value.min.h, value.max.h, available.y)
      );
    }

    private static float ResolveConstraint(
      StyleLength preferred,
      StyleLength minimum,
      StyleLength maximum,
      float available
    ) {
      var min = ResolveLength(minimum, available, 0f);
      var max = ResolveLength(maximum, available, float.PositiveInfinity);
      var value = ResolveLength(preferred, available, 0f, available);
      return Mathf.Clamp(value, min, Mathf.Max(min, max));
    }

    private static float ResolveLength(
      StyleLength length,
      float available,
      float fallback,
      float automatic = float.NaN
    ) {
      if (length.keyword == StyleKeyword.Undefined) {
        return length.value.unit == LengthUnit.Percent
          ? length.value.value * available / 100f
          : length.value.value;
      }
      if ((length.keyword is StyleKeyword.Auto or StyleKeyword.Initial) && !float.IsNaN(automatic)) {
        return automatic;
      }
      return fallback;
    }

    private static float FiniteOrZero(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
  }

  [ComposableProxy(Extension = false)]
  public sealed partial class HXOverlayHostElement : ComposableElement, ISlotHost {
    public static readonly UniqueStyleString ClassContent = new("hx-overlay-host-content");
    public static readonly UniqueStyleString ClassLayer = new("hx-overlay-host-layer");

    private readonly Dictionary<long, OverlayEntryElement> _elements = new();
    private readonly List<long> _removals = new();
    private readonly VisualElement _layer;
    private bool _isPlacing;
    private OverlayController _controller;

    public readonly ComposableSlot content;
    public IBoundary Boundary { get; set; }

    public HXOverlayHostElement() {
      this.MakeRelative();
      pickingMode = PickingMode.Ignore;
      content = new ComposableSlot(this, ClassContent)
        .WithClasses(ClassContent)
        .AddTo(hierarchy);
      content.style.display = DisplayStyle.Flex;
      content.Flexible(1f, 1f, Align.Stretch);

      _layer = new VisualElement().WithClasses(ClassLayer).Stretched().AddTo(hierarchy);
      _layer.pickingMode = PickingMode.Ignore;
      _layer.RegisterCallback<GeometryChangedEvent>(OnLayerGeometryChanged);
    }

    public void Update([Prop] IBoundary boundary, [Prop] OverlayController controller) {
      Boundary = boundary;
      _controller = controller;
      SynchronizeEntries();
    }

    public override void Reset() {
      base.Reset();
      content.Reset();
      ClearEntries();
      Boundary = null;
      _controller = null;
    }

    private void SynchronizeEntries() {
      if (_controller == null) {
        ClearEntries();
        return;
      }

      var entries = _controller.Entries;
      for (var i = 0; i < entries.Count; i++) {
        var entry = entries[i];
        if (!_elements.TryGetValue(entry.Id, out var element)) {
          element = new OverlayEntryElement(RequestPlacement);
          _elements.Add(entry.Id, element);
        }
        element.Bind(entry);
        if (!ReferenceEquals(element.parent, _layer)) _layer.Add(element);
        var currentIndex = _layer.IndexOf(element);
        if (currentIndex != i) VisualElementOrdering.Move(_layer.hierarchy, element, i);
      }

      _removals.Clear();
      foreach (var pair in _elements) {
        var retained = false;
        for (var i = 0; i < entries.Count; i++) {
          if (entries[i].Id != pair.Key) continue;
          retained = true;
          break;
        }
        if (!retained) _removals.Add(pair.Key);
      }
      foreach (var id in _removals) {
        var element = _elements[id];
        element.RemoveFromHierarchy();
        element.Dispose();
        _elements.Remove(id);
      }
      RequestPlacement();
    }

    private void RequestPlacement() {
      if (_isPlacing) return;
      try {
        _isPlacing = true;
        PlaceEntries();
      } finally {
        _isPlacing = false;
      }
    }

    private void PlaceEntries() {
      var offsets = new Dictionary<OverlayPlacement, float>();
      var pending = new HashSet<OverlayPlacement>();
      if (_controller == null) return;
      foreach (var entry in _controller.Entries) {
        if (!_elements.TryGetValue(entry.Id, out var element)) continue;
        var offset = 0f;
        var options = entry.Options;
        var stack = options.stacked && options.anchor == null;
        if (stack) {
          if (pending.Contains(options.placement)) continue;
          offsets.TryGetValue(options.placement, out offset);
        }
        if (!element.ApplyPlacement(offset)) {
          if (stack) pending.Add(options.placement);
          continue;
        }
        if (options.stacked && options.anchor == null) {
          offsets[options.placement] = offset + element.PlacementHeight + Mathf.Max(0f, options.stackSpacing);
        }
      }
    }

    private void ClearEntries() {
      foreach (var element in _elements.Values) {
        element.RemoveFromHierarchy();
        element.Dispose();
      }
      _elements.Clear();
      _layer.Clear();
    }

    private void OnLayerGeometryChanged(GeometryChangedEvent evt) => RequestPlacement();
  }

  [BoundaryComposable(Extension = false)]
  public partial class OverlayHostBoundary {
    public partial struct Props {
      [Prop(null)] public OverlayController controller;
    }

    public OverlayController controller { get; private set; }
    public HXOverlayHostElement viewElement { get; private set; }
    private bool _isAutomaticController;

    protected override void OnDetach() {
      if (_isAutomaticController) controller?.Dispose();
      controller = null;
      viewElement = null;
      _isAutomaticController = false;
    }

    protected override void OnRecompose(ref Composition cx) {
      EnsureController();
      cx.SubscribeTo(controller);
      var contextData = new OverlayContextData(controller, null);
      using (cx.WriteContext(out var context)) {
        OverlayContextData.Key[in context] = contextData;
      }
      Node.MakeRelative().Flexible(1f, 1f, Align.Stretch);
      HXOverlayHostElement.Compose(ref cx, Node.Parent, controller);
      viewElement = (HXOverlayHostElement)cx.CURSOR.element;
      cx.CURSOR.Fill().Focusable(false, pickingMode: PickingMode.Ignore);
    }

    private void EnsureController() {
      if (props.controller == null) {
        if (controller != null && _isAutomaticController) return;
        if (_isAutomaticController) controller?.Dispose();
        controller = new OverlayController();
        _isAutomaticController = true;
        return;
      }
      if (ReferenceEquals(controller, props.controller)) return;
      if (_isAutomaticController) controller?.Dispose();
      controller = props.controller;
      _isAutomaticController = false;
    }
  }

  public readonly struct OverlayHostScope {
    internal OverlayHostScope(OverlayHostBoundary boundary) {
      controller = boundary.controller;
    }

    public readonly OverlayController controller;
  }

  public static class OverlayExtensions {
    public static ScopeHandle OverlayHost(
      this ref Composition cx,
      OverlayController controller = null
    ) {
      ref var result = ref OverlayHostBoundary.ComposeBoundary(ref cx, controller);
      var boundary = (result.element as CompositionBoundaryNodeBase)?.BoundaryComposable as OverlayHostBoundary;
      if (boundary?.viewElement == null) throw new InvalidOperationException("Overlay host was not initialized.");
      return boundary.viewElement.content.Scope(cx);
    }

    public static ScopeHandle OverlayHost(
      this ref Composition cx,
      out OverlayHostScope scope,
      OverlayController controller = null
    ) {
      ref var result = ref OverlayHostBoundary.ComposeBoundary(ref cx, controller);
      var boundary = (result.element as CompositionBoundaryNodeBase)?.BoundaryComposable as OverlayHostBoundary;
      if (boundary?.viewElement == null) throw new InvalidOperationException("Overlay host was not initialized.");
      scope = new OverlayHostScope(boundary);
      return boundary.viewElement.content.Scope(cx);
    }

    public static OverlayController Overlays(this ref Composition cx, bool listen = true) =>
      cx.ReadContext(OverlayContextData.Key, listen).controller;

    public static OverlayController Overlays(this CompositionContext context) =>
      OverlayContextData.Key.ReadAt(context.element).controller ??
      context.Lookup<OverlayHostBoundary>()?.controller;

    public static OverlayHandle OverlayEntry(this ref Composition cx, bool listen = true) =>
      cx.ReadContext(OverlayContextData.Key, listen).handle;

    public static OverlayHandle OverlayEntry(this CompositionContext context) =>
      OverlayContextData.Key.ReadAt(context.element).handle ??
      context.Lookup<OverlayContentBoundary>()?.handle;
  }
}