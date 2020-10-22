using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.Multinode.ObsoleteSelectStorageNodeTestModel;

namespace Xtensive.Orm.Tests.Storage.Multinode.ObsoleteSelectStorageNodeTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string NodeTag { get; set; }

    public TestEntity(Session session)
      :base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  //should be removed with the obsolete method
  public class ObsoleteSelectStorageNodeTest : AutoBuildTest
  {
    private const string AdditionalNodeName = "Additional";
    private const string DefaultNodeTag = "<default>";
    private const string AdditionalNodeTag = "<additional>";

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.DefaultSchema = "dbo";
      return configuration;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      var nodeConfig = new NodeConfiguration(AdditionalNodeName) { UpgradeMode = DomainUpgradeMode.Recreate };
      nodeConfig.SchemaMapping.Add("dbo", "Model1");
      _ = domain.StorageNodeManager.AddNode(nodeConfig);
      return domain;
    }

    protected override void PopulateData()
    {
      using (var nodeSession = Domain.OpenSession())
      using (var tx = nodeSession.OpenTransaction()) {
        _ = new TestEntity(nodeSession) { NodeTag = DefaultNodeTag };
        _ = new TestEntity(nodeSession) { NodeTag = DefaultNodeTag };
        _ = new TestEntity(nodeSession) { NodeTag = DefaultNodeTag };
        tx.Complete();
      }

      var additonalNode = Domain.StorageNodeManager.GetNode(AdditionalNodeName);
      using (var nodeSession = additonalNode.OpenSession())
      using (var tx = nodeSession.OpenTransaction()) {
        _ = new TestEntity(nodeSession) { NodeTag = AdditionalNodeTag };
        _ = new TestEntity(nodeSession) { NodeTag = AdditionalNodeTag };
        _ = new TestEntity(nodeSession) { NodeTag = AdditionalNodeTag };
        tx.Complete();
      }

    }
#pragma warning disable CS0618 // Type or member is obsolete

    [Test]
    public void SelectExistingNodeTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode(WellKnown.DefaultNodeId);
        using(var tx = session.OpenTransaction()) {
          var count = 0;
          foreach(var item in session.Query.All<TestEntity>()) {
            Assert.That(item.NodeTag, Is.EqualTo(DefaultNodeTag));
            count++;
          }
          Assert.That(count, Is.GreaterThan(0));
        }
      }

      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode(AdditionalNodeName);
        using (var tx = session.OpenTransaction()) {
          var count = 0;
          foreach (var item in session.Query.All<TestEntity>()) {
            Assert.That(item.NodeTag, Is.EqualTo(AdditionalNodeTag));
            count++;
          }
          Assert.That(count, Is.GreaterThan(0));
        }
      }
    }

    [Test]
    public void SelectNonExistingNodeTest()
    {
      using (var session = Domain.OpenSession()) {
        _ = Assert.Throws<KeyNotFoundException>(() => session.SelectStorageNode(WellKnown.DefaultNodeId + "does not exist"));
      }
    }

    [Test]
    public void ReSelectNodeTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode(WellKnown.DefaultNodeId);
        _ = Assert.Throws<InvalidOperationException>(() => session.SelectStorageNode(WellKnown.DefaultNodeId));
      }

      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode(AdditionalNodeName);

        _ = Assert.Throws<InvalidOperationException>(() => session.SelectStorageNode(AdditionalNodeName));
      }
    }

    [Test]
    public void SessionWithoutSelectedNodeTest()
    {
      using (var session = Domain.OpenSession()) {
        Assert.That(session.GetStorageNodeInternal(), Is.Null);
        using (var tx = session.OpenTransaction()) {
          var count = 0;
          foreach (var item in session.Query.All<TestEntity>()) {
            Assert.That(item.NodeTag, Is.EqualTo(DefaultNodeTag));
            count++;
          }
          Assert.That(count, Is.GreaterThan(0));
        }
        Assert.That(session.GetStorageNodeInternal(), Is.Not.Null);
        Assert.That(session.GetStorageNodeInternal().Id, Is.EqualTo(WellKnown.DefaultNodeId));
      }
    }


#pragma warning restore CS0618 // Type or member is obsolete
  }
}
