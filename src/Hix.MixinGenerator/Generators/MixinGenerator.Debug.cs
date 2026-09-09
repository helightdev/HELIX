using System.Collections.Generic;
using Hix.Diagnostics;

namespace HelixSourceGenerator.Generators;

public sealed partial class MixinGenerator {
  private static string DebugStateKey(LateExpressionWork work) {
    return string.Join(
      "\u001f", work.Provider, work.SourceType, work.SourceMember, work.LateIdentity
    );
  }

  private static string BuildDebugTrace(HixRenderModel render) {
    return HixDebugRenderer.BuildTrace(
      new HixDebugRenderData(
        render.StringPool,
        render.DebugExpressions,
        render.DebugStringPool,
        render.GenerationVersion,
        new HixDebugFingerprintPart(render.Fingerprint.Outputs.Hash, render.Fingerprint.Outputs.Length),
        new HixDebugFingerprintPart(render.Fingerprint.Variables.Hash, render.Fingerprint.Variables.Length),
        new HixDebugFingerprintPart(render.Fingerprint.Signatures.Hash, render.Fingerprint.Signatures.Length)
      )
    );
  }

  private static string BuildFinalDebugState(
    IEnumerable<HixDebugFinalState> states,
    IReadOnlyDictionary<string, object> sharedVariables
  ) {
    return HixDebugRenderer.BuildFinalState(states, sharedVariables);
  }
}
