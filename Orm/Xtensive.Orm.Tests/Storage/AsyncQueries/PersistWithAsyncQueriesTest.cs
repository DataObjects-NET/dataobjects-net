// Copyright (C) 2019-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.07.12

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
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
      configuration.Types.Register(typeof(TestEntity).Assembly, typeof(TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (var i = 0; i < TestEntityCount; i++) {
          _ = new TestEntity(session) { Value = i };
        }

        transaction.Complete();
      }
    }

    [Test]
    public async Task AsyncButFullySequentialTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsync();
        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount));

        count = 0;
        foreach (var testEntity in anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount));
      }
    }

    [Test]
    public async Task QueryFirstWaitLaterTest()
    {
      Require.ProviderIsNot(StorageProvider.PostgreSql, "No parallel executing for commands, so having two readers opened is impossible");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = session.Query.All<TestEntity>().AsAsync();

        var anotherAsyncQuery = session.Query.All<TestEntity>().AsAsync();

        // some local non-DO work probably;

        var count = 0;
        foreach (var testEntity in await readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount));

        count = 0;
        foreach (var testEntity in await anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount));
      }
    }

    [Test]
    public async Task QueryFirstWaitLaterInDiffferentOrderTest()
    {
      Require.ProviderIsNot(StorageProvider.PostgreSql, "No parallel executing for commands. so having opened reader for query and making changes saved is impossible");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()){
        var readyToRockQuery = session.Query.All<TestEntity>().AsAsync();
        var anotherAsyncQuery = session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in await anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount));

        count = 0;
        foreach (var testEntity in await readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount));
      }
    }

    [Test]
    public async Task ProperPersistSequenceTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird, "Open reader reads lines that were inserted between getting reader and enumeratin it");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsync();

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        count = 0;
        foreach (var testEntity in anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task ProperPersistSequenceFirebirdTest()
    {
      Require.ProviderIs(StorageProvider.Firebird);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsync();

        count = 0;
        foreach (var testEntity in anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task PersistBeforeFirstAsyncQueryAwaitTest()
    {
      Require.ProviderIsNot(StorageProvider.PostgreSql, "No parallel executing for commands. so having two readers opened is impossible");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsync();

        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        count = 0;
        foreach (var testEntity in anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    [Test]
    public async Task PersistBeforeSecondAsyncQueryAwaitTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird, "Open reader reads lines that were inserted between getting reader and enumeratin it");
      Require.ProviderIsNot(StorageProvider.PostgreSql, "No parallel executing for commands, so having opened reader for query and making changes saved is impossible");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsync();

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount));

        count = 0;
        foreach (var testEntity in anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    [Test]
    public async Task PersistBeforeSecondAsyncQueryAwaitFirebirdTest()
    {
      Require.ProviderIs(StorageProvider.Firebird);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = await session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount));

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var anotherAsyncQuery = await session.Query.All<TestEntity>().AsAsync();

        count = 0;
        foreach (var testEntity in anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    [Test]
    public async Task PersistBeforeBothAsyncQueriesAndDelayedWaitTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird, "Open reader reads lines that were inserted between getting reader and enumeratin it");
      Require.ProviderIsNot(StorageProvider.PostgreSql, "No parallel executing for commands, so having opened reader for query and making changes saved is impossible");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestEntity(session) { Value = 101 };
        _ = new TestEntity(session) { Value = 102 };
        _ = new TestEntity(session) { Value = 103 };

        var readyToRockQuery = session.Query.All<TestEntity>().AsAsync();

        _ = new TestEntity(session) { Value = 104 };
        _ = new TestEntity(session) { Value = 105 };
        _ = new TestEntity(session) { Value = 106 };

        var anotherAsyncQuery = session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in await anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));

        count = 0;
        foreach (var testEntity in await readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    [Test]
    public async Task PersistBeforeBothAsyncQueriesAndDelayedWaitFirebirdTest()
    {
      Require.ProviderIs(StorageProvider.Firebird);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestEntity(session) { Value = 101 };
        _ = new TestEntity(session) { Value = 102 };
        _ = new TestEntity(session) { Value = 103 };

        var readyToRockQuery = session.Query.All<TestEntity>().AsAsync();

        _ = new TestEntity(session) { Value = 104 };
        _ = new TestEntity(session) { Value = 105 };
        _ = new TestEntity(session) { Value = 106 };

        var anotherAsyncQuery = session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in await anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));

        count = 0;
        foreach (var testEntity in await readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task PersistBeforeFirstAsyncQueryAndDalayedAwaitTest()
    {
      Require.ProviderIsNot(StorageProvider.PostgreSql, "No parallel executing for commands, so having two readers opened is impossible");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var readyToRockQuery =  session.Query.All<TestEntity>().AsAsync();
        var anotherAsyncQuery =  session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in await anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        count = 0;
        foreach (var testEntity in await readyToRockQuery) {
          count++;
        }

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    [Test]
    public async Task PersistBeforeSecondAsyncQueryAndDelayedAwaitTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird, "Open reader reads lines that were inserted between getting reader and enumeratin it");
      Require.ProviderIsNot(StorageProvider.PostgreSql, "No parallel executing for commands. so having opened reader for query and making changes saved is impossible");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery =  session.Query.All<TestEntity>().AsAsync();

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var anotherAsyncQuery =  session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in await anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        count = 0;
        foreach (var testEntity in await readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount));
      }
    }

    [Test]
    public async Task PersistBeforeSecondAsyncQueryAndDelayedAwaitFirebirdTest()
    {
      Require.ProviderIs(StorageProvider.Firebird);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = session.Query.All<TestEntity>().AsAsync();

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var anotherAsyncQuery = session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var testEntity in await anotherAsyncQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        count = 0;
        foreach (var testEntity in await readyToRockQuery) {
          count++;
        }
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    #region Manual persist

    [Test]
    public async Task ProperManualSavingChangesTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var count = 0;
        foreach (var entity in await session.Query.All<TestEntity>().AsAsync()) {
          count++;
        }

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        session.SaveChanges();

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
        count = session.Query.All<TestEntity>().AsEnumerable().Count();
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task AwaitingQueryBeforeManualSavingChangesTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var readyToRockQuery = session.Query.All<TestEntity>().AsAsync();

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);


        var count = 0;
        foreach (var entity in await readyToRockQuery) {
          count++;
        }

        session.SaveChanges();

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
        count = session.Query.All<TestEntity>().AsEnumerable().Count();
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task ManualSavingDuringAsyncQueryExecutionTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird, "Open reader reads lines that were inserted between getting reader and enumeratin it");
      Require.ProviderIsNot(StorageProvider.PostgreSql, "No parallel executing for commands. so having opened reader for query and making changes saved is impossible");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var readyToRockQuery = session.Query.All<TestEntity>().AsAsync();

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        session.SaveChanges();

        var count = 0;
        foreach (var entity in await readyToRockQuery) {
          count++;
        }

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
        count = session.Query.All<TestEntity>().AsEnumerable().Count();
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task ManualSavingDuringAsyncQueryExecutionFirebirdTest()
    {
      Require.ProviderIs(StorageProvider.Firebird);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var readyToRockQuery = session.Query.All<TestEntity>().AsAsync();

        var count = 0;
        foreach (var entity in await readyToRockQuery) {
          count++;
        }

        Assert.That(count, Is.EqualTo(TestEntityCount + 3));

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        session.SaveChanges();

        count = session.Query.All<TestEntity>().AsEnumerable().Count();
        Assert.That(count, Is.EqualTo(TestEntityCount + 6));
      }
    }

    [Test]
    public async Task ManualSavingDuringEnumerationTest()
    {
      Require.ProviderIsNot(StorageProvider.PostgreSql, "No parallel executing for commands. so having opened reader for query and making changes saved is impossible");
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var readyToRockQuery = session.Query.All<TestEntity>().AsAsync();

        _ = new TestEntity(session);
        _ = new TestEntity(session);
        _ = new TestEntity(session);

        var shouldPersist = true;

        var count = 0;
        foreach (var entity in await readyToRockQuery) {
          if (count > 10 && shouldPersist) {
            session.SaveChanges();
            shouldPersist = false;
          }
          count++;
        }

        Assert.That(count, Is.EqualTo(TestEntityCount));
        count = session.Query.All<TestEntity>().AsEnumerable().Count();
        Assert.That(count, Is.EqualTo(TestEntityCount + 3));
      }
    }

    #endregion
  }
}