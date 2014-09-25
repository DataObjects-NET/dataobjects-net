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
    [Test]
    public async void DelayedTaskStartsByAwaitTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        //Create delayed task
        var task = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.AreEqual(task.Status, TaskStatus.WaitingForActivation);
        //Ensure that it is a task
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        //try to await task
        //by await starts all delayed tasks
        //and maybe delayed query
        var disceplinesOfLastYearCourse = await task;
        Assert.AreEqual(task.Status, TaskStatus.RanToCompletion);
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public void DelayedTasksStartsByQuery()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {  
        var task = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.AreEqual(task.Status, TaskStatus.WaitingForActivation);
        //Ensure that it is a task
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        //try to await task
        //by await starts all delayed tasks
        //and maybe delayed query
        //we must execute all delayed tasks as sync tasks
        var allDisceplines = session.Query.All<Discepline>();
        Assert.AreEqual(task.Status, TaskStatus.RanToCompletion);
        Assert.IsInstanceOf<IEnumerable<Discepline>>(allDisceplines);
        var listOfLastYearCourseDisceplines = allDisceplines.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async void DelayedTaskStartsBySessionExecuteDelayedQueries()
    { 
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task1 = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.AreEqual(task1.Status, TaskStatus.WaitingForActivation);
        //we must execute delayed async query as async
        var task2 = await session.Query.ExecuteAsync(query => query.All<Discepline>());
        Assert.AreEqual(task1.Status, TaskStatus.RanToCompletion);
      }
    }

    [Test]
    public async void ListOfDelayedTasksExecution()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task1 = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        var task2 = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>());
        var task3 = session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());
        var listOfTasks = new List<Task>();
        listOfTasks.Add(task1.ToTask());
        listOfTasks.Add(task2.ToTask());
        listOfTasks.Add(task3.ToTask());
        var globalTask = Task.WhenAll(listOfTasks);
        Assert.AreEqual(task1.Status, TaskStatus.Running);
        Assert.AreEqual(task2.Status, TaskStatus.Running);
        Assert.AreEqual(task3.Status, TaskStatus.Running);
        Assert.AreEqual(globalTask.Status, TaskStatus.Running);
        await globalTask;
        Assert.AreEqual(task1.Status, TaskStatus.RanToCompletion);
        Assert.AreEqual(task2.Status, TaskStatus.RanToCompletion);
        Assert.AreEqual(task3.Status, TaskStatus.RanToCompletion);
        Assert.AreEqual(globalTask.Status, TaskStatus.RanToCompletion);
      }
    }

    [Test]
    public async void GetScalarValueQueryWithCustomQueryKeyDirectlyUsingSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteDelayedAsync(new object(),query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
      }
    }
    
    [Test]
    public async void GetScalarValueDirectlyUsingSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    [Test]
    public async void GetIEnumerableOfValuesDirectlyUsingSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var list = disceplinesOfLastYearCourse.ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.Equals(20,list.Count);
      }
    }

    [Test]
    public async void GetIEnumerableOfValuesWithCustomQueryKeyDirectlyUsingSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(new object(),query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var list = disceplinesOfLastYearCourse.ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.Equals(20,list.Count);
      }
    }

    [Test]
    public async void GetOrderedIEnumerableOfValuesDirectlyUsingSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).OrderBy(d=>d.Teacher));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var list = disceplinesOfLastYearCourse.ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.Equals(20, list.Count);
      }
    }

    [Test]
    public async void GetOrderedIEnmerableOfValuesDirectlyUsingSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).OrderBy(d => d.Teacher));
        Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var list = disceplinesOfLastYearCourse.ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.Equals(20, list.Count);
      }
    }

    [Test]
    public async void GetScalarValueUsingOnDemandSesion()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteFutureScalarAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<DelayedTask<Discepline>>(task);
        var discepline = await task;
        Assert.IsInstanceOf<Discepline>(task);
        Assert.NotNull(discepline);
      }
    }

    [Test]
    public async void GetIEnumerableOfValesUsingOnDemandSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteFutureAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var disceplinesOfCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfCourse);
        var list = disceplinesOfCourse.ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.AreEqual(20, list.Count);
      }
    }

    [Test]
    public async void GetIEnumerableOfValuesWithCustomQueryKeyUsingOnDemandSession()
    {
      
    }

    [Test]
    public async void Test04()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteAsync(new object(),() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<Task<Discepline>>(task);
        var disceplineOfLastYearCourse = await task;
        Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
        Assert.NotNull(disceplineOfLastYearCourse);
      }
    }

    //[Test]
    //public async void Test05()
    //{
    //  using (var session = Domain.OpenSession())
    //  using (session.Activate())
    //  using (var transaction = session.OpenTransaction()) {
    //    var task = Query.ExecuteAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
    //    Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
    //    var disceplinesOfLastYearCourse = await task;
    //    Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
    //    var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
    //    Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
    //  }
    //}

    //[Test]
    //public async void Test06()
    //{
    //  using (var session = Domain.OpenSession())
    //  using (session.Activate())
    //  using (var transaction = session.OpenTransaction()) {
    //    var task = Query.ExecuteAsync(new object(), () => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
    //    Assert.IsInstanceOf<Task<IEnumerable<Discepline>>>(task);
    //    var disceplinesOfLastYearCourse = await task;
    //    Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
    //    var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
    //    Assert.AreEqual(20, listOfLastYearCourseDisceplines.Count);
    //  }
    //}

    //[Test]
    //public async void Test07()
    //{
    //  using (var session = Domain.OpenSession())
    //  using (session.Activate())
    //  using (var transaction = session.OpenTransaction()) {
    //    var task = Query.ExecuteAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
    //    Assert.IsInstanceOf<Task<Discepline>>(task);
    //    var disceplineOfLastYearCourse = await task;
    //    Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
    //    Assert.NotNull(disceplineOfLastYearCourse);
    //  }
    //}

    //[Test]
    //public async void Test08()
    //{
    //  using (var session = Domain.OpenSession())
    //  using (session.Activate())
    //  using (var transaction = session.OpenTransaction()) {
    //    var task = Query.ExecuteAsync(new object(), ()=>Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
    //    Assert.IsInstanceOf<Task<Discepline>>(task);
    //    var disceplineOfLastYearCourse = await task;
    //    Assert.IsInstanceOf<Discepline>(disceplineOfLastYearCourse);
    //    Assert.NotNull(disceplineOfLastYearCourse);
    //  }
    //}
  }
}
