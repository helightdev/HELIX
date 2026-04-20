using HELIX.Widgets;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using UnityEngine.UIElements;

namespace Examples {
    public class ButtonsExample : StatefulWidget<ButtonsExample> {

        public override State<ButtonsExample> CreateState() {
            return new ButtonsExampleState();
        }

    }

    public class ButtonsExampleState : State<ButtonsExample> {

        private ButtonController _controlledButton;
        private WidgetStateController _controlledButtonState;
        private bool _enabled = true;
        private bool _selected;

        public override void InitState() {
            base.InitState();
            _controlledButtonState = new WidgetStateController();
            _controlledButton = new ButtonController(_controlledButtonState);
        }

        public override Widget Build(BuildContext context) {
            return new HColumn(gap: 16f, crossAxisAlign: Align.Stretch) {
                new HRow(gap: 16f) {
                    new HButton(
                        HButtonVariant.TwoState,
                        selected: _enabled,
                        child: new HText("Toggle Enabled"),
                        onClick: () => {
                            _enabled = !_enabled;
                            SetState();
                        }
                    ),
                    new HButton(
                        HButtonVariant.TwoState,
                        selected: _selected,
                        child: new HText("Toggle Selected"),
                        onClick: () => {
                            _selected = !_selected;
                            SetState();
                        }
                    )
                },
                new HBox().Size(height: 32),
                VariantRow(), // Showcases all the variants of the button
                SizingRow(), // Showcases the different sizes and radii of the button

                new HBox().Size(height: 32),
                new HRow(gap: 16f) {
                    new HButton(
                        HButtonVariant.FlatTwoState,
                        child: new HText("Controlled Button"),
                        controller: _controlledButton
                    ),
                    new HButton(
                        HButtonVariant.Soft,
                        child: new HText("Same Controller"),
                        controller: _controlledButton
                    )
                }
            }.Padding(16);
        }

        private HRow VariantRow() {
            return new HRow(gap: 16f) {
                new HButton(
                    child: new HText("Theme Default"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Flat,
                    child: new HText("Flat"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.FlatTwoState,
                    child: new HText("Flat Two State"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Soft,
                    child: new HText("Soft"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.SoftTwoState,
                    child: new HText("Soft Two State"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Outline,
                    child: new HText("Outline"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.TwoState,
                    child: new HText("Two State"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Ghost,
                    child: new HText("Ghost"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand()
            };
        }

        private HRow SizingRow() {
            return new HRow(gap: 16f, crossAxisAlign: Align.FlexStart) {
                new HButton(
                    HButtonVariant.Outline,
                    size: HButtonSize.Small,
                    child: new HText("Small"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Outline,
                    size: HButtonSize.Regular,
                    child: new HText("Regular"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Outline,
                    size: HButtonSize.Medium,
                    child: new HText("Medium"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Outline,
                    size: HButtonSize.Large,
                    child: new HText("Large"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HBox().Expand(), //
                new HButton(
                    HButtonVariant.Outline,
                    radius: HInputRadius.None,
                    child: new HText("None"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Outline,
                    radius: HInputRadius.Small,
                    child: new HText("Small"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Outline,
                    radius: HInputRadius.Medium,
                    child: new HText("Medium"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Outline,
                    radius: HInputRadius.Large,
                    child: new HText("Large"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand(),
                new HButton(
                    HButtonVariant.Outline,
                    radius: HInputRadius.Full,
                    child: new HText("Full"),
                    enabled: _enabled,
                    selected: _selected
                ).Expand()
            };
        }

    }
}