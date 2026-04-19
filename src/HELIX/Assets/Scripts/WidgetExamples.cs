using System;
using System.Collections.Generic;
using System.Linq;
using Examples;
using HELIX;
using HELIX.Coloring;
using HELIX.Coloring.Material;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Navigation;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Substances;
using HELIX.Widgets.Universal.Theme;
using HELIX.Widgets.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

[UxmlElement]
public partial class WidgetExamples : WidgetHostElement {
    private readonly GlobalKey<NavStackElement> _navStackKey = new();

    public List<(string, Widget)> Pages { get; } = new() {
        ("Buttons", new ButtonsExample()),
        ("Text Input", new TextInputExample()),
        ("Scroll Controller", new ScrollControllerExample()),
        ("Substances & States", new StateSubstanceExample()),
        ("Theme Tokens", new ThemeTokensExample()),
        ("ListView", new ListVirtualizationExample()),
        ("Red Box", new HBox(background: MaterialColors.Red))
    };

    public WidgetExamples() {
        Buildable = new HStatefulBuilder(
            builder: (context, state) =>
                new HColumn(
                    modifiers: new Modifier[] { new BackgroundStyleModifier(context.GetThemed(PrimitiveTheme.Surface)) }
                ) {
                    new NavStack(key: _navStackKey).Fill(),
                    new HScrollView(
                        axis: Axis.Horizontal,
                        modifiers: new Modifier[] {
                            FlexibleModifier.TightStretch, MarginModifier.Only(left: 16, right: 16, bottom: 8, top: 8),
                        }
                    ) {
                        new HRow(
                            gap: 8f,
                            children: Pages.Select(e =>
                                new HButton(
                                    onClick: () => {
                                        _navStackKey.Element.PushReplacement(
                                            new WidgetNavPage { Buildable = e.Item2.ToBuildable() }
                                        );
                                    }
                                ) { new HText(e.Item1) }
                            ).ToArray()
                        )
                    }
                }
        ).Stretch().ToBuildable();
    }

    public override bool Reconcile(Widget updated) {
        base.Reconcile(updated);
        return true;
    }
}