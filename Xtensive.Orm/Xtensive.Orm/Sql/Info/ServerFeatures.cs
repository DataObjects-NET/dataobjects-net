// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.05

using System;
using System.Diagnostics;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Features of RBMS.
  /// </summary>
  [Flags]
  public enum ServerFeatures
  {
    /// <summary>
    /// 
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS uses large objects (LOBs) for manipulating large data chunks.
    /// <seealso cref="ICharacterLargeObject"/>.
    /// <seealso cref="IBinaryLargeObject"/>.
    /// </summary>
    LargeObjects = 0x1,

    /// <summary>
    /// Indicates that RDBMS supports cursor parameters.
    /// </summary>
    CursorParameters = 0x2,

    /// <summary>
    /// Indicates whether RDBMS supports returning multiple results via cursor parameters.
    /// Does matter only if <see cref="CursorParameters"/> feature is avaliable.
    /// </summary>
    MultipleResultsViaCursorParameters = 0x4,

    /// <summary>
    /// Indicates whether RDBMS supports savepoints.
    /// </summary>
    Savepoints = 0x8,
  }
}