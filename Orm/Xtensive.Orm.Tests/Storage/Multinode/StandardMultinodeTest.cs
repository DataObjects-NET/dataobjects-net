// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2014.03.26

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.Keys;

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  public abstract class StandardMultinodeTest : MultinodeTest
  {
    private const string TestAppleTag = "{0AD2D57A-D2C0-4C42-B13D-6B3A114E0258}";

    private Key testAppleKey;
    private Key testFruitRefKey;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Apple).Assembly, typeof (Apple).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var apple = new Apple(TestAppleTag);
        var refObject = new FruitRef {Ref = apple};
        testAppleKey = apple.Key;
        testFruitRefKey = refObject.Key;
        tx.Complete();
      }
    }

    [Test]
    public void KeyNodeIdAssignTest()
    {
      Assert.That(testAppleKey.NodeId, Is.EqualTo(TestNodeId2));
      Assert.That(testFruitRefKey.NodeId, Is.EqualTo(TestNodeId2));
    }

    [Test]
    public void NodeIdLinq1Test()
    {
      // Entity fetched via LINQ
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<Apple>().Single(a => a.Tag==TestAppleTag);
        Assert.That(result.Key.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdLinq2Test()
    {
      // Key fetched via LINQ
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<Apple>().GroupBy(a => a.Key).Select(a => a.Key).Single();
        Assert.That(result.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdLinq3Test()
    {
      // Key fetched via LINQ
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<FruitRef>().Select(r => r.Ref.Key).Single();
        Assert.That(result.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdLinq4Test()
    {
      // Key fetched via LINQ
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<FruitRef>().GroupBy(a => a.Ref.Key).Select(a => a.Key).Single();
        Assert.That(result.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdPrefetch1Test()
    {
      // Entity fetched via fetch API (key object)
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.Single<Apple>(testAppleKey);
        Assert.That(result.Key.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdPrefetch2Test()
    {
      // Entity fetched via fetch API (key values)
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.Single<Apple>(TestAppleTag);
        Assert.That(result.Key.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdPrefetch3Test()
    {
      // Entity fetched via fetch API (multiple key values)
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.Many<Apple, string>(new[] {TestAppleTag}).ToList();
        Assert.That(result.Count, Is.EqualTo(1));
        var item = result[0];
        Assert.That(item.Key.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdReferenceFetchTest()
    {
      // Entity fetched via reference traversal
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var refObject = session.Query.Single<FruitRef>(testFruitRefKey);
        var result = refObject.Ref.Key;
        Assert.That(result.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void KeyNodeIdGenerateKeyTest()
    {
      // Custom generated key
      var selectedNode = Domain.StorageNodeManager.GetNode(TestNodeId2);
      using (var session = selectedNode.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var generatedKey = Key.Generate<Container>(session);
        Assert.That(generatedKey.NodeId, Is.EqualTo(TestNodeId2));
      }
    }
  }
}