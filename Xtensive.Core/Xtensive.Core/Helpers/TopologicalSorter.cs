// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.07

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using System.Linq;

namespace Xtensive.Core.Helpers
{
  public static class TopologicalSorter<TItem>
  {
    #region Nested type: Node

    public class Node
    {
      public TItem Item { get; private set; }
      public HashSet<Node> In { get; private set; }
      public HashSet<Node> Out { get; private set; }

      public int GetConnectionCount(bool outgoing)
      {
        if (outgoing)
          return Out==null ? 0 : Out.Count;
        else
          return In==null ? 0 : In.Count;
      }

      public void AddConnection(Node node, bool outgoing)
      {
        ArgumentValidator.EnsureArgumentNotNull(node, "node");
        Node a, b;
        if (outgoing) {
          a = this;
          b = node;
        }
        else {
          a = node;
          b = this;
        }
        if (a.Out==null)
          a.Out = new HashSet<Node>();
        if (b.In==null)
          b.In = new HashSet<Node>();
        a.Out.Add(b);
        b.In.Add(a);
      }

      public void RemoveConnection(Node node, bool outgoing)
      {
        ArgumentValidator.EnsureArgumentNotNull(node, "node");
        Node a, b;
        if (outgoing) {
          a = this;
          b = node;
        }
        else {
          a = node;
          b = this;
        }
        if (a.Out==null)
          return;
        if (b.In==null)
          return;
        a.Out.Remove(b);
        b.In.Remove(a);
      }


      // Constrcutors

      public Node(TItem item)
      {
        Item = item;
      }
    }

    #endregion

    /// <summary>
    /// Sorts the specified oriented graph of the items in their topological order
    /// (following the outgoing connections provided by <paramref name="connector"/>).
    /// </summary>
    /// <param name="items">The items to sort.</param>
    /// <param name="connector">The connector delegate returning <see langword="true" />
    /// if there is outgoing connection between the first and the second node.</param>
    /// <returns>
    /// Sorting result, if there were no loops;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public static List<TItem> Sort(IEnumerable<TItem> items, Predicate<TItem, TItem> connector)
    {
      List<Node> loops;
      return Sort(items, connector, out loops);
    }

    /// <summary>
    /// Sorts the specified oriented graph of the items in their topological order
    /// (following the outgoing connections provided by <paramref name="connector"/>).
    /// </summary>
    /// <param name="items">The items to sort.</param>
    /// <param name="connector">The connector delegate returning <see langword="true"/>
    /// if there is outgoing connection between the first and the second node.</param>
    /// <param name="loops">The loops, if found.</param>
    /// <returns>
    /// Sorting result, if there were no loops;
    /// otherwise, <see langword="null"/>.
    /// In this case <paramref name="loops"/> will contain only the loop edges.
    /// </returns>
    public static List<TItem> Sort(IEnumerable<TItem> items, Predicate<TItem, TItem> connector, out List<Node> loops)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      ArgumentValidator.EnsureArgumentNotNull(connector, "connector");
      List<Node> nodes = GetNodes(items, connector);
      return Sort(nodes, out loops);
    }

    /// <summary>
    /// Sorts the specified oriented graph of the items in their topological order
    /// (following the outgoing connections provided by <paramref name="connector"/>).
    /// </summary>
    /// <param name="items">The items to sort.</param>
    /// <param name="connector">The connector delegate returning <see langword="true"/>
    /// if there is outgoing connection between the first and the second node.</param>
    /// <param name="removedEdges">Edges removed to make graph non-cyclic.</param>
    /// <returns>
    /// Sorting result
    /// </returns>
    public static List<TItem> Sort(IEnumerable<TItem> items, Predicate<TItem, TItem> connector, out List<Pair<Node>> removedEdges)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      ArgumentValidator.EnsureArgumentNotNull(connector, "connector");
      List<Node> nodes = GetNodes(items, connector);
      return Sort(nodes, out removedEdges);
    }

    /// <summary>
    /// Sorts the specified oriented graph of the nodes in their topological order
    /// (following the outgoing connections).
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="loops">The loops, if found.</param>
    /// <returns>Sorting result, if there were no loops;
    /// otherwise, <see langword="null" />. 
    /// In this case <paramref name="nodes"/> will contain only the loop edges.</returns>
    public static List<TItem> Sort(List<Node> nodes, out List<Node> loops)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodes, "nodes");
      var queue = new Queue<Node>();
      var result = new List<TItem>();
      SortInternal(nodes, queue, result);
      loops = new List<Node>();
      foreach (var node in nodes) {
        if (node.GetConnectionCount(true) > 0)
          loops.Add(node);
      }
      if (loops.Count > 0)
        return null;
      loops = null;
      return result;
    }


    /// <summary>
    /// Sorts the specified oriented graph of the nodes in their topological order
    /// (following the outgoing connections).
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="removedEdges">Edges removed to make graph non-cyclic.</param>
    /// <returns>Sorting result.</returns>
    public static List<TItem> Sort(IEnumerable<Node> nodes, out List<Pair<Node>> removedEdges)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodes, "nodes");
      var nodeList = nodes.ToList();
      var queue = new Queue<Node>();
      var result = new List<TItem>();
      removedEdges = new List<Pair<Node>>();
      do {
        SortInternal(nodeList, queue, result);
        nodeList = new List<Node>(nodeList.Where(node => node.GetConnectionCount(true) > 0));
        if (nodeList.Count > 0) {
          Node destinationNode = nodeList[0].Out.First();
          removedEdges.Add(new Pair<Node>(nodeList[0], destinationNode));
          nodeList[0].RemoveConnection(destinationNode, true);
        }
      } while (nodeList.Count > 0);
      return result;
    }

    private static void SortInternal(IEnumerable<Node> nodes, Queue<Node> queue, ICollection<TItem> result)
    {
      // Enqueuing sources
      foreach (var node in nodes)
        if (node.GetConnectionCount(false)==0)
          queue.Enqueue(node);
      // Processing the queue
      while (queue.Count > 0) {
        var node = queue.Dequeue();
        result.Add(node.Item);
        if (node.GetConnectionCount(true) > 0) {
          var outgoing = node.Out.ToArray();
          foreach (var outNode in outgoing) {
            node.RemoveConnection(outNode, true);
            if (outNode.GetConnectionCount(false)==0)
              queue.Enqueue(outNode);
          }
        }
      }
    }

    private static List<Node> GetNodes(IEnumerable<TItem> items, Predicate<TItem, TItem> connector)
    {
      // Converting all the items to nodes
      var nodes = new List<Node>();
      foreach (var item in items)
        nodes.Add(new Node(item));
      // Adding connections
      foreach (var nodeA in nodes)
        foreach (var nodeB in nodes) {
          if (nodeA==nodeB)
            continue;
          if (connector.Invoke(nodeA.Item, nodeB.Item))
            nodeA.AddConnection(nodeB, true);
        }
      return nodes;
    }
  }
}