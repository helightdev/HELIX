using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hix.Runtime;
namespace Hix.Standalone;

public class HixStandaloneBackend : HixBackend {
  public HixStandaloneBackend(TextWriter output = null, string workingDirectory = null) {
    Output = output ?? Console.Out;
    WorkingDirectory = Path.GetFullPath(workingDirectory ?? Directory.GetCurrentDirectory());
  }
  public TextWriter Output { get; }
  public string WorkingDirectory { get; }
  public IReadOnlyList<string> CompilationSources { get; private set; } = [];
  public void SetCompilationSources(IEnumerable<string> sources) => CompilationSources = sources?.ToArray() ?? [];
  protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {

    base.RegisterFunctions(functions);
    functions.AddLibrary("""
      %opaque<hix.standalone.task>(1)
      type task = opaque;
      %opaque<hix.standalone.channel>(1)
      type channel = opaque;
      %opaque<hix.standalone.semaphore>(0)
      type semaphore = opaque;
      %description<Parses a Hal asset and returns its primary or named local section.>
      pure func parseHal(string arg0) -> any external standalone_parseHal;
      %description<Parses a Hal asset and returns its primary or named local section.>
      pure func parseHal(string arg0, string arg1) -> any external standalone_parseHal;

      %description<Writes a Hix value as a canonical single-section Hal asset.>
      pure func writeHal(any arg0) -> string external standalone_writeHal;
      %description<Writes a Hix value as a canonical single-section Hal asset.>
      pure func writeHal(any arg0, pattern arg1) -> string external standalone_writeHal;

      %description<>
      pure func joinPath(%variadic string arg0) -> string external standalone_joinPath;

      %description<>
      pure func splitPath(string arg0) -> tuple external standalone_splitPath;

      %description<>
      pure func trimExtension(string arg0) -> string external standalone_trimExtension;

      %description<>
      pure func normalizePath(string arg0) -> string external standalone_normalizePath;

      %description<>
      pure func resolvePath(string arg0) -> string external standalone_resolvePath;

      %description<>
      pure func pathDir(string arg0) -> string external standalone_pathDir;

      %description<>
      pure func pathName(string arg0) -> string external standalone_pathName;

      %description<>
      pure func isRelativePath(string arg0) -> bool external standalone_isRelativePath;

      %description<>
      pure func isAbsolutePath(string arg0) -> bool external standalone_isAbsolutePath;

      %description<>
      pure func isDirPath(string arg0) -> bool external standalone_isDirPath;

      %description<>
      pure func currentPath() -> string external standalone_currentPath;

      %description<>
      pure func lineTerminator() -> string external standalone_lineTerminator;

      %description<>
      pure func pathSeparator() -> string external standalone_pathSeparator;

      %description<>
      func readFile(string path, string encoding = <utf8>) -> string external standalone_readFile;
      %description<>
      func readFile(string path, number offset, number length, string encoding = <utf8>) -> string external standalone_readFile;

      %description<>
      func readFileLength(string arg0) -> number external standalone_readFileLength;

      %description<>
      func readFileLines(string path, string encoding = <utf8>) -> string external standalone_readFileLines;

      %description<>
      func createFile(string arg0) -> bool external standalone_createFile;

      %description<>
      func deleteFile(string arg0) -> bool external standalone_deleteFile;

      %description<>
      func copyFile(string arg0, string arg1) -> bool external standalone_copyFile;

      %description<>
      func renameFile(string arg0, string arg1) -> bool external standalone_renameFile;

      %description<>
      func existsFile(string arg0) -> bool external standalone_existsFile;

      %description<>
      func listFiles(string arg0) -> tuple external standalone_listFiles;

      %description<>
      func writeFile(string path, string content, string encoding = <utf8>, bool append = false) -> bool external standalone_writeFile;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func compile() -> table external standalone_compile;
      %description<Inspects standalone Hix compilation artifacts.>
      pure func compile(string arg0) -> table external standalone_compile;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func success(table arg0) -> bool external standalone_success;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func diagnostics(table arg0) -> tuple external standalone_diagnostics;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func functions(table arg0) -> tuple external standalone_functions;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func function(table arg0, string arg1) -> table external standalone_function;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func disassemble(table arg0) -> string external standalone_disassemble;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func instructions(table arg0) -> tuple external standalone_instructions;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func marker(table arg0, string arg1) -> table external standalone_marker;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func opcode(table arg0) -> string external standalone_opcode;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func offset(table arg0) -> number external standalone_offset;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func operands(table arg0) -> tuple external standalone_operands;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func target(table arg0) -> any external standalone_target;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func line(table arg0) -> number external standalone_line;

      %description<Inspects standalone Hix compilation artifacts.>
      pure func inferredType(table arg0) -> string external standalone_inferredType;

      %description<Queues a Hix function or lambda on the current worker, or explicitly on the VM pool. Arguments are positional.>
      %generic<T>
      %invokeArguments<function><arguments>
      func spawn(delegate() -> T function, tuple arguments = @[], bool useThreadPool = false) -> %task<T> external standalone_spawn;

      %description<Suspends until a task finishes and returns its value, propagating errors.>
      %generic<T>
      func awaitTask(%task<T> arg0) -> T external standalone_awaitTask;

      %description<Waits for every task and returns results in input order. An empty tuple returns an empty tuple.>
      %tupleResults<arg0>(0)
      func awaitAll(%many(task<any>) arg0) -> tuple external standalone_awaitAll;

      %description<Returns the index and value of the first completed task. Empty input is an error; other tasks keep running.>
      %firstResult<arg0>(0)
      func awaitAny(%many(task<any>) arg0) -> table external standalone_awaitAny;

      %description<Tests completion without waiting.>
      func taskDone(%task<any> arg0) -> bool external standalone_taskDone;

      %description<Yields to other runnable events on the current worker.>
      func yield() -> null external standalone_yield;

      %description<Suspends for a non-negative number of milliseconds without blocking the worker.>
      func sleep(number milliseconds) -> null external standalone_sleep;

      %description<Returns the current worker's OS thread ID.>
      func workerId() -> number external standalone_workerId;

      %description<Creates a FIFO channel. Capacity zero is a rendezvous channel; positive capacity buffers messages.>
      %generic<T>
      func channel(number capacity = 0) -> %channel<T> external standalone_channel;

      %description<Sends a value, suspending when the channel has no space.>
      %generic<T>
      func send(%channel<T> arg0, T arg1) -> null external standalone_send;

      %description<Receives the next value, suspending when the channel is empty. Closed and drained channels return an error.>
      %generic<T>
      func receive(%channel<T> arg0) -> T external standalone_receive;

      %description<Closes a channel, wakes waiting senders/receivers, and preserves buffered messages. Returns false if already closed.>
      func closeChannel(%channel<any> arg0) -> bool external standalone_closeChannel;

      %description<Creates a FIFO semaphore with non-negative initial permits and a positive maximum. One permit can be used as a mutex.>
      func semaphore(number permits = 1, number maximum = 2147483647) -> %semaphore external standalone_semaphore;

      %description<Acquires one permit, suspending when none is available.>
      func acquire(%semaphore arg0) -> null external standalone_acquire;

      %description<Releases one permit and wakes the oldest waiter. Exceeding the maximum is an error.>
      func release(%semaphore arg0) -> null external standalone_release;

      %description<Writes values to the host output stream.>
      func print(%variadic any values) -> null external;
      """);
    functions.Add(new PrintFunction());
    JsonFunctions.Register(functions);
    HalFunctions.Register(functions);
    IoFunctions.Register(functions);
    CompilationFunctions.Register(functions);
    ConcurrencyFunctions.Register(functions);
  }
  private sealed class PrintFunction() : FunctionDefinition("print") {
    public override IHixValue Execute(HixCoroutine context, IHixValue[] arguments, int line) {
      var output = ((HixStandaloneBackend)context.Backend).Output;
      for (var i = 0; i < arguments.Length; i++) {
        if (i != 0) output.Write(" ");
        output.Write(context.Render(arguments[i]).Resolve(context.Strings));
      }
      output.WriteLine();
      return NullHixValue.Instance;
    }
  }
}
