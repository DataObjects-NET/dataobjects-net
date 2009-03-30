// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Diagnostics;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Describes node removal.
  /// </summary>
  [Serializable]
  public class RemoveNodeAction : NodeAction
  {
    /// <inheritdoc/>
    protected override void PerformApply(IModel model, IPathNode item)
    {
      var node = (Node) item;
      node.Remove();
    }

    /// <inheritdoc/>
    public override string[] GetDependencies()
    {
      return new[] {Path + ".Remove()"};
    }
  }
}