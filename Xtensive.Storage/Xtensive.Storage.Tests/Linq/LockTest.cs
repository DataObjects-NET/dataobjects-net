// Copyright (C) 2009 Xtensive LLC.
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
using Xtensive.Core.Testing;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class LockTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      EnsureProviderIs(StorageProvider.Sql);
    }

    [Test]
    public void UpdateLockThrowTest()
    {
      var catchedException = ExecuteConcurrentQueries(LockMode.Update, LockBehavior.Wait,
        LockMode.Update, LockBehavior.ThrowIfLocked);
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }

    [Test]
    public void UpdateLockSkipTest()
    {
      EnsureProviderIs(StorageProvider.SqlServer | StorageProvider.SqlServerCe);
      var key = Query.All<Customer>().First().Key;
      var expected = Query.All<Customer>().Where(c => c.Key == key)
        .Lock(LockMode.Update, LockBehavior.Wait).ToList();
      Exception catchedException = null;
      int countAfterSkip = 0;
      var secondThread = new Thread(() => {
        try {
          using (Session.Open(Domain))
          using (Transaction.Open())
            countAfterSkip = Query.All<Customer>().Where(c => c.Key == key)
              .Lock(LockMode.Update, LockBehavior.Skip).ToList().Count;
        }
        catch(Exception e) {
          catchedException = e;
          return;
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
      EnsureProviderIs(StorageProvider.PostgreSql);
      var catchedException = ExecuteConcurrentQueries(LockMode.Update, LockBehavior.Wait,
        LockMode.Shared, LockBehavior.ThrowIfLocked);
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }
    
    [Test]
    public void ExclusiveLockThrowTest()
    {
      EnsureProviderIs(StorageProvider.SqlServer | StorageProvider.SqlServerCe);
      var catchedException = ExecuteConcurrentQueries(LockMode.Update, LockBehavior.Wait,
        LockMode.Exclusive, LockBehavior.ThrowIfLocked);
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }

    [Test]
    public void ShareLockTest()
    {
      EnsureProviderIs(StorageProvider.SqlServer | StorageProvider.PostgreSql | StorageProvider.SqlServerCe);
      var catchedException = ExecuteConcurrentQueries(LockMode.Shared, LockBehavior.ThrowIfLocked,
        LockMode.Shared, LockBehavior.ThrowIfLocked);
      Assert.IsNull(catchedException);
    }
    
    [Test]
    public void LockAfterJoinTest()
    {
      var customerKey = Query.All<Customer>().First().Key;
      var orderKey = Query.All<Order>().Where(o => o.Customer.Key==customerKey).First().Key;
      Exception firstThreadException = null;
      Exception result = null;
      var firstEvent = new ManualResetEvent(false);
      var secondEvent = new ManualResetEvent(false);
      var firstThread = new Thread(() => {
        try {
          using (Session.Open(Domain))
          using (Transaction.Open(IsolationLevel.ReadCommitted)) {
            Query.All<Customer>().Where(c => c.Key == customerKey)
              .Join(Query.All<Order>().Where(o => o.Key == orderKey), c => c, o => o.Customer, (c, o) => c)
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
      var secondException = ExecuteQueryAtSeparateThread(() => Query.All<Customer>()
        .Where(c => c.Key==customerKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      Assert.AreEqual(typeof(StorageException), secondException.GetType());
      var thirdException = ExecuteQueryAtSeparateThread(() => Query.All<Order>()
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
      var customer = Query.All<Customer>().First();
      var customerKey = customer.Key;
      customer.Lock(LockMode.Update, LockBehavior.Wait);
      var catchedException = ExecuteQueryAtSeparateThread(() =>
        Query.All<Customer>().Where(c => c == customer).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
      using (Session.Open(Domain))
      using (Transaction.Open())
        AssertEx.Throws<StorageException>(() =>
          Query.Single<Customer>(customerKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
    }
    
    private Exception ExecuteConcurrentQueries(LockMode lockMode0, LockBehavior lockBehavior0,
      LockMode lockMode1, LockBehavior lockBehavior1)
    {
      var key = Query.All<Customer>().First().Key;
      Exception firstThreadException = null;
      Exception result = null;
      var firstEvent = new ManualResetEvent(false);
      var secondEvent = new ManualResetEvent(false);
      var firstThread = new Thread(() => {
        try {
          using (Session.Open(Domain))
          using (Transaction.Open(IsolationLevel.ReadCommitted)) {
            Query.All<Customer>().Where(c => c.Key == key).Lock(lockMode0, lockBehavior0).ToList();
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
      result = ExecuteQueryAtSeparateThread(() => Query.All<Customer>()
        .Where(c => c.Key == key).Lock(lockMode1, lockBehavior1));
      firstEvent.Set();
      firstThread.Join();
      return result;
    }

    private Exception ExecuteQueryAtSeparateThread<T>(Func<IQueryable<T>> query)
    {
      Exception result = null;
      var thread = new Thread(() => {
        try {
          using (Session.Open(Domain))
          using (Transaction.Open(IsolationLevel.ReadCommitted)) {
            query.Invoke().ToList();
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