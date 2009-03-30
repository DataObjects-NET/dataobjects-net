// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Comparable object contract.
  /// </summary>
  public interface IDifferentiable
  {
    /// <summary>
    /// Compares this instance to the <paramref name="target"/> object.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="propertyName">Name of the property that is being compared now (if any).</param>
    /// <param name="swap">Indicates whether source (this instance)
    /// and target are swapped.</param>
    /// <returns>
    /// Differences, if any;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    Difference GetDifferenceWith(object target, string propertyName, bool swap);
  }
}