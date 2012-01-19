// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Include algorithm.
  /// </summary>
  public enum IncludeAlgorithm
  {
    /// <summary>
    /// Inclusion algorithm must be automatically chosen based on filter data.
    /// </summary>
    Auto = 0,
    /// <summary>
    /// Inclusion is described as a complex condition (expression).
    /// </summary>
    ComplexCondition = 1,
    /// <summary>
    /// Inclusion is described via temporary table.
    /// </summary>
    TemporaryTable = 2,
  }
}