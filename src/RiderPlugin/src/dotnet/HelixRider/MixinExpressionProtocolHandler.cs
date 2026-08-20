#if RIDER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HelixRider.Protocol;
using JetBrains.Application.Parts;
using JetBrains.DocumentModel;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Protocol;
using JetBrains.Roslyn.Host.Core;
using JetBrains.Roslyn.Host.Facade;
using JetBrains.Roslyn.Host.Integration.ProjectModel;
using JetBrains.Roslyn.Host.Models;
using JetBrains.Util;

namespace HelixRider
{
    /// <summary>
    /// Reads final mixin contributions from the source generator's hidden Roslyn diagnostics.
    /// </summary>
    [RoslynComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
    public sealed class MixinExpressionProtocolHandler
    {
        private readonly ISolution _solution;
        private readonly RoslynModel _roslynModel;
        private readonly IRoslynProjectsFacade _projectsFacade;
        private readonly RoslynTargetFrameworkCache _targetFrameworkCache;

        public MixinExpressionProtocolHandler(
            Lifetime lifetime,
            ISolution solution,
            RoslynModel roslynModel,
            IRoslynProjectsFacade projectsFacade,
            RoslynTargetFrameworkCache targetFrameworkCache)
        {
            _solution = solution;
            _roslynModel = roslynModel;
            _projectsFacade = projectsFacade;
            _targetFrameworkCache = targetFrameworkCache;
            var model = solution.GetProtocolSolution().GetHelixExpressionModel();
            model.GetMixinContributions.SetAsync(CollectContributionsAsync);
        }

        private async Task<MixinContributionsResponse> CollectContributionsAsync(
            Lifetime lifetime,
            MixinExpressionRequest request)
        {
            if (!File.Exists(request.FilePath))
                return new MixinContributionsResponse(Array.Empty<MixinContribution>());
            // The frontend document can contain unsaved edits. Mapping Roslyn's in-memory
            // diagnostics against the file on disk produces stale or invalid gutter offsets.
            var lineMap = new FileLineMap(request.SourceText);
            var path = VirtualFileSystemPath.Parse(request.FilePath, InteractionContext.SolutionContext);
            var projectFiles = _solution.FindProjectItemsByLocation(path).OfType<IProjectFile>().ToList();
            var contributions = new HashSet<MixinContribution>();
            foreach (var projectFile in projectFiles)
            {
                var project = projectFile.GetProject();
                if (!_projectsFacade.IsProjectSyncedWithWorker(project))
                    continue;
                var projectId = _projectsFacade.GetProjectId(project);
                if (projectId == null)
                    continue;
                foreach (var targetFramework in project.TargetFrameworkIds)
                {
                    var requestData = new RdGetHighlighterArgs(
                        lineMap.LineCount,
                        projectId,
                        _targetFrameworkCache.GetOrCreate(targetFramework),
                        request.FilePath,
                        AnalyzersKind.Custom,
                        false,
                        0);
                    var highlighters = await _roslynModel.Analyzers.Value.GetFileHighlighters
                        .Start(lifetime, requestData).AsTask();
                    foreach (var highlighter in highlighters)
                    {
                        var diagnostic = highlighter.Diagnostic;
                        if (diagnostic?.Key?.DiagnosticId != "HLXM14")
                            continue;
                        var offset = lineMap.GetOffset(diagnostic.Key.StartCoords);
                        if (TryParseContribution(diagnostic.Key.Message, offset, out var contribution))
                            contributions.Add(contribution);
                    }
                }
            }

            return new MixinContributionsResponse(contributions
                .OrderBy(item => item.Target, StringComparer.Ordinal)
                .ThenBy(item => item.Method, StringComparer.Ordinal)
                .ThenBy(item => item.Priority)
                .ThenBy(item => item.Mixin, StringComparer.Ordinal)
                .ToArray());
        }

        private static bool TryParseContribution(
            string message, int offset, out MixinContribution contribution)
        {
            contribution = null;
            if (message == null)
                return false;
            var parts = message.Split(new[] { '|' }, 8);
            if (parts.Length != 8 || !int.TryParse(
                    parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var priority) ||
                !int.TryParse(parts[7], NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var sourceParameterCount))
                return false;
            contribution = new MixinContribution(
                offset, parts[0], parts[1], parts[2], priority,
                parts[4], parts[5], parts[6], sourceParameterCount);
            return true;
        }

        private sealed class FileLineMap
        {
            private readonly int[] _lineStarts;

            public FileLineMap(string text)
            {
                var starts = new List<int> { 0 };
                for (var index = 0; index < text.Length; index++)
                    if (text[index] == '\n')
                        starts.Add(index + 1);
                _lineStarts = starts.ToArray();
            }

            public int LineCount => _lineStarts.Length;

            public int GetOffset(RdCoords coordinates)
            {
                if (coordinates == null || coordinates.Line < 0 || coordinates.Line >= LineCount)
                    return 0;
                return _lineStarts[coordinates.Line] + Math.Max(0, coordinates.Column);
            }
        }
    }
}
#endif
