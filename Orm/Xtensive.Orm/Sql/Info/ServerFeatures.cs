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
    /// Empty option set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that RDBMS uses large objects (LOBs) for manipulating large data chunks.
    /// <seealso cref="ICharacterLargeObject"/>.
    /// <seealso cref="IBinaryLargeObject"/>.
    /// </summary>
    LargeObjects = 1 << 0,

    /// <summary>
    /// Indicates that RDBMS supports cursor parameters.
    /// </summary>
    CursorParameters = 1 << 1,

    /// <summary>
    /// Indicates whether RDBMS supports returning multiple results via cursor parameters.
    /// Does matter only if <see cref="CursorParameters"/> feature is avaliable.
    /// </summary>
    MultipleResultsViaCursorParameters = 1 << 2,

    /// <summary>
    /// Indicates whether RDBMS supports savepoints.
    /// </summary>
    Savepoints = 1 << 3,

    /// <summary>
    /// Indicates whether RDBMS supports transactional DDL
    /// (except full-text indexes which have separate option).
    /// </summary>
    TransactionalDdl = 1 << 4,

    /// <summary>
    /// Indicates whether RDBMS supports transactional DDL for full-text indexes.
    /// </summary>
    TransactionalFullTextDdl = 1 << 5,

    /// <summary>
    /// Indicates whether RDBMS uses key generators that adhere to transaction boundaries.
    /// </summary>
    TransactionalKeyGenerators = 1 << 6,

    /// <summary>
    /// Indicates whether RDBMS allows only one session to modify database.
    /// </summary>
    SingleSessionAccess = 1 << 7,
  }
}