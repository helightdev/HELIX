using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Runtime;

internal sealed partial class LanguageExecution {
  private VmCompletion Run(int address) => new ExecutionFrame(this, address).Run();

  /// <summary>State owned by one block invocation. Nested blocks have independent stacks and handlers.</summary>
  private readonly struct ExecutionFrame {
    private readonly LanguageExecution execution;
    private readonly int address;
    private readonly int end;
    private readonly HixInstruction header;
    private readonly IReadOnlyList<byte> code;
    private readonly List<IMixinValue> stack;
    private readonly Stack<(int Target, int Stack)> checks;

    internal ExecutionFrame(LanguageExecution execution, int address) {
      this.execution = execution;
      this.address = address;
      code = execution.machine.Code(execution.program);
      header = HixInstruction.Decode(code, address);
      end = address + header.A;
      stack = new List<IMixinValue>();
      checks = new Stack<(int, int)>();
    }

    private string Name(int index) => execution.machine.StringPool[index];
    private IMixinValue Pop() {
      var index = stack.Count - 1;
      var value = stack[index];
      stack.RemoveAt(index);
      return value;
    }

    private IMixinValue[] PopMany(int count) {
      if (count == 0) return Array.Empty<IMixinValue>();
      var start = stack.Count - count;
      var values = new IMixinValue[count];
      for (var i = 0; i < count; i++) values[i] = MixinStorageValue.Capture(stack[start + i]);
      stack.RemoveRange(start, count);
      return values;
    }

    private VmCompletion Push(IMixinValue value, int line) {
      if (value is ErrorMixinValue {IsChecked: false}) return Failed(value, line);
      stack.Add(value);
      return default;
    }

    private void Reset() {
      stack.Clear(); checks.Clear();
    }

    internal VmCompletion Run() {
      for (var pc = address; pc < end;) {
        var line = execution.program.SourceLines[pc];
        if (execution.steps >= StepLimit) return Failed(execution.context.Error("execution limit exceeded"), line);
        execution.steps++;
        var instruction = HixInstruction.Decode(code, pc);
        var instructionAddress = pc;
        pc += instruction.Size;
        var a = instruction.A; var b = instruction.B;
        var completion = default(VmCompletion);
        switch (instruction.Opcode) {
          case HixOpcode.End: return default;
          case HixOpcode.Enter: break;
          case HixOpcode.LoadConst: completion = Push(execution.machine.ConstantPool[a], line); break;
          case HixOpcode.LoadString: stack.Add(String(Name(a))); break;
          case HixOpcode.LoadTrue: stack.Add(BooleanMixinValue.True); break;
          case HixOpcode.LoadFalse: stack.Add(BooleanMixinValue.False); break;
          case HixOpcode.LoadNull: stack.Add(NullMixinValue.Instance); break;
          case HixOpcode.LoadTuple: stack.Add(TupleMixinValue.Empty); break;
          case HixOpcode.LoadTable: stack.Add(MixinTableValue.Empty); break;
          case HixOpcode.LoadRoot: completion = Push(execution.Root(Name(a)), line); break;
          case HixOpcode.Equal:
            var right = Pop(); var left = Pop();
            stack.Add(Bool(execution.Equal(left, right))); break;
          case HixOpcode.Member:
            var receiver = Pop();
            completion = Push(receiver is LiteralMixinValue or NumberMixinValue or BooleanMixinValue or NullMixinValue or KindMixinValue
              ? execution.context.Error("member selection requires a container or host selector")
              : receiver.Select(execution.context, ExecutionContext.Dynamic(Name(a))), line); break;
          case HixOpcode.CheckStoreCarry:
          case HixOpcode.CheckStoreVariable:
          case HixOpcode.CheckStoreTarget:
          case HixOpcode.StoreCarry:
          case HixOpcode.StoreLocal:
          case HixOpcode.StoreVariable:
          case HixOpcode.StoreTarget:
            b = instruction.Opcode is HixOpcode.StoreVariable or HixOpcode.CheckStoreVariable ? 1
              : instruction.Opcode is HixOpcode.StoreTarget or HixOpcode.CheckStoreTarget ? 2 : 0;
            var isCarry = instruction.Opcode is HixOpcode.StoreCarry or HixOpcode.CheckStoreCarry;
            if (execution.pure && (b != 0 || isCarry)) { completion = Failed(execution.context.Error("pure functions cannot mutate shared storage"), line); break; }
            if (isCarry && (!execution.prelude || execution.depth != 0)) { completion = Failed(execution.context.Error("carry local assignments require a top-level prelude expression"), line); break; }
            if (instruction.Opcode is HixOpcode.CheckStoreCarry or HixOpcode.CheckStoreVariable or HixOpcode.CheckStoreTarget) break;
            var destination = b == 0 ? isCarry || execution.depth == 0 && execution.carriedLocals.Contains(Name(a)) ? execution.carries : execution.locals
              : b == 1 ? execution.variables : execution.targetVariables;
            if (isCarry) execution.carriedLocals.Add(Name(a));
            destination.StoreIsolated(ExecutionContext.Dynamic(Name(a)), MixinStorageValue.Capture(Pop())); break;
          case HixOpcode.Pop: Pop(); break;
          case HixOpcode.PackTuple: stack.Add(new TupleMixinValue(PopMany(a))); break;
          case HixOpcode.Pack: stack.Add(Pack(PopMany(a))); break;
          case HixOpcode.PackTable:
            var items = PopMany(a * 2);
            var entries = new KeyValuePair<MixinString, IMixinValue>[a];
            for (var i = 0; i < a; i++) entries[i] = new(((LiteralMixinValue)items[i * 2]).Value, items[i * 2 + 1]);
            stack.Add(new MixinTableValue(entries)); break;
          case HixOpcode.Throw: completion = Failed(execution.context.Error(Name(a)), line); break;
          case HixOpcode.CastString:
            var rendered = execution.RenderText(Pop());
            if (rendered is ErrorMixinValue) completion = Failed(rendered, line); else stack.Add(rendered);
            break;
          case HixOpcode.Interpolate: stack.Add(String(string.Concat(PopMany(a).Select(execution.Text)))); break;
          case HixOpcode.Call:
            var called = execution.Call(Name(a), PopMany(b), line);
            completion = execution.pendingControl; execution.pendingControl = default;
            if (completion.Kind == BytecodeFlow.Normal) completion = Push(called, line);
            break;
          case HixOpcode.CastBoolean:
            var boolean = Pop(); stack.Add(boolean is ErrorMixinValue ? boolean : Bool(boolean.IsTruthy(execution.context))); break;
          case HixOpcode.Not: stack.Add(Bool(!Pop().IsTruthy(execution.context))); break;
          case HixOpcode.Check: checks.Push((instructionAddress + a, stack.Count)); break;
          case HixOpcode.EndCheck:
            checks.Pop();
            if (stack[stack.Count - 1] is ErrorMixinValue checkedError)
              stack[stack.Count - 1] = checkedError with {IsChecked = true};
            break;
          case HixOpcode.Jump: pc = instructionAddress + a; break;
          case HixOpcode.JumpNotNull: if (stack[stack.Count - 1] is not NullMixinValue) pc = instructionAddress + a; break;
          case HixOpcode.JumpFalse: if (!Pop().IsTruthy(execution.context)) pc = instructionAddress + a; break;
          case HixOpcode.Block: completion = execution.Run(instructionAddress + a); break;
          case HixOpcode.Return: completion = new(BytecodeFlow.Return, MixinStorageValue.Capture(Pop()), Line: line); break;
          case HixOpcode.Goto: completion = new(BytecodeFlow.Goto, Target: instructionAddress + a, Line: line); break;
          case HixOpcode.InvalidGoto: completion = new(BytecodeFlow.Goto, Target: -1, Line: line); break;
          case HixOpcode.Break: completion = new(BytecodeFlow.Break, Line: line); break;
          case HixOpcode.Continue: completion = new(BytecodeFlow.Continue, Line: line); break;
          default: throw new InvalidOperationException("Invalid opcode at " + instructionAddress);
        }
        if (completion.Kind == BytecodeFlow.Error && checks.Count != 0) {
          var handler = checks.Pop();
          stack.RemoveRange(handler.Stack, stack.Count - handler.Stack);
          stack.Add(completion.Value is ErrorMixinValue error ? error with {IsChecked = true} : completion.Value);
          pc = handler.Target;
          continue;
        }
        if (completion.Kind == BytecodeFlow.Break) return default;
        if (completion.Kind == BytecodeFlow.Continue) { Reset(); pc = address + header.Size; continue; }
        if (completion.Kind == BytecodeFlow.Goto && completion.Target > address && completion.Target < end) {
          Reset(); pc = completion.Target; continue;
        }
        if (completion.Kind != BytecodeFlow.Normal) return completion;
      }
      return default;
    }
  }
}
