using System;
using System.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tracking.Tests
{
  public static class TestHelper
  {
    private static readonly Type TrackingItemType;
    private static readonly MethodInfo MergeWithMethod;

    public static void Merge(ITrackingItem target, ITrackingItem source)
    {
      MergeWithMethod.Invoke(target, new object[] {source});
    }

    public static ITrackingItem CreateTrackingItem(Key key, TrackingItemState state)
    {
      var tuple = Tuple.Create(typeof (string));
      var diff = new DifferentialTuple(tuple);
      return (ITrackingItem) Activator.CreateInstance(TrackingItemType, key, state, diff);
    }

    static TestHelper()
    {
      TrackingItemType = typeof (ITrackingItem).Assembly.GetType("Xtensive.Orm.Tracking.TrackingItem");
      MergeWithMethod = TrackingItemType.GetMethod("MergeWith");
    }
  }
}
