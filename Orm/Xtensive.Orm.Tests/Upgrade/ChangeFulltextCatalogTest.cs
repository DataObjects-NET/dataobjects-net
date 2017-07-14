using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Tests.Upgrade.ChangeFulltextCatalogTestModel;

namespace Xtensive.Orm.Tests.Upgrade.ChangeFulltextCatalogTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    [FullText("English")]
    public string Text { get; set; }
  }

  public class CustomFullTextCatalogNameBuilder : FullTextCatalogNameBuilder
  {
    public override bool IsEnabled
    {
      get { return true; }
    }

    protected override string Build(TypeInfo typeInfo, string databaseName, string schemaName, string tableName)
    {
      return string.Format("{0}_{1}", databaseName, schemaName);
    }
  }

}

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class ChangeFulltextCatalogTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.AllFeaturesSupported(ProviderFeatures.FullText);
    }

    [Test]
    public void Test01()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      using (Domain.Build(configuration)){ }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      using (Domain.Build(configuration)) { }
    }

    [Test]
    public void Test02()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain domain = null;
      var exception = Assert.Throws<SchemaSynchronizationException>(() => domain = Domain.Build(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);

      if (domain!=null)
        domain.Dispose();
    }

    [Test]
    public void Test03()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain domain = null;
      var exception = Assert.Throws<SchemaSynchronizationException>(() => domain = Domain.Build(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);

      if (domain != null)
        domain.Dispose();
    }

    [Test]
    public void Test04()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Perform, true);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      using (Domain.Build(configuration)) { }
    }

    [Test]
    public void Test05()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Perform, false);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      using (Domain.Build(configuration)) { }
    }

    [Test]
    public void Test06()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.PerformSafely, true);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      using (Domain.Build(configuration)) { }
    }

    [Test]
    public void Test07()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.PerformSafely, false);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      using (Domain.Build(configuration)) { }
    }

    [Test]
    public void Test08()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.LegacyValidate, false);
      using (Domain.Build(configuration)) { }
    }

    [Test]
    public void Test09()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain domain = null;
      var exception = Assert.Throws<SchemaSynchronizationException>(() => domain = Domain.Build(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);

      if (domain != null)
        domain.Dispose();
    }

    [Test]
    public void Test10()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      using (Domain.Build(configuration)) { }

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain domain = null;
      var exception = Assert.Throws<SchemaSynchronizationException>(() => domain = Domain.Build(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);

      if (domain != null)
        domain.Dispose();
    }

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode, bool withCustomResolver)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity));
      if (withCustomResolver)
        configuration.Types.Register(typeof (CustomFullTextCatalogNameBuilder));

      configuration.UpgradeMode = upgradeMode;
      return configuration;
    }
  }
}
