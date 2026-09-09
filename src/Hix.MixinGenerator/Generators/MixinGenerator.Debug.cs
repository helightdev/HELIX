using System.Collections.Generic;
using Hix.Diagnostics;

namespace HelixSourceGenerator.Generators;

public sealed partial class MixinGenerator {
  private static string DebugStateKey(LateExpressionWork work) {
    return string.Join(
      "\u001f", work.Provider, work.SourceType, work.SourceMember, work.LateIdentity
    );
  }

  private static string BuildFinalDebugState(
    IEnumerable<HixDebugFinalState> states,
    IReadOnlyDictionary<string, object> sharedVariables
  ) {
    return HixDebugRenderer.BuildFinalState(states, sharedVariables);
  }
}
