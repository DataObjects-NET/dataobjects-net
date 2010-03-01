// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

namespace Xtensive.Modelling
{
  /// <summary>
  /// Enumerates possible states of the node.
  /// </summary>
  public enum NodeState
  {
    /// <summary>
    /// Default node state: <see cref="Initializing"/>.
    /// </summary>
    Default = Initializing,
    /// <summary>
    /// Node isn't fully initialized yet.
    /// </summary>
    Initializing = 0x0,
    /// <summary>
    /// Node is "live".
    /// </summary>
    Live = 0x1,
    /// <summary>
    /// Node is removed.
    /// </summary>
    Removed = 0x2,
  }
}