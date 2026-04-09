using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Elements {
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public abstract class WidgetBaseElement : BaseElement, IWidgetElement {
        public Widget Descriptor { get; set; }
        public BuildContext ParentContext { get; set; }
        public abstract bool CanReconcile(Widget updated);
        public abstract bool Reconcile(Widget updated);

        public virtual List<DiagnosticsNode> DebugDescribeChildren() {
            return new List<DiagnosticsNode>();
        }

        public virtual string ToStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = null,
            DiagnosticLevel minLevel = DiagnosticLevel.Debug,
            int wrapWidth = 65
        ) {
            return this.ToDiagnosticsNodeSafe().ToStringDeep(
                prefixLineOne,
                prefixOtherLines,
                null,
                minLevel,
                wrapWidth
            );
        }

        public virtual string ToStringShallow(string joiner = ", ", DiagnosticLevel minLevel = DiagnosticLevel.Debug) {
            return IDiagnosticableTree.DefaultToStringShallow(this, joiner, minLevel);
        }

        public virtual DiagnosticsNode ToDiagnosticsNode(string name = null, DiagnosticsTreeStyle? style = null) {
            return new DiagnosticableTreeNode(name, this, style);
        }

        public virtual string ToStringShort() {
            return this.DescribeIdentity();
        }

        public virtual void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            properties.Add(new DiagnosticsProperty<Widget>("descriptor", Descriptor, showName: false));
        }

        public override string ToString() {
            return this.ToDiagnosticsNodeSafe(style: DiagnosticsTreeStyle.SingleLine).ToString();
        }
    }

    public abstract class WidgetBaseElement<T> : WidgetBaseElement, IWidgetElement where T : Widget {
        public T TypedDescriptor {
            get => Descriptor as T;
            set => Descriptor = value;
        }

        public override bool CanReconcile(Widget updated) {
            return updated is T;
        }
    }
}