// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.09.19

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0555_KeyRemapBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0555_KeyRemapBugModel
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class EntityWithEntityKeyField2 : Entity
  {
    [Field, Key(1)]
    public TestEntity EntityKey1 { get; set; }

    [Field, Key(0)]
    public EntityWithEntityKeyField1 EntityKey2 { get; set; }

    [Field]
    public string Text { get; set; }

    public EntityWithEntityKeyField2(Session session, TestEntity key1, EntityWithEntityKeyField1 key2)
      : base(session, key2, key1)
    {
    }
  }

  
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class EntityWithEntityKeyField1 : Entity
  {
    [Key, Field]
    public TestEntity EntityKey { get; private set; }

    [Field]
    public string Text { get; set; }

    public EntityWithEntityKeyField1(Session session, TestEntity key)
      : base(session, key)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }
  }

  [HierarchyRoot]
  [KeyGenerator]
  public class TestEntity2 : Entity
  {
    private static int Increment = 1;

    [Field, Key(0)]
    public int Key1 { get; set; }

    [Field, Key(1)]
    public TestEntity Key2 { get; set; }

    [Field]
    public string Text { get; set; }

    public TestEntity2(Session session, TestEntity key2)
      : base (session, Increment, key2)
    {
      Increment++;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0555_KeyRemapBug : AutoBuildTest
  {
    [Test]
    public void Test01()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var testEntity = new TestEntity {Text = "TestEntityTest01"};
        var entityWithEntityKey = new EntityWithEntityKeyField1(session, testEntity);

        Assert.That(testEntity.Key.IsTemporary(Domain), Is.True);
        Assert.That(entityWithEntityKey.Key.IsTemporary(Domain), Is.False);
        Assert.That(entityWithEntityKey.Key.Value.GetValue(0), Is.LessThan(0));

        Assert.DoesNotThrow(() => session.SaveChanges());

        Assert.That(testEntity.Key.IsTemporary(Domain), Is.False);
        Assert.That(entityWithEntityKey.Key.IsTemporary(Domain), Is.False);
        Assert.That(entityWithEntityKey.Key.Value.GetValue(0), Is.GreaterThan(0));
      }
    }

    [Test]
    public void Test02()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var testEntity = new TestEntity {Text = "TestEntityTest02"};
        var entityWithEntityKey = new EntityWithEntityKeyField1(session, testEntity);
        var entityWithEntityKey1 = new EntityWithEntityKeyField2(session, testEntity, entityWithEntityKey);

        Assert.That(testEntity.Key.IsTemporary(Domain), Is.True);
        Assert.That(entityWithEntityKey.Key.IsTemporary(Domain), Is.False);
        Assert.That(entityWithEntityKey.Key.Value.GetValue(0), Is.LessThan(0));
        Assert.That(entityWithEntityKey1.Key.IsTemporary(Domain), Is.False);
        Assert.That(entityWithEntityKey1.Key.Value.GetValue(0), Is.LessThan(0));
        Assert.That(entityWithEntityKey1.Key.Value.GetValue(1), Is.LessThan(0));

        Assert.DoesNotThrow(() => session.SaveChanges());

        Assert.That(testEntity.Key.IsTemporary(Domain), Is.False);
        Assert.That(entityWithEntityKey.Key.IsTemporary(Domain), Is.False);
        Assert.That(entityWithEntityKey.Key.Value.GetValue(0), Is.GreaterThan(0));
        Assert.That(entityWithEntityKey1.Key.IsTemporary(Domain), Is.False);
        Assert.That(entityWithEntityKey1.Key.Value.GetValue(0), Is.GreaterThan(0));
        Assert.That(entityWithEntityKey1.Key.Value.GetValue(1), Is.GreaterThan(0));
      }
    }

    [Test]
    public void Test03()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var testEntity = new TestEntity { Text = "TestEntityTest03" };
        var testEntity2 = new TestEntity2(session, testEntity);

        Assert.That(testEntity.Key.IsTemporary(Domain), Is.True);
        Assert.That(testEntity2.Key.IsTemporary(Domain), Is.False);
        Assert.That(testEntity2.Key.Value.GetValue(1), Is.LessThan(0));

        Assert.DoesNotThrow(() => session.SaveChanges());

        Assert.That(testEntity.Key.IsTemporary(Domain), Is.False);
        Assert.That(testEntity2.Key.IsTemporary(Domain), Is.False);
        Assert.That(testEntity2.Key.Value.GetValue(1), Is.GreaterThan(0));
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
