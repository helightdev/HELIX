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

  /// <summary>Semantic inline or block markup interpreted by presentation writers.</summary>
  public sealed class ProseTextStyleModifier : IProseModifier {
    public ProseTextStyleModifier(ProseTextStyle style) => Style = style;
    public ProseTextStyle Style { get; }
  }

  /// <summary>Associates a semantic link target with a span.</summary>
  public sealed class ProseLinkModifier : IProseModifier {
    public ProseLinkModifier(string target) => Target = target ?? throw new System.ArgumentNullException(nameof(target));
    public string Target { get; }
  }

  public enum ProseTextAlignment : byte { Left, Center, Right }

  /// <summary>Provides a preferred alignment, primarily for table cells.</summary>
  public sealed class ProseTextAlignmentModifier : IProseModifier {
    public ProseTextAlignmentModifier(ProseTextAlignment alignment) => Alignment = alignment;
    public ProseTextAlignment Alignment { get; }
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

  public sealed class ProseAllowTruncateModifier : IProseModifier { }
  public sealed class ProseNoWrapModifier : IProseModifier { }
  public sealed class ProseHiddenModifier : IProseModifier { }

  /// <summary>Suppresses the property key while retaining its semantic name.</summary>
  public sealed class ProseHideNameModifier : IProseModifier { }

  /// <summary>Suppresses the configured separator before a property value or description.</summary>
  public sealed class ProseHideSeparatorModifier : IProseModifier { }

  /// <summary>Marks a property whose value equals its configured default.</summary>
  public sealed class ProseDefaultValueModifier : IProseModifier { }

  /// <summary>Carries a property's unformatted value when its text presentation uses a description.</summary>
  public sealed class ProsePropertyValueModifier : IProseModifier {
    public ProsePropertyValueModifier(object value) => Value = value;
    public object Value { get; }
  }

  public sealed class ProseLevelModifier : IProseModifier {
    public ProseLevelModifier(ProseLevel level) => Level = level;
    public ProseLevel Level { get; }
  }

  /// <summary>Shared instances and factories for immutable Prose modifiers.</summary>
  public static class ProseModifiers {
    public static readonly ProseTextStyleModifier Emphasis = new(ProseTextStyle.Emphasis);
    public static readonly ProseTextStyleModifier Strong = new(ProseTextStyle.Strong);
    public static readonly ProseTextStyleModifier Code = new(ProseTextStyle.Code);
    public static readonly ProseTextStyleModifier Quote = new(ProseTextStyle.Quote);
    public static readonly ProseTextStyleModifier Error = new(ProseTextStyle.Error);
    public static readonly ProseTextAlignmentModifier Left = new(ProseTextAlignment.Left);
    public static readonly ProseTextAlignmentModifier Center = new(ProseTextAlignment.Center);
    public static readonly ProseTextAlignmentModifier Right = new(ProseTextAlignment.Right);
    public static readonly ProseAllowTruncateModifier AllowTruncate = new();
    public static readonly ProseNoWrapModifier NoWrap = new();
    public static readonly ProseHiddenModifier Hidden = new();
    public static readonly ProseHideNameModifier HideName = new();
    public static readonly ProseHideSeparatorModifier HideSeparator = new();
    public static readonly ProseDefaultValueModifier DefaultValue = new();

    private static readonly ProseLevelModifier[] _levels = {
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

    public static ProseTextStyleModifier TextStyle(ProseTextStyle style) => style switch {
      ProseTextStyle.Emphasis => Emphasis,
      ProseTextStyle.Strong => Strong,
      ProseTextStyle.Code => Code,
      ProseTextStyle.Quote => Quote,
      ProseTextStyle.Error => Error,
      _ => new ProseTextStyleModifier(style)
    };

    public static ProseTextAlignmentModifier Alignment(ProseTextAlignment alignment) => alignment switch {
      ProseTextAlignment.Center => Center,
      ProseTextAlignment.Right => Right,
      _ => Left
    };

    public static ProseLevelModifier Level(ProseLevel level) {
      var index = (int)level;
      return index >= 0 && index < _levels.Length ? _levels[index] : new ProseLevelModifier(level);
    }
  }
}
