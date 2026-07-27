using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public enum OverlayKind : byte { Popup, Popover, Modal, Menu, Notification }

  public enum OverlayPlacement : byte {
    Center,
    TopStart,
    Top,
    TopEnd,
    BottomStart,
    Bottom,
    BottomEnd,
    Left,
    Right
  }

  public enum OverlayDismissReason : byte {
    Manual,
    Action,
    Cancel,
    OutsidePointer,
    Timeout,
    AnchorDetached,
    Replaced,
    HostDetached
  }

  public readonly struct OverlayBarrier {
    public readonly bool visible;
    public readonly bool blocksPointer;
    public readonly bool dismissOnPointerDown;
    public readonly Color color;
    public readonly StateComposable visual;

    public OverlayBarrier(
      bool visible = true,
      bool blocksPointer = true,
      bool dismissOnPointerDown = true,
      Color color = default,
      StateComposable visual = null
    ) {
      this.visible = visible;
      this.blocksPointer = blocksPointer;
      this.dismissOnPointerDown = dismissOnPointerDown;
      this.color = color == default ? Colors.Black40 : color;
      this.visual = visual;
    }

    public static OverlayBarrier Scrim(
      bool dismissOnPointerDown = true,
      Color color = default,
      StateComposable visual = null
    ) {
      return new OverlayBarrier(
        blocksPointer: true,
        dismissOnPointerDown: dismissOnPointerDown,
        color: color,
        visual: visual
      );
    }
  }

  public delegate void OverlayComposable(ref Composition cx, OverlayHandle handle);

  public readonly struct OverlayOptions {
    public readonly OverlayPlacement placement;
    public readonly Vector2 offset;
    public readonly bool dismissOnOutsidePointer;
    public readonly bool dismissOnCancel;
    public readonly bool modal;
    public readonly bool showScrim;
    public readonly bool flip;
    public readonly bool clampToHost;
    public readonly bool restoreFocus;
    public readonly Color scrimColor;
    public readonly OverlayBarrier barrier;

    public OverlayOptions(
      OverlayPlacement placement = OverlayPlacement.Center,
      Vector2 offset = default,
      bool dismissOnOutsidePointer = false,
      bool dismissOnCancel = true,
      bool modal = false,
      bool showScrim = false,
      bool flip = true,
      bool clampToHost = true,
      bool restoreFocus = true,
      Color scrimColor = default,
      OverlayBarrier? barrier = null
    ) {
      this.placement = placement;
      this.offset = offset;
      this.dismissOnOutsidePointer = dismissOnOutsidePointer;
      this.dismissOnCancel = dismissOnCancel;
      this.modal = modal;
      this.barrier = barrier ?? (showScrim
        ? OverlayBarrier.Scrim(dismissOnPointerDown: dismissOnOutsidePointer, color: scrimColor)
        : default);
      this.showScrim = this.barrier.visible;
      this.flip = flip;
      this.clampToHost = clampToHost;
      this.restoreFocus = restoreFocus;
      this.scrimColor = this.barrier.visible
        ? this.barrier.color
        : scrimColor == default
          ? Colors.Black40
          : scrimColor;
    }

    public static OverlayOptions Popover(
      OverlayPlacement placement = OverlayPlacement.BottomStart,
      Vector2 offset = default
    ) {
      return new OverlayOptions(
        placement,
        offset,
        dismissOnOutsidePointer: true,
        dismissOnCancel: true,
        flip: true,
        clampToHost: true
      );
    }

    public static OverlayOptions Modal(bool dismissOnOutsidePointer = true) {
      return new OverlayOptions(
        OverlayPlacement.Center,
        dismissOnOutsidePointer: dismissOnOutsidePointer,
        dismissOnCancel: true,
        modal: true,
        showScrim: true,
        flip: false,
        clampToHost: true,
        barrier: OverlayBarrier.Scrim(dismissOnPointerDown: dismissOnOutsidePointer)
      );
    }
  }

  public readonly struct OverlaySpec {
    public readonly OverlayKind kind;
    public readonly OverlayComposable content;
    public readonly VisualElement anchor;
    public readonly OverlayOptions options;
    public readonly Action<OverlayHandle, OverlayDismissReason> dismissed;

    public OverlaySpec(
      OverlayKind kind,
      OverlayComposable content,
      VisualElement anchor = null,
      OverlayOptions? options = null,
      Action<OverlayHandle, OverlayDismissReason> dismissed = null
    ) {
      this.kind = kind;
      this.content = content ?? throw new ArgumentNullException(nameof(content));
      this.anchor = anchor;
      this.options = options ?? DefaultOptions(kind);
      this.dismissed = dismissed;
    }

    private static OverlayOptions DefaultOptions(OverlayKind kind) {
      return kind switch {
        OverlayKind.Popover or OverlayKind.Menu => OverlayOptions.Popover(),
        OverlayKind.Modal => OverlayOptions.Modal(),
        OverlayKind.Notification => new OverlayOptions(
          OverlayPlacement.Bottom,
          dismissOnCancel: false,
          flip: false,
          restoreFocus: false
        ),
        _ => new OverlayOptions()
      };
    }
  }

  public sealed class OverlayHandle {
    private readonly Composable _composition;
    private OverlayController _controller;

    internal OverlayHandle(OverlayController controller, int id, in OverlaySpec spec) {
      _controller = controller;
      Id = id;
      Spec = spec;
      _composition = Compose;
    }

    public int Id { get; }
    public OverlaySpec Spec { get; }
    public bool IsShown { get; internal set; }
    public bool IsPending { get; internal set; }
    public OverlayDismissReason? DismissReason { get; internal set; }
    public VisualElement View => Entry?.boundary;

    internal OverlayEntry Entry { get; set; }
    internal int TimeoutMs { get; set; }
    internal Composable Composition => _composition;

    public void Dismiss(OverlayDismissReason reason = OverlayDismissReason.Manual) {
      _controller?.Dismiss(this, reason);
    }

    internal void ReleaseController() {
      _controller = null;
    }

    private void Compose(ref Composition cx) {
      Spec.content(ref cx, this);
    }
  }

  internal sealed class OverlayEntry {
    public readonly OverlayHandle handle;
    public readonly OverlayEntryView view;
    public readonly CompositionBoundaryNode boundary;
    public readonly Focusable previousFocus;
    public IVisualElementScheduledItem timeout;

    public OverlayEntry(
      OverlayHandle handle,
      OverlayEntryView view,
      CompositionBoundaryNode boundary,
      Focusable previousFocus
    ) {
      this.handle = handle;
      this.view = view;
      this.boundary = boundary;
      this.previousFocus = previousFocus;
    }
  }

  internal sealed class OverlayEntryView : VisualElement {
    private readonly OverlayRootElement _root;

    public OverlayEntryView(OverlayRootElement root, OverlayHandle handle) {
      _root = root;
      Handle = handle;
      pickingMode = PickingMode.Ignore;
      style.position = Position.Absolute;
      style.left = 0f;
      style.top = 0f;
      style.right = 0f;
      style.bottom = 0f;
      RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    public OverlayHandle Handle { get; }
    public VisualElement content { get; private set; }

    public void SetContent(VisualElement element) {
      content = element;
      content.style.position = Position.Absolute;
      content.pickingMode = PickingMode.Position;
      content.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);
      hierarchy.Add(content);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) {
      _root.RefreshEntry(this);
    }

    private void OnContentGeometryChanged(GeometryChangedEvent evt) {
      _root.RefreshAllEntries();
    }
  }

  internal sealed class OverlayBarrierElement : VisualElement {
    private readonly OverlayHandle _handle;
    private readonly OverlayBarrier _barrier;

    public OverlayBarrierElement(OverlayHandle handle, in OverlayBarrier barrier) {
      _handle = handle;
      _barrier = barrier;
      name = "Barrier";
      pickingMode = barrier.blocksPointer ? PickingMode.Position : PickingMode.Ignore;
      style.position = Position.Absolute;
      style.left = 0f;
      style.top = 0f;
      style.right = 0f;
      style.bottom = 0f;
      if (barrier.visual == null) style.backgroundColor = barrier.color;
      RegisterCallback<PointerDownEvent>(OnPointerDown);

      if (barrier.visual == null) return;
      var composition = new BarrierComposition(barrier.visual);
      var visual = new CompositionBoundaryNode {
        name = "BarrierVisual",
        composable = composition.Compose,
        pickingMode = PickingMode.Ignore
      };
      visual.style.position = Position.Absolute;
      visual.style.left = 0f;
      visual.style.top = 0f;
      visual.style.right = 0f;
      visual.style.bottom = 0f;
      hierarchy.Add(visual);
      RecompositionScope.MarkDirty(visual);
    }

    private void OnPointerDown(PointerDownEvent evt) {
      if (_barrier.dismissOnPointerDown) {
        _handle.Dismiss(OverlayDismissReason.OutsidePointer);
      }
      if (_barrier.blocksPointer) evt.StopPropagation();
    }

    private sealed class BarrierComposition {
      private readonly StateComposable _visual;

      public BarrierComposition(StateComposable visual) {
        _visual = visual;
      }

      public void Compose(ref Composition cx) {
        _visual(ref cx, StateFlag.None);
      }
    }
  }

  internal sealed class OverlayRootElement : VisualElement, IComposable {
    private readonly VisualElement _body;
    private readonly VisualElement _overlay;
    private readonly IVisualElementScheduledItem _positionPoll;
    private OverlayController _controller;
    private int _lastKeyboardCancelFrame = -1;

    public OverlayRootElement() {
      style.flexGrow = 1f;
      style.position = Position.Relative;

      _body = new VisualElement { name = "Body" }.Flexible().AddTo(hierarchy);
      _overlay = new VisualElement {
        name = "Overlays",
        pickingMode = PickingMode.Ignore,
        focusable = true
      }.Stretched().AddTo(hierarchy);

      RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
      RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
      RegisterCallback<NavigationCancelEvent>(OnNavigationCancel, TrickleDown.TrickleDown);
      RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
      _positionPoll = schedule.Execute(PollPositions).Every(16);
      _positionPoll.Pause();
    }

    public VisualElement Element => _body;
    public VisualElement OverlayLayer => _overlay;
    public UssFlag Flag { get; set; }
    public ulong TypeId { get; set; }

    public void Bind(OverlayController controller) {
      if (ReferenceEquals(_controller, controller)) return;
      var previous = _controller;
      _controller = null;
      previous?.Detach(this, OverlayDismissReason.HostDetached);
      controller?.Attach(this);
      _controller = controller;
      UpdatePolling();
    }

    public void Reset() {
      _controller?.Detach(this, OverlayDismissReason.HostDetached);
      _controller = null;
      _positionPoll.Pause();
    }

    public void AddEntry(OverlayEntry entry) {
      _overlay.Add(entry.view);
      UpdatePolling();
      RefreshEntry(entry.view);
    }

    public void RemoveEntry(OverlayEntry entry) {
      entry.view.RemoveFromHierarchy();
      UpdatePolling();
    }

    public void RefreshEntry(OverlayEntryView view) {
      var handle = view.Handle;
      if (!handle.IsShown || view.content == null || panel == null) return;
      var options = handle.Spec.options;
      var rootRect = contentRect;
      var contentSize = view.content.layout.size;
      if (float.IsNaN(contentSize.x) || float.IsNaN(contentSize.y)) return;

      var anchorRect = handle.Spec.anchor != null && handle.Spec.anchor.panel == panel
        ? this.WorldToLocal(handle.Spec.anchor.worldBound)
        : rootRect;
      var position = ComputePosition(rootRect, anchorRect, contentSize, options);
      if (handle.Spec.kind == OverlayKind.Notification) {
        position += _controller?.GetNotificationStackOffset(handle) ?? Vector2.zero;
      }
      if (!Mathf.Approximately(view.content.resolvedStyle.left, position.x)) {
        view.content.style.left = position.x;
      }
      if (!Mathf.Approximately(view.content.resolvedStyle.top, position.y)) {
        view.content.style.top = position.y;
      }
    }

    public void RefreshAllEntries() {
      _controller?.RefreshPositions();
    }

    private static Vector2 ComputePosition(
      Rect root,
      Rect anchor,
      Vector2 size,
      in OverlayOptions options
    ) {
      var position = options.placement switch {
        OverlayPlacement.TopStart => new Vector2(anchor.xMin, anchor.yMin - size.y),
        OverlayPlacement.Top => new Vector2(anchor.center.x - size.x * 0.5f, anchor.yMin - size.y),
        OverlayPlacement.TopEnd => new Vector2(anchor.xMax - size.x, anchor.yMin - size.y),
        OverlayPlacement.BottomStart => new Vector2(anchor.xMin, anchor.yMax),
        OverlayPlacement.Bottom => new Vector2(anchor.center.x - size.x * 0.5f, anchor.yMax),
        OverlayPlacement.BottomEnd => new Vector2(anchor.xMax - size.x, anchor.yMax),
        OverlayPlacement.Left => new Vector2(anchor.xMin - size.x, anchor.center.y - size.y * 0.5f),
        OverlayPlacement.Right => new Vector2(anchor.xMax, anchor.center.y - size.y * 0.5f),
        _ => new Vector2(root.center.x - size.x * 0.5f, root.center.y - size.y * 0.5f)
      };
      position += options.offset;

      if (options.flip) {
        switch (options.placement) {
          case OverlayPlacement.BottomStart:
          case OverlayPlacement.Bottom:
          case OverlayPlacement.BottomEnd:
            if (position.y + size.y > root.yMax && anchor.yMin - size.y >= root.yMin) {
              position.y = anchor.yMin - size.y - options.offset.y;
            }
            break;
          case OverlayPlacement.TopStart:
          case OverlayPlacement.Top:
          case OverlayPlacement.TopEnd:
            if (position.y < root.yMin && anchor.yMax + size.y <= root.yMax) {
              position.y = anchor.yMax - options.offset.y;
            }
            break;
          case OverlayPlacement.Left:
            if (position.x < root.xMin && anchor.xMax + size.x <= root.xMax) {
              position.x = anchor.xMax - options.offset.x;
            }
            break;
          case OverlayPlacement.Right:
            if (position.x + size.x > root.xMax && anchor.xMin - size.x >= root.xMin) {
              position.x = anchor.xMin - size.x - options.offset.x;
            }
            break;
        }
      }

      if (options.clampToHost) {
        position.x = Mathf.Clamp(position.x, root.xMin, Mathf.Max(root.xMin, root.xMax - size.x));
        position.y = Mathf.Clamp(position.y, root.yMin, Mathf.Max(root.yMin, root.yMax - size.y));
      }
      return position;
    }

    private void UpdatePolling() {
      if (_controller?.HasAnchoredEntries == true) _positionPoll.Resume();
      else _positionPoll.Pause();
    }

    private void PollPositions() {
      _controller?.PollAnchors();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) {
      _controller?.RefreshPositions();
    }

    private void OnPointerDown(PointerDownEvent evt) {
      _controller?.HandlePointerDown(evt);
    }

    private void OnKeyDown(KeyDownEvent evt) {
      if (evt.keyCode == KeyCode.Escape && _controller?.DismissTopCancelable() == true) {
        _lastKeyboardCancelFrame = Time.frameCount;
        evt.StopPropagation();
      }
    }

    private void OnNavigationCancel(NavigationCancelEvent evt) {
      if (_lastKeyboardCancelFrame == Time.frameCount) {
        evt.StopPropagation();
        return;
      }
      if (_controller?.DismissTopCancelable() == true) evt.StopPropagation();
    }
  }

  public sealed class OverlayController : IDisposable {
    private readonly List<OverlayEntry> _entries = new(4);
    private readonly List<OverlayHandle> _pending = new(2);
    private OverlayRootElement _root;
    private int _nextId = 1;
    private bool _disposed;

    public int Count => _entries.Count + _pending.Count;
    public bool HasAnchoredEntries {
      get {
        for (var i = 0; i < _entries.Count; i++) {
          if (_entries[i].handle.Spec.anchor != null) return true;
        }
        return false;
      }
    }

    public OverlayHandle Show(in OverlaySpec spec) {
      if (_disposed) throw new ObjectDisposedException(nameof(OverlayController));
      var handle = new OverlayHandle(this, _nextId++, in spec);
      if (_root != null) Mount(handle);
      else {
        handle.IsPending = true;
        _pending.Add(handle);
      }
      return handle;
    }

    public OverlayHandle ShowPopover(
      VisualElement anchor,
      OverlayComposable content,
      OverlayOptions? options = null,
      Action<OverlayHandle, OverlayDismissReason> dismissed = null
    ) {
      var spec = new OverlaySpec(
        OverlayKind.Popover,
        content,
        anchor,
        options ?? OverlayOptions.Popover(),
        dismissed
      );
      return Show(in spec);
    }

    public OverlayHandle ShowModal(
      OverlayComposable content,
      OverlayOptions? options = null,
      Action<OverlayHandle, OverlayDismissReason> dismissed = null
    ) {
      var spec = new OverlaySpec(
        OverlayKind.Modal,
        content,
        options: options ?? OverlayOptions.Modal(),
        dismissed: dismissed
      );
      return Show(in spec);
    }

    public OverlayHandle ShowPopup(
      OverlayComposable content,
      OverlayOptions? options = null,
      Action<OverlayHandle, OverlayDismissReason> dismissed = null
    ) {
      var spec = new OverlaySpec(
        OverlayKind.Popup,
        content,
        options: options,
        dismissed: dismissed
      );
      return Show(in spec);
    }

    public OverlayHandle ShowNotification(
      OverlayComposable content,
      int durationMs = 4000,
      OverlayPlacement placement = OverlayPlacement.Bottom,
      Action<OverlayHandle, OverlayDismissReason> dismissed = null
    ) {
      var options = new OverlayOptions(
        placement,
        dismissOnCancel: false,
        flip: false,
        clampToHost: true,
        restoreFocus: false
      );
      var spec = new OverlaySpec(
        OverlayKind.Notification,
        content,
        options: options,
        dismissed: dismissed
      );
      var handle = Show(in spec);
      handle.TimeoutMs = durationMs;
      ScheduleTimeout(handle);
      return handle;
    }

    public bool Dismiss(
      OverlayHandle handle,
      OverlayDismissReason reason = OverlayDismissReason.Manual
    ) {
      if (handle == null || handle.DismissReason.HasValue) return false;
      var entry = handle.Entry;
      if (entry != null) {
        entry.timeout?.Pause();
        _entries.Remove(entry);
        _root?.RemoveEntry(entry);
      } else {
        _pending.Remove(handle);
      }

      handle.IsShown = false;
      handle.IsPending = false;
      handle.DismissReason = reason;
      handle.Entry = null;
      if (handle.Spec.options.restoreFocus && entry?.previousFocus != null) {
        entry.previousFocus.Focus();
      }
      handle.Spec.dismissed?.Invoke(handle, reason);
      handle.ReleaseController();
      RefreshPositions();
      return true;
    }

    public bool DismissTopCancelable() {
      for (var i = _entries.Count - 1; i >= 0; i--) {
        var handle = _entries[i].handle;
        if (!handle.Spec.options.dismissOnCancel) continue;
        return Dismiss(handle, OverlayDismissReason.Cancel);
      }
      return false;
    }

    public void DismissAll(OverlayDismissReason reason = OverlayDismissReason.Manual) {
      for (var i = _entries.Count - 1; i >= 0; i--) Dismiss(_entries[i].handle, reason);
      for (var i = _pending.Count - 1; i >= 0; i--) Dismiss(_pending[i], reason);
    }

    public void Dispose() {
      if (_disposed) return;
      _disposed = true;
      DismissAll(OverlayDismissReason.HostDetached);
      _root = null;
    }

    internal void Attach(OverlayRootElement root) {
      if (_root != null && !ReferenceEquals(_root, root)) {
        throw new InvalidOperationException(
          "An OverlayController can only be attached to one OverlayHost at a time."
        );
      }
      _root = root;
      if (_pending.Count == 0) return;
      for (var i = 0; i < _pending.Count; i++) Mount(_pending[i]);
      _pending.Clear();
    }

    internal void Detach(OverlayRootElement root, OverlayDismissReason reason) {
      if (!ReferenceEquals(_root, root)) return;
      DismissAll(reason);
      _root = null;
    }

    internal void RefreshPositions() {
      for (var i = 0; i < _entries.Count; i++) _root?.RefreshEntry(_entries[i].view);
    }

    internal void PollAnchors() {
      for (var i = _entries.Count - 1; i >= 0; i--) {
        var entry = _entries[i];
        var anchor = entry.handle.Spec.anchor;
        if (anchor == null) continue;
        if (anchor.panel == null || anchor.panel != _root?.panel) {
          Dismiss(entry.handle, OverlayDismissReason.AnchorDetached);
          continue;
        }
        _root.RefreshEntry(entry.view);
      }
    }

    internal void HandlePointerDown(PointerDownEvent evt) {
      var dismissedAny = false;
      for (var i = _entries.Count - 1; i >= 0; i--) {
        var entry = _entries[i];
        var options = entry.handle.Spec.options;
        var insideContent = entry.boundary.worldBound.Contains(evt.position);
        var insideAnchor = entry.handle.Spec.anchor?.worldBound.Contains(evt.position) == true;
        if (insideContent || insideAnchor) {
          if (dismissedAny) evt.StopPropagation();
          return;
        }

        var dismissOnBarrier =
          options.barrier.visible &&
          options.barrier.dismissOnPointerDown;
        if (!options.dismissOnOutsidePointer) {
          if (dismissOnBarrier) {
            Dismiss(entry.handle, OverlayDismissReason.OutsidePointer);
            dismissedAny = true;
            if (options.barrier.blocksPointer) {
              evt.StopPropagation();
              return;
            }
            continue;
          }
          if (options.modal || options.barrier.blocksPointer) {
            evt.StopPropagation();
            return;
          }
          continue;
        }

        Dismiss(entry.handle, OverlayDismissReason.OutsidePointer);
        dismissedAny = true;
        if (options.barrier.visible && options.barrier.blocksPointer) {
          evt.StopPropagation();
          return;
        }
      }
      if (dismissedAny) evt.StopPropagation();
    }

    internal Vector2 GetNotificationStackOffset(OverlayHandle handle) {
      var offset = 0f;
      for (var i = 0; i < _entries.Count; i++) {
        var entry = _entries[i];
        if (ReferenceEquals(entry.handle, handle)) break;
        if (entry.handle.Spec.kind != OverlayKind.Notification ||
            entry.handle.Spec.options.placement != handle.Spec.options.placement) {
          continue;
        }
        var height = entry.view.content?.layout.height ?? 0f;
        if (!float.IsNaN(height)) offset += height + 8f;
      }

      return handle.Spec.options.placement switch {
        OverlayPlacement.TopStart or OverlayPlacement.Top or OverlayPlacement.TopEnd =>
          new Vector2(0f, offset),
        _ => new Vector2(0f, -offset)
      };
    }

    private void Mount(OverlayHandle handle) {
      var view = new OverlayEntryView(_root, handle);
      if (handle.Spec.options.barrier.visible) {
        view.hierarchy.Add(new OverlayBarrierElement(handle, handle.Spec.options.barrier));
      }

      var boundary = new CompositionBoundaryNode {
        composable = handle.Composition,
        focusable = true
      };
      view.SetContent(boundary);
      var previousFocus = _root.panel?.focusController?.focusedElement;
      var entry = new OverlayEntry(handle, view, boundary, previousFocus);
      handle.Entry = entry;
      handle.IsShown = true;
      handle.IsPending = false;
      _entries.Add(entry);
      _root.AddEntry(entry);
      RecompositionScope.MarkDirty(boundary);
      if (handle.Spec.kind != OverlayKind.Notification) {
        boundary.schedule.Execute(boundary.Focus);
      }
      ScheduleTimeout(handle);
    }

    private void ScheduleTimeout(OverlayHandle handle) {
      if (handle.TimeoutMs <= 0 || handle.Entry == null || handle.Entry.timeout != null) return;
      var item = handle.Entry.view.schedule
        .Execute(() => Dismiss(handle, OverlayDismissReason.Timeout));
      handle.Entry.timeout = item;
      item.ExecuteLater(handle.TimeoutMs);
    }
  }

  public readonly struct OverlayContextData : IEquatable<OverlayContextData> {
    public static readonly ContextKey<OverlayContextData> Key = new("NW.OverlayContext");
    public readonly OverlayController controller;

    public OverlayContextData(OverlayController controller) {
      this.controller = controller;
    }

    public bool Equals(OverlayContextData other) => ReferenceEquals(controller, other.controller);
    public override bool Equals(object obj) => obj is OverlayContextData other && Equals(other);
    public override int GetHashCode() => controller?.GetHashCode() ?? 0;
  }

  public static class OverlayContextExtensions {
    public static OverlayController Overlays(this ref Composition cx, bool listen = true) {
      return cx.ReadContext(OverlayContextData.Key, listen).controller;
    }

    public static OverlayController RequireOverlays(this ref Composition cx, bool listen = true) {
      var controller = cx.Overlays(listen);
      if (controller == null) {
        throw new InvalidOperationException("This composition is not below an OverlayHost.");
      }
      return controller;
    }
  }

  public static partial class OverlayHostDefinition {
    private static readonly ushort _rootTypeId = CompositionId.GetTypeId("OverlayRoot");

    [CompositionBoundary]
    public static partial ref ElementRef OverlayHost(
      ref this Composition cx,
      [Prop] Composable<OverlayController> content,
      [Prop] OverlayController controller = null
    );

    public partial class OverlayHostComposable {
      private OverlayController _ownedController;
      private OverlayRootElement _root;

      protected override void OnAttach() {
        base.OnAttach();
        // OverlayHost is a composition boundary. It must participate in the
        // parent's flex layout before its retained overlay root can fill it.
        Node.style.flexGrow = 1f;
      }

      protected override void OnRecompose(ref Composition cx) {
        if (Props.Controller != null && _ownedController != null) {
          _ownedController.Dispose();
          _ownedController = null;
        }
        var controller = Props.Controller ?? (_ownedController ??= new OverlayController());

        if (!cx.AUTHORING.RequireComposable<OverlayRootElement>(_rootTypeId, out _root, out _)) {
          _root = new OverlayRootElement();
        }
        _root.Bind(controller);

        using (cx.AUTHORING.YieldScope(
          ref cx,
          _root,
          static (BoundaryCell cell, in ScopeHandle _) => cell.TrimChildren()
        )) {
          var context = new OverlayContextData(controller);
          using (WriteContextStable(ref cx, in context)) {
            Props.Content?.Invoke(ref cx, controller);
          }
        }
      }

      protected override void OnDetach() {
        _root?.Reset();
        _root = null;
        _ownedController?.Dispose();
        _ownedController = null;
        base.OnDetach();
      }

      private static ContextScope<OverlayContextData> WriteContextStable(
        ref Composition cx,
        in OverlayContextData value
      ) {
        var scope = cx.WritableContext(OverlayContextData.Key, out var data);
        if (!data.GetValueRef().Equals(value)) {
          data.GetValueRef() = value;
          data.IncrementContextVersion(ContextFlags.None);
        }
        return scope;
      }
    }
  }
}
