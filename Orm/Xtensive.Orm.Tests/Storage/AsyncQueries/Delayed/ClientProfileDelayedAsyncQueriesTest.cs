// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.09.29

#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class ClientProfileDelayedAsyncQueriesTest : AsyncQueriesBaseTest
  {
    private SessionConfiguration clientProfileConfiguration = new SessionConfiguration(SessionOptions.ClientProfile);

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public async void AwaitOutOfSessionTest()
    {
      DelayedTask<Discepline> task;
      using (var session = Domain.OpenSession(clientProfileConfiguration)) {
        task = session.Query.ExecuteDelayedAsync(
          endpoint => endpoint.All<DisceplinesOfCourse>().Where(el => el.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
      }
      var result = await task;
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public async void AwaitOutOfTransaction()
    {
      DelayedTask<Discepline> task;
      using (var session = Domain.OpenSession(clientProfileConfiguration)) {
        using (var transaction = session.OpenTransaction()) {
          task = session.Query.ExecuteDelayedAsync(
            endpoint => endpoint.All<DisceplinesOfCourse>().Where(el => el.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        }
        var result = await task;
        Assert.NotNull(result);
      }
    }

    [Test]
    public async void GetScalarResultUsingSessionDirectly()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration)) {
        var task = session.Query.ExecuteDelayedAsync(
          endpoint => endpoint.All<DisceplinesOfCourse>().Where(el => el.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<DelayedTask<Discepline>>(task);
        var result = await task;
        Assert.IsInstanceOf<Discepline>(result);
        Assert.NotNull(result);
      }
    }

    [Test]
    public async void GetIEnumerableOfResultsUsingSessionDirectly()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration)) {
        var task = session.Query.ExecuteDelayedAsync(
          endpoint => endpoint.All<DisceplinesOfCourse>().Where(el => el.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var result = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(result);
        var disceplinesOfCourse = result.ToList();
        Assert.NotNull(disceplinesOfCourse);
        Assert.AreNotEqual(0, disceplinesOfCourse.Count);
        Assert.AreEqual(20, disceplinesOfCourse.Count);
      }
    }

    [Test]
    public async void GetOrderedIEnumerableOfResultsUsingSessionDirectly()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration)) {
        var task = session.Query.ExecuteDelayedAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(el => el.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).OrderBy(d => d.Name));
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var result = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(result);
        var orderedDisceplines = result.ToList();
        Assert.NotNull(orderedDisceplines);
        Assert.AreNotEqual(0, orderedDisceplines.Count);
        Assert.AreEqual(20, orderedDisceplines.Count);
      }
    }
  }
}
#endif
