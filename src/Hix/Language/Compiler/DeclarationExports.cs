using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hix.Compiler;

internal static class DeclarationExports {
  internal static IReadOnlyList<HixIrNode> Expand(IEnumerable<HixIrNode> declarations) {
    var result = new List<HixIrNode>();
    foreach (var declaration in declarations) {
      var metadata = declaration switch {
        FunctionDeclarationIr function => function.Metadata,
        TypeDeclarationIr type => type.Metadata,
        _ => Array.Empty<MetadataIr>()
      };
      var aliasValues = metadata.Where(item => item.Name == "alias").SelectMany(item => item.Values).ToArray();
      if (aliasValues.Any(value => value is not StringExpressionIr)) throw new ArgumentException("export aliases require string names");
      var aliases = aliasValues.OfType<StringExpressionIr>().Select(item => item.Value).ToArray();
      foreach (var alias in aliases)
        if (!Regex.IsMatch(alias, @"\A[a-zA-Z_][a-zA-Z_0-9]*(\.[a-zA-Z_][a-zA-Z_0-9]*)*\z"))
          throw new ArgumentException("invalid export alias '" + alias + "'");
      if (declaration is MixinDeclarationIr mixin) {
        result.Add(new MixinDeclarationIr(mixin.Name, mixin.IsDerivation, Expand(mixin.Declarations.Select(child => new HixIrRewriter().Rewrite(child))),
          mixin.Metadata, mixin.Parameters) {SourceRange = mixin.SourceRange, Tokens = mixin.Tokens, UsingImports = mixin.UsingImports});
        continue;
      }
      if (aliases.Length == 0) { result.Add(declaration); continue; }
      var retained = metadata.Where(item => item.Name != "alias").ToArray();
      if (declaration is FunctionDeclarationIr source) {
        result.Add(Rename(source, source.Name, retained));
        foreach (var alias in aliases) result.Add(Rename(source, alias, retained));
      } else if (declaration is TypeDeclarationIr type) {
        result.Add(new TypeDeclarationIr(type.Name, type.Pattern, retained) {
          SourceRange = type.SourceRange, Tokens = type.Tokens, UsingImports = type.UsingImports});
        foreach (var alias in aliases)
          result.Add(new TypeDeclarationIr(alias, new NamedHixPattern(type.Name), retained) {
            SourceRange = type.SourceRange, Tokens = type.Tokens, UsingImports = type.UsingImports});
      }
    }
    return result;
  }

  internal static FunctionDeclarationIr Rename(FunctionDeclarationIr source, string name,
    IReadOnlyList<MetadataIr> metadata = null) => new(name, source.IsPure, source.IsNoinline,
      source.Signatures, new HixIrRewriter().Rewrite(source.Body),
      (metadata ?? source.Metadata).Select(item => new HixIrRewriter().Rewrite(item)).ToArray(), source.TypeParameters,
      source.IsExternal ? source.ExternalName ?? source.Name : source.ExternalName) {
        SourceRange = source.SourceRange, Tokens = source.Tokens, UsingImports = source.UsingImports
      };
}
