using System.Collections.Generic;
using Xtensive.Tuples;

namespace Xtensive.Orm.Tracking
{
  public interface ITrackingItem
  {
    Key Key { get; }

    DifferentialTuple RawData { get; }

    TrackingItemState State { get; }

    IList<ChangedValue> ChangedValues { get; }
  }
}