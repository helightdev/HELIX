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
    bool BeginFrame(IProseScope scope);
    void PopFrame();
    void PushModifier(IProseModifier modifier);
    void Write(IProse prose);
    void Write<T>(T value, IProseFormatter<T> formatter);
    void Write(string text);
  }

  /// <summary>A self-formatting semantic value.</summary>
  public interface IProse {
    void ToProse(IProseWriter writer);
  }

  /// <summary>
  /// Describes a value and provides its semantic fallback expansion when a sink does not handle it directly.
  /// Implementations should be immutable so configured instances can be defined statically and reused.
  /// </summary>
  public interface IProseFormatter<in T> {
    void ToProse(IProseWriter writer, T value);
  }

  /// <summary>Marker for semantic frame context.</summary>
  public interface IProseScope { }

  /// <summary>Marker for writer-owned presentation metadata.</summary>
  public interface IProseModifier { }

  /// <summary>Convenience base for implementing a monodirectional Prose sink.</summary>
  public abstract class ProseWriter : IProseWriter {
    public abstract bool BeginFrame(IProseScope scope);
    public abstract void PopFrame();
    public abstract void PushModifier(IProseModifier modifier);
    public abstract void Write(IProse prose);
    public abstract void Write<T>(T value, IProseFormatter<T> formatter);
    public abstract void Write(string text);
  }

  internal static class ProseLiterals {
    internal const string Null = "null";
  }
}
