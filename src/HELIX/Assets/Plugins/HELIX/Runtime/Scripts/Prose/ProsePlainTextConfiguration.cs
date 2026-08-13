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

  public readonly struct PTStringRule {
    public PTStringRule(TextMatching matching, int priority, string value) {
      Matching = matching;
      Priority = priority;
      String = value ?? string.Empty;
    }

    public TextMatching Matching { get; }
    public int Priority { get; }
    public string String { get; }
  }

  public readonly struct PTIndentRule {
    public PTIndentRule(TextMatching matching, LineMatching lineMatching, int priority, string value) {
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

  /// <summary>
  /// Controls which boundaries are emitted, whether wrapped continuations align, and whether an item
  /// requires a line break immediately before or after it.
  /// </summary>
  [Flags]
  public enum LineBreakMode : byte {
    None = 0,
    Align = 1 << 0,
    Item = 1 << 1,
    Wrap = 1 << 2,
    Hard = 1 << 3,
    Pre = 1 << 4,
    Post = 1 << 5
  }

  public readonly struct PTLineRule {
    public PTLineRule(
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
  public sealed class PTNodeFormat {
    private static readonly PTStringRule[] _noStrings = Array.Empty<PTStringRule>();
    private static readonly PTIndentRule[] _noIndent = Array.Empty<PTIndentRule>();
    private static readonly PTLineRule[] _noLineBreaks = Array.Empty<PTLineRule>();

    public PTNodeFormat(
      IReadOnlyList<PTStringRule> prefix = null,
      IReadOnlyList<PTStringRule> suffix = null,
      IReadOnlyList<PTStringRule> replacement = null,
      IReadOnlyList<PTIndentRule> indent = null,
      IReadOnlyList<PTLineRule> lines = null,
      int suffixRepeater = -1
    ) {
      if (suffixRepeater < -1) throw new ArgumentOutOfRangeException(nameof(suffixRepeater));
      Prefix = prefix ?? _noStrings;
      Suffix = suffix ?? _noStrings;
      Replacement = replacement ?? _noStrings;
      LinePrefix = indent ?? _noIndent;
      LineBreak = lines ?? _noLineBreaks;
      SuffixRepeater = suffixRepeater;
    }

    public IReadOnlyList<PTStringRule> Prefix { get; }
    public IReadOnlyList<PTStringRule> Suffix { get; }
    public IReadOnlyList<PTStringRule> Replacement { get; }
    public IReadOnlyList<PTIndentRule> LinePrefix { get; }
    public IReadOnlyList<PTLineRule> LineBreak { get; }
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
      StringBuilder builder, IReadOnlyList<PTStringRule> entries, TextMatching matching
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
      StringBuilder builder, IReadOnlyList<PTIndentRule> entries,
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
    private static readonly PTNodeFormat _noAnchors = new();

    public ProsePlainTextConfiguration(
      PTNodeFormat root = null,
      PTNodeFormat rootName = null,
      PTNodeFormat treeName = null,
      PTNodeFormat property = null,
      PTNodeFormat propertyValue = null,
      PTNodeFormat tree = null,
      bool showTrees = true,
      bool showNames = true,
      bool showProperties = true
    ) {
      Root = root ?? _noAnchors;
      RootName = rootName ?? _noAnchors;
      TreeName = treeName ?? _noAnchors;
      Property = property ?? _noAnchors;
      PropertyValue = propertyValue ?? _noAnchors;
      Tree = tree ?? _noAnchors;
      ShowTrees = showTrees;
      ShowNames = showNames;
      ShowProperties = showProperties;
    }

    public PTNodeFormat Root { get; }
    public PTNodeFormat RootName { get; }
    public PTNodeFormat TreeName { get; }
    public PTNodeFormat Property { get; }
    public PTNodeFormat PropertyValue { get; }
    public PTNodeFormat Tree { get; }
    public bool ShowTrees { get; }
    public bool ShowNames { get; }
    public bool ShowProperties { get; }

  }

  /// <summary>Reusable factories for the common building blocks of plain-text configurations.</summary>
  public static class PTRuleFactory {
    public static PTStringRule Anchor(
      string value, TextMatching matching = TextMatching.None, int priority = 0
    ) => new(matching, priority, value);

    public static PTIndentRule LinePrefix(
      string value, LineMatching lineMatching = LineMatching.None,
      TextMatching matching = TextMatching.None, int priority = 0
    ) => new(matching, lineMatching, priority, value);

    public static PTLineRule LineBreak(
      LineBreakMode mode, LineMatching lineMatching = LineMatching.None,
      TextMatching matching = TextMatching.None, int priority = 0
    ) => new(matching, lineMatching, priority, mode);

    public static IReadOnlyList<PTLineRule> LineBreaks(
      LineBreakMode mode, bool suppressLast = false
    ) => suppressLast
      ? new[] {
        LineBreak(LineBreakMode.None, LineMatching.Last),
        LineBreak(mode)
      }
      : new[] { LineBreak(mode) };

    public static PTNodeFormat Item(
      string prefix = "", string suffix = "", string firstLinePrefix = "",
      string continuationPrefix = "", LineBreakMode lineBreaks = LineBreakMode.None,
      bool suppressLastLineBreak = false, int suffixRepeater = -1
    ) => new(
      prefix: string.IsNullOrEmpty(prefix) ? null : new[] { Anchor(prefix) },
      suffix: string.IsNullOrEmpty(suffix) ? null : new[] { Anchor(suffix) },
      indent: string.IsNullOrEmpty(firstLinePrefix) && string.IsNullOrEmpty(continuationPrefix)
        ? null
        : new[] {
          LinePrefix(firstLinePrefix, LineMatching.First),
          LinePrefix(continuationPrefix)
        },
      lines: lineBreaks == LineBreakMode.None
        ? null
        : LineBreaks(lineBreaks, suppressLastLineBreak),
      suffixRepeater: suffixRepeater
    );

    public static PTNodeFormat InlineProperties(
      string before = "(", string separator = ", ", string after = ")"
    ) => new(
      prefix: new[] {
        Anchor(before, TextMatching.First),
        Anchor(separator)
      },
      suffix: new[] { Anchor(after, TextMatching.Last) },
      lines: LineBreaks(LineBreakMode.Wrap | LineBreakMode.Hard)
    );

    public static PTNodeFormat LineItem(
      string prefix = "", string suffix = "", string firstLinePrefix = "",
      string continuationPrefix = "", int suffixRepeater = -1
    ) => Item(
      prefix, suffix, firstLinePrefix, continuationPrefix,
      LineBreakMode.Item | LineBreakMode.Wrap | LineBreakMode.Hard,
      suffixRepeater: suffixRepeater
    );

    public static PTNodeFormat Container(
      string prefix = "", string suffix = "", string firstLinePrefix = "",
      string continuationPrefix = "", int suffixRepeater = -1
    ) => Item(
      prefix, suffix, firstLinePrefix, continuationPrefix,
      LineBreakMode.Wrap | LineBreakMode.Hard, true, suffixRepeater
    );

    public static PTNodeFormat Property(
      string prefix = "", string suffix = "", string firstLinePrefix = "",
      string continuationPrefix = ""
    ) => LineItem(prefix, suffix, firstLinePrefix, continuationPrefix);

    public static PTNodeFormat PropertyValue(
      string separator = ": ", string continuationPrefix = "", bool align = false
    ) => Item(
      prefix: separator,
      continuationPrefix: continuationPrefix,
      lineBreaks: LineBreakMode.Wrap | LineBreakMode.Hard |
                  (align ? LineBreakMode.Align : LineBreakMode.None)
    );

    public static PTNodeFormat Tree(
      string first, string last, string continuation, string lastContinuation
    ) => new(
      indent: new[] {
        LinePrefix(last, LineMatching.First, TextMatching.Last),
        LinePrefix(first, LineMatching.First),
        LinePrefix(lastContinuation, matching: TextMatching.Last),
        LinePrefix(continuation)
      },
      lines: LineBreaks(LineBreakMode.Wrap | LineBreakMode.Hard, true)
    );
  }

  public static class ProsePlainTextConfigurations {
    private const LineBreakMode AllBreaks = LineBreakMode.Item | LineBreakMode.Wrap | LineBreakMode.Hard;

    public static readonly ProsePlainTextConfiguration Sparse = CreateTree(
      "├─", "└─", "│ ", "  ", propertyPrefix: "│ "
    );
    public static readonly ProsePlainTextConfiguration Dashed = CreateTree(
      "╎╌", "└╌", "╎ ", "  ", propertyPrefix: "│ "
    );
    public static readonly ProsePlainTextConfiguration Dense = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Item(),
      treeName: PTRuleFactory.Item(),
      property: PTRuleFactory.InlineProperties(),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: PTRuleFactory.Tree("├", "└", "│", " ")
    );
    public static readonly ProsePlainTextConfiguration Transition = Boxed(
      "╞═╦══ ", "╘═╦══ ", " ║ ", " ╚═══════════", " ═══"
    );
    public static readonly ProsePlainTextConfiguration Error = new(
      root: PTRuleFactory.Container(suffix: "\n═════", suffixRepeater: 1),
      rootName: PTRuleFactory.LineItem(prefix: "══╡ ", suffix: " ╞══", suffixRepeater: 3),
      treeName: PTRuleFactory.LineItem(),
      property: PTRuleFactory.Property(firstLinePrefix: "╎ ", continuationPrefix: "╎ "),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: PTRuleFactory.Tree("╎╌", "└╌", "╎ ", "  ")
    );
    public static readonly ProsePlainTextConfiguration Whitespace = CreateTree("  ", "  ", "  ", "  ");
    public static readonly ProsePlainTextConfiguration Flat = CreateTree("", "", "", "");
    public static readonly ProsePlainTextConfiguration SingleLine = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Item(),
      property: PTRuleFactory.InlineProperties(),
      propertyValue: PTRuleFactory.PropertyValue(),
      showTrees: false
    );
    public static readonly ProsePlainTextConfiguration ErrorProperty = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.LineItem(suffix: ":"),
      property: PTRuleFactory.InlineProperties("  ("),
      propertyValue: PTRuleFactory.PropertyValue(),
      showTrees: false
    );
    public static readonly ProsePlainTextConfiguration Shallow = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.LineItem(suffix: ":"),
      property: PTRuleFactory.Property(firstLinePrefix: "  ", continuationPrefix: "  "),
      propertyValue: PTRuleFactory.PropertyValue(),
      showTrees: false
    );

    public static readonly ProsePlainTextConfiguration Unicode = CreateTree("├─ ", "└─ ", "│  ", "   ");
    public static readonly ProsePlainTextConfiguration Ascii = CreateTree("+- ", "`- ", "|  ", "   ", false);

    public static readonly ProsePlainTextConfiguration CurrentObjectFlat = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.LineItem(),
      property: PTRuleFactory.Property(),
      propertyValue: PTRuleFactory.PropertyValue(),
      showTrees: false
    );

    public static ProsePlainTextConfiguration CreateTree(
      string child, string lastChild, string continuation, string lastContinuation,
      bool alignWrappedPropertyValues = false, string propertyPrefix = "", string propertyContinuation = null
    ) => new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.LineItem(),
      treeName: PTRuleFactory.LineItem(),
      property: PTRuleFactory.Property(
        firstLinePrefix: propertyPrefix, continuationPrefix: propertyContinuation ?? propertyPrefix + " "
      ),
      propertyValue: PTRuleFactory.PropertyValue(align: alignWrappedPropertyValues),
      tree: PTRuleFactory.Tree(child, lastChild, continuation, lastContinuation)
    );

    private static ProsePlainTextConfiguration Boxed(
      string child, string last, string continuation, string footer, string nameSuffix
    ) => new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.LineItem(suffix: nameSuffix),
      treeName: PTRuleFactory.LineItem(suffix: nameSuffix),
      property: PTRuleFactory.Property(firstLinePrefix: "  ", continuationPrefix: "  "),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: new PTNodeFormat(
        suffix: new[] { PTRuleFactory.Anchor("\n" + footer) },
        indent: new[] {
          PTRuleFactory.LinePrefix(last, LineMatching.First, TextMatching.Last),
          PTRuleFactory.LinePrefix(child, LineMatching.First),
          PTRuleFactory.LinePrefix("   ", matching: TextMatching.Last),
          PTRuleFactory.LinePrefix(continuation)
        },
        lines: PTRuleFactory.LineBreaks(
          LineBreakMode.Wrap | LineBreakMode.Hard, true
        )
      )
    );
  }
}
