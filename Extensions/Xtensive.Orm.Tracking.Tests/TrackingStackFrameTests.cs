using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tracking;
using Xtensive.Orm.Tracking.Tests.Model;

namespace Xtensive.Orm.Tracking.Tests
{
  [TestFixture]
  public class TrackingStackFrameTests : TrackingTestBase
  {
    [Test]
    public void SafelyInsertTheSameItemTwiceTest()
    {
      var frame = new TrackingStackFrame();
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      var item = CreateTrackingItem(key, TrackingItemState.Created);
      frame.Register(item);
      frame.Register(item);
    }

    [Test]
    public void MergeTwoEmptyFramesTest()
    {
      var target = new TrackingStackFrame();
      var source = new TrackingStackFrame();
      target.MergeWith(source);

      Assert.AreEqual(0, target.Count);
    }

    [Test]
    public void MergeEmptyFrameWithNonEmptyFrameTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      var target = new TrackingStackFrame();

      var source = new TrackingStackFrame();
      source.Register(CreateTrackingItem(key, TrackingItemState.Created));

      target.MergeWith(source);

      Assert.AreEqual(source.Count, target.Count);
    }

    [Test]
    public void MergeNonEmptyFrameWithEmptyFrameTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      var target = new TrackingStackFrame();
      target.Register(CreateTrackingItem(key, TrackingItemState.Created));
      var count = target.Count;

      var source = new TrackingStackFrame();

      target.MergeWith(source);

      Assert.AreEqual(count, target.Count);
    }

    [Test]
    public void MergeFramesWithTheSameItemsTest()
    {
      var key = Key.Create(Domain, typeof(MyEntity), 1);
      var target = new TrackingStackFrame();
      target.Register(CreateTrackingItem(key, TrackingItemState.Created));
      var count = target.Count;

      var source = new TrackingStackFrame();
      source.Register(CreateTrackingItem(key, TrackingItemState.Changed));

      target.MergeWith(source);

      Assert.AreEqual(count, target.Count);
      Assert.AreEqual(TrackingItemState.Created, target.Single().State);
    }

    [Test]
    public void MergeFramesWithDifferentItemsTest()
    {
      var key1 = Key.Create(Domain, typeof(MyEntity), 1);
      var key2 = Key.Create(Domain, typeof(MyEntity), 2);
      var target = new TrackingStackFrame();
      target.Register(CreateTrackingItem(key1, TrackingItemState.Created));

      var source = new TrackingStackFrame();
      source.Register(CreateTrackingItem(key2, TrackingItemState.Changed));
      var count = target.Count + source.Count;

      target.MergeWith(source);

      Assert.AreEqual(count, target.Count);
    }

    private TrackingItem CreateTrackingItem(Key key, TrackingItemState state) => (TrackingItem) TestHelper.CreateTrackingItem(key, state);
  }
}
