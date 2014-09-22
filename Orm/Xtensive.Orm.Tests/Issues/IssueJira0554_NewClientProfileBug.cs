// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.09.15

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0554_NewClientProfileBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0554_NewClientProfileBugModel
{
  [HierarchyRoot]
  public class TestA : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }
  }

  [HierarchyRoot]
  public class TestB : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public TestA TestA { get; set; }
  }

  [HierarchyRoot]
  public class TestD : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public TestStructure Structure { get; set; }
  }

  public class TestStructure : Structure
  {
    [Field]
    public TestA TestA { get; set; }
  }

  [HierarchyRoot]
  public class TestC : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public EntitySet<TestA> TestAs { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithTwoReferencesToTheSameEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public TestA FirstTestA { get; set; }

    [Field]
    public TestA SecondTestA { get; set; }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class EntityWithEntityAsKey : Entity
  {
    [Field, Key]
    public TestA Key { get; set; }

    [Field]
    public string Text { get; set; }

    public EntityWithEntityAsKey(Session session, TestA key)
      : base(session, key)
    {
    }
  }

}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0554_NewClientProfileBug : AutoBuildTest
  {
    [Test]
    public void MainTestForBug()
    {
      Key keyA2, keyB, keyC, keyA3;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var testA1 = new TestA {Text = "A1"};
        keyA2 = new TestA {Text = "A2"}.Key;
        keyB = new TestB {Text = "B1", TestA = testA1}.Key;
        var testA3 = new TestA {Text = "A3"};
        keyA3 = testA3.Key;
        var testC1 = new TestC { Text = "C1" };
        testC1.TestAs.Add(testA3);
        keyC = testC1.Key;
        transaction.Complete();
      }
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testA2 = session.Query.Single<TestA>(keyA2);
        var testB = session.Query.Single<TestB>(keyB);
        var testA1 = testB.TestA;
        testB.TestA = null;
        testB.TestA = testA2;
        testA1.Remove();
      }
    }

    [Test]
    public void Test01()
    {
      Key testA1Key, testBKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var testA1 = new TestA {Text = "Test01TestA"};
        var testB = new TestB {Text = "Test01TestB", TestA = testA1};
        testA1Key = testA1.Key;
        testBKey = testB.Key;
        transaction.Complete();
      }

      using(var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testB = session.Query.Single<TestB>(testBKey);
        var testA1 = testB.TestA;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        testB.TestA = null;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
      }
    }

    [Test]
    public void Test02()
    {
      Key testA1Key, testBKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var testA1 = new TestA {Text = "Test02TestA"};
        var testB = new TestB {Text = "Test02TestB", TestA = testA1};
        testA1Key = testA1.Key;
        testBKey = testB.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testB = session.Query.Single<TestB>(testBKey);
        var testA1 = testB.TestA;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        testB.TestA = null;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        session.SaveChanges();
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
      }
    }

    [Test]
    public void Test03()
    {
      Key testA1Key, testBKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var testA1 = new TestA {Text = "Test03TestA"};
        var testB = new TestB {Text = "Test03TestB", TestA = testA1};
        testA1Key = testA1.Key;
        testBKey = testB.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testB = session.Query.Single<TestB>(testBKey);
        var testA1 = testB.TestA;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        testB.TestA = null;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        testB.TestA = testA1;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        session.SaveChanges();
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
      }
    }

    [Test]
    public void Test04()
    {
      Key testA2Key, testBKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var testA1 = new TestA {Text = "Test04TestA1"};
        var testB = new TestB {Text = "Test04TestB", TestA = testA1};
        var testA2 = new TestA {Text = "Test04TestA2"};
        testA2Key = testA2.Key;
        testBKey = testB.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testB = session.Query.Single<TestB>(testBKey);
        var testA2 = session.Query.Single<TestA>(testA2Key);
        var testA1 = testB.TestA;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA2.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA2.State).Count);
        testB.TestA = testA2;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA2.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetAddedReferences(testA2.State).Count);
        testB.TestA = testA1;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA2.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA2.State).Count);
        session.SaveChanges();
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA2.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA2.State).Count);
      }
    }

    [Test]
    public void Test05()
    {
      Key testA1Key, testA2Key, testD1Key;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var testA1 = new TestA {Text = "Test05TestA1"};
        var testD1 = new TestD {Text = "Test05TestD1", Structure = new TestStructure() {TestA = testA1}};
        var testA2 = new TestA {Text = "Test05TestA2"};
        testA1Key = testA1.Key;
        testA2Key = testA2.Key;
        testD1Key = testD1.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testD1 = session.Query.Single<TestD>(testD1Key);
        var testA1 = session.Query.Single<TestA>(testA1Key);
        var testA2 = session.Query.Single<TestA>(testA2Key);

        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testD1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testD1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA2.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA2.State).Count);
        testD1.Structure.TestA = testA2;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testD1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testD1.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(testD1.State, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Keys.First());
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA2.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetAddedReferences(testA2.State).Count);
        Assert.AreEqual(testD1.State, session.EntityReferenceChangesRegistry.GetAddedReferences(testA2.State).Keys.First());
        session.SaveChanges();
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testD1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testD1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA2.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA2.State).Count);
      }
    }

    [Test]
    public void Test06()
    {
      Key testA1Key, testBKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var testA1 = new TestA {Text = "Test06TestA"};
        var testB = new TestB {Text = "Test06TestB", TestA = testA1};
        testA1Key = testA1.Key;
        testBKey = testB.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testB = session.Query.Single<TestB>(testBKey);
        var testA1 = testB.TestA;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        testB.TestA = null;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
        session.CancelChanges();
        Assert.AreEqual(testA1, testB.TestA);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testB.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA1.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA1.State).Count);
      }
    }

    [Test]
    public void Test07()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var testA = new TestA {Text = "Test07TestA"};
        var entity = new EntityWithEntityAsKey(session, testA);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(entity.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State)[entity.State]);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA.State).Count);
      }
    }

    [Test]

    public void Test08()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var testA = new TestA {Text = "Test08TestA"};
        var entity = new EntityWithTwoReferencesToTheSameEntity {FirstTestA = testA, SecondTestA = testA};
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(entity.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State).Count);
        Assert.AreEqual(2, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State)[entity.State]);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA.State).Count);
        entity.SecondTestA = null;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(entity.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State)[entity.State]);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA.State).Count);
        entity.FirstTestA = null;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA.State).Count);
      }
    }

    [Test]
    public void Test09()
    {
      Key testAKey, entityKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var testA = new TestA {Text = "Test09TestA"};
        var entity = new EntityWithTwoReferencesToTheSameEntity { FirstTestA = testA, SecondTestA = testA };
        testAKey = testA.Key;
        entityKey = entity.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (var transaction = session.OpenTransaction()) {
        var testA = session.Query.Single<TestA>(testAKey);
        var entity = session.Query.Single<EntityWithTwoReferencesToTheSameEntity>(entityKey);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA.State).Count);
        entity.SecondTestA = null;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA.State)[entity.State]);
        entity.FirstTestA = null;
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetRemovedReferences(entity.State).Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.GetAddedReferences(testA.State).Count);
        Assert.AreEqual(1, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA.State).Count);
        Assert.AreEqual(2, session.EntityReferenceChangesRegistry.GetRemovedReferences(testA.State)[entity.State]);
      }
    }

    [Test]
    public void Test10()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        using (session.Activate())
        using (var transaction = session.OpenTransaction()) {
          var testA = new TestA {Text = "Test10TestA"};
          var entity = new EntityWithTwoReferencesToTheSameEntity { FirstTestA = testA, SecondTestA = testA };
        }
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.AddedReferencesCount);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.RemovedReferencesCount);
        Key testAKey, entityKey;
        using (session.Activate())
        using (var transaction = session.OpenTransaction()) {
          var testA = new TestA {Text = "Test10TestA"};
          var entity = new EntityWithTwoReferencesToTheSameEntity { FirstTestA = testA, SecondTestA = testA };
          testAKey = testA.Key;
          entityKey = entity.Key;
          transaction.Complete();
        }
        Assert.AreEqual(2, session.EntityReferenceChangesRegistry.AddedReferencesCount);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.RemovedReferencesCount);
      }
    }


    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestA).Assembly, typeof (TestB).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
