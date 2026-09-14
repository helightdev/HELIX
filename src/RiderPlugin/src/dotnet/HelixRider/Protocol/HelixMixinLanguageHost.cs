#if RIDER
using System;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Application.Parts;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Feature.Services.Protocol;
using JetBrains.ReSharper.Resources.Shell;
using WireRange = HelixRider.Protocol.MixinSourceRange;
namespace HelixRider.Protocol;

/// Only C# host metadata and symbol lookup cross Rider's backend channel.
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadSafe)]
public sealed class HelixMixinLanguageHost {
    private readonly ISymbolCache _symbolCache;
    public HelixMixinLanguageHost(ISolution solution, ISymbolCache symbolCache) {
        _symbolCache = symbolCache;
        var model = solution.GetProtocolSolution().GetHelixExpressionModel();
        model.CompleteCSharpTypes.SetAsync((_, prefix) => RdTask.Successful(new MixinCompletionResponse(CompleteTypes(prefix))));
        model.ResolveCSharpTypes.SetAsync((_, names) => RdTask.Successful(new MixinCompletionResponse(names.Select(ResolveType).Where(x => x != null).ToArray())));
        model.GetCSharpHostSchema.SetAsync((_, ignored) => RdTask.Successful(ExportCSharpHostSchema()));
    }
    internal static string ExportCSharpHostSchema() => new InteropBackend().Export();
    private static object HostDefinition(string name, string kind, string resultType, string documentation) => new {
        name, kind, argumentCount = 0, variadic = false, operandType = "None", receiverType = "None", resultType,
        argumentTypes = Array.Empty<string>(), documentation
    };
    private sealed class InteropBackend : Hix.HixMixinBackend {
        public string Export() {
            var builder = new Hix.FunctionSignatureRegistryBuilder();
            RegisterFunctions(builder);
            return Newtonsoft.Json.JsonConvert.SerializeObject(new {
                additionalDefinitions = Hix.HixRootLibrary.Enumerate().Select(x => HostDefinition(x.Name, "Root", x.Kind.ToString(), x.Documentation))
                    .Concat(Enum.GetNames(typeof(Hix.Mixins.MixinEmissionTarget)).Select(x => HostDefinition(x, "OutputTarget", "None", "Generated output destination"))),
                libraries = builder.Libraries,
                handlers = builder.Handlers.Select(x => x.Name),
                opaqueTypes = builder.OpaqueTypes.Select(x => new { x.Identity, x.Name, x.Arity, x.Documentation }),
                roots = Roots.Values.Select(x => new { x.Name, kind = x.Kind.ToString(), x.RequiresPrelude, x.HasEffects, opaqueType = (x.Pattern as Hix.OpaqueHixPattern)?.Definition.Identity })
            });
        }
    }
    private MixinCompletionItem[] CompleteTypes(string prefix)
    {
        return ReadLockCookie.Execute(() =>
        {
            var normalizedPrefix = (prefix ?? string.Empty).Trim().Replace("global::", string.Empty);
            var shortPrefix = normalizedPrefix.Substring(normalizedPrefix.LastIndexOf('.') + 1);
            var scope = _symbolCache.GetSymbolScope(LibrarySymbolScope.FULL, true);
            return scope.GetAllShortNames()
                .Where(name => name.StartsWith(shortPrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Take(256)
                .SelectMany(name => scope.GetElementsByShortName(name).OfType<ITypeElement>())
                .Select(type => type.GetClrName().FullName.Replace('+', '.'))
                .Where(name => string.IsNullOrEmpty(normalizedPrefix) ||
                               name.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase) ||
                               name.Substring(name.LastIndexOf('.') + 1)
                                   .StartsWith(shortPrefix, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Take(128)
                .Select(name => new MixinCompletionItem(name.Substring(name.LastIndexOf('.') + 1), name,
                    "CSharpType", name, string.Empty, Range(0, 0)))
                .ToArray();
        });
    }

    private MixinCompletionItem ResolveType(string name)
    {
        return ReadLockCookie.Execute(() =>
        {
            var normalized = (name ?? string.Empty).Trim().Replace("global::", string.Empty);
            if (string.IsNullOrEmpty(normalized)) return null;
            var shortName = normalized.Substring(normalized.LastIndexOf('.') + 1);
            var scope = _symbolCache.GetSymbolScope(LibrarySymbolScope.FULL, true);
            var type = scope.GetElementsByShortName(shortName).OfType<ITypeElement>()
                .FirstOrDefault(candidate => string.Equals(
                    candidate.GetClrName().FullName.Replace('+', '.'), normalized,
                    StringComparison.Ordinal));
            var declaration = type?.GetDeclarations().FirstOrDefault();
            var sourceFile = declaration?.GetSourceFile();
            var range = declaration?.GetNavigationRange();
            if (sourceFile == null || range == null || !range.Value.IsValid()) return null;
            var textRange = range.Value.TextRange;
            return new MixinCompletionItem(name, name, "CSharpType", name, sourceFile.GetLocation().FullPath, new WireRange(textRange.StartOffset, textRange.EndOffset));
        });
    }

    private static WireRange Range(int start, int end) => new(start, end);
}
#endif
