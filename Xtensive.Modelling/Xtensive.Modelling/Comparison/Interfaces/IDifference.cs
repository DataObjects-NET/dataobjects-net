// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System.Collections.Generic;
using Xtensive.Modelling.Actions;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Difference contract.
  /// </summary>
  public interface IDifference
  {
    /// <summary>
    /// Gets the name of the property this difference is applicable to.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets a value indicating whether this instance describes nested property difference.
    /// </summary>
    bool IsNestedPropertyDifference { get; }

    /// <summary>
    /// Gets the source object.
    /// </summary>
    object Source { get; }

    /// <summary>
    /// Gets the target object.
    /// </summary>
    object Target { get; }

    /// <summary>
    /// Gets the parent difference.
    /// <see langword="null" />, if none.
    /// </summary>
    Difference Parent { get; }

    /// <summary>
    /// Converts the difference to action sequence reproducing it.
    /// </summary>
    /// <returns>An action sequence reproducing the difference.</returns>
    IEnumerable<NodeAction> ToActions();

    /// <summary>
    /// Appends the actions required to build this difference to the specified
    /// action sequence.
    /// </summary>
    /// <param name="actions">The sequence to append to.</param>
    void AppendActions(IList<NodeAction> actions);
  }
}