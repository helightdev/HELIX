namespace HELIX.Prose {
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
