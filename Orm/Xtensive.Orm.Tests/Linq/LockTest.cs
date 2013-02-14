// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.08.25

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  public class LockTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.SingleSessionAccess);
    }

    [Test]
    public void UpdateLockThrowTest()
    {
      var catchedException = ExecuteConcurrentQueries(LockMode.Update, LockBehavior.Wait,
        LockMode.Update, LockBehavior.ThrowIfLocked);
//      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }

    [Test]
    public void LockNewlyCreatedEntity()
    {
      var product = new ActiveProduct();
      using (Session.DisableSaveChanges())
        product.Lock(LockMode.Exclusive, LockBehavior.ThrowIfLocked);
    }

    [Test]
    public void UpdateLockSkipTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var key = Session.Query.All<Customer>().First().Key;
      var expected = Session.Query.All<Customer>().Where(c => c.Key == key)
        .Lock(LockMode.Update, LockBehavior.Wait).ToList();
      Exception catchedException = null;
      int countAfterSkip = 0;
      var secondThread = new Thread(() => {
        try {
          using (var session = Domain.OpenSession())
          using (session.OpenTransaction())
            countAfterSkip = session.Query.All<Customer>()
              .Where(c => c.Key == key)
              .Lock(LockMode.Update, LockBehavior.Skip)
              .ToList().Count;
        }
        catch(Exception e) {
          catchedException = e;
        }
      });
      secondThread.Start();
      secondThread.Join();
      if (catchedException != null)
        throw catchedException;
      Assert.AreEqual(0, countAfterSkip);
    }

    [Test]
    public void ShareLockThrowTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      var catchedException = ExecuteConcurrentQueries(LockMode.Update, LockBehavior.Wait,
        LockMode.Shared, LockBehavior.ThrowIfLocked);
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }
    
    [Test]
    public void ExclusiveLockThrowTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var catchedException = ExecuteConcurrentQueries(LockMode.Update, LockBehavior.Wait,
        LockMode.Exclusive, LockBehavior.ThrowIfLocked);
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }

    [Test]
    public void ShareLockTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.PostgreSql);
      var catchedException = ExecuteConcurrentQueries(LockMode.Shared, LockBehavior.ThrowIfLocked,
        LockMode.Shared, LockBehavior.ThrowIfLocked);
      Assert.IsNull(catchedException);
    }
    
    [Test]
    public void LockAfterJoinTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.PostgreSql);
      var customerKey = Session.Query.All<Customer>().First().Key;
      var orderKey = Session.Query.All<Order>().Where(o => o.Customer.Key==customerKey).First().Key;
      Exception firstThreadException = null;
      Exception result = null;
      var firstEvent = new ManualResetEvent(false);
      var secondEvent = new ManualResetEvent(false);
      var firstThread = new Thread(() => {
        try {
          using (var session = Domain.OpenSession())
          using (session.OpenTransaction()) {
            session.Query.All<Customer>().Where(c => c.Key == customerKey)
              .Join(session.Query.All<Order>().Where(o => o.Key == orderKey), c => c, o => o.Customer, (c, o) => c)
              .Lock(LockMode.Update, LockBehavior.Wait).ToList();
            secondEvent.Set();
            firstEvent.WaitOne();
          }
        }
        catch(Exception e) {
          firstThreadException = e;
          secondEvent.Set();
          return;
        }
      });
      firstThread.Start();
      secondEvent.WaitOne();
      var secondException = ExecuteQueryAtSeparateThread(s => s.Query.All<Customer>()
        .Where(c => c.Key==customerKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      Assert.AreEqual(typeof(StorageException), secondException.GetType());
      var thirdException = ExecuteQueryAtSeparateThread(s => s.Query.All<Order>()
        .Where(o => o.Key==orderKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      Assert.AreEqual(typeof(StorageException), thirdException.GetType());
      firstEvent.Set();
      firstThread.Join();
      if (firstThreadException != null)
        throw firstThreadException;
    }

    [Test]
    public void LockEntityTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.PostgreSql);
      var customer = Session.Query.All<Customer>().First();
      var customerKey = customer.Key;
      customer.Lock(LockMode.Update, LockBehavior.Wait);
      var catchedException = ExecuteQueryAtSeparateThread(s =>
        s.Query.All<Customer>().Where(c => c == customer).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
        AssertEx.Throws<StorageException>(() =>
          session.Query.Single<Customer>(customerKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
    }
    
    private Exception ExecuteConcurrentQueries(LockMode lockMode0, LockBehavior lockBehavior0,
      LockMode lockMode1, LockBehavior lockBehavior1)
    {
      var key = Session.Query.All<Customer>().First().Key;
      Exception firstThreadException = null;
      Exception result = null;
      var firstEvent = new ManualResetEvent(false);
      var secondEvent = new ManualResetEvent(false);
      var firstThread = new Thread(() => {
        try {
          using (var session = Domain.OpenSession())
          using (session.OpenTransaction()) {
            session.Query.All<Customer>().Where(c => c.Key == key).Lock(lockMode0, lockBehavior0).ToList();
            secondEvent.Set();
            firstEvent.WaitOne();
          }
        }
        catch(Exception e) {
          firstThreadException = e;
          secondEvent.Set();
          return;
        }
      });
      firstThread.Start();
      secondEvent.WaitOne();
      if (firstThreadException != null)
        throw firstThreadException;
      result = ExecuteQueryAtSeparateThread(s => s.Query.All<Customer>()
        .Where(c => c.Key == key).Lock(lockMode1, lockBehavior1));
      firstEvent.Set();
      firstThread.Join();
      return result;
    }

    private Exception ExecuteQueryAtSeparateThread<T>(Func<Session,IQueryable<T>> query)
    {
      Exception result = null;
      var thread = new Thread(() => {
        try {
          using (var session = Domain.OpenSession())
          using (session.OpenTransaction()) {
            query.Invoke(session).ToList();
          }
        }
        catch(Exception e) {
          result = e;
          return;
        }
      });
      thread.Start();
      thread.Join();
      return result;
    }
  }
}