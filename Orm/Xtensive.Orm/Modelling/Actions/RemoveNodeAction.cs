// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Describes node removal.
  /// </summary>
  [Serializable]
  public class RemoveNodeAction : NodeAction
  {
    /// <inheritdoc/>
    protected override void PerformExecute(IModel model, IPathNode item)
    {
      ArgumentNullException.ThrowIfNull(item);
      var node = (Node) item;
      node.Remove();
    }
  }
}