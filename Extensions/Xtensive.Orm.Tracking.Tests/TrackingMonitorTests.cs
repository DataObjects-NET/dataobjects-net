// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.05.17

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tracking.Tests.Model;

namespace Xtensive.Orm.Tracking.Tests
{
  [TestFixture]
  public class TrackingMonitorTests : AutoBuildTest
  {
    private bool listenerIsCalled;

    public override void TestSetUp()
    {
      listenerIsCalled = false;
    }

    private void ListenerIsCalled()
    {
      listenerIsCalled = true;
    }

    [Test]
    public void CreateInOutermostTest()
    {
      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += CreateInOutermostListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var e = new MyEntity(session);
            t.Complete();
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= CreateInOutermostListener;
        Assert.IsTrue(listenerIsCalled);
      }
    }

    private void CreateInOutermostListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.IsNotNull(e.Changes);
      Assert.AreEqual(1, e.Changes.Count());
      var ti = e.Changes.First();
      Assert.AreEqual(TrackingItemState.Created, ti.State);
      Assert.IsNotNull(ti.Key);
      Assert.IsNotNull(ti.RawData);
    }

    [Test]
    public void CreateAndModifyInOutermostTest()
    {
      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += CreateAndModifyInOutermostListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var e = new MyEntity(session);
            session.SaveChanges();
            e.Text = "some text";
            t.Complete();
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= CreateAndModifyInOutermostListener;
        Assert.IsTrue(listenerIsCalled);
      }
    }

    private void CreateAndModifyInOutermostListener(object sender, TrackingCompletedEventArgs e)
    {
      foreach (var change in e.Changes) {
        Console.WriteLine(change.Key);
        Console.WriteLine(change.State);
        foreach (var value in change.ChangedValues) {
          Console.WriteLine(value.Field.Name);
          Console.WriteLine(value.OriginalValue);
          Console.WriteLine(value.NewValue);
        }
      }


      ListenerIsCalled();
      Assert.IsNotNull(e.Changes);
      Assert.AreEqual(1, e.Changes.Count());
      var ti = e.Changes.First();
      Assert.AreEqual(TrackingItemState.Created, ti.State);
      Assert.IsNotNull(ti.Key);
      Assert.IsNotNull(ti.RawData);
      Assert.AreEqual("some text", ti.RawData.GetValue(2));
    }

    [Test]
    public void CreateAndRemoveInOutermostTest()
    {
      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += CreateAndRemoveInOutermostListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var e = new MyEntity(session);
            e.Text = "some text";
            session.SaveChanges();
            e.Remove();
            t.Complete();
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= CreateAndRemoveInOutermostListener;
        Assert.IsTrue(listenerIsCalled);
      }
    }

    private void CreateAndRemoveInOutermostListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.IsNotNull(e.Changes);
      Assert.AreEqual(1, e.Changes.Count());
      var ti = e.Changes.First();
      Assert.AreEqual(TrackingItemState.Deleted, ti.State);
      Assert.IsNotNull(ti.Key);
      Assert.IsNotNull(ti.RawData);
      Assert.AreEqual("some text", ti.RawData.GetValue(2));
    }

    [Test]
    public void CreateAndRollbackInOutermostTest()
    {
      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += CreateAndRollbackInOutermostListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var e = new MyEntity(session);
            //t.Complete();  Emulating transaction rollback
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= CreateAndRollbackInOutermostListener;
      }
    }

    private void CreateAndRollbackInOutermostListener(object sender, TrackingCompletedEventArgs e)
    {
        throw new AssertionException("This must not be called when outermost transaction is rolled back");
    }

    [Test]
    public void CreateInOutermostAndNestedTest()
    {
      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += CreateInOutermostAndNestedListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var e1 = new MyEntity(session);
            e1.Text = "some text";
            using (var t2 = session.OpenTransaction(TransactionOpenMode.New)) {
              var e2 = new MyEntity(session);
              e2.Text = "some text";
              t2.Complete();
            }
            t.Complete();
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= CreateInOutermostAndNestedListener;
        Assert.IsTrue(listenerIsCalled);
      }
    }

    private void CreateInOutermostAndNestedListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.IsNotNull(e.Changes);
      Assert.AreEqual(2, e.Changes.Count());
      var ti = e.Changes.First();
      Assert.AreEqual(TrackingItemState.Created, ti.State);
      Assert.IsNotNull(ti.Key);
      Assert.IsNotNull(ti.RawData);
      Assert.AreEqual("some text", ti.RawData.GetValue(2));
      ti = e.Changes.Skip(1).First();
      Assert.AreEqual(TrackingItemState.Created, ti.State);
      Assert.IsNotNull(ti.Key);
      Assert.IsNotNull(ti.RawData);
      Assert.AreEqual("some text", ti.RawData.GetValue(2));
    }

    [Test]
    public void CreateInOutermostAndModifyInNestedTest()
    {
      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += CreateInOutermostAndModifyInNestedListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var e = new MyEntity(session);
            e.Text = "some text";
            session.SaveChanges();
            using (var t2 = session.OpenTransaction(TransactionOpenMode.New)) {
              e.Text2 = "another text";
              t2.Complete();
            }
            t.Complete();
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= CreateInOutermostAndModifyInNestedListener;
        Assert.IsTrue(listenerIsCalled);
      }
    }

    private void CreateInOutermostAndModifyInNestedListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.IsNotNull(e.Changes);
      Assert.AreEqual(1, e.Changes.Count());
      var ti = e.Changes.First();
      Assert.AreEqual(TrackingItemState.Created, ti.State);
      Assert.IsNotNull(ti.Key);
      Assert.IsNotNull(ti.RawData);
      Assert.AreEqual("some text", ti.RawData.GetValue(2));
      Assert.AreEqual("another text", ti.RawData.GetValue(3));
    }

    [Test]
    public void CreateInOutermostAndRemoveInNestedTest()
    {
      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += CreateInOutermostAndRemoveInNestedListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var e = new MyEntity(session);
            e.Text = "some text";
            session.SaveChanges();
            using (var t2 = session.OpenTransaction(TransactionOpenMode.New)) {
              e.Remove();
              t2.Complete();
            }
            t.Complete();
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= CreateInOutermostAndRemoveInNestedListener;
        Assert.IsTrue(listenerIsCalled);
      }
    }

    private void CreateInOutermostAndRemoveInNestedListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.IsNotNull(e.Changes);
      Assert.AreEqual(1, e.Changes.Count());
      var ti = e.Changes.First();
      Assert.AreEqual(TrackingItemState.Deleted, ti.State);
      Assert.IsNotNull(ti.Key);
      Assert.IsNotNull(ti.RawData);
      Assert.AreEqual("some text", ti.RawData.GetValue(2));
    }

    [Test]
    public void RemoveInOutermostAndCreateInNestedTest()
    {
      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += RemoveInOutermostAndCreateInNestedListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var e = new MyEntity(session);
            int id = e.Id;
            e.Text = "some text";
            e.Remove();
            session.SaveChanges();
            using (var t2 = session.OpenTransaction(TransactionOpenMode.New)) {
              e = new MyEntity(session, id);
              e.Text2 = "another text";
              t2.Complete();
            }
            t.Complete();
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= RemoveInOutermostAndCreateInNestedListener;
        Assert.IsTrue(listenerIsCalled);
      }
    }

    private void RemoveInOutermostAndCreateInNestedListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.IsNotNull(e.Changes);
      Assert.AreEqual(1, e.Changes.Count());
      var ti = e.Changes.First();
      Assert.AreEqual(TrackingItemState.Changed, ti.State);
      Assert.IsNotNull(ti.Key);
      Assert.IsNotNull(ti.RawData);
      Assert.AreEqual(null, ti.RawData.GetValue(2));
      Assert.AreEqual("another text", ti.RawData.GetValue(3));
    }

    [Test]
    public void CreateAndModifyInNextTest()
    {

      Key key;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var e = new MyEntity(session);
          key = e.Key;
          e.Text = "some text";
          t.Complete();
        }
      }

      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += CreateAndModifyInNextListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var e = session.Query.Single<MyEntity>(key);
            e.Text = "another text";
            t.Complete();
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= CreateAndModifyInNextListener;
        Assert.IsTrue(listenerIsCalled);
      }
    }

    [Test]
    public void TrackPartiallyLoadedEntity()
    {
      Key key;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var e = new MyChildEntity(session);
          key = e.Key;
          e.Text = "some text";
          t.Complete();
        }
      }

      var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
      monitor.TrackingCompleted += CreateAndModifyInNextListener;

      try {
        using (var session = Domain.OpenSession()) {
          using (var t = session.OpenTransaction()) {
            var entities = session.Query.All<MyEntity>().Where(e => e.Key==key);
            foreach (var e in entities)
              e.Text = "another text";
            t.Complete();
          }
        }
      }
      finally {
        monitor.TrackingCompleted -= CreateAndModifyInNextListener;
        Assert.IsTrue(listenerIsCalled);
      }
    }

    private void CreateAndModifyInNextListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.IsNotNull(e.Changes);
      Assert.AreEqual(1, e.Changes.Count());
      var ti = e.Changes.First();
      Assert.AreEqual(TrackingItemState.Changed, ti.State);
      Assert.IsNotNull(ti.Key);
      Assert.IsNotNull(ti.RawData);
      Assert.AreEqual(1, ti.ChangedValues.Count());
      var changedValue = ti.ChangedValues.First();
      Assert.AreEqual("some text", changedValue.OriginalValue);
      Assert.AreEqual("another text", changedValue.NewValue);
    }
  }
}