using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HELIX.Abstractions;
using HELIX.Extensions;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Navigation {
  [UxmlElement]
  public partial class NavStackElement : SingleChildWidgetBaseElement<HNavStack> {
    private readonly List<PageTransitionHandle> _activeTransitions = new();
    private readonly VisualElement _pageContainer;
    private readonly List<NavPageBase> _pages = new();
    private readonly Queue<StackOperation> _pendingOperations = new();

    private readonly VisualElement _rootContainer;
    private Action _afterTransitions;
    private bool _isTransitioning;
    private StackOperation _operation;
    private PageTransitionContext _transitionContext;

    private IVisualElementScheduledItem _updateSchedule;

    public NavStackElement() {
      _rootContainer = new Element("Root").Stretched().AddTo(hierarchy);
      _pageContainer = new Element("Pages").Stretched().AddTo(hierarchy);
      _pageContainer.pickingMode = PickingMode.Ignore;
    }

    public IReadOnlyList<NavPageBase> Pages => _pages;
    public NavPageBase CurrentPageBase => Pages.Count == 0 ? null : _pages[^1];

    public override VisualElement contentContainer => _rootContainer;

    [UxmlObjectReference] public PageTransition DefaultTransition { get; set; }

    protected override void OnAttached(AttachToPanelEvent evt) {
      base.OnAttached(evt);
      Refresh();
      _updateSchedule = schedule.Execute(Update).Every(0);
      _updateSchedule.Pause();
    }

    private void Update() {
      if (_operation == null) {
        _updateSchedule.Pause();
        Debug.LogWarning("NavStack update called without an active operation. This should not happen.");
        return;
      }

      if (_activeTransitions.All(t => t.IsComplete)) EndOperation();
    }

    private void BeginOperation(StackOperation operation) {
      _operation = operation;
      operation.action();
      _updateSchedule.Resume();
      if (_activeTransitions.Count != 0) return;
      _updateSchedule.Pause();
      EndOperation();
    }

    private void EndOperation() {
      _transitionContext?.transition?.Finish(_transitionContext);

      _updateSchedule.Pause();
      _afterTransitions?.Invoke();
      _afterTransitions = null;
      _activeTransitions.Clear();
      _operation = null;
      _transitionContext = null;
      if (_pendingOperations.Count <= 0) return;
      var nextOperation = _pendingOperations.Dequeue();
      BeginOperation(nextOperation);
    }

    private void SubmitOperation(Action action) {
      var operation = new StackOperation { action = action };
      if (_operation != null) _pendingOperations.Enqueue(operation);
      else BeginOperation(operation);
    }

    private void AnimateTransition(PageTransition transition, NavStackModificationResult modificationResult) {
      transition ??= DefaultTransition;

      if (modificationResult.After.IsEmpty) _rootContainer.Display(_rootContainer.childCount > 0);
      else _pageContainer.Display(true);

      if (transition == null) return;
      _transitionContext = new PageTransitionContext(transition, this, modificationResult);
      transition.Start(_transitionContext);
      _activeTransitions.AddRange(_transitionContext.handles);
    }

    public Awaitable<bool> Pop(PageTransition transition = null) {
      var completionSource = new AwaitableCompletionSource<bool>();
      SubmitOperation(() => {
          try {
            var lastPage = GetLastPage();
            if (lastPage == null) {
              completionSource.SetResult(false);
              return;
            }

            var before = GetCurrentlyVisiblePages();
            var after = GetCurrentlyVisiblePages(Pages.Except(new[] { lastPage }).ToList());
            var modificationResult = new NavStackModificationResult(before, after, NavModificationType.Pop);
            AnimateTransition(transition, modificationResult);
            _afterTransitions = () => {
              lastPage.RemoveFromHierarchy();
              Refresh();
              completionSource.SetResult(true);
            };
          } catch (Exception e) { completionSource.SetException(e); }
        }
      );
      return completionSource.Awaitable;
    }

    public Awaitable<bool> Pop(NavPageBase pageBase, [CanBeNull] PageTransition transition = null) {
      var completionSource = new AwaitableCompletionSource<bool>();
      SubmitOperation(() => {
          try {
            if (pageBase.parent != _pageContainer) {
              completionSource.SetResult(false);
              return;
            }

            var before = GetCurrentlyVisiblePages();
            if (!before.Contains(pageBase)) {
              pageBase.RemoveFromHierarchy();
              Refresh();
              completionSource.SetResult(true);
              return;
            }

            var after = GetCurrentlyVisiblePages(Pages.Except(new[] { pageBase }).ToList());
            var modificationResult = new NavStackModificationResult(before, after, NavModificationType.Pop);
            AnimateTransition(transition, modificationResult);
            _afterTransitions = () => {
              pageBase.RemoveFromHierarchy();
              Refresh();
              completionSource.SetResult(true);
            };
          } catch (Exception e) { completionSource.SetException(e); }
        }
      );
      return completionSource.Awaitable;
    }

    public Awaitable<bool> PopUntil(Func<NavPageBase, bool> predicate, PageTransition transition = null) {
      var completionSource = new AwaitableCompletionSource<bool>();
      SubmitOperation(() => {
          try {
            var toRemove = new List<NavPageBase>();
            for (var i = _pageContainer.childCount - 1; i >= 0; i--) {
              var child = _pageContainer.ElementAt(i);
              if (child is not NavPageBase page) continue;
              if (predicate(page)) break;
              toRemove.Add(page);
            }

            if (toRemove.Count == 0) {
              completionSource.SetResult(false);
              return;
            }

            var before = GetCurrentlyVisiblePages();
            var after = GetCurrentlyVisiblePages(Pages.Except(toRemove).ToList());
            var modificationResult = new NavStackModificationResult(before, after, NavModificationType.Pop);
            AnimateTransition(transition, modificationResult);
            _afterTransitions = () => {
              foreach (var page in toRemove) page.RemoveFromHierarchy();

              Refresh();
              completionSource.SetResult(true);
            };
          } catch (Exception e) { completionSource.SetException(e); }
        }
      );

      return completionSource.Awaitable;
    }

    public Awaitable Push(NavPageBase pageBase, [CanBeNull] PageTransition transition = null) {
      var completionSource = new AwaitableCompletionSource();
      SubmitOperation(() => {
          try {
            if (pageBase.parent == _pageContainer) {
              completionSource.SetResult();
              return;
            }

            var before = GetCurrentlyVisiblePages();
            var after = GetCurrentlyVisiblePages(before.pages.Append(pageBase).ToList());
            _pageContainer.Add(pageBase);

            var modificationResult = new NavStackModificationResult(
              before,
              after,
              NavModificationType.Push
            );
            AnimateTransition(transition, modificationResult);

            _afterTransitions = () => {
              Refresh();
              completionSource.SetResult();
            };
          } catch (Exception e) { completionSource.SetException(e); }
        }
      );
      return completionSource.Awaitable;
    }

    public Awaitable PushReplacement(NavPageBase pageBase, [CanBeNull] PageTransition transition = null) {
      var completionSource = new AwaitableCompletionSource();
      SubmitOperation(() => {
          try {
            var lastPage = GetLastPage();
            if (lastPage != null && lastPage == pageBase) {
              completionSource.SetResult();
              return;
            }

            var before = GetCurrentlyVisiblePages();
            var after = GetCurrentlyVisiblePages(
              before.pages.Take(before.pages.Count - 1).Append(pageBase).ToList()
            );
            _pageContainer.Add(pageBase);
            var modificationResult = new NavStackModificationResult(
              before,
              after,
              NavModificationType.Replace
            );
            AnimateTransition(transition, modificationResult);

            _afterTransitions = () => {
              lastPage?.RemoveFromHierarchy();
              Refresh();
              completionSource.SetResult();
            };
          } catch (Exception e) { completionSource.SetException(e); }
        }
      );
      return completionSource.Awaitable;
    }

    public void Refresh() {
      _pages.Clear();
      var isHidden = false;
      foreach (var child in _pageContainer.Children().Reverse()) {
        if (child is not NavPageBase page) continue;
        page.style.opacity = 1f;

        _pages.Add(page);
        if (isHidden) {
          page.Display(false);
          continue;
        }

        page.Display(true);

        if (page.Opaque) isHidden = true;
      }

      _pages.Reverse();

      _pageContainer.Display(_pageContainer.childCount > 0);

      if (_rootContainer.childCount == 0) _rootContainer.Display(false);
      else _rootContainer.Display(!isHidden);
    }

    private NavPageBase GetLastPage() {
      for (var i = _pageContainer.childCount - 1; i >= 0; i--) {
        var child = _pageContainer.ElementAt(i);
        if (child is not NavPageBase page) continue;
        return page;
      }

      return null;
    }

    private NavPageBuffer GetCurrentlyVisiblePages(
      List<NavPageBase> allPages = null,
      NavPageBase before = null
    ) {
      allPages ??= Pages.ToList();

      var beginCollecting = before == null;
      var buffer = new List<NavPageBase>();
      for (var i = allPages.Count - 1; i >= 0; i--) {
        var page = allPages[i];
        switch (beginCollecting) {
          case false when page == before:
            beginCollecting = true;
            continue;
          case false: continue;
        }

        buffer.Add(page);
        if (page.Opaque) break;
      }

      buffer.Reverse();
      return new NavPageBuffer(buffer);
    }

    public override void Apply(HNavStack previous, HNavStack widget) {
      base.Apply(previous, widget);
      DefaultTransition = widget.defaultTransition;
    }

    public override bool Reconcile(Widget updated) {
      var result = base.Reconcile(updated);
      Refresh();
      return result;
    }

    public static NavStackElement Get(VisualElement context) {
      return context.GetFirstAncestorOfType<NavStackElement>();
    }

    public static NavStackElement GetNamed(VisualElement context, string name) {
      return context.panel.visualTree.Q<NavStackElement>(name);
    }

    private class StackOperation {
      public Action action;
    }
  }

  /// <summary>
  /// A widget that manages a stack of <see cref="NavPageBase"/>s.
  /// </summary>
  public class HNavStack : SingleChildWidget {
    public readonly PageTransition defaultTransition;

    /// <summary>
    /// Creates a widget that manages a stack of <see cref="NavPageBase"/>s.
    /// </summary>
    /// <param name="defaultTransition">
    /// The default transition to use for page changes if no transition is specified for a particular operation.
    /// </param>
    /// <param name="child">The root widget at the back of the stack.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    /// <inheritdoc/>
    public HNavStack(
      PageTransition defaultTransition = null,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants) {
      this.defaultTransition = defaultTransition;

      DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
    }

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new NavStackElement());
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new DiagnosticsProperty<PageTransition>("defaultTransition", defaultTransition));
    }
  }

  public abstract class NavPageBase : VisualElement {
    public virtual bool Opaque => true;
  }

  public class WidgetNavPage : NavPageBase {
    private readonly WidgetHostElement _host;

    public WidgetNavPage() {
      this.Stretched();
      _host = new WidgetHostElement().Stretched().AddTo(this);
    }

    public IBuildable Buildable {
      get => _host.Buildable;
      set => _host.Buildable = value;
    }
  }

  public class NavPageBuffer : IEnumerable<NavPageBase> {
    public static readonly NavPageBuffer Empty = new(new List<NavPageBase>());
    public readonly IReadOnlyList<NavPageBase> pages;

    public NavPageBuffer(List<NavPageBase> pages) {
      this.pages = pages;
    }

    public bool IsEmpty => pages.Count == 0;
    public int Count => pages.Count;

    public IEnumerator<NavPageBase> GetEnumerator() {
      return pages.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public bool Contains(NavPageBase pageBase) {
      return pages.Contains(pageBase);
    }
  }

  public class NavStackModificationResult {
    private readonly Lazy<NavPageBuffer> _addedPages;
    private readonly Lazy<int4> _minMaxIndices;
    private readonly Lazy<NavPageBuffer> _removedPages;
    private readonly Lazy<(NavPageBuffer, NavPageBuffer)> _trimmed;

    public NavStackModificationResult(
      NavPageBuffer before,
      NavPageBuffer after,
      NavModificationType type = NavModificationType.Complex
    ) {
      Before = before;
      After = after;
      _addedPages = new Lazy<NavPageBuffer>(GetAddedPages, false);
      _removedPages = new Lazy<NavPageBuffer>(GetRemovedPages, false);
      _trimmed = new Lazy<(NavPageBuffer, NavPageBuffer)>(TrimUnchanged, false);
      _minMaxIndices = new Lazy<int4>(GetMinMaxIndices, false);
      Type = type;
    }

    public NavPageBuffer Before { get; }

    public NavPageBuffer After { get; }

    public NavPageBuffer AddedPages => _addedPages.Value;
    public NavPageBuffer RemovedPages => _removedPages.Value;
    public int4 MinMaxDeltaIndices => _minMaxIndices.Value;
    public (NavPageBuffer TrimmedBefore, NavPageBuffer TrimmedAfter) Trimmed => _trimmed.Value;
    public NavPageBuffer TrimmedBefore => Trimmed.TrimmedBefore;
    public NavPageBuffer TrimmedAfter => Trimmed.TrimmedAfter;

    public NavModificationType Type { get; private set; }

    private (NavPageBuffer, NavPageBuffer) TrimUnchanged() {
      var minCount = Math.Min(Before.pages.Count, After.pages.Count);
      var beforeBuffer = new List<NavPageBase>(Before.pages);
      var afterBuffer = new List<NavPageBase>(After.pages);
      for (var i = 0; i < minCount; i++) {
        if (Before.pages[i] == After.pages[i]) continue;
        beforeBuffer.RemoveRange(i, Before.pages.Count - i);
        afterBuffer.RemoveRange(i, After.pages.Count - i);
        break;
      }

      return (new NavPageBuffer(beforeBuffer), new NavPageBuffer(afterBuffer));
    }

    private NavPageBuffer GetAddedPages() {
      var buffer = After.pages.Except(Before.pages).ToList();
      return new NavPageBuffer(buffer);
    }

    private NavPageBuffer GetRemovedPages() {
      var buffer = Before.pages.Except(After.pages).ToList();
      return new NavPageBuffer(buffer);
    }

    private int4 GetMinMaxIndices() {
      var minRemovedIndex = int.MaxValue;
      var maxRemovedIndex = int.MinValue;
      var minAddedIndex = int.MaxValue;
      var maxAddedIndex = int.MinValue;
      if (RemovedPages.IsEmpty) {
        minRemovedIndex = -1;
        maxRemovedIndex = -1;
      } else {
        foreach (var page in RemovedPages) {
          var index = page.parent.IndexOf(page);
          if (index < minRemovedIndex) minRemovedIndex = index;
          if (index > maxRemovedIndex) maxRemovedIndex = index;
        }
      }

      if (AddedPages.IsEmpty) {
        minAddedIndex = -1;
        maxAddedIndex = -1;
      } else {
        foreach (var page in AddedPages) {
          var index = page.parent.IndexOf(page);
          if (index < minAddedIndex) minAddedIndex = index;
          if (index > maxAddedIndex) maxAddedIndex = index;
        }
      }

      return new int4(minRemovedIndex, maxRemovedIndex, minAddedIndex, maxAddedIndex);
    }
  }

  public enum NavModificationType {
    Push,
    Pop,
    Replace,
    Complex
  }
}