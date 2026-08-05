using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HELIX.Extensions;
using HELIX.Signals;
using Unity.Burst;
using UnityEngine.UIElements;
// ReSharper disable Unity.BurstLoadingManagedType
// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace HELIX.Compose {
  public sealed class NavigationArguments {
    public static readonly NavigationArguments Empty = new();

    private readonly Dictionary<string, object> _values;

    public NavigationArguments() {
      _values = new Dictionary<string, object>(StringComparer.Ordinal);
    }

    private NavigationArguments(Dictionary<string, object> values) {
      _values = values;
    }

    public int Count => _values.Count;
    public object this[string key] => _values[key];

    public NavigationArguments With(string key, object value) {
      if (string.IsNullOrEmpty(key)) throw new ArgumentException("An argument name is required.", nameof(key));
      var copy = new Dictionary<string, object>(_values, StringComparer.Ordinal) { [key] = value };
      return new NavigationArguments(copy);
    }

    public bool TryGet<T>(string key, out T value) {
      if (_values.TryGetValue(key, out var found) && found is T typed) {
        value = typed;
        return true;
      }
      value = default;
      return false;
    }

    public T Get<T>(string key, T fallback = default) => TryGet<T>(key, out var value) ? value : fallback;

    internal NavigationArguments Copy() => _values.Count == 0
      ? Empty
      : new NavigationArguments(new Dictionary<string, object>(_values, StringComparer.Ordinal));
  }


  public readonly struct NavigationContextData : IEquatable<NavigationContextData> {
    public static readonly ContextKey<NavigationContextData> Key = new("NavigationContext");

    public readonly NavigationController controller;
    public readonly NavigationEntry entry;

    internal NavigationContextData(NavigationController controller, NavigationEntry entry) {
      this.controller = controller;
      this.entry = entry;
    }

    public string Route => entry?.Name;
    public NavigationArguments Arguments => entry?.Arguments ?? NavigationArguments.Empty;
    public bool CanPop => controller?.CanPop ?? false;

    public bool Equals(NavigationContextData other) =>
      ReferenceEquals(controller, other.controller) && ReferenceEquals(entry, other.entry);

    public override bool Equals(object obj) => obj is NavigationContextData other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(controller, entry);
  }

  public sealed class NavigationRoute {
    internal NavigationRoute(string name, Composable<NavigationContextData> builder, bool opaque) {
      Name = name;
      Builder = builder;
      Opaque = opaque;
    }

    public string Name { get; }
    public Composable<NavigationContextData> Builder { get; }
    public bool Opaque { get; }
  }

  public sealed class NavigationGraph {
    private readonly Dictionary<string, NavigationRoute> _routes;

    internal NavigationGraph(
      string initialRoute,
      Dictionary<string, NavigationRoute> routes
    ) {
      InitialRoute = initialRoute;
      _routes = routes;
    }

    public string InitialRoute { get; }
    public int Count => _routes.Count;

    public static NavigationGraphBuilder Builder(string initialRoute) => new(initialRoute);

    public bool TryGetRoute(string name, out NavigationRoute route) {
      if (name == null) {
        route = null;
        return false;
      }
      return _routes.TryGetValue(name, out route);
    }

    public NavigationRoute GetRoute(string name) => TryGetRoute(name, out var route) ? route : null;
  }

  public sealed class NavigationGraphBuilder {
    private readonly Dictionary<string, NavigationRoute> _routes =
      new(StringComparer.Ordinal);
    private string _initialRoute;

    internal NavigationGraphBuilder(string initialRoute) {
      _initialRoute = initialRoute;
    }

    public NavigationGraphBuilder InitialRoute(string route) {
      _initialRoute = route;
      return this;
    }

    public NavigationGraphBuilder Route(
      string route,
      Composable<NavigationContextData> builder,
      bool opaque = true
    ) {
      if (string.IsNullOrWhiteSpace(route)) throw new ArgumentException("A route is required.", nameof(route));
      if (builder == null) throw new ArgumentNullException(nameof(builder));
      if (_routes.ContainsKey(route)) {
        throw new InvalidOperationException($"The navigation route '{route}' is already registered.");
      }
      _routes.Add(route, new NavigationRoute(route, builder, opaque));
      return this;
    }

    public NavigationGraph Build() {
      if (_routes.Count == 0) throw new InvalidOperationException("A navigation graph needs a route.");
      if (string.IsNullOrWhiteSpace(_initialRoute)) {
        throw new InvalidOperationException("A navigation graph needs an initial route.");
      }
      if (!_routes.ContainsKey(_initialRoute)) {
        throw new InvalidOperationException($"The initial route '{_initialRoute}' is not registered.");
      }
      return new NavigationGraph(
        _initialRoute,
        new Dictionary<string, NavigationRoute>(_routes, StringComparer.Ordinal)
      );
    }
  }

  [PropStruct] public readonly partial struct NavigationOptions : IEquatable<NavigationOptions> {
    public static readonly NavigationOptions Default = new();

    [Prop(false)] public readonly bool singleTop;
    [Prop(false)] public readonly bool clearStack;
    [Prop(null)] public readonly string popUpToRoute;
    [Prop(false)] public readonly bool popUpToInclusive;

    public NavigationOptions SingleTop(bool enabled = true) {
      return new NavigationOptions(enabled, clearStack, popUpToRoute, popUpToInclusive);
    }

    public NavigationOptions ClearStack(bool enabled = true) {
      return new NavigationOptions(singleTop, enabled, popUpToRoute, popUpToInclusive);
    }

    public NavigationOptions PopUpTo(string route, bool inclusive = false) {
      return new NavigationOptions(singleTop, clearStack, route, inclusive);
    }
  }

  public sealed class NavigationEntry {
    internal NavigationEntry(long id, NavigationRoute route, NavigationArguments arguments) {
      Id = id;
      Route = route;
      Arguments = arguments ?? NavigationArguments.Empty;
    }

    public long Id { get; }
    public string Name => Route.Name;
    public NavigationRoute Route { get; }
    public NavigationArguments Arguments { get; }
  }

  public sealed class NavigationController : Signal<int> {
    private readonly List<NavigationEntry> _backStack = new();
    private int _revision;
    private long _nextEntryId = 1;

    public NavigationController(NavigationGraph graph = null)
      : base("NavigationController", typeof(NavigationController)) {
      SetGraph(graph);
    }

    public NavigationGraph Graph { get; private set; }
    public IReadOnlyList<NavigationEntry> BackStack => _backStack;
    public NavigationEntry Current => _backStack.Count == 0 ? null : _backStack[_backStack.Count - 1];
    public bool CanPop => _backStack.Count > 1;

    public override int PeekValue() => _revision;

    public override void SetValue(int newValue) =>
      throw new NotSupportedException("Navigation state is changed with navigation operations.");

    public override void SetWithoutNotify(int newValue) =>
      throw new NotSupportedException("Navigation state is changed with navigation operations.");

    public void SetGraph(NavigationGraph graph, bool preserveStack = false) {
      if (ReferenceEquals(Graph, graph)) return;
      Graph = graph;
      if (preserveStack && _backStack.Count > 0 && IsStackValidFor(graph)) {
        RemapStack(graph);
        NotifyChanged();
        return;
      }

      _backStack.Clear();
      if (graph != null && graph.TryGetRoute(graph.InitialRoute, out var initial)) {
        _backStack.Add(CreateEntry(initial, NavigationArguments.Empty));
      }
      NotifyChanged();
    }

    public bool Navigate(
      string route,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) {
      if (Graph == null || !Graph.TryGetRoute(route, out var destination)) return false;
      var resolvedOptions = options ?? NavigationOptions.Default;

      if (resolvedOptions.clearStack) _backStack.Clear();
      if (!string.IsNullOrEmpty(resolvedOptions.popUpToRoute)) {
        PopToInternal(resolvedOptions.popUpToRoute, resolvedOptions.popUpToInclusive);
      }

      var copiedArguments = (arguments ?? NavigationArguments.Empty).Copy();
      if (resolvedOptions.singleTop && Current?.Name == route) {
        var current = Current;
        _backStack[_backStack.Count - 1] = new NavigationEntry(current.Id, destination, copiedArguments);
      } else {
        _backStack.Add(CreateEntry(destination, copiedArguments));
      }

      NotifyChanged();
      return true;
    }

    public bool Replace(string route, NavigationArguments arguments = null) {
      if (Graph == null || !Graph.TryGetRoute(route, out var destination)) return false;
      if (_backStack.Count > 0) _backStack.RemoveAt(_backStack.Count - 1);
      _backStack.Add(CreateEntry(destination, (arguments ?? NavigationArguments.Empty).Copy()));
      NotifyChanged();
      return true;
    }

    public bool Pop() {
      if (!CanPop) return false;
      _backStack.RemoveAt(_backStack.Count - 1);
      NotifyChanged();
      return true;
    }

    public bool PopTo(string route, bool inclusive = false) {
      if (!PopToInternal(route, inclusive)) return false;
      if (_backStack.Count == 0 && Graph?.TryGetRoute(Graph.InitialRoute, out var initial) == true) {
        _backStack.Add(CreateEntry(initial, NavigationArguments.Empty));
      }
      NotifyChanged();
      return true;
    }

    public bool Reset(string route = null, NavigationArguments arguments = null) {
      route ??= Graph?.InitialRoute;
      if (Graph == null || !Graph.TryGetRoute(route, out var destination)) return false;
      _backStack.Clear();
      _backStack.Add(CreateEntry(destination, (arguments ?? NavigationArguments.Empty).Copy()));
      NotifyChanged();
      return true;
    }

    public override void Dispose() {
      _backStack.Clear();
      Graph = null;
      base.Dispose();
    }

    private NavigationEntry CreateEntry(NavigationRoute route, NavigationArguments arguments) =>
      new(_nextEntryId++, route, arguments);

    private bool PopToInternal(string route, bool inclusive) {
      var index = -1;
      for (var i = _backStack.Count - 1; i >= 0; i--) {
        if (_backStack[i].Name != route) continue;
        index = i;
        break;
      }
      if (index < 0) return false;
      var keepCount = inclusive ? index : index + 1;
      if (keepCount == _backStack.Count) return false;
      _backStack.RemoveRange(keepCount, _backStack.Count - keepCount);
      return true;
    }

    private bool IsStackValidFor(NavigationGraph graph) {
      if (graph == null) return _backStack.Count == 0;
      foreach (var entry in _backStack) {
        if (!graph.TryGetRoute(entry.Name, out _)) return false;
      }
      return true;
    }

    private void RemapStack(NavigationGraph graph) {
      for (var i = 0; i < _backStack.Count; i++) {
        var entry = _backStack[i];
        if (!graph.TryGetRoute(entry.Name, out var destination)) continue;
        _backStack[i] = new NavigationEntry(entry.Id, destination, entry.Arguments);
      }
    }

    private void NotifyChanged() {
      unchecked { _revision++; }
      NotifyDirty();
      NotifyObservers();
    }
  }

  [BoundaryComposable(Extension = false)]
  internal partial class NavigationPageBoundary {
    public partial struct Props {
      public NavigationController controller;
      public NavigationEntry entry;
      public bool covered;
    }

    internal NavigationEntry Entry => props.entry;

    protected override void OnRecompose(ref Composition cx) {
      Node.Stretched();
      Node.pickingMode = PickingMode.Ignore;
      Node.style.display = props.covered ? DisplayStyle.None : DisplayStyle.Flex;

      var contextData = new NavigationContextData(props.controller, props.entry);
      using (cx.WriteContext(out var context)) {
        NavigationContextData.Key[in context] = contextData;
      }
      props.entry?.Route.Builder?.Invoke(ref cx, contextData);
    }
  }

  [BoundaryComposable(Extension = false)]
  public partial class NavigationHostBoundary {
    public partial struct Props {
      public NavigationGraph graph;
      [Prop(null)] public NavigationController controller;
    }

    public NavigationController Controller { get; private set; }
    private bool _isAutomaticController;

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
    }

    protected override void OnDetach() {
      Node.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
      if (_isAutomaticController) Controller?.Dispose();
      Controller = null;
      _isAutomaticController = false;
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      EnsureController();
      cx.SubscribeTo(Controller);
      var contextData = new NavigationContextData(Controller, null);
      using (cx.WriteContext(out var context)) {
        NavigationContextData.Key[in context] = contextData;
      }
      Node.MakeRelative();
      Node.focusable = true;
      Node.pickingMode = PickingMode.Ignore;

      var stack = Controller.BackStack;
      for (var i = 0; i < stack.Count; i++) {
        var entry = stack[i];
        var identity = unchecked((int)entry.Id ^ (int)(entry.Id >> 32));
        cx.AUTHORING.SetId(CompositionId.Generated(identity));
        NavigationPageBoundary.ComposeBoundary(ref cx, Controller, entry, IsCovered(stack, i));
      }
    }

    private void EnsureController() {
      if (props.controller == null) {
        if (Controller == null || !_isAutomaticController) {
          if (_isAutomaticController) Controller?.Dispose();
          Controller = new NavigationController(props.graph);
          _isAutomaticController = true;
        } else {
          Controller.SetGraph(props.graph);
        }
        return;
      }

      if (!ReferenceEquals(Controller, props.controller)) {
        if (_isAutomaticController) Controller?.Dispose();
        Controller = props.controller;
        _isAutomaticController = false;
      }
      Controller.SetGraph(props.graph, preserveStack: true);
    }

    private void OnNavigationCancel(NavigationCancelEvent evt) {
      if (Controller?.Pop() != true) return;
      evt.StopPropagation();
    }

    private static bool IsCovered(IReadOnlyList<NavigationEntry> stack, int index) {
      for (var i = stack.Count - 1; i > index; i--) {
        if (stack[i].Route.Opaque) return true;
      }
      return false;
    }
  }

  public static class NavigationExtensions {
    public static ref ElementRef NavigationHost(
      this ref Composition cx,
      NavigationGraph graph,
      NavigationController controller = null
    ) {
      if (graph == null) throw new ArgumentNullException(nameof(graph));
      return ref NavigationHostBoundary.ComposeBoundary(ref cx, graph, controller);
    }

    public static ref ElementRef NavigationHost(
      this ref Composition cx,
      NavigationGraph graph,
      out NavigationController resolvedController,
      NavigationController controller = null
    ) {
      ref var result = ref NavigationHostBoundary.ComposeBoundary(ref cx, graph, controller);
      resolvedController = (result.element as CompositionBoundaryNodeBase)?.BoundaryComposable
        is NavigationHostBoundary boundary
          ? boundary.Controller
          : controller;
      return ref result;
    }

    public static NavigationController NavigationController(this ref Composition cx, bool listen = true) =>
      cx.ReadContext(NavigationContextData.Key, listen).controller;

    public static NavigationEntry NavigationEntry(this ref Composition cx, bool listen = true) =>
      cx.ReadContext(NavigationContextData.Key, listen).entry;

    public static NavigationController NavigationController(this CompositionContext context) =>
      NavigationContextData.Key.ReadAt(context.element).controller ??
      context.Lookup<NavigationHostBoundary>()?.Controller;

    public static NavigationEntry NavigationEntry(this CompositionContext context) =>
      NavigationContextData.Key.ReadAt(context.element).entry ??
      context.Lookup<NavigationPageBoundary>()?.Entry;
  }
}