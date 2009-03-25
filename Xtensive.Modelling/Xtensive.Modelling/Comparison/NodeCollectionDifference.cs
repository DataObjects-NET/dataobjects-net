// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="NodeCollection"/> comparison result.
  /// </summary>
  [Serializable]
  public class NodeCollectionDifference : Difference,
    IDifference<NodeCollection>
  {
    /// <inheritdoc/>
    public new NodeCollection Source {
      get { return (NodeCollection) base.Source; }
    }

    /// <inheritdoc/>
    public new NodeCollection Target {
      get { return (NodeCollection) base.Target; }
    }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> value.</param>
    /// <param name="target">The <see cref="Target"/> value.</param>
    public NodeCollectionDifference(NodeCollection source, NodeCollection target)
      : base(source, target)
    {
    }
  }
}