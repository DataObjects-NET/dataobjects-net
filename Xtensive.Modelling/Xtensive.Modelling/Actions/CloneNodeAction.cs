// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.22

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Modelling.Actions
{
  [Serializable]
  public class CloneNodeAction : NodeAction
  {
    private Node source;
    private string name;
    private int? index;

    public Node Source {
      get { return source; }
      set {
        this.EnsureNotLocked();
        source = value;
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
    protected override void PerformExecute(IModel model, IPathNode item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      ArgumentValidator.EnsureArgumentNotNull(name, "name");
      source.Clone((Node) item, name);
      if (index.HasValue)
        source.Index = index.Value;
    }

    /// <inheritdoc/>
    protected override void GetParameters(List<Pair<string>> parameters)
    {
      base.GetParameters(parameters);
      parameters.Add(new Pair<string>("Source", source.ToString()));
      if (name!=null)
        parameters.Add(new Pair<string>("Name", name));
      if (index.HasValue)
        parameters.Add(new Pair<string>("Index", index.ToString()));
    }
  }
}