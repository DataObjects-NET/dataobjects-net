// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.SchemaSharing.SharedCacheTestModel;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.SharedCacheTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string StorageNodeMark { get; private set; }

    public TestEntity(Session session)
      : base(session)
    {
      StorageNodeMark = Session.StorageNodeId;
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.SchemaSharing
{
  [TestFixture]
  public class SharedCacheTest
  {
    private const string AdditionalNodeId = "Addtional";
    private const int DefaultNodeEntityCount = 3;
    private const int AdditionalNodeEntityCount = 5;

    private string additionalNodeSchema;
    private string defaultSchema;


    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      Require.AnyFeatureSupported(Orm.Providers.ProviderFeatures.Multischema);

      defaultSchema = StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.PostgreSql)
        ? WellKnownSchemas.PgSqlDefalutSchema
        : WellKnownSchemas.SqlServerDefaultSchema;
      additionalNodeSchema = WellKnownSchemas.Schema1;
    }

    [Test]
    [TestCase(true, true, 4, TestName = "SharedCacheOn")]
    [TestCase(true, false, 4 * 2, TestName = "OnlyParameters")]
    [TestCase(false, true, 4 * 2, TestName = "OnlySharedSchema")]
    public void MainTest(bool preferParameter, bool sharedSchame, int expectedCacheItems)
    {
      using (var domain = Domain.Build(BuildDomainConfiguration(preferParameter, sharedSchame))) {
        _ = domain.StorageNodeManager.AddNode(BuildNodeConfiguration());

        PopulateDefaultNode(domain);
        PopilateAdditionalNode(domain);

        RunQueries(domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId), DefaultNodeEntityCount);
        RunQueries(domain.StorageNodeManager.GetNode(AdditionalNodeId), AdditionalNodeEntityCount);

        Assert.That(domain.QueryCache.Count, Is.EqualTo(expectedCacheItems));
      }
    }

    private void RunQueries(StorageNode node, int expectedCount)
    {
      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {

        //method as key for cache
        var results = session.Query.Execute(q => q.All<TestEntity>());
        var rowCount = 0;
        foreach (var result in results) {
          Assert.That(result.StorageNodeMark, Is.EqualTo(node.Id));
          rowCount++;
        }
        Assert.That(rowCount, Is.EqualTo(expectedCount));

        var delayedResults = session.Query.CreateDelayedQuery(q => q.All<TestEntity>());
        rowCount = 0;
        foreach (var result in delayedResults) {
          Assert.That(result.StorageNodeMark, Is.EqualTo(node.Id));
          rowCount++;
        }
        Assert.That(rowCount, Is.EqualTo(expectedCount));

        // custom keys
        results = session.Query.Execute("fancyCustomKeyExec", q => q.All<TestEntity>());
        rowCount = 0;
        foreach (var result in results) {
          Assert.That(result.StorageNodeMark, Is.EqualTo(node.Id));
          rowCount++;
        }
        Assert.That(rowCount, Is.EqualTo(expectedCount));

        delayedResults = session.Query.CreateDelayedQuery("fancyCustomKeyDelayed", q => q.All<TestEntity>());
        rowCount = 0;
        foreach (var result in delayedResults) {
          Assert.That(result.StorageNodeMark, Is.EqualTo(node.Id));
          rowCount++;
        }
        Assert.That(rowCount, Is.EqualTo(expectedCount));
        tx.Complete();
      }
    }

    private static void PopulateDefaultNode(Domain domain) =>
      PopulateNode(domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId), DefaultNodeEntityCount);

    private static void PopilateAdditionalNode(Domain domain) =>
      PopulateNode(domain.StorageNodeManager.GetNode(AdditionalNodeId), AdditionalNodeEntityCount);

    private static void PopulateNode(StorageNode node, int itemCount)
    {
      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {
        for(var i = 0; i < itemCount; i++) {
          _ = new TestEntity(session);
        }
        tx.Complete();
      }
    }

    private DomainConfiguration BuildDomainConfiguration(bool preferParamters, bool shareSchema)
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestEntity));
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.DefaultSchema = defaultSchema;

      domainConfig.PreferTypeIdsAsQueryParameters = preferParamters;
      domainConfig.ShareStorageSchemaOverNodes = shareSchema;

      return domainConfig;
    }

    private NodeConfiguration BuildNodeConfiguration()
    {
      var nodeConfig = new NodeConfiguration(AdditionalNodeId) {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      nodeConfig.SchemaMapping.Add(defaultSchema, additionalNodeSchema);

      return nodeConfig;
    }
  }
}
