// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.12

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ExecuteAsyncExtensionTestModel;

namespace Xtensive.Orm.Tests.Storage.ExecuteAsyncExtensionTestModel
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
  public class ExecuteAsyncExtensionTest : AutoBuildTest
  {
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
        var container = new EntitySetContainer(session);
        foreach (var i in Enumerable.Range(-100, 200)) {
          _ = container.EntitySet.Add(new TestEntity(session) {Value = i});
        }
        transaction.Complete();
      }
    }

    [Test]
    public async Task QueryableTest()
    {
      var queryableEnumeable = (IQueryable<int>) new EnumerableQuery<int>(Enumerable.Range(-100, 200).ToArray()).Where(v => v < 5 && v > 0);
      var result = await queryableEnumeable.ExecuteAsync();
      

      var before = 1;
      foreach (var value in result) {
        Assert.That(value, Is.EqualTo(before));
        before++;
      }

      _ = Assert.ThrowsAsync<NotSupportedException>(async () => { await foreach (var i in result.AsAsyncEnumerable()) { } });
    }

    [Test]
    public async Task DoQueryableTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = await session.Query.All<TestEntity>().Where(e => e.Value < 5 && e.Value > 0).ExecuteAsync();

        var before = 1;
        await foreach (var value in result.AsAsyncEnumerable()) {
          Assert.That(value.Value, Is.EqualTo(before));
          before++;
        }
      }
    }

    [Test]
    public async Task PersistentCollectionTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var container = session.Query.All<EntitySetContainer>().First();
        var result = await container.EntitySet.Where(e => e.Value < 5 && e.Value > 0).ExecuteAsync();

        var before = 1;
        await foreach (var value in result.AsAsyncEnumerable()) {
          Assert.That(value.Value, Is.EqualTo(before));
          before++;
        }
      }
    }

    [Test]
    public async Task DelayedQueryTest()
    {
      await using (var session = await Domain.OpenSessionAsync())
      await using (var transaction = session.OpenTransaction()) {
        var delayed = session.Query.CreateDelayedQuery(
          q => q.All<TestEntity>().Where(e => e.Value < 5 && e.Value > 0));
        var result = await delayed.ExecuteAsync();

        var before = 1;
        await foreach (var value in result.AsAsyncEnumerable()) {
          Assert.That(value.Value, Is.EqualTo(before));
          before++;
        }
      }
    }
  }
}