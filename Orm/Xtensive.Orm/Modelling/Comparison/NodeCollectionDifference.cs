// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling.Actions;


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

    /// <summary>
    /// Gets the item changes.
    /// </summary>
    public List<NodeDifference> ItemChanges { get; private set; }

    /// <inheritdoc/>
    public override bool HasChanges {
      get { return ItemChanges.Count!=0; }
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      var sb = new StringBuilder();
      sb.AppendFormat(Strings.ItemChangeCountFormat, ItemChanges.Count);
      foreach (var difference in ItemChanges)
        sb.AppendLine().AppendFormat(Strings.ItemChangeFormat, difference);
      return sb.ToString().Indent(ToString_IndentSize, false);
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
      ItemChanges = new List<NodeDifference>();
    }
  }
}