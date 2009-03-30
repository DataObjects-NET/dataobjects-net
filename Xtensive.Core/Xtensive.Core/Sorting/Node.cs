// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.13

using System;
using System.Collections.Generic;
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
    private HashSet<NodeConnection<TNodeItem, TConnectionItem>> incomingConnections;
    private HashSet<NodeConnection<TNodeItem, TConnectionItem>> outgoingConnections;

    /// <summary>
    /// Gets node item.
    /// </summary>
    public TNodeItem Item { get; private set; }

    /// <summary>
    /// Gets <see cref="HashSet{T}"/> of incoming connections.
    /// </summary>
    public HashSet<NodeConnection<TNodeItem, TConnectionItem>> IncomingConnections {
      get {
        if (incomingConnections==null)
          incomingConnections = new HashSet<NodeConnection<TNodeItem, TConnectionItem>>();
        return incomingConnections;
      }
    }

    /// <summary>
    /// Gets <see cref="HashSet{T}"/> of outgoing connections.
    /// </summary>
    public HashSet<NodeConnection<TNodeItem, TConnectionItem>> OutgoingConnections {
      get {
        if (outgoingConnections==null)
          outgoingConnections = new HashSet<NodeConnection<TNodeItem, TConnectionItem>>();
        return outgoingConnections;
      }
    }

    /// <summary>
    /// Gets count of connections.
    /// </summary>
    /// <param name="outgoing">If <see langword="True" />, gets count for outgoing connections, otherwise gets count for incoming connections.</param>
    /// <returns>Count of corresponding connections.</returns>
    public int GetConnectionCount(bool outgoing)
    {
      if (outgoing)
        return outgoingConnections==null ? 0 : outgoingConnections.Count;
      else
        return incomingConnections==null ? 0 : incomingConnections.Count;
    }

    /// <summary>
    /// Adds new connection to node.
    /// </summary>
    /// <param name="connection">Connection to add to node.</param>
    public void AddConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      if (!connection.Source.OutgoingConnections.Contains(connection))
        connection.Source.OutgoingConnections.Add(connection);
      if (!connection.Destination.IncomingConnections.Contains(connection))
        connection.Destination.IncomingConnections.Add(connection);
    }

    /// <summary>
    /// Adds new connection to node.
    /// </summary>
    /// <param name="node">Paired node.</param>
    /// <param name="outgoing">If <see langword="True" />, adds outgoing connection, otherwise adds incoming connection.</param>
    /// <param name="connectionItem">Item of connection.</param>
    public void AddConnection(Node<TNodeItem, TConnectionItem> node, bool outgoing, TConnectionItem connectionItem)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      AddConnection(new NodeConnection<TNodeItem, TConnectionItem>(outgoing ? this : node, outgoing ? node : this, connectionItem));
    }

    /// <summary>
    /// Removes connection from node.
    /// </summary>
    /// <param name="connection">Connection to remove from node.</param>
    public void RemoveConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      connection.Source.OutgoingConnections.Remove(connection);
      connection.Destination.IncomingConnections.Remove(connection);
    }

    /// <summary>
    /// Removes connection from node.
    /// </summary>
    /// <param name="node">Paired node.</param>
    /// <param name="outgoing">If <see langword="True" />, removes outgoing connection, otherwise removes incoming connection.</param>
    public void RemoveConnection(Node<TNodeItem, TConnectionItem> node, bool outgoing)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      Node<TNodeItem, TConnectionItem> a, b;
      if (outgoing) {
        a = this;
        b = node;
      }
      else {
        a = node;
        b = this;
      }
      if (a.OutgoingConnections==null)
        return;
      if (b.IncomingConnections==null)
        return;
      a.OutgoingConnections.RemoveWhere(connection => connection.Destination.Equals(b));
      b.IncomingConnections.RemoveWhere(connection => connection.Destination.Equals(a));
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