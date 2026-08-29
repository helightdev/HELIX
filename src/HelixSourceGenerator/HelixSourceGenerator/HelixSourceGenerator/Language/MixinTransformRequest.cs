using System.Collections.Generic;
using HelixSourceGenerator.Language.Functions;

namespace HelixSourceGenerator.Language;

/// <summary>A suspended table-transform property pipeline resumed by the VM.</summary>
internal sealed class MixinTransformRequest : MixinValue {
  internal MixinTransformRequest(
    MixinExpressionTable source, TableTransformKind kind, string functionLabel
  ) {
    Source = source;
    Kind = kind;
    FunctionLabel = functionLabel;
  }

  internal MixinExpressionTable Source { get; }
  internal TableTransformKind Kind { get; }
  internal string FunctionLabel { get; }
  internal IReadOnlyList<MixinExpressionProperty> RemainingProperties { get; set; }
  public override object Value => this;
  public override bool IsTruthy => true;

  public override string Render() {
    return Source.Render();
  }

  public override void Fingerprint(MixinFingerprintBuilder builder) {
    builder.Append(nameof(MixinTransformRequest));
    Source.Fingerprint(builder);
    builder.Append((int)Kind);
    builder.Append(FunctionLabel);
  }
}