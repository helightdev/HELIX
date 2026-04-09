using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics.Error;

namespace HELIX.Widgets.Diagnostics {
    public sealed class InformationCollector {
        private readonly List<DiagnosticsNodeBuilder> _builders = new();

        public InformationCollector Add(string message) {
            _builders.Add(() => new[] { new ErrorDescription(message) });
            return this;
        }

        public InformationCollector Add(DiagnosticsNode node) {
            _builders.Add(() => new[] { node });
            return this;
        }

        public InformationCollector AddRange(IEnumerable<DiagnosticsNode> nodes) {
            _builders.Add(() => nodes ?? Enumerable.Empty<DiagnosticsNode>());
            return this;
        }

        public InformationCollector AddRange(params DiagnosticsNode[] nodes) {
            _builders.Add(() => nodes ?? Enumerable.Empty<DiagnosticsNode>());
            return this;
        }

        public InformationCollector AddBuilder(DiagnosticsNodeBuilder builder) {
            if (builder != null) _builders.Add(builder);
            return this;
        }

        public List<DiagnosticsNode> Collect() {
            var result = new List<DiagnosticsNode>();

            foreach (var builder in _builders) {
                try {
                    var nodes = builder?.Invoke();
                    if (nodes != null) result.AddRange(nodes.Where(n => n != null));
                } catch (Exception ex) {
                    result.Add(new ErrorDescription("Error while collecting additional diagnostic information."));
                    result.Add(new DiagnosticsPropertyErrorNode("collector", ex));
                }
            }

            return result;
        }
    }
}