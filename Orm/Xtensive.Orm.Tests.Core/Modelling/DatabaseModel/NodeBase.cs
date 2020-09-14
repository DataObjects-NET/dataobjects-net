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

namespace Xtensive.Orm.Tests.Core.Modelling.DatabaseModel
{
  [Serializable]
  public abstract class NodeBase<TParent> : Node<TParent, Server>
    where TParent : Node
  {
    protected override void PerformCreate()
    {
      base.PerformCreate();
      TestLog.Info("Created: {0}", this);
    }

    protected override void PerformMove(Node newParent, string newName, int newIndex)
    {
      using (TestLog.InfoRegion("Moving: {0}", this)) {
        if (Parent!=newParent)
          TestLog.Info("new Parent={0}", newParent);
        if (Name!=newName)
          TestLog.Info("new Name={0}", newName);
        if (Index!=newIndex)
          TestLog.Info("new Index={0}", newIndex);
        base.PerformMove(newParent, newName, newIndex);
      }
    }

    protected override void PerformShift(int offset)
    {
      TestLog.Info("Shifting: {0}, from {1} to {2}", this, Index, Index + offset);
      base.PerformShift(offset);
    }

    protected override void PerformRemove(Node source)
    {
      base.PerformRemove(source);
      if (source==this)
        TestLog.Info("Removed: {0}", this);
    }

    protected override void OnPropertyChanged(string name)
    {
      base.OnPropertyChanged(name);
      if (PropertyAccessors.TryGetValue(name, out var accessor) && accessor!=null) {
        if (!accessor.HasGetter)
          return;
        if (accessor.IsSystem)
          return;
        TestLog.Info("Changed: {0}, {1} = {2}", this, name, GetProperty(name));
      }
    }


    // Constructors

    protected NodeBase(TParent parent, string name)
      : base(parent, name)
    {
    }
  }
}