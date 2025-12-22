// Copyright (C) 2019-2020 Xtensive LLC.
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
using Xtensive.Orm.Internals;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public abstract class DelayedQueryTestBase : AsyncQueriesBaseTest
  {
    protected abstract SessionConfiguration SessionConfiguration { get; }

    [Test]
    public async Task GetScalarResultUsingSessionDirectly()
    {
      await using var session = await Domain.OpenSessionAsync(SessionConfiguration);
      await using (var tx = GetTransactionScope(session)) {
        var task = session.Query.CreateDelayedQuery(
          endpoint => endpoint.All<DisceplinesOfCourse>()
            .Where(el => el.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First()
        ).ExecuteAsync();
        Assert.That(task, Is.InstanceOf<ValueTask<Discepline>>());
        var result = await task;
        Assert.That(result, Is.InstanceOf<Discepline>());
        Assert.That(result, Is.Not.Null);
      }
    }

    [Test]
    public async Task GetIEnumerableOfResultsUsingSessionDirectly()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration))
      await using (var tx = GetTransactionScope(session)) {
        var query = session.Query.CreateDelayedQuery(endpoint => endpoint.All<DisceplinesOfCourse>()
          .Where(el => el.Course.Year==DateTime.Now.Year - 1)
          .Select(d => d.Discepline));
        Assert.That(query, Is.InstanceOf<DelayedQuery<Discepline>>());
        var result = await query.ExecuteAsync();
        Assert.That(result, Is.InstanceOf<QueryResult<Discepline>>());
        var disceplinesOfCourse = result.ToList();
        Assert.That(disceplinesOfCourse, Is.Not.Null);
        Assert.That(disceplinesOfCourse.Count, Is.Not.EqualTo(0));
        Assert.That(disceplinesOfCourse.Count, Is.EqualTo(20));
      }
    }

    [Test]
    public async Task GetOrderedIEnumerableOfResultsUsingSessionDirectly()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration))
      await using (var tx = GetTransactionScope(session)) {
        var query = session.Query.CreateDelayedQuery(endpoint =>
            endpoint.All<DisceplinesOfCourse>().Where(el => el.Course.Year==DateTime.Now.Year - 1)
              .Select(d => d.Discepline).OrderBy(d => d.Name));
        Assert.That(query, Is.InstanceOf<DelayedQuery<Discepline>>());
        var result = await query.ExecuteAsync();
        Assert.That(result, Is.InstanceOf<QueryResult<Discepline>>());
        var orderedDisceplines = result.ToList();
        Assert.That(orderedDisceplines, Is.Not.Null);
        Assert.That(orderedDisceplines.Count, Is.Not.EqualTo(0));
        Assert.That(orderedDisceplines.Count, Is.EqualTo(20));
      }
    }

    [Test]
    public async Task MultipleReadersTest()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration))
      await using (var tx = GetTransactionScope(session)) {

        foreach (var query1 in await session.Query.CreateDelayedQuery(q => q.All<Discepline>()).ExecuteAsync()) {
          foreach (var query2 in await session.Query.CreateDelayedQuery(q => q.All<Discepline>().OrderBy(e => e.Name)).ExecuteAsync()) {

          }
        }
      }
    }

    [Test]
    public async Task EnumerationOutsideSessionTest()
    {
      QueryResult<Discepline> result;
      await using (var session = Domain.OpenSession(SessionConfiguration))
      using (var tx = GetTransactionScope(session)) {
        result = await session.Query.CreateDelayedQuery(q => q.All<Discepline>())
          .ExecuteAsync();
      }

      var ex = Assert.Throws<InvalidOperationException>(() => result.ToList());
    }

    [Test]
    public async Task EnumerationInInnerTransactionTest()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration)) {
        QueryResult<Discepline> result;
        using (var outerTx = session.OpenTransaction()) {
          result = await session.Query.CreateDelayedQuery(q => q.All<Discepline>())
            .ExecuteAsync();
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
            // this is kind of valid behavior but just logically, I'd say that it is on the edge.
            // we cached results in the outer transaction then created a savepoint
            // and materialized the results after that.

            // logically after savepoint we should have an acces to everything what's made before
            // but i guess it may cause some problems
            _ = result.ToList();
          }
        }
      }
    }

    [Test]
    public async Task EnumerationInInnerVoidTransactionAsyncTest()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration)) {
        QueryResult<Discepline> result;
        using (var outerTx = session.OpenTransaction()) {
          result = await session.Query.CreateDelayedQuery(q => q.All<Discepline>())
            .ExecuteAsync();
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.Auto)) {
            _ = result.ToList();
          }
        }
      }
    }

    [Test]
    public async Task EnumerationInOuterTransactionAfterInnerRollbackTest()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration)) {
        QueryResult<Discepline> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
            result = await session.Query.CreateDelayedQuery(q => q.All<Discepline>())
              .ExecuteAsync();
          }

          _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
        }
      }
    }

    [Test]
    public async Task EnumerationInOuterTransactionAfterVoidInnerRollbackTest()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration)) {
        QueryResult<Discepline> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.Auto)) {
            result = await session.Query.CreateDelayedQuery(q => q.All<Discepline>())
              .ExecuteAsync();
          }
          _ = result.ToList();
        }
      }
    }

    [Test]
    public async Task EnumerationInOuterTransactionAfterInnerCommitTest()
    {
      Require.AllFeaturesSupported(Orm.Providers.ProviderFeatures.Savepoints);

      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration)) {
        QueryResult<Discepline> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
            result = await session.Query.CreateDelayedQuery(q => q.All<Discepline>())
              .ExecuteAsync();
            innerTx.Complete();
          }

          _ = result.ToList();
        }
      }
    }

    [Test]
    public async Task EnumerationInOuterTransactionAfterVoidInnerCommitTest()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration)) {
        QueryResult<Discepline> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.Auto)) {
            result = await session.Query.CreateDelayedQuery(q => q.All<Discepline>())
              .ExecuteAsync();
            innerTx.Complete();
          }

          _ = result.ToList();
        }
      }
    }

    private TransactionScope GetTransactionScope(Session session)
    {
      if (SessionConfiguration.Supports(SessionOptions.ServerProfile)) {
        return session.OpenTransaction();
      }
      return null;
    }
  }
}
