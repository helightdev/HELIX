using System;
using System.Collections.Generic;
using HELIX.Prose;
using NUnit.Framework;

namespace HELIX.Tests {
  public sealed class ProseWriterTests {
    [Test]
    public void PlainTextWriter_RendersPropertiesTreesAndFiltering() {
      var writer = new ProseTextWriter(minimumLevel: ProseLevel.Info);

      writer.Name("Person");
      writer.Property("Age", 12, ProseFormatters.Int);
      writer.Property("Debug", "filtered", ProseFormatters.String, ProseLevel.Debug);
      writer.Property("Tag", (string)null, ProseFormatters.String, hidden: true);

      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("Item 1");
      writer.End();

      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("Subtree");
      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("Subtree Item");
      writer.End();
      writer.End();
      writer.Name("Tail");

      Assert.That(
        writer.Build(),
        Is.EqualTo("Person\nAge: 12\n│\n├─ Item 1\n└─ Subtree\n   └─ Subtree Item\nTail")
      );
    }

    [Test]
    public void BeginFrame_BalancesIgnoredFramesWhileTryBeginFrameRemainsOptIn() {
      var writer = new ProseTextWriter(
        configuration: new ProseTextConfiguration(
          root: PTRuleFactory.Container(),
          property: PTRuleFactory.Property(),
          propertyValue: PTRuleFactory.PropertyValue(),
          showProperties: false
        )
      );

      Assert.That(writer.TryBeginFrame(ProseScopes.Property), Is.False);

      writer.BeginFrame(ProseScopes.Property);
      writer.PushModifier(ProseModifiers.Strong);
      writer.Write("discarded");
      writer.BeginFrame(ProseScopes.PropertyValue);
      writer.Write("value");
      writer.End();
      writer.End();

      writer.Write("retained");
      Assert.That(writer.Build(), Is.EqualTo("retained"));
    }

    [Test]
    public void ScopeExtension_EndsFramesIncludingIgnoredFrames() {
      var writer = new ProseTextWriter(
        configuration: new ProseTextConfiguration(
          root: PTRuleFactory.Container(),
          property: PTRuleFactory.Property(),
          propertyValue: PTRuleFactory.PropertyValue(),
          showProperties: false
        )
      );

      using (writer.Scope(ProseScopes.Property)) {
        writer.Write("discarded");
        using (writer.Scope(ProseScopes.PropertyValue)) writer.Write(" value");
      }

      using (writer.Scope(ProseScopes.Paragraph)) writer.Write("retained");
      Assert.That(writer.Build(), Is.EqualTo("retained"));
    }

    [Test]
    public void TextWriters_HonorPropertyNameSeparatorAndDescriptionPresentation() {
      var configuration = new ProseTextConfiguration(
        root: PTRuleFactory.Container(),
        property: PTRuleFactory.Property(),
        propertyValue: PTRuleFactory.PropertyValue(),
        propertyDescription: PTRuleFactory.PropertyDescription(" => ")
      );
      var writer = new ProseTextWriter(configuration: configuration);

      writer.Property("Named", "value", ProseFormatters.String, hideSeparator: true);
      writer.Property("Hidden", "standalone", ProseFormatters.String, hideName: true);
      writer.Property(
        "State", 42, ProseFormatters.Int,
        description: "the answer"
      );

      Assert.That(writer.Build().TrimEnd(), Is.EqualTo("Namedvalue\nstandalone\nState => the answer"));

      var unity = new ProseUnityRichTextWriter(configuration: configuration);
      unity.Property("Named", "value", ProseFormatters.String, hideSeparator: true);
      unity.Property("Hidden", "standalone", ProseFormatters.String, hideName: true);
      unity.Property("State", 42, ProseFormatters.Int, description: "the answer");
      Assert.That(unity.Build().TrimEnd(), Is.EqualTo("Namedvalue\nstandalone\nState => the answer"));
    }

    [Test]
    public void TextWriter_TreatsConfiguredDefaultValuesAsFine() {
      var writer = new ProseTextWriter(minimumLevel: ProseLevel.Info);
      writer.Property("Unchanged", 5, ProseFormatters.Int, defaultValue: 5);
      writer.Property("Changed", 6, ProseFormatters.Int, defaultValue: 5);

      Assert.That(writer.Build().TrimEnd(), Is.EqualTo("Changed: 6"));

      writer.Reset();
      writer.MinimumLevel = ProseLevel.Fine;
      writer.Property("Unchanged", 5, ProseFormatters.Int, defaultValue: 5);
      Assert.That(writer.Build().TrimEnd(), Is.EqualTo("Unchanged: 5"));
    }

    [Test]
    public void DictionaryWriter_RetainsRawValueWhenTextUsesDescription() {
      var writer = new ProseDictionaryWriter();
      writer.Property(
        "Answer", 42, ThrowingIntFormatter.Instance,
        hideName: true, hideSeparator: true, description: "the answer", defaultValue: 42
      );

      Assert.That(writer.Root["Answer"], Is.EqualTo(42));
    }

    [Test]
    public void PlainTextWriter_WrapsUnlessNoWrapIsActive() {
      var writer = new ProseTextWriter(wrapWidth: 12);

      Assert.That(writer.TryBeginFrame(ProseScopes.Property), Is.True);
      Assert.That(writer.TryBeginFrame(ProseScopes.PropertyKey), Is.True);
      writer.Write("Key");
      writer.End();
      Assert.That(writer.TryBeginFrame(ProseScopes.PropertyValue), Is.True);
      writer.Write("one two three");
      writer.End();
      writer.End();

      Assert.That(writer.Build(), Is.EqualTo("Key: one two\n three\n\n"));

      writer.Reset();
      Assert.That(writer.TryBeginFrame(ProseScopes.Property), Is.True);
      writer.PushModifier(ProseModifiers.NoWrap);
      writer.Write("one two three four");
      writer.End();
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

      writer.Property("Summary", "alpha beta gamma", ProseFormatters.String
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

      Assert.That(writer.TryBeginFrame(ProseScopes.Property), Is.True);
      writer.PushModifier(ProseModifiers.AllowTruncate);
      writer.Write("abcdefgh");
      writer.Write("ignored");
      writer.End();

      Assert.That(writer.Build(), Is.EqualTo("abcde…\n\n"));
    }

    [Test]
    public void PlainTextWriter_UsesConfiguredBoundariesAndRetainsAncestors() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Sparse);

      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("Parent");
      writer.Property("State", "ready", ProseFormatters.String);
      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("First");
      writer.End();
      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("Last");
      writer.End();
      writer.End();

      Assert.That(
        writer.Build(),
        Is.EqualTo("└─ Parent\n   State: ready\n   │\n   ├─ First\n   └─ Last")
      );
    }

    [Test]
    public void PlainTextWriter_RetainsTreeBoundaryAcrossWrappedPropertyLines() {
      var writer = new ProseTextWriter(wrapWidth: 30);

      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("First");
      writer.Property(
        "Message",
        "alpha beta gamma delta epsilon",
        ProseFormatters.String
      );
      writer.End();
      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("Second");
      writer.End();

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

      writer.Property(
        "Message",
        "alpha beta gamma delta",
        ProseFormatters.String
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
      populated.Property("A", 1, ProseFormatters.Int);
      populated.Property("B", 2, ProseFormatters.Int);
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
      Assert.That(populated.TryBeginFrame(ProseScopes.Tree), Is.True);
      populated.Name("Child");
      populated.End();
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

      writer.Property("A", 1, ProseFormatters.Int);
      writer.Property("B", 2, ProseFormatters.Int);

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

      Assert.That(writer.TryBeginFrame(ProseScopes.Property), Is.True);
      writer.End();

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
      Assert.That(nested.TryBeginFrame(ProseScopes.Property), Is.True);
      nested.Write("first\nsecond");
      nested.End();
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
      Assert.That(writer.TryBeginFrame(ProseScopes.Property), Is.True);
      writer.Write("a");
      writer.End();
      Assert.That(writer.TryBeginFrame(ProseScopes.Property), Is.True);
      writer.End();

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
      writer.Name("Root");
      writer.Property("First", 1, ProseFormatters.Int);
      writer.Property("Second", 2, ProseFormatters.Int);

      Assert.That(writer.Build(), Is.EqualTo("Root(First: 1, Second: 2)"));
    }

    [Test]
    public void PlainTextConfigurations_ShallowTerminatesWrappedProductionOutput() {
      var writer = new ProseTextWriter(
        wrapWidth: 96,
        configuration: ProseTextConfigurations.Shallow
      );
      writer.Name("Asteria Orbital Relay Station");
      writer.Property("Mission ID", "HX-ASTERIA-07", ProseFormatters.String);
      writer.Property("State", "Degraded", ProseFormatters.String);
      writer.Property("Crew aboard", "37 people", ProseFormatters.String);
      writer.Property("Orbit", "1842 completed", ProseFormatters.String);
      writer.Property("Autonomous control", "operational", ProseFormatters.String);
      writer.Property(
        "Summary",
        "Long-range relay and research platform holding a stable polar orbit while its secondary coolant " +
        "loop is isolated for inspection.",
        ProseFormatters.String
      );
      writer.Property(
        "Internal tracking token",
        "OPS-4A-9912",
        ProseFormatters.String,
        hidden: true
      );

      Assert.That(writer.Build(), Does.EndWith("inspection.)"));
    }

    [Test]
    public void PlainTextConfigurations_RenderACleanErrorSignature() {
      var error = RenderConfiguration(ProseTextConfigurations.Error);
      Assert.That(error, Does.StartWith("== Root =="));
      Assert.That(error, Does.Contain("\\- Child"));
    }

    [Test]
    public void PlainTextConfigurations_KeepStructuralPresetsLimitedToTreesAndProperties() {
      foreach (var configuration in new[] {
                 ProseTextConfigurations.Sparse,
                 ProseTextConfigurations.Shallow
               }) {
        var writer = new ProseTextWriter(configuration: configuration);
        writer.Name("Root");
        writer.Property("Value", 1, ProseFormatters.Int);
        writer.WriteSpan("important", ProseTextStyle.Strong);
        writer.WriteSpan("docs", linkTarget: "https://example.test");
        writer.WriteCodeBlock("run command", "shell");
        Assert.That(writer.TryBeginFrame(ProseScopes.Section), Is.False);
        Assert.That(writer.TryBeginFrame(ProseScopes.UnorderedList), Is.False);
        Assert.That(writer.TryBeginFrame(ProseScopes.Table), Is.False);

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
      writer.Name("Root");
      writer.Property("Value", 1, ProseFormatters.Int);
      WriteStructuredProse(writer);
      writer.WriteCodeBlock("run command", "shell");

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
      writer.WriteSpan("strong", ProseTextStyle.Strong);
      writer.Write(" ");
      writer.WriteSpan("code", ProseTextStyle.Code);
      writer.Write(" ");
      writer.WriteSpan("failure", ProseTextStyle.Error);

      Assert.That(writer.Build(), Is.EqualTo("**strong** `code` ‼ failure"));
    }

    [Test]
    public void ErrorConfiguration_UsesItsDiagnosticPrefixOnlyForProperties() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Error);

      Assert.That(writer.TryBeginFrame(ProseScopes.Paragraph), Is.True);
      writer.Write("Station narrative.");
      writer.End();
      writer.Property("State", "Degraded", ProseFormatters.String);

      Assert.That(writer.TryBeginFrame(ProseScopes.Paragraph), Is.True);
      writer.WriteSpan("Operator note.", ProseTextStyle.Quote);
      writer.End();
      writer.Property("Owner", "Operations", ProseFormatters.String);

      Assert.That(
        writer.Build(),
        Is.EqualTo(
          "\nStation narrative.\n\n" +
          ": State: Degraded\n\n" +
          "> Operator note.\n\n" +
          ": Owner: Operations"
        )
      );
    }

    [Test]
    public void DictionaryWriter_CapturesTheDataTreeAndIgnoresFormatters() {
      var writer = new ProseDictionaryWriter();
      writer.Name("Person");
      writer.Property("Age", 12, ThrowingIntFormatter.Instance, ProseLevel.Warning);

      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("Item");
      writer.Property("Enabled", true, ProseFormatters.Bool);
      writer.End();

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
      Assert.That(writer.TryBeginFrame(ProseScopes.Table), Is.True);
      Assert.That(writer.TryBeginFrame(ProseScopes.TableHeaderRow), Is.True);
      writer.WriteTableCell("Name");
      writer.WriteTableCell("Kind");
      writer.End();
      Assert.That(writer.TryBeginFrame(ProseScopes.TableBodyRow), Is.True);
      writer.WriteTableCell("extraordinary");
      writer.WriteTableCell("x");
      writer.End();
      writer.End();

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
      Assert.That(defaultWriter.Build(), Is.EqualTo("| alpha ¶ beta |"));

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
      writer.WriteSpan("important", ProseTextStyle.Emphasis | ProseTextStyle.Strong
      );
      writer.Write(" ");
      writer.WriteSpan("value", ProseTextStyle.Code);
      writer.Write(" ");
      writer.WriteSpan("quoted", ProseTextStyle.Quote);
      writer.Write(" ");
      writer.WriteSpan("failed", ProseTextStyle.Error);

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

      writer.WriteCodeBlock("var value = 123;\nreturn value;", "csharp");

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

      writer.WriteCodeBlock("code", "csharp");

      Assert.That(
        writer.Build(),
        Is.EqualTo("[]<---------------->\ncode\n-[=================]")
      );
    }

    [Test]
    public void DictionaryWriter_PreservesCodeBlockContentAndLanguage() {
      var writer = new ProseDictionaryWriter();

      writer.WriteCodeBlock("coolant.reset();", "csharp");

      var blocks = (List<object>)writer.Root[ProseDictionaryWriter.CodeBlocksKey];
      var block = (Dictionary<string, object>)blocks[0];
      Assert.That(block[ProseDictionaryWriter.ContentKey], Is.EqualTo("coolant.reset();"));
      Assert.That(block[ProseDictionaryWriter.LanguageKey], Is.EqualTo("csharp"));
    }

    [Test]
    public void UnityRichTextWriter_EmitsTagsWithoutCountingThemForWrappingOrLength() {
      var writer = new ProseUnityRichTextWriter(wrapWidth: 11);

      writer.WriteSpan("alpha beta", ProseTextStyle.Strong);
      writer.Write(" gamma");

      Assert.That(writer.Build(), Is.EqualTo("<b>alpha beta</b>\ngamma"));
      Assert.That(writer.Length, Is.EqualTo("alpha beta\ngamma".Length));
    }

    [Test]
    public void PlainTextWriter_CopiesFormattedOutputToCallerOwnedSpan() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Markdown);
      writer.WriteSpan("important", ProseTextStyle.Strong);

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
      writer.Name("Root");
      writer.Property("Value", 1, ProseFormatters.Int);
      Assert.That(writer.TryBeginFrame(ProseScopes.Tree), Is.True);
      writer.Name("Child");
      writer.Property("Child value", 2, ProseFormatters.Int);
      writer.End();

      Assert.That(
        writer.Build(),
        Is.EqualTo("Root\nValue: 1\n|\n\\- Child\n   Child value: 2\n\n")
      );
    }

    [Test]
    public void UnityRichTextWriter_RendersSemanticMarkupLinksAndCodeBlocks() {
      var writer = new ProseUnityRichTextWriter();

      writer.WriteSpan("important", ProseTextStyle.Emphasis | ProseTextStyle.Strong
      );
      writer.Write(" ");
      writer.WriteSpan("value", ProseTextStyle.Code);
      writer.Write(" ");
      writer.WriteSpan("quoted", ProseTextStyle.Quote);
      writer.Write(" ");
      writer.WriteSpan("failed", ProseTextStyle.Error);
      writer.Write(" ");
      writer.WriteSpan("docs", linkTarget: "https://example.test?a=1&b=2");
      writer.WriteCodeBlock("coolant.reset();", "csharp");

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
      writer.WriteCodeBlock("run command", "shell");

      var result = writer.Build();
      Assert.That(result, Does.Contain("Status:\nEverything"));
      Assert.That(result, Does.Contain("</link>.\n\n3. First"));
      Assert.That(result, Does.Contain("4. Second\n\n| Name"));
      Assert.That(result, Does.Contain("| Alpha |     3 |\n\n<color=#DCDCAA>run command"));
    }

    [Test]
    public void UnityRichTextWriter_MeasuresFormattedTableCellsByVisibleText() {
      var writer = new ProseUnityRichTextWriter();
      Assert.That(writer.TryBeginFrame(ProseScopes.Table), Is.True);
      Assert.That(writer.TryBeginFrame(ProseScopes.TableBodyRow), Is.True);
      Assert.That(writer.TryBeginFrame(ProseScopes.TableCell), Is.True);
      writer.WriteSpan("A", ProseTextStyle.Strong);
      writer.End();
      writer.WriteTableCell("longer");
      writer.End();
      writer.End();

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
    public void ConfiguredFormatters_CoverDiagnosticValuePresentation() {
      var writer = new ProseTextWriter();
      writer.Write("", new ProseStringFormatter(emptyText: "<empty>", quoted: true));
      writer.Write(" | ");
      writer.Write("value", new ProseStringFormatter(prefix: "<", suffix: ">", quoted: true));
      writer.Write(" | ");
      writer.Write((int?)null, new ProseIntFormatter(nullText: "missing"));
      writer.Write(" | ");
      writer.Write(12.5f, new ProseFloatFormatter(compact: true, unit: "px"));
      writer.Write(" | ");
      writer.Write(12.5f, ProseFormatters.Percent);
      writer.Write(" load");
      writer.Write(" | ");
      writer.Write(1.2f, ProseFormatters.PercentNormalized);
      writer.Write(" normalized");
      writer.Write(" | ");
      writer.Write((bool?)null, new ProseBoolFormatter("yes", "no", "unknown"));
      writer.Write(" | ");
      writer.Write(3, new ProseFormattingFormatter<int>(value => "#" + value));

      Assert.That(
        writer.Build(),
        Is.EqualTo(
          "<empty> | <\"value\"> | missing | 12.5px | 12.5% load | 120.0% normalized | unknown | #3"
        )
      );
    }

    [Test]
    public void DefaultFormatters_AreOwnedByTheCentralRegistry() {
      Assert.That(ProseFormatters.String, Is.SameAs(ProseFormatters.String));
      Assert.That(ProseFormatters.Int, Is.SameAs(ProseFormatters.Int));
      Assert.That(ProseFormatters.Enum<DayOfWeek>(), Is.SameAs(ProseFormatters.Enum<DayOfWeek>()));
      Assert.That(ProseFormatters.Object<object>(), Is.SameAs(ProseFormatters.Object<object>()));
      Assert.That(typeof(ProseStringFormatter).GetField("Instance"), Is.Null);
      Assert.That(typeof(ProseIntFormatter).GetField("Instance"), Is.Null);
    }

    [Test]
    public void DefaultScopesAndModifiers_AreOwnedByTheirCentralRegistries() {
      Assert.That(ProseScopes.Tree, Is.SameAs(ProseScopes.Tree));
      Assert.That(ProseScopes.OrderedList, Is.SameAs(ProseScopes.OrderedList));
      Assert.That(ProseModifiers.Strong, Is.SameAs(ProseModifiers.Strong));
      Assert.That(ProseModifiers.Level(ProseLevel.Info), Is.SameAs(ProseModifiers.Level(ProseLevel.Info)));
      Assert.That(typeof(ProseTree).GetField("Instance"), Is.Null);
      Assert.That(typeof(ProseHiddenModifier).GetField("Instance"), Is.Null);
    }

    [Test]
    public void ScopeExtensions_StartBuiltInScopesAndApplyTheirMetadata() {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Markdown);

      writer.WriteSpan("linked", ProseTextStyle.Strong, "https://example.test");

      using (writer.Scope(new ProseList(ProseListKind.Ordered, 3)))
      using (writer.Scope(ProseScopes.ListItem))
        writer.Write("third");

      using (writer.Scope(ProseScopes.Table)) {
        using (writer.Scope(ProseScopes.TableHeaderRow))
        using (writer.Scope(ProseScopes.TableCell)) {
          writer.PushModifier(ProseModifiers.Center);
          writer.Write("centered");
        }

        using (writer.Scope(ProseScopes.TableBodyRow))
        using (writer.Scope(ProseScopes.TableCell)) {
          writer.PushModifier(ProseModifiers.Center);
          writer.Write("x");
        }
      }

      var result = writer.Build();
      Assert.That(result, Does.Contain("[**linked**](https://example.test)"));
      Assert.That(result, Does.Contain("3. third"));
      Assert.That(result, Does.Contain("| centered |").And.Contain("|    x     |"));
    }

    [Test]
    public void IterableFormatter_StreamsItemsAndHandlesNullAndEmptyValues() {
      var formatter = new ProseIterableFormatter<int>(
        new ProseIntFormatter(format: "X"), prefix: "{", separator: "; ", suffix: "}"
      );
      var writer = new ProseTextWriter();
      writer.Write<IEnumerable<int>>(new[] { 10, 11, 12 }, formatter);
      writer.Write(" | ");
      writer.Write<IEnumerable<int>>(Array.Empty<int>(), formatter);
      writer.Write(" | ");
      writer.Write<IEnumerable<int>>(null, formatter);

      Assert.That(writer.Build(), Is.EqualTo("{A; B; C} | [] | null"));
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
      writer.Name("Root");
      writer.Property("Value", 1, ProseFormatters.Int);
      if (writer.TryBeginFrame(ProseScopes.Tree)) {
        writer.Name("Child");
        writer.Property("Child value", 2, ProseFormatters.Int);
        writer.End();
      }
      return writer.Build();
    }

    private static void WriteStructuredProse(IProseWriter writer) {
      Assert.That(writer.TryBeginFrame(ProseScopes.Section), Is.True);
      writer.WriteSectionHeader("Status");

      Assert.That(writer.TryBeginFrame(ProseScopes.Paragraph), Is.True);
      writer.Write("Everything ");
      writer.WriteSpan("works", ProseTextStyle.Strong);
      writer.Write(". See ");
      writer.WriteSpan("docs", linkTarget: "https://example.test");
      writer.Write(".");
      writer.End();

      Assert.That(writer.TryBeginFrame(new ProseList(ProseListKind.Ordered, 3)), Is.True);
      writer.WriteListItem("First");
      writer.WriteListItem("Second");
      writer.End();

      Assert.That(writer.TryBeginFrame(ProseScopes.Table), Is.True);
      Assert.That(writer.TryBeginFrame(ProseScopes.TableHeaderRow), Is.True);
      writer.WriteTableCell("Name");
      writer.WriteTableCell("Count");
      writer.End();
      Assert.That(writer.TryBeginFrame(ProseScopes.TableBodyRow), Is.True);
      writer.WriteTableCell("Alpha");
      writer.WriteTableCell(3, ProseFormatters.Int, ProseTextAlignment.Right
      );
      writer.End();
      writer.End();
      writer.End();
    }

    private static void WriteQuoteAndProperty(IProseWriter writer, string note) {
      Assert.That(writer.TryBeginFrame(ProseScopes.Paragraph), Is.True);
      writer.WriteSpan(note, ProseTextStyle.Quote);
      writer.End();
      writer.Property("State", "Degraded", ProseFormatters.String);
    }

    private static void WriteSingleCellTable(IProseWriter writer, string content) {
      Assert.That(writer.TryBeginFrame(ProseScopes.Table), Is.True);
      Assert.That(writer.TryBeginFrame(ProseScopes.TableBodyRow), Is.True);
      writer.WriteTableCell(content);
      writer.End();
      writer.End();
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
