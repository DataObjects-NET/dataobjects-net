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
          container.EntitySet.Add(new TestEntity(session) {Value = i});
        }
        transaction.Complete();
      }
    }

    [Test]
    public async Task QueryableTest()
    {
      IQueryable<int> queryableEnumeable = new EnumerableQuery<int>(Enumerable.Range(-100, 200).ToArray()).Where(v => v < 5 && v > 0);
      var task = queryableEnumeable.ExecuteAsync();
      Assert.That(task.IsCompleted, Is.True);

      int before = 1;
      foreach (var value in await task) {
        Assert.That(value, Is.EqualTo(before));
        before++;
      }
    }

    [Test]
    public async Task DoQueryableTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.All<TestEntity>().Where(e => e.Value < 5 && e.Value > 0).ExecuteAsync();
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
        var task = container.EntitySet.Where(e => e.Value < 5 && e.Value > 0).ExecuteAsync();
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
      await using (var session = await Domain.OpenSessionAsync())
      await using (var transaction = session.OpenTransaction()) {
        var delayed = session.Query.CreateDelayedQuery(
          q => q.All<TestEntity>().Where(e => e.Value < 5 && e.Value > 0));
        var task = delayed.ExecuteAsync();

        int before = 1;
        foreach (var value in await task) {
          Assert.That(value.Value, Is.EqualTo(before));
          before++;
        }
      }
    }
  }
}