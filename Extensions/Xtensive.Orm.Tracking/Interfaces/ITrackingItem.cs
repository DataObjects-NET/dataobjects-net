using System.Collections.Generic;
using Xtensive.Tuples;

namespace Xtensive.Orm.Tracking
{
  /// <summary>
  /// Represents changes happened to an <see cref="Entity"/>.
  /// </summary>
  public interface ITrackingItem
  {
    /// <summary>
    /// Gets <see cref="Entity"/> key.
    /// </summary>
    Key Key { get; }

    /// <summary>
    /// Gets raw <see cref="Entity"/> data as <see cref="DifferentialTuple"/>.
    /// </summary>
    DifferentialTuple RawData { get; }

    /// <summary>
    /// Gets a state of a tracked <see cref="Entity"/>.
    /// It might be <see cref="TrackingItemState.Created"/> or <see cref="TrackingItemState.Changed"/>
    /// or <see cref="TrackingItemState.Deleted"/>.
    /// </summary>
    TrackingItemState State { get; }

    /// <summary>
    /// Gets list of detected changes of field values.
    /// </summary>
    IList<ChangedValue> ChangedValues { get; }
  }
}