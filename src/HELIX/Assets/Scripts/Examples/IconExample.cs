using HELIX.Widgets;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Theme;

namespace Examples {
  public class IconExample : StatefulWidget<IconExample> {
    public override State<IconExample> CreateState() {
      return new IconExampleState();
    }
  }

  public class IconExampleState : State<IconExample> {
    public override Widget Build(BuildContext context) {
      var typograph = PrimitiveBaseTheme.Typography.Get(context);
      var colors = PrimitiveBaseTheme.Colors.Get(context);
      return new HIcon(
        FaSolidIcons.AddressBook,
        FaSolidIcons.FontDefinition,
        size: typograph.FontSize6,
        color: colors.primary.main
      );
    }
  }
}