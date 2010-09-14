// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.05

namespace Xtensive.Indexing
{
  /// <summary>
  /// <see cref="EntireValueType"/> related extension methods.
  /// </summary>
  public static class EntireValueTypeExtensions
  {
    /// <summary>
    /// Determines whether the specified <see cref="EntireValueType"/> is infinity.
    /// </summary>
    /// <param name="valueType">Entire value type to check.</param>
    /// <returns>
    /// Check result.
    /// </returns>
    public static bool IsInfinity(this EntireValueType valueType)
    {
      return 
        valueType==EntireValueType.PositiveInfinity || 
        valueType==EntireValueType.NegativeInfinity;
    }
  }
}