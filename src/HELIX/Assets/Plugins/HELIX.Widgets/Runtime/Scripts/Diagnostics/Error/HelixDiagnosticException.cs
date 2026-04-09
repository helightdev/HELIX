using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Error {
    public sealed class HelixDiagnosticException : Exception {
        private readonly List<DiagnosticsNode> _parts;
        private string _cachedMessage;

        public HelixDiagnosticException(string message)
            : this(new DiagnosticsNode[] { new ErrorSummary(message) }) { }

        public HelixDiagnosticException(IEnumerable<DiagnosticsNode> diagnostics)
            : base(null) {
            _parts = diagnostics?.ToList() ?? new List<DiagnosticsNode>();
        }

        public IReadOnlyList<DiagnosticsNode> Diagnostics => _parts;

        public override string Message => _cachedMessage ??= ToString();

        public static HelixDiagnosticException FromParts(params DiagnosticsNode[] diagnostics) {
            return new HelixDiagnosticException(diagnostics);
        }

        public static HelixDiagnosticException FromException(Exception exception, string summary = null) {
            var parts = new List<DiagnosticsNode>();

            if (!string.IsNullOrEmpty(summary)) parts.Add(new ErrorSummary(summary));

            if (exception != null) {
                parts.Add(new ErrorDescription($"{exception.GetType().Name}: {exception.Message}"));

                if (!string.IsNullOrEmpty(exception.StackTrace))
                    parts.Add(new DiagnosticsStackTrace("StackTrace", exception.StackTrace));
            }

            return new HelixDiagnosticException(parts);
        }

        public string ToStringDeep(int wrapWidth = 100) {
            return BuildRootBlock().ToStringDeep(
                minLevel: DiagnosticLevel.Debug,
                wrapWidth: wrapWidth
            ).TrimStart();
        }

        public override string ToString() {
            if (_parts.Count == 0) return "UnityDiagnosticsException";

            return ToStringDeep();
        }

        public DiagnosticsNode ToDiagnosticsNode() {
            return BuildRootBlock();
        }

        private DiagnosticsBlock BuildRootBlock() {
            return new DiagnosticsBlock(
                "EXCEPTION",
                DiagnosticsTreeStyle.Error,
                false,
                description: string.Empty,
                properties: _parts
            );
        }
    }
}