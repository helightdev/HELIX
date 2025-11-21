using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public readonly struct UpdateStyle : IStyle {
        private readonly IStyle _delegated;
        public UpdateStyle(IStyle style) {
            _delegated = style;
        }

        public UpdateStyle(VisualElement element) {
            _delegated = element.style;
        }

        public StyleEnum<Align> alignContent {
            get => _delegated.alignContent;
            set => _delegated.alignContent = value;
        }
        public StyleEnum<Align> alignItems {
            get => _delegated.alignItems;
            set => _delegated.alignItems = value;
        }
        public StyleEnum<Align> alignSelf {
            get => _delegated.alignSelf;
            set => _delegated.alignSelf = value;
        }
        public StyleRatio aspectRatio {
            get => _delegated.aspectRatio;
            set => _delegated.aspectRatio = value;
        }
        public StyleColor backgroundColor {
            get => _delegated.backgroundColor;
            set => _delegated.backgroundColor = value;
        }
        public StyleBackground backgroundImage {
            get => _delegated.backgroundImage;
            set => _delegated.backgroundImage = value;
        }
        public StyleBackgroundPosition backgroundPositionX {
            get => _delegated.backgroundPositionX;
            set => _delegated.backgroundPositionX = value;
        }
        public StyleBackgroundPosition backgroundPositionY {
            get => _delegated.backgroundPositionY;
            set => _delegated.backgroundPositionY = value;
        }
        public StyleBackgroundRepeat backgroundRepeat {
            get => _delegated.backgroundRepeat;
            set => _delegated.backgroundRepeat = value;
        }
        public StyleBackgroundSize backgroundSize {
            get => _delegated.backgroundSize;
            set => _delegated.backgroundSize = value;
        }
        public StyleColor borderBottomColor {
            get => _delegated.borderBottomColor;
            set => _delegated.borderBottomColor = value;
        }
        public StyleLength borderBottomLeftRadius {
            get => _delegated.borderBottomLeftRadius;
            set => _delegated.borderBottomLeftRadius = value;
        }
        public StyleLength borderBottomRightRadius {
            get => _delegated.borderBottomRightRadius;
            set => _delegated.borderBottomRightRadius = value;
        }
        public StyleFloat borderBottomWidth {
            get => _delegated.borderBottomWidth;
            set => _delegated.borderBottomWidth = value;
        }
        public StyleColor borderLeftColor {
            get => _delegated.borderLeftColor;
            set => _delegated.borderLeftColor = value;
        }
        public StyleFloat borderLeftWidth {
            get => _delegated.borderLeftWidth;
            set => _delegated.borderLeftWidth = value;
        }
        public StyleColor borderRightColor {
            get => _delegated.borderRightColor;
            set => _delegated.borderRightColor = value;
        }
        public StyleFloat borderRightWidth {
            get => _delegated.borderRightWidth;
            set => _delegated.borderRightWidth = value;
        }
        public StyleColor borderTopColor {
            get => _delegated.borderTopColor;
            set => _delegated.borderTopColor = value;
        }
        public StyleLength borderTopLeftRadius {
            get => _delegated.borderTopLeftRadius;
            set => _delegated.borderTopLeftRadius = value;
        }
        public StyleLength borderTopRightRadius {
            get => _delegated.borderTopRightRadius;
            set => _delegated.borderTopRightRadius = value;
        }
        public StyleFloat borderTopWidth {
            get => _delegated.borderTopWidth;
            set => _delegated.borderTopWidth = value;
        }
        public StyleLength bottom {
            get => _delegated.bottom;
            set => _delegated.bottom = value;
        }
        public StyleColor color {
            get => _delegated.color;
            set => _delegated.color = value;
        }
        public StyleCursor cursor {
            get => _delegated.cursor;
            set => _delegated.cursor = value;
        }
        public StyleEnum<DisplayStyle> display {
            get => _delegated.display;
            set => _delegated.display = value;
        }
        public StyleList<FilterFunction> filter {
            get => _delegated.filter;
            set => _delegated.filter = value;
        }
        public StyleLength flexBasis {
            get => _delegated.flexBasis;
            set => _delegated.flexBasis = value;
        }
        public StyleEnum<FlexDirection> flexDirection {
            get => _delegated.flexDirection;
            set => _delegated.flexDirection = value;
        }
        public StyleFloat flexGrow {
            get => _delegated.flexGrow;
            set => _delegated.flexGrow = value;
        }
        public StyleFloat flexShrink {
            get => _delegated.flexShrink;
            set => _delegated.flexShrink = value;
        }
        public StyleEnum<Wrap> flexWrap {
            get => _delegated.flexWrap;
            set => _delegated.flexWrap = value;
        }
        public StyleLength fontSize {
            get => _delegated.fontSize;
            set => _delegated.fontSize = value;
        }
        public StyleLength height {
            get => _delegated.height;
            set => _delegated.height = value;
        }
        public StyleEnum<Justify> justifyContent {
            get => _delegated.justifyContent;
            set => _delegated.justifyContent = value;
        }
        public StyleLength left {
            get => _delegated.left;
            set => _delegated.left = value;
        }
        public StyleLength letterSpacing {
            get => _delegated.letterSpacing;
            set => _delegated.letterSpacing = value;
        }
        public StyleLength marginBottom {
            get => _delegated.marginBottom;
            set => _delegated.marginBottom = value;
        }
        public StyleLength marginLeft {
            get => _delegated.marginLeft;
            set => _delegated.marginLeft = value;
        }
        public StyleLength marginRight {
            get => _delegated.marginRight;
            set => _delegated.marginRight = value;
        }
        public StyleLength marginTop {
            get => _delegated.marginTop;
            set => _delegated.marginTop = value;
        }
        public StyleLength maxHeight {
            get => _delegated.maxHeight;
            set => _delegated.maxHeight = value;
        }
        public StyleLength maxWidth {
            get => _delegated.maxWidth;
            set => _delegated.maxWidth = value;
        }
        public StyleLength minHeight {
            get => _delegated.minHeight;
            set => _delegated.minHeight = value;
        }
        public StyleLength minWidth {
            get => _delegated.minWidth;
            set => _delegated.minWidth = value;
        }
        public StyleFloat opacity {
            get => _delegated.opacity;
            set => _delegated.opacity = value;
        }
        public StyleEnum<Overflow> overflow {
            get => _delegated.overflow;
            set => _delegated.overflow = value;
        }
        public StyleLength paddingBottom {
            get => _delegated.paddingBottom;
            set => _delegated.paddingBottom = value;
        }
        public StyleLength paddingLeft {
            get => _delegated.paddingLeft;
            set => _delegated.paddingLeft = value;
        }
        public StyleLength paddingRight {
            get => _delegated.paddingRight;
            set => _delegated.paddingRight = value;
        }
        public StyleLength paddingTop {
            get => _delegated.paddingTop;
            set => _delegated.paddingTop = value;
        }
        public StyleEnum<Position> position {
            get => _delegated.position;
            set => _delegated.position = value;
        }
        public StyleLength right {
            get => _delegated.right;
            set => _delegated.right = value;
        }
        public StyleRotate rotate {
            get => _delegated.rotate;
            set => _delegated.rotate = value;
        }
        public StyleScale scale {
            get => _delegated.scale;
            set => _delegated.scale = value;
        }
        public StyleEnum<TextOverflow> textOverflow {
            get => _delegated.textOverflow;
            set => _delegated.textOverflow = value;
        }
        public StyleTextShadow textShadow {
            get => _delegated.textShadow;
            set => _delegated.textShadow = value;
        }
        public StyleLength top {
            get => _delegated.top;
            set => _delegated.top = value;
        }
        public StyleTransformOrigin transformOrigin {
            get => _delegated.transformOrigin;
            set => _delegated.transformOrigin = value;
        }
        public StyleList<TimeValue> transitionDelay {
            get => _delegated.transitionDelay;
            set => _delegated.transitionDelay = value;
        }
        public StyleList<TimeValue> transitionDuration {
            get => _delegated.transitionDuration;
            set => _delegated.transitionDuration = value;
        }
        public StyleList<StylePropertyName> transitionProperty {
            get => _delegated.transitionProperty;
            set => _delegated.transitionProperty = value;
        }
        public StyleList<EasingFunction> transitionTimingFunction {
            get => _delegated.transitionTimingFunction;
            set => _delegated.transitionTimingFunction = value;
        }
        public StyleTranslate translate {
            get => _delegated.translate;
            set => _delegated.translate = value;
        }
        public StyleColor unityBackgroundImageTintColor {
            get => _delegated.unityBackgroundImageTintColor;
            set => _delegated.unityBackgroundImageTintColor = value;
        }
        public StyleEnum<EditorTextRenderingMode> unityEditorTextRenderingMode {
            get => _delegated.unityEditorTextRenderingMode;
            set => _delegated.unityEditorTextRenderingMode = value;
        }
        public StyleFont unityFont {
            get => _delegated.unityFont;
            set => _delegated.unityFont = value;
        }
        public StyleFontDefinition unityFontDefinition {
            get => _delegated.unityFontDefinition;
            set => _delegated.unityFontDefinition = value;
        }
        public StyleEnum<FontStyle> unityFontStyleAndWeight {
            get => _delegated.unityFontStyleAndWeight;
            set => _delegated.unityFontStyleAndWeight = value;
        }
        public StyleMaterialDefinition unityMaterial {
            get => _delegated.unityMaterial;
            set => _delegated.unityMaterial = value;
        }
        public StyleEnum<OverflowClipBox> unityOverflowClipBox {
            get => _delegated.unityOverflowClipBox;
            set => _delegated.unityOverflowClipBox = value;
        }
        public StyleLength unityParagraphSpacing {
            get => _delegated.unityParagraphSpacing;
            set => _delegated.unityParagraphSpacing = value;
        }
        public StyleInt unitySliceBottom {
            get => _delegated.unitySliceBottom;
            set => _delegated.unitySliceBottom = value;
        }
        public StyleInt unitySliceLeft {
            get => _delegated.unitySliceLeft;
            set => _delegated.unitySliceLeft = value;
        }
        public StyleInt unitySliceRight {
            get => _delegated.unitySliceRight;
            set => _delegated.unitySliceRight = value;
        }
        public StyleFloat unitySliceScale {
            get => _delegated.unitySliceScale;
            set => _delegated.unitySliceScale = value;
        }
        public StyleInt unitySliceTop {
            get => _delegated.unitySliceTop;
            set => _delegated.unitySliceTop = value;
        }
        public StyleEnum<SliceType> unitySliceType {
            get => _delegated.unitySliceType;
            set => _delegated.unitySliceType = value;
        }
        public StyleEnum<TextAnchor> unityTextAlign {
            get => _delegated.unityTextAlign;
            set => _delegated.unityTextAlign = value;
        }
        public StyleTextAutoSize unityTextAutoSize {
            get => _delegated.unityTextAutoSize;
            set => _delegated.unityTextAutoSize = value;
        }
        public StyleEnum<TextGeneratorType> unityTextGenerator {
            get => _delegated.unityTextGenerator;
            set => _delegated.unityTextGenerator = value;
        }
        public StyleColor unityTextOutlineColor {
            get => _delegated.unityTextOutlineColor;
            set => _delegated.unityTextOutlineColor = value;
        }
        public StyleFloat unityTextOutlineWidth {
            get => _delegated.unityTextOutlineWidth;
            set => _delegated.unityTextOutlineWidth = value;
        }
        public StyleEnum<TextOverflowPosition> unityTextOverflowPosition {
            get => _delegated.unityTextOverflowPosition;
            set => _delegated.unityTextOverflowPosition = value;
        }
        public StyleEnum<Visibility> visibility {
            get => _delegated.visibility;
            set => _delegated.visibility = value;
        }
        public StyleEnum<WhiteSpace> whiteSpace {
            get => _delegated.whiteSpace;
            set => _delegated.whiteSpace = value;
        }
        public StyleLength width {
            get => _delegated.width;
            set => _delegated.width = value;
        }
        public StyleLength wordSpacing {
            get => _delegated.wordSpacing;
            set => _delegated.wordSpacing = value;
        }
        
        [Obsolete("Obsolete")]
        public StyleEnum<ScaleMode> unityBackgroundScaleMode {
            get => _delegated.unityBackgroundScaleMode;
            set => _delegated.unityBackgroundScaleMode = value;
        }
    }
}