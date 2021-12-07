﻿// Copyright (C) 2019-2020 Xtensive LLC.
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

namespace Xtensive.Orm.Tests.Storage.AsyncQueries.Compiled
{
  public class ServerProfileTest : AsyncQueriesBaseTest
  {
    private readonly SessionConfiguration inactivatedSession = new SessionConfiguration(SessionOptions.ServerProfile);

    [Test]
    public async Task Test01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Console.WriteLine(transaction.Transaction.Guid);
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<QueryResult<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Console.WriteLine(transaction.Transaction.Guid);
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Console.WriteLine(transaction.Transaction.Guid);
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async Task Test02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<QueryResult<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async Task Test03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
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
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    [Test]
    public async Task Test05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        var qelayedQuery = session.Query.CreateDelayedQuery(endpoint => { return endpoint.All<Discepline>(); });
        var task = session.Query.ExecuteAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.NotNull(((DelayedQuery<Discepline>)qelayedQuery).Task.Result);
      }
    }

    [Test]
    public async Task Test06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        var qelayedQuery = session.Query.CreateDelayedQuery(query => query.All<Discepline>());
        var task = session.Query.ExecuteAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.NotNull(((DelayedQuery<Discepline>)qelayedQuery).Task.Result);
      }
    }

    [Test]
    public async Task AsyncQueryInsideInactiveSessionTest01()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction())
      {
        Console.WriteLine(session.IsActive);
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    public async Task AsyncQueryInsideInactiveSessionTest02()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction())
      {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    public async Task AsyncQueryInsideInactiveSessionTest03()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction())
      {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var disceplineOfLastYearCourse = await task;
      }
    }

    [Test]
    public async Task AsyncQueryInsideInactiveSessionTest04()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction())
      {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var disceplineOfLastYearCourse = await task;
      }
    }

    [Test]
    public async Task EventsNotificationTest()
    {
      var transactionOpeningCount = 0;
      var transactionOpenedCount = 0;
      var sessionOpenedCount = 0;
      Domain.SessionOpen += (sender, args) => sessionOpenedCount++;
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionOpening += (sender, args) => transactionOpeningCount++;
        session.Events.TransactionOpened += (sender, args) => transactionOpenedCount++;
        using (var transaction = session.OpenTransaction()) {
          Console.WriteLine(transaction.Transaction.Guid);
          var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
          Assert.IsInstanceOf<Task<QueryResult<Discepline>>>(task);
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

    [Test]
    public async Task AsyncQueryResultsEnumeration()
    {
      Session session = null;
      try {
        session = Domain.OpenSession();
        using (var transaction = session.OpenTransaction())
          foreach (var item in (await session.Query.All<Discepline>().ExecuteAsync())) { }
      }
      finally {
        session.DisposeSafely();
      }
    }

    [Test]
    public async Task AsyncQueryResultsInterruptedEnumeration()
    {
      Session session = null;
      try {
        session = Domain.OpenSession();
        using (var transaction = session.OpenTransaction())
          foreach (var item in await session.Query.All<Discepline>().ExecuteAsync())
            break;
      }
      finally {
        session.DisposeSafely();
      }
    }

    [Test]
    public async Task RollbackTransactionWhileReaderIsOpen()
    {
      Session session = null;
      try {
        session = Domain.OpenSession();
        IEnumerator<Discepline> enumerator;
        using (var transaction = session.OpenTransaction()) {
          enumerator = (await session.Query.All<Discepline>().ExecuteAsync()).GetEnumerator();
          enumerator.MoveNext();
          var a = enumerator.Current;
        }
        enumerator.Dispose();
      }
      finally {
        session.DisposeSafely();
      }
    }
  }
}
