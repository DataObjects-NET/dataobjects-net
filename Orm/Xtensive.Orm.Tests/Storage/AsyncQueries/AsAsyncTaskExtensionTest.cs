// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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

namespace Xtensive.Orm.Tests.Storage
{
  public class AsAsyncTaskExtensionTest : AutoBuildTest
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
          container.EntitySet.Add(new TestEntity(session) {Value = i});
        }
        transaction.Complete();
      }
    }

    [Test]
    public async Task PureEnumerableTest()
    {
      var task = AsEnumerable(1, 2, 3, 4).AsAsyncTask();
      Assert.That(task.IsCompleted, Is.True);

      int before = 1;
      foreach (var value in await task) {
        Assert.That(value, Is.EqualTo(before));
        before++;
      }
    }

    [Test]
    public async Task CollectionAsEnumerableTest()
    {
      var list = new List<int> {1, 2, 3, 4};
      var task = list.AsAsyncTask();
      Assert.That(task.IsCompleted, Is.True);

      int before = 1;
      foreach (var value in await task) {
        Assert.That(value, Is.EqualTo(before));
        before++;
      }
    }

    [Test]
    public async Task QueryableAsEnumearbleTest()
    {
      IQueryable<int> queryableEnumeable = new EnumerableQuery<int>(Enumerable.Range(-100, 200).ToArray()).Where(v => v < 5 && v > 0);
      var task = queryableEnumeable.AsEnumerable().AsAsyncTask();
      Assert.That(task.IsCompleted, Is.True);

      int before = 1;
      foreach (var value in await task) {
        Assert.That(value, Is.EqualTo(before));
        before++;
      }
    }

    [Test]
    public async Task QueryableTest()
    {
      IQueryable<int> queryableEnumeable = new EnumerableQuery<int>(Enumerable.Range(-100, 200).ToArray()).Where(v => v < 5 && v > 0);
      var task = queryableEnumeable.AsAsyncTask();
      Assert.That(task.IsCompleted, Is.True);

      int before = 1;
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
        var task = session.Query.All<TestEntity>().Where(e => e.Value < 5 && e.Value > 0).AsEnumerable().AsAsyncTask();
        Assert.That(task.IsCompleted, Is.True);

        int before = 1;
        foreach (var value in await task) {
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
        var task = container.EntitySet.Where(e => e.Value < 5 && e.Value > 0).AsEnumerable().AsAsyncTask();
        Assert.That(task.IsCompleted, Is.True);

        int before = 1;
        foreach (var value in await task) {
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
        var task = session.Query.All<TestEntity>().Where(e => e.Value < 5 && e.Value > 0).AsAsyncTask();
        Assert.That(task.IsCompleted, Is.False);

        int before = 1;
        foreach (var value in await task) {
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
        var task = container.EntitySet.Where(e => e.Value < 5 && e.Value > 0).AsAsyncTask();
        Assert.That(task.IsCompleted, Is.False);

        int before = 1;
        foreach (var value in await task) {
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
        var task = delayed.AsAsyncTask();
        Assert.That(task.IsCompleted, Is.False);

        int before = 1;
        foreach (var value in await task) {
          Assert.That(value.Value, Is.EqualTo(before));
          before++;
        }
      }
    }

    private IEnumerable<T> AsEnumerable<T>(T a, T b, T c, T d)
    {
      yield return a;
      int counter = 0;
      while (counter < Int32.MaxValue/10000) {
        counter++;
      }
      yield return b;
      yield return c;
      yield return d;
    }
  }
}