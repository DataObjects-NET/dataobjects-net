// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.26

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.Multinode.DynamicTypeIdTestModel;

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  namespace DynamicTypeIdTestModel
  {
    [HierarchyRoot]
    public class BaseEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    public class Entity1 : BaseEntity
    {
      [Field]
      public string StringValue { get; set; }
    }

    public class Entity2 : BaseEntity
    {
      [Field]
      public int IntValue { get; set; }
    }
  }

  [TestFixture]
  public class DynamicTypeIdTest
  {
    private const string DefaultSchema = "n1";
    private const string AlternativeSchema = "n2";

    private void InitializeSchemas()
    {
      var connectionInfo = DomainConfigurationFactory.Create().ConnectionInfo;
      StorageTestHelper.DemandSchemas(connectionInfo, DefaultSchema, AlternativeSchema);
    }

    [Test]
    public void MainTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      InitializeSchemas();

      BuildDomain(DefaultSchema, DomainUpgradeMode.Recreate, typeof (Entity1)).Dispose();
      BuildDomain(DefaultSchema, DomainUpgradeMode.PerformSafely, typeof (Entity1), typeof (Entity2)).Dispose();
      
      BuildDomain(AlternativeSchema, DomainUpgradeMode.Recreate, typeof (Entity2));
      BuildDomain(AlternativeSchema, DomainUpgradeMode.PerformSafely, typeof (Entity2), typeof (Entity1)).Dispose();

      using (var domain = BuildDomain(DefaultSchema, DomainUpgradeMode.Validate, typeof (Entity1), typeof (Entity2))) {
        var nodeConfiguration = new NodeConfiguration(AlternativeSchema) {UpgradeMode = DomainUpgradeMode.Validate};
        nodeConfiguration.SchemaMapping.Add(DefaultSchema, AlternativeSchema);
        domain.StorageNodeManager.AddNode(nodeConfiguration);

        RunTest(domain, WellKnown.DefaultNodeId);
        RunTest(domain, AlternativeSchema);
      }
    }

    private void RunTest(Domain domain, string nodeId)
    {
      var baseName = "base " + nodeId;
      var entity1Name = "entity1 " + nodeId;
      var entity2Name = "entity2 " + nodeId;
      var stringValue = "entity1 " + nodeId;
      var intValue = stringValue.Length;

      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(nodeId);
        var baseEntity = new BaseEntity {Name = baseName};
        var entity1 = new Entity1 {Name = entity1Name, StringValue = stringValue};
        var entity2 = new Entity2 {Name = entity2Name, IntValue = intValue};
        tx.Complete();
      }

      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.SelectStorageNode(nodeId);
        var baseTypeId = GetTypeId(session, typeof (BaseEntity));
        var baseEntity = session.Query.All<BaseEntity>().Single(e => e.TypeId==baseTypeId);
        Assert.That(baseEntity.Name, Is.EqualTo(baseName));
        Assert.That(baseEntity.TypeId, Is.EqualTo(baseTypeId));

        var entity1 = session.Query.All<Entity1>().Single();
        Assert.That(entity1.Name, Is.EqualTo(entity1Name));
        Assert.That(entity1.StringValue, Is.EqualTo(stringValue));
        Assert.That(entity1.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity1))));

        var entity2 = session.Query.All<Entity2>().Single();
        Assert.That(entity2.Name, Is.EqualTo(entity2Name));
        Assert.That(entity2.IntValue, Is.EqualTo(intValue));
        Assert.That(entity2.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity2))));

        baseEntity.Remove();
        entity1.Remove();
        entity2.Remove();

        tx.Complete();
      }
    }

    private static int GetTypeId(Session session, Type type)
    {
      return session.StorageNode.TypeIdRegistry[session.Domain.Model.Types[type]];
    }

    private Domain BuildDomain(string schema, DomainUpgradeMode mode, params Type[] types)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.DefaultSchema = schema;
      configuration.UpgradeMode = mode;
      foreach (var type in types)
        configuration.Types.Register(type);
      return Domain.Build(configuration);
    }
  }
}