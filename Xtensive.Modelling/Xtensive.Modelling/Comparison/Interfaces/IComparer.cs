// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Compares two models.
  /// </summary>
  public interface IComparer
  {
    /// <summary>
    /// Gets the difference between <paramref name="source"/> 
    /// and <paramref name="target"/> models.
    /// </summary>
    /// <param name="source">The source model.</param>
    /// <param name="target">The target model.</param>
    /// <param name="hints">The comparison hints.</param>
    /// <returns>Difference between <paramref name="source"/> 
    /// and <paramref name="target"/> models;
    /// <see langword="null" />, if none.</returns>
    Difference Compare(IModel source, IModel target, HintSet hints);

    /// <summary>
    /// Gets the difference between <paramref name="source"/> 
    /// and <paramref name="target"/> models.
    /// </summary>
    /// <param name="source">The source model.</param>
    /// <param name="target">The target model.</param>
    /// <returns>Difference between <paramref name="source"/> 
    /// and <paramref name="target"/> models;
    /// <see langword="null" />, if none.</returns>
    Difference Compare(IModel source, IModel target);
  }
}