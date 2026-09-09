using System;
using NUnit.Framework;

namespace HELIX.Context.Tests.Discovery {
  public class ModuleDiscoveryTests {
    [Test]
    public void DiscoversExactAttributesInNamespaceAndNestedNamespaces() {
      var registrations = new ManagedRegistrations();
      FeatureModule.Instance.Discover(registrations);
      Assert.That(registrations.components.Keys, Is.EquivalentTo(new[] {
        typeof(Feature.Component), typeof(Feature.Nested.Component), typeof(Feature.Container.NestedComponent)
      }));
      Assert.That(FeatureModule.Instance, Is.InstanceOf<IHelixModule>());
    }

    [Test]
    public void ApplicationImportsModulesAndCreatesIndependentRegistrations() {
      var first = Application.Discover();
      var second = Application.Discover();
      Assert.That(first, Is.Not.SameAs(second));
      Assert.That(first.components.Keys, Is.EquivalentTo(new[] {
        typeof(Feature.Component), typeof(Feature.Nested.Component), typeof(Feature.Container.NestedComponent),
        typeof(Root.Component)
      }));
    }

    [Test]
    public void EmptyFilterScansOnlyCurrentCompilation() {
      var registrations = new ManagedRegistrations();
      AllModule.Instance.Discover(registrations);
      Assert.That(registrations.components.ContainsKey(typeof(Feature.Component)), Is.True);
      Assert.That(registrations.components.ContainsKey(typeof(FeatureExtra.Component)), Is.True);
      Assert.That(registrations.components.ContainsKey(typeof(Feature.DerivedOnly)), Is.False);
      foreach (var type in registrations.components.Keys) Assert.That(type.Assembly, Is.EqualTo(typeof(AllModule).Assembly));
    }
  }

  [HelixModule(filter: "HELIX.Context.Tests.Discovery.Feature")]
  public partial class FeatureModule { }

  [HelixApplication(filter: "HELIX.Context.Tests.Discovery.Root", import: new[] {typeof(FeatureModule)})]
  public partial class Application { }

  [HelixModule]
  public partial class AllModule { }

  public class SpecializedManagedAttribute : ManagedAttribute { }
}

namespace HELIX.Context.Tests.Discovery.Feature {
  [Managed]
  public class Component {
    public static void RegistrationConfigurator(ManagedRegistration registration) { }
  }
  [SpecializedManaged]
  public class DerivedOnly { }
  [Managed]
  public class Generic<T> { }
  public class GenericContainer<T> {
    [Managed] public class Nested { }
  }
  public class Container {
    [Managed] private class Hidden { }
    [Managed] public class NestedComponent {
      public static void RegistrationConfigurator(ManagedRegistration registration) { }
    }
  }
}
namespace HELIX.Context.Tests.Discovery.Feature.Nested {
  [Managed]
  public class Component {
    public static void RegistrationConfigurator(ManagedRegistration registration) { }
  }
}
namespace HELIX.Context.Tests.Discovery.FeatureExtra {
  [Managed]
  public class Component {
    public static void RegistrationConfigurator(ManagedRegistration registration) { }
  }
}
namespace HELIX.Context.Tests.Discovery.Root {
  [Managed]
  public class Component {
    public static void RegistrationConfigurator(ManagedRegistration registration) { }
  }
}
