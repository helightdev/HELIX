using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Helix.Weave.Lowering;

/// <summary>A weave method found on a mixin dataclass.</summary>
public sealed class WeaveMethod {
  public MethodDeclarationSyntax Syntax { get; init; }
  public IMethodSymbol Symbol { get; init; }
  public WeavePass Pass { get; init; }
  /// <summary>Carry type a prelude returns, or null for single-pass.</summary>
  public INamedTypeSymbol Carry { get; init; }
  public string TargetParameterType { get; init; }
}

public enum WeavePass { Single, Prelude, Late }

/// <summary>One <c>[Mixin]</c> dataclass, with everything lowering needs to relocate it.</summary>
public sealed class MixinDeclaration {
  public ClassDeclarationSyntax Syntax { get; init; }
  public INamedTypeSymbol Symbol { get; init; }
  public string Namespace { get; init; }
  public string Name { get; init; }
  public string AttributeMetadataName { get; init; }
  public Unit Unit { get; init; }
  public int Order { get; init; }
  /// <summary>Settable members an attribute argument can bind to: properties and fields alike.</summary>
  public List<ISymbol> Decodable { get; } = new();
  public List<MemberDeclarationSyntax> RelocatedMembers { get; } = new();
  public List<WeaveMethod> Methods { get; } = new();
  public string SourcePath { get; init; }

  /// <summary>File name for the relocated declaration.</summary>
  public string FileName => (Namespace is null ? "" : Namespace + ".") + Name + ".g.cs";
}
