using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ProjectModel.Properties;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Context;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Scope;
using JetBrains.ReSharper.Feature.Services.Protocol;
using HelixRider.Protocol;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Util;

namespace HelixRider;

public sealed class InHelixUnityCSharpProject : InLanguageSpecificProject
{
    private static readonly Guid DefaultUid = new("7EC54B22-CF66-4FB5-B62D-38699822E824");
    private static readonly Guid QuickListUid = new("A5565134-290E-4DC1-8180-6795E4285AE9");

    public InHelixUnityCSharpProject() : base(ProjectLanguage.CSHARP)
    {
        AdditionalSuperTypes.Add(typeof(InLanguageSpecificProject));
    }

    public override Guid GetDefaultUID() => DefaultUid;
    public override string PresentableShortName => "Unity projects using HELIX";
    public override PsiLanguageType RelatedLanguage => CSharpLanguage.Instance;
    public override string QuickListTitle => "HELIX";
    public override Guid QuickListUID => QuickListUid;
}

[ShellComponent(Instantiation.DemandAnyThreadSafe)]
public sealed class HelixTemplateScopeProvider : ScopeProvider
{
    public HelixTemplateScopeProvider()
    {
        Creators.Add(TryToCreate<InHelixUnityCSharpProject>);
    }

    public override IEnumerable<ITemplateScopePoint> ProvideScopePoints(TemplateAcceptanceContext context)
    {
        var project = context.GetProject();
        if (project != null && project.GetSolution().GetProtocolSolution()
                .GetHelixExpressionModel().IsHelixEnabled.Value)
            yield return new InHelixUnityCSharpProject();
    }
}
