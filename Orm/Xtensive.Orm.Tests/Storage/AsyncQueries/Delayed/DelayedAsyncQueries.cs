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
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.AreEqual(TaskStatus.Created, task.Status);
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var disceplinesOfLastYearCourse = await task;
        Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        Assert.IsInstanceOf<IEnumerable<Discepline>>(disceplinesOfLastYearCourse);
        var listOfLastYearCourseDisceplines = disceplinesOfLastYearCourse.ToList();
        Assert.AreEqual(DisceplinesPerCourse, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async void DelayedTasksStartedByAwaitTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var allCurrentYearDisceplinesTask = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year).Select(d => d.Discepline));
        var allLastYearDisceplinesTask = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year-1).Select(d => d.Discepline));
        var currentYearTeachersTask = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year).Select(d => d.Teacher));
        Assert.AreEqual(TaskStatus.Created, allCurrentYearDisceplinesTask.Status);
        Assert.AreEqual(TaskStatus.Created, allLastYearDisceplinesTask.Status);
        Assert.AreEqual(TaskStatus.Created, currentYearTeachersTask.Status);
        var allCurrentYearDisceplines = await allCurrentYearDisceplinesTask;
        Assert.AreEqual(TaskStatus.RanToCompletion, allCurrentYearDisceplinesTask.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, allLastYearDisceplinesTask.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, currentYearTeachersTask.Status);
        
        Assert.AreEqual(DisceplinesPerCourse, allLastYearDisceplinesTask.Result.Count());
        Assert.AreEqual(DisceplinesPerCourse, allCurrentYearDisceplinesTask.Result.Count());
        Assert.AreEqual(InitialTeachersCount,allCurrentYearDisceplines.Count());
      }
    }

    [Test]
    public void DelayedTasksStartsByQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {  
        var task = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.AreEqual(TaskStatus.Created, task.Status);
        Assert.IsInstanceOf<DelayedTask<IEnumerable<Discepline>>>(task);
        var allDisceplines = session.Query.All<Discepline>().ToList();
        Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        Assert.IsInstanceOf<IEnumerable<Discepline>>(allDisceplines);
        var listOfLastYearCourseDisceplines = allDisceplines.ToList();
        Assert.AreEqual(DisceplinesPerCourse, listOfLastYearCourseDisceplines.Count);
      }
    }

    [Test]
    public async void DelayedTaskStartsBySessionExecuteDelayedQueries()
    { 
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task1 = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.AreEqual(TaskStatus.Created, task1.Status);
        var task2 = await session.Query.ExecuteAsync(query => query.All<Discepline>());
        Assert.AreEqual(TaskStatus.RanToCompletion, task1.Status);
      }
    }

    [Test]
    public async void ListOfDelayedTasksExecution()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "No one storage library provides real async queries except MS Sql Server.");
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
        Assert.AreEqual(TaskStatus.Running, task1.Status);
        Assert.AreEqual(TaskStatus.Running, task2.Status);
        Assert.AreEqual(TaskStatus.Running, task3.Status);
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
    [ExpectedException(typeof (InvalidOperationException))]
    public async void DelayedTaskAwaitingOutOfTransaction()
    {
      using (var session = Domain.OpenSession()) {
        DelayedTask<Discepline> task;
        using (var transaction = session.OpenTransaction()) {
          task = session.Query.ExecuteDelayedAsync(endpoint => endpoint.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline).First());
        }
        var result = await task;
      }
    }

    [Test]
    public async void DelayedTaskCombinedWithDelayedQueryTest01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var allDisceplinesTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        var allDisceplinesOfLastYear = session.Query.ExecuteDelayed(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.AreEqual(TaskStatus.Created, allDisceplinesTask.Status);
        var allDisceplines = await allDisceplinesTask;
        Assert.AreEqual(TaskStatus.RanToCompletion, allDisceplinesTask.Status);
        Assert.AreEqual(InitialDisceplinesCount, allDisceplines.Count());
        Assert.AreEqual(DisceplinesPerCourse, allDisceplinesOfLastYear.Count());
      }
    }

    [Test]
    public void DelayedTaskCombinedWithDelayedQueryTest02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var allDisceplinesTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        var allDisceplinesOfLastYear = session.Query.ExecuteDelayed(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year == DateTime.Now.Year - 1).Select(d => d.Discepline));
        Assert.AreEqual(TaskStatus.Created, allDisceplinesTask.Status);
        Assert.AreEqual(DisceplinesPerCourse, allDisceplinesOfLastYear.Count());
        Assert.AreEqual(TaskStatus.RanToCompletion, allDisceplinesTask.Status);
        Assert.AreEqual(InitialDisceplinesCount, allDisceplinesTask.Result.Count());
      }
    }

    [Test]
    public void DelayedTasksExecutesWithInlineQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var disceplinesTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        var disceplinesOfCoursesTask = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>());
        var teachersTask = session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());
        Assert.AreEqual(TaskStatus.Created, disceplinesTask.Status);
        Assert.AreEqual(TaskStatus.Created, disceplinesOfCoursesTask.Status);
        Assert.AreEqual(TaskStatus.Created, teachersTask.Status);

        var specialities = session.Query.All<Speciality>().ToList();

        Assert.AreEqual(TaskStatus.RanToCompletion, disceplinesTask.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, disceplinesOfCoursesTask.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, teachersTask.Status);

        Assert.AreEqual(InitialSpecialitiesCount, specialities.Count);
        Assert.AreEqual(InitialDisceplinesCount, disceplinesTask.Result.Count());
        Assert.AreEqual(DisceplinesPerCourse * InitialCoursesCount, disceplinesOfCoursesTask.Result.Count());
        Assert.AreEqual(InitialTeachersCount, teachersTask.Result.Count());
      }
    }

    [Test]
    public async void DelayedTasksExecutesWithInlineAsyncQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var disceplinesTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        var disceplinesOfCoursesTask = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>());
        var teachersTask = session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());
        Assert.AreEqual(TaskStatus.Created, disceplinesTask.Status);
        Assert.AreEqual(TaskStatus.Created, disceplinesOfCoursesTask.Status);
        Assert.AreEqual(TaskStatus.Created, teachersTask.Status);

        var specialities = await session.Query.ExecuteAsync(query => query.All<Speciality>());

        Assert.AreEqual(TaskStatus.RanToCompletion, disceplinesTask.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, disceplinesOfCoursesTask.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, teachersTask.Status);

        Assert.AreEqual(InitialSpecialitiesCount, specialities.Count());
        Assert.AreEqual(InitialDisceplinesCount, disceplinesTask.Result.Count());
        Assert.AreEqual(DisceplinesPerCourse * InitialCoursesCount, disceplinesOfCoursesTask.Result.Count());
        Assert.AreEqual(InitialTeachersCount, teachersTask.Result.Count());
      }
    }

    [Test]
    public async void StartAntherDelayedTaskAfterStartedTheFirst()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var allSpecialitiesTask = session.Query.ExecuteDelayedAsync(query => query.All<Speciality>());
        var allDisceplinesTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        var allDisceplines = await allDisceplinesTask;
        var allTeachers = await session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());
        Assert.AreEqual(1, allSpecialitiesTask.Result.Count());
        Assert.AreEqual(InitialDisceplinesCount, allDisceplines.Count());
        Assert.AreEqual(InitialTeachersCount, allTeachers.Count());
      }
    }

    [Test]
    public async void StartAnotherDelayedTaskAfterStartedAsyncQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var allSpecialitiesTask = session.Query.ExecuteDelayedAsync(query => query.All<Speciality>());
        var allDisceplinesTask = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var allTeachers = await session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());
        Assert.AreEqual(1, allSpecialitiesTask.Result.Count());
        Assert.AreEqual(InitialDisceplinesCount, (await allDisceplinesTask).Count());
        Assert.AreEqual(InitialTeachersCount, allTeachers.Count());
      }
    }

    [Test]
    public void TwoPersistsStartedByAsyncQueryAndDelayedQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var oldTeacher = session.Query.All<Teacher>().First();
        var newTeacher = new Teacher(session){Name = "New teacher's name", Surname = "New teacher's surname."};
        oldTeacher.Name = "ChangedName";
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());//Persist #1;
        newTeacher.Name = "Edited teacher name";
        var teachers = session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());//Persist #2
      }
    }
  }
}
#endif