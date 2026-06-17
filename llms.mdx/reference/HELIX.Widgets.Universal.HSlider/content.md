# HSlider (/reference/HELIX.Widgets.Universal.HSlider)

# HSlider

```
public class HSlider : StatefulWidget<HSlider>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatefulWidget
```

A highly customizable slider widget that can be used for sliders and scrollbars.

## axis

```
public readonly Axis axis
```

## boxModifiers

```
public readonly WidgetStateProperty<ModifierSet> boxModifiers
```

## controller

```
public readonly SliderController controller
```

## enabled

```
public readonly bool enabled
```

## initialValue

```
public readonly float initialValue
```

## onChanged

```
public readonly Action<float> onChanged
```

## reverse

```
public readonly bool reverse
```

## scrollController

```
public readonly ScrollController scrollController
```

## style

```
public readonly HSliderStyle style
```

## thumbSize

```
public readonly float thumbSize
```

## focusKey

```
public readonly Key focusKey
```

## HSlider(SliderController, Key, Axis, bool, bool, float, float, Action<float>, WidgetStateProperty<ModifierSet>, HSliderStyle, Key, object[], IReadOnlyCollection<Modifier>)

```
public HSlider(SliderController controller = null, Key focusKey = default, Axis axis = Axis.Horizontal, bool enabled = true, bool reverse = false, float initialValue = 0, float thumbSize = -1, Action<float> onChanged = null, WidgetStateProperty<ModifierSet> boxModifiers = null, HSliderStyle style = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a highly customizable slider widget that can be used for sliders and scrollbars.

## HSlider(ScrollController, Key, Axis, bool, float, HSliderStyle, WidgetStateProperty<ModifierSet>, Key, object[], IReadOnlyCollection<Modifier>)

```
public HSlider(ScrollController scrollController, Key focusKey = default, Axis axis = Axis.Vertical, bool reverse = false, float thumbSize = -1, HSliderStyle style = null, WidgetStateProperty<ModifierSet> boxModifiers = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a highly customizable scrolling slider widget based on the given <code class="paramref">scrollController</code>.

## CreateState()

```
public override State<HSlider> CreateState()
```