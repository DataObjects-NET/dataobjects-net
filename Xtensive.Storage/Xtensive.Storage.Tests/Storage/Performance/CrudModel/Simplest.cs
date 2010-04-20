// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.23

using System;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;

namespace Xtensive.Storage.Tests.Storage.Performance.CrudModel
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