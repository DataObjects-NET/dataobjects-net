// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.01

namespace Xtensive.Orm.Building
{
  /// <summary>
  /// Schema comparison status.
  /// </summary>
  public enum SchemaComparisonStatus
  {
    /// <summary>
    /// Target schema is equal to the extracted schema.
    /// </summary>
    Equal = 0,
    /// <summary>
    /// Target schema contains additional elements.
    /// </summary>
    TargetIsSuperset = 1,
    /// <summary>
    /// Target schema doesn't contain some elements of the extracted schema.
    /// </summary>
    TargetIsSubset = 2,
    /// <summary>
    /// Both new and removed elements are found.
    /// </summary>
    NotEqual = 3
  }
}