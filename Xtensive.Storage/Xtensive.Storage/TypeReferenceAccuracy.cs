// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.09

namespace Xtensive.Storage
{
  /// <summary>
  /// Describes type reference accuracy.
  /// </summary>
  public enum TypeReferenceAccuracy
  {
    /// <summary>
    /// Referenced type could be any node from the specified type hierarchy.
    /// </summary>
    Hierarchy = 0,

    /// <summary>
    /// Referenced type could be any node from the specified hierarchy subtree (type and its ancestors).
    /// </summary>
    BaseType = 1,

    /// <summary>
    /// Referenced type is known.
    /// </summary>
    ExactType = 2,
  }
}