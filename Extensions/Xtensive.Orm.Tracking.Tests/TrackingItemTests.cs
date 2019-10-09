// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.05.16

using NUnit.Framework;
using Xtensive.Orm.Tracking.Tests.Model;
using Xtensive.Tuples;

namespace Xtensive.Orm.Tracking.Tests
{
  [TestFixture]
  public class TrackingItemTests : AutoBuildTest
  {
    [Test]
    public void MergeNewAndNewTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      
      var target = TestHelper.CreateTrackingItem(key, TrackingItemState.Created);
      target.RawData.SetValue(0, "value1");

      var source = TestHelper.CreateTrackingItem(key, TrackingItemState.Created);
      source.RawData.SetValue(0, "value2");

      TestHelper.Merge(target, source);

      Assert.IsFalse(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available);
      Assert.AreEqual("value2", target.RawData.Difference.GetValue<string>(0));
      Assert.AreEqual(TrackingItemState.Created, target.State);
    }

    [Test]
    public void MergeNewAndModifiedTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      
      var target = TestHelper.CreateTrackingItem(key, TrackingItemState.Created);
      target.RawData.SetValue(0, "value1");

      var source = TestHelper.CreateTrackingItem(key, TrackingItemState.Changed);
      source.RawData.SetValue(0, "value2");

      TestHelper.Merge(target, source);

      Assert.IsFalse(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available);
      Assert.AreEqual("value2", target.RawData.Difference.GetValue<string>(0));
      Assert.AreEqual(TrackingItemState.Created, target.State);
    }

    [Test]
    public void MergeNewAndRemovedTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      
      var target = TestHelper.CreateTrackingItem(key, TrackingItemState.Created);
      target.RawData.SetValue(0, "value1");

      var source = TestHelper.CreateTrackingItem(key, TrackingItemState.Deleted);
      source.RawData.SetValue(0, "value2");

      TestHelper.Merge(target, source);

      Assert.IsFalse(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available);
      Assert.AreEqual("value2", target.RawData.Difference.GetValue<string>(0));
      Assert.AreEqual(TrackingItemState.Deleted, target.State);
    }

    [Test]
    public void MergeModifiedAndModifiedTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      
      var target = TestHelper.CreateTrackingItem(key, TrackingItemState.Changed);
      target.RawData.Origin.SetValue(0, "value1");

      var source = TestHelper.CreateTrackingItem(key, TrackingItemState.Changed);
      source.RawData.SetValue(0, "value2");

      TestHelper.Merge(target, source);

      Assert.IsTrue(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available);
      Assert.AreEqual("value1", target.RawData.Origin.GetValue<string>(0));
      Assert.AreEqual("value2", target.RawData.Difference.GetValue<string>(0));
      Assert.AreEqual(TrackingItemState.Changed, target.State);
    }

    [Test]
    public void MergeModifiedAndRemovedTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      
      var target = TestHelper.CreateTrackingItem(key, TrackingItemState.Changed);
      target.RawData.Origin.SetValue(0, "value1");

      var source = TestHelper.CreateTrackingItem(key, TrackingItemState.Deleted);
      source.RawData.SetValue(0, "value2");

      TestHelper.Merge(target, source);

      Assert.IsTrue(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available);
      Assert.AreEqual("value1", target.RawData.Origin.GetValue<string>(0));
      Assert.AreEqual("value2", target.RawData.Difference.GetValue<string>(0));
      Assert.AreEqual(TrackingItemState.Deleted, target.State);
    }

    [Test]
    public void MergeRemovedAndRemovedTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      
      var target = TestHelper.CreateTrackingItem(key, TrackingItemState.Deleted);
      target.RawData.Origin.SetValue(0, "value1");

      var source = TestHelper.CreateTrackingItem(key, TrackingItemState.Deleted);
      source.RawData.SetValue(0, "value2");

      TestHelper.Merge(target, source);

      Assert.IsTrue(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available);
      Assert.AreEqual("value1", target.RawData.Origin.GetValue<string>(0));
      Assert.AreEqual("value2", target.RawData.Difference.GetValue<string>(0));
      Assert.AreEqual(TrackingItemState.Deleted, target.State);
    }

    [Test]
    public void MergeRemovedAndNewTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      
      var target = TestHelper.CreateTrackingItem(key, TrackingItemState.Deleted);
      target.RawData.Origin.SetValue(0, "value1");

      var source = TestHelper.CreateTrackingItem(key, TrackingItemState.Created);
      source.RawData.SetValue(0, "value2");

      TestHelper.Merge(target, source);

      Assert.IsFalse(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available);
      Assert.AreEqual("value2", target.RawData.Difference.GetValue<string>(0));
      Assert.AreEqual(TrackingItemState.Changed, target.State);
    }
  }
}