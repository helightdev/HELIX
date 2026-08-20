using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Feature.Services.Generate.Actions;
using JetBrains.ReSharper.Feature.Services.Generate.Workflows;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Resources;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Resources.Icons;

namespace HelixRider;

internal static class HelixGeneratorKinds {
  public const string MixinHooks = "Helix.MixinHooks";
  public const string EventHandlers = "Helix.EventHandlers";
}

internal static class HelixProjectAvailability {
  public static bool IsAvailable(IDataContext dataContext) {
    var project = dataContext.GetData(ProjectModelDataConstants.PROJECT);
    return project != null && project.GetSolution().GetComponent<HelixProjectSettings>().IsEnabled;
  }

  public static bool IsAvailable(IPsiModule module) =>
    module.GetSolution().GetComponent<HelixProjectSettings>().IsEnabled;
}

internal static class HelixHookDeclarations {
  public static IMethodDeclaration Create(CSharpElementFactory factory, string hook, IPsiModule module) {
    var mixinMethod = TypeFactory.CreateTypeByCLRName("HELIX.MixinMethod", module);
    var mixinOn = TypeFactory.CreateTypeByCLRName("HELIX.MixinOn", module);

    var methodName = "On" + hook;
    if (hook == "ConfigureComponent")
      return (IMethodDeclaration)factory.CreateTypeMemberDeclaration(
        "[$3($4.$0)] private void $1($2 registration) {}",
        hook,
        methodName,
        TypeFactory.CreateTypeByCLRName("HELIX.Context.ComponentRegistration", module),
        mixinMethod,
        mixinOn
      );
    if (hook is "Init" or "LoadComponent" or "LoadComponentLate")
      return (IMethodDeclaration)factory.CreateTypeMemberDeclaration(
        "[$3($4.$0)] private void $1($2 context) {}",
        hook,
        methodName,
        TypeFactory.CreateTypeByCLRName("HELIX.Context.ComponentLoadContext", module),
        mixinMethod,
        mixinOn
      );
    return (IMethodDeclaration)factory.CreateTypeMemberDeclaration(
      "[$2($3.$0)] private void $1() {}",
      hook,
      methodName,
      mixinMethod,
      mixinOn
    );
  }
}

[GenerateProvider]
public sealed class HelixGenerateWorkflowProvider : IGenerateWorkflowProvider {
  public IEnumerable<IGenerateActionWorkflow> CreateWorkflow(IDataContext dataContext) {
    yield return new HelixGenerateWorkflow(
      HelixGeneratorKinds.MixinHooks,
      "Mixin Methods",
      "Generate.HelixMixinHooks",
      110
    );
    yield return new HelixGenerateWorkflow(
      HelixGeneratorKinds.EventHandlers,
      "Event Handlers",
      "Generate.HelixEventHandlers",
      111
    );
  }
}

public sealed class HelixGenerateWorkflow : GenerateCodeWorkflowBase {
  private readonly double _order;

  public HelixGenerateWorkflow(string kind, string title, string actionId, double order)
    : base(
      kind,
      PsiFeaturesUnsortedThemedIcons.FuncZoneGenerate.Id,
      title,
      GenerateActionGroup.CLR_LANGUAGE,
      title,
      string.Empty,
      actionId
    ) {
    _order = order;
  }

  public override double Order => _order;

  public override bool IsAvailable(IDataContext dataContext) =>
    HelixProjectAvailability.IsAvailable(dataContext) && base.IsAvailable(dataContext);
}

[GeneratorElementProvider(HelixGeneratorKinds.MixinHooks, typeof(CSharpLanguage))]
public sealed class HelixMixinHookGeneratorProvider : GeneratorProviderBase<CSharpGeneratorContext> {
  private static readonly string[] Hooks = {
    "Init",
    "Dispose",
    "ConfigureComponent",
    "LoadComponent",
    "LoadComponentLate",
    "UnloadComponent",
    "MonoAwake",
    "MonoStart",
    "MonoReset",
    "MonoUpdate",
    "MonoFixedUpdate",
    "MonoLateUpdate",
    "MonoDestroy",
    "MonoEnable",
    "MonoDisable",
    "MonoValidate",
  };

  public override void Populate(CSharpGeneratorContext context) {
    var declaredType = context.ClassDeclaration.DeclaredElement;
    if (declaredType == null)
      return;
    var existingNames = new HashSet<string>(declaredType.Methods.Select(method => method.ShortName));
    var factory = CSharpElementFactory.GetInstance(context.ClassDeclaration);
    foreach (var hook in Hooks) {
      var methodName = "On" + hook;
      if (existingNames.Contains(methodName))
        continue;
      var declaration = HelixHookDeclarations.Create(factory, hook, context.PsiModule);
      var method = declaration.DeclaredElement;
      if (method != null)
        context.ProvidedElements.Add(new GeneratorDeclaredElement(method, method.IdSubstitution, declaredType));
    }
  }
}

[GeneratorBuilder(HelixGeneratorKinds.MixinHooks, typeof(CSharpLanguage))]
public sealed class HelixMixinHookGeneratorBuilder : GeneratorBuilderBase<CSharpGeneratorContext> {
  protected override bool IsAvailable(CSharpGeneratorContext context) =>
    HelixProjectAvailability.IsAvailable(context.PsiModule) && base.IsAvailable(context);

  protected override void Process(CSharpGeneratorContext context, IProgressIndicator progress) {
    var factory = CSharpElementFactory.GetInstance(context.ClassDeclaration);
    foreach (var element in context.InputElements.OfType<GeneratorDeclaredElement>()) {
      if (element.DeclaredElement is not IMethod method || !method.ShortName.StartsWith("On"))
        continue;
      var hook = method.ShortName.Substring(2);
      var declaration = HelixHookDeclarations.Create(factory, hook, context.PsiModule);
      declaration.FormatNode(CodeFormatProfile.GENERATOR);
      context.PutMemberDeclaration(declaration);
    }
  }
}

[GeneratorElementProvider(HelixGeneratorKinds.EventHandlers, typeof(CSharpLanguage))]
public sealed class HelixEventHandlerGeneratorProvider : GeneratorProviderBase<CSharpGeneratorContext> {
  private readonly ISymbolCache _symbolCache;

  public HelixEventHandlerGeneratorProvider(ISymbolCache symbolCache) {
    _symbolCache = symbolCache;
  }

  public override void Populate(CSharpGeneratorContext context) {
    var scope = _symbolCache.GetSymbolScope(LibrarySymbolScope.FULL, true);
    foreach (var eventType in scope.GetPossibleInheritors("Evt")
      .OfType<ITypeElement>()
      .Where(IsHelixEvent)
      .OrderBy(type => type.ShortName)) {
      context.ProvidedElements.Add(
        new GeneratorDeclaredElement(
          eventType,
          eventType.IdSubstitution,
          eventType.GetContainingType()
        )
      );
    }
  }

  private static bool IsHelixEvent(ITypeElement type) => type.GetAllSuperTypes().Any(superType =>
    superType.GetTypeElement()?.GetClrName().FullName == "HELIX.Context.Evt"
  );
}

[GeneratorBuilder(HelixGeneratorKinds.EventHandlers, typeof(CSharpLanguage))]
public sealed class HelixEventHandlerGeneratorBuilder : GeneratorBuilderBase<CSharpGeneratorContext> {
  protected override bool IsAvailable(CSharpGeneratorContext context) =>
    HelixProjectAvailability.IsAvailable(context.PsiModule) && base.IsAvailable(context);

  protected override void Process(CSharpGeneratorContext context, IProgressIndicator progress) {
    var factory = CSharpElementFactory.GetInstance(context.ClassDeclaration);
    foreach (var element in context.InputElements.OfType<GeneratorDeclaredElement>()) {
      if (element.DeclaredElement is not ITypeElement eventType) continue;
      var eventHandler = TypeFactory.CreateTypeByCLRName("HELIX.Context.EventHandler", context.PsiModule);
      var declaration = (IMethodDeclaration)factory.CreateTypeMemberDeclaration(
        "[$2] private void $0($1 evt) {}",
        "On" + eventType.ShortName,
        TypeFactory.CreateType(eventType),
        eventHandler
      );
      declaration.FormatNode(CodeFormatProfile.GENERATOR);
      context.PutMemberDeclaration(declaration);
    }
  }
}