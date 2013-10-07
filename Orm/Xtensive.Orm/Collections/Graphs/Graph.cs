// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2012.02.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;


namespace Xtensive.Collections.Graphs
{
  /// <summary>
  /// A graph.
  /// </summary>
  /// <typeparam name="TNode">Node type.</typeparam>
  /// <typeparam name="TEdge">Edge type.</typeparam>
  [Serializable]
  public class Graph<TNode, TEdge>
    where TNode : Node
    where TEdge : Edge
  {
    /// <summary>
    /// Get a list of graph nodes.
    /// </summary>
    public List<TNode> Nodes { get; private set; }

    /// <summary>
    /// Gets a sequence of graph edges.
    /// </summary>
    public IEnumerable<TEdge> Edges
    {
      get { return Nodes.SelectMany(node => node.OutgoingEdges.Cast<TEdge>()); }
    }

    /// <summary>
    /// Creates a mutable copy of the graph.
    /// Mutable copy of the graph is a graph having identical structure (e.g. set of nodes and edges),
    /// but values of its nodes and edges points to appropriate original nodes and edges from the graph. 
    /// </summary>
    /// <returns>A mutable copy of the graph.</returns>
    public Graph<Node<TNode>, Edge<TEdge>> CreateMutableCopy()
    {
      var copy = new Graph<Node<TNode>, Edge<TEdge>>();
      var nodeMap = new Dictionary<Node, Node<TNode>>();
      foreach (var node in Nodes) {
        if (nodeMap.ContainsKey(node))
          continue;
        var rNode = new Node<TNode>(node);
        copy.Nodes.Add(rNode);
        nodeMap.Add(node, rNode);
      }
      var processedEdges = new HashSet<Edge>();
      foreach (var rNode in copy.Nodes) {
        var node = rNode.Value;
        foreach (var edge in node.Edges) {
          if (!processedEdges.Contains(edge)) {
            var rEdge = new Edge<TEdge>(nodeMap[edge.Source], nodeMap[edge.Target], (TEdge) edge);
            processedEdges.Add(edge);
          }
        }
      }
      return copy;
    }

    /// <summary>
    /// Creates and attaches outgoing (and consequently, incoming) edges in the graph using specified <paramref name="connector"/>.
    /// </summary>
    /// <param name="connector">Connector delegate. 
    /// Must return a new edge, is there is an edge pointing from its first argument to the second one.
    /// Otherwise is must return null.</param>
    public void AddEdges(Func<TNode, TNode, TEdge> connector)
    {
      ArgumentValidator.EnsureArgumentNotNull(connector, "connector");
      foreach (var source in Nodes)
        foreach (var target in Nodes) {
          var edge = connector.Invoke(source, target);
          if (edge!=null && !edge.IsAttached)
            edge.Attach();
        }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public Graph()
    {
      Nodes = new List<TNode>();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="nodes">Graph nodes.</param>
    public Graph(List<TNode> nodes)
    {
      Nodes = nodes;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="nodes">Graph nodes.</param>
    public Graph(IEnumerable<TNode> nodes)
    {
      Nodes = nodes.ToList();
    }
  }
}