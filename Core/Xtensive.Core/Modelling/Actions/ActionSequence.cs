// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

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
    [NonSerialized]
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
    public void Add(NodeAction action)
    {
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      this.EnsureNotLocked();
      // Only locked actions can be added
      var ca = action as PropertyChangeAction;
      if (ca!=null && actions.Count!=0) {
        // Let's try to join two change actions
        var lastIndex = actions.Count - 1;
        var last = actions[lastIndex] as PropertyChangeAction;
        if (last!=null && ca.Path==last.Path) {
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

    /// <inheritdoc/>
    public void Add(IEnumerable<NodeAction> actions)
    {
      ArgumentValidator.EnsureArgumentNotNull(actions, "actions");
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
        var gna = action as GroupingNodeAction;
        if (gna!=null)
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
      foreach (var action in actions)
        sb.AppendLine(action.ToString());
      return sb.ToString();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ActionSequence()
    {
    }
  }
}