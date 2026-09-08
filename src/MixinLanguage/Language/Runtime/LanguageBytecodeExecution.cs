using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Runtime;

internal sealed partial class LanguageExecution {
  private VmCompletion Run(int address) {
    var code = machine.Code(program);
    var header = HixInstruction.Decode(code, address);
    var end = address + header.A;
    var stack = new List<IMixinValue>();
    var selectors = new List<IMixinValue>();
    var checks = new Stack<(int Target, int Stack, int Selectors, IMixinValue Selector)>();
    var initialSelector = selector;
    IMixinValue Pop() { var value = stack[stack.Count - 1]; stack.RemoveAt(stack.Count - 1); return value; }
    IMixinValue[] PopMany(int count) {
      var values = stack.GetRange(stack.Count - count, count).ToArray();
      stack.RemoveRange(stack.Count - count, count); return values;
    }
    VmCompletion Push(IMixinValue value, int line) {
      if (value is ErrorMixinValue {IsChecked: false}) return Failed(value, line);
      stack.Add(value);
      return default;
    }
    void Reset() { stack.Clear(); selectors.Clear(); checks.Clear(); selector = initialSelector; }
    try {
      for (var pc = address; pc < end;) {
        var line = program.SourceLines[pc];
        if (steps >= StepLimit) return Failed(context.Error("execution limit exceeded"), line);
        steps++;
        var instruction = HixInstruction.Decode(code, pc);
        var instructionAddress = pc;
        pc += instruction.Size;
        var a = instruction.A; var b = instruction.B;
        string Name(int index) => machine.StringPool[index];
        var completion = default(VmCompletion);
        switch (instruction.Opcode) {
          case HixOpcode.End: return default;
          case HixOpcode.Enter: break;
          case HixOpcode.Constant: completion = Push(machine.ConstantPool[a], line); break;
          case HixOpcode.String: stack.Add(String(Name(a))); break;
          case HixOpcode.Root:
          case HixOpcode.SmartRoot: completion = Push(Root(Name(a), instruction.Opcode == HixOpcode.SmartRoot), line); break;
          case HixOpcode.HostThis:
          case HixOpcode.HostTarget:
          case HixOpcode.HostAttribute:
            completion = Push(pure ? context.Error("pure functions cannot read host roots")
              : !prelude ? context.Error("host members require the prelude pass")
              : context.Resolve(instruction.Opcode == HixOpcode.HostThis ? MixinExpressionRoot.This : instruction.Opcode == HixOpcode.HostTarget ? MixinExpressionRoot.Target : MixinExpressionRoot.Attribute,
                ExecutionContext.Dynamic(Name(a))), line); break;
          case HixOpcode.LoadLocal:
          case HixOpcode.LoadVariable:
          case HixOpcode.LoadTarget:
            b = instruction.Opcode == HixOpcode.LoadLocal ? 0 : instruction.Opcode == HixOpcode.LoadVariable ? 1 : 2;
            var storage = b == 0 ? locals : b == 1 ? variables : targetVariables;
            completion = Push(pure && b != 0 ? context.Error("pure functions cannot read shared storage")
              : storage.TryGetValue(Name(a), out var found) ? found
              : b == 0 && depth == 0 && carries.TryGetValue(Name(a), out var carried) ? carried : NullMixinValue.Instance, line);
            break;
          case HixOpcode.Member:
            var receiver = Pop();
            completion = Push(receiver is LiteralMixinValue or NumberMixinValue or BooleanMixinValue or NullMixinValue or KindMixinValue
              ? context.Error("member selection requires a container or host selector")
              : receiver.Select(context, ExecutionContext.Dynamic(Name(a))), line); break;
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
            if (pure && (b != 0 || isCarry)) { completion = Failed(context.Error("pure functions cannot mutate shared storage"), line); break; }
            if (isCarry && (!prelude || depth != 0)) { completion = Failed(context.Error("carry local assignments require a top-level prelude expression"), line); break; }
            if (instruction.Opcode is HixOpcode.CheckStoreCarry or HixOpcode.CheckStoreVariable or HixOpcode.CheckStoreTarget) break;
            var destination = b == 0 ? isCarry || depth == 0 && carriedLocals.Contains(Name(a)) ? carries : locals
              : b == 1 ? variables : targetVariables;
            if (isCarry) carriedLocals.Add(Name(a));
            destination[Name(a)] = Pop(); break;
          case HixOpcode.Pop: Pop(); break;
          case HixOpcode.Tuple: stack.Add(new TupleMixinValue(PopMany(a))); break;
          case HixOpcode.Pack: stack.Add(Pack(PopMany(a))); break;
          case HixOpcode.Table:
            var items = PopMany(a * 2);
            var entries = new KeyValuePair<MixinString, IMixinValue>[a];
            for (var i = 0; i < a; i++) entries[i] = new(((LiteralMixinValue)items[i * 2]).Value, items[i * 2 + 1]);
            stack.Add(new MixinTableValue(entries)); break;
          case HixOpcode.Error: completion = Failed(context.Error(Name(a)), line); break;
          case HixOpcode.Text:
            var rendered = RenderText(Pop());
            if (rendered is ErrorMixinValue) completion = Failed(rendered, line); else stack.Add(rendered);
            break;
          case HixOpcode.Interpolate: stack.Add(String(string.Concat(PopMany(a).Select(Text)))); break;
          case HixOpcode.Call:
            var called = Call(Name(a), PopMany(b), line);
            completion = pendingControl; pendingControl = default;
            if (completion.Kind == BytecodeFlow.Normal) completion = Push(called, line);
            break;
          case HixOpcode.Boolean:
            var boolean = Pop(); stack.Add(boolean is ErrorMixinValue ? boolean : Bool(boolean.IsTruthy(context))); break;
          case HixOpcode.Not: stack.Add(Bool(!Pop().IsTruthy(context))); break;
          case HixOpcode.Check: checks.Push((instructionAddress + a, stack.Count, selectors.Count, selector)); break;
          case HixOpcode.EndCheck:
            checks.Pop();
            if (stack[stack.Count - 1] is ErrorMixinValue checkedError)
              stack[stack.Count - 1] = checkedError with {IsChecked = true};
            break;
          case HixOpcode.Jump: pc = instructionAddress + a; break;
          case HixOpcode.JumpNotNull: if (stack[stack.Count - 1] is not NullMixinValue) pc = instructionAddress + a; break;
          case HixOpcode.JumpFalse: if (!Pop().IsTruthy(context)) pc = instructionAddress + a; break;
          case HixOpcode.PushSelector: selectors.Add(selector); selector = Pop(); break;
          case HixOpcode.PopSelector:
            selector = selectors[selectors.Count - 1]; selectors.RemoveAt(selectors.Count - 1); break;
          case HixOpcode.MatchSelector: stack.Add(Bool(Equal(selector, Pop()))); break;
          case HixOpcode.Block: completion = Run(instructionAddress + a); break;
          case HixOpcode.Return: completion = new(BytecodeFlow.Return, Pop(), Line: line); break;
          case HixOpcode.Goto: completion = new(BytecodeFlow.Goto, Target: instructionAddress + a, Line: line); break;
          case HixOpcode.InvalidGoto: completion = new(BytecodeFlow.Goto, Target: -1, Line: line); break;
          case HixOpcode.Break: completion = new(BytecodeFlow.Break, Line: line); break;
          case HixOpcode.Continue: completion = new(BytecodeFlow.Continue, Line: line); break;
          default: throw new InvalidOperationException("Invalid opcode at " + instructionAddress);
        }
        if (completion.Kind == BytecodeFlow.Error && checks.Count != 0) {
          var handler = checks.Pop();
          stack.RemoveRange(handler.Stack, stack.Count - handler.Stack);
          selectors.RemoveRange(handler.Selectors, selectors.Count - handler.Selectors);
          selector = handler.Selector;
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
    } finally { selector = initialSelector; }
  }
}
