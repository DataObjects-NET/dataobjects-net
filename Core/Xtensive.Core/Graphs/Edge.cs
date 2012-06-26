// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2012.02.22

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Graphs
{
  /// <summary>
  /// A connection between two graph <see cref="Node"/>s.
  /// </summary>
  [Serializable]
  public class Edge
  {
    /// <summary>
    /// Gets edge value.
    /// </summary>
    public virtual object UntypedValue { get { return null; } }

    /// <summary>
    /// Gets or sets edge source.
    /// </summary>
    public Node Source { get; set; }

    /// <summary>
    /// Gets or sets edge target.
    /// </summary>
    public Node Target { get; set; }

    /// <summary>
    /// Indicates whether this edge is attached to its source and target nodes.
    /// </summary>
    public bool IsAttached { get; internal set; }

    /// <summary>
    /// Attaches the edge to source and target nodes.
    /// </summary>
    public void Attach()
    {
      Source.OutgoingEdges.Add(this);
      Target.IncomingEdges.Add(this);
      IsAttached = true;
    }

    /// <summary>
    /// Detaches the edge from source and target nodes.
    /// </summary>
    public void Detach()
    {
      Source.OutgoingEdges.Remove(this);
      Target.IncomingEdges.Remove(this);
      IsAttached = false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
      var value = UntypedValue==null ? "" : string.Concat(" (", UntypedValue, ")");
      return string.Concat(Source, " -> ", Target, value);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public Edge()
    {
    }

    /// <summary>
    /// Creates new graph edge instance and <see cref="Attach"/>es it to nodes.
    /// </summary>
    /// <param name="source">Source node.</param>
    /// <param name="target">Target node.</param>
    public Edge(Node source, Node target)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      Source = source;
      Target = target;
      Attach();
    }
  }
}