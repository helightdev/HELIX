namespace HELIX.Prose {
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
