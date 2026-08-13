namespace HELIX.Prose {
  /// <summary>A general inline span used as the target of markup modifiers.</summary>
  public sealed class ProseSpan : IProseScope { }

  /// <summary>A block containing an optional header and paragraph-oriented content.</summary>
  public sealed class ProseSection : IProseScope { }
  public sealed class ProseSectionHeader : IProseScope { }
  public sealed class ProseParagraph : IProseScope { }

  /// <summary>A preformatted block of source text with an optional language hint.</summary>
  public sealed class ProseCodeBlock : IProseScope {
    public ProseCodeBlock(string language = null) => Language = language ?? string.Empty;
    public string Language { get; }
  }

  public enum ProseListKind : byte { Unordered, Ordered }

  /// <summary>A semantic ordered or unordered list.</summary>
  public sealed class ProseList : IProseScope {
    public ProseList(ProseListKind kind, int start = 1) {
      if (start < 0) throw new System.ArgumentOutOfRangeException(nameof(start));
      Kind = kind;
      Start = start;
    }

    public ProseListKind Kind { get; }
    public int Start { get; }
  }

  public sealed class ProseListItem : IProseScope { }
  public sealed class ProseTable : IProseScope { }

  /// <summary>A table row whose header state is semantic rather than inferred from its position.</summary>
  public sealed class ProseTableRow : IProseScope {
    public ProseTableRow(bool isHeader) => IsHeader = isHeader;
    public bool IsHeader { get; }
  }

  public sealed class ProseTableCell : IProseScope { }
  public sealed class ProseProperty : IProseScope { }
  public sealed class ProseTree : IProseScope { }
  public sealed class ProseName : IProseScope { }
  public sealed class ProsePropertyKey : IProseScope { }
  public sealed class ProsePropertyValue : IProseScope { }
  public sealed class ProsePropertyDescription : IProseScope { }

  /// <summary>Shared instances for parameterless Prose scopes and common scope values.</summary>
  public static class ProseScopes {
    public static readonly ProseSpan Span = new();
    public static readonly ProseSection Section = new();
    public static readonly ProseSectionHeader SectionHeader = new();
    public static readonly ProseParagraph Paragraph = new();
    public static readonly ProseCodeBlock PlainCodeBlock = new();
    public static readonly ProseList UnorderedList = new(ProseListKind.Unordered);
    public static readonly ProseList OrderedList = new(ProseListKind.Ordered);
    public static readonly ProseListItem ListItem = new();
    public static readonly ProseTable Table = new();
    public static readonly ProseTableRow TableBodyRow = new(false);
    public static readonly ProseTableRow TableHeaderRow = new(true);
    public static readonly ProseTableCell TableCell = new();
    public static readonly ProseProperty Property = new();
    public static readonly ProseTree Tree = new();
    public static readonly ProseName Name = new();
    public static readonly ProsePropertyKey PropertyKey = new();
    public static readonly ProsePropertyValue PropertyValue = new();
    public static readonly ProsePropertyDescription PropertyDescription = new();
  }
}
