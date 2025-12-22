// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      TestLog.Info($"Created: {this}");
    }

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

    protected override void PerformShift(int offset)
    {
      TestLog.Info($"Shifting: {this}, from {Index} to {Index + offset}");
      base.PerformShift(offset);
    }

    protected override void PerformRemove(Node source)
    {
      base.PerformRemove(source);
      if (source==this)
        TestLog.Info($"Removed: {this}");
    }

    protected override void OnPropertyChanged(string name)
    {
      base.OnPropertyChanged(name);
      if (PropertyAccessors.TryGetValue(name, out var accessor) && accessor!=null) {
        if (!accessor.HasGetter)
          return;
        if (accessor.IsSystem)
          return;
        TestLog.Info($"Changed: {this}, {name} = {GetProperty(name)}");
      }
    }


    // Constructors

    protected NodeBase(TParent parent, string name)
      : base(parent, name)
    {
    }
  }
}
