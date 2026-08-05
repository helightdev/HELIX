using System;
using System.Collections.Generic;
using HELIX.Signals;
using UnityEngine;

namespace HELIX.Compose {
  public sealed class NavigationController : Signal<int> {
    private readonly List<NavigationEntry> _backStack = new(8);
    private readonly List<NavigationEntry> _cached = new(4);
    private readonly List<NavigationEntry> _before = new(8);
    private readonly List<NavigationEntry> _cachedBefore = new(4);
    private readonly List<NavigationEntry> _presentation = new(8);
    private readonly Queue<NavigationRequest> _requests = new(8);
    private readonly NavigationChange _change = new();
    private object _presenter;
    private object _popResult;
    private bool _hasPopResult;
    private bool _active;
    private int _revision;
    private long _nextEntryId = 1;
    private long _nextOperationId = 1;

    public NavigationController(NavigationGraph graph = null)
      : base("NavigationController", typeof(NavigationController)) {
      SetGraph(graph);
    }

    public NavigationGraph Graph { get; private set; }
    public IReadOnlyList<NavigationEntry> BackStack => _backStack;
    public IReadOnlyList<NavigationEntry> CachedEntries => _cached;
    public IReadOnlyList<NavigationEntry> PresentationStack => _presentation;
    public NavigationEntry Current => _backStack.Count == 0 ? null : _backStack[^1];
    public NavigationChange ActiveChange => _active ? _change : null;
    public bool CanPop => _backStack.Count > 1;
    public bool IsTransitioning => _active;
    public int PendingOperationCount => _requests.Count;
    public int MaxPendingOperations { get; set; } = 64;
    public CompositionAction<NavigationEvent> onLifecycle;

    public override int PeekValue() => _revision;

    public override void SetValue(int newValue) =>
      throw new NotSupportedException("Navigation state is changed with navigation operations.");

    public override void SetWithoutNotify(int newValue) =>
      throw new NotSupportedException("Navigation state is changed with navigation operations.");

    public void SetGraph(NavigationGraph graph, bool preserveStack = false) {
      if (ReferenceEquals(Graph, graph)) return;
      RejectPending(new InvalidOperationException("The navigation graph changed before the operation was handled."));
      if (_active) CompleteActive(pump: false);

      Graph = graph;
      if (preserveStack && _backStack.Count > 0 && AreEntriesValidFor(graph)) {
        RemapStack(graph);
      } else {
        var exception = new InvalidOperationException("The navigation stack was replaced.");
        FailEntries(_backStack, exception);
        FailEntries(_cached, exception);
        _backStack.Clear();
        ClearCachedEntries();
        if (graph?.TryGetRoute(graph.InitialRoute, out var initial) == true) {
          _backStack.Add(CreateEntry(initial, initial.Page, NavigationArguments.Empty));
        }
      }
      SynchronizePresentation();
      NotifyChanged();
    }

    public bool Navigate(
      string route,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => Push(route, arguments, options);

    public bool Push(
      string route,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => SubmitPush(route, arguments, options);

    public NavigationSubmission SubmitPush(
      string route,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => SubmitRoute(NavigationOperationKind.Push, route, arguments, options);

    public Awaitable<T> Push<T>(
      string route,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) {
      var completion = new NavigationCompletion<T>();
      SubmitRoute(NavigationOperationKind.Push, route, arguments, options, completion);
      return completion.Awaitable;
    }

    public NavigationSubmission Push<T>(
      string route,
      CompositionAction<T> onResult,
      NavigationArguments arguments = null,
      NavigationOptions? options = null,
      CompositionAction<Exception> onError = null
    ) => SubmitRoute(
      NavigationOperationKind.Push,
      route,
      arguments,
      options,
      new NavigationActionCompletion<T>(ResultBoundary(), onResult, onError)
    );

    public bool Push(
      NavigationPage page,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => SubmitPush(page, arguments, options);

    public NavigationSubmission SubmitPush(
      NavigationPage page,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => SubmitPage(NavigationOperationKind.Push, page, arguments, options);

    public Awaitable<T> Push<T>(
      NavigationPage page,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) {
      var completion = new NavigationCompletion<T>();
      SubmitPage(NavigationOperationKind.Push, page, arguments, options, completion);
      return completion.Awaitable;
    }

    public NavigationSubmission Push<T>(
      NavigationPage page,
      CompositionAction<T> onResult,
      NavigationArguments arguments = null,
      NavigationOptions? options = null,
      CompositionAction<Exception> onError = null
    ) => SubmitPage(
      NavigationOperationKind.Push,
      page,
      arguments,
      options,
      new NavigationActionCompletion<T>(ResultBoundary(), onResult, onError)
    );

    public bool Push(
      Composable<NavigationContextData> builder,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => Push(NavigationPage.Build(builder).Build(), arguments, options);

    public bool Push(
      NavigationPageBuilder page,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => Push(page?.Build(), arguments, options);

    public Awaitable<T> Push<T>(
      Composable<NavigationContextData> builder,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => Push<T>(NavigationPage.Build(builder).Build(), arguments, options);

    public NavigationSubmission Push<T>(
      Composable<NavigationContextData> builder,
      CompositionAction<T> onResult,
      NavigationArguments arguments = null,
      NavigationOptions? options = null,
      CompositionAction<Exception> onError = null
    ) => Push<T>(NavigationPage.Build(builder).Build(), onResult, arguments, options, onError);

    public Awaitable<T> Push<T>(
      NavigationPageBuilder page,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => Push<T>(page?.Build(), arguments, options);

    public NavigationSubmission Push<T>(
      NavigationPageBuilder page,
      CompositionAction<T> onResult,
      NavigationArguments arguments = null,
      NavigationOptions? options = null,
      CompositionAction<Exception> onError = null
    ) => Push(page?.Build(), onResult, arguments, options, onError);

    public bool Replace(
      string route,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => SubmitRoute(NavigationOperationKind.Replace, route, arguments, options);

    public bool Replace(
      NavigationPage page,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => SubmitPage(NavigationOperationKind.Replace, page, arguments, options);

    public bool Replace(
      Composable<NavigationContextData> builder,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => Replace(NavigationPage.Build(builder).Build(), arguments, options);

    public bool Replace(
      NavigationPageBuilder page,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => Replace(page?.Build(), arguments, options);

    public bool Go(
      string route,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => SubmitRoute(NavigationOperationKind.Go, route, arguments, options);

    public bool Activate(
      string route,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => SubmitRoute(NavigationOperationKind.Activate, route, arguments, options);

    public bool Activate(
      NavigationRoute route,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) {
      if (!Owns(route)) return false;
      return SubmitRoute(NavigationOperationKind.Activate, route.Name, arguments, options);
    }

    public bool Activate(
      NavigationPage page,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) => SubmitPage(NavigationOperationKind.Activate, page, arguments, options);

    public bool Activate(
      NavigationEntry entry,
      NavigationArguments arguments = null,
      NavigationOptions? options = null
    ) {
      var id = ClaimOperationId();
      if (entry == null) {
        return Reject(id, NavigationFailure.TargetNotFound, null, "The navigation entry is null.");
      }
      return Enqueue(NavigationRequest.ToEntry(
        id,
        NavigationOperationKind.Activate,
        entry,
        arguments,
        options ?? NavigationOptions.Default
      ));
    }

    public bool Preload(string route, NavigationArguments arguments = null) =>
      SubmitRoute(NavigationOperationKind.Preload, route, arguments, NavigationOptions.Default);

    public bool Preload(NavigationRoute route, NavigationArguments arguments = null) {
      if (!Owns(route)) return false;
      return SubmitRoute(NavigationOperationKind.Preload, route.Name, arguments, NavigationOptions.Default);
    }

    public bool Preload(NavigationPage page, NavigationArguments arguments = null) =>
      SubmitPage(NavigationOperationKind.Preload, page, arguments, NavigationOptions.Default);

    public bool Pop() => SubmitPop();

    public bool Pop<T>(T result) => SubmitPop(result, true);

    public NavigationSubmission SubmitPop(object result = null, bool hasResult = false) => Enqueue(
      NavigationRequest.Pop(ClaimOperationId(), result, hasResult)
    );

    public bool PopTo(string route, bool inclusive = false) => SubmitPopTo(route, inclusive);

    public NavigationSubmission SubmitPopTo(string route, bool inclusive = false) => Enqueue(
      NavigationRequest.PopTo(ClaimOperationId(), route, inclusive)
    );

    public bool Reset(string route = null, NavigationArguments arguments = null) {
      route ??= Graph?.InitialRoute;
      if (arguments == null && _requests.Count == 0 && _backStack.Count == 1 &&
          Current?.Origin == NavigationEntryOrigin.Registered &&
          string.Equals(Current.Name, route, StringComparison.Ordinal)) return false;
      return SubmitRoute(NavigationOperationKind.Reset, route, arguments, null);
    }

    internal bool AttachPresenter(object presenter) {
      if (presenter == null) return false;
      if (_presenter != null && !ReferenceEquals(_presenter, presenter)) {
        throw new InvalidOperationException("A navigation controller can only be presented by one host.");
      }
      _presenter = presenter;
      return true;
    }

    internal void DetachPresenter(object presenter) {
      if (!ReferenceEquals(_presenter, presenter)) return;
      _presenter = null;
      if (_active) CompleteActive();
    }

    internal bool TryStartPresentation(long changeId) {
      if (!_active || _change.Id != changeId || _change.Started) return false;
      _change.Started = true;
      var exception = Dispatch(_change.Entering, entering: true, completed: false);
      exception = Dispatch(_change.Exiting, entering: false, completed: false, exception);
      if (exception != null) throw exception;
      return true;
    }

    internal void CompletePresentation(long changeId) {
      if (!_active || _change.Id != changeId) return;
      CompleteActive();
    }

    internal void FailPresentation(NavigationEntry entry, Exception exception) {
      if (entry?.IsCached == true && _cached.Remove(entry)) {
        entry.IsCached = false;
        entry.Fail(exception);
        SynchronizePresentation();
        NotifyChanged();
        return;
      }
      if (!_active || !_change.EnteringMutable.Contains(entry)) return;
      FailEntries(_change.Added, exception);
      RejectPending(exception);
      _backStack.Clear();
      _backStack.AddRange(_before);
      RestoreCachedEntries();
      _active = false;
      _popResult = null;
      _hasPopResult = false;
      SynchronizePresentation();
      NotifyChanged();
    }

    internal NavigationPresentation PresentationOf(NavigationEntry entry, bool covered) {
      var value = ReferenceEquals(Current, entry) ? NavigationPresentation.Current : NavigationPresentation.None;
      if (entry.IsCached) value |= NavigationPresentation.Cached;
      if (covered) value |= NavigationPresentation.Covered;
      if (_active && _change.EnteringMutable.Contains(entry)) value |= NavigationPresentation.Entering;
      if (_active && _change.ExitingMutable.Contains(entry)) value |= NavigationPresentation.Exiting;
      return value;
    }

    public override void Dispose() {
      if (IsDisposed) return;
      _presenter = null;
      RejectPending(new ObjectDisposedException(nameof(NavigationController)));
      FailEntries(_backStack, new ObjectDisposedException(nameof(NavigationController)));
      FailEntries(_cached, new ObjectDisposedException(nameof(NavigationController)));
      if (_active) FailEntries(_change.Removed, new ObjectDisposedException(nameof(NavigationController)));
      _backStack.Clear();
      ClearCachedEntries();
      _presentation.Clear();
      _active = false;
      Graph = null;
      base.Dispose();
    }

    private NavigationSubmission SubmitRoute(
      NavigationOperationKind operation,
      string route,
      NavigationArguments arguments,
      NavigationOptions? options,
      INavigationCompletion completion = null
    ) {
      var id = ClaimOperationId();
      if (string.IsNullOrWhiteSpace(route) || Graph?.TryGetRoute(route, out _) != true) {
        return Reject(id, NavigationFailure.RouteNotFound, completion, $"Navigation route '{route}' was not found.");
      }
      return Enqueue(
        NavigationRequest.ToRoute(id, operation, route, arguments, options ?? NavigationOptions.Default, completion)
      );
    }

    private NavigationSubmission SubmitPage(
      NavigationOperationKind operation,
      NavigationPage page,
      NavigationArguments arguments,
      NavigationOptions? options,
      INavigationCompletion completion = null
    ) {
      var id = ClaimOperationId();
      if (page?.Builder == null) {
        return Reject(id, NavigationFailure.InvalidPage, completion, "A dynamic navigation page needs a builder.");
      }
      return Enqueue(
        NavigationRequest.ToPage(id, operation, page, arguments, options ?? NavigationOptions.Default, completion)
      );
    }

    private NavigationSubmission Enqueue(in NavigationRequest request) {
      if (IsDisposed) {
        return Reject(
          request.Id,
          NavigationFailure.ControllerDisposed,
          request.Completion,
          "The navigation controller has been disposed."
        );
      }
      if (_requests.Count >= Mathf.Max(1, MaxPendingOperations)) {
        return Reject(
          request.Id,
          NavigationFailure.QueueFull,
          request.Completion,
          "The navigation operation queue is full."
        );
      }
      _requests.Enqueue(request);
      Pump();
      return new NavigationSubmission(request.Id, NavigationFailure.None);
    }

    private void Pump() {
      while (!_active && _requests.Count > 0 && !IsDisposed) {
        var request = _requests.Dequeue();
        if (!TryApply(in request, out var failure)) {
          Reject(request.Id, failure, request.Completion, FailureMessage(failure));
          continue;
        }

        NotifyChanged();
        if (!_active) continue;
        if (_presenter != null) return;
        CompleteActive(pump: false);
      }
    }

    private bool TryApply(in NavigationRequest request, out NavigationFailure failure) {
      failure = NavigationFailure.None;
      _before.Clear();
      _before.AddRange(_backStack);
      _cachedBefore.Clear();
      _cachedBefore.AddRange(_cached);

      NavigationRoute route = null;
      var page = request.Page;
      if (request.Route != null && request.Operation != NavigationOperationKind.PopTo) {
        if (Graph?.TryGetRoute(request.Route, out route) != true) {
          failure = NavigationFailure.RouteNotFound;
          return false;
        }
        page = route.Page;
      }

      if (request.Operation == NavigationOperationKind.Preload) {
        if (FindExisting(route, page) == null) {
          var entry = CreateEntry(route, page, request.Arguments);
          entry.IsCached = true;
          _cached.Add(entry);
        }
        PrepareInactive(in request);
        return true;
      }

      if (request.Operation == NavigationOperationKind.Activate) {
        var entry = request.Entry ?? FindExisting(route, page);
        if (entry != null && !_backStack.Contains(entry) && !_cached.Contains(entry)) {
          failure = NavigationFailure.TargetNotFound;
          return false;
        }

        var created = entry == null;
        entry ??= CreateEntry(route, page, request.Arguments, request.Completion);
        if (request.HasArguments) entry.Update(route ?? entry.Route, page ?? entry.Page, request.Arguments);

        if (ReferenceEquals(Current, entry)) {
          PrepareInactive(in request);
          return true;
        }

        if (_cached.Remove(entry)) entry.IsCached = false;
        else _backStack.Remove(entry);
        _backStack.Add(entry);
        PrepareActivation(in request, entry, created);
        return true;
      }

      switch (request.Operation) {
        case NavigationOperationKind.Push:
          if (request.Options.Has(NavigationBehavior.SingleTop) &&
              !request.Options.Has(NavigationBehavior.ClearStack) &&
              string.IsNullOrEmpty(request.Options.popUpToRoute) &&
              Matches(Current, route, page)) {
            if (request.Completion != null) {
              failure = NavigationFailure.ResultUnavailable;
              return false;
            }
            Current.Update(route, page, request.Arguments);
            PrepareChange(in request, page);
            return true;
          }
          if (request.Options.Has(NavigationBehavior.ClearStack)) _backStack.Clear();
          else if (!string.IsNullOrEmpty(request.Options.popUpToRoute)) {
            var popIndex = FindEntryIndex(request.Options.popUpToRoute);
            if (popIndex < 0) {
              failure = NavigationFailure.TargetNotFound;
              return false;
            }
            var keepCount = request.Options.Has(NavigationBehavior.PopUpToInclusive)
              ? popIndex
              : popIndex + 1;
            if (keepCount < _backStack.Count) {
              _backStack.RemoveRange(keepCount, _backStack.Count - keepCount);
            }
          }
          _backStack.Add(CreateEntry(route, page, request.Arguments, request.Completion));
          break;
        case NavigationOperationKind.Replace:
          if (_backStack.Count > 0) _backStack.RemoveAt(_backStack.Count - 1);
          _backStack.Add(CreateEntry(route, page, request.Arguments, request.Completion));
          break;
        case NavigationOperationKind.Pop:
          if (!CanPop) {
            failure = NavigationFailure.CannotPop;
            return false;
          }
          _backStack.RemoveAt(_backStack.Count - 1);
          break;
        case NavigationOperationKind.PopTo:
          if (!PopToInternal(request.Route, request.Inclusive)) {
            failure = NavigationFailure.TargetNotFound;
            return false;
          }
          break;
        case NavigationOperationKind.Reset:
        case NavigationOperationKind.Go:
          _backStack.Clear();
          _backStack.Add(CreateEntry(route, page, request.Arguments, request.Completion));
          break;
        default: throw new ArgumentOutOfRangeException();
      }

      PrepareChange(in request, page);
      return true;
    }

    private void PrepareInactive(in NavigationRequest request) {
      _change.Reset(request.Id, request.Operation, null, NavigationDirection.Automatic);
      _active = false;
      _popResult = null;
      _hasPopResult = false;
      SynchronizePresentation();
    }

    private void PrepareActivation(in NavigationRequest request, NavigationEntry entry, bool created) {
      var transition = request.Options.transition ?? entry.Page?.Transition;
      var previous = _before.Count == 0 ? null : _before[^1];
      var direction = ResolveDirection(request.Options.direction, request.Operation, previous, entry);
      _change.Reset(request.Id, request.Operation, transition, direction);
      if (created) _change.AddedMutable.Add(entry);
      _change.EnteringMutable.Add(entry);
      if (_before.Count > 0 && entry.Opaque) AddUnique(_change.ExitingMutable, _before[^1]);

      _popResult = null;
      _hasPopResult = false;
      _active = true;
      SynchronizePresentation();
    }

    private void PrepareChange(in NavigationRequest request, NavigationPage targetPage) {
      var transition = request.Options.transition ?? targetPage?.Transition;
      if (request.Operation is NavigationOperationKind.Pop or NavigationOperationKind.PopTo) {
        transition ??= _before.Count == 0 ? null : _before[^1].Page?.Transition;
      }
      var previous = _before.Count == 0 ? null : _before[^1];
      var direction = ResolveDirection(request.Options.direction, request.Operation, previous, Current);
      _change.Reset(request.Id, request.Operation, transition, direction);

      var common = 0;
      var commonLimit = Mathf.Min(_before.Count, _backStack.Count);
      while (common < commonLimit && ReferenceEquals(_before[common], _backStack[common])) common++;

      for (var i = common; i < _before.Count; i++) _change.RemovedMutable.Add(_before[i]);
      for (var i = common; i < _backStack.Count; i++) _change.AddedMutable.Add(_backStack[i]);

      _presentation.Clear();
      _presentation.AddRange(_cached);
      for (var i = 0; i < common; i++) _presentation.Add(_backStack[i]);
      _presentation.AddRange(_change.RemovedMutable);
      _presentation.AddRange(_change.AddedMutable);

      _change.EnteringMutable.AddRange(_change.AddedMutable);
      _change.ExitingMutable.AddRange(_change.RemovedMutable);
      if (request.Operation == NavigationOperationKind.Push && targetPage?.Opaque != false && _before.Count > 0) {
        AddUnique(_change.ExitingMutable, _before[^1]);
      }
      if (request.Operation is NavigationOperationKind.Pop or NavigationOperationKind.PopTo &&
          _backStack.Count > 0 && _before.Count > 0 && _before[^1].Opaque) {
        AddUnique(_change.EnteringMutable, _backStack[^1]);
      }

      _popResult = request.Result;
      _hasPopResult = request.HasResult;
      _active = _change.Added.Count > 0 || _change.Removed.Count > 0;
      if (!_active) SynchronizePresentation();
    }

    private void CompleteActive(bool pump = true) {
      if (!_active) return;
      var exception = Dispatch(_change.Entering, entering: true, completed: true);
      exception = Dispatch(_change.Exiting, entering: false, completed: true, exception);
      var resultIndex = _change.Removed.Count - 1;
      for (var i = 0; i < _change.Removed.Count; i++) {
        try {
          _change.Removed[i].Complete(_hasPopResult && i == resultIndex ? _popResult : null);
        } catch (Exception completionException) {
          exception ??= completionException;
        }
      }

      _active = false;
      _popResult = null;
      _hasPopResult = false;
      SynchronizePresentation();
      NotifyChanged();
      if (pump) Pump();
      if (exception != null) throw exception;
    }

    private Exception Dispatch(
      IReadOnlyList<NavigationEntry> entries,
      bool entering,
      bool completed,
      Exception firstException = null
    ) {
      var phase = entering
        ? completed ? NavigationLifecyclePhase.Entered : NavigationLifecyclePhase.Entering
        : completed ? NavigationLifecyclePhase.Exited : NavigationLifecyclePhase.Exiting;
      for (var i = 0; i < entries.Count; i++) {
        var entry = entries[i];
        var action = entering
          ? completed ? entry.Page?.OnEntered : entry.Page?.OnEntering
          : completed ? entry.Page?.OnExited : entry.Page?.OnExiting;
        var boundary = entry.Boundary;
        if (boundary == null) continue;
        var navigationEvent = new NavigationEvent(this, entry, _change.Operation, _change.Direction, phase);
        if (action != null) {
          try {
            action.Call(boundary, navigationEvent);
          } catch (Exception exception) {
            firstException ??= exception;
          }
        }
        if (onLifecycle != null) {
          try {
            onLifecycle.Call(boundary, navigationEvent);
          } catch (Exception exception) {
            firstException ??= exception;
          }
        }
      }
      return firstException;
    }

    private bool PopToInternal(string route, bool inclusive) {
      var index = FindEntryIndex(route);
      if (index < 0) return false;
      var keepCount = inclusive ? index : index + 1;
      if (keepCount < 1 || keepCount == _backStack.Count) return false;
      _backStack.RemoveRange(keepCount, _backStack.Count - keepCount);
      return true;
    }

    private int FindEntryIndex(string route) {
      for (var i = _backStack.Count - 1; i >= 0; i--) {
        if (string.Equals(_backStack[i].Name, route, StringComparison.Ordinal)) return i;
      }
      return -1;
    }

    private bool AreEntriesValidFor(NavigationGraph graph) {
      if (graph == null) return _backStack.Count == 0 && _cached.Count == 0;
      return AreEntriesValidFor(_backStack, graph) && AreEntriesValidFor(_cached, graph);
    }

    private static bool AreEntriesValidFor(IReadOnlyList<NavigationEntry> entries, NavigationGraph graph) {
      for (var i = 0; i < entries.Count; i++) {
        var entry = entries[i];
        if (entry.Origin == NavigationEntryOrigin.Registered && !graph.TryGetRoute(entry.Name, out _)) return false;
      }
      return true;
    }

    private void RemapStack(NavigationGraph graph) {
      RemapEntries(_backStack, graph);
      RemapEntries(_cached, graph);
    }

    private static void RemapEntries(IReadOnlyList<NavigationEntry> entries, NavigationGraph graph) {
      for (var i = 0; i < entries.Count; i++) {
        var entry = entries[i];
        if (entry.Origin != NavigationEntryOrigin.Registered || !graph.TryGetRoute(entry.Name, out var route)) continue;
        entry.Update(route, route.Page, entry.Arguments);
      }
    }

    private NavigationEntry FindExisting(NavigationRoute route, NavigationPage page) {
      for (var i = _backStack.Count - 1; i >= 0; i--) {
        if (Matches(_backStack[i], route, page)) return _backStack[i];
      }
      for (var i = _cached.Count - 1; i >= 0; i--) {
        if (Matches(_cached[i], route, page)) return _cached[i];
      }
      return null;
    }

    private NavigationEntry CreateEntry(
      NavigationRoute route,
      NavigationPage page,
      NavigationArguments arguments,
      INavigationCompletion completion = null
    ) => new(_nextEntryId++, route, page, arguments ?? NavigationArguments.Empty, completion);

    private void SynchronizePresentation() {
      _presentation.Clear();
      _presentation.AddRange(_cached);
      _presentation.AddRange(_backStack);
    }

    private void RestoreCachedEntries() {
      ClearCachedEntries();
      for (var i = 0; i < _cachedBefore.Count; i++) {
        _cachedBefore[i].IsCached = true;
        _cached.Add(_cachedBefore[i]);
      }
    }

    private void ClearCachedEntries() {
      for (var i = 0; i < _cached.Count; i++) _cached[i].IsCached = false;
      _cached.Clear();
    }

    private void RejectPending(Exception exception) {
      while (_requests.Count > 0) _requests.Dequeue().Completion?.SetException(exception);
    }

    private static void FailEntries(IReadOnlyList<NavigationEntry> entries, Exception exception) {
      for (var i = 0; i < entries.Count; i++) entries[i].Fail(exception);
    }

    private NavigationSubmission Reject(
      long id,
      NavigationFailure failure,
      INavigationCompletion completion,
      string message
    ) {
      completion?.SetException(new InvalidOperationException(message));
      return new NavigationSubmission(id, failure);
    }

    private long ClaimOperationId() => _nextOperationId++;

    private IBoundary ResultBoundary() => Current?.Boundary ?? throw new InvalidOperationException(
      "A composable navigation result callback requires a currently presented navigation page."
    );

    private bool Owns(NavigationRoute route) => Graph?.Contains(route) == true;

    private static bool Matches(NavigationEntry entry, NavigationRoute route, NavigationPage page) =>
      entry != null && (route != null
        ? ReferenceEquals(entry.Route, route) || string.Equals(entry.Name, route.Name, StringComparison.Ordinal)
        : ReferenceEquals(entry.Page, page));

    private static void AddUnique(List<NavigationEntry> entries, NavigationEntry entry) {
      if (entry != null && !entries.Contains(entry)) entries.Add(entry);
    }

    private static NavigationDirection ResolveDirection(
      NavigationDirection requested,
      NavigationOperationKind operation,
      NavigationEntry previous,
      NavigationEntry next
    ) {
      if (requested != NavigationDirection.Automatic) return requested;
      if (operation is NavigationOperationKind.Pop or NavigationOperationKind.PopTo) {
        return NavigationDirection.Backward;
      }
      if (operation == NavigationOperationKind.Activate && previous?.Route != null && next?.Route != null &&
          previous.Route.Index != next.Route.Index) {
        return next.Route.Index > previous.Route.Index
          ? NavigationDirection.Forward
          : NavigationDirection.Backward;
      }
      return NavigationDirection.Forward;
    }

    private static string FailureMessage(NavigationFailure failure) => failure switch {
      NavigationFailure.RouteNotFound => "The requested navigation route was not found.",
      NavigationFailure.InvalidPage => "The requested dynamic navigation page is invalid.",
      NavigationFailure.CannotPop => "The root navigation entry cannot be popped.",
      NavigationFailure.TargetNotFound => "The requested navigation target was not found in the stack.",
      NavigationFailure.ResultUnavailable => "A single-top operation cannot create a second result for an existing entry.",
      _ => "The navigation operation was rejected."
    };

    private void NotifyChanged() {
      unchecked { _revision++; }
      NotifyDirty();
      NotifyObservers();
    }
  }

  internal readonly struct NavigationRequest {
    private NavigationRequest(
      long id,
      NavigationOperationKind operation,
      string route,
      NavigationPage page,
      NavigationEntry entry,
      NavigationArguments arguments,
      NavigationOptions options,
      INavigationCompletion completion,
      object result,
      bool hasResult,
      bool inclusive
    ) {
      Id = id;
      Operation = operation;
      Route = route;
      Page = page;
      Entry = entry;
      HasArguments = arguments != null;
      Arguments = arguments ?? NavigationArguments.Empty;
      Options = options;
      Completion = completion;
      Result = result;
      HasResult = hasResult;
      Inclusive = inclusive;
    }

    internal long Id { get; }
    internal NavigationOperationKind Operation { get; }
    internal string Route { get; }
    internal NavigationPage Page { get; }
    internal NavigationEntry Entry { get; }
    internal NavigationArguments Arguments { get; }
    internal bool HasArguments { get; }
    internal NavigationOptions Options { get; }
    internal INavigationCompletion Completion { get; }
    internal object Result { get; }
    internal bool HasResult { get; }
    internal bool Inclusive { get; }

    internal static NavigationRequest ToRoute(
      long id,
      NavigationOperationKind operation,
      string route,
      NavigationArguments arguments,
      NavigationOptions options,
      INavigationCompletion completion
    ) => new(id, operation, route, null, null, arguments, options, completion, null, false, false);

    internal static NavigationRequest ToPage(
      long id,
      NavigationOperationKind operation,
      NavigationPage page,
      NavigationArguments arguments,
      NavigationOptions options,
      INavigationCompletion completion
    ) => new(id, operation, null, page, null, arguments, options, completion, null, false, false);

    internal static NavigationRequest ToEntry(
      long id,
      NavigationOperationKind operation,
      NavigationEntry entry,
      NavigationArguments arguments,
      NavigationOptions options
    ) => new(id, operation, null, null, entry, arguments, options, null, null, false, false);

    internal static NavigationRequest Pop(long id, object result, bool hasResult) => new(
      id, NavigationOperationKind.Pop, null, null, null, null, NavigationOptions.Default, null, result, hasResult, false
    );

    internal static NavigationRequest PopTo(long id, string route, bool inclusive) => new(
      id, NavigationOperationKind.PopTo, route, null, null, null, NavigationOptions.Default, null, null, false, inclusive
    );
  }
}
