// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.20

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public abstract class Ref<TTarget, TParent> : NodeBase<TParent>,
    INodeReference
    where TTarget : Node
    where TParent : Node
  {
    private TTarget value;

    [Property]
    public TTarget Value {
      get { return value; }
      set {
        if (this.value!=null)
          throw Exceptions.AlreadyInitialized("Value");
        EnsureIsEditable();
        this.value = value;
      }
    }

    Node INodeReference.Value {
      get { return Value; }
    }


    public Ref(TParent parent, string name)
      : base(parent, name)
    {
    }

    public Ref(TParent parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}