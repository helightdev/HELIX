using System;

namespace HELIX.Prose {
  /// <summary>
  /// Consumes semantic prose in immediate mode. This contract is intentionally monodirectional: the sink
  /// owns all interpretation and state, while producers only describe content.
  /// </summary>
  public interface IProseWriter {
    /// <summary>
    /// Begins a semantic frame. A false result means that the producer must skip the frame and must not pop it.
    /// </summary>
    bool TryBegin<T>(T scope) where T : IProseScope;
    /// <summary>
    /// Begins a semantic frame. If the frame is not accepted, it is retained as an ignored frame so a matching
    /// <see cref="End"/> remains valid while all content and modifiers in that frame are discarded.
    /// </summary>
    void Begin<T>(T scope) where T : IProseScope;
    void End();
    void Push<T>(T modifier) where T : IProseModifier;
    void Write<T>(T prose) where T : IProse;
    void Write<T>(T value, IDatatype<T> datatype);
    void Write(string text);
  }

  /// <summary>Optional text-writer capability for semantic required line boundaries.</summary>
  public interface IProseLineBreakWriter {
    /// <param name="force">
    /// When true, always appends a hard break; otherwise appends one only after line content.
    /// </param>
    void WriteLineBreak(bool force);
  }

  /// <summary>A self-formatting semantic value.</summary>
  public interface IProse {
    void ToProse(IProseWriter writer);
  }

  /// <summary>Marker for semantic frame context.</summary>
  public interface IProseScope { }

  /// <summary>Marker for writer-owned presentation metadata.</summary>
  public interface IProseModifier { }

  /// <summary>Convenience base for implementing a monodirectional Prose sink.</summary>
  public abstract class ProseWriter : IProseWriter {
    public abstract bool TryBegin<T>(T scope) where T : IProseScope;
    public abstract void Begin<T>(T scope) where T : IProseScope;
    public abstract void End();
    public abstract void Push<T>(T modifier) where T : IProseModifier;
    public abstract void Write<T>(T prose) where T : IProse;
    public abstract void Write<T>(T value, IDatatype<T> datatype);
    public abstract void Write(string text);
  }

  internal static class ProseLiterals {
    internal const string Null = "null";
  }
}
