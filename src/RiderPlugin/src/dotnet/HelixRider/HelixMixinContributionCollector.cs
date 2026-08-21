#if RIDER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Application.Parts;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.Roslyn.Host.Core;
using JetBrains.Roslyn.Host.Facade;
using JetBrains.Roslyn.Host.Integration.ProjectModel;
using JetBrains.Roslyn.Host.Models;
using JetBrains.Util;

namespace HelixRider;

[RoslynComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public sealed class HelixMixinContributionCollector
{
    private readonly Lifetime _lifetime;
    private readonly ISolution _solution;
    private readonly RoslynModel _roslynModel;
    private readonly IRoslynProjectsFacade _projectsFacade;
    private readonly RoslynTargetFrameworkCache _targetFrameworkCache;

    public HelixMixinContributionCollector(Lifetime lifetime, ISolution solution, RoslynModel roslynModel,
        IRoslynProjectsFacade projectsFacade, RoslynTargetFrameworkCache targetFrameworkCache,
        HelixMixinContributionCache cache)
    {
        _lifetime = lifetime;
        _solution = solution;
        _roslynModel = roslynModel;
        _projectsFacade = projectsFacade;
        _targetFrameworkCache = targetFrameworkCache;
        cache.RegisterCollector(CollectAsync);
    }

    private async Task<IReadOnlyList<HelixMixinContribution>> CollectAsync(string filePath, string sourceText)
    {
        var lineMap = new FileLineMap(sourceText);
        var path = VirtualFileSystemPath.Parse(filePath, InteractionContext.SolutionContext);
        var projectFiles = _solution.FindProjectItemsByLocation(path).OfType<IProjectFile>().ToList();
        var contributions = new HashSet<HelixMixinContribution>();
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
                var request = new RdGetHighlighterArgs(lineMap.LineCount, projectId,
                    _targetFrameworkCache.GetOrCreate(targetFramework), filePath, AnalyzersKind.Custom, false, 0);
                var highlighters = await _roslynModel.Analyzers.Value.GetFileHighlighters
                    .Start(_lifetime, request).AsTask();
                foreach (var diagnostic in highlighters.Select(item => item.Diagnostic))
                {
                    if (diagnostic?.Key?.DiagnosticId != "HLXM14")
                        continue;
                    if (TryParse(diagnostic.Key.Message, lineMap.GetOffset(diagnostic.Key.StartCoords), out var item))
                        contributions.Add(item);
                }
            }
        }
        return contributions.OrderBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Method, StringComparer.Ordinal).ThenBy(item => item.Priority)
            .ThenBy(item => item.Mixin, StringComparer.Ordinal).ToList();
    }

    private static bool TryParse(string message, int offset, out HelixMixinContribution contribution)
    {
        contribution = null;
        var parts = message?.Split(new[] { '|' }, 8);
        if (parts == null || parts.Length != 8 ||
            !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var priority) ||
            !int.TryParse(parts[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parameterCount))
            return false;
        contribution = new HelixMixinContribution(offset, parts[0], parts[1], parts[2], priority,
            parts[4], parts[5], parts[6], parameterCount);
        return true;
    }

    private sealed class FileLineMap
    {
        private readonly int[] _starts;
        public FileLineMap(string text)
        {
            var starts = new List<int> { 0 };
            for (var index = 0; index < text.Length; index++)
                if (text[index] == '\n') starts.Add(index + 1);
            _starts = starts.ToArray();
        }
        public int LineCount => _starts.Length;
        public int GetOffset(RdCoords coordinates) => coordinates == null || coordinates.Line < 0 ||
            coordinates.Line >= LineCount ? 0 : _starts[coordinates.Line] + Math.Max(0, coordinates.Column);
    }
}
#endif
