// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2012.05.16

using NUnit.Framework;
using Xtensive.Orm.Tracking.Tests.Model;
using Xtensive.Tuples;

namespace Xtensive.Orm.Tracking.Tests
{
  [TestFixture]
  public class TrackingItemTests : TrackingTestBase
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

      Assert.That(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available, Is.False);
      Assert.That(target.RawData.Difference.GetValue<string>(0), Is.EqualTo("value2"));
      Assert.That(target.State, Is.EqualTo(TrackingItemState.Created));
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

      Assert.That(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available, Is.False);
      Assert.That(target.RawData.Difference.GetValue<string>(0), Is.EqualTo("value2"));
      Assert.That(target.State, Is.EqualTo(TrackingItemState.Created));
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

      Assert.That(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available, Is.False);
      Assert.That(target.RawData.Difference.GetValue<string>(0), Is.EqualTo("value2"));
      Assert.That(target.State, Is.EqualTo(TrackingItemState.Deleted));
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

      Assert.That(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available, Is.True);
      Assert.That(target.RawData.Origin.GetValue<string>(0), Is.EqualTo("value1"));
      Assert.That(target.RawData.Difference.GetValue<string>(0), Is.EqualTo("value2"));
      Assert.That(target.State, Is.EqualTo(TrackingItemState.Changed));
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

      Assert.That(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available, Is.True);
      Assert.That(target.RawData.Origin.GetValue<string>(0), Is.EqualTo("value1"));
      Assert.That(target.RawData.Difference.GetValue<string>(0), Is.EqualTo("value2"));
      Assert.That(target.State, Is.EqualTo(TrackingItemState.Deleted));
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

      Assert.That(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available, Is.True);
      Assert.That(target.RawData.Origin.GetValue<string>(0), Is.EqualTo("value1"));
      Assert.That(target.RawData.Difference.GetValue<string>(0), Is.EqualTo("value2"));
      Assert.That(target.State, Is.EqualTo(TrackingItemState.Deleted));
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

      Assert.That(target.RawData.Origin.GetFieldState(0) == TupleFieldState.Available, Is.False);
      Assert.That(target.RawData.Difference.GetValue<string>(0), Is.EqualTo("value2"));
      Assert.That(target.State, Is.EqualTo(TrackingItemState.Changed));
    }
  }
}