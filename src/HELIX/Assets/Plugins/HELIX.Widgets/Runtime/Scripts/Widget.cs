using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Properties;
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
        
        public void AddModifiers(IEnumerable<Modifier> additions) {
            foreach (var modifier in additions) AddModifier(modifier);
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

    public interface IWidgetElement : BuildContext {
        int HierarchyDepth { get; }
        bool CanReconcile(Widget updated);
        bool Reconcile(Widget updated);
    }

    public interface ITreeAncestorTraversalHint {
        IWidgetElement Owner { get; }
    }

    public class ElementTreeAncestorTraversalHint : ITreeAncestorTraversalHint {
        public IWidgetElement Owner { get; }

        public ElementTreeAncestorTraversalHint(IWidgetElement owner) {
            Owner = owner;
        }
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
    }
}