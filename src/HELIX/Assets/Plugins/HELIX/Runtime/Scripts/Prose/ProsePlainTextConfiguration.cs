using System;
using System.Collections.Generic;
using System.Text;

namespace HELIX.Prose {
  [Flags]
  public enum TextMatching : byte {
    None = 0,
    First = 1 << 0,
    Last = 1 << 1,
    Odd = 1 << 2,
    Empty = 1 << 3
  }

  [Flags]
  public enum LineMatching : byte {
    None = 0,
    First = 1 << 0,
    Hard = 1 << 1,
    Last = 1 << 2
  }

  public readonly struct AnchorEvaluationEntry {
    public AnchorEvaluationEntry(TextMatching matching, int priority, string value) {
      Matching = matching;
      Priority = priority;
      String = value ?? string.Empty;
    }

    public TextMatching Matching { get; }
    public int Priority { get; }
    public string String { get; }
  }

  public readonly struct LineEvaluationEntry {
    public LineEvaluationEntry(TextMatching matching, LineMatching lineMatching, int priority, string value) {
      Matching = matching;
      LineMatching = lineMatching;
      Priority = priority;
      String = value ?? string.Empty;
    }

    public TextMatching Matching { get; }
    public LineMatching LineMatching { get; }
    public int Priority { get; }
    public string String { get; }
  }

  /// <summary>Controls which boundaries are emitted and whether wrapped continuations align.</summary>
  [Flags]
  public enum LineBreakMode : byte {
    None = 0,
    Align = 1 << 0,
    Item = 1 << 1,
    Wrap = 1 << 2,
    Hard = 1 << 3
  }

  public readonly struct LineBreakEvaluationEntry {
    public LineBreakEvaluationEntry(
      TextMatching matching, LineMatching lineMatching, int priority, LineBreakMode value
    ) {
      Matching = matching;
      LineMatching = lineMatching;
      Priority = priority;
      Value = value;
    }

    public TextMatching Matching { get; }
    public LineMatching LineMatching { get; }
    public int Priority { get; }
    public LineBreakMode Value { get; }
  }

  /// <summary>
  /// Formatting properties resolved from an item's collection and line state. Entries are evaluated in
  /// definition order. Matching strings are appended and matching line-break modes are combined. After a
  /// match, following entries of the same or lower priority are skipped; a later, strictly higher-priority
  /// match is additive.
  /// </summary>
  public sealed class ItemAnchors {
    private static readonly AnchorEvaluationEntry[] NoAnchors = Array.Empty<AnchorEvaluationEntry>();
    private static readonly LineEvaluationEntry[] NoLines = Array.Empty<LineEvaluationEntry>();
    private static readonly LineBreakEvaluationEntry[] NoLineBreaks =
      Array.Empty<LineBreakEvaluationEntry>();

    public ItemAnchors(
      IReadOnlyList<AnchorEvaluationEntry> prefix = null,
      IReadOnlyList<AnchorEvaluationEntry> suffix = null,
      IReadOnlyList<AnchorEvaluationEntry> replacement = null,
      IReadOnlyList<LineEvaluationEntry> linePrefix = null,
      IReadOnlyList<LineBreakEvaluationEntry> lineBreak = null,
      int suffixRepeater = -1
    ) {
      Prefix = prefix ?? NoAnchors;
      Suffix = suffix ?? NoAnchors;
      Replacement = replacement ?? NoAnchors;
      LinePrefix = linePrefix ?? NoLines;
      LineBreak = lineBreak ?? NoLineBreaks;
      SuffixRepeater = suffixRepeater;
    }

    public IReadOnlyList<AnchorEvaluationEntry> Prefix { get; }
    public IReadOnlyList<AnchorEvaluationEntry> Suffix { get; }
    public IReadOnlyList<AnchorEvaluationEntry> Replacement { get; }
    public IReadOnlyList<LineEvaluationEntry> LinePrefix { get; }
    public IReadOnlyList<LineBreakEvaluationEntry> LineBreak { get; }
    public int SuffixRepeater { get; }

    internal void AppendPrefix(StringBuilder builder, TextMatching matching) => Append(builder, Prefix, matching);
    internal void AppendSuffix(StringBuilder builder, TextMatching matching) => Append(builder, Suffix, matching);
    internal bool AppendReplacement(StringBuilder builder, TextMatching matching) =>
      Append(builder, Replacement, matching);

    internal bool AppendLinePrefix(
      StringBuilder builder, TextMatching matching, LineMatching lineMatching
    ) => Append(builder, LinePrefix, matching, lineMatching);

    internal LineBreakMode EvaluateLineBreak(TextMatching matching, LineMatching lineMatching) {
      var matched = false;
      var priority = int.MinValue;
      var value = LineBreakMode.None;
      for (var i = 0; i < LineBreak.Count; i++) {
        var entry = LineBreak[i];
        if ((matched && entry.Priority <= priority) ||
            !Matches(entry.Matching, matching) ||
            !Matches(entry.LineMatching, lineMatching)) continue;
        value |= entry.Value;
        priority = entry.Priority;
        matched = true;
      }
      return value;
    }

    private static bool Append(
      StringBuilder builder, IReadOnlyList<AnchorEvaluationEntry> entries, TextMatching matching
    ) {
      var matched = false;
      var priority = int.MinValue;
      for (var i = 0; i < entries.Count; i++) {
        var entry = entries[i];
        if ((matched && entry.Priority <= priority) || !Matches(entry.Matching, matching)) continue;
        builder.Append(entry.String);
        priority = entry.Priority;
        matched = true;
      }
      return matched;
    }

    private static bool Append(
      StringBuilder builder, IReadOnlyList<LineEvaluationEntry> entries,
      TextMatching matching, LineMatching lineMatching
    ) {
      var matched = false;
      var priority = int.MinValue;
      for (var i = 0; i < entries.Count; i++) {
        var entry = entries[i];
        if ((matched && entry.Priority <= priority) ||
            !Matches(entry.Matching, matching) ||
            !Matches(entry.LineMatching, lineMatching)) continue;
        builder.Append(entry.String);
        priority = entry.Priority;
        matched = true;
      }
      return matched;
    }

    private static bool Matches(TextMatching required, TextMatching actual) =>
      required == TextMatching.None || (actual & required) == required;

    private static bool Matches(LineMatching required, LineMatching actual) =>
      required == LineMatching.None || (actual & required) == required;
  }

  /// <summary>State properties used to project semantic Prose items into plain text.</summary>
  public sealed class ProsePlainTextConfiguration {
    private static readonly ItemAnchors NoAnchors = new();

    public ProsePlainTextConfiguration(
      ItemAnchors root = null,
      ItemAnchors rootName = null,
      ItemAnchors treeName = null,
      ItemAnchors property = null,
      ItemAnchors propertyValue = null,
      ItemAnchors tree = null,
      bool showTrees = true,
      bool showNames = true,
      bool showProperties = true
    ) {
      Root = root ?? NoAnchors;
      RootName = rootName ?? NoAnchors;
      TreeName = treeName ?? NoAnchors;
      Property = property ?? NoAnchors;
      PropertyValue = propertyValue ?? NoAnchors;
      Tree = tree ?? NoAnchors;
      ShowTrees = showTrees;
      ShowNames = showNames;
      ShowProperties = showProperties;
    }

    public ItemAnchors Root { get; }
    public ItemAnchors RootName { get; }
    public ItemAnchors TreeName { get; }
    public ItemAnchors Property { get; }
    public ItemAnchors PropertyValue { get; }
    public ItemAnchors Tree { get; }
    public bool ShowTrees { get; }
    public bool ShowNames { get; }
    public bool ShowProperties { get; }

  }

  public static class ProsePlainTextConfigurations {
    public static readonly ProsePlainTextConfiguration Unicode = Tree("├─ ", "└─ ", "│  ", "   ");
    public static readonly ProsePlainTextConfiguration Ascii = Tree(
      "+- ", "`- ", "|  ", "   ", alignWrappedPropertyValues: false
    );
    public static readonly ProsePlainTextConfiguration Whitespace = Tree("  ", "  ", "  ", "  ");
    public static readonly ProsePlainTextConfiguration Flat = Tree("", "", "", "");

    public static readonly ProsePlainTextConfiguration CurrentObjectFlat = new(
      root: ContainerItem(),
      rootName: LineItem(),
      property: PropertyItem(),
      propertyValue: PropertyValue(),
      showTrees: false
    );

    private static ProsePlainTextConfiguration Tree(
      string child, string lastChild, string continuation, string lastContinuation,
      bool alignWrappedPropertyValues = true
    ) => new(
      root: ContainerItem(),
      rootName: LineItem(),
      treeName: LineItem(),
      property: PropertyItem(),
      propertyValue: PropertyValue(alignWrappedPropertyValues),
      tree: new ItemAnchors(
        linePrefix: new[] {
          new LineEvaluationEntry(TextMatching.Last, LineMatching.First, 0, lastChild),
          new LineEvaluationEntry(TextMatching.None, LineMatching.First, 0, child),
          new LineEvaluationEntry(TextMatching.Last, LineMatching.None, 0, lastContinuation),
          new LineEvaluationEntry(TextMatching.None, LineMatching.None, 0, continuation)
        },
        lineBreak: NonTerminatingLines()
      )
    );

    private static ItemAnchors LineItem() => new(
      lineBreak: Lines(LineBreakMode.Item | LineBreakMode.Wrap | LineBreakMode.Hard)
    );

    private static ItemAnchors ContainerItem() => new(
      lineBreak: NonTerminatingLines()
    );

    private static ItemAnchors PropertyItem() => new(
      lineBreak: Lines(LineBreakMode.Item | LineBreakMode.Wrap | LineBreakMode.Hard)
    );

    private static ItemAnchors PropertyValue(bool align = true) => new(
      prefix: new[] { new AnchorEvaluationEntry(TextMatching.None, 0, ": ") },
      lineBreak: Lines(
        LineBreakMode.Wrap | LineBreakMode.Hard | (align ? LineBreakMode.Align : LineBreakMode.None)
      )
    );

    private static LineBreakEvaluationEntry[] Lines(LineBreakMode mode) => new[] {
      new LineBreakEvaluationEntry(TextMatching.None, LineMatching.None, 0, mode)
    };

    private static LineBreakEvaluationEntry[] NonTerminatingLines() => new[] {
      new LineBreakEvaluationEntry(TextMatching.None, LineMatching.Last, 0, LineBreakMode.None),
      new LineBreakEvaluationEntry(
        TextMatching.None, LineMatching.None, 0, LineBreakMode.Wrap | LineBreakMode.Hard
      )
    };
  }
}
