// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System.Collections.Generic;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Core.Helpers;
using Xtensive.Modelling.Resources;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="Node"/> comparison result.
  /// </summary>
  public class NodeDifference : Difference,
    IDifference<Node>,
    IHasPropertyChanges
  {
    /// <inheritdoc/>
    public new Node Source {
      get { return (Node) base.Source; }
    }

    /// <inheritdoc/>
    public new Node Target {
      get { return (Node) base.Target; }
    }

    /// <summary>
    /// Gets or sets the movement info.
    /// </summary>
    public MovementInfo MovementInfo { get; set; }

    /// <inheritdoc/>
    public Dictionary<string, Difference> PropertyChanges { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this difference describes changed node.
    /// </summary>
    public bool IsChanged {
      get { return (MovementInfo & MovementInfo.Changed)!=0 || PropertyChanges.Count!=0; }
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      var sb = new StringBuilder();
      sb.Append(MovementInfo);
      foreach (var pair in PropertyChanges)
        sb.AppendLine().AppendFormat(Strings.PropertyChangeFormat,
          pair.Key, pair.Value);
      return sb.ToString().Indent(ToString_IndentSize, false);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> value.</param>
    /// <param name="target">The <see cref="Target"/> value.</param>
    public NodeDifference(Node source, Node target)
      : base(source, target)
    {
      PropertyChanges = new Dictionary<string, Difference>();
    }
  }
}