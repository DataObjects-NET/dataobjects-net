// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Resources;

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

    /// <summary>
    /// Gets or set a value indicating whether source node 
    /// must be removed on cleanup stage.
    /// </summary>
    public bool IsRemoveOnCleanup { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether source node 
    /// must be removed before his parent.
    /// </summary>
    public bool IsDependentOnParent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether data is changed.
    /// </summary>
    public bool IsDataChanged { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="MovementInfo"/> contain flag 
    /// <see cref="Comparison.MovementInfo.Removed"/>.
    /// </summary>
    public bool IsRemoved
    {
      get { return (MovementInfo & MovementInfo.Removed)!=0; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="MovementInfo"/> contain flag 
    /// <see cref="Comparison.MovementInfo.Created"/>.
    /// </summary>
    public bool IsCreated
    {
      get { return (MovementInfo & MovementInfo.Created)!=0; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="MovementInfo"/> contain flag 
    /// <see cref="Comparison.MovementInfo.Changed"/>.
    /// </summary>
    public bool IsChanged
    {
      get { return (MovementInfo & MovementInfo.Changed)!=0; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="MovementInfo"/> contain flag 
    /// <see cref="Comparison.MovementInfo.NameChanged"/>.
    /// </summary>
    public bool IsNameChanged
    {
      get { return (MovementInfo & MovementInfo.NameChanged)!=0; }
    }

    /// <inheritdoc/>
    public Dictionary<string, Difference> PropertyChanges { get; private set; }

    /// <inheritdoc/>
    public override bool HasChanges {
      get { 
        return 
          (MovementInfo & MovementInfo.Changed)!=0 || 
          PropertyChanges.Count!=0 || 
          IsDataChanged; 
      }
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      var sb = new StringBuilder();
      sb.Append(MovementInfo);
      if (IsRemoveOnCleanup)
        sb.Append(" RemoveOnCleanup");
      if (IsDataChanged)
        sb.Append(" DataChanged");
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