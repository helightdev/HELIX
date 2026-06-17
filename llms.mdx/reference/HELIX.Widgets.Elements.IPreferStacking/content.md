# IPreferStacking (/reference/HELIX.Widgets.Elements.IPreferStacking)

# IPreferStacking

```
public interface IPreferStacking
```

Indicates that widgets should automatically use absolute positioning instead of the flex layout.

This is useful for alignment widgets (e.g. <a data-furef-uid="HELIX.Widgets.Universal.HAlign">HAlign</a>, <a data-furef-uid="HELIX.Widgets.Universal.HCenter">HCenter</a>) that are supposed to also
be usable inside stacking layouts such as an <a data-furef-uid="HELIX.Widgets.Universal.HStack">HStack</a> without specifying
<a data-furef-uid="HELIX.Widgets.ModifierExtensions.Positioned%60%601(%60%600%2cSystem.Nullable%7bHELIX.Types.StyleLength4%7d%2cUnityEngine.UIElements.Position)">ModifierExtensions.Positioned</a> or <a data-furef-uid="HELIX.Widgets.ModifierExtensions.Stretch%60%601(%60%600)">ModifierExtensions.Stretch</a>.