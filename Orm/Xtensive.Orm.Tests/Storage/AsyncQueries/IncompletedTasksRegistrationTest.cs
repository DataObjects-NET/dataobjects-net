﻿// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.29

#if NET45
using System;
using System.Linq;
using Xtensive.Orm.Internals;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class IncompletedTasksRegistrationTest : AsyncQueriesBaseTest
  {
    [Test]
    public void DisposingTransactionScopeWhileTaskIncompleted01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var asyncQueriesManager = session.AsyncQueriesManager;
        Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        Assert.AreEqual(1, asyncQueriesManager.WorkingAsyncQueriesCount);
      }
    }

    [Test]
    public void DisposingTransactionScopeWhileTaskIncompleted02()
    {
      var key = new object();
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var asyncQueriesManager = session.AsyncQueriesManager;
        Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
        var task = session.Query.ExecuteAsync(key, query => query.All<Discepline>());
        Assert.AreEqual(1, asyncQueriesManager.WorkingAsyncQueriesCount);
      }
    }

    [Test]
    public void DisposingTransactionScopeWhileTaskIncompleted03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var asyncQueriesManager = session.AsyncQueriesManager;
        Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>().First());
        Assert.AreEqual(1, asyncQueriesManager.WorkingAsyncQueriesCount);
      }
    }

    [Test]
    public void DisposingTransactionScopeWhileTaskIncompleted04()
    {
      var key = new object();
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var asyncQueriesManager = session.AsyncQueriesManager;
        Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
        var task = session.Query.ExecuteAsync(key, query => query.All<Discepline>().First());
        Assert.AreEqual(1, asyncQueriesManager.WorkingAsyncQueriesCount);
      }
    }

    [Test]
    public void DisposingTransactionScopeWhileTaskIncompleted05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var asyncQueriesManager = session.AsyncQueriesManager;
        Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
        //var aaa = session.Query.Execute(query => query.All<Discepline>().OrderBy(discepline => discepline.Name));
        var task = session.Query.ExecuteAsync(query => query.All<Discepline>().OrderBy(discepline => discepline.Name));
        Assert.AreEqual(1, asyncQueriesManager.WorkingAsyncQueriesCount);
      }
    }

    [Test]
    public void DisposingTransactionScopeWhileTaskIncompleted06()
    {
      var key = new object();
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var asyncQueriesManager = session.AsyncQueriesManager;
        Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
        var task = session.Query.ExecuteAsync(key, query => query.All<Discepline>().OrderBy(discepline => discepline.Name));
        Assert.AreEqual(1, asyncQueriesManager.WorkingAsyncQueriesCount);
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public void DisposingInnerTransaction()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var incompletedTasksRegistry = session.AsyncQueriesManager;
        Assert.AreEqual(0, incompletedTasksRegistry.WorkingAsyncQueriesCount);
        using (var innerTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
          var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        }
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public void DisposingInnerTransactionWhenSessionWithNonTransactionalReadsOption()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "No one storage library provides real async queries except Sql Server");
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads)))
      using (var transaction = session.OpenTransaction()) {
        var asyncQueriesManager = session.AsyncQueriesManager;
        Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
        using (var innerTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
          var task = session.Query.ExecuteAsync(query => query.All<Discepline>());
        }
      }
    }

    [Test]
    public void RollbackTransactionWhenTaskIncompleted()
    {
      bool exceptionCatched = false;
      using (var session = Domain.OpenSession()) {
        try {
          using (var transaction = session.OpenTransaction()) {
            new ClassWithParameterizedConstructor(session) {TextField = "Text string"};
            var task = session.Query.ExecuteAsync(query => query.All<Discepline>().First());
            transaction.Complete();
          }
        }
        catch (InvalidOperationException exception) {
          exceptionCatched = true;
        }
        Assert.IsTrue(exceptionCatched);
        using (var transaction = session.OpenTransaction()) {
          var count = session.Query.All<ClassWithParameterizedConstructor>().Count();
          Assert.AreEqual(0, count);
        }
      }
    }

    [Test]
    public void NotStartedDelayedTasksTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var delayedTask1 = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
          var delayedTask2 = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>());
          var delayedTask3 = session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());
          transaction.Complete();
          // Disposing of transaction scope doesn't throw exception because execution of delayed tasks didn't started.
        }
      }
    }

    [Test]
    public void StartedDelayedTasksTest()
    {
      using (var session = Domain.OpenSession()) {
        var exceptionCatched = false;
        try {
          using (var transaction = session.OpenTransaction()) {
            var asyncQueriesManager = session.AsyncQueriesManager;
            Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
            var delayedTask1 = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
            var delayedTask2 = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>());
            var delayedTask3 = session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());
            //Count equals to 0 because tasks didn't started.
            Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
            delayedTask1.ToTask();
            //Count equals to 1 because all delayed tasks execute in one real task
            Assert.AreEqual(1, asyncQueriesManager.WorkingAsyncQueriesCount);
            transaction.Complete();
          }
        }
        catch (InvalidOperationException exception) {
          exceptionCatched = true;
        }
        Assert.IsTrue(exceptionCatched);
      }
    }

    [Test]
    public async void StartingDelayedTaskOutOfOwnerTransaction01()
    {
      using (var session = Domain.OpenSession()) {
        var exceptionCatched = false;
        try {
          using (var transaction = session.OpenTransaction()) {
            var asyncQueriesManager = session.AsyncQueriesManager;
            Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
            DelayedTask<IEnumerable<Discepline>> delayedTask1;
            using (var innerTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              delayedTask1 = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
              var delayedTask2 = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>());
              var delayedTask3 = session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());
              transaction.Complete();
            }
            await delayedTask1;
          }
        }
        catch (Exception exception) {
          exceptionCatched = true;
        }
        Assert.IsTrue(exceptionCatched);
      }
    }

    [Test]
    public async void StartingDelayedTaskOutOfOwnerTransaction02()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads))) {
        var exceptionCatched = false;
        try {
          using (var transaction = session.OpenTransaction()) {
            var asyncQueriesManager = session.AsyncQueriesManager;
            Assert.AreEqual(0, asyncQueriesManager.WorkingAsyncQueriesCount);
            DelayedTask<IEnumerable<Discepline>> delayedTask1;
            using (var innerTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              delayedTask1 = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
              var delayedTask2 = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>());
              var delayedTask3 = session.Query.ExecuteDelayedAsync(query => query.All<Teacher>());
              transaction.Complete();
            }
            await delayedTask1;
          }
        }
        catch (Exception exception) {
          exceptionCatched = true;
        }
        Assert.IsTrue(exceptionCatched);
      }
    }

    [Test]
    [ExpectedException(typeof (StorageException))]
    public void DisposingHighLevelTransaction()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var session = Domain.OpenSession()) {
        var transaction = session.OpenTransaction();
        using (var innerTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
          using (var innerTransaction1 = session.OpenTransaction(TransactionOpenMode.New)) {
            var result = session.Query.ExecuteAsync(query => query.All<Discepline>());
            transaction.Dispose();
          }
        }
      }
    }
  }
}
#endif