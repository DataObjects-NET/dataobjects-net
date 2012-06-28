// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2012.02.22

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections.Graphs
{
  /// <summary>
  /// Graph node with value. 
  /// </summary>
  /// <typeparam name="TValue">Node value.</typeparam>
  [Serializable]
  public class Node<TValue> : Node
  {
    /// <summary>
    /// Node value.
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
    public Node()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">Node value.</param>
    public Node(TValue value)
    {
      Value = value;
    }
  }
}