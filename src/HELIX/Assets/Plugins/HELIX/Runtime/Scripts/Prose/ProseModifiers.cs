namespace HELIX.Prose {
  [System.Flags]
  public enum ProseTextStyle : byte {
    None = 0,
    Emphasis = 1 << 0,
    Strong = 1 << 1,
    Code = 1 << 2,
    Quote = 1 << 3,
    Error = 1 << 4
  }

  /// <summary>Semantic inline/block markup interpreted by presentation writers.</summary>
  public sealed class TextStyleMarker : IProseModifier {
    public static readonly TextStyleMarker Emphasis = new(ProseTextStyle.Emphasis);
    public static readonly TextStyleMarker Strong = new(ProseTextStyle.Strong);
    public static readonly TextStyleMarker Code = new(ProseTextStyle.Code);
    public static readonly TextStyleMarker Quote = new(ProseTextStyle.Quote);
    public static readonly TextStyleMarker Error = new(ProseTextStyle.Error);

    public TextStyleMarker(ProseTextStyle style) => Style = style;
    public ProseTextStyle Style { get; }

    public static TextStyleMarker For(ProseTextStyle style) => style switch {
      ProseTextStyle.Emphasis => Emphasis,
      ProseTextStyle.Strong => Strong,
      ProseTextStyle.Code => Code,
      ProseTextStyle.Quote => Quote,
      ProseTextStyle.Error => Error,
      _ => new TextStyleMarker(style)
    };
  }

  /// <summary>Associates a semantic link target with a span.</summary>
  public sealed class LinkMarker : IProseModifier {
    public LinkMarker(string target) => Target = target ?? throw new System.ArgumentNullException(nameof(target));
    public string Target { get; }
  }

  public enum ProseTextAlignment : byte { Left, Center, Right }

  /// <summary>Provides a preferred alignment, primarily for table cells.</summary>
  public sealed class TextAlignmentMarker : IProseModifier {
    public static readonly TextAlignmentMarker Left = new(ProseTextAlignment.Left);
    public static readonly TextAlignmentMarker Center = new(ProseTextAlignment.Center);
    public static readonly TextAlignmentMarker Right = new(ProseTextAlignment.Right);

    public TextAlignmentMarker(ProseTextAlignment alignment) => Alignment = alignment;
    public ProseTextAlignment Alignment { get; }

    public static TextAlignmentMarker For(ProseTextAlignment alignment) => alignment switch {
      ProseTextAlignment.Center => Center,
      ProseTextAlignment.Right => Right,
      _ => Left
    };
  }

  public enum ProseLevel {
    Hidden,
    Fine,
    Debug,
    Info,
    Warning,
    Hint,
    Summary,
    Error,
    Off
  }

  public sealed class AllowTruncate : IProseModifier {
    public static readonly AllowTruncate Instance = new();
    private AllowTruncate() { }
  }

  public sealed class NoWrap : IProseModifier {
    public static readonly NoWrap Instance = new();
    private NoWrap() { }
  }

  public sealed class Hidden : IProseModifier {
    public static readonly Hidden Instance = new();
    private Hidden() { }
  }

  public sealed class LevelMarker : IProseModifier {
    private static readonly LevelMarker[] Cache = {
      new(ProseLevel.Hidden),
      new(ProseLevel.Fine),
      new(ProseLevel.Debug),
      new(ProseLevel.Info),
      new(ProseLevel.Warning),
      new(ProseLevel.Hint),
      new(ProseLevel.Summary),
      new(ProseLevel.Error),
      new(ProseLevel.Off)
    };

    public LevelMarker(ProseLevel level) {
      Level = level;
    }

    public ProseLevel Level { get; }

    public static LevelMarker For(ProseLevel level) {
      var index = (int)level;
      return index >= 0 && index < Cache.Length ? Cache[index] : new LevelMarker(level);
    }
  }
}
