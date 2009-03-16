// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Abstract base class for node with specified <see cref="Node.Parent"/> type.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  [Serializable]
  public abstract class Node<TParent> : Node
    where TParent : Node
  {
    /// <summary>
    /// Gets the parent node.
    /// </summary>
    public new TParent Parent
    { 
      [DebuggerStepThrough]
      get { return base.Parent as TParent; }
    }
    

    // Constructors

    /// <inheritdoc/>
    protected Node(TParent parent, string name)
      : base(parent, name)
    {
    }

    /// <inheritdoc/>
    protected Node(string name)
      : base(name)
    {
    }

    /// <inheritdoc/>
    protected Node()
    {
    }
  }
}