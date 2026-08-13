using System;
using System.Collections.Generic;
using HELIX.Prose;
using NUnit.Framework;

namespace HELIX.Tests {
  public sealed class ProseWriterTests {
    [Test]
    public void PlainTextWriter_RendersPropertiesTreesAndFiltering() {
      var writer = new ProseTextWriter(minimumLevel: ProseLevel.Info);

      Prose.Prose.WriteName(writer, "Person");
      Prose.Prose.WriteProperty(writer, "Age", 12, ProseIntFormatter.Instance);
      Prose.Prose.WriteProperty(writer, "Debug", "filtered", ProseStringFormatter.Instance, ProseLevel.Debug);
      Prose.Prose.WriteProperty(writer, "Tag", (string)null, ProseStringFormatter.Instance, hidden: true);

      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "Item 1");
      writer.PopFrame();

      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "Subtree");
      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "Subtree Item");
      writer.PopFrame();
      writer.PopFrame();
      Prose.Prose.WriteName(writer, "Tail");

      Assert.That(
        writer.Build(),
        Is.EqualTo("Person\nAge: 12\n│\n├─ Item 1\n└─ Subtree\n   └─ Subtree Item\nTail")
      );
    }

    [Test]
    public void PlainTextWriter_WrapsUnlessNoWrapIsActive() {
      var writer = new ProseTextWriter(wrapWidth: 12);

      Assert.That(writer.BeginFrame(ProseProperty.Instance), Is.True);
      Assert.That(writer.BeginFrame(ProsePropertyKey.Instance), Is.True);
      writer.Write("Key");
      writer.PopFrame();
      Assert.That(writer.BeginFrame(ProsePropertyValue.Instance), Is.True);
      writer.Write("one two three");
      writer.PopFrame();
      writer.PopFrame();

      Assert.That(writer.Build(), Is.EqualTo("Key: one two\n three\n\n"));

      writer.Reset();
      Assert.That(writer.BeginFrame(ProseProperty.Instance), Is.True);
      writer.PushModifier(NoWrap.Instance);
      writer.Write("one two three four");
      writer.PopFrame();
      Assert.That(writer.Build(), Is.EqualTo("one two three four\n\n"));
    }

    [Test]
    public void PlainTextWriter_AlignsBeforeTheValueContinuationPrefix() {
      var configuration = new ProseTextConfiguration(
        root: new PTNodeFormat(lines: NonTerminatingLineBreaks()),
        property: new PTNodeFormat(
          indent: new[] {
            new PTIndentRule(TextMatching.None, LineMatching.First, 0, "• "),
            new PTIndentRule(TextMatching.None, LineMatching.None, 0, "  ")
          },
          lines: EnabledLineBreaks()
        ),
        propertyValue: new PTNodeFormat(
          prefix: new[] { new PTStringRule(TextMatching.None, 0, ": ") },
          indent: new[] {
            new PTIndentRule(TextMatching.None, LineMatching.First, 0, ""),
            new PTIndentRule(TextMatching.None, LineMatching.None, 0, "↳ ")
          },
          lines: new[] {
            new PTLineRule(
              TextMatching.None,
              LineMatching.None,
              0,
              LineBreakMode.Wrap | LineBreakMode.Hard | LineBreakMode.Align
            )
          }
        )
      );
      var writer = new ProseTextWriter(wrapWidth: 20, configuration: configuration);

      Prose.Prose.WriteProperty(
        writer, "Summary", "alpha beta gamma", ProseStringFormatter.Instance
      );

      Assert.That(
        writer.Build(),
        Is.EqualTo("• Summary: alpha\n           ↳ beta\n           ↳ gamma")
      );
    }

    [Test]
    public void PlainTextWriter_RepeatsASuffixCharacterToTheFullLineWidth() {
      var configured = new ProseTextWriter(
        wrapWidth: 7,
        configuration: new ProseTextConfiguration(
          root: new PTNodeFormat(
            suffix: new[] { new PTStringRule(TextMatching.None, 0, "==]") },
            suffixRepeater: 1
          )
        )
      );
      configured.Write("-");
      Assert.That(configured.Build(), Is.EqualTo("-=====]"));

      var fallback = new ProseTextWriter(
        wrapWidth: 5,
        configuration: new ProseTextConfiguration(
          root: new PTNodeFormat(
            suffix: new[] { new PTStringRule(TextMatching.None, 0, "ab") },
            suffixRepeater: 99
          )
        )
      );
      fallback.Write("x");
      Assert.That(fallback.Build(), Is.EqualTo("xaaab"));

      var absent = new ProseTextWriter(
        wrapWidth: 5,
        configuration: new ProseTextConfiguration(
          root: new PTNodeFormat(
            suffix: new[] { new PTStringRule(TextMatching.Odd, 0, "==]") },
            suffixRepeater: 0
          )
        )
      );
      absent.Write("-");
      Assert.That(absent.Build(), Is.EqualTo("-"));
    }

    [Test]
    public void PlainTextWriter_HonorsTruncation() {
      var writer = new ProseTextWriter(maxTruncatableFrameLength: 5);

      Assert.That(writer.BeginFrame(ProseProperty.Instance), Is.True);
      writer.PushModifier(AllowTruncate.Instance);
      writer.Write("abcdefgh");
      writer.Write("ignored");
      writer.PopFrame();

      Assert.That(writer.Build(), Is.EqualTo("abcde…\n\n"));
    }

    [Test]
    public void PlainTextWriter_UsesConfiguredBoundariesAndRetainsAncestors() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Sparse);

      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "Parent");
      Prose.Prose.WriteProperty(writer, "State", "ready", ProseStringFormatter.Instance);
      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "First");
      writer.PopFrame();
      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "Last");
      writer.PopFrame();
      writer.PopFrame();

      Assert.That(
        writer.Build(),
        Is.EqualTo("└─ Parent\n   State: ready\n   │\n   ├─ First\n   └─ Last")
      );
    }

    [Test]
    public void PlainTextWriter_RetainsTreeBoundaryAcrossWrappedPropertyLines() {
      var writer = new ProseTextWriter(wrapWidth: 30);

      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "First");
      Prose.Prose.WriteProperty(
        writer,
        "Message",
        "alpha beta gamma delta epsilon",
        ProseStringFormatter.Instance
      );
      writer.PopFrame();
      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "Second");
      writer.PopFrame();

      Assert.That(writer.Build(), Does.Contain("│  Message: alpha beta gamma\n│   delta epsilon"));
    }

    [Test]
    public void PlainTextWriter_ShallowOmitsChildrenWhileWhitespaceRetainsTheirIndentation() {
      Assert.That(
        RenderConfiguration(ProseTextConfigurations.Whitespace),
        Is.EqualTo("Root\nValue: 1\n  Child\n  Child value: 2")
      );
      Assert.That(
        RenderConfiguration(ProseTextConfigurations.Shallow),
        Is.EqualTo("Root(Value: 1)")
      );
    }

    [Test]
    public void PlainTextWriter_ConfiguresLineBreaksAndContinuationPrefixes() {
      var configuration = new ProseTextConfiguration(
        root: new PTNodeFormat(
          indent: new[] {
            new PTIndentRule(TextMatching.None, LineMatching.First, 1, ""),
            new PTIndentRule(TextMatching.None, LineMatching.Hard, 0, "! ")
          },
          lines: NonTerminatingLineBreaks()
        ),
        property: new PTNodeFormat(
          indent: new[] {
            new PTIndentRule(TextMatching.None, LineMatching.First, 0, ""),
            new PTIndentRule(TextMatching.None, LineMatching.Hard, 0, "! "),
            new PTIndentRule(TextMatching.None, LineMatching.None, 0, "> ")
          },
          lines: EnabledLineBreaks()
        ),
        propertyValue: new PTNodeFormat(
          prefix: new[] { new PTStringRule(TextMatching.None, 0, ": ") },
          lines: new[] {
            new PTLineRule(
              TextMatching.None, LineMatching.None, 0, LineBreakMode.Wrap | LineBreakMode.Hard
            )
          }
        )
      );
      var writer = new ProseTextWriter(wrapWidth: 25, configuration: configuration);

      Prose.Prose.WriteProperty(
        writer,
        "Message",
        "alpha beta gamma delta",
        ProseStringFormatter.Instance
      );
      writer.Write("first\nsecond");

      Assert.That(writer.Build(), Is.EqualTo("Message: alpha beta gamma\n> delta\n! first\n! second"));
    }

    [Test]
    public void PlainTextWriter_InjectsConditionalAndMandatoryPropertyContent() {
      var configuration = new ProseTextConfiguration(
        root: new PTNodeFormat(
          prefix: new[] {
            new PTStringRule(TextMatching.Empty, 0, ""),
            new PTStringRule(TextMatching.None, 0, "[")
          },
          suffix: new[] {
            new PTStringRule(TextMatching.Empty, 0, "!"),
            new PTStringRule(TextMatching.None, 0, "]!")
          }
        ),
        property: new PTNodeFormat(
          prefix: new[] {
            new PTStringRule(TextMatching.First, 0, ""),
            new PTStringRule(TextMatching.None, 0, ", ")
          }
        ),
        propertyValue: new PTNodeFormat(
          prefix: new[] { new PTStringRule(TextMatching.None, 0, ": ") }
        )
      );
      var populated = new ProseTextWriter(configuration: configuration);
      Prose.Prose.WriteProperty(populated, "A", 1, ProseIntFormatter.Instance);
      Prose.Prose.WriteProperty(populated, "B", 2, ProseIntFormatter.Instance);
      Assert.That(populated.Build(), Is.EqualTo("[A: 1, B: 2]!"));

      var empty = new ProseTextWriter(configuration: configuration);
      Assert.That(empty.Build(), Is.EqualTo("!"));
    }

    [Test]
    public void PlainTextWriter_InjectsChildContentOnlyWhenChildrenExist() {
      var configuration = new ProseTextConfiguration(
        root: new PTNodeFormat(
          prefix: new[] {
            new PTStringRule(TextMatching.Empty, 0, ""),
            new PTStringRule(TextMatching.None, 0, "<\n")
          },
          suffix: new[] {
            new PTStringRule(TextMatching.Empty, 0, "!"),
            new PTStringRule(TextMatching.None, 0, "!>!")
          },
          lines: NonTerminatingLineBreaks()
        ),
        treeName: new PTNodeFormat(
          lines: EnabledLineBreaks()
        ),
        tree: new PTNodeFormat(
          lines: EnabledLineBreaks()
        )
      );
      var populated = new ProseTextWriter(configuration: configuration);
      Assert.That(populated.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(populated, "Child");
      populated.PopFrame();
      Assert.That(populated.Build(), Is.EqualTo("<\nChild\n!>!"));

      var empty = new ProseTextWriter(configuration: configuration);
      Assert.That(empty.Build(), Is.EqualTo("!"));
    }

    [Test]
    public void PlainTextWriter_EvaluatesCollectionStateLazilyAndPrioritiesAdditively() {
      var configuration = new ProseTextConfiguration(
        property: new PTNodeFormat(
          prefix: new[] {
            new PTStringRule(TextMatching.First, 0, "["),
            new PTStringRule(TextMatching.Odd, 1, ";"),
            new PTStringRule(TextMatching.None, 0, ",")
          },
          suffix: new[] { new PTStringRule(TextMatching.Last, 0, "]") },
          replacement: new[] {
            new PTStringRule(TextMatching.Odd, 0, "X"),
            new PTStringRule(TextMatching.Last, 1, "Y")
          }
        ),
        propertyValue: new PTNodeFormat(
          prefix: new[] { new PTStringRule(TextMatching.None, 0, ": ") }
        )
      );
      var writer = new ProseTextWriter(configuration: configuration);

      Prose.Prose.WriteProperty(writer, "A", 1, ProseIntFormatter.Instance);
      Prose.Prose.WriteProperty(writer, "B", 2, ProseIntFormatter.Instance);

      Assert.That(writer.Build(), Is.EqualTo("[A: 1;XY]"));
    }

    [Test]
    public void PlainTextWriter_ReplacesEmptyItems() {
      var configuration = new ProseTextConfiguration(
        property: new PTNodeFormat(
          replacement: new[] { new PTStringRule(TextMatching.Empty, 0, "<empty>") }
        )
      );
      var writer = new ProseTextWriter(configuration: configuration);

      Assert.That(writer.BeginFrame(ProseProperty.Instance), Is.True);
      writer.PopFrame();

      Assert.That(writer.Build(), Is.EqualTo("<empty>"));
    }

    [Test]
    public void PlainTextWriter_ConfiguresLineBreaksPerItem() {
      var configured = new ProseTextWriter(
        configuration: new ProseTextConfiguration(
          root: new PTNodeFormat(lines: NonTerminatingLineBreaks())
        )
      );
      configured.Write("first\nsecond\nthird");
      Assert.That(configured.Build(), Is.EqualTo("first\nsecond\nthird"));

      var disabled = new ProseTextWriter(configuration: new ProseTextConfiguration());
      disabled.Write("first\nsecond");
      Assert.That(disabled.Build(), Is.EqualTo("firstsecond"));

    }

    [Test]
    public void PlainTextWriter_OnlyPrefixesLinesWhoseBoundaryIsEnabled() {
      var linePrefix = new[] {
        new PTIndentRule(TextMatching.None, LineMatching.First, 1, ""),
        new PTIndentRule(TextMatching.None, LineMatching.None, 0, "> ")
      };
      var enabled = new ProseTextWriter(
        configuration: new ProseTextConfiguration(
          root: new PTNodeFormat(
            indent: linePrefix,
            lines: new[] {
              new PTLineRule(
                TextMatching.None, LineMatching.None, 0, LineBreakMode.Hard
              )
            }
          )
        )
      );
      enabled.Write("first\nsecond");
      Assert.That(enabled.Build(), Is.EqualTo("first\n> second"));

      var disabled = new ProseTextWriter(
        configuration: new ProseTextConfiguration(
          root: new PTNodeFormat(indent: linePrefix)
        )
      );
      disabled.Write("first\nsecond");
      Assert.That(disabled.Build(), Is.EqualTo("firstsecond"));

      var nested = new ProseTextWriter(
        configuration: new ProseTextConfiguration(
          property: new PTNodeFormat(
            indent: linePrefix,
            lines: new[] {
              new PTLineRule(
                TextMatching.None, LineMatching.None, 0, LineBreakMode.Hard
              )
            }
          )
        )
      );
      Assert.That(nested.BeginFrame(ProseProperty.Instance), Is.True);
      nested.Write("first\nsecond");
      nested.PopFrame();
      Assert.That(nested.Build(), Is.EqualTo("firstsecond"));
    }

    [Test]
    public void AnchorFactory_BuildsReusableCommonAnchors() {
      var item = PTRuleFactory.Line(
        prefix: "[", suffix: "]", firstLinePrefix: ">", continuationPrefix: "|",
        suffixRepeater: 0
      );

      Assert.That(item.Prefix[0].String, Is.EqualTo("["));
      Assert.That(item.Suffix[0].String, Is.EqualTo("]"));
      Assert.That(item.LinePrefix[0].LineMatching, Is.EqualTo(LineMatching.First));
      Assert.That(item.LinePrefix[1].String, Is.EqualTo("|"));
      Assert.That(item.SuffixRepeater, Is.Zero);
      Assert.That(
        item.LineBreak[0].Value,
        Is.EqualTo(LineBreakMode.Item | LineBreakMode.Wrap | LineBreakMode.Hard)
      );
    }

    [Test]
    public void AnchorFactory_RejectsInvalidSuffixRepeater() {
      Assert.That(() => new PTNodeFormat(suffixRepeater: -2), Throws.TypeOf<System.ArgumentOutOfRangeException>());
    }

    [Test]
    public void AnchorFactory_SuppressesOnlyTheTerminalBoundary() {
      var writer = new ProseTextWriter(
        configuration: new ProseTextConfiguration(
          root: PTRuleFactory.Container(firstLinePrefix: "[", continuationPrefix: ">")
        )
      );
      writer.Write("a\nb");

      Assert.That(writer.Build(), Is.EqualTo("[a\n>b"));
    }

    [Test]
    public void PlainTextWriter_ControlsHardWrapAndItemBreaksIndependently() {
      var hardOnly = new ProseTextWriter(
        wrapWidth: 4,
        configuration: new ProseTextConfiguration(
          root: PTRuleFactory.Block(lineBreaks: LineBreakMode.Hard)
        )
      );
      hardOnly.Write("aa bb\ncc");
      Assert.That(hardOnly.Build(), Is.EqualTo("aabb\ncc"));

      var wrapOnly = new ProseTextWriter(
        wrapWidth: 4,
        configuration: new ProseTextConfiguration(
          root: PTRuleFactory.Block(lineBreaks: LineBreakMode.Wrap)
        )
      );
      wrapOnly.Write("aa bb\ncc");
      Assert.That(wrapOnly.Build(), Is.EqualTo("aa\nbbcc"));
    }

    [Test]
    public void PlainTextWriter_AppliesFirstLastOddAndEmptyStates() {
      var configuration = new ProseTextConfiguration(
        root: PTRuleFactory.Container(),
        property: new PTNodeFormat(
          prefix: new[] {
            PTRuleFactory.Anchor("F", TextMatching.First),
            PTRuleFactory.Anchor("O", TextMatching.Odd, 1),
            PTRuleFactory.Anchor("N")
          },
          suffix: new[] { PTRuleFactory.Anchor("L", TextMatching.Last) },
          replacement: new[] { PTRuleFactory.Anchor("E", TextMatching.Empty) },
          lines: PTRuleFactory.LineBreaks(LineBreakMode.Item)
        )
      );
      var writer = new ProseTextWriter(configuration: configuration);
      Assert.That(writer.BeginFrame(ProseProperty.Instance), Is.True);
      writer.Write("a");
      writer.PopFrame();
      Assert.That(writer.BeginFrame(ProseProperty.Instance), Is.True);
      writer.PopFrame();

      Assert.That(writer.Build(), Is.EqualTo("Fa\nOEL"));
    }

    [Test]
    public void PlainTextWriter_HonorsEachVisibilitySwitch() {
      var hiddenNames = RenderVisibility(showTrees: true, showNames: false, showProperties: true);
      Assert.That(hiddenNames, Does.Not.StartWith("Root\n").And.Not.Contain("`- Child\n"));
      Assert.That(hiddenNames, Does.Contain("Value: 1").And.Contain("Child value: 2"));

      var hiddenProperties = RenderVisibility(showTrees: true, showNames: true, showProperties: false);
      Assert.That(hiddenProperties, Does.Contain("Root").And.Contain("Child"));
      Assert.That(hiddenProperties, Does.Not.Contain("Value: 1").And.Not.Contain("Child value: 2"));

      var hiddenTrees = RenderVisibility(showTrees: false, showNames: true, showProperties: true);
      Assert.That(hiddenTrees, Does.Contain("Root").And.Contain("Value: 1"));
      Assert.That(hiddenTrees, Does.Not.Contain("Child").And.Not.Contain("Child value: 2"));
    }

    [Test]
    public void PlainTextConfigurations_ExposeTheSupportedLayouts() {
      var configurations = new[] {
        ProseTextConfigurations.Sparse,
        ProseTextConfigurations.Error,
        ProseTextConfigurations.Whitespace,
        ProseTextConfigurations.Shallow,
        ProseTextConfigurations.Plain,
        ProseTextConfigurations.Markdown
      };

      Assert.That(configurations, Has.All.Not.Null);
      foreach (var configuration in configurations)
        Assert.That(RenderConfiguration(configuration), Is.Not.Empty);
    }

    [Test]
    public void PlainTextConfigurations_RenderSparseAndWhitespaceTrees() {
      Assert.That(RenderConfiguration(ProseTextConfigurations.Sparse), Does.Contain("└─ Child"));
      Assert.That(RenderConfiguration(ProseTextConfigurations.Sparse), Does.Contain("Value: 1"));
      Assert.That(RenderConfiguration(ProseTextConfigurations.Whitespace), Does.Not.Contain("└"));
      Assert.That(RenderConfiguration(ProseTextConfigurations.Whitespace), Does.Contain("  Child"));
    }

    [Test]
    public void PlainTextConfigurations_RenderShallowLayoutOnOneLine() {
      Assert.That(
        RenderConfiguration(ProseTextConfigurations.Shallow),
        Is.EqualTo("Root(Value: 1)")
      );
    }

    [Test]
    public void PlainTextConfigurations_ShallowTerminatesTheFinalPropertyList() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Shallow);
      Prose.Prose.WriteName(writer, "Root");
      Prose.Prose.WriteProperty(writer, "First", 1, ProseIntFormatter.Instance);
      Prose.Prose.WriteProperty(writer, "Second", 2, ProseIntFormatter.Instance);

      Assert.That(writer.Build(), Is.EqualTo("Root(First: 1, Second: 2)"));
    }

    [Test]
    public void PlainTextConfigurations_ShallowTerminatesWrappedProductionOutput() {
      var writer = new ProseTextWriter(
        wrapWidth: 96,
        configuration: ProseTextConfigurations.Shallow
      );
      Prose.Prose.WriteName(writer, "Asteria Orbital Relay Station");
      Prose.Prose.WriteProperty(writer, "Mission ID", "HX-ASTERIA-07", ProseStringFormatter.Instance);
      Prose.Prose.WriteProperty(writer, "State", "Degraded", ProseStringFormatter.Instance);
      Prose.Prose.WriteProperty(writer, "Crew aboard", "37 people", ProseStringFormatter.Instance);
      Prose.Prose.WriteProperty(writer, "Orbit", "1842 completed", ProseStringFormatter.Instance);
      Prose.Prose.WriteProperty(writer, "Autonomous control", "operational", ProseStringFormatter.Instance);
      Prose.Prose.WriteProperty(
        writer,
        "Summary",
        "Long-range relay and research platform holding a stable polar orbit while its secondary coolant " +
        "loop is isolated for inspection.",
        ProseStringFormatter.Instance
      );
      Prose.Prose.WriteProperty(
        writer,
        "Internal tracking token",
        "OPS-4A-9912",
        ProseStringFormatter.Instance,
        hidden: true
      );

      Assert.That(writer.Build(), Does.EndWith("inspection.)"));
    }

    [Test]
    public void PlainTextConfigurations_RenderACleanErrorSignature() {
      var error = RenderConfiguration(ProseTextConfigurations.Error);
      Assert.That(error, Does.StartWith("══ Root ══"));
      Assert.That(error, Does.Contain("└─ Child"));
    }

    [Test]
    public void PlainTextConfigurations_KeepStructuralPresetsLimitedToTreesAndProperties() {
      foreach (var configuration in new[] {
                 ProseTextConfigurations.Sparse,
                 ProseTextConfigurations.Shallow
               }) {
        var writer = new ProseTextWriter(configuration: configuration);
        Prose.Prose.WriteName(writer, "Root");
        Prose.Prose.WriteProperty(writer, "Value", 1, ProseIntFormatter.Instance);
        Prose.Prose.WriteSpan(writer, "important", ProseTextStyle.Strong);
        Prose.Prose.WriteSpan(writer, "docs", linkTarget: "https://example.test");
        Prose.Prose.WriteCodeBlock(writer, "run command", "shell");
        Assert.That(writer.BeginFrame(ProseSection.Instance), Is.False);
        Assert.That(writer.BeginFrame(ProseList.Unordered), Is.False);
        Assert.That(writer.BeginFrame(ProseTable.Instance), Is.False);

        var result = writer.Build();
        Assert.That(result, Does.Contain("Root").And.Contain("Value: 1"));
        Assert.That(
          result,
          Does.Not.Contain("important").And.Not.Contain("docs").And.Not.Contain("run command")
        );
      }
    }

    [Test]
    public void PlainConfiguration_UsesAsciiTreeAndIncludesAllTextFeatures() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Plain);
      Prose.Prose.WriteName(writer, "Root");
      Prose.Prose.WriteProperty(writer, "Value", 1, ProseIntFormatter.Instance);
      WriteStructuredProse(writer);
      Prose.Prose.WriteCodeBlock(writer, "run command", "shell");

      var result = writer.Build();
      Assert.That(result, Does.StartWith("Root\nValue: 1\n"));
      Assert.That(result, Does.Contain("Status:").And.Contain("3. First"));
      Assert.That(result, Does.Contain("| Name").And.Contain("Code: shell\nrun command"));
      Assert.That(result, Does.Contain("Status:\nEverything"));
      Assert.That(result, Does.Contain("docs (https://example.test).\n\n3. First"));
      Assert.That(result, Does.Contain("4. Second\n\n| Name"));
      Assert.That(result, Does.Contain("| Alpha |     3 |\n\nCode: shell"));
      Assert.That(
        RenderConfiguration(ProseTextConfigurations.Plain),
        Is.EqualTo("Root\nValue: 1\n|\n\\- Child\n   Child value: 2\n\n")
      );
    }

    [Test]
    public void ErrorConfiguration_ProjectsMarkupIntoItsDiagnosticStyle() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Error);
      Prose.Prose.WriteSpan(writer, "strong", ProseTextStyle.Strong);
      writer.Write(" ");
      Prose.Prose.WriteSpan(writer, "code", ProseTextStyle.Code);
      writer.Write(" ");
      Prose.Prose.WriteSpan(writer, "failure", ProseTextStyle.Error);

      Assert.That(writer.Build(), Is.EqualTo("«strong» ⟦code⟧ ‼ failure"));
    }

    [Test]
    public void ErrorConfiguration_UsesExclamationPrefixOnlyForProperties() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Error);

      Assert.That(writer.BeginFrame(ProseParagraph.Instance), Is.True);
      writer.Write("Station narrative.");
      writer.PopFrame();
      Prose.Prose.WriteProperty(writer, "State", "Degraded", ProseStringFormatter.Instance);

      Assert.That(writer.BeginFrame(ProseParagraph.Instance), Is.True);
      Prose.Prose.WriteSpan(writer, "Operator note.", ProseTextStyle.Quote);
      writer.PopFrame();
      Prose.Prose.WriteProperty(writer, "Owner", "Operations", ProseStringFormatter.Instance);

      Assert.That(
        writer.Build(),
        Is.EqualTo(
          "\nStation narrative.\n\n" +
          "! State: Degraded\n\n" +
          "│ Operator note.\n\n" +
          "! Owner: Operations"
        )
      );
    }

    [Test]
    public void DictionaryWriter_CapturesTheDataTreeAndIgnoresFormatters() {
      var writer = new ProseDictionaryWriter();
      Prose.Prose.WriteName(writer, "Person");
      Prose.Prose.WriteProperty(writer, "Age", 12, ThrowingIntFormatter.Instance, ProseLevel.Warning);

      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "Item");
      Prose.Prose.WriteProperty(writer, "Enabled", true, ProseBoolFormatter.Instance);
      writer.PopFrame();

      Assert.That(writer.Root[ProseDictionaryWriter.NameKey], Is.EqualTo("Person"));
      Assert.That(writer.Root["Age"], Is.TypeOf<int>().And.EqualTo(12));
      var children = (List<object>)writer.Root[ProseDictionaryWriter.ChildrenKey];
      var item = (Dictionary<string, object>)children[0];
      Assert.That(item[ProseDictionaryWriter.NameKey], Is.EqualTo("Item"));
      Assert.That(item["Enabled"], Is.TypeOf<bool>().And.EqualTo(true));
      Assert.That(writer.Root.ContainsKey("type"), Is.False);
    }

    [Test]
    public void DictionaryWriter_PreservesSectionsListsAndTables() {
      var writer = new ProseDictionaryWriter();
      WriteStructuredProse(writer);

      var sections = (List<object>)writer.Root[ProseDictionaryWriter.SectionsKey];
      var section = (Dictionary<string, object>)sections[0];
      Assert.That(section[ProseDictionaryWriter.HeaderKey], Is.EqualTo("Status"));
      Assert.That(section[ProseDictionaryWriter.ContentKey], Is.EqualTo("Everything works. See docs."));

      var lists = (List<object>)section[ProseDictionaryWriter.ListsKey];
      var list = (Dictionary<string, object>)lists[0];
      Assert.That(list[ProseDictionaryWriter.ListKindKey], Is.EqualTo("ordered"));
      Assert.That(list[ProseDictionaryWriter.StartKey], Is.EqualTo(3));
      var items = (List<object>)list[ProseDictionaryWriter.ItemsKey];
      Assert.That(
        ((Dictionary<string, object>)items[1])[ProseDictionaryWriter.ContentKey],
        Is.EqualTo("Second")
      );

      var tables = (List<object>)section[ProseDictionaryWriter.TablesKey];
      var table = (Dictionary<string, object>)tables[0];
      var rows = (List<object>)table[ProseDictionaryWriter.RowsKey];
      var header = (Dictionary<string, object>)rows[0];
      Assert.That(header[ProseDictionaryWriter.IsHeaderKey], Is.True);
      Assert.That((List<object>)header[ProseDictionaryWriter.CellsKey], Is.EqualTo(new[] { "Name", "Count" }));
      var body = (Dictionary<string, object>)rows[1];
      Assert.That((List<object>)body[ProseDictionaryWriter.CellsKey], Is.EqualTo(new object[] { "Alpha", 3 }));
    }

    [Test]
    public void PlainTextWriter_RendersMarkupListsAndMeasuredTables() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Markdown);
      WriteStructuredProse(writer);

      Assert.That(
        writer.Build(),
        Is.EqualTo(
          "## Status\n\n" +
          "Everything **works**. See [docs](https://example.test).\n\n" +
          "3. First\n" +
          "4. Second\n\n" +
          "| Name  | Count |\n" +
          "| ----- | ----- |\n" +
          "| Alpha |     3 |"
        )
      );
    }

    [Test]
    public void PlainTextWriter_LimitsTableColumnTargetsToTheWrapWidth() {
      var writer = new ProseTextWriter(
        wrapWidth: 16, configuration: ProseTextConfigurations.Markdown
      );
      Assert.That(writer.BeginFrame(ProseTable.Instance), Is.True);
      Assert.That(writer.BeginFrame(ProseTableRow.Header), Is.True);
      Prose.Prose.WriteTableCell(writer, "Name");
      Prose.Prose.WriteTableCell(writer, "Kind");
      writer.PopFrame();
      Assert.That(writer.BeginFrame(ProseTableRow.Body), Is.True);
      Prose.Prose.WriteTableCell(writer, "extraordinary");
      Prose.Prose.WriteTableCell(writer, "x");
      writer.PopFrame();
      writer.PopFrame();

      var lines = writer.Build().Split('\n');
      Assert.That(lines[0], Is.EqualTo("| Name  | Kind |"));
      Assert.That(lines[1], Is.EqualTo("| ----- | ---- |"));
      Assert.That(lines[0].Length, Is.EqualTo(writer.WrapWidth));
      Assert.That(lines[2], Is.EqualTo("| extraordinary | x    |"));
      Assert.That(lines[2].Length, Is.GreaterThan(writer.WrapWidth));
    }

    [Test]
    public void PlainTextWriter_ReplacesLineBreaksInsideTableCells() {
      var defaultWriter = new ProseTextWriter(
        configuration: ProseTextConfigurations.Markdown
      );
      WriteSingleCellTable(defaultWriter, "alpha\nbeta");
      Assert.That(defaultWriter.Build(), Is.EqualTo("| alpha¶beta |"));

      var configuredWriter = new ProseTextWriter(
        configuration: new ProseTextConfiguration(
          root: PTRuleFactory.Container(),
          table: new PTTableFormat(lineBreakReplacement: " / ")
        )
      );
      WriteSingleCellTable(configuredWriter, "alpha\nbeta");
      Assert.That(configuredWriter.Build(), Is.EqualTo("| alpha / beta |"));
    }

    [Test]
    public void PlainTextWriter_CombinesGeneralMarkupModifiers() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Markdown);
      Prose.Prose.WriteSpan(
        writer, "important", ProseTextStyle.Emphasis | ProseTextStyle.Strong
      );
      writer.Write(" ");
      Prose.Prose.WriteSpan(writer, "value", ProseTextStyle.Code);
      writer.Write(" ");
      Prose.Prose.WriteSpan(writer, "quoted", ProseTextStyle.Quote);
      writer.Write(" ");
      Prose.Prose.WriteSpan(writer, "failed", ProseTextStyle.Error);

      Assert.That(
        writer.Build(),
        Is.EqualTo("***important*** `value` > quoted **Error: failed**")
      );
    }

    [Test]
    public void TextWriters_SeparateQuotedParagraphsFromFollowingProperties() {
      const string note =
        "Operator note: the relay remains mission-capable; prioritize thermal stability over throughput.";

      var plain = new ProseTextWriter(configuration: ProseTextConfigurations.Plain);
      WriteQuoteAndProperty(plain, note);
      Assert.That(plain.Build(), Does.Contain("”\n\nState: Degraded"));

      var markdown = new ProseTextWriter(configuration: ProseTextConfigurations.Markdown);
      WriteQuoteAndProperty(markdown, note);
      Assert.That(markdown.Build(), Does.Contain(note + "\n\n- State: Degraded"));

      var unity = new ProseUnityRichTextWriter();
      WriteQuoteAndProperty(unity, note);
      Assert.That(unity.Build(), Does.Contain("</color></i>\n\nState: Degraded"));
    }

    [Test]
    public void PlainTextWriter_RendersPreformattedCodeBlocksWithoutWrapping() {
      var writer = new ProseTextWriter(
        wrapWidth: 8,
        configuration: ProseTextConfigurations.Markdown
      );

      Prose.Prose.WriteCodeBlock(writer, "var value = 123;\nreturn value;", "csharp");

      Assert.That(
        writer.Build(),
        Is.EqualTo("```csharp\nvar value = 123;\nreturn value;\n```")
      );
    }

    [Test]
    public void PlainTextWriter_FillsCodeBlockBoundariesAndCanHideLanguage() {
      var configuration = new ProseTextConfiguration(
        root: PTRuleFactory.Container(),
        codeBlock: new PTCodeBlockFormat(
          prefix: "[", prefixSuffix: "]", suffix: "-",
          prefixFill: "<->", prefixFillRepeater: 1,
          suffixFill: "[=]", suffixFillRepeater: 1,
          showLanguage: false
        )
      );
      var writer = new ProseTextWriter(wrapWidth: 20, configuration: configuration);

      Prose.Prose.WriteCodeBlock(writer, "code", "csharp");

      Assert.That(
        writer.Build(),
        Is.EqualTo("[]<---------------->\ncode\n-[=================]")
      );
    }

    [Test]
    public void DictionaryWriter_PreservesCodeBlockContentAndLanguage() {
      var writer = new ProseDictionaryWriter();

      Prose.Prose.WriteCodeBlock(writer, "coolant.reset();", "csharp");

      var blocks = (List<object>)writer.Root[ProseDictionaryWriter.CodeBlocksKey];
      var block = (Dictionary<string, object>)blocks[0];
      Assert.That(block[ProseDictionaryWriter.ContentKey], Is.EqualTo("coolant.reset();"));
      Assert.That(block[ProseDictionaryWriter.LanguageKey], Is.EqualTo("csharp"));
    }

    [Test]
    public void UnityRichTextWriter_EmitsTagsWithoutCountingThemForWrappingOrLength() {
      var writer = new ProseUnityRichTextWriter(wrapWidth: 11);

      Prose.Prose.WriteSpan(writer, "alpha beta", ProseTextStyle.Strong);
      writer.Write(" gamma");

      Assert.That(writer.Build(), Is.EqualTo("<b>alpha beta</b>\ngamma"));
      Assert.That(writer.Length, Is.EqualTo("alpha beta\ngamma".Length));
    }

    [Test]
    public void PlainTextWriter_CopiesFormattedOutputToCallerOwnedSpan() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Markdown);
      Prose.Prose.WriteSpan(writer, "important", ProseTextStyle.Strong);

      Assert.That(writer.FormattedLength, Is.EqualTo("**important**".Length));
      Span<char> tooSmall = stackalloc char[4];
      Assert.That(writer.TryCopyTo(tooSmall, out var rejectedLength), Is.False);
      Assert.That(rejectedLength, Is.Zero);

      Span<char> destination = stackalloc char[32];
      Assert.That(writer.TryCopyTo(destination, out var charsWritten), Is.True);
      Assert.That(new string(destination[..charsWritten]), Is.EqualTo("**important**"));
    }

    [Test]
    public void PlainTextWriter_EmitsAtMostOneFullyBlankLineAtTheEnd() {
      var writer = new ProseTextWriter();
      writer.Write("value\n\n\n\n");

      Assert.That(writer.Build(), Is.EqualTo("value\n\n"));
    }

    [Test]
    public void UnityRichTextWriter_DefaultConfigurationIncludesAnAsciiTree() {
      var writer = new ProseUnityRichTextWriter();
      Prose.Prose.WriteName(writer, "Root");
      Prose.Prose.WriteProperty(writer, "Value", 1, ProseIntFormatter.Instance);
      Assert.That(writer.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(writer, "Child");
      Prose.Prose.WriteProperty(writer, "Child value", 2, ProseIntFormatter.Instance);
      writer.PopFrame();

      Assert.That(
        writer.Build(),
        Is.EqualTo("Root\nValue: 1\n|\n\\- Child\n   Child value: 2\n\n")
      );
    }

    [Test]
    public void UnityRichTextWriter_RendersSemanticMarkupLinksAndCodeBlocks() {
      var writer = new ProseUnityRichTextWriter();

      Prose.Prose.WriteSpan(
        writer, "important", ProseTextStyle.Emphasis | ProseTextStyle.Strong
      );
      writer.Write(" ");
      Prose.Prose.WriteSpan(writer, "value", ProseTextStyle.Code);
      writer.Write(" ");
      Prose.Prose.WriteSpan(writer, "quoted", ProseTextStyle.Quote);
      writer.Write(" ");
      Prose.Prose.WriteSpan(writer, "failed", ProseTextStyle.Error);
      writer.Write(" ");
      Prose.Prose.WriteSpan(writer, "docs", linkTarget: "https://example.test?a=1&b=2");
      Prose.Prose.WriteCodeBlock(writer, "coolant.reset();", "csharp");

      Assert.That(
        writer.Build(),
        Is.EqualTo(
          "<b><i>important</i></b> " +
          "<color=#DCDCAA>value</color> " +
          "<i><color=#A0A0A0>quoted</color></i> " +
          "<b><color=#FF6B6B>failed</color></b> " +
          "<link=\"https://example.test?a=1&amp;b=2\"><u>docs</u></link>\n\n" +
          "<color=#DCDCAA>coolant.reset();</color>"
        )
      );
    }

    [Test]
    public void UnityRichTextWriter_PadsSignificantBlocks() {
      var writer = new ProseUnityRichTextWriter();
      WriteStructuredProse(writer);
      Prose.Prose.WriteCodeBlock(writer, "run command", "shell");

      var result = writer.Build();
      Assert.That(result, Does.Contain("Status:\nEverything"));
      Assert.That(result, Does.Contain("</link>.\n\n3. First"));
      Assert.That(result, Does.Contain("4. Second\n\n| Name"));
      Assert.That(result, Does.Contain("| Alpha |     3 |\n\n<color=#DCDCAA>run command"));
    }

    [Test]
    public void UnityRichTextWriter_MeasuresFormattedTableCellsByVisibleText() {
      var writer = new ProseUnityRichTextWriter();
      Assert.That(writer.BeginFrame(ProseTable.Instance), Is.True);
      Assert.That(writer.BeginFrame(ProseTableRow.Body), Is.True);
      Assert.That(writer.BeginFrame(ProseTableCell.Instance), Is.True);
      Prose.Prose.WriteSpan(writer, "A", ProseTextStyle.Strong);
      writer.PopFrame();
      Prose.Prose.WriteTableCell(writer, "longer");
      writer.PopFrame();
      writer.PopFrame();

      Assert.That(writer.Build(), Is.EqualTo("\n| <b>A</b>   | longer |"));
    }

    [Test]
    public void PlainTextWriter_UsesReusableFormatterConfigurationAsAHint() {
      var writer = new ProseTextWriter();
      writer.Write(255, HexFormatter);
      writer.Write(" ");
      writer.Write(true, YesNoFormatter);
      Assert.That(writer.Build(), Is.EqualTo("FF yes"));
    }

    [Test]
    public void PropertyFormatter_IsSemanticMacroOrNativeDataDescriptor() {
      var textWriter = new ProseTextWriter();
      textWriter.Write(12, AgeFormatter);
      Assert.That(textWriter.Build(), Is.EqualTo("Age: 12 years\n\n"));

      var dictionaryWriter = new ProseDictionaryWriter();
      dictionaryWriter.Write(12, AgeFormatter);
      Assert.That(dictionaryWriter.Root["Age"], Is.TypeOf<int>().And.EqualTo(12));
    }

    [Test]
    public void Writers_FallBackToSelfFormattingProseAndCustomFormatters() {
      var writer = new ProseTextWriter();
      writer.Write("before");
      writer.Write(ProseLineBreak.Instance);
      writer.Write(new WrappedText("fallback"), WrappedTextFormatter.Instance);
      writer.Write(new WrappedProse(" prose"));
      Assert.That(writer.Build(), Is.EqualTo("before\nfallback prose"));
    }

    [Test]
    public void PlainTextWriter_DistinguishesHardAndSoftSemanticLineBreaks() {
      var writer = new ProseTextWriter(
        configuration: new ProseTextConfiguration(root: PTRuleFactory.Container())
      );
      writer.Write("first");
      writer.Write(ProseSoftLineBreak.Instance);
      writer.Write(ProseSoftLineBreak.Instance);
      writer.Write("second");
      writer.Write(ProseLineBreak.Instance);
      writer.Write(ProseLineBreak.Instance);
      writer.Write(ProseSoftLineBreak.Instance);
      writer.Write("third");

      Assert.That(writer.Build(), Is.EqualTo("first\nsecond\n\nthird"));
    }

    [Test]
    public void MarkdownConfiguration_RendersSemanticBreaksAsMarkdownHardBreaks() {
      var writer = new ProseTextWriter(
        configuration: ProseTextConfigurations.Markdown
      );
      writer.Write("first");
      writer.Write(ProseLineBreak.Instance);
      writer.Write("second");

      Assert.That(writer.Build(), Is.EqualTo("first  \nsecond"));
      Assert.That(writer.Length, Is.EqualTo("first\nsecond".Length));
    }

    private readonly struct WrappedText {
      public WrappedText(string value) => Value = value;
      public string Value { get; }
    }

    private static string RenderConfiguration(ProseTextConfiguration configuration) {
      var writer = new ProseTextWriter(configuration: configuration);
      Prose.Prose.WriteName(writer, "Root");
      Prose.Prose.WriteProperty(writer, "Value", 1, ProseIntFormatter.Instance);
      if (writer.BeginFrame(ProseTree.Instance)) {
        Prose.Prose.WriteName(writer, "Child");
        Prose.Prose.WriteProperty(writer, "Child value", 2, ProseIntFormatter.Instance);
        writer.PopFrame();
      }
      return writer.Build();
    }

    private static void WriteStructuredProse(IProseWriter writer) {
      Assert.That(writer.BeginFrame(ProseSection.Instance), Is.True);
      Prose.Prose.WriteSectionHeader(writer, "Status");

      Assert.That(writer.BeginFrame(ProseParagraph.Instance), Is.True);
      writer.Write("Everything ");
      Prose.Prose.WriteSpan(writer, "works", ProseTextStyle.Strong);
      writer.Write(". See ");
      Prose.Prose.WriteSpan(writer, "docs", linkTarget: "https://example.test");
      writer.Write(".");
      writer.PopFrame();

      Assert.That(writer.BeginFrame(new ProseList(ProseListKind.Ordered, 3)), Is.True);
      Prose.Prose.WriteListItem(writer, "First");
      Prose.Prose.WriteListItem(writer, "Second");
      writer.PopFrame();

      Assert.That(writer.BeginFrame(ProseTable.Instance), Is.True);
      Assert.That(writer.BeginFrame(ProseTableRow.Header), Is.True);
      Prose.Prose.WriteTableCell(writer, "Name");
      Prose.Prose.WriteTableCell(writer, "Count");
      writer.PopFrame();
      Assert.That(writer.BeginFrame(ProseTableRow.Body), Is.True);
      Prose.Prose.WriteTableCell(writer, "Alpha");
      Prose.Prose.WriteTableCell(
        writer, 3, ProseIntFormatter.Instance, ProseTextAlignment.Right
      );
      writer.PopFrame();
      writer.PopFrame();
      writer.PopFrame();
    }

    private static void WriteQuoteAndProperty(IProseWriter writer, string note) {
      Assert.That(writer.BeginFrame(ProseParagraph.Instance), Is.True);
      Prose.Prose.WriteSpan(writer, note, ProseTextStyle.Quote);
      writer.PopFrame();
      Prose.Prose.WriteProperty(writer, "State", "Degraded", ProseStringFormatter.Instance);
    }

    private static void WriteSingleCellTable(IProseWriter writer, string content) {
      Assert.That(writer.BeginFrame(ProseTable.Instance), Is.True);
      Assert.That(writer.BeginFrame(ProseTableRow.Body), Is.True);
      Prose.Prose.WriteTableCell(writer, content);
      writer.PopFrame();
      writer.PopFrame();
    }

    private static string RenderVisibility(bool showTrees, bool showNames, bool showProperties) {
      var configuration = new ProseTextConfiguration(
        root: PTRuleFactory.Container(),
        rootName: PTRuleFactory.Line(),
        treeName: PTRuleFactory.Line(),
        property: PTRuleFactory.Property(),
        propertyValue: PTRuleFactory.PropertyValue(),
        tree: PTRuleFactory.Tree("+- ", "`- ", "|  ", "   "),
        showTrees: showTrees,
        showNames: showNames,
        showProperties: showProperties
      );
      return RenderConfiguration(configuration);
    }

    private static PTLineRule[] EnabledLineBreaks() => new[] {
      new PTLineRule(
        TextMatching.None, LineMatching.None, 0,
        LineBreakMode.Item | LineBreakMode.Wrap | LineBreakMode.Hard
      )
    };

    private static PTLineRule[] NonTerminatingLineBreaks() => new[] {
      new PTLineRule(
        TextMatching.None, LineMatching.Last, 0, LineBreakMode.None
      ),
      new PTLineRule(
        TextMatching.None, LineMatching.None, 0, LineBreakMode.Wrap | LineBreakMode.Hard
      )
    };

    private sealed class WrappedTextFormatter : IProseFormatter<WrappedText> {
      public static readonly WrappedTextFormatter Instance = new();
      private WrappedTextFormatter() { }
      public void ToProse(IProseWriter writer, WrappedText value) => writer.Write(value.Value);
    }

    private static readonly ProseIntFormatter HexFormatter = new("X");
    private static readonly ProseBoolFormatter YesNoFormatter = new("yes", "no");
    private static readonly ProsePropertyFormatter<int> AgeFormatter =
      new("Age", new ProseIntFormatter(min: 0, max: 150, suffix: " years"));

    private sealed class ThrowingIntFormatter : IProseFormatter<int> {
      public static readonly ThrowingIntFormatter Instance = new();
      private ThrowingIntFormatter() { }
      public void ToProse(IProseWriter writer, int value) =>
        throw new AssertionException("The dictionary writer must retain the value without invoking its formatter.");
    }

    private sealed class WrappedProse : IProse {
      private readonly string _value;
      public WrappedProse(string value) => _value = value;
      public void ToProse(IProseWriter writer) => writer.Write(_value);
    }
  }
}
