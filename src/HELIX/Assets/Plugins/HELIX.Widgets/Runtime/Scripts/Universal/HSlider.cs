using System;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Substances;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class HSlider : StatefulWidget<HSlider> {
        public SliderController controller;
        public Axis axis = Axis.Horizontal;
        public bool enabled = true;
        public bool reverse = false;
        public float value = 0f;
        public float thumbSize = -1;
        public Action<float> onChanged;

        public HSliderStyle style;
        public SubstanceLayers trackLayers = default;
        public SubstanceLayers progressLayers = default;
        public SubstanceLayers thumbLayers = default;

        public WidgetStateProperty<ModifierSet> boxModifiers = WidgetStateProperties.Never<ModifierSet>();

        public override State<HSlider> CreateState() {
            return new HSliderState();
        }
    }

    public class HSliderState : State<HSlider> {
        private SliderController _controller;
        private WidgetStateController _widgetStateController;

        public override void InitState() {
            if (widget.controller != null) {
                _controller = widget.controller;
                _widgetStateController = _controller.widgetState ?? AddDisposable(new WidgetStateController());
            } else {
                _widgetStateController = AddDisposable(new WidgetStateController());
                _controller = AddDisposable(new SliderController(_widgetStateController, widget.value));
                _controller.onChanged = widget.onChanged;
                _controller.enabled = widget.enabled;
            }

            _widgetStateController?.Toggle(WidgetState.Disabled, !_controller.Enabled);
            _widgetStateController?.DisableEnable(
                WidgetState.Special1 | WidgetState.Special2,
                widget.axis == Axis.Horizontal ? WidgetState.Special1 : WidgetState.Special2
            );
        }

        public override bool CanReconcile(HSlider oldWidget) {
            return base.CanReconcile(oldWidget) && widget.controller == oldWidget.controller;
        }

        public override void DidUpdateWidget(HSlider oldWidget) {
            if (oldWidget.controller == null) {
                _controller.onChanged = widget.onChanged;
                _controller.enabled = widget.enabled;
                if (!Mathf.Approximately(widget.value, oldWidget.value)) _controller.Value = widget.value;
            }

            _widgetStateController?.Toggle(WidgetState.Disabled, !_controller.Enabled);
            _widgetStateController?.DisableEnable(
                WidgetState.Special1 | WidgetState.Special2,
                widget.axis == Axis.Horizontal ? WidgetState.Special1 : WidgetState.Special2
            );
        }

        public override Widget Build(BuildContext context) {
            var style = widget.style ?? PrimitiveTheme.SliderTheme.Get(context);

            var state = _widgetStateController?.Value ?? WidgetState.None;
            var sliderValue = Mathf.Clamp01(_controller.Value);

            var thumbRange = widget.thumbSize >= 0 ? 0.05f : _controller.ThumbRange;
            var availableRange = Mathf.Max(0f, 1f - thumbRange);
            var thumbOffset = sliderValue * availableRange;
            var progressValue = Mathf.Clamp01(thumbOffset + thumbRange * 0.5f);

            var rootModifiers = WidgetStateProperties.Modifiers(
                widget.boxModifiers,
                WidgetStateProperties.Func(resolved => {
                        var set = new ModifierSet {
                            new SliderControllerModifier(
                                _controller,
                                widget.axis,
                                widget.thumbSize,
                                widget.reverse,
                                _widgetStateController
                            ),
                            SizeModifier.Of(style.constraints.ResolveOrDefault(resolved, BoxConstraints.Initial)),
                            FocusModifier.Focusable
                        };
                        if (_widgetStateController != null) set.Add(new WidgetStateModifier(_widgetStateController));
                        return set;
                    }
                )
            );

            return new HWrap {
                wrap = false,
                axis = Axis.Horizontal,
                children = new WidgetList {
                    BuildLayer(
                        style.track,
                        StyleLength4.Zero
                    ).If(style.track.Count > 0),
                    BuildLayer(
                        style.progress,
                        ProgressPosition(widget.axis, progressValue, widget.reverse)
                    ).If(style.progress.Count > 0),
                    BuildLayer(
                        style.thumb,
                        ThumbPosition(widget.axis, thumbOffset, availableRange, widget.reverse)
                    ).If(style.thumb.Count > 0)
                },
                Modifiers = rootModifiers.ResolveOrDefault(state, ModifierSet.Empty)
            }.Fill();
        }

        private Widget BuildLayer(SubstanceLayers layers, StyleLength4 position) {
            return new HSubstanceBox {
                controller = _widgetStateController,
                substances = layers
            }.Positioned(position, Position.Absolute);
        }

        private static SubstanceLayers DefaultTrackLayers() {
            return new[] {
                new BoxSubstance {
                    backgroundStyle = WidgetStateProperties.All<BackgroundStyle>(new Color(0.22f, 0.22f, 0.22f, 1f)),
                    borderRadius = WidgetStateProperties.All(BorderRadius.All(10))
                }
            };
        }

        private static SubstanceLayers DefaultProgressLayers() {
            return new[] {
                new BoxSubstance {
                    backgroundStyle = WidgetStateProperties.All<BackgroundStyle>(new Color(0.31f, 0.58f, 0.97f, 1f)),
                    borderRadius = WidgetStateProperties.All(BorderRadius.All(10))
                }
            };
        }

        private static SubstanceLayers DefaultThumbLayers() {
            return new[] {
                new BoxSubstance {
                    backgroundStyle = new WidgetStatePropertyMap<BackgroundStyle>() {
                        [WidgetState.Focused | WidgetState.Navigated] = new Color(0.12f, 0.72f, 0.24f, 1f),
                        [WidgetState.None] = new Color(0.92f, 0.92f, 0.92f, 1f),
                    },
                    borderRadius = WidgetStateProperties.All(BorderRadius.All(10f))
                }
            };
        }

        private static StyleLength4 ProgressPosition(Axis axis, float progress, bool reverse) {
            var remainder = Mathf.Clamp01(1f - progress);
            var remainderStyle = new StyleLength(remainder.NormalizedPercent());

            return axis == Axis.Horizontal
                ? StyleLength4.Only(
                    left: reverse ? remainderStyle : 0,
                    top: 0,
                    right: reverse ? 0 : remainderStyle,
                    bottom: 0
                )
                : StyleLength4.Only(
                    left: 0,
                    top: reverse ? remainderStyle : 0,
                    right: 0,
                    bottom: reverse ? 0 : remainderStyle
                );
        }

        private static StyleLength4 ThumbPosition(Axis axis, float offset, float availableRange, bool reverse) {
            var start = Mathf.Clamp01(offset);
            var end = Mathf.Clamp01(availableRange - start);
            var startStyle = new StyleLength(start.NormalizedPercent());
            var endStyle = new StyleLength(end.NormalizedPercent());

            return axis == Axis.Horizontal
                ? StyleLength4.Only(
                    left: reverse ? endStyle : startStyle,
                    top: 0,
                    right: reverse ? startStyle : endStyle,
                    bottom: 0
                )
                : StyleLength4.Only(
                    left: 0,
                    top: reverse ? endStyle : startStyle,
                    right: 0,
                    bottom: reverse ? startStyle : endStyle
                );
        }
    }
}