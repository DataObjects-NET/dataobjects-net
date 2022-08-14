// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.13

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Core;


namespace Xtensive.Sorting
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
    private ReadOnlyCollection<NodeConnection<TNodeItem, TConnectionItem>> incomingConnectionsReadOnlyList;
    private List<NodeConnection<TNodeItem, TConnectionItem>> outgoingConnections;
    private ReadOnlyCollection<NodeConnection<TNodeItem, TConnectionItem>> outgoingConnectionsReadOnlyList;

    /// <summary>
    /// Gets node item.
    /// </summary>
    public TNodeItem Item { get; private set; }

    /// <summary>
    /// Gets <see cref="HashSet{T}"/> of incoming connections.
    /// </summary>
    public IReadOnlyList<NodeConnection<TNodeItem, TConnectionItem>> IncomingConnections {
      get
      {
        EnsureIncomingConnections();
        return incomingConnectionsReadOnlyList;
      }
    }

    /// <summary>
    /// Gets <see cref="HashSet{T}"/> of outgoing connections.
    /// </summary>
    public IReadOnlyList<NodeConnection<TNodeItem, TConnectionItem>> OutgoingConnections {
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
    /// Gets count of breakable outgoing connections.
    /// </summary>
    public int BreakableOutgoingConnectionCount { get{ return outgoingConnections==null ? 0 : outgoingConnections.Count - PermanentOutgoingConnectionCount;} }

    /// <summary>
    /// Gets count of permanent outgoing connections.
    /// </summary>
    public int PermanentOutgoingConnectionCount { get; private set; }

    /// <summary>
    /// Gets count of breakable incoming connections.
    /// </summary>
    public int BreakableIncomingConnectionCount { get{ return incomingConnections==null ? 0 : incomingConnections.Count - PermanentIncomingConnectionCount;} }

    /// <summary>
    /// Gets count of breakable incoming connections.
    /// </summary>
    public int IncomingConnectionCount { get{ return incomingConnections==null ? 0 : incomingConnections.Count;} }

    /// <summary>
    /// Gets count of permanent incoming connections.
    /// </summary>
    public int PermanentIncomingConnectionCount { get; private set; }

    internal void AddOutgoingConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      EnsureOutgoingConnections();
      outgoingConnections.Add(connection);
      if (connection.ConnectionType==ConnectionType.Permanent) 
        PermanentOutgoingConnectionCount ++;
    }

    internal void AddIncomingConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      EnsureIncomingConnections();
      incomingConnections.Add(connection);
      if (connection.ConnectionType==ConnectionType.Permanent) 
        PermanentIncomingConnectionCount ++;
    }

    internal void RemoveOutgoingConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      if (outgoingConnections!=null) {
        outgoingConnections.Remove(connection);
      if (connection.ConnectionType==ConnectionType.Permanent) 
        PermanentOutgoingConnectionCount --;
      }
    }

    internal void RemoveIncomingConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      if (incomingConnections!=null) {
        incomingConnections.Remove(connection);
      if (connection.ConnectionType==ConnectionType.Permanent) 
        PermanentIncomingConnectionCount --;
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
    /// <param name="connectionType">Type of connection.</param>
    public NodeConnection<TNodeItem, TConnectionItem> AddConnection(Node<TNodeItem, TConnectionItem> node, TConnectionItem connectionItem, ConnectionType connectionType)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      var connection = new NodeConnection<TNodeItem, TConnectionItem>(this, node, connectionItem, connectionType);
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
      if (outgoingConnections==null)
        return Enumerable.Empty<NodeConnection<TNodeItem, TConnectionItem>>();

      var nodesToRemove = outgoingConnections.Where(connection => connection.Destination==destination).ToList();
      EnumerableExtensions.ForEach(nodesToRemove, nodeConnection=>nodeConnection.UnbindFromNodes());
      return nodesToRemove;
    }


    // Private

    private void EnsureIncomingConnections()
    {
      if (incomingConnectionsReadOnlyList==null) {
        incomingConnections = new List<NodeConnection<TNodeItem, TConnectionItem>>();
        incomingConnectionsReadOnlyList = incomingConnections.AsReadOnly();
      }
    }

    private void EnsureOutgoingConnections()
    {
      if (outgoingConnectionsReadOnlyList==null) {
        outgoingConnections = new List<NodeConnection<TNodeItem, TConnectionItem>>();
        outgoingConnectionsReadOnlyList = outgoingConnections.AsReadOnly();
      }
    }

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="item">The item.</param>
    public Node(TNodeItem item)
    {
      Item = item;
    }
  }
}