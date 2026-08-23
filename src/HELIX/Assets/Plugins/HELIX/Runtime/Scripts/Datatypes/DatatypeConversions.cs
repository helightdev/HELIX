namespace HELIX.Datatypes {
  /// <summary>Describes a datatype that can round-trip values through their string representation.</summary>
  public interface IStringConvertible<T> {
    string ToString(T value);
    T FromString(string value);
  }

  /// <summary>Describes a datatype that can convert values to and from the commonly used numeric types.</summary>
  public interface INumericConvertible<T> {
    byte ToByte(T value);
    T FromByte(byte value);
    short ToShort(T value);
    T FromShort(short value);
    int ToInt(T value);
    T FromInt(int value);
    long ToLong(T value);
    T FromLong(long value);
    float ToFloat(T value);
    T FromFloat(float value);
    double ToDouble(T value);
    T FromDouble(double value);
  }
}
