// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.06

using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Enumerates possible storage build modes.
  /// <seealso cref="Domain.Build"/>
  /// <seealso cref="Domain"/>
  /// </summary>
  public enum DomainBuildMode
  {
    /// <summary>
    /// Default upgrade mode.
    /// The same as <see cref="Perform"/>
    /// </summary>
    Default = Perform,
    
    /// <summary>
    /// Restricts any modifications to storage.
    /// <see cref="DomainBuilderException"/> will be 
    /// thrown if storage different to model.
    /// </summary>
    BlockUpgrade = 0x00,

    /// <summary>
    /// Recreates all storage structures. Storage will
    /// contain no instances after this type of update.
    /// </summary>
    Recreate = 0x01,

    /// <summary>
    /// Storage upgrade will be performed. Missing columns and tables will be added, 
    /// excess columns and tables will be removed.
    /// </summary>
    Perform = 0x02,
    
    /// <summary>
    /// Storage upgrade with special upgrade routine is needed if storage different to model.
    /// Storage will be upgraded to intermediate state with reciled types. <see cref="IUpgrader"/> instance
    /// must bring new storage data to correct state using new model and reciling types.
    /// </summary>
    PerformStrict = 0x03,
  }
}