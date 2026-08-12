using System.Collections.Generic;
using HELIX.Prose;
using NUnit.Framework;

namespace HELIX.Tests {
  public sealed class ProseWriterTests {
    [Test]
    public void PlainTextWriter_RendersPropertiesTreesAndFiltering() {
      var writer = new ProsePlainTextWriter(minimumLevel: ProseLevel.Info);

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
        Is.EqualTo("Person\nAge: 12\n├─ Item 1\n└─ Subtree\n   └─ Subtree Item\nTail")
      );
    }

    [Test]
    public void PlainTextWriter_WrapsUnlessNoWrapIsActive() {
      var writer = new ProsePlainTextWriter(wrapWidth: 12);

      Assert.That(writer.BeginFrame(ProseProperty.Instance), Is.True);
      Assert.That(writer.BeginFrame(ProsePropertyKey.Instance), Is.True);
      writer.Write("Key");
      writer.PopFrame();
      Assert.That(writer.BeginFrame(ProsePropertyValue.Instance), Is.True);
      writer.Write("one two three");
      writer.PopFrame();
      writer.PopFrame();

      Assert.That(writer.Build(), Is.EqualTo("Key: one two\n     three"));

      writer.Reset();
      Assert.That(writer.BeginFrame(ProseProperty.Instance), Is.True);
      writer.PushModifier(NoWrap.Instance);
      writer.Write("one two three four");
      writer.PopFrame();
      Assert.That(writer.Build(), Is.EqualTo("one two three four"));
    }

    [Test]
    public void PlainTextWriter_HonorsTruncation() {
      var writer = new ProsePlainTextWriter(maxTruncatableFrameLength: 5);

      Assert.That(writer.BeginFrame(ProseProperty.Instance), Is.True);
      writer.PushModifier(AllowTruncate.Instance);
      writer.Write("abcdefgh");
      writer.Write("ignored");
      writer.PopFrame();

      Assert.That(writer.Build(), Is.EqualTo("abcde…"));
    }

    [Test]
    public void PlainTextWriter_UsesConfiguredBoundariesAndRetainsAncestors() {
      var writer = new ProsePlainTextWriter(configuration: ProsePlainTextConfigurations.Ascii);

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
        Is.EqualTo("`- Parent\n   State: ready\n   +- First\n   `- Last")
      );
    }

    [Test]
    public void PlainTextWriter_RetainsTreeBoundaryAcrossWrappedPropertyLines() {
      var writer = new ProsePlainTextWriter(wrapWidth: 30);

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

      Assert.That(writer.Build(), Does.Contain("│  Message: alpha beta gamma\n│           delta epsilon"));
    }

    [Test]
    public void PlainTextWriter_PreconfiguredFlatModesHaveDistinctChildSemantics() {
      Assert.That(
        RenderConfiguration(ProsePlainTextConfigurations.Whitespace),
        Is.EqualTo("Root\nValue: 1\n  Child\n  Child value: 2")
      );
      Assert.That(
        RenderConfiguration(ProsePlainTextConfigurations.Flat),
        Is.EqualTo("Root\nValue: 1\nChild\nChild value: 2")
      );
      Assert.That(
        RenderConfiguration(ProsePlainTextConfigurations.CurrentObjectFlat),
        Is.EqualTo("Root\nValue: 1")
      );
    }

    [Test]
    public void PlainTextWriter_ConfiguresLineBreaksAndContinuationPrefixes() {
      var configuration = new ProsePlainTextConfiguration(
        childPrefix: "",
        lastChildPrefix: "",
        continuationPrefix: "",
        lastContinuationPrefix: "",
        lineBreak: "\r\n",
        wrappedLinePrefix: "> ",
        explicitLineBreakPrefix: "! ",
        alignWrappedPropertyValues: false
      );
      var writer = new ProsePlainTextWriter(wrapWidth: 25, configuration: configuration);

      Prose.Prose.WriteProperty(
        writer,
        "Message",
        "alpha beta gamma delta",
        ProseStringFormatter.Instance
      );
      writer.Write("first\nsecond");

      Assert.That(writer.Build(), Is.EqualTo("Message: alpha beta gamma\r\n> delta\r\nfirst\r\n! second"));
    }

    [Test]
    public void PlainTextWriter_InjectsConditionalAndMandatoryPropertyContent() {
      var configuration = new ProsePlainTextConfiguration(
        childPrefix: "",
        lastChildPrefix: "",
        continuationPrefix: "",
        lastContinuationPrefix: "",
        lineBreakProperties: false,
        beforeProperties: "[",
        afterProperties: "]",
        mandatoryAfterProperties: "!",
        propertySeparator: ", "
      );
      var populated = new ProsePlainTextWriter(configuration: configuration);
      Prose.Prose.WriteProperty(populated, "A", 1, ProseIntFormatter.Instance);
      Prose.Prose.WriteProperty(populated, "B", 2, ProseIntFormatter.Instance);
      Assert.That(populated.Build(), Is.EqualTo("[A: 1, B: 2]!"));

      var empty = new ProsePlainTextWriter(configuration: configuration);
      Assert.That(empty.Build(), Is.EqualTo("!"));
    }

    [Test]
    public void PlainTextWriter_InjectsChildContentOnlyWhenChildrenExist() {
      var configuration = new ProsePlainTextConfiguration(
        childPrefix: "",
        lastChildPrefix: "",
        continuationPrefix: "",
        lastContinuationPrefix: "",
        beforeChildren: "<",
        footer: ">",
        mandatoryFooter: "!"
      );
      var populated = new ProsePlainTextWriter(configuration: configuration);
      Assert.That(populated.BeginFrame(ProseTree.Instance), Is.True);
      Prose.Prose.WriteName(populated, "Child");
      populated.PopFrame();
      Assert.That(populated.Build(), Is.EqualTo("<\nChild\n!>!"));

      var empty = new ProsePlainTextWriter(configuration: configuration);
      Assert.That(empty.Build(), Is.EqualTo("!"));
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
    public void PlainTextWriter_UsesReusableFormatterConfigurationAsAHint() {
      var writer = new ProsePlainTextWriter();
      writer.Write(255, HexFormatter);
      writer.Write(" ");
      writer.Write(true, YesNoFormatter);
      Assert.That(writer.Build(), Is.EqualTo("FF yes"));
    }

    [Test]
    public void PropertyFormatter_IsSemanticMacroOrNativeDataDescriptor() {
      var textWriter = new ProsePlainTextWriter();
      textWriter.Write(12, AgeFormatter);
      Assert.That(textWriter.Build(), Is.EqualTo("Age: 12 years"));

      var dictionaryWriter = new ProseDictionaryWriter();
      dictionaryWriter.Write(12, AgeFormatter);
      Assert.That(dictionaryWriter.Root["Age"], Is.TypeOf<int>().And.EqualTo(12));
    }

    [Test]
    public void Writers_FallBackToSelfFormattingProseAndCustomFormatters() {
      var writer = new ProsePlainTextWriter();
      writer.Write("before");
      writer.Write(ProseLineBreak.Instance);
      writer.Write(new WrappedText("fallback"), WrappedTextFormatter.Instance);
      writer.Write(new WrappedProse(" prose"));
      Assert.That(writer.Build(), Is.EqualTo("before\nfallback prose"));
    }

    private readonly struct WrappedText {
      public WrappedText(string value) => Value = value;
      public string Value { get; }
    }

    private static string RenderConfiguration(ProsePlainTextConfiguration configuration) {
      var writer = new ProsePlainTextWriter(configuration: configuration);
      Prose.Prose.WriteName(writer, "Root");
      Prose.Prose.WriteProperty(writer, "Value", 1, ProseIntFormatter.Instance);
      if (writer.BeginFrame(ProseTree.Instance)) {
        Prose.Prose.WriteName(writer, "Child");
        Prose.Prose.WriteProperty(writer, "Child value", 2, ProseIntFormatter.Instance);
        writer.PopFrame();
      }
      return writer.Build();
    }

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
