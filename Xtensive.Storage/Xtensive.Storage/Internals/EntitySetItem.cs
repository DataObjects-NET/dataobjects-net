// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Internals
{
  public abstract class EntitySetItem<TMaster, TSlave> : Entity
    where TMaster : Entity
    where TSlave : Entity
  {
    [Field]
    public TMaster Master { get; private set; }

    [Field]
    public TSlave Slave { get; private set; }


    // Constructor

    protected EntitySetItem(Tuple tuple)
      :base(tuple)
    {
    }
  }
}