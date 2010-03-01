// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.12

using System;
using System.Collections.Generic;

namespace Xtensive.Storage.Building.DependencyGraph
{
  [Serializable]
  public class Node<TValue>
  {
    internal TValue Value { get; private set; }

    internal HashSet<Edge<TValue>> OutgoingEdges { get; private set; }

    internal HashSet<Edge<TValue>> IncomingEdges { get; private set; }

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