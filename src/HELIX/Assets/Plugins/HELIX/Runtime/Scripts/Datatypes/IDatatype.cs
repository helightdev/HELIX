using HELIX.Prose;

namespace HELIX {
  public interface IDatatype { }

  /// <summary>
  /// Describes a value and provides its semantic fallback expansion when a sink does not handle it directly.
  /// Implementations should be immutable so configured instances can be defined statically and reused.
  /// </summary>
  public interface IDatatype<in T> : IDatatype {
    void ToProse(IProseWriter writer, T value);
  }
}
