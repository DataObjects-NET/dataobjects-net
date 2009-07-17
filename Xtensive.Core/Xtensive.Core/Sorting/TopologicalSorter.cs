// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.07

using System.Collections.Generic;
using Xtensive.Core.Collections;
using System.Linq;

namespace Xtensive.Core.Sorting
{
  /// <summary>
  /// Topological sorter for oriented graph of the items.
  /// </summary>
  public static class TopologicalSorter
  {
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
    public static List<TNodeItem> Sort<TNodeItem>(IEnumerable<TNodeItem> items, Predicate<TNodeItem, TNodeItem> connector)
    {
      List<Node<TNodeItem, object>> loops;
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
    public static List<TNodeItem> Sort<TNodeItem>(IEnumerable<TNodeItem> items, Predicate<TNodeItem, TNodeItem> connector, out List<Node<TNodeItem, object>> loops)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      ArgumentValidator.EnsureArgumentNotNull(connector, "connector");
      List<Node<TNodeItem, object>> nodes = GetNodes(items, connector);
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
    public static List<TNodeItem> Sort<TNodeItem>(IEnumerable<TNodeItem> items, Predicate<TNodeItem, TNodeItem> connector, out List<NodeConnection<TNodeItem, object>> removedEdges)
    {
      return Sort<TNodeItem>(items, connector, out removedEdges, false);
    }

    /// <summary>
    /// Sorts the specified oriented graph of the items in their topological order
    /// (following the outgoing connections provided by <paramref name="connector"/>).
    /// </summary>
    /// <param name="items">The items to sort.</param>
    /// <param name="connector">The connector delegate returning <see langword="true"/>
    /// if there is outgoing connection between the first and the second node.</param>
    /// <param name="removedEdges">Edges removed to make graph non-cyclic.</param>
    /// <param name="removeWholeNode">If <see langword="true"/> removes whole node in the case of loop, otherwise removes only one edge.</param>
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
    /// <param name="loops">The loops, if found.</param>
    /// <returns>Sorting result, if there were no loops;
    /// otherwise, <see langword="null" />. 
    /// In this case <paramref name="nodes"/> will contain only the loop edges.</returns>
    public static List<TNodeItem> Sort<TNodeItem, TConnectionItem>(List<Node<TNodeItem, TConnectionItem>> nodes, out List<Node<TNodeItem, TConnectionItem>> loops)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodes, "nodes");
      var head = new Queue<TNodeItem>();
      var tail = new Queue<TNodeItem>();
      var nodeList = nodes.ToList();
      var nodesToRemove = new List<Node<TNodeItem, TConnectionItem>>();
      while (nodeList.Count > 0) {
        nodesToRemove.Clear();
        foreach (var node in nodeList) {
          if (node.GetConnectionCount(false)==0) {
            // Add to head
            head.Enqueue(node.Item);
            nodesToRemove.Add(node);
            var connections = node.OutgoingConnections.ToArray();
            foreach (var connection in connections)
              node.RemoveConnection(connection);
          }
          else if (node.GetConnectionCount(true)==0) {
            // Add to tail
            tail.Enqueue(node.Item);
            nodesToRemove.Add(node);
            var connections = node.IncomingConnections.ToArray();
            foreach (var connection in connections)
              node.RemoveConnection(connection);
          }
        }
        if (nodesToRemove.Count==0) {
          loops = new List<Node<TNodeItem, TConnectionItem>>(nodeList);
          return null;
        }
        foreach (var nodeToRemove in nodesToRemove)
          nodeList.Remove(nodeToRemove);
      }
      loops = null;
      return head.Concat(tail.Reverse()).ToList();
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
    /// <param name="removeWholeNode">If <see langword="true"/> removes whole node in the case of loop, otherwise removes only one edge.</param>
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
        Node<TNodeItem, TConnectionItem> nodeToBreakLoop = null;
        foreach (var node in nodeList) {
          if (node.GetConnectionCount(false)==0) {
            // Add to head
            head.Enqueue(node.Item);
            nodesToRemove.Add(node);
            var connections = node.OutgoingConnections.ToArray();
            foreach (var connection in connections)
              node.RemoveConnection(connection);
          }
          else if (node.GetConnectionCount(true)==0) {
            // Add to tail
            tail.Enqueue(node.Item);
            nodesToRemove.Add(node);
            var connections = node.IncomingConnections.ToArray();
            foreach (var connection in connections)
              node.RemoveConnection(connection);
          }
          else {
            if (removeWholeNode && (nodeToBreakLoop==null || node.GetConnectionCount(true) > nodeToBreakLoop.GetConnectionCount(true)))
              nodeToBreakLoop = node;
          }
        }
        if (nodesToRemove.Count==0) {
          if (nodeToBreakLoop!=null) {
            var connections = nodeToBreakLoop.OutgoingConnections.ToArray();
            foreach (var connection in connections)
              nodeToBreakLoop.RemoveConnection(connection);
            foreach (var connection in nodeToBreakLoop.IncomingConnections.ToArray())
              nodeToBreakLoop.RemoveConnection(connection);
            removedEdges.AddRange(connections);
            tail.Enqueue(nodeToBreakLoop.Item);
            nodeList.Remove(nodeToBreakLoop);
          }
          else {
            // remove edge
            var removedConnection = nodeList[0].OutgoingConnections.First();
            removedEdges.Add(removedConnection);
            nodeList[0].RemoveConnection(removedConnection);
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
          if (nodeA==nodeB)
            continue;
          if (connector.Invoke(nodeA.Item, nodeB.Item))
            nodeA.AddConnection(nodeB, true, null);
        }
      return nodes;
    }
  }
}