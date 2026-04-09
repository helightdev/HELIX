using System.Collections.Generic;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics {
    public sealed class DiagnosticPropertiesBuilder {
        public DiagnosticPropertiesBuilder() {
            Properties = new List<DiagnosticsNode>();
            DefaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.Sparse;
        }

        public DiagnosticPropertiesBuilder(List<DiagnosticsNode> properties) {
            Properties = properties ?? new List<DiagnosticsNode>();
            DefaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.Sparse;
        }

        public List<DiagnosticsNode> Properties { get; }
        public DiagnosticsTreeStyle DefaultDiagnosticsTreeStyle { get; set; }
        public string EmptyBodyDescription { get; set; }

        public void Add(DiagnosticsNode property) {
            if (property != null) Properties.Add(property);
        }
    }
}