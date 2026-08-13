namespace HELIX.Prose {
  /// <summary>Always adds a required hard line break, including on an already empty line.</summary>
  public sealed class ProseLineBreak : IProse {
    public static readonly ProseLineBreak Instance = new();
    private ProseLineBreak() { }

    public void ToProse(IProseWriter writer) {
      if (writer is IProseLineBreakWriter lineBreakWriter) lineBreakWriter.WriteLineBreak(true);
      else writer.Write("\n");
    }
  }

  /// <summary>Adds a required line break only when the writer is not already on a new line.</summary>
  public sealed class ProseSoftLineBreak : IProse {
    public static readonly ProseSoftLineBreak Instance = new();
    private ProseSoftLineBreak() { }

    public void ToProse(IProseWriter writer) {
      if (writer is IProseLineBreakWriter lineBreakWriter) lineBreakWriter.WriteLineBreak(false);
      else writer.Write("\n");
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
