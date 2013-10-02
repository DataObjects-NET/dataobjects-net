// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2012.02.22

using System.Collections.Generic;

namespace Xtensive.Collections.Graphs
{
  /// <summary>
  /// Topological sorter result.
  /// </summary>
  /// <typeparam name="TEdge">Edge type.</typeparam>
  /// <typeparam name="TNode">Node type.</typeparam>
  public sealed class TopologicalSortResult<TNode, TEdge>
    where TNode: Node
    where TEdge: Edge
  {
    /// <summary>
    /// Sorted nodes.
    /// </summary>
    public List<TNode> SortedNodes { get; private set; }

    /// <summary>
    /// Loop nodes.
    /// </summary>
    public List<TNode> LoopNodes { get; private set; }

    /// <summary>
    /// Broken edges.
    /// </summary>
    public List<TEdge> BrokenEdges { get; private set; }

    /// <summary>
    /// Indicates whether result has loops.
    /// </summary>
    public bool HasLoops { get { return LoopNodes.Count!=0; } }

    
    // Constructors

    public TopologicalSortResult()
    {
      SortedNodes = new List<TNode>();
      LoopNodes = new List<TNode>();
      BrokenEdges = new List<TEdge>();
    }
  }
}
