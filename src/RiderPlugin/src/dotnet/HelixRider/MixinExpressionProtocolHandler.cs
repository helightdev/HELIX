#if RIDER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelixRider.Protocol;
using JetBrains.Application.Parts;
using JetBrains.Lifetimes;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Protocol;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace HelixRider
{
    /// <summary>
    /// Resolves HELIX expression arguments in the ReSharper backend.  The frontend deliberately
    /// does not try to understand C# attributes: aliases, qualified names and malformed code are
    /// all handled by the C# PSI here.
    /// </summary>
    [SolutionComponent(Instantiation.ContainerAsyncAnyThreadSafe)]
    public sealed class MixinExpressionProtocolHandler
    {
        private static readonly IClrTypeName MixinExpressionAttribute =
            new ClrTypeName("HELIX.MixinExpressionAttribute");
        private static readonly IClrTypeName MixinPrepareGlobalAttribute =
            new ClrTypeName("HELIX.MixinPrepareGlobalAttribute");

        private readonly ISolution _solution;
        private readonly IPsiFiles _psiFiles;

        public MixinExpressionProtocolHandler(Lifetime lifetime, ISolution solution, IPsiFiles psiFiles)
        {
            _solution = solution;
            _psiFiles = psiFiles;
            var model = solution.GetProtocolSolution().GetHelixExpressionModel();
            model.GetMixinExpressionRanges.SetAsync(CollectRangesAsync);
        }

        private async Task<MixinExpressionResponse> CollectRangesAsync(
            Lifetime lifetime,
            MixinExpressionRequest request)
        {
            // Frontend requests are commonly issued while Rider is synchronising an editor
            // document. GetPrimaryPsiFile asserts for dirty PSI, so wait for a committed snapshot
            // and automatically retry if a concurrent edit interrupts the commit.
            return await _psiFiles.CommitWithRetryBackgroundRead(lifetime, () => CollectRanges(request));
        }

        private MixinExpressionResponse CollectRanges(MixinExpressionRequest request)
        {
            var path = VirtualFileSystemPath.Parse(request.FilePath, InteractionContext.SolutionContext);
            // Unity can include one physical script in several generated projects. Some of the
            // corresponding project-model entries don't own a C# PSI file, so selecting the first
            // item makes the result depend on project load order.
            var projectFiles = _solution.FindProjectItemsByLocation(path).OfType<IProjectFile>().ToList();
            var csharpFiles = projectFiles
                .Select(projectFile => projectFile.GetPrimaryPsiFile())
                .OfType<ICSharpFile>()
                .Distinct()
                .ToList();
            if (csharpFiles.Count == 0)
                return new MixinExpressionResponse(projectFiles.Count != 0, false, 0, 0,
                    Array.Empty<MixinExpressionRange>());

            // A Unity source file can have several C# PSI views, one for each generated project
            // context. References (including the HELIX meta-attributes) are not necessarily
            // resolvable in the first view. Prefer the view that resolves the most target
            // attributes; use its ranges so offsets aren't duplicated across contexts.
            var best = csharpFiles
                .Select(CollectRanges)
                .OrderByDescending(result => result.MatchedAttributeCount)
                .ThenByDescending(result => result.AttributeCount)
                .First();
            return new MixinExpressionResponse(true, true, best.AttributeCount,
                best.MatchedAttributeCount, best.Ranges.ToArray());
        }

        private static CollectedRanges CollectRanges(ICSharpFile file)
        {
            var ranges = new List<MixinExpressionRange>();
            var attributeCount = 0;
            var matchedAttributeCount = 0;
            foreach (var attribute in file.Descendants<IAttribute>())
            {
                attributeCount++;
                var typeElement = attribute.TypeReference?.Resolve().DeclaredElement as ITypeElement;
                if (typeElement == null)
                    continue;

                var typeName = typeElement.GetClrName();
                var arguments = attribute.Arguments;
                if (Equals(typeName, MixinPrepareGlobalAttribute))
                {
                    matchedAttributeCount++;
                    AddLiteralRange(arguments.FirstOrDefault()?.Value, ranges);
                }
                else if (Equals(typeName, MixinExpressionAttribute))
                {
                    matchedAttributeCount++;
                    // Every supported overload names its expression parameter "expression" and
                    // keeps it last. Named arguments are selected explicitly before that fallback.
                    var expressionArgument = arguments.FirstOrDefault(argument =>
                        string.Equals(argument.NameIdentifier?.Name, "expression", StringComparison.Ordinal))
                        ?? arguments.LastOrDefault();
                    AddLiteralRange(expressionArgument?.Value, ranges);
                }
            }

            return new CollectedRanges(attributeCount, matchedAttributeCount, ranges);
        }

        private sealed class CollectedRanges
        {
            public CollectedRanges(int attributeCount, int matchedAttributeCount,
                List<MixinExpressionRange> ranges)
            {
                AttributeCount = attributeCount;
                MatchedAttributeCount = matchedAttributeCount;
                Ranges = ranges;
            }

            public int AttributeCount { get; }
            public int MatchedAttributeCount { get; }
            public List<MixinExpressionRange> Ranges { get; }
        }

        private static void AddLiteralRange(ICSharpExpression expression, ICollection<MixinExpressionRange> ranges)
        {
            if (!(expression is ICSharpLiteralExpression literal))
                return;

            var textRange = literal.GetDocumentRange().TextRange;
            ranges.Add(new MixinExpressionRange(textRange.StartOffset, textRange.EndOffset));
        }
    }
}
#endif
