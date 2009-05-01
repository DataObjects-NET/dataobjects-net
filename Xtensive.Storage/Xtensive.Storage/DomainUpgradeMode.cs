// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.06

namespace Xtensive.Storage
{
  /// <summary>
  /// Enumerates possible <see cref="Domain"/> upgrade modes.
  /// <seealso cref="Domain.Build"/>
  /// <seealso cref="Domain"/>
  /// </summary>
  public enum DomainUpgradeMode
  {
    /// <summary>
    /// Default upgrade mode.
    /// The same as <see cref="PerformSafely"/>
    /// </summary>
    Default = PerformSafely,
    
    /// <summary>
    /// Restricts any modifications to storage.
    /// <see cref="DomainBuilderException"/> will be 
    /// thrown if storage schema differs from the expected one.
    /// </summary>
    Validate = 0x00,

    /// <summary>
    /// Recreates all storage structures. Storage will
    /// contain no instances after this type of update.
    /// </summary>
    Recreate = 0x01,

    /// <summary>
    /// Storage upgrade will be performed. 
    /// Missing columns and tables will be added, excess columns and tables will be removed.
    /// </summary>
    Perform = 0x02,
    
    /// <summary>
    /// Storage upgrade will be performed. 
    /// Missing columns and tables will be added, 
    /// excess columns and tables will be removed only if there are corresponding hints.
    /// </summary>
    PerformSafely = 0x03,
  }
}