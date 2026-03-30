// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2012.05.17

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tracking.Tests.Model;

namespace Xtensive.Orm.Tracking.Tests
{
  [TestFixture]
  public class TrackingMonitorTests : TrackingTestBase
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
        Assert.That(listenerIsCalled, Is.True);
      }
    }

    private void CreateInOutermostListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.That(e.Changes, Is.Not.Null);
      Assert.That(e.Changes.Count(), Is.EqualTo(1));
      var ti = e.Changes.First();
      Assert.That(ti.State, Is.EqualTo(TrackingItemState.Created));
      Assert.That(ti.Key, Is.Not.Null);
      Assert.That(ti.RawData, Is.Not.Null);
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
        Assert.That(listenerIsCalled, Is.True);
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
      Assert.That(e.Changes, Is.Not.Null);
      Assert.That(e.Changes.Count(), Is.EqualTo(1));
      var ti = e.Changes.First();
      Assert.That(ti.State, Is.EqualTo(TrackingItemState.Created));
      Assert.That(ti.Key, Is.Not.Null);
      Assert.That(ti.RawData, Is.Not.Null);
      Assert.That(ti.RawData.GetValue(2), Is.EqualTo("some text"));
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
        Assert.That(listenerIsCalled, Is.True);
      }
    }

    private void CreateAndRemoveInOutermostListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.That(e.Changes, Is.Not.Null);
      Assert.That(e.Changes.Count(), Is.EqualTo(1));
      var ti = e.Changes.First();
      Assert.That(ti.State, Is.EqualTo(TrackingItemState.Deleted));
      Assert.That(ti.Key, Is.Not.Null);
      Assert.That(ti.RawData, Is.Not.Null);
      Assert.That(ti.RawData.GetValue(2), Is.EqualTo("some text"));
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
        Assert.That(listenerIsCalled, Is.True);
      }
    }

    private void CreateInOutermostAndNestedListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.That(e.Changes, Is.Not.Null);
      Assert.That(e.Changes.Count(), Is.EqualTo(2));
      var ti = e.Changes.First();
      Assert.That(ti.State, Is.EqualTo(TrackingItemState.Created));
      Assert.That(ti.Key, Is.Not.Null);
      Assert.That(ti.RawData, Is.Not.Null);
      Assert.That(ti.RawData.GetValue(2), Is.EqualTo("some text"));
      ti = e.Changes.Skip(1).First();
      Assert.That(ti.State, Is.EqualTo(TrackingItemState.Created));
      Assert.That(ti.Key, Is.Not.Null);
      Assert.That(ti.RawData, Is.Not.Null);
      Assert.That(ti.RawData.GetValue(2), Is.EqualTo("some text"));
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
        Assert.That(listenerIsCalled, Is.True);
      }
    }

    private void CreateInOutermostAndModifyInNestedListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.That(e.Changes, Is.Not.Null);
      Assert.That(e.Changes.Count(), Is.EqualTo(1));
      var ti = e.Changes.First();
      Assert.That(ti.State, Is.EqualTo(TrackingItemState.Created));
      Assert.That(ti.Key, Is.Not.Null);
      Assert.That(ti.RawData, Is.Not.Null);
      Assert.That(ti.RawData.GetValue(2), Is.EqualTo("some text"));
      Assert.That(ti.RawData.GetValue(3), Is.EqualTo("another text"));
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
        Assert.That(listenerIsCalled, Is.True);
      }
    }

    private void CreateInOutermostAndRemoveInNestedListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.That(e.Changes, Is.Not.Null);
      Assert.That(e.Changes.Count(), Is.EqualTo(1));
      var ti = e.Changes.First();
      Assert.That(ti.State, Is.EqualTo(TrackingItemState.Deleted));
      Assert.That(ti.Key, Is.Not.Null);
      Assert.That(ti.RawData, Is.Not.Null);
      Assert.That(ti.RawData.GetValue(2), Is.EqualTo("some text"));
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
        Assert.That(listenerIsCalled, Is.True);
      }
    }

    private void RemoveInOutermostAndCreateInNestedListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.That(e.Changes, Is.Not.Null);
      Assert.That(e.Changes.Count(), Is.EqualTo(1));
      var ti = e.Changes.First();
      Assert.That(ti.State, Is.EqualTo(TrackingItemState.Changed));
      Assert.That(ti.Key, Is.Not.Null);
      Assert.That(ti.RawData, Is.Not.Null);
      Assert.That(ti.RawData.GetValue(2), Is.EqualTo(null));
      Assert.That(ti.RawData.GetValue(3), Is.EqualTo("another text"));
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
        Assert.That(listenerIsCalled, Is.True);
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
        Assert.That(listenerIsCalled, Is.True);
      }
    }

    private void CreateAndModifyInNextListener(object sender, TrackingCompletedEventArgs e)
    {
      ListenerIsCalled();
      Assert.That(e.Changes, Is.Not.Null);
      Assert.That(e.Changes.Count(), Is.EqualTo(1));
      var ti = e.Changes.First();
      Assert.That(ti.State, Is.EqualTo(TrackingItemState.Changed));
      Assert.That(ti.Key, Is.Not.Null);
      Assert.That(ti.RawData, Is.Not.Null);
      Assert.That(ti.ChangedValues.Count(), Is.EqualTo(1));
      var changedValue = ti.ChangedValues.First();
      Assert.That(changedValue.OriginalValue, Is.EqualTo("some text"));
      Assert.That(changedValue.NewValue, Is.EqualTo("another text"));
    }
  }
}