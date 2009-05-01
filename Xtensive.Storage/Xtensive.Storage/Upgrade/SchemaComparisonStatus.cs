// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.01

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Target and extracted schema comparison status.
  /// </summary>
  public enum SchemaComparisonStatus
  {
    /// <summary>
    /// Target schema equal to the extracted schema.
    /// </summary>
    Equal = 0,
    /// <summary>
    /// Target schema contains additional elements.
    /// </summary>
    Superset = 1,
    /// <summary>
    /// Target schema doesn't contain some elements of the extracted schema.
    /// </summary>
    Subset = 2,
    /// <summary>
    /// Both new and removed elements are found.
    /// </summary>
    NotEqual = 3
  }
}