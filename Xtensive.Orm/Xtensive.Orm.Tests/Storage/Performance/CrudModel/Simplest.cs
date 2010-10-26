// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.23

using System;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Storage.Performance.CrudModel
{
  [Serializable]
  [HierarchyRoot]
  [Index("Value")]
  public class Simplest : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public long Value { get; set; }


    // Constructors

    public Simplest(long id, long value)
      : base (id)
    {
      Value = value;
    }
  }
}