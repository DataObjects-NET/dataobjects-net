// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.20

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Typed node collection implementation.
  /// </summary>
  /// <typeparam name="TNode">The type of the node.</typeparam>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  /// <typeparam name="TModel">The type of the model.</typeparam>
  [Serializable]
  public abstract class NodeCollection<TNode, TParent, TModel> : NodeCollection,
    INodeCollection<TNode>
    where TNode : Node
    where TParent : Node
    where TModel : class, IModel
  {
    /// <summary>
    /// Gets the parent node.
    /// </summary>
    public new TParent Parent { 
      [DebuggerStepThrough]
      get { return base.Parent as TParent; }
    }

    /// <summary>
    /// Gets the model this node belongs to.
    /// </summary>
    public new TModel Model {
      [DebuggerStepThrough]
      get { return base.Model as TModel; }
    }

    /// <inheritdoc/>
    public new TNode this[int index] {
      [DebuggerStepThrough]
      get { return (TNode) base[index]; }
    }

    /// <inheritdoc/>
    public new TNode this[string name] {
      [DebuggerStepThrough]
      get { return (TNode) base[name]; }
    }

    /// <inheritdoc/>
    public new IEnumerator<TNode> GetEnumerator() 
    {
      foreach (var node in (this as IEnumerable))
        yield return (TNode) node;
    }


    // Constructors

    /// <inheritdoc/>
    protected NodeCollection(Node parent, string name)
      : base(parent, name)
    {
    }
  }
}