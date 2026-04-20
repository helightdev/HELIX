using System;
using System.Collections.Generic;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class HSlider : StatefulWidget<HSlider> {

        public readonly Axis axis;

        public readonly WidgetStateProperty<ModifierSet> boxModifiers;
        public readonly SliderController controller;
        public readonly bool enabled;
        public readonly float initialValue;
        public readonly Action<float> onChanged;
        public readonly bool reverse;
        public readonly ScrollController scrollController;
        public readonly HSliderStyle style;
        public readonly float thumbSize;

        public Key focusKey;

        public HSlider(
            SliderController controller = null,
            Key focusKey = default,
            Axis axis = Axis.Horizontal,
            bool enabled = true,
            bool reverse = false,
            float initialValue = 0f,
            float thumbSize = -1f,
            Action<float> onChanged = null,
            WidgetStateProperty<ModifierSet> boxModifiers = null,
            HSliderStyle style = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants, modifiers) {
            this.controller = controller;
            this.focusKey = focusKey;
            this.axis = axis;
            this.enabled = enabled;
            this.reverse = reverse;
            this.initialValue = initialValue;
            this.thumbSize = thumbSize;
            this.onChanged = onChanged;
            this.style = style;
            this.boxModifiers = boxModifiers ?? WidgetStateProperties.Never<ModifierSet>();
        }

        public HSlider(
            ScrollController scrollController,
            Key focusKey = default,
            Axis axis = Axis.Vertical,
            bool enabled = true,
            bool reverse = false,
            float initialValue = 0f,
            float thumbSize = -1f,
            Action<float> onChanged = null,
            HSliderStyle style = null,
            WidgetStateProperty<ModifierSet> boxModifiers = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants, modifiers) {
            this.scrollController = scrollController;
            this.focusKey = focusKey;
            this.axis = axis;
            this.enabled = enabled;
            this.reverse = reverse;
            this.initialValue = initialValue;
            this.thumbSize = thumbSize;
            this.onChanged = onChanged;
            this.style = style;
            this.boxModifiers = boxModifiers ?? WidgetStateProperties.Never<ModifierSet>();
        }

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
                _controller = AddDisposable(new SliderController(_widgetStateController, widget.initialValue));
                if (widget.scrollController != null) _controller.LinkScrollController(widget.scrollController);

                _controller.onChanged = widget.onChanged;
                _controller.enabled = widget.enabled;
            }

            _widgetStateController?.Toggle(WidgetState.Disabled, !_controller.Enabled);
            _widgetStateController?.DisableEnable(
                WidgetState.Special1 | WidgetState.Special2,
                widget.axis == Axis.Horizontal ? WidgetState.Special1 : WidgetState.Special2
            );
            mount.Element.schedule.Execute(() => { _controller.RefreshFromLinkedScroll(); }).ExecuteLater(2);
        }

        public override bool CanReconcile(HSlider oldWidget) {
            return base.CanReconcile(oldWidget) && widget.controller == oldWidget.controller;
        }

        public override void DidUpdateWidget(HSlider oldWidget) {
            if (oldWidget.controller == null) {
                _controller.onChanged = widget.onChanged;
                _controller.enabled = widget.enabled;
            }

            _widgetStateController?.Toggle(WidgetState.Disabled, !_controller.Enabled);
            _widgetStateController?.DisableEnable(
                WidgetState.Special1 | WidgetState.Special2,
                widget.axis == Axis.Horizontal ? WidgetState.Special1 : WidgetState.Special2
            );
        }

        public override Widget Build(BuildContext context) {
            var style = widget.style ?? (widget.scrollController == null
                ? PrimitiveTheme.Slider.Get(context)
                : PrimitiveTheme.Scrollbar.Get(context));

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

            return new HStatefulBuilder((buildContext, parameter) =>
                BuildInner(buildContext, parameter, style, rootModifiers)
            );
        }

        // Run the composition inside an anonymous stateful widget so that the composite state properties don't get
        // rebuilt for every slider value change. This is still not really ideal, but for a slider it's fine.
        private Widget BuildInner(
            BuildContext context,
            State<HStatefulBuilder> parameter,
            HSliderStyle style,
            WidgetStateProperty<ModifierSet> rootModifiers
        ) {
            try {
                var state = _widgetStateController?.Value ?? WidgetState.None;
                var sliderValue = Mathf.Clamp01(_controller.Value);

                var thumbRange = widget.thumbSize >= 0 ? 0.05f : _controller.ThumbRange;
                var availableRange = Mathf.Max(0f, 1f - thumbRange);
                var thumbOffset = sliderValue * availableRange;
                var progressValue = Mathf.Clamp01(thumbOffset + thumbRange * 0.5f);

                return new HFlex(
                    key: widget.focusKey,
                    wrap: false,
                    axis: Axis.Horizontal,
                    modifiers: rootModifiers.ResolveOrDefault(state, ModifierSet.Empty)
                ) {
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
                }.Fill();
            } catch (Exception e) {
                Debug.LogError($"Error building HSlider: {e}");
                return new HText("Error").Fill();
            }
        }

        private Widget BuildLayer(SubstanceLayers layers, StyleLength4 position) {
            return new HSubstanceBox(
                _widgetStateController,
                layers
            ).Positioned(position);
        }

        private static StyleLength4 ProgressPosition(Axis axis, float progress, bool reverse) {
            var remainder = Mathf.Clamp01(1f - progress);
            var remainderStyle = new StyleLength(remainder.NormalizedPercent());

            return axis == Axis.Horizontal
                ? StyleLength4.Only(
                    reverse ? remainderStyle : 0,
                    top: 0,
                    right: reverse ? 0 : remainderStyle,
                    bottom: 0
                )
                : StyleLength4.Only(
                    0,
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
                    reverse ? endStyle : startStyle,
                    top: 0,
                    right: reverse ? startStyle : endStyle,
                    bottom: 0
                )
                : StyleLength4.Only(
                    0,
                    top: reverse ? endStyle : startStyle,
                    right: 0,
                    bottom: reverse ? startStyle : endStyle
                );
        }

    }
}