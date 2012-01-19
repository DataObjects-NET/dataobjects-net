// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.20

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Core.Tests.Modelling.DatabaseModel
{
  [Serializable]
  public abstract class Ref<TTarget, TParent> : NodeBase<TParent>,
    INodeReference
    where TTarget : Node
    where TParent : Node
  {
    private TTarget value;

    [Property(IsImmutable = true)]
    public TTarget Value {
      get { return value; }
      set {
        ChangeProperty("Value", value, (x,v) => ((Ref<TTarget, TParent>)x).value = v);
      }
    }

    Node INodeReference.Value {
      get { return Value; }
      set { Value = (TTarget) value; }
    }


    public Ref(TParent parent, string name)
      : base(parent, name)
    {
    }
  }
}