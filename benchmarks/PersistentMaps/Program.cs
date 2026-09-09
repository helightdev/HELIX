using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Hix.Collections;

// No Roslyn or Hix dependencies: exercises the exact production generic map source.
internal static class Program {
  private static long sink;
  private static void Main(string[] args) {
    Verify();
    Console.WriteLine($"{RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSDescription}; {RuntimeInformation.ProcessArchitecture}");
    Console.WriteLine("Correctness: randomized versions, collisions, deep prefixes, comparer, builder isolation passed.");
    if (args.Contains("--verify-only")) return;
    Console.WriteLine("Median of 5 samples after 2 warmups. ns/op and allocated bytes/op; setup outside timed region.");
    Console.WriteLine("size,key,operation,implementation,ns/op,bytes/op");
    foreach (var size in new[] {1, 4, 8, 16, 32, 128, 1024}) {
      Bench(size, Enumerable.Range(0, size).ToArray(), Enumerable.Range(size, size).ToArray(), EqualityComparer<int>.Default, "int");
      Bench(size, Enumerable.Range(0, size).Select(i => "key-" + i).ToArray(), Enumerable.Range(size, size).Select(i => "key-" + i).ToArray(), StringComparer.Ordinal, "string");
    }
    GC.KeepAlive(sink);
  }
  private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
  private sealed class CollisionComparer(int mode) : IEqualityComparer<int> {
    public bool Equals(int a, int b) => a == b;
    public int GetHashCode(int value) => mode == 0 ? 1 : unchecked(value << 25);
  }
  private static void Verify() {
    foreach (var threshold in new[] {0, 16, int.MaxValue})
      foreach (var comparer in new IEqualityComparer<int>[] {EqualityComparer<int>.Default, new CollisionComparer(0), new CollisionComparer(1)}) {
        var random = new Random(1979);
        var map = new PersistentMap<int, int>(comparer, threshold);
        var expected = new Dictionary<int, int>(comparer);
        var versions = new List<(PersistentMap<int,int> Map, Dictionary<int,int> Expected)>();
        for (var step = 0; step < 5000; step++) {
          var key = random.Next(100); var value = random.Next();
          if (random.Next(3) == 0) { map = map.Remove(key); expected.Remove(key); }
          else { map = map.SetItem(key, value); expected[key] = value; }
          Require(map.Count == expected.Count, "count");
          foreach (var entry in expected) Require(map.TryGetValue(entry.Key, out var actual) && actual == entry.Value, "lookup");
          Require(map.ToDictionary(e => e.Key, e => e.Value).Count == expected.Count, "enumeration uniqueness");
          if (step % 100 == 0) versions.Add((map, new Dictionary<int,int>(expected, comparer)));
        }
        foreach (var version in versions) {
          Require(version.Map.Count == version.Expected.Count, "old version count mutated");
          foreach (var entry in version.Expected) Require(version.Map[entry.Key] == entry.Value, "old version mutated");
          var fork = version.Map.SetItem(1000, 123).Remove(1000);
          Require(fork.Count == version.Map.Count && !version.Map.ContainsKey(1000), "branch persistence");
        }
        var bulk = new PersistentMap<int,int>.Builder(comparer, threshold);
        foreach (var entry in expected) bulk.SetItem(entry.Key, entry.Value);
        var built = bulk.ToImmutable();
        Require(built.Count == expected.Count, "bulk count");
        foreach (var entry in built) Require(expected[entry.Key] == entry.Value, "bulk enumeration");
        foreach (var entry in expected) Require(built[entry.Key] == entry.Value, "bulk lookup");
        foreach (var key in expected.Keys) map = map.Remove(key);
        Require(map.Count == 0 && !map.Any(), "delete to empty");
      }
    var builder = new PersistentMap<string,int>.Builder(StringComparer.OrdinalIgnoreCase);
    builder.SetItem("A", 1); var before = builder.ToImmutable(); builder.SetItem("a", 2);
    Require(before["a"] == 1 && builder.ToImmutable()["A"] == 2, "builder isolation/comparer");
    Require(ReferenceEquals(before, before.SetItem("a", 1)), "no-op set");
    Require(ReferenceEquals(before, before.Remove("missing")), "no-op removal");
  }
  private static void Bench<TKey>(int size, TKey[] keys, TKey[] missing, IEqualityComparer<TKey> comparer, string keyType) {
    var flat = new PersistentMap<TKey,int>(comparer, int.MaxValue);
    var hamt = new PersistentMap<TKey,int>(comparer, 0);
    var hybrid = new PersistentMap<TKey,int>(comparer);
    var immutable = ImmutableDictionary.Create<TKey,int>(comparer);
    var dictionary = new Dictionary<TKey,int>(comparer);
    for (var i = 0; i < size; i++) {
      flat = flat.SetItem(keys[i], i); hamt = hamt.SetItem(keys[i], i); hybrid = hybrid.SetItem(keys[i], i);
      immutable = immutable.SetItem(keys[i], i); dictionary[keys[i]] = i;
    }
    foreach (var item in new[] {("flat", flat), ("hamt", hamt), ("hybrid", hybrid)}) {
      Measure(size, keyType, "lookup", item.Item1, 50000, () => {
        long sum = 0; for (var i = 0; i < 50000; i++) sum += item.Item2[keys[i % size]]; return sum;
      });
      Measure(size, keyType, "lookup-miss", item.Item1, 50000, () => {
        long found = 0; for (var i = 0; i < 50000; i++) if (item.Item2.TryGetValue(missing[i % size], out _)) found++; return found;
      });
      Measure(size, keyType, "update", item.Item1, 5000, () => {
        var map = item.Item2; for (var i = 0; i < 5000; i++) map = map.SetItem(keys[i % size], i + size); return map.Count;
      });
      Measure(size, keyType, "remove+insert", item.Item1, 5000, () => {
        var map = item.Item2; for (var i = 0; i < 5000; i++) { var key = keys[i % size]; map = map.Remove(key).SetItem(key, i); } return map.Count;
      });
      Measure(size, keyType, "enumerate", item.Item1, 100 * size, () => {
        long sum = 0; for (var i = 0; i < 100; i++) foreach (var entry in item.Item2) sum += entry.Value; return sum;
      });
      Measure(size, keyType, "build", item.Item1, 30 * size, () => {
        long sum = 0; for (var i = 0; i < 30; i++) {
          var b = new PersistentMap<TKey,int>.Builder(comparer, item.Item1 == "flat" ? int.MaxValue : item.Item1 == "hamt" ? 0 : 16);
          for (var j = 0; j < size; j++) b.SetItem(keys[j], j); sum += b.ToImmutable().Count;
        } return sum;
      });
    }
    Measure(size, keyType, "lookup", "ImmutableDictionary", 50000, () => { long sum = 0; for (var i = 0; i < 50000; i++) sum += immutable[keys[i % size]]; return sum; });
    Measure(size, keyType, "lookup-miss", "ImmutableDictionary", 50000, () => { long found = 0; for (var i = 0; i < 50000; i++) if (immutable.TryGetValue(missing[i % size], out _)) found++; return found; });
    Measure(size, keyType, "update", "ImmutableDictionary", 5000, () => { var map = immutable; for (var i = 0; i < 5000; i++) map = map.SetItem(keys[i % size], i + size); return map.Count; });
    Measure(size, keyType, "remove+insert", "ImmutableDictionary", 5000, () => { var map = immutable; for (var i = 0; i < 5000; i++) { var key = keys[i % size]; map = map.Remove(key).SetItem(key, i); } return map.Count; });
    Measure(size, keyType, "enumerate", "ImmutableDictionary", 100 * size, () => { long sum = 0; for (var i = 0; i < 100; i++) foreach (var entry in immutable) sum += entry.Value; return sum; });
    Measure(size, keyType, "build", "ImmutableDictionary", 30 * size, () => { long sum = 0; for (var i = 0; i < 30; i++) { var b = ImmutableDictionary.CreateBuilder<TKey,int>(comparer); for (var j = 0; j < size; j++) b[keys[j]] = j; sum += b.ToImmutable().Count; } return sum; });
    Measure(size, keyType, "lookup", "Dictionary", 50000, () => { long sum = 0; for (var i = 0; i < 50000; i++) sum += dictionary[keys[i % size]]; return sum; });
    Measure(size, keyType, "lookup-miss", "Dictionary", 50000, () => { long found = 0; for (var i = 0; i < 50000; i++) if (dictionary.TryGetValue(missing[i % size], out _)) found++; return found; });
    Measure(size, keyType, "update", "Dictionary-copy", 5000, () => { var map = dictionary; for (var i = 0; i < 5000; i++) { map = new Dictionary<TKey,int>(map, comparer); map[keys[i % size]] = i + size; } return map.Count; });
  }
  private static void Measure(int size, string key, string operation, string implementation, int count, Func<long> action) {
    for (var i = 0; i < 2; i++) sink = action();
    var timings = new double[5]; var allocations = new double[5];
    for (var i = 0; i < 5; i++) {
      GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
      var bytes = GC.GetAllocatedBytesForCurrentThread(); var start = Stopwatch.GetTimestamp();
      sink = action();
      timings[i] = (Stopwatch.GetTimestamp() - start) * 1e9 / Stopwatch.Frequency / count;
      allocations[i] = (GC.GetAllocatedBytesForCurrentThread() - bytes) / (double)count;
    }
    Array.Sort(timings); Array.Sort(allocations);
    Console.WriteLine(FormattableString.Invariant($"{size},{key},{operation},{implementation},{timings[2]:F1},{allocations[2]:F1}"));
  }
}
