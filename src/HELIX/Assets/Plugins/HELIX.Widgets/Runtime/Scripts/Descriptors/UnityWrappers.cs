using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Descriptors {
    public class UnityLabel : WrappingBaseWidget<Label, UnityLabel> {
        private static readonly Modifier _spacingFallback =
            new SpacingModifier(StyleLength4.Zero, StyleLength4.Zero) { isFallback = true };

        public readonly string text;
        public readonly bool enableRichText;
        public readonly bool emojiFallbackSupport;
        public readonly bool parseEscapeSequences;
        public readonly bool selectable;
        public readonly bool doubleClickSelectsWords;
        public readonly bool tripleClickSelectsLine;
        public readonly LanguageDirection languageDirection;
        public readonly TextStyle style;

        public UnityLabel(
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

        public override void Apply(UnityLabel previous, Label element) {
            if (text != element.text) element.text = text;
            element.enableRichText = enableRichText;
            element.emojiFallbackSupport = emojiFallbackSupport;
            element.parseEscapeSequences = parseEscapeSequences;
            element.languageDirection = languageDirection;
            element.selection.isSelectable = selectable;
            element.selection.doubleClickSelectsWord = doubleClickSelectsWords;
            element.selection.tripleClickSelectsLine = tripleClickSelectsLine;

            if (previous == null || !Equals(style, previous.style)) { (style ?? TextStyle.Default).Apply(element); }
        }
    }

    public struct ButtonStyle {
        public TextStyle textStyle;
        public BackgroundStyle backgroundStyle;
        public BorderRadius borderRadius;
        public Border border;
    }
    
    public class UnityButton : WrappingBaseWidget<Button, UnityButton> {
        private static readonly Modifier _spacingFallback =
            new SpacingModifier(StyleLength4.Zero, StyleLength4.All(10)) { isFallback = true };
        
        public readonly string text;
        public readonly bool enableRichText;
        public readonly bool emojiFallbackSupport;
        public readonly bool parseEscapeSequences;
        public readonly LanguageDirection languageDirection;
        public readonly TextStyle textStyle;
        public readonly BackgroundStyle backgroundStyle;
        public readonly BorderRadius borderRadius;
        public readonly Border border;

        public UnityButton(
            string text,
            TextStyle textStyle = null,
            BackgroundStyle backgroundStyle = null,
            bool enableRichText = false,
            bool emojiFallbackSupport = true,
            bool parseEscapeSequences = true,
            LanguageDirection languageDirection = LanguageDirection.Inherit
        ) {
            this.text = text;
            this.enableRichText = enableRichText;
            this.emojiFallbackSupport = emojiFallbackSupport;
            this.parseEscapeSequences = parseEscapeSequences;
            this.languageDirection = languageDirection;
            this.textStyle = textStyle;
            this.backgroundStyle = backgroundStyle;
            modifiers.Add(_spacingFallback);
        }

        public override Button Create() {
            return new Button();
        }

        public override void Apply(UnityButton previous, Button element) {
            if (text != element.text) element.text = text;
            element.enableRichText = enableRichText;
            element.emojiFallbackSupport = emojiFallbackSupport;
            element.parseEscapeSequences = parseEscapeSequences;
            element.languageDirection = languageDirection;

            if (previous == null || !Equals(textStyle, previous.textStyle)) {
                (textStyle ?? TextStyle.Default).Apply(element);
            }

            if (previous == null || !Equals(backgroundStyle, previous.backgroundStyle)) {
                (backgroundStyle ?? BackgroundStyle.Default).Apply(element);
            }
        }
    }
}