// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.28
#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
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
        Assert.AreEqual(TaskStatus.WaitingForActivation, task.Status);
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
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
        var task = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.AreEqual(TaskStatus.WaitingForActivation, task.Status);
        //Ensure that it is a task
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        //try to await task
        //by await starts all delayed tasks
        //and maybe delayed query
        //we must execute all delayed tasks as sync tasks
        var allDisceplines = session.Query.All<Discepline>().ToList();
        Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
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
        Assert.AreEqual(TaskStatus.WaitingForActivation, task1.Status);
        //we must execute delayed async query as async
        var task2 = await session.Query.ExecuteAsync(query => query.All<Discepline>());
        Assert.AreEqual(TaskStatus.RanToCompletion, task1.Status);
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
        Assert.AreEqual(TaskStatus.WaitingForActivation, task1.Status);
        Assert.AreEqual(TaskStatus.WaitingForActivation, task2.Status);
        Assert.AreEqual(TaskStatus.WaitingForActivation, task3.Status);
        Assert.AreEqual(TaskStatus.WaitingForActivation, globalTask.Status);
        await globalTask;
        Assert.AreEqual(TaskStatus.RanToCompletion, task1.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, task2.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, task3.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, globalTask.Status);
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
        var task = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        Assert.IsInstanceOf<DelayedTask<Discepline>>(task);
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
        var task = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d=>d.Discepline));
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var list = disceplinesOfLastYearCourse.ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.AreEqual(20, list.Count);
      }
    }

    [Test]
    public async void GetIEnumerableOfValuesWithCustomQueryKeyDirectlyUsingSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteDelayedAsync(new object(),query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d=>d.Discepline));
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var list = disceplinesOfLastYearCourse.ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.AreEqual(20,list.Count);
      }
    }

    [Test]
    public async void GetOrderedIEnumerableOfValuesDirectlyUsingSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).OrderBy(d=>d.Teacher));
        Assert.IsInstanceOf<DelayedTask<IEnumerable<DisceplinesOfCourse>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<DisceplinesOfCourse>>(disceplinesOfLastYearCourse);
        var list = disceplinesOfLastYearCourse.Select(d=>d.Discepline).ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.AreEqual(20, list.Count);
      }
    }

    [Test]
    public async void GetOrderedIEnmerableOfValuesDirectlyUsingSession()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteDelayedAsync(new object(), query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).OrderBy(d => d.Teacher));
        Assert.IsInstanceOf<DelayedTask<IEnumerable<DisceplinesOfCourse>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.IsInstanceOf<IEnumerable<DisceplinesOfCourse>>(disceplinesOfLastYearCourse);
        var list = disceplinesOfLastYearCourse.Select(d=>d.Discepline).ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.AreEqual(20, list.Count);
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
        Assert.IsInstanceOf<Discepline>(discepline);
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
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var task = Query.ExecuteFutureAsync(new object(), () => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var disceplinesOfCourses = await task;
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfCourses);
        var list = disceplinesOfCourses.ToList();
        Assert.NotNull(list);
        Assert.AreNotEqual(0, list.Count);
        Assert.AreEqual(20, list.Count);
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public async void DelayedTaskAwaitingOutOfTransaction()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        DelayedTask<Discepline> task;
        using (var transaction = session.OpenTransaction()) {
          task = session.Query.ExecuteDelayedAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        }
        var result = await task;
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public async void DelayedTaskAwaitingOutOfActiveSession()
    {
      using (var session = Domain.OpenSession()) {
        DelayedTask<Discepline> task;
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            task = Query.ExecuteFutureScalarAsync(() => Query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline).First());
          }
        }
        var result = await task;
      }
    }
  }
}
#endif