// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.20

using System.Collections.Generic;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Produces upgrade script (<see cref="Actions"/>)
  /// for the specified <see cref="Difference"/> and <see cref="Hints"/>.
  /// </summary>
  public interface IUpgrader
  {
    /// <summary>
    /// Gets the comparison hints.
    /// </summary>
    HintSet Hints { get; }

    /// <summary>
    /// Gets the difference.
    /// </summary>
    Difference Difference { get; }

    /// <summary>
    /// Gets the upgrade actions.
    /// </summary>
    IEnumerable<NodeAction> Actions { get; }
  }
}