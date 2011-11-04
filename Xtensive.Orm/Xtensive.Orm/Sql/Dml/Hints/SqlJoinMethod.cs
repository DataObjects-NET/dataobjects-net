// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// A list of available join methods for a table.
  /// </summary>
  /// <remarks>
  /// MS SQL Server supports only <see cref="Loop"/>, <see cref="Merge"/>,
  /// <see cref="Hash"/> and <see cref="Remote"/> methods.
  /// Oracle supports all methods except <see cref="Remote"/> method.
  /// </remarks>
  [Serializable]
  public enum SqlJoinMethod
  {
    /// <summary>
    /// Join method is selected by query optimizer.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Instructs the optimizer to join each specified table to another row source with a nested loops join,
    /// using the specified table as the inner table.
    /// </summary>
    Loop = 1,

    /// <summary>
    /// Instructs the optimizer to join the specified table to another row source with a nested loops join
    /// using the specified table as the inner table.
    /// </summary>
    LoopWithIndex = 2,

    /// <summary>
    /// Instructs the optimizer to exclude nested loops joins when joining each specified table to another row source
    /// using the specified table as the inner table.
    /// </summary>
    NoLoop = 3,

    /// <summary>
    /// Instructs the optimizer to join each specified table with another row source using a sort-merge join.
    /// </summary>
    Merge = 4,

    /// <summary>
    /// Instructs the optimizer to exclude sort-merge joins when joining each specified table to another row source
    /// using the specified table as the inner table.
    /// </summary>
    NoMerge = 5,

    /// <summary>
    /// Instructs the optimizer to join each specified table with another row source using a hash join.
    /// </summary>
    Hash = 6,

    /// <summary>
    /// Instructs the optimizer to exclude hash joins when joining each specified table to another row source using
    /// the specified table as the inner table.
    /// </summary>
    NoHash = 7,

    /// <summary>
    /// Specifies that the join operation is performed on the site of the right table. This is useful
    /// when the left table is a local table and the right table is a remote table. This method should be used
    /// only when the left table has fewer rows than the right table.
    /// </summary>
    Remote = 8,
  }
}