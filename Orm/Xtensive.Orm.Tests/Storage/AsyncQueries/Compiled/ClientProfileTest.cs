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
using Xtensive.Orm.Internals;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries.Compiled
{
  public sealed class ClientProfileTest : AsyncQueriesBaseTest
  {
    private readonly SessionConfiguration inactiveSession = new SessionConfiguration(SessionOptions.ClientProfile);

    [Test]
    public async Task Test01()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async Task Test02()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async Task Test03()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    [Test]
    public async Task Test04()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    [Test]
    public async Task Test09()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var qelayedQuery = session.Query.ExecuteDelayed(endpoint => { return endpoint.All<Discepline>(); });
        var task = session.Query.ExecuteAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.NotNull(((DelayedSequence<Discepline>)qelayedQuery).Task.Result);
      }
    }

    [Test]
    public async Task AsyncQueryInsideInactiveSessionTest01()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        Console.WriteLine(session.IsActive);
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    public async Task AsyncQueryInsideInactiveSessionTest02()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    public async Task AsyncQueryInsideInactiveSessionTest03()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var disceplineOfLastYearCourse = await task;
      }
    }

    [Test]
    public async Task AsyncQueryInsideInactiveSessionTest04()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var disceplineOfLastYearCourse = await task;
      }
    }

    [Test]
    public async Task EventsNotificationTest()
    {
      var sessionOpenedCount = 0;
      Domain.SessionOpen += (sender, args) => sessionOpenedCount++;
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
      Assert.AreEqual(1, sessionOpenedCount);
    }

    [Test]
    public async Task AsyncQueryResultsEnumeration()
    {
      Session session = null;
      try
      {
        session = Domain.OpenSession(inactiveSession);
        using (var transaction = session.OpenTransaction())
          foreach (var item in (await session.Query.All<Discepline>().AsAsyncTask()))
          {
          }
      }
      finally
      {
        session.DisposeSafely();
      }
    }

    [Test]
    public async Task AsyncQueryResultsInterruptedEnumeration()
    {
      Session session = null;
      try
      {
        session = Domain.OpenSession(inactiveSession);
        using (var transaction = session.OpenTransaction())
          foreach (var item in await session.Query.All<Discepline>().AsAsyncTask())
          {
            break;
          }
      }
      finally
      {
        session.DisposeSafely();
      }
    }

    [Test]
    public async Task RollbackTransactionWhileReaderIsOpen()
    {
      Session session = null;
      try
      {
        session = Domain.OpenSession(inactiveSession);
        IEnumerator<Discepline> enumerator;
        using (var transaction = session.OpenTransaction())
        {
          enumerator = (await session.Query.All<Discepline>().AsAsyncTask()).GetEnumerator();
          enumerator.MoveNext();
          var a = enumerator.Current;
        }
        enumerator.Dispose();
      }
      finally
      {
        session.DisposeSafely();
      }
    }
  }
}
