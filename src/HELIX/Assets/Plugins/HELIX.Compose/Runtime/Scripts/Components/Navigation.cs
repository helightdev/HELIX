using System;
using System.Collections.Generic;
using UnityEngine;

namespace HELIX.Compose {
  public sealed class NavigationArguments {
    public static readonly NavigationArguments Empty = new();

    private readonly Dictionary<string, object> _values;

    public NavigationArguments() : this(new Dictionary<string, object>(StringComparer.Ordinal), null) { }

    private NavigationArguments(Dictionary<string, object> values, object extra) {
      _values = values;
      Extra = extra;
    }

    public int Count => _values.Count;
    public object Extra { get; }
    public object this[string key] => _values[key];

    public NavigationArguments With(string key, object value) {
      if (string.IsNullOrEmpty(key)) throw new ArgumentException("An argument name is required.", nameof(key));
      var values = new Dictionary<string, object>(_values, StringComparer.Ordinal) { [key] = value };
      return new NavigationArguments(values, Extra);
    }

    public NavigationArguments WithExtra(object extra) {
      return new NavigationArguments(_values, extra);
    }

    public bool TryGet<T>(string key, out T value) {
      if (_values.TryGetValue(key, out var found) && found is T typed) {
        value = typed;
        return true;
      }
      value = default;
      return false;
    }

    public T Get<T>(string key, T fallback = default) {
      return TryGet<T>(key, out var value) ? value : fallback;
    }

    public T GetExtra<T>(T fallback = default) {
      return Extra is T value ? value : fallback;
    }
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

    public bool Equals(NavigationContextData other) {
      return ReferenceEquals(controller, other.controller) && ReferenceEquals(entry, other.entry);
    }

    public override bool Equals(object obj) {
      return obj is NavigationContextData other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(controller, entry);
    }
  }

  public enum NavigationEntryOrigin : byte { Registered, Dynamic }

  public enum NavigationOperationKind : byte {
    Push,
    Replace,
    Pop,
    PopTo,
    Reset,
    Go,
    Activate,
    Preload
  }

  public enum NavigationLifecyclePhase : byte { Entering, Entered, Exiting, Exited }

  public enum NavigationDirection : sbyte { Backward = -1, Automatic = 0, Forward = 1 }

  [Flags]
  public enum NavigationHostBehavior : byte { None = 0, PopOnCancel = 1 << 0, Default = PopOnCancel }

  [Flags]
  public enum NavigationBehavior : byte { None = 0, SingleTop = 1 << 0, ClearStack = 1 << 1, PopUpToInclusive = 1 << 2 }

  [Flags]
  public enum NavigationPresentation : byte {
    None = 0, Current = 1 << 0, Covered = 1 << 1, Entering = 1 << 2, Exiting = 1 << 3, Cached = 1 << 4
  }

  public enum NavigationFailure : byte {
    None,
    ControllerDisposed,
    QueueFull,
    RouteNotFound,
    InvalidPage,
    CannotPop,
    TargetNotFound,
    ResultUnavailable
  }

  public readonly struct NavigationSubmission {
    internal NavigationSubmission(long operationId, NavigationFailure failure) {
      OperationId = operationId;
      Failure = failure;
    }

    public long OperationId { get; }
    public NavigationFailure Failure { get; }
    public bool Accepted => Failure == NavigationFailure.None;

    public static implicit operator bool(NavigationSubmission value) {
      return value.Accepted;
    }
  }

  public readonly struct NavigationEvent {
    internal NavigationEvent(
      NavigationController controller,
      NavigationEntry entry,
      NavigationOperationKind operation,
      NavigationDirection direction,
      NavigationLifecyclePhase phase
    ) {
      Controller = controller;
      Entry = entry;
      Operation = operation;
      Direction = direction;
      Phase = phase;
    }

    public NavigationController Controller { get; }
    public NavigationEntry Entry { get; }
    public NavigationOperationKind Operation { get; }
    public NavigationDirection Direction { get; }
    public NavigationLifecyclePhase Phase { get; }
  }

  public sealed class NavigationPage {
    internal NavigationPage(
      string name,
      Composable<NavigationContextData> builder,
      bool opaque,
      INavigationTransition transition,
      CompositionAction<NavigationEvent> onEntering,
      CompositionAction<NavigationEvent> onEntered,
      CompositionAction<NavigationEvent> onExiting,
      CompositionAction<NavigationEvent> onExited
    ) {
      Name = name;
      Builder = builder;
      Opaque = opaque;
      Transition = transition;
      OnEntering = onEntering;
      OnEntered = onEntered;
      OnExiting = onExiting;
      OnExited = onExited;
    }

    public string Name { get; }
    public Composable<NavigationContextData> Builder { get; }
    public bool Opaque { get; }
    public INavigationTransition Transition { get; }
    internal CompositionAction<NavigationEvent> OnEntering { get; }
    internal CompositionAction<NavigationEvent> OnEntered { get; }
    internal CompositionAction<NavigationEvent> OnExiting { get; }
    internal CompositionAction<NavigationEvent> OnExited { get; }

    public static NavigationPageBuilder Build(Composable<NavigationContextData> builder) {
      return new NavigationPageBuilder(builder);
    }
  }

  public sealed class NavigationPageBuilder {
    private readonly Composable<NavigationContextData> _builder;
    private string _name;
    private CompositionAction<NavigationEvent> _onEntered;
    private CompositionAction<NavigationEvent> _onEntering;
    private CompositionAction<NavigationEvent> _onExited;
    private CompositionAction<NavigationEvent> _onExiting;
    private bool _opaque = true;
    private INavigationTransition _transition;

    internal NavigationPageBuilder(Composable<NavigationContextData> builder) {
      _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public NavigationPageBuilder Name(string name) {
      _name = name;
      return this;
    }

    public NavigationPageBuilder Opaque(bool opaque = true) {
      _opaque = opaque;
      return this;
    }

    public NavigationPageBuilder Transition(INavigationTransition transition) {
      _transition = transition;
      return this;
    }

    public NavigationPageBuilder OnEntering(CompositionAction<NavigationEvent> action) {
      _onEntering = action;
      return this;
    }

    public NavigationPageBuilder OnEntered(CompositionAction<NavigationEvent> action) {
      _onEntered = action;
      return this;
    }

    public NavigationPageBuilder OnExiting(CompositionAction<NavigationEvent> action) {
      _onExiting = action;
      return this;
    }

    public NavigationPageBuilder OnExited(CompositionAction<NavigationEvent> action) {
      _onExited = action;
      return this;
    }

    public NavigationPage Build() {
      return new NavigationPage(
        _name,
        _builder,
        _opaque,
        _transition,
        _onEntering,
        _onEntered,
        _onExiting,
        _onExited
      );
    }
  }

  public sealed class NavigationRoute {
    internal NavigationRoute(string name, NavigationPage page, int index) {
      Name = name;
      Page = page;
      Index = index;
    }

    public string Name { get; }
    public string DisplayName => string.IsNullOrWhiteSpace(Page.Name) ? Name : Page.Name;
    public int Index { get; }
    public NavigationPage Page { get; }
    public Composable<NavigationContextData> Builder => Page.Builder;
    public bool Opaque => Page.Opaque;
    public INavigationTransition Transition => Page.Transition;
  }

  public sealed class NavigationGraph {
    private readonly Dictionary<string, NavigationRoute> _routes;

    internal NavigationGraph(
      string initialRoute,
      NavigationRoute[] orderedRoutes,
      Dictionary<string, NavigationRoute> routes
    ) {
      InitialRoute = initialRoute;
      Routes = Array.AsReadOnly(orderedRoutes);
      _routes = routes;
    }

    public string InitialRoute { get; }
    public int Count => Routes.Count;
    public IReadOnlyList<NavigationRoute> Routes { get; }
    public NavigationRoute this[int index] => Routes[index];

    public static NavigationGraphBuilder Builder(string initialRoute) {
      return new NavigationGraphBuilder(initialRoute);
    }

    public bool TryGetRoute(string name, out NavigationRoute route) {
      route = null;
      return name != null && _routes.TryGetValue(name, out route);
    }

    public NavigationRoute GetRoute(string name) {
      return TryGetRoute(name, out var route) ? route : null;
    }

    public int IndexOf(string name) {
      return TryGetRoute(name, out var route) ? route.Index : -1;
    }

    public bool Contains(NavigationRoute route) {
      return route != null &&
        ReferenceEquals(GetRoute(route.Name), route);
    }
  }

  public sealed class NavigationGraphBuilder {
    private readonly List<NavigationRoute> _orderedRoutes = new();
    private readonly Dictionary<string, NavigationRoute> _routes = new(StringComparer.Ordinal);

    internal NavigationGraphBuilder(string initialRoute) {
      InitialRoutePath = initialRoute;
    }

    public int Count => _orderedRoutes.Count;
    public bool IsInitialRouteSet => _routes.ContainsKey(InitialRoutePath);
    public string InitialRoutePath { get; private set; }

    public NavigationGraphBuilder InitialRoute(string route) {
      InitialRoutePath = route;
      return this;
    }

    public NavigationGraphBuilder Route(
      string route,
      Composable<NavigationContextData> builder,
      bool opaque = true
    ) {
      return Route(route, NavigationPage.Build(builder).Name(route).Opaque(opaque).Build());
    }

    public NavigationGraphBuilder Route(string route, NavigationPageBuilder page) {
      return Route(route, page?.Build() ?? throw new ArgumentNullException(nameof(page)));
    }

    public NavigationGraphBuilder Route(string route, NavigationPage page) {
      if (string.IsNullOrWhiteSpace(route)) throw new ArgumentException("A route is required.", nameof(route));
      if (page == null) throw new ArgumentNullException(nameof(page));
      if (_routes.ContainsKey(route))
        throw new InvalidOperationException($"The navigation route '{route}' is already registered.");
      var destination = new NavigationRoute(route, page, _orderedRoutes.Count);
      _routes.Add(route, destination);
      _orderedRoutes.Add(destination);
      return this;
    }

    public NavigationGraph Build() {
      if (_routes.Count == 0) throw new InvalidOperationException("A navigation graph needs a route.");
      if (string.IsNullOrWhiteSpace(InitialRoutePath))
        throw new InvalidOperationException("A navigation graph needs an initial route.");
      if (!_routes.ContainsKey(InitialRoutePath))
        throw new InvalidOperationException($"The initial route '{InitialRoutePath}' is not registered.");
      return new NavigationGraph(
        InitialRoutePath,
        _orderedRoutes.ToArray(),
        new Dictionary<string, NavigationRoute>(_routes, StringComparer.Ordinal)
      );
    }
  }

  [EnableMixins, Structure]
  public readonly partial struct NavigationOptions : IEquatable<NavigationOptions> {
    public static readonly NavigationOptions Default = new(NavigationBehavior.None);

    [Prop(NavigationBehavior.None)] public readonly NavigationBehavior behavior;
    [Prop(null)] public readonly string popUpToRoute;
    [Prop(null, Equatable = false)] public readonly INavigationTransition transition;
    [Prop(NavigationDirection.Automatic)] public readonly NavigationDirection direction;

    public bool Has(NavigationBehavior value) {
      return (behavior & value) == value;
    }

    public NavigationOptions Behavior(NavigationBehavior value, bool enabled = true) {
      return new NavigationOptions(
        enabled ? behavior | value : behavior & ~value,
        popUpToRoute,
        transition,
        direction
      );
    }

    public NavigationOptions SingleTop(bool enabled = true) {
      return Behavior(NavigationBehavior.SingleTop, enabled);
    }

    public NavigationOptions ClearStack(bool enabled = true) {
      return Behavior(NavigationBehavior.ClearStack, enabled);
    }

    public NavigationOptions PopUpTo(string route, bool inclusive = false) {
      return new NavigationOptions(
        inclusive ? behavior | NavigationBehavior.PopUpToInclusive : behavior & ~NavigationBehavior.PopUpToInclusive,
        route,
        transition,
        direction
      );
    }

    public NavigationOptions WithTransition(INavigationTransition value) {
      return new NavigationOptions(behavior, popUpToRoute, value, direction);
    }

    public NavigationOptions WithDirection(NavigationDirection value) {
      return new NavigationOptions(behavior, popUpToRoute, transition, value);
    }
  }

  public sealed class NavigationEntry {
    private INavigationCompletion _completion;

    internal NavigationEntry(
      long id,
      NavigationRoute route,
      NavigationPage page,
      NavigationArguments arguments,
      INavigationCompletion completion = null
    ) {
      Id = id;
      Route = route;
      Page = page;
      Arguments = arguments ?? NavigationArguments.Empty;
      _completion = completion;
    }

    public long Id { get; }
    public string Name => Route?.Name ?? Page?.Name;
    public NavigationEntryOrigin Origin =>
      Route == null ? NavigationEntryOrigin.Dynamic : NavigationEntryOrigin.Registered;
    public NavigationRoute Route { get; private set; }
    public NavigationPage Page { get; private set; }
    public NavigationArguments Arguments { get; private set; }
    public bool Opaque => Page?.Opaque ?? true;
    public bool IsCached { get; internal set; }
    internal IBoundary Boundary { get; set; }

    internal void Update(NavigationRoute route, NavigationPage page, NavigationArguments arguments) {
      Route = route;
      Page = page;
      Arguments = arguments ?? NavigationArguments.Empty;
    }

    internal void Complete(object result) {
      var completion = _completion;
      _completion = null;
      completion?.SetResult(result);
    }

    internal void Fail(Exception exception) {
      var completion = _completion;
      _completion = null;
      completion?.SetException(exception);
    }
  }

  public sealed class NavigationChange {
    public long Id { get; internal set; }
    public NavigationOperationKind Operation { get; internal set; }
    public NavigationDirection Direction { get; internal set; }
    public INavigationTransition Transition { get; internal set; }
    public IReadOnlyList<NavigationEntry> Added => AddedMutable;
    public IReadOnlyList<NavigationEntry> Removed => RemovedMutable;
    public IReadOnlyList<NavigationEntry> Entering => EnteringMutable;
    public IReadOnlyList<NavigationEntry> Exiting => ExitingMutable;
    internal bool Started { get; set; }
    internal List<NavigationEntry> AddedMutable { get; } = new(2);
    internal List<NavigationEntry> RemovedMutable { get; } = new(2);
    internal List<NavigationEntry> EnteringMutable { get; } = new(2);
    internal List<NavigationEntry> ExitingMutable { get; } = new(2);

    internal void Reset(
      long id,
      NavigationOperationKind operation,
      INavigationTransition transition,
      NavigationDirection direction
    ) {
      Id = id;
      Operation = operation;
      Transition = transition;
      Direction = direction;
      Started = false;
      AddedMutable.Clear();
      RemovedMutable.Clear();
      EnteringMutable.Clear();
      ExitingMutable.Clear();
    }
  }

  internal interface INavigationCompletion {
    void SetResult(object value);
    void SetException(Exception exception);
  }

  internal sealed class NavigationCompletion<T> : INavigationCompletion {
    private readonly AwaitableCompletionSource<T> _source = new();

    internal Awaitable<T> Awaitable => _source.Awaitable;

    public void SetResult(object value) {
      if (value == null) {
        _source.SetResult(default);
        return;
      }
      if (value is T typed) {
        _source.SetResult(typed);
        return;
      }
      _source.SetException(
        new InvalidCastException($"Navigation returned {value.GetType()} but the caller expects {typeof(T)}.")
      );
    }

    public void SetException(Exception exception) {
      _source.SetException(exception);
    }
  }

  internal sealed class NavigationActionCompletion<T> : INavigationCompletion {
    private IBoundary _boundary;
    private CompositionAction<Exception> _onError;
    private CompositionAction<T> _onResult;

    internal NavigationActionCompletion(
      IBoundary boundary,
      CompositionAction<T> onResult,
      CompositionAction<Exception> onError
    ) {
      _boundary = boundary ?? throw new ArgumentNullException(nameof(boundary));
      _onResult = onResult ?? throw new ArgumentNullException(nameof(onResult));
      _onError = onError;
    }

    public void SetResult(object value) {
      if (value == null) {
        Complete(default);
        return;
      }
      if (value is T typed) {
        Complete(typed);
        return;
      }
      SetException(
        new InvalidCastException($"Navigation returned {value.GetType()} but the callback expects {typeof(T)}.")
      );
    }

    public void SetException(Exception exception) {
      var boundary = _boundary;
      var action = _onError;
      Clear();
      action?.Call(boundary, exception);
    }

    private void Complete(T value) {
      var boundary = _boundary;
      var action = _onResult;
      Clear();
      action.Call(boundary, value);
    }

    private void Clear() {
      _boundary = null;
      _onResult = null;
      _onError = null;
    }
  }
}
