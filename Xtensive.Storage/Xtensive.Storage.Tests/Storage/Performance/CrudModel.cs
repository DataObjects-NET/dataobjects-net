// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.23

using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Performance.CrudModel
{
  [HierarchyRoot("Id")]
  public class Simplest : Entity
  {
    [Field]
    public long Id { get; private set; }

    [Field]
    public long Value { get; set; }


    // Constructors

    public Simplest(long id, long value)
      : base (Tuple.Create(id))
    {
      Value = value;
    }
  }

  public class NativeSimplest
  {
    public long Id { get; set; }

    public long Value { get; set; }

    
    // Constructors

    public NativeSimplest()
    {
    }

    public NativeSimplest(long id, long value)
    {
      Id = id;
      Value = value;
    }
  }
}