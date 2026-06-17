# VisualElementExtensions (/reference/HELIX.Extensions.VisualElementExtensions)

# VisualElementExtensions

```
public static class VisualElementExtensions
```

## IsInitial<T>(IStyleValue<T>)

```
public static bool IsInitial<T>(this IStyleValue<T> value)
```

## GetDepth(VisualElement)

```
public static int GetDepth(this VisualElement element)
```

## AddTo<T>(T, VisualElement)

```
public static T AddTo<T>(this T element, VisualElement target) where T : VisualElement
```

## AddTo<T>(T, Hierarchy)

```
public static T AddTo<T>(this T element, VisualElement.Hierarchy target) where T : VisualElement
```

## WithAdded<T>(T, VisualElement)

```
public static T WithAdded<T>(this T target, VisualElement element) where T : VisualElement
```

## AddClasses<T>(T, params string[])

```
public static T AddClasses<T>(this T element, params string[] classNames) where T : VisualElement
```

## WithClasses<T>(T, params string[])

```
public static T WithClasses<T>(this T element, params string[] classNames) where T : VisualElement
```

## WithName<T>(T, string)

```
public static T WithName<T>(this T element, string name) where T : VisualElement
```

## Display<T>(T, bool)

```
public static T Display<T>(this T element, bool display) where T : VisualElement
```

## Visible<T>(T, bool)

```
public static T Visible<T>(this T element, bool visible) where T : VisualElement
```

## Opacity<T>(T, float)

```
public static T Opacity<T>(this T element, float opacity) where T : VisualElement
```

## Pickable<T>(T, bool)

```
public static T Pickable<T>(this T element, bool pickable) where T : VisualElement
```

## NoPaddingAndMargin<T>(T)

```
public static T NoPaddingAndMargin<T>(this T element) where T : VisualElement
```

## WithStyle<T>(T, Action<IStyle>)

```
public static T WithStyle<T>(this T element, Action<IStyle> updater) where T : VisualElement
```

## TextColor<T>(T, Color)

```
public static T TextColor<T>(this T element, Color color) where T : VisualElement
```

## TextSize<T>(T, float)

```
public static T TextSize<T>(this T element, float size) where T : VisualElement
```

## TextAlign<T>(T, TextAnchor)

```
public static T TextAlign<T>(this T element, TextAnchor alignment) where T : VisualElement
```

## BackgroundColor<T>(T, Color)

```
public static T BackgroundColor<T>(this T element, Color color) where T : VisualElement
```

## Image<T>(T, Background, BackgroundSizeType?, BackgroundSize?, Color?)

```
public static T Image<T>(this T element, Background background, BackgroundSizeType? fit = null, BackgroundSize? size = null, Color? tintColor = null) where T : VisualElement
```

## BackgroundImage<T>(T, Background)

```
public static T BackgroundImage<T>(this T element, Background background) where T : VisualElement
```

## BackgroundImageScaling<T>(T, BackgroundSize)

```
public static T BackgroundImageScaling<T>(this T element, BackgroundSize size) where T : VisualElement
```

## BackgroundImageScaling<T>(T, BackgroundSizeType)

```
public static T BackgroundImageScaling<T>(this T element, BackgroundSizeType size) where T : VisualElement
```

## BackgroundImageColor<T>(T, Color)

```
public static T BackgroundImageColor<T>(this T element, Color color) where T : VisualElement
```

## WithStylesheet<T>(T, StyleSheet)

```
public static T WithStylesheet<T>(this T element, StyleSheet stylesheet) where T : VisualElement
```

## WithStylesheet<T>(T, string)

```
public static T WithStylesheet<T>(this T element, string resourcePath) where T : VisualElement
```

## Transitions<T>(T, params Transition[])

```
public static T Transitions<T>(this T element, params Transition[] transitions) where T : VisualElement
```

## WithCallback<T>(T, IEventHandler)

```
public static T WithCallback<T>(this T element, IEventHandler action) where T : VisualElement
```

## WithCallback<T>(T, params IEventHandler[])

```
public static T WithCallback<T>(this T element, params IEventHandler[] actions) where T : VisualElement
```

## RegisterOnAttachRetroactively<T>(T, Action<T>)

```
public static T RegisterOnAttachRetroactively<T>(this T element, Action<T> action) where T : VisualElement
```

## BindDisposable<T>(T, IDisposable)

```
public static T BindDisposable<T>(this T element, IDisposable disposable) where T : VisualElement
```

## BindDisposable<T>(T, Func<IDisposable>)

```
public static T BindDisposable<T>(this T element, Func<IDisposable> disposable) where T : VisualElement
```

## Percent(int)

```
public static Length Percent(this int value)
```

## Percent(float)

```
public static Length Percent(this float value)
```

## NormalizedPercent(int)

```
public static Length NormalizedPercent(this int value)
```

## NormalizedPercent(float)

```
public static Length NormalizedPercent(this float value)
```

## Flexible<T>(T, float, float, Align)

```
public static T Flexible<T>(this T element, float grow = 1, float shrink = 1, Align selfAlign = Align.Auto) where T : VisualElement
```

## Fill<T>(T, Align)

```
public static T Fill<T>(this T element, Align selfAlign = Align.Stretch) where T : VisualElement
```

## Tight<T>(T)

```
public static T Tight<T>(this T element) where T : VisualElement
```

## TightStretch<T>(T, Align)

```
public static T TightStretch<T>(this T element, Align selfAlign = Align.Stretch) where T : VisualElement
```

## FlexContainer<T>(T, Axis, Justify, Align, Wrap, bool)

```
public static T FlexContainer<T>(this T element, Axis axis = Axis.Vertical, Justify mainAxisAlign = Justify.FlexStart, Align crossAxisAlign = Align.Center, Wrap wrap = Wrap.NoWrap, bool reverse = false) where T : VisualElement
```

## Tween(IVisualElementScheduler, long, Action<float>)

```
public static IVisualElementScheduledItem Tween(this IVisualElementScheduler scheduler, long durationMs, Action<float> onUpdate)
```

## Tween(IVisualElementScheduler, long, float, float, Action<float>)

```
public static IVisualElementScheduledItem Tween(this IVisualElementScheduler scheduler, long durationMs, float from, float to, Action<float> onUpdate)
```

## Tween(IVisualElementScheduler, long, Color, Color, Action<Color>)

```
public static IVisualElementScheduledItem Tween(this IVisualElementScheduler scheduler, long durationMs, Color from, Color to, Action<Color> onUpdate)
```

## NoPadding<T>(T)

```
public static T NoPadding<T>(this T element) where T : VisualElement
```

## Padding<T>(T, float)

```
public static T Padding<T>(this T element, float padding) where T : VisualElement
```

## Padding<T>(T, StyleLength)

```
public static T Padding<T>(this T element, StyleLength padding) where T : VisualElement
```

## Padding<T>(T, StyleLength2)

```
public static T Padding<T>(this T element, StyleLength2 padding) where T : VisualElement
```

## Padding<T>(T, StyleLength4)

```
public static T Padding<T>(this T element, StyleLength4 padding) where T : VisualElement
```

## Padding<T>(T)

```
public static StyleLength4 Padding<T>(this T element) where T : VisualElement
```

## Padded<T>(T, float, float, float, float)

```
public static T Padded<T>(this T element, float top = 0, float right = 0, float bottom = 0, float left = 0) where T : VisualElement
```

## Size<T>(T, StyleLength2)

```
public static T Size<T>(this T element, StyleLength2 size) where T : VisualElement
```

## Constraints<T>(T, StyleLength2, StyleLength2)

```
public static T Constraints<T>(this T element, StyleLength2 width, StyleLength2 height) where T : VisualElement
```

## WidthConstraints<T>(T, StyleLength2)

```
public static T WidthConstraints<T>(this T element, StyleLength2 width) where T : VisualElement
```

## HeightConstraints<T>(T, StyleLength2)

```
public static T HeightConstraints<T>(this T element, StyleLength2 height) where T : VisualElement
```

## Sized<T>(T, StyleLength?, StyleLength?)

```
public static T Sized<T>(this T element, StyleLength? width = null, StyleLength? height = null) where T : VisualElement
```

## Constrained<T>(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?, StyleLength?, StyleLength?)

```
public static T Constrained<T>(this T element, StyleLength? preferredWidth = null, StyleLength? preferredHeight = null, StyleLength? minWidth = null, StyleLength? minHeight = null, StyleLength? maxWidth = null, StyleLength? maxHeight = null) where T : VisualElement
```

## NoPosition<T>(T)

```
public static T NoPosition<T>(this T element) where T : VisualElement
```

## Position<T>(T, float)

```
public static T Position<T>(this T element, float position) where T : VisualElement
```

## Position<T>(T, StyleLength)

```
public static T Position<T>(this T element, StyleLength position) where T : VisualElement
```

## Position<T>(T, StyleLength2)

```
public static T Position<T>(this T element, StyleLength2 position) where T : VisualElement
```

## Position<T>(T, StyleLength4)

```
public static T Position<T>(this T element, StyleLength4 position) where T : VisualElement
```

## Position<T>(T)

```
public static StyleLength4 Position<T>(this T element) where T : VisualElement
```

## MakeRelative<T>(T)

```
public static T MakeRelative<T>(this T element) where T : VisualElement
```

## MakeAbsolute<T>(T)

```
public static T MakeAbsolute<T>(this T element) where T : VisualElement
```

## Stretched<T>(T)

```
public static T Stretched<T>(this T element) where T : VisualElement
```

## Loosen<T>(T)

```
public static T Loosen<T>(this T element) where T : VisualElement
```

## Positioned<T>(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?, Position)

```
public static T Positioned<T>(this T element, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null, StyleLength? left = null, Position type = Position.Absolute) where T : VisualElement
```

## Translated<T>(T, Length, Length)

```
public static T Translated<T>(this T element, Length x, Length y) where T : VisualElement
```

## Translated<T>(T, Vector2)

```
public static T Translated<T>(this T element, Vector2 translation) where T : VisualElement
```

## Translated<T>(T)

```
public static T Translated<T>(this T element) where T : VisualElement
```

## NoBorder<T>(T)

```
public static T NoBorder<T>(this T element) where T : VisualElement
```

## Border<T>(T, float)

```
public static T Border<T>(this T element, float value) where T : VisualElement
```

## Border<T>(T, Vector2)

```
public static T Border<T>(this T element, Vector2 value) where T : VisualElement
```

## Border<T>(T, Vector4)

```
public static T Border<T>(this T element, Vector4 value) where T : VisualElement
```

## Border<T>(T)

```
public static Vector4 Border<T>(this T element) where T : VisualElement
```

## NoBorderRadius<T>(T)

```
public static T NoBorderRadius<T>(this T element) where T : VisualElement
```

## BorderRadius<T>(T, float)

```
public static T BorderRadius<T>(this T element, float value) where T : VisualElement
```

## BorderRadius<T>(T, Vector4)

```
public static T BorderRadius<T>(this T element, Vector4 value) where T : VisualElement
```

## BorderColor<T>(T, Color)

```
public static T BorderColor<T>(this T element, Color color) where T : VisualElement
```

## NoMargin<T>(T)

```
public static T NoMargin<T>(this T element) where T : VisualElement
```

## Margin<T>(T, float)

```
public static T Margin<T>(this T element, float margin) where T : VisualElement
```

## Margin<T>(T, StyleLength)

```
public static T Margin<T>(this T element, StyleLength margin) where T : VisualElement
```

## Margin<T>(T, StyleLength2)

```
public static T Margin<T>(this T element, StyleLength2 margin) where T : VisualElement
```

## Margin<T>(T, StyleLength4)

```
public static T Margin<T>(this T element, StyleLength4 margin) where T : VisualElement
```

## Margin<T>(T)

```
public static StyleLength4 Margin<T>(this T element) where T : VisualElement
```

## Margined<T>(T, float, float, float, float)

```
public static T Margined<T>(this T element, float top = 0, float right = 0, float bottom = 0, float left = 0) where T : VisualElement
```