// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.29

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Action that is taken to truncate a graph. This action is applied
  /// when a limit of an object graph depth has been exceeded.
  /// </summary>
  public enum GraphTruncationType
  {
    /// <summary>
    /// Default action.
    /// </summary>
    Default = Throw,

    /// <summary>
    /// Throw the exception.
    /// </summary>
    Throw = 0,

    /// <summary>
    /// Set <see langword="null" /> to a property whose value exceeds a graph depth limit.
    /// </summary>
    SetNull = 1
  }
}