using HELIX.Types;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class Text : WrappingBaseWidget<Text, Label> {
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

        public Text(
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

        public override void Apply(Text previous, Label element) {
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
    }
}