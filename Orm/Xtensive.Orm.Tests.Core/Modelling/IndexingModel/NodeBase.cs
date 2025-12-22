// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{

  /// <summary>
  /// A base class for all nodes in storage model.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent node.</typeparam>
  [Serializable]
  public abstract class NodeBase<TParent> : Node<TParent, StorageInfo> 
    where TParent : Node
  {
    /// <inheritdoc/>
    protected override void PerformCreate()
    {
      base.PerformCreate();
      TestLog.Info($"Created: {this}");
    }

    /// <inheritdoc/>
    protected override void PerformMove(Node newParent, string newName, int newIndex)
    {
      using (TestLog.InfoRegion($"Moving: {this}")) {
        if (Parent!=newParent)
          TestLog.Info($"new Parent={newParent}");
        if (Name!=newName)
          TestLog.Info($"new Name={newName}");
        if (Index!=newIndex)
          TestLog.Info($"new Index={newIndex}");
        base.PerformMove(newParent, newName, newIndex);
      }
    }

    /// <inheritdoc/>
    protected override void PerformShift(int offset)
    {
      TestLog.Info($"Shifting: {this}, from {Index} to {Index + offset}");
      base.PerformShift(offset);
    }

    /// <inheritdoc/>
    protected override void ValidateRemove()
    {
      base.ValidateRemove();
      TestLog.Info($"Removed: {this}");
    }

    
    //Constructors

    protected NodeBase(TParent parent, string name)
      : base(parent, name)
    {
    }
  }
}