using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// A decoded attribute argument of unknown static type, keeping both the value and its C# literal
/// rendering. Mirrors Hix's distinction between <c>:unwrap</c> (the raw content, typically a
/// string holding source text) and <c>:csharpLiteral</c> (the value as a literal).
/// </summary>
public readonly struct Literal {
  private Literal(object value, string text, bool exists) {
    Value = value;
    Text = text;
    Exists = exists;
  }

  /// <summary>The decoded value: a boxed primitive, a string, or null.</summary>
  public object Value { get; }

  /// <summary>The value rendered as C# source that reproduces it.</summary>
  public string Text { get; }

  /// <summary>True when the attribute supplied this argument at all.</summary>
  public bool Exists { get; }

  /// <summary>
  /// The raw string content, for arguments that carry source text rather than a value.
  /// Null when the argument was not a string.
  /// </summary>
  public string Source => Value as string;

  public bool IsNull => Value is null;
  public override string ToString() => Text ?? "null";

  public static Literal None => default;

  public static Literal From(TypedConstant constant) =>
    new(constant.Kind == TypedConstantKind.Array ? null : constant.Value,
        Lit.OfTypedConstant(constant), true);

  public static Literal Of(object value) => new(value, Lit.Of(value), true);
}
