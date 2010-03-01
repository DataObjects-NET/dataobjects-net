// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.15

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Join operation type.
  /// </summary>
  public enum JoinType
  {
    /// <summary>
    /// Default join operation type.
    /// </summary>
    Default = Inner,

    /// <summary>
    /// Inner join.
    /// </summary>
    Inner = 0,

    /// <summary>
    /// Left outer join.
    /// </summary>
    LeftOuter
  }
}