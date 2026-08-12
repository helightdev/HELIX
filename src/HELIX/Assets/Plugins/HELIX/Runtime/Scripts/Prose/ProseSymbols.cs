namespace HELIX.Prose {
  public sealed class ProseLineBreak : IProse {
    public static readonly ProseLineBreak Instance = new();
    private ProseLineBreak() { }

    public void ToProse(IProseWriter writer) {
      writer.Write("\n");
    }
  }

  public sealed class ProseSpace : IProse {
    public static readonly ProseSpace Instance = new();
    private ProseSpace() { }

    public void ToProse(IProseWriter writer) {
      writer.Write(" ");
    }
  }
}
