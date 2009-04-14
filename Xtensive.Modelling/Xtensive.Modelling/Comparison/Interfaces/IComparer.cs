// Copyright (C) 2009 Xtensive LLC.
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
    /// Gets the source model to compare.
    /// </summary>
    IModel Source { get; }

    /// <summary>
    /// Gets the target model to compare.
    /// </summary>
    IModel Target { get; }

    /// <summary>
    /// Gets the comparison hints.
    /// </summary>
    HintSet Hints { get; }

    /// <summary>
    /// Gets the difference.
    /// </summary>
    Difference Difference { get; }
  }
}