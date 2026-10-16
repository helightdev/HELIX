using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Helix.Weave.Lowering;

namespace Helix.Weave.Tests;

/// <summary>
/// Runs the whole pipeline the way the build will: lower the authoring sources, compile the
/// lowered output into a generator assembly, then run that generator over a sample compilation.
/// </summary>
public static class Harness {
  public static IReadOnlyList<MetadataReference> References { get; } = BuildReferences();

  private static IReadOnlyList<MetadataReference> BuildReferences() {
    var runtime = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    var paths = new[] {
      typeof(object).Assembly.Location,
      Path.Combine(runtime, "System.Runtime.dll"),
      Path.Combine(runtime, "System.Collections.dll"),
      Path.Combine(runtime, "System.Linq.dll"),
      Path.Combine(runtime, "netstandard.dll"),
      typeof(System.Collections.Immutable.ImmutableArray).Assembly.Location,
      typeof(SyntaxTree).Assembly.Location,
      typeof(CSharpSyntaxTree).Assembly.Location,
      typeof(Mixin).Assembly.Location,
    };
    return paths.Where(File.Exists).Distinct()
      .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToList();
  }

  public static string FixturesDirectory =>
    Path.Combine(AppContext.BaseDirectory, "Fixtures");

  public static LoweringResult LowerFixtures() => Lowerer.LowerDirectory(FixturesDirectory, References);

  public static LoweringResult Lower(params (string Path, string Text)[] sources) =>
    Lowerer.Lower(sources, References);

  /// <summary>Compiles lowered output into an in-memory assembly and returns its generator.</summary>
  public static IIncrementalGenerator CompileGenerator(LoweringResult lowered) {
    var trees = lowered.Files
      .OrderBy(file => file.Key, StringComparer.Ordinal)
      .Select(file => CSharpSyntaxTree.ParseText(file.Value, new CSharpParseOptions(LanguageVersion.Preview),
        path: file.Key))
      .ToList();

    var compilation = CSharpCompilation.Create("Helix.Weave.GeneratedTest", trees, References,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    using var stream = new MemoryStream();
    var result = compilation.Emit(stream);
    if (!result.Success) {
      var errors = string.Join("\n", result.Diagnostics
        .Where(d => d.Severity == DiagnosticSeverity.Error)
        .Select(d => d.ToString()));
      throw new InvalidOperationException("lowered output does not compile:\n" + errors);
    }

    var assembly = Assembly.Load(stream.ToArray());
    var type = assembly.GetType("Helix.Weave.Generated.WeaveGenerator")
               ?? throw new InvalidOperationException("no WeaveGenerator in lowered output");
    return (IIncrementalGenerator)Activator.CreateInstance(type)!;
  }

  /// <summary>
  /// The runtime side of the duplication: what user code actually annotates with and emits
  /// against. The mixin project declares the same attributes separately, which is what the
  /// metadata-name match in the registry binds together.
  /// </summary>
  public const string RuntimeSource = """
    using System;

    namespace Fixtures {
      [AttributeUsage(AttributeTargets.Method)]
      public sealed class EventHandlerAttribute : Attribute {
        public EventHandlerAttribute(int priority = 0) { }
        public int Priority { get; init; }
      }

      [AttributeUsage(AttributeTargets.Method)]
      public sealed class CommandAttribute : Attribute {
        public CommandAttribute(string name = null) { }
        public string Name { get; init; }
        public string Description { get; init; }
      }

      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
      public sealed class StructureAttribute : Attribute {
        public bool Equality { get; init; } = true;
      }

      [AttributeUsage(AttributeTargets.Field)]
      public sealed class PropAttribute : Attribute {
        public PropAttribute(string defaultValue = null) { }
        public string DefaultValue { get; init; }
        public bool Equatable { get; init; } = true;
      }

      public class Evt { }
      public interface IAsyncEvt { }
      public interface IEventListener { EventHandlerList HandlerList { get; } }

      public sealed class EventHandlerList {
        public void Register<T>(object handler, int priority) { }
        public void RegisterAsync<T>(object handler, int priority) { }
        public void UnregisterAll() { }
      }

      public sealed class Registry {
        public void Add(string name, string description, Type[] parameters, Action<object> invoke) { }
      }

      public delegate void RegistrationConfigurator(Registry registry);
    }
    """;

  /// <summary>Runs a generator over sample user code and returns what it produced.</summary>
  public static GeneratorRun Run(IIncrementalGenerator generator, params string[] sources) {
    sources = new[] { RuntimeSource }.Concat(sources).ToArray();
    var trees = sources.Select((text, index) =>
      CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Preview), path: $"user{index}.cs"));

    var compilation = CSharpCompilation.Create("UserCode", trees, References,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    // The driver parses generated sources too; its options must match the compilation's.
    var driver = CSharpGeneratorDriver
      .Create(new[] { generator.AsSourceGenerator() }, parseOptions: new CSharpParseOptions(LanguageVersion.Preview))
      .RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var diagnostics);

    var produced = driver.GetRunResult().Results
      .SelectMany(result => result.GeneratedSources)
      .ToDictionary(source => source.HintName, source => source.SourceText.ToString(), StringComparer.Ordinal);

    return new GeneratorRun(produced, diagnostics, updated.GetDiagnostics()
      .Where(d => d.Severity == DiagnosticSeverity.Error).ToList());
  }

  public static GeneratorRun RunFixtures(params string[] sources) {
    var lowered = LowerFixtures();
    if (!lowered.Success)
      throw new InvalidOperationException("lowering failed:\n" + string.Join("\n", lowered.Errors));
    return Run(CompileGenerator(lowered), sources);
  }
}

public sealed record GeneratorRun(
  IReadOnlyDictionary<string, string> Sources,
  IReadOnlyList<Diagnostic> GeneratorDiagnostics,
  IReadOnlyList<Diagnostic> CompilationErrors) {
  public string Single() => Sources.Values.Single();
  public string All() => string.Join("\n", Sources.OrderBy(s => s.Key, StringComparer.Ordinal).Select(s => s.Value));
}
