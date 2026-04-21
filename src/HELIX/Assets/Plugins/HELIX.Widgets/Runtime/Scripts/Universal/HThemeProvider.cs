using System.Collections.Generic;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Theming;

namespace HELIX.Widgets.Universal {
  /// <summary>
  /// A widget that provides inheritable theme properties to its descendants.
  /// </summary>
  public class HThemeProvider : SingleChildWidget {
    public readonly List<ThemeComponent> components;
    public readonly Dictionary<ThemeProperty, object> properties;

    /// <summary>
    /// Creates a widget that provides inheritable theme properties to its descendants.
    /// </summary>
    /// <param name="components">A list of theme components to apply to the descendants.</param>
    /// <param name="properties">A dictionary of theme properties to apply to the descendants.</param>
    /// <param name="child">The widget that is contained within the theme provider.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    public HThemeProvider(
      List<ThemeComponent> components = null,
      Dictionary<ThemeProperty, object> properties = null,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants) {
      this.components = components;
      this.properties = properties;

      DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
    }

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new ThemeProviderElement());
    }
  }
}