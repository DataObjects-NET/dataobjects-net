// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.07

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Sorting
{
  /// <summary>
  /// Topological sorter for oriented graph of the items.
  /// </summary>
  public static class TopologicalSorter
  {
    private enum Color : byte
    {
      White,
      Gray,
      Black
    }

    /// <summary>
    /// Sorts the specified oriented graph of the items in their topological order
    /// (following the outgoing connections provided by <paramref name="connector"/>).
    /// </summary>
    /// <param name="items">The items to sort.</param>
    /// <param name="connector">The connector delegate returning <see langword="true" />
    /// if there is outgoing connection between the first and the second node.</param>
    /// <returns>
    /// Sorting result, if there were no cycles;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public static IEnumerable<TNodeItem> Sort<TNodeItem>(IEnumerable<TNodeItem> items, Predicate<TNodeItem, TNodeItem> connector) =>
      SortWithCycles(items, connector).sorted;

    /// <summary>
    /// Sorts the specified oriented graph of the items in their topological order
    /// (following the outgoing connections provided by <paramref name="connector"/>).
    /// </summary>
    /// <param name="items">The items to sort.</param>
    /// <param name="connector">The connector delegate returning <see langword="true"/>
    /// if there is outgoing connection between the first and the second node.</param>
    /// <param name="cycles">The cycles, if found.</param>
    /// <returns>
    /// Sorting result, if there were no cycles;
    /// otherwise, <see langword="null"/>.
    /// In this case <paramref name="cycles"/> will contain only the cycle edges.
    /// </returns>
    public static (IEnumerable<TNodeItem> sorted, List<TNodeItem> cycles) SortWithCycles<TNodeItem>(IEnumerable<TNodeItem> items, Predicate<TNodeItem, TNodeItem> connector)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      ArgumentValidator.EnsureArgumentNotNull(connector, "connector");

      List<TNodeItem> cycles = null;
      var ar = items.ToArray();
      var n = ar.Length;
      var colors = new Color[n];
      var result = new List<TNodeItem>(n);
      var stack = new Stack<(int index, int curConn)>(n);
      for (var i = 0; i < n; ++i) {
        ref var cur = ref colors[i];
        if (cur == Color.White) {
          cur = Color.Gray;
          stack.Push((i, -1));
        }
        while (stack.TryPop(out var t)) {
          while (true) {
            if (++t.curConn >= n) {
              colors[t.index] = Color.Black;
              result.Add(ar[t.index]);
              break;
            }
            if (t.index != t.curConn) {
              ref var targetColor = ref colors[t.curConn];
              if (targetColor != Color.Black && connector(ar[t.curConn], ar[t.index])) {
                if (targetColor == Color.Gray) {
                  cycles ??= new();           // Cycle discovered
                  for (int cycleStart = t.curConn; stack.TryPop(out t);) {
                    cycles.Add(ar[t.index]);
                    colors[t.index] = Color.Black;
                    if (t.index == cycleStart) {
                      break;
                    }
                  };
                }
                else {                      // targetColor == Color.White
                  targetColor = Color.Gray;
                  stack.Push(t);
                  stack.Push((t.curConn, -1));
                }
                break;
              }
            }
          }
        }
      }
      return (cycles == null ? result : null, cycles);
    }

    /// <summary>
    /// Sorts the specified oriented graph of the nodes in their topological order
    /// (following the outgoing connections).
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="cycles">The loops, if found.</param>
    /// <returns>Sorting result, if there were no loops;
    /// otherwise, <see langword="null" />. 
    /// In this case <paramref name="nodes"/> will contain only the loop edges.</returns>
    public static IEnumerable<TNodeItem> Sort<TNodeItem, TConnectionItem>(List<Node<TNodeItem, TConnectionItem>> nodes, out List<Node<TNodeItem, TConnectionItem>> cycles)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodes, "nodes");
      var head = new Queue<TNodeItem>();
      var tail = new Queue<TNodeItem>();
      var nodeList = nodes.ToList();
      var nodesToRemove = new List<Node<TNodeItem, TConnectionItem>>();
      while (nodeList.Count > 0) {
        nodesToRemove.Clear();
        foreach (var node in nodeList) {
          if (node.IncomingConnectionCount==0) {
            // Add to head
            head.Enqueue(node.Item);
            nodesToRemove.Add(node);
            var connections = node.OutgoingConnections.ToArray();
            foreach (var connection in connections)
              connection.UnbindFromNodes();
          }
          else if (node.OutgoingConnectionCount==0) {
            // Add to tail
            tail.Enqueue(node.Item);
            nodesToRemove.Add(node);
            var connections = node.IncomingConnections.ToArray();
            foreach (var connection in connections)
              connection.UnbindFromNodes();
          }
        }
        if (nodesToRemove.Count==0) {
          cycles = new List<Node<TNodeItem, TConnectionItem>>(nodeList);
          return null;
        }
        foreach (var nodeToRemove in nodesToRemove)
          nodeList.Remove(nodeToRemove);
      }
      cycles = null;
      return head.Concat(tail.Reverse());
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
    public static List<TNodeItem> Sort<TNodeItem>(IEnumerable<TNodeItem> items, Predicate<TNodeItem, TNodeItem> connector, out List<NodeConnection<TNodeItem, object>> removedEdges) =>
      Sort(items, connector, out removedEdges, false);

    /// <summary>
    /// Sorts the specified oriented graph of the items in their topological order
    /// (following the outgoing connections provided by <paramref name="connector"/>).
    /// </summary>
    /// <param name="items">The items to sort.</param>
    /// <param name="connector">The connector delegate returning <see langword="true"/>
    /// if there is outgoing connection between the first and the second node.</param>
    /// <param name="removedEdges">Edges removed to make graph non-cyclic.</param>
    /// <param name="removeWholeNode">If <see langword="true"/> removes whole node in the case of cycle, otherwise removes only one edge.</param>
    /// <returns>
    /// Sorting result
    /// </returns>
    public static List<TNodeItem> Sort<TNodeItem>(IEnumerable<TNodeItem> items, Predicate<TNodeItem, TNodeItem> connector, out List<NodeConnection<TNodeItem, object>> removedEdges, bool removeWholeNode)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      ArgumentValidator.EnsureArgumentNotNull(connector, "connector");
      List<Node<TNodeItem, object>> nodes = GetNodes(items, connector);
      return Sort(nodes, out removedEdges, removeWholeNode);
    }

    /// <summary>
    /// Sorts the specified oriented graph of the nodes in their topological order
    /// (following the outgoing connections).
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="removedEdges">Edges removed to make graph non-cyclic.</param>
    /// <returns>Sorting result.</returns>
    public static List<TNodeItem> Sort<TNodeItem, TConnectionItem>(IEnumerable<Node<TNodeItem, TConnectionItem>> nodes, out List<NodeConnection<TNodeItem, TConnectionItem>> removedEdges)
    {
      return Sort(nodes, out removedEdges, false);
    }

    /// <summary>
    /// Sorts the specified oriented graph of the nodes in their topological order
    /// (following the outgoing connections).
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="removedEdges">Edges removed to make graph non-cyclic.</param>
    /// <param name="removeWholeNode">If <see langword="true"/> removes whole node in the case of cycle, otherwise removes only one edge.</param>
    /// <returns>Sorting result.</returns>
    public static List<TNodeItem> Sort<TNodeItem, TConnectionItem>(IEnumerable<Node<TNodeItem, TConnectionItem>> nodes, out List<NodeConnection<TNodeItem, TConnectionItem>> removedEdges, bool removeWholeNode)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodes, "nodes");
      var head = new Queue<TNodeItem>();
      var tail = new Queue<TNodeItem>();
      removedEdges = new List<NodeConnection<TNodeItem, TConnectionItem>>();
      var nodeList = nodes.ToList();
      var nodesToRemove = new List<Node<TNodeItem, TConnectionItem>>();
      while (nodeList.Count > 0) {
        nodesToRemove.Clear();
        Node<TNodeItem, TConnectionItem> nodeToBreakCycle = null;
        NodeConnection<TNodeItem, TConnectionItem> edgeToBreakCycle = null;
        foreach (var node in nodeList) {
          if (node.IncomingConnectionCount == 0) {
            // Add to head
            head.Enqueue(node.Item);
            nodesToRemove.Add(node);
            var connections = node.OutgoingConnections.ToArray();
            foreach (var connection in connections)
              connection.UnbindFromNodes();
          }
          else if (node.OutgoingConnectionCount == 0) {
            // Add to tail
            tail.Enqueue(node.Item);
            nodesToRemove.Add(node);
            var connections = node.IncomingConnections.ToArray();
            foreach (var connection in connections)
              connection.UnbindFromNodes();
          }
          else {
            if (removeWholeNode) {
              if (node.PermanentOutgoingConnectionCount == 0 && (nodeToBreakCycle == null || node.OutgoingConnectionCount > nodeToBreakCycle.OutgoingConnectionCount))
                nodeToBreakCycle = node;
            }
            else {
              if (node.BreakableOutgoingConnectionCount > 0 && edgeToBreakCycle == null)
                edgeToBreakCycle = node.OutgoingConnections.First(connection => connection.ConnectionType == ConnectionType.Breakable);
            }
          }
        }
        if (nodesToRemove.Count == 0) {
          if (nodeToBreakCycle != null) {
            // Remove node
            var connections = nodeToBreakCycle.OutgoingConnections.ToArray();
            foreach (var connection in connections)
              connection.UnbindFromNodes();
            foreach (var connection in nodeToBreakCycle.IncomingConnections.ToArray())
              connection.UnbindFromNodes();
            removedEdges.AddRange(connections);
            tail.Enqueue(nodeToBreakCycle.Item);
            nodeList.Remove(nodeToBreakCycle);
          }
          else if (edgeToBreakCycle != null) {
            // remove edge
            removedEdges.Add(edgeToBreakCycle);
            edgeToBreakCycle.UnbindFromNodes();
          }
          else {
            throw new InvalidOperationException(Strings.ExOnlyBreakableNodesSadSmile);
          }
        }
        foreach (var nodeToRemove in nodesToRemove)
          nodeList.Remove(nodeToRemove);
      }
      return head.Concat(tail.Reverse()).ToList();
    }

    private static List<Node<TNodeItem, object>> GetNodes<TNodeItem>(IEnumerable<TNodeItem> items, Predicate<TNodeItem, TNodeItem> connector)
    {
      // Converting all the items to nodes
      var nodes = new List<Node<TNodeItem, object>>();
      foreach (var item in items)
        nodes.Add(new Node<TNodeItem, object>(item));
      // Adding connections
      foreach (var nodeA in nodes)
        foreach (var nodeB in nodes) {
          if (nodeA == nodeB)
            continue;
          if (connector.Invoke(nodeA.Item, nodeB.Item))
            nodeA.AddConnection(nodeB, null);
        }
      return nodes;
    }
  }
}