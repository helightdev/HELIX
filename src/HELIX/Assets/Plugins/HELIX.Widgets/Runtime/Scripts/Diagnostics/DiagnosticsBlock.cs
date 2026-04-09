using System.Collections.Generic;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics {
    public class DiagnosticsBlock : DiagnosticsNode {
        private readonly List<DiagnosticsNode> _children;
        private readonly string _description;
        private readonly List<DiagnosticsNode> _properties;

        public DiagnosticsBlock(
            string name = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.Whitespace,
            bool showName = true,
            bool showSeparator = true,
            string linePrefix = null,
            object value = null,
            string description = "",
            DiagnosticLevel level = DiagnosticLevel.Info,
            bool allowTruncate = false,
            List<DiagnosticsNode> children = null,
            List<DiagnosticsNode> properties = null
        )
            : base(name, style, showName && name != null, showSeparator, linePrefix) {
            ValueObject = value;
            _description = description ?? string.Empty;
            LevelValue = level;
            AllowTruncateValue = allowTruncate;
            _children = children ?? new List<DiagnosticsNode>();
            _properties = properties ?? new List<DiagnosticsNode>();
        }

        public object ValueObject { get; }
        public DiagnosticLevel LevelValue { get; }
        public bool AllowTruncateValue { get; }

        public override object Value => ValueObject;
        public override DiagnosticLevel Level => LevelValue;
        public override bool AllowTruncate => AllowTruncateValue;

        public override List<DiagnosticsNode> GetChildren() {
            return _children;
        }

        public override List<DiagnosticsNode> GetProperties() {
            return _properties;
        }

        public override string ToDescription(TextTreeConfiguration parentConfiguration = null) {
            return _description;
        }
    }
}