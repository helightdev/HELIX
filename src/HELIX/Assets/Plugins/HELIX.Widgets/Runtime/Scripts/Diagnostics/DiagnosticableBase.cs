using System;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics {
    public interface IDiagnosticable {
        string ToStringShort() {
            return this.DescribeIdentity();
        }

        DiagnosticsNode ToDiagnosticsNode(string name = null, DiagnosticsTreeStyle? style = null) {
            return new DiagnosticableNode(name, this, style);
        }

        void DebugFillProperties(DiagnosticPropertiesBuilder properties) { }
    }

    public abstract class DiagnosticableBase : IDiagnosticable {
        public virtual string ToStringShort() {
            return this.DescribeIdentity();
        }

        public virtual DiagnosticsNode ToDiagnosticsNode(string name = null, DiagnosticsTreeStyle? style = null) {
            return new DiagnosticableNode(name, this, style);
        }

        public virtual void DebugFillProperties(DiagnosticPropertiesBuilder properties) { }

        public override string ToString() {
            return ToDiagnosticsNode(style: DiagnosticsTreeStyle.SingleLine).ToString();
        }
    }

    public static class DiagnosticableExtensions {
        public static DiagnosticsNode ToDiagnosticsNodeSafe(
            this IDiagnosticable diagnosticable,
            string name = null,
            DiagnosticsTreeStyle? style = null
        ) {
            if (diagnosticable == null) return DiagnosticsNode.Null;
            try { return diagnosticable.ToDiagnosticsNode(name, style); } catch (Exception ex) {
                return new DiagnosticsPropertyErrorNode("Error describing diagnosticable", ex);
            }
        }
    }
}