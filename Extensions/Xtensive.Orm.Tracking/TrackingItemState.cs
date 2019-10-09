// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.05.16

using System;

namespace Xtensive.Orm.Tracking
{
  /// <summary>
  /// State of <see cref="ITrackingItem"/>
  /// </summary>
  [Serializable]
  public enum TrackingItemState
  {
    /// <summary>
    /// Entity was created
    /// </summary>
    Created = 0,

    /// <summary>
    /// Entity was changed
    /// </summary>
    Changed = 1,

    /// <summary>
    /// Entity was removed
    /// </summary>
    Deleted = 2,
  }
}