// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2012.02.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Collections.Graphs
{
  /// <summary>
  /// Topological sorter.
  /// </summary>
  public static class TopologicalSorter
  {
    #region Nested type: OrderedSet<T>

    private sealed class OrderedSet<T> : IEnumerable<T>
    {
      private readonly System.Collections.Generic.LinkedList<T> list;
      private readonly Dictionary<T, LinkedListNode<T>> map;

      public int Count
      {
        get { return list.Count; }
      }

      public T Pop()
      {
        var node = list.First;
        if (node == null)
          return default(T);
        list.RemoveFirst();
        map.Remove(node.Value);
        return node.Value;
      }

      public void Remove(T item)
      {
        if (map.Remove(item, out var node)) {
          list.Remove(node);
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      public IEnumerator<T> GetEnumerator()
      {
        return list.GetEnumerator();
      }

      public OrderedSet(IEnumerable<T> source)
      {
        list = new System.Collections.Generic.LinkedList<T>();
        map = new Dictionary<T, LinkedListNode<T>>();
        foreach (var item in source) {
          var node = list.AddLast(item);
          map.Add(item, node);
        }
      }
    }

    #endregion

    /// <summary>
    /// Sorts the <paramref name="graph"/> in topological order (nodes without incoming edges go first).
    /// <note>
    /// This method modifies the <paramref name="graph"/> by removing all non-loop edges from it!
    /// </note>
    /// </summary>
    /// <returns>Sorting result.</returns>
    public static TopologicalSortResult<TNode, TEdge> Sort<TNode, TEdge>(Graph<TNode, TEdge> graph, Predicate<TEdge> edgeBreaker = null)
      where TNode: Node
      where TEdge: Edge
    {
      ArgumentValidator.EnsureArgumentNotNull(graph, "graph");

      var result = new TopologicalSortResult<TNode, TEdge>();

      HashSet<TNode> sortedNodes;
      OrderedSet<TNode> unsortedNodes;
      Queue<TEdge> breakableEdges;

      if (edgeBreaker!=null) {
        sortedNodes = null;
        unsortedNodes = new OrderedSet<TNode>(graph.Nodes);
        breakableEdges = new Queue<TEdge>(graph.Edges);
      }
      else {
        sortedNodes = new HashSet<TNode>();
        unsortedNodes = null;
        breakableEdges = null;
      }
      var nodesWithoutIncomingEdges = new Queue<TNode>(graph.Nodes.Where(n => !n.HasIncomingEdges));

    restart:
      // Sorting
      while (nodesWithoutIncomingEdges.TryDequeue(out var node)) {
        if (unsortedNodes!=null) // Break edges
          unsortedNodes.Remove(node);
        else
          sortedNodes.Add(node);
        result.SortedNodes.Add(node);
        if (!node.HasOutgoingEdges)
          continue;
        foreach (var edge in node.OutgoingEdges) {
          edge.Target.IncomingEdges.Remove(edge);
          edge.IsAttached = false; // actually, Detach(), because we perform node.OutgoingEdges.Clear() further
          var target = (TNode) edge.Target;
          if (!target.HasIncomingEdges)
            nodesWithoutIncomingEdges.Enqueue(target);
        }
        node.OutgoingEdges.Clear();
      }

      if (unsortedNodes!=null) { // Break edges
        if (unsortedNodes.Count != 0) {
          // Trying to break edges (collection is always empty when breakEdges==false)
          while (breakableEdges.TryDequeue(out var edge)) {
            if (!edge.IsAttached || !edgeBreaker(edge))
              continue;
            result.BrokenEdges.Add(edge);
            edge.Detach();
            var target = (TNode) edge.Target;
            if (!target.HasIncomingEdges) {
              nodesWithoutIncomingEdges.Enqueue(target);
              goto restart;
            }
          }
          result.LoopNodes.AddRange(unsortedNodes);
        }
      }
      else {
        if (sortedNodes.Count != graph.Nodes.Count)
          result.LoopNodes.AddRange(graph.Nodes.Where(node => !sortedNodes.Contains(node)));
      }

      return result;
    }
  }
}