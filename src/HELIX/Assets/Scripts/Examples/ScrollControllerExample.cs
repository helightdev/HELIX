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
            return new HColumn {
                gap = 16,
                crossAxisAlign = Align.Stretch,
                Modifiers = new Modifier[] { MarginModifier.Of(16), },
                children = new Widget[] {
                    new HText("Scroll controller mechanics: animated jumps + synced slider").Heading(context),
                    new HRow {
                        gap = 8,
                        children = new Widget[] {
                            new HButton {
                                variant = HButtonVariant.Flat,
                                child = new HText("Top"),
                                onClick = () => { _controller.AnimateTo(0f, 0.35f); }
                            },
                            new HButton {
                                variant = HButtonVariant.Outline,
                                child = new HText("Middle"),
                                onClick = () => {
                                    var middle = _controller.ScrollPosition.Max * 0.5f;
                                    _controller.AnimateTo(middle, 0.45f);
                                }
                            },
                            new HButton {
                                variant = HButtonVariant.Soft,
                                child = new HText("Bottom"),
                                onClick = () => { _controller.AnimateTo(_controller.ScrollPosition.Max, 0.55f); }
                            }
                        }
                    },
                    new HRow {
                        gap = 16f,
                        children = new Widget[] {
                            new HBox {
                                borderRadius = BorderRadius.All(8),
                                child = new HScrollView {
                                    controller = _controller,
                                    children = new Widget[] {
                                        new HBox { backgroundStyle = MaterialColors.Red.Value }.Size(height: 280, width: StyleKeyword.Auto),
                                        new HBox { backgroundStyle = MaterialColors.Blue.Value }.Size(height: 480, width: StyleKeyword.Auto),
                                        new HBox { backgroundStyle = MaterialColors.Green.Value }.Size(height: 640, width: StyleKeyword.Auto),
                                        new HBox { backgroundStyle = MaterialColors.Purple.Value }.Size(height: 360, width: StyleKeyword.Auto),
                                    }
                                }
                            }.WithModifier(ClipModifier.Clip).Fill(),
                            new HSlider(_controller).Flexible(selfCrossAxisAlign: Align.Stretch),
                        }
                    }.Fill(),
                    new HSlider(_controller, axis: Axis.Horizontal).Flexible(selfCrossAxisAlign: Align.Stretch),
                }
            };
        }
    }
}