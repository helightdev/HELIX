using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Mixins;
using Mixins.Compiler;
using Mixins.Diagnostics;
using Mixins.Env;
using Mixins.Roslyn;
using Mixins.Runtime;

using static Mixins.Roslyn.GeneratorAnalysis;
using static Mixins.Roslyn.GeneratorDiagnostics.Mixins;
using static Mixins.Roslyn.GeneratorSource;
using static Mixins.Roslyn.GeneratorStrings;
using ExecutionContext = Mixins.Runtime.ExecutionContext;

namespace HelixSourceGenerator.Generators;

public sealed partial class MixinGenerator {
  private sealed record AnnotatedSymbolData(ISymbol Symbol, IReadOnlyList<AttributeData> Attributes);

  private sealed class MixinGenerationContext {
    private readonly List<MixinDebugExpression> _debugExpressions = [];
    private readonly List<MixinDiagnostic> _diagnostics = [];
    private readonly List<LateExpressionWork> _lateExpressions = [];

    internal MixinGenerationContext(bool debug, bool debugStringPool) {
      Debug = debug;
      DebugStringPool = debugStringPool;
    }

    internal bool Debug { get; }
    internal bool DebugStringPool { get; }
    internal bool HasLateExpressions => _lateExpressions.Count != 0;
    internal IReadOnlyList<LateExpressionWork> LateExpressions => _lateExpressions;
    internal IReadOnlyList<MixinDebugExpression> DebugExpressions => _debugExpressions;

    internal void ReportDiagnostic(Diagnostic diagnostic) {
      _diagnostics.Add(MixinDiagnostic.Detach(diagnostic));
    }

    internal void AddLateExpression(
      MixinExpressionExecutionProgram program,
      string preludeIdentity,
      string lateIdentity,
      IReadOnlyDictionary<string, object> variables,
      IReadOnlyDictionary<string, object> carries,
      ImmutableArray<LateTarget> targets,
      Location location,
      string provider,
      string sourceType,
      string sourceMember,
      string sourceKind,
      int sourceParameterCount
    ) {
      _lateExpressions.Add(
        new LateExpressionWork(
          program, preludeIdentity, lateIdentity, variables, carries,
          targets,
          MixinDiagnostic.Detach(Diagnostic.Create(ExpressionLog, location, "")),
          provider ?? "", sourceType ?? "",
          sourceMember ?? "", sourceKind ?? "", sourceParameterCount
        )
      );
    }

    internal void AddDebugExpression(
      MixinExpressionExecutionProgram preludeProgram,
      MixinExpressionExecutionProgram lateProgram,
      IReadOnlyDictionary<string, object> variables,
      IReadOnlyDictionary<string, object> carries,
      string provider,
      ISymbol source,
      int preludeOperations,
      double preludeMilliseconds
    ) {
      if (!Debug) return;
      _debugExpressions.Add(
        new MixinDebugExpression(
          preludeProgram, lateProgram, variables.ToImmutableDictionary(StringComparer.Ordinal),
          carries.ToImmutableDictionary(StringComparer.Ordinal),
          provider ?? "", (source as INamedTypeSymbol ?? source.ContainingType)
          ?.ToDisplayString(TypeDisplayFormat) ?? "",
          source is INamedTypeSymbol ? "" : source.MetadataName,
          preludeOperations, preludeMilliseconds
        )
      );
    }

    internal MixinOutputModel Complete(MixinRenderModel render = null) => new(
      render?.Wrapper.HintName, null, render, _lateExpressions.ToImmutableArray(),
      _diagnostics.ToImmutableArray(), ImmutableArray<string>.Empty, ImmutableArray<MixinReportedLog>.Empty
    );
  }

  private sealed record LateExpressionWork(
    MixinExpressionExecutionProgram Program,
    string PreludeIdentity,
    string LateIdentity,
    IReadOnlyDictionary<string, object> Variables,
    IReadOnlyDictionary<string, object> Carries,
    ImmutableArray<LateTarget> Targets,
    MixinDiagnostic Location,
    string Provider,
    string SourceType,
    string SourceMember,
    string SourceKind,
    int SourceParameterCount
  );

  private sealed record LateTarget(
    string DeclaredTarget,
    string EmittedTarget,
    bool IsStatic,
    bool IsPublic,
    string DelegateTarget,
    int Order
  );

  private sealed class MixinRenderModel {
    internal MixinRenderModel(
      DetachedTypeWrapper wrapper,
      ImmutableArray<GeneratedMethod> methods,
      MixinOutputCollection annotations,
      MixinOutputCollection @class,
      MixinOutputCollection file,
      MixinOutputCollection implements,
      MixinOutputCollection usings,
      IEnumerable<KeyValuePair<string, object>> primaryVariables,
      MixinStringPool stringPool,
      IEnumerable<KeyValuePair<MixinString, IMixinValue>> targetVariables,
      ImmutableArray<MixinDebugExpression> debugExpressions,
      bool debug,
      bool debugStringPool
    ) {
      using var profile = MixinProfiler.Measure("model.render.create");
      Wrapper = wrapper;
      Methods = methods;
      Annotations = annotations;
      Class = @class;
      File = file;
      Implements = implements;
      Usings = usings;
      PrimaryVariables = new MixinValueDictionary(
        primaryVariables.Select(item =>
          new KeyValuePair<MixinString, IMixinValue>(
            stringPool.Get(item.Key), RuntimeValue(item.Value, stringPool)
          )
        )
      );
      TargetVariables = new MixinValueDictionary(targetVariables);
      DebugExpressions = debugExpressions;
      Debug = debug;
      DebugStringPool = debugStringPool;
      StringPool = stringPool;
      GenerationVersion = Interlocked.Increment(ref _generationCounter);
      Fingerprint = MixinRenderFingerprint.Create(this);
    }

    internal DetachedTypeWrapper Wrapper { get; }
    internal ImmutableArray<GeneratedMethod> Methods { get; }
    internal MixinOutputCollection Annotations { get; }
    internal MixinOutputCollection Class { get; }
    internal MixinOutputCollection File { get; }
    internal MixinOutputCollection Implements { get; }
    internal MixinOutputCollection Usings { get; }
    internal MixinValueDictionary PrimaryVariables { get; }
    internal MixinValueDictionary TargetVariables { get; }
    internal ImmutableArray<MixinDebugExpression> DebugExpressions { get; }
    internal bool Debug { get; }
    internal bool DebugStringPool { get; }
    internal MixinStringPool StringPool { get; }

    internal long GenerationVersion { get; }
    internal MixinRenderFingerprint Fingerprint { get; }

    private static IMixinValue RuntimeValue(object value, MixinStringPool strings) {
      return value switch {
        IMixinValue typed => typed,
        null => NullMixinValue.Instance,
        bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
        string text => new LiteralMixinValue(strings.Get(text)),
        DetachedSemanticData detached => DetachedSemanticMixinValue.Materialize(detached),
        _ => new ObjectMixinValue(value)
      };
    }
  }

  private sealed record MixinDiagnostic(
    DiagnosticDescriptor Descriptor,
    string Message,
    string Path,
    TextSpan SourceSpan,
    LinePositionSpan LineSpan,
    bool HasLocation,
    ImmutableDictionary<string, string> Properties
  ) {
    internal static MixinDiagnostic Detach(Diagnostic diagnostic) {
      var location = diagnostic.Location;
      if (location == Location.None || !location.IsInSource) {
        return new MixinDiagnostic(
          diagnostic.Descriptor, diagnostic.GetMessage(CultureInfo.InvariantCulture),
          "", default, default, false, diagnostic.Properties
        );
      }
      var lineSpan = location.GetLineSpan();
      return new MixinDiagnostic(
        diagnostic.Descriptor, diagnostic.GetMessage(CultureInfo.InvariantCulture),
        lineSpan.Path ?? "", location.SourceSpan, lineSpan.Span, true, diagnostic.Properties
      );
    }

    internal Diagnostic Create() {
      var descriptor = new DiagnosticDescriptor(
        Descriptor.Id, Descriptor.Title, "{0}", Descriptor.Category,
        Descriptor.DefaultSeverity, Descriptor.IsEnabledByDefault,
        Descriptor.Description, Descriptor.HelpLinkUri, [.. Descriptor.CustomTags]
      );
      var location = HasLocation ? Location.Create(Path, SourceSpan, LineSpan) : Location.None;
      return Diagnostic.Create(descriptor, location, null, Properties, Message);
    }

    internal Diagnostic Create(DiagnosticDescriptor descriptor, string message) {
      var location = HasLocation
        ? Location.Create(Path, SourceSpan, LineSpan)
        : Location.None;
      return Diagnostic.Create(descriptor, location, message);
    }
  }

  private sealed record MixinOutputModel(
    string HintName, string Source, MixinRenderModel Render,
    ImmutableArray<LateExpressionWork> LateExpressions,
    ImmutableArray<MixinDiagnostic> Diagnostics, ImmutableArray<string> Errors,
    ImmutableArray<MixinReportedLog> Logs
  );

  private sealed record MixinReportedLog(MixinExpressionLog Log, MixinDiagnostic Location);

  private readonly struct MixinRenderFingerprint : IEquatable<MixinRenderFingerprint> {
    private MixinRenderFingerprint(
      FingerprintPart outputs,
      FingerprintPart variables,
      FingerprintPart signatures,
      bool debug,
      bool debugStringPool
    ) {
      Outputs = outputs;
      Variables = variables;
      Signatures = signatures;
      Debug = debug;
      DebugStringPool = debugStringPool;
    }

    internal FingerprintPart Outputs { get; }
    internal FingerprintPart Variables { get; }
    internal FingerprintPart Signatures { get; }
    private bool Debug { get; }
    private bool DebugStringPool { get; }

    internal static MixinRenderFingerprint Create(MixinRenderModel render) {
      using var profile = MixinProfiler.Measure("model.render.fingerprint");
      var outputs = new MixinFingerprintBuilder();
      AppendOutputCollection(outputs, render.Annotations);
      AppendOutputCollection(outputs, render.Class);
      AppendOutputCollection(outputs, render.File);
      AppendOutputCollection(outputs, render.Implements);
      AppendOutputCollection(outputs, render.Usings);

      var variables = new MixinFingerprintBuilder();
      var fingerprintContext = new UnlinkedMixinExpressionContext(render.StringPool);
      variables.Append(render.PrimaryVariables.Count);
      foreach (var variable in render.PrimaryVariables
        .OrderBy(item => item.Key.Id)
        .ThenBy(item => item.Key.DynamicValue, StringComparer.Ordinal)) {
        variables.Append(variable.Key.IsInterned);
        variables.Append(variable.Key.Id);
        if (!variable.Key.IsInterned) variables.Append(variable.Key.DynamicValue);
        variable.Value.Fingerprint(variables, fingerprintContext);
      }
      variables.Append(render.TargetVariables.Count);
      foreach (var variable in render.TargetVariables
        .OrderBy(item => item.Key.DynamicValue, StringComparer.Ordinal)) {
        variables.Append(variable.Key.DynamicValue);
        variable.Value.Fingerprint(variables, fingerprintContext);
      }

      var signatures = new MixinFingerprintBuilder();
      signatures.Append(render.Wrapper.NamespaceName);
      signatures.Append(render.Wrapper.HintName);
      signatures.Append(render.Wrapper.Declarations.Count);
      foreach (var declaration in render.Wrapper.Declarations) signatures.Append(declaration);
      signatures.Append(render.Methods.Length);
      foreach (var method in render.Methods) {
        signatures.Append(method.Declaration);
        signatures.Append(method.ReturnType);
        signatures.Append(method.CallBase);
        signatures.Append(method.Name);
        signatures.Append(method.TypeParameters.Count);
        foreach (var parameter in method.TypeParameters) signatures.Append(parameter);
        signatures.Append(method.Constraints.Count);
        foreach (var constraint in method.Constraints) signatures.Append(constraint);
        signatures.Append(method.Parameters.Count);
        foreach (var parameter in method.Parameters) {
          signatures.Append(parameter.Declaration);
          signatures.Append(parameter.Argument);
        }
        outputs.Append(method.Contributions.Count);
        foreach (var contribution in method.Contributions) {
          outputs.Append(contribution.EmittedTarget);
          outputs.Append(contribution.Order);
          AppendOutputs(outputs, contribution.ExpressionResult.Outputs);
        }
      }
      signatures.Append(render.DebugStringPool);
      if (render.DebugStringPool) {
        signatures.Append(render.StringPool.Count);
        for (var id = 0; id < render.StringPool.Count; id++)
          signatures.Append(render.StringPool[id]);
      }
      return new MixinRenderFingerprint(
        new FingerprintPart(outputs.Hash, outputs.Length),
        new FingerprintPart(variables.Hash, variables.Length),
        new FingerprintPart(signatures.Hash, signatures.Length),
        render.Debug, render.DebugStringPool
      );
    }

    private static IMixinValue RuntimeValue(object value, MixinStringPool strings) {
      return value switch {
        IMixinValue typed => typed,
        null => NullMixinValue.Instance,
        bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
        string text => new LiteralMixinValue(strings.Get(text)),
        DetachedSemanticData detached => DetachedSemanticMixinValue.Materialize(detached),
        _ => new ObjectMixinValue(value)
      };
    }

    private static void AppendOutputCollection(
      MixinFingerprintBuilder builder,
      MixinOutputCollection outputs
    ) {
      builder.Append(outputs.Count);
      builder.Append(unchecked((long)outputs.Hash));
      builder.Append(outputs.Length);
    }

    private static void AppendOutputs(
      MixinFingerprintBuilder builder,
      IReadOnlyCollection<MixinExpressionOutput> outputs
    ) {
      builder.Append(outputs.Count);
      foreach (var output in outputs) {
        builder.Append((int)output.Target);
        builder.Append(output.InjectionTarget);
        builder.Append(output.InjectionPriority);
        builder.Append(output.Segments.Count);
        foreach (var segment in output.Segments) {
          builder.Append(segment.IsInterned);
          builder.Append(output.Resolve(segment));
        }
      }
    }

    public bool Equals(MixinRenderFingerprint other) {
      return Outputs.Equals(other.Outputs) &&
        Variables.Equals(other.Variables) &&
        Signatures.Equals(other.Signatures) && Debug == other.Debug &&
        DebugStringPool == other.DebugStringPool;
    }

    public override bool Equals(object value) {
      return value is MixinRenderFingerprint other && Equals(other);
    }

    public override int GetHashCode() {
      return unchecked(
        (((((((Outputs.GetHashCode() * 397) ^ Variables.GetHashCode()) * 397) ^
          Signatures.GetHashCode()) * 397) ^ Debug.GetHashCode()) * 397) ^
        DebugStringPool.GetHashCode()
      );
    }

    internal readonly struct FingerprintPart : IEquatable<FingerprintPart> {
      internal FingerprintPart(ulong hash, long length) {
        Hash = hash;
        Length = length;
      }

      internal ulong Hash { get; }
      internal long Length { get; }

      public bool Equals(FingerprintPart other) {
        return Hash == other.Hash && Length == other.Length;
      }

      public override bool Equals(object value) {
        return value is FingerprintPart other && Equals(other);
      }

      public override int GetHashCode() {
        return unchecked(((int)(Hash ^ (Hash >> 32)) * 397) ^ Length.GetHashCode());
      }
    }
  }

  private sealed class MixinOutputModelComparer : IEqualityComparer<MixinOutputModel> {
    internal static readonly MixinOutputModelComparer Instance = new();

    public bool Equals(MixinOutputModel x, MixinOutputModel y) {
      using var profile = MixinProfiler.Measure("comparer.output.equals");
      return ReferenceEquals(x, y) || (x is not null &&
        y is not null &&
        string.Equals(x.HintName, y.HintName, StringComparison.Ordinal) &&
        Nullable.Equals(x.Render?.Fingerprint, y.Render?.Fingerprint) &&
        DiagnosticKey(x.Diagnostics) == DiagnosticKey(y.Diagnostics) &&
        LateEqual(x.LateExpressions, y.LateExpressions));
    }

    public int GetHashCode(MixinOutputModel value) {
      using var profile = MixinProfiler.Measure("comparer.output.hash");
      return unchecked(
        (StringComparer.Ordinal.GetHashCode(value.HintName ?? "") * 397) ^
        (value.Render?.Fingerprint.GetHashCode() ?? 0)
      );
    }

    private static string DiagnosticKey(ImmutableArray<MixinDiagnostic> diagnostics) {
      using var profile = MixinProfiler.Measure("comparer.output.diagnostic_key");
      return string.Join(
        "\u001e", diagnostics.Select(diagnostic => string.Join(
            "\u001f", diagnostic.Descriptor.Id, diagnostic.Descriptor.DefaultSeverity.ToString(), diagnostic.Message,
            diagnostic.Path, diagnostic.SourceSpan.Start.ToString(CultureInfo.InvariantCulture),
            diagnostic.SourceSpan.Length.ToString(CultureInfo.InvariantCulture),
            diagnostic.LineSpan.Start.Line.ToString(CultureInfo.InvariantCulture),
            diagnostic.LineSpan.Start.Character.ToString(CultureInfo.InvariantCulture),
            diagnostic.LineSpan.End.Line.ToString(CultureInfo.InvariantCulture),
            diagnostic.LineSpan.End.Character.ToString(CultureInfo.InvariantCulture), string.Join(
              "\u001d", diagnostic.Properties.OrderBy(item => item.Key).Select(item => item.Key + "=" + item.Value)
            )
          )
        )
      );
    }

    private static bool LateEqual(ImmutableArray<LateExpressionWork> x, ImmutableArray<LateExpressionWork> y) {
      using var profile = MixinProfiler.Measure("comparer.output.late_equal");
      if (x.Length != y.Length) return false;
      for (var index = 0; index < x.Length; index++) {
        var left = x[index];
        var right = y[index];
        if (left.PreludeIdentity != right.PreludeIdentity || left.LateIdentity != right.LateIdentity ||
          left.Variables.Count != right.Variables.Count || left.Carries.Count != right.Carries.Count ||
          left.Provider != right.Provider ||
          left.SourceType != right.SourceType || left.SourceMember != right.SourceMember ||
          left.SourceKind != right.SourceKind ||
          left.SourceParameterCount != right.SourceParameterCount ||
          left.Targets.Length != right.Targets.Length) return false;
        for (var targetIndex = 0; targetIndex < left.Targets.Length; targetIndex++) {
          if (left.Targets[targetIndex] != right.Targets[targetIndex])
            return false;
        }
        foreach (var item in left.Variables) {
          if (!right.Variables.TryGetValue(item.Key, out var value) || !Equals(item.Value, value))
            return false;
        }
        foreach (var item in left.Carries) {
          if (!right.Carries.TryGetValue(item.Key, out var value) || !Equals(item.Value, value))
            return false;
        }
      }
      return true;
    }
  }

  private sealed class UnlinkedMixinExpressionContext : ExecutionContext {
    internal UnlinkedMixinExpressionContext(MixinStringPool strings) : base(strings) { }

    protected override IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member) {
      return Error("late expressions cannot resolve host values");
    }
  }

  private sealed class ExpressionOutputs {
    private MixinOutputAccumulator _annotations, _class, _file, _implements, _usings;
    internal bool Any { get; private set; }

    internal void Add(MixinExpressionOutput output) {
      if (output.IsEmpty) return;
      if (output.Target == MixinEmissionTarget.Class) {
        Any = true;
        (_class ??= new MixinOutputAccumulator()).Add(output);
        return;
      }
      if (output.Target == MixinEmissionTarget.File) {
        Any = true;
        (_file ??= new MixinOutputAccumulator()).Add(output);
        return;
      }
      Any = true;
      switch (output.Target) {
        case MixinEmissionTarget.Extends or MixinEmissionTarget.Implements:
          (_implements ??= new MixinOutputAccumulator()).Add(output); break;
        case MixinEmissionTarget.Annotation:
          (_annotations ??= new MixinOutputAccumulator()).Add(output); break;
        case MixinEmissionTarget.Using:
          (_usings ??= new MixinOutputAccumulator()).Add(output);
          break;
      }
    }

    internal ExpressionOutputSet Finish() {
      return new ExpressionOutputSet(
        MixinOutputCollection.Finish(_annotations),
        MixinOutputCollection.Finish(_class),
        MixinOutputCollection.Finish(_file),
        MixinOutputCollection.Finish(_implements),
        MixinOutputCollection.Finish(_usings)
      );
    }
  }

  private sealed record ExpressionOutputSet(
    MixinOutputCollection Annotations,
    MixinOutputCollection Class,
    MixinOutputCollection File,
    MixinOutputCollection Implements,
    MixinOutputCollection Usings
  );

  private sealed class MixinOutputAccumulator {
    private readonly List<MixinExpressionOutput> _outputs = [];
    private OutputFingerprintBuilder _fingerprint;

    internal IReadOnlyList<MixinExpressionOutput> Outputs => _outputs;
    internal ulong Hash => _fingerprint.Hash;
    internal long Length => _fingerprint.Length;

    internal void Add(MixinExpressionOutput output) {
      _outputs.Add(output);
      _fingerprint.Append(output);
    }
  }

  private sealed class MixinOutputCollection : IReadOnlyList<MixinExpressionOutput> {
    private static readonly MixinOutputCollection Empty = new(
      [], OutputFingerprintBuilder.EmptyHash, 0
    );
    private readonly MixinExpressionOutput[] _outputs;

    private MixinOutputCollection(MixinExpressionOutput[] outputs, ulong hash, long length) {
      _outputs = outputs;
      Hash = hash;
      Length = length;
    }

    internal ulong Hash { get; }
    internal long Length { get; }
    public int Count => _outputs.Length;
    public MixinExpressionOutput this[int index] => _outputs[index];

    public IEnumerator<MixinExpressionOutput> GetEnumerator() {
      return ((IEnumerable<MixinExpressionOutput>)_outputs).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return _outputs.GetEnumerator();
    }

    internal static MixinOutputCollection Finish(MixinOutputAccumulator accumulator) {
      return accumulator is null || accumulator.Outputs.Count == 0
        ? Empty
        : new MixinOutputCollection(
          [.. accumulator.Outputs], accumulator.Hash, accumulator.Length
        );
    }
  }

  private struct OutputFingerprintBuilder {
    internal const ulong EmptyHash = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;
    private ulong _hash;

    internal ulong Hash => _hash == 0 ? EmptyHash : _hash;
    internal long Length { get; private set; }

    internal void Append(MixinExpressionOutput output) {
      Mix((ulong)output.Target);
      Append(output.InjectionTarget);
      Mix(unchecked((ulong)output.InjectionPriority));
      Mix((ulong)output.Segments.Count);
      foreach (var segment in output.Segments) {
        Mix(segment.IsInterned ? 1UL : 0UL);
        Append(output.Resolve(segment));
      }
    }

    private void Append(string value) {
      Mix((ulong)(value?.Length ?? -1));
      if (value is null) return;
      for (var index = 0; index < value.Length; index++) Mix(value[index]);
      Length += value.Length;
    }

    private void Mix(ulong value) {
      if (_hash == 0) _hash = EmptyHash;
      unchecked {
        _hash ^= value;
        _hash *= Prime;
      }
      Length++;
    }
  }

  private sealed class MixinContribution {
    private MixinContribution(
      LateTarget target,
      int order,
      int sequence,
      MixinExpressionResult expressionResult,
      LateExpressionWork work,
      bool placeholder
    ) : this(target, order, sequence, expressionResult, work) {
      IsPlaceholder = placeholder;
    }

    internal MixinContribution(
      LateTarget target,
      int order,
      int sequence,
      MixinExpressionResult expressionResult,
      LateExpressionWork work
    ) {
      EmittedTarget = target.EmittedTarget;
      IsStaticTarget = target.IsStatic;
      IsPublicTarget = target.IsPublic;
      DelegateTarget = target.DelegateTarget;
      Order = order;
      Sequence = sequence;
      ExpressionResult = expressionResult;
      Provider = work.Provider;
      SourceType = work.SourceType;
      SourceMember = work.SourceMember;
      SourceKind = work.SourceKind;
      SourceParameterCount = work.SourceParameterCount;
    }

    internal MixinContribution(
      string target,
      int order,
      int sequence,
      IReadOnlyDictionary<string, string> targetDefinitions,
      MixinExpressionResult expressionResult,
      string provider,
      ISymbol source
    ) {
      var targetSyntax = RoslynMixinContext.ParseMixinTarget(target, targetDefinitions);
      EmittedTarget = targetSyntax.Name;
      IsStaticTarget = targetSyntax.IsStatic;
      IsPublicTarget = targetSyntax.IsPublic;
      DelegateTarget = targetSyntax.DelegateType;
      Order = order;
      Sequence = sequence;
      ExpressionResult = expressionResult;
      Provider = provider;
      SourceType = (source as INamedTypeSymbol ?? source.ContainingType)
        ?.ToDisplayString(TypeDisplayFormat) ?? "";
      SourceMember = source is INamedTypeSymbol ? "" : source.MetadataName;
      SourceKind = source.Kind.ToString();
      SourceParameterCount = source is IMethodSymbol method ? method.Parameters.Length : 0;
    }

    internal string EmittedTarget { get; }
    internal bool IsStaticTarget { get; }
    internal bool IsPublicTarget { get; }
    internal string DelegateTarget { get; }
    internal int Order { get; }
    internal int Sequence { get; }
    internal MixinExpressionResult ExpressionResult { get; }
    internal string Provider { get; }
    internal string SourceType { get; }
    internal string SourceMember { get; }
    internal string SourceKind { get; }
    internal int SourceParameterCount { get; }
    internal bool IsPlaceholder { get; }

    internal static MixinContribution Placeholder(
      LateTarget target, int sequence, LateExpressionWork work
    ) {
      return new MixinContribution(
        target, target.Order, sequence,
        new MixinExpressionResult(true, null, 0, []),
        work, true
      );
    }
  }

  private sealed record MixinTargetParameter(string Declaration, string Argument) {
    internal static MixinTargetParameter FromSymbol(IParameterSymbol parameter) {
      var type = parameter.Type.ToDisplayString(TypeDisplayFormat);
      return new MixinTargetParameter(
        (parameter.IsParams ? "params " : RefPrefix(parameter.RefKind)) + type + " " + EscapeIdentifier(parameter.Name),
        RefPrefix(parameter.RefKind) + EscapeIdentifier(parameter.Name)
      );
    }
  }

  private sealed record GeneratedMethod(
    string Declaration,
    IReadOnlyList<string> TypeParameters,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<MixinTargetParameter> Parameters,
    string ReturnType,
    bool CallBase,
    string Name,
    IReadOnlyList<MixinContribution> Contributions
  );

  private sealed record MixinTarget(INamedTypeSymbol Type, CSharpCompilation Compilation);
}
