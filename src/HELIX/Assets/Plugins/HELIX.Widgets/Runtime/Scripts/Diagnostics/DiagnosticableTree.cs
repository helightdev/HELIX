using System.Collections.Generic;
using System.Linq;
using System.Text;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics {
    public interface IDiagnosticableTree : IDiagnosticable {

        List<DiagnosticsNode> DebugDescribeChildren() {
            return new List<DiagnosticsNode>();
        }

        string ToStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = null,
            DiagnosticLevel minLevel = DiagnosticLevel.Debug,
            int wrapWidth = 65
        ) {
            return ToDiagnosticsNode().ToStringDeep(prefixLineOne, prefixOtherLines, null, minLevel, wrapWidth);
        }

        string ToStringShallow(string joiner = ", ", DiagnosticLevel minLevel = DiagnosticLevel.Debug) {
            return DefaultToStringShallow(this, joiner, minLevel);
        }

        static string DefaultToStringShallow(
            IDiagnosticableTree tree,
            string joiner = ", ",
            DiagnosticLevel minLevel = DiagnosticLevel.Debug
        ) {
            var result = new StringBuilder();
            result.Append(tree.ToStringShort());
            result.Append(joiner);
            var builder = new DiagnosticPropertiesBuilder();
            tree.DebugFillProperties(builder);
            result.Append(
                string.Join(joiner, builder.Properties.Where(n => !n.IsFiltered(minLevel)).Select(n => n.ToString()))
            );
            return result.ToString();
        }

    }

    public sealed class DiagnosticableTreeNode : DiagnosticableNode {

        private readonly IDiagnosticableTree _tree;

        public DiagnosticableTreeNode(string name, IDiagnosticableTree value, DiagnosticsTreeStyle? style)
            : base(name, value, style) {
            _tree = value;
        }

        public override List<DiagnosticsNode> GetChildren() {
            return _tree.DebugDescribeChildren();
        }

    }

    public abstract class DiagnosticableTreeBase : DiagnosticableBase, IDiagnosticableTree {

        public virtual List<DiagnosticsNode> DebugDescribeChildren() {
            return new List<DiagnosticsNode>();
        }

        public virtual string ToStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = null,
            DiagnosticLevel minLevel = DiagnosticLevel.Debug,
            int wrapWidth = 65
        ) {
            return ToDiagnosticsNode().ToStringDeep(
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

        public override DiagnosticsNode ToDiagnosticsNode(string name = null, DiagnosticsTreeStyle? style = null) {
            return new DiagnosticableTreeNode(name, this, style);
        }

    }
}