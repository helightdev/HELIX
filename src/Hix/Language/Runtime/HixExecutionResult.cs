using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hix.Runtime;

public readonly record struct HixLog(HixString Text = default, int Line = -1);

/// <summary>A detached value emitted to an optional, host-defined destination.</summary>
public readonly struct HixOutput(IHixValue value, HixString target = default) {
  public IHixValue Value => value ?? NullHixValue.Instance;
  public HixString Target => target.IsNull ? HixString.Empty : target;
}

public readonly struct HixExecutionResult {
  internal static readonly IReadOnlyDictionary<HixString, IHixValue> EmptyValues =
    new ReadOnlyDictionary<HixString, IHixValue>(new Dictionary<HixString, IHixValue>());
  private readonly IReadOnlyList<HixOutput> outputs;
  private readonly IReadOnlyList<HixLog> logs;
  private readonly IReadOnlyDictionary<HixString, IHixValue> variables;
  private readonly IReadOnlyDictionary<HixString, IHixValue> carries;
  public HixExecutionResult(
    bool success, HixString error, int errorLine,
    IReadOnlyList<HixOutput> outputs, IReadOnlyList<HixLog> logs = null,
    IReadOnlyDictionary<HixString, IHixValue> variables = null, IReadOnlyDictionary<HixString, IHixValue> carries = null,
    int executedOperations = 0,
    double executionMilliseconds = 0, HixStringPool strings = null
  ) {
    Strings = strings;
    Success = success;
    Error = error;
    ErrorLine = errorLine;
    this.outputs = outputs;
    this.logs = logs;
    this.variables = variables;
    this.carries = carries;
    ExecutedOperations = executedOperations;
    ExecutionMilliseconds = executionMilliseconds;
  }

  public bool Success { get; }
  public HixString Error { get; }
  public HixStringPool Strings { get; }
  public int ErrorLine { get; }
  public IReadOnlyList<HixOutput> Outputs => outputs ?? Array.Empty<HixOutput>();
  public IReadOnlyList<HixLog> Logs => logs ?? Array.Empty<HixLog>();
  public IReadOnlyDictionary<HixString, IHixValue> Variables => variables ?? EmptyValues;
  public IReadOnlyDictionary<HixString, IHixValue> Carries => carries ?? EmptyValues;
  public int ExecutedOperations { get; }
  public double ExecutionMilliseconds { get; }
}

