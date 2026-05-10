using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;

namespace HELIX.Widgets.Universal {

  /// <summary>
  /// A primitive widget that can be used to apply a background and border.
  /// </summary>
  public class HBox : SingleChildWidget {
    public readonly Alignment alignment;
    public readonly BackgroundStyle background;
    public readonly Border border;
    public readonly BorderRadius borderRadius;


    /// <summary>
    /// Creates a primitive widget that can be used to apply a background and border.
    /// </summary>
    /// <param name="alignment">
    /// If a child widget is provided and doesn't flex, this controls how it is aligned within the HBox.
    /// The <see cref="Alignment"/> will be quantized to the nearest <see cref="TextAnchor"/> before being applied.
    /// </param>
    /// <param name="background">The background applied to the box. If not provided, no background will be used.</param>
    /// <param name="border">Defines the border of the box. Defaults to <c>Border.None</c>.</param>
    /// <param name="borderRadius">Defines the border radius of the box. Defaults to <c>BorderRadius.None</c>.</param>
    /// <param name="child">The widget that is contained within the box.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    /// <inheritdoc/>
    public HBox(
      Alignment? alignment = null,
      BackgroundStyle background = null,
      Border? border = null,
      BorderRadius? borderRadius = null,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants) {
      this.alignment = alignment ?? Alignment.Center;
      this.background = background;
      this.border = border ?? Border.None;
      this.borderRadius = borderRadius ?? BorderRadius.None;

      DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
    }

    // ReSharper disable once InconsistentNaming

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new ContainerElement());
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new DiagnosticsProperty<Alignment>("alignment", alignment));
      properties.Add(new BackgroundStyleProperty("backgroundStyle", background));
      properties.Add(new DiagnosticsProperty<Border>("border", border, defaultValue: Border.None));
      properties.Add(
        new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius, defaultValue: BorderRadius.None)
      );
    }

    public class ContainerElement : SingleChildWidgetBaseElement<HBox> {
      public override void Apply(HBox previous, HBox widget) {
        if (previous == null || !Equals(previous.background, widget.background))
          (widget.background ?? BackgroundStyle.Default).Apply(this);

        widget.border.Apply(this);
        widget.borderRadius.Apply(this);
        widget.alignment.AlignAsColumn(this);
      }
    }
  }
}