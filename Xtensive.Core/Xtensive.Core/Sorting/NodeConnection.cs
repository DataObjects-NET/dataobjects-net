// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.13

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Sorting
{
  /// <summary>
  /// Connection between two <see cref="Node{TNodeItem,TConnectionItem}"/>s.
  /// </summary>
  /// <typeparam name="TNodeItem">Type of node item.</typeparam>
  /// <typeparam name="TConnectionItem">Type of connection item.</typeparam>
  [Serializable]
  public class NodeConnection<TNodeItem, TConnectionItem> : 
    IEquatable<NodeConnection<TNodeItem, TConnectionItem>>
  {
    private readonly TConnectionItem connectionItem;
    private readonly Node<TNodeItem, TConnectionItem> source;
    private readonly Node<TNodeItem, TConnectionItem> destination;

    /// <summary>
    /// Gets connection item.
    /// </summary>
    public TConnectionItem ConnectionItem {
      get { return connectionItem; }
    }

    /// <summary>
    /// Gets connection source.
    /// </summary>
    public Node<TNodeItem, TConnectionItem> Source {
      get { return source; }
    }

    /// <summary>
    /// Gets connection destination.
    /// </summary>
    public Node<TNodeItem, TConnectionItem> Destination {
      get { return destination; }
    }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(NodeConnection<TNodeItem, TConnectionItem> obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj.connectionItem, connectionItem) && 
        Equals(obj.source, source) && 
          Equals(obj.destination, destination);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (Equals(this, obj))
        return true;
      if (obj.GetType()!=typeof (NodeConnection<TNodeItem, TConnectionItem>))
        return false;
      return Equals((NodeConnection<TNodeItem, TConnectionItem>) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = ReferenceEquals(null, connectionItem) ? 0 : connectionItem.GetHashCode();
        result = (result * 397) ^ (source!=null ? source.GetHashCode() : 0);
        result = (result * 397) ^ (destination!=null ? destination.GetHashCode() : 0);
        return result;
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="connectionItem">The connection item.</param>
    public NodeConnection(Node<TNodeItem, TConnectionItem> source, Node<TNodeItem, TConnectionItem> destination, TConnectionItem connectionItem)
    {
      this.connectionItem = connectionItem;
      this.source = source;
      this.destination = destination;
    }  
  }
}