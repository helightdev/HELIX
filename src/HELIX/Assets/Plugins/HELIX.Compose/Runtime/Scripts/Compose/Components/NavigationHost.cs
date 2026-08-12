using System;
using System.Collections.Generic;
using HELIX.Extensions;
using HELIX.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [BoundaryComposable(Extension = false)]
  internal partial class NavigationPageBoundary {
    public partial struct Props {
      public NavigationController controller;
      public NavigationEntry entry;
      public NavigationPresentation presentation;
    }

    internal NavigationEntry Entry => props.entry;
    internal VisualElement Element => Node;

    protected override void OnDetach() {
      if (ReferenceEquals(props.entry?.Boundary, Node))
        if (props.entry != null) props.entry.Boundary = null;
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      var transitioning = (props.presentation &
                            (NavigationPresentation.Entering | NavigationPresentation.Exiting)) != 0;
      var hidden = (props.presentation & NavigationPresentation.Cached) != 0 ||
                   (props.presentation & NavigationPresentation.Covered) != 0 && !transitioning;

      Node.Stretched();
      Node.pickingMode = PickingMode.Ignore;
      Node.style.display = hidden ? DisplayStyle.None : DisplayStyle.Flex;
      Node.SetEnabled((props.presentation & NavigationPresentation.Exiting) == 0);
      props.entry.Boundary = Node;

      var contextData = new NavigationContextData(props.controller, props.entry);
      using (cx.WriteContext(out var context)) {
        NavigationContextData.Key[in context] = contextData;
      }
      try {
        props.entry.Page?.Builder?.Invoke(ref cx, contextData);
      } catch (Exception exception) {
        props.controller.FailPresentation(props.entry, exception);
        throw;
      }
    }
  }

  [BoundaryComposable(Extension = false, UseLookupCache = true)]
  public partial class NavigationHostBoundary {
    public partial struct Props {
      public NavigationGraph graph;
      [Prop(null)] public NavigationController controller;
      [Prop(null, Equatable = false)] public INavigationTransition transition;
      [Prop(NavigationHostBehavior.Default)] public NavigationHostBehavior behavior;
    }

    private readonly List<NavigationPageBoundary> _pages = new(8);
    private NavigationTransitionRunner _transitionRunner;
    private bool _isAutomaticController;

    public NavigationController Controller { get; private set; }

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
      _transitionRunner ??= new NavigationTransitionRunner(this);
    }

    protected override void OnDetach() {
      Node.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
      _transitionRunner?.Cancel();
      Controller?.DetachPresenter(this);
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

      Node.MakeRelative().Flexible(1f, 1f, Align.Stretch);
      Node.focusable = true;
      Node.pickingMode = PickingMode.Ignore;

      _pages.Clear();
      var stack = Controller.PresentationStack;
      for (var i = 0; i < stack.Count; i++) {
        var entry = stack[i];
        var identity = unchecked((int)entry.Id ^ (int)(entry.Id >> 32));
        cx.AUTHORING.SetId(CompositionId.Generated(identity)); // TODO: Probably change this
        var presentation = Controller.PresentationOf(entry, IsCovered(stack, i));
        ref var result = ref NavigationPageBoundary.ComposeBoundary(ref cx, Controller, entry, presentation);
        if ((result.element as CompositionBoundaryNodeBase)?.BoundaryComposable
            is NavigationPageBoundary boundary) _pages.Add(boundary);
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
        } else {
          Controller.SetGraph(props.graph);
        }
      } else if (!ReferenceEquals(Controller, resolved)) {
        Controller?.DetachPresenter(this);
        if (_isAutomaticController) Controller?.Dispose();
        Controller = resolved;
        _isAutomaticController = false;
        Controller.SetGraph(props.graph, preserveStack: true);
      } else {
        Controller.SetGraph(props.graph, preserveStack: true);
      }
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

    private static bool IsCovered(IReadOnlyList<NavigationEntry> stack, int index) {
      for (var i = stack.Count - 1; i > index; i--) {
        if (stack[i].Opaque) return true;
      }
      return false;
    }

    internal IVisualElementScheduler Scheduler => Node.schedule;

    internal void TransitionCompleted(long changeId) => Controller?.CompletePresentation(changeId);
  }

  [BoundaryComposable(Extension = true, Name = "NavigationLink")]
  public partial class HXNavigationLink {
    public partial struct Props {
      public NavigationRoute route;
      [Prop(null)] public NavigationController controller;
      [Prop(null)] public Composable content;
      [Prop(true)] public bool enabled;
      [Prop(null)] public HXControlBoxStyle? style;
      [Prop("NavigationOptions.Default", PropInit.Deferred)]
      public NavigationOptions options;
    }

    private NavigationController _controller;

    protected override void OnRecompose(ref Composition cx) {
      _controller = props.controller ?? cx.ReadContext(NavigationContextData.Key, false).controller;
      if (_controller != null) cx.SubscribeTo(_controller);
      var available = _controller?.Graph?.Contains(props.route) == true;

      Node.pickingMode = PickingMode.Ignore;
      cx.Button(
        props.content ?? ComposeDefaultContent,
        Activate,
        props.enabled && available,
        _controller?.Current?.Route == props.route,
        props.style
      );
    }

    private static void ComposeDefaultContent(ref Composition cx) {
      var link = cx.Lookup<HXNavigationLink>();
      cx.Text(link?.props.route?.DisplayName ?? string.Empty);
    }

    private static void Activate(CompositionContext context) {
      var link = context.Lookup<HXNavigationLink>();
      if (link?._controller == null || link.props.route == null) return;
      link._controller.Activate(link.props.route, options: link.props.options);
    }
  }

  internal sealed class NavigationTransitionRunner {
    private readonly List<NavigationPageBoundary> _entering = new(2);
    private readonly List<NavigationPageBoundary> _exiting = new(2);
    private readonly List<NavigationPageBoundary> _removed = new(2);
    private readonly NavigationHostBoundary _host;
    private IVisualElementScheduledItem _scheduled;
    private INavigationTransition _transition;
    private NavigationOperationKind _operation;
    private NavigationDirection _direction;
    private float _elapsedMs;
    private long _changeId;
    private bool _active;

    internal NavigationTransitionRunner(NavigationHostBoundary host) {
      _host = host;
    }

    internal bool Matches(long changeId) => _active && _changeId == changeId;

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
      _elapsedMs += (float)state.deltaTime;
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
      for (var i = 0; i < _exiting.Count; i++) {
        if (!_removed.Contains(_exiting[i])) NavigationTransitions.Reset(_exiting[i].Element);
      }
    }

    private static bool Contains(IReadOnlyList<NavigationEntry> entries, NavigationEntry entry) {
      for (var i = 0; i < entries.Count; i++) {
        if (ReferenceEquals(entries[i], entry)) return true;
      }
      return false;
    }
  }

  public static class NavigationExtensions {
    public static ref ElementRef NavigationHost(
      this ref Composition cx,
      NavigationGraph graph,
      NavigationController controller = null,
      INavigationTransition transition = null,
      NavigationHostBehavior behavior = NavigationHostBehavior.Default
    ) {
      if (graph == null) throw new ArgumentNullException(nameof(graph));
      return ref NavigationHostBoundary.ComposeBoundary(ref cx, graph, controller, transition, behavior);
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
