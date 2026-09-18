// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2012.02.22

using System;


namespace Xtensive.Collections.Graphs
{
  /// <summary>
  /// Graph node with value. 
  /// </summary>
  /// <typeparam name="TValue">Node value.</typeparam>
  public class Node<TValue> : Node
  {
    /// <summary>
    /// Node value.
    /// </summary>
    public TValue Value { get; set; }

    /// <summary>
    /// Gets node value.
    /// </summary>
    public override object UntypedValue => Value;


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public Node()
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="value">Node value.</param>
    public Node(TValue value)
    {
      Value = value;
    }
  }
}