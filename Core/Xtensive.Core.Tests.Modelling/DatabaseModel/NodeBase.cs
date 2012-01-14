// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.20

using System;
using System.Diagnostics;
using Xtensive.Modelling;
using Xtensive.Reflection;
using Xtensive.Modelling.Attributes;
using Xtensive.Core;

namespace Xtensive.Core.Tests.Modelling.DatabaseModel
{
  [Serializable]
  public abstract class NodeBase<TParent> : Node<TParent, Server>
    where TParent : Node
  {
    protected override void PerformCreate()
    {
      base.PerformCreate();
      Log.Info("Created: {0}", this);
    }

    protected override void PerformMove(Node newParent, string newName, int newIndex)
    {
      using (Log.InfoRegion("Moving: {0}", this)) {
        if (Parent!=newParent)
          Log.Info("new Parent={0}", newParent);
        if (Name!=newName)
          Log.Info("new Name={0}", newName);
        if (Index!=newIndex)
          Log.Info("new Index={0}", newIndex);
        base.PerformMove(newParent, newName, newIndex);
      }
    }

    protected override void PerformShift(int offset)
    {
      Log.Info("Shifting: {0}, from {1} to {2}", this, Index, Index + offset);
      base.PerformShift(offset);
    }

    protected override void PerformRemove(Node source)
    {
      base.PerformRemove(source);
      if (source==this)
        Log.Info("Removed: {0}", this);
    }

    protected override void OnPropertyChanged(string name)
    {
      base.OnPropertyChanged(name);
      var accessor = PropertyAccessors.GetValueOrDefault(name);
      if (accessor!=null) {
        if (!accessor.HasGetter)
          return;
        if (accessor.IsSystem)
          return;
        Log.Info("Changed: {0}, {1} = {2}", this, name, GetProperty(name));
      }
    }


    // Constructors

    protected NodeBase(TParent parent, string name)
      : base(parent, name)
    {
    }
  }
}