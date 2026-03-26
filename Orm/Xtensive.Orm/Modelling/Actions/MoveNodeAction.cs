// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Collections;

using System.Linq;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Any kinds of node movement (parent changed, name changed or index changed).
  /// </summary>
  [Serializable]
  public class MoveNodeAction : NodeAction
  {
    private string parent;
    private string name;
    private int? index;
    private string newPath;

    /// <summary>
    /// Gets or sets the node parent path.
    /// </summary>
    public string Parent {
      get { return parent; }
      set {
        EnsureNotLocked();
        parent = value;
      }
    }

    /// <summary>
    /// Gets or sets the node name.
    /// </summary>
    public string Name {
      get { return name; }
      set {
        EnsureNotLocked();
        name = value;
      }
    }

    /// <summary>
    /// Gets or sets the node index.
    /// </summary>
    public int? Index {
      get { return index; }
      set {
        EnsureNotLocked();
        index = value;
      }
    }

    /// <summary>
    /// Gets or sets the new node path.
    /// </summary>
    /// <value>The new path.</value>
    public string NewPath {
      get { return newPath; }
      set {
        EnsureNotLocked();
        newPath = value;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Required constructor isn't found.</exception>
    protected override void PerformExecute(IModel model, IPathNode item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var node = (Node) item;
      var newParent = parent==null ? node.Parent : (Node) model.Resolve(parent, true);
      if ((node is IModel) && (newParent is IModel))
        newParent = null;
      var newName = name ?? node.Name;
      var newIndex = !index.HasValue ? node.Index : index.Value;
      node.Move(newParent, newName, newIndex);
    }

    /// <inheritdoc/>
    protected override void GetParameters(List<Pair<string>> parameters)
    {
      base.GetParameters(parameters);
      if (parent!=null)
        parameters.Add(new Pair<string>("Parent", parent));
      if (name!=null)
        parameters.Add(new Pair<string>("Name", name));
      if (index.HasValue)
        parameters.Add(new Pair<string>("Index", index.ToString()));
    }
  }
}