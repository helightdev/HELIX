#if RIDER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Application.Parts;
using JetBrains.DocumentModel;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;

namespace HelixRider;

[SolutionComponent(Instantiation.DemandAnyThreadSafe)]
public sealed class HelixMixinContributionCache
{
    private readonly Lifetime _lifetime;
    private readonly ISolution _solution;
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();
    private Func<string, string, Task<IReadOnlyList<HelixMixinContribution>>> _collector;

    public HelixMixinContributionCache(Lifetime lifetime, ISolution solution)
    {
        _lifetime = lifetime;
        _solution = solution;
    }

    public void RegisterCollector(Func<string, string, Task<IReadOnlyList<HelixMixinContribution>>> collector) =>
        _collector = collector;

    public IReadOnlyList<HelixMixinContribution> Request(IPsiSourceFile sourceFile, string sourceText)
    {
        var path = sourceFile.GetLocation().FullPath;
        var entry = _entries.GetOrAdd(path, _ => new CacheEntry());
        var revision = sourceText;
        lock (entry)
        {
            var collector = _collector;
            if (collector != null && entry.RequestedRevision != revision && !entry.RequestInFlight)
            {
                entry.RequestedRevision = revision;
                entry.RequestInFlight = true;
                _ = CompleteAsync(entry, revision, collector(path, sourceText));
            }
            return entry.Contributions;
        }
    }

    private async Task CompleteAsync(CacheEntry entry, string revision,
        Task<IReadOnlyList<HelixMixinContribution>> task)
    {
        IReadOnlyList<HelixMixinContribution> result = null;
        try
        {
            result = await task;
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            // Roslyn can be temporarily unavailable while its worker is restarting.
        }
        var changed = false;
        lock (entry)
        {
            entry.RequestInFlight = false;
            if (result != null && entry.RequestedRevision == revision)
            {
                changed = !entry.Contributions.SequenceEqual(result);
                entry.Contributions = result;
            }
        }
        if (changed && _lifetime.IsAlive)
            _ = _lifetime.StartMainRead(() => DaemonBase.GetInstance(_solution).Invalidate());
    }

    private sealed class CacheEntry
    {
        public string RequestedRevision;
        public bool RequestInFlight;
        public IReadOnlyList<HelixMixinContribution> Contributions = Array.Empty<HelixMixinContribution>();
    }

}
#endif
