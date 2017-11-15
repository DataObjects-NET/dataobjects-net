// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.26

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.Multinode.DynamicTypeIdTestModel;

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  namespace DynamicTypeIdTestModel
  {
    #region Main test classes

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

    #endregion

    #region Interface test classes

    public interface IContainsGuidField : IEntity
    {
      [Field]
      Guid GuidValue { get; set; }
    }

    public class SomeInterfaceImpl1 : BaseEntity, IContainsGuidField
    {
      [Field]
      public string StringValue { get; set; }

      [Field]
      public Guid GuidValue { get; set; }
    }

    public class SomeInterfaceImpl2 : BaseEntity, IContainsGuidField
    {
      [Field]
      public int IntValue { get; set; }

      [Field]
      public Guid GuidValue { get; set; }
    }

    #endregion

    #region Abstract test classes

    [HierarchyRoot]
    public abstract class BaseAbstractEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    public class Entity3 : BaseAbstractEntity
    {
      [Field]
      public string StringValue { get; set; }
    }

    public class Entity4 : BaseAbstractEntity
    {
      [Field]
      public int IntValue { get; set; }
    }

    #endregion
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

        RunTest(domain, WellKnown.DefaultNodeId, MainTestBody);
        RunTest(domain, AlternativeSchema, MainTestBody);
      }
    }

    [Test]
    public void SelectAbstractClassDescendantsTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      InitializeSchemas();

      BuildDomain(DefaultSchema, DomainUpgradeMode.Recreate, typeof (Entity3)).Dispose();
      using (var domain = BuildDomain(DefaultSchema, DomainUpgradeMode.PerformSafely, typeof (Entity3), typeof (Entity4)))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new Entity3 {
            Name = "1 before test " + i,
            StringValue = "1 before test " + i
          };
          new Entity4 {
            Name = "1 before test " + i,
            IntValue = i
          };
        }
        transaction.Complete();
      }

      BuildDomain(AlternativeSchema, DomainUpgradeMode.Recreate, typeof (Entity4));
      using (var domain = BuildDomain(AlternativeSchema, DomainUpgradeMode.PerformSafely, typeof (Entity4), typeof (Entity3)))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new Entity3 {
            Name = "2 before test " + i,
            StringValue = "1 before test " + i
          };
          new Entity4 {
            Name = "2 before test " + i,
            IntValue = i
          };
        }
        transaction.Complete();
      }

      using (var domain = BuildDomain(DefaultSchema, DomainUpgradeMode.Validate, typeof (Entity3), typeof (Entity4))) {
        var nodeConfiguration = new NodeConfiguration(AlternativeSchema) { UpgradeMode = DomainUpgradeMode.Validate };
        nodeConfiguration.SchemaMapping.Add(DefaultSchema, AlternativeSchema);
        domain.StorageNodeManager.AddNode(nodeConfiguration);

        RunTest(domain, WellKnown.DefaultNodeId, AbstractClassDescendantsTestBody);
        RunTest(domain, AlternativeSchema, AbstractClassDescendantsTestBody);
      }
    }

    [Test]
    public void SelectInterfaceImplementationsTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      InitializeSchemas();

      BuildDomain(DefaultSchema, DomainUpgradeMode.Recreate, typeof (SomeInterfaceImpl1)).Dispose();
      using (var domain = BuildDomain(DefaultSchema, DomainUpgradeMode.PerformSafely, typeof (SomeInterfaceImpl1), typeof (SomeInterfaceImpl2)))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new SomeInterfaceImpl1 {
            Name = "1 before test " + i,
            StringValue = "1 before test " + i,
            GuidValue = Guid.NewGuid()
          };
          new SomeInterfaceImpl2 {
            Name = "1 before test " + i,
            IntValue = i,
            GuidValue = Guid.NewGuid()
          };
        }
        transaction.Complete();
      }

      BuildDomain(AlternativeSchema, DomainUpgradeMode.Recreate, typeof (SomeInterfaceImpl2));
      using (var domain = BuildDomain(AlternativeSchema, DomainUpgradeMode.PerformSafely, typeof (SomeInterfaceImpl1), typeof (SomeInterfaceImpl2)))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new SomeInterfaceImpl1 {
            Name = "2 before test " + i,
            StringValue = "1 before test " + i,
            GuidValue = Guid.NewGuid()
          };
          new SomeInterfaceImpl2 {
            Name = "2 before test " + i,
            IntValue = i,
            GuidValue = Guid.NewGuid()
          };
        }
        transaction.Complete();
      }

      using (var domain = BuildDomain(DefaultSchema, DomainUpgradeMode.Validate, typeof (SomeInterfaceImpl1), typeof (SomeInterfaceImpl2))) {
        var nodeConfiguration = new NodeConfiguration(AlternativeSchema) { UpgradeMode = DomainUpgradeMode.Validate };
        nodeConfiguration.SchemaMapping.Add(DefaultSchema, AlternativeSchema);
        domain.StorageNodeManager.AddNode(nodeConfiguration);

        RunTest(domain, WellKnown.DefaultNodeId, InterfaceTestBody);
        RunTest(domain, AlternativeSchema, InterfaceTestBody);
      }
    }

    [Test] 
    public void SelectNonAbstractDescendant()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      InitializeSchemas();

      BuildDomain(DefaultSchema, DomainUpgradeMode.Recreate, typeof (Entity1)).Dispose();
      using (var domain = BuildDomain(DefaultSchema, DomainUpgradeMode.PerformSafely, typeof (Entity1), typeof (Entity2)))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new Entity1 {
            Name = "1 before test " + i,
            StringValue = "1 before test " + i
          };
          new Entity2 {
            Name = "1 before test " + i,
            IntValue = i
          };
        }
        transaction.Complete();
      }

      BuildDomain(AlternativeSchema, DomainUpgradeMode.Recreate, typeof (Entity2));
      using (var domain = BuildDomain(AlternativeSchema, DomainUpgradeMode.PerformSafely, typeof (Entity2), typeof (Entity1)))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new Entity1 {
            Name = "2 before test " + i,
            StringValue = "1 before test " + i
          };
          new Entity2 {
            Name = "2 before test " + i,
            IntValue = i
          };
        }
        transaction.Complete();
      }

      using (var domain = BuildDomain(DefaultSchema, DomainUpgradeMode.Validate, typeof (Entity1), typeof (Entity2))) {
        var nodeConfiguration = new NodeConfiguration(AlternativeSchema) { UpgradeMode = DomainUpgradeMode.Validate };
        nodeConfiguration.SchemaMapping.Add(DefaultSchema, AlternativeSchema);
        domain.StorageNodeManager.AddNode(nodeConfiguration);

        RunTest(domain, WellKnown.DefaultNodeId, NonAbstractTypesTestBody);
        RunTest(domain, AlternativeSchema, NonAbstractTypesTestBody);
      }
    }

    [Test]
    public void TypeIdExtractionTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      InitializeSchemas();

      var defaultSchemaMap = new Dictionary<Type, int>();
      var alternativeSchemaMap = new Dictionary<Type, int>();

      BuildDomain(DefaultSchema, DomainUpgradeMode.Recreate, typeof (Entity1)).Dispose();
      using (var domain = BuildDomain(DefaultSchema, DomainUpgradeMode.PerformSafely, typeof (Entity1), typeof (Entity2))) 
        defaultSchemaMap = domain.Model.Types.ToDictionary(el => el.UnderlyingType, el => el.TypeId);

      BuildDomain(AlternativeSchema, DomainUpgradeMode.Recreate, typeof (Entity2));
      using ( var domain = BuildDomain(AlternativeSchema, DomainUpgradeMode.PerformSafely, typeof (Entity2), typeof (Entity1)))
        alternativeSchemaMap = domain.Model.Types.ToDictionary(el => el.UnderlyingType, el => el.TypeId);

      using (var domain = BuildDomain(DefaultSchema, DomainUpgradeMode.Validate, typeof (Entity1), typeof (Entity2))) {
        var nodeConfiguration = new NodeConfiguration(AlternativeSchema) {UpgradeMode = DomainUpgradeMode.Validate};
        nodeConfiguration.SchemaMapping.Add(DefaultSchema, AlternativeSchema);
        domain.StorageNodeManager.AddNode(nodeConfiguration);

        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(WellKnown.DefaultNodeId);
          var types = new[] {typeof (BaseEntity), typeof (Entity1), typeof (Entity2)};
          foreach (var type in types)
            Assert.That(session.StorageNode.TypeIdRegistry[domain.Model.Types[type]], Is.EqualTo(defaultSchemaMap[type]));
        }

        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(AlternativeSchema);
          var types = new[] {typeof (BaseEntity), typeof (Entity1), typeof (Entity2)};
          foreach (var type in types)
            Assert.That(session.StorageNode.TypeIdRegistry[domain.Model.Types[type]], Is.EqualTo(alternativeSchemaMap[type]));
        }
      }
    }

    private void MainTestBody(Domain domain, string nodeId)
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

    private void NonAbstractTypesTestBody(Domain domain, string nodeId)
    {
      using (var session = domain.OpenSession())
      using (var trasaction = session.OpenTransaction()) {
        session.SelectStorageNode(nodeId);
        var query1 = session.Query.All<Entity1>();
        Assert.That(query1.Count(), Is.EqualTo(10));
        foreach (var entity in query1) {
          var id = entity.Id;
          var name = entity.Name;
          var stringValue = entity.StringValue;
          Assert.That(entity.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity1))));
        }
        var query2 = session.Query.All<Entity2>();
        Assert.That(query2.Count(), Is.EqualTo(10));
        foreach (var entity in query2) {
          var id = entity.Id;
          var name = entity.Name;
          var intValue = entity.IntValue;
          Assert.That(entity.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity2))));
        }

        var firstCounter = 0;
        var secondCounter = 0;
        var all = session.Query.All<BaseEntity>();
        Assert.That(all.Count(), Is.EqualTo(20));
        foreach (var baseAbstractEntity in all) {
          var firstImplementation = baseAbstractEntity as Entity1;
          if (firstImplementation!=null) {
            firstCounter++;
            Assert.That(firstImplementation.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity1))));
            continue;
          }
          var secondImplementation = baseAbstractEntity as Entity2;
          if (secondImplementation!=null) {
            secondCounter++;
            Assert.That(secondImplementation.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity2))));
          }
        }
        Assert.That(firstCounter + secondCounter, Is.EqualTo(20));
      }
    }

    private void AbstractClassDescendantsTestBody(Domain domain, string nodeId)
    {
      using (var session = domain.OpenSession())
      using (var trasaction = session.OpenTransaction()) {
        session.SelectStorageNode(nodeId);
        var query1 = session.Query.All<Entity3>();
        Assert.That(query1.Count(), Is.EqualTo(10));
        foreach (var entity in query1) {
          var id = entity.Id;
          var name = entity.Name;
          var stringValue = entity.StringValue;
          Assert.That(entity.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity3))));
        }

        var query2 = session.Query.All<Entity4>();
        Assert.That(query2.Count(), Is.EqualTo(10));
        foreach (var entity in query2) {
          var id = entity.Id;
          var name = entity.Name;
          var intValue = entity.IntValue;
          Assert.That(entity.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity4))));
        }

        var firstCounter = 0;
        var secondCounter = 0;
        var all = session.Query.All<BaseAbstractEntity>();
        Assert.That(all.Count(), Is.EqualTo(20));
        foreach (var baseAbstractEntity in all) {
          var firstImplementation = baseAbstractEntity as Entity3;
          if (firstImplementation!=null) {
            firstCounter++;
            Assert.That(firstImplementation.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity3))));
            continue;
          }
          var secondImplementation = baseAbstractEntity as Entity4;
          if (secondImplementation!=null) {
            secondCounter++;
            Assert.That(secondImplementation.TypeId, Is.EqualTo(GetTypeId(session, typeof (Entity4))));
          }
        }
        Assert.That(firstCounter + secondCounter, Is.EqualTo(20));
      }
    }

    private void InterfaceTestBody(Domain domain, string nodeId)
    {
      using (var session = domain.OpenSession())
      using (var trasaction = session.OpenTransaction()) {
        session.SelectStorageNode(nodeId);
        var firstImplementationsBaseQuery = session.Query.All<SomeInterfaceImpl1>();
        Assert.That(firstImplementationsBaseQuery.Count(), Is.EqualTo(10));
        foreach (var someInterfaceImpl1 in firstImplementationsBaseQuery) {
          var id = someInterfaceImpl1.Id;
          var name = someInterfaceImpl1.Name;
          var stringValue = someInterfaceImpl1.StringValue;
          var guidValue = someInterfaceImpl1.GuidValue;
          Assert.That(someInterfaceImpl1.TypeId, Is.EqualTo(GetTypeId(session, typeof (SomeInterfaceImpl1))));
        }

        var secondImplementationsBaseQuery = session.Query.All<SomeInterfaceImpl2>();
        Assert.That(secondImplementationsBaseQuery.Count(), Is.EqualTo(10));
        foreach (var someInterfaceImpl2 in secondImplementationsBaseQuery) {
          var id = someInterfaceImpl2.Id;
          var name = someInterfaceImpl2.Name;
          var intValue = someInterfaceImpl2.IntValue;
          var guidValue = someInterfaceImpl2.GuidValue;
          Assert.That(someInterfaceImpl2.TypeId, Is.EqualTo(GetTypeId(session, typeof (SomeInterfaceImpl2))));
        }

        var allImplementationsBaseQuery = session.Query.All<IContainsGuidField>();
        Assert.That(allImplementationsBaseQuery.Count(), Is.EqualTo(20));
        var firstImplementationsCount = 0;
        var secondImplementationsCount = 0;
        foreach (var containsGuidField in allImplementationsBaseQuery) {
          var firstImplementation = containsGuidField as SomeInterfaceImpl1;
          if (firstImplementation!=null) {
            firstImplementationsCount++;
            Assert.That(firstImplementation.TypeId, Is.EqualTo(GetTypeId(session, typeof (SomeInterfaceImpl1))));
            continue;  
          }
          var secondImplementation = containsGuidField as SomeInterfaceImpl2;
          if (secondImplementation!=null) {
            secondImplementationsCount++;
            Assert.That(secondImplementation.TypeId, Is.EqualTo(GetTypeId(session, typeof (SomeInterfaceImpl2))));
          }
        }
        Assert.That(firstImplementationsCount + secondImplementationsCount, Is.EqualTo(20));
      }
    }

    private void RunTest(Domain domain, string nodeId, Action<Domain, string> testBody)
    {
      testBody.Invoke(domain, nodeId);
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