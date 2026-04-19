using HELIX.Widgets;
using HELIX.Widgets.Modifiers;
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
        private readonly TextEditingController _sharedController = new();
        private readonly GlobalKey _firstInputFocus = new();
        private readonly GlobalKey _secondInputFocus = new();

        public override void InitState() {
            base.InitState();
            _sharedController.SetValue("Type something...");
        }

        public override void Dispose() {
            base.Dispose();
            _sharedController.Dispose();
        }

        public override Widget Build(BuildContext context) {
            return new HColumn {
                gap = 16f,
                crossAxisAlign = Align.Stretch,
                Modifiers = new Modifier[] { MarginModifier.Of(16), },
                children = new Widget[] {
                    new HText($"Live value: {_sharedController.Value}").Body(context),
                    new HRow {
                        gap = 8,
                        children = new Widget[] {
                            new HButton {
                                variant = HButtonVariant.Flat,
                                child = new HText("Append !"),
                                onClick = () => { _sharedController.SetValue(_sharedController.PeekValue() + "!"); }
                            },
                            new HButton {
                                variant = HButtonVariant.Outline,
                                child = new HText("Clear"),
                                onClick = () => { _sharedController.SetValue(string.Empty); }
                            },
                            new HButton {
                                variant = HButtonVariant.Soft,
                                child = new HText("Focus First"),
                                onClick = () => { _firstInputFocus.Focus(); }
                            },
                            new HButton {
                                variant = HButtonVariant.Soft,
                                child = new HText("Focus Second"),
                                onClick = () => { _secondInputFocus.Focus(); }
                            }
                        }
                    },
                    new HTextField(
                        focusKey: _firstInputFocus,
                        controller: _sharedController
                    ).Flexible(selfCrossAxisAlign: Align.Stretch),
                    new HTextField(
                        focusKey: _secondInputFocus,
                        controller: _sharedController
                    ).Flexible(selfCrossAxisAlign: Align.Stretch),
                }
            };
        }
    }
}