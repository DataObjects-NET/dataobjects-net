// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Resources;
using System.Linq;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Describes node creation.
  /// </summary>
  [Serializable]
  public class MoveNodeAction : NodeAction
  {
    private string parent;
    private string name;
    private int? index;

    public string Parent {
      get { return parent; }
      set {
        this.EnsureNotLocked();
        parent = value;
      }
    }

    public string Name {
      get { return name; }
      set {
        this.EnsureNotLocked();
        name = value;
      }
    }

    public int? Index {
      get { return index; }
      set {
        this.EnsureNotLocked();
        index = value;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Required constructor isn't found.</exception>
    protected override void PerformApply(IModel model, IPathNode item)
    {
      var node = (Node) item;
      var newParent = parent==null ? node.Parent : (Node) model.Resolve(parent);
      var newName = name==null ? node.Name : name;
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