// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.CustomTypeModel;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using English = Xtensive.Orm.Localization.Tests.WellKnownCultures.English;
using Spanish = Xtensive.Orm.Localization.Tests.WellKnownCultures.Spanish;

namespace Xtensive.Orm.Localization.Tests
{
  [TestFixture]
  public class ProjectionToCustomTypesTests : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(ILocalizable<>).Assembly);
      configuration.Types.Register(typeof(Country).Assembly, typeof(Country).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        // populating database
        var m1 = new Country(session) {
          Identifier = "HUN",
          Name = "Magyarország"
        };
        var m2 = new Country(session) {
          Identifier = "RUS",
          Name = "Oroszország"
        };
        using (new LocalizationScope(English.Culture)) {
          m2.Name = "Russia";
        }
        using (new LocalizationScope(Spanish.Culture)) {
          m2.Name = "Rusia";
        }
        ts.Complete();
      }
    }

    [Test]
    public void EntityHierarchyWithAbstractPropertyTest()
    {
      var currentCulture = Thread.CurrentThread.CurrentCulture;
      try {
        Thread.CurrentThread.CurrentCulture = English.Culture;
        using (var session = Domain.OpenSession())
        using (var ts = session.OpenTransaction()) {
          var q = session.Query.All<Country>().OrderBy(e => e.Identifier).Select(e => new { e.Name });
          var l = q.ToList();
          // assertions
          Assert.That(l.Count, Is.EqualTo(2));

          var propertyInfos = l.First().GetType().GetProperties();
          Assert.That(propertyInfos.Length, Is.EqualTo(1));
          Assert.That(propertyInfos.First().Name, Is.EqualTo(nameof(Country.Name)));
          Assert.That(l.First().Name, Is.EqualTo("Magyarország"));
          Assert.That(l.Last().Name, Is.EqualTo("Russia"));
        }
      }
      finally {
        Thread.CurrentThread.CurrentCulture = currentCulture;
      }
    }
  }

  public sealed class CustomTypesUpgradeTest
  {
    [Test]
    [TestCase(DomainUpgradeMode.Skip)]
    [TestCase(DomainUpgradeMode.Validate)]
    [TestCase(DomainUpgradeMode.Recreate)]
    [TestCase(DomainUpgradeMode.Perform)]
    [TestCase(DomainUpgradeMode.PerformSafely)]
    [TestCase(DomainUpgradeMode.LegacySkip)]
    [TestCase(DomainUpgradeMode.LegacyValidate)]
    public void MainTest(DomainUpgradeMode upgradeMode)
    {
      TestDomainBuild(DomainUpgradeMode.Recreate, upgradeMode);
    }

    private void TestDomainBuild(DomainUpgradeMode initialDomainMode, DomainUpgradeMode upgradedDomainMode)
    {
      using (var initialDomain = Domain.Build(BuildConfiguration(initialDomainMode))) {
        using (var session = initialDomain.OpenSession())
        using (var ts = session.OpenTransaction()) {
          // populating database
          var m1 = new Country(session) {
            Identifier = "HUN",
            Name = "Magyarország"
          };
          var m2 = new Country(session) {
            Identifier = "RUS",
            Name = "Oroszország"
          };
          using (new LocalizationScope(English.Culture)) {
            m2.Name = "Russia";
          }
          using (new LocalizationScope(Spanish.Culture)) {
            m2.Name = "Rusia";
          }
          ts.Complete();
        }
      }

      using (var upgradedDomain = Domain.Build(BuildConfiguration(upgradedDomainMode))) {

        var currentCulture = Thread.CurrentThread.CurrentCulture;
        try {
          Thread.CurrentThread.CurrentCulture = English.Culture;
          using (var session = upgradedDomain.OpenSession())
          using (var ts = session.OpenTransaction()) {

            if (upgradedDomainMode == DomainUpgradeMode.Recreate) {
              var q = session.Query.All<Country>().OrderBy(e => e.Identifier).Select(e => new { e.Name });
              var l = q.ToList();
              // assertions
              Assert.That(l.Count, Is.EqualTo(0));
            }
            else {
              var q = session.Query.All<Country>().OrderBy(e => e.Identifier).Select(e => new { e.Name });
              var l = q.ToList();
              // assertions
              Assert.That(l.Count, Is.EqualTo(2));

              var propertyInfos = l.First().GetType().GetProperties();
              Assert.That(propertyInfos.Length, Is.EqualTo(1));
              Assert.That(propertyInfos.First().Name, Is.EqualTo(nameof(Country.Name)));
              Assert.That(l.First().Name, Is.EqualTo("Magyarország"));
              Assert.That(l.Last().Name, Is.EqualTo("Russia"));
            }
          }
        }
        finally {
          Thread.CurrentThread.CurrentCulture = currentCulture;
        }
      }
    }

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(ILocalizable<>).Assembly);
      configuration.Types.Register(typeof(Country).Assembly, typeof(Country).Namespace);
      configuration.UpgradeMode = upgradeMode;
      return configuration;
    }
  }
}