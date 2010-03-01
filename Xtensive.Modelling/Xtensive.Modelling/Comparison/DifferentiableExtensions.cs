// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.28

using System;
using System.Diagnostics;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="IDifferentiable"/> related extension methods.
  /// </summary>
  public static class DifferentiableExtensions
  {
    /// <summary>
    /// Compares this instance to the <paramref name="target"/> object.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="propertyName">Name of the property that is being compared now (if any).</param>
    /// <returns>
    /// Differences, if any;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public static Difference GetDifferenceWith(this IDifferentiable source, IDifferentiable target, string propertyName)
    {
      if (source!=null)
        return source.GetDifferenceWith(target, propertyName, false);
      else
        return target.GetDifferenceWith(source, propertyName, true);
    }

    /// <summary>
    /// Compares this instance to the <paramref name="target"/> object.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>
    /// Differences, if any;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public static Difference GetDifferenceWith(this IDifferentiable source, IDifferentiable target)
    {
      return GetDifferenceWith(source, target, null);
    }
  }
}