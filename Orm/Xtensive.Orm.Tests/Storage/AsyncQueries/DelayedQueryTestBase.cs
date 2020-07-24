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
        Assert.IsInstanceOf<ValueTask<Discepline>>(task);
        var result = await task;
        Assert.IsInstanceOf<Discepline>(result);
        Assert.NotNull(result);
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
        Assert.IsInstanceOf<DelayedQuery<Discepline>>(query);
        var result = await query.ExecuteAsync();
        Assert.IsInstanceOf<QueryResult<Discepline>>(result);
        var disceplinesOfCourse = result.ToList();
        Assert.NotNull(disceplinesOfCourse);
        Assert.AreNotEqual(0, disceplinesOfCourse.Count);
        Assert.AreEqual(20, disceplinesOfCourse.Count);
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
        Assert.IsInstanceOf<DelayedQuery<Discepline>>(query);
        var result = await query.ExecuteAsync();
        Assert.IsInstanceOf<QueryResult<Discepline>>(result);
        var orderedDisceplines = result.ToList();
        Assert.NotNull(orderedDisceplines);
        Assert.AreNotEqual(0, orderedDisceplines.Count);
        Assert.AreEqual(20, orderedDisceplines.Count);
      }
    }

    private TransactionScope GetTransactionScope(Session session)
    {
      if (SessionConfiguration.Supports(SessionOptions.ServerProfile))
        return session.OpenTransaction();
      return null;
    }
  }
}
