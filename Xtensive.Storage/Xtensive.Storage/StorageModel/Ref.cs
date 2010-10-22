// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Storage.StorageModel
{
  /// <summary>
  /// The reference to model node.
  /// </summary>
  /// <typeparam name="TTarget">The type of the target node.</typeparam>
  /// <typeparam name="TParent">The type of the parent node.</typeparam>
  [Serializable]
  public abstract class Ref<TTarget, TParent> : NodeBase<TParent>,
    IUnnamedNode,
    INodeReference
    where TTarget : Node
    where TParent : Node
  {
    private TTarget value;

    /// <summary>
    /// Gets or sets referenced node.
    /// </summary>
    /// <exception cref="NotSupportedException">Value is already initialized.</exception>
    [Property(Priority = 0)]
    public TTarget Value
    {
      get { return value; }
      set
      {
//        if (this.value!=null)
//          throw Exceptions.AlreadyInitialized("Value");
        EnsureIsEditable();
        using (var scope = LogPropertyChange("Value", value)) {
          this.value = value;
          scope.Commit();
        }
      }
    }

    Node INodeReference.Value
    {
      get { return Value; }
      set { Value = (TTarget)value; }
    }


    // Constructors

    /// <inheritdoc/>
    protected Ref(TParent parent)
      : base(parent, null)
    {
    }
  }
}