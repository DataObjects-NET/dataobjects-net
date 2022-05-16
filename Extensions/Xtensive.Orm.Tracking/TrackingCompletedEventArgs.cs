// Copyright (C) 2019-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Tracking
{
  /// <summary>
  /// Event arguments for <see cref="ITrackingMonitor.TrackingCompleted"/> event.
  /// </summary>
  public readonly struct TrackingCompletedEventArgs
  {
    /// <summary>
    /// Gets session this changes occured in.
    /// </summary>
    public Session Session { get; }

    /// <summary>
    /// Gets the changes.
    /// </summary>
    public IEnumerable<ITrackingItem> Changes { get; }

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
