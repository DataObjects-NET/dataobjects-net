// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Resources;
using Xtensive.Core.Helpers;

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
    public Dictionary<string, NodeDifference> ItemChanges { get; private set; }

    /// <inheritdoc/>
    public override void AppendActions(IList<NodeAction> sequence)
    {
      // Processing item changes
      foreach (var pair in ItemChanges)
        pair.Value.AppendActions(sequence);
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      var sb = new StringBuilder();
      sb.AppendFormat(Strings.ItemChangeCountFormat, ItemChanges.Count);
      foreach (var pair in ItemChanges)
        sb.AppendLine().AppendFormat(Strings.ItemChangeFormat, pair.Key, pair.Value);
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
      ItemChanges = new Dictionary<string, NodeDifference>();
    }
  }
}