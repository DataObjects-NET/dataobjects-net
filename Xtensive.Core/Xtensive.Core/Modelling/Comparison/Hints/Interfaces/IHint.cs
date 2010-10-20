// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.26

using System.Collections.Generic;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Comparison hint contract.
  /// </summary>
  public interface IHint
  {
    /// <summary>
    /// Gets the targets of this hint - paths to the nodes affected by it directly.
    /// </summary>
    /// <returns>A sequence of hint targets.</returns>
    IEnumerable<HintTarget> GetTargets();
  }
}