// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.20

using Xtensive.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Produces upgrade script (sequence of <see cref="NodeAction"/>s)
  /// for the specified <see cref="Difference"/> and <see cref="HintSet"/>.
  /// </summary>
  public interface IUpgrader
  {
    /// <summary>
    /// Gets the upgrade script (sequence of <see cref="NodeAction"/>s)
    /// for the specified <see cref="Difference"/> and <see cref="HintSet"/>.
    /// If <paramref name="comparer"/> is provided, it is used to validate
    /// the result of upgrade script.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <param name="hints">The upgrade hints.</param>
    /// <param name="comparer">The comparer to use to validate
    /// the result of upgrade script.</param>
    /// <returns>
    /// Sequence of <see cref="NodeAction"/>s describing the upgrade.
    /// </returns>
    ReadOnlyList<NodeAction> GetUpgradeSequence(Difference difference, HintSet hints, IComparer comparer);

    /// <summary>
    /// Gets the upgrade script (sequence of <see cref="NodeAction"/>s)
    /// for the specified <see cref="Difference"/> and <see cref="HintSet"/>.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <param name="hints">The upgrade hints.</param>
    /// <returns>
    /// Sequence of <see cref="NodeAction"/>s describing the upgrade.
    /// </returns>
    ReadOnlyList<NodeAction> GetUpgradeSequence(Difference difference, HintSet hints);
  }
}