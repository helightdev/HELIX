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
  /// Controls which boundaries are emitted and whether wrapped continuations align.
  /// </summary>
  [Flags]
  public enum LineBreakMode : byte {
    None = 0,
    Align = 1 << 0,
    Item = 1 << 1,
    Wrap = 1 << 2,
    Hard = 1 << 3
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
      char headerFill = '-', int minimumColumnWidth = 3,
      string lineBreakReplacement = "¶", PTNodeFormat format = null
    ) {
      if (minimumColumnWidth < 0) throw new ArgumentOutOfRangeException(nameof(minimumColumnWidth));
      LeftBorder = leftBorder ?? string.Empty;
      ColumnSeparator = columnSeparator ?? string.Empty;
      RightBorder = rightBorder ?? string.Empty;
      HeaderFill = headerFill;
      MinimumColumnWidth = minimumColumnWidth;
      LineBreakReplacement = lineBreakReplacement ?? string.Empty;
      Format = format ?? PTRuleFactory.Container();
    }

    public string LeftBorder { get; }
    public string ColumnSeparator { get; }
    public string RightBorder { get; }
    public char HeaderFill { get; }
    public int MinimumColumnWidth { get; }
    /// <summary>Text substituted for line breaks inside individual cells.</summary>
    public string LineBreakReplacement { get; }
    public PTNodeFormat Format { get; }
  }

  /// <summary>Presentation and optional line filling for preformatted source blocks.</summary>
  public sealed class PTCodeBlockFormat {
    public PTCodeBlockFormat(
      PTNodeFormat format = null,
      string prefix = "```",
      string prefixSuffix = "",
      string suffix = "```",
      string prefixFill = null,
      int prefixFillRepeater = -1,
      string suffixFill = null,
      int suffixFillRepeater = -1,
      bool showLanguage = true
    ) {
      if (prefixFillRepeater < -1) throw new ArgumentOutOfRangeException(nameof(prefixFillRepeater));
      if (suffixFillRepeater < -1) throw new ArgumentOutOfRangeException(nameof(suffixFillRepeater));
      Format = format ?? PTRuleFactory.Container();
      Prefix = prefix ?? string.Empty;
      PrefixSuffix = prefixSuffix ?? string.Empty;
      Suffix = suffix ?? string.Empty;
      PrefixFill = prefixFill;
      PrefixFillRepeater = prefixFillRepeater;
      SuffixFill = suffixFill;
      SuffixFillRepeater = suffixFillRepeater;
      ShowLanguage = showLanguage;
    }

    public PTNodeFormat Format { get; }
    public string Prefix { get; }
    /// <summary>Text placed after the optional language and before prefix fill.</summary>
    public string PrefixSuffix { get; }
    public string Suffix { get; }
    /// <summary>Optional text appended to and expanded across the prefix line.</summary>
    public string PrefixFill { get; }
    /// <summary>Index of the character in <see cref="PrefixFill"/> repeated to fill the line.</summary>
    public int PrefixFillRepeater { get; }
    /// <summary>Optional text appended to and expanded across the suffix line.</summary>
    public string SuffixFill { get; }
    /// <summary>Index of the character in <see cref="SuffixFill"/> repeated to fill the line.</summary>
    public int SuffixFillRepeater { get; }
    public bool ShowLanguage { get; }
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
      PTCodeBlockFormat codeBlock = null,
      string requiredLineBreak = "\n",
      bool showTextFeatures = true,
      string propertyChildContinuation = null,
      bool blankLineAfterSectionHeader = true
    ) {
      Root = root ?? _noAnchors;
      RootName = rootName ?? _noAnchors;
      TreeName = treeName ?? _noAnchors;
      Property = property ?? _noAnchors;
      PropertyValue = propertyValue ?? _noAnchors;
      Tree = tree ?? _noAnchors;
      PropertyChildContinuation = propertyChildContinuation;
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
      CodeBlock = codeBlock ?? new PTCodeBlockFormat();
      Table = table ?? new PTTableFormat();
      UnorderedListMarker = unorderedListMarker ?? string.Empty;
      OrderedListMarkerSuffix = orderedListMarkerSuffix ?? string.Empty;
      LinkTargetPrefix = linkTargetPrefix ?? string.Empty;
      LinkTargetSuffix = linkTargetSuffix ?? string.Empty;
      RequiredLineBreak = requiredLineBreak ?? string.Empty;
      ShowTrees = showTrees;
      ShowNames = showNames;
      ShowProperties = showProperties;
      ShowTextFeatures = showTextFeatures;
      BlankLineAfterSectionHeader = blankLineAfterSectionHeader;
    }

    public PTNodeFormat Root { get; }
    public PTNodeFormat RootName { get; }
    public PTNodeFormat TreeName { get; }
    public PTNodeFormat Property { get; }
    public PTNodeFormat PropertyValue { get; }
    public PTNodeFormat Tree { get; }
    /// <summary>
    /// Content rendered on the property spacer line when the owning node continues into children.
    /// Null disables property spacer lines; an empty value emits an unadorned spacer.
    /// </summary>
    public string PropertyChildContinuation { get; }
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
    public PTCodeBlockFormat CodeBlock { get; }
    public PTTableFormat Table { get; }
    public string UnorderedListMarker { get; }
    public string OrderedListMarkerSuffix { get; }
    public string LinkTargetPrefix { get; }
    public string LinkTargetSuffix { get; }
    /// <summary>External presentation used for semantic required line breaks.</summary>
    public string RequiredLineBreak { get; }
    public bool ShowTrees { get; }
    public bool ShowNames { get; }
    public bool ShowProperties { get; }
    /// <summary>Whether sections, paragraphs, spans, lists, tables, and code blocks are accepted.</summary>
    public bool ShowTextFeatures { get; }
    /// <summary>Whether a padded first block retains its leading blank line after a section header.</summary>
    public bool BlankLineAfterSectionHeader { get; }

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
      string before = "(", string separator = ", ", string after = ")"
    ) => new(
      prefix: new[] {
        Anchor(before, TextMatching.First),
        Anchor(separator)
      },
      suffix: new[] { Anchor(after, TextMatching.Last) },
      lines: LineBreaks(LineBreakMode.Wrap | LineBreakMode.Hard)
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
      string continuationPrefix = ""
    ) => Block(
      prefix, suffix, firstLinePrefix, continuationPrefix,
      LineBreakMode.Item | LineBreakMode.Wrap | LineBreakMode.Hard
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

    /// <summary>A block separated from adjacent content by one fully blank line.</summary>
    public static PTNodeFormat PaddedBlock() => Block(
      prefix: "\n", suffix: "\n",
      lineBreaks: LineBreakMode.Wrap | LineBreakMode.Hard
    );

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
      "├─ ", "└─ ", "│  ", "   ", propertyChildContinuation: "│"
    );

    /// <summary>A cleaned-up diagnostic tree with a clearly delimited root heading.</summary>
    public static readonly ProsePlainTextConfiguration Error = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Line(prefix: "══ ", suffix: " ══", suffixRepeater: 1),
      treeName: PTRuleFactory.Line(),
      property: PTRuleFactory.Property(firstLinePrefix: "! ", continuationPrefix: "  "),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: PTRuleFactory.Tree("├─ ", "└─ ", "│  ", "   "),
      section: PTRuleFactory.Section(),
      sectionHeader: PTRuleFactory.Line(prefix: "── ", suffix: " ──", suffixRepeater: 1),
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
      codeBlock: new PTCodeBlockFormat(
        prefix: "╭─ code: ", prefixSuffix: " ─", suffix: "╰─",
        prefixFill: "─╮", prefixFillRepeater: 0,
        suffixFill: "─╯", suffixFillRepeater: 0
      )
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
      codeBlock: new PTCodeBlockFormat(prefix: "", suffix: "", showLanguage: false),
      showTextFeatures: false
    );

    /// <summary>An ASCII tree with sparse properties and every supported general-text feature.</summary>
    public static readonly ProsePlainTextConfiguration Plain = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Line(),
      treeName: PTRuleFactory.Line(),
      property: PTRuleFactory.Property(continuationPrefix: " "),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: PTRuleFactory.Tree("|- ", "\\- ", "|  ", "   "),
      propertyChildContinuation: "|",
      blankLineAfterSectionHeader: false,
      section: PTRuleFactory.Section(),
      sectionHeader: PTRuleFactory.Line(suffix: ":"),
      paragraph: PTRuleFactory.PaddedBlock(),
      list: PTRuleFactory.PaddedBlock(),
      listItem: PTRuleFactory.ListItem(),
      table: new PTTableFormat(format: PTRuleFactory.PaddedBlock()),
      emphasizedText: PTRuleFactory.Markup("/", "/"),
      strongText: PTRuleFactory.Markup("*", "*"),
      codeText: PTRuleFactory.Markup("'", "'"),
      quoteText: PTRuleFactory.Markup("“", "”"),
      errorText: PTRuleFactory.Markup("Error: ", ""),
      codeBlock: new PTCodeBlockFormat(
        format: PTRuleFactory.PaddedBlock(), prefix: "Code: ", suffix: ""
      )
    );

    /// <summary>
    /// Full Unity rich-text layout with sparse properties and an ASCII-only child tree.
    /// Rich-text tags are supplied by <see cref="ProseUnityRichTextWriter"/>.
    /// </summary>
    public static readonly ProsePlainTextConfiguration UnityRichText = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Line(),
      treeName: PTRuleFactory.Line(),
      property: PTRuleFactory.Property(continuationPrefix: " "),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: PTRuleFactory.Tree("|- ", "\\- ", "|  ", "   "),
      propertyChildContinuation: "|",
      blankLineAfterSectionHeader: false,
      section: PTRuleFactory.Section(),
      sectionHeader: PTRuleFactory.Line(suffix: ":"),
      paragraph: PTRuleFactory.PaddedBlock(),
      list: PTRuleFactory.PaddedBlock(),
      listItem: PTRuleFactory.ListItem(),
      table: new PTTableFormat(format: PTRuleFactory.PaddedBlock()),
      linkTargetPrefix: "",
      linkTargetSuffix: "",
      codeBlock: new PTCodeBlockFormat(
        format: PTRuleFactory.PaddedBlock(), prefix: "", suffix: "", showLanguage: false
      )
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
      string child, string lastChild, string continuation, string lastContinuation,
      string propertyChildContinuation = null
    ) => new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Line(),
      treeName: PTRuleFactory.Line(),
      property: PTRuleFactory.Property(continuationPrefix: " "),
      propertyValue: PTRuleFactory.PropertyValue(),
      tree: PTRuleFactory.Tree(child, lastChild, continuation, lastContinuation),
      propertyChildContinuation: propertyChildContinuation,
      section: PTRuleFactory.Section(),
      sectionHeader: PTRuleFactory.Line(suffix: ":"),
      paragraph: PTRuleFactory.Paragraph(),
      list: PTRuleFactory.Container(),
      listItem: PTRuleFactory.ListItem(),
      linkTargetPrefix: "",
      linkTargetSuffix: "",
      codeBlock: new PTCodeBlockFormat(prefix: "", suffix: "", showLanguage: false),
      showTextFeatures: false
    );
  }
}
