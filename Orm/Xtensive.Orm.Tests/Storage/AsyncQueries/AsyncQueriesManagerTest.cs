// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.12.15

#if NET45
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class AsyncQueriesManagerTest : AsyncQueriesBaseTest
  {
    [Test]
    public async void AsyncQueryRegistration()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task1 = session.Query
          .ExecuteAsync(
            query => query.All<Teacher>()
              .Where(teacher=>(teacher.Name.Contains("s") || teacher.Name.Contains("S") && teacher.Surname.Contains("N")))
              .LeftJoin(query.All<DisceplinesOfCourse>(), 
                        teacher => teacher.Id, 
                        course => course.Teacher.Id, 
                        (teacher, course) => new{ TeacherName = teacher.Name, TeacherSurname = teacher.Surname, TeacherId = teacher.Id, Discepline = course.Discepline})
          );
        Assert.IsTrue(session.HasIncompletedAsyncQueries());
        Assert.IsTrue(session.HasIncompletedAsyncQueriesForToken(transaction.Transaction.LifetimeToken));
        Assert.IsFalse(session.HasBlockingQueries());
        Assert.IsFalse(session.HasBlockingCommandsForToken(transaction.Transaction.LifetimeToken));

        var task2 = session.Query
          .ExecuteAsync(
            query => query.All<DisceplinesOfCourse>()
              .Where(doc=> doc.Course.Year==DateTime.Now.Year)
              .Join(((IQueryable<Discepline>)query.All<Discepline>().Where(d=>d.Name.Contains("A"))),
              course => course.Discepline.Id,
              discepline => discepline.Id, (course, discepline) => new {DisceplineName = discepline.Name, Teacher = course.Teacher}));
        Assert.IsTrue(session.HasIncompletedAsyncQueries());
        Assert.IsTrue(session.HasIncompletedAsyncQueriesForToken(transaction.Transaction.LifetimeToken));
        Assert.IsFalse(session.HasBlockingQueries());
        Assert.IsFalse(session.HasBlockingCommandsForToken(transaction.Transaction.LifetimeToken));

        var result1 = await task1;
        Assert.IsTrue(session.HasIncompletedAsyncQueries());
        Assert.IsTrue(session.HasIncompletedAsyncQueriesForToken(transaction.Transaction.LifetimeToken));
        Assert.IsTrue(session.HasBlockingQueries());
        Assert.IsTrue(session.HasBlockingCommandsForToken(transaction.Transaction.LifetimeToken));

        result1.FirstOrDefault();
        Assert.IsTrue(session.HasIncompletedAsyncQueries());
        Assert.IsTrue(session.HasIncompletedAsyncQueriesForToken(transaction.Transaction.LifetimeToken));
        Assert.IsFalse(session.HasBlockingQueries());
        Assert.IsFalse(session.HasBlockingCommandsForToken(transaction.Transaction.LifetimeToken));

        var result2 = await task2;
        Assert.IsFalse(session.HasIncompletedAsyncQueries());
        Assert.IsFalse(session.HasIncompletedAsyncQueriesForToken(transaction.Transaction.LifetimeToken));
        Assert.IsTrue(session.HasBlockingQueries());
        Assert.IsTrue(session.HasBlockingCommandsForToken(transaction.Transaction.LifetimeToken));

        result2.FirstOrDefault();
        Assert.IsFalse(session.HasIncompletedAsyncQueries());
        Assert.IsFalse(session.HasIncompletedAsyncQueriesForToken(transaction.Transaction.LifetimeToken));
        Assert.IsFalse(session.HasBlockingQueries());
        Assert.IsFalse(session.HasBlockingCommandsForToken(transaction.Transaction.LifetimeToken));
      }
    }

    [Test]
    public void CancellationUnawaitableTaskOnTransactionDisposing()
    {
      Exception exception = null;
      try {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var name = MethodBase.GetCurrentMethod().Name;
          new Teacher(session) {Gender = Gender.Male, Name = name, Surname = name, DateOfBirth = DateTime.Now.AddYears(-20)};
          var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
          transaction.Complete();
        }
      }
      catch (InvalidOperationException ex) {
        exception = ex;
      }
      Assert.IsNotNull(exception);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var name = MethodBase.GetCurrentMethod().Name;
        var teacher = session.Query.All<Teacher>().Where(tchr => tchr.Name==name).FirstOrDefault();
        Assert.IsNull(teacher);
      }
    }

    [Test]
    public async void CancellationOfBlockingCommandsOnTransactionDisposing()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var name = MethodBase.GetCurrentMethod().Name;
        new Teacher(session) {Gender = Gender.Male, Name = name, Surname = name, DateOfBirth = DateTime.Now.AddYears(-20)};
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var disceplines = await task;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var name = MethodBase.GetCurrentMethod().Name;
        var teacher = session.Query.All<Teacher>().Where(tchr => tchr.Name==name).FirstOrDefault();
        Assert.IsNotNull(teacher);
      }
    }

    [Test]
    public void TryOpenVoidTransactionWhenAsyncTaskNotCompleted()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        //DO creates VoidScope
        using (var transaction1 = session.OpenTransaction())
        { }
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public void TryOpenNewTransactionWhenAsyncTaskNotCompleted()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        
        using (var transaction1 = session.OpenTransaction(TransactionOpenMode.New))
        { }
      }
    }

    [Test]
    public async void TryOpenVoidTransactionWhenThereIsBlockingCommand()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var disceplines = await task;
        using (var transaction1 = session.OpenTransaction())
        { }
      }
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public async void TryOpenNewTransactionWhenThereIsBlockingCommand()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var disceplines = await task;
        using (var transaction1 = session.OpenTransaction(TransactionOpenMode.New))
        { }
      }
    }

    [Test]
    public async void NonTransactionalReadsOpenTransacton()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads);
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var disceplines = await task;
        disceplines.First();
      }
    }

    [Test]
    public void NonTransactionalReadsOpenTransaction01()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads);
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        using (var transaction = session.OpenTransaction()) { }
      }
    }

    [Test]
    public void NonTransactionalReadsOpenTransaction02()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads);
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        using (var transaction = session.OpenTransaction(TransactionOpenMode.New)) { }
      }
    }

    [Test]
    public async void NonTransactionalReadsOpenTransaction03()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads);
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var disceplines = await task;
        using (var transaction = session.OpenTransaction()) { }
      }
    }

    [Test]
    public async void NonTransactionalReadsOpenTransaction04()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads);
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var disceplines = await task;
        using (var transaction = session.OpenTransaction(TransactionOpenMode.New)) { }
      }
    }

    [Test]
    public void NonTransactionalReadsAsyncQueryNotFinished()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads);
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
      }
    }

    [Test]
    public async void NonTransactionalReadsThereIsBlockingCommand()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads);
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var disceplines = await task;
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public async void TryStartAnotherAsyncQueryWhenThereIsIncompletedTask()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new Teacher(session) {Name = MethodBase.GetCurrentMethod().Name, Surname = MethodBase.GetCurrentMethod().Name, DateOfBirth = DateTime.Now, Gender = Gender.Male};
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        new Teacher(session) {Name = MethodBase.GetCurrentMethod() + "1", Surname = MethodBase.GetCurrentMethod() + "1", DateOfBirth = DateTime.Now, Gender = Gender.Female};
        var anotherTask = session.Query.ExecuteAsync(query => query.All<DisceplinesOfCourse>());
        await anotherTask;
      }
    }

    [Test]
    public async void PersistChangedEnititiesWithAsyncQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var teacher1 = new Teacher(session) { Name = "PersistChangedEnititiesWithAsyncQuery" + "1", Surname = "PersistChangedEnititiesWithAsyncQuery", DateOfBirth = DateTime.Now, Gender = Gender.Male };
        var teacher2 = new Teacher(session) { Name = "PersistChangedEnititiesWithAsyncQuery" + "2", Surname = "PersistChangedEnititiesWithAsyncQuery", DateOfBirth = DateTime.Now, Gender = Gender.Male };
        var teacher3 = new Teacher(session) { Name = "PersistChangedEnititiesWithAsyncQuery" + "3", Surname = "PersistChangedEnititiesWithAsyncQuery", DateOfBirth = DateTime.Now, Gender = Gender.Male };
        var disceplines = await session.Query.ExecuteAsync(query => query.All<Discepline>());
        var lastTeachers = session.Query.All<Teacher>().Where(t => t.Surname == "PersistChangedEnititiesWithAsyncQuery").OrderBy(t => t.DateOfBirth).Take(3).ToArray();
        Assert.AreEqual(3, lastTeachers.Length);
        Assert.IsTrue(lastTeachers.Contains(teacher1));
        Assert.IsTrue(lastTeachers.Contains(teacher2));
        Assert.IsTrue(lastTeachers.Contains(teacher3));
      }
    }

    [Test]
    public async void TryPersistChangesAfterAsyncQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var teacher1 = new Teacher(session) { Name = "TryPersistChangesAfterAsyncQuery" + "1", Surname = "TryPersistChangesAfterAsyncQuery", DateOfBirth = DateTime.Now, Gender = Gender.Male };
        var teacher2 = new Teacher(session) { Name = "TryPersistChangesAfterAsyncQuery" + "2", Surname = "TryPersistChangesAfterAsyncQuery", DateOfBirth = DateTime.Now, Gender = Gender.Male };
        var teacher3 = new Teacher(session) { Name = "TryPersistChangesAfterAsyncQuery" + "3", Surname = "TryPersistChangesAfterAsyncQuery", DateOfBirth = DateTime.Now, Gender = Gender.Male };

        Assert.Throws<InvalidOperationException>(() => { var teachers = session.Query.All<Teacher>().ToArray(); });
        var disceplines = await task;
        foreach (var discepline in disceplines) {
          Console.WriteLine(discepline);
        }
      }
    }

    [Test]
    public async void TrySaveChangesDuringAsyncQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        var teacher1 = new Teacher(session) { Name = "TryPersistChangesAfterAsyncQuery" + "1", Surname = "TryPersistChangesAfterAsyncQuery", DateOfBirth = DateTime.Now, Gender = Gender.Male };
        var teacher2 = new Teacher(session) { Name = "TryPersistChangesAfterAsyncQuery" + "2", Surname = "TryPersistChangesAfterAsyncQuery", DateOfBirth = DateTime.Now, Gender = Gender.Male };
        var teacher3 = new Teacher(session) { Name = "TryPersistChangesAfterAsyncQuery" + "3", Surname = "TryPersistChangesAfterAsyncQuery", DateOfBirth = DateTime.Now, Gender = Gender.Male };

        Assert.Throws<InvalidOperationException>(session.SaveChanges);
        var disceplines = await task;
        foreach (var discepline in disceplines) {
          Console.WriteLine(discepline);
        }
      }
    }
  }
}
#endif