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

    public void RegisterCollector(Func<string, string, Task<IReadOnlyList<HelixMixinContribution>>> collector)
    {
        _collector = collector;
        // The visible-file daemon can run before the Roslyn component is composed.
        if (_lifetime.IsAlive)
            _ = _lifetime.StartMainRead(() => DaemonBase.GetInstance(_solution).Invalidate());
    }

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
                _ = CompleteAsync(entry, revision, collector, path, sourceText);
            }
            return entry.Contributions;
        }
    }

    public void Invalidate(IPsiSourceFile sourceFile)
    {
        var path = sourceFile.GetLocation().FullPath;
        if (!_entries.TryGetValue(path, out var entry))
            return;
        lock (entry)
        {
            entry.Generation++;
            entry.RequestedRevision = null;
        }
    }

    private async Task CompleteAsync(CacheEntry entry, string revision,
        Func<string, string, Task<IReadOnlyList<HelixMixinContribution>>> collector,
        string path, string sourceText)
    {
        IReadOnlyList<HelixMixinContribution> result = null;
        int generation;
        lock (entry)
            generation = entry.Generation;
        try
        {
            result = await collector(path, sourceText);
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            // Roslyn can be temporarily unavailable while its worker is restarting.
        }
        var changed = false;
        var invalidatedWhileCollecting = false;
        lock (entry)
        {
            entry.RequestInFlight = false;
            invalidatedWhileCollecting = entry.Generation != generation;
            if (result != null && !invalidatedWhileCollecting && entry.RequestedRevision == revision)
            {
                changed = !entry.Contributions.SequenceEqual(result);
                entry.Contributions = result;
            }
        }
        if ((changed || invalidatedWhileCollecting) && _lifetime.IsAlive)
            _ = _lifetime.StartMainRead(() => DaemonBase.GetInstance(_solution).Invalidate());
    }

    private sealed class CacheEntry
    {
        public string RequestedRevision;
        public bool RequestInFlight;
        public int Generation;
        public IReadOnlyList<HelixMixinContribution> Contributions = Array.Empty<HelixMixinContribution>();
    }

}
#endif
