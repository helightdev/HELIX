using System.Collections.Generic;

namespace HELIX.SourceGen.Expressions;

/// <summary>A suspended table-transform property pipeline resumed by the VM.</summary>
internal sealed class MixinTransformRequest {
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
}
