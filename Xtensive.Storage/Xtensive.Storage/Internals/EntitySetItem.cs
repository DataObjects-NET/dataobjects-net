// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public abstract class EntitySetItem<TMaster, TSlave> : Entity
    where TMaster : IEntity
    where TSlave : IEntity
  {
    [Field, Key(0)]
    public TMaster Master { get; private set; }

    [Field, Key(1)]
    public TSlave Slave { get; private set; }


    // Constructors

    protected EntitySetItem(Tuple tuple)
      : base(tuple)
    {
    }
  }
}