using System;
using System.Text;
using Newtonsoft.Json;

namespace HELIX.Serialization {
  public sealed class NewtonsoftJsonUniversalReader : IUniversalReader {
    private readonly JsonReader _reader;
    private readonly JsonSerializer _serializer;
    private bool _pendingValue;
    private string _pendingName;

    public NewtonsoftJsonUniversalReader(JsonReader reader, JsonSerializer serializer = null) {
      _reader = reader ?? throw new ArgumentNullException(nameof(reader));
      _serializer = serializer ?? JsonSerializer.CreateDefault();
    }

    public bool SupportsCustomType(Type type) => IsSupported(type);

    public bool TryEnterObject(string name = null) {
      if (!MoveToValue(name) || _reader.TokenType != JsonToken.StartObject) return false;
      ConsumeValue();
      return true;
    }
    public bool TryExitObject() => MoveToEnd(JsonToken.EndObject);
    public bool TryEnterArray(string name, out int length) {
      length = -1;
      if (!MoveToValue(name) || _reader.TokenType != JsonToken.StartArray) return false;
      ConsumeValue();
      return true;
    }
    public bool TryReadArrayEnd(out bool isEnd) {
      isEnd = false;
      if (!MoveToValue(null)) return false;
      isEnd = _reader.TokenType == JsonToken.EndArray;
      if (isEnd) ConsumeValue();
      return true;
    }
    public bool TryExitArray() => MoveToEnd(JsonToken.EndArray);
    public bool TryReadNull(string name, out bool isNull) {
      isNull = false;
      if (!MoveToValue(name)) return false;
      isNull = _reader.TokenType == JsonToken.Null;
      if (isNull) ConsumeValue();
      return true;
    }
    public bool TryReadBoolean(string name, out bool value) => TryRead(name, JsonToken.Boolean, out value);
    public bool TryReadInt32(string name, out int value) => TryConvert(name, out value);
    public bool TryReadInt64(string name, out long value) => TryConvert(name, out value);
    public bool TryReadSingle(string name, out float value) => TryConvert(name, out value);
    public bool TryReadDouble(string name, out double value) => TryConvert(name, out value);
    public bool TryReadString(string name, out string value) {
      value = null;
      if (!MoveToValue(name)) return false;
      if (_reader.TokenType != JsonToken.String || _reader.Value is not string text) return false;
      value = text;
      ConsumeValue();
      return true;
    }

    public bool TryReadUtf8(string name, Span<byte> destination, out int bytesWritten) {
      bytesWritten = 0;
      if (!MoveToValue(name) || _reader.TokenType != JsonToken.String || _reader.Value is not string value)
        return false;
      var required = Encoding.UTF8.GetByteCount(value);
      if (required > destination.Length) return false;
      bytesWritten = Encoding.UTF8.GetBytes(value.AsSpan(), destination);
      ConsumeValue();
      return true;
    }

    public bool TryReadBytes(string name, out byte[] value) {
      value = null;
      if (!MoveToValue(name)) return false;
      if (_reader.TokenType == JsonToken.Bytes) {
        value = (byte[])_reader.Value;
        ConsumeValue();
        return true;
      }
      if (_reader.TokenType != JsonToken.String || _reader.Value is not string encoded) return false;
      try {
        value = Convert.FromBase64String(encoded);
        ConsumeValue();
        return true;
      } catch (FormatException) {
        return false;
      }
    }

    public bool TryReadCustom<T>(string name, out T value) {
      value = default;
      if (!SupportsCustomType(typeof(T)) || !MoveToValue(name)) return false;
      try {
        value = _serializer.Deserialize<T>(_reader);
        ConsumeValue();
        return true;
      } catch (JsonException) {
        return false;
      }
    }

    private bool TryRead<T>(string name, JsonToken token, out T value) {
      value = default;
      if (!MoveToValue(name) || _reader.TokenType != token || _reader.Value is not T typed) return false;
      value = typed;
      ConsumeValue();
      return true;
    }

    private bool TryConvert<T>(string name, out T value) {
      value = default;
      if (!MoveToValue(name) || _reader.TokenType is not (JsonToken.Integer or JsonToken.Float)) return false;
      try {
        value = (T)Convert.ChangeType(_reader.Value, typeof(T), _reader.Culture);
        ConsumeValue();
        return true;
      } catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException) {
        return false;
      }
    }

    private bool MoveToValue(string name) {
      if (_pendingValue)
        return name == null || _pendingName == null || string.Equals(_pendingName, name, StringComparison.Ordinal);
      if (_reader.TokenType == JsonToken.None || IsValue(_reader.TokenType) ||
          _reader.TokenType is JsonToken.StartObject or JsonToken.StartArray)
        if (!ReadContent()) return false;
      if (_reader.TokenType == JsonToken.PropertyName) {
        _pendingName = (string)_reader.Value;
        if (name != null && !string.Equals(_pendingName, name, StringComparison.Ordinal)) return false;
        if (!ReadContent()) return false;
        _pendingValue = true;
        return true;
      }
      if (name != null) return false;
      _pendingName = null;
      _pendingValue = true;
      return true;
    }

    private bool MoveToEnd(JsonToken end) {
      if (_pendingValue) return false;
      if (_reader.TokenType != end && !ReadContent()) return false;
      return _reader.TokenType == end;
    }

    private void ConsumeValue() {
      _pendingValue = false;
      _pendingName = null;
    }

    private bool ReadContent() {
      do {
        if (!_reader.Read()) return false;
      } while (_reader.TokenType == JsonToken.Comment);
      return true;
    }

    private static bool IsValue(JsonToken token) => token is
      JsonToken.Integer or JsonToken.Float or JsonToken.String or JsonToken.Boolean or JsonToken.Null or
      JsonToken.Date or JsonToken.Bytes or JsonToken.EndObject or JsonToken.EndArray;

    private bool IsSupported(Type type) {
      if (type == null || type.IsPointer || type.IsByRef || type.ContainsGenericParameters) return false;
      try {
        return _serializer.ContractResolver.ResolveContract(type) != null;
      } catch (JsonException) {
        return false;
      }
    }
  }

  public sealed class NewtonsoftJsonUniversalWriter : IUniversalWriter {
    private readonly JsonWriter _writer;
    private readonly JsonSerializer _serializer;

    public NewtonsoftJsonUniversalWriter(JsonWriter writer, JsonSerializer serializer = null) {
      _writer = writer ?? throw new ArgumentNullException(nameof(writer));
      _serializer = serializer ?? JsonSerializer.CreateDefault();
    }

    public bool SupportsCustomType(Type type) {
      if (type == null || type.IsPointer || type.IsByRef || type.ContainsGenericParameters) return false;
      try {
        return _serializer.ContractResolver.ResolveContract(type) != null;
      } catch (JsonException) {
        return false;
      }
    }

    public bool TryBeginObject(string name = null) => TryWrite(name, _writer.WriteStartObject);
    public bool TryEndObject() => TryWrite(null, _writer.WriteEndObject, false);
    public bool TryBeginArray(string name, int length) => TryWrite(name, _writer.WriteStartArray);
    public bool TryEndArray() => TryWrite(null, _writer.WriteEndArray, false);
    public bool TryWriteNull(string name, bool isNull) => !isNull || TryWrite(name, _writer.WriteNull);
    public bool TryWriteBoolean(string name, bool value) => TryWrite(name, () => _writer.WriteValue(value));
    public bool TryWriteInt32(string name, int value) => TryWrite(name, () => _writer.WriteValue(value));
    public bool TryWriteInt64(string name, long value) => TryWrite(name, () => _writer.WriteValue(value));
    public bool TryWriteSingle(string name, float value) => TryWrite(name, () => _writer.WriteValue(value));
    public bool TryWriteDouble(string name, double value) => TryWrite(name, () => _writer.WriteValue(value));
    public bool TryWriteString(string name, string value) =>
      value != null && TryWrite(name, () => _writer.WriteValue(value));
    public bool TryWriteUtf8(string name, Span<byte> value) =>
      TryWriteString(name, Encoding.UTF8.GetString(value));
    public bool TryWriteBytes(string name, byte[] value) => TryWrite(name, () => _writer.WriteValue(value));

    public bool TryWriteCustom<T>(string name, T value) {
      if (!SupportsCustomType(typeof(T))) return false;
      try {
        WriteName(name);
        _serializer.Serialize(_writer, value, typeof(T));
        return true;
      } catch (JsonException) {
        return false;
      }
    }

    private bool TryWrite(string name, Action write, bool writeName = true) {
      try {
        if (writeName) WriteName(name);
        write();
        return true;
      } catch (JsonException) {
        return false;
      }
    }

    private void WriteName(string name) {
      if (name != null) _writer.WritePropertyName(name);
    }
  }
}
