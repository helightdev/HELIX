using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Theming;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class Widget : DiagnosticableTreeBase, IWidgetListCandidate {
        public object[] constants;
        public Key key;
        protected ModifierSet modifiers = ModifierSet.Empty;

        protected Widget(
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) {
            this.constants = constants;
            this.key = key;
            if (modifiers is ModifierSet set) this.modifiers = set;
            if (modifiers != null) AddModifiers(modifiers);
        }

        protected Widget() { }

        public abstract IWidgetElement CreateElement();

        public ModifierSet GetModifiers() {
            return modifiers;
        }

        public void AddModifier(Modifier modifier) {
            if (modifiers.ReadOnly) modifiers = new ModifierSet(modifiers);
            modifiers.AddThrowing(modifier);
        }

        public void AddModifiers(IEnumerable<Modifier> additions) {
            if (modifiers.ReadOnly) modifiers = new ModifierSet(modifiers);
            modifiers.AddThrowing(additions);
        }

        protected void DefaultModifiers(ModifierSet defaults, IReadOnlyCollection<Modifier> user) {
            if (user == null) {
                modifiers = defaults;
                return;
            }

            var applied = new ModifierSet(modifiers.Count + defaults.Count + user.Count);
            applied.AddCollection(modifiers);
            applied.AddCollection(defaults);
            applied.AddCollection(user);
            modifiers = applied;
        }

        public override string ToStringShort() {
            var name = GetWidgetName();
            if (key.IsNone) return name;
            return $"{name}-{key}";
        }

        public virtual string GetWidgetName() {
            return GetType().Name;
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            properties.Add(
                new IterableProperty<object>(
                    "retention",
                    constants,
                    ifNull: null,
                    identityOnly: true,
                    ifEmpty: "Constant",
                    level: constants == null ? DiagnosticLevel.Hidden : DiagnosticLevel.Info
                )
            );

            properties.Add(
                new IterableProperty<Modifier>(
                    "modifiers",
                    modifiers.Where(x => !x.isFallback).ToList(),
                    ifEmpty: null,
                    level: DiagnosticLevel.Info
                )
            );

            properties.Add(
                new IterableProperty<Modifier>(
                    "modifiers[fallback]",
                    modifiers.Where(x => x.isFallback).ToList(),
                    ifEmpty: null,
                    level: DiagnosticLevel.Fine
                )
            );

            base.DebugFillProperties(properties);
        }

        protected IWidgetElement ReconcileInto(IWidgetElement element) {
            element.Element.RegisterCallbackOnce<AttachToPanelEvent>(_ =>
                Reconciler.Reconcile(element, this)
            );
            return element;
        }

        public static implicit operator BuildFunction(Widget widget) {
            return _ => widget;
        }

        public static implicit operator BuildFunction<WidgetState>(Widget widget) {
            return (_, _) => widget;
        }
    }

    public abstract class SingleChildWidget : Widget, IEnumerable<Widget> {
        public Widget child;

        protected SingleChildWidget() { }

        protected SingleChildWidget(
            Widget child = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants, modifiers) {
            this.child = child;
        }

        public IEnumerator<Widget> GetEnumerator() {
            if (child != null) yield return child;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override List<DiagnosticsNode> DebugDescribeChildren() {
            return child == null
                ? new List<DiagnosticsNode>()
                : new List<DiagnosticsNode> { child.ToDiagnosticsNodeSafe() };
        }

        public void Add(Widget candidate) {
            if (child != null) throw new InvalidOperationException("SingleChildWidget already has a child");
            child = candidate;
        }
    }

    public abstract class MultiChildWidget : Widget, IReadOnlyList<Widget> {
        public IReadOnlyList<Widget> children;

        protected MultiChildWidget() { }

        protected MultiChildWidget(
            IReadOnlyList<Widget> children = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants, modifiers) {
            this.children = children;
        }

        public IEnumerator<Widget> GetEnumerator() {
            return children?.GetEnumerator() ?? Enumerable.Empty<Widget>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count => children?.Count ?? 0;

        public Widget this[int index] => children[index];

        public override List<DiagnosticsNode> DebugDescribeChildren() {
            return children.Select(child => child.ToDiagnosticsNodeSafe()).ToList();
        }

        public void Add(IWidgetListCandidate candidate) {
            if (children is WidgetList list) list.Add(candidate);
            else {
                var newList = new WidgetList();
                if (children != null) newList.Add(children.Spread());
                newList.Add(candidate);
                children = newList;
            }
        }
    }

    public interface IWidgetElement : BuildContext {
        int HierarchyDepth { get; }
        bool CanReconcile(Widget updated);
        bool Reconcile(Widget updated);
    }

    public interface ITreeAncestorTraversalHint {
        IWidgetElement Owner { get; }
    }

    public class ElementTreeAncestorTraversalHint : ITreeAncestorTraversalHint {
        public ElementTreeAncestorTraversalHint(IWidgetElement owner) {
            Owner = owner;
        }

        public IWidgetElement Owner { get; }
    }

    public static class WidgetExtensions {
        public static IBuildable ToBuildable(this Widget widget) {
            return new FunctionBuildable(_ => widget);
        }

        public static ElementFactory<VisualElement> ToFactory(this Widget widget, Action<VisualElement> apply = null) {
            return new InlineElementFactory<VisualElement>(_ => {
                    var element = new WidgetHostElement { Buildable = widget.ToBuildable() };
                    apply?.Invoke(element);
                    return element;
                }
            );
        }

        public static Widget ToWidget(this ElementFactory factory) {
            return new FactoryWidget<VisualElement> { creator = () => factory.Create(null) };
        }

        public static InformationCollector WithSpace(this InformationCollector collector) {
            collector.Add(new ErrorSpacer());
            return collector;
        }

        public static InformationCollector OffendingWidget(this InformationCollector collector, Widget widget) {
            collector.AddRange(new ErrorSpacer(), new ErrorProperty("The offending widget was", widget));
            return collector;
        }

        public static InformationCollector OffendingElement(
            this InformationCollector collector,
            IWidgetElement widget
        ) {
            collector.AddRange(new ErrorSpacer(), new ErrorProperty("The offending element was", widget));
            return collector;
        }

        public static InformationCollector OwnerChain(this InformationCollector collector, BuildContext context) {
            collector.AddRange(new ErrorSpacer(), OwnershipChainErrorProperty.FromBuildContext(context));
            return collector;
        }

        public static T Get<T>(this ThemeProperty<T> property, IThemeProvider context, bool listen = true) {
            if (context != null) return context.GetThemed(property, listen);
            return ThemeProviderElement.Resolve(null, property);
        }

        public static bool TryGet<T>(
            this ThemeProperty<T> property,
            IThemeProvider context,
            out T value,
            bool listen = true
        ) {
            if (context != null) return context.TryGetThemed(property, out value, listen);
            return ThemeProviderElement.TryResolve(null, property, out value);
        }
    }
}