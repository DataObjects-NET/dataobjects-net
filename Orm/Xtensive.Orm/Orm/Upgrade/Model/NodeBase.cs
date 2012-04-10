// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// A base class for all nodes in storage model.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent node.</typeparam>
  [Serializable]
  public abstract class NodeBase<TParent> : Node<TParent, StorageModel>
    where TParent : Node
  {
//    
//    protected override void PerformCreate()
//    {
//      base.PerformCreate();
//      Log.Info("Created: {0}", this);
//    }
//
//    
//    protected override void PerformMove(Node newParent, string newName, int newIndex)
//    {
//      using (Log.InfoRegion("Moving: {0}", this))
//      {
//        if (Parent != newParent)
//          Log.Info("new Parent={0}", newParent);
//        if (Name != newName)
//          Log.Info("new Name={0}", newName);
//        if (Index != newIndex)
//          Log.Info("new Index={0}", newIndex);
//        base.PerformMove(newParent, newName, newIndex);
//      }
//    }
//
//    
//    protected override void PerformShift(int offset)
//    {
//      Log.Info("Shifting: {0}, from {1} to {2}", this, Index, Index + offset);
//      base.PerformShift(offset);
//    }
//
//    
//    protected override void ValidateRemove()
//    {
//      base.ValidateRemove();
//      Log.Info("Removed: {0}", this);
//    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="name">The name.</param>
    protected NodeBase(TParent parent, string name)
      : base(parent, name)
    {
    }
  }
}