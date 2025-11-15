using HELIX.Painting;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class PaintingWidget : BaseWidget {
        protected PaintingWidget() : base() {
            generateVisualContent += OnGenerateVisualContent;
        }

        protected virtual void OnGenerateVisualContent(MeshGenerationContext mgc) {
            var context = new PaintCanvas(mgc);
            Paint(context, context.canvasRect);
        }

        protected override ThemeValue<T> RegisterThemeValue<T>(ThemeValue<T> themeValue) {
            themeValue.OnValueChanged += _ => MarkDirtyRepaint();
            return base.RegisterThemeValue(themeValue);
        }

        public abstract void Paint(PaintCanvas canvas, Rect bounds);
    }
}