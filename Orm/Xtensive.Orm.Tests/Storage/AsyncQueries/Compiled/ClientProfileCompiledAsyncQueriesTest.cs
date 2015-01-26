// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.09.02

#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public sealed class ClientProfileCompiledAsyncQueriesTest : AsyncQueriesBaseTest
  {
    private readonly SessionConfiguration inactiveSession = new SessionConfiguration(SessionOptions.ClientProfile);

    [Test]
    public async void Test01()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async void Test02()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async void Test03()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    [Test]
    public async void Test04()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    [Test]
    public async void Test09()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var qelayedQuery = session.Query.ExecuteDelayed(endpoint => { return endpoint.All<Discepline>(); });
        var task = session.Query.ExecuteAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.NotNull(((DelayedSequence<Discepline>)qelayedQuery).Task.Result);
      }
    }

    [Test]
    public async void AsyncQueryInsideInactiveSessionTest01()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        Console.WriteLine(session.IsActive);
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    public async void AsyncQueryInsideInactiveSessionTest02()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    public async void AsyncQueryInsideInactiveSessionTest03()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var disceplineOfLastYearCourse = await task;
      }
    }

    [Test]
    public async void AsyncQueryInsideInactiveSessionTest04()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var disceplineOfLastYearCourse = await task;
      }
    }

    [Test]
    public async void EventsNotificationTest()
    {
      var sessionOpenedCount = 0;
      Domain.SessionOpen += (sender, args) => sessionOpenedCount++;
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
      Assert.AreEqual(1, sessionOpenedCount);
    }
  }
}
#endif
