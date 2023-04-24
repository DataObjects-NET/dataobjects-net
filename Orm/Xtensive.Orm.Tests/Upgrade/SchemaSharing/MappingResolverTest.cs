// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.02

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Info;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.MappingResolverTestModel;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.MappingResolverTestModel
{
  namespace Part1
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string Name { get; set; }
    }
  }

  namespace Part2
  {
    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string Name { get; set; }
    }
  }

  namespace Part3
  {
    [HierarchyRoot]
    public class TestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string Name { get; set; }
    }
  }

  namespace Part4
  {
    [HierarchyRoot]
    public class TestEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string Name { get; set; }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing
{
  [TestFixture]
  public class MappingResolverTest
  {
    private const string DOTests1Db = WellKnownDatabases.MultiDatabase.AdditionalDb1;
    private const string DOTests2Db = WellKnownDatabases.MultiDatabase.AdditionalDb2;
    private const string DOTests3Db = WellKnownDatabases.MultiDatabase.AdditionalDb3;
    private const string DOTests4Db = WellKnownDatabases.MultiDatabase.AdditionalDb4;

    private const string Schema1 = WellKnownSchemas.Schema1;
    private const string Schema2 = WellKnownSchemas.Schema2;
    private const string Schema3 = WellKnownSchemas.Schema3;
    private const string Schema4 = WellKnownSchemas.Schema4;

    private SqlDriver driver;

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      driver = TestSqlDriver.Create(DomainConfigurationFactory.Create().ConnectionInfo);
    }

    [Test]
    public void SimpleMappingResolverTest()
    {
      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfiguration.Types.Register(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace);
      using (var domain = Domain.Build(domainConfiguration)) {
        var node = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
        var nodeConfiguration = node.Configuration;

        DefaultSchemaInfo defaultSchemaInfo;
        using (var connection = driver.CreateConnection()) {
          connection.Open();
          defaultSchemaInfo = driver.GetDefaultSchema(connection);
        }
        var mappingResolver = MappingResolver.Create(domainConfiguration, nodeConfiguration, defaultSchemaInfo);
        Assert.That(mappingResolver, Is.InstanceOf<SimpleMappingResolver>());

        var metadataExtactionTasks = mappingResolver.GetMetadataTasks();
        Assert.That(metadataExtactionTasks.Count(), Is.EqualTo(1));
        Assert.That(metadataExtactionTasks.First().Catalog, Is.EqualTo(defaultSchemaInfo.Database));
        Assert.That(metadataExtactionTasks.First().Schema, Is.EqualTo(defaultSchemaInfo.Schema));

        var schemaExtractionTasks = mappingResolver.GetSchemaTasks();
        Assert.That(schemaExtractionTasks.SequenceEqual(metadataExtactionTasks));

        SchemaExtractionResult extractionResult;
        using (var connection = driver.CreateConnection()) {
          connection.Open();
          extractionResult = new SchemaExtractionResult(driver.Extract(connection, mappingResolver.GetSchemaTasks()));
        }
        var fullName = mappingResolver.GetNodeName("dummy", "dummy", "Table1");
        Assert.That(fullName.ToLower(), Is.EqualTo("Table1".ToLower()));

        fullName = mappingResolver.GetNodeName(extractionResult.Catalogs.First().DefaultSchema.Tables["TestEntity1"]);
        Assert.That(fullName.ToLower(), Is.EqualTo("TestEntity1".ToLower()));

        var typeInfo = domain.Model.Types[typeof(model.Part1.TestEntity1)];
        fullName = mappingResolver.GetNodeName(typeInfo);
        Assert.That(fullName.ToLower(), Is.EqualTo("TestEntity1".ToLower()));

        var sequence = typeInfo.Hierarchy.Key.Sequence;
        fullName = mappingResolver.GetNodeName(sequence);
        Assert.That(fullName.ToLower(), Is.EqualTo(sequence.MappingName.ToLower()));

        var schema = mappingResolver.ResolveSchema(extractionResult, defaultSchemaInfo.Database, defaultSchemaInfo.Schema);
        Assert.That(schema, Is.EqualTo(extractionResult.Catalogs.First().DefaultSchema));

        var resolveResult = mappingResolver.Resolve(extractionResult, typeInfo.MappingName);
        Assert.That(resolveResult.Name, Is.EqualTo(typeInfo.MappingName));
        Assert.That(resolveResult.Schema, Is.EqualTo(extractionResult.Catalogs.Single().DefaultSchema));
        Assert.That(resolveResult.GetTable(), Is.EqualTo(extractionResult.Catalogs.Single().DefaultSchema.Tables["TestEntity1"]));

        resolveResult = mappingResolver.Resolve(extractionResult, sequence.MappingName);
        Assert.That(resolveResult.Name, Is.EqualTo(sequence.MappingName));
        Assert.That(resolveResult.Schema, Is.EqualTo(extractionResult.Catalogs.Single().DefaultSchema));
      }
    }

    [Test]
    public void MultischemaMappingResolverTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfiguration.Types.Register(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace);

      domainConfiguration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).ToSchema(Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).ToSchema(Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).ToSchema(Schema2);
      domainConfiguration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).ToSchema(Schema2);

      domainConfiguration.DefaultSchema = Schema1;

      var nodeConfiguration = new NodeConfiguration("Additional");
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration.SchemaMapping.Add(Schema1, Schema3);
      nodeConfiguration.SchemaMapping.Add(Schema2, Schema4);

      using (var domain = Domain.Build(domainConfiguration)) {
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration);

        var defaultNodeConfig = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId).Configuration;
        var additionalNodeConfig = domain.StorageNodeManager.GetNode("Additional").Configuration;

        DefaultSchemaInfo defaultSchemaInfo;
        using (var connection = driver.CreateConnection()) {
          connection.Open();
          defaultSchemaInfo = driver.GetDefaultSchema(connection);
        }

        var defaultMappingResolver = MappingResolver.Create(domainConfiguration, defaultNodeConfig, defaultSchemaInfo);
        Assert.That(defaultMappingResolver, Is.InstanceOf<MultischemaMappingResolver>());
        var additionalMappingResolver = MappingResolver.Create(domainConfiguration, additionalNodeConfig, defaultSchemaInfo);
        Assert.That(additionalMappingResolver, Is.InstanceOf<MultischemaMappingResolver>());

        var resolverPerNodeMap = new Dictionary<NodeConfiguration, MappingResolver> {
          { defaultNodeConfig, defaultMappingResolver },
          { additionalNodeConfig, additionalMappingResolver }
        };

        foreach (var pair in resolverPerNodeMap) {
          var nodeConfig = pair.Key;
          var mappingResolver = pair.Value;

          Assert.That(mappingResolver, Is.InstanceOf<MultischemaMappingResolver>());
          var metadataExtactionTasks = mappingResolver.GetMetadataTasks();
          Assert.That(metadataExtactionTasks.Count(), Is.EqualTo(1));
          Assert.That(metadataExtactionTasks.First().Catalog, Is.EqualTo(defaultSchemaInfo.Database));
          Assert.That(metadataExtactionTasks.First().Schema, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1)));

          var schemaExtractionTasks = mappingResolver.GetSchemaTasks();
          Assert.That(schemaExtractionTasks.Count(), Is.EqualTo(2));
          Assert.That(schemaExtractionTasks.Any(t => t.Catalog == defaultSchemaInfo.Database && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);
          Assert.That(schemaExtractionTasks.Any(t => t.Catalog == defaultSchemaInfo.Database && t.Schema == nodeConfig.SchemaMapping.Apply(Schema2)), Is.True);

          SchemaExtractionResult extractionResult;
          using (var connection = driver.CreateConnection()) {
            connection.Open();
            extractionResult = new SchemaExtractionResult(driver.Extract(connection, mappingResolver.GetSchemaTasks()));
          }

          var fullName = mappingResolver.GetNodeName("dummy", "SchemaName", "Table1");
          Assert.That(fullName, Is.EqualTo("SchemaName:Table1"));

          fullName = mappingResolver.GetNodeName("dummy", Schema1, "Table1");
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1) + ":Table1"));

          var productTable = extractionResult.Catalogs.First().Schemas[nodeConfig.SchemaMapping.Apply(Schema1)].Tables["TestEntity1"];
          fullName = mappingResolver.GetNodeName(productTable);
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1) + ":TestEntity1"));

          var currencyTable = extractionResult.Catalogs.First().Schemas[nodeConfig.SchemaMapping.Apply(Schema2)].Tables["TestEntity4"];
          fullName = mappingResolver.GetNodeName(currencyTable);
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema2) + ":TestEntity4"));

          var typeInfo = domain.Model.Types[typeof (model.Part1.TestEntity1)];
          fullName = mappingResolver.GetNodeName(typeInfo);
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1) + ":TestEntity1"));

          var resolveResult = mappingResolver.Resolve(extractionResult, fullName);
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1)));

          var schema = mappingResolver.ResolveSchema(extractionResult, typeInfo.MappingDatabase, typeInfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1)));

          typeInfo = domain.Model.Types[typeof (model.Part4.TestEntity4)];
          fullName = mappingResolver.GetNodeName(typeInfo);
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema2) + ":TestEntity4"));

          resolveResult = mappingResolver.Resolve(extractionResult, fullName);
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema2)));

          schema = mappingResolver.ResolveSchema(extractionResult, typeInfo.MappingDatabase, typeInfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema2)));

          var sequence = typeInfo.Hierarchy.Key.Sequence;
          fullName = mappingResolver.GetNodeName(sequence);
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1) + ":" + sequence.MappingName));
        }
      }
    }

    [Test]
    public void MultischemaMappingResolverOnSharedSchemaTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

      domainConfiguration.Types.Register(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace);

      domainConfiguration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).ToSchema(Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).ToSchema(Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).ToSchema(Schema2);
      domainConfiguration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).ToSchema(Schema2);

      domainConfiguration.DefaultSchema = Schema1;

      var nodeConfiguration = new NodeConfiguration("Additional");
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration.SchemaMapping.Add(Schema1, Schema3);
      nodeConfiguration.SchemaMapping.Add(Schema2, Schema4);

      using (var domain = Domain.Build(domainConfiguration)) {
        domain.StorageNodeManager.AddNode(nodeConfiguration);

        var defaultNodeConfig = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId).Configuration;
        var additionalNodeConfig = domain.StorageNodeManager.GetNode("Additional").Configuration;

        DefaultSchemaInfo defaultSchemaInfo;
        using (var connection = driver.CreateConnection()) {
          connection.Open();
          defaultSchemaInfo = driver.GetDefaultSchema(connection);
        }

        var defaultMappingResolver = MappingResolver.Create(domainConfiguration, defaultNodeConfig, defaultSchemaInfo);
        Assert.That(defaultMappingResolver, Is.InstanceOf<MultischemaMappingResolver>());
        var additionalMappingResolver = MappingResolver.Create(domainConfiguration, additionalNodeConfig, defaultSchemaInfo);
        Assert.That(additionalMappingResolver, Is.InstanceOf<MultischemaMappingResolver>());

        SchemaExtractionResult extractionResult;
        using (var connection = driver.CreateConnection()) {
          connection.Open();
          extractionResult = new SchemaExtractionResult(driver.Extract(connection, defaultMappingResolver.GetSchemaTasks()));
        }
        _ = extractionResult.MakeShared();

        var resolverPerNodeMap = new Dictionary<NodeConfiguration, MappingResolver> {
          { defaultNodeConfig, defaultMappingResolver },
          { additionalNodeConfig, additionalMappingResolver }
        };

        foreach (var pair in resolverPerNodeMap) {
          var nodeConfig = pair.Key;
          var mappingResolver = pair.Value;

          Assert.That(mappingResolver, Is.InstanceOf<MultischemaMappingResolver>());
          var metadataExtactionTasks = mappingResolver.GetMetadataTasks();
          Assert.That(metadataExtactionTasks.Count(), Is.EqualTo(1));
          Assert.That(metadataExtactionTasks.First().Catalog, Is.EqualTo(defaultSchemaInfo.Database));
          Assert.That(metadataExtactionTasks.First().Schema, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1)));

          var schemaExtractionTasks = mappingResolver.GetSchemaTasks();
          Assert.That(schemaExtractionTasks.Count(), Is.EqualTo(2));
          Assert.That(schemaExtractionTasks.Any(t => t.Catalog == defaultSchemaInfo.Database && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);
          Assert.That(schemaExtractionTasks.Any(t => t.Catalog == defaultSchemaInfo.Database && t.Schema == nodeConfig.SchemaMapping.Apply(Schema2)), Is.True);

          var fullName = mappingResolver.GetNodeName("dummy", "SchemaName", "Table1");
          Assert.That(fullName, Is.EqualTo("SchemaName:Table1"));

          fullName = mappingResolver.GetNodeName("dummy", Schema1, "Table1");
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1) + ":Table1"));

          var table = extractionResult.Catalogs.First().Schemas[Schema1].Tables["TestEntity1"];
          fullName = mappingResolver.GetNodeName(table);
          Assert.That(fullName, Is.EqualTo(Schema1 + ":TestEntity1"));

          table = extractionResult.Catalogs.First().Schemas[Schema2].Tables["TestEntity4"];
          fullName = mappingResolver.GetNodeName(table);
          Assert.That(fullName, Is.EqualTo(Schema2 + ":TestEntity4"));

          var typeInfo = domain.Model.Types[typeof(model.Part1.TestEntity1)];
          fullName = mappingResolver.GetNodeName(typeInfo);
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1) + ":TestEntity1"));

          var resolveResult = mappingResolver.Resolve(extractionResult, fullName);
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(Schema1));

          var schema = mappingResolver.ResolveSchema(extractionResult, typeInfo.MappingDatabase, typeInfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(Schema1));

          typeInfo = domain.Model.Types[typeof(model.Part4.TestEntity4)];
          fullName = mappingResolver.GetNodeName(typeInfo);
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema2) + ":TestEntity4"));

          resolveResult = mappingResolver.Resolve(extractionResult, fullName);
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(Schema2));

          schema = mappingResolver.ResolveSchema(extractionResult, typeInfo.MappingDatabase, typeInfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(Schema2));

          var sequence = typeInfo.Hierarchy.Key.Sequence;
          fullName = mappingResolver.GetNodeName(sequence);
          Assert.That(fullName, Is.EqualTo(nodeConfig.SchemaMapping.Apply(Schema1) + ":" + sequence.MappingName));

          resolveResult = mappingResolver.Resolve(extractionResult, fullName);
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(Schema1));
        }
      }
    }

    [Mute]
    [Test]
    public void MultidatebaseMappingResolverTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);

      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfiguration.Types.Register(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace);

      _ = domainConfiguration.Databases.Add(DOTests1Db);
      _ = domainConfiguration.Databases.Add(DOTests2Db);

      domainConfiguration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).To(DOTests1Db, Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).To(DOTests1Db, Schema2);
      domainConfiguration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).To(DOTests2Db, Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).To(DOTests2Db, Schema2);

      domainConfiguration.DefaultDatabase = DOTests1Db;
      domainConfiguration.DefaultSchema = Schema1;

      var nodeConfiguration = new NodeConfiguration("Additional");
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration.DatabaseMapping.Add(DOTests1Db, DOTests3Db);
      nodeConfiguration.DatabaseMapping.Add(DOTests2Db, DOTests4Db);
      nodeConfiguration.SchemaMapping.Add(Schema1, Schema3);
      nodeConfiguration.SchemaMapping.Add(Schema2, Schema4);


      using (var domain = Domain.Build(domainConfiguration)) {
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration);

        var defaultNodeConfig = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId).Configuration;
        var additionalNodeConfig = domain.StorageNodeManager.GetNode("Additional").Configuration;

        DefaultSchemaInfo defaultSchemaInfo;
        using (var connection = driver.CreateConnection()) {
          connection.Open();
          defaultSchemaInfo = driver.GetDefaultSchema(connection);
        }

        var defaultMappingResolver = MappingResolver.Create(domainConfiguration, defaultNodeConfig, defaultSchemaInfo);
        Assert.That(defaultMappingResolver, Is.InstanceOf<MultidatabaseMappingResolver>());

        var additionalMappingResolver = MappingResolver.Create(domainConfiguration, additionalNodeConfig, defaultSchemaInfo);
        Assert.That(additionalMappingResolver, Is.InstanceOf<MultidatabaseMappingResolver>());

        var resolverPerNodeMap = new Dictionary<NodeConfiguration, MappingResolver> {
          { defaultNodeConfig, defaultMappingResolver },
          { additionalNodeConfig, additionalMappingResolver }
        };

        var baseDb1 = DOTests1Db;
        var baseDb2 = DOTests2Db;
        var baseSch1 = Schema1;
        var baseSch2 = Schema2;
        foreach (var pair in resolverPerNodeMap) {
          var nodeConfig = pair.Key;
          var resolver = pair.Value;

          var tasks = resolver.GetSchemaTasks();
          Assert.That(tasks.Count(), Is.EqualTo(4));
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests1Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests1Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema2)), Is.True);
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests2Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests2Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema2)), Is.True);

          tasks = resolver.GetMetadataTasks();
          Assert.That(tasks.Count(), Is.EqualTo(2));
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests1Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests2Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);

          SchemaExtractionResult extractionResult;
          using (var connection = driver.CreateConnection()) {
            connection.Open();
            extractionResult = new SchemaExtractionResult(driver.Extract(connection, resolver.GetSchemaTasks()));
          }

          var fullName = resolver.GetNodeName("TestDb", "TestSchema", "TestEntity1");
          Assert.That(fullName, Is.EqualTo("TestDb:TestSchema:TestEntity1"));

          var expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb1);
          var expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch1);

          fullName = resolver.GetNodeName(DOTests1Db, Schema1, "TestEntity1");
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity1", expectedDatabase, expectedSchema)));

          var table = extractionResult
            .Catalogs[nodeConfig.DatabaseMapping.Apply(baseDb1)]
            .Schemas[nodeConfig.SchemaMapping.Apply(baseSch1)]
            .Tables["TestEntity1"];
          fullName = resolver.GetNodeName(table);

          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity1", expectedDatabase, expectedSchema)));

          table = extractionResult
            .Catalogs[nodeConfig.DatabaseMapping.Apply(baseDb1)]
            .Schemas[nodeConfig.SchemaMapping.Apply(baseSch2)]
            .Tables["TestEntity2"];
          fullName = resolver.GetNodeName(table);
          expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb1);
          expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch2);
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity2", expectedDatabase, expectedSchema)));

          table = extractionResult
            .Catalogs[nodeConfig.DatabaseMapping.Apply(baseDb2)]
            .Schemas[nodeConfig.SchemaMapping.Apply(baseSch1)]
            .Tables["TestEntity3"];
          expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb2);
          expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch1);
          fullName = resolver.GetNodeName(table);
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity3", expectedDatabase, expectedSchema)));

          var typeinfo = domain.Model.Types[typeof(model.Part1.TestEntity1)];
          expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb1);
          expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch1);
          fullName = resolver.GetNodeName(typeinfo);
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity1", expectedDatabase, expectedSchema)));

          var resolveResult = resolver.Resolve(extractionResult, fullName);
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(resolveResult.Schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));

          var schema = resolver.ResolveSchema(extractionResult, typeinfo.MappingDatabase, typeinfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));

          typeinfo = domain.Model.Types[typeof(model.Part2.TestEntity2)];
          expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb1);
          expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch2);
          fullName = resolver.GetNodeName(typeinfo);
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity2", expectedDatabase, expectedSchema)));

          resolveResult = resolver.Resolve(extractionResult, fullName);
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(resolveResult.Schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));

          schema = resolver.ResolveSchema(extractionResult, typeinfo.MappingDatabase, typeinfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));


          typeinfo = domain.Model.Types[typeof(model.Part3.TestEntity3)];
          expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb2);
          expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch1);
          fullName = resolver.GetNodeName(typeinfo);
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity3", expectedDatabase, expectedSchema)));

          resolveResult = resolver.Resolve(extractionResult, fullName);
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(resolveResult.Schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));

          schema = resolver.ResolveSchema(extractionResult, typeinfo.MappingDatabase, typeinfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));
        }
      }
    }

    [Mute]
    [Test]
    public void MultidatabaseMappingResolverOnSharedSchemaTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);

      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfiguration.Types.Register(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace);
      domainConfiguration.Types.Register(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace);

      _ = domainConfiguration.Databases.Add(DOTests1Db);
      _ = domainConfiguration.Databases.Add(DOTests2Db);

      domainConfiguration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).To(DOTests1Db, Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).To(DOTests1Db, Schema2);
      domainConfiguration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).To(DOTests2Db, Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).To(DOTests2Db, Schema2);

      domainConfiguration.DefaultDatabase = DOTests1Db;
      domainConfiguration.DefaultSchema = Schema1;

      var nodeConfiguration = new NodeConfiguration("Additional");
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfiguration.DatabaseMapping.Add(DOTests1Db, DOTests3Db);
      nodeConfiguration.DatabaseMapping.Add(DOTests2Db, DOTests4Db);
      nodeConfiguration.SchemaMapping.Add(Schema1, Schema3);
      nodeConfiguration.SchemaMapping.Add(Schema2, Schema4);


      using (var domain = Domain.Build(domainConfiguration)) {
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration);

        var defaultNodeConfig = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId).Configuration;
        var additionalNodeConfig = domain.StorageNodeManager.GetNode("Additional").Configuration;

        DefaultSchemaInfo defaultSchemaInfo;
        using (var connection = driver.CreateConnection()) {
          connection.Open();
          defaultSchemaInfo = driver.GetDefaultSchema(connection);
        }

        var defaultMappingResolver = MappingResolver.Create(domainConfiguration, defaultNodeConfig, defaultSchemaInfo);
        Assert.That(defaultMappingResolver, Is.InstanceOf<MultidatabaseMappingResolver>());

        var additionalMappingResolver = MappingResolver.Create(domainConfiguration, additionalNodeConfig, defaultSchemaInfo);
        Assert.That(additionalMappingResolver, Is.InstanceOf<MultidatabaseMappingResolver>());

        SchemaExtractionResult extractionResult;
        using (var connection = driver.CreateConnection()) {
          connection.Open();
          extractionResult = new SchemaExtractionResult(driver.Extract(connection, defaultMappingResolver.GetSchemaTasks()));
        }
        _ = extractionResult.MakeShared();

        var resolverPerNodeMap = new Dictionary<NodeConfiguration, MappingResolver> {
          { defaultNodeConfig, defaultMappingResolver },
          { additionalNodeConfig, additionalMappingResolver }
        };

        var baseDb1 = DOTests1Db;
        var baseDb2 = DOTests2Db;
        var baseSch1 = Schema1;
        var baseSch2 = Schema2;
        foreach (var pair in resolverPerNodeMap) {
          var nodeConfig = pair.Key;
          var resolver = pair.Value;

          var tasks = resolver.GetSchemaTasks();
          Assert.That(tasks.Count(), Is.EqualTo(4));
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests1Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests1Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema2)), Is.True);
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests2Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests2Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema2)), Is.True);

          tasks = resolver.GetMetadataTasks();
          Assert.That(tasks.Count(), Is.EqualTo(2));
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests1Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);
          Assert.That(tasks.Any(t => t.Catalog == nodeConfig.DatabaseMapping.Apply(DOTests2Db) && t.Schema == nodeConfig.SchemaMapping.Apply(Schema1)), Is.True);


          var fullName = resolver.GetNodeName("TestDb", "TestSchema", "TestEntity1");
          Assert.That(fullName, Is.EqualTo("TestDb:TestSchema:TestEntity1"));

          var expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb1);
          var expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch1);

          fullName = resolver.GetNodeName(DOTests1Db, Schema1, "TestEntity1");
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity1", expectedDatabase, expectedSchema)));


          var table = extractionResult
            .Catalogs[baseDb1]
            .Schemas[baseSch1]
            .Tables["TestEntity1"];
          fullName = resolver.GetNodeName(table);
          expectedDatabase = baseDb1;
          expectedSchema = baseSch1;
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity1", expectedDatabase, expectedSchema)));

          table = extractionResult
            .Catalogs[baseDb1]
            .Schemas[baseSch2]
            .Tables["TestEntity2"];
          fullName = resolver.GetNodeName(table);
          expectedDatabase = baseDb1;
          expectedSchema = baseSch2;
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity2", expectedDatabase, expectedSchema)));

          table = extractionResult
            .Catalogs[baseDb2]
            .Schemas[baseSch1]
            .Tables["TestEntity3"];
          expectedDatabase = baseDb2;
          expectedSchema = baseSch1;
          fullName = resolver.GetNodeName(table);
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity3", expectedDatabase, expectedSchema)));

          var typeinfo = domain.Model.Types[typeof(model.Part1.TestEntity1)];
          expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb1);
          expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch1);
          fullName = resolver.GetNodeName(typeinfo);
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity1", expectedDatabase, expectedSchema)));

          var resolveResult = resolver.Resolve(extractionResult, fullName);
          expectedDatabase = baseDb1;
          expectedSchema = baseSch1;
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(resolveResult.Schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));

          var schema = resolver.ResolveSchema(extractionResult, typeinfo.MappingDatabase, typeinfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));

          typeinfo = domain.Model.Types[typeof(model.Part2.TestEntity2)];
          expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb1);
          expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch2);
          fullName = resolver.GetNodeName(typeinfo);
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity2", expectedDatabase, expectedSchema)));

          resolveResult = resolver.Resolve(extractionResult, fullName);
          expectedDatabase = baseDb1;
          expectedSchema = baseSch2;
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(resolveResult.Schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));

          schema = resolver.ResolveSchema(extractionResult, typeinfo.MappingDatabase, typeinfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));


          typeinfo = domain.Model.Types[typeof(model.Part3.TestEntity3)];
          expectedDatabase = nodeConfig.DatabaseMapping.Apply(baseDb2);
          expectedSchema = nodeConfig.SchemaMapping.Apply(baseSch1);
          fullName = resolver.GetNodeName(typeinfo);
          Assert.That(fullName, Is.EqualTo(string.Format("{0}:{1}:TestEntity3", expectedDatabase, expectedSchema)));

          resolveResult = resolver.Resolve(extractionResult, fullName);
          expectedDatabase = baseDb2;
          expectedSchema = baseSch1;
          Assert.That(resolveResult.Schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(resolveResult.Schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));

          schema = resolver.ResolveSchema(extractionResult, typeinfo.MappingDatabase, typeinfo.MappingSchema);
          Assert.That(schema.GetNameInternal(), Is.EqualTo(expectedSchema));
          Assert.That(schema.Catalog.GetNameInternal(), Is.EqualTo(expectedDatabase));
        }
      }
    }
  }
}
