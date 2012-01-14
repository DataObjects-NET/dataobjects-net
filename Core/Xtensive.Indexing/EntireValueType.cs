// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

namespace Xtensive.Indexing
{
  /// <summary>
  /// Enumerates possible types of entire value type.
  /// </summary>
  public enum EntireValueType: sbyte 
  {
    /// <summary>
    /// Default type. Is equal to <see cref="Exact"/>.
    /// </summary>
    Default = Exact,

    /// <summary>
    /// The exact value. No infinity.
    /// </summary>
    Exact = 0,

    /// <summary>
    /// Exact value plus infinitesimal.
    /// </summary>
    PositiveInfinitesimal = 1,

    /// <summary>
    /// Exact value minus infinitesimal.
    /// </summary>
    NegativeInfinitesimal = -1,

    /// <summary>
    /// Positive infinity.
    /// </summary>
    PositiveInfinity = 2,

    /// <summary>
    /// Negative infinity.
    /// </summary>
    NegativeInfinity = -2,
  }
}