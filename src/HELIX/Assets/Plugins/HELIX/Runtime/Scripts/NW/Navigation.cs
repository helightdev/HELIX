using System;
using System.Collections.Generic;
using HELIX.Extensions;
using UnityEngine.UIElements;

namespace HELIX.NW.Navigation {
  public readonly struct RouteId : IEquatable<RouteId> {
    public readonly string value;

    public RouteId(string value) {
      if (string.IsNullOrWhiteSpace(value)) {
        throw new ArgumentException("A route id cannot be null or empty.", nameof(value));
      }
      this.value = value;
    }

    public bool Equals(RouteId other) {
      return string.Equals(value, other.value, StringComparison.Ordinal);
    }

    public override bool Equals(object obj) => obj is RouteId other && Equals(other);
    public override int GetHashCode() => value == null ? 0 : StringComparer.Ordinal.GetHashCode(value);
    public override string ToString() => value ?? string.Empty;
    public static bool operator ==(RouteId left, RouteId right) => left.Equals(right);
    public static bool operator !=(RouteId left, RouteId right) => !left.Equals(right);
    public static implicit operator RouteId(string value) => new(value);
  }

  public enum NavigationTransition : byte {
    None,
    Platform,
    Fade,
    SlideHorizontal,
    SlideVertical
  }

  public enum NavigationChangeKind : byte {
    Push,
    Pop,
    Replace,
    Reset
  }

  public readonly struct NavigationOptions : IEquatable<NavigationOptions> {
    public static readonly NavigationOptions Default = new(
      keepAlive: true,
      transition: NavigationTransition.None
    );

    public readonly bool keepAlive;
    public readonly NavigationTransition transition;

    public NavigationOptions(
      bool keepAlive = true,
      NavigationTransition transition = NavigationTransition.None
    ) {
      this.keepAlive = keepAlive;
      this.transition = transition;
    }

    public bool Equals(NavigationOptions other) {
      return keepAlive == other.keepAlive && transition == other.transition;
    }

    public override bool Equals(object obj) => obj is NavigationOptions other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(keepAlive, (int)transition);
  }

  public delegate void NavigationComposable(ref Composition cx, NavigationEntry entry);

  public abstract class NavigationRouteBase {
    protected NavigationRouteBase(RouteId id) {
      Id = id;
    }

    public RouteId Id { get; }
    internal abstract void Compose(ref Composition cx, NavigationEntry entry);
  }

  public sealed class NavigationRoute : NavigationRouteBase {
    private readonly Composable<NavigationEntry> _content;

    public NavigationRoute(RouteId id, Composable<NavigationEntry> content) : base(id) {
      _content = content ?? throw new ArgumentNullException(nameof(content));
    }

    internal override void Compose(ref Composition cx, NavigationEntry entry) {
      _content(ref cx, entry);
    }
  }

  public sealed class NavigationRoute<T> : NavigationRouteBase {
    private readonly Composable<T> _content;

    public NavigationRoute(RouteId id, Composable<T> content) : base(id) {
      _content = content ?? throw new ArgumentNullException(nameof(content));
    }

    internal override void Compose(ref Composition cx, NavigationEntry entry) {
      _content(ref cx, entry.GetArgument<T>());
    }
  }

  public sealed class NavigationMap {
    private readonly Dictionary<RouteId, NavigationRouteBase> _routes = new();

    public int Count => _routes.Count;

    public NavigationMap Register(NavigationRouteBase route) {
      if (route == null) throw new ArgumentNullException(nameof(route));
      _routes.Add(route.Id, route);
      return this;
    }

    public NavigationMap RegisterOrReplace(NavigationRouteBase route) {
      if (route == null) throw new ArgumentNullException(nameof(route));
      _routes[route.Id] = route;
      return this;
    }

    public bool TryGet(RouteId id, out NavigationRouteBase route) {
      return _routes.TryGetValue(id, out route);
    }

    public NavigationRouteBase Get(RouteId id) {
      if (!_routes.TryGetValue(id, out var route)) {
        throw new KeyNotFoundException($"No navigation route is registered for '{id}'.");
      }
      return route;
    }
  }

  public sealed class NavigationEntry {
    private readonly object _argument;

    internal NavigationEntry(
      int key,
      NavigationRouteBase route,
      object argument,
      in NavigationOptions options
    ) {
      Key = key;
      Route = route;
      _argument = argument;
      Options = options;
    }

    public int Key { get; }
    public RouteId Id => Route.Id;
    public NavigationRouteBase Route { get; }
    public NavigationOptions Options { get; }
    public object Argument => _argument;

    public T GetArgument<T>() {
      if (_argument is T value) return value;
      if (_argument == null && default(T) is null) return default;
      throw new InvalidCastException(
        $"Route '{Id}' expects an argument compatible with {typeof(T).FullName}."
      );
    }

    internal void Compose(ref Composition cx) {
      Route.Compose(ref cx, this);
    }
  }

  public readonly struct NavigationChange {
    public readonly NavigationChangeKind kind;
    public readonly NavigationEntry previous;
    public readonly NavigationEntry current;
    public readonly int version;

    internal NavigationChange(
      NavigationChangeKind kind,
      NavigationEntry previous,
      NavigationEntry current,
      int version
    ) {
      this.kind = kind;
      this.previous = previous;
      this.current = current;
      this.version = version;
    }
  }

  public sealed class NavigationController : IDisposable {
    private readonly List<NavigationEntry> _entries = new(4);
    private readonly NavigationMap _routes;
    private int _nextKey = 1;
    private bool _disposed;

    public NavigationController(NavigationMap routes = null) {
      _routes = routes;
    }

    public event Action<NavigationChange> Changed;

    public int Count => _entries.Count;
    public int Version { get; private set; }
    public bool CanPop => _entries.Count > 1;
    public NavigationEntry Current => _entries.Count == 0 ? null : _entries[^1];
    public IReadOnlyList<NavigationEntry> Entries => _entries;

    public NavigationEntry Push(
      NavigationRouteBase route,
      object argument = null,
      NavigationOptions? options = null
    ) {
      ThrowIfDisposed();
      if (route == null) throw new ArgumentNullException(nameof(route));
      var resolvedOptions = options ?? NavigationOptions.Default;
      var previous = Current;
      var entry = new NavigationEntry(_nextKey++, route, argument, in resolvedOptions);
      _entries.Add(entry);
      Notify(NavigationChangeKind.Push, previous, entry);
      return entry;
    }

    public NavigationEntry Push(
      RouteId route,
      object argument = null,
      NavigationOptions? options = null
    ) {
      if (_routes == null) {
        throw new InvalidOperationException("This controller has no NavigationMap.");
      }
      return Push(_routes.Get(route), argument, options);
    }

    public NavigationEntry Push<T>(
      NavigationRoute<T> route,
      T argument,
      NavigationOptions? options = null
    ) {
      return Push((NavigationRouteBase)route, argument, options);
    }

    public bool Pop() {
      ThrowIfDisposed();
      if (!CanPop) return false;
      var previous = Current;
      _entries.RemoveAt(_entries.Count - 1);
      Notify(NavigationChangeKind.Pop, previous, Current);
      return true;
    }

    public bool PopTo(RouteId id) {
      ThrowIfDisposed();
      var index = -1;
      for (var i = _entries.Count - 1; i >= 0; i--) {
        if (_entries[i].Id != id) continue;
        index = i;
        break;
      }
      if (index < 0 || index == _entries.Count - 1) return false;
      var previous = Current;
      _entries.RemoveRange(index + 1, _entries.Count - index - 1);
      Notify(NavigationChangeKind.Pop, previous, Current);
      return true;
    }

    public bool PopToRoot() {
      ThrowIfDisposed();
      if (_entries.Count <= 1) return false;
      var previous = Current;
      _entries.RemoveRange(1, _entries.Count - 1);
      Notify(NavigationChangeKind.Pop, previous, Current);
      return true;
    }

    public NavigationEntry Replace(
      NavigationRouteBase route,
      object argument = null,
      NavigationOptions? options = null
    ) {
      ThrowIfDisposed();
      if (route == null) throw new ArgumentNullException(nameof(route));
      var resolvedOptions = options ?? NavigationOptions.Default;
      var previous = Current;
      var entry = new NavigationEntry(_nextKey++, route, argument, in resolvedOptions);
      if (_entries.Count == 0) _entries.Add(entry);
      else _entries[^1] = entry;
      Notify(NavigationChangeKind.Replace, previous, entry);
      return entry;
    }

    public NavigationEntry Reset(
      NavigationRouteBase route,
      object argument = null,
      NavigationOptions? options = null
    ) {
      ThrowIfDisposed();
      if (route == null) throw new ArgumentNullException(nameof(route));
      var resolvedOptions = options ?? NavigationOptions.Default;
      var previous = Current;
      var entry = new NavigationEntry(_nextKey++, route, argument, in resolvedOptions);
      _entries.Clear();
      _entries.Add(entry);
      Notify(NavigationChangeKind.Reset, previous, entry);
      return entry;
    }

    public void Dispose() {
      if (_disposed) return;
      _disposed = true;
      _entries.Clear();
      Changed = null;
    }

    private void Notify(
      NavigationChangeKind kind,
      NavigationEntry previous,
      NavigationEntry current
    ) {
      Version++;
      Changed?.Invoke(new NavigationChange(kind, previous, current, Version));
    }

    private void ThrowIfDisposed() {
      if (_disposed) throw new ObjectDisposedException(nameof(NavigationController));
    }
  }

  public readonly struct NavigationContextData : IEquatable<NavigationContextData> {
    public static readonly ContextKey<NavigationContextData> Key = new("NW.NavigationContext");

    public readonly NavigationController controller;
    public readonly NavigationEntry entry;

    public NavigationContextData(NavigationController controller, NavigationEntry entry) {
      this.controller = controller;
      this.entry = entry;
    }

    public bool Equals(NavigationContextData other) {
      return ReferenceEquals(controller, other.controller) && ReferenceEquals(entry, other.entry);
    }

    public override bool Equals(object obj) {
      return obj is NavigationContextData other && Equals(other);
    }

    public override int GetHashCode() => HashCode.Combine(controller, entry);
  }

  public static class NavigationContextExtensions {
    public static NavigationController NavigationController(this ref Composition cx) {
      return cx.ReadContext(NavigationContextData.Key).controller;
    }

    public static NavigationEntry NavigationEntry(this ref Composition cx) {
      return cx.ReadContext(NavigationContextData.Key).entry;
    }

    public static T NavigationArgument<T>(this ref Composition cx) {
      var entry = cx.ReadContext(NavigationContextData.Key).entry;
      if (entry == null) throw new InvalidOperationException("There is no current navigation entry.");
      return entry.GetArgument<T>();
    }
  }

  internal static class NavigationContextWriter {
    public static ContextScope<NavigationContextData> WriteStable(
      ref Composition cx,
      in NavigationContextData value
    ) {
      var scope = cx.WritableContext(NavigationContextData.Key, out var data);
      if (!data.GetValueRef().Equals(value)) {
        data.GetValueRef() = value;
        data.IncrementContextVersion(ContextFlags.None);
      }
      return scope;
    }
  }

  public static partial class NavigationHostDefinition {
    [CompositionBoundary]
    public static partial ref ElementRef NavigationHost(
      ref this Composition cx,
      [Prop] NavigationController controller = null,
      [Prop] Composable<NavigationController> empty = null,
      [Prop] NavigationRouteBase initialRoute = null,
      [Prop] object initialArgument = null,
      [Prop] NavigationOptions? initialOptions = null
    );

    public partial class NavigationHostComposable{
      private readonly Action<NavigationChange> _changed;
      private NavigationController _controller;
      private NavigationController _ownedController;
      private int _lastKeyboardCancelFrame = -1;

      public NavigationHostComposable() {
        _changed = HandleChanged;
      }

      protected override void OnAttach() {
        base.OnAttach();
        Node.RegisterCallback<NavigationCancelEvent>(HandleCancel);
        Node.RegisterCallback<KeyDownEvent>(HandleKeyDown, TrickleDown.TrickleDown);
      }

      protected override void OnDetach() {
        if (_controller != null) _controller.Changed -= _changed;
        _controller = null;
        _ownedController?.Dispose();
        _ownedController = null;
        Node.UnregisterCallback<NavigationCancelEvent>(HandleCancel);
        Node.UnregisterCallback<KeyDownEvent>(HandleKeyDown, TrickleDown.TrickleDown);
        base.OnDetach();
      }

      protected override void OnRecompose(ref Composition cx) {
        if (Props.Controller != null && _ownedController != null) {
          _ownedController.Dispose();
          _ownedController = null;
        }
        var controller = Props.Controller ?? (_ownedController ??= new NavigationController());
        if (!ReferenceEquals(_controller, controller)) {
          if (_controller != null) _controller.Changed -= _changed;
          _controller = controller;
          if (_controller != null) _controller.Changed += _changed;
        }

        Node.style.position = Position.Relative;
        Node.style.flexGrow = 1f;
        Node.style.overflow = Overflow.Hidden;
        Node.focusable = true;
        Node.Flag |= UssFlag.Position | UssFlag.Flex | UssFlag.Clipping | UssFlag.Focus;

        if (_controller.Count == 0 && Props.InitialRoute != null) {
          _controller.Push(
            Props.InitialRoute,
            Props.InitialArgument,
            Props.InitialOptions
          );
        }

        if (_controller.Count == 0) {
          Props.Empty?.Invoke(ref cx, _controller);
          return;
        }

        var entries = _controller.Entries;
        var last = entries.Count - 1;
        for (var i = 0; i < entries.Count; i++) {
          cx.NavigationPage(_controller, entries[i], i == last);
        }
      }

      private void HandleChanged(NavigationChange change) {
        if (Node != null) Node.MarkDirty();
      }

      private void HandleCancel(NavigationCancelEvent evt) {
        if (_lastKeyboardCancelFrame == UnityEngine.Time.frameCount) {
          evt.StopPropagation();
          return;
        }
        if (_controller?.Pop() != true) return;
        evt.StopPropagation();
      }

      private void HandleKeyDown(KeyDownEvent evt) {
        if (evt.keyCode != UnityEngine.KeyCode.Escape || _controller?.Pop() != true) return;
        _lastKeyboardCancelFrame = UnityEngine.Time.frameCount;
        evt.StopPropagation();
      }
    }
  }

  public static partial class NavigationPageDefinition {
    [CompositionBoundary]
    public static partial ref ElementRef NavigationPage(
      ref this Composition cx,
      [Prop] NavigationController controller,
      [Prop] NavigationEntry entry,
      [Prop] bool visible
    );

    public partial class NavigationPageComposable {
      private NavigationEntry _composedEntry;

      protected override void OnRecompose(ref Composition cx) {
        Node.Stretched().Visible(Props.Visible);
        //Node.SetEnabled(Props.Visible);
        Node.Flag |= UssFlag.Position | UssFlag.Visibility;

        if (Props.Entry == null) {
          _composedEntry = null;
          return;
        }
        if (!Props.Visible) {
          if (!Props.Entry.Options.keepAlive) {
            _composedEntry = null;
            return;
          }
          if (ReferenceEquals(_composedEntry, Props.Entry)) {
            var retainedContext = new NavigationContextData(Props.Controller, Props.Entry);
            using (NavigationContextWriter.WriteStable(ref cx, in retainedContext)) {
              cx.AUTHORING.RetainChildren();
            }
            return;
          }
        }

        var context = new NavigationContextData(Props.Controller, Props.Entry);
        using (NavigationContextWriter.WriteStable(ref cx, in context)) {
          Props.Entry.Compose(ref cx);
        }
        _composedEntry = Props.Entry;
      }
    }
  }
}
