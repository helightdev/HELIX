using System.Collections.Generic;
using HelixSourceGenerator.Language;

namespace HelixSourceGenerator.Generators;

public sealed partial class MixinGenerator {
  private static string DebugStateKey(LateExpressionWork work) {
    return string.Join(
      "\u001f", work.Provider, work.SourceType, work.SourceMember, work.LateProgram
    );
  }

  private static string BuildDebugTrace(MixinRenderModel render) {
    return MixinDebugRenderer.BuildTrace(
      new MixinDebugRenderData(
        render.StringPool,
        render.DebugExpressions,
        render.DebugStringPool,
        render.GenerationVersion,
        new MixinDebugFingerprintPart(render.Fingerprint.Outputs.Hash, render.Fingerprint.Outputs.Length),
        new MixinDebugFingerprintPart(render.Fingerprint.Variables.Hash, render.Fingerprint.Variables.Length),
        new MixinDebugFingerprintPart(render.Fingerprint.Signatures.Hash, render.Fingerprint.Signatures.Length)
      )
    );
  }

  private static string BuildFinalDebugState(
    IEnumerable<MixinDebugFinalState> states,
    IReadOnlyDictionary<string, object> sharedVariables
  ) {
    return MixinDebugRenderer.BuildFinalState(states, sharedVariables);
  }
}
