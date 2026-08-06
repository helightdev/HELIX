using HELIX.Extensions;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public static class ElementStyleExtensions {
    
    public static ref ElementRef With(this ref ElementRef scope, Flex flex) {
      flex.Apply(scope.element);
      scope.MarkFlag(UssFlag.Flex | UssFlag.GroupAlign);
      return ref scope;
    }

    public static ref ElementRef With(this ref ElementRef scope, FlexGroup group) {
      group.Apply(scope.element);
      scope.MarkFlag(UssFlag.GroupAlign);
      return ref scope;
    }

    public static ref ElementRef With(this ref ElementRef scope, BoxConstraints constraints) {
      constraints.Apply(scope.element);
      scope.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef With(this ref ElementRef scope, Border border) {
      border.Apply(scope.element);
      scope.MarkFlag(UssFlag.BorderWidth | UssFlag.BorderColor);
      return ref scope;
    }

    public static ref ElementRef With(this ref ElementRef scope, BorderRadius radius) {
      radius.Apply(scope.element);
      scope.MarkFlag(UssFlag.Radius);
      return ref scope;
    }

    public static ref readonly ScopeHandle With(this in ScopeHandle scope, Flex flex) {
      flex.Apply(scope.current.Element);
      scope.current.MarkFlag(UssFlag.Flex | UssFlag.GroupAlign);
      return ref scope;
    }

    public static ref readonly ScopeHandle With(this in ScopeHandle scope, BoxConstraints constraints) {
      constraints.Apply(scope.current.Element);
      scope.current.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef Padding(this ref ElementRef scope, StyleLength4 size) {
      scope.element.Padding(size);
      scope.MarkFlag(UssFlag.Padding);
      return ref scope;
    }

    public static ref ElementRef Focusable(
      this ref ElementRef scope,
      bool focusable = true,
      int tabIndex = 0,
      bool delegatesFocus = false,
      PickingMode pickingMode = PickingMode.Position
    ) {
      scope.element.focusable = focusable;
      scope.element.tabIndex = tabIndex;
      scope.element.delegatesFocus = delegatesFocus;
      scope.element.pickingMode = pickingMode;
      // scope.composable.Flag |= UssFlag.Focus;
      scope.MarkFlag(UssFlag.Focus);
      return ref scope;
    }

    public static ref ElementRef Margin(this ref ElementRef scope, StyleLength4 size) {
      scope.element.Margin(size);
      scope.MarkFlag(UssFlag.Margin);
      return ref scope;
    }

    public static ref ElementRef Size(this ref ElementRef scope, BoxConstraints constraints) {
      constraints.Apply(scope.element);
      scope.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef Position(this ref ElementRef scope, StyleLength4 position) {
      scope.element.Position(position);
      scope.MarkFlag(UssFlag.Position);
      return ref scope;
    }

    public static ref ElementRef Flexible(this ref ElementRef scope, float grow = 1f, float shrink = 1f) {
      scope.element.style.flexGrow = grow;
      scope.element.style.flexShrink = shrink;
      scope.MarkFlag(UssFlag.Flex);
      return ref scope;
    }

    public static ref ElementRef Group(
      this ref ElementRef scope,
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      bool reverse = false
    ) {
      scope.element.style.flexDirection = mainAxis.ToFlexDirection(reverse);
      scope.element.style.justifyContent = main;
      scope.element.style.alignItems = cross;
      scope.MarkFlag(UssFlag.GroupAlign);
      return ref scope;
    }

    public static ref ElementRef FlexGrow(this ref ElementRef scope, float grow) {
      scope.element.style.flexGrow = grow;
      scope.MarkFlag(UssFlag.Flex);
      return ref scope;
    }

    public static ref ElementRef FlexShrink(this ref ElementRef scope, float shrink) {
      scope.element.style.flexShrink = shrink;
      scope.MarkFlag(UssFlag.Flex);
      return ref scope;
    }

    public static ref ElementRef FlexBasis(this ref ElementRef scope, StyleLength basis) {
      scope.element.style.flexBasis = basis;
      scope.MarkFlag(UssFlag.Flex);
      return ref scope;
    }

    public static ref ElementRef AlignSelf(this ref ElementRef scope, Align alignment) {
      scope.element.style.alignSelf = alignment;
      scope.MarkFlag(UssFlag.GroupAlign);
      return ref scope;
    }

    public static ref ElementRef Width(this ref ElementRef scope, StyleLength width) {
      scope.element.style.width = width;
      scope.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef Height(this ref ElementRef scope, StyleLength height) {
      scope.element.style.height = height;
      scope.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef MinWidth(this ref ElementRef scope, StyleLength width) {
      scope.element.style.minWidth = width;
      scope.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef MinHeight(this ref ElementRef scope, StyleLength height) {
      scope.element.style.minHeight = height;
      scope.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef MaxWidth(this ref ElementRef scope, StyleLength width) {
      scope.element.style.maxWidth = width;
      scope.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef MaxHeight(this ref ElementRef scope, StyleLength height) {
      scope.element.style.maxHeight = height;
      scope.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef AspectRatio(this ref ElementRef scope, float ratio) {
      scope.element.style.aspectRatio = ratio;
      scope.MarkFlag(UssFlag.Size);
      return ref scope;
    }

    public static ref ElementRef Absolute(this ref ElementRef scope, bool absolute = true) {
      scope.element.style.position =
        absolute ? UnityEngine.UIElements.Position.Absolute : UnityEngine.UIElements.Position.Relative;
      scope.MarkFlag(UssFlag.Position);
      return ref scope;
    }

    public static ref ElementRef Border(this ref ElementRef scope, Border border) {
      border.Apply(scope.element);
      scope.MarkFlag(UssFlag.BorderWidth | UssFlag.BorderColor);
      return ref scope;
    }

    public static ref ElementRef BorderRadius(this ref ElementRef scope, BorderRadius radius) {
      radius.Apply(scope.element);
      scope.MarkFlag(UssFlag.Radius);
      return ref scope;
    }


    public static ref ElementRef BackgroundColor(this ref ElementRef scope, Color color) {
      scope.element.BackgroundColor(color);
      scope.MarkFlag(UssFlag.Background);
      return ref scope;
    }

    public static ref ElementRef BackgroundImage(this ref ElementRef scope, Background image) {
      scope.element.style.backgroundImage = image;
      scope.MarkFlag(UssFlag.Background);
      return ref scope;
    }

    public static ref ElementRef BackgroundSize(this ref ElementRef scope, BackgroundSize size) {
      scope.element.style.backgroundSize = size;
      scope.MarkFlag(UssFlag.Background);
      return ref scope;
    }

    public static ref ElementRef BackgroundTint(this ref ElementRef scope, Color color) {
      scope.element.style.unityBackgroundImageTintColor = color;
      scope.MarkFlag(UssFlag.BackgroundAdvanced);
      return ref scope;
    }

    public static ref ElementRef TextColor(this ref ElementRef scope, StyleColor color) {
      scope.element.TextColor(color);
      scope.MarkFlag(UssFlag.Text);
      return ref scope;
    }

    public static ref ElementRef TextSize(this ref ElementRef scope, StyleLength size) {
      scope.element.style.fontSize = size;
      scope.MarkFlag(UssFlag.Text);
      return ref scope;
    }

    public static ref ElementRef TextFont(this ref ElementRef scope, StyleFont font) {
      scope.element.style.unityFont = font;
      scope.MarkFlag(UssFlag.Text);
      return ref scope;
    }

    public static ref ElementRef TextFont(this ref ElementRef scope, StyleFontDefinition font) {
      scope.element.style.unityFontDefinition = font;
      scope.MarkFlag(UssFlag.Text);
      return ref scope;
    }

    public static ref ElementRef TextAlign(this ref ElementRef scope, TextAnchor alignment) {
      scope.element.style.unityTextAlign = alignment;
      scope.MarkFlag(UssFlag.Text);
      return ref scope;
    }

    public static ref ElementRef WhiteSpace(this ref ElementRef scope, WhiteSpace whiteSpace) {
      scope.element.style.whiteSpace = whiteSpace;
      scope.MarkFlag(UssFlag.Text);
      return ref scope;
    }

    public static ref ElementRef Display(this ref ElementRef scope, bool display) {
      scope.element.Display(display);
      scope.MarkFlag(UssFlag.Visibility);
      return ref scope;
    }

    public static ref ElementRef Opacity(this ref ElementRef scope, float opacity) {
      scope.element.Opacity(opacity);
      scope.MarkFlag(UssFlag.Visibility);
      return ref scope;
    }

    public static ref ElementRef Visible(this ref ElementRef scope, bool visible) {
      scope.element.style.visibility =
        visible ? Visibility.Visible : Visibility.Hidden;
      scope.MarkFlag(UssFlag.Visibility);
      return ref scope;
    }

    public static ref ElementRef Overflow(this ref ElementRef scope, Overflow overflow) {
      scope.element.style.overflow = overflow;
      scope.MarkFlag(UssFlag.Clipping);
      return ref scope;
    }

    public static ref ElementRef Translate(this ref ElementRef scope, Translate translate) {
      scope.element.style.translate = translate;
      scope.MarkFlag(UssFlag.Transform);
      return ref scope;
    }

    public static ref ElementRef Rotate(this ref ElementRef scope, Rotate rotate) {
      scope.element.style.rotate = rotate;
      scope.MarkFlag(UssFlag.Transform);
      return ref scope;
    }

    public static ref ElementRef Scale(this ref ElementRef scope, Scale scale) {
      scope.element.style.scale = scale;
      scope.MarkFlag(UssFlag.Transform);
      return ref scope;
    }

    public static ref ElementRef TransformOrigin(
      this ref ElementRef scope,
      TransformOrigin origin
    ) {
      scope.element.style.transformOrigin = origin;
      scope.MarkFlag(UssFlag.Transform);
      return ref scope;
    }

    public static ref ElementRef Cursor(
      this ref ElementRef scope,
      UnityEngine.UIElements.Cursor cursor
    ) {
      scope.element.style.cursor = cursor;
      scope.MarkFlag(UssFlag.Special);
      return ref scope;
    }

    public static ref ElementRef Transition(
      this ref ElementRef scope,
      TransitionPreset preset
    ) {
      if (preset == null) {
        TransitionApplicator.Clear(scope.element.style);
        return ref scope;
      }

      preset.Apply(scope.element.style);
      scope.composable.MarkFlag(UssFlag.Transition);
      return ref scope;
    }

    public static ref ElementRef Transition(
      this ref ElementRef scope,
      TransitionPreset preset,
      TransitionOptions options
    ) {
      if (preset == null) {
        TransitionApplicator.Clear(scope.element.style);
        return ref scope;
      }

      preset.Apply(scope.element.style, options);
      scope.composable.MarkFlag(UssFlag.Transition);
      return ref scope;
    }
    
    // public static ref ElementRef TextRole(this ref ElementRef scope, TextRole role) {
    //   var data = ThemeData.Key[scope];
    //   ref var style = ref data[role].style;
    //   style.Apply(scope.composable);
    //   return ref scope;
    // }

    public static ref ElementRef Class(this ref ElementRef scope, string className, bool enabled = true) {
      scope.element.EnableInClassList(className, enabled);
      scope.MarkFlag(UssFlag.Classes);
      return ref scope;
    }

    public static ref ElementRef Name(this ref ElementRef scope, string name) {
      scope.element.name = name;
      scope.MarkFlag(UssFlag.Name);
      return ref scope;
    }

    public static ref ElementRef Fill(this ref ElementRef scope) {
      return ref scope.Flexible().AlignSelf(Align.Stretch);
    }
    
  }
}