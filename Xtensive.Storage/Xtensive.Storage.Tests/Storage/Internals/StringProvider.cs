// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.06

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Generators;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests.Storage.Internals
{
  [KeyProvider(typeof (string))]
  public class StringProvider : GeneratorBase
  {
    public override Tuple Next()
    {
      Tuple tuple = Tuple.Create(Hierarchy.TupleDescriptor);
      tuple.SetValue(0, Guid.NewGuid().ToString());
      return tuple;
    }

    public StringProvider(HierarchyInfo hierarchy)
      : base(hierarchy)
    {
    }
  }
}