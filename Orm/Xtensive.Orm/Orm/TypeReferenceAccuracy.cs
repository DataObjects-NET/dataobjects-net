// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.09

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes type reference accuracy.
  /// </summary>
  public enum TypeReferenceAccuracy
  {
    /// <summary>
    /// Referenced type is limited to the entire hierarchy.
    /// </summary>
    Hierarchy = 0,

    /// <summary>
    /// Referenced type is limited to the hierarchy subtree (specified type and its ancestors).
    /// </summary>
    BaseType = 1,

    /// <summary>
    /// Referenced type is exactly known.
    /// </summary>
    ExactType = 2,
  }
}