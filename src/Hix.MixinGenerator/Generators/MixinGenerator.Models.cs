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
using Hix;
using Hix.Compiler;
using Hix.Diagnostics;
using Hix.Env;
using Hix.Roslyn;
using Hix.Runtime;

using static Hix.Roslyn.GeneratorAnalysis;
using static Hix.Roslyn.GeneratorDiagnostics.Mixins;
using static Hix.Roslyn.GeneratorSource;
using static Hix.Roslyn.GeneratorStrings;
using HixExecutionContext = Hix.Runtime.HixExecutionContext;

namespace HelixSourceGenerator.Generators;

public sealed partial class MixinGenerator {
  private sealed record AnnotatedSymbolData(ISymbol Symbol, IReadOnlyList<AttributeData> Attributes);

  private sealed class HixGenerationContext {
    private readonly List<HixDebugExpression> _debugExpressions = [];
    private readonly List<HixDiagnostic> _diagnostics = [];
    private readonly List<LateExpressionWork> _lateExpressions = [];

    internal HixGenerationContext(bool captureDebug, bool vmDebug) {
      Debug = captureDebug;
      VmDebug = vmDebug;
    }

    internal bool Debug { get; }
    internal bool VmDebug { get; }
    internal bool HasLateExpressions => _lateExpressions.Count != 0;
    internal IReadOnlyList<LateExpressionWork> LateExpressions => _lateExpressions;
    internal IReadOnlyList<HixDebugExpression> DebugExpressions => _debugExpressions;

    internal void ReportDiagnostic(Diagnostic diagnostic) {
      _diagnostics.Add(HixDiagnostic.Detach(diagnostic));
    }

    internal void AddLateExpression(
      HixExpressionExecutionProgram program,
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
          HixDiagnostic.Detach(Diagnostic.Create(ExpressionLog, location, "")),
          provider ?? "", sourceType ?? "",
          sourceMember ?? "", sourceKind ?? "", sourceParameterCount
        )
      );
    }

    internal void AddDebugExpression(
      HixExpressionExecutionProgram preludeProgram,
      HixExpressionExecutionProgram lateProgram,
      IReadOnlyDictionary<string, object> variables,
      IReadOnlyDictionary<string, object> carries,
      string provider,
      ISymbol source,
      int preludeOperations,
      double preludeMilliseconds
    ) {
      if (!Debug) return;
      _debugExpressions.Add(
        new HixDebugExpression(
          preludeProgram, lateProgram, variables.ToImmutableDictionary(StringComparer.Ordinal),
          carries.ToImmutableDictionary(StringComparer.Ordinal),
          provider ?? "", (source as INamedTypeSymbol ?? source.ContainingType)
          ?.ToDisplayString(TypeDisplayFormat) ?? "",
          source is INamedTypeSymbol ? "" : source.MetadataName,
          preludeOperations, preludeMilliseconds
        )
      );
    }

    internal HixOutputModel Complete(HixRenderModel render = null) => new(
      render?.Wrapper.HintName, null, render, _lateExpressions.ToImmutableArray(),
      _diagnostics.ToImmutableArray(), ImmutableArray<string>.Empty, ImmutableArray<HixReportedLog>.Empty
    );
  }

  private sealed record LateExpressionWork(
    HixExpressionExecutionProgram Program,
    string PreludeIdentity,
    string LateIdentity,
    IReadOnlyDictionary<string, object> Variables,
    IReadOnlyDictionary<string, object> Carries,
    ImmutableArray<LateTarget> Targets,
    HixDiagnostic Location,
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

  private sealed class HixRenderModel {
    internal HixRenderModel(
      DetachedTypeWrapper wrapper,
      ImmutableArray<GeneratedMethod> methods,
      HixOutputCollection annotations,
      HixOutputCollection @class,
      HixOutputCollection file,
      HixOutputCollection implements,
      HixOutputCollection usings,
      IEnumerable<KeyValuePair<string, object>> primaryVariables,
      HixStringPool stringPool,
      IEnumerable<KeyValuePair<HixString, IHixValue>> targetVariables,
      ImmutableArray<HixDebugExpression> debugExpressions,
      bool debug, bool vmDebug
    ) {
      using var profile = HixProfiler.Measure("model.render.create");
      Wrapper = wrapper;
      Methods = methods;
      Annotations = annotations;
      Class = @class;
      File = file;
      Implements = implements;
      Usings = usings;
      PrimaryVariables = new HixValueDictionary(
        primaryVariables.Select(item =>
          new KeyValuePair<HixString, IHixValue>(
            stringPool.Get(item.Key), RuntimeValue(item.Value, stringPool)
          )
        )
      );
      TargetVariables = new HixValueDictionary(targetVariables);
      DebugExpressions = debugExpressions;
      Debug = debug;
      VmDebug = vmDebug;
      StringPool = stringPool;
      GenerationVersion = Interlocked.Increment(ref _generationCounter);
      Fingerprint = HixRenderFingerprint.Create(this);
    }

    internal DetachedTypeWrapper Wrapper { get; }
    internal ImmutableArray<GeneratedMethod> Methods { get; }
    internal HixOutputCollection Annotations { get; }
    internal HixOutputCollection Class { get; }
    internal HixOutputCollection File { get; }
    internal HixOutputCollection Implements { get; }
    internal HixOutputCollection Usings { get; }
    internal HixValueDictionary PrimaryVariables { get; }
    internal HixValueDictionary TargetVariables { get; }
    internal ImmutableArray<HixDebugExpression> DebugExpressions { get; }
    internal bool Debug { get; }
    internal bool VmDebug { get; }
    internal HixStringPool StringPool { get; }

    internal long GenerationVersion { get; }
    internal HixRenderFingerprint Fingerprint { get; }

    private static IHixValue RuntimeValue(object value, HixStringPool strings) {
      return value switch {
        IHixValue typed => typed,
        null => NullHixValue.Instance,
        bool boolean => boolean ? BooleanHixValue.True : BooleanHixValue.False,
        string text => new LiteralHixValue(strings.Get(text)),
        DetachedSemanticData detached => DetachedSemanticHixValue.Materialize(detached),
        _ => new ObjectHixValue(value)
      };
    }
  }

  private sealed record HixDiagnostic(
    DiagnosticDescriptor Descriptor,
    string Message,
    string Path,
    TextSpan SourceSpan,
    LinePositionSpan LineSpan,
    bool HasLocation,
    ImmutableDictionary<string, string> Properties
  ) {
    internal static HixDiagnostic Detach(Diagnostic diagnostic) {
      var location = diagnostic.Location;
      if (location == Location.None || !location.IsInSource) {
        return new HixDiagnostic(
          diagnostic.Descriptor, diagnostic.GetMessage(CultureInfo.InvariantCulture),
          "", default, default, false, diagnostic.Properties
        );
      }
      var lineSpan = location.GetLineSpan();
      return new HixDiagnostic(
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

  private sealed record HixOutputModel(
    string HintName, string Source, HixRenderModel Render,
    ImmutableArray<LateExpressionWork> LateExpressions,
    ImmutableArray<HixDiagnostic> Diagnostics, ImmutableArray<string> Errors,
    ImmutableArray<HixReportedLog> Logs
  );

  private sealed record HixReportedLog(HixExpressionLog Log, HixDiagnostic Location);

  private readonly struct HixRenderFingerprint : IEquatable<HixRenderFingerprint> {
    private HixRenderFingerprint(
      FingerprintPart outputs,
      FingerprintPart variables,
      FingerprintPart signatures,
      bool vmDebug
    ) {
      Outputs = outputs;
      Variables = variables;
      Signatures = signatures;
      VmDebug = vmDebug;
    }

    internal FingerprintPart Outputs { get; }
    internal FingerprintPart Variables { get; }
    internal FingerprintPart Signatures { get; }
    private bool VmDebug { get; }

    internal static HixRenderFingerprint Create(HixRenderModel render) {
      using var profile = HixProfiler.Measure("model.render.fingerprint");
      var outputs = new HixFingerprintBuilder();
      AppendOutputCollection(outputs, render.Annotations);
      AppendOutputCollection(outputs, render.Class);
      AppendOutputCollection(outputs, render.File);
      AppendOutputCollection(outputs, render.Implements);
      AppendOutputCollection(outputs, render.Usings);

      var variables = new HixFingerprintBuilder();
      var fingerprintContext = new UnlinkedHixExpressionContext(render.StringPool);
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

      var signatures = new HixFingerprintBuilder();
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
      signatures.Append(render.VmDebug);
      return new HixRenderFingerprint(
        new FingerprintPart(outputs.Hash, outputs.Length),
        new FingerprintPart(variables.Hash, variables.Length),
        new FingerprintPart(signatures.Hash, signatures.Length),
        render.VmDebug
      );
    }

    private static IHixValue RuntimeValue(object value, HixStringPool strings) {
      return value switch {
        IHixValue typed => typed,
        null => NullHixValue.Instance,
        bool boolean => boolean ? BooleanHixValue.True : BooleanHixValue.False,
        string text => new LiteralHixValue(strings.Get(text)),
        DetachedSemanticData detached => DetachedSemanticHixValue.Materialize(detached),
        _ => new ObjectHixValue(value)
      };
    }

    private static void AppendOutputCollection(
      HixFingerprintBuilder builder,
      HixOutputCollection outputs
    ) {
      builder.Append(outputs.Count);
      builder.Append(unchecked((long)outputs.Hash));
      builder.Append(outputs.Length);
    }

    private static void AppendOutputs(
      HixFingerprintBuilder builder,
      IReadOnlyCollection<HixExpressionOutput> outputs
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

    public bool Equals(HixRenderFingerprint other) {
      return Outputs.Equals(other.Outputs) &&
        Variables.Equals(other.Variables) &&
        Signatures.Equals(other.Signatures) && VmDebug == other.VmDebug;
    }

    public override bool Equals(object value) {
      return value is HixRenderFingerprint other && Equals(other);
    }

    public override int GetHashCode() {
      return unchecked(
        (((Outputs.GetHashCode() * 397) ^ Variables.GetHashCode()) * 397 ^
          Signatures.GetHashCode()) * 397 ^ VmDebug.GetHashCode()
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

  private sealed class HixOutputModelComparer : IEqualityComparer<HixOutputModel> {
    internal static readonly HixOutputModelComparer Instance = new();

    public bool Equals(HixOutputModel x, HixOutputModel y) {
      using var profile = HixProfiler.Measure("comparer.output.equals");
      return ReferenceEquals(x, y) || (x is not null &&
        y is not null &&
        string.Equals(x.HintName, y.HintName, StringComparison.Ordinal) &&
        Nullable.Equals(x.Render?.Fingerprint, y.Render?.Fingerprint) &&
        DiagnosticKey(x.Diagnostics) == DiagnosticKey(y.Diagnostics) &&
        LateEqual(x.LateExpressions, y.LateExpressions));
    }

    public int GetHashCode(HixOutputModel value) {
      using var profile = HixProfiler.Measure("comparer.output.hash");
      return unchecked(
        (StringComparer.Ordinal.GetHashCode(value.HintName ?? "") * 397) ^
        (value.Render?.Fingerprint.GetHashCode() ?? 0)
      );
    }

    private static string DiagnosticKey(ImmutableArray<HixDiagnostic> diagnostics) {
      using var profile = HixProfiler.Measure("comparer.output.diagnostic_key");
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
      using var profile = HixProfiler.Measure("comparer.output.late_equal");
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

  private sealed class UnlinkedHixExpressionContext : HixExecutionContext {
    internal UnlinkedHixExpressionContext(HixStringPool strings) : base(HixMixinBackend.Instance, strings) { }

    protected override IHixValue ResolveHost(HixExpressionRoot root, HixString member) {
      return Error("late expressions cannot resolve host values");
    }
  }

  private sealed class ExpressionOutputs {
    private HixOutputAccumulator _annotations, _class, _file, _implements, _usings;
    internal bool Any { get; private set; }

    internal void Add(HixExpressionOutput output) {
      if (output.IsEmpty) return;
      if (output.Target == HixEmissionTarget.Class) {
        Any = true;
        (_class ??= new HixOutputAccumulator()).Add(output);
        return;
      }
      if (output.Target == HixEmissionTarget.File) {
        Any = true;
        (_file ??= new HixOutputAccumulator()).Add(output);
        return;
      }
      Any = true;
      switch (output.Target) {
        case HixEmissionTarget.Extends or HixEmissionTarget.Implements:
          (_implements ??= new HixOutputAccumulator()).Add(output); break;
        case HixEmissionTarget.Annotation:
          (_annotations ??= new HixOutputAccumulator()).Add(output); break;
        case HixEmissionTarget.Using:
          (_usings ??= new HixOutputAccumulator()).Add(output);
          break;
      }
    }

    internal ExpressionOutputSet Finish() {
      return new ExpressionOutputSet(
        HixOutputCollection.Finish(_annotations),
        HixOutputCollection.Finish(_class),
        HixOutputCollection.Finish(_file),
        HixOutputCollection.Finish(_implements),
        HixOutputCollection.Finish(_usings)
      );
    }
  }

  private sealed record ExpressionOutputSet(
    HixOutputCollection Annotations,
    HixOutputCollection Class,
    HixOutputCollection File,
    HixOutputCollection Implements,
    HixOutputCollection Usings
  );

  private sealed class HixOutputAccumulator {
    private readonly List<HixExpressionOutput> _outputs = [];
    private OutputFingerprintBuilder _fingerprint;

    internal IReadOnlyList<HixExpressionOutput> Outputs => _outputs;
    internal ulong Hash => _fingerprint.Hash;
    internal long Length => _fingerprint.Length;

    internal void Add(HixExpressionOutput output) {
      _outputs.Add(output);
      _fingerprint.Append(output);
    }
  }

  private sealed class HixOutputCollection : IReadOnlyList<HixExpressionOutput> {
    private static readonly HixOutputCollection Empty = new(
      [], OutputFingerprintBuilder.EmptyHash, 0
    );
    private readonly HixExpressionOutput[] _outputs;

    private HixOutputCollection(HixExpressionOutput[] outputs, ulong hash, long length) {
      _outputs = outputs;
      Hash = hash;
      Length = length;
    }

    internal ulong Hash { get; }
    internal long Length { get; }
    public int Count => _outputs.Length;
    public HixExpressionOutput this[int index] => _outputs[index];

    public IEnumerator<HixExpressionOutput> GetEnumerator() {
      return ((IEnumerable<HixExpressionOutput>)_outputs).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return _outputs.GetEnumerator();
    }

    internal static HixOutputCollection Finish(HixOutputAccumulator accumulator) {
      return accumulator is null || accumulator.Outputs.Count == 0
        ? Empty
        : new HixOutputCollection(
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

    internal void Append(HixExpressionOutput output) {
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

  private sealed class HixContribution {
    private HixContribution(
      LateTarget target,
      int order,
      int sequence,
      HixExpressionResult expressionResult,
      LateExpressionWork work,
      bool placeholder
    ) : this(target, order, sequence, expressionResult, work) {
      IsPlaceholder = placeholder;
    }

    internal HixContribution(
      LateTarget target,
      int order,
      int sequence,
      HixExpressionResult expressionResult,
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

    internal HixContribution(
      string target,
      int order,
      int sequence,
      IReadOnlyDictionary<string, string> targetDefinitions,
      HixExpressionResult expressionResult,
      string provider,
      ISymbol source
    ) {
      var targetSyntax = HixMixinContext.ParseMixinTarget(target, targetDefinitions);
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
    internal HixExpressionResult ExpressionResult { get; }
    internal string Provider { get; }
    internal string SourceType { get; }
    internal string SourceMember { get; }
    internal string SourceKind { get; }
    internal int SourceParameterCount { get; }
    internal bool IsPlaceholder { get; }

    internal static HixContribution Placeholder(
      LateTarget target, int sequence, LateExpressionWork work
    ) {
      return new HixContribution(
        target, target.Order, sequence,
        new HixExpressionResult(true, null, 0, []),
        work, true
      );
    }
  }

  private sealed record HixTargetParameter(string Declaration, string Argument) {
    internal static HixTargetParameter FromSymbol(IParameterSymbol parameter) {
      var type = parameter.Type.ToDisplayString(TypeDisplayFormat);
      return new HixTargetParameter(
        (parameter.IsParams ? "params " : RefPrefix(parameter.RefKind)) + type + " " + EscapeIdentifier(parameter.Name),
        RefPrefix(parameter.RefKind) + EscapeIdentifier(parameter.Name)
      );
    }
  }

  private sealed record GeneratedMethod(
    string Declaration,
    IReadOnlyList<string> TypeParameters,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<HixTargetParameter> Parameters,
    string ReturnType,
    bool CallBase,
    string Name,
    IReadOnlyList<HixContribution> Contributions
  );

  private sealed record HixTarget(INamedTypeSymbol Type, CSharpCompilation Compilation);
}
