using HELIX.Prose;
using NUnit.Framework;
using UnityEngine;

namespace HELIX.Tests {
  public sealed class DatatypeConversionTests {
    private enum TestChoice : short {
      First = 1,
      Second = 2
    }

    [Test]
    public void NumericDatatypes_ConvertToAndFromCommonNumericTypes() {
      AssertNumericConversions(Datatypes.Int, 12);
      AssertNumericConversions(Datatypes.Long, 12L);
      AssertNumericConversions(Datatypes.Float, 12f);
      AssertNumericConversions(Datatypes.Double, 12d);
    }

    [Test]
    public void BuiltinDatatypes_RoundTripStrings() {
      AssertStringRoundTrip(Datatypes.String, "value");
      AssertStringRoundTrip(Datatypes.Int, 42);
      AssertStringRoundTrip(Datatypes.Long, 42000000000L);
      AssertStringRoundTrip(Datatypes.Float, 12.5f);
      AssertStringRoundTrip(Datatypes.Double, 12.5d);
      AssertStringRoundTrip(Datatypes.Bool, true);
      AssertStringRoundTrip(Datatypes.Enum<TestChoice>(), TestChoice.Second);
      AssertStringRoundTrip(Datatypes.Color, (Color)new Color32(0x33, 0x66, 0x99, 0xff));
    }

    [Test]
    public void FloatDatatype_AppliesDisplayFormatAndScaleToControlConversions() {
      var datatype = new FloatDatatype(format: "0.00", scale: 100f);
      var text = (IStringConvertible<float>)datatype;

      Assert.That(text.ToString(0.125f), Is.EqualTo("12.50"));
      Assert.That(text.FromString("12.50"), Is.EqualTo(0.125f));
      Assert.That(datatype.ToFloat(0.125f), Is.EqualTo(12.5f));
      Assert.That(datatype.FromFloat(12.5f), Is.EqualTo(0.125f));
    }

    [Test]
    public void EnumDatatype_ConvertsNamesAndNumericValues() {
      var datatype = Datatypes.Enum<TestChoice>();

      Assert.That(datatype.ToString(TestChoice.Second), Is.EqualTo("Second"));
      Assert.That(datatype.FromString("First"), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.ToByte(TestChoice.Second), Is.EqualTo(2));
      Assert.That(datatype.ToShort(TestChoice.Second), Is.EqualTo(2));
      Assert.That(datatype.ToInt(TestChoice.Second), Is.EqualTo(2));
      Assert.That(datatype.ToLong(TestChoice.Second), Is.EqualTo(2));
      Assert.That(datatype.ToFloat(TestChoice.Second), Is.EqualTo(2f));
      Assert.That(datatype.ToDouble(TestChoice.Second), Is.EqualTo(2d));
      Assert.That(datatype.FromByte(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromShort(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromInt(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromLong(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromFloat(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromDouble(1), Is.EqualTo(TestChoice.First));
    }

    [Test]
    public void ChoiceDatatype_ConvertsLabelsAndStoredNumericValues() {
      var datatype = new DatatypeChoiceDatatype<TestChoice>(new[] {
        new DatatypeChoice<TestChoice>(TestChoice.First, "First choice"),
        new DatatypeChoice<TestChoice>(TestChoice.Second, "Second choice")
      });

      Assert.That(datatype.ToString(TestChoice.Second), Is.EqualTo("Second choice"));
      Assert.That(datatype.FromString("First choice"), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromString("Second"), Is.EqualTo(TestChoice.Second));
      Assert.That(datatype.ToInt(TestChoice.Second), Is.EqualTo(2));
      Assert.That(datatype.FromByte(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromShort(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromInt(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromLong(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromFloat(1), Is.EqualTo(TestChoice.First));
      Assert.That(datatype.FromDouble(1), Is.EqualTo(TestChoice.First));
    }

    private static void AssertNumericConversions<T>(INumericConvertible<T> datatype, T value) {
      Assert.That(datatype.ToByte(value), Is.EqualTo(12));
      Assert.That(datatype.ToShort(value), Is.EqualTo(12));
      Assert.That(datatype.ToInt(value), Is.EqualTo(12));
      Assert.That(datatype.ToLong(value), Is.EqualTo(12));
      Assert.That(datatype.ToFloat(value), Is.EqualTo(12f));
      Assert.That(datatype.ToDouble(value), Is.EqualTo(12d));
      Assert.That(datatype.ToByte(datatype.FromByte(12)), Is.EqualTo(12));
      Assert.That(datatype.ToShort(datatype.FromShort(12)), Is.EqualTo(12));
      Assert.That(datatype.ToInt(datatype.FromInt(12)), Is.EqualTo(12));
      Assert.That(datatype.ToLong(datatype.FromLong(12)), Is.EqualTo(12));
      Assert.That(datatype.ToFloat(datatype.FromFloat(12)), Is.EqualTo(12f));
      Assert.That(datatype.ToDouble(datatype.FromDouble(12)), Is.EqualTo(12d));
    }

    private static void AssertStringRoundTrip<T>(IStringConvertible<T> datatype, T value) =>
      Assert.That(datatype.FromString(datatype.ToString(value)), Is.EqualTo(value));
  }
}
