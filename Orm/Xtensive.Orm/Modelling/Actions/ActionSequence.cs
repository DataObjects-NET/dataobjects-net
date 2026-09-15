// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core;


namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// <see cref="NodeAction"/> sequence implementation.
  /// </summary>
  public class ActionSequence : LockableBase,
    IActionSequence
  {
    private readonly List<NodeAction> actions = new List<NodeAction>();
    private ActionScope currentScope;

    /// <inheritdoc/>
    public ActionScope CurrentScope => currentScope;

    /// <inheritdoc/>
    public ActionScope LogAction()
    {
      var newScope = new ActionScope(this);
      if (currentScope is null)
        currentScope = newScope;
      return newScope;
    }

    /// <inheritdoc/>
    public void Add(NodeAction action)
    {
      ArgumentNullException.ThrowIfNull(action);
      EnsureNotLocked();
      // Only locked actions can be added
      if (action is PropertyChangeAction ca && actions.Count != 0) {
        // Let's try to join two change actions
        var lastIndex = actions.Count - 1;
        if (actions[lastIndex] is PropertyChangeAction last && ca.Path == last.Path) {
          foreach (var pair in last.Properties) {
            _ = ca.Properties.TryAdd(pair.Key, pair.Value);
          }
          actions.RemoveAt(lastIndex);
        }
      }
      action.Lock(true);
      actions.Add(action);
    }

    /// <inheritdoc/>
    public void Add(IEnumerable<NodeAction> actions)
    {
      ArgumentNullException.ThrowIfNull(actions);
      foreach (NodeAction action in actions)
        Add(action);
    }

    /// <inheritdoc/>
    public void Apply(IModel model)
    {
      foreach (var action in actions)
        action.Execute(model);
    }

    /// <inheritdoc/>
    public IEnumerable<NodeAction> Flatten()
    {
      foreach (var action in actions) {
        if (action is GroupingNodeAction gna)
          foreach (var nestedAction in gna.Flatten())
            yield return nestedAction;
        else
          yield return action;
      }
    }

    #region Private \ internal methods

    internal void OnCommit(ActionScope scope)
    {
      try {
        var action = scope.Action;
        if (scope.IsCommittable && scope.IsCommitted)
          Add(action);
      }
      finally {
        if (scope.IsCommittable)
          currentScope = null;
      }
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public IEnumerator<NodeAction> GetEnumerator()
    {
      return actions.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return actions.GetEnumerator();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder();
      foreach (var action in actions) {
        _ = sb.AppendLine(action.ToString());
      }
      return sb.ToString();
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public ActionSequence()
    {
    }
  }
}