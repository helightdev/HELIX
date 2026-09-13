using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

/// <summary>File-scoped namespace imports retained through compiler rewriting.</summary>
public static class HixUsingImports {
  public static IReadOnlyList<HixUsingImport> Read(IEnumerable<MetadataIr> metadata,
    ICollection<HixParseDiagnostic> diagnostics) {
    var imports = new List<HixUsingImport>();
    foreach (var item in metadata.Where(item => item.Name == "using")) {
      if (item.Values.Count is < 1 or > 2) continue;
      var prefix = (item.Values[0] as StringExpressionIr)?.Value;
      var alias = item.Values.Count == 2 ? (item.Values[1] as StringExpressionIr)?.Value : null;
      if (!ValidName(prefix) || item.Values.Count == 2 && !ValidName(alias)) {
        diagnostics.Add(new(item.Line, "%using requires literal namespace identifiers"));
        continue;
      }
      if (alias != null && imports.Any(import => import.Alias == alias)) {
        diagnostics.Add(new(item.Line, "duplicate using alias '" + alias + "'"));
        continue;
      }
      imports.Add(new(prefix, alias));
    }
    return imports;
  }

  private static bool ValidName(string name) => !string.IsNullOrEmpty(name) && name.Split('.').All(part =>
    part.Length != 0 && (char.IsLetter(part[0]) && part[0] < 128 || part[0] == '_') &&
    part.All(character => character < 128 && (char.IsLetterOrDigit(character) || character == '_')));

  public static IEnumerable<string> Candidates(string name, IReadOnlyList<HixUsingImport> imports) {
    foreach (var import in imports.Where(import => import.Alias != null)) {
      if (name == import.Alias || name.StartsWith(import.Alias + ".", StringComparison.Ordinal)) {
        yield return import.Prefix + name.Substring(import.Alias.Length);
        yield break;
      }
    }
    foreach (var import in imports.Where(import => import.Alias == null)) yield return import.Prefix + "." + name;
    yield return name;
  }
}
