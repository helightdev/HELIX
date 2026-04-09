using System.Collections.Generic;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics {
    public class DiagnosticableNode : DiagnosticsNode {
        private DiagnosticPropertiesBuilder _cachedBuilder;

        public DiagnosticableNode(string name, IDiagnosticable value, DiagnosticsTreeStyle? style)
            : base(name, style) {
            ValueTyped = value;
        }

        public IDiagnosticable ValueTyped { get; }
        public override object Value => ValueTyped;

        private DiagnosticPropertiesBuilder Builder {
            get {
                if (_cachedBuilder == null) {
                    _cachedBuilder = new DiagnosticPropertiesBuilder();
                    ValueTyped.DebugFillProperties(_cachedBuilder);
                }

                return _cachedBuilder;
            }
        }

        public override string EmptyBodyDescription => Builder.EmptyBodyDescription;

        public override string ToDescription(TextTreeConfiguration parentConfiguration = null) {
            return ValueTyped.ToStringShort();
        }

        public override List<DiagnosticsNode> GetProperties() {
            return Builder.Properties;
        }

        public override List<DiagnosticsNode> GetChildren() {
            return new List<DiagnosticsNode>();
        }
    }
}