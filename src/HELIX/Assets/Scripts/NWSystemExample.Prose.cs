using HELIX.Compose;
using HELIX.Prose;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Examples {
  public partial class HomeComposable {
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
            "Whitespace tree", ProseTextConfigurations.Whitespace
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
            "test wide decorated", DetailedProseExample.TestWideDecorated
          )
        );
      }
      cx.Spacing(1);
      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        cx.Button(
          static (ref Composition child) => child.Text("Test: compact sections"),
          action: static _ => DetailedProseExample.PrintPlainText(
            "test compact sections", DetailedProseExample.TestCompactSections
          )
        );
      }
    }
  }
}
