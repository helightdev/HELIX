# ModifierFallbacks (/reference/HELIX.Widgets.Modifiers.ModifierFallbacks)

# ModifierFallbacks

```
public static class ModifierFallbacks
```

## ImplicitFlexFill

```
public static readonly FlexibleModifier ImplicitFlexFill
```

This fallback modifier causes the widget to use flex filling to fill available space unless placed under
a parent using <a data-furef-uid="HELIX.Widgets.Elements.IPreferExplicitFlex">IPreferExplicitFlex</a>.

## StackingStretch

```
public static readonly PositionModifier StackingStretch
```

This fallback modifier causes the widget to use absolute positioning to fill available space when placed under
a parent using <a data-furef-uid="HELIX.Widgets.Elements.IPreferStacking">IPreferStacking</a>.

## FlexTight

```
public static readonly FlexibleModifier FlexTight
```

This fallback modifier causes the widget to use no flex layouting, forcing the implicit preferred size of its
content to be used.

## FlexFill

```
public static readonly FlexibleModifier FlexFill
```

## FlexExpand

```
public static readonly FlexibleModifier FlexExpand
```

## FlexTightStretch

```
public static readonly FlexibleModifier FlexTightStretch
```

## PosStretch

```
public static readonly PositionModifier PosStretch
```

## PaddingZero

```
public static readonly PaddingModifier PaddingZero
```

## MarginZero

```
public static readonly MarginModifier MarginZero
```