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
  public abstract class Ref<TTarget, TParent> : NodeBase<TParent>, 
    IUnnamedNode
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
      set
      {
        if (this.value!=null)
          throw Core.Exceptions.AlreadyInitialized("Value");
        EnsureIsEditable();
        using (var scope = LogChange("Value", value)) {
          this.value = value;
          scope.Commit();
        }
      }
    }


    //Constructors

    protected Ref(TParent parent, int index)
      : base(parent, null, index)
    {
    }
  }
}