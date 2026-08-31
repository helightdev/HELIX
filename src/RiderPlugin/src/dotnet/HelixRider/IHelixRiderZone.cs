using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Resources.Shell;
#if RIDER
using JetBrains.Application.Environment;
using JetBrains.RdBackend.Common.Env;
using JetBrains.Rider.Backend.Env;
#endif

namespace HelixRider
{
    [ZoneDefinition]
    // [ZoneDefinitionConfigurableFeature("Title", "Description", IsInProductSection: false)]
    public interface IHelixRiderZone : IZone,
        IRequire<ILanguageCSharpZone>,
        IRequire<ICodeEditingZone>,
        IRequire<DaemonZone>,
        IRequire<NavigationZone>,
        IRequire<PsiFeaturesImplZone>
    {
    }

    // A zone definition only describes a feature; it does not activate it. Keep
    // this marker in the production assembly so Rider's component catalog can
    // instantiate completion, references, typing assist, and daemon components.
    [ZoneMarker]
    public sealed class HelixRiderZoneMarker : IRequire<IHelixRiderZone>
#if RIDER
        , IRequire<IReSharperHostNetFeatureZone>
#endif
    {
    }

#if RIDER
    // Custom feature zones are not activated merely by being discovered. Rider
    // activates this one only in its full .NET backend environment, matching the
    // activation pattern used by Rider's bundled language plugins.
    [ZoneActivator]
    [ZoneMarker(typeof(IRiderBackendFeatureEnvironmentZone))]
    public sealed class HelixRiderZoneActivator : IActivate<IHelixRiderZone>
    {
    }
#endif
}
