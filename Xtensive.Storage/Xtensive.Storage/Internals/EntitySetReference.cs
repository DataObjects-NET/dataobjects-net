// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [HierarchyRoot(typeof (Generator), "Id")]
  public abstract class EntitySetReference<T1, T2> : Entity
    where T1 : Entity
    where T2 : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public T1 Left { get; set; }

    [Field]
    public T2 Right { get; set; }

  }
}