// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Sorting
{
  /// <summary>
  /// Sorting node. 
  /// </summary>
  /// <typeparam name="TNodeItem">Type of node item.</typeparam>
  /// <typeparam name="TConnectionItem">Type of connection item.</typeparam>
  [Serializable]
  public class Node<TNodeItem, TConnectionItem>
  {
    private List<NodeConnection<TNodeItem, TConnectionItem>> incomingConnections;
    private ReadOnlyList<NodeConnection<TNodeItem, TConnectionItem>> incomingConnectionsReadOnlyList;
    private List<NodeConnection<TNodeItem, TConnectionItem>> outgoingConnections;
    private ReadOnlyList<NodeConnection<TNodeItem, TConnectionItem>> outgoingConnectionsReadOnlyList;

    /// <summary>
    /// Gets node item.
    /// </summary>
    public TNodeItem Item { get; private set; }

    /// <summary>
    /// Gets <see cref="HashSet{T}"/> of incoming connections.
    /// </summary>
    public ReadOnlyList<NodeConnection<TNodeItem, TConnectionItem>> IncomingConnections {
      get
      {
        EnsureIncomingConnections();
        return incomingConnectionsReadOnlyList;
      }
    }

    /// <summary>
    /// Gets <see cref="HashSet{T}"/> of outgoing connections.
    /// </summary>
    public ReadOnlyList<NodeConnection<TNodeItem, TConnectionItem>> OutgoingConnections {
      get
      {
        EnsureOutgoingConnections();
        return outgoingConnectionsReadOnlyList;
      }
    }

    /// <summary>
    /// Gets count of outgoing connections.
    /// </summary>
    public int OutgoingConnectionCount { get{ return outgoingConnections==null ? 0 : outgoingConnections.Count;} }

    /// <summary>
    /// Gets weight of outgoing connections.
    /// </summary>
    public int OutgoingConnectionWeight { get; private set; }

    /// <summary>
    /// Gets count of incoming connections.
    /// </summary>
    public int IncomingConnectionCount { get{ return incomingConnections==null ? 0 : incomingConnections.Count;} }


    /// <summary>
    /// Gets weight of incoming connections.
    /// </summary>
    public int IncomingConnectionWeight { get; private set; }

    internal void AddOutgoingConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      EnsureOutgoingConnections();
      outgoingConnections.Add(connection);
      OutgoingConnectionWeight += connection.Weight;
    }

    internal void AddIncomingConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      EnsureIncomingConnections();
      incomingConnections.Add(connection);
      IncomingConnectionWeight += connection.Weight;
    }

    internal void RemoveOutgoingConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      if (OutgoingConnectionCount>0) {
        outgoingConnections.Remove(connection);
        OutgoingConnectionWeight -= connection.Weight;
      }
    }

    internal void RemoveIncomingConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      if (IncomingConnectionCount>0) {
        incomingConnections.Remove(connection);
        IncomingConnectionWeight -= connection.Weight;
      }
    }

    /// <summary>
    /// Adds new outgoing connection to node.
    /// </summary>
    /// <param name="node">Paired node.</param>
    /// <param name="connectionItem">Item of connection.</param>
    public NodeConnection<TNodeItem, TConnectionItem> AddConnection(Node<TNodeItem, TConnectionItem> node, TConnectionItem connectionItem)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      var connection = new NodeConnection<TNodeItem, TConnectionItem>(this, node, connectionItem);
      connection.BindToNodes();
      return connection;
    }

    /// <summary>
    /// Adds new outgoing connection to node.
    /// </summary>
    /// <param name="node">Paired node.</param>
    /// <param name="connectionItem">Item of connection.</param>
    /// <param name="weight">Item weight.</param>
    public NodeConnection<TNodeItem, TConnectionItem> AddConnection(Node<TNodeItem, TConnectionItem> node, TConnectionItem connectionItem, int weight)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      var connection = new NodeConnection<TNodeItem, TConnectionItem>(this, node, connectionItem, weight);
      connection.BindToNodes();
      return connection;
    }

    /// <summary>
    /// Removes outgoing connections from node.
    /// </summary>
    /// <param name="destination">Paired node.</param>
    public IEnumerable<NodeConnection<TNodeItem, TConnectionItem>> RemoveConnections(Node<TNodeItem, TConnectionItem> destination)
    {
      ArgumentValidator.EnsureArgumentNotNull(destination, "destination");
      if (OutgoingConnectionCount==0)
        return EnumerableUtils<NodeConnection<TNodeItem, TConnectionItem>>.Empty;

      var nodesToRemove = outgoingConnections.Where(connection => connection.Destination==destination).ToList();
      nodesToRemove.Apply(nodeConnection=>nodeConnection.UnbindFromNodes());
      return nodesToRemove;
    }


    // Private

    private void EnsureIncomingConnections()
    {
      if (incomingConnectionsReadOnlyList==null) {
        incomingConnections = new List<NodeConnection<TNodeItem, TConnectionItem>>();
        incomingConnectionsReadOnlyList = new ReadOnlyList<NodeConnection<TNodeItem, TConnectionItem>>(incomingConnections);
      }
    }

    private void EnsureOutgoingConnections()
    {
      if (outgoingConnectionsReadOnlyList==null) {
        outgoingConnections = new List<NodeConnection<TNodeItem, TConnectionItem>>();
        outgoingConnectionsReadOnlyList = new ReadOnlyList<NodeConnection<TNodeItem, TConnectionItem>>(outgoingConnections);
      }
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="item">The item.</param>
    public Node(TNodeItem item)
    {
      Item = item;
    }
  }
}