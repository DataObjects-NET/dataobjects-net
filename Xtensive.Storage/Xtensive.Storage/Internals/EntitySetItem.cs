// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Internals
{
  [KeyGenerator(null)]
  [HierarchyRoot]
  public abstract class EntitySetItem<TMaster, TSlave> : Entity
    where TMaster : Entity
    where TSlave : Entity
  {
    [Field, Key(0)]
    public TMaster Master { get; private set; }

    [Field, Key(1)]
    public TSlave Slave { get; private set; }


    // Constructors

    protected EntitySetItem(Tuple tuple)
      :base(tuple)
    {
    }
  }
}