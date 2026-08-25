using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HELIX.Serialization;
using Newtonsoft.Json;
using NUnit.Framework;
using Unity.Collections;

namespace HELIX.Tests {
  /// <summary>Round-trip coverage for format-specific datatype codecs.</summary>
  public sealed class DatatypeCodecTests {
    private struct TestSettings {
      public int count;
      public string label;
      public bool enabled;
    }

    private static readonly StructureDatatype<TestSettings> _settingsDatatype = new(
      "settings",
      new StructurePropertyDatatype<TestSettings>[] {
        new StructurePropertyDatatype<TestSettings, int>(
          "count", Datatypes.Int, (ref TestSettings value) => value.count,
          (ref TestSettings value, int field) => value.count = field
        ),
        new StructurePropertyDatatype<TestSettings, string>(
          "label", Datatypes.String, (ref TestSettings value) => value.label,
          (ref TestSettings value, string field) => value.label = field
        ),
        new StructurePropertyDatatype<TestSettings, bool>(
          "enabled", Datatypes.Bool, (ref TestSettings value) => value.enabled,
          (ref TestSettings value, bool field) => value.enabled = field
        )
      }
    );

    [Test]
    public void NewtonsoftJsonCodec_RoundTripsStructuredValues() {
      var text = new StringWriter();
      var writer = new NewtonsoftJsonUniversalWriter(new JsonTextWriter(text));

      Assert.That(writer.TryBeginObject(), Is.True);
      Assert.That(writer.TryWriteInt32("count", 3), Is.True);
      Assert.That(writer.TryWriteString("name", "HELIX"), Is.True);
      Assert.That(writer.TryBeginArray("values", 2), Is.True);
      Assert.That(writer.TryWriteBoolean(null, true), Is.True);
      Assert.That(writer.TryWriteDouble(null, 2.5), Is.True);
      Assert.That(writer.TryEndArray(), Is.True);
      Assert.That(writer.TryEndObject(), Is.True);

      var reader = new NewtonsoftJsonUniversalReader(new JsonTextReader(new StringReader(text.ToString())));
      Assert.That(reader.TryEnterObject(), Is.True);
      Assert.That(reader.TryReadInt32("count", out var count), Is.True);
      Assert.That(count, Is.EqualTo(3));
      Assert.That(reader.TryReadString("name", out var name), Is.True);
      Assert.That(name, Is.EqualTo("HELIX"));
      Assert.That(reader.TryEnterArray("values", out var length), Is.True);
      Assert.That(length, Is.EqualTo(-1));
      Assert.That(reader.TryReadBoolean(null, out var enabled), Is.True);
      Assert.That(enabled, Is.True);
      Assert.That(reader.TryReadDouble(null, out var number), Is.True);
      Assert.That(number, Is.EqualTo(2.5));
      Assert.That(reader.TryExitArray(), Is.True);
      Assert.That(reader.TryExitObject(), Is.True);
    }

    [Test]
    public void NewtonsoftJsonReader_FailedNullProbeDoesNotConsumeValue() {
      var reader = new NewtonsoftJsonUniversalReader(new JsonTextReader(new StringReader("{\"value\":\"text\"}")));
      Assert.That(reader.TryEnterObject(), Is.True);
      Assert.That(reader.TryReadNull("value", out var isNull), Is.True);
      Assert.That(isNull, Is.False);
      Assert.That(reader.TryReadString("value", out var value), Is.True);
      Assert.That(value, Is.EqualTo("text"));
    }

    [Test]
    public void DataStreamCodec_RoundTripsPositionalValues() {
      using var buffer = new NativeArray<byte>(128, Allocator.Temp);
      var writer = new DataStreamUniversalWriter(new DataStreamWriter(buffer));

      Assert.That(writer.TryBeginObject(), Is.True);
      Assert.That(writer.TryWriteInt32("count", 3), Is.True);
      Assert.That(writer.TryWriteString("name", "HELIX"), Is.True);
      Assert.That(writer.TryBeginArray("values", 2), Is.True);
      Assert.That(writer.TryWriteBoolean(null, true), Is.True);
      Assert.That(writer.TryWriteDouble(null, 2.5), Is.True);
      Assert.That(writer.TryEndArray(), Is.True);
      Assert.That(writer.TryEndObject(), Is.True);

      var written = writer.Writer;
      var reader = new DataStreamUniversalReader(new DataStreamReader(buffer.GetSubArray(0, written.Length)));
      Assert.That(reader.TryEnterObject(), Is.True);
      Assert.That(reader.TryReadInt32("count", out var count), Is.True);
      Assert.That(count, Is.EqualTo(3));
      Assert.That(reader.TryReadString("name", out var name), Is.True);
      Assert.That(name, Is.EqualTo("HELIX"));
      Assert.That(reader.TryEnterArray("values", out var length), Is.True);
      Assert.That(length, Is.EqualTo(2));
      Assert.That(reader.TryReadBoolean(null, out var enabled), Is.True);
      Assert.That(enabled, Is.True);
      Assert.That(reader.TryReadDouble(null, out var number), Is.True);
      Assert.That(number, Is.EqualTo(2.5));
      Assert.That(reader.TryExitArray(), Is.True);
      Assert.That(reader.TryExitObject(), Is.True);
    }

    [Test]
    public void DataStreamCodec_RoundTripsUtf8WithoutManagedStringBuffers() {
      using var buffer = new NativeArray<byte>(64, Allocator.Temp);
      var writer = new DataStreamUniversalWriter(new DataStreamWriter(buffer));
      Span<byte> source = stackalloc byte[] { 0x48, 0x45, 0x4c, 0x49, 0x58 };

      Assert.That(writer.TryWriteUtf8(null, source), Is.True);

      var written = writer.Writer;
      var reader = new DataStreamUniversalReader(new DataStreamReader(buffer.GetSubArray(0, written.Length)));
      Span<byte> destination = stackalloc byte[8];
      Assert.That(reader.TryReadUtf8(null, destination, out var length), Is.True);
      Assert.That(Encoding.UTF8.GetString(destination[..length]), Is.EqualTo("HELIX"));
    }

    [Test]
    public void DataStreamCodec_NullStringConsumesItsPresenceMarkerOnly() {
      using var buffer = new NativeArray<byte>(32, Allocator.Temp);
      var writer = new DataStreamUniversalWriter(new DataStreamWriter(buffer));
      Assert.That(Datatypes.String.TryWrite(writer, null, null), Is.True);
      Assert.That(writer.TryWriteInt32(null, 42), Is.True);

      var written = writer.Writer;
      var reader = new DataStreamUniversalReader(new DataStreamReader(buffer.GetSubArray(0, written.Length)));
      Assert.That(Datatypes.String.TryRead(reader, null, out var text), Is.True);
      Assert.That(text, Is.Null);
      Assert.That(reader.TryReadInt32(null, out var number), Is.True);
      Assert.That(number, Is.EqualTo(42));
    }

    [Test]
    public void HXRuntimeContext_ProvidesRegistryFactoriesAndReusableProseWriter() {
      using var context = new HXRuntimeContext();
      context.Extra["source"] = "test";
      Assert.That(context.Extra["source"], Is.EqualTo("test"));

      var text = new StringWriter();
      var writer = context.CreateStringWriter(text);
      Assert.That(Datatypes.Int.TryWrite(writer, null, 7), Is.True);
      Assert.That(text.ToString(), Is.EqualTo("7"));

      var proseWriter = context.ResetProseWriter();
      proseWriter.Write("first");
      Assert.That(proseWriter.Build(), Is.EqualTo("first"));
      Assert.That(context.ResetProseWriter(), Is.SameAs(proseWriter));
      Assert.That(proseWriter.Build(), Is.Empty);

      var reader = context.CreateStringReader(new StringReader("8"));
      Assert.That(Datatypes.Int.TryRead(reader, null, out var value), Is.True);
      Assert.That(value, Is.EqualTo(8));
    }

    [Test]
    public void StructureDatatype_IntrospectsAndRoundTripsProperties() {
      var initial = new TestSettings { count = 7, label = "HELIX", enabled = true };
      var untyped = (ICompositeDatatype)_settingsDatatype;
      Assert.That(untyped.ComponentCount, Is.EqualTo(3));
      Assert.That(untyped.GetComponentName(1), Is.EqualTo("label"));
      Assert.That(untyped.GetComponentType(1), Is.EqualTo(typeof(string)));
      Assert.That(untyped.GetComponentDatatype(1), Is.SameAs(Datatypes.String));
      var count = (StructurePropertyDatatype<TestSettings, int>)_settingsDatatype.Properties[0];
      Assert.That(count.GetValue(ref initial), Is.EqualTo(7));
      var changed = initial;
      count.SetValue(ref changed, 9);
      Assert.That(changed.count, Is.EqualTo(9));

      var text = new StringWriter();
      var writer = new NewtonsoftJsonUniversalWriter(new JsonTextWriter(text));
      Assert.That(_settingsDatatype.TryWrite(writer, null, initial), Is.True);

      var reader = new NewtonsoftJsonUniversalReader(new JsonTextReader(new StringReader(text.ToString())));
      Assert.That(_settingsDatatype.TryRead(reader, null, out var result), Is.True);
      Assert.That(result.count, Is.EqualTo(7));
      Assert.That(result.label, Is.EqualTo("HELIX"));
      Assert.That(result.enabled, Is.True);
    }

    [Test]
    public void StructureDatatype_CollectionsCanBeExtendedByMixins() {
      var datatype = new StructureDatatype<TestSettings>(
        "settings", new List<StructurePropertyDatatype<TestSettings>>()
      );
      datatype.Properties.Add(new StructurePropertyDatatype<TestSettings, int>(
        "count", Datatypes.Int, (ref TestSettings value) => value.count,
        (ref TestSettings value, int field) => value.count = field
      ));

      Assert.That(((ICompositeDatatype)datatype).ComponentCount, Is.EqualTo(1));
      Assert.That(((ICompositeDatatype)datatype).GetComponentName(0), Is.EqualTo("count"));
    }

    [Test]
    public void ListDatatype_IntrospectsAndMutatesItems() {
      ICollectionDatatype datatype = new ListDatatype<int>(Datatypes.Int);
      var proxy = datatype.CollectionProxy;
      object values = new List<int> { 1, 2 };

      Assert.That(proxy.ItemType, Is.EqualTo(typeof(int)));
      Assert.That(datatype.ItemDatatype, Is.SameAs(Datatypes.Int));
      Assert.That(proxy.GetItemCount(values), Is.EqualTo(2));
      Assert.That(proxy.GetItem(values, 1), Is.EqualTo(2));
      values = proxy.SetItem(values, 1, 3);
      values = proxy.AddItem(values, 4);
      values = proxy.RemoveItem(values, 0);
      Assert.That((List<int>)values, Is.EqualTo(new[] { 3, 4 }));
    }

    [Test]
    public void ListDatatype_RoundTripsNewtonsoftJsonAndDataStream() {
      var datatype = new ListDatatype<int>(Datatypes.Int);
      var expected = new List<int> { 1, 2, 3 };

      var text = new StringWriter();
      var jsonWriter = new NewtonsoftJsonUniversalWriter(new JsonTextWriter(text));
      Assert.That(datatype.TryWrite(jsonWriter, null, expected), Is.True);
      var jsonReader = new NewtonsoftJsonUniversalReader(new JsonTextReader(new StringReader(text.ToString())));
      Assert.That(datatype.TryRead(jsonReader, null, out var jsonResult), Is.True);
      Assert.That(jsonResult, Is.EqualTo(expected));

      using var buffer = new NativeArray<byte>(64, Allocator.Temp);
      var streamWriter = new DataStreamUniversalWriter(new DataStreamWriter(buffer));
      Assert.That(datatype.TryWrite(streamWriter, null, expected), Is.True);
      var written = streamWriter.Writer;
      var streamReader = new DataStreamUniversalReader(
        new DataStreamReader(buffer.GetSubArray(0, written.Length))
      );
      Assert.That(datatype.TryRead(streamReader, null, out var streamResult), Is.True);
      Assert.That(streamResult, Is.EqualTo(expected));
    }

    [Test]
    public void StructureDatatype_ThrowsForNonSerializableFieldDatatype() {
      var datatype = new StructureDatatype<TestSettings>(
        "settings",
        new StructurePropertyDatatype<TestSettings>[] {
          new StructurePropertyDatatype<TestSettings, int>(
            "count", new FormattingDatatype<int>(value => value.ToString()),
            (ref TestSettings value) => value.count,
            (ref TestSettings value, int field) => value.count = field
          )
        }
      );
      var writer = new NewtonsoftJsonUniversalWriter(new JsonTextWriter(new StringWriter()));

      var exception = Assert.Throws<NotSupportedException>(() =>
        datatype.TryWrite(writer, null, new TestSettings { count = 1 })
      );
      Assert.That(exception.Message, Does.Contain("count"));
      Assert.That(exception.Message, Does.Contain(nameof(ISerializableDatatype<int>)));
    }
  }
}
