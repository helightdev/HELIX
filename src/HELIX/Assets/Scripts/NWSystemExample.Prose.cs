using HELIX.Compose;
using HELIX.Prose;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Examples {
  public partial class HomeComposable {
    private enum DisplayMode { Windowed, Borderless, Fullscreen }

    private void ComposeProseTab(ref Composition cx) {
      using (cx.ScrollView())
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.AlignSelf(Align.Stretch);
        cx.Text("Semantic Prose writers", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text(
          "The same immediate-mode station report can be projected through several allocation-conscious " +
          "plain-text configurations, Markdown, Unity rich text, a Compose tree, or the data-only dictionary writer.",
          TextRole.BodySmall
        );
        cx.Spacing(2);

        ComposeProseWriterExample(ref cx);
        cx.Spacing(2);
        ComposeSpecWriterExample(ref cx);
        cx.Spacing(2);
        ComposeProseConsoleExamples(ref cx);

        cx.Spacing(2);
        cx.Text("Example contents", TextRole.LabelLarge);
        cx.Spacing(1);
        cx.Text(
          "Mission metadata, typed and constrained properties, hidden data, severity markers, power and " +
          "communications subsystems, nested reactor and antenna trees, cargo, crew, and active alerts.",
          TextRole.BodySmall
        );
      }
    }

    private static OptionPages CreateProseOptionPages() {
      var fieldReducer = new ComposeProseFieldReducer().Add(OptionPageFieldFactory.Create);
      var handlers = new ProseScopeDelegates<Composable>().Add(new ComposeProseFieldHandler(fieldReducer));
      var writer = new PathSectionedComposeProseWriter(delegates: handlers);
      using (writer.Path("graphics")) {
        writer.PushModifier(PathSectionModifiers.Title("Graphics", TextRole.TitleLarge));
        using (writer.Field<DisplayMode>(
          "graphics.display-mode",
          "Display mode",
          ProseFormatters.Enum<DisplayMode>()
        )) {
          writer.FieldDescription("Select how the game occupies the display.");
          writer.FieldTooltip("Borderless uses the desktop resolution and usually switches applications faster.");
          using (writer.FieldSuffix()) writer.Write("3 choices");
        }
        using (writer.Field(
          "graphics.upscaler",
          "Upscaler",
          new ProseChoiceFormatter<string>(
            new[] {
              new ProseChoice<string>("off", "Off"),
              new ProseChoice<string>("quality", "Quality"),
              new ProseChoice<string>("performance", "Performance")
            }
          )
        )) { }
        using (writer.Field("graphics.notice", "Display notice", ProseFormatters.String)) {
          writer.PushModifier(ProseFields.FullWidth);
          writer.FieldDescription("Display changes may briefly blank the screen.");
        }
        using (writer.Path("quality")) {
          writer.PushModifier(PathSectionModifiers.Title("Quality presets", TextRole.TitleSmall));
          writer.PushModifier(PathSectionModifiers.Description("Choose the rendering quality used by the game."));
          using (writer.Field(
            "graphics.texture-quality",
            "Texture quality",
            new ProseChoiceFormatter<string>(
              new[] {
                new ProseChoice<string>("low", "Low"),
                new ProseChoice<string>("medium", "Medium"),
                new ProseChoice<string>("high", "High")
              }
            )
          )) { }
          using (writer.Field(
            "graphics.shadow-quality",
            "Shadow quality",
            new ProseChoiceFormatter<string>(
              new[] {
                new ProseChoice<string>("off", "Off"),
                new ProseChoice<string>("medium", "Medium"),
                new ProseChoice<string>("high", "High")
              }
            )
          )) { }
        }
      }
      using (writer.Path("audio")) {
        writer.PushModifier(PathSectionModifiers.Title("Audio", TextRole.TitleLarge));
        using (writer.Field<int>(
          "audio.master-volume",
          "Master volume",
          new ProseIntFormatter(min: 0, max: 100, unit: "%", step: 1)
        )) {
          writer.PushModifier(ProseFields.LabelWidth(new Length(32f, LengthUnit.Percent)));
          writer.FieldDescription("Overall output volume.");
        }
        using (writer.Path("voice")) {
          writer.PushModifier(PathSectionModifiers.Title("Voice communication", TextRole.TitleSmall));
          using (writer.Field<bool>(
            "audio.voice-chat",
            "Voice chat",
            new ProseFlagFormatter(ifTrue: "Enabled", ifFalse: "Disabled")
          )) { }
        }
      }
      using (writer.Path("gameplay")) {
        writer.PushModifier(PathSectionModifiers.Title("Gameplay", TextRole.TitleLarge));
        using (writer.Field(
          "gameplay.difficulty",
          "Difficulty",
          new ProseChoiceFormatter<string>(
            new[] {
              new ProseChoice<string>("story", "Story"),
              new ProseChoice<string>("normal", "Normal"),
              new ProseChoice<string>("veteran", "Veteran")
            }
          )
        )) {
          writer.PushModifier(ProseFields.LabelWidth(new Length(144f, LengthUnit.Pixel)));
        }
        using (writer.Field<bool>("gameplay.autosave", "Autosave", ProseFormatters.Bool)) {
          writer.PushModifier(ProseFields.LabelWidth(new Length(144f, LengthUnit.Pixel)));
          using (writer.FieldSuffix()) writer.Write("Recommended");
        }
      }
      return new OptionPages(
        writer.BuildSections(),
        options: new OptionPagesOptions(collapseTooltipIntoDescription: true)
      );
    }

    private void ComposeOptionsTab(ref Composition cx) {
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Fill();
        _optionPages?.Compose(ref cx);
      }
    }

    private static void ComposeProseWriterExample(ref Composition cx) {
      cx.Text("Compose writer output", TextRole.LabelLarge);
      cx.Spacing(1);

      var writer = new ComposeProseWriter();
      writer.Write(DetailedProseExample.Prose);

      var prose = writer.Build();
      prose(ref cx);
    }

    private static void ComposeSpecWriterExample(ref Composition cx) {
      cx.Text("ISpec inside prose", TextRole.LabelLarge);
      cx.Spacing(1);

      var writer = new ComposeProseWriter();
      using (writer.Paragraph()) {
        writer.Write("This text run is followed by a LabelSpec composable: ");
        writer.Write(new LabelSpec("communications nominal"));
      }
      writer.Build()(ref cx);
    }

    private static void ComposeProseConsoleExamples(ref Composition cx) {
      cx.Text("Other writer projections", TextRole.LabelLarge);
      cx.Spacing(1);
      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        cx.Button(
          static (ref Composition child) => child.Text("Sparse tree"),
          action: static _ => DetailedProseExample.PrintPlainText("Sparse tree", ProseTextConfigurations.Sparse)
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Error tree"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static _ => DetailedProseExample.PrintPlainText("Error tree", ProseTextConfigurations.Error)
        );
      }
      cx.Spacing(1);
      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        cx.Button(
          static (ref Composition child) => child.Text("Plain"),
          action: static _ => DetailedProseExample.PrintPlainText("Plain", ProseTextConfigurations.Plain)
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Markdown"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static _ => DetailedProseExample.PrintPlainText("Markdown", ProseTextConfigurations.Markdown)
        );
      }
      cx.Spacing(1);
      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        cx.Button(
          static (ref Composition child) => child.Text("Unity rich text"),
          action: static _ => DetailedProseExample.PrintUnityRichText()
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Dictionary tree"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static _ => DetailedProseExample.PrintDictionary()
        );
      }
      cx.Spacing(1);
      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        cx.Button(
          static (ref Composition child) => child.Text("Whitespace tree"),
          action: static _ => DetailedProseExample.PrintPlainText(
            "Whitespace tree",
            ProseTextConfigurations.Whitespace
          )
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Shallow"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static _ => DetailedProseExample.PrintPlainText("Shallow", ProseTextConfigurations.Shallow)
        );
      }
      cx.Spacing(1);
      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        cx.Button(
          static (ref Composition child) => child.Text("Test: wide decorated"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static _ => DetailedProseExample.PrintPlainText(
            "test wide decorated",
            DetailedProseExample.TestWideDecorated
          )
        );
      }
      cx.Spacing(1);
      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        cx.Button(
          static (ref Composition child) => child.Text("Test: compact sections"),
          action: static _ => DetailedProseExample.PrintPlainText(
            "test compact sections",
            DetailedProseExample.TestCompactSections
          )
        );
      }
    }
  }
}