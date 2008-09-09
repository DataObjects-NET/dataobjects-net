// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Internals
{
  [HierarchyRoot("Entity1", "Entity2")]
  public abstract class EntitySetReference<T1, T2> : Entity
    where T1 : Entity
    where T2 : Entity
  {
    [Field]
    public T1 Entity1 { get; set; }

    [Field]
    public T2 Entity2 { get; set; }
  }
}