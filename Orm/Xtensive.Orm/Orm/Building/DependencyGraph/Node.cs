// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.12

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Building.DependencyGraph
{
  internal class Node<TValue>
  {
    public TValue Value { get; }

    public HashSet<Edge<TValue>> OutgoingEdges { get; }

    public HashSet<Edge<TValue>> IncomingEdges { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Value.ToString();
    }


    // Constructors

    public Node(TValue value)
    {
      Value = value;
      OutgoingEdges = new HashSet<Edge<TValue>>();
      IncomingEdges = new HashSet<Edge<TValue>>();
    }
  }
}