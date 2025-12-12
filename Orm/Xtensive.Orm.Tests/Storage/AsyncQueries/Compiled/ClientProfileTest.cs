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
        Assert.That(task, Is.InstanceOf<Task<QueryResult<Discepline>>>());
        var disceplinesOfLastYearCourse = await task;
        Assert.That(disceplinesOfLastYearCourse, Is.InstanceOf<IEnumerable<Discepline>>());
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.That(listOfLastYearCourseDisceplines.Count, Is.EqualTo(20));
      }
    }

    [Test]
    public async Task Test02()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.That(task, Is.InstanceOf<Task<QueryResult<Discepline>>>());
        var disceplinesOfLastYearCourse = await task;
        Assert.That(disceplinesOfLastYearCourse, Is.InstanceOf<IEnumerable<Discepline>>());
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.That(listOfLastYearCourseDisceplines.Count, Is.EqualTo(20));
      }
    }

    [Test]
    public async Task Test03()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.That(task, Is.InstanceOf<Task<Discepline>>());
        var disceplineOfLastYearCourse = await task;
        Assert.That(disceplineOfLastYearCourse, Is.InstanceOf<Discepline>());
        Assert.That(disceplineOfLastYearCourse, Is.Not.Null);
      }
    }

    [Test]
    public async Task Test04()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.That(task, Is.InstanceOf<Task<Discepline>>());
        var disceplineOfLastYearCourse = await task;
        Assert.That(disceplineOfLastYearCourse, Is.InstanceOf<Discepline>());
        Assert.That(disceplineOfLastYearCourse, Is.Not.Null);
      }
    }

    [Test]
    public async Task Test09()
    {
      using (var session = Domain.OpenSession(inactiveSession)) {
        var qelayedQuery = session.Query.CreateDelayedQuery(endpoint => { return endpoint.All<Discepline>(); });
        var task = session.Query.ExecuteAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        var result = await task;
        Assert.That(((DelayedQuery<Discepline>)qelayedQuery).Task.Result, Is.Not.Null);
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
        Assert.That(task, Is.InstanceOf<Task<QueryResult<Discepline>>>());
        var disceplinesOfLastYearCourse = await task;
        Assert.That(disceplinesOfLastYearCourse, Is.InstanceOf<IEnumerable<Discepline>>());
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.That(listOfLastYearCourseDisceplines.Count, Is.EqualTo(20));
      }
      Assert.That(sessionOpenedCount, Is.EqualTo(1));
    }

    [Test]
    public async Task AsyncQueryResultsEnumeration()
    {
      Session session = null;
      try
      {
        session = Domain.OpenSession(inactiveSession);
        using (var transaction = session.OpenTransaction())
          foreach (var item in (await session.Query.All<Discepline>().ExecuteAsync()))
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
      try {
        session = Domain.OpenSession(inactiveSession);
        using (var transaction = session.OpenTransaction())
          foreach (var item in await session.Query.All<Discepline>().ExecuteAsync()) {
            break;
          }
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
        session = Domain.OpenSession(inactiveSession);
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
