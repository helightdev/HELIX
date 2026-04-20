using HELIX.Coloring.Material;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using UnityEngine.UIElements;

namespace Examples {
    public class ScrollControllerExample : StatefulWidget<ScrollControllerExample> {
        public override State<ScrollControllerExample> CreateState() {
            return new ScrollControllerExampleState();
        }
    }

    public class ScrollControllerExampleState : State<ScrollControllerExample> {
        private readonly ScrollController _controller = new();

        public override void Dispose() {
            base.Dispose();
            _controller.Dispose();
        }

        public override Widget Build(BuildContext context) {
            return new HColumn(
                gap: 16,
                crossAxisAlign: Align.Stretch,
                modifiers: new Modifier[] { MarginModifier.Of(16) }
            ) {
                new HText("Scroll controller mechanics: animated jumps + synced slider").Heading(context),
                new HRow(gap: 8) {
                    new HButton(
                        HButtonVariant.Flat,
                        onClick: () => { _controller.AnimateTo(0f, 0.35f); },
                        child: new HText("Top")
                    ),
                    new HButton(
                        HButtonVariant.Outline,
                        onClick: () => {
                            var middle = _controller.ScrollPosition.Max * 0.5f;
                            _controller.AnimateTo(middle, 0.45f);
                        },
                        child: new HText("Middle")
                    ),
                    new HButton(
                        HButtonVariant.Soft,
                        onClick: () => { _controller.AnimateTo(_controller.ScrollPosition.Max, 0.55f); },
                        child: new HText("Bottom")
                    )
                },
                new HRow(gap: 16f) {
                    new HBox(borderRadius: 8) {
                        new HScrollView(controller: _controller) {
                            new HBox(background: MaterialColors.Red.Value).Size(
                                height: 280,
                                width: StyleKeyword.Auto
                            ),
                            new HBox(background: MaterialColors.Blue.Value).Size(
                                height: 480,
                                width: StyleKeyword.Auto
                            ),
                            new HBox(background: MaterialColors.Green.Value).Size(
                                height: 640,
                                width: StyleKeyword.Auto
                            ),
                            new HBox(background: MaterialColors.Purple.Value).Size(
                                height: 360,
                                width: StyleKeyword.Auto
                            )
                        }
                    }.WithModifier(ClipModifier.Clip).Fill(),
                    new HSlider(_controller).TightStretch()
                }.Fill(),
                new HSlider(_controller, axis: Axis.Horizontal).TightStretch()
            };
        }
    }
}