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
        var task = Query.ExecuteAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async void Test06()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(new object(), () => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async void Test07()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    [Test]
    public async void Test08()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(new object(), () => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    [Test]
    public async void Test09()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var qelayedQuery = session.Query.ExecuteDelayed(endpoint => { return endpoint.All<Discepline>(); });
        var task = session.Query.ExecuteAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.NotNull(((DelayedSequence<Discepline>) qelayedQuery).Task.Result);
      }
    }

    public async void Test10()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var qelayedQuery = session.Query.ExecuteDelayed(endpoint => { return endpoint.All<Discepline>(); });
        var task = Query.ExecuteAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.NotNull(((DelayedSequence<Discepline>) qelayedQuery).Task.Result);
      }
    }

    [Test]
    public async void Test11()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var qelayedQuery = Query.ExecuteFuture(() => { return Query.All<Discepline>(); });
        var task = session.Query.ExecuteAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.NotNull(((DelayedSequence<Discepline>) qelayedQuery).Task.Result);
      }
    }

    [Test]
    public async void Test12()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var delayedQuery = Query.ExecuteFuture(() => { return Query.All<Discepline>(); });
        var task = Query.ExecuteAsync(() => { return Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First(); });
        var result = await task;
        Assert.NotNull(((DelayedSequence<Discepline>) delayedQuery).Task.Result);
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException), ExpectedMessage = "Active Transaction is required for this operation. Use Session.OpenTransaction(...) to open it.")]
    public async void AsyncQueryAwaitingOutOfTransaction()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        Task<Discepline> delayedQuery;
        using (var transaction = session.OpenTransaction()) {
          delayedQuery = Query.ExecuteAsync(() => {
                                              Thread.Sleep(15000);
                                              return Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First();
                                            });
          transaction.Complete();
        }
        var result = await delayedQuery;
      }
    }

    [Test]
    public async void TransactionCompletingDuringAsyncQuery()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        Task<Discepline> delayedQuery;
        using (var transaction = session.OpenTransaction()) {
          delayedQuery = Query.ExecuteAsync(() => {
                                              Thread.Sleep(15000);
                                              return Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First();
                                            });
          transaction.Complete();
          Assert.False(delayedQuery.IsCompleted);
        }
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException), ExpectedMessage = "Active Transaction is required for this operation. Use Session.OpenTransaction(...) to open it.")]
    public async void AsyncQueryOutOfActivationSession()
    {
      using (var session = Domain.OpenSession()) {
        Task<Discepline> delayedQuery;
        using (session.Activate())
        using (var transaction = session.OpenTransaction()) {
          delayedQuery = Query.ExecuteAsync(() => {
                                              Thread.Sleep(15000);
                                              return Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First();
                                            });
          transaction.Complete();
        }
        var result = await delayedQuery;
      }
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Active Session is required for this operation. Use Session.Open(...) to open it.")]
    public async void AsyncQueryOutOfSession01()
    {
      Task<Discepline> delayedQuery;
      using (var session = Domain.OpenSession()) {
        using (session.Activate())
        using (var transaction = session.OpenTransaction()) {
          delayedQuery = Query.ExecuteAsync(() => {
                                              Thread.Sleep(15000);
                                              return Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First();
                                            });
          transaction.Complete();
        }
      }
      var result = await delayedQuery;
    }

    [Test]
    [ExpectedException(typeof (ObjectDisposedException))]
    public async void AsyncQueryOutOfSession02()
    {
      Task<Discepline> delayedQuery;
      using (var session = Domain.OpenSession()) {
        using (session.Activate())
        using (var transaction = session.OpenTransaction()) {
          delayedQuery = session.Query.ExecuteAsync((query) => {
            Thread.Sleep(15000);
            return session.Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First();
          });
          transaction.Complete();
        }
      }
      var result = await delayedQuery;
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
    [ExpectedException(typeof(InvalidOperationException))]
    public async void AsyncQueryInsideInactiveSessionTest05()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public async void AsyncQueryInsideInactiveSessionTest06()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(new object(), () => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        var disceplinesOfLastYearCourse = await task;
        disceplinesOfLastYearCourse.ToList();
      }
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public async void AsyncQueryInsideUnactiveSessionTest07()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var disceplineOfLastYearCourse = await task;
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public async void AsyncQueryInsideUnactiveSessionTest08()
    {
      using (var session = Domain.OpenSession(inactivatedSession))
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(new object(), () => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
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
