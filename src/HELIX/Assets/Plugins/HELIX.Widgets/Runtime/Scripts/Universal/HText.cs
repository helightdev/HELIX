using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class HText : WrappingBaseWidget<HText, Label> {
        private static readonly Modifier _spacingFallback =
            new SpacingModifier(StyleLength4.Zero, StyleLength4.Zero) { isFallback = true };

        public readonly bool doubleClickSelectsWords;
        public readonly bool emojiFallbackSupport;
        public readonly bool enableRichText;
        public readonly LanguageDirection languageDirection;
        public readonly bool parseEscapeSequences;
        public readonly bool selectable;
        public readonly TextStyle style;

        public readonly string text;
        public readonly bool tripleClickSelectsLine;

        public HText(
            string text,
            bool enableRichText = false,
            bool emojiFallbackSupport = true,
            bool parseEscapeSequences = true,
            bool selectable = false,
            bool doubleClickSelectsWords = true,
            bool tripleClickSelectsLine = true,
            LanguageDirection languageDirection = LanguageDirection.Inherit,
            TextStyle style = null
        ) {
            this.text = text;
            this.enableRichText = enableRichText;
            this.emojiFallbackSupport = emojiFallbackSupport;
            this.parseEscapeSequences = parseEscapeSequences;
            this.selectable = selectable;
            this.doubleClickSelectsWords = doubleClickSelectsWords;
            this.tripleClickSelectsLine = tripleClickSelectsLine;
            this.languageDirection = languageDirection;
            this.style = style;
            modifiers.Add(_spacingFallback);
        }

        public override Label Create() {
            return new Label();
        }

        public override void Apply(HText previous, Label element) {
            if (text != element.text) element.text = text;
            element.enableRichText = enableRichText;
            element.emojiFallbackSupport = emojiFallbackSupport;
            element.parseEscapeSequences = parseEscapeSequences;
            element.languageDirection = languageDirection;
            element.selection.isSelectable = selectable;
            element.selection.doubleClickSelectsWord = doubleClickSelectsWords;
            element.selection.tripleClickSelectsLine = tripleClickSelectsLine;

            if (previous == null || !Equals(style, previous.style)) (style ?? TextStyle.Default).Apply(element);
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new StringProperty("text", text));
            properties.Add(new TextStyleProperty("style", style));
            
            properties.Add(new FlagProperty("enableRichText", enableRichText, ifTrue: "RichText"));
            properties.Add(new FlagProperty("emojiFallbackSupport", emojiFallbackSupport, ifFalse: "NoEmojiFallback"));
            properties.Add(new FlagProperty("parseEscapeSequences", parseEscapeSequences, ifFalse: "NoEscapeSequenceParsing"));
            properties.Add(new FlagProperty("selectable", selectable, ifTrue: "Selectable"));
            properties.Add(new FlagProperty("doubleClickSelectsWords", doubleClickSelectsWords, ifFalse: "NoDoubleClickWordSelection"));
            properties.Add(new FlagProperty("tripleClickSelectsLine", tripleClickSelectsLine, ifFalse: "NoTripleClickLineSelection"));
            properties.Add(new EnumProperty<LanguageDirection>("languageDirection", languageDirection, defaultValue: LanguageDirection.Inherit));
        }
    }
}