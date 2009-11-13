// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Include algorithm.
  /// </summary>
  public enum IncludeAlgorithm
  {
    /// <summary>
    /// Include algorithm is automatically chosen according to a filter data.
    /// </summary>
    Auto = 0,
    /// <summary>
    /// Include is performed using comprex condition.
    /// </summary>
    ComplexCondition = 1,
    /// <summary>
    /// Include is performed using temporary tables.
    /// </summary>
    TemporaryTable = 2,
  }
}