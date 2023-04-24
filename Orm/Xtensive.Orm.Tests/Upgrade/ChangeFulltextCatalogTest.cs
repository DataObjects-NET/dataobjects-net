// Copyright (C) 2017-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.07.12

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Tests.Upgrade.ChangeFulltextCatalogTestModel;
using System.Threading.Tasks;

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
    public override bool IsEnabled => true;

    protected override string Build(TypeInfo typeInfo,
      string databaseName, string schemaName, string tableName) => $"{databaseName}_{schemaName}";
  }

}

namespace Xtensive.Orm.Tests.Upgrade
{
  [Category("FTS")]
  [TestFixture]
  public class ChangeFulltextCatalogTest
  {
    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.AllFeaturesSupported(ProviderFeatures.FullText);
    }

    [Test]
    public void Test01()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain.Build(configuration).Dispose();
    }

    [Mute]
    [Test]
    public async Task AsyncTest01()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      (await Domain.BuildAsync(configuration)).Dispose();
    }

    [Test]
    public void Test02()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);
    }

    [Mute]
    [Test]
    public void AsyncTest02()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      var exception = Assert.ThrowsAsync<SchemaSynchronizationException>(async () => await Domain.BuildAsync(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);
    }

    [Test]
    public void Test03()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);
    }

    [Mute]
    [Test]
    public void AsyncTest03()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      var exception = Assert.ThrowsAsync<SchemaSynchronizationException>(async () => await Domain.BuildAsync(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);
    }

    [Test]
    public void Test04()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Perform, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain.Build(configuration).Dispose();
    }

    [Mute]
    [Test]
    public async Task AsyncTest04()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Perform, true);
      (await Domain.BuildAsync(configuration)).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain.Build(configuration).Dispose();
    }

    [Test]
    public void Test05()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Perform, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain.Build(configuration).Dispose();
    }

    [Test]
    public async Task AsyncTest05()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Perform, false);
      (await Domain.BuildAsync(configuration)).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain.Build(configuration).Dispose();
    }

    [Test]
    public void Test06()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.PerformSafely, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain.Build(configuration).Dispose();
    }

    [Test]
    public async Task AsyncTest06()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.PerformSafely, true);
      (await Domain.BuildAsync(configuration)).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain.Build(configuration).Dispose();
    }

    [Test]
    public void Test07()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.PerformSafely, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain.Build(configuration).Dispose();
    }

    [Test]
    public async Task AsyncTest07()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.PerformSafely, false);
      (await Domain.BuildAsync(configuration)).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      Domain.Build(configuration).Dispose();
    }

    [Test]
    public void Test08()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.LegacyValidate, false);
      Domain.Build(configuration).Dispose();
    }

    [Test]
    public async Task AsyncTest08()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.LegacyValidate, false);
      (await Domain.BuildAsync(configuration)).Dispose();
    }

    [Test]
    public void Test09()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);
    }

    [Test]
    public void AsyncTest09()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, true);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, false);
      var exception = Assert.ThrowsAsync<SchemaSynchronizationException>(async () => await Domain.BuildAsync(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);
    }

    [Test]
    public void Test10()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);
    }

    [Test]
    public void AsyncTest10()
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, false);
      Domain.Build(configuration).Dispose();

      configuration = BuildConfiguration(DomainUpgradeMode.Validate, true);
      var exception = Assert.ThrowsAsync<SchemaSynchronizationException>(async () => await Domain.BuildAsync(configuration));
      Assert.That(exception.ComparisonResult.SchemaComparisonStatus, Is.EqualTo(SchemaComparisonStatus.TargetIsSuperset));
      Assert.That(exception.ComparisonResult.Difference.HasChanges, Is.True);
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
