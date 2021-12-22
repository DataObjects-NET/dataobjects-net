// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.08.25

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  public class LockTest : ChinookDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.ExclusiveWriterConnection);
    }

    [Test]
    public void UpdateLockThrowTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      var catchedException = ExecuteConcurrentQueries(LockMode.Update, LockBehavior.Wait,
        LockMode.Update, LockBehavior.ThrowIfLocked);
//      Assert.AreEqual(typeof(StorageException), catchedException.GetType());
    }

    [Test]
    public void CachingTest()
    {
      var trackIds = new List<int>(32);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (var i = 0; i < trackIds.Capacity; i++) {
          trackIds.Add(new AudioTrack() { Name = "SomeTrack" }.TrackId);
        }

        transaction.Complete();
      }

      var results = new ConcurrentBag<Pair<int>>();
      var source = trackIds.Select(t => new {PId = t, Bag = results});

      _ = Parallel.ForEach(
        source,
        new ParallelOptions { MaxDegreeOfParallelism = 4 },
        (sourceItem, state, currentItteration) => {
          using (var session = Domain.OpenSession())
          using (var transaction = session.OpenTransaction()) {
            var trackToLock = session.Query.All<AudioTrack>().FirstOrDefault(t => t.TrackId==sourceItem.PId);

            EventHandler<DbCommandEventArgs> handler = (sender, args) => {
              sourceItem.Bag.Add(new Pair<int>(sourceItem.PId, (int)args.Command.Parameters[0].Value));
            };
            session.Events.DbCommandExecuting += handler;

            if (trackToLock != null) {
              trackToLock.Lock(LockMode.Update, LockBehavior.Wait);
            }

            session.Events.DbCommandExecuting -= handler;
          }
        });

      try {
        Assert.That(results.Count, Is.EqualTo(trackIds.Capacity));
        Assert.That(results.All(t => t.First == t.Second), Is.True);
      }
      finally {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          session.Remove(session.Query.All<AudioTrack>().Where(t => t.TrackId.In(trackIds)));
          transaction.Complete();
        }
      }
    }


    [Test]
    public void LockNewlyCreatedEntity()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      var track = new AudioTrack() { Name = "SomeTrack" };
      using (Session.DisableSaveChanges()) {
        track.Lock(LockMode.Exclusive, LockBehavior.ThrowIfLocked);
      }
    }

    [Test]
    public void UpdateLockSkipTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var key = Session.Query.All<Customer>().First().Key;
      var expected = Session.Query.All<Customer>().Where(c => c.Key == key)
        .Lock(LockMode.Update, LockBehavior.Wait).ToList();
      Exception catchedException = null;
      var countAfterSkip = 0;
      var secondThread = new Thread(() => {
        try {
          using (var session = Domain.OpenSession())
          using (session.OpenTransaction()) {
            countAfterSkip = session.Query.All<Customer>()
              .Where(c => c.Key == key)
              .Lock(LockMode.Update, LockBehavior.Skip)
              .ToList().Count;
          }
        }
        catch(Exception e) {
          catchedException = e;
        }
      });
      secondThread.Start();
      secondThread.Join();
      if (catchedException != null) {
        throw catchedException;
      }

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
      Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.PostgreSql | StorageProvider.MySql);
      var catchedException = ExecuteConcurrentQueries(LockMode.Shared, LockBehavior.ThrowIfLocked,
        LockMode.Shared, LockBehavior.ThrowIfLocked);
      Assert.IsNull(catchedException);
    }

    [Test]
    public void LockAfterJoinTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.PostgreSql);
      var customerKey = Session.Query.All<Customer>().First().Key;
      var invoiceKey = Session.Query.All<Invoice>().Where(i => i.Customer.Key == customerKey).First().Key;
      Exception firstThreadException = null;
      var firstEvent = new ManualResetEvent(false);
      var secondEvent = new ManualResetEvent(false);
      var firstThread = new Thread(() => {
        try {
          using (var session = Domain.OpenSession())
          using (session.OpenTransaction()) {
            _ = session.Query.All<Customer>().Where(c => c.Key == customerKey)
              .Join(session.Query.All<Invoice>().Where(i => i.Key == invoiceKey), c => c, i => i.Customer, (c, i) => c)
              .Lock(LockMode.Update, LockBehavior.Wait).ToList();
            _ = secondEvent.Set();
            _ = firstEvent.WaitOne();
          }
        }
        catch(Exception e) {
          firstThreadException = e;
          _ = secondEvent.Set();
          return;
        }
      });
      firstThread.Start();
      _ = secondEvent.WaitOne();
      var secondException = ExecuteQueryAtSeparateThread(s => s.Query.All<Customer>()
        .Where(c => c.Key == customerKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      Assert.AreEqual(typeof(StorageException), secondException.GetType());
      var thirdException = ExecuteQueryAtSeparateThread(s => s.Query.All<Invoice>()
        .Where(i => i.Key == invoiceKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      Assert.AreEqual(typeof(StorageException), thirdException.GetType());
      _ = firstEvent.Set();
      firstThread.Join();
      if (firstThreadException != null) {
        throw firstThreadException;
      }
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
      using (session.OpenTransaction()) {
        AssertEx.Throws<StorageException>(() =>
          session.Query.Single<Customer>(customerKey).Lock(LockMode.Update, LockBehavior.ThrowIfLocked));
      }
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
            _ = session.Query.All<Customer>().Where(c => c.Key == key).Lock(lockMode0, lockBehavior0).ToList();
            _ = secondEvent.Set();
            _ = firstEvent.WaitOne();
          }
        }
        catch(Exception e) {
          firstThreadException = e;
          _ = secondEvent.Set();
          return;
        }
      });
      firstThread.Start();
      _ = secondEvent.WaitOne();
      if (firstThreadException != null) {
        throw firstThreadException;
      }

      result = ExecuteQueryAtSeparateThread(s => s.Query.All<Customer>()
        .Where(c => c.Key == key).Lock(lockMode1, lockBehavior1));
      _ = firstEvent.Set();
      firstThread.Join();
      return result;
    }

    private Exception ExecuteQueryAtSeparateThread<T>(Func<Session, IQueryable<T>> query)
    {
      Exception result = null;
      var thread = new Thread(() => {
        try {
          using (var session = Domain.OpenSession())
          using (session.OpenTransaction()) {
            _ = query.Invoke(session).ToList();
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