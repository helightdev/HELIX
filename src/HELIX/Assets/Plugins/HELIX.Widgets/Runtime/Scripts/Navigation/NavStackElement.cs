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
    public partial class NavStackElement : SingleChildWidgetBaseElement<NavStack> {
        private readonly List<PageTransitionHandle> _activeTransitions = new();
        private readonly VisualElement _pageContainer;
        private readonly List<NavPage> _pages = new();
        private readonly Queue<StackOperation> _pendingOperations = new();

        private readonly VisualElement _rootContainer;
        private Action _afterTransitions;
        private bool _isTransitioning;
        private StackOperation _operation;
        private PageTransitionContext _transitionContext;

        private IVisualElementScheduledItem _updateSchedule;

        public NavStackElement() {
            this.Stretched();
            _rootContainer = new Element("Root").Stretched().AddTo(hierarchy);
            _pageContainer = new Element("Pages").Stretched().AddTo(hierarchy);
            _pageContainer.pickingMode = PickingMode.Ignore;
        }

        public IReadOnlyList<NavPage> Pages => _pages;
        public NavPage CurrentPage => Pages.Count == 0 ? null : _pages[^1];

        public override VisualElement contentContainer => _rootContainer;

        [UxmlObjectReference]
        public PageTransition DefaultTransition { get; set; }

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

        public Awaitable<bool> Pop(NavPage page, [CanBeNull] PageTransition transition = null) {
            var completionSource = new AwaitableCompletionSource<bool>();
            SubmitOperation(() => {
                    try {
                        if (page.parent != _pageContainer) {
                            completionSource.SetResult(false);
                            return;
                        }

                        var before = GetCurrentlyVisiblePages();
                        if (!before.Contains(page)) {
                            page.RemoveFromHierarchy();
                            Refresh();
                            completionSource.SetResult(true);
                            return;
                        }

                        var after = GetCurrentlyVisiblePages(Pages.Except(new[] { page }).ToList());
                        var modificationResult = new NavStackModificationResult(before, after, NavModificationType.Pop);
                        AnimateTransition(transition, modificationResult);
                        _afterTransitions = () => {
                            page.RemoveFromHierarchy();
                            Refresh();
                            completionSource.SetResult(true);
                        };
                    } catch (Exception e) { completionSource.SetException(e); }
                }
            );
            return completionSource.Awaitable;
        }

        public Awaitable<bool> PopUntil(Func<NavPage, bool> predicate, PageTransition transition = null) {
            var completionSource = new AwaitableCompletionSource<bool>();
            SubmitOperation(() => {
                    try {
                        var toRemove = new List<NavPage>();
                        for (var i = _pageContainer.childCount - 1; i >= 0; i--) {
                            var child = _pageContainer.ElementAt(i);
                            if (child is not NavPage page) continue;
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

        public Awaitable Push(NavPage page, [CanBeNull] PageTransition transition = null) {
            var completionSource = new AwaitableCompletionSource();
            SubmitOperation(() => {
                    try {
                        if (page.parent == _pageContainer) {
                            completionSource.SetResult();
                            return;
                        }

                        var before = GetCurrentlyVisiblePages();
                        var after = GetCurrentlyVisiblePages(before.pages.Append(page).ToList());
                        _pageContainer.Add(page);

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

        public Awaitable PushReplacement(NavPage page, [CanBeNull] PageTransition transition = null) {
            var completionSource = new AwaitableCompletionSource();
            SubmitOperation(() => {
                    try {
                        var lastPage = GetLastPage();
                        if (lastPage != null && lastPage == page) {
                            completionSource.SetResult();
                            return;
                        }

                        var before = GetCurrentlyVisiblePages();
                        var after = GetCurrentlyVisiblePages(
                            before.pages.Take(before.pages.Count - 1).Append(page).ToList()
                        );
                        _pageContainer.Add(page);
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
                if (child is not NavPage page) continue;
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

        private NavPage GetLastPage() {
            for (var i = _pageContainer.childCount - 1; i >= 0; i--) {
                var child = _pageContainer.ElementAt(i);
                if (child is not NavPage page) continue;
                return page;
            }

            return null;
        }

        private NavPageBuffer GetCurrentlyVisiblePages(
            List<NavPage> allPages = null,
            NavPage before = null
        ) {
            allPages ??= Pages.ToList();

            var beginCollecting = before == null;
            var buffer = new List<NavPage>();
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

        public override void Apply(NavStack previous, NavStack widget) {
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

    public class NavStack : SingleChildWidget {
        public PageTransition defaultTransition;

        public NavStack() {
            AddModifier(ModifierFallbacks.FlexFill);
        }

        public override IWidgetElement CreateElement() => ReconcileInto(new NavStackElement());

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<PageTransition>("defaultTransition", defaultTransition));
        }
    }

    public abstract class NavPage : VisualElement {
        public virtual bool Opaque => true;
    }

    public class NavPageBuffer : IEnumerable<NavPage> {
        public static readonly NavPageBuffer Empty = new(new List<NavPage>());
        public readonly IReadOnlyList<NavPage> pages;

        public NavPageBuffer(List<NavPage> pages) {
            this.pages = pages;
        }

        public bool IsEmpty => pages.Count == 0;
        public int Count => pages.Count;

        public IEnumerator<NavPage> GetEnumerator() {
            return pages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool Contains(NavPage page) {
            return pages.Contains(page);
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
            var beforeBuffer = new List<NavPage>(Before.pages);
            var afterBuffer = new List<NavPage>(After.pages);
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