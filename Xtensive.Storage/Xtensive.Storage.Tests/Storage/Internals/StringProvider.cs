// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.06

using System;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Tests.Storage.Internals
{
  public class StringProvider : DefaultGenerator
  {
    public override Tuple Next()
    {
      Tuple tuple = Tuple.Create(Hierarchy.TupleDescriptor);
      tuple.SetValue(0, Guid.NewGuid().ToString());
      return tuple;
    }
  }
}