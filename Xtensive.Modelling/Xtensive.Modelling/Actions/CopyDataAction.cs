// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.14

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Copy data contained in node to another node.
  /// </summary>
  [Serializable]
  public class CopyDataAction : NodeAction
  {
    private string newPath;
    private readonly CollectionBaseSlim<CopyParameter> parameters = 
      new CollectionBaseSlim<CopyParameter>();

    /// <summary>
    /// Gets or sets the new path (path in target model).
    /// </summary>
    public string NewPath {
      get { return newPath; }
      set {
        this.EnsureNotLocked();
        newPath = value;
      }
    }

    /// <summary>
    /// Gets the copy parameters.
    /// </summary>
    public CollectionBaseSlim<CopyParameter> Parameters
    {
      get { return parameters; }
    }
    
    /// <inheritdoc/>
    protected override void PerformExecute(IModel model, IPathNode item)
    {
    }

    /// <inheritdoc/>
    protected override void GetParameters(List<Pair<string>> parameters)
    {
      base.GetParameters(parameters);
      if (newPath!=null)
        parameters.Add(new Pair<string>("NewPath", newPath));
      parameters.Add(new Pair<string>("Conditions", 
        string.Join(", ", this.parameters.Select(condition=>condition.ToString()).ToArray())));
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      parameters.Lock(recursive);
    }
  }
}