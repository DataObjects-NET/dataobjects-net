// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Property change action.
  /// </summary>
  [Serializable]
  public class PropertyChangeAction : NodeAction
  {
    private IDictionary<string, object> properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the properties.
    /// </summary>
    public IDictionary<string, object> Properties {
      get { return properties; }
    }

    /// <inheritdoc/>
    protected override void PerformExecute(IModel model, IPathNode item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var node = (Node) item;
      foreach (var pair in properties)
        node.SetProperty(pair.Key, PathNodeReference.Resolve(model, pair.Value));
    }

    /// <inheritdoc/>
    protected override void GetParameters(List<Pair<string>> parameters)
    {
      base.GetParameters(parameters);
      foreach (var pair in properties)
        parameters.Add(new Pair<string>(pair.Key, pair.Value==null ? null : pair.Value.ToString()));
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      properties = new ReadOnlyDictionary<string, object>(properties, true);
      base.Lock(recursive);
    }
  }
}