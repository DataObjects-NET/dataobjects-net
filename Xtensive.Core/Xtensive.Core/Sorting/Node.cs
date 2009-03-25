// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.13

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Sorting
{
  /// <summary>
  /// Sorting node. 
  /// </summary>
  /// <typeparam name="TNodeItem"></typeparam>
  /// <typeparam name="TConnectionItem"></typeparam>
  [Serializable]
  public class Node<TNodeItem, TConnectionItem>
  {
    private HashSet<NodeConnection<TNodeItem, TConnectionItem>> incomingConnections;
    private HashSet<NodeConnection<TNodeItem, TConnectionItem>> outgoingConnections;

    public TNodeItem Item { get; private set; }

    public HashSet<NodeConnection<TNodeItem, TConnectionItem>> IncomingConnections
    {
      get
      {
        if (incomingConnections==null)
          incomingConnections = new HashSet<NodeConnection<TNodeItem, TConnectionItem>>();
        return incomingConnections;
      }
    }

    public HashSet<NodeConnection<TNodeItem, TConnectionItem>> OutgoingConnections
    {
      get
      {
        if (outgoingConnections==null)
          outgoingConnections = new HashSet<NodeConnection<TNodeItem, TConnectionItem>>();
        return outgoingConnections;
      }
    }

    public int GetConnectionCount(bool outgoing)
    {
      if (outgoing)
        return outgoingConnections==null ? 0 : outgoingConnections.Count;
      else
        return incomingConnections==null ? 0 : incomingConnections.Count;
    }

    public void AddConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      if (!connection.Source.OutgoingConnections.Contains(connection))
        connection.Source.OutgoingConnections.Add(connection);
      if (!connection.Destination.IncomingConnections.Contains(connection))
        connection.Destination.IncomingConnections.Add(connection);
    }

    public void AddConnection(Node<TNodeItem, TConnectionItem> node, bool outgoing, TConnectionItem connectionItem)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      AddConnection(new NodeConnection<TNodeItem, TConnectionItem>(outgoing ? this : node, outgoing ? node : this, connectionItem));
    }

    public void RemoveConnection(NodeConnection<TNodeItem, TConnectionItem> connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      connection.Source.OutgoingConnections.Remove(connection);
      connection.Destination.IncomingConnections.Remove(connection);
    }

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

    public Node(TNodeItem item)
    {
      Item = item;
    }
  }
}