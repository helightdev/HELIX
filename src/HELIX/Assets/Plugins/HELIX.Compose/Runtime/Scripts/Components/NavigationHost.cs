using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [EnableMixins]
  [BoundaryElementMixin]
  internal partial class NavigationPageBoundary {
    internal NavigationEntry Entry => props.entry;

    [Hook]
    private void OnDispose() {
      if (ReferenceEquals(props.entry?.Boundary, this))
        if (props.entry != null)
          props.entry.Boundary = null;
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      var transitioning = (props.presentation &
        (NavigationPresentation.Entering | NavigationPresentation.Exiting)) != 0;
      var hidden = (props.presentation & NavigationPresentation.Cached) != 0 ||
        ((props.presentation & NavigationPresentation.Covered) != 0 && !transitioning);

      this.Stretched();
      pickingMode = PickingMode.Ignore;
      style.display = hidden ? DisplayStyle.None : DisplayStyle.Flex;
      SetEnabled((props.presentation & NavigationPresentation.Exiting) == 0);
      props.entry.Boundary = this;

      var contextData = new NavigationContextData(props.controller, props.entry);
      using (cx.WriteContext(out var context)) NavigationContextData.Key[in context] = contextData;
      try {
        props.entry.Page?.Builder?.Invoke(ref cx, contextData);
      } catch (Exception exception) {
        props.controller.FailPresentation(props.entry, exception);
        throw;
      }
    }

    public partial struct Props {
      public NavigationController controller;
      public NavigationEntry entry;
      public NavigationPresentation presentation;
    }
  }

  [EnableMixins]
  [BoundaryElementMixin(cacheLookups: true)]
  public partial class NavigationHostBoundary {
    private readonly List<NavigationPageBoundary> _pages = new(8);
    private bool _isAutomaticController;
    private NavigationTransitionRunner _transitionRunner;

    public NavigationController Controller { get; private set; }

    internal IVisualElementScheduler Scheduler => schedule;

    [Hook]
    private void OnInit() {
      RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
      _transitionRunner ??= new NavigationTransitionRunner(this);
      RequestFocus();
    }

    [Hook]
    private void OnDispose() {
      UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
      _transitionRunner?.Cancel();
      Controller?.DetachPresenter(this);
      if (_isAutomaticController) Controller?.Dispose();
      Controller = null;
      _isAutomaticController = false;
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      EnsureController();
      cx.SubscribeTo(Controller);
      var contextData = new NavigationContextData(Controller, null);
      using (cx.WriteContext(out var context)) NavigationContextData.Key[in context] = contextData;

      this.MakeRelative().Flexible(1f, 1f, Align.Stretch);
      focusable = true;
      pickingMode = PickingMode.Ignore;

      _pages.Clear();
      var stack = Controller.PresentationStack;
      for (var i = 0; i < stack.Count; i++) {
        var entry = stack[i];
        var identity = unchecked((int)entry.Id ^ (int)(entry.Id >> 32));
        cx.AUTHORING.SetId(CompositionId.Generated(identity)); // TODO: Probably change this
        var presentation = Controller.PresentationOf(entry, IsCovered(stack, i));
        ref var result = ref NavigationPageBoundary.ComposeBoundary(ref cx, Controller, entry, presentation);
        if (result.element is NavigationPageBoundary boundary) _pages.Add(boundary);
      }

      PresentActiveChange();
    }

    private void EnsureController() {
      var resolved = props.controller;
      if (resolved == null) {
        if (Controller == null || !_isAutomaticController) {
          Controller?.DetachPresenter(this);
          if (_isAutomaticController) Controller?.Dispose();
          Controller = new NavigationController(props.graph);
          _isAutomaticController = true;
        } else if (props.graph != null) Controller.SetGraph(props.graph);
      } else if (!ReferenceEquals(Controller, resolved)) {
        Controller?.DetachPresenter(this);
        if (_isAutomaticController) Controller?.Dispose();
        Controller = resolved;
        _isAutomaticController = false;
        if (props.graph != null) Controller.SetGraph(props.graph, true);
      } else if (props.graph != null) Controller.SetGraph(props.graph, true);
      Controller.AttachPresenter(this);
      _transitionRunner ??= new NavigationTransitionRunner(this);
    }

    private void PresentActiveChange() {
      var change = Controller.ActiveChange;
      if (change == null) {
        _transitionRunner.Cancel();
        return;
      }
      if (!_transitionRunner.Matches(change.Id)) _transitionRunner.Cancel();
      try {
        if (!Controller.TryStartPresentation(change.Id)) return;
        _transitionRunner.Start(change, change.Transition ?? props.transition ?? NavigationTransitions.Default, _pages);
        RequestFocus();
      } catch {
        Controller.CompletePresentation(change.Id);
        throw;
      }
    }

    private void OnNavigationCancel(NavigationCancelEvent evt) {
      if ((props.behavior & NavigationHostBehavior.PopOnCancel) == 0) return;
      if (Controller?.Pop() != true) return;
      evt.StopImmediatePropagation();
    }

    private void RequestFocus() {
      schedule.Execute(() => {
          if (panel == null || resolvedStyle.display == DisplayStyle.None) return;
          Focus();
        }
      ).ExecuteLater(1);
    }

    private static bool IsCovered(IReadOnlyList<NavigationEntry> stack, int index) {
      for (var i = stack.Count - 1; i > index; i--)
        if (stack[i].Opaque)
          return true;
      return false;
    }

    internal void TransitionCompleted(long changeId) {
      Controller?.CompletePresentation(changeId);
    }

    public partial struct Props {
      [Prop(null)] public NavigationGraph graph;
      [Prop(null)] public NavigationController controller;
      [Prop(null, Equatable = false)] public INavigationTransition transition;
      [Prop(NavigationHostBehavior.Default)] public NavigationHostBehavior behavior;
    }
  }

  [EnableMixins]
  [BoundaryElementMixin(name: "NavigationLink", extension: true)]
  [InputStateListener]
  public partial class HXNavigationLink {
    private NavigationController _controller;

    [Hook]
    private void OnCompose(ref Composition cx) {
      _controller = props.controller ?? cx.ReadContext(NavigationContextData.Key, false).controller;
      if (_controller != null) cx.SubscribeTo(_controller);
      var available = _controller?.Graph?.Contains(props.route) == true;
      var enabled = props.enabled && available;

      this.Toggle(State.Selected, _controller?.Current?.Route == props.route);
      this.Toggle(State.Disabled, !enabled);
      focusable = enabled;

      var boxStyle = props.style ?? HXButton.Style.ReadOrThemeProperty(in cx, ThemeProperties.ButtonFilled);
      boxStyle.RenderBoundary(ref cx, InputState);
      if (props.content != null) props.content(ref cx);
      else cx.Text(props.route?.DisplayName ?? string.Empty);
    }

    [ClickHandler]
    private void OnClick(EventBase evt) {
      if (!props.enabled || _controller?.Graph?.Contains(props.route) != true) return;
      _controller.Activate(props.route, options: props.options);
    }

    public partial struct Props {
      public NavigationRoute route;
      [Prop(null)] public NavigationController controller;
      [Prop(null)] public Composable content;
      [Prop(true)] public bool enabled;
      [Prop(null)] public HXControlBoxStyle? style;
      [Prop("NavigationOptions.Default", PropInit.Deferred)]
      public NavigationOptions options;
    }
  }

  internal sealed class NavigationTransitionRunner {
    private readonly List<NavigationPageBoundary> _entering = new(2);
    private readonly List<NavigationPageBoundary> _exiting = new(2);
    private readonly NavigationHostBoundary _host;
    private readonly List<NavigationPageBoundary> _removed = new(2);
    private bool _active;
    private long _changeId;
    private NavigationDirection _direction;
    private float _elapsedMs;
    private NavigationOperationKind _operation;
    private IVisualElementScheduledItem _scheduled;
    private INavigationTransition _transition;

    internal NavigationTransitionRunner(NavigationHostBoundary host) {
      _host = host;
    }

    internal bool Matches(long changeId) {
      return _active && _changeId == changeId;
    }

    internal void Start(
      NavigationChange change,
      INavigationTransition transition,
      IReadOnlyList<NavigationPageBoundary> pages
    ) {
      Cancel();
      _changeId = change.Id;
      _operation = change.Operation;
      _direction = change.Direction;
      _transition = transition ?? NavigationTransitions.Instant;
      _elapsedMs = 0f;
      _active = true;

      for (var i = 0; i < pages.Count; i++) {
        var page = pages[i];
        if (Contains(change.Entering, page.Entry)) _entering.Add(page);
        if (Contains(change.Exiting, page.Entry)) _exiting.Add(page);
        if (Contains(change.Removed, page.Entry)) _removed.Add(page);
      }

      Apply(0f);
      EnsureScheduled();
      _scheduled.Resume();
    }

    internal void Cancel() {
      _scheduled?.Pause();
      ResetElements();
      _active = false;
      _changeId = 0;
      _transition = null;
      _entering.Clear();
      _exiting.Clear();
      _removed.Clear();
    }

    private void EnsureScheduled() {
      if (_scheduled != null) return;
      _scheduled = _host.Scheduler.Execute(Update).Every(0);
      _scheduled.Pause();
    }

    private void Update(TimerState state) {
      if (!_active) {
        _scheduled.Pause();
        return;
      }

      var duration = Mathf.Max(0, _transition.DurationMs);
      _elapsedMs += state.deltaTime;
      var progress = duration == 0 ? 1f : Mathf.Clamp01(_elapsedMs / duration);
      try {
        Apply(progress);
      } catch {
        var failedId = _changeId;
        Cancel();
        _host.TransitionCompleted(failedId);
        throw;
      }
      if (progress < 1f) return;

      var completedId = _changeId;
      _scheduled.Pause();
      ResetRetainedElements();
      _active = false;
      _changeId = 0;
      _transition = null;
      _entering.Clear();
      _exiting.Clear();
      _removed.Clear();
      _host.TransitionCompleted(completedId);
    }

    private void Apply(float progress) {
      var eased = _transition.Easing.Eval(progress);
      var entering = new NavigationTransitionFrame(_operation, _direction, NavigationTransitionRole.Entering, eased);
      var exiting = new NavigationTransitionFrame(_operation, _direction, NavigationTransitionRole.Exiting, eased);
      for (var i = 0; i < _exiting.Count; i++) _transition.Apply(_exiting[i].Element, in exiting);
      for (var i = 0; i < _entering.Count; i++) _transition.Apply(_entering[i].Element, in entering);
    }

    private void ResetElements() {
      for (var i = 0; i < _entering.Count; i++) NavigationTransitions.Reset(_entering[i].Element);
      for (var i = 0; i < _exiting.Count; i++) NavigationTransitions.Reset(_exiting[i].Element);
    }

    private void ResetRetainedElements() {
      for (var i = 0; i < _entering.Count; i++) NavigationTransitions.Reset(_entering[i].Element);
      for (var i = 0; i < _exiting.Count; i++)
        if (!_removed.Contains(_exiting[i]))
          NavigationTransitions.Reset(_exiting[i].Element);
    }

    private static bool Contains(IReadOnlyList<NavigationEntry> entries, NavigationEntry entry) {
      for (var i = 0; i < entries.Count; i++)
        if (ReferenceEquals(entries[i], entry))
          return true;
      return false;
    }
  }

  public static class NavigationExtensions {
    public static ref ElementRef NavigationHost(
      this ref Composition cx,
      NavigationGraph graph = null,
      NavigationController controller = null,
      INavigationTransition transition = null,
      NavigationHostBehavior behavior = NavigationHostBehavior.Default
    ) {
      return ref NavigationHostBoundary.ComposeBoundary(ref cx, graph, controller, transition, behavior);
    }

    public static ref ElementRef NavigationHost(
      this ref Composition cx,
      NavigationController controller,
      INavigationTransition transition = null,
      NavigationHostBehavior behavior = NavigationHostBehavior.Default
    ) {
      return ref NavigationHostBoundary.ComposeBoundary(ref cx, null, controller, transition, behavior);
    }

    public static ref ElementRef NavigationHost(
      this ref Composition cx,
      out NavigationController resolvedController,
      NavigationGraph graph = null,
      NavigationController controller = null,
      INavigationTransition transition = null,
      NavigationHostBehavior behavior = NavigationHostBehavior.Default
    ) {
      ref var result = ref NavigationHostBoundary.ComposeBoundary(ref cx, graph, controller, transition, behavior);
      resolvedController = result.element is NavigationHostBoundary boundary ? boundary.Controller : controller;
      return ref result;
    }

    public static ref ElementRef NavigationHost(
      this ref Composition cx,
      NavigationGraph graph,
      out NavigationController resolvedController,
      NavigationController controller = null,
      INavigationTransition transition = null,
      NavigationHostBehavior behavior = NavigationHostBehavior.Default
    ) {
      ref var result = ref NavigationHostBoundary.ComposeBoundary(ref cx, graph, controller, transition, behavior);
      resolvedController = result.element is NavigationHostBoundary boundary ? boundary.Controller : controller;
      return ref result;
    }

    public static NavigationController NavigationController(this ref Composition cx, bool listen = true) {
      return cx.ReadContext(NavigationContextData.Key, listen).controller;
    }

    public static NavigationEntry NavigationEntry(this ref Composition cx, bool listen = true) {
      return cx.ReadContext(NavigationContextData.Key, listen).entry;
    }

    public static NavigationController NavigationController(this CompositionContext context) {
      return NavigationContextData.Key.ReadAt(context.element).controller ??
        context.Lookup<NavigationHostBoundary>()?.Controller;
    }

    public static NavigationEntry NavigationEntry(this CompositionContext context) {
      return NavigationContextData.Key.ReadAt(context.element).entry ??
        context.Lookup<NavigationPageBoundary>()?.Entry;
    }
  }
}