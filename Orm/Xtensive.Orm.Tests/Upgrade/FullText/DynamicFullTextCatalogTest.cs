// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
using System.Threading.Tasks;

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

    public override bool IsEnabled => true;

    protected override string Build(TypeInfo typeInfo, string databaseName, string schemaName, string tableName) =>
      CatalogNameTemplate.Replace("{catalog}", databaseName).Replace("{schema}", schemaName);
  }

  public class CustomUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    public override void OnComplete(Domain domain)
    {
      domain.Extensions.Set(UpgradeContext.TargetStorageModel);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  [Category("FTS")]
  [TestFixture]
  public class DynamicFullTextCatalogTest
  {
    private const string DOTestsDb = WellKnownDatabases.MultiDatabase.MainDb;
    private const string DOTests1Db = WellKnownDatabases.MultiDatabase.AdditionalDb1;
    private const string DOTests2Db = WellKnownDatabases.MultiDatabase.AdditionalDb2;

    private const string dbo = WellKnownSchemas.SqlServerDefaultSchema;
    private const string Schema1 = WellKnownSchemas.Schema1;
    private const string Schema2 = WellKnownSchemas.Schema2;

    [OneTimeSetUp]
    public void TestFixtureSetUp() => Require.AllFeaturesSupported(ProviderFeatures.FullText);

    [Test]
    public void NoResolverTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));

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
    public async Task NoResolverAsyncTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = await Domain.BuildAsync(configuration)) {
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
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();
        var table = targetStorageModel.Tables["TestEntity"];
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        var database = domain.StorageProviderInfo.DefaultDatabase;
        var schema = domain.StorageProviderInfo.DefaultSchema;
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{database}_{schema}"));
      }
    }

    [Test]
    public async Task SingleSchemaAsyncTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = await Domain.BuildAsync(configuration)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();
        var table = targetStorageModel.Tables["TestEntity"];
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        var database = domain.StorageProviderInfo.DefaultDatabase;
        var schema = domain.StorageProviderInfo.DefaultSchema;
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{database}_{schema}"));
      }
    }

    [Test]
    public void SingleSchemaWithDatabaseSwitchTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));
      configuration.ConnectionInitializationSql = $"USE [{DOTests1Db}]";
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();
        var table = targetStorageModel.Tables["TestEntity"];
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{dbo}"));
      }
    }

    [Test]
    public async Task SingleSchemaWithDatabaseSwitchAsyncTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));
      configuration.ConnectionInitializationSql = $"USE [{DOTests1Db}]";
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = await Domain.BuildAsync(configuration)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();
        var table = targetStorageModel.Tables["TestEntity"];
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{dbo}"));
      }
    }

    [Test]
    public void MultischemaTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      var defaultSchemaType = typeof(Database1.Default.TestEntity1);
      var model1Type = typeof(Database1.Model1.TestEntity2);
      var model2Type = typeof(Database1.Model2.TestEntity3);

      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(defaultSchemaType);
      configuration.Types.Register(model1Type);
      configuration.Types.Register(model2Type);
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = dbo;

      configuration.MappingRules.Map(defaultSchemaType.Assembly, defaultSchemaType.Namespace)
        .ToSchema(dbo);
      configuration.MappingRules.Map(model1Type.Assembly, model1Type.Namespace)
        .ToSchema(WellKnownSchemas.Schema1);
      configuration.MappingRules.Map(model2Type.Assembly, model2Type.Namespace)
        .ToSchema(WellKnownSchemas.Schema2);

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var database = domain.StorageProviderInfo.DefaultDatabase;

        var targetModel = domain.Extensions.Get<StorageModel>();
        var defaultSchemaTable = targetModel.Tables[$"{dbo}:TestEntity1"];
        var ftIndex = defaultSchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{database}_{dbo}"));

        var model1SchemaTable = targetModel.Tables[$"{Schema1}:TestEntity2"];
        ftIndex = model1SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{database}_{Schema1}"));

        var model2SchemaTable = targetModel.Tables[$"{Schema2}:TestEntity3"];
        ftIndex = model2SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{database}_{Schema2}"));
      }
    }

    [Test]
    public async Task MultischemaAsyncTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      var defaultSchemaType = typeof(Database1.Default.TestEntity1);
      var model1Type = typeof(Database1.Model1.TestEntity2);
      var model2Type = typeof(Database1.Model2.TestEntity3);

      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(defaultSchemaType);
      configuration.Types.Register(model1Type);
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(model2Type);
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = dbo;

      configuration.MappingRules.Map(defaultSchemaType.Assembly, defaultSchemaType.Namespace)
        .ToSchema(dbo);
      configuration.MappingRules.Map(model1Type.Assembly, model1Type.Namespace)
        .ToSchema(WellKnownSchemas.Schema1);
      configuration.MappingRules.Map(model2Type.Assembly, model2Type.Namespace)
        .ToSchema(WellKnownSchemas.Schema2);

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = await Domain.BuildAsync(configuration)) {
        var database = domain.StorageProviderInfo.DefaultDatabase;

        var targetModel = domain.Extensions.Get<StorageModel>();
        var defaultSchemaTable = targetModel.Tables[$"{dbo}:TestEntity1"];
        var ftIndex = defaultSchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{database}_{dbo}"));

        var model1SchemaTable = targetModel.Tables["Model1:TestEntity2"];
        ftIndex = model1SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{database}_{Schema1}"));

        var model2SchemaTable = targetModel.Tables["Model2:TestEntity3"];
        ftIndex = model2SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{database}_{Schema2}"));
      }
    }

    [Test]
    public void MultidatabaseTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
      var db1DefaultSchemaType = typeof(Database1.Default.TestEntity1);
      var db1Model1SchemaType = typeof(Database1.Model1.TestEntity2);
      var db1Model2SchemaType = typeof(Database1.Model2.TestEntity3);

      var db2DefaultSchemaType = typeof(Database2.Default.TestEntity4);
      var db2Model1SchemaType = typeof(Database2.Model1.TestEntity5);
      var db2Model2SchemaType = typeof(Database2.Model2.TestEntity6);

      var configuragtion = DomainConfigurationFactory.Create();
      configuragtion.Types.Register(db1DefaultSchemaType);
      configuragtion.Types.Register(db1Model1SchemaType);
      configuragtion.Types.Register(db1Model2SchemaType);

      configuragtion.Types.Register(db2DefaultSchemaType);
      configuragtion.Types.Register(db2Model1SchemaType);
      configuragtion.Types.Register(db2Model2SchemaType);

      configuragtion.Types.Register(typeof(CustomUpgradeHandler));
      configuragtion.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuragtion.DefaultDatabase = DOTestsDb;
      configuragtion.DefaultSchema = dbo;

      configuragtion.MappingRules.Map(db1DefaultSchemaType.Assembly, db1DefaultSchemaType.Namespace)
        .To(DOTestsDb, dbo);
      configuragtion.MappingRules.Map(db1Model1SchemaType.Assembly, db1Model1SchemaType.Namespace)
        .To(DOTestsDb, WellKnownSchemas.Schema1);
      configuragtion.MappingRules.Map(db1Model2SchemaType.Assembly, db1Model2SchemaType.Namespace)
        .To(DOTestsDb, WellKnownSchemas.Schema2);

      configuragtion.MappingRules.Map(db2DefaultSchemaType.Assembly, db2DefaultSchemaType.Namespace)
        .To(DOTests1Db, dbo);
      configuragtion.MappingRules.Map(db2Model1SchemaType.Assembly, db2Model1SchemaType.Namespace)
        .To(DOTests1Db, WellKnownSchemas.Schema1);
      configuragtion.MappingRules.Map(db2Model2SchemaType.Assembly, db2Model2SchemaType.Namespace)
        .To(DOTests1Db, WellKnownSchemas.Schema2);

      configuragtion.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuragtion)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();

        var db1DefaultSchemaTable = targetStorageModel
          .Tables[$"{DOTestsDb}:{dbo}:TestEntity1"];
        Assert.That(db1DefaultSchemaTable, Is.Not.Null);
        var ftIndex = db1DefaultSchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{dbo}"));

        var db1Model1SchemaTable = targetStorageModel
          .Tables[$"{DOTestsDb}:{Schema1}:TestEntity2"];
        ftIndex = db1Model1SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{Schema1}"));

        var db1Model2SchemaTable = targetStorageModel
          .Tables[$"{DOTestsDb}:{Schema2}:TestEntity3"];
        ftIndex = db1Model2SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{Schema2}"));

        var db2DefaultSchemaTable = targetStorageModel
          .Tables[$"{DOTests1Db}:{dbo}:TestEntity4"];
        ftIndex = db2DefaultSchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{dbo}"));

        var db2Model1SchemaTable = targetStorageModel.Tables[$"{DOTests1Db}:{Schema1}:TestEntity5"];
        ftIndex = db2Model1SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{Schema1}"));

        var db2Model2SchemaTable = targetStorageModel.Tables[$"{DOTests1Db}:{Schema2}:TestEntity6"];
        ftIndex = db2Model2SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{Schema2}"));
      }
    }

    [Test]
    public async Task MultidatabaseAsyncTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
      var db1DefaultSchemaType = typeof(Database1.Default.TestEntity1);
      var db1Model1SchemaType = typeof(Database1.Model1.TestEntity2);
      var db1Model2SchemaType = typeof(Database1.Model2.TestEntity3);

      var db2DefaultSchemaType = typeof(Database2.Default.TestEntity4);
      var db2Model1SchemaType = typeof(Database2.Model1.TestEntity5);
      var db2Model2SchemaType = typeof(Database2.Model2.TestEntity6);

      var configuragtion = DomainConfigurationFactory.Create();
      configuragtion.Types.Register(db1DefaultSchemaType);
      configuragtion.Types.Register(db1Model1SchemaType);
      configuragtion.Types.Register(db1Model2SchemaType);

      configuragtion.Types.Register(db2DefaultSchemaType);
      configuragtion.Types.Register(db2Model1SchemaType);
      configuragtion.Types.Register(db2Model2SchemaType);

      configuragtion.Types.Register(typeof(CustomUpgradeHandler));
      configuragtion.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuragtion.DefaultDatabase = DOTestsDb;
      configuragtion.DefaultSchema = dbo;

      configuragtion.MappingRules.Map(db1DefaultSchemaType.Assembly, db1DefaultSchemaType.Namespace)
        .To(DOTestsDb, dbo);
      configuragtion.MappingRules.Map(db1Model1SchemaType.Assembly, db1Model1SchemaType.Namespace)
        .To(DOTestsDb, WellKnownSchemas.Schema1);
      configuragtion.MappingRules.Map(db1Model2SchemaType.Assembly, db1Model2SchemaType.Namespace)
        .To(DOTestsDb, WellKnownSchemas.Schema2);

      configuragtion.MappingRules.Map(db2DefaultSchemaType.Assembly, db2DefaultSchemaType.Namespace)
        .To(DOTests1Db, dbo);
      configuragtion.MappingRules.Map(db2Model1SchemaType.Assembly, db2Model1SchemaType.Namespace)
        .To(DOTests1Db, WellKnownSchemas.Schema1);
      configuragtion.MappingRules.Map(db2Model2SchemaType.Assembly, db2Model2SchemaType.Namespace)
        .To(DOTests1Db, WellKnownSchemas.Schema2);

      configuragtion.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = await Domain.BuildAsync(configuragtion)) {
        var targetStorageModel = domain.Extensions.Get<StorageModel>();

        var db1DefaultSchemaTable = targetStorageModel.Tables[$"{DOTestsDb}:{dbo}:TestEntity1"];
        Assert.That(db1DefaultSchemaTable, Is.Not.Null);
        var ftIndex = db1DefaultSchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{dbo}"));

        var db1Model1SchemaTable = targetStorageModel.Tables[$"{DOTestsDb}:{Schema1}:TestEntity2"];
        ftIndex = db1Model1SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{Schema1}"));

        var db1Model2SchemaTable = targetStorageModel.Tables[$"{DOTestsDb}:{Schema2}:TestEntity3"];
        ftIndex = db1Model2SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{Schema2}"));

        var db2DefaultSchemaTable = targetStorageModel.Tables[$"{DOTests1Db}:{dbo}:TestEntity4"];
        ftIndex = db2DefaultSchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{dbo}"));

        var db2Model1SchemaTable = targetStorageModel.Tables[$"{DOTests1Db}:{Schema1}:TestEntity5"];
        ftIndex = db2Model1SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{Schema1}"));

        var db2Model2SchemaTable = targetStorageModel.Tables[$"{DOTests1Db}:{Schema2}:TestEntity6"];
        ftIndex = db2Model2SchemaTable.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{Schema2}"));
      }
    }

    [Test]
    public void MultinodeTest1()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = dbo;

      var nodeConfiguration1 = new NodeConfiguration("AdditionalNode1");
      nodeConfiguration1.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration1.SchemaMapping.Add(dbo, WellKnownSchemas.Schema1);

      var nodeConfiguration2 = new NodeConfiguration("AdditionalNode2");
      nodeConfiguration2.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration2.SchemaMapping.Add(dbo, WellKnownSchemas.Schema2);

      using (var domain = Domain.Build(configuration)) {
        var domainStorageModel = domain.Extensions.Get<StorageModel>();
        var table = domainStorageModel.Tables[$"{dbo}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{dbo}"));

        domain.Extensions.Clear();
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration1);
        var node1StorageModel = domain.Extensions.Get<StorageModel>();
        table = node1StorageModel.Tables[$"{Schema1}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{Schema1}"));

        domain.Extensions.Clear();
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration2);
        var node2StorageModel = domain.Extensions.Get<StorageModel>();
        table = node2StorageModel.Tables[$"{Schema2}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{Schema2}"));
      }
    }

    [Test]
    public async Task MultinodeAsyncTest1()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = dbo;

      var nodeConfiguration1 = new NodeConfiguration("AdditionalNode1");
      nodeConfiguration1.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration1.SchemaMapping.Add(dbo, WellKnownSchemas.Schema1);

      var nodeConfiguration2 = new NodeConfiguration("AdditionalNode2");
      nodeConfiguration2.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration2.SchemaMapping.Add(dbo, WellKnownSchemas.Schema2);

      using (var domain = await Domain.BuildAsync(configuration)) {
        var domainStorageModel = domain.Extensions.Get<StorageModel>();
        var table = domainStorageModel.Tables[$"{dbo}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{dbo}"));

        domain.Extensions.Clear();
        _ = await domain.StorageNodeManager.AddNodeAsync(nodeConfiguration1);
        var node1StorageModel = domain.Extensions.Get<StorageModel>();
        table = node1StorageModel.Tables[$"{Schema1}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{Schema1}"));

        domain.Extensions.Clear();
        _ = await domain.StorageNodeManager.AddNodeAsync(nodeConfiguration2);
        var node2StorageModel = domain.Extensions.Get<StorageModel>();
        table = node2StorageModel.Tables[$"{Schema2}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{Schema2}"));
      }
    }

    [Test]
    public void MultinodeTest2()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = dbo;
      configuration.DefaultDatabase = DOTestsDb;

      var nodeConfiguration1 = new NodeConfiguration("AdditionalNode1");
      nodeConfiguration1.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration1.SchemaMapping.Add(dbo, WellKnownSchemas.Schema1);
      nodeConfiguration1.DatabaseMapping.Add(DOTestsDb, DOTests1Db);

      var nodeConfiguration2 = new NodeConfiguration("AdditionalNode2");
      nodeConfiguration2.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration2.SchemaMapping.Add(dbo, WellKnownSchemas.Schema2);
      nodeConfiguration2.DatabaseMapping.Add(DOTestsDb, DOTests2Db);

      using (var domain = Domain.Build(configuration)) {
        var domainStorageModel = domain.Extensions.Get<StorageModel>();
        var table = domainStorageModel.Tables[$"{DOTestsDb}:{dbo}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{dbo}"));

        domain.Extensions.Clear();
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration1);
        var node1StorageModel = domain.Extensions.Get<StorageModel>();
        table = node1StorageModel.Tables[$"{DOTests1Db}:{Schema1}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{Schema1}"));

        domain.Extensions.Clear();
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration2);
        var node2StorageModel = domain.Extensions.Get<StorageModel>();
        table = node2StorageModel.Tables[$"{DOTests2Db}:{Schema2}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests2Db}_{Schema2}"));
      }
    }

    [Test]
    public async Task MultinodeAsyncTest2()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = dbo;
      configuration.DefaultDatabase = DOTestsDb;

      var nodeConfiguration1 = new NodeConfiguration("AdditionalNode1");
      nodeConfiguration1.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration1.SchemaMapping.Add(dbo, WellKnownSchemas.Schema1);
      nodeConfiguration1.DatabaseMapping.Add(DOTestsDb, DOTests1Db);

      var nodeConfiguration2 = new NodeConfiguration("AdditionalNode2");
      nodeConfiguration2.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration2.SchemaMapping.Add(dbo, WellKnownSchemas.Schema2);
      nodeConfiguration2.DatabaseMapping.Add(DOTestsDb, DOTests2Db);

      using (var domain = await Domain.BuildAsync(configuration)) {
        var domainStorageModel = domain.Extensions.Get<StorageModel>();
        var table = domainStorageModel.Tables[$"{DOTestsDb}:{dbo}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{dbo}"));

        domain.Extensions.Clear();
        _ = await domain.StorageNodeManager.AddNodeAsync(nodeConfiguration1);
        var node1StorageModel = domain.Extensions.Get<StorageModel>();
        table = node1StorageModel.Tables[$"{DOTests1Db}:{Schema1}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{Schema1}"));

        domain.Extensions.Clear();
        _ = await domain.StorageNodeManager.AddNodeAsync(nodeConfiguration2);
        var node2StorageModel = domain.Extensions.Get<StorageModel>();
        table = node2StorageModel.Tables[$"{DOTests2Db}:{Schema2}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests2Db}_{Schema2}"));
      }
    }

    [Test]
    public void MultinodeTest3()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = dbo;

      var nodeConfiguration1 = new NodeConfiguration("AdditionalNode1");
      nodeConfiguration1.ConnectionInitializationSql = $"USE [{DOTests1Db}]";
      nodeConfiguration1.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration1.SchemaMapping.Add(dbo, WellKnownSchemas.Schema1);

      var nodeConfiguration2 = new NodeConfiguration("AdditionalNode2");
      nodeConfiguration2.ConnectionInitializationSql = $"USE [{DOTests2Db}]";
      nodeConfiguration2.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration2.SchemaMapping.Add(dbo, WellKnownSchemas.Schema2);

      using (var domain = Domain.Build(configuration)) {
        var domainStorageModel = domain.Extensions.Get<StorageModel>();
        var table = domainStorageModel.Tables[$"{dbo}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{dbo}"));

        domain.Extensions.Clear();
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration1);
        var node1StorageModel = domain.Extensions.Get<StorageModel>();
        table = node1StorageModel.Tables[$"{Schema1}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{Schema1}"));

        domain.Extensions.Clear();
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration2);
        var node2StorageModel = domain.Extensions.Get<StorageModel>();
        table = node2StorageModel.Tables[$"{Schema2}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests2Db}_{Schema2}"));
      }
    }

    [Test]
    public async Task MultinodeAsyncTest3()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgradeHandler));
      configuration.Types.Register(typeof(CustomFullTextCatalogNameBuilder));

      configuration.DefaultSchema = dbo;

      var nodeConfiguration1 = new NodeConfiguration("AdditionalNode1");
      nodeConfiguration1.ConnectionInitializationSql = $"USE [{DOTests1Db}]";
      nodeConfiguration1.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration1.SchemaMapping.Add(dbo, WellKnownSchemas.Schema1);

      var nodeConfiguration2 = new NodeConfiguration("AdditionalNode2");
      nodeConfiguration2.ConnectionInitializationSql = $"USE [{DOTests2Db}]";
      nodeConfiguration2.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration2.SchemaMapping.Add(dbo, WellKnownSchemas.Schema2);

      using (var domain = await Domain.BuildAsync(configuration)) {
        var domainStorageModel = domain.Extensions.Get<StorageModel>();
        var table = domainStorageModel.Tables[$"{dbo}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        var ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTestsDb}_{dbo}"));

        domain.Extensions.Clear();
        _ = await domain.StorageNodeManager.AddNodeAsync(nodeConfiguration1);
        var node1StorageModel = domain.Extensions.Get<StorageModel>();
        table = node1StorageModel.Tables[$"{Schema1}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests1Db}_{Schema1}"));

        domain.Extensions.Clear();
        _ = await domain.StorageNodeManager.AddNodeAsync(nodeConfiguration2);
        var node2StorageModel = domain.Extensions.Get<StorageModel>();
        table = node2StorageModel.Tables[$"{Schema2}:TestEntity"];
        Assert.That(table, Is.Not.Null);
        ftIndex = table.FullTextIndexes[0];
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.Not.Null);
        Assert.That(ftIndex.FullTextCatalog, Is.EqualTo($"{DOTests2Db}_{Schema2}"));
      }
    }
  }
}
