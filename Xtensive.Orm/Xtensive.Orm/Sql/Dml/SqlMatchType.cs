// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Enums types of search of matching in MATCH SQL statement.
  /// </summary>
  [Serializable]
  public enum SqlMatchType
  {
    /// <summary>
    /// If there exists a null value of <see cref="SqlMatch.Value"/> then the MATCH
    /// SQL statement is true.
    /// If there exists a row of the <see cref="SqlMatch.SubQuery"/> such that each
    /// value of <see cref="SqlMatch.Value"/> equals its corresponding value in row,
    /// then the MATCH SQL statement is true. Otherwise, the MATCH SQL statement is false.
    /// </summary>
    None = 0,

    /// <summary>
    /// If there exists a row of the <see cref="SqlMatch.SubQuery"/> such that each
    /// non-null value of <see cref="SqlMatch.Value"/> equals its corresponding value in row,
    /// then the MATCH SQL statement is true. Otherwise, the MATCH SQL statement is false.
    /// </summary>
    Partial = 1,

    /// <summary>
    /// If there exists a null value of <see cref="SqlMatch.Value"/> then the MATCH
    /// SQL statement is false.
    /// If there exists a row of the <see cref="SqlMatch.SubQuery"/> such that each
    /// value of <see cref="SqlMatch.Value"/> equals its corresponding value in row,
    /// then the MATCH SQL statement is true. Otherwise, the MATCH SQL statement is false.
    /// </summary>
    Full = 2,
  }
}
