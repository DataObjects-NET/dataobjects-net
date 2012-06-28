// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2012.02.22

using System;
using System.Collections.Generic;

namespace Xtensive.Collections.Graphs
{
  /// <summary>
  /// Graph node. 
  /// </summary>
  [Serializable]
  public class Node
  {
    private HashSet<Edge> incomingEdges;
    private HashSet<Edge> outgoingEdges;

    /// <summary>
    /// Gets node value.
    /// </summary>
    public virtual object UntypedValue { get { return null; } }

    /// <summary>
    /// Indicates whether this node has incoming edges.
    /// </summary>
    public bool HasIncomingEdges { get { return incomingEdges!=null && incomingEdges.Count!=0; } }
    
    /// <summary>
    /// Indicates whether this node has outgoing edges.
    /// </summary>
    public bool HasOutgoingEdges { get { return outgoingEdges!=null && outgoingEdges.Count!=0; } }

    /// <summary>
    /// Gets incoming edges.
    /// </summary>
    public HashSet<Edge> IncomingEdges
    {
      get { return incomingEdges ?? (incomingEdges = new HashSet<Edge>()); }
    }

    /// <summary>
    /// Gets outgoing edges.
    /// </summary>
    public HashSet<Edge> OutgoingEdges
    {
      get { return outgoingEdges ?? (outgoingEdges = new HashSet<Edge>()); }
    }

    /// <summary>
    /// Gets all node edges.
    /// </summary>
    public IEnumerable<Edge> Edges
    {
      get
      {
        if (HasIncomingEdges)
          foreach (var c in incomingEdges)
            yield return c;
        if (HasOutgoingEdges)
          foreach (var c in outgoingEdges)
            yield return c;
      }
    } 

    /// <inheritdoc />
    public override string ToString()
    {
      return string.Concat("[", UntypedValue, "]");
    }
  }
}