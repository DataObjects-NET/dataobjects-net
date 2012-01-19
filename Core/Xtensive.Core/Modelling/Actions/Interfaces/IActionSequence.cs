// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// <see cref="NodeAction"/> sequence contract.
  /// </summary>
  public interface IActionSequence : ILockable,
    IEnumerable<NodeAction>
  {
    /// <summary>
    /// Gets the current action scope.
    /// </summary>
    ActionScope CurrentScope { get; }

    /// <summary>
    /// Appends a new action to this sequence.
    /// </summary>
    /// <returns>An <see cref="ActionScope"/> object allowing to append it.</returns>
    ActionScope LogAction();

    /// <summary>
    /// Adds the specified action to the sequence.
    /// </summary>
    /// <param name="action">The action to add.</param>
    void Add(NodeAction action);

    /// <summary>
    /// Adds the specified action sequence to the sequence.
    /// </summary>
    /// <param name="actions">The sequence of actions to add.</param>
    void Add(IEnumerable<NodeAction> actions);

    /// <summary>
    /// Applies all the actions from the sequence to specified model.
    /// </summary>
    void Apply(IModel model);

    /// <summary>
    /// Flattens all the <see cref="GroupingNodeAction"/>s from this instance.
    /// </summary>
    /// <returns>Flattened action sequence.</returns>
    IEnumerable<NodeAction> Flatten();
  }
}