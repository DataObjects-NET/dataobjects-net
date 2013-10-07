// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2012.02.22

using System;


namespace Xtensive.Collections.Graphs
{
  /// <summary>
  /// A connection between two graph <see cref="Node"/>s with value.
  /// </summary>
  /// <typeparam name="TValue">Edge value.</typeparam>
  [Serializable]
  public class Edge<TValue> : Edge
  {
    /// <summary>
    /// Gets or sets edge value.
    /// </summary>
    public TValue Value { get; set; }

    /// <summary>
    /// Gets node value.
    /// </summary>
    public override object UntypedValue { get { return Value; } }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public Edge()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">Edge value.</param>
    public Edge(TValue value)
    {
      Value = value;
    }

    /// <summary>
    /// Creates new graph edge instance and <see cref="Edge.Attach"/>es it to nodes.
    /// </summary>
    /// <param name="source">Source node.</param>
    /// <param name="target">Target node.</param>
    /// <param name="value">Edge value.</param>
    public Edge(Node source, Node target, TValue value = default(TValue))
      : base(source, target)
    {
      Value = value;
    }
  }
}