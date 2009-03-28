// Copyright (C) 2009 Xtensive LLC.
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
    /// <returns>
    /// Differences, if any;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public static Difference GetDifferenceWith(this IDifferentiable source, object target)
    {
      return source.GetDifferenceWith(target, false);
    }
  }
}