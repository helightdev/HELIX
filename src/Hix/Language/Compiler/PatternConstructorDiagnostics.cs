using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

internal static class PatternConstructorDiagnostics {
  public static string Describe(string name, IReadOnlyList<string> argumentNames,
    IReadOnlyList<HixPattern> supplied, IReadOnlyList<HixPatternField> fields,
    IReadOnlyDictionary<string, HixPattern> patterns) {
    var problems = new List<string>();
    var assigned = new bool[fields.Count];
    var positional = 0; var sawNamed = false;
    for (var index = 0; index < supplied.Count; index++) {
      var argumentName = argumentNames[index];
      int target;
      if (argumentName == null) {
        if (sawNamed) { problems.Add("positional arguments cannot follow named arguments"); continue; }
        if (positional >= fields.Count) { problems.Add("too many positional arguments"); continue; }
        target = positional++;
      } else {
        sawNamed = true;
        target = fields.ToList().FindIndex(field => field.Name == argumentName);
        if (target < 0) { problems.Add("unknown argument '" + argumentName + "'"); continue; }
      }
      if (assigned[target]) { problems.Add("argument '" + fields[target].Name + "' is supplied more than once"); continue; }
      assigned[target] = true;
      if (HixPatternRelations.Relate(supplied[index], fields[target].Pattern, patterns) == HixPatternRelation.Never)
        problems.Add("argument '" + fields[target].Name + "' expects " + fields[target].Pattern.Display +
          ", found " + supplied[index].Display);
    }
    var missing = fields.Select((field, index) => (field, index))
      .Where(item => !assigned[item.index] && !item.field.AllowsMissing).Select(item => item.field.Name).ToArray();
    if (missing.Length != 0) problems.Add("missing required argument" + (missing.Length == 1 ? " " : "s ") +
      string.Join(", ", missing.Select(value => "'" + value + "'")));
    var signature = name + "(" + string.Join(", ", fields.Select(field => field.Name + ": " +
      field.Pattern.Display + (field.AllowsMissing ? "?" : ""))) + ")";
    return "no constructor of '" + name + "' accepts the supplied arguments" +
      (problems.Count == 0 ? "" : ": " + string.Join("; ", problems.Distinct())) +
      ". Available signature: " + signature;
  }
}
