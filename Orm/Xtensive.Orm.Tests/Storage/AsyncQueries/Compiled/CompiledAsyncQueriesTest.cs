// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.28

#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class CompiledAsyncQueriesTest : AsyncQueriesBaseTest
  {
    private readonly SessionConfiguration inactivatedSession = new SessionConfiguration(SessionOptions.ServerProfile);

    [Test]
    public async void Test01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Console.WriteLine(transaction.Transaction.Guid);
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Console.WriteLine(transaction.Transaction.Guid);
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Console.WriteLine(transaction.Transaction.Guid);
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async void Test02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
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
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
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
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    [Test]
    public async void Test05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var qelayedQuery = session.Query.ExecuteDelayed(endpoint => { return endpoint.All<Discepline>(); });
        var task = session.Query.ExecuteAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.NotNull(((DelayedSequence<Discepline>) qelayedQuery).Task.Result);
      }
    }

    [Test]
    public async void Test06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var qelayedQuery = session.Query.ExecuteDelayed(query => query.All<Discepline>());
        var task = session.Query.ExecuteAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.NotNull(((DelayedSequence<Discepline>) qelayedQuery).Task.Result);
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public async void AsyncQueryOutOfSession02()
    {
      Task<Discepline> asyncQuery;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        asyncQuery = session.Query.ExecuteAsync((query) => session.Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        transaction.Complete();
      }
      var result = await asyncQuery;
    }

    [Test]
    public async void AsyncQueryInsideInactiveSessionTest01()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction()) {
        Console.WriteLine(session.IsActive);
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    public async void AsyncQueryInsideInactiveSessionTest02()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    public async void AsyncQueryInsideInactiveSessionTest03()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var disceplineOfLastYearCourse = await task;
      }
    }

    [Test]
    public async void AsyncQueryInsideInactiveSessionTest04()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var disceplineOfLastYearCourse = await task;
      }
    }

    [Test]
    public async void EventsNotificationTest()
    {
      var transactionOpeningCount = 0;
      var transactionOpenedCount = 0;
      var sessionOpeningCount = 0;
      var sessionOpenedCount = 0;
      Domain.SessionOpen += (sender, args) => sessionOpenedCount++;
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionOpening += (sender, args) => transactionOpeningCount++;
        session.Events.TransactionOpened+= (sender, args) => transactionOpenedCount++;
        using (var transaction = session.OpenTransaction()) {
          Console.WriteLine(transaction.Transaction.Guid);
          var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
          Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
          var disceplinesOfLastYearCourse = await task;
          Console.WriteLine(transaction.Transaction.Guid);
          Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
          var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
          Console.WriteLine(transaction.Transaction.Guid);
          Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
        }
      }
      Assert.AreEqual(1, sessionOpenedCount);
      Assert.AreEqual(1, transactionOpeningCount);
      Assert.AreEqual(1, transactionOpenedCount);
    }
  }
}
#endif
