// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.11

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Defines the set of aggregate functions.
  /// </summary>
  public enum AggregateType
  {
    /// <summary>
    /// Average of the values in a column.
    /// </summary>
    Avg = 0,

    /// <summary>
    /// A count of the values in a column.
    /// </summary>
    Count = 1,

    /// <summary>
    /// Highest value in a column.
    /// </summary>
    Max = 2,

    /// <summary>
    /// Lowest value in a column.
    /// </summary>
    Min = 3,

    /// <summary>
    /// Total of values in a column.
    /// </summary>
    Sum = 4
  }
}