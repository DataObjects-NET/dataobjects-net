// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

namespace Xtensive.Indexing
{
  /// <summary>
  /// Enumerates possible types of infinity.
  /// </summary>
  public enum InfinityType: sbyte 
  {
    /// <summary>
    /// No infinity.
    /// </summary>
    None = 0,

    /// <summary>
    /// Positive infinity.
    /// </summary>
    Positive = 1,

    /// <summary>
    /// Negative infinity.
    /// </summary>
    Negative = -1,
  }
}