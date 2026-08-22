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

  [Flags]
  public enum OverlayBehavior : ushort {
    None = 0,
    FlipToFit = 1 << 0,
    ClampToHost = 1 << 1,
    FollowAnchor = 1 << 2,
    MatchAnchorWidth = 1 << 3,
    DismissWhenAnchorDetached = 1 << 4,
    Barrier = 1 << 5,
    DismissOnOutsidePointer = 1 << 6,
    DismissOnCancel = 1 << 7,
    CaptureFocus = 1 << 8,
    RestoreFocus = 1 << 9,
    Prelayout = 1 << 10,
    Stacked = 1 << 11,
    Default = FlipToFit | ClampToHost | FollowAnchor | DismissWhenAnchorDetached |
              DismissOnCancel | RestoreFocus | Prelayout
  }

  [PropStruct] public readonly partial struct OverlayOptions {
    // Force the generated constructor to run; an empty struct initializer zeroes every option.
    public static readonly OverlayOptions Default = new(behavior: OverlayBehavior.Default);

    [Prop(OverlayPlacement.Center)] public readonly OverlayPlacement placement;
    [Prop(OverlayBehavior.Default)] public readonly OverlayBehavior behavior;
    [Prop(null)] public readonly OverlayHandle parent;
    [Prop(null)] public readonly VisualElement anchor;
    [Prop("default", PropInit.Constant)] public readonly Vector2 offset;
    [Prop(12f)] public readonly float margin;
    [Prop("new UnityEngine.Color(0f, 0f, 0f, 0.45f)", PropInit.Deferred)]
    public readonly Color barrierColor;
    [Prop(null)] public readonly BoxConstraints? constraints;
    [Prop(8f)] public readonly float stackSpacing;
    [Prop(0)] public readonly int timeoutMs;
    [Prop(null)] public readonly CompositionAction<OverlayDismissReason> onDismissed;

    public bool Has(OverlayBehavior value) => (behavior & value) == value;
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
    public Composable<OverlayContextData> Content { get; private set; }
    public OverlayOptions Options { get; }
    public OverlayHandle Handle { get; }
    internal uint ContentRevision { get; private set; }
    internal IBoundary CallbackBoundary { get; }
    internal OverlayDismissReason? DismissReason { get; set; }

    internal void Replace(Composable<OverlayContextData> content) {
      Content = content;
      unchecked { ContentRevision++; }
    }
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
    private OverlayBehavior _behavior;
    private Color _barrierColor;
    private BoxConstraints? _constraints;
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
      _behavior = options.behavior;
      _barrierColor = options.barrierColor;
      _constraints = options.constraints;
      _stackSpacing = options.stackSpacing;
      _timeoutMs = options.timeoutMs;
      _onDismissed = options.onDismissed;
    }

    public OverlayBuilder Modal(
      bool dismissOnOutsidePointer = false,
      Color? barrierColor = null
    ) {
      Set(OverlayBehavior.Barrier | OverlayBehavior.CaptureFocus, true);
      Set(OverlayBehavior.DismissOnOutsidePointer, dismissOnOutsidePointer);
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

    public OverlayBuilder Parent(OverlayHandle parent) {
      _parent = parent;
      return this;
    }

    public OverlayBuilder Behaviors(OverlayBehavior behavior) {
      _behavior = behavior;
      return this;
    }

    public OverlayBuilder FollowAnchor(bool enabled = true) => Set(OverlayBehavior.FollowAnchor, enabled);

    public OverlayBuilder MatchAnchorWidth(bool enabled = true) => Set(OverlayBehavior.MatchAnchorWidth, enabled);

    public OverlayBuilder FlipToFit(bool enabled = true) => Set(OverlayBehavior.FlipToFit, enabled);

    public OverlayBuilder ClampToHost(bool enabled = true, float margin = 12f) {
      _margin = Mathf.Max(0f, margin);
      return Set(OverlayBehavior.ClampToHost, enabled);
    }

    public OverlayBuilder DismissOnOutsidePointer(bool enabled = true) =>
      Set(OverlayBehavior.DismissOnOutsidePointer, enabled);

    public OverlayBuilder DismissOnCancel(bool enabled = true) => Set(OverlayBehavior.DismissOnCancel, enabled);

    public OverlayBuilder DismissWhenAnchorDetaches(bool enabled = true) =>
      Set(OverlayBehavior.DismissWhenAnchorDetached, enabled);

    public OverlayBuilder CaptureFocus(bool enabled = true, bool restore = true) {
      Set(OverlayBehavior.RestoreFocus, enabled && restore);
      return Set(OverlayBehavior.CaptureFocus, enabled);
    }

    /// <summary>
    /// Measures the composed surface while hidden before placing it. When disabled, content measurements
    /// never request placement updates and positioning uses the supplied constraints instead.
    /// </summary>
    public OverlayBuilder Prelayout(bool enabled = true) => Set(OverlayBehavior.Prelayout, enabled);

    /// <summary>
    /// Constrains the overlay surface and provides its expected size to placement without a prelayout pass.
    /// </summary>
    public OverlayBuilder Constraints(in BoxConstraints constraints) {
      _constraints = constraints;
      return this;
    }

    public OverlayBuilder Stacked(bool enabled = true, float spacing = 8f) {
      _stackSpacing = Mathf.Max(0f, spacing);
      return Set(OverlayBehavior.Stacked, enabled);
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

    private OverlayBuilder Set(OverlayBehavior behavior, bool enabled) {
      if (enabled) _behavior |= behavior;
      else _behavior &= ~behavior;
      return this;
    }

    private OverlayOptions BuildOptions() => new(
      placement: _placement, behavior: _behavior, parent: _parent, anchor: _anchor,
      offset: _offset, margin: _margin, barrierColor: _barrierColor, constraints: _constraints,
      stackSpacing: _stackSpacing, timeoutMs: _timeoutMs, onDismissed: _onDismissed
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

    internal bool DismissFromCancel(OverlayEntry scope) {
      if (scope == null) return false;
      for (var i = _entries.Count - 1; i >= 0; i--) {
        var entry = _entries[i];
        if (!entry.Options.Has(OverlayBehavior.DismissOnCancel) || !IsDescendantOf(entry, scope)) continue;
        return Dismiss(entry.Id, OverlayDismissReason.Cancel);
      }
      return false;
    }

    internal OverlayEntry FocusScopeRoot(OverlayEntry entry) {
      var root = entry;
      var current = entry;
      while (current != null && current.Options.parent != null) {
        current = Find(current.Options.parent.Id);
        if (current != null && current.Options.Has(OverlayBehavior.CaptureFocus)) root = current;
      }
      return root;
    }

    public bool Replace(long id, Composable<OverlayContextData> content) {
      if (content == null) throw new ArgumentNullException(nameof(content));
      var index = FindIndex(id);
      if (index < 0) return false;
      _entries[index].Replace(content);
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

  [ComposableProxy(Extension = false)]
  public sealed partial class HXOverlayHostElement : ComposableElement, ISlotHost {
    public static readonly UniqueStyleString ClassContent = new("hx-overlay-host-content");
    public static readonly UniqueStyleString ClassLayer = new("hx-overlay-host-layer");

    private readonly Dictionary<long, OverlayEntryElement> _elements = new();
    private readonly List<long> _removals = new();
    private readonly List<OverlayEntry> _placementEntries = new();
    private readonly Dictionary<OverlayPlacement, float> _placementOffsets = new();
    private readonly HashSet<OverlayPlacement> _pendingPlacements = new();
    private readonly VisualElement _layer;
    private bool _isPlacing;
    private bool _placementPending;
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
      if (_isPlacing) {
        _placementPending = true;
        return;
      }
      do {
        _placementPending = false;
        try {
          _isPlacing = true;
          PlaceEntries();
        } finally {
          _isPlacing = false;
        }
      } while (_placementPending);
    }

    private void PlaceEntries() {
      var controller = _controller;
      if (controller == null) return;
      _placementEntries.Clear();
      var entries = controller.Entries;
      for (var i = 0; i < entries.Count; i++) _placementEntries.Add(entries[i]);
      _placementOffsets.Clear();
      _pendingPlacements.Clear();

      for (var i = 0; i < _placementEntries.Count; i++) {
        var entry = _placementEntries[i];
        if (!entry.Handle.IsOpen) continue;
        if (!_elements.TryGetValue(entry.Id, out var element)) continue;
        var offset = 0f;
        var options = entry.Options;
        var stack = options.Has(OverlayBehavior.Stacked) && options.anchor == null;
        if (stack) {
          if (_pendingPlacements.Contains(options.placement)) continue;
          _placementOffsets.TryGetValue(options.placement, out offset);
        }
        if (!element.ApplyPlacement(offset)) {
          if (stack) _pendingPlacements.Add(options.placement);
          continue;
        }
        if (!entry.Handle.IsOpen || !_elements.ContainsKey(entry.Id)) continue;
        if (stack) {
          _placementOffsets[options.placement] =
            offset + element.PlacementHeight + Mathf.Max(0f, options.stackSpacing);
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
      _placementEntries.Clear();
      _placementOffsets.Clear();
      _pendingPlacements.Clear();
      _placementPending = false;
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
      return boundary.viewElement.content.Scope(ref cx);
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
      return boundary.viewElement.content.Scope(ref cx);
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
      context.Lookup<OverlayContentBoundary>()?.Handle;
  }
}
