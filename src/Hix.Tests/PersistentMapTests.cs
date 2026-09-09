using System;
using System.Collections.Generic;
using System.Linq;
using Hix;
using Hix.Collections;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class PersistentMapTests {
  private sealed class HashComparer(int mode) : IEqualityComparer<int> {
    public bool Equals(int x, int y) => x == y;
    public int GetHashCode(int value) => mode == 0 ? value : mode == 1 ? 7 : unchecked(value << 25);
  }

  [Theory]
  [InlineData(0, 0)] [InlineData(0, 1)] [InlineData(0, 2)]
  [InlineData(16, 0)] [InlineData(16, 1)] [InlineData(16, 2)]
  [InlineData(1000, 0)] [InlineData(1000, 1)] [InlineData(1000, 2)]
  public void RandomUpdatesRetainOldVersionsAndMatchDictionary(int threshold, int hashMode) {
    var comparer = new HashComparer(hashMode);
    var expected = new Dictionary<int,int>();
    var map = new PersistentMap<int,int>(comparer, threshold);
    var versions = new List<(PersistentMap<int,int>, Dictionary<int,int>)>();
    var random = new Random(427);
    for (var i = 0; i < 2000; i++) {
      var key = random.Next(100);
      if (random.Next(3) == 0) { map = map.Remove(key); expected.Remove(key); }
      else { map = map.SetItem(key, i); expected[key] = i; }
      Assert.Equal(expected.Count, map.Count);
      foreach (var pair in expected) Assert.Equal(pair.Value, map[pair.Key]);
      Assert.Equal(expected.OrderBy(pair => pair.Key), map.OrderBy(pair => pair.Key));
      if (i % 100 == 0) versions.Add((map, new Dictionary<int,int>(expected)));
    }
    foreach (var (version, snapshot) in versions) {
      Assert.Equal(snapshot.OrderBy(pair => pair.Key), version.OrderBy(pair => pair.Key));
      Assert.Equal(version.Count, version.SetItem(1000, 1).Remove(1000).Count);
      Assert.False(version.ContainsKey(1000));
    }
    var builder = new PersistentMap<int,int>.Builder(comparer, threshold);
    foreach (var pair in expected) builder.SetItem(pair.Key, pair.Value);
    var built = builder.ToImmutable();
    Assert.Equal(expected.OrderBy(pair => pair.Key), built.OrderBy(pair => pair.Key));
    foreach (var pair in expected) Assert.Equal(pair.Value, built[pair.Key]);
    foreach (var key in expected.Keys) built = built.Remove(key);
    Assert.Empty(built);
  }

  [Fact]
  public void PromotionBuilderIsolationAndNoOpUpdates() {
    var map = new PersistentMap<string,int>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < 16; i++) map = map.SetItem("key" + i, i);
    Assert.True(map.IsFlat);
    var large = map.SetItem("extra", 1);
    Assert.False(large.IsFlat);
    Assert.Same(large, large.SetItem("EXTRA", 1));
    Assert.Same(large, large.Remove("absent"));
    Assert.False(large.Remove("extra").IsFlat);
    Assert.Equal(16, map.Count);
    var builder = new PersistentMap<string,int>.Builder(StringComparer.OrdinalIgnoreCase);
    builder.SetItem("key", 1); var first = builder.ToImmutable();
    builder.SetItem("KEY", 2);
    Assert.Equal(1, first["KEY"]);
    Assert.Equal(2, builder.ToImmutable()["key"]);
  }

  private sealed class Context() : HixExecutionContext(TestBackend.Instance, new HixStringPoolBuilder().Freeze()) {
    protected override IHixValue ResolveHost(HixExpressionRoot root, HixString member) => NullHixValue.Instance;
  }

  [Fact]
  public void TablesOwnInputAndNormalizePooledKeysAcrossRepresentations() {
    var poolBuilder = new HixStringPoolBuilder();
    var key = poolBuilder.Intern("name"); var pool = poolBuilder.Freeze();
    var input = new[] {new KeyValuePair<HixString,IHixValue>(key, new NumberHixValue(1))};
    var small = new HixTableValue(input, pool);
    input[0] = new(key, new NumberHixValue(99));
    var context = new Context();
    Assert.Equal(new NumberHixValue(1), small.Select(context, HixString.Dynamic("name")));
    var large = small;
    for (var i = 0; i < 100; i++) large = large.Put(context, HixString.Dynamic("key" + i), new NumberHixValue(i));
    var changed = large.Put(context, HixString.Dynamic("name"), new NumberHixValue(2));
    Assert.Equal(new NumberHixValue(1), large.Select(context, HixString.Dynamic("name")));
    Assert.Equal(new NumberHixValue(2), changed.Select(context, HixString.Dynamic("name")));
    Assert.Equal(new NumberHixValue(1), small.Select(context, HixString.Dynamic("name")));
    Assert.Equal(100, changed.Remove(context, HixString.Dynamic("name")).Count);
  }
}
