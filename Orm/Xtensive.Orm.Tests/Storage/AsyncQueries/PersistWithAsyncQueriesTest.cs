// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.07.12

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.AsyncQueries.PersistWithAsyncQueriesTestModel;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries.PersistWithAsyncQueriesTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }

    [Field]
    public long Value { get; set; }

    public TestEntity(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class EntitySetContainer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<TestEntity> EntitySet { get; set; }

    public EntitySetContainer(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class PersistWithAsyncQueriesTest : AutoBuildTest
  {
    private const int TestEntityCount = 25;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < TestEntityCount; i++)
          new TestEntity(session) {Value = i};
        transaction.Complete();
      }
    }

    [Test]
    public async Task AsyncButFullySequential()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsyncTask();
        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsyncTask();

        int count = 0;
        foreach (var testEntity in readyToRockQuery)
          count++;
        Assert.That(count, Is.EqualTo(TestEntityCount));

        count = 0;
        foreach (var testEntity in anotherAsyncQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount));
      }
    }

    [Test]
    public async Task QueryFirstWaitLater()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = session.Query.All<TestEntity>().AsAsyncTask();

        var anotherAsyncQuery = session.Query.All<TestEntity>().AsAsyncTask();

        // some local non-DO work probably;

        int count = 0;
        foreach (var testEntity in await readyToRockQuery)
          count++;
        Assert.That(count, Is.EqualTo(TestEntityCount));

        count = 0;
        foreach (var testEntity in await anotherAsyncQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount));
      }
    }

    [Test]
    public async Task QueryFirstWaitLaterInDiffferentOrder()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()){
        var readyToRockQuery = session.Query.All<TestEntity>().AsAsyncTask();
        var anotherAsyncQuery = session.Query.All<TestEntity>().AsAsyncTask();

        int count = 0;
        foreach (var testEntity in await anotherAsyncQuery)
          count++;
        Assert.That(count, Is.EqualTo(TestEntityCount));

        count = 0;
        foreach (var testEntity in await readyToRockQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount));
      }
    }

    [Test]
    public async Task ProperPersistSequenceTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsyncTask();

        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsyncTask();

        int count = 0;
        foreach (var testEntity in readyToRockQuery)
          count++;
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        count = 0;
        foreach (var testEntity in anotherAsyncQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task PersistBeforeFirstAsyncQueryAwaitTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsyncTask();

        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsyncTask();

        int count = 0;
        foreach (var testEntity in readyToRockQuery)
          count++;
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        count = 0;
        foreach (var testEntity in anotherAsyncQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    [Test]
    public async Task PersistBeforeSecondAsyncQueryAwaitTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsyncTask();

        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsyncTask();

        int count = 0;
        foreach (var testEntity in readyToRockQuery)
          count++;
        Assert.That(count, Is.EqualTo(TestEntityCount));

        count = 0;
        foreach (var testEntity in anotherAsyncQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    [Test]
    public async Task PersistBeforeBothAsyncQueriesAndDelayedWaitTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new TestEntity(session) {Value = 101};
        new TestEntity(session) {Value = 102};
        new TestEntity(session) {Value = 103};

        var readyToRockQuery = session.Query.All<TestEntity>().AsAsyncTask();

        new TestEntity(session) { Value = 104 };
        new TestEntity(session) { Value = 105 };
        new TestEntity(session) { Value = 106 };

        var anotherAsyncQuery = session.Query.All<TestEntity>().AsAsyncTask();

        int count = 0;
        foreach (var testEntity in await anotherAsyncQuery)
          count++;
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));

        count = 0;
        foreach (var testEntity in await readyToRockQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    [Test]
    public async Task PersistBeforeFirstAsyncQueryAndDalayedAwaitTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        var readyToRockQuery =  session.Query.All<TestEntity>().AsAsyncTask();
        var anotherAsyncQuery =  session.Query.All<TestEntity>().AsAsyncTask();

        int count = 0;
        foreach (var testEntity in await anotherAsyncQuery)
          count++;
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        count = 0;
        foreach (var testEntity in await readyToRockQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    [Test]
    public async Task PersistBeforeSecondAsyncQueryAndDelayedAwaitTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery =  session.Query.All<TestEntity>().AsAsyncTask();

        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        var anotherAsyncQuery =  session.Query.All<TestEntity>().AsAsyncTask();

        int count = 0;
        foreach (var testEntity in await anotherAsyncQuery)
          count++;
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        count = 0;
        foreach (var testEntity in await readyToRockQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount));
      }
    }

    #region Manual persist

    [Test]
    public async Task ProperManualSavingChanges()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        var count = 0;
        foreach (var entity in await session.Query.All<TestEntity>().AsAsyncTask())
          count++;

        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        session.SaveChanges();

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
        count = 0;
        session.Query.All<TestEntity>().ForEach((e) => count++);
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task AwaitingQueryBeforeManualSavingChanges()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        var readyToRockQuery = session.Query.All<TestEntity>().AsAsyncTask();

        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);


        int count = 0;
        foreach (var entity in await readyToRockQuery) 
          count++;

        session.SaveChanges();

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
        count = 0;
        session.Query.All<TestEntity>().ForEach((e) => count++);
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task ManualSavingDuringAsyncQueryExecution()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        var readyToRockQuery = session.Query.All<TestEntity>().AsAsyncTask();

        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        session.SaveChanges();

        int count = 0;
        foreach (var entity in await readyToRockQuery)
          count++;

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
        count = 0;
        session.Query.All<TestEntity>().ForEach((e)=> count++);
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task ManualSavingDuringEnumeration()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = session.Query.All<TestEntity>().AsAsyncTask();

        new TestEntity(session);
        new TestEntity(session);
        new TestEntity(session);

        bool shouldPersist = true;

        int count = 0;
        foreach (var entity in await readyToRockQuery) {
          if (count > 10 && shouldPersist) {
            session.SaveChanges();
            shouldPersist = false;
          }
          count++;
        }

        Assert.That(count, Is.EqualTo(TestEntityCount));
        count = 0;
        session.Query.All<TestEntity>().ForEach((e) => count++);
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    #endregion
  }
}