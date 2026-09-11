using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hix.Compiler;
using Hix.Runtime;
using K = Hix.HixValueKind;

namespace Hix.Standalone;

internal static class IoFunctions {
  public static void Register(FunctionSignatureRegistryBuilder functions) {
    functions.Add(
      Pure("joinPath", [new FunctionSignature(K.String, [K.String], true)], (thread, args) =>
        Text(Normalize(string.Join("/", args.Select(thread.ResolveText))))),
      Pure("splitPath", [new FunctionSignature(K.Tuple, [K.String])], (thread, args) =>
        new TupleHixValue(Normalize(thread.ResolveText(args[0])).Split(['/'], StringSplitOptions.RemoveEmptyEntries)
          .Select(value => (IHixValue)Text(value)).ToArray())),
      Pure("trimExtension", [new FunctionSignature(K.String, [K.String])], (thread, args) => {
        var path = Normalize(thread.ResolveText(args[0])); var extension = Path.GetExtension(Native(path));
        return Text(extension.Length == 0 ? path : path.Substring(0, path.Length - extension.Length));
      }),
      Pure("normalizePath", [new FunctionSignature(K.String, [K.String])], (thread, args) => Text(Normalize(thread.ResolveText(args[0])))),
      Pure("resolvePath", [new FunctionSignature(K.String, [K.String])], (thread, args) => Text(Resolve(thread, args[0]))),
      Pure("pathDir", [new FunctionSignature(K.String, [K.String])], (thread, args) => Text(PathDirectory(thread.ResolveText(args[0])))),
      Pure("pathName", [new FunctionSignature(K.String, [K.String])], (thread, args) => Text(PathName(thread.ResolveText(args[0])))),
      Pure("isRelativePath", [new FunctionSignature(K.Bool, [K.String])], (thread, args) =>
        BooleanHixValue.From(!Path.IsPathRooted(Native(thread.ResolveText(args[0]))))),
      Pure("isAbsolutePath", [new FunctionSignature(K.Bool, [K.String])], (thread, args) =>
        BooleanHixValue.From(Path.IsPathRooted(Native(thread.ResolveText(args[0]))))),
      Pure("isDirPath", [new FunctionSignature(K.Bool, [K.String])], (thread, args) => {
        var path = thread.ResolveText(args[0]);
        return BooleanHixValue.From(path.EndsWith("/", StringComparison.Ordinal) || path.EndsWith("\\", StringComparison.Ordinal));
      }),
      Pure("currentPath", [new FunctionSignature(K.String, [])], (thread, _) => Text(Forward(Backend(thread).WorkingDirectory))),
      Pure("lineTerminator", [new FunctionSignature(K.String, [])], (_, _) => Text(Environment.NewLine)),
      Pure("pathSeparator", [new FunctionSignature(K.String, [])], (_, _) => Text("/"))
    );
    functions.Add(
      Effect("readFile", [
        new FunctionSignature(K.String, [K.String, K.String], ArgumentNames: ["path", "encoding"],
          ArgumentDefaults: [null, new StringExpressionIr("utf8")]),
        new FunctionSignature(K.String, [K.String, K.Number, K.Number, K.String],
          ArgumentNames: ["path", "offset", "length", "encoding"],
          ArgumentDefaults: [null, null, null, new StringExpressionIr("utf8")])
      ], ReadFile),
      Effect("readFileLength", [new FunctionSignature(K.Number, [K.String])], (thread, args) => Try(thread,
        () => (IHixValue)new NumberHixValue(new FileInfo(ResolveNative(thread, args[0])).Length))),
      Effect("readFileLines", [new FunctionSignature(K.String, [K.String, K.String],
          ArgumentNames: ["path", "encoding"], ArgumentDefaults: [null, new StringExpressionIr("utf8")])],
        (thread, args) => Try(thread, () => Text(string.Join(Environment.NewLine,
          File.ReadAllLines(ResolveNative(thread, args[0]), EncodingOf(thread, args, 1)))))),
      Effect("createFile", [new FunctionSignature(K.Bool, [K.String])], (thread, args) => Mutate(() => {
        using var stream = File.Create(ResolveNative(thread, args[0]));
      })),
      Effect("deleteFile", [new FunctionSignature(K.Bool, [K.String])], (thread, args) => Mutate(() => File.Delete(ResolveNative(thread, args[0])))),
      Effect("copyFile", [new FunctionSignature(K.Bool, [K.String, K.String])], (thread, args) =>
        Mutate(() => File.Copy(ResolveNative(thread, args[0]), ResolveNative(thread, args[1])))),
      Effect("renameFile", [new FunctionSignature(K.Bool, [K.String, K.String])], (thread, args) =>
        Mutate(() => File.Move(ResolveNative(thread, args[0]), ResolveNative(thread, args[1])))),
      Effect("existsFile", [new FunctionSignature(K.Bool, [K.String])], (thread, args) =>
        BooleanHixValue.From(File.Exists(ResolveNative(thread, args[0])))),
      Effect("listFiles", [new FunctionSignature(K.Tuple, [K.String])], (thread, args) => Try(thread, () =>
        (IHixValue)new TupleHixValue(Directory.EnumerateFiles(ResolveNative(thread, args[0]))
          .OrderBy(path => path, StringComparer.Ordinal).Select(path => (IHixValue)Text(Forward(path))).ToArray()))),
      Effect("writeFile", [new FunctionSignature(K.Bool, [K.String, K.String, K.String, K.Bool],
        ArgumentNames: ["path", "content", "encoding", "append"],
        ArgumentDefaults: [null, null, new StringExpressionIr("utf8"), new BooleanExpressionIr(false)])], WriteFile)
    );
  }

  private static SimpleFunction Pure(string name, IReadOnlyList<FunctionSignature> signatures, InlineFunction implementation) =>
    new(name, signatures, implementation);
  private static SimpleFunction Effect(string name, IReadOnlyList<FunctionSignature> signatures, InlineFunction implementation) =>
    new(name, signatures, implementation, effects: true);
  private static LiteralHixValue Text(string value) => new(HixString.Dynamic(value ?? ""));
  private static HixStandaloneBackend Backend(HixThread thread) => (HixStandaloneBackend)thread.Backend;
  private static string Native(string path) => path.Replace('/', Path.DirectorySeparatorChar);
  private static string Forward(string path) => path.Replace('\\', '/');
  private static string Resolve(HixThread thread, IHixValue value) => Forward(Path.GetFullPath(Path.Combine(
    Backend(thread).WorkingDirectory, Native(thread.ResolveText(value)))));
  private static string ResolveNative(HixThread thread, IHixValue value) => Native(Resolve(thread, value));

  private static string Normalize(string value) {
    if (string.IsNullOrEmpty(value)) return "";
    value = Forward(value);
    var rooted = value.StartsWith("/", StringComparison.Ordinal);
    var prefix = rooted ? "/" : "";
    var parts = new List<string>();
    foreach (var part in value.Split(['/'], StringSplitOptions.RemoveEmptyEntries)) {
      if (part == ".") continue;
      if (part == ".." && parts.Count != 0 && parts[parts.Count - 1] != "..") parts.RemoveAt(parts.Count - 1);
      else if (part != ".." || !rooted) parts.Add(part);
    }
    return prefix + string.Join("/", parts);
  }
  private static string PathDirectory(string value) {
    var path = Normalize(value).TrimEnd('/'); var index = path.LastIndexOf('/');
    return index < 0 ? "" : index == 0 ? "/" : path.Substring(0, index);
  }
  private static string PathName(string value) {
    var path = Normalize(value).TrimEnd('/'); var index = path.LastIndexOf('/');
    return index < 0 ? path : path.Substring(index + 1);
  }
  private static Encoding EncodingOf(HixThread thread, IHixValue[] args, int index) =>
    thread.ResolveText(args[index]) is var name && string.Equals(name, "utf8", StringComparison.OrdinalIgnoreCase)
      ? new UTF8Encoding(false) : Encoding.GetEncoding(name);
  private static IHixValue ReadFile(HixThread thread, IHixValue[] args) => Try(thread, () => {
    var path = ResolveNative(thread, args[0]);
    if (args.Length < 3) return (IHixValue)Text(File.ReadAllText(path, EncodingOf(thread, args, 1)));
    var offset = ExactInt(thread, args[1], "offset"); var length = ExactInt(thread, args[2], "length");
    if (offset < 0 || length < 0) throw new ArgumentOutOfRangeException("offset and length must be non-negative");
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    if (offset > stream.Length || length > stream.Length - offset) throw new ArgumentOutOfRangeException("file range is outside the file");
    stream.Position = offset; var bytes = new byte[length]; var read = 0;
    while (read < bytes.Length) { var count = stream.Read(bytes, read, bytes.Length - read); if (count == 0) break; read += count; }
    return Text(EncodingOf(thread, args, 3).GetString(bytes, 0, read));
  });
  private static IHixValue WriteFile(HixThread thread, IHixValue[] args) => Mutate(() => {
    var path = ResolveNative(thread, args[0]); var text = thread.ResolveText(args[1]); var encoding = EncodingOf(thread, args, 2);
    var append = ((BooleanHixValue)args[3]).Value;
    if (append) File.AppendAllText(path, text, encoding); else File.WriteAllText(path, text, encoding);
  });
  private static int ExactInt(HixThread thread, IHixValue value, string name) {
    var number = ((NumberHixValue)value).Value;
    if (number != Math.Truncate(number) || number < int.MinValue || number > int.MaxValue)
      throw new ArgumentOutOfRangeException(name + " must be an integer");
    return (int)number;
  }
  private static IHixValue Try(HixThread thread, Func<IHixValue> action) {
    try { return action(); }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) {
      return thread.Error(exception.Message);
    }
  }
  private static IHixValue Mutate(Action action) {
    try { action(); return BooleanHixValue.True; }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) {
      return BooleanHixValue.False;
    }
  }
}
