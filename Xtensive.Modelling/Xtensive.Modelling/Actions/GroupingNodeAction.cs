// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.22

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using System.Linq;
using Xtensive.Core.Helpers;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// A grouping node action.
  /// </summary>
  [Serializable]
  public class GroupingNodeAction : NodeAction
  {
    private string comment;
    private IList<NodeAction> actions = new List<NodeAction>();

    /// <summary>
    /// Gets or sets the comment.
    /// </summary>
    public string Comment {
      get { return comment; }
      set {
        this.EnsureNotLocked();
        comment = value;
      }
    }

    /// <summary>
    /// Gets the list of nested actions.
    /// </summary>
    public IList<NodeAction> Actions {
      get { return actions; }
    }

    /// <summary>
    /// Flattens all the <see cref="GroupingNodeAction"/>s from <see cref="Actions"/> sequence.
    /// </summary>
    /// <returns>Flattened sequence of nested actions.</returns>
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

    /// <inheritdoc/>
    protected override void PerformExecute(IModel model, IPathNode item)
    {
      foreach (var action in actions)
        action.Execute(model);
    }

    protected override void GetParameters(List<Xtensive.Core.Pair<string>> parameters)
    {
      base.GetParameters(parameters);
      if (!comment.IsNullOrEmpty())
        parameters.Add(new Pair<string>("Comment", comment));
    }

    /// <inheritdoc/>
    protected override IEnumerable<NodeAction> GetNestedActions()
    {
      return base.GetNestedActions().Concat(actions);
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      actions = new ReadOnlyList<NodeAction>(actions, true);
    }
  }
}