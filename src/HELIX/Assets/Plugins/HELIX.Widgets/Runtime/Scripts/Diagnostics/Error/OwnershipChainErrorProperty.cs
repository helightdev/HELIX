using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Error {
    public class OwnershipChainErrorProperty : DiagnosticsProperty<List<BuildContext>> {
        public OwnershipChainErrorProperty(List<BuildContext> chain) : base(
            "The ownership chain of this widget was",
            chain,
            style: DiagnosticsTreeStyle.ErrorProperty,
            level: DiagnosticLevel.Info
        ) { }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            if (ValueTyped == null || ValueTyped.Count == 0) return "None";
            return string.Join(
                " <- ",
                ValueTyped.Select((x, i) => {
                        if (i == 0) return "<this>";
                        var name = x?.Descriptor?.GetWidgetName() ?? x?.ToStringShort();
                        return !string.IsNullOrEmpty(name) ? name : "Unknown";
                    }
                )
            );
        }

        public static OwnershipChainErrorProperty FromBuildContext(BuildContext context) {
            var chain = new List<BuildContext>();
            var current = context;
            while (current != null) {
                chain.Add(current);
                current = current.ParentContext;
            }

            return new OwnershipChainErrorProperty(chain);
        }
    }

    public class OffendingWidgetErrorProperty : DiagnosticsProperty<Widget> {
        public OffendingWidgetErrorProperty(Widget widget) : base(
            "The offending widget was",
            widget,
            style: DiagnosticsTreeStyle.ErrorProperty,
            level: DiagnosticLevel.Info
        ) { }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            return ValueTyped != null ? ValueTyped.GetType().Name : "None";
        }
    }

    public class ExceptionThrownErrorProperty : DiagnosticsProperty<Exception> {
        public ExceptionThrownErrorProperty(Exception exception) : base(
            "The following exception was thrown",
            exception,
            style: DiagnosticsTreeStyle.ErrorProperty,
            level: DiagnosticLevel.Info
        ) { }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            return ValueTyped != null ? $"{ValueTyped.GetType().Name}: {ValueTyped.Message}" : "None";
        }
    }
}