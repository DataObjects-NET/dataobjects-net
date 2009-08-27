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
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using Xtensive.Storage.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class LockTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    [Test]
    public void UpdateLockThrowTest()
    {
      var catchedException = ExecuteConcurrentQeuries(LockMode.Update, LockBehavior.Wait,
        LockMode.Update, LockBehavior.ThrowIfLocked);
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }

    [Test]
    public void UpdateLockSkipTest()
    {
      if (Protocol != StorageProtocol.SqlServer)
        Assert.Ignore();
      var key = Query<Customer>.All.First().Key;
      var expected = Query<Customer>.All.Where(c => c.Key == key)
        .Lock(LockMode.Update, LockBehavior.Wait).ToList();
      Exception catchedException = null;
      int countAfterSkip = 0;
      var secondThread = new Thread(() => {
        try {
          using (Session.Open(Domain))
          using (Transaction.Open())
            countAfterSkip = Query<Customer>.All.Where(c => c.Key == key)
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
      if (Protocol == StorageProtocol.SqlServer)
        Assert.Ignore();
      var catchedException = ExecuteConcurrentQeuries(LockMode.Update, LockBehavior.Wait,
        LockMode.Shared, LockBehavior.ThrowIfLocked);
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }
    
    [Test]
    public void ExclusiveLockThrowTest()
    {
      if (Protocol != StorageProtocol.SqlServer)
        Assert.Ignore();
      var catchedException = ExecuteConcurrentQeuries(LockMode.Update, LockBehavior.Wait,
        LockMode.Exclusive, LockBehavior.ThrowIfLocked);
      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }

    [Test]
    public void ShareLockTest()
    {
      var catchedException = ExecuteConcurrentQeuries(LockMode.Shared, LockBehavior.ThrowIfLocked,
        LockMode.Shared, LockBehavior.ThrowIfLocked);
      Assert.IsNull(catchedException);
    }
    
    [Test]
    public void LockAfterJoinTest()
    {
      var customerKey = Query<Customer>.All.First().Key;
      var orderKey = Query<Order>.All.Where(o => o.Customer.Key==customerKey).First().Key;
      Exception firstThreadException = null;
      Exception result = null;
      var firstEvent = new ManualResetEvent(false);
      var secondEvent = new ManualResetEvent(false);
      var firstThread = new Thread(() => {
        try {
          using (Session.Open(Domain))
          using (Transaction.Open(IsolationLevel.ReadCommitted)) {
            Query<Customer>.All.Where(c => c.Key == customerKey)
              .Join(Query<Order>.All.Where(o => o.Key == orderKey), c => c, o => o.Customer, (c, o) => c)
              .Lock(LockMode.Update, LockBehavior.Wait).ToList();
            secondEvent.Set();
            firstEvent.WaitOne();
          }
        }
        catch(Exception e) {
          firstThreadException = e;
          return;
        }
      });
      firstThread.Start();
      secondEvent.WaitOne();
      var secondException = ExecuteQueryAtSeparateThread(() => Query<Customer>.All
        .Where(c => c.Key==customerKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      Assert.AreEqual(typeof(StorageException), secondException.GetType());
      var thirdException = ExecuteQueryAtSeparateThread(() => Query<Order>.All
        .Where(o => o.Key==orderKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      Assert.AreEqual(typeof(StorageException), thirdException.GetType());
      firstEvent.Set();
      firstThread.Join();
      if (firstThreadException != null)
        throw firstThreadException;
    }
    
    private Exception ExecuteConcurrentQeuries(LockMode lockMode0, LockBehavior lockBehavior0,
      LockMode lockMode1, LockBehavior lockBehavior1)
    {
      var key = Query<Customer>.All.First().Key;
      Exception firstThreadException = null;
      Exception result = null;
      var firstEvent = new ManualResetEvent(false);
      var secondEvent = new ManualResetEvent(false);
      var firstThread = new Thread(() => {
        try {
          using (Session.Open(Domain))
          using (Transaction.Open(IsolationLevel.ReadCommitted)) {
            Query<Customer>.All.Where(c => c.Key == key).Lock(lockMode0, lockBehavior0).ToList();
            secondEvent.Set();
            firstEvent.WaitOne();
          }
        }
        catch(Exception e) {
          firstThreadException = e;
          return;
        }
      });
      firstThread.Start();
      secondEvent.WaitOne();
      result = ExecuteQueryAtSeparateThread(() => Query<Customer>.All
        .Where(c => c.Key == key).Lock(lockMode1, lockBehavior1));
      firstEvent.Set();
      firstThread.Join();
      if (firstThreadException != null)
        throw firstThreadException;
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