using System;
using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class HError : StatelessWidget<HError> {

        private static readonly ModifierSet _defaultModifiers = new ModifierSet(3) {
            ModifierFallbacks.FlexFill,
            ModifierFallbacks.StackingStretch,
            SizeModifier.Min(new StyleLength2(16)).Fallback(),
        }.Sealed();

        public readonly string message;
        public readonly Exception exception;
        private bool _reported;

        public HError(
            string message,
            Exception exception = null,
            bool report = true,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants) {
            this.message = message;
            this.exception = exception;

            DefaultModifiers(_defaultModifiers, modifiers);
        }

        public HError ReportIn(BuildContext context) {
            if (_reported) return this;
            HelixDiagnostics.Build(
                message,
                collector => collector.OwnerChain(context).OffendingElement(context as IWidgetElement),
                exception
            ).Report(DiagnosticLevel.Error);
            _reported = true;
            return this;
        }

        public override Widget Build(BuildContext context) {
            var colors = PrimitiveBaseTheme.Colors.Get(context, false);
            var line = $"Error: {message}";
            return new HBox(background: colors.error.container, child: new HText(line, style: new TextStyle {
                color = colors.error.onContainer
            }));
        }

    }
}