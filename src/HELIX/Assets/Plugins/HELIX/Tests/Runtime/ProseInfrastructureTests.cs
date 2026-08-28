using System.Collections.Generic;
using HELIX.Prose;
using NUnit.Framework;

namespace HELIX.Tests {
  public sealed class ProseInfrastructureTests {
    private sealed class StringReducer : ProseReducer<string> {
      public override bool IsEmpty(string value) => string.IsNullOrEmpty(value);
      public override bool TryMap<TProse>(
        TProse prose, IReadOnlyList<IProseModifier> modifiers, out string result
      ) {
        result = prose is null ? null : prose.ToString();
        return result != null;
      }
      public override bool TryMap(string text, IReadOnlyList<IProseModifier> modifiers, out string result) {
        result = text;
        return text != null;
      }
      public override bool TryMap<T>(
        T value, IDatatype<T> datatype, IReadOnlyList<IProseModifier> modifiers, out string result
      ) {
        result = value is null ? null : value.ToString();
        return result != null;
      }
      public override string Reduce(IReadOnlyList<string> children) => string.Join("", children);
    }

    private struct ReusableFrame : IProseFrame {
      public List<int> values;
      public int initializeCount;
      public int disposeCount;

      public void Initialize() {
        values ??= new List<int>();
        initializeCount++;
      }

      public void Dispose() => disposeCount++;
      public void Reset() => values?.Clear();
    }

    [Test]
    public void FrameStack_ReusesStorageAndAllowsReentrantPushAfterPop() {
      var stack = new ProseFrameStack<ReusableFrame>();
      ref var first = ref stack.Push();
      var storage = first.values;
      first.values.Add(1);

      var popped = stack.Pop();
      ref var nested = ref stack.Push();
      nested.values.Add(2);
      var nestedPopped = stack.Pop();
      stack.Release(ref nestedPopped);
      stack.Release(ref popped);

      ref var reused = ref stack.Push();
      Assert.That(reused.values, Is.SameAs(storage));
      Assert.That(reused.values, Is.Empty);
      Assert.That(reused.initializeCount, Is.EqualTo(2));
      Assert.That(reused.disposeCount, Is.EqualTo(1));
    }

    [Test]
    public void ReducingWriter_CanBeConfiguredWithoutInheritance() {
      var writer = new ReducingProseWriter<string>(new StringReducer());
      using (writer.Paragraph()) {
        writer.Write("value=");
        writer.Write(12, Datatypes.Int);
      }

      Assert.That(writer.Build(), Is.EqualTo("value=12"));
    }

    [Test]
    public void ReducingWriter_DelegatesWholeScopeAndResumesOwner() {
      var reducer = new StringReducer();
      var delegates = new ProseReducerChain<string>()
        .Delegate<ProseSection, ReducingProseWriter<string>>(
          _ => new ReducingProseWriter<string>(reducer),
          delegated => delegated.Build()
        );
      var writer = new ReducingProseWriter<string>(reducer, delegates);

      writer.Write("before-");
      using (writer.Section()) {
        writer.Write("inside");
        using (writer.Paragraph()) writer.Write("-nested");
      }
      writer.Write("-after");

      Assert.That(writer.Build(), Is.EqualTo("before-inside-nested-after"));
    }

  }
}
