// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.06

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Enumerates possible <see cref="Domain"/> upgrade modes.
  /// <seealso cref="Domain.Build"/>
  /// <seealso cref="Domain"/>
  /// </summary>
  public enum DomainUpgradeMode
  {
    /// <summary>
    /// Minimal validation is performed.
    /// Assembly versions are checked.
    /// Type identifiers are extracted.
    /// </summary>
    Skip = 0,

    /// <summary>
    /// Validation only mode.
    /// <see cref="DomainBuilderException"/> will be 
    /// thrown if storage schema differs from the expected one.
    /// </summary>
    Validate = 1,

    /// <summary>
    /// Recreates all the necessary structures. 
    /// Storage will contain no instances after this type of update.
    /// </summary>
    Recreate = 2,

    /// <summary>
    /// Storage upgrade will be performed. 
    /// Missing columns and tables will be added, 
    /// unmapped columns and tables will be removed.
    /// </summary>
    Perform = 3,
    
    /// <summary>
    /// Storage upgrade will be performed. 
    /// Missing columns and tables will be added, 
    /// unmapped columns and tables will be removed 
    /// only if there are corresponding hints.
    /// </summary>
    PerformSafely = 4,

    /// <summary>
    /// Legacy database support mode.
    /// No validation is performed.
    /// Use at your own risk.
    /// </summary>
    LegacySkip = 5,

    /// <summary>
    /// Legacy database support mode. 
    /// Similar to <see cref="Validate"/>, but schema comparison
    /// is limited to comparison of tables and columns, everything
    /// else is ignored.
    /// <see cref="DomainBuilderException"/> will be 
    /// thrown if storage schema significantly differs 
    /// from the expected one.
    /// </summary>
    LegacyValidate = 6,

    /// <summary>
    /// Default upgrade mode.
    /// The same as <see cref="PerformSafely"/>
    /// </summary>
    Default = PerformSafely,

    /// <summary>
    /// The same as <see cref="LegacyValidate"/> for backward compatibility.
    /// </summary>
    [Obsolete("Use DomainUpgradeMode.LegacyValidate")]
    Legacy = LegacyValidate,
  }
}