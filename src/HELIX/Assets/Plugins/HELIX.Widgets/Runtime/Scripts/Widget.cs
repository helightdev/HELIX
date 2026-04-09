using System.Collections.Generic;
using System.Linq;
using HELIX.Abstractions;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class Widget : DiagnosticableTreeBase, IWidgetListCandidate {
        protected readonly HashSet<Modifier> modifiers = new();
        public object[] constants;
        public Key key;

        public IReadOnlyList<Modifier> Modifiers {
            set {
                foreach (var modifier in value) AddModifier(modifier);
            }
        }

        public abstract IWidgetElement CreateElement();

        public HashSet<Modifier> GetModifiers() {
            return modifiers;
        }

        public void AddModifier(Modifier modifier) {
            if (modifiers.TryGetValue(modifier, out var existing)) {
                if (existing.isFallback) modifiers.Remove(existing);
                else {
                    Debug.LogWarning(
                        $"Modifier of type {modifier.GetType().Name} already exists on widget with key {key}. Modifiers must be unique per widget."
                    );
                    return;
                }
            }

            modifiers.Add(modifier);
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
                    "fallback modifiers",
                    modifiers.Where(x => x.isFallback).ToList(),
                    ifEmpty: null,
                    level: DiagnosticLevel.Fine
                )
            );

            base.DebugFillProperties(properties);
        }

        protected IWidgetElement ReconcileInto(IWidgetElement element) {
            element.Element.RegisterCallbackOnce<AttachToPanelEvent>(_ =>
                DefaultReconciler.PerformReconcileGuaranteed(element, this)
            );
            return element;
        }
    }

    public abstract class SingleChildWidget : Widget {
        public Widget child;

        public override List<DiagnosticsNode> DebugDescribeChildren() {
            return child == null
                ? new List<DiagnosticsNode>()
                : new List<DiagnosticsNode> { child.ToDiagnosticsNodeSafe() };
        }
    }

    public abstract class MultiChildWidget : Widget {
        public IReadOnlyList<Widget> children;

        public override List<DiagnosticsNode> DebugDescribeChildren() {
            return children.Select(child => child.ToDiagnosticsNodeSafe()).ToList();
        }
    }

    // ReSharper disable once InconsistentNaming
    public interface BuildContext : IDiagnosticableTree, IElement {
        static BuildContext Current = null;
        static BuildContext ReconcilerCurrent = null;
        Widget Descriptor { get; }
        BuildContext ParentContext { get; set; }

        protected bool IsUserWidget => this is IStatelessWidget || this is IStatefulWidget;

        T GetThemed<T>(ThemeProperty<T> property) {
            if (Element is not BaseElement baseElement) return property.defaultValue;
            return baseElement.ThemeProvider.Resolve(property);
        }

        static BuildContext GetUserTarget(BuildContext start, BuildContext except = null) {
            var current = start;
            while (current != null && (!current.IsUserWidget || current == except))
                current = current.ParentContext as IWidgetElement;
            return current ?? start;
        }
    }

    public interface IWidgetElement : BuildContext {
        int HierarchyDepth { get; }
        bool CanReconcile(Widget updated);
        bool Reconcile(Widget updated);
    }

    public static class WidgetExtensions {
        public static IBuildable ToConstantBuildable(this Widget widget) {
            return new FunctionBuildable(_ => widget);
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
    }
}