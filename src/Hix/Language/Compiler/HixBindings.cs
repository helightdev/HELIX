namespace Hix.Compiler;

public enum HixCallKind { Unbound, Static, Dynamic, Pattern }

/// <summary>Compile-time call semantics. Dynamic dispatch is an explicit bound result.</summary>
public sealed record HixCallBinding {
  private HixCallBinding(HixCallKind kind, SignatureHixPattern signature = null,
    FunctionDeclarationIr function = null, global::Hix.FunctionDefinition backend = null) {
    Kind = kind; Signature = signature; Function = function; Backend = backend;
  }
  public HixCallKind Kind { get; }
  public SignatureHixPattern Signature { get; }
  public FunctionDeclarationIr Function { get; }
  public global::Hix.FunctionDefinition Backend { get; }
  public static readonly HixCallBinding Unbound = new(HixCallKind.Unbound);
  public static readonly HixCallBinding Dynamic = new(HixCallKind.Dynamic);
  public static readonly HixCallBinding Pattern = new(HixCallKind.Pattern);
  public static HixCallBinding Language(SignatureHixPattern signature, FunctionDeclarationIr function) =>
    new(HixCallKind.Static, signature, function);
  public static HixCallBinding Host(SignatureHixPattern signature, global::Hix.FunctionDefinition backend) =>
    new(HixCallKind.Static, signature, backend: backend);
}

public enum HixReferenceKind { Unbound, Root, Local, Parameter, Variable, TargetVariable, Function, Member, Invalid }

/// <summary>Stable identity shared by assignments and references in a lexical body.</summary>
public sealed class HixVariableSymbol(string name, StorageSpace storage) {
  public string Name { get; } = name;
  public StorageSpace Storage { get; } = storage;
}

public sealed record HixReferenceBinding(HixReferenceKind Kind, string Name,
  HixVariableSymbol Variable = null, int ParameterIndex = -1) {
  public static readonly HixReferenceBinding Unbound = new(HixReferenceKind.Unbound, null);
}
