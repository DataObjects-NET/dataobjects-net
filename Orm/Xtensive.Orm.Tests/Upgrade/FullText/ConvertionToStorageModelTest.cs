// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.07.17

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Tests.Upgrade.FullText.ConversionToStorageModelModel;

namespace Xtensive.Orm.Tests.Upgrade.FullText.ConversionToStorageModelModel
{
  [HierarchyRoot]
  public class TestEntity1 : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    [FullText("English")]
    public string Text { get; set; }
  }

  [HierarchyRoot]
  public class TestEntity2 : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    [FullText("English")]
    public string Title { get; set; }

    [Field]
    [FullText("German")]
    public string Text { get; set; }
  }

  [HierarchyRoot]
  public class TestEntity3 : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Length = int.MaxValue)]
    [FullText("English", DataTypeField = "DataType")]
    public byte[] Data { get; set; }

    [Field(Length = 30)]
    public string DataType { get; set; }
  }

  public class StorageModelCatcher : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnComplete(Domain domain)
    {
      domain.Extensions.Set(UpgradeContext.TargetStorageModel);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullText
{
  [TestFixture]
  public class ConvertionToStorageModelTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Require.AllFeaturesSupported(ProviderFeatures.FullText);
    }

    [Test]
    public void MainTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity1).Assembly, typeof (TestEntity1).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var storageModel = domain.Extensions.Get<StorageModel>();
        var table = storageModel.Tables["TestEntity1"];
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex.FullTextCatalog, Is.Null);
        Assert.That(ftIndex.ChangeTrackingMode, Is.EqualTo(FullTextChangeTrackingMode.Default));
        Assert.That(ftIndex.Columns.Count, Is.EqualTo(1));
        Assert.That(ftIndex.Columns.All(c => c.Configuration=="English" && c.TypeColumnName==null));

        table = storageModel.Tables["TestEntity2"];
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex.FullTextCatalog, Is.Null);
        Assert.That(ftIndex.ChangeTrackingMode, Is.EqualTo(FullTextChangeTrackingMode.Default));
        Assert.That(ftIndex.Columns.Count, Is.EqualTo(2));
        Assert.That(ftIndex.Columns.Any(c => c.Configuration=="English" && c.TypeColumnName==null));
        Assert.That(ftIndex.Columns.Any(c => c.Configuration=="German" && c.TypeColumnName==null));

        table = storageModel.Tables["TestEntity3"];
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex.FullTextCatalog, Is.Null);
        Assert.That(ftIndex.ChangeTrackingMode, Is.EqualTo(FullTextChangeTrackingMode.Default));
        Assert.That(ftIndex.Columns.Count, Is.EqualTo(1));
        if (domain.StorageProviderInfo.Supports(ProviderFeatures.FullTextColumnDataTypeSpecification))
          Assert.That(ftIndex.Columns.Any(c => c.Configuration=="English" && c.TypeColumnName=="DataType"));
        else
          Assert.That(ftIndex.Columns.Any(c => c.Configuration=="English" && c.TypeColumnName==null));
      }
    }

    [Test]
    public void ChangeTrackingModeTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity1).Assembly, typeof (TestEntity1).Namespace);
      configuration.FullTextChangeTrackingMode = FullTextChangeTrackingMode.Auto;
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var storageModel = domain.Extensions.Get<StorageModel>();
        var ftIndexes = storageModel.Tables.SelectMany(t => t.FullTextIndexes).ToArray();
        Assert.That(ftIndexes.All(i => i.ChangeTrackingMode==FullTextChangeTrackingMode.Auto));
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity1).Assembly, typeof (TestEntity1).Namespace);
      configuration.FullTextChangeTrackingMode = FullTextChangeTrackingMode.Manual;
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var storageModel = domain.Extensions.Get<StorageModel>();
        var ftIndexes = storageModel.Tables.SelectMany(t => t.FullTextIndexes).ToArray();
        Assert.That(ftIndexes.All(i => i.ChangeTrackingMode==FullTextChangeTrackingMode.Manual));
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity1).Assembly, typeof (TestEntity1).Namespace);
      configuration.FullTextChangeTrackingMode = FullTextChangeTrackingMode.Off;
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var storageModel = domain.Extensions.Get<StorageModel>();
        var ftIndexes = storageModel.Tables.SelectMany(t => t.FullTextIndexes).ToArray();
        Assert.That(ftIndexes.All(i => i.ChangeTrackingMode==FullTextChangeTrackingMode.Off));
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity1).Assembly, typeof (TestEntity1).Namespace);
      configuration.FullTextChangeTrackingMode = FullTextChangeTrackingMode.OffWithNoPopulation;
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var storageModel = domain.Extensions.Get<StorageModel>();
        var ftIndexes = storageModel.Tables.SelectMany(t => t.FullTextIndexes).ToArray();
        Assert.That(ftIndexes.All(i => i.ChangeTrackingMode==FullTextChangeTrackingMode.OffWithNoPopulation));
      }
    }
  }
}
