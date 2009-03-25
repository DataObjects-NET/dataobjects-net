// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Typed version of <see cref="IDifferentiable"/>
  /// </summary>
  /// <typeparam name="T">The type of target object.</typeparam>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  public interface IDifferentiable<T,TResult> : IDifferentiable
    where TResult : Difference
  {
    /// <summary>
    /// Compares this instance to the <paramref name="target"/> object.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <returns>
    /// Differences, if any;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    TResult GetDifferenceWith(T target);
  }
}