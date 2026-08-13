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

  /// <summary>Characters and sizing rules used when a plain-text table is measured and rendered.</summary>
  public sealed class PTTableFormat {
    public PTTableFormat(
      string leftBorder = "| ", string columnSeparator = " | ", string rightBorder = " |",
      char headerFill = '-', int minimumColumnWidth = 3
    ) {
      if (minimumColumnWidth < 0) throw new ArgumentOutOfRangeException(nameof(minimumColumnWidth));
      LeftBorder = leftBorder ?? string.Empty;
      ColumnSeparator = columnSeparator ?? string.Empty;
      RightBorder = rightBorder ?? string.Empty;
      HeaderFill = headerFill;
      MinimumColumnWidth = minimumColumnWidth;
    }

    public string LeftBorder { get; }
    public string ColumnSeparator { get; }
    public string RightBorder { get; }
    public char HeaderFill { get; }
    public int MinimumColumnWidth { get; }
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
      bool showProperties = true,
      PTNodeFormat section = null,
      PTNodeFormat sectionHeader = null,
      PTNodeFormat paragraph = null,
      PTNodeFormat list = null,
      PTNodeFormat listItem = null,
      PTNodeFormat emphasizedText = null,
      PTNodeFormat strongText = null,
      PTNodeFormat codeText = null,
      PTNodeFormat quoteText = null,
      PTNodeFormat errorText = null,
      PTTableFormat table = null,
      string unorderedListMarker = "- ",
      string orderedListMarkerSuffix = ". ",
      PTNodeFormat linkText = null,
      string linkTargetPrefix = " (",
      string linkTargetSuffix = ")",
      PTNodeFormat codeBlock = null,
      string codeBlockPrefix = "```",
      string codeBlockSuffix = "```",
      string requiredLineBreak = "\n",
      bool showTextFeatures = true
    ) {
      Root = root ?? _noAnchors;
      RootName = rootName ?? _noAnchors;
      TreeName = treeName ?? _noAnchors;
      Property = property ?? _noAnchors;
      PropertyValue = propertyValue ?? _noAnchors;
      Tree = tree ?? _noAnchors;
      Section = section ?? _noAnchors;
      SectionHeader = sectionHeader ?? _noAnchors;
      Paragraph = paragraph ?? _noAnchors;
      List = list ?? _noAnchors;
      ListItem = listItem ?? _noAnchors;
      EmphasizedText = emphasizedText ?? _noAnchors;
      StrongText = strongText ?? _noAnchors;
      CodeText = codeText ?? _noAnchors;
      QuoteText = quoteText ?? _noAnchors;
      ErrorText = errorText ?? _noAnchors;
      LinkText = linkText ?? _noAnchors;
      CodeBlock = codeBlock ?? PTRuleFactory.Container();
      Table = table ?? new PTTableFormat();
      UnorderedListMarker = unorderedListMarker ?? string.Empty;
      OrderedListMarkerSuffix = orderedListMarkerSuffix ?? string.Empty;
      LinkTargetPrefix = linkTargetPrefix ?? string.Empty;
      LinkTargetSuffix = linkTargetSuffix ?? string.Empty;
      CodeBlockPrefix = codeBlockPrefix ?? string.Empty;
      CodeBlockSuffix = codeBlockSuffix ?? string.Empty;
      RequiredLineBreak = requiredLineBreak ?? string.Empty;
      ShowTrees = showTrees;
      ShowNames = showNames;
      ShowProperties = showProperties;
      ShowTextFeatures = showTextFeatures;
    }

    public PTNodeFormat Root { get; }
    public PTNodeFormat RootName { get; }
    public PTNodeFormat TreeName { get; }
    public PTNodeFormat Property { get; }
    public PTNodeFormat PropertyValue { get; }
    public PTNodeFormat Tree { get; }
    public PTNodeFormat Section { get; }
    public PTNodeFormat SectionHeader { get; }
    public PTNodeFormat Paragraph { get; }
    public PTNodeFormat List { get; }
    public PTNodeFormat ListItem { get; }
    public PTNodeFormat EmphasizedText { get; }
    public PTNodeFormat StrongText { get; }
    public PTNodeFormat CodeText { get; }
    public PTNodeFormat QuoteText { get; }
    public PTNodeFormat ErrorText { get; }
    public PTNodeFormat LinkText { get; }
    public PTNodeFormat CodeBlock { get; }
    public PTTableFormat Table { get; }
    public string UnorderedListMarker { get; }
    public string OrderedListMarkerSuffix { get; }
    public string LinkTargetPrefix { get; }
    public string LinkTargetSuffix { get; }
    public string CodeBlockPrefix { get; }
    public string CodeBlockSuffix { get; }
    /// <summary>External presentation used for semantic required line breaks.</summary>
    public string RequiredLineBreak { get; }
    public bool ShowTrees { get; }
    public bool ShowNames { get; }
    public bool ShowProperties { get; }
    /// <summary>Whether sections, paragraphs, spans, lists, tables, and code blocks are accepted.</summary>
    public bool ShowTextFeatures { get; }

  }

  /// <summary>Composable factories for the common parts of a plain-text layout.</summary>
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
      LineBreakMode mode, bool omitFinalItemBreak = false
    ) => omitFinalItemBreak
      ? new[] {
        LineBreak(LineBreakMode.None, LineMatching.Last),
        LineBreak(mode)
      }
      : new[] { LineBreak(mode) };

    /// <summary>
    /// Creates a block whose boundaries, indentation, and line behavior can be combined independently.
    /// </summary>
    public static PTNodeFormat Block(
      string prefix = "", string suffix = "", string firstLineIndent = "",
      string continuationIndent = "", LineBreakMode lineBreaks = LineBreakMode.None,
      bool omitFinalItemBreak = false, int suffixRepeater = -1
    ) => new(
      prefix: string.IsNullOrEmpty(prefix) ? null : new[] { Anchor(prefix) },
      suffix: string.IsNullOrEmpty(suffix) ? null : new[] { Anchor(suffix) },
      indent: string.IsNullOrEmpty(firstLineIndent) && string.IsNullOrEmpty(continuationIndent)
        ? null
        : new[] {
          LinePrefix(firstLineIndent, LineMatching.First),
          LinePrefix(continuationIndent)
        },
      lines: lineBreaks == LineBreakMode.None
        ? null
        : LineBreaks(lineBreaks, omitFinalItemBreak),
      suffixRepeater: suffixRepeater
    );

    public static PTNodeFormat InlineProperties(
      string before = "(", string separator = ", ", string after = ")",
      bool trailingNewLine = false
    ) => new(
      prefix: new[] {
        Anchor(before, TextMatching.First),
        Anchor(separator)
      },
      suffix: new[] { Anchor(after, TextMatching.Last) },
      lines: LineBreaks(
        LineBreakMode.Wrap | LineBreakMode.Hard |
        (trailingNewLine ? LineBreakMode.Post : LineBreakMode.None)
      )
    );

    public static PTNodeFormat Line(
      string prefix = "", string suffix = "", string firstLinePrefix = "",
      string continuationPrefix = "", int suffixRepeater = -1
    ) => Block(
      prefix, suffix, firstLinePrefix, continuationPrefix,
      LineBreakMode.Item | LineBreakMode.Wrap | LineBreakMode.Hard,
      suffixRepeater: suffixRepeater
    );

    public static PTNodeFormat Container(
      string prefix = "", string suffix = "", string firstLinePrefix = "",
      string continuationPrefix = "", int suffixRepeater = -1
    ) => Block(
      prefix, suffix, firstLinePrefix, continuationPrefix,
      LineBreakMode.Wrap | LineBreakMode.Hard, true, suffixRepeater
    );

    public static PTNodeFormat Property(
      string prefix = "", string suffix = "", string firstLinePrefix = "",
      string continuationPrefix = "", bool trailingNewLine = false
    ) => Block(
      prefix, suffix, firstLinePrefix, continuationPrefix,
      LineBreakMode.Item | LineBreakMode.Wrap | LineBreakMode.Hard |
      (trailingNewLine ? LineBreakMode.Post : LineBreakMode.None)
    );

    public static PTNodeFormat PropertyValue(
      string separator = ": ", string continuationPrefix = "", bool align = false
    ) => Block(
      prefix: separator,
      continuationIndent: continuationPrefix,
      lineBreaks: LineBreakMode.Wrap | LineBreakMode.Hard |
                  (align ? LineBreakMode.Align : LineBreakMode.None)
    );

    public static PTNodeFormat Section() => Container();

    public static PTNodeFormat Paragraph() => Container();

    public static PTNodeFormat ListItem(string continuationPrefix = "  ") => Block(
      continuationIndent: continuationPrefix,
      lineBreaks: LineBreakMode.Wrap | LineBreakMode.Hard
    );

    public static PTNodeFormat Markup(string prefix, string suffix) =>
      Block(prefix: prefix, suffix: suffix, lineBreaks: LineBreakMode.Wrap | LineBreakMode.Hard);

    public static PTNodeFormat IndentedMarkup(string firstLinePrefix, string continuationPrefix) =>
      Block(
        firstLineIndent: firstLinePrefix,
        continuationIndent: continuationPrefix,
        lineBreaks: LineBreakMode.Wrap | LineBreakMode.Hard
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
    /// <summary>A simple tree with a small continuation indent for wrapped properties.</summary>
    public static readonly ProsePlainTextConfiguration Sparse = Tree(
      "├─ ", "└─ ", "│  ", "   "
    );

    /// <summary>A cleaned-up diagnostic tree with a clearly delimited root heading.</summary>
    public static readonly ProsePlainTextConfiguration Error = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Line(prefix: "══ ", suffix: " ══"),
      treeName: PTRuleFactory.Line(),
      property: PTRuleFactory.Property(firstLinePrefix: "! ", continuationPrefix: "  "),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: PTRuleFactory.Tree("├─ ", "└─ ", "│  ", "   "),
      section: PTRuleFactory.Section(),
      sectionHeader: PTRuleFactory.Line(prefix: "── ", suffix: " ──"),
      paragraph: PTRuleFactory.IndentedMarkup("! ", "! "),
      list: PTRuleFactory.Container(),
      listItem: PTRuleFactory.ListItem(),
      emphasizedText: PTRuleFactory.Markup("‹", "›"),
      strongText: PTRuleFactory.Markup("«", "»"),
      codeText: PTRuleFactory.Markup("⟦", "⟧"),
      quoteText: PTRuleFactory.IndentedMarkup("│ ", "│ "),
      errorText: PTRuleFactory.IndentedMarkup("‼ ", "  "),
      linkText: PTRuleFactory.Markup("<", ">"),
      linkTargetPrefix: " [",
      linkTargetSuffix: "]",
      codeBlockPrefix: "╭─ code: ",
      codeBlockSuffix: "╰─"
    );

    /// <summary>Shows the current object on one line, with its properties in parentheses.</summary>
    public static readonly ProsePlainTextConfiguration Shallow = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Block(),
      property: PTRuleFactory.InlineProperties(),
      propertyValue: PTRuleFactory.PropertyValue(),
      showTrees: false,
      section: PTRuleFactory.Section(),
      sectionHeader: PTRuleFactory.Line(suffix: ":"),
      paragraph: PTRuleFactory.Paragraph(),
      list: PTRuleFactory.Container(),
      listItem: PTRuleFactory.ListItem(),
      linkTargetPrefix: "",
      linkTargetSuffix: "",
      codeBlockPrefix: "",
      codeBlockSuffix: "",
      showTextFeatures: false
    );

    /// <summary>A shallow property summary followed by every supported general-text feature.</summary>
    public static readonly ProsePlainTextConfiguration Plain = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Block(),
      property: PTRuleFactory.InlineProperties(trailingNewLine: true),
      propertyValue: PTRuleFactory.PropertyValue(),
      showTrees: false,
      section: PTRuleFactory.Section(),
      sectionHeader: PTRuleFactory.Line(suffix: ":"),
      paragraph: PTRuleFactory.Paragraph(),
      list: PTRuleFactory.Container(),
      listItem: PTRuleFactory.ListItem(),
      emphasizedText: PTRuleFactory.Markup("/", "/"),
      strongText: PTRuleFactory.Markup("*", "*"),
      codeText: PTRuleFactory.Markup("'", "'"),
      quoteText: PTRuleFactory.Markup("“", "”"),
      errorText: PTRuleFactory.Markup("Error: ", ""),
      codeBlockPrefix: "Code: ",
      codeBlockSuffix: ""
    );

    /// <summary>A Sparse layout that uses indentation only; no tree glyphs are emitted.</summary>
    public static readonly ProsePlainTextConfiguration Whitespace = Tree("  ", "  ", "  ", "  ");

    /// <summary>
    /// A complete Markdown projection with headings, lists, tables, links, fenced code, and inline markup.
    /// </summary>
    public static readonly ProsePlainTextConfiguration Markdown = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Line(prefix: "# "),
      treeName: PTRuleFactory.Line(prefix: "- **", suffix: "**"),
      property: PTRuleFactory.Property(firstLinePrefix: "- ", continuationPrefix: "  "),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: PTRuleFactory.Tree("  ", "  ", "  ", "  "),
      section: PTRuleFactory.Section(),
      sectionHeader: PTRuleFactory.Line(prefix: "## "),
      paragraph: PTRuleFactory.Block(
        prefix: "\n", suffix: "\n",
        lineBreaks: LineBreakMode.Wrap | LineBreakMode.Hard
      ),
      list: PTRuleFactory.Block(
        prefix: "\n", suffix: "\n\n",
        lineBreaks: LineBreakMode.Wrap | LineBreakMode.Hard
      ),
      listItem: PTRuleFactory.ListItem(),
      emphasizedText: PTRuleFactory.Markup("*", "*"),
      strongText: PTRuleFactory.Markup("**", "**"),
      codeText: PTRuleFactory.Markup("`", "`"),
      quoteText: PTRuleFactory.IndentedMarkup("> ", "> "),
      errorText: PTRuleFactory.Markup("**Error: ", "**"),
      linkText: PTRuleFactory.Markup("[", "]"),
      linkTargetPrefix: "(",
      linkTargetSuffix: ")",
      requiredLineBreak: "  \n"
    );

    private static ProsePlainTextConfiguration Tree(
      string child, string lastChild, string continuation, string lastContinuation
    ) => new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Line(),
      treeName: PTRuleFactory.Line(),
      property: PTRuleFactory.Property(continuationPrefix: " "),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: PTRuleFactory.Tree(child, lastChild, continuation, lastContinuation),
      section: PTRuleFactory.Section(),
      sectionHeader: PTRuleFactory.Line(suffix: ":"),
      paragraph: PTRuleFactory.Paragraph(),
      list: PTRuleFactory.Container(),
      listItem: PTRuleFactory.ListItem(),
      linkTargetPrefix: "",
      linkTargetSuffix: "",
      codeBlockPrefix: "",
      codeBlockSuffix: "",
      showTextFeatures: false
    );
  }
}
