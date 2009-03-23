// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core.Helpers;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// <see cref="NodeAction"/> sequence implementation.
  /// </summary>
  [Serializable]
  public class ActionSequence : LockableBase,
    IActionSequence
  {
    private readonly List<NodeAction> actions = new List<NodeAction>();
    private ActionScope currentScope;

    /// <inheritdoc/>
    public ActionScope CurrentScope {
      get { return currentScope; }
    }

    /// <inheritdoc/>
    public ActionScope LogAction()
    {
      var newScope = new ActionScope(this);
      if (currentScope==null)
        currentScope = newScope;
      return newScope;
    }

    /// <inheritdoc/>
    public void Apply(IModel model)
    {
      foreach (var action in actions)
        action.Apply(model);
    }

    #region Private \ internal methods

    internal void OnCommit(ActionScope scope)
    {
      try {
        this.EnsureNotLocked();
        var action = scope.Action;
        // Only locked actions can be added
        if (scope.IsCommittable && scope.IsCommitted) {
          var ca = action as ChangeAction;
          if (ca!=null && actions.Count!=0) {
            // Let's try to join two change actions
            var lastIndex = actions.Count - 1;
            var last = actions[lastIndex] as ChangeAction;
            if (last!=null) {
              foreach (var pair in last.Properties) {
                if (!ca.Properties.ContainsKey(pair.Key))
                  ca.Properties.Add(pair.Key, pair.Value);
              }
              actions.RemoveAt(lastIndex);
            }
          }
          action.Lock(true); 
          actions.Add(action);
        }
      }
      finally {
        if (scope.IsCommittable)
          currentScope = null;
      }
    }

    #endregion

    #region IEnumerable<...> methods

    public IEnumerator<NodeAction> GetEnumerator()
    {
      return actions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return actions.GetEnumerator();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder();
      foreach (var action in actions)
        sb.AppendLine(action.ToString());
      return sb.ToString();
    }


    // Constructors

    public ActionSequence()
    {
    }
  }
}