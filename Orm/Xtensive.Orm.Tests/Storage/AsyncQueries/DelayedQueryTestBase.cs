﻿// Copyright (C) 2019 Xtensive LLC.
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
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public abstract class DelayedQueryTestBase : AsyncQueriesBaseTest
  {
    protected abstract SessionConfiguration SessionConfiguration { get; }

    [Test]
    public async Task GetScalarResultUsingSessionDirectly()
    {
      using (var session = Domain.OpenSession(SessionConfiguration))
      using (var tx = GetTransactionScope(session)) {
        var task = session.Query.ExecuteDelayed(
          endpoint => endpoint.All<DisceplinesOfCourse>().Where(el => el.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First()).AsAsync();
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var result = await task;
        Assert.IsInstanceOf<Discepline>(result);
        Assert.NotNull(result);
      }
    }

    [Test]
    public async Task GetIEnumerableOfResultsUsingSessionDirectly()
    {
      using (var session = Domain.OpenSession(SessionConfiguration))
      using (var tx = GetTransactionScope(session)) {
        var task = session.Query.ExecuteDelayed(
          endpoint => endpoint.All<DisceplinesOfCourse>().Where(el => el.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline)).AsAsync();
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var result = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(result);
        var disceplinesOfCourse = result.ToList();
        Assert.NotNull(disceplinesOfCourse);
        Assert.AreNotEqual(0, disceplinesOfCourse.Count);
        Assert.AreEqual(20, disceplinesOfCourse.Count);
      }
    }

    [Test]
    public async Task GetOrderedIEnumerableOfResultsUsingSessionDirectly()
    {
      using (var session = Domain.OpenSession(SessionConfiguration))
      using (var tx = GetTransactionScope(session)) {
        var task = session.Query.ExecuteDelayed(endpoint =>
            endpoint.All<DisceplinesOfCourse>().Where(el => el.Course.Year==DateTime.Now.Year - 1)
              .Select(d => d.Discepline).OrderBy(d => d.Name))
          .AsAsync();
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var result = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(result);
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
