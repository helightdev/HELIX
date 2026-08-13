namespace HELIX.Prose {
  /// <summary>A general inline span used as the target of markup modifiers.</summary>
  public sealed class ProseSpan : IProseScope {
    public static readonly ProseSpan Instance = new();
    private ProseSpan() { }
  }

  /// <summary>A block containing an optional header and paragraph-oriented content.</summary>
  public sealed class ProseSection : IProseScope {
    public static readonly ProseSection Instance = new();
    private ProseSection() { }
  }

  public sealed class ProseSectionHeader : IProseScope {
    public static readonly ProseSectionHeader Instance = new();
    private ProseSectionHeader() { }
  }

  public sealed class ProseParagraph : IProseScope {
    public static readonly ProseParagraph Instance = new();
    private ProseParagraph() { }
  }

  /// <summary>A preformatted block of source text with an optional language hint.</summary>
  public sealed class ProseCodeBlock : IProseScope {
    public static readonly ProseCodeBlock Plain = new();

    public ProseCodeBlock(string language = null) => Language = language ?? string.Empty;
    public string Language { get; }
  }

  public enum ProseListKind : byte { Unordered, Ordered }

  /// <summary>A semantic ordered or unordered list.</summary>
  public sealed class ProseList : IProseScope {
    public static readonly ProseList Unordered = new(ProseListKind.Unordered);
    public static readonly ProseList Ordered = new(ProseListKind.Ordered);

    public ProseList(ProseListKind kind, int start = 1) {
      if (start < 0) throw new System.ArgumentOutOfRangeException(nameof(start));
      Kind = kind;
      Start = start;
    }

    public ProseListKind Kind { get; }
    public int Start { get; }
  }

  public sealed class ProseListItem : IProseScope {
    public static readonly ProseListItem Instance = new();
    private ProseListItem() { }
  }

  public sealed class ProseTable : IProseScope {
    public static readonly ProseTable Instance = new();
    private ProseTable() { }
  }

  /// <summary>A table row whose header state is semantic rather than inferred from its position.</summary>
  public sealed class ProseTableRow : IProseScope {
    public static readonly ProseTableRow Body = new(false);
    public static readonly ProseTableRow Header = new(true);

    public ProseTableRow(bool isHeader) => IsHeader = isHeader;
    public bool IsHeader { get; }
  }

  public sealed class ProseTableCell : IProseScope {
    public static readonly ProseTableCell Instance = new();
    private ProseTableCell() { }
  }

  public sealed class ProseProperty : IProseScope {
    public static readonly ProseProperty Instance = new();
    private ProseProperty() { }
  }

  public sealed class ProseTree : IProseScope {
    public static readonly ProseTree Instance = new();
    private ProseTree() { }
  }

  public sealed class ProseName : IProseScope {
    public static readonly ProseName Instance = new();
    private ProseName() { }
  }

  public sealed class ProsePropertyKey : IProseScope {
    public static readonly ProsePropertyKey Instance = new();
    private ProsePropertyKey() { }
  }

  public sealed class ProsePropertyValue : IProseScope {
    public static readonly ProsePropertyValue Instance = new();
    private ProsePropertyValue() { }
  }
}
