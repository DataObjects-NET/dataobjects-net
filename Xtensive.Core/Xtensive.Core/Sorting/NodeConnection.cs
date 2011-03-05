// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.13

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Sorting
{
  /// <summary>
  /// Connection between two <see cref="Node{TNodeItem,TConnectionItem}"/>s.
  /// </summary>
  /// <typeparam name="TNodeItem">Type of node item.</typeparam>
  /// <typeparam name="TConnectionItem">Type of connection item.</typeparam>
  [Serializable]
  public class NodeConnection<TNodeItem, TConnectionItem>
  {
    /// <summary>
    /// Gets connection item.
    /// </summary>
    public TConnectionItem ConnectionItem { get; private set; }

    /// <summary>
    /// Gets connection source.
    /// </summary>
    public Node<TNodeItem, TConnectionItem> Source { get; private set; }

    /// <summary>
    /// Gets connection destination.
    /// </summary>
    public Node<TNodeItem, TConnectionItem> Destination { get; private set; }
    
    /// <summary>
    /// Gets connection type.
    /// </summary>
    public ConnectionType ConnectionType { get; private set; }

    public bool IsBinded { get; private set; }

    public void BindToNodes()
    {
      if (!IsBinded) {
        Source.AddOutgoingConnection(this);
        Destination.AddIncomingConnection(this);
        IsBinded = true;
      }
    }

    public void UnbindFromNodes()
    {
      if (IsBinded) {
        Source.RemoveOutgoingConnection(this);
        Destination.RemoveIncomingConnection(this);
        IsBinded = false;
      }
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="connectionItem">The connection item.</param>
    public NodeConnection(Node<TNodeItem, TConnectionItem> source, Node<TNodeItem, TConnectionItem> destination, TConnectionItem connectionItem)
      : this(source, destination, connectionItem, ConnectionType.Breakable)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="connectionItem">The connection item.</param>
    /// <param name="connectionType">Connection type.</param>
    public NodeConnection(Node<TNodeItem, TConnectionItem> source, Node<TNodeItem, TConnectionItem> destination, TConnectionItem connectionItem, ConnectionType connectionType)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(destination, "destination");
      ConnectionItem = connectionItem;
      Source = source;
      Destination = destination;
      ConnectionType = connectionType;
    }
  }
}