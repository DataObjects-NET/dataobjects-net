// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.23

using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Tests.Storage.Performance.CrudModel
{
  [HierarchyRoot]
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