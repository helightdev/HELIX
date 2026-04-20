using HELIX.Widgets;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Utilities;
using UnityEngine.UIElements;

namespace Examples {
    public class TextInputExample : StatefulWidget<TextInputExample> {
        public override State<TextInputExample> CreateState() {
            return new TextInputExampleState();
        }
    }

    public class TextInputExampleState : State<TextInputExample> {
        private readonly GlobalKey _firstInputFocus = new();
        private readonly GlobalKey _secondInputFocus = new();
        private readonly TextEditingController _sharedController = new();

        public override void InitState() {
            base.InitState();
            _sharedController.SetValue("Type something...");
        }

        public override void Dispose() {
            base.Dispose();
            _sharedController.Dispose();
        }

        public override Widget Build(BuildContext context) {
            return new HColumn(gap: 16f, crossAxisAlign: Align.Stretch) {
                new HText($"Live value: {_sharedController.Value}").Body(context),
                new HRow(gap: 8f) {
                    new HButton(
                        HButtonVariant.Flat,
                        child: new HText("Append !"),
                        onClick: () => { _sharedController.SetValue(_sharedController.PeekValue() + "!"); }
                    ),
                    new HButton(
                        HButtonVariant.Outline,
                        child: new HText("Clear"),
                        onClick: () => { _sharedController.SetValue(string.Empty); }
                    ),
                    new HButton(
                        HButtonVariant.Soft,
                        child: new HText("Focus First"),
                        onClick: () => { _firstInputFocus.Focus(); }
                    ),
                    new HButton(
                        HButtonVariant.Soft,
                        child: new HText("Focus Second"),
                        onClick: () => { _secondInputFocus.Focus(); }
                    )
                },
                new HTextField(focusKey: _firstInputFocus, controller: _sharedController).TightStretch(),
                new HTextField(focusKey: _secondInputFocus, controller: _sharedController).TightStretch()
            }.Margin(16);
        }
    }
}