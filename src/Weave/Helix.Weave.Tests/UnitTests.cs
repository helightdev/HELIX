using System.Linq;
using Xunit;

namespace Helix.Weave.Tests;

public class LitTests {
  [Fact]
  public void QuotesAndEscapes() {
    Assert.Equal("\"a\\\"b\"", Lit.Of("a\"b"));
    Assert.Equal("\"a\\\\b\"", Lit.Of("a\\b"));
    Assert.Equal("\"line\\nbreak\"", Lit.Of("line\nbreak"));
    Assert.Equal("\"tab\\there\"", Lit.Of("tab\there"));
    Assert.Equal("null", Lit.Of((string)null));
  }

  [Fact]
  public void EscapesControlCharacters() => Assert.Equal("\"\\u0001\"", Lit.Of("\u0001"));

  [Fact]
  public void RendersNumbersRoundTrippably() {
    Assert.Equal("1.5f", Lit.Of(1.5f));
    Assert.Equal("2.0f", Lit.Of(2f));
    Assert.Equal("3.0d", Lit.Of(3d));
    Assert.Equal("42", Lit.Of(42));
    Assert.Equal("7L", Lit.Of(7L));
  }

  [Fact]
  public void RefusesBareEnums() =>
    // An unqualified enum member is the classic silent codegen bug; make it loud instead.
    Assert.Throws<System.ArgumentException>(() => Lit.Of((object)System.DayOfWeek.Monday));

  [Fact]
  public void QualifiesEnumMembers() =>
    Assert.Equal("global::X.Mode.Fast", Lit.Enum("global::X.Mode", "Fast"));
}

public class StringPoolTests {
  [Fact]
  public void InternsByValue() {
    var pool = new StringPool();
    Assert.Equal(pool.Intern("same"), pool.Intern("same"));
    Assert.NotEqual(pool.Intern("a"), pool.Intern("b"));
    Assert.Equal(3, pool.Count); // same, a, b
  }

  [Fact]
  public void RoundTripsText() {
    var pool = new StringPool();
    Assert.Equal("value", pool.Resolve(pool.Intern("value")));
    Assert.True(pool.Intern(null).IsNull);
    Assert.Null(pool.Resolve(StringHandle.Null));
  }
}

public class OutputBufferTests {
  [Fact]
  public void FingerprintTracksContent() {
    var pool = new StringPool();
    var left = new OutputBuffer();
    var right = new OutputBuffer();
    left.Add(EmissionTarget.Class, pool.Intern("a"));
    right.Add(EmissionTarget.Class, pool.Intern("a"));
    Assert.Equal(left.Fingerprint, right.Fingerprint);

    right.Add(EmissionTarget.Class, pool.Intern("b"));
    Assert.NotEqual(left.Fingerprint, right.Fingerprint);
  }

  [Fact]
  public void RollbackRestoresFingerprint() {
    var pool = new StringPool();
    var buffer = new OutputBuffer();
    buffer.Add(EmissionTarget.Class, pool.Intern("keep"));
    var fingerprint = buffer.Fingerprint;
    var checkpoint = buffer.Checkpoint();

    buffer.Add(EmissionTarget.Class, pool.Intern("drop"));
    buffer.Rollback(checkpoint);

    Assert.Equal(1, buffer.Count);
    Assert.Equal(fingerprint, buffer.Fingerprint);
  }

  [Fact]
  public void SequenceIsInsertionOrder() {
    var pool = new StringPool();
    var buffer = new OutputBuffer();
    buffer.Add(EmissionTarget.Injection, pool.Intern("first"), pool.Intern("T"), priority: 5);
    buffer.Add(EmissionTarget.Injection, pool.Intern("second"), pool.Intern("T"), priority: 5);
    Assert.Equal(new[] { 0, 1 }, buffer.Outputs.Select(o => o.Sequence));
  }
}

public class CodeTests {
  [Fact]
  public void JoinSkipsEmptyEntries() =>
    Assert.Equal("a, c", Code.Join(new[] { "a", "", "c" }));

  [Fact]
  public void DedentKeepsRelativeShape() {
    var text = "\n    if (x) {\n      y();\n    }\n";
    Assert.Equal("if (x) {\n  y();\n}", Code.Dedent(text));
  }
}
