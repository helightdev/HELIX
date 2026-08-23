namespace HELIX.Prose {
  public interface IProseDatatype { }

  /// <summary>
  /// Describes a value and provides its semantic fallback expansion when a sink does not handle it directly.
  /// Implementations should be immutable so configured instances can be defined statically and reused.
  /// </summary>
  public interface IProseDatatype<in T> : IProseDatatype {
    void ToProse(IProseWriter writer, T value);
  }
}
