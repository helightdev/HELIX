using HelixRider.Protocol;
using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Protocol;

namespace HelixRider;

[SolutionComponent(Instantiation.DemandAnyThreadSafe)]
public sealed class HelixProjectSettings
{
    private readonly HelixExpressionModel _model;

    public HelixProjectSettings(ISolution solution)
    {
        _model = solution.GetProtocolSolution().GetHelixExpressionModel();
    }

    public bool IsEnabled => _model.IsHelixEnabled.Maybe.ValueOrDefault;
}
