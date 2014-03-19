// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.14

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.Keys;

namespace Xtensive.Orm.Tests.Storage
{
  public class MultinodeKeysTest
  {
    private const string DefaultSchema = "n1";
    private const string SecondSchema = "n2";
    private const string ThirdSchema = "n3";

    private const string TestNodeId2 = "{4657EED5-AD12-4E3A-9324-BE570C897452}";
    private const string TestNodeId3 = "{5854CFC3-B153-411C-8090-E0369034BDEF}";
    private const string TestAppleTag = "{0AD2D57A-D2C0-4C42-B13D-6B3A114E0258}";

    private Key testAppleKey;
    private Key testFruitRefKey;

    private Domain domain;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (Apple).Assembly, typeof (Apple).Namespace);
      configuration.DefaultSchema = DefaultSchema;

      StorageTestHelper.DemandSchemas(configuration.ConnectionInfo, DefaultSchema, SecondSchema, ThirdSchema);
      domain = Domain.Build(configuration);

      PopulateNodes();
      PopulateData();
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if (domain!=null)
        domain.Dispose();
    }

    private void PopulateNodes()
    {
      var configuration = new NodeConfiguration(TestNodeId2);
      configuration.SchemaMapping.Add(DefaultSchema, SecondSchema);
      domain.NodeManager.AddNode(configuration);

      configuration = new NodeConfiguration(TestNodeId3);
      configuration.SchemaMapping.Add(DefaultSchema, ThirdSchema);
      domain.NodeManager.AddNode(configuration);
    }

    private void PopulateData()
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
        var apple = new Apple(TestAppleTag);
        var refObject = new FruitRef {Ref = apple};
        testAppleKey = apple.Key;
        testFruitRefKey = refObject.Key;
        tx.Complete();
      }
    }

    [Test]
    public void NodeIdDoubleSetTest()
    {
      using (var session = domain.OpenSession()) {
        session.SelectStorageNode(TestNodeId2);
        AssertEx.Throws<InvalidOperationException>(() => session.SelectStorageNode(TestNodeId3));
      }
    }

    [Test]
    public void NodeIdAssignTest()
    {
      Assert.That(testAppleKey.NodeId, Is.EqualTo(TestNodeId2));
      Assert.That(testFruitRefKey.NodeId, Is.EqualTo(TestNodeId2));
    }

    [Test]
    public void NodeIdLinqTest()
    {
      // Entity fetched via LINQ
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
        var result = session.Query.All<Apple>().Single(a => a.Tag==TestAppleTag);
        Assert.That(result.Key.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdLinq2Test()
    {
      // Key fetched via LINQ
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
        var result = session.Query.All<Apple>().GroupBy(a => a.Key).Select(a => a.Key).Single();
        Assert.That(result.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdLinq3Test()
    {
      // Key fetched via LINQ
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
        var result = session.Query.All<FruitRef>().Select(r => r.Ref.Key).Single();
        Assert.That(result.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdLinq4Test()
    {
      // Key fetched via LINQ
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
        var result = session.Query.All<FruitRef>().GroupBy(a => a.Ref.Key).Select(a => a.Key).Single();
        Assert.That(result.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdPrefetch1Test()
    {
      // Entity fetched via fetch API (key object)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
        var result = session.Query.Single<Apple>(testAppleKey);
        Assert.That(result.Key.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdPrefetch2Test()
    {
      // Entity fetched via fetch API (key values)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
        var result = session.Query.Single<Apple>(TestAppleTag);
        Assert.That(result.Key.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdPrefetch3Test()
    {
      // Entity fetched via fetch API (multiple key values)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
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
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
        var refObject = session.Query.Single<FruitRef>(testFruitRefKey);
        var result = refObject.Ref.Key;
        Assert.That(result.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdGenerateKeyTest()
    {
      // Custom generated key
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(TestNodeId2);
        var generatedKey = Key.Generate<Container>(session);
        Assert.That(generatedKey.NodeId, Is.EqualTo(TestNodeId2));
      }
    }

    [Test]
    public void NodeIdCreateFormatParseTest()
    {
      var nodeId = "{59EE3D42-C207-4E65-8EC0-220058B4F8F2}";
      var key = Key.Create(domain, nodeId, typeof (Apple), TypeReferenceAccuracy.ExactType, "1");
      Assert.That(key.NodeId, Is.EqualTo(nodeId));

      var keyString = key.Format();
      var key2 = Key.Parse(domain, keyString);
      Assert.That(key2.NodeId, Is.EqualTo(nodeId));
    }

    [Test]
    public void NodeIdCreateFormatParseDefaultTest()
    {
      var key = Key.Create(domain, typeof (Apple), TypeReferenceAccuracy.ExactType, "1");
      Assert.That(key.NodeId, Is.EqualTo(WellKnown.DefaultNodeId));

      var keyString = key.Format();
      var key2 = Key.Parse(domain, keyString);
      Assert.That(key2.NodeId, Is.EqualTo(WellKnown.DefaultNodeId));
    }
  }
}