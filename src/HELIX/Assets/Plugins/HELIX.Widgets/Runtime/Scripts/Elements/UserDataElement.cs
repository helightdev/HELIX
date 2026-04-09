using System.Collections.Generic;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Formatting;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    public abstract class UserDataWidgetBaseElement : IWidgetElement {
        public VisualElement Element { get; set; }
        public Widget Descriptor { get; set; }
        public BuildContext ParentContext { get; set; }
        public int HierarchyDepth { get; set; }
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
            return ToDiagnosticsNode().ToStringDeep(prefixLineOne, prefixOtherLines, null, minLevel, wrapWidth);
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
            return ToDiagnosticsNode(style: DiagnosticsTreeStyle.SingleLine).ToString();
        }
    }

    public interface IUserDataWidget<W, E> where W : Widget, IUserDataWidget<W, E> where E : VisualElement {
        void Apply(W previous, E element) { }
    }

    public class UserDataWidgetElement<W, T> : UserDataWidgetBaseElement
        where T : VisualElement where W : Widget, IUserDataWidget<W, T> {
        public W TypedDescriptor {
            get => Descriptor as W;
            set => Descriptor = value;
        }

        public override bool CanReconcile(Widget updated) {
            return updated is W && Element is T;
        }

        public override bool Reconcile(Widget updated) {
            if (updated is not W typed) return false;
            if (Element is not T element) return false;
            typed.Apply(TypedDescriptor, element);
            Modifier.ApplyDelta(Descriptor, updated, element);
            Descriptor = updated;
            return true;
        }

        public override string ToStringShort() {
            return $"{TypedDescriptor.GetWidgetName()}:{typeof(T).Name}:UserData#{this.ShortHash()}";
        }
    }
}