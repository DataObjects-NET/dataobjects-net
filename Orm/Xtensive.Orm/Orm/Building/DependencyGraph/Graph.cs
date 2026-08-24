// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.12

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Building.DependencyGraph
{
  internal class Graph<TValue>
  {
    private readonly Dictionary<TValue, Node<TValue>> nodeTable = new Dictionary<TValue, Node<TValue>>();

    public IEnumerable<Node<TValue>> Nodes => nodeTable.Values;

    public Node<TValue> TryGetNode(TValue value)
    {
      if (!nodeTable.TryGetValue(value, out var result))
        return null;
      return result;
    }

    private Node<TValue> GetNode(TValue value)
    {
      var result = TryGetNode(value);
      if (result == null) {
        result = new Node<TValue>(value);
        nodeTable[value] = result;
      }
      return result;
    }

    public Edge<TValue> AddEdge(TValue tail, TValue head, EdgeKind kind, EdgeWeight weight)
    {
      var tailNode = GetNode(tail);
      var headNode = GetNode(head);
      var edge = new Edge<TValue>(tailNode, headNode, kind, weight);
      _ = tailNode.OutgoingEdges.Add(edge);
      _ = headNode.IncomingEdges.Add(edge);
      return edge;
    }

    public void RemoveNode(TValue value)
    {
      var node = TryGetNode(value);
      if (node == null)
        return;

      foreach (var edge in node.IncomingEdges)
        _ = edge.Tail.OutgoingEdges.Remove(edge);
      foreach (var edge in node.OutgoingEdges)
        _ = edge.Head.IncomingEdges.Remove(edge);

      node.IncomingEdges.Clear();
      node.OutgoingEdges.Clear();
      _ = nodeTable.Remove(value);
    }
  }
}