// Copyright (C) 2019-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ConcurrentCommandProcessorExecutionModel;

namespace Xtensive.Orm.Tests.Storage.ConcurrentCommandProcessorExecutionModel
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
  public class AsAsyncExtensionTest : AutoBuildTest
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
    public async Task PureEnumerableTest()
    {
      var task = AsEnumerable(1, 2, 3, 4).AsAsync();
      Assert.That(task.IsCompleted, Is.True);

      var before = 1;
      foreach (var value in await task) {
        Assert.That(value, Is.EqualTo(before));
        before++;
      }
    }

    [Test]
    public async Task CollectionAsEnumerableTest()
    {
      var list = new List<int> {1, 2, 3, 4};
      var task = list.AsAsync();
      Assert.That(task.IsCompleted, Is.True);

      var before = 1;
      foreach (var value in await task) {
        Assert.That(value, Is.EqualTo(before));
        before++;
      }
    }

    [Test]
    public async Task QueryableAsEnumearbleTest()
    {
      var queryableEnumeable = new EnumerableQuery<int>(Enumerable.Range(-100, 200).ToArray()).Where(v => v < 5 && v > 0);
      var task = queryableEnumeable.AsEnumerable().AsAsync();
      Assert.That(task.IsCompleted, Is.True);

      var before = 1;
      foreach (var value in await task) {
        Assert.That(value, Is.EqualTo(before));
        before++;
      }
    }

    [Test]
    public async Task QueryableTest()
    {
      var queryableEnumeable = new EnumerableQuery<int>(Enumerable.Range(-100, 200).ToArray()).Where(v => v < 5 && v > 0);
      var task = queryableEnumeable.AsAsync();
      Assert.That(task.IsCompleted, Is.True);

      var before = 1;
      foreach (var value in await task) {
        Assert.That(value, Is.EqualTo(before));
        before++;
      }
    }

    [Test]
    public async Task DoQueryableAsEnumeableTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = await session.Query.All<TestEntity>().Where(e => e.Value < 5 && e.Value > 0).AsEnumerable().AsAsync();

        var before = 1;
        foreach (var value in result) {
          Assert.That(value.Value, Is.EqualTo(before));
          before++;
        }
      }
    }

    [Test]
    public async Task PersistentCollectionAsEnumerableTesk()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var container = session.Query.All<EntitySetContainer>().First();
        var result = await container.EntitySet.Where(e => e.Value < 5 && e.Value > 0).AsEnumerable().AsAsync();

        var before = 1;
        foreach (var value in result) {
          Assert.That(value.Value, Is.EqualTo(before));
          before++;
        }
      }
    }

    [Test]
    public async Task DoQueryableTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = await session.Query.All<TestEntity>().Where(e => e.Value < 5 && e.Value > 0).AsAsync();

        var before = 1;
        foreach (var value in result) {
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
        var result = await container.EntitySet.Where(e => e.Value < 5 && e.Value > 0).AsAsync();

        var before = 1;
        foreach (var value in result) {
          Assert.That(value.Value, Is.EqualTo(before));
          before++;
        }
      }
    }

    [Test]
    public async Task DelayedQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var delayed = session.Query.ExecuteDelayed((q) => q.All<TestEntity>().Where(e => e.Value < 5 && e.Value > 0));
        var task = await delayed.AsAsync();

        var before = 1;
        foreach (var value in task) {
          Assert.That(value.Value, Is.EqualTo(before));
          before++;
        }
      }
    }

    private IEnumerable<T> AsEnumerable<T>(T a, T b, T c, T d)
    {
      yield return a;
      var counter = 0;
      while (counter < int.MaxValue/10000) {
        counter++;
      }
      yield return b;
      yield return c;
      yield return d;
    }
  }
}