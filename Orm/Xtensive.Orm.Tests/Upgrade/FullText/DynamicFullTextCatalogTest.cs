// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.07.12

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Tests.Upgrade.DynamicFulltextCatalogTestModel;
using Xtensive.Orm.Upgrade.Model;
using Database1 = Xtensive.Orm.Tests.Upgrade.DynamicFulltextCatalogTestModel.Database1;
using Database2 = Xtensive.Orm.Tests.Upgrade.DynamicFulltextCatalogTestModel.Database2;

namespace Xtensive.Orm.Tests.Upgrade.DynamicFulltextCatalogTestModel
{
  namespace Database1
  {
    namespace Default
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
    }

    namespace Model1
    {
      [HierarchyRoot]
      public class TestEntity2 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        [FullText("English")]
        public string Text { get; set; }
      }
    }

    namespace Model2
    {
      [HierarchyRoot]
      public class TestEntity3 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        [FullText("English")]
        public string Text { get; set; }
      }
    }
  }

  namespace Database2
  {
    namespace Default
    {
      [HierarchyRoot]
      public class TestEntity4 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        [FullText("English")]
        public string Text { get; set; }
      }
    }

    namespace Model1
    {
      [HierarchyRoot]
      public class TestEntity5 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        [FullText("English")]
        public string Text { get; set; }
      }
    }

    namespace Model2
    {
      [HierarchyRoot]
      public class TestEntity6 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        [FullText("English")]
        public string Text { get; set; }
      }
    }
  }

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
    private const string CatalogNameTemplate = "{catalog}_{schema}";

    public override bool IsEnabled
    {
      get { return true; }
    }

    protected override string Build(TypeInfo typeInfo, string databaseName, string schemaName, string tableName)
    {
      return CatalogNameTemplate.Replace("{catalog}", databaseName).Replace("{schema}", schemaName);
    }
  }

  public class CustomUpgradeHandler : UpgradeHandler
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

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class DynamicFullTextCatalogTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Require.AllFeaturesSupported(ProviderFeatures.FullText);
    }

    [Test]
    public void NoResolverTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity));
      configuration.Types.Register(typeof (CustomUpgradeHandler));

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();
        var table = targetStorageModel.Tables["TestEntity"];
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Null);
      }
    }

    [Test]
    public void SingleSchemaTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity));
      configuration.Types.Register(typeof (CustomUpgradeHandler));
      configuration.Types.Register(typeof (CustomFullTextCatalogNameBuilder));

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();
        var table = targetStorageModel.Tables["TestEntity"];
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        var database = domain.StorageProviderInfo.DefaultDatabase;
        var schema = domain.StorageProviderInfo.DefaultSchema;
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo(string.Format("{0}_{1}", database, schema)));
      }
    }

    [Test]
    public void SingleSchemaWithDatabaseSwitchTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity));
      configuration.Types.Register(typeof (CustomUpgradeHandler));
      configuration.Types.Register(typeof (CustomFullTextCatalogNameBuilder));
      configuration.ConnectionInitializationSql = "USE [DO-Tests-1]";
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();
        var table = targetStorageModel.Tables["TestEntity"];
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests-1_dbo"));
      }
    }

    [Test]
    public void MultischemaTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      var defaultSchemaType = typeof (Database1.Default.TestEntity1);
      var model1Type = typeof (Database1.Model1.TestEntity2);
      var model2Type = typeof (Database1.Model2.TestEntity3);

      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(defaultSchemaType);
      configuration.Types.Register(model1Type);
      configuration.Types.Register(model2Type);
      configuration.Types.Register(typeof (CustomUpgradeHandler));
      configuration.Types.Register(typeof (CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = "dbo";

      configuration.MappingRules.Map(defaultSchemaType.Assembly, defaultSchemaType.Namespace).ToSchema("dbo");
      configuration.MappingRules.Map(model1Type.Assembly, model1Type.Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(model2Type.Assembly, model2Type.Namespace).ToSchema("Model2");

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var database = domain.StorageProviderInfo.DefaultDatabase;

        var targetModel = domain.Extensions.Get<StorageModel>();
        var defaultSchemaTable = targetModel.Tables["dbo:TestEntity1"];
        var ftIndex = defaultSchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo(string.Format("{0}_dbo", database)));

        var model1SchemaTable = targetModel.Tables["Model1:TestEntity2"];
        ftIndex = model1SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo(string.Format("{0}_Model1", database)));

        var model2SchemaTable = targetModel.Tables["Model2:TestEntity3"];
        ftIndex = model2SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo(string.Format("{0}_Model2", database)));
      }
    }

    [Test]
    public void MultidatabaseTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
      var db1DefaultSchemaType = typeof (Database1.Default.TestEntity1);
      var db1Model1SchemaType = typeof (Database1.Model1.TestEntity2);
      var db1Model2SchemaType = typeof (Database1.Model2.TestEntity3);

      var db2DefaultSchemaType = typeof (Database2.Default.TestEntity4);
      var db2Model1SchemaType = typeof (Database2.Model1.TestEntity5);
      var db2Model2SchemaType = typeof (Database2.Model2.TestEntity6);

      var configuragtion = DomainConfigurationFactory.Create();
      configuragtion.Types.Register(db1DefaultSchemaType);
      configuragtion.Types.Register(db1Model1SchemaType);
      configuragtion.Types.Register(db1Model2SchemaType);

      configuragtion.Types.Register(db2DefaultSchemaType);
      configuragtion.Types.Register(db2Model1SchemaType);
      configuragtion.Types.Register(db2Model2SchemaType);

      configuragtion.Types.Register(typeof (CustomUpgradeHandler));
      configuragtion.Types.Register(typeof (CustomFullTextCatalogNameBuilder));

      configuragtion.DefaultDatabase = "DO-Tests";
      configuragtion.DefaultSchema = "dbo";

      configuragtion.MappingRules.Map(db1DefaultSchemaType.Assembly, db1DefaultSchemaType.Namespace).To("DO-Tests", "dbo");
      configuragtion.MappingRules.Map(db1Model1SchemaType.Assembly, db1Model1SchemaType.Namespace).To("DO-Tests", "Model1");
      configuragtion.MappingRules.Map(db1Model2SchemaType.Assembly, db1Model2SchemaType.Namespace).To("DO-Tests", "Model2");

      configuragtion.MappingRules.Map(db2DefaultSchemaType.Assembly, db2DefaultSchemaType.Namespace).To("DO-Tests-1", "dbo");
      configuragtion.MappingRules.Map(db2Model1SchemaType.Assembly, db2Model1SchemaType.Namespace).To("DO-Tests-1", "Model1");
      configuragtion.MappingRules.Map(db2Model2SchemaType.Assembly, db2Model2SchemaType.Namespace).To("DO-Tests-1", "Model2");

      configuragtion.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuragtion)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();

        var db1DefaultSchemaTable = targetStorageModel.Tables["DO-Tests:dbo:TestEntity1"];
        Assert.That(db1DefaultSchemaTable, Is.Not.Null);
        var ftIndex = db1DefaultSchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests_dbo"));

        var db1Model1SchemaTable = targetStorageModel.Tables["DO-Tests:Model1:TestEntity2"];
        ftIndex = db1Model1SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests_Model1"));

        var db1Model2SchemaTable = targetStorageModel.Tables["DO-Tests:Model2:TestEntity3"];
        ftIndex = db1Model2SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests_Model2"));

        var db2DefaultSchemaTable = targetStorageModel.Tables["DO-Tests-1:dbo:TestEntity4"];
        ftIndex = db2DefaultSchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests-1_dbo"));

        var db2Model1SchemaTable = targetStorageModel.Tables["DO-Tests-1:Model1:TestEntity5"];
        ftIndex = db2Model1SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests-1_Model1"));

        var db2Model2SchemaTable = targetStorageModel.Tables["DO-Tests-1:Model2:TestEntity6"];
        ftIndex = db2Model2SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests-1_Model2"));
      }
    }

    [Test]
    public void MultinodeTest1()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (TestEntity));
      configuration.Types.Register(typeof (CustomUpgradeHandler));
      configuration.Types.Register(typeof (CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = "dbo";

      var nodeConfiguration1 = new NodeConfiguration("AdditionalNode1");
      nodeConfiguration1.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model1");

      var nodeConfiguration2 = new NodeConfiguration("AdditionalNode2");
      nodeConfiguration2.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration2.SchemaMapping.Add("dbo", "Model2");

      using (var domain = Domain.Build(configuration)) {
        var domainStorageModel = domain.Extensions.Get<StorageModel>();
        var table = domainStorageModel.Tables["dbo:TestEntity"];
        Assert.That(table, Is.Not.Null);
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests_dbo"));

        domain.Extensions.Clear();
        domain.StorageNodeManager.AddNode(nodeConfiguration1);
        var node1StorageModel = domain.Extensions.Get<StorageModel>();
        table = node1StorageModel.Tables["Model1:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests_Model1"));

        domain.Extensions.Clear();
        domain.StorageNodeManager.AddNode(nodeConfiguration2);
        var node2StorageModel = domain.Extensions.Get<StorageModel>();
        table = node2StorageModel.Tables["Model2:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests_Model2"));
      }
    }

    [Test]
    public void MultinodeTest2()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (TestEntity));
      configuration.Types.Register(typeof (CustomUpgradeHandler));
      configuration.Types.Register(typeof (CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = "dbo";
      configuration.DefaultDatabase = "DO-Tests";

      var nodeConfiguration1 = new NodeConfiguration("AdditionalNode1");
      nodeConfiguration1.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model1");
      nodeConfiguration1.DatabaseMapping.Add("DO-Tests", "DO-Tests-1");

      var nodeConfiguration2 = new NodeConfiguration("AdditionalNode2");
      nodeConfiguration2.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration2.SchemaMapping.Add("dbo", "Model2");
      nodeConfiguration2.DatabaseMapping.Add("DO-Tests", "DO-Tests-2");

      using (var domain = Domain.Build(configuration)) {
        var domainStorageModel = domain.Extensions.Get<StorageModel>();
        var table = domainStorageModel.Tables["DO-Tests:dbo:TestEntity"];
        Assert.That(table, Is.Not.Null);
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests_dbo"));

        domain.Extensions.Clear();
        domain.StorageNodeManager.AddNode(nodeConfiguration1);
        var node1StorageModel = domain.Extensions.Get<StorageModel>();
        table = node1StorageModel.Tables["DO-Tests-1:Model1:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests-1_Model1"));

        domain.Extensions.Clear();
        domain.StorageNodeManager.AddNode(nodeConfiguration2);
        var node2StorageModel = domain.Extensions.Get<StorageModel>();
        table = node2StorageModel.Tables["DO-Tests-2:Model2:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests-2_Model2"));
      }
    }

    [Test]
    public void MultinodeTest3()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (TestEntity));
      configuration.Types.Register(typeof (CustomUpgradeHandler));
      configuration.Types.Register(typeof (CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = "dbo";

      var nodeConfiguration1 = new NodeConfiguration("AdditionalNode1");
      nodeConfiguration1.ConnectionInitializationSql = "USE [DO-Tests-1]";
      nodeConfiguration1.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model1");

      var nodeConfiguration2 = new NodeConfiguration("AdditionalNode2");
      nodeConfiguration2.ConnectionInitializationSql = "USE [DO-Tests-2]";
      nodeConfiguration2.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration2.SchemaMapping.Add("dbo", "Model2");

      using (var domain = Domain.Build(configuration)) {
        var domainStorageModel = domain.Extensions.Get<StorageModel>();
        var table = domainStorageModel.Tables["dbo:TestEntity"];
        Assert.That(table, Is.Not.Null);
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests_dbo"));

        domain.Extensions.Clear();
        domain.StorageNodeManager.AddNode(nodeConfiguration1);
        var node1StorageModel = domain.Extensions.Get<StorageModel>();
        table = node1StorageModel.Tables["Model1:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests-1_Model1"));

        domain.Extensions.Clear();
        domain.StorageNodeManager.AddNode(nodeConfiguration2);
        var node2StorageModel = domain.Extensions.Get<StorageModel>();
        table = node2StorageModel.Tables["Model2:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo("DO-Tests-2_Model2"));
      }
    }
  }
}
