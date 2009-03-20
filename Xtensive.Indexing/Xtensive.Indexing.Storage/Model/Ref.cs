// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Modelling;
using System.Diagnostics;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a reference to model node.
  /// </summary>
  /// <typeparam name="TTarget">The type of the target node.</typeparam>
  /// <typeparam name="TParent">The type of the parent node.</typeparam>
  [Serializable]
  public abstract class Ref<TTarget, TParent> : NodeBase<TParent>
    where TTarget : Node
    where TParent : Node
  {
    private TTarget value;

    /// <summary>
    /// Gets or sets referenced node.
    /// </summary>
    [Property]
    public TTarget Value
    {
      get { return value; }
      set {
        if (this.value!=null)
          throw Exceptions.AlreadyInitialized("Value");
        EnsureIsEditable();
        this.value = value;
      }
    }


    //Constructors

    protected Ref(TParent parent, string name)
      : base(parent, name)
    {
    }

    protected Ref(TParent parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}