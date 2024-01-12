using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Tracking
{
  /// <summary>
  /// Event arguments for <see cref="ITrackingMonitor.TrackingCompleted"/> event.
  /// </summary>
  public sealed class TrackingCompletedEventArgs : EventArgs
  {
    /// <summary>
    /// Gets session this changes occured in.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets the changes.
    /// </summary>
    public IEnumerable<ITrackingItem> Changes { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackingCompletedEventArgs"/> class.
    /// </summary>
    /// <param name="session"><see cref="T:Session"/> instance where the <see cref="Changes"/>
    /// where collected.</param>
    /// <param name="changes">The changes.</param>
    public TrackingCompletedEventArgs(Session session, IEnumerable<ITrackingItem> changes)
    {
      Session = session ?? throw new ArgumentNullException("session");
      Changes = changes ?? throw new ArgumentNullException("changes");
    }
  }
}
