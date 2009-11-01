// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.01

namespace Xtensive.Integrity.Relations
{
  /// <summary>
  /// Possible states of linked property pair change sequence.
  /// </summary>
  internal enum OneToOneRelationSyncStage
  {
    /// <summary>
    /// In sync.
    /// </summary>
    Default = 0,
    /// <summary>
    /// Master setter is invoked.
    /// </summary>
    MasterSetterInvoked,
    /// <summary>
    /// Slave setter is invoked.
    /// </summary>
    SlaveSetterInvoked,
    /// <summary>
    /// Old slave setter is invoked.
    /// </summary>
    OldSlaveSetterInvoked,
  }
}