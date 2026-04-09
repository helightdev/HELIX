using System.Collections.Generic;
using System.Linq;
using HELIX.Coloring;
using HELIX.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Styles {
    public class BoxShadowStyle {
        public static readonly BoxShadowStyle None = new() {
            shadowColor = Colors.Transparent,
            blurRadius = 0f
        };

        public float blurRadius = 4f;
        public Vector4 borderRadius = new(0, 0, 0, 0);
        public EasingMode easingFunction = EasingMode.Linear;
        public Vector2 offset = Vector2.zero;
        public Color shadowColor = new(0, 0, 0, 0.25f);
        public float spreadRadius;
        public TimeValue transitionDuration = 0.1f;

        public void ApplyToElement(VisualElement element) {
            element.Positioned(
                left: offset.x - spreadRadius,
                top: offset.y - spreadRadius,
                bottom: -offset.y - spreadRadius,
                right: -offset.x - spreadRadius
            ).BorderRadius(borderRadius).BackgroundColor(shadowColor);

            var current = element.style.filter.value?.FirstOrDefault();
            if (current != null && current.Value.parameterCount == 1 &&
                Mathf.Approximately(current.Value.GetParameter(0).floatValue, blurRadius)) return;

            var function = new FilterFunction(FilterFunctionType.Blur);
            function.AddParameter(new FilterParameter(blurRadius));
            element.style.filter = new StyleList<FilterFunction>(new List<FilterFunction> { function });
        }
    }
}