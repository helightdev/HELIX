using System;
using HELIX.Compose;

namespace HELIX.Prose {
  /// <summary>A prose formatter that can materialize its value directly as a composable.</summary>
  public interface IComposeProseFormatter<in T> : IProseFormatter<T> {
    Composable ToComposable(T value);
  }

  /// <summary>
  /// Bridges a Compose spec into prose. Compose writers retain the spec as a composable, while other
  /// prose writers receive its string representation as a useful fallback.
  /// </summary>
  public sealed class SpecProseFormatter<T> : IComposeProseFormatter<T> where T : struct, ISpec {
    public static readonly SpecProseFormatter<T> Instance = new();

    private SpecProseFormatter() { }

    public Composable ToComposable(T value) => value.Composable();
    public void ToProse(IProseWriter writer, T value) => writer.Write(value.ToString());
  }

  public static class SpecProseWriterExtensions {
    public static void Write<T>(this IProseWriter writer, in T spec) where T : struct, ISpec {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      writer.Write(spec, SpecProseFormatter<T>.Instance);
    }
  }
}
