# IPreferExplicitFlex (/reference/HELIX.Widgets.Elements.IPreferExplicitFlex)

# IPreferExplicitFlex

```
public interface IPreferExplicitFlex
```

<p>
Indicates that widgets using implicit flex filling (e.g. <a data-furef-uid="HELIX.Widgets.Modifiers.ModifierFallbacks.FlexFill">ModifierFallbacks.FlexFill</a>)
should use tight constraints by default, preventing them and their children from expanding beyond their
intrinsic preferred size.
</p>
<p>
Also provides the preferred flex-axis for widgets such as <a data-furef-uid="HELIX.Widgets.Universal.HGap">HGap</a> that need to know what their
main-axis is going to be.
</p>

<p>
By default, many non-layouting widgets (e.g. <a data-furef-uid="HELIX.Widgets.StatelessWidget%601">StatelessWidget</a>,
<a data-furef-uid="HELIX.Widgets.StatefulWidget%601">StatefulWidget</a>, <a data-furef-uid="HELIX.Widgets.Universal.HThemeProvider">HThemeProvider</a>) adopt
<a data-furef-uid="HELIX.Widgets.Modifiers.ModifierFallbacks.FlexFill">ModifierFallbacks.FlexFill</a>, causing them to expand along both axes.
</p>
<p>
This is convenient in isolation, but can lead to unexpected behavior in layouts
like <a data-furef-uid="HELIX.Widgets.Universal.HRow">HRow</a>, where children are typically expected to tightly size to their
content unless wrapped in a <a data-furef-uid="HELIX.Widgets.Modifiers.FlexibleModifier">FlexibleModifier</a>. Applying a flex fill here would
create unintended whitespace not occupied by the child if it is not fully flexing.
</p>
<p>
Implementing <a data-furef-uid="HELIX.Widgets.Elements.IPreferExplicitFlex">IPreferExplicitFlex</a> changes this default by applying
tight constraints to direct children when possible, preventing the widget from expanding beyond
its intrinsic size unless explicitly instructed.
</p>

## PreferredFlexAxis

```
Axis PreferredFlexAxis { get; }
```