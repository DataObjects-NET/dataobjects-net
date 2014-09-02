// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.28

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class DelayedAsyncQueries : AsyncQueriesBaseTest
  {
    //[Test]
    //public async void Test01()
    //{
    //  using (var session = Domain.OpenSession())
    //  using (session.Activate())
    //  using (var transaction = session.OpenTransaction()) {
    //    var task = session.Query.ExecuteDelayedAsync(query => query.All<DisciplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
    //    Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
    //    var disceplinesOfLastYearCourse = await task;
    //    Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
    //    var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
    //    Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
    //  }
    //}

    //[Test]
    //public async void Test02()
    //{
    //  using (var session = Domain.OpenSession())
    //  using (session.Activate())
    //  using (var transaction = session.OpenTransaction()) {
    //    var task = session.Query.ExecuteDelayedAsync(new object(),query => query.All<DisciplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
    //    Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
    //    var disceplinesOfLastYearCourse = await task;
    //    Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
    //    var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
    //    Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
    //  }
    //}

    [Test]
    public async void Test03()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<DisciplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
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
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(new object(),() => Query.All<DisciplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
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
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(() => Query.All<DisciplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
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
        var task = Query.ExecuteAsync(new object(), () => Query.All<DisciplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
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
        var task = Query.ExecuteAsync(() => Query.All<DisciplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
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
        var task = Query.ExecuteAsync(new object(), ()=>Query.All<DisciplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }
  }
}
