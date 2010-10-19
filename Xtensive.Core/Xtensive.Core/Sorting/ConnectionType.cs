// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.10

using System;

namespace Xtensive.Sorting
{
  /// <summary>
  /// Type of <see cref="NodeConnection{TNodeItem,TConnectionItem}"/> connection.
  /// </summary>
  [Serializable]
  public enum ConnectionType
  {
    /// <summary>
    /// Connection may be breaked by topological sorter.
    /// </summary>
    Breakable,

    /// <summary>
    /// Connection cannot be breaked by topological sorter.
    /// </summary>
    Permanent,
  }
}