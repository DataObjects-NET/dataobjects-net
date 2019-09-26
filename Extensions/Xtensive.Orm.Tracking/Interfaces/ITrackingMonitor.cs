using System;

namespace Xtensive.Orm.Tracking
{
  /// <summary>
  /// Base tracking monitor interface
  /// </summary>
  public interface ITrackingMonitor
  {
    /// <summary>
    /// Occurs when a single tracking operation is completed.
    /// </summary>
    event EventHandler<TrackingCompletedEventArgs> TrackingCompleted;

    /// <summary>
    /// Enables tracking.
    /// </summary>
    void Enable();

    /// <summary>
    /// Disables tracking.
    /// </summary>
    void Disable();
  }
}